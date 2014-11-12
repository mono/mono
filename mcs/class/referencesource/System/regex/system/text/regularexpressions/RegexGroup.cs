//------------------------------------------------------------------------------
// <copyright file="RegexGroup.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// Group represents the substring or substrings that
// are captured by a single capturing group after one
// regular expression match.

namespace System.Text.RegularExpressions {
    using System.Security.Permissions;

    /// <devdoc>
    ///    Group 
    ///       represents the results from a single capturing group. A capturing group can
    ///       capture zero, one, or more strings in a single match because of quantifiers, so
    ///       Group supplies a collection of Capture objects. 
    ///    </devdoc>
#if !SILVERLIGHT
    [ Serializable() ] 
#endif
    public class Group : Capture {
        // the empty group object
        internal static Group   _emptygroup = new Group(String.Empty, new int[0], 0);
        
        internal int[] _caps;
        internal int _capcount;
        internal CaptureCollection _capcoll;

        internal Group(String text, int[] caps, int capcount)

        : base(text, capcount == 0 ? 0 : caps[(capcount - 1) * 2],
               capcount == 0 ? 0 : caps[(capcount * 2) - 1]) {

            _caps = caps;
            _capcount = capcount;
        }

        /*
         * True if the match was successful
         */
        /// <devdoc>
        ///    <para>Indicates whether the match is successful.</para>
        /// </devdoc>
        public bool Success {
            get {
                return _capcount != 0;
            }
        }

        /*
         * The collection of all captures for this group
         */
        /// <devdoc>
        ///    <para>
        ///       Returns a collection of all the captures matched by the capturing
        ///       group, in innermost-leftmost-first order (or innermost-rightmost-first order if
        ///       compiled with the "r" option). The collection may have zero or more items.
        ///    </para>
        /// </devdoc>
        public CaptureCollection Captures {
            get {
                if (_capcoll == null)
                    _capcoll = new CaptureCollection(this);

                return _capcoll;
            }
        }

        /*
         * Convert to a thread-safe object by precomputing cache contents
         */
        /// <devdoc>
        ///    <para>Returns 
        ///       a Group object equivalent to the one supplied that is safe to share between
        ///       multiple threads.</para>
        /// </devdoc>
#if !SILVERLIGHT
        [HostProtection(Synchronization=true)]
        static public Group Synchronized(Group inner) {
#else
        static internal Group Synchronized(Group inner) {
#endif
            if (inner == null)
                throw new ArgumentNullException("inner");

            // force Captures to be computed.

            CaptureCollection capcoll;
            Capture dummy;

            capcoll = inner.Captures;

            if (inner._capcount > 0)
                dummy = capcoll[0];

            return inner;
        }
    }


}
