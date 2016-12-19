using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Playground.Exe
{
    public class ServiceBusSettings
    {
        public string Namespace { get; set; }

        public string PolicyName { get; set; }

        public string Key { get; set; }
    }
}
