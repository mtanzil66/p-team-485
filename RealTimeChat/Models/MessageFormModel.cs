using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeChat.Models
{
    public class MessageFormModel
    {
        [Required]
        public string Message { get; set; }

        [Required]
        public string Channel { get; set; }
    }
}
