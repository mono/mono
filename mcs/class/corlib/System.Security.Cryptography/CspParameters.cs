//
// System.Security.Cryptography CspParameters.cs
//
// Author:
//   Thomas Neidhart (tome@sbox.tugraz.at)
//
// (C) 2004 Novell (http://www.novell.com)
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
