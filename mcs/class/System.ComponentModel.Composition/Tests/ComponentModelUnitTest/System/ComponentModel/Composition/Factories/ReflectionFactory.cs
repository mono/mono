// -----------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// -----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.AttributedModel;
using System.Reflection;

namespace System.ComponentModel.Composition.Factories
{
    internal static partial class ReflectionFactory
    {
        public static ParameterInfo CreateParameter()
        {
            return CreateParameter((string)null);
        }

        public static ParameterInfo CreateParameter(Type parameterType)
        {
            return new MockParameterInfo(parameterType);
        }

        public static ParameterInfo CreateParameter(string name)
        {
            return new MockParameterInfo(name);
        }
    }
}
