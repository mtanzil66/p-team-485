using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Models
{
    public class User
    {
        public string firstname { get; set; }
        public string lastname { get; set; }

        public string city { get; set; }
        public string country { get; set; }

        public string username { get; set; }
        public string channel { get; set; }

    }
}
