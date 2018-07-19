//------------------------------------------------------------------------------
// <copyright file="SubstitutionList.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Diagnostics;

namespace System.Xml.Xsl.Qil {    

    /// <summary>
    /// Data structure for use in CloneAndReplace
    /// </summary>
    /// <remarks>Isolates the many QilNode classes from changes in
    /// the underlying data structure.</remarks>
    internal sealed class SubstitutionList {
        // 
        private ArrayList s;
        
        public SubstitutionList() {
            this.s = new ArrayList(4);
        }

        /// <summary>
        /// Add a substituion pair
        /// </summary>
        /// <param name="find">a node to be replaced</param>
        /// <param name="replace">its replacement</param>
        public void AddSubstitutionPair(QilNode find, QilNode replace) {
            s.Add(find);
            s.Add(replace);
        }

        /// <summary>
        /// Remove the last a substituion pair
        /// </summary>
        public void RemoveLastSubstitutionPair() {
            s.RemoveRange(s.Count - 2, 2);
        }

        /// <summary>
        /// Remove the last N substitution pairs
        /// </summary>
        public void RemoveLastNSubstitutionPairs(int n) {
            Debug.Assert(n >= 0, "n must be nonnegative");
            if (n > 0) {
                n *= 2;
                s.RemoveRange(s.Count - n, n);
            }
        }

        /// <summary>
        /// Find the replacement for a node
        /// </summary>
        /// <param name="n">the node to replace</param>
        /// <returns>null if no replacement is found</returns>
        public QilNode FindReplacement(QilNode n) {
            Debug.Assert(s.Count % 2 == 0);
            for (int i = s.Count-2; i >= 0; i-=2)
                if (s[i] == n)
                    return (QilNode)s[i+1];
            return null;
        }
    }
}
