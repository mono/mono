namespace System.IO
{
	static class PathInternal
	{
		public static bool IsPartiallyQualified (string path)
		{
			return false;
		}

		public static bool HasIllegalCharacters (string path, bool checkAdditional)
		{
			return path.IndexOfAny (Path.InvalidPathChars) != -1;
		}
	}
}