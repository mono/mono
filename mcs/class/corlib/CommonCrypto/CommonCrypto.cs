// CommonCrypto bindings for MonoMac and MonoTouch
//
// Authors:
//	Sebastien Pouliot  <sebastien@xamarin.com>
//
// Copyright 2012-2014 Xamarin Inc.

using System;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

namespace Crimson.CommonCrypto {

	// int32_t -> CommonCryptor.h
	enum CCCryptorStatus {
	    Success			= 0,
    	ParamError		= -4300,
	    BufferTooSmall	= -4301,
    	MemoryFailure	= -4302,
    	AlignmentError	= -4303,
    	DecodeError		= -4304,
    	Unimplemented	= -4305
	}
	
	// uint32_t -> CommonCryptor.h
	// note: not exposed publicly so it can stay signed
	enum CCOperation {
		Encrypt = 0, 
		Decrypt,     
	}

	// uint32_t -> CommonCryptor.h
	// note: not exposed publicly so it can stay signed
	enum CCAlgorithm {
		AES128 = 0,
		DES,        
		TripleDES,       
		CAST,       
		RC4,
		RC2,   
		Blowfish    
	}
	
	// uint32_t -> CommonCryptor.h
	// note: not exposed publicly so it can stay signed
	[Flags]
	enum CCOptions {
		None			= 0,
		PKCS7Padding	= 1,
		ECBMode			= 2
	}
	
	static class Cryptor {
		
		const string libSystem = "/usr/lib/libSystem.dylib";
		
		// size_t was changed to IntPtr for 32/64 bits size difference - even if mono is (moslty) used in 32bits only on OSX today
		// not using `nint` to be able to resue this outside (if needed)
		
		[DllImport (libSystem)]
		extern internal static CCCryptorStatus CCCryptorCreate (CCOperation op, CCAlgorithm alg, CCOptions options, /* const void* */ byte[] key, /* size_t */ IntPtr keyLength, /* const void* */ byte[] iv, /* CCCryptorRef* */ ref IntPtr cryptorRef);

		[DllImport (libSystem)]
		extern internal static CCCryptorStatus CCCryptorRelease (/* CCCryptorRef */ IntPtr cryptorRef);

		[DllImport (libSystem)]
		extern internal static CCCryptorStatus CCCryptorUpdate (/* CCCryptorRef */ IntPtr cryptorRef, /* const void* */ byte[] dataIn, /* size_t */ IntPtr dataInLength, /* void* */ byte[] dataOut, /* size_t */ IntPtr dataOutAvailable, /* size_t* */ ref IntPtr dataOutMoved);

		[DllImport (libSystem)]
		extern internal static CCCryptorStatus CCCryptorUpdate (/* CCCryptorRef */ IntPtr cryptorRef, /* const void* */ IntPtr dataIn, /* size_t */ IntPtr dataInLength, /* void* */ IntPtr dataOut, /* size_t */ IntPtr dataOutAvailable, /* size_t* */ ref IntPtr dataOutMoved);

		[DllImport (libSystem)]
		extern internal static CCCryptorStatus CCCryptorFinal (/* CCCryptorRef */ IntPtr cryptorRef, /* void* */ byte[] dataOut, /* size_t */ IntPtr dataOutAvailable, /* size_t* */ ref IntPtr dataOutMoved);

		[DllImport (libSystem)]
		extern internal static int CCCryptorGetOutputLength (/* CCCryptorRef */ IntPtr cryptorRef, /* size_t */ IntPtr inputLength, bool final);

		[DllImport (libSystem)]
		extern internal static CCCryptorStatus CCCryptorReset (/* CCCryptorRef */ IntPtr cryptorRef, /* const void* */ IntPtr iv);
		
		// helper method to reduce the amount of generate code for each cipher algorithm
		static internal IntPtr Create (CCOperation operation, CCAlgorithm algorithm, CCOptions options, byte[] key, byte[] iv)
		{
			if (key == null)
				throw new CryptographicException ("A null key was provided");
			
			// unlike the .NET framework CommonCrypto does not support two-keys triple-des (128 bits) ref: #6967
			if ((algorithm == CCAlgorithm.TripleDES) && (key.Length == 16)) {
				byte[] key3 = new byte [24];
				Buffer.BlockCopy (key, 0, key3, 0, 16);
				Buffer.BlockCopy (key, 0, key3, 16, 8);
				key = key3;
			}
			
			IntPtr cryptor = IntPtr.Zero;
			CCCryptorStatus status = Cryptor.CCCryptorCreate (operation, algorithm, options, key, (IntPtr) key.Length, iv, ref cryptor);
			if (status != CCCryptorStatus.Success)
				throw new CryptographicUnexpectedOperationException ();
			return cryptor;
		}

		// size_t was changed to IntPtr for 32/64 bits size difference - even if mono is (moslty) used in 32bits only on OSX today
		[DllImport ("/System/Library/Frameworks/Security.framework/Security")]
		unsafe extern internal static /* int */ int SecRandomCopyBytes (/* SecRandomRef */ IntPtr rnd, /* size_t */ IntPtr count, /* uint8_t* */ byte *data);
		
		unsafe static internal void GetRandom (byte[] buffer)
		{
			fixed (byte* fixed_bytes = buffer) {
				if (SecRandomCopyBytes (IntPtr.Zero, (IntPtr)buffer.Length, fixed_bytes) != 0)
					throw new CryptographicException (Marshal.GetLastWin32Error ()); // errno
			}
		}

		static internal unsafe void GetRandom (byte* data, IntPtr data_length)
		{
			if (SecRandomCopyBytes (IntPtr.Zero, data_length, data) != 0)
				throw new CryptographicException (Marshal.GetLastWin32Error ()); // errno
		}

	}
	
#if !MONOTOUCH && !XAMMAC
	static class KeyBuilder {
		static public byte[] Key (int size) 
		{
			byte[] buffer = new byte [size];
			Cryptor.GetRandom (buffer);
			return buffer;
		}
	
		static public byte[] IV (int size) 
		{
			byte[] buffer = new byte [size];
			Cryptor.GetRandom (buffer);
			return buffer;
		}
	}
#endif
}
