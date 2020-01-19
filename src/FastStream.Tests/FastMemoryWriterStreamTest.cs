using System.Linq;
using System.Text;
using Xunit;

namespace FastStream.Tests
{
    public class FastMemoryWriterStreamTest
    {
        [Fact]
        public void Read()
        {
            var text = "JonnoJ";
            var textBytes = Encoding.UTF8.GetBytes(text);

            var stream = new FastMemoryWriter();
            stream.Write(text);
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            var result = new byte[stream.Length];
            stream.Read(result, 0, result.Length);

            // skip string length
            Assert.True(textBytes.SequenceEqual(result.Skip(4)));
        }

        [Fact]
        public void ReadOnEmpty()
        {
            var text = "JonnoJ";

            var stream = new FastMemoryWriter();
            stream.Write(text);
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            var result = new byte[stream.Length];
            var read = stream.Read(result, 0, result.Length);
            Assert.Equal(result.Length, read);

            read = stream.Read(result, 0, result.Length);
            Assert.Equal(0, read);
        }


        [Fact]
        public void SeekToBegin()
        {
            var stream = new FastMemoryWriter();
            stream.Write("JonnoJ");
            stream.Seek(0, System.IO.SeekOrigin.Begin);

            Assert.Equal(0, stream.Position);
        }

        [Fact]
        public void TwoSeeks()
        {
            var stream = new FastMemoryWriter();
            stream.Write("JonnoJ");
            var initilPoisition = stream.Position;
            stream.Seek(-2, System.IO.SeekOrigin.Current);
            Assert.Equal(initilPoisition - 2, stream.Position);

            stream.Seek(-2, System.IO.SeekOrigin.Current);
            Assert.Equal(initilPoisition - 4, stream.Position);
            Assert.Equal(initilPoisition, stream.Length);
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

        [Fact]
        public void SetLength()
        {
            var stream = new FastMemoryWriter();
            stream.Write("JonnoJJonnoJJonnoJJonnoJ");

            var newLength = 10;
            stream.SetLength(newLength);

            Assert.Equal(newLength, stream.Length);
            Assert.Equal(newLength, stream.Position);
        }
    }
}
