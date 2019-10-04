using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FastStream
{
    public class FastMemoryWriter : Stream, IDisposable
    {
        private const int INITIAL_SIZE = 1024;

        private ArrayPool<byte> pool;
        private byte[] buffer;
        private long currentPosition;
        private long length;

        private int maxBufferIndex;

        public static ArrayPool<byte> HugeArrayPool { get; set; } = ArrayPool<byte>.Create(int.MaxValue, 10);

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length
            => this.length < this.currentPosition ?
                this.currentPosition :
                this.length;

        public override long Position
        {
            get => this.currentPosition;
            set
            {
                if (value < this.currentPosition && this.length < this.currentPosition)
                {
                    this.length = this.currentPosition;
                }
                this.currentPosition = value;
            }
        }


        public FastMemoryWriter()
            : this(HugeArrayPool, INITIAL_SIZE)
        {

        }

        public FastMemoryWriter(int initialSize)
            : this(HugeArrayPool, initialSize)
        {

        }

        public FastMemoryWriter(ArrayPool<byte> pool, int initialSize)
        {
            this.pool = pool;
            this.buffer = this.pool.Rent(initialSize);
            this.maxBufferIndex = this.buffer.Length - 1;
        }

        public byte[] Merge(FastMemoryWriter writer)
        {
            var bytes = new byte[this.currentPosition + writer.currentPosition];
            Buffer.BlockCopy(this.buffer, 0, bytes, 0, (int)this.currentPosition);
            Buffer.BlockCopy(writer.buffer, 0, bytes, (int)this.currentPosition, (int)writer.currentPosition);

            return bytes;
        }

        public byte[] ToArray()
        {
            var resultBuffer = new byte[this.currentPosition];
            Buffer.BlockCopy(this.buffer, 0, resultBuffer, 0, (int)this.currentPosition);

            return resultBuffer;
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
        public unsafe void Write(char value)
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
            this.Write(value, 0, value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(Span<byte> value)
        {
            var length = value.Length;
            this.EnshureCapacity(length);

            var destination = this.buffer.AsSpan().Slice((int)this.currentPosition);
            value.CopyTo(destination);
            this.currentPosition += length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(byte value)
        {
            this.EnshureCapacity(1);

            this.buffer[this.currentPosition] = value;
            this.currentPosition++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void EnshureCapacity(int count)
        {
            if (this.maxBufferIndex < this.currentPosition + count)
            {
                var newSize = (maxBufferIndex + count) * 2;
                this.SetLengthInternal(newSize);
            }
        }

        private unsafe void SetLengthInternal(int newSize)
        {
            var oldBuffer = this.buffer;
            var newBuffer = this.pool.Rent(newSize);

            var newPosition = Math.Min((int)currentPosition, newSize);
            Buffer.BlockCopy(oldBuffer, 0, newBuffer, 0, newPosition);
            this.pool.Return(oldBuffer);

            this.buffer = newBuffer;
            this.currentPosition = newPosition;
            this.maxBufferIndex = this.buffer.Length - 1;
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

        public override void Flush()
        {
        }

        public override int Read(byte[] inputBuffer, int offset, int count)
        {
            var dataLength = this.Length - this.Position;
            var toRead = (int)Math.Min(dataLength, count);

            Buffer.BlockCopy(this.buffer, (int)this.Position, inputBuffer, offset, toRead);

            this.Position += toRead;

            return toRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    this.Position = offset;
                    break;

                case SeekOrigin.Current:
                    this.Position += offset;
                    break;

                case SeekOrigin.End:
                    this.Position = this.Length + offset;
                    break;
            }

            return this.Position;
        }

        public override void SetLength(long value)
        {
                this.SetLengthInternal((int)value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.EnshureCapacity(count);

            Buffer.BlockCopy(buffer, offset, this.buffer, (int)this.currentPosition, count);
            this.currentPosition += count;
        }

        #region IDisposable Support

        private bool disposedValue = false; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    this.pool.Return(this.buffer);
                }

                this.buffer = null;
                disposedValue = true;
            }
        }
        #endregion IDisposable Support
    }

    public class FastReader : BinaryReader
    {
        public FastReader(Stream input)
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