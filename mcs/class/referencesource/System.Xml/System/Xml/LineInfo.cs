//------------------------------------------------------------------------------
// <copyright file="LineInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Xml {

    internal struct LineInfo {
        internal int lineNo;
        internal int linePos;

        public LineInfo( int lineNo, int linePos ) {
            this.lineNo = lineNo;
            this.linePos = linePos;
        }

        public void Set( int lineNo, int linePos ) {
            this.lineNo = lineNo;
            this.linePos = linePos;
        }
    }
}
