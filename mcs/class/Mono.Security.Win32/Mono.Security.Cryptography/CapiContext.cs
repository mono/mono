//
// Mono.Security.Cryptography.CapiContext
//
// Authors:
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Cryptography;

namespace Mono.Security.Cryptography {

// we deal with unmanaged resources - they MUST be released after use!
public class CapiContext : IDisposable {

	// handles to CryptoAPI - they are 
	protected IntPtr providerHandle;
        
	protected CspParameters cspParams;

	// has the last call succeded ?
	protected bool lastResult;

	// Create an instance using the default CSP
	public CapiContext ()
	{
		Acquire (null);
	}

	// Create an instance using the specified CSP
	public CapiContext (CspParameters csp) 
	{
		Acquire (csp);
		// do not show user interface (CRYPT_SILENT) - if UI is required then the function fails.
		lastResult = CryptoAPI.CryptAcquireContextA (ref providerHandle, cspParams.KeyContainerName, cspParams.ProviderName, cspParams.ProviderType, CryptoAPI.CRYPT_SILENT);
	}

	~CapiContext () 
	{
		Dispose ();
	}

	public int Error {
		get { return CryptoAPI.GetLastError(); }
	}

	public IntPtr Handle {
		get { return providerHandle; }
	}

	public bool Result {
		get { return lastResult; }
	}

	internal bool InternalResult {
		set { lastResult = value; }
	}

	private void Acquire (CspParameters csp) 
	{
		providerHandle = IntPtr.Zero;
		if (csp == null) {
			// default parameters
			cspParams = new CspParameters ();
		}
		else {
			// keep of copy of the parameters
			cspParams = new CspParameters (csp.ProviderType, csp.ProviderName, csp.KeyContainerName);
			cspParams.KeyNumber = csp.KeyNumber;
			cspParams.Flags = csp.Flags;
		}
		// do not show user interface (CRYPT_SILENT) -  if UI is required then the function fails.
		lastResult = CryptoAPI.CryptAcquireContextA (ref providerHandle, cspParams.KeyContainerName, cspParams.ProviderName, cspParams.ProviderType, 0);
	}

	// release unmanaged resources
	public void Dispose () 
	{
		if (providerHandle != IntPtr.Zero) {
			lastResult = CryptoAPI.CryptReleaseContext (providerHandle, 0);
			providerHandle = IntPtr.Zero;
		}
	}
}

}
