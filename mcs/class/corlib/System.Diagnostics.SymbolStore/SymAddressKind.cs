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

[MonoTODO("find 'real' values of enum values")]
public enum SymAddressKind {
	BitField = 9,
	ILOffset = 1,
	NativeOffset = 5,
	NativeRegister = 3,
	NativeRegisterRegister = 6,
	NativeRegisterRelative = 4,
	NativeRegisterStack = 7,
	NativeRVA = 2,
	NativeStackRegister = 8
}

}
