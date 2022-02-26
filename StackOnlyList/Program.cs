using System;

namespace StackOnlyList
{
	public static class Util
	{
		public static unsafe int SafeStackSize<T>() where T : unmanaged
		{
			return 256 / sizeof(T);
		}
	}
	
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Hello World!");
		}
	}
}