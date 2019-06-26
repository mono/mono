using System;

namespace System
{
	internal static class AppContextDefaultValues
	{
		internal const string SwitchNoAsyncCurrentCulture = "Switch.System.Globalization.NoAsyncCurrentCulture";
		internal static readonly string SwitchEnforceJapaneseEraYearRanges = "Switch.System.Globalization.EnforceJapaneseEraYearRanges";
		internal static readonly string SwitchFormatJapaneseFirstYearAsANumber = "Switch.System.Globalization.FormatJapaneseFirstYearAsANumber";
		internal static readonly string SwitchEnforceLegacyJapaneseDateParsing = "Switch.System.Globalization.EnforceLegacyJapaneseDateParsing";		
		internal const string SwitchThrowExceptionIfDisposedCancellationTokenSource = "Switch.System.Threading.ThrowExceptionIfDisposedCancellationTokenSource";
		internal const string SwitchPreserveEventListnerObjectIdentity = "Switch.System.Diagnostics.EventSource.PreserveEventListnerObjectIdentity";
		internal const string SwitchUseLegacyPathHandling = "Switch.System.IO.UseLegacyPathHandling";
		internal const string SwitchBlockLongPaths = "Switch.System.IO.BlockLongPaths";
		internal const string SwitchDoNotAddrOfCspParentWindowHandle = "Switch.System.Security.Cryptography.DoNotAddrOfCspParentWindowHandle";
		internal const string SwitchSetActorAsReferenceWhenCopyingClaimsIdentity = "Switch.System.Security.ClaimsIdentity.SetActorAsReferenceWhenCopyingClaimsIdentity";

		public static void PopulateDefaultValues () {
		}

		//TODO Use the values in app.config
		public static bool TryGetSwitchOverride (string switchName, out bool overrideValue)
		{
			// The default value for a switch is 'false'
			overrideValue = false;

			return false;
		}
	}
}

