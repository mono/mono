//------------------------------------------------------------------------------
// <copyright file="BaseCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                         
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
// <owner current="false" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data {
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;

    /// <devdoc>
    ///    <para>Provides the base functionality for creating collections.</para>
    /// </devdoc>
    public class InternalDataCollectionBase : ICollection {
        internal static CollectionChangeEventArgs RefreshEventArgs = new CollectionChangeEventArgs(CollectionChangeAction.Refresh, null);

        //==================================================
        // the ICollection methods
        //==================================================
        /// <devdoc>
        ///    <para>Gets the total number of elements in a collection.</para>
        /// </devdoc>
        [
        Browsable(false)
        ]
        public virtual int Count {
            get {
                return List.Count;
            }
        }

        public virtual void CopyTo(Array ar, int index) {
            List.CopyTo(ar, index);
        }

        public virtual IEnumerator GetEnumerator() {
            return List.GetEnumerator();
        }

        [
        Browsable(false)
        ]
        public bool IsReadOnly {
            get {
                return false;
            }
        }

        [Browsable(false)]
        public bool IsSynchronized {
            get {
                // so the user will know that it has to lock this object
                return false;
            }
        }

        // Return value: 
        // > 0 (1)  : CaseSensitve equal      
        // < 0 (-1) : Case-Insensitive Equal
        // = 0      : Not Equal
        internal int NamesEqual(string s1, string s2, bool fCaseSensitive, CultureInfo locale) {
            if (fCaseSensitive) {
                if (String.Compare(s1, s2, false, locale) == 0)
                    return 1;
                else
                    return 0;
            }
            
            // Case, kana and width -Insensitive compare
            if (locale.CompareInfo.Compare(s1, s2, 
                CompareOptions.IgnoreCase | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth) == 0) {
                if (String.Compare(s1, s2, false, locale) == 0)
                    return 1;
                else
                    return -1;
            }
            return 0;
        }


        [Browsable(false)]
        public object SyncRoot {
            get {
                return this;
            }
        }

        protected virtual ArrayList List {
            get {
                return null;
            }
        }
    }
}
