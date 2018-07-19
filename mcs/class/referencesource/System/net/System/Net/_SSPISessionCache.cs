/*++
Copyright (c) Microsoft Corporation

Module Name:

    _SspiSessionCache.cs

Abstract:
    The file implements trivial SSPI credential caching mechanism based on lru list


Author:

    Alexei Vopilov    20-Oct-2004

Revision History:


--*/
namespace System.Net.Security {
using System.Net;
using System.Threading;
using System.Collections;


    //
    // Implements delayed SSPI handle release, like a finalizable object though the handles are kept alive until being pushed out
    // by the newly incoming ones.
    //
    internal static class SSPIHandleCache
    {
          private const int                         c_MaxCacheSize = 0x1F;  // must a (power of 2) - 1
          private static SafeCredentialReference[]  _CacheSlots = new SafeCredentialReference[c_MaxCacheSize+1];
          private static int                        _Current = -1;

          internal static void CacheCredential(SafeFreeCredentials newHandle)
          {
                try {
                    SafeCredentialReference newRef = SafeCredentialReference.CreateReference(newHandle);
                    if (newRef == null)
                        return;
                    unchecked
                    {
                        int index = Interlocked.Increment(ref _Current) & c_MaxCacheSize;
                        newRef = Interlocked.Exchange<SafeCredentialReference>(ref _CacheSlots[index], newRef);
                    }
                    if (newRef != null)
                        newRef.Close();
                }
                catch(Exception e) {
                    if (!NclUtilities.IsFatal(e)){
                        GlobalLog.Assert("SSPIHandlCache", "Attempted to throw: " + e.ToString());
                    }
                }
          }
    }
}
