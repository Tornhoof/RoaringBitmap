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

# TODO
* Improve memory usage, especially NOT will put objects on the Large Object Heap

# How to use it?
Compile the RoaringBitmap.sln and use 'RoaringBitmap.Create' to create your bitmap, then use bitwise operations on it.
```csharp
var rb = RoaringBitmap.Create(1,2,3,4,5,7,8,9,10);
var rb2 = RoaringBitmap.Create(Enumerable.Range(10000,200000));
var rb3 = rb | rb2;


# Performance
As this is a fairly direct port of the Java library the performance is comparable, on average around 10-20% slower, probably because C# has no popcnt intrinsic and due to the Immutable datastructure overhead.
It also depends heavily on the system used, on a Intel XEON W3550 the .NET code was actually a tad faster.
The iterator is several times slower than its Java pendant, I do not yet know why.
The RoaringBitmap.Benchmark project contains microbenchmarks for the real-roaring-data set. This set is also used by the Unit Tests.


```ini
BenchmarkDotNet=v0.7.8.0
OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-4790K CPU @ 4.00GHz, ProcessorCount=8
HostCLR=MS.NET 4.0.30319.42000, Arch=64-bit  [RyuJIT]
Type=MicroBenchmarkCensusIncome  Mode=Throughput  Platform=HostPlatform  Jit=HostJit  .NET=HostFramework
```

  Method |    AvrTime |    StdDev |   op/s |
-------- |----------- |---------- |------- |
     And |  2.2842 ms | 0.0125 ms | 437.79 |
  AndNot |  2.3849 ms | 0.0135 ms | 419.31 |
 Iterate | 89.8270 ms | 0.1454 ms |  11.13 |
      Or |  2.2951 ms | 0.0253 ms | 435.70 |
     Xor |  2.3010 ms | 0.0175 ms | 434.60 |
