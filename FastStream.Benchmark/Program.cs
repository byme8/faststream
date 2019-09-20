using System;
using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace FastStream.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<FastStreamVSMemoryStream>();
            //BenchmarkRunner.Run<FastStreamVSMemoryStreamWithBinaryWriter>();
        }
    }

    public class FastStreamVSMemoryStreamWithBinaryWriter
    {
        [Params(10, 100, 1000)]
        public int ItemCount { get; set; }

        [Benchmark]
        public void BytesBinaryWriter()
        {
            for (int j = 0; j < 10; j++)
            {
                using (var memory = new MemoryStream())
                using (var writer = new BinaryWriter(memory))
                {
                    for (int i = 0; i < this.ItemCount; i++)
                    {
                        writer.Write(1.0);
                    }
                }
            }
        }

        [Benchmark]
        public void FastBytesBinaryWriter()
        {
            for (int j = 0; j < 10; j++)
            {
                using (var memory = new FastMemoryWriter())
                using (var writer = new BinaryWriter(memory))
                {
                    for (int i = 0; i < this.ItemCount; i++)
                    {
                        writer.Write(1.0);
                    }
                }
            }
        }

        [Benchmark]
        public void FastBytes()
        {
            for (int j = 0; j < 10; j++)
            {
                using (var memory = new FastMemoryWriter())
                {
                    for (int i = 0; i < this.ItemCount; i++)
                    {
                        memory.Write(1.0);
                    }
                }
            }
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
                        memory.Write(data, 0, data.Length);
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
                        memory.Write(data, 0, data.Length);
                    }
                }
            }
        }
    }
}
