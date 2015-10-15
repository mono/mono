//------------------------------------------------------------------------------
// <copyright file="QilInvokeLateBound.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// A function invocation node which reperesents a call to an late bound function.
    /// </summary>
    internal class QilInvokeLateBound : QilBinary {

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilInvokeLateBound(QilNodeType nodeType, QilNode name, QilNode arguments) : base(nodeType, name, arguments) {
        }


        //-----------------------------------------------
        // QilInvokeLateBound methods
        //-----------------------------------------------

        public QilName Name {
            get { return (QilName) Left; }
            set { Left = value; }
        }

        public QilList Arguments {
            get { return (QilList) Right; }
            set { Right = value; }
        }
    }
}
