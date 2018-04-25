//------------------------------------------------------------------------------
// <copyright file="StringPropertyBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;

    /// <devdoc>
    /// Builds inner string properties.
    /// </devdoc>
    internal sealed class StringPropertyBuilder : ControlBuilder {
        private string _text;


        /// <devdoc>
        /// Creates a new instance of StringPropertyBuilder.
        /// </devdoc>
        internal StringPropertyBuilder() {
        }

        internal StringPropertyBuilder(string text) {
            _text = text;
        }

        /// <devdoc>
        /// Returns the inner text of the property.
        /// </devdoc>
        public string Text {
            get {
                return (_text == null) ? String.Empty : _text;
            }
        }

        /// <devdoc>
        /// Gets the inner text of the property.
        /// </devdoc>
        public override void AppendLiteralString(string s) {
            if (ParentBuilder != null && ParentBuilder.HtmlDecodeLiterals())
                s = HttpUtility.HtmlDecode(s);

            _text = s;
        }

        /// <devdoc>
        /// Throws an exception - string properties cannot contain other objects.
        /// </devdoc>
        public override void AppendSubBuilder(ControlBuilder subBuilder) {
            throw new HttpException(SR.GetString(SR.StringPropertyBuilder_CannotHaveChildObjects, TagName, (ParentBuilder != null ? ParentBuilder.TagName : String.Empty)));
        }

        public override object BuildObject() {
            return Text;
        }

        public override void Init(TemplateParser parser, ControlBuilder parentBuilder,
                                  Type type, string tagName, string ID, IDictionary attribs) {

            base.Init(parser, parentBuilder, type /*type*/, tagName, ID, attribs);

            SetControlType(typeof(string));
        }
    }
}
