using StackExchange.Redis;
using System;
using System.Net;

namespace FM.Redis
{
    /// <summary>
    /// Redis客户端
    /// </summary>
    public class RedisClient
    {
        private static readonly object SyncLock = new object();
        private ConnectionMultiplexer _redisConnection = null;
        private readonly RedisConfig _configuration;

        public RedisClient(RedisConfig configuration)
        {
            this._configuration = configuration;
        }

        public bool IsConnected => (_redisConnection != null && _redisConnection.IsConnected &&
                                    _redisConnection.GetDatabase().IsConnected(default(RedisKey)));

        public IDatabase GetDatabase()
        {
            var connection = ConstructCacheInstance();
            return connection.GetDatabase(_configuration.Database);
        }

        public ISubscriber GetSubscriber()
        {
            var connection = ConstructCacheInstance();
            return connection.GetSubscriber();
        }

        public EndPoint[] GetServerEndPoints()
        {
            var connection = ConstructCacheInstance();
            return connection.GetEndPoints();
        }

        public IServer GetServer(EndPoint ep)
        {
            var connection = ConstructCacheInstance();
            return connection.GetServer(ep);
        }

        public void ClearAll(IDatabase db)
        {
            var endPoints = db.Multiplexer.GetEndPoints();

            foreach (var endpoint in endPoints)
            {
                if (!IsEndPointReadonly(endpoint.ToString()))
                {
                    db.Multiplexer.GetServer(endpoint).FlushDatabase(db.Database);
                }
            }
        }

        private bool IsEndPointReadonly(string hostName)
        {
            foreach (var host in _configuration.Hosts)
            {
                if (host.HostFullName == hostName)
                {
                    return host.IsReadonly;
                }
            }
            return false;
        }

        private ConnectionMultiplexer ConstructCacheInstance()
        {
            if (IsConnected)
            {
                return _redisConnection;
            }

            lock (SyncLock)
            {
                var reConnectionTimes = 0;
                while (true)
                {
                    try
                    {
                        var connectionOptions = ConstructConnectionOptions();
                        _redisConnection = ConnectionMultiplexer.Connect(connectionOptions);

                        return _redisConnection;
                    }
                    catch (Exception ex)
                    {
                        reConnectionTimes++;
                        if (reConnectionTimes >= 3)
                        {
                            throw new Exception($"redis connect error达到了最大连接次数{reConnectionTimes},详情查看innerException",
                                ex);
                        }

                        //休息50ms，再次尝试
                        System.Threading.Thread.Sleep(50);
                    }
                }
            }
        }

        private ConfigurationOptions ConstructConnectionOptions()
        {
            var redisOptions = new ConfigurationOptions
            {
                Ssl = _configuration.Ssl,
                AllowAdmin = _configuration.AllowAdmin,
                ConnectTimeout = _configuration.ConnectTimeout,
                KeepAlive = 5,
                ClientName = _configuration.Name + DateTime.Now.ToString("yyyyMMddHHmmss"),
                Proxy = Proxy.None,
                AbortOnConnectFail = true
            };

            if (!string.IsNullOrWhiteSpace(_configuration.Password)) redisOptions.Password = _configuration.Password;

            foreach (RedisHost redisHost in _configuration.Hosts)
            {
                redisOptions.EndPoints.Add(redisHost.IP, redisHost.Port);
            }
            return redisOptions;
        }
    }
}