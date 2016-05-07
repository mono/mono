//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.FreeFormEditing
{
    using System;
    using System.Windows;

    internal static class DoubleExtensions
    {
        public static bool IsEqualTo(this double a, double b)
        {
            return Math.Abs(a - b) < DesignerGeometryHelper.EPS;
        }

        public static bool IsNoGreaterThan(this double a, double b)
        {
            return a <= b + DesignerGeometryHelper.EPS;
        }

        public static bool IsNoLessThan(this double a, double b)
        {
            return a >= b - DesignerGeometryHelper.EPS;
        }
    }
}
