using System;
using System.Linq;
using System.Text;

namespace iddis.io
{
    internal static class FastIntegerExtensions
    {
        private static readonly char[] CharsTable = new[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

        /// <summary>
        /// Preenche o buffer com os caracteres representando o valor inteiro e retorna o número de dígitos que foi preenchido no buffer
        /// </summary>
        /// <param name="value">Valor inteiro maior que zero que será convertido para string</param>
        /// <param name="buffer">Array de caracteres que conterá a representação char do valor numérico</param>
        /// <param name="startIndex">Posição inicial no buffer onde os caracteres serão colocados</param>
        /// <returns>A quantidade de dígitos que foram escritas no array indicado por buffer</returns>
        public static int FastToArrayChar(this int value, char[] buffer, int startIndex)
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

    public class RedisCommands
    {
        private class NXBuffers
        {
            private static readonly Tuple<NXXX, char[]> NX = new Tuple<NXXX, char[]>(NXXX.NX, new[] { 'N', 'X' });
            private static readonly Tuple<NXXX, char[]> XX = new Tuple<NXXX, char[]>(NXXX.XX, new[] { 'X', 'X' });
            public static readonly Tuple<NXXX, char[]>[] Values = new[] { NX, XX };
        }

        private static readonly string CRLF = new string(new[] { '\r', '\n' });
        private static readonly int CRLFLength = CRLF.Length;
        private static readonly int NXXXLength = DATA_TYPE_BYTE_LENGTH + 1 + CRLFLength + 2 + CRLFLength;
        private const byte DATA_TYPE_BYTE_LENGTH = 1;

        //private static char TYPE_SIMPLE_STRINGS = '+';
        //private static char TYPE_ERRORS = '-';
        //private static char TYPE_INTEGERS = ':';
        private const char TYPE_BULK_STRINGS = '$';
        //private static char TYPE_ARRAYS = '*';

        public static char[] LLEN(string key)
        {
            var size = CalcSizeOfBuffer(iddisio.CMD_LLEN, key);
            var buffer = new char[size];
            
            var i = BulkString(buffer, 0, iddisio.CMD_LLEN);
            BulkString(buffer, i, key);

            return buffer;
        }

        public static char[] SET(string key, string value)
        {
            return SET(key, Encoding.UTF8.GetBytes(value));
        }

        public static char[] SET(string key, byte[] value)
        {
            var size = CalcSizeOfBuffer(iddisio.CMD_SET, key) + CalcSizeOfBuffer(value);
            var buffer = new char[size];

            var i = BulkString(buffer, 0, iddisio.CMD_SET);
            i = BulkString(buffer, i, key);
            BulkString(buffer, i, value);

            return buffer;
        }

        public static char[] SET(string key, string value, NXXX nxxx)
        {
            var size = NXXXLength + CalcSizeOfBuffer(iddisio.CMD_SET, key, value);
            var buffer = new char[size];

            var i = BulkString(buffer, 0, iddisio.CMD_SET);
            i = BulkString(buffer, i, key);
            i = BulkString(buffer, i, value);
            BulkString(buffer, i, NXBuffers.Values[(int)nxxx].Item2);

            return buffer;
        }

        public static char[] SET(string key, string value, int secondsToExpire)
        {
            var size = CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX) + CalcSizeOfBuffer(secondsToExpire);
            var buffer = new char[size];

            var i = BulkString(buffer, 0, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            BulkString(buffer, i, secondsToExpire);
            return buffer;
        }

        public static char[] SET(string key, string value, int secondsToExpire, NXXX nxxx)
        {
            var size = CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX) + CalcSizeOfBuffer(secondsToExpire) + NXXXLength;
            var buffer = new char[size];

            var i = BulkString(buffer, 0, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            i = BulkString(buffer, i, secondsToExpire);
            BulkString(buffer, i, NXBuffers.Values[(int)nxxx].Item2);

            return buffer;
        }

        public static char[] SET(string key, string value, int secondsToExpire, int miliseconds)
        {
            var size = CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, iddisio.PAR_SET_PX) + CalcSizeOfBuffer(secondsToExpire, miliseconds);
            var buffer = new char[size];

            var i = BulkString(buffer, 0, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            i = BulkString(buffer, i, secondsToExpire);
            i = BulkString(buffer, i, iddisio.PAR_SET_PX);
            BulkString(buffer, i, miliseconds);

            return buffer;
        }

        public static char[] SET(string key, string value, int secondsToExpire, int miliseconds, NXXX nxxx)
        {
            var size = CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, iddisio.PAR_SET_PX) + CalcSizeOfBuffer(secondsToExpire, miliseconds) + NXXXLength;
            var buffer = new char[size];

            var i = BulkString(buffer, 0, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            i = BulkString(buffer, i, secondsToExpire);
            i = BulkString(buffer, i, iddisio.PAR_SET_PX);
            i = BulkString(buffer, i, miliseconds);
            BulkString(buffer, i, NXBuffers.Values[(int)nxxx].Item2);

            return buffer;
        }

        private static int BulkString(char[] buffer, int i, params string[] words)
        {
            foreach (var word in words)
            {
                buffer[i++] = TYPE_BULK_STRINGS;
                i += word.Length.FastToArrayChar(buffer, i);
                CRLF.CopyTo(0, buffer, i, CRLFLength);
                i += CRLFLength;
                word.CopyTo(0, buffer, i, word.Length);
                i += word.Length;
                CRLF.CopyTo(0, buffer, i, CRLFLength);
                i += CRLFLength;
            }

            return i;
        }

        private static int BulkString(char[] buffer, int i, params byte[][] words)
        {
            foreach (var word in words)
            {
                buffer[i++] = TYPE_BULK_STRINGS;
                i += word.Length.FastToArrayChar(buffer, i);
                CRLF.CopyTo(0, buffer, i, CRLFLength);
                i += CRLFLength;
                word.CopyTo(buffer, i);
                i += word.Length;
                CRLF.CopyTo(0, buffer, i, CRLFLength);
                i += CRLFLength;
            }
            return i;
        }

        private static int BulkString(char[] buffer, int i, params char[][] words)
        {
            foreach (char[] word in words)
            {
                buffer[i++] = TYPE_BULK_STRINGS;
                i += word.Length.FastToArrayChar(buffer, i);
                CRLF.CopyTo(0, buffer, i, CRLFLength); i += CRLFLength;
                word.CopyTo(buffer, i); i += word.Length;
                CRLF.CopyTo(0, buffer, i, CRLFLength); i += CRLFLength;
            }

            return i;
        }

        private static int BulkString(char[] buffer, int i, params int[] words)
        {
            foreach (var word in words)
            {
                buffer[i] = TYPE_BULK_STRINGS;
                i++;
                i += word.DigitsCount().FastToArrayChar(buffer, i);
                CRLF.CopyTo(0, buffer, i, CRLFLength);
                i += CRLFLength;
                i += word.FastToArrayChar(buffer, i);
                CRLF.CopyTo(0, buffer, i, CRLFLength);
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