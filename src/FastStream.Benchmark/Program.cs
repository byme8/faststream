using System.Buffers;
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
            BenchmarkRunner.Run<TypesFormattingBenchmark>();
            //BenchmarkRunner.Run<FastStreamVSMemoryStream>();
            //BenchmarkRunner.Run<FastStreamVSMemoryStreamWithBinaryWriter>();
        }
    }

    public class TypesFormattingBenchmark
    {
        [Benchmark]
        public void UShort()
        {
            const int count = 10_000;
            using (var stream = new FastMemoryWriter(count * 5))
            {
                for (int i = 0; i < 10_000; i++)
                {
                    stream.Write((ushort)257);
                }
            }
        }

        [Benchmark]
        public void UInteger()
        {
            const int count = 10_000;
            using (var stream = new FastMemoryWriter(count * 5))
            {
                for (int i = 0; i < 10_000; i++)
                {
                    stream.Write((uint)257);
                }
            }
        }
    }

    [MemoryDiagnoser]
    public class FastStreamVSMemoryStreamWithBinaryWriter
    {
        private RecyclableMemoryStreamManager manager;
        private ArrayPool<byte> hugeArrayPool;

        [Params(10, 100, 1000)]
        public int ItemCount { get; set; }

        public FastStreamVSMemoryStreamWithBinaryWriter()
        {
            this.manager = new RecyclableMemoryStreamManager();
            this.hugeArrayPool = ArrayPool<byte>.Create(int.MaxValue, 10);
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
                using (var memory = new FastMemoryWriter(this.hugeArrayPool))
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
                using (var memory = new FastMemoryWriter(this.hugeArrayPool))
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
        private ArrayPool<byte> hugeArrayPool;

        [Params(100_000)]
        public int Size { get; set; }

        [Params(1000)]
        public int ItemCount { get; set; }

        public FastStreamVSMemoryStream()
        {
            this.hugeArrayPool = ArrayPool<byte>.Create(int.MaxValue, 10);
        }


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
                using (var memory = new FastMemoryWriter(this.hugeArrayPool))
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
