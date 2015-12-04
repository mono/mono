//---------------------------------------------------------------------
// <copyright file="SchemaEnumType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.EntityModel.SchemaObjectModel
{
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml;

    /// <summary>
    /// Represents EnumType element from CSDL. 
    /// </summary>
    internal class SchemaEnumType : SchemaType
    {
        /// <summary>
        /// Indicates whether the enum type is defined as flags (i.e. can be treated as a bit field)
        /// </summary>
        private bool _isFlags;

        /// <summary>
        /// Underlying type of this enum type as read from the schema.
        /// </summary>
        private string _unresolvedUnderlyingTypeName;

        /// <summary>
        /// Resolved underlying type of this enum type. 
        /// </summary>
        private SchemaType _underlyingType; 

        /// <summary>
        /// Members of this EnumType.
        /// </summary>
        private readonly IList<SchemaEnumMember> _enumMembers = new List<SchemaEnumMember>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SchemaEnumType"/> class.
        /// </summary>
        /// <param name="parentElement">
        /// Parent element.
        /// </param>
        public SchemaEnumType(Schema parentElement)
            : base(parentElement)
        {
            if (Schema.DataModel == SchemaDataModelOption.EntityDataModel)
            {
                OtherContent.Add(Schema.SchemaSource);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the enum type is defined as flags (i.e. can be treated as a bit field)
        /// </summary>
        public bool IsFlags
        {
            get { return _isFlags; }
        }

        /// <summary>
        /// Returns underlying type for this enum. 
        /// </summary>
        public SchemaType UnderlyingType
        {
            get
            {
                Debug.Assert(_underlyingType != null, "The type has not been resolved yet");

                return _underlyingType;
            }
        }

        /// <summary>
        /// Gets members for this EnumType.
        /// </summary>
        public IEnumerable<SchemaEnumMember> EnumMembers
        {
            get { return _enumMembers; }
        }

        /// <summary>
        /// Generic handler for the EnumType element child elements.
        /// </summary>
        /// <param name="reader">Xml reader positioned on a child element.</param>
        /// <returns>
        /// <c>true</c> if the child element is a known element and was handled. Otherwise <c>false</c>
        /// </returns>
        protected override bool HandleElement(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");

            if (!base.HandleElement(reader))
            {
                if (CanHandleElement(reader, XmlConstants.Member))
                {
                    HandleMemberElement(reader);
                }
                else if (CanHandleElement(reader, XmlConstants.ValueAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
                else if (CanHandleElement(reader, XmlConstants.TypeAnnotation))
                {
                    // EF does not support this EDM 3.0 element, so ignore it.
                    SkipElement(reader);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Generic handler for the EnumType element attributes
        /// </summary>
        /// <param name="reader">Xml reader positioned on an attribute.</param>
        /// <c>true</c> if the attribute is a known attribute and was handled. Otherwise <c>false</c>
        protected override bool HandleAttribute(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");

            if (!base.HandleAttribute(reader))
            {
                if(CanHandleAttribute(reader, XmlConstants.IsFlags))
                {
                    HandleBoolAttribute(reader, ref _isFlags);
                }
                else if (CanHandleAttribute(reader, XmlConstants.UnderlyingType))
                {
                    Utils.GetDottedName(Schema, reader, out _unresolvedUnderlyingTypeName);
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Handler for the Member element.
        /// </summary>
        /// <param name="reader">XmlReader positioned on the Member element.</param>
        private void HandleMemberElement(XmlReader reader)
        {
            Debug.Assert(reader != null, "reader != null");

            SchemaEnumMember enumMember = new SchemaEnumMember(this);
            enumMember.Parse(reader);

            // if the value has not been specified we need to fix it up.
            if (!enumMember.Value.HasValue)
            {
                if (_enumMembers.Count == 0)
                {
                    enumMember.Value = 0;
                }
                else
                {
                    long previousValue = (long)_enumMembers[_enumMembers.Count - 1].Value;
                    if (previousValue < long.MaxValue)
                    {
                        enumMember.Value = previousValue + 1;
                    }
                    else
                    {
                        AddError(ErrorCode.CalculatedEnumValueOutOfRange,
                            EdmSchemaErrorSeverity.Error,
                            System.Data.Entity.Strings.CalculatedEnumValueOutOfRange);

                        // the error has been reported. Assigning previous + 1 would cause an overflow. Null is not really 
                        // expected later on so just assign the previous value. 
                        enumMember.Value = previousValue;
                    }
                }
            }

            _enumMembers.Add(enumMember);
        }

        /// <summary>
        /// Resolves the underlying type.
        /// </summary>
        internal override void ResolveTopLevelNames()
        {
            // if the underlying type was not specified in the CSDL we use int by default
            if (_unresolvedUnderlyingTypeName == null)
            {
                _underlyingType = Schema.SchemaManager.SchemaTypes
                    .Single(t => t is ScalarType && ((ScalarType)t).TypeKind == PrimitiveTypeKind.Int32);
            }
            else 
            {
                Debug.Assert(_unresolvedUnderlyingTypeName != string.Empty);
                Schema.ResolveTypeName(this, _unresolvedUnderlyingTypeName, out _underlyingType);
            }
        }

        /// <summary>
        /// Validates the specified enumeration type as a whole.
        /// </summary>
        internal override void Validate()
        {
            base.Validate();

            var enumUnderlyingType = UnderlyingType as ScalarType;

            if (enumUnderlyingType == null || !Helper.IsSupportedEnumUnderlyingType(enumUnderlyingType.TypeKind))
            {
                AddError(ErrorCode.InvalidEnumUnderlyingType,
                    EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.InvalidEnumUnderlyingType);
            }
            else
            {
                Debug.Assert(!_enumMembers.Any(m => !m.Value.HasValue), "member values should have been fixed up already.");

                // Check for underflows and overflows
                var invalidEnumMembers = _enumMembers
                    .Where(m => !Helper.IsEnumMemberValueInRange(enumUnderlyingType.TypeKind, (long)m.Value));

                foreach (var invalidEnumMember in invalidEnumMembers)
                {
                    invalidEnumMember.AddError(ErrorCode.EnumMemberValueOutOfItsUnderylingTypeRange,
                        EdmSchemaErrorSeverity.Error,
                        System.Data.Entity.Strings.EnumMemberValueOutOfItsUnderylingTypeRange(invalidEnumMember.Value, invalidEnumMember.Name, UnderlyingType.Name));
                }
            }

            // Check for duplicate enumeration members.
            if (_enumMembers.GroupBy(o => o.Name).Where(g => g.Count() > 1).Any())
            {
                AddError(ErrorCode.DuplicateEnumMember,
                    EdmSchemaErrorSeverity.Error,
                    System.Data.Entity.Strings.DuplicateEnumMember);
            }
        }
    }
}
