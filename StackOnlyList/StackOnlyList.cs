﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace StackOnlyList
{
	// Always pass this as ref to methods, never return it (unless you're ref returning, then its fine)
	public ref struct StackOnlyList<T>
	{
		Span<T> Span;
		T[] ArrayFromPool;
		public int Capacity { get; private set; }
		public int Count { get; private set; }

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
					throw new InvalidOperationException($"Negative capacity '{initialCapacity}' is not allowed.");
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
					Capacity = initialCapacity;
					Count = 0;
					break;
				}
			}
		}

		public T this[int index]
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				CheckIndexOutOfRangeAndThrow(index);
				return Span[index];
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public ref T ElementAsRef(int index)
		{
			CheckIndexOutOfRangeAndThrow(index);
			return ref Span[index];
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

		public void Clear()
		{
			Count = 0;
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
			// Check for resizing
			if(Capacity == Count)
			{
				var newCapacity = Capacity == 0 ? 4 : 2 * Capacity;
				EnsureCapacity(newCapacity);
			}

			Span[Count++] = item;
		}

		public void EnsureCapacity(int newCapacity)
		{
			if(newCapacity <= Capacity)
				return;

			if(Capacity == 0)
			{
				// Use a fresh array, don't do any copying
				Capacity = newCapacity;
				ArrayFromPool = ArrayPool<T>.Shared.Rent(Capacity);
				Span = ArrayFromPool;
			}
			else
			{
				// First copy to new array, then return to the pool
				var previousSpan = Span;
				var newArray = ArrayPool<T>.Shared.Rent(newCapacity);
				Span = newArray;
				previousSpan.CopyTo(Span);

				if(ArrayFromPool != null)
				{
					ArrayPool<T>.Shared.Return(ArrayFromPool);
				}

				Capacity = newCapacity;
			}
		}

		public void AddRange(IEnumerable<T> items)
		{
			foreach(var item in items)
			{
				Add(item);
			}
		}

		public void RemoveAtSwapBack(int index)
		{
			CheckIndexOutOfRangeAndThrow(index);
			Span[index] = Span[--Count];
		}

		public void Dispose()
		{
			var toReturn = ArrayFromPool;

			// Clear data, so using after disposed is safer
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

		[Conditional("Debug")]
		void CheckIndexOutOfRangeAndThrow(int index)
		{
			if(index >= Count)
				throw new IndexOutOfRangeException($"Index '{index}' is out of range. Count: '{Count}'.");
		}
	}
}