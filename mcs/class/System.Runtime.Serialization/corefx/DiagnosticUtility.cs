// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Runtime.Serialization
{
    partial class DiagnosticUtility
    {
        internal static bool IsFatal(Exception exception)
        {
            while (exception != null)
            {
                // These exceptions aren't themselves fatal, but since the CLR uses them to wrap other exceptions,
                // we want to check to see whether they've been used to wrap a fatal exception.  If so, then they
                // count as fatal.
                if (exception is TypeInitializationException)
                {
                    exception = exception.InnerException;
                }
                else
                {
                    break;
                }
            }

            return false;
        }
    }
}