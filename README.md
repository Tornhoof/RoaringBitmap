# RoaringBitmap for .NET
=========================
This is a fairly simple port of the Java bitmap library RoaringBitmap by Daniel Lemire et al.
You can find the original version here: https://github.com/lemire/RoaringBitmap
Most of the algorithms are ports of the original Java algorithms.
This is an early version, the test coverage is ok, but edge case tests are probably missing.

# What is in it?
================
* Immutable data structure using readonly fields and private constructors
* overloaded operators for AND, OR, NOT and XOR
* No RLE at the moment

# How to use it?
================
Compile the RoaringBitmap.sln and use RoaringBitmap.Create to create your bitmap, then use boolean operators on it. See the Unit Tests for examples.


# Performance
=============
As this is a fairly direct port of the Java library the performance is comparable, on average around 10% slower, mostly because C# has no popcnt intrinsic and due to the Immutable datastructure overhead.
The RoaringBitmap.Benchmark project contains microbenchmarks for the real-roaring-data set. This set is also used by the Unit Tests.
