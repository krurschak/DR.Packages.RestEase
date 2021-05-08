using System;
using System.Collections.Generic;

namespace DR.Packages.RestEase
{
    [Serializable]
    public class RestEaseOptions
    {
        public RestEaseOptions() { }

        public RestEaseOptions(Uri uri, string name)
        {
            Services = new List<Service>()
            {
                new Service
                {
                    Name = name,
                    Scheme = uri.Scheme,
                    Host = uri.Host,
                    Prefix = string.Empty,
                    Port = uri.Port
                }
            };
        }

        public IEnumerable<Service> Services { get; set; }

        public class Service
        {
            public string Name { get; set; }
            public string Scheme { get; set; }
            public string Host { get; set; }
            public int Port { get; set; }
            public string Prefix { get; set; }
        }
    }
}
