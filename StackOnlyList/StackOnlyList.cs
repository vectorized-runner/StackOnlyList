using System;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace StackOnlyList
{
	public ref struct StackOnlyList<T> where T : IEquatable<T>
	{
		// These fields are internal because they're used in unit tests
		internal Span<T> Span;
		internal T[] ArrayFromPool;
		public int Count { get; private set; }

		public int Capacity
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Span.Length;
		}

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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public Span<T>.Enumerator GetEnumerator()
		{
			return AsSpan().GetEnumerator();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public StackOnlyList(Span<T> initialBuffer)
		{
			ArrayFromPool = null;
			Span = initialBuffer;
			Count = 0;
		}

		// Is it good to inline this or not?
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
					Span = Span<T>.Empty;
					Count = 0;
					break;
				}
				case > 0:
				{
					ArrayFromPool = ArrayPool<T>.Shared.Rent(initialCapacity);
					Span = ArrayFromPool;
					Count = 0;
					break;
				}
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Reverse()
		{
			AsSpan().Reverse();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int IndexOf(in T item)
		{
			return AsSpan().IndexOf(item);
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(in T item)
		{
			return AsSpan().Contains(item);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Add(in T item)
		{
			if(Capacity == Count)
			{
				Grow();
			}

			Span[Count++] = item;
		}

		// We don't want to inline the rare path
		[MethodImpl(MethodImplOptions.NoInlining)]
		void Grow()
		{
			var desiredCapacity = Capacity == 0 ? 4 : 2 * Capacity;

			if(Capacity == 0)
			{
				var newArray = ArrayPool<T>.Shared.Rent(desiredCapacity);
				ArrayFromPool = newArray;
				Span = newArray;
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
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAt(int index)
		{
			RemoveAt(index, out _);
		}

		// Is it good to inline this or not?
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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RemoveAtSwapBack(int index)
		{
			CheckIndexGreaterOrEqualToCountAndThrow(index);
			Span[index] = Span[--Count];
		}

		// Is it good to inline this or not?
		public void Insert(in T item, int index)
		{
			CheckIndexGreaterThanCountAndThrow(index);

			if(Capacity == Count)
			{
				Grow();
			}

			for(int i = Count; i > index; i--)
			{
				Span[i] = Span[i - 1];
			}

			Span[index] = item;
			Count++;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			var toReturn = ArrayFromPool;

			// Prevent using existing data, if this struct is erroneously used after it is disposed.
			// This can be commented out for extra performance.
			// this = default;

			if(toReturn != null)
			{
				ArrayFromPool = null;
				ArrayPool<T>.Shared.Return(toReturn);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void CheckIndexGreaterThanCountAndThrow(int index)
		{
			if(index > Count)
				throw new IndexOutOfRangeException($"Index '{index}' is out of range. Count: '{Count}'.");
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void CheckIndexGreaterOrEqualToCountAndThrow(int index)
		{
			if(index >= Count)
				throw new IndexOutOfRangeException($"Index '{index}' is out of range. Count: '{Count}'.");
		}
	}
}