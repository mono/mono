//------------------------------------------------------------------------------
// <copyright file="QilInvoke.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// A function invocation node which represents a call to a Qil functions.
    /// </summary>
    internal class QilInvoke : QilBinary {

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilInvoke(QilNodeType nodeType, QilNode function, QilNode arguments) : base(nodeType, function, arguments) {
        }


        //-----------------------------------------------
        // QilInvoke methods
        //-----------------------------------------------

        public QilFunction Function {
            get { return (QilFunction) Left; }
            set { Left = value; }
        }

        public QilList Arguments {
            get { return (QilList) Right; }
            set { Right = value; }
        }
    }
}
