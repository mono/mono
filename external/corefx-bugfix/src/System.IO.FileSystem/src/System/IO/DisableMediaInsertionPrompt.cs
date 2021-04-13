// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.IO
{
    /// <summary>
    /// Simple wrapper to safely disable the normal media insertion prompt for
    /// removable media (floppies, cds, memory cards, etc.)
    /// </summary>
    /// <remarks>
    /// Note that removable media file systems lazily load. After starting the OS
    /// they won't be loaded until you have media in the drive- and as such the
    /// prompt won't happen. You have to have had media in at least once to get
    /// the file system to load and then have removed it.
    /// </remarks>
    internal struct DisableMediaInsertionPrompt : IDisposable
    {
        private bool _disableSuccess;
        private uint _oldMode;

#if UNITY_AOT && WIN_PLATFORM
		static bool useUWPFallback = false;
#endif

        public static DisableMediaInsertionPrompt Create()
        {
            DisableMediaInsertionPrompt prompt = new DisableMediaInsertionPrompt();
            // The SetThreadErrorMode method does not exist on Windows SDK versions
            // before 16299. This is manifested at runtime in IL2CPP as a DllNotFoundException
            // for kernel32.dll. If this happens, assume that there is no media insertion
            // prompt and continue.
#if UNITY_AOT && WIN_PLATFORM
			if (useUWPFallback)
			{
				prompt._disableSuccess = false;
				return prompt;
			}

            try
            {
#endif
                prompt._disableSuccess = Interop.Kernel32.SetThreadErrorMode(Interop.Kernel32.SEM_FAILCRITICALERRORS, out prompt._oldMode);
#if UNITY_AOT && WIN_PLATFORM
            }
            catch (DllNotFoundException)
            {
				useUWPFallback = true;
                prompt._disableSuccess = false;
            }
#endif
            return prompt;
        }

        public void Dispose()
        {
            uint ignore;
            if (_disableSuccess)
                Interop.Kernel32.SetThreadErrorMode(_oldMode, out ignore);
        }
    }
}
