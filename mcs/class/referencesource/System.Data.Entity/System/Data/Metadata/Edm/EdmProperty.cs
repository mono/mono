//---------------------------------------------------------------------
// <copyright file="EdmProperty.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common;
using System.Threading;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represent the edm property class
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Edm")]
    public sealed class EdmProperty : EdmMember
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the property class
        /// </summary>
        /// <param name="name">name of the property</param>
        /// <param name="typeUsage">TypeUsage object containing the property type and its facets</param>
        /// <exception cref="System.ArgumentNullException">Thrown if name or typeUsage arguments are null</exception>
        /// <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal EdmProperty(string name, TypeUsage typeUsage)
            : base(name, typeUsage)
        {
            EntityUtil.CheckStringArgument(name, "name");
            EntityUtil.GenericCheckArgumentNull(typeUsage, "typeUsage");
        }
        #endregion

        #region Fields
        /// <summary>Store the handle, allowing the PropertyInfo/MethodInfo/Type references to be GC'd</summary>
        internal readonly System.RuntimeMethodHandle PropertyGetterHandle;

        /// <summary>Store the handle, allowing the PropertyInfo/MethodInfo/Type references to be GC'd</summary>
        internal readonly System.RuntimeMethodHandle PropertySetterHandle;

        /// <summary>Store the handle, allowing the PropertyInfo/MethodInfo/Type references to be GC'd</summary>
        internal readonly System.RuntimeTypeHandle EntityDeclaringType;

        /// <summary>cached dynamic method to get the property value from a CLR instance</summary> 
        private Func<object,object> _memberGetter;

        /// <summary>cached dynamic method to set a CLR property value on a CLR instance</summary> 
        private Action<object,object> _memberSetter;
        #endregion

        /// <summary>
        /// Initializes a new OSpace instance of the property class
        /// </summary>
        /// <param name="name">name of the property</param>
        /// <param name="typeUsage">TypeUsage object containing the property type and its facets</param>
        /// <param name="propertyInfo">for the property</param>
        /// <param name="entityDeclaringType">The declaring type of the entity containing the property</param>
        internal EdmProperty(string name, TypeUsage typeUsage, System.Reflection.PropertyInfo propertyInfo, RuntimeTypeHandle entityDeclaringType)
            : this(name, typeUsage)
        {
            System.Diagnostics.Debug.Assert(name == propertyInfo.Name, "different PropertyName");
            if (null != propertyInfo)
            {
                System.Reflection.MethodInfo method;

                method = propertyInfo.GetGetMethod(true); // return public or non-public getter
                PropertyGetterHandle = ((null != method) ? method.MethodHandle : default(System.RuntimeMethodHandle));

                method = propertyInfo.GetSetMethod(true); // return public or non-public getter
                PropertySetterHandle = ((null != method) ? method.MethodHandle : default(System.RuntimeMethodHandle));

                EntityDeclaringType = entityDeclaringType;
            }
        }

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.EdmProperty; } }

        /// <summary>
        /// Returns true if this property is nullable.
        /// </summary>
        /// <remarks>
        /// Nullability in the conceptual model and store model is a simple indication of whether or not
        /// the property is considered nullable. Nullability in the object model is more complex.
        /// When using convention based mapping (as usually happens with POCO entities), a property in the
        /// object model is considered nullable if and only if the underlying CLR type is nullable and
        /// the property is not part of the primary key.
        /// When using attribute based mapping (usually used with entities that derive from the EntityObject
        /// base class), a property is considered nullable if the IsNullable flag is set to true in the
        /// <see cref="System.Data.Objects.DataClasses.EdmScalarPropertyAttribute"/> attribute. This flag can
        /// be set to true even if the underlying type is not nullable, and can be set to false even if the
        /// underlying type is nullable. The latter case happens as part of default code generation when
        /// a non-nullable property in the conceptual model is mapped to a nullable CLR type such as a string.
        /// In such a case, the Entity Framework treats the property as non-nullable even though the CLR would
        /// allow null to be set.
        /// There is no good reason to set a non-nullable CLR type as nullable in the object model and this
        /// should not be done even though the attribute allows it.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when the EdmProperty instance is in ReadOnly state</exception>
        public bool Nullable
        {
            get
            {
                return (bool)TypeUsage.Facets[DbProviderManifest.NullableFacetName].Value;
            }
        }

        /// <summary>
        /// Returns the default value for this property
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Thrown if the setter is called when the EdmProperty instance is in ReadOnly state</exception>
        public Object DefaultValue
        {
            get
            {
                return TypeUsage.Facets[DbProviderManifest.DefaultValueFacetName].Value;
            }
        }

        /// <summary>cached dynamic method to get the property value from a CLR instance</summary> 
        internal Func<object,object> ValueGetter {
            get { return _memberGetter; }
            set
            {
                System.Diagnostics.Debug.Assert(null != value, "clearing ValueGetter");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberGetter, value, null);
            }
        }

        /// <summary>cached dynamic method to set a CLR property value on a CLR instance</summary> 
        internal Action<object,object> ValueSetter
        {
            get { return _memberSetter; }
            set
            {
                System.Diagnostics.Debug.Assert(null != value, "clearing ValueSetter");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _memberSetter, value, null);
            }
        }
    }
}
