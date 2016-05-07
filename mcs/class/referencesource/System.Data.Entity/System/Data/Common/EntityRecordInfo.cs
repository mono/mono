//------------------------------------------------------------------------------
// <copyright file="EntityRecordInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System.Collections.Generic;
    using System.Data;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// EntityRecordInfo class providing a simple way to access both the type information and the column information.
    /// </summary>
    public class EntityRecordInfo : DataRecordInfo {

        private readonly EntityKey _entityKey;
        private readonly EntitySet _entitySet;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="memberInfo"></param>
        /// <param name="entityKey"></param>
        public EntityRecordInfo(EntityType metadata, IEnumerable<EdmMember> memberInfo, EntityKey entityKey, EntitySet entitySet)
            : base(TypeUsage.Create(metadata), memberInfo) {
            EntityUtil.CheckArgumentNull<EntityKey>(entityKey, "entityKey");
            EntityUtil.CheckArgumentNull(entitySet, "entitySet");

            _entityKey = entityKey;
            _entitySet = entitySet;
            ValidateEntityType(entitySet);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="metadata"></param>
        /// <param name="entityKey"></param>
        internal EntityRecordInfo(EntityType metadata, EntityKey entityKey, EntitySet entitySet)
            : base(TypeUsage.Create(metadata)) {
            EntityUtil.CheckArgumentNull<EntityKey>(entityKey, "entityKey");

            _entityKey = entityKey;
            _entitySet = entitySet;
#if DEBUG
            try
            {
                ValidateEntityType(entitySet);
            }
            catch
            {
                Debug.Assert(false, "should always be valid EntityType when internally constructed");
                throw;
            }
#endif
        }
        
        /// <summary>
        /// Reusing TypeUsage and FieldMetadata from another EntityRecordInfo which has all the same info
        /// but with a different EntityKey instance.
        /// </summary>
        internal EntityRecordInfo(DataRecordInfo info, EntityKey entityKey, EntitySet entitySet)
            : base(info)
        {
            _entityKey = entityKey;
            _entitySet = entitySet;
#if DEBUG
            try
            {
                ValidateEntityType(entitySet);
            }
            catch
            {
                Debug.Assert(false, "should always be valid EntityType when internally constructed");
                throw;
            }
#endif
        }

        /// <summary>
        /// the EntityKey
        /// </summary>
        public EntityKey EntityKey {
            get {
                return _entityKey;
            }
        }

        // using EntitySetBase versus EntitySet prevents the unnecessary cast of ElementType to EntityType
        private void ValidateEntityType(EntitySetBase entitySet)
        {
            if (!object.ReferenceEquals(RecordType.EdmType, null) &&
                !object.ReferenceEquals(_entityKey, EntityKey.EntityNotValidKey) &&
                !object.ReferenceEquals(_entityKey, EntityKey.NoEntitySetKey) &&
                !object.ReferenceEquals(RecordType.EdmType, entitySet.ElementType) &&
                !entitySet.ElementType.IsBaseTypeOf(RecordType.EdmType))
            {
                throw EntityUtil.Argument(System.Data.Entity.Strings.EntityTypesDoNotAgree);
            }
        }
    }
}
