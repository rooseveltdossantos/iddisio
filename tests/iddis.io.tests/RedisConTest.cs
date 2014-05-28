using System.Net.Sockets;
using NUnit.Framework;

namespace iddis.io.tests
{
    [TestFixture]
    public class RedisConTest
    {
        [Test]
        public void should_send_command()
        {
            var redisCommands = new RedisCommands(new RedisBuffer(1024));
            var redisCon = new RedisCon("localhost", 6379, redisCommands);
            redisCon.SET("key", "value");
        }
    }

    public class RedisCon
    {
        private readonly RedisCommands redisCommands;
        public string Host { get; private set; }
        public int Port { get; private set; }
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public RedisCon(string host, int port, RedisCommands redisCommands)
        {
            this.redisCommands = redisCommands;
            Host = host;
            Port = port;
        }

        public void SET(string key, string value)
        {
            var setProtocol = redisCommands.SET(key, value);
            socket.Connect(Host, Port);
            try
            {
                //TODO:Review
                socket.Send(setProtocol.GetRaw());
            }
            finally
            {
                socket.Close();
            }
        }
    }
}