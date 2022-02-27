using BenchmarkDotNet.Attributes;

namespace StackOnlyList
{
	[MemoryDiagnoser]
	[ShortRunJob]
	public class Benchmarks_Allocation
	{
		// Total allocations should be only 10_000 integer, if we're using array pool properly
		[Benchmark]
		public void EnsureNearZeroMemoryUsed()
		{
			for(int i = 0; i < 100_000; i++)
			{
				CreateDummyList();
			}
		}

		void CreateDummyList()
		{
			using var list = new StackOnlyList<int>(0);
			
			// 10_000 additions
			for(int i = 0; i < 10_000; i++)
			{
				list.Add(i);
			}
		}
	}
}