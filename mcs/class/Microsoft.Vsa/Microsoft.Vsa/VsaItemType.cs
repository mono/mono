//
// VsaItemType.cs:
//
// Author: Cesar Octavio Lopez Nataren <cesar@ciencias.unam.mx>
//

namespace Microsoft.Vsa
{
	using System;


	[Serializable]
	public enum VsaItemType {
		AppGlobal = 1,
		Code = 2,
		Reference = 0,
	}
}
