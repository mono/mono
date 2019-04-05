namespace System.Runtime.InteropServices
{
	public static class InteropExtensions
	{
		public static bool IsBlittable (this object obj)
		{
			throw new NotImplementedException ();
		}

		public static int GetElementSize (this Array array)
		{
			if (array == null)
				throw new ArgumentNullException (nameof (array));
			return Marshal.GetArrayElementSize (array.GetType ());
		}
	}
}