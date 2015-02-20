//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

// Compiler Error CS3001
// Argument type 'uint' is not CLS-compliant
#pragma warning disable 3001

namespace System.Activities.Presentation.Sqm
{
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly, Justification = "False positive for SQM")]
    public interface IVSSqmService
    {
        // Session: A session starts when VS launches and ends when VS is closed.

        // Simple data point: Only one value will be saved per session. The value
        //  that is set last for the same data point will win.

        // Stream: Multiple values can be saved per session.

        // Set an UINT value for a simple data point
        [SuppressMessage(FxCop.Category.Naming, FxCop.Rule.IdentifiersShouldBeSpelledCorrectly, Justification = "In order to keep consistent with the same method in IVSSqm")]
        void SetDatapoint(int dataPointId, uint value);
        // Add a UINT value to a stream
        void AddItemToStream(int dataPointId, uint value);
        // Add an UINT array to stream, essentially creating a record
        void AddArrayToStream(int dataPointId, uint[] data, int count);
    }
}
