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
    using System.Reflection;
    using System.Windows;
    using System.Activities.Presentation;

    // <summary>
    // An attribute table is a read only blob of data.  How
    // do you create one?  We will have a class called an
    // Attribute Builder that can be used to create an attribute
    // table.  Attribute builders have methods you can call to
    // add metadata.  When you�re finished, you can produce an
    // attribute table from the builder.  Builder methods also
    // support callback delegates so the entire process can be
    // deferred until needed.
    // </summary>
    public class AttributeTableBuilder 
    {

        private MutableAttributeTable _table = new MutableAttributeTable();
        private bool _cloneOnUse;

        //
        // Returns an attribute table we can make changes to
        //
        private MutableAttributeTable MutableTable 
        {
            get {
                if (_cloneOnUse) 
                {
                    MutableAttributeTable clone = new MutableAttributeTable();
                    clone.AddTable(_table);
                    _table = clone;
                    _cloneOnUse = false;
                }

                return _table;
            }
        }

        // <summary>
        // Adds a callback that will be invoked when metadata for the
        // given type is needed.  The callback can add metadata to
        // to the attribute table on demand, which is much more efficient
        // than adding metadata up front.
        // </summary>
        // <param name="type"></param>
        // <param name="callback"></param>
        public void AddCallback(Type type, AttributeCallback callback) 
        {
            if (type == null) 
            {
                throw FxTrace.Exception.ArgumentNull("type");
            }
            if (callback == null) 
            {
                throw FxTrace.Exception.ArgumentNull("callback");
            }
            MutableTable.AddCallback(type, callback);
        }

        // <summary>
        // Adds the contents of the provided attributes to this builder.
        // Conflicts are resolved with a last-in-wins strategy.  When
        // building a large attribute table it is best to use AddCallback
        // to defer the work of creating attributes until they are needed.
        // </summary>
        // <param name="type">The type to add class-level attributes to.</param>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        // <exception cref="ArgumentNullException">if type or attributes is null</exception>
        public void AddCustomAttributes(Type type, params Attribute[] attributes) {
            if (type == null) 
            {
                throw FxTrace.Exception.ArgumentNull("type");
            }
            if (attributes == null) 
            {
                throw FxTrace.Exception.ArgumentNull("attributes");
            }
            MutableTable.AddCustomAttributes(type, attributes);
        }

        // <summary>
        // Adds the contents of the provided attributes to this builder.
        // Conflicts are resolved with a last-in-wins strategy.  When
        // building a large attribute table it is best to use AddCallback
        // to defer the work of creating attributes until they are needed.
        // </summary>
        // <param name="ownerType">
        // The type the member lives on.
        // </param>
        // <param name="descriptor">An event or property descriptor to add attributes to.</param>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        // <exception cref="ArgumentNullException">if descriptor or attributes is null</exception>
        public void AddCustomAttributes(Type ownerType, MemberDescriptor descriptor, params Attribute[] attributes) {
            if (ownerType == null) 
            {
                throw FxTrace.Exception.ArgumentNull("ownerType");
            }
            if (descriptor == null) 
            {
                throw FxTrace.Exception.ArgumentNull("descriptor");
            }
            if (attributes == null) 
            {
                throw FxTrace.Exception.ArgumentNull("attributes");
            }
            MutableTable.AddCustomAttributes(ownerType, descriptor, attributes);
        }

        // <summary>
        // Adds the contents of the provided attributes to this builder.
        // Conflicts are resolved with a last-in-wins strategy.  When
        // building a large attribute table it is best to use AddCallback
        // to defer the work of creating attributes until they are needed.
        // </summary>
        // <param name="ownerType">
        // The type the member lives on.
        // </param>
        // <param name="member">An event or property info to add attributes to.</param>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        // <exception cref="ArgumentNullException">if member or attributes is null</exception>
        public void AddCustomAttributes(Type ownerType, MemberInfo member, params Attribute[] attributes) {
            if (ownerType == null) 
            {
                throw FxTrace.Exception.ArgumentNull("ownerType");
            }
            if (member == null) 
            {
                throw FxTrace.Exception.ArgumentNull("member");
            }
            if (attributes == null) 
            {
                throw FxTrace.Exception.ArgumentNull("attributes");
            }
            MutableTable.AddCustomAttributes(ownerType, member, attributes);
        }

        // <summary>
        // Adds attributes to the member with the given name.  The member can be a property
        // or an event.  The member is evaluated on demand when the user queries
        // attributes on a given property or event.
        // </summary>
        // <param name="ownerType">
        // The type the member lives on.
        // </param>
        // <param name="memberName">
        // The member to add attributes for.  Only property and event members are supported;
        // all others will be ignored.
        // </param>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        public void AddCustomAttributes(Type ownerType, string memberName, params Attribute[] attributes) {
            if (ownerType == null) 
            {
                throw FxTrace.Exception.ArgumentNull("ownerType");
            }
            if (memberName == null) 
            {
                throw FxTrace.Exception.ArgumentNull("memberName");
            }
            MutableTable.AddCustomAttributes(ownerType, memberName, attributes);
        }

        // <summary>
        // Adds the contents of the provided attributes to this builder.
        // Conflicts are resolved with a last-in-wins strategy.  When
        // building a large attribute table it is best to use AddCallback
        // to defer the work of creating attributes until they are needed.
        // </summary>
        // <param name="ownerType">
        // The type that owns the dependency property.
        // </param>
        // <param name="dp">A dependency property to add attributes to.</param>
        // <param name="attributes">
        // The new attributes to add.
        // </param>
        // <exception cref="ArgumentNullException">if dp, ownerType or attributes is null</exception>
        public void AddCustomAttributes(Type ownerType, DependencyProperty dp, params Attribute[] attributes) {
            if (ownerType == null) 
            {
                throw FxTrace.Exception.ArgumentNull("ownerType");
            }
            if (dp == null) 
            {
                throw FxTrace.Exception.ArgumentNull("dp");
            }
            if (attributes == null) 
            {
                throw FxTrace.Exception.ArgumentNull("attributes");
            }
            MutableTable.AddCustomAttributes(ownerType, dp, attributes);
        }

        // <summary>
        // Adds the contents of the provided attribute table to
        // this builder.  Conflicts are resolved with a last-in-wins
        // strategy.
        // </summary>
        // <param name="table">An existing attribute table.</param>
        // <exception cref="ArgumentNullException">if table is null</exception>
        public void AddTable(AttributeTable table) 
        {
            if (table == null) 
            {
                throw FxTrace.Exception.ArgumentNull("table");
            }
            MutableTable.AddTable(table.MutableTable);
        }

        // <summary>
        // Creates an attribute table that contains all of the attribute
        // definitions provided through AddAttribute calls.  The table is
        // a snapshot of the current state of the attribute builder; any
        // subsequent AddAttribute calls are not included in the table.
        //
        // If callback methods were used to declare attributes, those methods
        // will not be evaluated during CreateTable.  Instead, the table will
        // contain those callbacks and will evaluate them as needed.
        // </summary>
        // <returns>
        // An attribute table that can be passed to the metadata store.
        // </returns>
        public AttributeTable CreateTable() 
        {
            _cloneOnUse = true;
            return new AttributeTable(_table);
        }

        // <summary>
        // This method can be used to verify that the attribute table
        // that is being built contains valid attribute information.
        // Some overrides of AddCustomAttributes cannot validate that
        // values passed to their prameters represent valid members on
        // classes.  Therefore, incorrect information passed to
        // AddCustomAttributes may go undetected.  ValidateTable will
        // run through the contents of the AttributeTableBuilder and
        // verify that all custom attribute information matches up with
        // physical members.  Note:  calling this method can be very
        // costly so you should only do it when validation is absolutely
        // needed.
        // </summary>
        // <exception cref="AttributeTableValidationException">if the state of the table is invalid.</exception>
        public void ValidateTable() 
        {
            MutableTable.ValidateTable();
        }
    }
}
