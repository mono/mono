// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.IO;

namespace System.Xaml
{
    internal class LineInfo
    {
        #region Fields
        private int _lineNumber;
        private int _linePosition;
        #endregion

        #region Constructors
        internal LineInfo(int lineNumber, int linePosition)
        {
            _lineNumber = lineNumber;
            _linePosition = linePosition;
        }
        #endregion

        #region Public Properties
        public int LineNumber
        {
            get { return _lineNumber; }
        }

        public int LinePosition
        {
            get { return _linePosition; }            
        }
        #endregion
    }
}
