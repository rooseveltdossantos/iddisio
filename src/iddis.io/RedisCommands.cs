using System;
using System.Linq;
using System.Text;

namespace iddis.io
{
    public class RedisCommands
    {
        private static readonly string CRLF = new string(new[] {'\r', '\n'});
        private static readonly int CRLFLength = CRLF.Length;
        private const  byte DATA_TYPE_BYTE_LENGTH = 1;

        private static char TYPE_SIMPLE_STRINGS = '+';
        private static char TYPE_ERRORS = '-';
        private static char TYPE_INTEGERS = ':';
        private const char TYPE_BULK_STRINGS = '$';
        private static char TYPE_ARRAYS = '*';

        public static string LLEN(string key)
        {
            return BulkString(iddisio.CMD_LLEN, key);
        }

        public static string SET(string key, string value)
        {
            return BulkString(iddisio.CMD_SET, key, value);
        }
        
        public static string SET(string key, string value, NXXX nxxx)
        {
            return BulkString(iddisio.CMD_SET, key, value, nxxx.ToString());
        }

        public static string SET(string key, string value, int seconds)
        {
            return BulkString(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, seconds.ToString());
        }

        public static string SET(string key, string value, int seconds, NXXX nxxx)
        {
            return BulkString(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, seconds.ToString(), nxxx.ToString());
        }

        public static string SET(string key, string value, int seconds, int miliseconds)
        {
            return BulkString(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, seconds.ToString(), iddisio.PAR_SET_PX, miliseconds.ToString());
        }

        public static string SET(string key, string value, int seconds, int miliseconds, NXXX nxxxx)
        {
            return BulkString(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, seconds.ToString(), iddisio.PAR_SET_PX, miliseconds.ToString(), nxxxx.ToString());
        }

        private static string BulkString(params string[] strings)
        {
            var size = strings.Select(s => DATA_TYPE_BYTE_LENGTH + ((int) (Math.Log10(s.Length)) + 1) + CRLFLength + s.Length + CRLFLength).Sum();
            var sb = new StringBuilder(size, size);
            Array.ForEach(strings, s => sb.AppendFormat("{0}{1}{2}{3}{4}", TYPE_BULK_STRINGS, s.Length, CRLF, s, CRLF));
            return sb.ToString();
        }
    }

    public enum NXXX
    {
        NX = 0,
        XX = 1
    }
}