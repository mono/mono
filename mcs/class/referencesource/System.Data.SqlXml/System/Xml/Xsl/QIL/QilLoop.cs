//------------------------------------------------------------------------------
// <copyright file="QilLoop.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil operators that introduce iterators (Loop, Filter, etc.).
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilLoop : QilBinary {

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilLoop(QilNodeType nodeType, QilNode variable, QilNode body) : base(nodeType, variable, body) {
        }


        //-----------------------------------------------
        // QilLoop methods
        //-----------------------------------------------

        public QilIterator Variable {
            get { return (QilIterator) Left; }
            set { Left = value; }
        }

        public QilNode Body {
            get { return Right; }
            set { Right = value; }
        }
    }
}
