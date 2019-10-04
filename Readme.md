The goal of the project to make faster MemoryStream powered by new dotnet core APIs. 
The main power gain happened because classic allocations is replaced by ArrayPool.

[![Build status](https://ci.appveyor.com/api/projects/status/mxi7d4xhe7afjpw0?svg=true)](https://ci.appveyor.com/project/byme8/faststream)


### Nuget
You can install it via nuget. Search for ``` FastStream ```.

### Writer
You can use  ``` FastMemoryWriter ``` like that:

``` cs

using (var memory = new FastMemoryWriter())
using (var writer = new BinaryWriter(memory))
{
    for (int i = 0; i < this.ItemCount; i++)
    {
        writer.Write(1.0);
    }
}

```

but because ``` FastMemoryWriter ``` implements interfaces of ``` BinaryWriter ``` you can use it like that:

``` cs

using (var memory = new FastMemoryWriter())
{
    for (int i = 0; i < this.ItemCount; i++)
    {
        memory.Write(1.0);
    }
}

```

Here is benchmark results:

|                          Method | ItemCount |       Mean |     Error |    StdDev |   Gen 0 | Gen 1 | Gen 2 | Allocated |
|-------------------------------- |---------- |-----------:|----------:|----------:|--------:|------:|------:|----------:|
|     MemoryStreamAndBinaryWriter |        10 |   2.632 us | 0.0116 us | 0.0109 us |  1.4648 |     - |     - |   6.02 KB |
| FastMemoryWriterAndBinaryWriter |        10 |   2.411 us | 0.0347 us | 0.0341 us |  0.8011 |     - |     - |   3.28 KB |
|            OnlyFastMemoryWriter |        10 |   1.121 us | 0.0061 us | 0.0054 us |  0.4177 |     - |     - |   1.72 KB |
|     MemoryStreamAndBinaryWriter |       100 |  18.407 us | 0.2236 us | 0.1867 us |  6.9580 |     - |     - |  28.52 KB |
| FastMemoryWriterAndBinaryWriter |       100 |  14.175 us | 0.1982 us | 0.1757 us |  2.5024 |     - |     - |  10.31 KB |
|            OnlyFastMemoryWriter |       100 |   5.205 us | 0.0373 us | 0.0311 us |  2.1286 |     - |     - |   8.75 KB |
|     MemoryStreamAndBinaryWriter |      1000 | 166.776 us | 0.7487 us | 0.6637 us | 58.3496 |     - |     - | 239.53 KB |
| FastMemoryWriterAndBinaryWriter |      1000 | 127.801 us | 1.2563 us | 1.1751 us | 19.5313 |     - |     - |  80.63 KB |
|            OnlyFastMemoryWriter |      1000 |  49.242 us | 0.2441 us | 0.2284 us | 19.2261 |     - |     - |  79.06 KB |

As you can see in combination ``` BinaryWriter ``` and ``` FastMemoryWriter ``` you can gain up to ~25% better perfomance.
If you replace ``` BinaryWriter ``` by ``` FastMemoryWriter ``` you can gain up to ~70% better perfomance.

Also it's good to mention there is noticeable difference in memory allocation in favor of ``` FastMemoryWriter ```.

### Reader
To read data you can use ``` FastReader ```. Basically it's a BinaryReader with minor changes inside:
```
using (var memory = new FastReader())
{
    for (int i = 0; i < this.ItemCount; i++)
    {
        var value = memory.ReadDouble();
    }
}

```
