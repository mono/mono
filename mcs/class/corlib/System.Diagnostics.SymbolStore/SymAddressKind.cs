//
// System.Diagnostics.SymbolStore.SymAddressKind
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (c) 2002 Duco Fijma
// 

namespace System.Diagnostics.SymbolStore
{

public enum SymAddressKind {
	ILOffset = 1,
	NativeRVA = 2,
	NativeRegister = 3,
	NativeRegisterRelative = 4,
	NativeOffset = 5,
	NativeRegisterRegister = 6,
	NativeRegisterStack = 7,
	NativeStackRegister = 8,
	BitField = 9
}

}
