using System;
using System.Linq;
using System.Text;
using Xunit;

namespace FastStream.Tests
{
    public class FastMemoryWriterTest
    {
        [Fact]
        public void Empty()
        {
            var writer = new FastMemoryWriter();
            var result = writer.ToArray();

            var empty = new byte[0];
            Assert.True(empty.SequenceEqual(result));
        }

        [Fact]
        public void Int16()
        {
            short digit = 10;

            var writer = new FastMemoryWriter();
            writer.Write(digit);
            var result = writer.ToArray();

            var value = BitConverter.GetBytes(digit);
            Assert.True(value.SequenceEqual(result));
        }

        [Fact]
        public void Int32()
        {
            int digit = 10;

            var writer = new FastMemoryWriter();
            writer.Write(digit);
            var result = writer.ToArray();

            var value = BitConverter.GetBytes(digit);
            Assert.True(value.SequenceEqual(result));
        }

        [Fact]
        public void Int64()
        {
            long digit = 10;

            var writer = new FastMemoryWriter();
            writer.Write(digit);
            var result = writer.ToArray();

            var value = BitConverter.GetBytes(digit);
            Assert.True(value.SequenceEqual(result));
        }

        [Fact]
        public void Float32()
        {
            float digit = 10;

            var writer = new FastMemoryWriter();
            writer.Write(digit);
            var result = writer.ToArray();

            var value = BitConverter.GetBytes(digit);
            Assert.True(value.SequenceEqual(result));
        }

        [Fact]
        public void Float64()
        {
            double digit = 10;

            var writer = new FastMemoryWriter();
            writer.Write(digit);
            var result = writer.ToArray();

            var value = BitConverter.GetBytes(digit);
            Assert.True(value.SequenceEqual(result));
        }

        [Fact]
        public void String()
        {
            var text = "10";

            var writer = new FastMemoryWriter();
            writer.Write(text);
            var result = writer.ToArray();

            var bytes = BitConverter.GetBytes(text.Length);
                bytes = bytes.Concat(Encoding.UTF8.GetBytes(text)).ToArray();

            Assert.True(bytes.SequenceEqual(result));
        }

        [Fact]
        public void Bytes()
        {
            var bytes = new byte[] { 10, 10, 11, 12 };

            var writer = new FastMemoryWriter();
            writer.Write(bytes);
            var result = writer.ToArray();

            Assert.True(bytes.SequenceEqual(result));
        }
    }
}
