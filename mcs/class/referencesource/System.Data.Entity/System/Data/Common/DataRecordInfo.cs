//------------------------------------------------------------------------------
// <copyright file="DataRecordInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common
{
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// DataRecordInfo class providing a simple way to access both the type information and the column information.
    /// </summary>
    public class DataRecordInfo
    {

        private readonly System.Collections.ObjectModel.ReadOnlyCollection<FieldMetadata> _fieldMetadata;
        private readonly TypeUsage _metadata;

        /// <summary>
        /// Construct DataRecordInfo with list of EdmMembers.
        /// Each memberInfo must be a member of metadata.
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="memberInfo"></param>
        public DataRecordInfo(TypeUsage metadata, IEnumerable<EdmMember> memberInfo)
        {
            EntityUtil.CheckArgumentNull(metadata, "metadata");
            IBaseList<EdmMember> members = TypeHelpers.GetAllStructuralMembers(metadata.EdmType);

            List<FieldMetadata> fieldList = new List<FieldMetadata>(members.Count);

            if (null != memberInfo)
            {
                foreach (EdmMember member in memberInfo)
                {
                    if ((null != member) &&
                        (0 <= members.IndexOf(member)) &&
                        ((BuiltInTypeKind.EdmProperty == member.BuiltInTypeKind) ||         // for ComplexType, EntityType; BuiltTypeKind.NaviationProperty not allowed
                         (BuiltInTypeKind.AssociationEndMember == member.BuiltInTypeKind))) // for AssociationType
                    {   // each memberInfo must be non-null and be part of Properties or AssociationEndMembers
                        //validate that EdmMembers are from the same type or base type of the passed in metadata.
                        if((member.DeclaringType != metadata.EdmType) && 
                            !member.DeclaringType.IsBaseTypeOf(metadata.EdmType))
                        {
                            throw EntityUtil.Argument(System.Data.Entity.Strings.EdmMembersDefiningTypeDoNotAgreeWithMetadataType);
                        }
                        fieldList.Add(new FieldMetadata(fieldList.Count, member));
                    }
                    else
                    {   // expecting empty memberInfo for non-structural && non-null member part of members if structural
                        throw EntityUtil.Argument("memberInfo");
                    }
                }
            }

            // expecting structural types to have something at least 1 property
            // (((null == structural) && (0 == fieldList.Count)) || ((null != structural) && (0 < fieldList.Count)))
            if (Helper.IsStructuralType(metadata.EdmType) == (0 < fieldList.Count))
            {
                _fieldMetadata = new System.Collections.ObjectModel.ReadOnlyCollection<FieldMetadata>(fieldList);
                _metadata = metadata;
            }
            else
            {
                throw EntityUtil.Argument("memberInfo");
            }
        }

        /// <summary>
        /// Construct FieldMetadata for structuralType.Members from TypeUsage
        /// </summary>
        internal DataRecordInfo(TypeUsage metadata)
        {
            Debug.Assert(null != metadata, "invalid attempt to instantiate DataRecordInfo with null metadata information");

            IBaseList<EdmMember> structuralMembers = TypeHelpers.GetAllStructuralMembers(metadata);
            FieldMetadata[] fieldList = new FieldMetadata[structuralMembers.Count];
            for (int i = 0; i < fieldList.Length; ++i)
            {
                EdmMember member = structuralMembers[i];
                Debug.Assert((BuiltInTypeKind.EdmProperty == member.BuiltInTypeKind) ||
                             (BuiltInTypeKind.AssociationEndMember == member.BuiltInTypeKind),
                             "unexpected BuiltInTypeKind for member");
                fieldList[i] = new FieldMetadata(i, member);
            }
            _fieldMetadata = new System.Collections.ObjectModel.ReadOnlyCollection<FieldMetadata>(fieldList);
            _metadata = metadata;

        }


        /// <summary>
        /// Reusing TypeUsage and FieldMetadata from another EntityRecordInfo which has all the same info
        /// but with a different EntityKey instance.
        /// </summary>
        internal DataRecordInfo(DataRecordInfo recordInfo)
        {
            _fieldMetadata = recordInfo._fieldMetadata;
            _metadata = recordInfo._metadata;
        }

        /// <summary>
        /// Column information.
        /// </summary>
        public System.Collections.ObjectModel.ReadOnlyCollection<FieldMetadata> FieldMetadata
        {
            get
            {
                return _fieldMetadata;
            }
        }

        /// <summary>
        /// Type information.
        /// </summary>
        public TypeUsage RecordType
        {
            get
            {
                return _metadata;
            }
        }
    }
}
