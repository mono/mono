//
// System.Security.Cryptography CspParameters.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// (C) 2004 Novell (http://www.novell.com)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Cryptography {

	public sealed class CspParameters {

		private CspProviderFlags _Flags;
	
		public CspParameters () 
			: this (1)
		{
		}
		
		public CspParameters (int dwTypeIn) 
			: this (dwTypeIn, null) 
		{
		}
		
		public CspParameters (int dwTypeIn, string strProviderNameIn)
			: this (dwTypeIn, null, null)
		{
		}
		
		public CspParameters (int dwTypeIn, string strProviderNameIn, string strContainerNameIn)
		{
			ProviderType = dwTypeIn;
			ProviderName = strProviderNameIn;
			KeyContainerName = strContainerNameIn;
			
			// not defined in specs, only tested from M$ impl
			KeyNumber = -1;
		}
		
		public string KeyContainerName;
		
		public int KeyNumber;
		
		public string ProviderName;
		
		public int ProviderType;
		
		public CspProviderFlags Flags {
			get { return _Flags; }
			set { _Flags = value; }
		}
	}
}
