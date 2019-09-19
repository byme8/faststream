using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastStream
{
    public class FastMemoryWriter : IDisposable
    {
        private const int INITIAL_SIZE = 1024;

        private int maxBufferIndex = INITIAL_SIZE - 1;
        private byte[] buffer = ArrayPool<byte>.Shared.Rent(INITIAL_SIZE);
        private int currentPosition;

        private void EnshureCapacity(int count)
        {
            if (this.maxBufferIndex < this.currentPosition + count)
            {
                var oldBuffer = this.buffer;
                this.buffer = ArrayPool<byte>.Shared.Rent(maxBufferIndex * 2);
                Buffer.BlockCopy(oldBuffer, 0, this.buffer, 0, oldBuffer.Length);
                ArrayPool<byte>.Shared.Return(oldBuffer);

                this.maxBufferIndex = this.buffer.Length - 1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
        {
            this.Write(value.Length);
            var bytes = Encoding.UTF8.GetBytes(value);
            this.Write(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            this.Write(value ? (byte)1 : (byte)0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(short value)
        {
            this.Write2Bytes((byte*)&value);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(int value)
        {
            this.Write4Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(long value)
        {
            this.Write8Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Write8Bytes(byte* p)
        {
            this.EnshureCapacity(8);
            this.buffer[this.currentPosition] = *p;
            this.buffer[this.currentPosition + 1] = *(p + 1);
            this.buffer[this.currentPosition + 2] = *(p + 2);
            this.buffer[this.currentPosition + 3] = *(p + 3);
            this.buffer[this.currentPosition + 4] = *(p + 4);
            this.buffer[this.currentPosition + 5] = *(p + 5);
            this.buffer[this.currentPosition + 6] = *(p + 6);
            this.buffer[this.currentPosition + 7] = *(p + 7);

            this.currentPosition += 8;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Write2Bytes(byte* p)
        {
            this.EnshureCapacity(2);

            this.buffer[this.currentPosition] = *p;
            this.buffer[this.currentPosition + 1] = *(p + 1);

            this.currentPosition += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Write4Bytes(byte* p)
        {
            this.EnshureCapacity(4);

            this.buffer[this.currentPosition] = *p;
            this.buffer[this.currentPosition + 1] = *(p + 1);
            this.buffer[this.currentPosition + 2] = *(p + 2);
            this.buffer[this.currentPosition + 3] = *(p + 3);

            this.currentPosition += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(float value)
        {
            this.Write4Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(double value)
        {
            this.Write8Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] value)
        {
            var length = value.Length;
            this.EnshureCapacity(length);

            Buffer.BlockCopy(value, 0, this.buffer, this.currentPosition, value.Length);
            this.currentPosition += length;
        }

        public byte[] Merge(FastMemoryWriter writer)
        {
            var bytes = new byte[this.currentPosition + writer.currentPosition];
            Buffer.BlockCopy(this.buffer, 0, bytes, 0, this.currentPosition);
            Buffer.BlockCopy(writer.buffer, 0, bytes, this.currentPosition, writer.currentPosition);

            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(byte value)
        {
            this.EnshureCapacity(1);

            this.buffer[this.currentPosition] = value;
            this.currentPosition++;
        }

        public byte[] ToArray()
        {
            var resultBuffer = new byte[this.currentPosition];
            Buffer.BlockCopy(this.buffer, 0, resultBuffer, 0, this.currentPosition);

            return resultBuffer;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ArrayPool<byte>.Shared.Return(this.buffer);
                }

                this.buffer = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }

    public class SchemaPackReader : BinaryReader
    {
        public SchemaPackReader(Stream input)
            : base(input, Encoding.UTF8, true)
        {
        }

        public override string ReadString()
        {
            var size = this.ReadInt32();
            var bytes = new byte[size];
            this.Read(bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}
