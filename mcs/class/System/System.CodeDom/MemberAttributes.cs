//
// System.CodeDom MemberAttributes Enum implementation
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//   Daniel Stodden (stodden@in.tum.de)
//
// (C) 2001 Ximian, Inc.
//

using System.Runtime.InteropServices;

namespace System.CodeDom {

	[Serializable]
	[ComVisible(true)]
	public enum MemberAttributes {
		Abstract =		0x00000001,
		Final =			0x00000002,
		Static =		0x00000003,
		Override =		0x00000004,
		Const =			0x00000005,
		ScopeMask =		0x0000000F,

		New =			0x00000010,
		VTableMask =		0x000000F0,

		Overloaded =		0x00000100,

		Assembly =		0x00001000, // internal
		FamilyAndAssembly =	0x00002000, // protected AND internal
		Family =		0x00003000, // protected
		FamilyOrAssembly =	0x00004000, // protected internal
		Private =		0x00005000, // private
		Public =		0x00006000, // public
		AccessMask =		0x0000F000
	}
}
