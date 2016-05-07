// -------------------------------------------------------------------
// Description:
// Provides helper functions handling move and hit detection 
// tolerances.
//
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------

//Cider comment:
//  - Class has many members that we don't use. 
//  - I have removed them to avoid FXCop warning and lowering code coverage 

//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework
{
    using System;
    using System.Windows;
    using System.Windows.Media.Media3D;
    internal static class Tolerances
    {
        //Cider private static readonly double ZeroThreshold = 2.2204460492503131e-015; 
        private const double ZeroThreshold = 2.2204460492503131e-015;

        public static bool NearZero(double d)
        {
            return Math.Abs(d) < Tolerances.ZeroThreshold;
        }

    }
}
