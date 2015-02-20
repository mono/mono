// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Debugger
{
    using System.Collections.Generic;

    internal partial class CharacterSpottingTextReader : ICharacterSpottingTextReaderForUnitTest
    {
        int ICharacterSpottingTextReaderForUnitTest.CurrentLine
        {
            get { return this.currentLine; }
        }

        int ICharacterSpottingTextReaderForUnitTest.CurrentPosition
        {
            get { return this.currentPosition; }
        }

        List<DocumentLocation> ICharacterSpottingTextReaderForUnitTest.StartBrackets
        {
            get { return this.startAngleBrackets; }
        }

        List<DocumentLocation> ICharacterSpottingTextReaderForUnitTest.EndBrackets
        {
            get { return this.endAngleBrackets; }
        }

        List<DocumentLocation> ICharacterSpottingTextReaderForUnitTest.SingleQuotes
        {
            get { return this.singleQuotes; }
        }

        List<DocumentLocation> ICharacterSpottingTextReaderForUnitTest.DoubleQuotes
        {
            get { return this.doubleQuotes; }
        }
    }
}
