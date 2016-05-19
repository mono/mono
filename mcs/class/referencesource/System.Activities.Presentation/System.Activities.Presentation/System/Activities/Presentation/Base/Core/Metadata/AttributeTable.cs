//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Metadata 
{

    using System.Activities.Presentation.Internal.Metadata;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;
    using System.Windows;
    using System.Runtime;
    using System.Activities.Presentation;

    // <summary>
    // Attribute tables are essentially read-only dictionaries, but the keys
    // and values are computed separately.  It is very efficient to ask an
    // attribute table if it contains attributes for a particular type.
    // The actual set of attributes is demand created.
    // </summary>
    [Fx.Tag.XamlVisible(false)]
    public sealed class AttributeTable 
    {

        private MutableAttributeTable _attributes;

        //
        // Creates a new attribute table given dictionary information
        // from the attribute table builder.
        //
        internal AttributeTable(MutableAttributeTable attributes) 
        {
            Fx.Assert(attributes != null, "attributes parameter should not be null");
            _attributes = attributes;
        }

        // <summary>
        // Returns an enumeration of all types that have attribute overrides
        // of some kind (on a property, on the type itself, etc).  This can be
        // used to determine what types will be refreshed when this attribute
        // table is added to the metadata store.
        // </summary>
        // <returns></returns>
        public IEnumerable<Type> AttributedTypes 
        {
            get { return _attributes.AttributedTypes; }
        }

        //
        // Returns our internal mutable table.  This is used
        // by AttributeTableBuilder's AddTable method.
        //
        internal MutableAttributeTable MutableTable 
        {
            get { return _attributes; }
        }

        // <summary>
        // Returns true if this table contains any metadata for the given type.
        // The metadata may be class-level metadata or metadata associated with
        // a DepenendencyProperty or MemberDescriptor.  The AttributeStore uses
        // this method to identify loaded types that need a Refresh event raised
        // when a new attribute table is added, and to quickly decide which
        // tables should be further queried during attribute queries.
        // </summary>
        // <param name="type">The type to check.</param>
        // <returns>true if the table contains attributes for the given type.</returns>
        // <exception cref="ArgumentNullException">if type is null</exception>
        public bool ContainsAttributes(Type type) 
        {
            if (type == null) 
            {
                throw FxTrace.Exception.ArgumentNull("type");
            }
            return _attributes.ContainsAttributes(type);
        }

        // <summary>
        // Returns an enumeration of all attributes provided for the
        // given argument.  This will never return a null enumeration.
        // </summary>
        // <param name="type">The type to get class-level attributes for.</param>
        // <returns>An enumeration of attributes.</returns>
        // <exception cref="ArgumentNullException">if type is null</exception>
        public IEnumerable GetCustomAttributes(Type type) 
        {
            if (type == null) 
            {
                throw FxTrace.Exception.ArgumentNull("type");
            }
            return _attributes.GetCustomAttributes(type);
        }

        // <summary>
        // Returns an enumeration of all attributes provided for the
        // given argument.  This will never return a null enumeration.
        // </summary>
        // <param name="ownerType">The type that declares this descriptor.</param>
        // <param name="descriptor">A member descriptor to get custom attributes for.</param>
        // <returns>An enumeration of attributes.</returns>
        // <exception cref="ArgumentNullException">if descriptor is null</exception>
        public IEnumerable GetCustomAttributes(Type ownerType, MemberDescriptor descriptor) 
        {
            if (ownerType == null) 
            {
                throw FxTrace.Exception.ArgumentNull("ownerType");
            }
            if (descriptor == null) 
            {
                throw FxTrace.Exception.ArgumentNull("descriptor");
            }
            return _attributes.GetCustomAttributes(ownerType, descriptor);
        }

        // <summary>
        // Returns an enumeration of all attributes provided for the
        // given argument.  This will never return a null enumeration.
        // </summary>
        // <param name="ownerType">The owner type of the dependency property.</param>
        // <param name="dp">A dependency property to get custom attributes for.</param>
        // <returns>An enumeration of attributes.</returns>
        // <exception cref="ArgumentNullException">if ownerType or dp is null</exception>
        public IEnumerable GetCustomAttributes(Type ownerType, DependencyProperty dp) 
        {
            if (ownerType == null) 
            {
                throw FxTrace.Exception.ArgumentNull("ownerType");
            }
            if (dp == null) 
            {
                throw FxTrace.Exception.ArgumentNull("dp");
            }
            return _attributes.GetCustomAttributes(ownerType, dp);
        }

        // <summary>
        // Returns an enumeration of all attributes provided for the
        // given argument.  This will never return a null enumeration.
        // </summary>
        // <param name="ownerType">The owner type of the dependency property.</param>
        // <param name="member">The member to provide attributes for.</param>
        // <returns>An enumeration of attributes.</returns>
        // <exception cref="ArgumentNullException">if ownerType or member is null</exception>
        public IEnumerable GetCustomAttributes(Type ownerType, MemberInfo member) 
        {
            if (ownerType == null) 
            {
                throw FxTrace.Exception.ArgumentNull("ownerType");
            }
            if (member == null) 
            {
                throw FxTrace.Exception.ArgumentNull("member");
            }
            return _attributes.GetCustomAttributes(ownerType, member);
        }

        // <summary>
        // Returns an enumeration of all attributes provided for the
        // given argument.  This will never return a null enumeration.
        // </summary>
        // <param name="ownerType">The owner type of the dependency property.</param>
        // <param name="memberName">The name of the member to provide attributes for.</param>
        // <returns>An enumeration of attributes.</returns>
        // <exception cref="ArgumentNullException">if ownerType or member is null</exception>
        public IEnumerable GetCustomAttributes(Type ownerType, string memberName) 
        {
            if (ownerType == null) 
            {
                throw FxTrace.Exception.ArgumentNull("ownerType");
            }
            if (memberName == null) 
            {
                throw FxTrace.Exception.ArgumentNull("memberName");
            }
            return _attributes.GetCustomAttributes(ownerType, memberName);
        }

        //
        // Called by the MetadataStore to walk through all the metadata and
        // ensure that it can be found on the appropriate types and members.
        // Any asserts that come from here are bugs in the type description
        // provider.
        //
        internal void DebugValidateProvider()
        {
#if DEBUG
            _attributes.DebugValidateProvider();
#else
#endif
        }
    }
}
