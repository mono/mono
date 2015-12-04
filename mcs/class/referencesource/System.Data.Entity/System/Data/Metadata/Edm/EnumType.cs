//---------------------------------------------------------------------
// <copyright file="EnumType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Metadata.Edm
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Represents an enumeration type.
    /// </summary>
    public class EnumType : SimpleType
    {
        #region Fields

        /// <summary>
        /// A collection of enumeration members for this enumeration type
        /// </summary>
        private readonly ReadOnlyMetadataCollection<EnumMember> _members = 
            new ReadOnlyMetadataCollection<EnumMember>(new MetadataCollection<EnumMember>());

        /// <summary>
        /// Indicates whether the enum type is defined as flags (i.e. can be treated as a bit field)
        /// </summary>
        private readonly bool _isFlags;

        /// <summary>
        /// Underlying type of this enumeration type.
        /// </summary>
        private readonly PrimitiveType _underlyingType;

        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the EnumType class. This default constructor is used for bootstraping
        /// </summary>
        internal EnumType()
        {
            _underlyingType = PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.Int32);
            _isFlags = false;
        }

        /// <summary>
        /// Initializes a new instance of the EnumType class by using the specified <paramref name="name"/>,
        /// <paramref name="namespaceName"/> and <paramref name="isFlags"/>.
        /// </summary>
        /// <param name="name">The name of this enum type.</param>
        /// <param name="namespaceName">The namespace this enum type belongs to.</param>
        /// <param name="isFlags">Indicates whether the enum type is defined as flags (i.e. can be treated as a bit field).</param>
        /// <param name="underlyingType">Underlying type of this enumeration type.</param>
        /// <param name="dataSpace">DataSpace this enum type lives in. Can be either CSpace or OSpace</param>
        /// <exception cref="System.ArgumentNullException">Thrown if name or namespace arguments are null</exception>
        /// <remarks>Note that enums live only in CSpace.</remarks>
        internal EnumType(string name, string namespaceName, PrimitiveType underlyingType, bool isFlags, DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        { 
            Debug.Assert(underlyingType != null, "underlyingType != null");
            Debug.Assert(Helper.IsSupportedEnumUnderlyingType(underlyingType.PrimitiveTypeKind), "Unsupported underlying type for enum.");
            Debug.Assert(dataSpace == DataSpace.CSpace || dataSpace == DataSpace.OSpace, "Enums can be only defined in CSpace or OSpace.");

            _isFlags = isFlags;
            _underlyingType = underlyingType;
        }

        /// <summary>
        /// Initializes a new instance of the EnumType class from CLR enumeration type.
        /// </summary>
        /// <param name="clrType">
        /// CLR enumeration type to create EnumType from.
        /// </param>
        /// <remarks>
        /// Note that this method expects that the <paramref name="clrType"/> is a valid CLR enum type
        /// whose underlying type is a valid EDM primitive type.
        /// Ideally this constructor should be protected and internal (Family and Assembly modifier) but
        /// C# does not support this. In order to not expose this constructor to everyone internal is the
        /// only option.
        /// </remarks>
        internal EnumType(Type clrType) :
            base(clrType.Name, clrType.Namespace ?? string.Empty, DataSpace.OSpace)
        {
            Debug.Assert(clrType != null, "clrType != null");
            Debug.Assert(clrType.IsEnum, "enum type expected");

            ClrProviderManifest.Instance.TryGetPrimitiveType(clrType.GetEnumUnderlyingType(), out _underlyingType);

            Debug.Assert(_underlyingType != null, "only primitive types expected here.");
            Debug.Assert(Helper.IsSupportedEnumUnderlyingType(_underlyingType.PrimitiveTypeKind), 
                "unsupported CLR types should have been filtered out by .TryGetPrimitiveType() method.");

            _isFlags = clrType.GetCustomAttributes(typeof(FlagsAttribute), false).Any();

            foreach (string name in Enum.GetNames(clrType))
            {
                this.AddMember(
                    new EnumMember(
                        name,
                        Convert.ChangeType(Enum.Parse(clrType, name), clrType.GetEnumUnderlyingType(), CultureInfo.InvariantCulture)));
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.EnumType; } }

        /// <summary>
        /// Gets a collection of enumeration members for this enumeration type.
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EnumMember, true)]
        public ReadOnlyMetadataCollection<EnumMember> Members
        {
            get { return _members; }
        }

        /// <summary>
        /// Gets a value indicating whether the enum type is defined as flags (i.e. can be treated as a bit field)
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        public bool IsFlags
        {
            get { return _isFlags; }
        }

        /// <summary>
        /// Gets the underlying type for this enumeration type.
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.PrimitiveType, false)]
        public PrimitiveType UnderlyingType
        {
            get { return _underlyingType; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                this.Members.Source.SetReadOnly();
            }
        }

        /// <summary>
        /// Adds the specified member to the member collection
        /// </summary>
        /// <param name="enumMember">Enumeration member to add to the member collection.</param>
        internal void AddMember(EnumMember enumMember)
        {
            Debug.Assert(enumMember != null, "enumMember != null");
            Debug.Assert(Helper.IsEnumMemberValueInRange(UnderlyingType.PrimitiveTypeKind, Convert.ToInt64(enumMember.Value, CultureInfo.InvariantCulture)));
            Debug.Assert(enumMember.Value.GetType() == UnderlyingType.ClrEquivalentType);

            this.Members.Source.Add(enumMember);
        }

        #endregion
    }

    /// <summary>
    /// Represents an enumeration type that has a reference to the backing CLR type.
    /// </summary>
    internal sealed class ClrEnumType : EnumType
    {
        /// <summary>cached CLR type handle, allowing the Type reference to be GC'd</summary>
        private readonly System.RuntimeTypeHandle _type;

        private readonly string _cspaceTypeName;

        /// <summary>
        /// Initializes a new instance of ClrEnumType class with properties from the CLR type.
        /// </summary>
        /// <param name="clrType">The CLR type to construct from.</param>
        /// <param name="cspaceNamespaceName">CSpace namespace name.</param>
        /// <param name="cspaceTypeName">CSpace type name.</param>
        internal ClrEnumType(Type clrType, string cspaceNamespaceName, string cspaceTypeName)
            : base(clrType)  
        {
            Debug.Assert(clrType != null, "clrType != null");
            Debug.Assert(clrType.IsEnum, "enum type expected");
            Debug.Assert(!String.IsNullOrEmpty(cspaceNamespaceName) && !String.IsNullOrEmpty(cspaceTypeName), "Mapping information must never be null");

            _type = clrType.TypeHandle;
            _cspaceTypeName = cspaceNamespaceName + "." + cspaceTypeName;
        }

        /// <summary>
        /// Gets the clr type backing this enum type.
        /// </summary>
        internal override System.Type ClrType
        {
            get { return Type.GetTypeFromHandle(_type); }
        }

        /// <summary>
        /// Get the full CSpaceTypeName for this enum type.
        /// </summary>
        internal string CSpaceTypeName 
        { 
            get 
            { 
                return _cspaceTypeName; 
            } 
        }
    }
}
