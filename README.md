# RoaringBitmap for .NET
This is a fairly simple port of the Java bitmap library RoaringBitmap by Daniel Lemire et al.
You can find the original version here: https://github.com/lemire/RoaringBitmap
Most of the algorithms are ports of the original Java algorithms.
This is an early version, the test coverage is ok, but edge case tests are probably missing.
Target Framework is .NET 4.6

# What is in it?

* Immutable data structure using readonly fields and private constructors
* overloaded operators for AND, OR, NOT and XOR
* No RLE at the moment

# How to use it?
Compile the RoaringBitmap.sln and use 'RoaringBitmap.Create' to create your bitmap, then use bitwise operations on it. See the Unit Tests for examples.


# Performance
As this is a fairly direct port of the Java library the performance is comparable, on average around 10-20% slower, probably because C# has no popcnt intrinsic and due to the Immutable datastructure overhead.
It also depends heavily on the system used, on a Intel XEON W3550 the .NET code was actually a tad faster. I imagine that the JVM is better optimized as the new RyuJIT compiler has rather meager optimization support for e.g. loop unrolling.
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
     And |  2.7200 ms | 0.0094 ms | 367.65 |
 Iterate | 93.8802 ms | 0.7333 ms |  10.65 |
      Or |  2.7114 ms | 0.0198 ms | 368.82 |
     Xor |  2.4673 ms | 0.0145 ms | 405.30 |