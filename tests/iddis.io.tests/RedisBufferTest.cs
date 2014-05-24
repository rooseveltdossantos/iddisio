using System;
using FluentAssertions;
using NUnit.Framework;

namespace iddis.io.tests
{
    [TestFixture(Category = "Unity:RedisBuffer", Description = "Check all public methods of RedisBuffer class")]
    public class RedisBufferTest
    {
        private const byte _8Bytes = 8;
        private const byte _64Bytes = 64;

        [Test]
        public void should_be_instanced()
        {
            var redisBuffer = new RedisBuffer(_64Bytes);
            redisBuffer.Should().NotBeNull();
            
            var raw = redisBuffer.GetRaw();
            
            raw.Length.Should().Be(_64Bytes);
            
            raw.Should().NotContain(v => v != 0);

            var otherRaw = redisBuffer.GetRaw();

            ReferenceEquals(raw, otherRaw).Should().BeTrue();
        }

        [Test]
        public void should_be_copy_from_array_of_bytes()
        {
            var redisBuffer = new RedisBuffer(_64Bytes);
            var arr = new byte[_64Bytes];
            for (byte i = 0; i < _64Bytes; i++)
                arr[i] = i;
            redisBuffer.CopyFrom(arr, 0, 0, _64Bytes);
            byte[] raw = redisBuffer.GetRaw();
            raw.Should().BeEquivalentTo(arr);
        }

        [Test]
        public void should_raise_ArgumentNullException()
        {
            var redisBuffer = new RedisBuffer(_64Bytes);
            this.Invoking(x => redisBuffer.CopyFrom(null, 0, 0, _64Bytes)).ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void should_raise_ArgumentOutOfRangeException()
        {
            const int lessthanZero = -1;
            var redisBuffer = new RedisBuffer(_64Bytes);
            var arr = new byte[_8Bytes];
            this.Invoking(x => redisBuffer.CopyFrom(arr, arr.GetLowerBound(0) - 1, 0, _8Bytes)).ShouldThrow<ArgumentOutOfRangeException>();
            this.Invoking(x => redisBuffer.CopyFrom(arr, 0, lessthanZero, _8Bytes)).ShouldThrow<ArgumentOutOfRangeException>();
            this.Invoking(x => redisBuffer.CopyFrom(arr, 0, 0, lessthanZero)).ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void should_write_byte()
        {
            var redisBuffer = new RedisBuffer(_64Bytes);
            redisBuffer.WriteByte(1, 0);
            redisBuffer.GetRaw()[0].Should().Be(1);
        }

        [Test]
        public unsafe void should_GetAddressFromIndex()
        {
            var redisBuffer = new RedisBuffer(_64Bytes);
            byte* addressFromIndexZero = redisBuffer.GetAddressFromIndex(0);
            byte* addressFromIndexOne = redisBuffer.GetAddressFromIndex(1);

            (addressFromIndexZero < addressFromIndexOne).Should().BeTrue();

            redisBuffer.WriteByte(65, 0);

            byte v = *addressFromIndexZero;
            v.Should().Be(65);
        }

        [Test]
        public void should_GetRaw()
        {
            var redisBuffer = new RedisBuffer(_64Bytes);
            
            for (byte index = 0; index < _64Bytes; index++)
                redisBuffer.WriteByte(index, index);
            
            var raw = redisBuffer.GetRaw();

            for (byte index = 0; index < _64Bytes; index++)
                raw[index].Should().Be(index);
        }
    }
}