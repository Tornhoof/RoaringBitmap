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
* Add RunContainer support

# How to use it?
Compile the RoaringBitmap.sln and use 'RoaringBitmap.Create' to create your bitmap, then use bitwise operations on it.
```csharp
var rb = RoaringBitmap.Create(1,2,3,4,5,7,8,9,10);
var rb2 = RoaringBitmap.Create(Enumerable.Range(10000,200000));
var rb3 = rb | rb2;
```

# Performance
As this is a fairly direct port of the immutable part of the Java Version of RoaringBitmap we can directly compare the Java Benchmark with this version, both benchmarks are equivalent and are also used for the Unit Testing to make sure they are actually doing the same.
The CensusIncome dataset shows slightly faster performance numbers than the Java version.
The Census1881 dataset is, except for AND, quite a bit faster than the Java version.

``` ini

BenchmarkDotNet=v0.10.1, OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-4790K CPU 4.00GHz, ProcessorCount=8
Frequency=3906246 Hz, Resolution=256.0003 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1586.0
  DefaultJob : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1586.0
  
Type=MicroBenchmarkCensusIncome  Mode=Throughput
```
  Method |       Mean |    StdDev |    Gen 0 | Allocated |
-------- |----------- |---------- |--------- |---------- |
      Or |  2.2688 ms | 0.0042 ms | 846.8750 |   3.89 MB |
     Xor |  2.3919 ms | 0.0073 ms | 872.9167 |   3.96 MB |
     And |  2.2721 ms | 0.0031 ms | 233.3333 |   1.32 MB |
  AndNot |  2.3636 ms | 0.0040 ms | 517.1875 |   2.49 MB |
 Iterate | 89.4243 ms | 0.0203 ms |        - |  43.52 kB |
 
 ``` ini

BenchmarkDotNet=v0.10.1, OS=Microsoft Windows NT 6.2.9200.0
Processor=Intel(R) Core(TM) i7-4790K CPU 4.00GHz, ProcessorCount=8
Frequency=3906246 Hz, Resolution=256.0003 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1586.0
  DefaultJob : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.1586.0

Type=MicroBenchmarkCensus1881  Mode=Throughput  
```
  Method |           Mean |    StdDev |   Gen 0 |   Gen 1 | Allocated |
-------- |--------------- |---------- |-------- |-------- |---------- |
      Or |    234.7337 us | 0.1643 us | 82.4219 |       - | 392.01 kB |
     Xor |    194.7747 us | 3.3360 us | 88.0859 |       - | 392.01 kB |
     And |     65.8376 us | 0.3064 us |  5.7617 |       - |  35.42 kB |
  AndNot |    156.3676 us | 0.0581 us | 73.4375 | 10.4167 | 330.47 kB |
 Iterate | 10,166.3848 us | 7.7011 us |       - |       - |  68.35 kB |


The Java output from the recent 0.6.32-SNAPSHOT version looks like this:
``` ini
# JMH 1.17 (released 47 days ago)
# VM version: JDK 1.8.0_112, VM 25.112-b15
# VM invoker: C:\Program Files\Java\jre1.8.0_112\bin\java.exe
# VM options: -DBITMAP_TYPES=ROARING_ONLY
# Warmup: 5 iterations, 1 s each
# Measurement: 5 iterations, 1 s each
# Timeout: 10 min per iteration
# Threads: 1 thread, will synchronize iterations
# Benchmark mode: Average time, time/op
```

 Method | Dataset       | Score    /  Error  | Units |
------- | ------------- | ------------------ | ----- |
 And    | census-income | 2251,305 ±  34,711 | us/op |
 And    | census1881    |   53,618 ±   0,603 | us/op |
 AndNot | census-income | 2829,971 ±   5,168 | us/op |
 AndNot | census1881    |  474,517 ±   6,497 | us/op |
 Or     | census-income | 2916,778 ±   3,628 | us/op |
 Or     | census1881    |  748,054 ±  15,839 | us/op |
 Xor    | census-income | 2845,861 ±  18,825 | us/op |
 Xor    | census1881    |  811,914 ±   1,656 | us/op |
