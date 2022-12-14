using System;
using NUnit.Framework;

namespace StackOnlyList
{
	public class Tests
	{
		// This does throw StackOverFlow exception, but you can't catch it.
		// [Test]
		// public void StackAllocThrowsWhenCalledInlined()
		// {
		// 	Assert.Throws<StackOverflowException>(() =>
		// 	{
		// 		for(int i = 0; i < 100_000; i++)
		// 		{
		// 			using var list = new StackOnlyList<int>(stackalloc int[32]);
		// 			list.Add(0);
		// 			list.Add(0);
		// 			list.Add(0);
		// 		}
		// 	});
		// }

		[Test]
		public void TestInsert()
		{
			using var list = new StackOnlyList<int>();
			list.Add(1);
			list.Add(2);
			list.Add(3);

			list.Insert(5, 0);
			
			Assert.AreEqual(5, list[0]);
			Assert.AreEqual(1, list[1]);
			Assert.AreEqual(2, list[2]);
			Assert.AreEqual(3, list[3]);
		}
		
		[Test]
		public void TestInsertBehaviour()
		{
			using var list = new StackOnlyList<int>();
			list.Insert(5, 0);
			list.Insert(10, 0);
			list.Insert(7, 1);
			list.Insert(3, 2);
			
			Assert.AreEqual(10, list[0]);
			Assert.AreEqual(7, list[1]);
			Assert.AreEqual(3, list[2]);
			Assert.AreEqual(5, list[3]);

			Assert.AreEqual(4, list.Count);
		}

		[Test]
		public void CanInsertAtCount()
		{
			using var list = new StackOnlyList<int>();
			list.Insert(5, 0);
			
			Assert.AreEqual(5, list[0]);
			Assert.AreEqual(1, list.Count);
		}

		[Test]
		public void WorksWithNullBuffer()
		{
			using var list = new StackOnlyList<int>(null);
			list.Add(1);
			list.Add(2);
			list.Add(3);
			
			Assert.AreEqual(1, list[0]);
			Assert.AreEqual(2, list[1]);
			Assert.AreEqual(3, list[2]);
		}
		
		[Test]
		public void WorksWithZeroBuffer()
		{
			using var list = new StackOnlyList<int>(stackalloc int[0]);
			list.Add(1);
			list.Add(2);
			list.Add(3);

			Assert.AreEqual(1, list[0]);
			Assert.AreEqual(2, list[1]);
			Assert.AreEqual(3, list[2]);
		}

		[Test]
		public void DoesNotThrowOnZeroBuffer()
		{
			Assert.DoesNotThrow(() =>
			{
				using var list = new StackOnlyList<int>(stackalloc int[0]);
			});
		}
		
		[Test]
		public void PoolIsNotNullOnResize()
		{
			var list = new StackOnlyList<int>(0);
			list.Add(1);
			list.Add(2);
			list.Add(3);
			list.Add(4);
			
			Assert.NotNull(list.ArrayFromPool);
		}

		// This does throw, but only in debug mode
#if Debug
		[Test]
		public void RemoveAtThrowsOnInvalidIndex()
		{
			Assert.Throws<InvalidOperationException>(() =>
			{
				using var list = new StackOnlyList<int>(10);
				list.RemoveAt(0);
			});
		}
		
		
		[Test]
		public void ThrowsOnInsertingAtInvalidPlace()
		{
			Assert.Throws<InvalidOperationException>(() =>
			{
				using var list = new StackOnlyList<int>();
				list.Insert(5, 0);

				list.Insert(10, 2);
			});
		}
#endif
		
		[Test]
		public void RemoveAtFirst()
		{
			using var list = new StackOnlyList<int>(10);
			
			list.Add(2);
			list.Add(4);
			list.Add(6);
			list.Add(8);
			list.Add(10);
			
			list.RemoveAt(0);
			
			Assert.AreEqual(4, list[0]);
			Assert.AreEqual(6, list[1]);			
			Assert.AreEqual(8, list[2]);			
			Assert.AreEqual(10, list[3]);			
			
			Assert.AreEqual(4, list.Count);
		}

		[Test]
		public void RemoveAtLast()
		{
			using var list = new StackOnlyList<int>(10);
			
			list.Add(2);
			list.Add(4);
			list.Add(6);
			list.Add(8);
			list.Add(10);
			
			list.RemoveAt(4);
			
			Assert.AreEqual(2, list[0]);
			Assert.AreEqual(4, list[1]);			
			Assert.AreEqual(6, list[2]);			
			Assert.AreEqual(8, list[3]);			
			
			Assert.AreEqual(4, list.Count);
		}

		[Test]
		public void RemoveAtRandom()
		{
			using var list = new StackOnlyList<int>(10);
			
			list.Add(2);
			list.Add(4);
			list.Add(6);
			list.Add(8);
			list.Add(10);
			
			list.RemoveAt(1);
			list.RemoveAt(2);
			
			Assert.AreEqual(2, list[0]);
			Assert.AreEqual(6, list[1]);			
			Assert.AreEqual(10, list[2]);			
			
			Assert.AreEqual(3, list.Count);
		}

		// [Test]
		// public void AddRange()
		// {
		// 	using var list = new StackOnlyList<int>();
		// 	list.AddRange(new [] { 1, 2, 3 });
		// 	
		// 	Assert.AreEqual(1, list[0]);
		// 	Assert.AreEqual(2, list[1]);
		// 	Assert.AreEqual(3, list[2]);
		// }

		[Test]
		public void UsingRefReturnAfterAdd()
		{
			using var list = new StackOnlyList<int>(stackalloc int[2]);
			
			list.Add(0);
			ref var firstElement = ref list[0];
			list[0] = 1;
			
			Assert.AreEqual(1, firstElement);

			list.Add(0);
			list.Add(0);
			list.Add(0);
			list.Add(0);
			list.Add(0);
			list.Add(0);
			list.Add(0);
			list.Add(0);
			list.Add(0);

			list[0] = 2;
			Assert.AreNotEqual(2, firstElement);
		}

		[Test]
		public void ElementAsRef()
		{
			using var list = new StackOnlyList<int>(3);
			list.Add(2);
			ref var item = ref list[0];
			item++;
			
			Assert.AreEqual(3, list[0]);
		}

		[Test]
		public void Reverse()
		{
			using var list = new StackOnlyList<int>(3);
			list.Add(1);
			list.Add(2);
			list.Add(3);
			list.Reverse();
			Assert.AreEqual(3, list[0]);
			Assert.AreEqual(2, list[1]);
			Assert.AreEqual(1, list[2]);

			using var list2 = new StackOnlyList<int>(3);
			list.Add(2);
			list.Add(4);
			list.Add(6);
			list.Reverse();
			Assert.AreEqual(6, list[0]);
			Assert.AreEqual(4, list[1]);
			Assert.AreEqual(2, list[2]);
		}
		
		[Test]
		public void ZeroCapacityWorks()
		{
			using var list = new StackOnlyList<int>(0);
			
			Assert.AreEqual(0, list.Count);
			
			list.Add(2);
			list.Add(4);
			list.Add(6);
			list.Add(8);
			
			Assert.AreEqual(2, list[0]);
			Assert.AreEqual(4, list[1]);
			Assert.AreEqual(6, list[2]);
			Assert.AreEqual(8, list[3]);
		}

		[Test]
		public void IndexOf()
		{
			using var list = new StackOnlyList<int>();
			Assert.AreEqual(-1, list.IndexOf(0));
		}
		
		[Test]
		public void IndexOf_1()
		{
			using var list = new StackOnlyList<int>();
			list.Add(5);
			list.Add(7);
			
			Assert.AreEqual(1, list.IndexOf(7));
			Assert.AreEqual(0, list.IndexOf(5));
		}

		[Test]
		public void RemoveFromMiddle()
		{
			using var list = new StackOnlyList<int>(10);
			list.Add(0);
			list.Add(2);
			list.Add(4);
			list.Add(6);
			list.Add(8);

			list.Remove(4);

			Assert.AreEqual(0, list[0]);
			Assert.AreEqual(2, list[1]);
			Assert.AreEqual(6, list[2]);
			Assert.AreEqual(8, list[3]);
			Assert.AreEqual(4, list.Count);
		}
		
		[Test]
		public void RemoveFirst()
		{
			using var list = new StackOnlyList<int>(10);
			list.Add(0);
			list.Add(2);
			list.Add(4);
			list.Add(6);
			list.Add(8);

			list.Remove(0);

			Assert.AreEqual(2, list[0]);
			Assert.AreEqual(4, list[1]);
			Assert.AreEqual(6, list[2]);
			Assert.AreEqual(8, list[3]);
			Assert.AreEqual(4, list.Count);
		}

		[Test]
		public void ForEachWorks()
		{
			using var list = new StackOnlyList<int>(10);
			list.Add(3);
			list.Add(5);
			list.Add(7);

			var sum = 0;
			foreach(ref readonly var item in list)
			{
				sum += item;
			}
			
			Assert.AreEqual(3 + 5 + 7, sum);
		}
		
		[Test]
		public void RemoveFromLast()
		{
			using var list = new StackOnlyList<int>(10);
			list.Add(0);
			list.Add(2);
			list.Add(4);
			list.Add(6);
			list.Add(8);

			list.Remove(8);

			Assert.AreEqual(0, list[0]);
			Assert.AreEqual(2, list[1]);
			Assert.AreEqual(4, list[2]);
			Assert.AreEqual(6, list[3]);
			Assert.AreEqual(4, list.Count);
		}

		[Test]
		public void CountNotDecreasedAfterRemoveNonExisting()
		{
			using var list = new StackOnlyList<int>(10);
			list.Add(5);
			list.Add(10);
			list.Remove(2);

			Assert.AreEqual(2, list.Count);	
		}

		[Test]
		public void CountDecreasedAfterRemove()
		{
			using var list = new StackOnlyList<int>(10);
			list.Add(5);
			list.Add(10);
			list.Remove(5);

			Assert.AreEqual(1, list.Count);
		}

		[Test]
		public void DoesNotContainAfterRemove()
		{
			using var list = new StackOnlyList<int>(10);
			list.Add(5);
			list.Remove(5);

			Assert.IsFalse(list.Contains(5));
		}

		[Test]
		public void DoesNotContainBeforeAdd()
		{
			using var list = new StackOnlyList<int>(10);
			Assert.IsFalse(list.Contains(10));
		}
		
		[Test]
		public void ContainsAfterAdd()
		{
			using var list = new StackOnlyList<int>(10);
			list.Add(5);
			Assert.IsTrue(list.Contains(5));
		}

		[Test]
		public void StackAllocDoesntThrowWhenCalledOnSeparateMethod()
		{
			Assert.DoesNotThrow(() =>
			{
				for(int i = 0; i < 100_000; i++)
				{
					InitStackAllocList();
				}
			});
			
			void InitStackAllocList()
			{
				using var list = new StackOnlyList<int>(stackalloc int[32]);
				list.Add(0);
				list.Add(0);
				list.Add(0);
			}
		}
		
		[Test]
		public void WorksWithStackAlloc()
		{
			using var list = new StackOnlyList<int>(stackalloc int[2]);
			list.Add(5);
			list.Add(10);
			
			Assert.AreEqual(5, list[0]);
			Assert.AreEqual(10, list[1]);
		}

		[Test]
		public void StackAllocCanGrow()
		{
			using var list = new StackOnlyList<int>(stackalloc int[2]);
			list.Add(5);
			list.Add(10);
			list.Add(15);

			Assert.AreEqual(5, list[0]);
			Assert.AreEqual(10, list[1]);
			Assert.AreEqual(15, list[2]);
		}

		// [Test]
		// public void CanAddAfterDispose()
		// {
		// 	var list = new StackOnlyList<int>(stackalloc int[10]);
		//
		// 	list.Add(2);
		// 	list.Add(4);
		// 	list.Add(6);
		// 	
		// 	list.Dispose();
		//
		// 	list.Add(10);
		// 	
		// 	Assert.AreEqual(10, list[0]);
		// 	Assert.AreEqual(1, list.Count);
		// }

		[Test]
		public void CountDoesNotIncreaseWithoutPassingRef()
		{
			var theList = new StackOnlyList<int>(stackalloc int[10]);
			AddNum(theList, 2);
			AddNum(theList, 5);
			AddNum(theList, 7);
			
			Assert.AreEqual(0, theList.Count);
			
			void AddNum(StackOnlyList<int> list, int num)
			{
				list.Add(num);
			}
		}
		
		[Test]
		public void WorksWhenPassingByRef()
		{
			var theList = new StackOnlyList<int>(stackalloc int[10]);
			AddNum(ref theList, 2);
			AddNum(ref theList, 5);
			AddNum(ref theList, 7);

			Assert.AreEqual(2, theList[0]);
			Assert.AreEqual(5, theList[1]);
			Assert.AreEqual(7, theList[2]);

			Assert.AreEqual(3, theList.Count);
			
			void AddNum(ref StackOnlyList<int> list, int num)
			{
				list.Add(num);
			}

			theList.Dispose();
		}
		
		[Test]
		public void AddOneElement()
		{
			using var list = new StackOnlyList<int>(1);
			list.Add(2);
			
			Assert.AreEqual(2, list[0]);
		}

		[Test]
		public void AddElementsTriggerResize()
		{
			using var list = new StackOnlyList<int>(1);
			list.Add(2);
			list.Add(4);
			list.Add(6);
			list.Add(8);
			list.Add(10);

			Assert.AreEqual(2, list[0]);
			Assert.AreEqual(4, list[1]);
			Assert.AreEqual(6, list[2]);
			Assert.AreEqual(8, list[3]);
			Assert.AreEqual(10, list[4]);

			Assert.AreEqual(5, list.Count);
			Assert.LessOrEqual(8, list.Capacity);
		}

		[Test]
		public void ThrowsOnRemoveAtInvalidIndex()
		{
			Assert.Throws<IndexOutOfRangeException>(() =>
			{
				using var list = new StackOnlyList<int>(1);
				list.RemoveAtSwapBack(0);
			});
		}

		[Test]
		public void RemoveAtSwapBackWorks()
		{
			using var list = new StackOnlyList<int>(1);
			list.Add(1);
			list.Add(2);
			list.Add(3);
			list.RemoveAtSwapBack(0);
			
			Assert.AreEqual(3, list[0]);
			Assert.AreEqual(2, list.Count);
		}

		[TestCase(-1)]
		[TestCase(-10)]
		[TestCase(-24382934)]
		[TestCase(int.MinValue)]
		public void NegativeCapacityThrows(int negativeCapacity)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() =>
			{
				using var list = new StackOnlyList<int>(negativeCapacity);
			});
		}
		
		[Test]
		public void ZeroCapacityDoesNotThrow()
		{
			Assert.DoesNotThrow(() =>
			{
				using var list = new StackOnlyList<int>(0);
			});
		}

		[TestCase(1)]
		[TestCase(10)]
		[TestCase(34287)]
		// [TestCase(int.MaxValue)]
		public void PositiveCapacityDoesNotThrow(int capacity)
		{
			Assert.DoesNotThrow(() =>
			{
				using var list = new StackOnlyList<int>(capacity);
			});
		}
	}
}