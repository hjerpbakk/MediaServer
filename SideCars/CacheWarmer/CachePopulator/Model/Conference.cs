using System;
using Newtonsoft.Json;

namespace CachePopulator.Model
{
	public struct Conference : IEquatable<Conference>
    {
        [JsonConstructor]
        public Conference(string id)
        {
            Id = id;
        }

        public string Id { get; }

        public bool Equals(Conference other) => Id == other.Id;
    }
}
