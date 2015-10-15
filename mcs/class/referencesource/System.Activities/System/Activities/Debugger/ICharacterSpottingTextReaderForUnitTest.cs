// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Collections.Generic;

    internal interface ICharacterSpottingTextReaderForUnitTest
    {
        int CurrentLine { get; }

        int CurrentPosition { get; }

        List<DocumentLocation> StartBrackets { get; }

        List<DocumentLocation> EndBrackets { get; }

        List<DocumentLocation> SingleQuotes { get; }

        List<DocumentLocation> DoubleQuotes { get; }
    }
}