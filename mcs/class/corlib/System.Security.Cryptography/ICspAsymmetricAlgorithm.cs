//
// ICspAsymmetricAlgorithm.cs: interface for CSP based asymmetric algorithm
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

#if NET_1_2

using System;

namespace System.Security.Cryptography {

	public interface ICspAsymmetricAlgorithm {

		byte[] ExportCspBlob (bool includePrivateParameters);

		void ImportCspBlob (byte[] rawData);

		CspKeyContainerInfo CspKeyContainerInfo { get; }
	}
}

#endif