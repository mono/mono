//------------------------------------------------------------------------------
// <copyright file="ModelMemberCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Activities.Presentation.Model {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation;

    /// <summary>
    /// ModelMemberCollection is an abstract base class that 
    /// ModelPropertyCollection and ModelEventCollection derive from.
    /// </summary>
    /// <typeparam name="TItemType">The type of item the collection represents.</typeparam>
    /// <typeparam name="TFindType">The type that should be used as a key in "Find" methods.</typeparam>
    public abstract class ModelMemberCollection<TItemType, TFindType> : IEnumerable<TItemType>, IEnumerable {

        /// <summary>
        /// Internal constructor.  Only our own collections can derive from this class.
        /// </summary>
        internal ModelMemberCollection() { }

        /// <summary>
        /// Searches the collection for the given key and returns it 
        /// if it is found.  If not found, this throws an exception.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if name is null.</exception>
        /// <exception cref="ArgumentException">if name is not found.</exception>
        public TItemType this[string name] {
            get {
                if (name == null) throw FxTrace.Exception.ArgumentNull("name");
                return Find(name, true);
            }
        }

        /// <summary>
        /// Searches the collection for the given key and returns it 
        /// if it is found.  If not found, this throws an exception.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if value is null.</exception>
        /// <exception cref="ArgumentException">if value is not found.</exception>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public TItemType this[TFindType value] {
            get {
                if (value == null) throw FxTrace.Exception.ArgumentNull("value");
                return Find(value, true);
            }
        }

        /// <summary>
        /// Searches the collection for the given key and returns it if it is 
        /// found.  If not found, this returns null.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if name is null.</exception>
        public TItemType Find(string name) {
            if (name == null) throw FxTrace.Exception.ArgumentNull("name");
            return Find(name, false);
        }

        /// <summary>
        /// Searches the collection for the given key and returns it if it is 
        /// found.  If not found, this throws an exception or returns null, 
        /// depending on the value passed to throwOnError.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">if name is not found and throwOnError is true.</exception>
        protected abstract TItemType Find(string name, bool throwOnError);

        /// <summary>
        /// Searches the collection for the given key and returns it if it is 
        /// found.  If not found, this returns null.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">if value is null.</exception>
        public TItemType Find(TFindType value) {
            if (value == null) throw FxTrace.Exception.ArgumentNull("value");
            return Find(value, false);
        }

        /// <summary>
        /// Searches the collection for the given key and returns it if it is 
        /// found.  If not found, this throws an exception or returns null, 
        /// depending on the value passed to throwOnError.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">if value is not found and throwOnError is true.</exception>
        protected abstract TItemType Find(TFindType value, bool throwOnError);

        /// <summary>
        /// Returns an enumerator to enumerate values.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerator<TItemType> GetEnumerator();

        #region IEnumerable Members

        /// <summary>
        /// IEnumerable Implementation.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion
    }
}
