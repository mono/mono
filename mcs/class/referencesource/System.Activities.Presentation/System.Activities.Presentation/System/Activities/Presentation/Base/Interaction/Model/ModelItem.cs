//------------------------------------------------------------------------------
// <copyright file="ModelItem.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Model 
{

    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Collections.Generic;

    // <summary>
    // The ModelItem class represents a single item in the editing model.  An
    // item can be anything from a Window or Control down to a color or integer.
    //
    // You may access the item�s properties through its Properties collection
    // and make changes to the values of the properties.
    //
    // A ModelItem is essentially a wrapper around the designer�s underlying
    // data model.  You can access the underlying model through the
    // GetCurrentValue method.  Note that you should never make any serializable
    // changes to an object returned from the GetCurrentValue method:  it will
    // not be reflected back in the designer�s serialization or undo systems.
    // </summary>
    public abstract class ModelItem : INotifyPropertyChanged 
    {

        // <summary>
        // Creates a new ModelItem.
        // </summary>
        protected ModelItem() 
        {
        }

        // <summary>
        // Implements INotifyPropertyChanged.  This can be used to tell when a property
        // on the model changes.  It is also useful because INotifyPropertyChanged can
        // be used by the data binding features of WPF.
        //
        // You should take care to disconnect events from items when you are finished
        // when them.  Otherwise you can prevent the item from garbage collecting.
        // </summary>
        public abstract event PropertyChangedEventHandler PropertyChanged;

        // <summary>
        // Returns the attributes declared on this item.
        // </summary>
        public abstract AttributeCollection Attributes { get; }

        // <summary>
        // If this item's ItemType declares a ContentPropertyAttribute,
        // that property will be exposed here.  Otherwise, this will
        // return null.
        // </summary>
        public abstract ModelProperty Content { get; }

        // <summary>
        // Returns the type of object the item represents.
        // </summary>
        public abstract Type ItemType { get; }

        // <summary>
        // This property represents the name or ID of the item.  Not
        // all items need to have names so this may return null.  Also,
        // depending on the type of item and where it sits in the
        // hierarchy, it may not always be legal to set the name on
        // an item.  If this item's ItemType declares a
        // RuntimeNamePropertyAttribute, this Name property will be
        // a direct mapping to the property dictated by that attribute.
        // </summary>
        public abstract string Name { get; set; }

        // <summary>
        // Returns the item that is the parent of this item.
        // If an item is contained in a collection or dictionary
        // the collection or dictionary is skipped, returning the
        // object owns the collection or dictionary.
        // </summary>
        public abstract ModelItem Parent { get; }

        // <summary>
        // Returns the al the parents of this item.
        // </summary>
        public abstract IEnumerable<ModelItem> Parents { get; }

        // <summary>
        // Returns the item that is the root of this tree.
        // If there is no root yet, as this isn't part of a tree
        // returns null.
        // </summary>
        public abstract ModelItem Root { get; }

        // <summary>
        // Returns the public properties on this object.  The set of
        // properties returned may change based on attached
        // properties or changes to the editing scope.
        // </summary>
        public abstract ModelPropertyCollection Properties { get; }

        // <summary>
        // Returns the property that provided this value. If the item represents
        // the root of the object graph, this will return null.  If an item is a
        // member of a collection or dictionary, the property returned from Source
        // will be a pseudo-property provided by the collection or dictionary.
        // For other values, the Source property returns the property where the
        // value was actually set.  Therefore, if a value is being inherited,
        // Source allows you to find out who originally provided the value.
        // </summary>
        public abstract ModelProperty Source { get; }


        // <summary>
        // Returns all the properties that hold this value.
        // </summary>
        public abstract IEnumerable<ModelProperty> Sources { get; }

        // <summary>
        // Returns the visual or visual3D representing the UI for this item.  This
        // may return null if there is no view for this object.
        // </summary>
        public abstract DependencyObject View { get; }

        // <summary>
        // If you are doing multiple operations on an object or group of objects
        // you may call BeginEdit.  Once an editing scope is open, all changes
        // across all objects will be saved into the scope.
        //
        // Editing scopes are global to the designer.  An editing scope may be
        // created for any item in the designer; you do not need to create an
        // editing scope for the specific item you are changing.
        //
        // Editing scopes can be nested, but must be committed in order.
        // </summary>
        // <returns>
        // An editing scope that must be either completed or reverted.
        // </returns>
        public abstract ModelEditingScope BeginEdit();

        // <summary>
        // If you are doing multiple operations on an object or group of objects
        // you may call BeginEdit.  Once an editing scope is open, all changes
        // across all objects will be saved into the scope.
        //
        // Editing scopes are global to the designer.  An editing scope may be
        // created for any item in the designer; you do not need to create an
        // editing scope for the specific item you are changing.
        //
        // Editing scopes can be nested, but must be committed in order.
        // </summary>
        // <param name="description">
        // An optional description that describes the change.  This will be set
        // into the editing scope�s Description property.
        // </param>
        // <returns>
        // An editing scope that must be either completed or reverted.
        // </returns>
        public abstract ModelEditingScope BeginEdit(string description);

        // <summary>
        // If you are doing multiple operations on an object or group of objects
        // you may call BeginEdit.  Once an editing scope is open, all changes
        // across all objects will be saved into the scope.
        //
        // Editing scopes are global to the designer.  An editing scope may be
        // created for any item in the designer; you do not need to create an
        // editing scope for the specific item you are changing.
        //
        // Editing scopes can be nested, but must be committed in order.
        // </summary>
        // <param name="shouldApplyChangesImmediately">
        // A flag to control whether changes should take effect immediately.
        // </param>
        // <returns>
        // An editing scope that must be either completed or reverted.
        // </returns>
        // <exception cref="NotImplementedException">
        // If the derived class doesn't override this method.
        // </exception>
        // <exception cref="InvalidOperationException">
        // If shouldApplyChangesImmediately == true but the outer editing scope is not null.
        // </exception>
        public virtual ModelEditingScope BeginEdit(bool shouldApplyChangesImmediately)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        // <summary>
        // If you are doing multiple operations on an object or group of objects
        // you may call BeginEdit.  Once an editing scope is open, all changes
        // across all objects will be saved into the scope.
        //
        // Editing scopes are global to the designer.  An editing scope may be
        // created for any item in the designer; you do not need to create an
        // editing scope for the specific item you are changing.
        //
        // Editing scopes can be nested, but must be committed in order.
        // </summary>
        // <param name="description">
        // An optional description that describes the change.  This will be set
        // into the editing scope�s Description property.
        // </param>
        // <param name="shouldApplyChangesImmediately">
        // A flag to control whether changes should take effect immediately.
        // </param>
        // <returns>
        // An editing scope that must be either completed or reverted.
        // </returns>
        // <exception cref="NotImplementedException">
        // If the derived class doesn't override this method.
        // </exception>
        // <exception cref="InvalidOperationException">
        // If shouldApplyChangesImmediately == true but the outer editing scope is not null.
        // </exception>
        public virtual ModelEditingScope BeginEdit(string description, bool shouldApplyChangesImmediately)
        {
            throw FxTrace.Exception.AsError(new NotImplementedException());
        }

        // <summary>
        // Returns the current value of the underlying model object the ModelItem
        // is wrapping.  You can inspect this object, but you should not make any
        // changes to it.  Changes made to the object returned will not be incorporated
        // into the designer.  The GetCurrentValue method may return either an existing
        // or new cloned instance of the object.
        // </summary>
        // <returns>
        // Returns the current value of the underlying model object the ModelItem is wrapping.
        // </returns>
        public abstract object GetCurrentValue();

        // <summary>
        // Returns string representation of the ModelItem which is the string representation 
        // of underlying object the ModelItem is wrapping.
        // </summary>
        // <returns>
        // Returns string representation of the ModelItem.
        // </returns>        
        public override string ToString()
        {                        
            object instance = this.GetCurrentValue();
            if (instance != null)
            {
                return instance.ToString();
            }
            else 
            {
                return base.ToString();
            }
        }

        internal virtual event PropertyReferenceChangedEventHandler PropertyReferenceChanged;

        internal virtual void OnPropertyReferenceChanged(string targetProperty)
        {
            if (this.PropertyReferenceChanged != null)
            {
                this.PropertyReferenceChanged(this, new PropertyReferenceChangedEventArgs(targetProperty));
            }
        }
    }
}
