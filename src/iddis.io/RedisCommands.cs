using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace iddis.io
{
    internal static class FastIntegerExtensions
    {
        private static readonly byte[] CharsTable = new[] { (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9' };

        /// <summary>
        /// Preenche o buffer com os caracteres representando o valor inteiro e retorna o número de dígitos que foi preenchido no buffer
        /// </summary>
        /// <param name="value">Valor inteiro maior que zero que será convertido para string</param>
        /// <param name="buffer">Array de caracteres que conterá a representação char do valor numérico</param>
        /// <param name="startIndex">Posição inicial no buffer onde os caracteres serão colocados</param>
        /// <returns>A quantidade de dígitos que foram escritas no array indicado por buffer</returns>
        public static int FastToArrayByte(this int value, byte[] buffer, int startIndex)
        {
            var digits = value.DigitsCount();
            startIndex += digits;
            while (value != 0)
            {
                var rem = value % 10;
                value /= 10;
                buffer[--startIndex] = CharsTable[rem];
            }
            return digits;
        }

        /// <summary>
        /// Retorna a quantidade de dígitos em um inteiro
        /// </summary>
        /// <param name="value">Valor inteiro que se deseja obter a quantidade de dígitos</param>
        /// <returns>Quantidade de dígitos</returns>
        public static int DigitsCount(this int value)
        {
            return ((int)(Math.Log10(value)) + 1);
        }
    }

    internal static class FastStringExtensions
    {
        public unsafe static void ASCIICopyTo(this string str, int sourceIndex, byte[] destination, int destinationIndex, int count)
        {
            fixed(char* pBytes = str)
        }
    }

    public class RedisCommands
    {
        private class NXBuffers
        {
            private static readonly Tuple<NXXX, char[]> NX = new Tuple<NXXX, char[]>(NXXX.NX, new[] { 'N', 'X' });
            private static readonly Tuple<NXXX, char[]> XX = new Tuple<NXXX, char[]>(NXXX.XX, new[] { 'X', 'X' });
            public static readonly Tuple<NXXX, char[]>[] Values = new[] { NX, XX };
        }

        private const byte CR = ASCIITable.CarriageReturn;
        private const byte LF = ASCIITable.LineFeed;

        private static readonly byte[] CRLF = new[] { CR, LF };
        private static readonly int CRLFLength = CRLF.Length;
        private static readonly int NXXXLength = DATA_TYPE_BYTE_LENGTH + 1 + CRLFLength + 2 + CRLFLength;
        private const byte DATA_TYPE_BYTE_LENGTH = 1;

        //private static char TYPE_SIMPLE_STRINGS = '+';
        //private static char TYPE_ERRORS = '-';
        //private static char TYPE_INTEGERS = ':';
        private const byte TYPE_BULK_STRINGS = ASCIITable.DollarSign;
        private const byte TYPE_ARRAYS = ASCIITable.Asterisk;

        private static readonly byte[] LLENDescriptor = new[] { TYPE_ARRAYS, ASCIITable.Two, CR, LF };
        private static readonly byte[] CMD_LLEN = new[] { ASCIITable.L, ASCIITable.L, ASCIITable.E, ASCIITable.N };

        /// <summary>
        /// Returns the length of the list stored at key. If key does not exist, it is interpreted as an empty list and 0 is returned. An error is returned when the value stored at key is not a list.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Integer reply: the length of the list at key.</returns>
        //TODO: Perform conversion to integer value
        public static byte[] LLEN(string key)
        {
            var size = LLENDescriptor.Length + CalcSizeOfBuffer(CMD_LLEN) + CalcSizeOfBuffer(key);
            var buffer = new byte[size];

            LLENDescriptor.CopyTo(buffer, 0);
            var i = BulkString(buffer, LLENDescriptor.Length, CMD_LLEN);
            BulkString(buffer, i, key);

            return buffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public static byte[] SET(string key, string value)
        {
            return SET(key, Encoding.UTF8.GetBytes(value));
        }

        private static readonly byte[] SETDescriptor = new[] { TYPE_ARRAYS, (byte)'0', CR, LF };

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">byte[] value to hold</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public static byte[] SET(string key, byte[] value)
        {
            const byte parameterCount = (byte)'3';
            var size = SETDescriptor.Length + CalcSizeOfBuffer(iddisio.CMD_SET, key) + CalcSizeOfBuffer(value);
            var buffer = new byte[size];

            SETDescriptor.CopyTo(buffer, 0);
            buffer[1] = parameterCount;

            var i = BulkString(buffer, SETDescriptor.Length, iddisio.CMD_SET);
            i = BulkString(buffer, i, key);
            BulkString(buffer, i, value);

            return buffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <param name="nxxx">NX -- Only set the key if it does not already exist. XX -- Only set the key if it already exist.</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public static byte[] SET(string key, string value, NXXX nxxx)
        {
            const byte parameterCount = (byte)'4';

            var size = SETDescriptor.Length + NXXXLength + CalcSizeOfBuffer(iddisio.CMD_SET, key, value);
            var buffer = new byte[size];

            SETDescriptor.CopyTo(buffer, 0);
            buffer[1] = parameterCount;

            var i = BulkString(buffer, SETDescriptor.Length, iddisio.CMD_SET);
            i = BulkString(buffer, i, key);
            i = BulkString(buffer, i, value);
            BulkString(buffer, i, NXBuffers.Values[(int)nxxx].Item2);

            return buffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <param name="secondsToExpire">Set the specified expire time, in seconds.</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public static byte[] SET(string key, string value, int secondsToExpire)
        {
            const byte parameterCount = (byte)'4';

            var size = SETDescriptor.Length + CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX) + CalcSizeOfBuffer(secondsToExpire);
            var buffer = new byte[size];

            SETDescriptor.CopyTo(buffer, 0);
            buffer[1] = parameterCount;

            var i = BulkString(buffer, SETDescriptor.Length, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            BulkString(buffer, i, secondsToExpire);
            return buffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <param name="secondsToExpire">Set the specified expire time, in seconds.</param>
        /// <param name="nxxx">NX -- Only set the key if it does not already exist. XX -- Only set the key if it already exist.</param>         
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public static byte[] SET(string key, string value, int secondsToExpire, NXXX nxxx)
        {
            const byte parameterCount = (byte)'5';

            var size = SETDescriptor.Length + CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX) + CalcSizeOfBuffer(secondsToExpire) + NXXXLength;
            var buffer = new byte[size];

            SETDescriptor.CopyTo(buffer, 0);
            buffer[1] = parameterCount;

            var i = BulkString(buffer, SETDescriptor.Length, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            i = BulkString(buffer, i, secondsToExpire);
            BulkString(buffer, i, NXBuffers.Values[(int)nxxx].Item2);

            return buffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <param name="secondsToExpire">Set the specified expire time, in seconds.</param>
        /// <param name="miliseconds">Set the specified expire time, in milliseconds.</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public static byte[] SET(string key, string value, int secondsToExpire, int miliseconds)
        {
            const byte parameterCount = (byte)'5';

            var size = SETDescriptor.Length + CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, iddisio.PAR_SET_PX) + CalcSizeOfBuffer(secondsToExpire, miliseconds);
            var buffer = new byte[size];

            SETDescriptor.CopyTo(buffer, 0);
            buffer[1] = parameterCount;

            var i = BulkString(buffer, SETDescriptor.Length, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            i = BulkString(buffer, i, secondsToExpire);
            i = BulkString(buffer, i, iddisio.PAR_SET_PX);
            BulkString(buffer, i, miliseconds);

            return buffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <param name="secondsToExpire">Set the specified expire time, in seconds.</param>
        /// <param name="miliseconds">Set the specified expire time, in milliseconds.</param>
        /// <param name="nxxx">NX -- Only set the key if it does not already exist. XX -- Only set the key if it already exist.</param>         
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public static byte[] SET(string key, string value, int secondsToExpire, int miliseconds, NXXX nxxx)
        {
            const byte parameterCount = (byte)'6';

            var size = SETDescriptor.Length + CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, iddisio.PAR_SET_PX) + CalcSizeOfBuffer(secondsToExpire, miliseconds) + NXXXLength;
            var buffer = new byte[size];

            SETDescriptor.CopyTo(buffer, 0);
            buffer[1] = parameterCount;

            var i = BulkString(buffer, SETDescriptor.Length, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            i = BulkString(buffer, i, secondsToExpire);
            i = BulkString(buffer, i, iddisio.PAR_SET_PX);
            i = BulkString(buffer, i, miliseconds);
            BulkString(buffer, i, NXBuffers.Values[(int)nxxx].Item2);

            return buffer;
        }

        private static int BulkString(byte[] buffer, int i, params string[] words)
        {

            foreach (var word in words)
            {
                buffer[i++] = TYPE_BULK_STRINGS;
                i += word.Length.FastToArrayByte(buffer, i);
                CRLF.CopyTo(buffer, i);
                i += CRLFLength;
                //word.CopyTo(0, buffer, i, word.Length);
                i += word.Length;
                //CRLF.CopyTo(0, buffer, i, CRLFLength);
                i += CRLFLength;
            }

            return i;
        }

        private static int BulkString(byte[] buffer, int i, params byte[][] words)
        {
            foreach (var word in words)
            {
                buffer[i++] = TYPE_BULK_STRINGS;
                i += word.Length.FastToArrayByte(buffer, i);
                CRLF.CopyTo(buffer, i);
                i += CRLFLength;
                word.CopyTo(buffer, i);
                i += word.Length;
                CRLF.CopyTo(buffer, i);
                i += CRLFLength;
            }
            return i;
        }

        private static int BulkString(byte[] buffer, int i, params char[][] words)
        {
            foreach (char[] word in words)
            {
                buffer[i++] = TYPE_BULK_STRINGS;
                i += word.Length.FastToArrayByte(buffer, i);
                CRLF.CopyTo(buffer, i); i += CRLFLength;
                word.CopyTo(buffer, i); i += word.Length;
                CRLF.CopyTo(buffer, i); i += CRLFLength;
            }

            return i;
        }

        private static int BulkString(byte[] buffer, int i, params int[] words)
        {
            foreach (var word in words)
            {
                buffer[i] = TYPE_BULK_STRINGS;
                i++;
                i += word.DigitsCount().FastToArrayByte(buffer, i);
                CRLF.CopyTo(buffer, i);
                i += CRLFLength;
                i += word.FastToArrayByte(buffer, i);
                CRLF.CopyTo(buffer, i);
                i += CRLFLength;
            }
            return i;
        }

        private static int CalcSizeOfBuffer(params byte[][] words)
        {
            return words.Sum(word => DATA_TYPE_BYTE_LENGTH + word.Length.DigitsCount() + CRLFLength + word.Length + CRLFLength);
        }

        private static int CalcSizeOfBuffer(params string[] words)
        {
            return words.Sum(word => DATA_TYPE_BYTE_LENGTH + word.Length.DigitsCount() + CRLFLength + word.Length + CRLFLength);
        }

        private static int CalcSizeOfBuffer(params int[] lengthOfWords)
        {
            return lengthOfWords.Sum(lengthWord => DATA_TYPE_BYTE_LENGTH + lengthWord.DigitsCount().DigitsCount() + CRLFLength + lengthWord.DigitsCount() + CRLFLength);
        }

    }

    public enum NXXX
    {
        NX = 0,
        XX = 1
    }
}