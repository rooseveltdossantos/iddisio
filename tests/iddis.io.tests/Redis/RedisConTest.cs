using System;
using System.Linq;
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
            var buffer = new byte[500];
            var setProtocol = RedisCommands.SET("key", "value").Select(c => (byte)c).ToArray();
            var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            s.Connect("localhost", 6379);
            var send = s.Send(setProtocol);
            s.Receive(buffer);
            foreach (var b in buffer)
                Console.Write((char) b);
            s.Close();
        }
    }
}