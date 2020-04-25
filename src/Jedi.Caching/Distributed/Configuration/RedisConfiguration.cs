using System.Collections.Generic;

namespace Jedi.Caching.Distributed
{
    public sealed class RedisConfiguration
    {
        public int ConnectionTimeout { get; set; } = 5000;

        public int SyncTimeout { get; set; } = 5000;

        public int DefaultDatabase { get; set; } = 0;

        public List<string> EndPoints { get; set; }

        public int ConnectionRetryAttemps { get; set; } = 3;

        public bool AllowAdmin { get; set; } = true;

        public bool AbortConnect { get; set; } = false;
    }
}
