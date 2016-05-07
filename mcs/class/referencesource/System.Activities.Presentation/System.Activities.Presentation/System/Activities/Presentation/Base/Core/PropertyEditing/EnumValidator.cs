//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.PropertyEditing
{
    using System;

    // <summary>
    // Class containing a single IsValid() method that ensures
    // that the given public enum is of expected value.  Use this
    // method to validate the enum value of all publicly exposed setters.
    // </summary>
    internal static partial class EnumValidator
    {
        public static bool IsValid(PropertyValueExceptionSource value)
        {
            return value == PropertyValueExceptionSource.Get || value == PropertyValueExceptionSource.Set;
        }
    }
}
