//------------------------------------------------------------------------------
// <copyright file="Pair.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
//------------------------------------------------------------------------------
using System;
using System.Diagnostics;

namespace System.Xml.Xsl {
    internal struct Int32Pair {
        private int left;
        private int right;

        public Int32Pair(int left, int right) {
            this.left = left;
            this.right = right;
        }

        public int Left  { get { return this.left ; } }
        public int Right { get { return this.right; } }

        public override bool Equals(object other) {
            if (other is Int32Pair) {
                Int32Pair o = (Int32Pair) other;
                return this.left == o.left && this.right == o.right;
            }

            return false;
        }

        public override int GetHashCode() {
            return this.left.GetHashCode() ^ this.right.GetHashCode();
        }
    }

    internal struct StringPair {
        private string left;
        private string right;

        public StringPair(string left, string right) {
            this.left = left;
            this.right = right;
        }

        public string Left  { get { return this.left ; } }
        public string Right { get { return this.right; } }
    }
}


