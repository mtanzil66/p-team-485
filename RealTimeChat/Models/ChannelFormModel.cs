using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeChat.Models
{
    public class ChannelFormModel
    {
        [Required]
        public string channel { get; set; }
    }
}
