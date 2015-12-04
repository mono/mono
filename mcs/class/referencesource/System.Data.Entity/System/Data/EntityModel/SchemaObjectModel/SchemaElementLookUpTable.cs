//---------------------------------------------------------------------
// <copyright file="SchemaElementLookUpTable.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Summary description for SchemaElementLookUpTable.
    /// </summary>
    internal sealed class SchemaElementLookUpTable<T> : IEnumerable<T>, ISchemaElementLookUpTable<T>
    where T : SchemaElement
    {
        #region Instance Fields
        private Dictionary<string,T> _keyToType = null;
        private List<string> _keysInDefOrder = new List<string>();
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        public SchemaElementLookUpTable()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public int Count
        {
            get
            {
                return KeyToType.Count;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool ContainsKey(string key)
        {
            return KeyToType.ContainsKey(KeyFromName(key));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public T LookUpEquivalentKey(string key)
        {
            key = KeyFromName(key);
            T element;

            if (KeyToType.TryGetValue(key, out element))
            {
                return element;
            }

            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        public T this[string key]
        {
            get
            {
                return KeyToType[KeyFromName(key)];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public T GetElementAt(int index)
        {
                return KeyToType[_keysInDefOrder[index]];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new SchemaElementLookUpTableEnumerator<T,T>(KeyToType,_keysInDefOrder);
        }
        IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new SchemaElementLookUpTableEnumerator<T,T>(KeyToType,_keysInDefOrder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerator<S> GetFilteredEnumerator<S>()
        where S : T
        {
            return new SchemaElementLookUpTableEnumerator<S,T>(KeyToType,_keysInDefOrder);
        }

        /// <summary>
        /// Add the given type to the schema look up table. If there is an error, it
        /// adds the error and returns false. otherwise, it adds the type to the lookuptable
        /// and returns true
        /// </summary>
        public AddErrorKind TryAdd(T type)
        {
            Debug.Assert(type != null, "type parameter is null");

            if (String.IsNullOrEmpty(type.Identity))
            {
                return AddErrorKind.MissingNameError;
            }

            string key = KeyFromElement(type);
            T element;
            if (KeyToType.TryGetValue(key, out element))
            {
                return AddErrorKind.DuplicateNameError;
            }

            KeyToType.Add(key,type);
            _keysInDefOrder.Add(key);

            return AddErrorKind.Succeeded;
        }

        public void Add(T type, bool doNotAddErrorForEmptyName, Func<object, string> duplicateKeyErrorFormat)
        {
            Debug.Assert(type != null, "type parameter is null");
            Debug.Assert(null != duplicateKeyErrorFormat, "duplicateKeyErrorFormat cannot be null");

            AddErrorKind error = TryAdd(type);

            if (error == AddErrorKind.MissingNameError)
            {
                if (!doNotAddErrorForEmptyName)
                {
                    type.AddError(ErrorCode.InvalidName, EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.MissingName);
                }
                return;
            }
            else if (error == AddErrorKind.DuplicateNameError)
            {
                type.AddError(ErrorCode.AlreadyDefined, EdmSchemaErrorSeverity.Error,
                        duplicateKeyErrorFormat(type.FQName));
            }
            else
            {
                Debug.Assert(error == AddErrorKind.Succeeded, "Invalid error encountered");
            }
        }

        #endregion

        #region Internal Methods
        #endregion

        #region Private Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string KeyFromElement(T type)
        {
            return KeyFromName(type.Identity);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unnormalizedKey"></param>
        /// <returns></returns>
        private static string KeyFromName(string unnormalizedKey)
        {
            Debug.Assert(!String.IsNullOrEmpty(unnormalizedKey), "unnormalizedKey parameter is null or empty");

            return unnormalizedKey;
        }
        #endregion

        #region Private Properties
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string,T> KeyToType
        {
            get
            {
                if ( _keyToType == null )
                {
                    _keyToType = new Dictionary<string,T>(StringComparer.Ordinal);
                }
                return _keyToType;
            }
        }
        #endregion
    }

    enum AddErrorKind
    {
        Succeeded,

        MissingNameError,

        DuplicateNameError,
    }
}
