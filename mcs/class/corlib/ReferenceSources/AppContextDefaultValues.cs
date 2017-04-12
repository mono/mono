using System;


namespace System
{
    internal static class AppContextDefaultValues
    {
        internal static readonly string SwitchNoAsyncCurrentCulture = "Switch.System.Globalization.NoAsyncCurrentCulture";
        internal static readonly string SwitchThrowExceptionIfDisposedCancellationTokenSource = "Switch.System.Threading.ThrowExceptionIfDisposedCancellationTokenSource";
        internal static readonly string SwitchPreserveEventListnerObjectIdentity = "Switch.System.Diagnostics.EventSource.PreserveEventListnerObjectIdentity";
        internal static readonly string SwitchUseLegacyPathHandling = "Switch.System.IO.UseLegacyPathHandling";
        internal static readonly string SwitchBlockLongPaths = "Switch.System.IO.BlockLongPaths";
        internal static readonly string SwitchDoNotAddrOfCspParentWindowHandle = "Switch.System.Security.Cryptography.DoNotAddrOfCspParentWindowHandle";
        internal static readonly string SwitchSetActorAsReferenceWhenCopyingClaimsIdentity = "Switch.System.Security.ClaimsIdentity.SetActorAsReferenceWhenCopyingClaimsIdentity";

		static AppContextDefaultValues () {
			//Defaults from mono 5.0
			AppContext.DefineSwitchDefault (SwitchThrowExceptionIfDisposedCancellationTokenSource, true);
			AppContext.DefineSwitchDefault (SwitchSetActorAsReferenceWhenCopyingClaimsIdentity, false);
			AppContext.DefineSwitchDefault (SwitchNoAsyncCurrentCulture, false);
		}
    }
}
