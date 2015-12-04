//------------------------------------------------------------------------------
// <copyright file="QilStrConcat.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil StrConcat operator.
    /// </summary>
    /// <remarks>
    /// Don't construct QIL nodes directly; instead, use the <see cref="QilFactory">QilFactory</see>.
    /// </remarks>
    internal class QilStrConcat : QilBinary {

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a new node
        /// </summary>
        public QilStrConcat(QilNodeType nodeType, QilNode delimiter, QilNode values) : base(nodeType, delimiter, values) {
        }


        //-----------------------------------------------
        // QilStrConcat methods
        //-----------------------------------------------

        /// <summary>
        /// A string delimiter to insert between successive values of the concatenation
        /// </summary>
        public QilNode Delimiter {
            get { return Left; }
            set { Left = value; }
        }

        /// <summary>
        /// List of values to concatenate
        /// </summary>
        public QilNode Values {
            get { return Right; }
            set { Right = value; }
        }
    }
}

