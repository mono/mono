using System;

namespace NUnit.Framework
{
	/// <summary>
	/// GlobalSettings is a place for setting default values used
	/// by the framework in performing asserts.
	/// </summary>
	public class GlobalSettings
	{
		/// <summary>
		/// Default tolerance for floating point equality
		/// </summary>
		public static double DefaultFloatingPointTolerance = 0.0d;
	}
}
