using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.IO;

namespace FastStream.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            //BenchmarkRunner.Run<FastStreamVSMemoryStream>();
            BenchmarkRunner.Run<FastStreamVSMemoryStreamWithBinaryWriter>();
        }
    }

    [MemoryDiagnoser]
    public class FastStreamVSMemoryStreamWithBinaryWriter
    {
        private RecyclableMemoryStreamManager manager;

        [Params(10, 100, 1000)]
        public int ItemCount { get; set; }

        public FastStreamVSMemoryStreamWithBinaryWriter()
        {
            this.manager = new RecyclableMemoryStreamManager();
        }

        [Benchmark]
        public void MemoryStreamAndBinaryWriter()
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

                    var bytes = memory.ToArray();
                }
            }
        }

        [Benchmark]
        public void RecyclableBytes()
        {
            for (int j = 0; j < 10; j++)
            {
                using (var memory = this.manager.GetStream())
                using (var writer = new BinaryWriter(memory))
                {
                    for (int i = 0; i < this.ItemCount; i++)
                    {
                        writer.Write(1.0);
                    }

                    var bytes = memory.ToArray();
                }
            }
        }

        [Benchmark]
        public void FastMemoryWriterAndBinaryWriter()
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

                    var bytes = memory.ToArray();
                }
            }
        }

        [Benchmark]
        public void OnlyFastMemoryWriter()
        {
            for (int j = 0; j < 10; j++)
            {
                using (var memory = new FastMemoryWriter())
                {
                    for (int i = 0; i < this.ItemCount; i++)
                    {
                        memory.Write(1.0);
                    }

                    var bytes = memory.ToArray();
                }
            }
        }
    }

    [MemoryDiagnoser]
    public class FastStreamVSMemoryStream
    {
        [Params(100, 100_000)]
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

                    var bytes = memory.ToArray();
                }
            }
        }


        [Benchmark]
        public void RecyclableBytes()
        {
            var data = new byte[this.Size];

            var manager = new RecyclableMemoryStreamManager();
            for (int j = 0; j < 10; j++)
            {
                using (var memory = manager.GetStream())
                {
                    for (int i = 0; i < this.ItemCount; i++)
                    {
                        memory.Write(data, 0, data.Length);
                    }

                    var bytes = memory.ToArray();
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

                    var bytes = memory.ToArray();
                }
            }
        }
    }
}
