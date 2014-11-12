// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
////////////////////////////////////////////////////////////////////////////////
////////////////////////////////////////////////////////////////////////////////
//
// IReflectableType is an interface that is implemented by a Type produced 
// by a ReflectionContext
// 
// <OWNER>[....]</OWNER>

//
namespace System.Reflection {
    
    using System;
    
    public interface IReflectableType {
        TypeInfo GetTypeInfo();
    }
}
