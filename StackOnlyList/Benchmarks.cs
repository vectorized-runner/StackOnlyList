using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace StackOnlyList
{
	[ShortRunJob]
	[MemoryDiagnoser]
	public class Benchmarks
	{
		// Array pool, Just new'ing, vs StackOnlyList
		// Simulate Lots of allocations (millions)
		// Will we trigger stackoverflow?
		// Just do integer sum
		// Also want to check memory footprint
		// Bonus: Disable Stack zero'ing
		// Bonus: Some items exceed stack size

		int[] CopyArray;
		int Min = 0;
		int Max = 1000;
		int ArrayCount = 16;
		int IterationCount = 10_000_000;

		Stack<List<int>> Pool = new Stack<List<int>>();

		public Benchmarks()
		{
			var random = new Random(0);

			CopyArray = new int[ArrayCount];
			for(int i = 0; i < ArrayCount; i++)
			{
				CopyArray[i] = random.Next(Min, Max);
			}
		}

		[Benchmark(Baseline = true)]
		public int NewListSum()
		{
			var sum = 0;

			for(int iIter = 0; iIter < IterationCount; iIter++)
			{
				sum += SumListOnce();
			}

			return sum;
		}

		[Benchmark]
		public int NewListSum_InitialCapacity()
		{
			var sum = 0;

			for(int i = 0; i < IterationCount; i++)
			{
				sum += SumListOnceInitialCapacity();
			}

			return sum;
		}

		[Benchmark]
		public int StackOnlyListSum()
		{
			var sum = 0;

			for(int i = 0; i < IterationCount; i++)
			{
				sum += StackOnlyListSumOnce();
			}

			return sum;
		}

		int StackOnlyListSumOnce()
		{
			using var list = new StackOnlyList<int>(stackalloc int[32]);

			for(int i = 0; i < ArrayCount; i++)
			{
				list.Add(CopyArray[i]);
			}

			var sum = 0;

			for(int i = 0; i < list.Count; i++)
			{
				sum += list[i];
			}

			return sum;
		}

		int SumListOnceInitialCapacity()
		{
			var list = new List<int>(ArrayCount);

			for(int i = 0; i < ArrayCount; i++)
			{
				list.Add(CopyArray[i]);
			}

			var sum = 0;

			for(int i = 0; i < list.Count; i++)
			{
				sum += list[i];
			}

			return sum;
		}

		int SumListOnce()
		{
			var list = new List<int>();

			for(int i = 0; i < ArrayCount; i++)
			{
				list.Add(CopyArray[i]);
			}

			var sum = 0;

			for(int i = 0; i < list.Count; i++)
			{
				sum += list[i];
			}

			return sum;
		}
	}
}