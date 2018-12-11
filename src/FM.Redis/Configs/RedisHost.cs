namespace FM.Redis
{
	public class RedisHost 
	{
        public string IP { get; set; }
        public int Port { get; set; }
        public bool IsReadonly { get; set; }
        public string HostFullName => string.Format("{0}:{1}", IP, Port);
	}
}