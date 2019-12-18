// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;

namespace Microsoft.DotNet.XUnitExtensions
{
    /// <summary>
    /// Trace Listener for corefx Desktop test execution to avoid showing assert pop-ups and making the test fail when an Assert fails.
    /// </summary>
    public class DesktopTestTraceListener : DefaultTraceListener
    {
        /// <summary>
        /// Override of <see cref="DefaultTraceListener.Fail" /> to handle Assert failures with custom behavior.
        /// When an Assert failure happens during test execution we will rather throw a DebugAssertException so that the test fails and we have a full StackTrace.
        /// </summary>
        public override void Fail(string message, string detailMessage)
        {
            throw new DebugAssertException(message, detailMessage);
        }

        private sealed class DebugAssertException : Exception
        {
            internal DebugAssertException(string message, string detailMessage) :
                base(message + Environment.NewLine + detailMessage)
            {
            }
        }
    }
}
