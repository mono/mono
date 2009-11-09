// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.ComponentModel.Composition.ReflectionModel;
using System.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace System.UnitTesting
{
    internal static class ReflectionItemAssert
    {
        public static void AreSame(ReflectionItem expected, ReflectionItem actual)
        {
            switch (expected.ItemType)
            {
                case ReflectionItemType.Property:
                {
                    ReflectionProperty expectedProperty = (ReflectionProperty)expected;
                    ReflectionProperty actualProperty = (ReflectionProperty)actual;

                    ReflectionAssert.AreSame(expectedProperty.UnderlyingGetMethod, actualProperty.UnderlyingGetMethod);
                    ReflectionAssert.AreSame(expectedProperty.UnderlyingSetMethod, actualProperty.UnderlyingSetMethod);
                    return;
                }

                case ReflectionItemType.Parameter:
                {
                    ReflectionParameter expectedParameter = (ReflectionParameter)expected;
                    ReflectionParameter actualParameter = (ReflectionParameter)actual;

                    ReflectionAssert.AreSame(expectedParameter.UnderlyingParameter, actualParameter.UnderlyingParameter);
                    return;
                }

                default:
                {
                    ReflectionMember expectedMember = (ReflectionMember)expected;
                    ReflectionMember actualMember = (ReflectionMember)actual;

                    ReflectionAssert.AreSame(expectedMember.UnderlyingMember, actualMember.UnderlyingMember);
                    return;
                }
            }
        }
    }    
}
