# RoaringBitmap for .NET
This is a fairly simple port of the Java bitmap library RoaringBitmap by Daniel Lemire et al.
You can find the original version here: https://github.com/lemire/RoaringBitmap
Most of the algorithms are ports of the original Java algorithms.
This is an early version, the test coverage is ok, but edge case tests are probably missing.
Target Framework is .NET 4.6

# Details
* Immutable data structure using readonly fields and private constructors, so it's thread-safe
* Overloaded operators for AND, OR, NOT and XOR
* Support for the Set Difference Operator using RoaringBitmap.AndNot

# NuGet
https://www.nuget.org/packages/RoaringBitmap/

# TODO
* Improve memory usage, especially NOT will put objects on the Large Object Heap

# How to use it?
Compile the RoaringBitmap.sln and use 'RoaringBitmap.Create' to create your bitmap, then use bitwise operations on it.
```csharp
var rb = RoaringBitmap.Create(1,2,3,4,5,7,8,9,10);
var rb2 = RoaringBitmap.Create(Enumerable.Range(10000,200000));
var rb3 = rb | rb2;
```

# Performance
As this is a fairly direct port of the Java library the performance is comparable, on average around 10-20% slower, probably because C# has no popcnt intrinsic and due to the Immutable datastructure overhead.
It also depends heavily on the system used, on a Intel XEON W3550 the .NET code was actually a tad faster.
The iterator is several times slower than its Java pendant, I do not yet know why.
The RoaringBitmap.Benchmark project contains microbenchmarks for the real-roaring-data set. This set is also used by the Unit Tests.


```ini

Host Process Environment Information:
BenchmarkDotNet.Core=v0.9.9.0
OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-4790K CPU 4.00GHz, ProcessorCount=8
Frequency=3906250 ticks, Resolution=256.0000 ns, Timer=TSC
CLR=MS.NET 4.0.30319.42000, Arch=64-bit RELEASE [RyuJIT]
GC=Concurrent Workstation
JitModules=clrjit-v4.6.1586.0

Type=MicroBenchmarkCensusIncome  Mode=Throughput  

```
  Method |     Median |    StdDev | Gen 0 | Gen 1 | Gen 2 | Bytes Allocated/Op |
-------- |----------- |---------- |------ |------ |------ |------------------- |
      Or |  2.2656 ms | 0.0079 ms | 94.15 |  1.68 |  3.30 |       2.708.059,64 |
     Xor |  2.4074 ms | 0.0085 ms | 84.24 |  1.79 |  3.50 |       2.457.820,69 |
     And |  2.2318 ms | 0.0033 ms | 26.30 |  1.72 |  3.36 |         840.223,48 |
  AndNot |  2.3633 ms | 0.0036 ms | 56.54 |  1.91 |  3.74 |       1.705.377,83 |
 Iterate | 88.0278 ms | 0.0888 ms | 33.00 | 49.00 | 96.00 |       6.331.163,29 |

