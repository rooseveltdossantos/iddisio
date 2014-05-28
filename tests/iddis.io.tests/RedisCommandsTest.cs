using System.Text;
using FluentAssertions;
using NUnit.Framework;

namespace iddis.io.tests
{
    [TestFixture(Category = "Unity:RedisCommands", Description = "Check all public methods of RedisCommands class")]
    public class RedisCommandsTest
    {
        private RedisCommands redisCommands;

        [SetUp]
        public void Setup()
        {
            redisCommands = new RedisCommands(new RedisBuffer(1024));
        }

        [Test]
        public void should_get_llen_protocol()
        {
            redisCommands.LLEN("mylist").ASCIIToString().Should().Be("*2\r\n$4\r\nLLEN\r\n$6\r\nmylist\r\n");
        }

        [Test]
        public void should_get_set_protocol()
        {
            redisCommands.SET("key", Encoding.UTF8.GetBytes("value")).ASCIIToString().Should().Be("*3\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n");
            redisCommands.SET("key", "value").ASCIIToString().Should().Be("*3\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n");

            redisCommands.SET("key", "value", 1000).ASCIIToString().Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n");

            redisCommands.SET("key", "value", 2000, 2000).ASCIIToString().Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n");

            redisCommands.SET("key", "value", 2000, 2000, NXXX.NX).ASCIIToString().Should().Be("*6\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n$2\r\nNX\r\n");
            redisCommands.SET("key", "value", 2000, 2000, NXXX.XX).ASCIIToString().Should().Be("*6\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n$2\r\nXX\r\n");

            redisCommands.SET("key", "value", NXXX.NX).ASCIIToString().Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nNX\r\n");
            redisCommands.SET("key", "value", NXXX.XX).ASCIIToString().Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nXX\r\n");

            redisCommands.SET("key", "value", 1000, NXXX.NX).ASCIIToString().Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n$2\r\nNX\r\n");
            redisCommands.SET("key", "value", 1000, NXXX.XX).ASCIIToString().Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n$2\r\nXX\r\n");
        }
    }
}