using System;

namespace System.Diagnostics.SymbolStore
{

public interface ISymbolDocumentWriter {

	void SetCheckSum (Guid algorithmId, byte[] checkSum);
	void SetSource (byte[] source);

}

}
