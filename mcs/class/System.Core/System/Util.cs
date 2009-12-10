namespace System {

	internal static class MonoUtil {
		static public bool IsUnix {
			get {
				int platform = (int) Environment.OSVersion.Platform;

				return (platform == 4 || platform == 128 || platform == 6);
			}
		}

		
	}
}