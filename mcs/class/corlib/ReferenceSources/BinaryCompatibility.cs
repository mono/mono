namespace System.Runtime.Versioning
{
	static class BinaryCompatibility
	{
		public static readonly bool TargetsAtLeast_Desktop_V4_5 = true;
		public static readonly bool TargetsAtLeast_Desktop_V4_5_1 = true;
		// should be a property for System.Xml.BinaryCompatibility
		public static bool TargetsAtLeast_Desktop_V4_5_2 => true;
	}
}