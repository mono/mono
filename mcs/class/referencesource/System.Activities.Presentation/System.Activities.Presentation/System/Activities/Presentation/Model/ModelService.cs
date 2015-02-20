//------------------------------------------------------------------------------
// <copyright file="ModelService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Services {

    using System;
    using System.Collections.Generic;
    using System.Activities.Presentation.Model;

    /// <summary>
    /// The ModelService class is the main entry point the designer 
    /// uses to obtain the model.  The service actually has a split 
    /// between public and protected methods you must implement.  
    /// The public methods are (obviously) callable by anyone.  
    /// The protected methods are invoked by the model.
    /// </summary>
    public abstract class ModelService {

        /// <summary>
        /// Constructs a new ModelService.
        /// </summary>
        protected ModelService() {
        }

        /// <summary>
        /// The root of the object hierarchy.  For purely linear stores 
        /// this will be the first object in the store.  For stores that 
        /// represent a tree of objects this returns the topmost node of 
        /// the tree.
        /// </summary>
        public abstract ModelItem Root { get; }

        /// <summary>
        /// This event is raised when something in the model has changed.  
        /// The event args in the event can be used to find what has changed.
        /// </summary>
        public abstract event EventHandler<ModelChangedEventArgs> ModelChanged;

        /// <summary>
        /// Creates a ModelItem for a given type.  This method is called by 
        /// ModelFactory when the user wishes to create a new item.
        /// </summary>
        /// <param name="itemType">
        /// The type of item to create.
        /// </param>
        /// <param name="options">
        /// Creation options.  You can specify if you would like to initialize 
        /// default values for an item.
        /// </param>
        /// <param name="arguments">
        /// An array of arguments to the constructor of the item.
        /// </param>
        /// <returns>The newly created model item.</returns>
        /// <exception cref="ArgumentNullException">if itemType is null</exception>
        protected abstract ModelItem CreateItem(Type itemType, CreateOptions options, params object[] arguments);

        /// <summary>
        /// Takes an existing instance and creates a model item that is a deep clone
        /// of the instance.
        /// </summary>
        /// <param name="item">
        /// The item to wrap.
        /// </param>
        /// <returns>A newly created model item that is a clone of the existing item.</returns>
        /// <exception cref="ArgumentNullException">if item is null</exception>
        protected abstract ModelItem CreateItem(object item);

        /// <summary>
        /// Create a new model item that represents a the value of a static member of a the given class.
        /// For example, to add a reference to Brushes.Red to the model call this methods with 
        /// typeof(Brushes) and the string "Red". This will be serialized into XAML as 
        /// {x:Static Brushes.Red}.
        /// </summary>
        /// <param name="type">
        /// The type that contains the static member being referenced.
        /// </param>
        /// <param name="memberName">
        /// The name of the static member being referenced.
        /// </param>
        protected abstract ModelItem CreateStaticMemberItem(Type type, string memberName);

        /// <summary>
        /// Finds matching model items given a starting point to look.  All 
        /// walks are recursive.
        /// </summary>
        /// <param name="startingItem">
        /// The model item to start the search.  Items above this item 
        /// will be ignored.  This item, and any item below it in the 
        /// hierarchy, will be included in the search.  If this value is 
        /// null, the root is used.
        /// </param>
        /// <param name="type">
        /// The type of the object to find.  This will enumerate all items 
        /// within the given parent scope that are of the requested type.
        /// </param>
        /// <returns>
        /// An enumeration of model items matching the query.
        /// </returns>
        /// <exception cref="ArgumentNullException">if type is null</exception>
        public abstract IEnumerable<ModelItem> Find(ModelItem startingItem, Type type);

        /// <summary>
        /// Finds matching model items given a starting point to look.  All 
        /// walks are recursive.
        /// </summary>
        /// <param name="startingItem">
        /// The model item to start the search.  Items above this item 
        /// will be ignored.  This item, and any item below it in the 
        /// hierarchy, will be included in the search.  If this value is 
        /// null, the root is used.
        /// </param>
        /// <param name="match">
        /// A predicate that allows more complex type matching to be used.  
        /// For example, the predicate could return true for both 
        /// FrameworkElement and FrameworkContentElement.
        /// </param>
        /// <returns>
        /// An enumeration of model items matching the query.
        /// </returns>
        /// <exception cref="ArgumentNullException">if match is null</exception>
        public abstract IEnumerable<ModelItem> Find(ModelItem startingItem, Predicate<Type> match);

        /// <summary>
        /// Locates the model item in the given scope with the given name.  Returns null if 
        /// the model item could not be located.
        /// </summary>
        /// <param name="scope">
        /// An optional scope to provide.  If not provided, the root element will
        /// be used as a scope.  If provided, the nearest INameScope in the hierarchy
        /// will be used to locate the item.
        /// </param>
        /// <param name="name">
        /// The name to locate.
        /// </param>
        /// <returns>
        /// A model item whose name matches that provided, or null if no match was
        /// found.
        /// </returns>
        /// <exception cref="ArgumentNullException">If name is null.</exception>
        public ModelItem FromName(ModelItem scope, string name) {
            return FromName(scope, name, StringComparison.Ordinal);
        }

        /// <summary>
        /// Locates the model item in the given scope with the given name.  Returns null if 
        /// the model item could not be located.
        /// </summary>
        /// <param name="scope">
        /// An optional scope to provide.  If not provided, the root element will
        /// be used as a scope.  If provided, the nearest INameScope in the hierarchy
        /// will be used to locate the item.
        /// </param>
        /// <param name="name">
        /// The name to locate.
        /// </param>
        /// <param name="comparison">
        /// Determines how the name should be compared.  The default is to compare against
        /// ordinal.
        /// </param>
        /// <returns>
        /// A model item whose name matches that provided, or null if no match was
        /// found.
        /// </returns>
        /// <exception cref="ArgumentNullException">If name is null.</exception>
        public abstract ModelItem FromName(ModelItem scope, string name, StringComparison comparison);

        /// <summary>
        /// Creates a ModelItem for a given type.  This method is called by 
        /// ModelFactory when the user wishes to create a new item.
        /// </summary>
        internal ModelItem InvokeCreateItem(Type itemType, CreateOptions options, params object[] arguments) {
            return CreateItem(itemType, options, arguments);
        }

        /// <summary>
        /// Creates a member item that refers to a static member of the given type.
        /// </summary>
        internal ModelItem InvokeCreateStaticMemberItem(Type type, string memberName) {
            return CreateStaticMemberItem(type, memberName);
        }

        /// <summary>
        /// Takes an existing instance and wraps it in a ModelItem.  The set 
        /// properties on the instance are promoted to the model item.
        /// </summary>
        internal ModelItem InvokeCreateItem(object item) {
            return CreateItem(item);
        }
    }
}
