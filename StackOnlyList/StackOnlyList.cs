﻿using System;
using System.Buffers;

namespace StackOnlyList
{
	public ref struct StackOnlyList<T>
	{
		Span<T> Span;
		T[] ArrayFromPool;
		public int Capacity { get; private set; }
		public int Count { get; private set; }

		public Span<T> AsSpan()
		{
			return Span[..Count];
		}

		public ReadOnlySpan<T> AsReadOnlySpan()
		{
			return AsSpan();
		}

		public StackOnlyList(Span<T> initialBuffer)
		{
			ArrayFromPool = null;
			Span = initialBuffer;
			Capacity = initialBuffer.Length;
			Count = 0;
		}

		public StackOnlyList(int initialCapacity)
		{
			if(initialCapacity <= 0)
				throw new InvalidOperationException($"Non-positive capacity '{initialCapacity}' is not allowed.");

			ArrayFromPool = ArrayPool<T>.Shared.Rent(initialCapacity);
			Span = ArrayFromPool;
			Capacity = initialCapacity;
			Count = 0;
		}

		public T this[int index]
		{
			get
			{
#if Debug
				if(index >= Count)
					throw new IndexOutOfRangeException($"Index '{index}' is out of range. Count: '{Count}'.");
#endif
				return Span[index];
			}
		}

		public void Add(in T item)
		{
			if(Capacity == Count)
			{
				// Resize
				{
					// First copy to new array, then return to the pool
					var previousSpan = Span;
					var newCapacity = Capacity * 2;
					var newArray = ArrayPool<T>.Shared.Rent(newCapacity);
					Span = newArray;
					previousSpan.CopyTo(Span);

					if(ArrayFromPool != null)
					{
						ArrayPool<T>.Shared.Return(ArrayFromPool);
					}

					ArrayFromPool = newArray;
					Capacity = newCapacity;
				}
			}

			Span[Count++] = item;
		}

		public void Dispose()
		{
			var toReturn = ArrayFromPool;

			// Avoid using this struct again if it was erroneously appended again
			this = default;

			if(toReturn != null)
			{
				ArrayPool<T>.Shared.Return(toReturn);
			}
		}
	}
}