//
// System.CodeDom MemberAttributes Enum implementation
//
// Author:
//   Sean MacIsaac (macisaac@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

namespace System.CodeDom {
	public enum MemberAttributes {
		Abstract = 0x1,
		Final,
		Static,
		Override,
		Const,
		New = 0x10,
		Overloaded = 0x100,
		Assembly = 0x1000,
		FamilyAndAssembly = 0x2000,
		Family = 0x3000,
		FamilyOrAssembly = 0x4000,
		Private = 0x5000,
		Public = 0x6000,

		ScopeMask = 0xf,
		VTableMask = 0xf0,
		AccessMask = 0xf000,
	}
}
