//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;

    // <summary>
    // Class containing a single IsValid() method that ensures
    // that the given public enum is of expected value.  Use this
    // method to validate the enum value of all publicly exposed setters.
    // </summary>
    internal static partial class EnumValidator
    {
        public static bool IsValid(CreateOptions value)
        {
            return value == CreateOptions.None || value == CreateOptions.InitializeDefaults;
        }

    }
}
