//
// System.Diagnostics.SymbolStore.ISymbolReader
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (c) 2002 Duco Fijma
// 

namespace System.Diagnostics.SymbolStore
{

public interface ISymbolReader {

	SymbolToken UserEntryPoint {get; }

	ISymbolDocument GetDocument (
		string url,
		Guid language,
		Guid languageVendor,
		Guid documentType);
	ISymbolDocument[] GetDocuments ();
	ISymbolVariable[] GetGlobalVariables ();

	ISymbolMethod GetMethod (SymbolToken method);
	ISymbolMethod GetMethod (SymbolToken method, int version);

	ISymbolMethod GetMethodFromDocumentPosition (
		ISymbolDocument document,
		int line,
		int column);
	ISymbolNamespace[] GetNamespaces ();
	byte[] GetSymAttribute (SymbolToken parent, string name);
	ISymbolVariable[] GetVariables (SymbolToken parent);

}

}
