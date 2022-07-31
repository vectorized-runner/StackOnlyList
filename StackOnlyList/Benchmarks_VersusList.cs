﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

namespace StackOnlyList
{
	[ShortRunJob]
	[MemoryDiagnoser]
	[HardwareCounters(HardwareCounter.CacheMisses, HardwareCounter.BranchMispredictions, HardwareCounter.InstructionRetired)]
	[SkipLocalsInit]
	public class Benchmarks_VersusList
	{
		int[] CopyArray;
		int Min = 0;
		int Max = 1000;
		int ArrayCount = 16;
		int IterationCount = 1_000_000;

		public Benchmarks_VersusList()
		{
			var random = new Random(0);

			CopyArray = new int[ArrayCount];
			for(int i = 0; i < ArrayCount; i++)
			{
				CopyArray[i] = random.Next(Min, Max);
			}
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

		[Benchmark(Baseline = true)]
		public int NewListSum()
		{
			var sum = 0;

			for(int i = 0; i < IterationCount; i++)
			{
				sum += SumListOnce();
			}

			return sum;
		}

		[Benchmark]
		public int NewListSumWithInitialCapacity()
		{
			var sum = 0;

			for(int i = 0; i < IterationCount; i++)
			{
				sum += SumListOnceInitialCapacity();
			}

			return sum;
		}

		int StackOnlyListSumOnce()
		{ 
			using var list = new StackOnlyList<int>(stackalloc int[ArrayCount]);

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