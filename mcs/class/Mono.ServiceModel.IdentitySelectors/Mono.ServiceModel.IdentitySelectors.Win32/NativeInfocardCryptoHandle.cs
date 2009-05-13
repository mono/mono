//
// NativeInfocardCryptoHandle.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2007 Novell, Inc.  http://www.novell.com
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Runtime.InteropServices;
using System.Xml;

namespace Mono.ServiceModel.IdentitySelectors.Win32
{
	// see http://msdn2.microsoft.com/en-us/library/aa702727.aspx

	[StructLayout (LayoutKind.Sequential)]
	class NativeInfocardCryptoHandle
	{
		// This field order must be fixed for win32 API interop:
		NativeInfocardHandleType handle_type;
		long expiration;
		IntPtr parameters;

		public long Expiration {
			get { return expiration; }
		}

		public AsymmetricSecurityKey GetAsymmetricKey ()
		{
			switch (handle_type) {
			case NativeInfocardHandleType.Asymmetric:
				NativeAsymmetricCryptoParameters a = (NativeAsymmetricCryptoParameters) Marshal.PtrToStructure (parameters, typeof (NativeAsymmetricCryptoParameters));
				return new AsymmetricProofTokenSecurityKey (a, this);
			}
			throw new NotImplementedException ();
		}
	}

	[StructLayout (LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	struct NativeAsymmetricCryptoParameters
	{
		int key_size;
		string encalg;
		string sigalg;

		public int KeySize {
			get { return key_size; }
		}

		public string EncryptionAlgorithm {
			get { return encalg; }
		}

		public string SignatureAlgorithm {
			get { return sigalg; }
		}
	}

#pragma warning disable 169
	[StructLayout (LayoutKind.Sequential)]
	struct NativeSymmetricCryptoParameters
	{
		int key_size;
		int block_size;
		int feedback_size;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct NativeTransformCryptoParameters
	{
		int input_block_size;
		int output_block_size;
		bool multi_block_supported;
		bool reusable;
	}

	[StructLayout (LayoutKind.Sequential)]
	struct NativeHashCryptoParameters
	{
		int hash_size;
		NativeTransformCryptoParameters transform;
	}

#pragma warning restore 169
}
