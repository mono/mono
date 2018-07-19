//------------------------------------------------------------------------------
// <copyright file="RegexCaptureCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// The CaptureCollection lists the captured Capture numbers
// contained in a compiled Regex.

namespace System.Text.RegularExpressions {

    using System.Collections;

    /*
     * This collection returns the Captures for a group
     * in the order in which they were matched (left to right
     * or right to left). It is created by Group.Captures
     */
    /// <devdoc>
    ///    <para>
    ///       Represents a sequence of capture substrings. The object is used
    ///       to return the set of captures done by a single capturing group.
    ///    </para>
    /// </devdoc>
#if !SILVERLIGHT
    [ Serializable() ]
#endif
    public class CaptureCollection : ICollection {
        internal Group _group;
        internal int _capcount;
        internal Capture[] _captures;

        /*
         * Nonpublic constructor
         */
        internal CaptureCollection(Group group) {
            _group = group;
            _capcount = _group._capcount;
        }

        /*
         * The object on which to synchronize
         */
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public Object SyncRoot {
            get {
                return _group;
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

        /*
         * The number of captures for the group
         */
        /// <devdoc>
        ///    <para>
        ///       Returns the number of captures.
        ///    </para>
        /// </devdoc>
        public int Count {
            get {
                return _capcount;
            }
        }

        /*
         * The ith capture in the group
         */
        /// <devdoc>
        ///    <para>
        ///       Provides a means of accessing a specific capture in the collection.
        ///    </para>
        /// </devdoc>
        public Capture this[int i]
        {
            get {
                return GetCapture(i);
            }
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
            return new CaptureEnumerator(this);
        }

        /*
         * Nonpublic code to return set of captures for the group
         */
        internal Capture GetCapture(int i) {
            if (i == _capcount - 1 && i >= 0)
                return _group;

            if (i >= _capcount || i < 0)
                throw new ArgumentOutOfRangeException("i");

            // first time a capture is accessed, compute them all
            if (_captures == null) {
                _captures = new Capture[_capcount];
                for (int j = 0; j < _capcount - 1; j++) {
                    _captures[j] = new Capture(_group._text, _group._caps[j * 2], _group._caps[j * 2 + 1]);
                }
            }

            return _captures[i];
        }
    }


    /*
     * This non-public enumerator lists all the captures
     * Should it be public?
     */
#if !SILVERLIGHT
    [ Serializable() ]
#endif
    internal class CaptureEnumerator : IEnumerator {
        internal CaptureCollection _rcc;
        internal int _curindex;

        /*
         * Nonpublic constructor
         */
        internal CaptureEnumerator(CaptureCollection rcc) {
            _curindex = -1;
            _rcc = rcc;
        }

        /*
         * As required by IEnumerator
         */
        public bool MoveNext() {
            int size = _rcc.Count;

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
                if (_curindex < 0 || _curindex >= _rcc.Count)
                    throw new InvalidOperationException(SR.GetString(SR.EnumNotStarted));

                return _rcc[_curindex];
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
