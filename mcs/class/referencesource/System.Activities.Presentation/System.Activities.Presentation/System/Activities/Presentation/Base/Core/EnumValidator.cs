//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;

    // <summary>
    // Class containing a single IsValid() method that ensures
    // that the given public enum is of expected value.  Use this
    // method to validate the enum value of all publicly exposed setters.
    // </summary>
    internal static partial class EnumValidator
    {
        public static bool IsValid(OrderTokenPrecedence value)
        {
            return value == OrderTokenPrecedence.Before || value == OrderTokenPrecedence.After;
        }

        public static bool IsValid(OrderTokenConflictResolution value)
        {
            return value == OrderTokenConflictResolution.Win || value == OrderTokenConflictResolution.Lose;
        }
    }
}
