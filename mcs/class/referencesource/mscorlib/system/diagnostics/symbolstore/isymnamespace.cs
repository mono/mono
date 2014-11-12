// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** Class:  ISymbolNamespace
**
**
** Represents a namespace within a symbol reader.
**
** 
===========================================================*/
namespace System.Diagnostics.SymbolStore {
    
    using System;
    
    // Interface does not need to be marked with the serializable attribute
[System.Runtime.InteropServices.ComVisible(true)]
    public interface ISymbolNamespace
    {
        // Get the name of this namespace
        String Name { get; }
    
        // Get the children of this namespace
        ISymbolNamespace[] GetNamespaces();
    
        // Get the variables in this namespace
        ISymbolVariable[] GetVariables();
    }
}
