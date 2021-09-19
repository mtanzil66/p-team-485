using DataAccessLayer.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NRediSearch;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace DataAccessLayer.IServices
{
    public class RedisService : IRedisService
    {

        private readonly ConfigurationOptions configs;
        private readonly ConnectionMultiplexer redis;
        private readonly IDatabase _redisDb;
        private readonly Client userSerachClient;
        private readonly Client messageSerachClient;

        public RedisService()
        {
            //keys left here for testing,  its a test env
            configs = new ConfigurationOptions()
            {
                SyncTimeout = 500000,
                AllowAdmin = true,
                Password = "qlVJZ7SebyI89XHGtkuWGPq2kc0UNig8",
                EndPoints =
            {
                {"redis-19522.c251.east-us-mz.azure.cloud.redislabs.com",19522 }
            },
                AbortOnConnectFail = false
            };



            redis = ConnectionMultiplexer.Connect(configs);

            _redisDb = redis.GetDatabase();

            userSerachClient = new Client("idx:users", _redisDb);
            messageSerachClient = new Client("idx:messages", _redisDb);


        }


        public async Task AddUser(User user)
        {

            await _redisDb.HashSetAsync("user:ayaz", ToHashEntries(user));

            await _redisDb.SetAddAsync("usernames", user.username);


        }


        public async Task<User> GetUser(string username)
        {

            var userHash = await _redisDb.HashGetAllAsync(username);

            return ConvertFromRedis<User>(userHash);



        }

        public async Task<List<string>> GetUsers()
        {
            var users = await _redisDb.SetMembersAsync("usernames");

            return users.Select(u => u.ToString()).ToList();
        }

        public async Task<List<string>> GetUsersByChanel(string chanel)
        {

            var users = await _redisDb.SetMembersAsync("usernames");

            return users.Select(u => u.ToString()).ToList();
        }


        private User MapToUser(Document doc)
        {
            return new User
            {
                channel = doc["channel"],
                city = doc["city"],
                country = doc["country"],
                firstname = doc["firstname"],
                lastname = doc["lastname"],
                username = doc["username"]
            };
        }
        public async Task<List<User>> GetUsersByCity(string city)
        {

            var users = await userSerachClient.SearchAsync(new NRediSearch.Query("@city:{" + city + "}") { WithPayloads = true });

            return users.Documents.Select(d => MapToUser(d)).ToList();

        }

        public async Task<List<User>> GetUsersByCountry(string country)
        {
            var users = await userSerachClient.SearchAsync(new NRediSearch.Query("@city:{" + country + "}") { WithPayloads = true });

            return users.Documents.Select(d => MapToUser(d)).ToList();
        }


        private static HashEntry[] ToHashEntries(object obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties();
            return properties
                .Where(x => x.GetValue(obj) != null)
                .Select
                (
                      property =>
                      {
                          object propertyValue = property.GetValue(obj);
                          string hashValue;

                          if (propertyValue is IEnumerable<object>)
                          {

                              hashValue = JsonConvert.SerializeObject(propertyValue);
                          }
                          else
                          {
                              hashValue = propertyValue.ToString();
                          }

                          return new HashEntry(property.Name, hashValue);
                      }
                )
                .ToArray();
        }

        private static T ConvertFromRedis<T>(HashEntry[] hashEntries)
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in properties)
            {
                HashEntry entry = hashEntries.FirstOrDefault(g => g.Name.ToString().Equals(property.Name));
                if (entry.Equals(new HashEntry())) continue;
                property.SetValue(obj, Convert.ChangeType(entry.Value.ToString(), property.PropertyType));
            }
            return (T)obj;
        }

        public async Task<bool> IsUserNameExists(string username)
        {
            return await _redisDb.SetContainsAsync("usernames", username);
        }

        public async Task<List<string>> ListChannels()
        {

            var channeles = await _redisDb.SetMembersAsync("channels");

            return channeles.Select(c => c.ToString()).ToList();


        }

        public async Task<bool> IsChannelExists(string channel)
        {
            return await _redisDb.SetContainsAsync("channels", channel);
        }

        public async Task AddChannel(string channel)
        {
            await _redisDb.SetAddAsync("channels", channel);

        }

        public async Task PostMessage(Message message)
        {
            var messageId = await _redisDb.StreamAddAsync($"channel:{message.channel}", "message", "*");

            await _redisDb.HashSetAsync($"message:{messageId}", ToHashEntries(message));
        }

        public async Task<List<Message>> GetLastMessages(string channel)
        {

            var messageIds = await _redisDb.StreamReadAsync($"channel:{channel}", "0-2");

            var messageList = new List<Message>();

            foreach (var messageId in messageIds)
            {
                messageList.Add(await GetMessageById(messageId.Id.ToString()));
            }

            return messageList;


        }

        private async Task<Message> GetMessageById(string Id)
        {
            var message = await _redisDb.HashGetAllAsync($"message:{Id}");
            return ConvertFromRedis<Message>(message);
        }

        public async Task<List<Message>> GetAllMessagesByChannel(string channel, string username)
        {
            var messageIds = _redisDb.StreamRead($"channel:{channel}", "0");

            var messageList = new List<Message>();

            foreach (var messageId in messageIds)
            {
                messageList.Add(await GetMessageById(messageId.Id.ToString()));
            }



            return messageList;
        }

        private async Task SetLastreadMessageId(string username, string messageId, string channel)
        {
            var user = await _redisDb.HashGetAllAsync(username);

            var userObject = ConvertFromRedis<User>(user);

        }

        public async Task<List<Message>> SearchMessageByWord(string word)
        {


            var users = await messageSerachClient.SearchAsync(new NRediSearch.Query(word) { WithPayloads = true });

            return users.Documents.Select(d => MapToMessage(d)).ToList();

        }



        private Message MapToMessage(Document doc)
        {
            return new Message
            {
                channel = doc["channel"],
                message = doc["message"],
                username = doc["username"]
            };
        }
    }
}