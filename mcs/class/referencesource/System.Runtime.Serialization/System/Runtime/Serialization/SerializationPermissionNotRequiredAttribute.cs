//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Runtime.Serialization
{
#if ENABLE_PARTIALTRUST
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class SerializationPermissionNotRequiredAttribute : Attribute 
    {
    }
#endif
}
