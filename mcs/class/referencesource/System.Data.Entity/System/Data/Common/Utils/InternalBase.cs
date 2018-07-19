//---------------------------------------------------------------------
// <copyright file="InternalBase.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------


using System;
using System.Collections;
using System.Text;

namespace System.Data.Common.Utils {
    // A basic class from which all classes derive so that ToString can be
    // more controlled
    internal abstract class InternalBase {

        // effects: Modify builder to contain a compact string representation
        // of this
        internal abstract void ToCompactString(StringBuilder builder);

        // effects: Modify builder to contain a verbose string representation
        // of this
        internal virtual void ToFullString(StringBuilder builder) {
            ToCompactString(builder);
        }

        public override string ToString() {
			StringBuilder builder = new StringBuilder();
            ToCompactString(builder);
            return builder.ToString();
        }        

        internal virtual string ToFullString() {
			StringBuilder builder = new StringBuilder();
            ToFullString(builder);
            return builder.ToString();
        }
    }
}
