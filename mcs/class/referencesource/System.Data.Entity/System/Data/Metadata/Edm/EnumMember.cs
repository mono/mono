//---------------------------------------------------------------------
// <copyright file="EnumMember.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents an enumeration member.
    /// </summary>
    public sealed class EnumMember : MetadataItem
    {
        #region Fields

        /// <summary>
        /// The name of this enumeration member.
        /// </summary>
        private readonly string _name;

        /// <summary>
        /// The value of this enumeration member.
        /// </summary>
        private readonly object _value;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumMember"/> type by using the specified name and value.
        /// </summary>
        /// <param name="name">The name of this enumeration member. Must not be null or the empty string.</param>
        /// <param name="value">The value of this enumeration member. </param>
        /// <exception cref="System.ArgumentNullException">Thrown if name argument is null</exception>
        /// <exception cref="System.ArgumentException">Thrown if name argument is empty string</exception>
        internal EnumMember(string name, object value)
            : base(MetadataFlags.Readonly)
        {
            EntityUtil.CheckStringArgument(name, "name");
            Debug.Assert(value != null, "value != null");
            Debug.Assert(value is SByte || value is Byte || value is Int16 || value is Int32 || value is Int64, "Unsupported type of enum member value.");

            _name = name;
            _value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the kind of this type.
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind 
        { 
            get { return BuiltInTypeKind.EnumMember; } 
        }

        /// <summary>
        /// Gets the name of this enumeration member.
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the value of this enumeration member.
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.PrimitiveType, false)]
        public object Value
        {
            get
            {
                return _value;
            }
        }

        /// <summary>
        /// Gets the identity for this item as a string
        /// </summary>
        internal override string Identity
        {
            get
            {
                return Name;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Overriding System.Object.ToString to provide better String representation for this type.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        #endregion
    }
}
