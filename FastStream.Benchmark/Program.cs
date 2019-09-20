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
        [Params(10, 10_000, 500_000)]
        public int Size { get; set; }

        [Params(10, 100, 1000)]
        public int ItemCount { get; set; }


        [Benchmark]
        public void Bytes()
        {
            var data = new byte[this.Size];
            for (int j = 0; j < 10; j++)
            {
                using (var memory = new MemoryStream())
                {
                    for (int i = 0; i < this.ItemCount; i++)
                    {
                        memory.Write(data);
                    }
                }
            }
        }

        [Benchmark]
        public void FastBytes()
        {
            var data = new byte[this.Size];
            for (int j = 0; j < 10; j++)
            {
                using (var memory = new FastMemoryWriter())
                {
                    for (int i = 0; i < this.ItemCount; i++)
                    {
                        memory.Write(data);
                    }
                }
            }
        }
    }
}
