using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text;

namespace StackOnlyList
{
	// Always pass this as ref to methods, never return it (unless you're ref returning, then its fine)
	public ref struct StackOnlyList<T> where T : IEquatable<T>
	{
		// These fields are internal because they're used in unit tests
		internal Span<T> Span;
		internal T[] ArrayFromPool;
		public int Capacity { get; private set; }
		public int Count { get; private set; }

		public ref T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				CheckIndexGreaterOrEqualToCountAndThrow(index);
				return ref Span[index];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T> AsSpan()
		{
			return Span[..Count];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ReadOnlySpan<T> AsReadOnlySpan()
		{
			return AsSpan();
		}

		public override string ToString()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.Append("Count: ");
			stringBuilder.Append(Count);
			stringBuilder.AppendLine();
			stringBuilder.Append("Capacity: ");
			stringBuilder.Append(Capacity);
			stringBuilder.AppendLine();
			stringBuilder.Append("Elements: ");

			for(int i = 0; i < Count; i++)
			{
				stringBuilder.Append(Span[i]);
				stringBuilder.Append(", ");
			}

			return stringBuilder.ToString();
		}

		public StackOnlyList(Span<T> initialBuffer)
		{
			ArrayFromPool = null;
			Span = initialBuffer;
			Capacity = initialBuffer.Length;
			Count = 0;
		}

		public StackOnlyList(int initialCapacity = 0)
		{
			switch(initialCapacity)
			{
				case < 0:
				{
					throw new ArgumentOutOfRangeException($"Negative capacity '{initialCapacity}' is not allowed.");
				}
				case 0:
				{
					ArrayFromPool = null;
					Span = null;
					Capacity = 0;
					Count = 0;
					break;
				}
				case > 0:
				{
					ArrayFromPool = ArrayPool<T>.Shared.Rent(initialCapacity);
					Span = ArrayFromPool;
					Capacity = ArrayFromPool.Length;
					Count = 0;
					break;
				}
			}
		}

		public void Reverse()
		{
			// Don't dispose this! We'll use its memory
			var reverseList = new StackOnlyList<T>(Count);

			for(int i = 0; i < Count; i++)
			{
				reverseList.Span[Count - i - 1] = Span[i];
			}

			// Return our memory, since we'll use the memory from tempList
			if(ArrayFromPool != null)
			{
				ArrayPool<T>.Shared.Return(ArrayFromPool);
			}

			this = reverseList;
		}

		public void ClearWithoutMemoryRelease()
		{
			Count = 0;
		}

		public void Clear()
		{
			Dispose();
		}
		
		public int IndexOf(in T item)
		{
			for(int i = 0; i < Count; i++)
			{
				if(Span[i].Equals(item))
				{
					return i;
				}
			}

			return -1;
		}

		public bool Remove(in T item)
		{
			for(int i = 0; i < Count; i++)
			{
				if(Span[i].Equals(item))
				{
					for(int j = i; j < Count - 1; j++)
					{
						Span[j] = Span[j + 1];
					}

					Count--;
					return true;
				}
			}

			return false;
		}

		public bool Contains(in T item)
		{
			for(int i = 0; i < Count; i++)
			{
				if(Span[i].Equals(item))
				{
					return true;
				}
			}

			return false;
		}

		public void Add(in T item)
		{
			if(Capacity == Count)
			{
				var desiredCapacity = Capacity == 0 ? 4 : 2 * Capacity;
				Grow(desiredCapacity);
			}
			
			Span[Count++] = item;
		}

		void Grow(int desiredCapacity)
		{
			if(Capacity == 0)
			{
				var newArray = ArrayPool<T>.Shared.Rent(desiredCapacity);
				ArrayFromPool = newArray;
				Span = newArray;
				Capacity = newArray.Length;
			}
			else
			{
				// First copy to new array, then return to the pool
				var previousSpan = Span;
				var newArray = ArrayPool<T>.Shared.Rent(desiredCapacity);
				Span = newArray;
				previousSpan.CopyTo(Span);

				if(ArrayFromPool != null)
				{
					ArrayPool<T>.Shared.Return(ArrayFromPool);
				}

				ArrayFromPool = newArray;
				Capacity = newArray.Length;
			}
		}

		public void RemoveAt(int index)
		{
			RemoveAt(index, out _);
		}

		public void RemoveAt(int index, out T element)
		{
			CheckIndexGreaterOrEqualToCountAndThrow(index);

			element = Span[index];

			for(int i = index; i < Count - 1; i++)
			{
				Span[i] = Span[i + 1];
			}

			Count--;
		}

		public void RemoveAtSwapBack(int index)
		{
			CheckIndexGreaterOrEqualToCountAndThrow(index);
			Span[index] = Span[--Count];
		}

		public void RemoveFirst()
		{
			RemoveAt(0);
		}

		public void RemoveLast()
		{
			RemoveAt(Count - 1);
		}

		public void Insert(in T item, int index)
		{
			CheckIndexGreaterThanCountAndThrow(index);
			
			if(Capacity == Count)
			{
				var newCapacity = Capacity == 0 ? 4 : 2 * Capacity;
				Grow(newCapacity);
			}
			for(int i = Count; i > index; i--)
			{
				Span[i] = Span[i - 1];
			}

			Span[index] = item;
			Count++;
		}

		public void Dispose()
		{
			var toReturn = ArrayFromPool;

			// Prevent using existing data, if this struct is erroneously used after it is disposed.
			this = default;

			if(toReturn != null)
			{
				ArrayPool<T>.Shared.Return(toReturn);
			}
		}

		public Span<T>.Enumerator GetEnumerator()
		{
			return AsSpan().GetEnumerator();
		}

		void CheckIndexGreaterThanCountAndThrow(int index)
		{
			if(index > Count)
				throw new IndexOutOfRangeException($"Index '{index}' is out of range. Count: '{Count}'.");
		}

		void CheckIndexGreaterOrEqualToCountAndThrow(int index)
		{
			if(index >= Count)
				throw new IndexOutOfRangeException($"Index '{index}' is out of range. Count: '{Count}'.");
		}
	}
}