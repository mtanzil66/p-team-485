using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.IServices
{
    public interface IRedisService
    {


        public Task<bool> IsUserNameExists(string username);
        public Task AddUser(User user);

        public Task<User> GetUser(string username);

        public Task<List<string>> GetUsers();

        public Task<List<string>> GetUsersByChanel(string chanel);

        public Task<List<User>> GetUsersByCity(string city);

        public Task<List<User>> GetUsersByCountry(string country);

        public Task<List<string>> ListChannels();

        public Task<bool> IsChannelExists(string channel);

        public Task AddChannel(string channel);

        public Task PostMessage(Message message);

        public Task<List<Message>> GetLastMessages(string channel);

        public Task<List<Message>> GetAllMessagesByChannel(string channel, string username);

        public Task<List<Message>> SearchMessageByWord(string word);





    }
}
