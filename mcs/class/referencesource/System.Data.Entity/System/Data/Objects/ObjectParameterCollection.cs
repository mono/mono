//---------------------------------------------------------------------
// <copyright file="ObjectParameterCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupowner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Objects
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Text;

    /// <summary>
    ///   This class represents a collection of query parameters at the object layer.
    /// </summary>
    public sealed class ObjectParameterCollection : ICollection<ObjectParameter>
    {
        // Note: There are NO public constructors for this class - it is for internal
        // ObjectQuery<T> use only, but must be public so that an instance thereof can be
        // a public property on ObjectQuery<T>.
    
        #region Internal Constructors

        // ---------------------
        // Internal Constructors
        // ---------------------

        #region ObjectParameterCollection (ClrPerspective)

        /// <summary>
        ///   This internal constructor creates a new query parameter collection and
        ///   initializes the internal parameter storage.
        /// </summary>
        internal ObjectParameterCollection(ClrPerspective perspective)
        {
            EntityUtil.CheckArgumentNull(perspective, "perspective");
 
            // The perspective is required to do type-checking on parameters as they
            // are added to the collection.
            this._perspective = perspective;

            // Create a new list to store the parameters.
            _parameters = new List<ObjectParameter>();
        }

        #endregion

        #endregion

        #region Private Fields

        // --------------
        // Private Fields
        // --------------

        /// <summary>
        ///     Can parameters be added or removed from this collection?
        /// </summary>
        private bool _locked;

        /// <summary>
        ///   The internal storage for the query parameters in the collection.
        /// </summary>
        private List<ObjectParameter> _parameters;

        /// <summary>
        ///   A CLR perspective necessary to do type-checking on parameters as they 
        ///   are added to the collection.
        /// </summary>
        private ClrPerspective _perspective;

        /// <summary>
        /// A string that can be used to represent the current state of this parameter collection in an ObjectQuery cache key.
        /// </summary>
        private string _cacheKey;

        #endregion

        #region Public Properties

        // -----------------
        // Public Properties
        // -----------------

        /// <summary>
        ///   The number of parameters currently in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return this._parameters.Count;
            }
        }

        /// <summary>
        ///   This collection is read-write - parameters may be added, removed
        ///   and [somewhat] modified at will (value only) - provided that the
        ///   implementation the collection belongs to has not locked its parameters
        ///   because it's command definition has been prepared.
        /// </summary>
        bool ICollection<ObjectParameter>.IsReadOnly
        {
            get
            {
                return (this._locked);
            }
        }

        #endregion

        #region Public Indexers

        // ---------------
        // Public Indexers
        // ---------------

        /// <summary>
        ///   This indexer allows callers to retrieve parameters by name. If no
        ///   parameter by the given name exists, an exception is thrown. For
        ///   safe existence-checking, use the Contains method instead.
        /// </summary>
        /// <param name="name">
        ///   The name of the parameter to find.
        /// </param>
        /// <returns>
        ///   The parameter object with the specified name.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   If no parameter with the specified name is found in the collection.
        /// </exception>
        public ObjectParameter this[string name]
        {
            get
            {
                int index = this.IndexOf(name);

                if (index == -1)
                {
                    throw EntityUtil.ArgumentOutOfRange(System.Data.Entity.Strings.ObjectParameterCollection_ParameterNameNotFound(name), "name");
                }

                return this._parameters[index];
            }
        }

        #endregion

        #region Public Methods

        // --------------
        // Public Methods
        // --------------

        #region Add

        /// <summary>
        ///   This method adds the specified parameter object to the collection. If
        ///   the parameter object already exists in the collection, an exception is 
        ///   thrown.
        /// </summary>
        /// <param name="parameter">
        ///   The parameter object to add to the collection.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        ///   If the value of the parameter argument is null.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   If the parameter argument already exists in the collection. This 
        ///   behavior differs from that of most collections which allow duplicate
        ///   entries.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///   If another parameter with the same name as the parameter argument
        ///   already exists in the collection. Note that the lookup is case-
        ///   insensitive. This behavior differs from that of most collections,  
        ///   and is more like that of a Dictionary.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///   If the type of the specified parameter is invalid. 
        /// </exception>
        public void Add (ObjectParameter parameter)
        {
            EntityUtil.CheckArgumentNull(parameter, "parameter");
            CheckUnlocked();

            if (this.Contains(parameter))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectParameterCollection_ParameterAlreadyExists(parameter.Name), "parameter");
            }

            if (this.Contains(parameter.Name))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.ObjectParameterCollection_DuplicateParameterName(parameter.Name), "parameter");
            }

            if (!parameter.ValidateParameterType(this._perspective))
            {
                throw EntityUtil.ArgumentOutOfRange(System.Data.Entity.Strings.ObjectParameter_InvalidParameterType(parameter.ParameterType.FullName), "parameter");
            }

            this._parameters.Add(parameter);
            this._cacheKey = null;
        }

        #endregion

        #region Clear

        /// <summary>
        ///   This method empties the entire parameter collection.
        /// </summary>
        /// <returns></returns>
        public void Clear()
        {
            CheckUnlocked();
            this._parameters.Clear();
            this._cacheKey = null;
        }

        #endregion

        #region Contains (ObjectParameter)

        /// <summary>
        ///   This methods checks for the existence of a given parameter object in the
        ///   collection by reference.
        /// </summary>
        /// <param name="parameter">
        ///   The parameter object to look for in the collection.
        /// </param>
        /// <returns>
        ///   True if the parameter object was found in the collection, false otherwise.
        ///   Note that this is a reference-based lookup, which means that if the para-
        ///   meter argument has the same name as a parameter object in the collection,
        ///   this method will only return true if it's the same object. 
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If the value of the parameter argument is null.
        /// </exception>
        public bool Contains (ObjectParameter parameter)
        {
            EntityUtil.CheckArgumentNull(parameter, "parameter");
            
            return this._parameters.Contains(parameter);
        }

        #endregion

        #region Contains (string)

        /// <summary>
        ///   This method checks for the existence of a given parameter in the collection
        ///   by name. 
        /// </summary>
        /// <param name="name">
        ///   The name of the parameter to look for in the collection.
        /// </param>
        /// <returns>
        ///   True if a parameter with the specified name was found in the collection,
        ///   false otherwise. Note that the lookup is case-insensitive.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If the value of the parameter argument is null.
        /// </exception>
        public bool Contains (string name)
        {
            EntityUtil.CheckArgumentNull(name, "name");
            
            if (this.IndexOf(name) != -1)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region CopyTo

        /// <summary>
        ///   This method allows the parameters in the collection to be copied into a
        ///   supplied array, beginning at the specified index therein.
        /// </summary>
        /// <param name="array">
        ///   The array into which to copy the parameters.
        /// </param>
        /// <param name="index">
        ///   The index in the array at which to start copying the parameters.
        /// </param>
        /// <returns></returns>
        public void CopyTo (ObjectParameter[] array, int index)
        {
            this._parameters.CopyTo(array, index);
        }

        #endregion

        #region Remove

        /// <summary>
        ///   This method removes an instance of a parameter from the collection by 
        ///   reference if it exists in the collection.  To remove a parameter by name, 
        ///   first use the Contains(name) method or this[name] indexer to retrieve
        ///   the parameter instance, then remove it using this method.
        /// </summary>
        /// <param name="parameter">
        ///   The parameter object to remove from the collection.
        /// </param>
        /// <returns>
        ///   True if the parameter object was found and removed from the collection,
        ///   false otherwise. Note that this is a reference-based lookup, which means 
        ///   that if the parameter argument has the same name as a parameter object 
        ///   in the collection, this method will remove it only if it's the same object.  
        /// </returns>
        /// <exception cref="ArgumentNullException">
        ///   If the value of the parameter argument is null.
        /// </exception>
        public bool Remove (ObjectParameter parameter)
        {
            EntityUtil.CheckArgumentNull(parameter, "parameter");
            CheckUnlocked();
            
            bool removed = this._parameters.Remove(parameter);

            // If the specified parameter was found in the collection and removed, 
            // clear out the cached string representation of this parameter collection
            // so that the next call to GetCacheKey (if any) will regenerate it based on
            // the new state of this collection.
            if (removed)
            {
                this._cacheKey = null;
            }

            return removed;
        }

        #endregion
  
        #region GetEnumerator

        /// <summary>
        ///   These methods return enumerator instances, which allow the collection to
        ///   be iterated through and traversed.
        /// </summary>
        IEnumerator<ObjectParameter> IEnumerable<ObjectParameter>.GetEnumerator() 
        {
            return ((System.Collections.Generic.ICollection<ObjectParameter>)this._parameters).GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator() 
        {
            return ((System.Collections.ICollection)this._parameters).GetEnumerator();
        }

        #endregion

        #endregion

        #region Internal Methods

        // ---------------
        // Internal Methods
        // ---------------

        /// <summary>
        /// Retrieves a string that may be used to represent this parameter collection in an ObjectQuery cache key.
        /// If this collection has not changed since the last call to this method, the same string instance is returned.
        /// Note that this string is used by various ObjectQueryImplementations to version the parameter collection.
        /// </summary>
        /// <returns>A string that may be used to represent this parameter collection in an ObjectQuery cache key.</returns>
        internal string GetCacheKey()
        {
            if (null == this._cacheKey)
            {
                if(this._parameters.Count > 0)
                {
                    // Future Enhancement: If the separate branch for a single parameter does not have a measurable perf advantage, remove it.
                    if (1 == this._parameters.Count)
                    {
                        // if its one parameter only, there is no need to use stringbuilder
                        ObjectParameter theParam = this._parameters[0];
                        this._cacheKey = "@@1" + theParam.Name + ":" + theParam.ParameterType.FullName;
                    }
                    else
                    {
                        // Future Enhancement: Investigate whether precalculating the required size of the string builder is a better time/space tradeoff.
                        StringBuilder keyBuilder = new StringBuilder(this._parameters.Count * 20);
                        keyBuilder.Append("@@");
                        keyBuilder.Append(this._parameters.Count);
                        for (int idx = 0; idx < this._parameters.Count; idx++)
                        {
                            //
                            // 

                            if (idx > 0)
                            {
                                keyBuilder.Append(";");
                            }

                            ObjectParameter thisParam = this._parameters[idx];
                            keyBuilder.Append(thisParam.Name);
                            keyBuilder.Append(":");
                            keyBuilder.Append(thisParam.ParameterType.FullName);
                        }

                        this._cacheKey = keyBuilder.ToString();
                    }
                }
            }

            return this._cacheKey;
        }

        /// <summary>
        /// Locks or unlocks this parameter collection, allowing its contents to be added to, removed from, or cleared.
        /// Calling this method consecutively with the same value has no effect but does not throw an exception.
        /// </summary>
        /// <param name="isReadOnly">If <c>true</c>, this parameter collection is now locked; otherwise it is unlocked</param>
        internal void SetReadOnly(bool isReadOnly) { this._locked = isReadOnly; }

        /// <summary>
        /// Creates a new copy of the specified parameter collection containing copies of its element <see cref="ObjectParameter"/>s.
        /// If the specified argument is <c>null</c>, then <c>null</c> is returned.
        /// </summary>
        /// <param name="copyParams">The parameter collection to copy</param>
        /// <returns>The new collection containing copies of <paramref name="copyParams"/> parameters, if <paramref name="copyParams"/> is non-null; otherwise <c>null</c>.</returns>
        internal static ObjectParameterCollection DeepCopy(ObjectParameterCollection copyParams)
        {
            if (null == copyParams)
            {
                return null;
            }

            ObjectParameterCollection retParams = new ObjectParameterCollection(copyParams._perspective);
            foreach (ObjectParameter param in copyParams)
            {
                retParams.Add(param.ShallowCopy());
            }

            return retParams;
        }

        #endregion

        #region Private Methods

        // ---------------
        // Private Methods
        // ---------------

        /// <summary>
        ///   This private method checks for the existence of a given parameter object
        ///   by name by iterating through the list and comparing each parameter name 
        ///   to the specified name. This is a case-insensitive lookup.
        /// </summary>
        private int IndexOf (string name)
        {
            int index = 0;

            foreach (ObjectParameter parameter in this._parameters)
            {
                if (0 == String.Compare(name, parameter.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        /// <summary>
        ///     This method successfully returns only if the parameter collection is not considered 'locked';
        ///     otherwise an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        private void CheckUnlocked()
        {
            if (this._locked)
            {
                throw EntityUtil.InvalidOperation(Strings.ObjectParameterCollection_ParametersLocked);
            }
        }

        #endregion
    }
}
