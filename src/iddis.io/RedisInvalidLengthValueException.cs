using System;

namespace iddis.io
{
    [Serializable]
    public class RedisInvalidLengthValueException : Exception
    {
        public RedisInvalidLengthValueException(int lengthValue, int capacityOfRedisBuffer) : base(string.Format("Invalid length value {0} to a RedisBuffer with {1} capacity", lengthValue, capacityOfRedisBuffer))
        {
        }
    }
}