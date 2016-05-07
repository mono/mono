//------------------------------------------------------------------------------
// <copyright file="QilReference.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {

    /// <summary>
    /// View over a Qil node which is the target of a reference (functions, variables, parameters).
    /// </summary>
    internal class QilReference : QilNode {
        // Names longer than 1023 characters cause AV in cscompee.dll, see VSWhidbey 485526
        // So we set the internal limit to 1000. Needs to be lower since we might later append
        //   few characters (for example "(2)") if we end up with two same named methods after
        //   the truncation.
        private const int MaxDebugNameLength = 1000;

        private string debugName;

        //-----------------------------------------------
        // Constructor
        //-----------------------------------------------

        /// <summary>
        /// Construct a reference
        /// </summary>
        public QilReference(QilNodeType nodeType) : base(nodeType) {
        }


        //-----------------------------------------------
        // QilReference methods
        //-----------------------------------------------

        /// <summary>
        /// Name of this reference, preserved for debugging (may be null).
        /// </summary>
        public string DebugName {
            get { return this.debugName; }
            set {
                if (value.Length > MaxDebugNameLength)
                    value = value.Substring(0, MaxDebugNameLength);

                this.debugName = value;
            }
        }
    }
}
