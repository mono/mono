// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities
{
    using System;
    using System.Runtime;

    internal static class SharedFx
    {
        internal static bool IsFatal(Exception exception)
        {
            return Fx.IsFatal(exception);
        }

        internal static void Assert(bool condition, string messageText)
        {
            Fx.Assert(condition, messageText);
        }

        internal static void Assert(string messageText)
        {
            Fx.Assert(messageText);
        }
    }
}
