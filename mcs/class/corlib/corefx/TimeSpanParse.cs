// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Text;

namespace System.Globalization
{
    partial class TimeSpanParse
    {
        internal static void ValidateStyles (TimeSpanStyles style, String parameterName) 
        {
            if (style != TimeSpanStyles.None && style != TimeSpanStyles.AssumeNegative)
                throw new ArgumentException (Environment.GetResourceString ("Argument_InvalidTimeSpanStyles"), parameterName);
        }
    }
}