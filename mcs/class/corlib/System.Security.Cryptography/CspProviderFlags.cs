//
// System.Security.Cryptography CspProviderFlags enumeration
//
// Authors:
//   Thomas Neidhart <tome@sbox.tugraz.at>
//

namespace System.Security.Cryptography {

	/// <summary>
	/// CSP Provider Flags
	/// </summary>
	[Flags]
	[Serializable]
	public enum CspProviderFlags {
		UseMachineKeyStore = 1,
		UseDefaultKeyContainer,
	}
}

