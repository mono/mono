//------------------------------------------------------------------------------
// <copyright file="RegexGroupCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// The GroupCollection lists the captured Capture numbers
// contained in a compiled Regex.

namespace System.Text.RegularExpressions {

    using System.Collections;
    using System.Collections.Generic;

    /// <devdoc>
    ///    <para>
    ///       Represents a sequence of capture substrings. The object is used
    ///       to return the set of captures done by a single capturing group.
    ///    </para>
    /// </devdoc>
#if !SILVERLIGHT
    [ Serializable() ]
#endif
    public class GroupCollection : ICollection {
        internal Match _match;
#if SILVERLIGHT
        internal Dictionary<Int32, Int32> _captureMap;
#else
        internal Hashtable _captureMap;
#endif

        // cache of Group objects fed to the user
        internal Group[]             _groups;

        /*
         * Nonpublic constructor
         */
#if SILVERLIGHT
        internal GroupCollection(Match match, Dictionary<Int32, Int32> caps) {
#else
        internal GroupCollection(Match match, Hashtable caps) {
#endif
            _match = match;
            _captureMap = caps;
        }

        /*
         * The object on which to synchronize
         */
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Object SyncRoot {
            get {
                return _match;
            }
        }

        /*
         * ICollection
         */
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsSynchronized {
            get {
                return false;
            }
        }

        /*
         * ICollection
         */
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool IsReadOnly {
            get {
                return true;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Returns the number of groups.
        ///    </para>
        /// </devdoc>
        public int Count {
            get {
                return _match._matchcount.Length;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Group this[int groupnum]
        {
            get {
                return GetGroup(groupnum);
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Group this[String groupname] {
            get {
                if (_match._regex == null)
                    return Group._emptygroup;

                return GetGroup(_match._regex.GroupNumberFromName(groupname));
            }
        }

        internal Group GetGroup(int groupnum) {
            if (_captureMap != null) {
                Object o;

                o = _captureMap[groupnum];
                if (o == null)
                    return Group._emptygroup;
                    //throw new ArgumentOutOfRangeException("groupnum");

                return GetGroupImpl((int)o);
            }
            else {
                //if (groupnum >= _match._regex.CapSize || groupnum < 0)
                //   throw new ArgumentOutOfRangeException("groupnum");
                if (groupnum >= _match._matchcount.Length || groupnum < 0)
                    return Group._emptygroup;

                return GetGroupImpl(groupnum);
            }
        }


        /*
         * Caches the group objects
         */
        internal Group GetGroupImpl(int groupnum) {
            if (groupnum == 0)
                return _match;

            // Construct all the Group objects the first time GetGroup is called

            if (_groups == null) {
                _groups = new Group[_match._matchcount.Length - 1];
                for (int i = 0; i < _groups.Length; i++) {
                    _groups[i] = new Group(_match._text, _match._matches[i + 1], _match._matchcount[i + 1]);
                }
            }

            return _groups[groupnum - 1];
        }

        /*
         * As required by ICollection
         */
        /// <devdoc>
        ///    <para>
        ///       Copies all the elements of the collection to the given array
        ///       beginning at the given index.
        ///    </para>
        /// </devdoc>
        public void CopyTo(Array array, int arrayIndex) {
            if (array == null)
                throw new ArgumentNullException("array");

            for (int i = arrayIndex, j = 0; j < Count; i++, j++) {
                array.SetValue(this[j], i);
            }
        }

        /*
         * As required by ICollection
         */
        /// <devdoc>
        ///    <para>
        ///       Provides an enumerator in the same order as Item[].
        ///    </para>
        /// </devdoc>
        public IEnumerator GetEnumerator() {
            return new GroupEnumerator(this);
        }
    }


    /*
     * This non-public enumerator lists all the captures
     * Should it be public?
     */
    internal class GroupEnumerator : IEnumerator {
        internal GroupCollection _rgc;
        internal int _curindex;

        /*
         * Nonpublic constructor
         */
        internal GroupEnumerator(GroupCollection rgc) {
            _curindex = -1;
            _rgc = rgc;
        }

        /*
         * As required by IEnumerator
         */
        public bool MoveNext() {
            int size = _rgc.Count;

            if (_curindex >= size)
                return false;

            _curindex++;

            return(_curindex < size);
        }

        /*
         * As required by IEnumerator
         */
        public Object Current {
            get { return Capture;}
        }

        /*
         * Returns the current capture
         */
        public Capture Capture {
            get {
                if (_curindex < 0 || _curindex >= _rgc.Count)
                    throw new InvalidOperationException(SR.GetString(SR.EnumNotStarted));

                return _rgc[_curindex];
            }
        }

        /*
         * Reset to before the first item
         */
        public void Reset() {
            _curindex = -1;
        }
    }

}
