//------------------------------------------------------------------------------
// <copyright file="CollectionsUtil.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// Wrapper for a case insensitive Hashtable.

namespace System.Collections.Specialized {

    using System.Collections;

    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class CollectionsUtil {

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static Hashtable CreateCaseInsensitiveHashtable()  {
            return new Hashtable(StringComparer.CurrentCultureIgnoreCase);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static Hashtable CreateCaseInsensitiveHashtable(int capacity)  {
            return new Hashtable(capacity, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static Hashtable CreateCaseInsensitiveHashtable(IDictionary d)  {
            return new Hashtable(d, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public static SortedList CreateCaseInsensitiveSortedList() {
            return new SortedList(CaseInsensitiveComparer.Default);
        }

    }

}
