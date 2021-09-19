using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RealTimeChat
{
    public static class SessionExtensions
    {
        public static void Set(this ISession session, string key, string value)
        {
            session.SetString(key, value);
        }

        public static string Get(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? "" : value;
        }
    }
}
