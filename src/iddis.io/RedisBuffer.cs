﻿using System;
using System.Text;

namespace iddis.io
{
    public class RedisBuffer
    {
        public readonly byte[] buffer;

        public RedisBuffer(int size)
        {
            buffer = new byte[size];
        }

        /// <summary>
        /// Copies length elements from sourceArray, starting at sourceIndex, to ProtocolBuffer, starting at destinationIndex.
        /// </summary>
        /// <param name="sourceArray">The <see cref="T:System.Array"/> that contains the data to copy.</param>
        /// <param name="sourceIndex">A 32-bit integer that represents the index in the <paramref name="sourceArray"/> at which copying begins.</param>
        /// <param name="destinationIndex">A 32-bit integer that represents the index in the ProtocolBuffer at which storing begins.</param>
        /// <param name="length">A 32-bit integer that represents the number of elements to copy.</param>
        /// <exception cref="T:System.ArgumentNullException"><paramref name="sourceArray"/> is null.</exception>
        /// <exception cref="T:System.RankException"><paramref name="sourceArray"/> have different ranks different of 1</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="sourceIndex"/> is less than the lower bound of the first dimension of <paramref name="sourceArray"/>.-or-<paramref name="destinationIndex"/> is less than Zero.-or-<paramref name="length"/> is less than zero.</exception>
        /// <exception cref="T:System.ArgumentException"><paramref name="length"/> is greater than the number of elements from <paramref name="sourceIndex"/> to the end of <paramref name="sourceArray"/>.-or-<paramref name="length"/> is greater than the number of elements from <paramref name="destinationIndex"/> to greather than of size of ProtocolBuffer/>.</exception>
        internal void CopyFrom(byte[] sourceArray, int sourceIndex, int destinationIndex, int length)
        {
            Array.Copy(sourceArray, sourceIndex, buffer, destinationIndex, length);
        }

        /// <summary>
        /// Write a byte value at index.
        /// </summary>
        /// <param name="value">Byte value to be write.</param>
        /// <param name="index">A 32-bit integer that represents the index in the Protocol buffer at which will be write a byte value.</param>
        internal void WriteByte(byte value, int index)
        {
            buffer[index] = value;
        }


        /// <summary>
        /// Get a address inside of RedisBuffer of the contiguous byte array.
        /// </summary>
        /// <param name="index">A 32-bit integer that represents the index in the Protocol buffer to get the address.</param>
        /// <returns>A byte* of at the address at index position.</returns>
        internal unsafe byte* GetAddressFromIndex(int index)
        {
            fixed (byte* addressFromIndex = &buffer[index])
            {
                return addressFromIndex;
            }
        }

        /// <summary>
        /// Get a .NET String from ascii RedisBuffer
        /// </summary>
        /// <returns>A string value of RedisBuffer</returns>
        public string ASCIIToString()
        {
            return Encoding.ASCII.GetString(buffer);
        }


        /// <summary>
        /// Get an array byte that is the raw buffer from RedisBuffer
        /// </summary>
        /// <returns>An array byte with the RedisBuffer</returns>
        public byte[] GetRaw()
        {
            return buffer;
        }
    }
}