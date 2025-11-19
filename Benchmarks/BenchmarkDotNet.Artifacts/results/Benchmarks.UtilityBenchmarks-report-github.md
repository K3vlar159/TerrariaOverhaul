```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26200.7171)
Unknown processor
.NET SDK 8.0.414
  [Host]     : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2
  Job-HVEUKH : .NET 8.0.20 (8.0.2025.41914), X64 RyuJIT AVX2

IterationCount=5  WarmupCount=3  

```
| Method                              | Mean          | Error       | StdDev     | Allocated |
|------------------------------------ |--------------:|------------:|-----------:|----------:|
| Counter_Increase_Dispose            |     0.8158 ns |   0.5317 ns |  0.1381 ns |         - |
| Surface_IndexAccess_10000Times      | 6,131.7503 ns | 351.8505 ns | 54.4492 ns |         - |
| Surface_IndexCalculation_10000Times | 3,498.8301 ns | 306.2011 ns | 79.5195 ns |         - |
| DodgerollStats_Initialization       |     4.4461 ns |   1.2518 ns |  0.3251 ns |         - |
