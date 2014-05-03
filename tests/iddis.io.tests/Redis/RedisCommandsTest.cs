using System;
using System.Runtime.InteropServices;
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
            //new string(RedisCommands.LLEN("mylist")).Should().Be("*2\r\n$4\r\nLLEN\r\n$6\r\nmylist\r\n");
        }

        [Test]
        public void should_get_set_protocol()
        {
            //new string(RedisCommands.SET("key", Encoding.UTF8.GetBytes("value"))).Should().Be("*3\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n");
            //new string(RedisCommands.SET("key", "value")).Should().Be("*3\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n");

            //new string(RedisCommands.SET("key", "value", 1000)).Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n");

            //new string(RedisCommands.SET("key", "value", 2000, 2000)).Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n");

            //new string(RedisCommands.SET("key", "value", 2000, 2000, NXXX.NX)).Should().Be("*6\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n$2\r\nNX\r\n");
            //new string(RedisCommands.SET("key", "value", 2000, 2000, NXXX.XX)).Should().Be("*6\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n2000\r\n$2\r\nPX\r\n$4\r\n2000\r\n$2\r\nXX\r\n");

            //new string(RedisCommands.SET("key", "value", NXXX.NX)).Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nNX\r\n");
            //new string(RedisCommands.SET("key", "value", NXXX.XX)).Should().Be("*4\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nXX\r\n");

            //new string(RedisCommands.SET("key", "value", 1000, NXXX.NX)).Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n$2\r\nNX\r\n");
            //new string(RedisCommands.SET("key", "value", 1000, NXXX.XX)).Should().Be("*5\r\n$3\r\nSET\r\n$3\r\nkey\r\n$5\r\nvalue\r\n$2\r\nEX\r\n$4\r\n1000\r\n$2\r\nXX\r\n");
        }

        [Test]
        public unsafe void should_get()
        {
            var s = "ROOSEVELT";
            var b = new byte[s.Length];
            fixed (char* pChars = s)
            {
                var pSrc = pChars;
                fixed (byte* pBuffer = &b[0])
                {
                    var pDest = pBuffer;
                    for (var i = 0; i < s.Length; i++)
                    {
                        *pDest = (byte) *pSrc;
                        pDest++;
                        pSrc++;
                    }
                }
            }
            for (var i = 0; i < s.Length; i++)
            {
                b[i].Should().Be((byte)s[i]);
            }
        }
        
        [DllImport("kernel32.dll", SetLastError=true, EntryPoint="CopyMemory")]
        public unsafe extern static void CopyMemory(void* destination, void* source, uint length);

        [Test]
        public unsafe void should_get2()
        {
            const string s = "ROOSEVELT";
            var b = new byte[s.Length];
            fixed (char* pChars = s)
            {
                fixed (byte* pBuffer = &b[0])
                    CopyMemory(pBuffer, pChars, (uint)s.Length);
            }
            for (var i = 0; i < s.Length; i++)
            {
                b[i].Should().Be((byte)s[i]);
            }
        }
    }
}