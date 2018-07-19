// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  ISymbolDocumentWriter
**
**
** Represents a document referenced by a symbol store. A document is
** defined by a URL and a document type GUID. Document source can
** optionally be stored in the symbol store.
**
** 
===========================================================*/
namespace System.Diagnostics.SymbolStore {
    
    using System;
    
    // Interface does not need to be marked with the serializable attribute
    [System.Runtime.InteropServices.ComVisible(true)]
    public interface ISymbolDocumentWriter
    {
        // SetSource will store the raw source for a document into the
        // symbol store. An array of unsigned bytes is used instead of
        // character data to accommodate a wider variety of "source".
        void SetSource(byte[] source);
    
        // Check sum support.
        void SetCheckSum(Guid algorithmId, byte[] checkSum);
    }
}
