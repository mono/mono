//
// System.Diagnostics.SymbolStore.SymbolToken
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (c) 2002 Duco Fijma
// 

namespace System.Diagnostics.SymbolStore
{

public struct SymbolToken {

	private int _val;

	public SymbolToken (int val) { _val = val; }

	[MonoTODO]
	public override bool Equals (object obj) { return false; }

	[MonoTODO]
	public override int GetHashCode() { return 0; }

	public int GetToken() { return _val; }
	
}

}
