using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace FastStream
{
    public class FastMemoryWriter : Stream
    {
        private const int InitialSize = 1024;

        private readonly ArrayPool<byte> _pool;
        private byte[] _buffer;
        private long _currentPosition;
        private long _length;

        private int _maxBufferIndex;

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length
            => _length < _currentPosition ?
                _currentPosition :
                _length;

        public override long Position
        {
            get => _currentPosition;
            set
            {
                if (value < _currentPosition && _length < _currentPosition)
                {
                    _length = _currentPosition;
                }
                _currentPosition = value;
            }
        }


        public FastMemoryWriter()
             : this(ArrayPool<byte>.Shared)
        {

        }

        public FastMemoryWriter(int initialSize)
            : this(ArrayPool<byte>.Shared, initialSize)
        {

        }

        public FastMemoryWriter(ArrayPool<byte> pool, int initialSize = InitialSize)
        {
            _pool = pool;
            _buffer = _pool.Rent(initialSize);
            _maxBufferIndex = _buffer.Length - 1;
        }

        public byte[] Merge(FastMemoryWriter writer)
        {
            var bytes = new byte[_currentPosition + writer._currentPosition];
            Buffer.BlockCopy(_buffer, 0, bytes, 0, (int)_currentPosition);
            Buffer.BlockCopy(writer._buffer, 0, bytes, (int)_currentPosition, (int)writer._currentPosition);

            return bytes;
        }

        public byte[] ToArray()
        {
            var resultBuffer = new byte[_currentPosition];
            Buffer.BlockCopy(_buffer, 0, resultBuffer, 0, (int)_currentPosition);

            return resultBuffer;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(string value)
        {
            Write(value.Length);
            var bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(bool value)
        {
            Write(value ? (byte)1 : (byte)0);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(short value)
        {
            EnsureCapacity(2);

            unchecked
            {
                _buffer[_currentPosition] = (byte)value;
                _buffer[_currentPosition + 1] = (byte)(value >> 8);
            }

            _currentPosition += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(ushort value)
        {
            EnsureCapacity(2);

            unchecked
            {
                _buffer[_currentPosition] = (byte)value;
                _buffer[_currentPosition + 1] = (byte)(value >> 8);
            }

            _currentPosition += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(char value)
        {
            EnsureCapacity(2);

            unchecked
            {
                _buffer[_currentPosition] = (byte)value;
                _buffer[_currentPosition + 1] = (byte)(value >> 8);
            }

            _currentPosition += 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(int value)
        {
            EnsureCapacity(4);

            unchecked
            {
                _buffer[_currentPosition] = (byte)value;
                _buffer[_currentPosition + 1] = (byte)(value >> 8);
                _buffer[_currentPosition + 2] = (byte)(value >> 16);
                _buffer[_currentPosition + 3] = (byte)(value >> 24);
            }

            _currentPosition += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(uint value)
        {
            EnsureCapacity(4);

            unchecked
            {
                _buffer[_currentPosition] = (byte)value;
                _buffer[_currentPosition + 1] = (byte)(value >> 8);
                _buffer[_currentPosition + 2] = (byte)(value >> 16);
                _buffer[_currentPosition + 3] = (byte)(value >> 24);
            }

            _currentPosition += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(long value)
        {
            Write8Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(ulong value)
        {
            Write8Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(float value)
        {
            Write4BytesFromPointer((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void Write(double value)
        {
            Write8Bytes((byte*)&value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte[] value)
        {
            Write(value, 0, value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WriteByteArray(byte[] value)
        {
            Write(value.Length);
            Write(value, 0, value.Length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Write(byte value)
        {
            EnsureCapacity(1);

            _buffer[_currentPosition] = value;
            _currentPosition++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureCapacity(int count)
        {
            if (_maxBufferIndex < _currentPosition + count)
            {
                var newSize = (_maxBufferIndex + count) * 2;
                SetLengthInternal(newSize);
            }
        }

        private void SetLengthInternal(int newSize)
        {
            var oldBuffer = _buffer;
            var newBuffer = _pool.Rent(newSize);

            var newPosition = Math.Min((int)_currentPosition, newSize);
            Buffer.BlockCopy(oldBuffer, 0, newBuffer, 0, newPosition);
            _pool.Return(oldBuffer);

            _buffer = newBuffer;
            _currentPosition = newPosition;
            _maxBufferIndex = _buffer.Length - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Write4BytesFromPointer(byte* p)
        {
            EnsureCapacity(4);

            _buffer[_currentPosition] = *p;
            _buffer[_currentPosition + 1] = *(p + 1);
            _buffer[_currentPosition + 2] = *(p + 2);
            _buffer[_currentPosition + 3] = *(p + 3);

            _currentPosition += 4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe void Write8Bytes(byte* p)
        {
            EnsureCapacity(8);
            _buffer[_currentPosition] = *p;
            _buffer[_currentPosition + 1] = *(p + 1);
            _buffer[_currentPosition + 2] = *(p + 2);
            _buffer[_currentPosition + 3] = *(p + 3);
            _buffer[_currentPosition + 4] = *(p + 4);
            _buffer[_currentPosition + 5] = *(p + 5);
            _buffer[_currentPosition + 6] = *(p + 6);
            _buffer[_currentPosition + 7] = *(p + 7);

            _currentPosition += 8;
        }

        public override void Flush()
        {
        }
        
        public void WriteToSteam(Stream stream)
        {
            stream.Write(_buffer, 0, (int)_currentPosition);
        }

        public override int Read(byte[] inputBuffer, int offset, int count)
        {
            var dataLength = Length - Position;
            var toRead = (int)Math.Min(dataLength, count);

            Buffer.BlockCopy(_buffer, (int)Position, inputBuffer, offset, toRead);

            Position += toRead;

            return toRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }

            return Position;
        }

        public override void SetLength(long value)
        {
                SetLengthInternal((int)value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            EnsureCapacity(count);

            Buffer.BlockCopy(buffer, offset, _buffer, (int)_currentPosition, count);
            _currentPosition += count;
        }

        #region IDisposable Support

        private bool _disposedValue; // To detect redundant calls

        protected override void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _pool.Return(_buffer);
                }

                _buffer = null;
                _disposedValue = true;
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

        public byte[] ReadByteArray()
        {
            var length = ReadInt32();
            var bytes = new byte[length];
            var position = 0;
            do
            {
                var read = Read(bytes, position, length - position);
                if (read == 0)
                {
                    throw new EndOfStreamException();
                }
                position += read;
            } while (position != length);
            
            return bytes;
        }

        public override string ReadString()
        {
            var size = ReadInt32();
            var bytes = new byte[size];
            Read(bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}