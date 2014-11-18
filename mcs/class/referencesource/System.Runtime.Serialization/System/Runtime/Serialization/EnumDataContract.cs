//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Threading;
    using System.Text;
    using System.Xml;
    using System.Security;

#if USE_REFEMIT
    public sealed class EnumDataContract : DataContract
#else
    internal sealed class EnumDataContract : DataContract
#endif
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        EnumDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal EnumDataContract()
            : base(new EnumDataContractCriticalHelper())
        {
            helper = base.Helper as EnumDataContractCriticalHelper;
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal EnumDataContract(Type type)
            : base(new EnumDataContractCriticalHelper(type))
        {
            helper = base.Helper as EnumDataContractCriticalHelper;
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up base contract name for a type.",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        static internal XmlQualifiedName GetBaseContractName(Type type)
        {
            return EnumDataContractCriticalHelper.GetBaseContractName(type);
        }

        [Fx.Tag.SecurityNote(Critical = "Accesses SecurityCritical static cache to look up a base contract name.",
            Safe = "Read only access.")]
        [SecuritySafeCritical]
        static internal Type GetBaseType(XmlQualifiedName baseContractName)
        {
            return EnumDataContractCriticalHelper.GetBaseType(baseContractName);
        }

        internal XmlQualifiedName BaseContractName
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical BaseContractName property.",
                Safe = "BaseContractName only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.BaseContractName; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical BaseContractName property.")]
            [SecurityCritical]
            set { helper.BaseContractName = value; }
        }

        internal List<DataMember> Members
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical Members property.",
                Safe = "Members only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.Members; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical Members property.")]
            [SecurityCritical]
            set { helper.Members = value; }
        }

        internal List<long> Values
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical Values property.",
                Safe = "Values only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.Values; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical Values property.")]
            [SecurityCritical]
            set { helper.Values = value; }
        }

        internal bool IsFlags
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical IsFlags property.",
                Safe = "IsFlags only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsFlags; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical IsFlags property.")]
            [SecurityCritical]
            set { helper.IsFlags = value; }
        }

        internal bool IsULong
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical IsULong property.",
                Safe = "IsULong only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsULong; }
        }

        XmlDictionaryString[] ChildElementNames
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical ChildElementNames property.",
                Safe = "ChildElementNames only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.ChildElementNames; }
        }

        internal override bool CanContainReferences
        {
            get { return false; }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for (de)serializing enums."
            + " Since the data is cached statically, we lock down access to it.")]
        [SecurityCritical(SecurityCriticalScope.Everything)]
        class EnumDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            static Dictionary<Type, XmlQualifiedName> typeToName;
            static Dictionary<XmlQualifiedName, Type> nameToType;

            XmlQualifiedName baseContractName;
            List<DataMember> members;
            List<long> values;
            bool isULong;
            bool isFlags;
            bool hasDataContract;
            XmlDictionaryString[] childElementNames;

            static EnumDataContractCriticalHelper()
            {
                typeToName = new Dictionary<Type, XmlQualifiedName>();
                nameToType = new Dictionary<XmlQualifiedName, Type>();
                Add(typeof(sbyte), "byte");
                Add(typeof(byte), "unsignedByte");
                Add(typeof(short), "short");
                Add(typeof(ushort), "unsignedShort");
                Add(typeof(int), "int");
                Add(typeof(uint), "unsignedInt");
                Add(typeof(long), "long");
                Add(typeof(ulong), "unsignedLong");
            }

            [SuppressMessage(FxCop.Category.Usage, "CA2301:EmbeddableTypesInContainersRule", MessageId = "typeToName", Justification = "No need to support type equivalence here.")]
            static internal void Add(Type type, string localName)
            {
                XmlQualifiedName stableName = CreateQualifiedName(localName, Globals.SchemaNamespace);
                typeToName.Add(type, stableName);
                nameToType.Add(stableName, type);
            }

            static internal XmlQualifiedName GetBaseContractName(Type type)
            {
                XmlQualifiedName retVal = null;
                typeToName.TryGetValue(type, out retVal);
                return retVal;
            }

            static internal Type GetBaseType(XmlQualifiedName baseContractName)
            {
                Type retVal = null;
                nameToType.TryGetValue(baseContractName, out retVal);
                return retVal;
            }

            internal EnumDataContractCriticalHelper()
            {
                IsValueType = true;
            }

            internal EnumDataContractCriticalHelper(Type type)
                : base(type)
            {
                this.StableName = DataContract.GetStableName(type, out hasDataContract);
                Type baseType = Enum.GetUnderlyingType(type);
                baseContractName = GetBaseContractName(baseType);
                ImportBaseType(baseType);
                IsFlags = type.IsDefined(Globals.TypeOfFlagsAttribute, false);
                ImportDataMembers();

                XmlDictionary dictionary = new XmlDictionary(2 + Members.Count);
                Name = dictionary.Add(StableName.Name);
                Namespace = dictionary.Add(StableName.Namespace);
                childElementNames = new XmlDictionaryString[Members.Count];
                for (int i = 0; i < Members.Count; i++)
                    childElementNames[i] = dictionary.Add(Members[i].Name);

                DataContractAttribute dataContractAttribute;
                if (TryGetDCAttribute(type, out dataContractAttribute))
                {
                    if (dataContractAttribute.IsReference)
                    {
                        DataContract.ThrowInvalidDataContractException(
                                SR.GetString(SR.EnumTypeCannotHaveIsReference,
                                    DataContract.GetClrTypeFullName(type),
                                    dataContractAttribute.IsReference,
                                    false),
                                type);
                    }
                }
            }

            internal XmlQualifiedName BaseContractName
            {
                get
                {
                    return baseContractName;
                }
                set
                {
                    baseContractName = value;
                    Type baseType = GetBaseType(baseContractName);
                    if (baseType == null)
                        ThrowInvalidDataContractException(SR.GetString(SR.InvalidEnumBaseType, value.Name, value.Namespace, StableName.Name, StableName.Namespace));
                    ImportBaseType(baseType);
                }
            }

            internal List<DataMember> Members
            {
                get { return members; }
                set { members = value; }
            }

            internal List<long> Values
            {
                get { return values; }
                set { values = value; }
            }

            internal bool IsFlags
            {
                get { return isFlags; }
                set { isFlags = value; }
            }

            internal bool IsULong
            {
                get { return isULong; }
            }

            internal XmlDictionaryString[] ChildElementNames
            {
                get { return childElementNames; }
            }

            void ImportBaseType(Type baseType)
            {
                isULong = (baseType == Globals.TypeOfULong);
            }

            void ImportDataMembers()
            {
                Type type = this.UnderlyingType;
                FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
                Dictionary<string, DataMember> memberValuesTable = new Dictionary<string, DataMember>();
                List<DataMember> tempMembers = new List<DataMember>(fields.Length);
                List<long> tempValues = new List<long>(fields.Length);

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo field = fields[i];
                    bool enumMemberValid = false;
                    if (hasDataContract)
                    {
                        object[] memberAttributes = field.GetCustomAttributes(Globals.TypeOfEnumMemberAttribute, false);
                        if (memberAttributes != null && memberAttributes.Length > 0)
                        {
                            if (memberAttributes.Length > 1)
                                ThrowInvalidDataContractException(SR.GetString(SR.TooManyEnumMembers, DataContract.GetClrTypeFullName(field.DeclaringType), field.Name));
                            EnumMemberAttribute memberAttribute = (EnumMemberAttribute)memberAttributes[0];

                            DataMember memberContract = new DataMember(field);
                            if (memberAttribute.IsValueSetExplicit)
                            {
                                if (memberAttribute.Value == null || memberAttribute.Value.Length == 0)
                                    ThrowInvalidDataContractException(SR.GetString(SR.InvalidEnumMemberValue, field.Name, DataContract.GetClrTypeFullName(type)));
                                memberContract.Name = memberAttribute.Value;
                            }
                            else
                                memberContract.Name = field.Name;
                            ClassDataContract.CheckAndAddMember(tempMembers, memberContract, memberValuesTable);
                            enumMemberValid = true;
                        }

                        object[] dataMemberAttributes = field.GetCustomAttributes(Globals.TypeOfDataMemberAttribute, false);
                        if (dataMemberAttributes != null && dataMemberAttributes.Length > 0)
                            ThrowInvalidDataContractException(SR.GetString(SR.DataMemberOnEnumField, DataContract.GetClrTypeFullName(field.DeclaringType), field.Name));
                    }
                    else
                    {
                        if (!field.IsNotSerialized)
                        {
                            DataMember memberContract = new DataMember(field);
                            memberContract.Name = field.Name;
                            ClassDataContract.CheckAndAddMember(tempMembers, memberContract, memberValuesTable);
                            enumMemberValid = true;
                        }
                    }

                    if (enumMemberValid)
                    {
                        object enumValue = field.GetValue(null);
                        if (isULong)
                            tempValues.Add((long)((IConvertible)enumValue).ToUInt64(null));
                        else
                            tempValues.Add(((IConvertible)enumValue).ToInt64(null));
                    }
                }

                Thread.MemoryBarrier();
                members = tempMembers;
                values = tempValues;
            }
        }

        internal void WriteEnumValue(XmlWriterDelegator writer, object value)
        {
            long longValue = IsULong ? (long)((IConvertible)value).ToUInt64(null) : ((IConvertible)value).ToInt64(null);
            for (int i = 0; i < Values.Count; i++)
            {
                if (longValue == Values[i])
                {
                    writer.WriteString(ChildElementNames[i].Value);
                    return;
                }
            }
            if (IsFlags)
            {
                int zeroIndex = -1;
                bool noneWritten = true;
                for (int i = 0; i < Values.Count; i++)
                {
                    long current = Values[i];
                    if (current == 0)
                    {
                        zeroIndex = i;
                        continue;
                    }
                    if (longValue == 0)
                        break;
                    if ((current & longValue) == current)
                    {
                        if (noneWritten)
                            noneWritten = false;
                        else
                            writer.WriteString(DictionaryGlobals.Space.Value);

                        writer.WriteString(ChildElementNames[i].Value);
                        longValue &= ~current;
                    }
                }
                // enforce that enum value was completely parsed
                if (longValue != 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.InvalidEnumValueOnWrite, value, DataContract.GetClrTypeFullName(UnderlyingType))));

                if (noneWritten && zeroIndex >= 0)
                    writer.WriteString(ChildElementNames[zeroIndex].Value);
            }
            else
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.InvalidEnumValueOnWrite, value, DataContract.GetClrTypeFullName(UnderlyingType))));
        }

        internal object ReadEnumValue(XmlReaderDelegator reader)
        {
            string stringValue = reader.ReadElementContentAsString();
            long longValue = 0;
            int i = 0;
            if (IsFlags)
            {
                // Skip initial spaces
                for (; i < stringValue.Length; i++)
                    if (stringValue[i] != ' ')
                        break;

                // Read space-delimited values
                int startIndex = i;
                int count = 0;
                for (; i < stringValue.Length; i++)
                {
                    if (stringValue[i] == ' ')
                    {
                        count = i - startIndex;
                        if (count > 0)
                            longValue |= ReadEnumValue(stringValue, startIndex, count);
                        for (++i; i < stringValue.Length; i++)
                            if (stringValue[i] != ' ')
                                break;
                        startIndex = i;
                        if (i == stringValue.Length)
                            break;
                    }
                }
                count = i - startIndex;
                if (count > 0)
                    longValue |= ReadEnumValue(stringValue, startIndex, count);
            }
            else
            {
                if (stringValue.Length == 0)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.InvalidEnumValueOnRead, stringValue, DataContract.GetClrTypeFullName(UnderlyingType))));
                longValue = ReadEnumValue(stringValue, 0, stringValue.Length);
            }

            if (IsULong)
                return Enum.ToObject(UnderlyingType, (ulong)longValue);
            return Enum.ToObject(UnderlyingType, longValue);
        }

        long ReadEnumValue(string value, int index, int count)
        {
            for (int i = 0; i < Members.Count; i++)
            {
                string memberName = Members[i].Name;
                if (memberName.Length == count && String.CompareOrdinal(value, index, memberName, 0, count) == 0)
                {
                    return Values[i];
                }
            }
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(XmlObjectSerializer.CreateSerializationException(SR.GetString(SR.InvalidEnumValueOnRead, value.Substring(index, count), DataContract.GetClrTypeFullName(UnderlyingType))));
        }

        internal string GetStringFromEnumValue(long value)
        {
            if (IsULong)
                return XmlConvert.ToString((ulong)value);
            else
                return XmlConvert.ToString(value);
        }

        internal long GetEnumValueFromString(string value)
        {
            if (IsULong)
                return (long)XmlConverter.ToUInt64(value);
            else
                return XmlConverter.ToInt64(value);
        }

        internal override bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if (IsEqualOrChecked(other, checkedContracts))
                return true;

            if (base.Equals(other, null))
            {
                EnumDataContract dataContract = other as EnumDataContract;
                if (dataContract != null)
                {
                    if (Members.Count != dataContract.Members.Count || Values.Count != dataContract.Values.Count)
                        return false;
                    string[] memberNames1 = new string[Members.Count], memberNames2 = new string[Members.Count];
                    for (int i = 0; i < Members.Count; i++)
                    {
                        memberNames1[i] = Members[i].Name;
                        memberNames2[i] = dataContract.Members[i].Name;
                    }
                    Array.Sort(memberNames1);
                    Array.Sort(memberNames2);
                    for (int i = 0; i < Members.Count; i++)
                    {
                        if (memberNames1[i] != memberNames2[i])
                            return false;
                    }

                    return (IsFlags == dataContract.IsFlags);
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void WriteXmlValue(XmlWriterDelegator xmlWriter, object obj, XmlObjectSerializerWriteContext context)
        {
            WriteEnumValue(xmlWriter, obj);
        }

        public override object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            object obj = ReadEnumValue(xmlReader);
            if (context != null)
                context.AddNewObject(obj);
            return obj;
        }

    }
}
