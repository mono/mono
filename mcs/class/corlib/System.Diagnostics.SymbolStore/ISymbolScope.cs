//
// System.Diagnostics.SymbolStore.ISymbolScope
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (c) 2002 Duco Fijma
// 

namespace System.Diagnostics.SymbolStore
{

public interface ISymbolScope {

	int EndOffset {get ;}
	ISymbolMethod Method {get; }
	ISymbolScope Parent {get ;}
	int StartOffset {get ;}

	ISymbolScope[] GetChildren ();
	ISymbolVariable[] GetLocals ();
	ISymbolNamespace[] GetNamespaces ();

}

}
