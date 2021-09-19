using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.IServices;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Logging;
using RealTimeChat.Models;

namespace RealTimeChat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IRedisService _redis;

        public HomeController(ILogger<HomeController> logger, IRedisService redis)
        {
            _logger = logger;
            _redis = redis;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(UserModel formModel) 
        {
            if (ModelState.IsValid)
            {
                var userExists = await _redis.IsUserNameExists(formModel.Username).ConfigureAwait(false);
                if (!userExists)
                {
                    var user = new User
                    {
                        firstname = formModel.FirstName,
                        lastname = formModel.LastName,
                        city = formModel.City,
                        country = formModel.Country,
                        username = formModel.Username
                    };

                    await _redis.AddUser(user);

                    HttpContext.Session.Set("username", user.username);

                    return Redirect("/Home/Channels");
                }
                else
                {
                    ModelState.AddModelError("Username", "Username already exists");
                }
            }

            return View("Index");
        }

        public async Task<IActionResult> Channels(string c = null)
        {
            var userSignedUp = HttpContext.Session.Get("username");
            if (string.IsNullOrEmpty(userSignedUp))
            {
                return Redirect("/Home/Index");
            }

            var channels = await _redis.ListChannels().ConfigureAwait(false);
            var rChannel = string.IsNullOrEmpty(c) ? (channels.Any() ? channels[0] : null) : c;

            var users = !string.IsNullOrEmpty(rChannel) ? await _redis.GetUsersByChanel(rChannel).ConfigureAwait(false) : null;
            var messages = !string.IsNullOrEmpty(rChannel) ? await _redis.GetAllMessagesByChannel(rChannel, null).ConfigureAwait(false) : null;

            var viewModel = new ChannelViewModel
            {
                ListChannels = new ListChannel
                {
                    channels = channels
                },
                ChannelUsers = new ChannelUser
                {
                    users = users
                },
                CurrentChannel = rChannel,
                loggedUser = userSignedUp,
                Messages = messages
            };

            return View(viewModel);
        }

        public IActionResult CreateChannel()
        {
            var userSignedUp = HttpContext.Session.Get("username");
            if (string.IsNullOrEmpty(userSignedUp))
            {
                return Redirect("/Home/Index");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateChannel(ChannelFormModel model) 
        {
            if (ModelState.IsValid)
            {
                var channelExist = await _redis.IsChannelExists(model.channel).ConfigureAwait(false);
                if (!channelExist)
                {
                    await _redis.AddChannel(model.channel).ConfigureAwait(false);

                    return Redirect("/Home/Channels?c="+model.channel);
                } else
                {
                    ModelState.AddModelError("channel", "Channel already exists");
                }
            }

            return View();
        }

        public async Task<IActionResult> Users()
        {
            var userSignedUp = HttpContext.Session.Get("username");
            if (string.IsNullOrEmpty(userSignedUp))
            {
                return Redirect("/Home/Index");
            }

            List<string> users = await _redis.GetUsers().ConfigureAwait(false);
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> PostMessage(MessageFormModel model)
        {
            if (ModelState.IsValid)
            {
                var message = new Message
                {
                    channel = model.Channel,
                    message = model.Message,
                    username = HttpContext.Session.Get("username")
                };

                await _redis.PostMessage(message).ConfigureAwait(false);

                return Redirect("/Home/Channels?c="+message.channel);
            }

            return Redirect("/Home/Channels?c="+model.Channel);
        }
    }
}
