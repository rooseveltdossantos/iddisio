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

        [Test]
        public void should_raise_ArgumentOutOfRangeException()
        {
            const int lessthanZero = -1;
            var redisBuffer = new RedisBuffer(100);
            var arr = new byte[10];
            this.Invoking(x => redisBuffer.CopyFrom(arr, arr.GetLowerBound(0) - 1, 0, 10)).ShouldThrow<ArgumentOutOfRangeException>();
            this.Invoking(x => redisBuffer.CopyFrom(arr, 0, lessthanZero, 10)).ShouldThrow<ArgumentOutOfRangeException>();
            this.Invoking(x => redisBuffer.CopyFrom(arr, 0, 0, lessthanZero)).ShouldThrow<ArgumentOutOfRangeException>();
        }
    }
}