using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SenseNet.Client;

namespace SnWebApplication.Mvc.Client.Models
{
    public class SnContent
    {
        public dynamic Content { get; set; }
        public IEnumerable<dynamic> Children { get; set; }
    }
}
