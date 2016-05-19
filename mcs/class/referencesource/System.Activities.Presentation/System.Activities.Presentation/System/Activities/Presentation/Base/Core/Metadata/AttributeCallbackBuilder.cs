//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Metadata 
{
    using System.Runtime;

    using System.Activities.Presentation.Internal.Metadata;
    using System.Activities.Presentation.Internal.Properties;

    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Windows;
    using System.Activities.Presentation;

    // <summary>
    // An instance of this class is passed to callback delegates to lazily
    // populate the attributes for a type.
    // </summary>
    [Fx.Tag.XamlVisible(false)]
    public sealed class AttributeCallbackBuilder 
    {
        private MutableAttributeTable _table;
        private Type _callbackType;

        internal AttributeCallbackBuilder(MutableAttributeTable table, Type callbackType) 
        {
            _table = table;
            _callbackType = callbackType;
        }

        // <summary>
        // The type this callback is being invoked for.
        // </summary>
        public Type CallbackType 
        {
            get { return _callbackType; }
        }

        // <summary>
        // Adds the contents of the provided attributes to this builder.
        // Conflicts are resolved with a last-in-wins strategy.
        // </summary>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        // <exception cref="ArgumentNullException">if type or attributes is null</exception>
        public void AddCustomAttributes(params Attribute[] attributes) {
            if (attributes == null) 
            {
                throw FxTrace.Exception.ArgumentNull("attributes");
            }
            _table.AddCustomAttributes(_callbackType, attributes);
        }

        // <summary>
        // Adds the contents of the provided attributes to this builder.
        // Conflicts are resolved with a last-in-wins strategy.
        // </summary>
        // <param name="descriptor">An event or property descriptor to add attributes to.</param>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        // <exception cref="ArgumentNullException">if descriptor or attributes is null</exception>
        public void AddCustomAttributes(MemberDescriptor descriptor, params Attribute[] attributes) {
            if (descriptor == null) 
            {
                throw FxTrace.Exception.ArgumentNull("descriptor");
            }
            if (attributes == null) 
            {
                throw FxTrace.Exception.ArgumentNull("attributes");
            }
            _table.AddCustomAttributes(_callbackType, descriptor, attributes);
        }

        // <summary>
        // Adds the contents of the provided attributes to this builder.
        // Conflicts are resolved with a last-in-wins strategy.
        // </summary>
        // <param name="member">An event or property info to add attributes to.</param>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        // <exception cref="ArgumentNullException">if member or attributes is null</exception>
        public void AddCustomAttributes(MemberInfo member, params Attribute[] attributes) {
            if (member == null) 
            {
                throw FxTrace.Exception.ArgumentNull("member");
            }
            if (attributes == null) 
            {
                throw FxTrace.Exception.ArgumentNull("attributes");
            }
            _table.AddCustomAttributes(_callbackType, member, attributes);
        }

        // <summary>
        // Adds attributes to the member with the given name.  The member can be a property
        // or an event.  The member is evaluated on demand when the user queries
        // attributes on a given property or event.
        // </summary>
        // <param name="memberName">
        // The member to add attributes for.  Only property and event members are supported;
        // all others will be ignored.
        // </param>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        public void AddCustomAttributes(string memberName, params Attribute[] attributes) {
            if (memberName == null) 
            {
                throw FxTrace.Exception.ArgumentNull("memberName");
            }
            if (attributes == null) 
            {
                throw FxTrace.Exception.ArgumentNull("attributes");
            }
            _table.AddCustomAttributes(_callbackType, memberName, attributes);
        }

        // <summary>
        // Adds the contents of the provided attributes to this builder.
        // Conflicts are resolved with a last-in-wins strategy.
        // </summary>
        // <param name="dp">A dependency property to add attributes to.</param>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        // <exception cref="ArgumentNullException">if dp or attributes is null</exception>
        public void AddCustomAttributes(DependencyProperty dp, params Attribute[] attributes) {
            if (dp == null) 
            {
                throw FxTrace.Exception.ArgumentNull("dp");
            }
            if (attributes == null) 
            {
                throw FxTrace.Exception.ArgumentNull("attributes");
            }
            _table.AddCustomAttributes(_callbackType, dp, attributes);
        }
    }
}
