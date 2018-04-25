//---------------------------------------------------------------------
// <copyright file="EdmType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Data.Common;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Base EdmType class for all the model types
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public abstract class EdmType : GlobalItem
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of EdmType
        /// </summary>
        internal EdmType()
        {
            // No initialization of item attributes in here, it's used as a pass thru in the case for delay population
            // of item attributes
        }

        /// <summary>
        /// Constructs a new instance of EdmType with the given name, namespace and version
        /// </summary>
        /// <param name="name">name of the type</param>
        /// <param name="namespaceName">namespace of the type</param>
        /// <param name="version">version of the type</param>
        /// <param name="dataSpace">dataSpace in which this edmtype belongs to</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either the name, namespace or version arguments are null</exception>
        internal EdmType(string name,
                         string namespaceName,
                         DataSpace dataSpace)
        {
            EntityUtil.GenericCheckArgumentNull(name, "name");
            EntityUtil.GenericCheckArgumentNull(namespaceName, "namespaceName");

            // Initialize the item attributes
            EdmType.Initialize(this,
                               name,
                               namespaceName,
                               dataSpace,
                               false,
                               null);
        }
        #endregion

        #region Fields
        private CollectionType _collectionType;
        private string _identity;
        private string _name;
        private string _namespace;
        private EdmType _baseType;
        #endregion

        #region Properties
        /// <summary>
        /// Direct accessor for the field Identity. The reason we need to do this is that for derived class,
        /// they want to cache things only when they are readonly. Plus they want to check for null before
        /// updating the value
        /// </summary>
        internal string CacheIdentity
        {
            get
            {
                return _identity;
            }
            private set
            {
                _identity = value;
            }
        }

        /// <summary>
        /// Returns the identity of the edm type
        /// </summary>
        internal override string Identity
        {
            get
            {
                if (this.CacheIdentity == null)
                {
                    StringBuilder builder = new StringBuilder(50);
                    BuildIdentity(builder);
                    this.CacheIdentity = builder.ToString();
                }
                return this.CacheIdentity;
            }
        }

        /// <summary>
        /// Returns the name of the EdmType
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public String Name
        {
            get
            {
                return _name;
            }
            internal set
            {
                Debug.Assert(value != null, "The name should never be set to null");
                _name = value; 
            }
        }

        /// <summary>
        /// Returns the namespace of the EdmType
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public String NamespaceName
        {
            get
            {
                return _namespace;
            }
            internal set
            {
                Debug.Assert(value != null, "Namespace should never be set to null");
                _namespace = value;
            }
        }

        /// <summary>
        /// Returns true if the EdmType is abstract
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called on instance that is in ReadOnly state</exception>
        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        public bool Abstract
        {
            get
            {
                return GetFlag(MetadataFlags.IsAbstract);
            }
            internal set
            {
                SetFlag(MetadataFlags.IsAbstract, value);
            }
        }

        /// <summary>
        /// Returns the base type of the EdmType
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called on instance that is in ReadOnly state</exception>
        /// <exception cref="System.ArgumentException">Thrown if the value passed in for setter will create a loop in the inheritance chain</exception>
        [MetadataProperty(BuiltInTypeKind.EdmType, false)]
        public EdmType BaseType
        {
            get
            {
                return _baseType;
            }
            internal set
            {
                Util.ThrowIfReadOnly(this);
                Debug.Assert(_baseType == null, "BaseType can't be set multiple times");

                // Check to make sure there won't be a loop in the inheritance
                EdmType type = value;
                while (type != null)
                {
                    Debug.Assert(type != this, "Cannot set the given type as base type because it would introduce a loop in inheritance");

                    type = type.BaseType;
                }

                // Also if the base type is EntityTypeBase, make sure it doesn't have keys
                Debug.Assert(value == null ||
                             !Helper.IsEntityTypeBase(this) ||
                             ((EntityTypeBase)this).KeyMembers.Count == 0 ||
                             ((EntityTypeBase)value).KeyMembers.Count == 0,
                             " For EntityTypeBase, both base type and derived types cannot have keys defined");

                _baseType = value;
            }
        }

        /// <summary>
        /// Returns the full name of this type, which is namespace + "." + name. 
        /// Since the identity of all EdmTypes, except EdmFunction, is same as of that
        /// of the full name, FullName just returns the identity. This property is 
        /// over-ridden in EdmFunctin, just to return NamespaceName + "." + Name
        /// </summary>
        public virtual string FullName
        {
            get
            {
                return this.Identity;
           }
        }

        /// <summary>
        /// If OSpace, return the CLR Type else null
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called on instance that is in ReadOnly state</exception>
        internal virtual System.Type ClrType
        {
            get { return null; }
        }

        internal override void BuildIdentity(StringBuilder builder)
        {
            // if we already know the identity, simply append it
            if (null != this.CacheIdentity)
            {
                builder.Append(this.CacheIdentity);
                return;
            }

            builder.Append(CreateEdmTypeIdentity(NamespaceName, Name));
        }

        internal static string CreateEdmTypeIdentity(string namespaceName, string name)
        {
            string identity = string.Empty;
            if (string.Empty != namespaceName)
            {
                identity = namespaceName + ".";
            }

            identity += name;

            return identity;
            
        }

        #endregion

        #region Methods
        /// <summary>
        /// Initialize the type. This method must be called since for bootstraping we only call the constructor. 
        /// This method will help us initialize the type
        /// </summary>
        /// <param name="edmType">The edm type to initialize with item attributes</param>
        /// <param name="name">The name of this type</param>
        /// <param name="namespaceName">The namespace of this type</param>
        /// <param name="version">The version of this type</param>
        /// <param name="dataSpace">dataSpace in which this edmtype belongs to</param>
        /// <param name="isAbstract">If the type is abstract</param>
        /// <param name="isSealed">If the type is sealed</param>
        /// <param name="baseType">The base type for this type</param>
        internal static void 
            Initialize(EdmType edmType,
                                        string name,
                                        string namespaceName,
                                        DataSpace dataSpace,
                                        bool isAbstract,
                                        EdmType baseType)
        {
            edmType._baseType = baseType;
            edmType._name = name;
            edmType._namespace = namespaceName;
            edmType.DataSpace = dataSpace;
            edmType.Abstract = isAbstract;
        }

        /// <summary>
        /// Overriding System.Object.ToString to provide better String representation 
        /// for this type.
        /// </summary>
        public override string ToString()
        {
            return this.FullName;
        }

        /// <summary>
        /// Returns the collection type whose element type is this edm type
        /// </summary>
        public CollectionType GetCollectionType()
        {
            if (_collectionType == null)
            {
                Interlocked.CompareExchange<CollectionType>(ref _collectionType, new CollectionType(this), null);
            }

            return _collectionType;
        }

        /// <summary>
        /// check to see if otherType is among the base types,
        /// </summary>
        /// <param name="otherType"></param>
        /// <returns>
        /// if otherType is among the base types, return true, 
        /// otherwise returns false.
        /// when othertype is same as the current type, return false.
        /// </returns>
        internal virtual bool IsSubtypeOf(EdmType otherType)
        {
            return Helper.IsSubtypeOf(this, otherType);
        }

        /// <summary>
        /// check to see if otherType is among the sub-types,
        /// </summary>
        /// <param name="otherType"></param>
        /// <returns>
        /// if otherType is among the sub-types, returns true,
        /// otherwise returns false.
        /// when othertype is same as the current type, return false.
        /// </returns>
        internal virtual bool IsBaseTypeOf(EdmType otherType)
        {
            if (otherType == null)
                return false;
            return otherType.IsSubtypeOf(this);
        }

        /// <summary>
        /// Check if this type is assignable from otherType
        /// </summary>
        /// <param name="otherType"></param>
        /// <returns></returns>
        internal virtual bool IsAssignableFrom(EdmType otherType)
        {
            return Helper.IsAssignableFrom(this, otherType);
        }

        /// <summary>
        /// Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();

                EdmType baseType = BaseType;
                if (baseType != null)
                {
                    baseType.SetReadOnly();
                }
            }
        }

        /// <summary>
        /// Returns all facet descriptions associated with this type.
        /// </summary>
        /// <returns>Descriptions for all built-in facets for this type.</returns>
        internal virtual IEnumerable<FacetDescription> GetAssociatedFacetDescriptions()
        {
            return MetadataItem.GetGeneralFacetDescriptions();
        }
        #endregion
    }
}
