//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using DataContractDictionary = System.Collections.Generic.Dictionary<System.Xml.XmlQualifiedName, DataContract>;

    internal delegate IXmlSerializable CreateXmlSerializableDelegate();
#if USE_REFEMIT
    public sealed class XmlDataContract : DataContract
#else
    internal sealed class XmlDataContract : DataContract
#endif
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        XmlDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal XmlDataContract()
            : base(new XmlDataContractCriticalHelper())
        {
            helper = base.Helper as XmlDataContractCriticalHelper;
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal XmlDataContract(Type type)
            : base(new XmlDataContractCriticalHelper(type))
        {
            helper = base.Helper as XmlDataContractCriticalHelper;
        }

        internal override DataContractDictionary KnownDataContracts
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical KnownDataContracts property.",
                Safe = "KnownDataContracts only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.KnownDataContracts; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical KnownDataContracts property.")]
            [SecurityCritical]
            set { helper.KnownDataContracts = value; }
        }

        internal XmlSchemaType XsdType
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical XsdType property.",
                Safe = "XsdType only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.XsdType; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical XsdType property.")]
            [SecurityCritical]
            set { helper.XsdType = value; }
        }

        internal bool IsAnonymous
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical IsAnonymous property.",
                Safe = "IsAnonymous only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsAnonymous; }
        }

        internal override bool HasRoot
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical HasRoot property.",
                Safe = "HasRoot only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.HasRoot; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical HasRoot property.")]
            [SecurityCritical]
            set { helper.HasRoot = value; }
        }

        internal override XmlDictionaryString TopLevelElementName
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical TopLevelElementName property.",
                Safe = "TopLevelElementName only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.TopLevelElementName; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical TopLevelElementName property.")]
            [SecurityCritical]
            set { helper.TopLevelElementName = value; }
        }

        internal override XmlDictionaryString TopLevelElementNamespace
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical TopLevelElementNamespace property.",
                Safe = "TopLevelElementNamespace only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.TopLevelElementNamespace; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical TopLevelElementNamespace property.")]
            [SecurityCritical]
            set { helper.TopLevelElementNamespace = value; }
        }

        internal bool IsTopLevelElementNullable
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical IsTopLevelElementNullable property.",
                Safe = "IsTopLevelElementNullable only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsTopLevelElementNullable; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical IsTopLevelElementNullable property.")]
            [SecurityCritical]
            set { helper.IsTopLevelElementNullable = value; }
        }

        internal bool IsTypeDefinedOnImport
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical IsTypeDefinedOnImport property.",
                Safe = "IsTypeDefinedOnImport only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.IsTypeDefinedOnImport; }

            [Fx.Tag.SecurityNote(Critical = "Sets the critical IsTypeDefinedOnImport property.")]
            [SecurityCritical]
            set { helper.IsTypeDefinedOnImport = value; }
        }

        internal CreateXmlSerializableDelegate CreateXmlSerializableDelegate
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical CreateXmlSerializableDelegate property.",
                Safe = "CreateXmlSerializableDelegate only needs to be protected for write; initialized in getter if null.")]
            [SecuritySafeCritical]
            get
            {
                if (helper.CreateXmlSerializableDelegate == null)
                {
                    lock (this)
                    {
                        if (helper.CreateXmlSerializableDelegate == null)
                        {
                            CreateXmlSerializableDelegate tempCreateXmlSerializable = GenerateCreateXmlSerializableDelegate();
                            Thread.MemoryBarrier();
                            helper.CreateXmlSerializableDelegate = tempCreateXmlSerializable;
                        }
                    }
                }
                return helper.CreateXmlSerializableDelegate;
            }
        }

        internal override bool CanContainReferences
        {
            get { return false; }
        }

        internal override bool IsBuiltInDataContract
        {
            get
            {
                return UnderlyingType == Globals.TypeOfXmlElement || UnderlyingType == Globals.TypeOfXmlNodeArray;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for for (de)serializing XML types."
            + " Since the data is cached statically, we lock down access to it.")]
        [SecurityCritical(SecurityCriticalScope.Everything)]
        class XmlDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            DataContractDictionary knownDataContracts;
            bool isKnownTypeAttributeChecked;
            XmlDictionaryString topLevelElementName;
            XmlDictionaryString topLevelElementNamespace;
            bool isTopLevelElementNullable;
            bool isTypeDefinedOnImport;
            XmlSchemaType xsdType;
            bool hasRoot;
            CreateXmlSerializableDelegate createXmlSerializable;

            internal XmlDataContractCriticalHelper()
            {
            }

            internal XmlDataContractCriticalHelper(Type type)
                : base(type)
            {
                if (type.IsDefined(Globals.TypeOfDataContractAttribute, false))
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.IXmlSerializableCannotHaveDataContract, DataContract.GetClrTypeFullName(type))));
                if (type.IsDefined(Globals.TypeOfCollectionDataContractAttribute, false))
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.IXmlSerializableCannotHaveCollectionDataContract, DataContract.GetClrTypeFullName(type))));
                XmlSchemaType xsdType;
                bool hasRoot;
                XmlQualifiedName stableName;
                SchemaExporter.GetXmlTypeInfo(type, out stableName, out xsdType, out hasRoot);
                this.StableName = stableName;
                this.XsdType = xsdType;
                this.HasRoot = hasRoot;
                XmlDictionary dictionary = new XmlDictionary();
                this.Name = dictionary.Add(StableName.Name);
                this.Namespace = dictionary.Add(StableName.Namespace);
                object[] xmlRootAttributes = (UnderlyingType == null) ? null : UnderlyingType.GetCustomAttributes(Globals.TypeOfXmlRootAttribute, false);
                if (xmlRootAttributes == null || xmlRootAttributes.Length == 0)
                {
                    if (hasRoot)
                    {
                        topLevelElementName = Name;
                        topLevelElementNamespace = (this.StableName.Namespace == Globals.SchemaNamespace) ? DictionaryGlobals.EmptyString : Namespace;
                        isTopLevelElementNullable = true;
                    }
                }
                else
                {
                    if (hasRoot)
                    {
                        XmlRootAttribute xmlRootAttribute = (XmlRootAttribute)xmlRootAttributes[0];
                        isTopLevelElementNullable = xmlRootAttribute.IsNullable;
                        string elementName = xmlRootAttribute.ElementName;
                        topLevelElementName = (elementName == null || elementName.Length == 0) ? Name : dictionary.Add(DataContract.EncodeLocalName(elementName));
                        string elementNs = xmlRootAttribute.Namespace;
                        topLevelElementNamespace = (elementNs == null || elementNs.Length == 0) ? DictionaryGlobals.EmptyString : dictionary.Add(elementNs);
                    }
                    else
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.IsAnyCannotHaveXmlRoot, DataContract.GetClrTypeFullName(UnderlyingType))));
                    }
                }
            }

            internal override DataContractDictionary KnownDataContracts
            {
                get
                {
                    if (!isKnownTypeAttributeChecked && UnderlyingType != null)
                    {
                        lock (this)
                        {
                            if (!isKnownTypeAttributeChecked)
                            {
                                knownDataContracts = DataContract.ImportKnownTypeAttributes(this.UnderlyingType);
                                Thread.MemoryBarrier();
                                isKnownTypeAttributeChecked = true;
                            }
                        }
                    }
                    return knownDataContracts;
                }
                set { knownDataContracts = value; }
            }

            internal XmlSchemaType XsdType
            {
                get { return xsdType; }
                set { xsdType = value; }
            }

            internal bool IsAnonymous
            {
                get { return xsdType != null; }
            }

            internal override bool HasRoot
            {
                get { return hasRoot; }
                set { hasRoot = value; }
            }

            internal override XmlDictionaryString TopLevelElementName
            {
                get { return topLevelElementName; }
                set { topLevelElementName = value; }
            }

            internal override XmlDictionaryString TopLevelElementNamespace
            {
                get { return topLevelElementNamespace; }
                set { topLevelElementNamespace = value; }
            }

            internal bool IsTopLevelElementNullable
            {
                get { return isTopLevelElementNullable; }
                set { isTopLevelElementNullable = value; }
            }

            internal bool IsTypeDefinedOnImport
            {
                get { return isTypeDefinedOnImport; }
                set { isTypeDefinedOnImport = value; }
            }

            internal CreateXmlSerializableDelegate CreateXmlSerializableDelegate
            {
                get { return createXmlSerializable; }
                set { createXmlSerializable = value; }
            }

        }

        ConstructorInfo GetConstructor()
        {
            Type type = UnderlyingType;

            if (type.IsValueType)
                return null;

            ConstructorInfo ctor = type.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Globals.EmptyTypeArray, null);
            if (ctor == null)
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidDataContractException(SR.GetString(SR.IXmlSerializableMustHaveDefaultConstructor, DataContract.GetClrTypeFullName(type))));

            return ctor;
        }

        [Fx.Tag.SecurityNote(Critical = "Sets critical properties on XmlDataContract.")]
        [SecurityCritical]
        internal void SetTopLevelElementName(XmlQualifiedName elementName)
        {
            if (elementName != null)
            {
                XmlDictionary dictionary = new XmlDictionary();
                this.TopLevelElementName = dictionary.Add(elementName.Name);
                this.TopLevelElementNamespace = dictionary.Add(elementName.Namespace);
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Calls CodeGenerator.BeginMethod which is SecurityCritical.",
            Safe = "Self-contained: returns the delegate to the generated IL but otherwise all IL generation is self-contained here.")]
        [SecuritySafeCritical]
        internal CreateXmlSerializableDelegate GenerateCreateXmlSerializableDelegate()
        {
            Type type = this.UnderlyingType;
            CodeGenerator ilg = new CodeGenerator();
            bool memberAccessFlag = RequiresMemberAccessForCreate(null);
            try
            {
                ilg.BeginMethod("Create" + DataContract.GetClrTypeFullName(type), typeof(CreateXmlSerializableDelegate), memberAccessFlag);
            }
            catch (SecurityException securityException)
            {
                if (memberAccessFlag && securityException.PermissionType.Equals(typeof(ReflectionPermission)))
                {
                    RequiresMemberAccessForCreate(securityException);
                }
                else
                {
                    throw;
                }
            }
            if (type.IsValueType)
            {
                System.Reflection.Emit.LocalBuilder local = ilg.DeclareLocal(type, type.Name + "Value");
                ilg.Ldloca(local);
                ilg.InitObj(type);
                ilg.Ldloc(local);
            }
            else
            {
                ilg.New(GetConstructor());
            }
            ilg.ConvertValue(this.UnderlyingType, Globals.TypeOfIXmlSerializable);
            ilg.Ret();
            return (CreateXmlSerializableDelegate)ilg.EndMethod();
        }

        [Fx.Tag.SecurityNote(Miscellaneous = "RequiresReview - Calculates whether this Xml type requires MemberAccessPermission for deserialization."
            + " Since this information is used to determine whether to give the generated code access"
            + " permissions to private members, any changes to the logic should be reviewed.")]
        bool RequiresMemberAccessForCreate(SecurityException securityException)
        {
            if (!IsTypeVisible(UnderlyingType))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(SR.PartialTrustIXmlSerializableTypeNotPublic, DataContract.GetClrTypeFullName(UnderlyingType)),
                        securityException));
                }
                return true;
            }

            if (ConstructorRequiresMemberAccess(GetConstructor()))
            {
                if (securityException != null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new SecurityException(SR.GetString(SR.PartialTrustIXmlSerialzableNoPublicConstructor, DataContract.GetClrTypeFullName(UnderlyingType)),
                        securityException));
                }
                return true;
            }

            return false;
        }

        internal override bool Equals(object other, Dictionary<DataContractPairKey, object> checkedContracts)
        {
            if (IsEqualOrChecked(other, checkedContracts))
                return true;

            XmlDataContract dataContract = other as XmlDataContract;
            if (dataContract != null)
            {
                if (this.HasRoot != dataContract.HasRoot)
                    return false;

                if (this.IsAnonymous)
                {
                    return dataContract.IsAnonymous;
                }
                else
                {
                    return (StableName.Name == dataContract.StableName.Name && StableName.Namespace == dataContract.StableName.Namespace);
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
            if (context == null)
                XmlObjectSerializerWriteContext.WriteRootIXmlSerializable(xmlWriter, obj);
            else
                context.WriteIXmlSerializable(xmlWriter, obj);
        }

        public override object ReadXmlValue(XmlReaderDelegator xmlReader, XmlObjectSerializerReadContext context)
        {
            object o;
            if (context == null)
            {
                o = XmlObjectSerializerReadContext.ReadRootIXmlSerializable(xmlReader, this, true /*isMemberType*/);
            }
            else
            {
                o = context.ReadIXmlSerializable(xmlReader, this, true /*isMemberType*/);
                context.AddNewObject(o);
            }
            xmlReader.ReadEndElement();
            return o;
        }

    }
}

