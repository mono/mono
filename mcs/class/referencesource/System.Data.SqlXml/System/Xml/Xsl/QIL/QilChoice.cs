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
    /// View over a Qil choice operator.
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilChoice : QilBinary {

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilChoice(QilNodeType nodeType, QilNode expression, QilNode branches) : base(nodeType, expression, branches) {
        }


        //-----------------------------------------------
        // QilChoice methods
        //-----------------------------------------------

        public QilNode Expression {
            get { return Left; }
            set { Left = value; }
        }

        public QilList Branches {
            get { return (QilList) Right; }
            set { Right = value; }
        }
    }
}
