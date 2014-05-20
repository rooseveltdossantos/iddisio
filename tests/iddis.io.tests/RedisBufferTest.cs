using System;
using FluentAssertions;
using NUnit.Framework;

namespace iddis.io.tests
{
    [TestFixture]
    public class RedisBufferTest
    {
        [Test]
        public void should_be_instanced()
        {
            var redisBuffer = new RedisBuffer(100);
            redisBuffer.Should().NotBeNull();
        }

        [Test]
        public void should_be_copy_from_array_of_bytes()
        {
            var redisBuffer = new RedisBuffer(100);
            var arr = new byte[100];
            for (byte i = 0; i < 100; i++)
                arr[i] = i;
            redisBuffer.CopyFrom(arr, 0, 0, 100);
            byte[] raw = redisBuffer.GetRaw();
            raw.Should().BeEquivalentTo(arr);
        }

        [Test]
        public void should_raise_ArgumentNullException()
        {
            var redisBuffer = new RedisBuffer(100);
            this.Invoking(x => redisBuffer.CopyFrom(null, 0, 0, 100)).ShouldThrow<ArgumentNullException>();

        }
    }
}