// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Globalization
{
	internal static partial class GlobalizationMode
	{
		internal static bool Invariant { get; } = GetGlobalizationInvariantMode ();

		static bool GetGlobalizationInvariantMode () {
			var val = Environment.GetEnvironmentVariable ("DOTNET_SYSTEM_GLOBALIZATION_INVARIANT");
			if (val != null)
				return Boolean.IsTrueStringIgnoreCase (val) || val.Equals ("1");
			int res = Interop.Globalization.LoadICU ();
			if (res == 0) {
				string message = "Couldn't find a valid ICU package installed on the system. " +
					"Set the configuration flag System.Globalization.Invariant to true if you want to run with no globalization support.";
				Environment.FailFast (message);
			}
			return false;
		}
	}
}