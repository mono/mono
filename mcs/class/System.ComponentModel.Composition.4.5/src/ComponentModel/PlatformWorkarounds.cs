// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

#if !FEATURE_SERIALIZATION
namespace System
{
    [Conditional("NOT_FEATURE_SERIALIZATION")]    // Trick so that the attribute is never actually applied
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate, Inherited = false)]
    internal sealed class SerializableAttribute : Attribute
    {
    }
}
#endif //!FEATURE_SERIALIZATION

#if !FEATURE_LEGACYCOMPONENTMODEL
namespace System.ComponentModel
{
    [Conditional("NOT_FEATURE_LEGACYCOMPONENTMODEL")]    // Trick so that the attribute is never actually applied
    internal sealed class LocalizableAttribute : Attribute
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "isLocalizable")]
        public LocalizableAttribute(bool isLocalizable)
        {
        }
    }
}
#endif //!FEATURE_LEGACYCOMPONENTMODEL

// This is temporary as contracts should actually be everywhere. Once CORE_CLR adds back this attribute, this will be gone
#if FEATURE_MISSINGCONTRACTARGUMENTVALIDATOR
namespace System.Diagnostics.Contracts
{
    [Conditional("NOT_FEATURE_MISSINGCONTRACTARGUMENTVALIDATOR")]    // Trick so that the attribute is never actually applied
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    internal sealed class ContractArgumentValidatorAttribute : Attribute
    {
    }
}
#endif //FEATURE_MISSINGCONTRACTARGUMENTVALIDATOR
