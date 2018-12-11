using System.Collections.Generic;


namespace FM.Redis
{
    public class RedisConfig
    {
       public string Name { get; set; }
        public List<RedisHost> Hosts { get; set; }
        public bool AllowAdmin { get; set; }
        public bool Ssl { get; set; }
        public int ConnectTimeout { get; set; }
        public int Database { get; set; }
        public string Password { get; set; }
    }
}
