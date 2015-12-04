//---------------------------------------------------------------------
// <copyright file="ItemCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common.Utils;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    /// <summary>
    /// Class for representing a collection of items.
    /// Most of the implemetation for actual maintainance of the collection is
    /// done by MetadataCollection
    /// </summary>
    [CLSCompliant(false)]
    public abstract class ItemCollection : ReadOnlyMetadataCollection<GlobalItem>
    {
        #region Constructors
        /// <summary>
        /// The default constructor for ItemCollection
        /// </summary>
        internal ItemCollection(DataSpace dataspace)
            : base(new MetadataCollection<GlobalItem>())
        {
            _space = dataspace;
        }
        #endregion

        #region Fields
        private readonly DataSpace _space;
        private Dictionary<string, System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction>> _functionLookUpTable;
        private Memoizer<Type, ICollection> _itemsCache;
        private int _itemCount;
        #endregion

        #region Properties
        /// <summary>
        /// Dataspace associated with ItemCollection
        /// </summary>
        public DataSpace DataSpace
        {
            get
            {
                return this._space;
            }
        }

        /// <summary>
        /// Return the function lookUpTable
        /// </summary>
        internal Dictionary<string, System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction>> FunctionLookUpTable
        {
            get
            {
                if (_functionLookUpTable == null)
                {
                    Dictionary<string, System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction>> functionLookUpTable = PopulateFunctionLookUpTable(this);
                    Interlocked.CompareExchange(ref _functionLookUpTable, functionLookUpTable, null);
                }

                return _functionLookUpTable;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Adds an item to the collection 
        /// </summary>
        /// <param name="item">The item to add to the list</param>
        /// <exception cref="System.ArgumentNullException">Thrown if item argument is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the item passed in or the collection itself instance is in ReadOnly state</exception>
        /// <exception cref="System.ArgumentException">Thrown if the item that is being added already belongs to another ItemCollection</exception>
        /// <exception cref="System.ArgumentException">Thrown if the ItemCollection already contains an item with the same identity</exception>
        internal void AddInternal(GlobalItem item)
        {
            Debug.Assert(item.IsReadOnly, "The item is not readonly, it should be by the time it is added to the item collection");
            Debug.Assert(item.DataSpace == this.DataSpace);
            base.Source.Add(item);
        }

        /// <summary>
        /// Adds a collection of items to the collection 
        /// </summary>
        /// <param name="items">The items to add to the list</param>
        /// <exception cref="System.ArgumentNullException">Thrown if item argument is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the item passed in or the collection itself instance is in ReadOnly state</exception>
        /// <exception cref="System.ArgumentException">Thrown if the item that is being added already belongs to another ItemCollection</exception>
        /// <exception cref="System.ArgumentException">Thrown if the ItemCollection already contains an item with the same identity</exception>
        internal bool AtomicAddRange(List<GlobalItem> items)
        {
#if DEBUG
            // We failed to add, so undo the setting of the ItemCollection reference
            foreach (GlobalItem item in items)
            {
                Debug.Assert(item.IsReadOnly, "The item is not readonly, it should be by the time it is added to the item collection");
                Debug.Assert(item.DataSpace == this.DataSpace);
            }

#endif
            if (base.Source.AtomicAddRange(items))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns strongly typed MetadataItem from the collection that has
        /// the passed in identity.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity">Identity of the item to look up for</param>
        /// <returns>returns the item if a match is found, otherwise throwns an exception</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the Collection does not have an item with the given identity</exception>
        public T GetItem<T>(string identity) where T : GlobalItem
        {
            return this.GetItem<T>(identity, false /*ignoreCase*/);
        }

        /// <summary>
        /// Returns strongly typed MetadataItem from the collection that has
        /// the passed in identity.
        /// Returns null if the item is not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">if identity argument is null</exception>
        public bool TryGetItem<T>(string identity, out T item) where T : GlobalItem
        {
            return this.TryGetItem<T>(identity, false /*ignorecase*/, out item);
        }

        /// <summary>
        /// Returns strongly typed MetadataItem from the collection that has
        /// the passed in identity.
        /// Returns null if the item is not found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity">identity of the type to look up for</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="item">item with the given identity if a match is found, otherwise returns null</param>
        /// <returns>returns true if a match is found, otherwise returns false</returns>
        /// <exception cref="System.ArgumentNullException">if identity argument is null</exception>
        public bool TryGetItem<T>(string identity, bool ignoreCase, out T item) where T : GlobalItem
        {
            GlobalItem outItem = null;
            TryGetValue(identity, ignoreCase, out outItem);
            item = outItem as T;
            return item != null;
        }

        /// <summary>
        /// Returns strongly typed MetadataItem from the collection that has
        /// the passed in identity with either case sensitive or case insensitive search
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="identity">identity of the type to look up for</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <returns>returns item if a match is found, otherwise returns throws an argument exception</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if identity argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if no item is found with the given identity</exception>
        public T GetItem<T>(string identity, bool ignoreCase) where T : GlobalItem
        {
            T item;
            if (TryGetItem<T>(identity, ignoreCase, out item))
            {
                return item;
            }
            throw EntityUtil.ItemInvalidIdentity(identity, "identity");
        }

        /// <summary>
        /// Returns ReadOnlyCollection of the Items of the given type 
        /// in the item collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual System.Collections.ObjectModel.ReadOnlyCollection<T> GetItems<T>() where T : GlobalItem
        {
            Memoizer<Type, ICollection> currentValueForItemCache = _itemsCache;
            // initialize the memoizer, update the _itemCache and _itemCount
            if (_itemsCache == null || this._itemCount != this.Count)
            {
                Memoizer<Type, ICollection> itemsCache =
                               new Memoizer<Type, ICollection>(InternalGetItems, null);
                Interlocked.CompareExchange(ref _itemsCache, itemsCache, currentValueForItemCache);
                
                this._itemCount = this.Count;
            }

            Debug.Assert(_itemsCache != null, "check the initialization of the Memoizer");

            // use memoizer so that it won't create a new list every time this method get called
            ICollection items = this._itemsCache.Evaluate(typeof(T));
            System.Collections.ObjectModel.ReadOnlyCollection<T> returnItems = items as System.Collections.ObjectModel.ReadOnlyCollection<T>;

            return returnItems;
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        internal ICollection InternalGetItems(Type type)
        {
            MethodInfo mi = typeof(ItemCollection).GetMethod("GenericGetItems", BindingFlags.NonPublic | BindingFlags.Static);
            MethodInfo genericMi = mi.MakeGenericMethod(type);

            return genericMi.Invoke(null, new object[] { this }) as ICollection;
        }

        private static System.Collections.ObjectModel.ReadOnlyCollection<TItem> GenericGetItems<TItem>(ItemCollection collection) where TItem : GlobalItem
        {
            List<TItem> list = new List<TItem>();
            foreach (GlobalItem item in collection)
            {
                TItem stronglyTypedItem = item as TItem;
                if (stronglyTypedItem != null)
                {
                    list.Add(stronglyTypedItem);
                }
            }
            return list.AsReadOnly();
        }


        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if name or namespaceName arguments passed in are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if the ItemCollection for this space does not have a type with the given name and namespaceName</exception>
        public EdmType GetType(string name, string namespaceName)
        {
            return this.GetType(name, namespaceName, false /*ignoreCase*/);
        }

        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="type">The type that needs to be filled with the return value</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName argument is null</exception>
        public bool TryGetType(string name, string namespaceName, out EdmType type)
        {
            return this.TryGetType(name, namespaceName, false /*ignoreCase*/, out type);
        }

        /// <summary>
        /// Search for a type metadata with the specified key.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if name or namespaceName arguments passed in are null</exception>
        public EdmType GetType(string name, string namespaceName, bool ignoreCase)
        {
            EntityUtil.GenericCheckArgumentNull(name, "name");
            EntityUtil.GenericCheckArgumentNull(namespaceName, "namespaceName");
            return GetItem<EdmType>(EdmType.CreateEdmTypeIdentity(namespaceName, name), ignoreCase);
        }

        /// <summary>
        /// Search for a type metadata with the specified name and namespace name in the given space.
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="type">The type that needs to be filled with the return value</param>
        /// <returns>Returns null if no match found.</returns>
        /// <exception cref="System.ArgumentNullException">if name or namespaceName argument is null</exception>
        public bool TryGetType(string name, string namespaceName, bool ignoreCase, out EdmType type)
        {
            EntityUtil.GenericCheckArgumentNull(name, "name");
            EntityUtil.GenericCheckArgumentNull(namespaceName, "namespaceName");
            GlobalItem item = null;
            TryGetValue(EdmType.CreateEdmTypeIdentity(namespaceName, name), ignoreCase, out item);
            type = item as EdmType;
            return type != null;
        }

        /// <summary>
        /// Get all the overloads of the function with the given name
        /// </summary>
        /// <param name="functionName">The full name of the function</param>
        /// <returns>A collection of all the functions with the given name in the given data space</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if functionaName argument passed in is null</exception>
        public System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetFunctions(string functionName)
        {
            return this.GetFunctions(functionName, false /*ignoreCase*/);
        }

        /// <summary>
        /// Get all the overloads of the function with the given name
        /// </summary>
        /// <param name="functionName">The full name of the function</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <returns>A collection of all the functions with the given name in the given data space</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if functionaName argument passed in is null</exception>
        public System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetFunctions(string functionName, bool ignoreCase)
        {
            return GetFunctions(this.FunctionLookUpTable, functionName, ignoreCase);
        }

        /// <summary>
        /// Look for the functions in the given collection and 
        /// returns all the functions with the given name
        /// </summary>
        /// <param name="functionCollection"></param>
        /// <param name="functionName"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        protected static System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetFunctions(
            Dictionary<string, System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction>> functionCollection,
            string functionName, bool ignoreCase)
        {
            System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> functionOverloads;

            if (functionCollection.TryGetValue(functionName, out functionOverloads))
            {
                if (ignoreCase)
                {
                    return functionOverloads;
                }

                return GetCaseSensitiveFunctions(functionOverloads, functionName);
            }

            return Helper.EmptyEdmFunctionReadOnlyCollection;
        }

        internal static System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> GetCaseSensitiveFunctions(
            System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction> functionOverloads,
            string functionName)
        {
            // For case-sensitive match, first check if there are anything with a different case
            // its very rare to have functions with different case. So optimizing the case where all
            // functions are of same case
            // Else create a new list with the functions with the exact name
            List<EdmFunction> caseSensitiveFunctionOverloads = new List<EdmFunction>(functionOverloads.Count);

            for (int i = 0; i < functionOverloads.Count; i++)
            {
                if (functionOverloads[i].FullName == functionName)
                {
                    caseSensitiveFunctionOverloads.Add(functionOverloads[i]);
                }
            }

            // If there are no functions with different case, just return the collection
            if (caseSensitiveFunctionOverloads.Count != functionOverloads.Count)
            {
                functionOverloads = caseSensitiveFunctionOverloads.AsReadOnly();
            }
            return functionOverloads;
        }

        /// <summary>
        /// Gets the function as specified by the function key.
        /// All parameters are assumed to be <see cref="ParameterMode.In"/>.
        /// </summary>
        /// <param name="functionName">Name of the function</param>
        /// <param name="parameterTypes">types of the parameters</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="function">The function that needs to be returned</param>
        /// <returns> The function as specified in the function key or null</returns>
        /// <exception cref="System.ArgumentNullException">if functionName or parameterTypes argument is null</exception>
        /// <exception cref="System.ArgumentException">if no function is found with the given name or with given input parameters</exception>
        internal bool TryGetFunction(string functionName, TypeUsage[] parameterTypes, bool ignoreCase, out EdmFunction function)
        {
            EntityUtil.GenericCheckArgumentNull(functionName, "functionName");
            EntityUtil.GenericCheckArgumentNull(parameterTypes, "parameterTypes");
            string functionIdentity = EdmFunction.BuildIdentity(functionName, parameterTypes);
            GlobalItem item = null;
            function = null;
            if (TryGetValue(functionIdentity, ignoreCase, out item) && Helper.IsEdmFunction(item))
            {
                function = (EdmFunction)item;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <returns>The EntityContainer</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if name argument passed in is null</exception>
        public EntityContainer GetEntityContainer(string name)
        {
            EntityUtil.GenericCheckArgumentNull(name, "name");
            return this.GetEntityContainer(name, false /*ignoreCase*/);
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="entityContainer"></param>
        /// <exception cref="System.ArgumentNullException">if name argument is null</exception>
        public bool TryGetEntityContainer(string name, out EntityContainer entityContainer)
        {
            EntityUtil.GenericCheckArgumentNull(name, "name");
            return this.TryGetEntityContainer(name, false /*ignoreCase*/, out entityContainer);
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <returns>The EntityContainer</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if name argument passed in is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if no entity container with the given name is found</exception>
        public EntityContainer GetEntityContainer(string name, bool ignoreCase)
        {
            EntityContainer container = GetValue(name, ignoreCase) as EntityContainer;
            if (null != container)
            {
                return container;
            }
            throw EntityUtil.ItemInvalidIdentity(name, "name");
        }

        /// <summary>
        /// Get an entity container based upon the strong name of the container
        /// If no entity container is found, returns null, else returns the first one/// </summary>
        /// <param name="name">name of the entity container</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="entityContainer"></param>
        /// <exception cref="System.ArgumentNullException">if name argument is null</exception>
        public bool TryGetEntityContainer(string name, bool ignoreCase, out EntityContainer entityContainer)
        {
            EntityUtil.GenericCheckArgumentNull(name, "name");
            GlobalItem item = null;
            if (TryGetValue(name, ignoreCase, out item) && Helper.IsEntityContainer(item))
            {
                entityContainer = (EntityContainer)item;
                return true;
            }
            entityContainer = null;
            return false;
        }

        /// <summary>
        /// Given the canonical primitive type, get the mapping primitive type in the given dataspace
        /// </summary>
        /// <param name="primitiveTypeKind">canonical primitive type</param>
        /// <returns>The mapped scalar type</returns>
        internal virtual PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        {
            //The method needs to be overloaded on methods that support this
            throw System.Data.Entity.Error.NotSupported();
        }

        /// <summary>
        /// Determines whether this item collection is equivalent to another. At present, we look only
        /// at object reference equivalence. This is a somewhat reasonable approximation when caching
        /// is enabled, because collections are identical when their source resources (including 
        /// provider) are known to be identical.
        /// </summary>
        /// <param name="other">Collection to compare.</param>
        /// <returns>true if the collections are equivalent; false otherwise</returns>
        internal virtual bool MetadataEquals(ItemCollection other)
        {
            return Object.ReferenceEquals(this, other);
        }

        static private Dictionary<string, System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction>> PopulateFunctionLookUpTable(ItemCollection itemCollection)
        {
            var tempFunctionLookUpTable = new Dictionary<string, List<EdmFunction>>(StringComparer.OrdinalIgnoreCase);

            foreach (EdmFunction function in itemCollection.GetItems<EdmFunction>())
            {
                List<EdmFunction> functionList;
                if (!tempFunctionLookUpTable.TryGetValue(function.FullName, out functionList))
                {
                    functionList = new List<EdmFunction>();
                    tempFunctionLookUpTable[function.FullName] = functionList;
                }
                functionList.Add(function);
            }

            var functionLookUpTable = new Dictionary<string, System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction>>(StringComparer.OrdinalIgnoreCase);
            foreach (List<EdmFunction> functionList in tempFunctionLookUpTable.Values)
            {
                functionLookUpTable.Add(functionList[0].FullName, new System.Collections.ObjectModel.ReadOnlyCollection<EdmFunction>(functionList.ToArray()));
            }

            return functionLookUpTable;
        }

        #endregion
    }//---- ItemCollection
}//---- 
