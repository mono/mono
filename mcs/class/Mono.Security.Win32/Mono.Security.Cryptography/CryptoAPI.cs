//
// Mono.Security.Cryptography.CryptoAPI
//
// Authors:
//	Sebastien Pouliot (spouliot@motus.com)
//
// Copyright (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Runtime.InteropServices;

namespace Mono.Security.Cryptography {

internal class CryptoAPI {

	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptAcquireContextA (ref IntPtr phProv, string pszContainer, string pszProvider, int dwProvType, uint dwFlags);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptCreateHash (IntPtr hProv, uint Algid, IntPtr hKey, uint dwFlags, ref IntPtr phHash);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptDecrypt (IntPtr hKey, IntPtr hHash, bool Final, uint dwFlags, byte[] pbData, ref uint pdwDataLen);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptDestroyHash (IntPtr hHash);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptDestroyKey (IntPtr hKey);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptEncrypt (IntPtr hKey, IntPtr hHash, bool Final, uint dwFlags, byte[] pbData, ref uint pdwDataLen, uint dwBufLen);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptGenKey (IntPtr hProv, uint Algid, uint dwFlags, ref IntPtr phKey);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptGenRandom (IntPtr hProv, uint dwLen, byte[] pbBuffer);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptGetHashParam (IntPtr hHash, uint dwParam, byte[] pbData, ref uint pdwDataLen, uint dwFlags);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptHashData (IntPtr hHash, byte[] pbData, uint dwDataLen, uint dwFlags);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptImportKey (IntPtr hProv, byte[] pbData, uint dwDataLen, IntPtr hPubKey, uint dwFlags, ref IntPtr phKey);
	[DllImport ("advapi32.dll", CallingConvention=CallingConvention.StdCall, SetLastError=true)]
	public static extern bool CryptReleaseContext (IntPtr hProv, uint dwFlags);

	public static readonly uint CRYPT_VERIFYCONTEXT  = 0xF0000000;
	public static readonly uint CRYPT_NEWKEYSET      = 0x00000008;
	public static readonly uint CRYPT_DELETEKEYSET   = 0x00000010;
	public static readonly uint CRYPT_MACHINE_KEYSET = 0x00000020;
	public static readonly uint CRYPT_SILENT         = 0x00000040;

	public static readonly int PROV_RSA_FULL         = 1;

	public static readonly uint HP_HASHVAL           = 0x0002;

	public static readonly int CALG_MD2  = 0x8001;
	public static readonly int CALG_MD4  = 0x8002;
	public static readonly int CALG_MD5  = 0x8003;
	public static readonly int CALG_SHA1 = 0x8004;

	// just so we don't have to add System.Runtime.InteropServices
	// in every file
	static public int GetLastError () 
	{
		return Marshal.GetLastWin32Error ();
	}
}

}
