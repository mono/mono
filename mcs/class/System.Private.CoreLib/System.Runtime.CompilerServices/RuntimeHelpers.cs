namespace System.Runtime.CompilerServices
{
	partial class RuntimeHelpers
	{
		internal static unsafe bool ObjectHasComponentSize (object obj)
		{
			throw new NotImplementedException ();
		}

		public static object GetUninitializedObject (Type type)
		{
			throw new NotImplementedException ();
		}

		public static T[] GetSubArray<T> (T[] array, Range range)
		{
			Type elementType = array.GetType().GetElementType();
			Span<T> source = array.AsSpan(range);

			if (elementType.IsValueType)
				return source.ToArray();

			T[] newArray = (T[])Array.CreateInstance(elementType, source.Length);
			source.CopyTo(newArray);
			return newArray;
		}
	}
}