//
// System.Security.Cryptography CspProviderFlags enumeration
//
// Authors:
//   Thomas Neidhart <tome@sbox.tugraz.at>
//
// (C) 2004 Novell (http://www.novell.com)
//

namespace System.Security.Cryptography {

	[Flags]
	[Serializable]
	public enum CspProviderFlags {
		UseMachineKeyStore = 1,
		UseDefaultKeyContainer = 2
	}
}

