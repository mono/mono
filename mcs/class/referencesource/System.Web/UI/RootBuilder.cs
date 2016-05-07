//------------------------------------------------------------------------------
// <copyright file="RootBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Implements the root builder
 *
 * Copyright (c) 1998 Microsoft Corporation
 */

namespace System.Web.UI {
    using System.Runtime.InteropServices;

    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Web;
    using System.Web.Util;

    /// <internalonly/>
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    public class RootBuilder : TemplateBuilder {
        private MainTagNameToTypeMapper _typeMapper;

        // Contains a mapping of all objects to their associated builders
        private IDictionary _builtObjects;


        public RootBuilder() {
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public RootBuilder(TemplateParser parser) {
        }

        public IDictionary BuiltObjects {
            get {
                // Store any objects created by this control builder
                // so we can properly persist items
                if (_builtObjects == null) {
                    _builtObjects = new Hashtable(ReferenceKeyComparer.Default);
                }
                return _builtObjects;
            }
        }

        /// <summary>
        /// The method gets called when the CodeDom tree is fully built.  The way it is used is by first overriding
        /// ProcessGeneratedCode to keep track of the CodeCompileUnit, and to then have the ability to modify
        /// the CodeCompileUnit in this method
        /// </summary>
        protected internal virtual void OnCodeGenerationComplete() {
        }

        internal void SetTypeMapper(MainTagNameToTypeMapper typeMapper) {
            _typeMapper = typeMapper;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override Type GetChildControlType(string tagName,
                                                 IDictionary attribs) {
            // Is there a type to handle this control
            Type type = _typeMapper.GetControlType(tagName, attribs,
                                                   true /*fAllowHtmlTags*/);

            return type;
        }

        internal override void PrepareNoCompilePageSupport() {
            base.PrepareNoCompilePageSupport();

            // This is needed to break any connection with the TemplateParser, allowing it
            // to be fully collected when the parsing is complete
            _typeMapper = null;
        }

        private class ReferenceKeyComparer : IComparer, IEqualityComparer {
            internal static readonly ReferenceKeyComparer Default = new ReferenceKeyComparer();

            bool IEqualityComparer.Equals(object x, object y) {
                return Object.ReferenceEquals(x, y);
            }

            int IEqualityComparer.GetHashCode(object obj) {
                return obj.GetHashCode();
            }

            int IComparer.Compare(object x, object y) {
                if (Object.ReferenceEquals(x, y)) {
                    return 0;
                }
                if (x == null) {
                    return -1;
                }
                if (y == null) {
                    return 1;
                }
                return 1;
            }
        }
    }
}
