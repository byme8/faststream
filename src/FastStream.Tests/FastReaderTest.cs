using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using Xunit;
using System.Threading.Tasks;
using System.IO;

namespace FastStream.Tests
{
    public class FastReaderTest
    {
        [Fact]
        public void ReadString()
        {
            var text = "hello";
            var memory = new FastMemoryWriter();

            memory.Write(text);
            memory.Seek(0, System.IO.SeekOrigin.Begin);

            var reader = new FastReader(memory);
            var result = reader.ReadString();

            Assert.Equal(text, result);
        }

        [Fact]
        public void ReadByteArray()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, };
            var memory = new FastMemoryWriter();

            memory.WriteByteArray(bytes);
            memory.Seek(0, SeekOrigin.Begin);

            var reader = new FastReader(memory);
            var result = reader.ReadByteArray();

            Assert.True(result.SequenceEqual(bytes));
        }


        [Fact]
        public void ReadWrongByteArray()
        {
            var bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, };
            var memory = new FastMemoryWriter();

            memory.WriteByteArray(bytes);
            memory.SetLength(9);
            memory.Seek(0, SeekOrigin.Begin);

            var reader = new FastReader(memory);
            
            Assert.Throws<EndOfStreamException>(() => reader.ReadByteArray());
        }
    }
}
