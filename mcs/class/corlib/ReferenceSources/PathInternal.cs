namespace System.IO
{
	static partial class PathInternal
	{
		// Removing since this is already defined by corert's PathInternal
		/*public static bool IsPartiallyQualified (string path)
		{
			return false;
		}*/

		public static bool HasIllegalCharacters (string path, bool checkAdditional)
		{
			return path.IndexOfAny (Path.InvalidPathChars) != -1;
		}
	}
}