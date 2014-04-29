using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace iddis.io.tests.Redis
{
    [TestFixture]
    public class RedisCommandsTest
    {
        [Test]
        public void should_get_llen_protocol()
        {
            new string(RedisCommands.LLEN("mylist")).Should().Be("*2\r\n$4\r\nLLEN\r\n$6\r\nmylist\r\n");
        }

        [Test]
        public void should_get_set_protocol()
        {
            new string(RedisCommands.SET("key", Encoding.UTF8.GetBytes("value"))).Should().Be("*3\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n");
            new string(RedisCommands.SET("key", "value")).Should().Be("*3\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n");

            new string(RedisCommands.SET("key", "value", 1000)).Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n");

            new string(RedisCommands.SET("key", "value", 2000, 2000)).Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n");

            new string(RedisCommands.SET("key", "value", 2000, 2000, NXXX.NX)).Should().Be("*6\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n$2\r\nNX\r\n");
            new string(RedisCommands.SET("key", "value", 2000, 2000, NXXX.XX)).Should().Be("*6\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n$2\r\nXX\r\n");

            new string(RedisCommands.SET("key", "value", NXXX.NX)).Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nNX\r\n");
            new string(RedisCommands.SET("key", "value", NXXX.XX)).Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nXX\r\n");

            new string(RedisCommands.SET("key", "value", 1000, NXXX.NX)).Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n$2\r\nNX\r\n");
            new string(RedisCommands.SET("key", "value", 1000, NXXX.XX)).Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n$2\r\nXX\r\n");
        }
    }
}