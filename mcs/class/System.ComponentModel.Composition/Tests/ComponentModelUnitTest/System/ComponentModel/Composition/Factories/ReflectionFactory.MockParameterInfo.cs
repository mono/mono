// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.AttributedModel;
using System.Reflection;
using System.Linq;

namespace System.ComponentModel.Composition.Factories
{
    partial class ReflectionFactory
    {
        private class MockParameterInfo : ParameterInfo
        {
            private readonly string _name;
            private readonly Type _parameterType = typeof(string);

            public MockParameterInfo(Type parameterType)
            {
                _parameterType = parameterType;
            }

            public MockParameterInfo(string name)
            {
                _name = name;
            }

            public override string Name
            {
                get { return _name; }
            }

            public override Type ParameterType
            {
                get { return _parameterType; }
            }

            public override MemberInfo Member
            {
                get { return typeof(object).GetConstructors().First(); }
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return (object[])Array.CreateInstance(attributeType, 0);
            }
        }
    }
}
