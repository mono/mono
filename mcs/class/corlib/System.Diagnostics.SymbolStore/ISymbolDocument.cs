//
// System.Diagnostics.SymbolStore.ISymbolDocument
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (c) 2002 Duco Fijma
// 

namespace System.Diagnostics.SymbolStore
{

public interface ISymbolDocument {
	
	Guid CheckSumAlgorithmId {get; }
	Guid DocumentType {get; }
	bool HasEmbeddedSource {get; }
	Guid Language {get; }	
	Guid LanguageVendor {get; }
	int SourceLength {get; }
	string URL {get; }
	

	int FindClosestLine (int line);
	byte[] GetCheckSum ();
	byte[] GetSourceRange (int startLine, int startColumn, int endLine, int endColumn);

}

}
