using System.Net.Sockets;
using NUnit.Framework;

namespace iddis.io.tests.Redis
{
    [TestFixture]
    public class RedisConTest
    {
        [Test]
        public void should_send_command()
        {
            var redisCon = new RedisCon("localhost", 6379);
            redisCon.SET("key", "value");
        }
    }

    public class RedisCon
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        private readonly Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public RedisCon(string host, int port)
        {
            Host = host;
            Port = port;
        }

        public void SET(string key, string value)
        {
            var setProtocol = RedisCommands.SET(key, value);
            socket.Connect(Host, Port);
            try
            {
                //TODO:Rever
                //socket.Send(setProtocol);
            }
            finally
            {
                socket.Close();
            }
        }
    }
}