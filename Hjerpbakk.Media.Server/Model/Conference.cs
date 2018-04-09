using System;
using Newtonsoft.Json;

namespace Hjerpbakk.Media.Server.Model
{
    public struct Conference
    {
        [JsonConstructor]
        public Conference(string name, string description, string path) 
        {
            Name = name;
            Description = description;
            Path = path;
        }

        public string Name { get; }
        public string Description { get; }
        public string Path { get; }
    }
}
