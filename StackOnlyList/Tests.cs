using System;
using NUnit.Framework;

namespace StackOnlyList
{
	public class Tests
	{
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
			Assert.AreEqual(8, list.Capacity);
		}
		[TestCase(-1)]
		[TestCase(-10)]
		[TestCase(-24382934)]
		[TestCase(int.MinValue)]
		public void NegativeCapacityThrows(int negativeCapacity)
		{
			Assert.Throws<InvalidOperationException>(() =>
			{
				using var list = new StackOnlyList<int>(negativeCapacity);
			});
		}
		
		[Test]
		public void ZeroCapacityThrows()
		{
			Assert.Throws<InvalidOperationException>(() =>
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