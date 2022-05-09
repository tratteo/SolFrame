using System;
using System.Collections.Generic;

namespace dotnetstandard_bip39
{
    public class BigEndianBuffer
    {
        private readonly List<byte> bytes = new List<byte>();

        public void WriteUInt(uint i)
        {
            bytes.Add((byte)((i >> 0x18) & 0xff));
            bytes.Add((byte)((i >> 0x10) & 0xff));
            bytes.Add((byte)((i >> 8) & 0xff));
            bytes.Add((byte)(i & 0xff));
        }

        public void Write(byte b) => bytes.Add(b);

        public void Write(byte[] bytes) => Write(bytes, 0, bytes.Length);

        public void Write(byte[] bytes, int offset, int count)
        {
            var newBytes = new byte[count];
            Array.Copy(bytes, offset, newBytes, 0, count);

            this.bytes.AddRange(newBytes);
        }

        public byte[] ToArray() => bytes.ToArray();
    }
}