//
// System.Runtime.InteropServices.AssemblyRegistrationFlags.cs
//
// Author:
//   Kevin Winchester (kwin@ns.sympatico.ca)
//
// (C) 2002 Kevin Winchester
//

namespace System.Runtime.InteropServices
{
	[Flags]
	[Serializable]
	public enum AssemblyRegistrationFlags {
		None = 0,
		SetCodeBase = 0,
	}
}
