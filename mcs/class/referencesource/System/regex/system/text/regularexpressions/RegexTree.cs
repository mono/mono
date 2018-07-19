//------------------------------------------------------------------------------
// <copyright file="RegexTree.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

// RegexTree is just a wrapper for a node tree with some
// global information attached.

namespace System.Text.RegularExpressions {

    using System.Collections;
    using System.Collections.Generic;

    internal sealed class RegexTree {
#if SILVERLIGHT
        internal RegexTree(RegexNode root, Dictionary<Int32, Int32> caps, Int32[] capnumlist, int captop, Dictionary<String, Int32> capnames, String[] capslist, RegexOptions opts)
#else
        internal RegexTree(RegexNode root, Hashtable caps, Int32[] capnumlist, int captop, Hashtable capnames, String[] capslist, RegexOptions opts)
#endif

        {
            _root = root;
            _caps = caps;
            _capnumlist = capnumlist;
            _capnames = capnames;
            _capslist = capslist;
            _captop = captop;
            _options = opts;
        }

        internal RegexNode _root;
#if SILVERLIGHT
        internal Dictionary<Int32, Int32> _caps;
#else
        internal Hashtable _caps;
#endif
        internal Int32[]  _capnumlist;
#if SILVERLIGHT
        internal Dictionary<String, Int32> _capnames;
#else
        internal Hashtable _capnames;
#endif
        internal String[]  _capslist;
        internal RegexOptions _options;
        internal int       _captop;

#if DBG
        internal void Dump() {
            _root.Dump();
        }

        internal bool Debug {
            get {
                return(_options & RegexOptions.Debug) != 0;
            }
        }
#endif
    }
}
