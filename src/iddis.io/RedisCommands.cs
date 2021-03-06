﻿using System;
using System.Linq;
using System.Text;

namespace iddis.io
{
    internal static class FastIntegerExtensions
    {
        private static readonly byte[] CharsTable = { ASCIITable.Zero, ASCIITable.One, ASCIITable.Two, ASCIITable.Three, ASCIITable.Four, ASCIITable.Five, ASCIITable.Six, ASCIITable.Seven, ASCIITable.Eight, ASCIITable.Nine };

        /// <summary>
        /// Preenche o buffer com os caracteres representando o valor inteiro e retorna o número de dígitos que foi preenchido no buffer
        /// </summary>
        /// <param name="value">Valor inteiro maior que zero que será convertido para string</param>
        /// <param name="redisBuffer">Array de caracteres que conterá a representação char do valor numérico</param>
        /// <param name="startIndex">Posição inicial no buffer onde os caracteres serão colocados</param>
        /// <returns>A quantidade de dígitos que foram escritas no array indicado por buffer</returns>
        public static int FastToArrayByte(this int value, RedisBuffer redisBuffer, int startIndex)
        {
            var digits = value.DigitsCount();
            startIndex += digits;
            while (value != 0)
            {
                var rem = value % 10;
                value /= 10;
                redisBuffer.WriteByte(CharsTable[rem], --startIndex);
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
        /// <summary>
        /// Copy an ASCII string to byte array
        /// </summary>
        /// <param name="str">ASCII string to be copied</param>
        /// <param name="sourceIndex">Index on source string to start the copy</param>
        /// <param name="redisBuffer">The ProtocolBuffer to ASCII string to be copied</param>
        /// <param name="destinationIndex">Index on destination to start the copy</param>
        /// <param name="count">Count of ASCII chars to be copied from string</param>
        public unsafe static void ASCIICopyTo(this string str, int sourceIndex, RedisBuffer redisBuffer, int destinationIndex, int count)
        {
            fixed (char* pBytes = str)
            {
                var pSrc = pBytes;
                byte* pBuffer = redisBuffer.GetAddressFromIndex(destinationIndex);

                var pDest = pBuffer;
                var start = sourceIndex;
                var end = Math.Min(start + count, str.Length);
                // I would liked to evict this for loop
                for (var i = sourceIndex; i < end; i++)
                {
                    *pDest = (byte)*pSrc;
                    pDest++;
                    pSrc++;
                }

            }
        }
    }

    public sealed class RedisCommands
    {
        private readonly RedisBuffer redisBuffer;

        public RedisCommands(RedisBuffer redisBuffer)
        {
            this.redisBuffer = redisBuffer;
        }

        private class NXBuffers
        {
            private static readonly Tuple<NXXX, byte[]> NX = new Tuple<NXXX, byte[]>(NXXX.NX, new[] { ASCIITable.N, ASCIITable.X });
            private static readonly Tuple<NXXX, byte[]> XX = new Tuple<NXXX, byte[]>(NXXX.XX, new[] { ASCIITable.X, ASCIITable.X });
            public static readonly Tuple<NXXX, byte[]>[] Values = { NX, XX };
        }

        private const byte CR = ASCIITable.CarriageReturn;
        private const byte LF = ASCIITable.LineFeed;

        private static readonly byte[] CRLF = { CR, LF };
        private static readonly int CRLFLength = CRLF.Length;
        private static readonly int NXXXLength = DATA_TYPE_BYTE_LENGTH + 1 + CRLFLength + 2 + CRLFLength;
        private const byte DATA_TYPE_BYTE_LENGTH = 1;

        //private static char TYPE_SIMPLE_STRINGS = '+';
        //private static char TYPE_ERRORS = '-';
        //private static char TYPE_INTEGERS = ':';
        private const byte TYPE_BULK_STRINGS = ASCIITable.DollarSign;
        private const byte TYPE_ARRAYS = ASCIITable.Asterisk;

        private static readonly byte[] LLENDescriptor = { TYPE_ARRAYS, ASCIITable.Two, CR, LF };
        private static readonly int LLENDescriptorLength = LLENDescriptor.Length;
        private static readonly byte[] CMD_LLEN = { ASCIITable.L, ASCIITable.L, ASCIITable.E, ASCIITable.N };

        private static readonly byte[] SETDescriptor = { TYPE_ARRAYS, ASCIITable.Null, CR, LF };
        private static readonly int SETDescriptorLength = SETDescriptor.Length;
        private static readonly byte[] CMD_SET = { ASCIITable.S, ASCIITable.E, ASCIITable.T };
        

        /// <summary>
        /// Returns the length of the list stored at key. If key does not exist, it is interpreted as an empty list and 0 is returned. An error is returned when the value stored at key is not a list.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Integer reply: the length of the list at key.</returns>
        //TODO: Perform conversion to integer value
        public RedisBuffer LLEN(string key)
        {
            redisBuffer.Length = LLENDescriptor.Length + CalcSizeOfBuffer(CMD_LLEN) + CalcSizeOfBuffer(key);
            redisBuffer.CopyFrom(LLENDescriptor, 0, 0, LLENDescriptorLength);
            var i = BulkString(redisBuffer, LLENDescriptorLength, CMD_LLEN);
            BulkString(redisBuffer, i, key);

            return redisBuffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public RedisBuffer SET(string key, string value)
        {
            return SET(key, Encoding.UTF8.GetBytes(value));
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">byte[] value to hold</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public RedisBuffer SET(string key, byte[] value)
        {
            const byte parameterCount = ASCIITable.Three;
            redisBuffer.Length = SETDescriptor.Length + CalcSizeOfBuffer(CMD_SET) + CalcSizeOfBuffer(key) + CalcSizeOfBuffer(value);

            redisBuffer.CopyFrom(SETDescriptor, 0, 0, SETDescriptorLength);
            redisBuffer.WriteByte(parameterCount, 1);

            var i = BulkString(redisBuffer, SETDescriptor.Length, iddisio.CMD_SET);
            i = BulkString(redisBuffer, i, key);
            BulkString(redisBuffer, i, value);

            return redisBuffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <param name="nxxx">NX -- Only set the key if it does not already exist. XX -- Only set the key if it already exist.</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public RedisBuffer SET(string key, string value, NXXX nxxx)
        {
            const byte parameterCount = ASCIITable.Four;

            redisBuffer.Length = SETDescriptor.Length + NXXXLength + CalcSizeOfBuffer(iddisio.CMD_SET, key, value);

            redisBuffer.CopyFrom(SETDescriptor, 0, 0, SETDescriptorLength);
            redisBuffer.WriteByte(parameterCount, 1);

            var i = BulkString(redisBuffer, SETDescriptor.Length, iddisio.CMD_SET);
            i = BulkString(redisBuffer, i, key);
            i = BulkString(redisBuffer, i, value);
            BulkString(redisBuffer, i, NXBuffers.Values[(int)nxxx].Item2);

            return redisBuffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <param name="secondsToExpire">Set the specified expire time, in seconds.</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public RedisBuffer SET(string key, string value, int secondsToExpire)
        {
            const byte parameterCount = ASCIITable.Four;

            redisBuffer.Length = SETDescriptor.Length + CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX) + CalcSizeOfBuffer(secondsToExpire);

            redisBuffer.CopyFrom(SETDescriptor, 0, 0, SETDescriptorLength);
            redisBuffer.WriteByte(parameterCount, 1);

            var i = BulkString(redisBuffer, SETDescriptor.Length, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            BulkString(redisBuffer, i, secondsToExpire);
            return redisBuffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <param name="secondsToExpire">Set the specified expire time, in seconds.</param>
        /// <param name="nxxx">NX -- Only set the key if it does not already exist. XX -- Only set the key if it already exist.</param>         
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public RedisBuffer SET(string key, string value, int secondsToExpire, NXXX nxxx)
        {
            const byte parameterCount = ASCIITable.Five;
            redisBuffer.Length = SETDescriptor.Length + CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX) + CalcSizeOfBuffer(secondsToExpire) + NXXXLength;


            redisBuffer.CopyFrom(SETDescriptor, 0, 0, SETDescriptorLength);
            redisBuffer.WriteByte(parameterCount, 1);


            var i = BulkString(redisBuffer, SETDescriptor.Length, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            i = BulkString(redisBuffer, i, secondsToExpire);
            BulkString(redisBuffer, i, NXBuffers.Values[(int)nxxx].Item2);

            return redisBuffer;
        }

        /// <summary>
        /// Set key to hold the string value. If key already holds a value, it is overwritten, regardless of its type. Any previous time to live associated with the key is discarded on successful SET operation.
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">String value to hold</param>
        /// <param name="secondsToExpire">Set the specified expire time, in seconds.</param>
        /// <param name="miliseconds">Set the specified expire time, in milliseconds.</param>
        /// <returns>Simple string reply: OK if SET was executed correctly. Null reply: a Null Bulk Reply is returned if the SET operation was not performed becase the user specified the NX or XX option but the condition was not met.</returns>
        public RedisBuffer SET(string key, string value, int secondsToExpire, int miliseconds)
        {
            const byte parameterCount = ASCIITable.Five;
            redisBuffer.Length = SETDescriptor.Length + CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, iddisio.PAR_SET_PX) + CalcSizeOfBuffer(secondsToExpire, miliseconds);

            redisBuffer.CopyFrom(SETDescriptor, 0, 0, SETDescriptorLength);
            redisBuffer.WriteByte(parameterCount, 1);

            var i = BulkString(redisBuffer, SETDescriptor.Length, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            i = BulkString(redisBuffer, i, secondsToExpire);
            i = BulkString(redisBuffer, i, iddisio.PAR_SET_PX);
            BulkString(redisBuffer, i, miliseconds);

            return redisBuffer;
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
        public RedisBuffer SET(string key, string value, int secondsToExpire, int miliseconds, NXXX nxxx)
        {
            const byte parameterCount = ASCIITable.Six;

            redisBuffer.Length = SETDescriptor.Length + CalcSizeOfBuffer(iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX, iddisio.PAR_SET_PX) + CalcSizeOfBuffer(secondsToExpire, miliseconds) + NXXXLength;
            redisBuffer.CopyFrom(SETDescriptor, 0, 0, SETDescriptorLength);
            redisBuffer.WriteByte(parameterCount, 1);

            var i = BulkString(redisBuffer, SETDescriptor.Length, iddisio.CMD_SET, key, value, iddisio.PAR_SET_EX);
            i = BulkString(redisBuffer, i, secondsToExpire);
            i = BulkString(redisBuffer, i, iddisio.PAR_SET_PX);
            i = BulkString(redisBuffer, i, miliseconds);
            BulkString(redisBuffer, i, NXBuffers.Values[(int)nxxx].Item2);

            return redisBuffer;
        }

        private static int BulkString(RedisBuffer redisBuffer, int i, params string[] words)
        {

            foreach (var word in words)
            {
                redisBuffer.WriteByte(TYPE_BULK_STRINGS, i++);
                var wordLength = word.Length;
                i += wordLength.FastToArrayByte(redisBuffer, i);
                redisBuffer.CopyFrom(CRLF, 0, i, CRLFLength);
                i += CRLFLength;
                word.ASCIICopyTo(0, redisBuffer, i, wordLength);
                i += wordLength;
                redisBuffer.CopyFrom(CRLF, 0, i, CRLFLength);
                i += CRLFLength;
            }

            return i;
        }

        private static int BulkString(RedisBuffer redisBuffer, int i, params byte[][] words)
        {
            foreach (var word in words)
            {
                redisBuffer.WriteByte(TYPE_BULK_STRINGS, i++);
                var wordLength = word.Length;
                i += wordLength.FastToArrayByte(redisBuffer, i);
                redisBuffer.CopyFrom(CRLF, 0, i, CRLFLength);
                i += CRLFLength;
                redisBuffer.CopyFrom(word, 0, i, wordLength);
                i += wordLength;
                redisBuffer.CopyFrom(CRLF, 0, i, CRLFLength);
                i += CRLFLength;
            }
            return i;
        }

        private static int BulkString(RedisBuffer redisBuffer, int i, params int[] words)
        {
            foreach (var word in words)
            {
                redisBuffer.WriteByte(TYPE_BULK_STRINGS, i);
                i++;
                i += word.DigitsCount().FastToArrayByte(redisBuffer, i);
                redisBuffer.CopyFrom(CRLF, 0, i, CRLFLength);
                i += CRLFLength;
                i += word.FastToArrayByte(redisBuffer, i);
                redisBuffer.CopyFrom(CRLF, 0, i, CRLFLength);
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