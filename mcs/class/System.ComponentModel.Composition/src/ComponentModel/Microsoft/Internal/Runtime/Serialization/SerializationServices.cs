// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
#if !SILVERLIGHT

using System;
using System.Runtime.Serialization;

namespace Microsoft.Internal.Runtime.Serialization
{
    internal static class SerializationServices
    {
        public static T GetValue<T>(this SerializationInfo info, string name)
        {
            Assumes.NotNull(info, name);

            return (T)info.GetValue(name, typeof(T));
        }
    }
}

#endif