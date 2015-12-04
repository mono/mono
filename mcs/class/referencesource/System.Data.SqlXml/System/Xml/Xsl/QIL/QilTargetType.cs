//------------------------------------------------------------------------------
// <copyright file="QilTargetType.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil operator having two children, the second of which is a literal type.
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilTargetType : QilBinary {

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilTargetType(QilNodeType nodeType, QilNode expr, QilNode targetType) : base(nodeType, expr, targetType) {
        }


        //-----------------------------------------------
        // QilTargetType methods
        //-----------------------------------------------

        public QilNode Source {
            get { return Left; }
            set { Left = value; }
        }

        public XmlQueryType TargetType {
            get { return (XmlQueryType) ((QilLiteral) Right).Value; }
            set { ((QilLiteral) Right).Value = value; }
        }
    }
}

