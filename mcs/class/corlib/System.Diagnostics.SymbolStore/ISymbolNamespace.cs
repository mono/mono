//
// System.Diagnostics.SymbolStore.ISymbolNamespace
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (c) 2002 Duco Fijma
// 

namespace System.Diagnostics.SymbolStore
{

public interface ISymbolNamespace {

	string Name {get ;}

	ISymbolNamespace[] GetNamespaces ();
	ISymbolVariable[] GetVariables ();
}

}
