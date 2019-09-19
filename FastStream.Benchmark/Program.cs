using System;
using System.Buffers;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace FastStream.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //var a = new FastStreamVSMemoryStream() { Size = 1_000_000 };
            //a.Bytes();
            //a.FastBytes();

            BenchmarkRunner.Run<FastStreamVSMemoryStream>();
        }
    }

    [MemoryDiagnoser]
    public class FastStreamVSMemoryStream
    {
        [Params(/*10, 10_000, */1_000_000)]
        public int Size { get; set; }


        [Benchmark]
        public void Bytes()
        {
            var data = new byte[this.Size];
            using (var memory = new MemoryStream())
            {
                for (int i = 0; i < 100; i++)
                {
                    memory.Write(data);
                }
            }

        }

        [Benchmark]
        public void FastBytes()
        {
            var data = new byte[this.Size];
            using (var memory = new FastMemoryWriter())
            {
                for (int i = 0; i < 100; i++)
                {
                    memory.Write(data);
                }
            }
        }
    }
}
