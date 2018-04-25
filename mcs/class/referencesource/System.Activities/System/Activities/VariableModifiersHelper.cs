//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.ComponentModel;
    using System.Runtime;

    static class VariableModifiersHelper
    {
        static bool IsDefined(VariableModifiers modifiers)
        {
            return (modifiers == VariableModifiers.None ||
                ((modifiers & (VariableModifiers.Mapped | VariableModifiers.ReadOnly)) == modifiers));
        }

        public static bool IsReadOnly(VariableModifiers modifiers)
        {
            return (modifiers & VariableModifiers.ReadOnly) == VariableModifiers.ReadOnly;
        }

        public static bool IsMappable(VariableModifiers modifiers)
        {
            return (modifiers & VariableModifiers.Mapped) == VariableModifiers.Mapped;
        }

        public static void Validate(VariableModifiers modifiers, string argumentName)
        {
            if (!IsDefined(modifiers))
            {
                throw FxTrace.Exception.AsError(
                    new InvalidEnumArgumentException(argumentName, (int)modifiers, typeof(VariableModifiers)));
            }
        }
    }
}
