//
// System.Diagnostics.SymbolStore.ISymbolWriter
//
// Author:
//   Duco Fijma (duco@lorentz.xs4all.nl)
//
//   (c) 2002 Duco Fijma
// 

using System.Reflection;

namespace System.Diagnostics.SymbolStore
{

public interface ISymbolWriter {

	void Close ();
	void CloseMethod ();
	void CloseNamespace ();
	void CloseScope (int endOffset);
	ISymbolDocumentWriter DefineDocument(
		string url,
		Guid language,
		Guid languageVendor,
		Guid documentType);
	void DefineField (
		SymbolToken parent,
		string name,
		FieldAttributes attributes,
		byte[] signature,
		SymAddressKind addrKind,
		int addr1,
		int addr2,
		int addr3);
	void DefineGlobalVariable (
		string name,
		FieldAttributes attributes,
		byte[] signature,
		SymAddressKind addrKind,
		int addr1,
		int addr2,
		int addr3);
	void DefineLocalVariable (
		string name,
		FieldAttributes attributes,
		byte[] signature,
		SymAddressKind addrKind,
		int addr1,
		int addr2,
		int addr3,
		int startOffset,
		int endOffset);
	void DefineParameter (
		string name,
		ParameterAttributes attributes,
		int sequence,
		SymAddressKind addrKind,
		int addr1,
		int addr2,
		int addr3);
	void DefineSequencePoints (
		ISymbolDocumentWriter document,
		int[] offsets,
		int[] lines,
		int[] columns,
		int[] endLines,
		int[] endColumns);
	void Initialize (IntPtr emitter, string filename, bool fFullBuild);
	void OpenMethod (SymbolToken method);
	void OpenNamespace (string name);
	int OpenScope (int startOffset);
	void SetMethodSourceRange (
		ISymbolDocumentWriter startDoc,
		int startLine,
		int startColumn,
		ISymbolDocumentWriter endDoc,
		int endLine,
		int endColumn);
	void SetScopeRange (int scopeID, int startOffset, int endOffset);
	void SetSymAttribute (SymbolToken parent, string name, byte[] data);
	void SetUnderlyingWriter (IntPtr underlyingWriter);
	void SetUserEntryPoint (SymbolToken entryMethod);
	void UsingNamespace (string fullName);
	

}

}
