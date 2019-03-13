using System.IO;
using System.Reflection;

namespace System
{
	partial class AppContext
	{
		// Called by the runtime
		internal static unsafe void Setup (char** pNames, char** pValues, int count) {
			for (int i = 0; i < count; i++)
				s_dataStore.Add (new string ((sbyte*)pNames[i]), new string ((sbyte*)pValues[i]));
		}

		private static string GetBaseDirectoryCore () {
			// Fallback path for hosts that do not set APP_CONTEXT_BASE_DIRECTORY explicitly
			string directory = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
			if (directory != null && !PathInternal.EndsInDirectorySeparator(directory))
				directory += Path.DirectorySeparatorChar;
			return directory;
		}
	}
}