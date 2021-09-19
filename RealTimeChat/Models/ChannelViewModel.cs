using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeChat.Models
{
    public class ChannelViewModel
    {
        public ListChannel ListChannels { get; set; }
        public ChannelUser ChannelUsers { get; set; }

        public string CurrentChannel { get; set; }
        public string loggedUser { get; set; }

        public List<Message> Messages { get; set; }
    }

    public class ListChannel
    {
        public List<string> channels { get; set; }
    }

    public class ChannelUser
    {
        public List<string> users { get; set; }
    }
}
