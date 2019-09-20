using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xunit;

namespace FastStream.Tests
{
    public class FastMemoryWriterStreamTest
    {

        [Fact]
        public void SeekToBegin()
        {
            var stream = new FastMemoryWriter();
            stream.Write("JonnoJ");
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            Assert.Equal(0, stream.Position);
        }

        [Fact]
        public void SeekToEnd()
        {
            var stream = new FastMemoryWriter();
            stream.Write("JonnoJ");

            var result = stream.Position;
            stream.Seek(0, System.IO.SeekOrigin.Begin);
            stream.Seek(0, System.IO.SeekOrigin.End);

            Assert.Equal(result, stream.Position);
        }

        [Fact]
        public void SeekWithCurrent()
        {
            var stream = new FastMemoryWriter();
            stream.Write("JonnoJ");
            var result = stream.Position;

            stream.Seek(-1, System.IO.SeekOrigin.Current);

            Assert.Equal(result - 1, stream.Position);
        }
    }
}
