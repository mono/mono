//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Internal.Metadata 
{

    using System.Activities.Presentation.Internal.Properties;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Activities.Presentation.Metadata;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;

    //
    // This class is used by the attribute table builder to
    // add attributes.  It is then handed to AttributeTable
    // and accessed in a read-only fashion.
    //
    internal class MutableAttributeTable 
    {
        private static object[] _empty = new object[0];

        private Dictionary<Type, TypeMetadata> _metadata;

        internal MutableAttributeTable() 
        {
            _metadata = new Dictionary<Type, TypeMetadata>();
        }

        //
        // Returns the types we're handing metadata for
        //
        internal IEnumerable<Type> AttributedTypes 
        {
            get { return _metadata.Keys; }
        }

        //
        // Private helper to add a portion of an existing table.
        //
        private static void AddAttributeMetadata(TypeMetadata newMd, TypeMetadata existingMd) 
        {
            if (newMd.TypeAttributes != null) 
            {
                if (existingMd.TypeAttributes != null) 
                {
                    existingMd.TypeAttributes.AddRange(newMd.TypeAttributes);
                }
                else 
                {
                    existingMd.TypeAttributes = newMd.TypeAttributes;
                }
            }
        }

        //
        // Helper to add a enum of attributes ot an existing list
        //
        private static void AddAttributes(AttributeList list, IEnumerable<object> attributes) 
        {
            // Attributes are ordered so those at the end of the
            // list take prececence over those at the front.
            list.AddRange(attributes);
        }

        internal void AddCallback(Type type, AttributeCallback callback) 
        {
            Fx.Assert(type != null && callback != null, "type or callback parameter is null");
            AttributeList list = GetTypeList(type);
            list.Add(callback);
        }

        //
        // Adds custom attrs for a type
        //
        internal void AddCustomAttributes(Type type, IEnumerable<object> attributes) 
        {
            Fx.Assert(type != null && attributes != null, "type or attributes parameter is null");
            AddAttributes(GetTypeList(type), attributes);
        }

        //
        // Adds custom attrs for a descriptor
        //
        internal void AddCustomAttributes(Type ownerType, MemberDescriptor descriptor, IEnumerable<object> attributes) 
        {
            Fx.Assert(ownerType != null && descriptor != null && attributes != null, "ownerType/descriptor/attributes is null");
            AddAttributes(GetMemberList(ownerType, descriptor.Name), attributes);
        }

        //
        // Adds custom attrs for a member
        //
        internal void AddCustomAttributes(Type ownerType, MemberInfo member, IEnumerable<object> attributes) 
        {
            Fx.Assert(ownerType != null && member != null && attributes != null, "ownertype/member/attributes parameter is null");
            AddAttributes(GetMemberList(ownerType, member.Name), attributes);
        }

        //
        // Adds custom attrs for a dp
        //
        internal void AddCustomAttributes(Type ownerType, DependencyProperty dp, IEnumerable<object> attributes) 
        {
            Fx.Assert(ownerType != null && dp != null && attributes != null, "ownerType/dp/attributes parameter is null");
            AddAttributes(GetMemberList(ownerType, dp.Name), attributes);
        }

        //
        // Adds custom attrs for a member name
        //
        internal void AddCustomAttributes(Type ownerType, string memberName, IEnumerable<object> attributes) 
        {
            Fx.Assert(ownerType != null && memberName != null && attributes != null, "ownerType/membername/attributes parameter is null");
            AddAttributes(GetMemberList(ownerType, memberName), attributes);
        }

        //
        // Private helper to add a portion of an existing table.
        //
        private static void AddMemberMetadata(TypeMetadata newMd, TypeMetadata existingMd) 
        {
            if (newMd.MemberAttributes != null) 
            {
                if (existingMd.MemberAttributes != null) 
                {
                    foreach (KeyValuePair<string, AttributeList> kv in newMd.MemberAttributes) 
                    {
                        AttributeList existing;
                        if (existingMd.MemberAttributes.TryGetValue(kv.Key, out existing)) 
                        {
                            existing.AddRange(kv.Value);
                        }
                        else 
                        {
                            existingMd.MemberAttributes.Add(kv.Key, kv.Value);
                        }
                    }
                }
                else 
                {
                    existingMd.MemberAttributes = newMd.MemberAttributes;
                }
            }
        }

        //
        // Adds an existing table.
        //
        internal void AddTable(MutableAttributeTable table) 
        {
            Fx.Assert(table != null, "table parameter is null");
            foreach (KeyValuePair<Type, TypeMetadata> kv in table._metadata) 
            {
                AddTypeMetadata(kv.Key, kv.Value);
            }
        }

        //
        // Private helper to add a portion of an existing table.
        //
        private void AddTypeMetadata(Type type, TypeMetadata md) 
        {
            TypeMetadata existing;
            if (_metadata.TryGetValue(type, out existing)) 
            {
                AddAttributeMetadata(md, existing);
                AddMemberMetadata(md, existing);
            }
            else 
            {
                _metadata.Add(type, md);
            }
        }

        //
        // Returns true if this table contains attributes for the
        // given type
        //
        internal bool ContainsAttributes(Type type) 
        {
            Fx.Assert(type != null, "type parameter is null");
            return (_metadata.ContainsKey(type));
        }

        //
        // Helper method that walks through an attribute list and expands all callbacks
        // within it.
        //
        private void ExpandAttributes(Type type, AttributeList attributes) 
        {
            Fx.Assert(!attributes.IsExpanded, "Should not call expand attributes with an expanded list.");

            // First, expand all the callbacks.  This may add more attributes
            // into our list
            //
            for (int idx = 0; idx < attributes.Count; idx++) 
            {
                AttributeCallback callback = attributes[idx] as AttributeCallback;
                while (callback != null) 
                {
                    attributes.RemoveAt(idx);
                    AttributeCallbackBuilder builder = new AttributeCallbackBuilder(this, type);
                    callback(builder);

                    if (idx < attributes.Count) 
                    {
                        callback = attributes[idx] as AttributeCallback;
                    }
                    else 
                    {
                        callback = null;
                    }
                }
            }
        }

        //
        // Returns custom attributes for the type.
        //
        internal IEnumerable GetCustomAttributes(Type type) 
        {
            Fx.Assert(type != null, "type parameter is null");

            AttributeList attributes = GetExpandedAttributes(type, null, delegate(Type typeToGet, object callbackParam) 
            {
                TypeMetadata md;
                if (_metadata.TryGetValue(typeToGet, out md)) 
                {
                    return md.TypeAttributes;
                }
                return null;
            });

            if (attributes != null) 
            {
                return attributes.AsReadOnly();
            }

            return _empty;
        }

        //
        // Returns custom attributes for the descriptor.
        //
        internal IEnumerable GetCustomAttributes(Type ownerType, MemberDescriptor descriptor) 
        {
            Fx.Assert(ownerType != null && descriptor != null, "ownerType or descriptor parameter is null");
            return GetCustomAttributes(ownerType, descriptor.Name);
        }

        //
        // Returns custom attributes for the dp.
        //
        internal IEnumerable GetCustomAttributes(Type ownerType, DependencyProperty dp) 
        {
            Fx.Assert(ownerType != null && dp != null, "ownerType or dp parameter is null");
            return GetCustomAttributes(ownerType, dp.Name);
        }

        //
        // Returns custom attributes for the member.
        //
        internal IEnumerable GetCustomAttributes(Type ownerType, MemberInfo member) 
        {
            Fx.Assert(ownerType != null && member != null, "ownerType or memeber parameter is null");
            return GetCustomAttributes(ownerType, member.Name);
        }

        //
        // Returns custom attributes for the member.
        //
        internal IEnumerable GetCustomAttributes(Type ownerType, string memberName) 
        {
            Fx.Assert(ownerType != null && memberName != null, "ownerType or memberName parameter is null");

            AttributeList attributes = GetExpandedAttributes(ownerType, memberName, delegate(Type typeToGet, object callbackParam) 
            {
                string name = (string)callbackParam;
                TypeMetadata md;

                if (_metadata.TryGetValue(typeToGet, out md)) 
                {

                    // If member attributes are null but type attributes are not,
                    // it is possible that expanding type attributes could cause
                    // member attributes to be added.  Check.

                    if (md.MemberAttributes == null && md.TypeAttributes != null && !md.TypeAttributes.IsExpanded) 
                    {
                        ExpandAttributes(ownerType, md.TypeAttributes);
                    }

                    if (md.MemberAttributes != null) 
                    {
                        AttributeList list;
                        if (md.MemberAttributes.TryGetValue(name, out list)) 
                        {
                            return list;
                        }
                    }
                }

                return null;
            });

            if (attributes != null) 
            {
                return attributes.AsReadOnly();
            }

            return _empty;
        }

        //
        // Helper to demand create the attribute list ofr a dependency property.
        //
        private AttributeList GetMemberList(Type ownerType, string memberName) 
        {
            Fx.Assert(ownerType != null && memberName != null, "ownerType or memberName parameter is null");
            TypeMetadata md = GetTypeMetadata(ownerType);

            if (md.MemberAttributes == null) 
            {
                md.MemberAttributes = new Dictionary<string, AttributeList>();
            }

            AttributeList list;
            if (!md.MemberAttributes.TryGetValue(memberName, out list)) 
            {
                list = new AttributeList();
                md.MemberAttributes.Add(memberName, list);
            }

            return list;
        }

        //
        // Expands a type attribute table for use.
        // Attribute tables only contain attributes for
        // the given type, and may have callbacks embedded
        // within them.
        //
        private AttributeList GetExpandedAttributes(Type type, object callbackParam, GetAttributesCallback callback) 
        {

            // Do we have attributes to expand?

            AttributeList attributes = callback(type, callbackParam);
            if (attributes != null) 
            {

                // If these attributes haven't been expanded yet, do that
                // now.

                if (!attributes.IsExpanded) 
                {

                    // We have a lock here because multiple people could be
                    // surfing type information at the same time from multiple
                    // threads.  While we are read only once we are expanded,
                    // we do modify the list here to expand the callbacks and 
                    // merge.  Therefore, we need to acquire a lock.

                    lock (attributes) 
                    {
                        if (!attributes.IsExpanded) 
                        {
                            ExpandAttributes(type, attributes);
                            attributes.IsExpanded = true;
                        }
                    }
                }
            }

            return attributes;
        }

        //
        // Helper to demand create the attribute list for a type.
        //
        private AttributeList GetTypeList(Type type) 
        {
            Fx.Assert(type != null, "type parameter is null");
            TypeMetadata md = GetTypeMetadata(type);
            if (md.TypeAttributes == null) 
            {
                md.TypeAttributes = new AttributeList();
            }
            return md.TypeAttributes;
        }

        //
        // Helper to demand create the type metadata.
        //
        private TypeMetadata GetTypeMetadata(Type type) 
        {
            Fx.Assert(type != null, "type parameter is null");
            TypeMetadata md;
            if (!_metadata.TryGetValue(type, out md)) 
            {
                md = new TypeMetadata();
                _metadata.Add(type, md);
            }
            return md;
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
            foreach (KeyValuePair<Type, TypeMetadata> kv in _metadata
) {
                if (kv.Value.TypeAttributes != 
null) {
                    AttributeCollection attrs = TypeDescriptor.GetAttributes(kv.Key);
                    foreach (object o in kv.Value
.TypeAttributes) {
                        Attribute a = o as Attribute;
                        if (a 
!= null) {
                            bool found = false;
                            foreach (Attribute a2
 in attrs) {
                                if (a.TypeId.Equals
(a2.TypeId)) {
                                    found = true;
                                    break;
                                }
                            }

                            Fx.Assert(
                                found,
                                string.Format(CultureInfo.CurrentCulture, "Attribute {0} on type {1} is missing from provider.",
                                a.GetType().Name, kv.Key.Name));
                        }
                    }
                }

                if (kv.Value
.MemberAttributes != null) {
                    foreach (KeyValuePair<string, AttributeList> kvDesc 
in kv.Value.MemberAttributes) {
                        PropertyDescriptor p;
                        EventDescriptor e;
                        AttributeCollection attrs = null;
                        string member = "unknown";

                        if ((p = TypeDescriptor.GetProperties(kv.Key)[kvDesc.Key]) != null) {
                            attrs = p.Attributes;
                            member = p.Name;
                        }
                        else if ((e = TypeDescriptor.GetEvents(kv.Key)[kvDesc.Key]) != null) {
                            attrs = e.Attributes;
                            member = e.Name;
                        }
                        else if ((p = DependencyPropertyDescriptor.FromName(kvDesc.Key, kv.Key, typeof(
DependencyObject))) != null) {
                            attrs = p.Attributes;
                            member = p.Name;
                        }
                       
 if (attrs != null) {
                            foreach 
(object o in kvDesc.Value) {
                                Attribute a = o as Attribute;
                             
   if (a != null) {
                                    bool found = false;
                                  
  foreach (Attribute a2 in attrs) {
                                        
if (a.TypeId.Equals(a2.TypeId)) {
                                            found = true;
                                            break;
                                        }
                                    }

                                    Fx.Assert(
                                        found,
                                        string.Format(CultureInfo.CurrentCulture, "Attribute {0} on member {1}.{2} is missing from provider.",
                                        a.GetType().Name, kv.Key.Name, member));
                                }
                            }
                        }
                    }
                }
            }
#else
#endif
        }

        //
        // Performs validation of all metadata in the table.
        // This expands all callbacks so it can be very expensive
        // for large tables.
        //
        public void ValidateTable() 
        {

            List<string> errors = null;

            foreach (KeyValuePair<Type, TypeMetadata> kv in _metadata) 
            {

                // Walk type attributes.  We don't need to compare these
                // to anything because there is no way for them to be
                // invalid.  We simply get them to ensure they don't throw

                GetCustomAttributes(kv.Key);

                // Walk member attributes.  We need to ensure that all member descriptors
                // of type LookupMemberDescriptor have matching members (property or event)
                // on the target type.  Other members are already validated by the fact
                // that they exist.

                if (kv.Value.MemberAttributes != null) 
                {

                    foreach (KeyValuePair<string, AttributeList> kvMember in kv.Value.MemberAttributes) 
                    {

                        // Validate that the attribute expansion doesn't throw
                        GetCustomAttributes(kv.Key, kvMember.Key);

                        // Validate that the member name matches a real proeprty/event and there
                        // are no duplicates

                        PropertyDescriptor p = DependencyPropertyDescriptor.FromName(kvMember.Key, kv.Key, typeof(DependencyObject));
                        EventDescriptor e = TypeDescriptor.GetEvents(kv.Key)[kvMember.Key];
                        if (p == null && e == null) 
                        {
                            p = TypeDescriptor.GetProperties(kv.Key)[kvMember.Key];
                        }

                        string errorMsg = null;
                        if (p == null && e == null) 
                        {
                            errorMsg = string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Error_ValidationNoMatchingMember,
                                kvMember.Key, kv.Key.FullName);
                        }
                        else if (p != null && e != null) 
                        {
                            errorMsg = string.Format(
                                CultureInfo.CurrentCulture,
                                Resources.Error_ValidationAmbiguousMember,
                                kvMember.Key, kv.Key.FullName);
                        }

                        if (errorMsg != null) 
                        {
                            if (errors == null) 
                            {
                                errors = new List<string>();
                            }
                            errors.Add(errorMsg);
                        }
                    }
                }
            }

            // Did we get any errors?
            if (errors != null) 
            {
                throw FxTrace.Exception.AsError(new AttributeTableValidationException(
                    Resources.Error_TableValidationFailed,
                    errors));
            }
        }

        //
        // We have a generic attribute expansion routine
        // that relies on someone else providing a mechanism
        // for returning the base attribute list.  If there
        // is no base list, this callback can return null.
        //
        private delegate AttributeList GetAttributesCallback(Type type, object callbackParam);

        //
        // All metadata for a type is stored here.
        //
        private class TypeMetadata 
        {
            internal AttributeList TypeAttributes;
            internal Dictionary<string, AttributeList> MemberAttributes;
        }

        //
        // Individual attributes for a member or type are stored
        // here.  Attribute lists can be "expanded", so their
        // callbacks are evaluated and their attributes are
        // merged with their base attribute list.
        //
        private class AttributeList : List<object> 
        {
            private bool _isExpanded;

            internal bool IsExpanded 
            {
                get { return _isExpanded; }
                set { _isExpanded = value; }
            }
        }
    }
}
