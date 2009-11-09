// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Reflection;
using System.Collections.Generic;

namespace System.UnitTesting
{
    public static class TestServices
    {
        public static string GenerateRandomString()
        {
            return Guid.NewGuid().ToString().Replace('-', '_');
        }

        public static IEnumerable<TEnum> GetEnumValues<TEnum>() where TEnum : struct
        {   // Silverlight 2.0 does not have Enum.GetValues() 
            // so we need to write our own

            foreach (FieldInfo field in typeof(TEnum).GetFields())
            {
                if (!field.IsLiteral)
                    continue;

                yield return (TEnum)field.GetRawConstantValue();
            }
        }
    }
}