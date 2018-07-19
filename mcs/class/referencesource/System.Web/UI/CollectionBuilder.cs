//------------------------------------------------------------------------------
// <copyright file="CollectionBuilder.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * Classes related to complex property support.
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.UI {

    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.Util;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class IgnoreUnknownContentAttribute : Attribute {
        internal IgnoreUnknownContentAttribute() {}
    }


    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    internal sealed class CollectionBuilder : ControlBuilder {

        private Type _itemType;
        private bool _ignoreUnknownContent;

        internal CollectionBuilder(bool ignoreUnknownContent) { _ignoreUnknownContent = ignoreUnknownContent; }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void Init(TemplateParser parser, ControlBuilder parentBuilder,
                                  Type type, string tagName, string ID, IDictionary attribs) {

            base.Init(parser, parentBuilder, type /*type*/, tagName, ID, attribs);

            // 



            PropertyInfo propInfo = TargetFrameworkUtil.GetProperty(parentBuilder.ControlType, 
                tagName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase);
            SetControlType(propInfo.PropertyType);
            Debug.Assert(ControlType != null, "ControlType != null");

            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            // Look for an "item" property on the collection that takes in an integer index
            // (similar to IList::Item)
            propInfo = TargetFrameworkUtil.GetProperty(ControlType, "Item", bindingFlags, types: new Type[] { typeof(int) });
            if (propInfo == null) {
                // fall-back on finding a non-specific Item property
                // a type with overloaded indexed properties will result in an exception however
                propInfo = TargetFrameworkUtil.GetProperty(ControlType, "Item", bindingFlags);
            }

            // If we got one, use it to determine the type of the items
            if (propInfo != null)
                _itemType = propInfo.PropertyType;
        }

        // This code is only executed when used from the desiger

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override object BuildObject() {
            return this;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override Type GetChildControlType(string tagName, IDictionary attribs) {

            Type childType = Parser.MapStringToType(tagName, attribs);

            // If possible, check if the item is of the required type
            if (_itemType != null) {

                if (!_itemType.IsAssignableFrom(childType)) {
                    if (_ignoreUnknownContent)
                        return null;

                    string controlTypeName = String.Empty;
                    if (ControlType != null) {
                        controlTypeName = ControlType.FullName;
                    }
                    else {
                        controlTypeName = TagName;
                    }

                    throw new HttpException(SR.GetString(SR.Invalid_collection_item_type, new String[] { controlTypeName,
                                                                        _itemType.FullName,
                                                                        tagName,
                                                                        childType.FullName}));
                }

            }

            return childType;
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public override void AppendLiteralString(string s) {

            if (_ignoreUnknownContent)
                return;

            // Don't allow non-whitespace literal content
            if (!Util.IsWhiteSpaceString(s)) {
                throw new HttpException(SR.GetString(SR.Literal_content_not_allowed, ControlType.FullName, s.Trim()));
            }
        }
    }

}
