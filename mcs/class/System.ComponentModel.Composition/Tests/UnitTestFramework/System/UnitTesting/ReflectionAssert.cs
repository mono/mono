// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace System.UnitTesting
{
    // Unfortunately, you can't rely on reference equality for MemberInfo and ParameterInfo
    // objects because, you may get different instances representing the same members depending
    // on the type that the member was retrieived from.

    public static class ReflectionAssert
    {
        public static void AreSame(MemberInfo expected, MemberInfo actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }

            Assert.AreEqual(expected.MetadataToken, actual.MetadataToken);
            Assert.AreSame(expected.Module, actual.Module);
            Assert.AreEqual(expected.MemberType, actual.MemberType);            
        }

        public static void AreSame(ParameterInfo expected, ParameterInfo actual)
        {
            if (expected == null && actual == null)
            {
                return;
            }

            ReflectionAssert.AreSame(expected.Member, actual.Member);
            Assert.AreEqual(expected.MetadataToken, actual.MetadataToken);
        }
    }
}
