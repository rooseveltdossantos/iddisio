using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace iddis.io.tests
{
    [TestFixture]
    public class RedisCommandsTest
    {
        [Test]
        public void should_get_llen_protocol()
        {
            RedisCommands.LLEN("mylist").ASCIIToString().Should().Be("*2\r\n$4\r\nLLEN\r\n$6\r\nmylist\r\n");
        }

        [Test]
        public void should_get_set_protocol()
        {
            RedisCommands.SET("key", Encoding.UTF8.GetBytes("value")).ASCIIToString().Should().Be("*3\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n");
            RedisCommands.SET("key", "value").ASCIIToString().Should().Be("*3\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n");

            RedisCommands.SET("key", "value", 1000).ASCIIToString().Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n");

            RedisCommands.SET("key", "value", 2000, 2000).ASCIIToString().Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n");

            RedisCommands.SET("key", "value", 2000, 2000, NXXX.NX).ASCIIToString().Should().Be("*6\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n$2\r\nNX\r\n");
            RedisCommands.SET("key", "value", 2000, 2000, NXXX.XX).ASCIIToString().Should().Be("*6\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n$2\r\nXX\r\n");

            RedisCommands.SET("key", "value", NXXX.NX).ASCIIToString().Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nNX\r\n");
            RedisCommands.SET("key", "value", NXXX.XX).ASCIIToString().Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nXX\r\n");

            RedisCommands.SET("key", "value", 1000, NXXX.NX).ASCIIToString().Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n$2\r\nNX\r\n");
            RedisCommands.SET("key", "value", 1000, NXXX.XX).ASCIIToString().Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n$2\r\nXX\r\n");
        }
    }
}