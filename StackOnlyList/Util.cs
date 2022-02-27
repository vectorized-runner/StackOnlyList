namespace StackOnlyList
{
	public static class Util
	{
		public static unsafe int SafeStackSize<T>() where T : unmanaged
		{
			return 512 / sizeof(T);
		}
	}
}