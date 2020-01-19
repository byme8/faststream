using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FastStream.Tests
{
    public class FastMemoryWriterTest
    {
        public class Writers
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
            public void Byte()
            {
                byte digit = 10;

                var writer = new FastMemoryWriter();
                writer.Write(digit);
                var result = writer.ToArray();

                Assert.True(result.First() == digit);
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
            public void UInt16()
            {
                ushort digit = 10;

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
            public void UInt32()
            {
                uint digit = 10;

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
            public void UInt64()
            {
                ulong digit = 10;

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
                var text = "lorem te ipsum";

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
                var bytes = new byte[] {10, 10, 11, 12};

                var writer = new FastMemoryWriter();
                writer.Write(bytes);
                writer.Seek(0, SeekOrigin.Begin);

                var result = new byte[4];
                writer.Read(result);

                Assert.True(bytes.SequenceEqual(result));
            }

            [Fact]
            public void Bytesv2()
            {
                var bytes = new byte[] {10, 10, 11, 12};

                var writer = new FastMemoryWriter();
                writer.Write(bytes, 0, bytes.Length);
                var result = writer.ToArray();

                Assert.True(bytes.SequenceEqual(result));
            }

            [Fact]
            public void Bool()
            {
                var writer = new FastMemoryWriter();
                writer.Write(true);
                writer.Write(false);
                var result = writer.ToArray();

                Assert.True(result[0] == 1);
                Assert.True(result[1] == 0);
            }

            [Fact]
            public void Char()
            {
                var writer = new FastMemoryWriter();
                writer.Write('a');
                var result = writer.ToArray();

                Assert.True(result[0] == (byte) 'a');
            }

            [Fact]
            public void Merge()
            {
                var bytes = new byte[] {10, 10};

                var writer1 = new FastMemoryWriter();
                writer1.Write(bytes);
                
                var writer2 = new FastMemoryWriter();
                var result = writer2.Merge(writer1);

                Assert.Equal(bytes, result);
            }
            
            [Fact]
            public void CopyTo()
            {
                var bytes = new byte[] {10, 10};
             
                var writer = new FastMemoryWriter();
                writer.Write(bytes);
                writer.Seek(0, SeekOrigin.Begin);
                
                var stream = new MemoryStream();
                writer.CopyTo(stream);

                var result = stream.ToArray();
                Assert.Equal(bytes, result);
            }
            
            [Fact]
            public async Task CopyToAsync()
            {
                var bytes = new byte[] {10, 10};
             
                var writer = new FastMemoryWriter();
                writer.Write(bytes);
                writer.Seek(0, SeekOrigin.Begin);
                             
                var stream = new MemoryStream();
                await writer.CopyToAsync(stream);

                var result = stream.ToArray();
                Assert.Equal(bytes, result);
            }
            
            [Fact]
            public void WriteToStream()
            {
                var bytes = new byte[] {10, 10};
             
                var writer = new FastMemoryWriter();
                writer.Write(bytes);
                             
                var stream = new MemoryStream();
                writer.WriteToSteam(stream);

                var result = stream.ToArray();
                Assert.Equal(bytes, result);
            }
        }

        public class ChunkSize
        {
            [Theory]
            [InlineData(10)]
            [InlineData(10_000)]
            [InlineData(1_000_000)]
            public void AbleToProcess(int size)
            {
                var count = 100;
                var bytes = new byte[size];

                using (var writer = new FastMemoryWriter())
                {
                    for (int i = 0; i < count; i++)
                    {
                        writer.Write(bytes);
                    }

                    var result = writer.ToArray();
                    Assert.True(Enumerable.Repeat(bytes, count).SelectMany(o => o).SequenceEqual(result));
                }
            }
        }
    }
}