//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security;

    class DataMember
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        CriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal DataMember()
        {
            helper = new CriticalHelper();
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal DataMember(MemberInfo memberInfo)
        {
            helper = new CriticalHelper(memberInfo);
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal DataMember(string name)
        {
            helper = new CriticalHelper(name);
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal DataMember(DataContract memberTypeContract, string name, bool isNullable, bool isRequired, bool emitDefaultValue, int order)
        {
            helper = new CriticalHelper(memberTypeContract, name, isNullable, isRequired, emitDefaultValue, order);
        }

        internal MemberInfo MemberInfo
        {
            [SecuritySafeCritical]
            get { return helper.MemberInfo; }
        }

        internal string Name
        {
            [SecuritySafeCritical]
            get { return helper.Name; }
            [SecurityCritical]
            set { helper.Name = value; }
        }

        internal int Order
        {
            [SecuritySafeCritical]
            get { return helper.Order; }
            [SecurityCritical]
            set { helper.Order = value; }
        }

        internal bool IsRequired
        {
            [SecuritySafeCritical]
            get { return helper.IsRequired; }
            [SecurityCritical]
            set { helper.IsRequired = value; }
        }

        internal bool EmitDefaultValue
        {
            [SecuritySafeCritical]
            get { return helper.EmitDefaultValue; }
            [SecurityCritical]
            set { helper.EmitDefaultValue = value; }
        }

        internal bool IsNullable
        {
            [SecuritySafeCritical]
            get { return helper.IsNullable; }
            [SecurityCritical]
            set { helper.IsNullable = value; }
        }

        internal bool IsGetOnlyCollection
        {
            [SecuritySafeCritical]
            get { return helper.IsGetOnlyCollection; }
            [SecurityCritical]
            set { helper.IsGetOnlyCollection = value; }
        }

        internal Type MemberType
        {
            [SecuritySafeCritical]
            get { return helper.MemberType; }
        }

        internal DataContract MemberTypeContract
        {
            [SecuritySafeCritical]
            get { return helper.MemberTypeContract; }
            [SecurityCritical]
            set { helper.MemberTypeContract = value; }
        }

        internal bool HasConflictingNameAndType
        {
            [SecuritySafeCritical]
            get { return helper.HasConflictingNameAndType; }
            [SecurityCritical]
            set { helper.HasConflictingNameAndType = value; }
        }

        internal DataMember ConflictingMember
        {
            [SecuritySafeCritical]
            get { return helper.ConflictingMember; }
            [SecurityCritical]
            set { helper.ConflictingMember = value; }
        }

        [Fx.Tag.SecurityNote(Critical = "Critical.")]
        [SecurityCritical(SecurityCriticalScope.Everything)]
        class CriticalHelper
        {
            DataContract memberTypeContract;
            string name;
            int order;
            bool isRequired;
            bool emitDefaultValue;
            bool isNullable;
            bool isGetOnlyCollection = false;
            MemberInfo memberInfo;
            bool hasConflictingNameAndType;
            DataMember conflictingMember;

            internal CriticalHelper()
            {
                this.emitDefaultValue = Globals.DefaultEmitDefaultValue;
            }

            internal CriticalHelper(MemberInfo memberInfo)
            {
                this.emitDefaultValue = Globals.DefaultEmitDefaultValue;
                this.memberInfo = memberInfo;
            }

            internal CriticalHelper(string name)
            {
                this.Name = name;
            }

            internal CriticalHelper(DataContract memberTypeContract, string name, bool isNullable, bool isRequired, bool emitDefaultValue, int order)
            {
                this.MemberTypeContract = memberTypeContract;
                this.Name = name;
                this.IsNullable = isNullable;
                this.IsRequired = isRequired;
                this.EmitDefaultValue = emitDefaultValue;
                this.Order = order;
            }

            internal MemberInfo MemberInfo
            {
                get { return memberInfo; }
            }

            internal string Name
            {
                get { return name; }
                set { name = value; }
            }

            internal int Order
            {
                get { return order; }
                set { order = value; }
            }

            internal bool IsRequired
            {
                get { return isRequired; }
                set { isRequired = value; }
            }

            internal bool EmitDefaultValue
            {
                get { return emitDefaultValue; }
                set { emitDefaultValue = value; }
            }

            internal bool IsNullable
            {
                get { return isNullable; }
                set { isNullable = value; }
            }

            internal bool IsGetOnlyCollection
            {
                get { return isGetOnlyCollection; }
                set { isGetOnlyCollection = value; }
            }

            internal Type MemberType
            {
                get
                {
                    FieldInfo field = MemberInfo as FieldInfo;
                    if (field != null)
                        return field.FieldType;
                    return ((PropertyInfo)MemberInfo).PropertyType;
                }
            }

            internal DataContract MemberTypeContract
            {
                get
                {
                    if (memberTypeContract == null)
                    {
                        if (MemberInfo != null)
                        {
                            if (this.IsGetOnlyCollection)
                            {
                                memberTypeContract = DataContract.GetGetOnlyCollectionDataContract(DataContract.GetId(MemberType.TypeHandle), MemberType.TypeHandle, MemberType, SerializationMode.SharedContract);
                            }
                            else
                            {
                                memberTypeContract = DataContract.GetDataContract(MemberType);
                            }
                        }
                    }
                    return memberTypeContract;
                }
                set
                {
                    memberTypeContract = value;
                }
            }

            internal bool HasConflictingNameAndType
            {
                get { return this.hasConflictingNameAndType; }
                set { this.hasConflictingNameAndType = value; }
            }

            internal DataMember ConflictingMember
            {
                get { return this.conflictingMember; }
                set { this.conflictingMember = value; }
            }
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks member visibility to calculate if access to it requires MemberAccessPermission for serialization."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        internal bool RequiresMemberAccessForGet()
        {
            MemberInfo memberInfo = MemberInfo;
            FieldInfo field = memberInfo as FieldInfo;
            if (field != null)
            {
                return DataContract.FieldRequiresMemberAccess(field);
            }
            else
            {
                PropertyInfo property = (PropertyInfo)memberInfo;
                MethodInfo getMethod = property.GetGetMethod(true /*nonPublic*/);
                if (getMethod != null)
                    return DataContract.MethodRequiresMemberAccess(getMethod) || !DataContract.IsTypeVisible(property.PropertyType);
            }
            return false;
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - checks member visibility to calculate if access to it requires MemberAccessPermission for deserialization."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        internal bool RequiresMemberAccessForSet()
        {
            MemberInfo memberInfo = MemberInfo;
            FieldInfo field = memberInfo as FieldInfo;
            if (field != null)
            {
                return DataContract.FieldRequiresMemberAccess(field);
            }
            else
            {
                PropertyInfo property = (PropertyInfo)memberInfo;
                MethodInfo setMethod = property.GetSetMethod(true /*nonPublic*/);
                if (setMethod != null)
                    return DataContract.MethodRequiresMemberAccess(setMethod) || !DataContract.IsTypeVisible(property.PropertyType);
            }
            return false;
        }

        internal DataMember BindGenericParameters(DataContract[] paramContracts, Dictionary<DataContract, DataContract> boundContracts)
        {
            DataContract memberTypeContract = this.MemberTypeContract.BindGenericParameters(paramContracts, boundContracts);
            DataMember boundDataMember = new DataMember(memberTypeContract,
                this.Name,
                !memberTypeContract.IsValueType,
                this.IsRequired,
                this.EmitDefaultValue,
                this.Order);
            return boundDataMember;
        }

        internal bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if ((object)this == other)
                return true;

            DataMember dataMember = other as DataMember;
            if (dataMember != null)
            {
                // Note: comparison does not use Order hint since it influences element order but does not specify exact order
                bool thisIsNullable = (MemberTypeContract == null) ? false : !MemberTypeContract.IsValueType;
                bool dataMemberIsNullable = (dataMember.MemberTypeContract == null) ? false : !dataMember.MemberTypeContract.IsValueType;
                return (Name == dataMember.Name
                        && (IsNullable || thisIsNullable) == (dataMember.IsNullable || dataMemberIsNullable)
                        && IsRequired == dataMember.IsRequired
                        && EmitDefaultValue == dataMember.EmitDefaultValue
                        && MemberTypeContract.Equals(dataMember.MemberTypeContract, checkedContracts));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
