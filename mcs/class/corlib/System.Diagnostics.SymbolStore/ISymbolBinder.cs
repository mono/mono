//
// System.Diagnostics.SymbolStore.ISymbolBinder
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (c) 2002 Duco Fijma
// 

namespace System.Diagnostics.SymbolStore
{

public interface ISymbolBinder {

	ISymbolReader GetReader (int importer, string filename, string searchPath);

}

}
