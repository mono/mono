// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System
{
    public static class TypeExtensions
    {
        public static MemberInfo GetSingleMember(this Type type, string name)
        {
            Assert.IsNotNull(type);
            Assert.IsNotNull(name);

            return type.GetMember(name).Single();
        }
    }
}
