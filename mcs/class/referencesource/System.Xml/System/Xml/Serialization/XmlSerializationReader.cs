//------------------------------------------------------------------------------
// <copyright file="XmlSerializationReader.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System.IO;
    using System;
    using System.Security;
    using System.Collections;
    using System.Reflection;
    using System.Text;
    using System.Xml;
    using System.Xml.Schema;
    using System.ComponentModel;
    using System.Globalization;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.Threading;
    using System.Configuration;
    using System.Xml.Serialization.Configuration;

    /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader"]/*' />
    ///<internalonly/>
    public abstract class XmlSerializationReader : XmlSerializationGeneratedCode {
        XmlReader r;
        XmlCountingReader countingReader;
        XmlDocument d;
        Hashtable callbacks;
        Hashtable types;
        Hashtable typesReverse;
        XmlDeserializationEvents events;
        Hashtable targets;
        Hashtable referencedTargets;
        ArrayList targetsWithoutIds;
        ArrayList fixups;
        ArrayList collectionFixups;
        bool soap12;
        bool isReturnValue;
        bool decodeName = true;

        string schemaNsID;
        string schemaNs1999ID;
        string schemaNs2000ID;
        string schemaNonXsdTypesNsID;
        string instanceNsID;
        string instanceNs2000ID;
        string instanceNs1999ID;
        string soapNsID;
        string soap12NsID;
        string schemaID;
        string wsdlNsID;
        string wsdlArrayTypeID;
        string nullID;
        string nilID;
        string typeID;
        string arrayTypeID;
        string itemTypeID;
        string arraySizeID;
        string arrayID;
        string urTypeID;
        string stringID;
        string intID;
        string booleanID;
        string shortID;
        string longID;
        string floatID;
        string doubleID;
        string decimalID;
        string dateTimeID;
        string qnameID;
        string dateID;
        string timeID;
        string hexBinaryID;
        string base64BinaryID;
        string base64ID;
        string unsignedByteID;
        string byteID;
        string unsignedShortID;
        string unsignedIntID;
        string unsignedLongID;
        string oldDecimalID;
        string oldTimeInstantID;

        string anyURIID;
        string durationID;
        string ENTITYID;
        string ENTITIESID;
        string gDayID;
        string gMonthID;
        string gMonthDayID;
        string gYearID;
        string gYearMonthID;
        string IDID;
        string IDREFID;
        string IDREFSID;
        string integerID;
        string languageID;
        string NameID;
        string NCNameID;
        string NMTOKENID;
        string NMTOKENSID;
        string negativeIntegerID;
        string nonPositiveIntegerID;
        string nonNegativeIntegerID;
        string normalizedStringID;
        string NOTATIONID;
        string positiveIntegerID;
        string tokenID;

        string charID;
        string guidID;

        static bool checkDeserializeAdvances;

        static XmlSerializationReader()
        {
            XmlSerializerSection configSection = ConfigurationManager.GetSection(ConfigurationStrings.XmlSerializerSectionPath) as XmlSerializerSection;
            checkDeserializeAdvances = (configSection == null) ? false : configSection.CheckDeserializeAdvances;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.InitIDs"]/*' />
        protected abstract void InitIDs();

        // this method must be called before any generated deserialization methods are called
        internal void Init(XmlReader r, XmlDeserializationEvents events, string encodingStyle, TempAssembly tempAssembly) {
            this.events = events;
            if (checkDeserializeAdvances)
            {
                this.countingReader = new XmlCountingReader(r);
                this.r = this.countingReader;
            }
            else 
                this.r = r;
            this.d = null;
            this.soap12 = (encodingStyle == Soap12.Encoding);
            Init(tempAssembly);

            schemaNsID = r.NameTable.Add(XmlSchema.Namespace);
            schemaNs2000ID = r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema");
            schemaNs1999ID = r.NameTable.Add("http://www.w3.org/1999/XMLSchema");
            schemaNonXsdTypesNsID = r.NameTable.Add(UrtTypes.Namespace);
            instanceNsID = r.NameTable.Add(XmlSchema.InstanceNamespace);
            instanceNs2000ID = r.NameTable.Add("http://www.w3.org/2000/10/XMLSchema-instance");
            instanceNs1999ID = r.NameTable.Add("http://www.w3.org/1999/XMLSchema-instance");
            soapNsID = r.NameTable.Add(Soap.Encoding);
            soap12NsID = r.NameTable.Add(Soap12.Encoding);
            schemaID = r.NameTable.Add("schema");
            wsdlNsID = r.NameTable.Add(Wsdl.Namespace);
            wsdlArrayTypeID = r.NameTable.Add(Wsdl.ArrayType);
            nullID = r.NameTable.Add("null");
            nilID = r.NameTable.Add("nil");
            typeID = r.NameTable.Add("type");
            arrayTypeID = r.NameTable.Add("arrayType");
            itemTypeID = r.NameTable.Add("itemType");
            arraySizeID = r.NameTable.Add("arraySize");
            arrayID = r.NameTable.Add("Array");
            urTypeID = r.NameTable.Add(Soap.UrType);
            InitIDs();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.DecodeName"]/*' />
        protected bool DecodeName {
            get {
                return decodeName;
            }
            set {
                decodeName = value;
            }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.Reader"]/*' />
        protected XmlReader Reader {
            get {
                return r;
            }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReaderCount"]/*' />
        protected int ReaderCount {
            get {
                return checkDeserializeAdvances ? countingReader.AdvanceCount : 0;
            }
        }
        
        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.Document"]/*' />
        protected XmlDocument Document {
            get {
                if (d == null) {
                    d = new XmlDocument(r.NameTable);
                    d.SetBaseURI(r.BaseURI);
                }
                return d;
            }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ResolveDynamicAssembly"]/*' />
        ///<internalonly/>
        protected static Assembly ResolveDynamicAssembly(string assemblyFullName){
            return DynamicAssemblies.Get(assemblyFullName);
        }
        
        void InitPrimitiveIDs() {
            if (tokenID != null) return;
            object ns = r.NameTable.Add(XmlSchema.Namespace);
            object ns2 = r.NameTable.Add(UrtTypes.Namespace);
            
            stringID = r.NameTable.Add("string");
            intID = r.NameTable.Add("int");
            booleanID = r.NameTable.Add("boolean");
            shortID = r.NameTable.Add("short");
            longID = r.NameTable.Add("long");
            floatID = r.NameTable.Add("float");
            doubleID = r.NameTable.Add("double");
            decimalID = r.NameTable.Add("decimal");
            dateTimeID = r.NameTable.Add("dateTime");
            qnameID = r.NameTable.Add("QName");
            dateID = r.NameTable.Add("date");
            timeID = r.NameTable.Add("time");
            hexBinaryID = r.NameTable.Add("hexBinary");
            base64BinaryID = r.NameTable.Add("base64Binary");
            unsignedByteID = r.NameTable.Add("unsignedByte");
            byteID = r.NameTable.Add("byte");
            unsignedShortID = r.NameTable.Add("unsignedShort");
            unsignedIntID = r.NameTable.Add("unsignedInt");
            unsignedLongID = r.NameTable.Add("unsignedLong");
            oldDecimalID = r.NameTable.Add("decimal");
            oldTimeInstantID = r.NameTable.Add("timeInstant");
            charID = r.NameTable.Add("char");
            guidID = r.NameTable.Add("guid");
            base64ID = r.NameTable.Add("base64");

            anyURIID = r.NameTable.Add("anyURI");
            durationID = r.NameTable.Add("duration");
            ENTITYID = r.NameTable.Add("ENTITY");
            ENTITIESID = r.NameTable.Add("ENTITIES");
            gDayID = r.NameTable.Add("gDay");
            gMonthID = r.NameTable.Add("gMonth");
            gMonthDayID = r.NameTable.Add("gMonthDay");
            gYearID = r.NameTable.Add("gYear");
            gYearMonthID = r.NameTable.Add("gYearMonth");
            IDID = r.NameTable.Add("ID");
            IDREFID = r.NameTable.Add("IDREF");
            IDREFSID = r.NameTable.Add("IDREFS");
            integerID = r.NameTable.Add("integer");
            languageID = r.NameTable.Add("language");
            NameID = r.NameTable.Add("Name");
            NCNameID = r.NameTable.Add("NCName");
            NMTOKENID = r.NameTable.Add("NMTOKEN");
            NMTOKENSID = r.NameTable.Add("NMTOKENS");
            negativeIntegerID = r.NameTable.Add("negativeInteger");
            nonNegativeIntegerID = r.NameTable.Add("nonNegativeInteger");
            nonPositiveIntegerID = r.NameTable.Add("nonPositiveInteger");
            normalizedStringID = r.NameTable.Add("normalizedString");
            NOTATIONID = r.NameTable.Add("NOTATION");
            positiveIntegerID = r.NameTable.Add("positiveInteger");
            tokenID = r.NameTable.Add("token");
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.GetXsiType"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected XmlQualifiedName GetXsiType() {
            string type = r.GetAttribute(typeID, instanceNsID);
            if (type == null) {
                type = r.GetAttribute(typeID, instanceNs2000ID);
                if (type == null) {
                    type = r.GetAttribute(typeID, instanceNs1999ID);
                    if (type == null)
                        return null;
                }
            }
            return ToXmlQualifiedName(type, false);
        }

        // throwOnUnknown flag controls whether this method throws an exception or just returns 
        // null if typeName.Namespace is unknown. the method still throws if typeName.Namespace
        // is recognized but typeName.Name isn't.
        Type GetPrimitiveType(XmlQualifiedName typeName, bool throwOnUnknown) {
            InitPrimitiveIDs();

            if ((object)typeName.Namespace == (object)schemaNsID || (object)typeName.Namespace == (object)soapNsID || (object)typeName.Namespace == (object)soap12NsID) {
                if ((object) typeName.Name == (object) stringID ||
                    (object) typeName.Name == (object) anyURIID ||
                    (object) typeName.Name == (object) durationID ||
                    (object) typeName.Name == (object) ENTITYID ||
                    (object) typeName.Name == (object) ENTITIESID ||
                    (object) typeName.Name == (object) gDayID ||
                    (object) typeName.Name == (object) gMonthID ||
                    (object) typeName.Name == (object) gMonthDayID ||
                    (object) typeName.Name == (object) gYearID ||
                    (object) typeName.Name == (object) gYearMonthID ||
                    (object) typeName.Name == (object) IDID ||
                    (object) typeName.Name == (object) IDREFID ||
                    (object) typeName.Name == (object) IDREFSID ||
                    (object) typeName.Name == (object) integerID ||
                    (object) typeName.Name == (object) languageID ||
                    (object) typeName.Name == (object) NameID ||
                    (object) typeName.Name == (object) NCNameID ||
                    (object) typeName.Name == (object) NMTOKENID ||
                    (object) typeName.Name == (object) NMTOKENSID ||
                    (object) typeName.Name == (object) negativeIntegerID ||
                    (object) typeName.Name == (object) nonPositiveIntegerID ||
                    (object) typeName.Name == (object) nonNegativeIntegerID ||
                    (object) typeName.Name == (object) normalizedStringID ||
                    (object) typeName.Name == (object) NOTATIONID ||
                    (object) typeName.Name == (object) positiveIntegerID ||
                    (object) typeName.Name == (object) tokenID)
                    return typeof(string);
                else if ((object) typeName.Name == (object) intID)
                    return typeof(int);
                else if ((object) typeName.Name == (object) booleanID)
                    return typeof(bool);
                else if ((object) typeName.Name == (object) shortID)
                    return typeof(short);
                else if ((object) typeName.Name == (object) longID)
                    return typeof(long);
                else if ((object) typeName.Name == (object) floatID)
                    return typeof(float);
                else if ((object) typeName.Name == (object) doubleID)
                    return typeof(double);
                else if ((object) typeName.Name == (object) decimalID)
                    return typeof(decimal);
                else if ((object) typeName.Name == (object) dateTimeID)
                    return typeof(DateTime);
                else if ((object) typeName.Name == (object) qnameID)
                    return typeof(XmlQualifiedName);
                else if ((object) typeName.Name == (object) dateID)
                    return typeof(DateTime);
                else if ((object) typeName.Name == (object) timeID)
                    return typeof(DateTime);
                else if ((object) typeName.Name == (object) hexBinaryID)
                    return typeof(byte[]);
                else if ((object)typeName.Name == (object)base64BinaryID)
                    return typeof(byte[]);
                else if ((object)typeName.Name == (object)unsignedByteID)
                    return typeof(byte);
                else if ((object) typeName.Name == (object) byteID)
                    return typeof(SByte);
                else if ((object) typeName.Name == (object) unsignedShortID)
                    return typeof(UInt16);
                else if ((object) typeName.Name == (object) unsignedIntID)
                    return typeof(UInt32);
                else if ((object) typeName.Name == (object) unsignedLongID)
                    return typeof(UInt64);
                else
                    throw CreateUnknownTypeException(typeName);
            } 
            else if ((object) typeName.Namespace == (object) schemaNs2000ID || (object) typeName.Namespace == (object) schemaNs1999ID) {
                if ((object) typeName.Name == (object) stringID ||
                    (object) typeName.Name == (object) anyURIID ||
                    (object) typeName.Name == (object) durationID ||
                    (object) typeName.Name == (object) ENTITYID ||
                    (object) typeName.Name == (object) ENTITIESID ||
                    (object) typeName.Name == (object) gDayID ||
                    (object) typeName.Name == (object) gMonthID ||
                    (object) typeName.Name == (object) gMonthDayID ||
                    (object) typeName.Name == (object) gYearID ||
                    (object) typeName.Name == (object) gYearMonthID ||
                    (object) typeName.Name == (object) IDID ||
                    (object) typeName.Name == (object) IDREFID ||
                    (object) typeName.Name == (object) IDREFSID ||
                    (object) typeName.Name == (object) integerID ||
                    (object) typeName.Name == (object) languageID ||
                    (object) typeName.Name == (object) NameID ||
                    (object) typeName.Name == (object) NCNameID ||
                    (object) typeName.Name == (object) NMTOKENID ||
                    (object) typeName.Name == (object) NMTOKENSID ||
                    (object) typeName.Name == (object) negativeIntegerID ||
                    (object) typeName.Name == (object) nonPositiveIntegerID ||
                    (object) typeName.Name == (object) nonNegativeIntegerID ||
                    (object) typeName.Name == (object) normalizedStringID ||
                    (object) typeName.Name == (object) NOTATIONID ||
                    (object) typeName.Name == (object) positiveIntegerID ||
                    (object) typeName.Name == (object) tokenID)
                    return typeof(string);
                else if ((object) typeName.Name == (object) intID)
                    return typeof(int);
                else if ((object) typeName.Name == (object) booleanID)
                    return typeof(bool);
                else if ((object) typeName.Name == (object) shortID)
                    return typeof(short);
                else if ((object) typeName.Name == (object) longID)
                    return typeof(long);
                else if ((object) typeName.Name == (object) floatID)
                    return typeof(float);
                else if ((object) typeName.Name == (object) doubleID)
                    return typeof(double);
                else if ((object) typeName.Name == (object) oldDecimalID)
                    return typeof(decimal);
                else if ((object) typeName.Name == (object) oldTimeInstantID)
                    return typeof(DateTime);
                else if ((object) typeName.Name == (object) qnameID)
                    return typeof(XmlQualifiedName);
                else if ((object) typeName.Name == (object) dateID)
                    return typeof(DateTime);
                else if ((object) typeName.Name == (object) timeID)
                    return typeof(DateTime);
                else if ((object) typeName.Name == (object) hexBinaryID)
                    return typeof(byte[]);
                else if ((object) typeName.Name == (object) byteID)
                    return typeof(SByte);
                else if ((object) typeName.Name == (object) unsignedShortID)
                    return typeof(UInt16);
                else if ((object) typeName.Name == (object) unsignedIntID)
                    return typeof(UInt32);
                else if ((object) typeName.Name == (object) unsignedLongID)
                    return typeof(UInt64);
                else
                    throw CreateUnknownTypeException(typeName);
            }
            else if ((object) typeName.Namespace == (object) schemaNonXsdTypesNsID) {
                if ((object) typeName.Name == (object) charID)
                    return typeof(char);
                else if ((object) typeName.Name == (object) guidID)
                    return typeof(Guid);
                else
                    throw CreateUnknownTypeException(typeName);
            }
            else if (throwOnUnknown)
                throw CreateUnknownTypeException(typeName);
            else
                return null;
        }

        bool IsPrimitiveNamespace(string ns) {
            return (object) ns == (object) schemaNsID ||
                   (object) ns == (object) schemaNonXsdTypesNsID ||
                   (object) ns == (object) soapNsID ||
                   (object) ns == (object) soap12NsID ||
                   (object) ns == (object) schemaNs2000ID ||
                   (object) ns == (object) schemaNs1999ID;
        }

        private string ReadStringValue(){
            if (r.IsEmptyElement){
                r.Skip();
                return string.Empty;
            }
            r.ReadStartElement();
            string retVal = r.ReadString();
            ReadEndElement();
            return retVal;
        }

        private XmlQualifiedName ReadXmlQualifiedName(){
            string s;
            bool isEmpty = false;
            if (r.IsEmptyElement) {
                s = string.Empty;
                isEmpty = true;
            }
            else{
                r.ReadStartElement();
                s = r.ReadString();
            }
            XmlQualifiedName retVal = ToXmlQualifiedName(s);
            if (isEmpty)
                r.Skip();
            else
                ReadEndElement();
            return retVal;
        }

        private byte[] ReadByteArray(bool isBase64) {
            ArrayList list = new ArrayList();
            const   int MAX_ALLOC_SIZE = 64*1024;
            int     currentSize = 1024;
            byte[]  buffer;
            int     bytes = -1;
            int     offset = 0;
            int     total = 0;
            buffer = new byte[currentSize];
            list.Add(buffer);
            while (bytes != 0) {
                if (offset == buffer.Length) {
                    currentSize = Math.Min(currentSize*2, MAX_ALLOC_SIZE);
                    buffer = new byte[currentSize];
                    offset = 0;              
                    list.Add(buffer);
                }
                if (isBase64) {
                    bytes = r.ReadElementContentAsBase64(buffer, offset, buffer.Length-offset);
                }
                else {
                    bytes = r.ReadElementContentAsBinHex(buffer, offset, buffer.Length-offset);
                }
                offset += bytes;
                total += bytes;
            }

            byte[] result = new byte[total];
            offset = 0;
            foreach (byte[] block in list) {
                currentSize = Math.Min(block.Length, total);
                if (currentSize > 0) {
                    Buffer.BlockCopy(block, 0, result, offset, currentSize);
                    offset += currentSize;
                    total -= currentSize;
                }
            }
            list.Clear();
            return result;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadTypedPrimitive"]/*' />
        protected object ReadTypedPrimitive(XmlQualifiedName type) {
            return ReadTypedPrimitive(type, false);
        }

        private object ReadTypedPrimitive(XmlQualifiedName type, bool elementCanBeType) {
            InitPrimitiveIDs();
            object value = null;
            if (!IsPrimitiveNamespace(type.Namespace) || (object)type.Name == (object)urTypeID) 
                return ReadXmlNodes(elementCanBeType);

            if ((object)type.Namespace == (object)schemaNsID || (object)type.Namespace == (object)soapNsID || (object)type.Namespace == (object)soap12NsID) {
                if ((object) type.Name == (object) stringID ||
                    (object) type.Name == (object) normalizedStringID)
                    value = ReadStringValue();
                else if ((object) type.Name == (object) anyURIID ||
                    (object) type.Name == (object) durationID ||
                    (object) type.Name == (object) ENTITYID ||
                    (object) type.Name == (object) ENTITIESID ||
                    (object) type.Name == (object) gDayID ||
                    (object) type.Name == (object) gMonthID ||
                    (object) type.Name == (object) gMonthDayID ||
                    (object) type.Name == (object) gYearID ||
                    (object) type.Name == (object) gYearMonthID ||
                    (object) type.Name == (object) IDID ||
                    (object) type.Name == (object) IDREFID ||
                    (object) type.Name == (object) IDREFSID ||
                    (object) type.Name == (object) integerID ||
                    (object) type.Name == (object) languageID ||
                    (object) type.Name == (object) NameID ||
                    (object) type.Name == (object) NCNameID ||
                    (object) type.Name == (object) NMTOKENID ||
                    (object) type.Name == (object) NMTOKENSID ||
                    (object) type.Name == (object) negativeIntegerID ||
                    (object) type.Name == (object) nonPositiveIntegerID ||
                    (object) type.Name == (object) nonNegativeIntegerID ||
                    (object) type.Name == (object) NOTATIONID ||
                    (object) type.Name == (object) positiveIntegerID ||
                    (object) type.Name == (object) tokenID)
                    value = CollapseWhitespace(ReadStringValue());
                else if ((object) type.Name == (object) intID)
                    value = XmlConvert.ToInt32(ReadStringValue());
                else if ((object) type.Name == (object) booleanID)
                    value = XmlConvert.ToBoolean(ReadStringValue());
                else if ((object) type.Name == (object) shortID)
                    value = XmlConvert.ToInt16(ReadStringValue());
                else if ((object) type.Name == (object) longID)
                    value = XmlConvert.ToInt64(ReadStringValue());
                else if ((object)type.Name == (object)floatID)
                    value = XmlConvert.ToSingle(ReadStringValue());
                else if ((object)type.Name == (object)doubleID)
                    value = XmlConvert.ToDouble(ReadStringValue());
                else if ((object)type.Name == (object)decimalID)
                    value = XmlConvert.ToDecimal(ReadStringValue());
                else if ((object)type.Name == (object)dateTimeID)
                    value = ToDateTime(ReadStringValue());
                else if ((object) type.Name == (object) qnameID)
                    value = ReadXmlQualifiedName();
                else if ((object) type.Name == (object) dateID)
                    value = ToDate(ReadStringValue());
                else if ((object) type.Name == (object) timeID)
                    value = ToTime(ReadStringValue());
                else if ((object) type.Name == (object) unsignedByteID)
                    value = XmlConvert.ToByte(ReadStringValue());
                else if ((object) type.Name == (object) byteID)
                    value = XmlConvert.ToSByte(ReadStringValue());
                else if ((object) type.Name == (object) unsignedShortID)
                    value = XmlConvert.ToUInt16(ReadStringValue());
                else if ((object) type.Name == (object) unsignedIntID)
                    value = XmlConvert.ToUInt32(ReadStringValue());
                else if ((object) type.Name == (object) unsignedLongID)
                    value = XmlConvert.ToUInt64(ReadStringValue());
                else if ((object) type.Name == (object) hexBinaryID)
                    value = ToByteArrayHex(false);
                else if ((object) type.Name == (object) base64BinaryID)
                    value = ToByteArrayBase64(false);
                else if ((object) type.Name == (object)base64ID && ((object)type.Namespace == (object)soapNsID || (object)type.Namespace == (object)soap12NsID))
                    value = ToByteArrayBase64(false);
                else
                    value = ReadXmlNodes(elementCanBeType);
            }
            else if ((object) type.Namespace == (object) schemaNs2000ID || (object) type.Namespace == (object) schemaNs1999ID) {
                if ((object) type.Name == (object) stringID ||
                    (object) type.Name == (object) normalizedStringID)
                    value = ReadStringValue();
                else if ((object) type.Name == (object) anyURIID ||
                    (object) type.Name == (object) anyURIID ||
                    (object) type.Name == (object) durationID ||
                    (object) type.Name == (object) ENTITYID ||
                    (object) type.Name == (object) ENTITIESID ||
                    (object) type.Name == (object) gDayID ||
                    (object) type.Name == (object) gMonthID ||
                    (object) type.Name == (object) gMonthDayID ||
                    (object) type.Name == (object) gYearID ||
                    (object) type.Name == (object) gYearMonthID ||
                    (object) type.Name == (object) IDID ||
                    (object) type.Name == (object) IDREFID ||
                    (object) type.Name == (object) IDREFSID ||
                    (object) type.Name == (object) integerID ||
                    (object) type.Name == (object) languageID ||
                    (object) type.Name == (object) NameID ||
                    (object) type.Name == (object) NCNameID ||
                    (object) type.Name == (object) NMTOKENID ||
                    (object) type.Name == (object) NMTOKENSID ||
                    (object) type.Name == (object) negativeIntegerID ||
                    (object) type.Name == (object) nonPositiveIntegerID ||
                    (object) type.Name == (object) nonNegativeIntegerID ||
                    (object) type.Name == (object) NOTATIONID ||
                    (object) type.Name == (object) positiveIntegerID ||
                    (object) type.Name == (object) tokenID)
                    value = CollapseWhitespace(ReadStringValue());
                else if ((object) type.Name == (object) intID)
                    value = XmlConvert.ToInt32(ReadStringValue());
                else if ((object) type.Name == (object) booleanID)
                    value = XmlConvert.ToBoolean(ReadStringValue());
                else if ((object) type.Name == (object) shortID)
                    value = XmlConvert.ToInt16(ReadStringValue());
                else if ((object) type.Name == (object) longID)
                    value = XmlConvert.ToInt64(ReadStringValue());
                else if ((object)type.Name == (object)floatID)
                    value = XmlConvert.ToSingle(ReadStringValue());
                else if ((object)type.Name == (object)doubleID)
                    value = XmlConvert.ToDouble(ReadStringValue());
                else if ((object)type.Name == (object) oldDecimalID)
                    value = XmlConvert.ToDecimal(ReadStringValue());
                else if ((object)type.Name == (object) oldTimeInstantID)
                    value = ToDateTime(ReadStringValue());
                else if ((object) type.Name == (object) qnameID)
                    value = ReadXmlQualifiedName();
                else if ((object) type.Name == (object) dateID)
                    value = ToDate(ReadStringValue());
                else if ((object) type.Name == (object) timeID)
                    value = ToTime(ReadStringValue());
                else if ((object) type.Name == (object) unsignedByteID)
                    value = XmlConvert.ToByte(ReadStringValue());
                else if ((object) type.Name == (object) byteID)
                    value = XmlConvert.ToSByte(ReadStringValue());
                else if ((object) type.Name == (object) unsignedShortID)
                    value = XmlConvert.ToUInt16(ReadStringValue());
                else if ((object) type.Name == (object) unsignedIntID)
                    value = XmlConvert.ToUInt32(ReadStringValue());
                else if ((object) type.Name == (object) unsignedLongID)
                    value = XmlConvert.ToUInt64(ReadStringValue());
                else
                    value = ReadXmlNodes(elementCanBeType);
            }
            else if ((object) type.Namespace == (object) schemaNonXsdTypesNsID) {
                if ((object) type.Name == (object) charID)
                    value = ToChar(ReadStringValue());
                else if ((object) type.Name == (object) guidID)
                    value = new Guid(CollapseWhitespace(ReadStringValue()));
                else
                    value = ReadXmlNodes(elementCanBeType);
            }
            else
                value = ReadXmlNodes(elementCanBeType);
            return value;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadTypedNull"]/*' />
        protected object ReadTypedNull(XmlQualifiedName type) {
            InitPrimitiveIDs();
            object value = null;
            if (!IsPrimitiveNamespace(type.Namespace) || (object)type.Name == (object)urTypeID) {
                return null;
            }

            if ((object)type.Namespace == (object)schemaNsID || (object)type.Namespace == (object)soapNsID || (object)type.Namespace == (object)soap12NsID) {
                if ((object) type.Name == (object) stringID ||
                    (object) type.Name == (object) anyURIID ||
                    (object) type.Name == (object) durationID ||
                    (object) type.Name == (object) ENTITYID ||
                    (object) type.Name == (object) ENTITIESID ||
                    (object) type.Name == (object) gDayID ||
                    (object) type.Name == (object) gMonthID ||
                    (object) type.Name == (object) gMonthDayID ||
                    (object) type.Name == (object) gYearID ||
                    (object) type.Name == (object) gYearMonthID ||
                    (object) type.Name == (object) IDID ||
                    (object) type.Name == (object) IDREFID ||
                    (object) type.Name == (object) IDREFSID ||
                    (object) type.Name == (object) integerID ||
                    (object) type.Name == (object) languageID ||
                    (object) type.Name == (object) NameID ||
                    (object) type.Name == (object) NCNameID ||
                    (object) type.Name == (object) NMTOKENID ||
                    (object) type.Name == (object) NMTOKENSID ||
                    (object) type.Name == (object) negativeIntegerID ||
                    (object) type.Name == (object) nonPositiveIntegerID ||
                    (object) type.Name == (object) nonNegativeIntegerID ||
                    (object) type.Name == (object) normalizedStringID ||
                    (object) type.Name == (object) NOTATIONID ||
                    (object) type.Name == (object) positiveIntegerID ||
                    (object) type.Name == (object) tokenID)
                    value = null;
                else if ((object) type.Name == (object) intID) {
                    value = default(Nullable<int>);
                }
                else if ((object) type.Name == (object) booleanID)
                    value = default(Nullable<bool>);
                else if ((object) type.Name == (object) shortID)
                    value = default(Nullable<Int16>);
                        else if ((object) type.Name == (object) longID)
                    value = default(Nullable<long>);
                else if ((object)type.Name == (object)floatID)
                    value = default(Nullable<float>);
                else if ((object)type.Name == (object)doubleID)
                    value = default(Nullable<double>);
                else if ((object)type.Name == (object)decimalID)
                    value = default(Nullable<decimal>);
                else if ((object)type.Name == (object)dateTimeID)
                    value = default(Nullable<DateTime>);
                        else if ((object) type.Name == (object) qnameID)
                    value = null;
                else if ((object) type.Name == (object) dateID)
                    value = default(Nullable<DateTime>);
                        else if ((object) type.Name == (object) timeID)
                    value = default(Nullable<DateTime>);
                        else if ((object) type.Name == (object) unsignedByteID)
                    value = default(Nullable<byte>);
                else if ((object) type.Name == (object) byteID)
                    value = default(Nullable<SByte>);
                        else if ((object) type.Name == (object) unsignedShortID)
                    value = default(Nullable<UInt16>);
                        else if ((object) type.Name == (object) unsignedIntID)
                    value = default(Nullable<UInt32>);
                        else if ((object) type.Name == (object) unsignedLongID)
                    value = default(Nullable<UInt64>);
                        else if ((object) type.Name == (object) hexBinaryID)
                    value = null;
                else if ((object) type.Name == (object) base64BinaryID)
                    value = null;
                else if ((object) type.Name == (object)base64ID && ((object)type.Namespace == (object)soapNsID || (object)type.Namespace == (object)soap12NsID))
                    value = null;
                else
                    value = null;
            }
            else if ((object) type.Namespace == (object) schemaNonXsdTypesNsID) {
                if ((object) type.Name == (object) charID)
                    value = default(Nullable<char>);
                else if ((object) type.Name == (object) guidID)
                    value = default(Nullable<Guid>);
                        else
                    value = null;
            }
            else
                value = null;
            return value;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.IsXmlnsAttribute"]/*' />
        protected bool IsXmlnsAttribute(string name) {
            if (!name.StartsWith("xmlns", StringComparison.Ordinal)) return false;
            if (name.Length == 5) return true;
            return name[5] == ':';
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.IsArrayTypeAttribute"]/*' />
        protected void ParseWsdlArrayType(XmlAttribute attr) {
            if ((object)attr.LocalName == (object)wsdlArrayTypeID && (object)attr.NamespaceURI == (object)wsdlNsID ) {

                int colon = attr.Value.LastIndexOf(':');
                if (colon < 0) {
                    attr.Value = r.LookupNamespace("") + ":" + attr.Value;
                }
                else {
                    attr.Value = r.LookupNamespace(attr.Value.Substring(0, colon)) + ":" + attr.Value.Substring(colon + 1);
                }
            }
            return;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.IsReturnValue"]/*' />
        protected bool IsReturnValue {
            // value only valid for soap 1.1
            get { return isReturnValue && !soap12; }
            set { isReturnValue = value; }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadNull"]/*' />
        protected bool ReadNull() {
            if (!GetNullAttr()) return false;
            if (r.IsEmptyElement) {
                r.Skip();
                return true;
            }
            r.ReadStartElement();
            int whileIterations = 0;
            int readerCount = ReaderCount;
            while (r.NodeType != XmlNodeType.EndElement)
            {
                UnknownNode(null);
                CheckReaderCount(ref whileIterations, ref readerCount);
            }
            ReadEndElement();
            return true;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.GetNullAttr"]/*' />
        protected bool GetNullAttr() {
            string isNull = r.GetAttribute(nilID, instanceNsID);
            if(isNull == null)
                isNull = r.GetAttribute(nullID, instanceNsID);
            if (isNull == null) {
                isNull = r.GetAttribute(nullID, instanceNs2000ID);
                if (isNull == null)
                    isNull = r.GetAttribute(nullID, instanceNs1999ID);
            }
            if (isNull == null || !XmlConvert.ToBoolean(isNull)) return false;
            return true;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadNullableString"]/*' />
        protected string ReadNullableString() {
            if (ReadNull()) return null;
            return r.ReadElementString();
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadNullableQualifiedName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected XmlQualifiedName ReadNullableQualifiedName() {
            if (ReadNull()) return null;
            return ReadElementQualifiedName();
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadElementQualifiedName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected XmlQualifiedName ReadElementQualifiedName() {
            if (r.IsEmptyElement) {
                XmlQualifiedName empty = new XmlQualifiedName(string.Empty, r.LookupNamespace(""));
                r.Skip();
                return empty;
            }
            XmlQualifiedName qname = ToXmlQualifiedName(CollapseWhitespace(r.ReadString()));
            r.ReadEndElement();
            return qname;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadXmlDocument"]/*' />
        protected XmlDocument ReadXmlDocument(bool wrapped) {
            XmlNode n = ReadXmlNode(wrapped);
            if (n == null)
                return null;
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.ImportNode(n, true));
            return doc;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CollapseWhitespace"]/*' />
        protected string CollapseWhitespace(string value) {
            if (value == null)
                return null;
            return value.Trim();
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadXmlNode"]/*' />
        protected XmlNode ReadXmlNode(bool wrapped) {
            XmlNode node = null;
            if (wrapped) {
                if (ReadNull()) return null;
                r.ReadStartElement();
                r.MoveToContent();
                if (r.NodeType != XmlNodeType.EndElement)
                    node = Document.ReadNode(r);
                int whileIterations = 0;
                int readerCount = ReaderCount;
                while (r.NodeType != XmlNodeType.EndElement)
                {
                    UnknownNode(null);
                    CheckReaderCount(ref whileIterations, ref readerCount);
                }
                r.ReadEndElement();
            }
            else {
                node = Document.ReadNode(r);
            }
            return node;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToByteArrayBase64"]/*' />
        protected static byte[] ToByteArrayBase64(string value) {
            return XmlCustomFormatter.ToByteArrayBase64(value);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToByteArrayBase641"]/*' />
        protected byte[] ToByteArrayBase64(bool isNull) {
            if (isNull) {
                return null;
            }
            return ReadByteArray(true); //means use Base64
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToByteArrayHex"]/*' />
        protected static byte[] ToByteArrayHex(string value) {
            return XmlCustomFormatter.ToByteArrayHex(value);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToByteArrayHex1"]/*' />
        protected byte[] ToByteArrayHex(bool isNull) {
            if (isNull) {
                return null;
            }
            return ReadByteArray(false); //means use BinHex
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.GetArrayLength"]/*' />
        protected int GetArrayLength(string name, string ns) {
            if (GetNullAttr()) return 0;
            string arrayType = r.GetAttribute(arrayTypeID, soapNsID);
            SoapArrayInfo arrayInfo = ParseArrayType(arrayType);
            if (arrayInfo.dimensions != 1) throw new InvalidOperationException(Res.GetString(Res.XmlInvalidArrayDimentions, CurrentTag()));
            XmlQualifiedName qname = ToXmlQualifiedName(arrayInfo.qname, false);
            if (qname.Name != name) throw new InvalidOperationException(Res.GetString(Res.XmlInvalidArrayTypeName, qname.Name, name, CurrentTag()));
            if (qname.Namespace != ns) throw new InvalidOperationException(Res.GetString(Res.XmlInvalidArrayTypeNamespace, qname.Namespace, ns, CurrentTag()));
            return arrayInfo.length;
        }

        struct SoapArrayInfo {
            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.qname;"]/*' />
            public string qname;
            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.dimensions;"]/*' />
            public int dimensions;
            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.length;"]/*' />
            public int length;
            public int jaggedDimensions;
        }
 
        private SoapArrayInfo ParseArrayType(string value) {
            if (value == null) {
                throw new ArgumentNullException(Res.GetString(Res.XmlMissingArrayType, CurrentTag()));
            }

            if (value.Length == 0) {
                throw new ArgumentException(Res.GetString(Res.XmlEmptyArrayType, CurrentTag()), "value");
            }
 
            char[] chars = value.ToCharArray();
            int charsLength = chars.Length;
        
            SoapArrayInfo soapArrayInfo = new SoapArrayInfo(); 
 
            // Parse backwards to get length first, then optional dimensions, then qname.
            int pos = charsLength - 1;
 
            // Must end with ]
            if (chars[pos] != ']') {
                throw new ArgumentException(Res.GetString(Res.XmlInvalidArraySyntax), "value");
            }
            pos--;   
 
            // Find [
            while (pos != -1 && chars[pos] != '[') {
                if (chars[pos] == ',')
                    throw new ArgumentException(Res.GetString(Res.XmlInvalidArrayDimentions, CurrentTag()), "value");
                pos--;
            }
            if (pos == -1) {
                throw new ArgumentException(Res.GetString(Res.XmlMismatchedArrayBrackets), "value");
            }
 
            int len = charsLength - pos - 2;
            if (len > 0) {
                string lengthString = new String(chars, pos + 1, len);
                try {
                    soapArrayInfo.length = Int32.Parse(lengthString, CultureInfo.InvariantCulture);
                }
                catch (Exception e) {
                    if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                        throw;
                    }
                    throw new ArgumentException(Res.GetString(Res.XmlInvalidArrayLength, lengthString), "value");
                }
            }
            else {
                soapArrayInfo.length = -1;
            }

            pos--;         

            soapArrayInfo.jaggedDimensions = 0;
            while (pos != -1 && chars[pos] == ']') {
                pos--;
                if (pos < 0)
                    throw new ArgumentException(Res.GetString(Res.XmlMismatchedArrayBrackets), "value");
                if (chars[pos] == ',')
                    throw new ArgumentException(Res.GetString(Res.XmlInvalidArrayDimentions, CurrentTag()), "value");
                else if (chars[pos] != '[')
                    throw new ArgumentException(Res.GetString(Res.XmlInvalidArraySyntax), "value");
                pos--;
                soapArrayInfo.jaggedDimensions++;
            }

            soapArrayInfo.dimensions = 1;
 
            // everything else is qname - validation of qnames?
            soapArrayInfo.qname = new String(chars, 0, pos + 1);
            return soapArrayInfo;
        }

        private SoapArrayInfo ParseSoap12ArrayType(string itemType, string arraySize) {
            SoapArrayInfo soapArrayInfo = new SoapArrayInfo(); 

            if (itemType != null && itemType.Length > 0)
                soapArrayInfo.qname = itemType;
            else
                soapArrayInfo.qname = "";

            string[] dimensions;
            if (arraySize != null && arraySize.Length > 0)
                dimensions = arraySize.Split(null);
            else
                dimensions = new string[0];

            soapArrayInfo.dimensions = 0;
            soapArrayInfo.length = -1;
            for (int i = 0; i < dimensions.Length; i++) {
                if (dimensions[i].Length > 0) {
                    if (dimensions[i] == "*") {
                        soapArrayInfo.dimensions++;
                    }
                    else {
                        try {
                            soapArrayInfo.length = Int32.Parse(dimensions[i], CultureInfo.InvariantCulture);
                            soapArrayInfo.dimensions++;
                        }
                        catch (Exception e) {
                            if (e is ThreadAbortException || e is StackOverflowException || e is OutOfMemoryException) {
                                throw;
                            }
                            throw new ArgumentException(Res.GetString(Res.XmlInvalidArrayLength, dimensions[i]), "value");
                        }
                    }
                }
            }
            if (soapArrayInfo.dimensions == 0)
                soapArrayInfo.dimensions = 1; // default is 1D even if no arraySize is specified

            return soapArrayInfo;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToDateTime"]/*' />
        protected static DateTime ToDateTime(string value) {
            return XmlCustomFormatter.ToDateTime(value);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToDate"]/*' />
        protected static DateTime ToDate(string value) {
            return XmlCustomFormatter.ToDate(value);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToTime"]/*' />
        protected static DateTime ToTime(string value) {
            return XmlCustomFormatter.ToTime(value);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToChar"]/*' />
        protected static char ToChar(string value) {
            return XmlCustomFormatter.ToChar(value);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToEnum"]/*' />
        protected static long ToEnum(string value, Hashtable h, string typeName) {
            return XmlCustomFormatter.ToEnum(value, h, typeName, true);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToXmlName"]/*' />
        protected static string ToXmlName(string value) {
            return XmlCustomFormatter.ToXmlName(value);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToXmlNCName"]/*' />
        protected static string ToXmlNCName(string value) {
            return XmlCustomFormatter.ToXmlNCName(value);
        }
        
        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToXmlNmToken"]/*' />
        protected static string ToXmlNmToken(string value) {
            return XmlCustomFormatter.ToXmlNmToken(value);
        }
        
        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToXmlNmTokens"]/*' />
        protected static string ToXmlNmTokens(string value) {
            return XmlCustomFormatter.ToXmlNmTokens(value);
        }
        
        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ToXmlQualifiedName"]/*' />
        protected XmlQualifiedName ToXmlQualifiedName(string value) {
            return ToXmlQualifiedName(value, DecodeName);
        }

        internal XmlQualifiedName ToXmlQualifiedName(string value, bool decodeName) {
            int colon = value == null ? -1 : value.LastIndexOf(':');
            string prefix = colon < 0 ? null : value.Substring(0, colon);
            string localName = value.Substring(colon + 1);

            if (decodeName) {
                prefix = XmlConvert.DecodeName(prefix);
                localName = XmlConvert.DecodeName(localName);
            }
            if (prefix == null || prefix.Length == 0) {
                return new XmlQualifiedName(r.NameTable.Add(value), r.LookupNamespace(String.Empty));
            }
            else {
                string ns = r.LookupNamespace(prefix);
                if (ns == null) {
                    // Namespace prefix '{0}' is not defined.
                    throw new InvalidOperationException(Res.GetString(Res.XmlUndefinedAlias, prefix));
                }
                return new XmlQualifiedName(r.NameTable.Add(localName), ns);
            }
        }
        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.UnknownAttribute"]/*' />
        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.UnknownAttribute"]/*' />
        protected void UnknownAttribute(object o, XmlAttribute attr) {
            UnknownAttribute(o, attr, null);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.UnknownAttribute1"]/*' />
        protected void UnknownAttribute(object o, XmlAttribute attr, string qnames) {
            if (events.OnUnknownAttribute != null) {
                int lineNumber, linePosition;
                GetCurrentPosition(out lineNumber, out linePosition);
                XmlAttributeEventArgs e = new XmlAttributeEventArgs(attr, lineNumber, linePosition, o, qnames);
                events.OnUnknownAttribute(events.sender, e);
            }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.UnknownAttribute"]/*' />
        protected void UnknownElement(object o, XmlElement elem) {
            UnknownElement(o, elem, null);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.UnknownElement1"]/*' />
        protected void UnknownElement(object o, XmlElement elem, string qnames) {
            if (events.OnUnknownElement != null) {
                int lineNumber, linePosition;
                GetCurrentPosition(out lineNumber, out linePosition);
                XmlElementEventArgs e = new XmlElementEventArgs(elem, lineNumber, linePosition, o, qnames);
                events.OnUnknownElement(events.sender, e);
            }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.UnknownNode"]/*' />
        protected void UnknownNode(object o) {
            UnknownNode(o, null);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.UnknownNode1"]/*' />
        protected void UnknownNode(object o, string qnames) {
            if (r.NodeType == XmlNodeType.None || r.NodeType == XmlNodeType.Whitespace) {
                r.Read();
                return;
            }
            if (r.NodeType == XmlNodeType.EndElement)
                return;
            if (events.OnUnknownNode != null) {
                UnknownNode(Document.ReadNode(r), o, qnames);
            }
            else if (r.NodeType == XmlNodeType.Attribute && events.OnUnknownAttribute == null) {
                return;
            }
            else if (r.NodeType == XmlNodeType.Element && events.OnUnknownElement == null) {
                r.Skip();
                return;
            }
            else {
                UnknownNode(Document.ReadNode(r), o, qnames);
            }
        }

        void UnknownNode(XmlNode unknownNode, object o, string qnames) {
            if (unknownNode == null)
                return;
            if (unknownNode.NodeType != XmlNodeType.None && unknownNode.NodeType != XmlNodeType.Whitespace && events.OnUnknownNode != null) {
                int lineNumber, linePosition;
                GetCurrentPosition(out lineNumber, out linePosition);
                XmlNodeEventArgs e = new XmlNodeEventArgs(unknownNode, lineNumber, linePosition, o);
                events.OnUnknownNode(events.sender, e);
            }
            if (unknownNode.NodeType == XmlNodeType.Attribute) {
                UnknownAttribute(o, (XmlAttribute)unknownNode, qnames);
            }
            else if (unknownNode.NodeType == XmlNodeType.Element) {
                UnknownElement(o, (XmlElement)unknownNode, qnames);
            }
        }


        void GetCurrentPosition(out int lineNumber, out int linePosition){
            if (Reader is IXmlLineInfo){
                IXmlLineInfo lineInfo = (IXmlLineInfo)Reader;
                lineNumber = lineInfo.LineNumber;
                linePosition = lineInfo.LinePosition;
            }
            else
                lineNumber = linePosition = -1;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.UnreferencedObject"]/*' />
        protected void UnreferencedObject(string id, object o) {
            if (events.OnUnreferencedObject != null) {
                UnreferencedObjectEventArgs e = new UnreferencedObjectEventArgs(o, id);
                events.OnUnreferencedObject(events.sender, e);
            }
        }

        string CurrentTag() {
            switch (r.NodeType) {
                case XmlNodeType.Element:
                    return "<" + r.LocalName + " xmlns='" + r.NamespaceURI + "'>";
                case XmlNodeType.EndElement:
                    return ">";
                case XmlNodeType.Text:
                    return r.Value;
                case XmlNodeType.CDATA:
                    return "CDATA";
                case XmlNodeType.Comment:
                    return "<--";
                case XmlNodeType.ProcessingInstruction:
                    return "<?";
                default:
                    return "(unknown)";
            }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateUnknownTypeException"]/*' />
        protected Exception CreateUnknownTypeException(XmlQualifiedName type) {
            return new InvalidOperationException(Res.GetString(Res.XmlUnknownType, type.Name, type.Namespace, CurrentTag()));
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateReadOnlyCollectionException"]/*' />
        protected Exception CreateReadOnlyCollectionException(string name) {
            return new InvalidOperationException(Res.GetString(Res.XmlReadOnlyCollection, name));
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateAbstractTypeException"]/*' />
        protected Exception CreateAbstractTypeException(string name, string ns) {
            return new InvalidOperationException(Res.GetString(Res.XmlAbstractType, name, ns, CurrentTag()));
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateInaccessibleConstructorException"]/*' />
        protected Exception CreateInaccessibleConstructorException(string typeName) {
            return new InvalidOperationException(Res.GetString(Res.XmlConstructorInaccessible, typeName));
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateCtorHasSecurityException"]/*' />
        protected Exception CreateCtorHasSecurityException(string typeName) {
            return new InvalidOperationException(Res.GetString(Res.XmlConstructorHasSecurityAttributes, typeName));
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateUnknownNodeException"]/*' />
        protected Exception CreateUnknownNodeException() {
            return new InvalidOperationException(Res.GetString(Res.XmlUnknownNode, CurrentTag()));
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateUnknownConstantException"]/*' />
        protected Exception CreateUnknownConstantException(string value, Type enumType) {
            return new InvalidOperationException(Res.GetString(Res.XmlUnknownConstant, value, enumType.Name));
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateInvalidCastException"]/*' />
        protected Exception CreateInvalidCastException(Type type, object value) {
            return CreateInvalidCastException(type, value, null);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateInvalidCastException1"]/*' />
        protected Exception CreateInvalidCastException(Type type, object value, string id) {
            if (value == null)
                return new InvalidCastException(Res.GetString(Res.XmlInvalidNullCast, type.FullName));
            else if (id == null)
                return new InvalidCastException(Res.GetString(Res.XmlInvalidCast, value.GetType().FullName, type.FullName));
            else
                return new InvalidCastException(Res.GetString(Res.XmlInvalidCastWithId, value.GetType().FullName, type.FullName, id));

        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateBadDerivationException"]/*' />
        protected Exception CreateBadDerivationException(string xsdDerived, string nsDerived, string xsdBase, string nsBase, string clrDerived, string clrBase) {
            return new InvalidOperationException(Res.GetString(Res.XmlSerializableBadDerivation, xsdDerived, nsDerived, xsdBase, nsBase, clrDerived, clrBase));
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CreateMissingIXmlSerializableType"]/*' />
        protected Exception CreateMissingIXmlSerializableType(string name, string ns, string clrType) {
            return new InvalidOperationException(Res.GetString(Res.XmlSerializableMissingClrType, name, ns, typeof(XmlIncludeAttribute).Name, clrType));
            //XmlSerializableMissingClrType= Type '{0}' from namespace '{1}' doesnot have corresponding IXmlSerializable type. Please consider adding {2} to '{3}'.
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.EnsureArrayIndex"]/*' />
        protected Array EnsureArrayIndex(Array a, int index, Type elementType) {
            if (a == null) return Array.CreateInstance(elementType, 32);
            if (index < a.Length) return a;
            Array b = Array.CreateInstance(elementType, a.Length * 2);
            Array.Copy(a, b, index);
            return b;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ShrinkArray"]/*' />
        protected Array ShrinkArray(Array a, int length, Type elementType, bool isNullable) {
            if (a == null) {
                if (isNullable) return null;
                return Array.CreateInstance(elementType, 0);
            }
            if (a.Length == length) return a;
            Array b = Array.CreateInstance(elementType, length);
            Array.Copy(a, b, length);
            return b;
        } 

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadString"]/*' />
        protected string ReadString(string value) {
            return ReadString(value, false);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadString1"]/*' />
        protected string ReadString(string value, bool trim) {
            string str = r.ReadString();
            if (str != null && trim)
                str = str.Trim();
            if (value == null || value.Length == 0)
                return str;
            return value + str;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadSerializable"]/*' />
        protected IXmlSerializable ReadSerializable(IXmlSerializable serializable) {
            return ReadSerializable(serializable, false);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadSerializable"]/*' />
        protected IXmlSerializable ReadSerializable(IXmlSerializable serializable, bool wrappedAny)
        {
            string name = null;
            string ns = null;

            if (wrappedAny) {
                name = r.LocalName;
                ns = r.NamespaceURI;
                r.Read();
                r.MoveToContent();
            }
            serializable.ReadXml(r);

            if (wrappedAny) {
                while (r.NodeType == XmlNodeType.Whitespace) r.Skip();
                if (r.NodeType == XmlNodeType.None) r.Skip();
                if (r.NodeType == XmlNodeType.EndElement && r.LocalName == name && r.NamespaceURI == ns) {
                    Reader.Read();
                }
            }
            return serializable;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadReference"]/*' />
        protected bool ReadReference(out string fixupReference) {
            string href = soap12 ? r.GetAttribute("ref", Soap12.Encoding) : r.GetAttribute("href");
            if (href == null) {
                fixupReference = null;
                return false;
            }
            if (!soap12) {
                // soap 1.1 href starts with '#'; soap 1.2 ref does not
                if (!href.StartsWith("#", StringComparison.Ordinal)) throw new InvalidOperationException(Res.GetString(Res.XmlMissingHref, href));
                fixupReference = href.Substring(1);
            }
            else
                fixupReference = href;

            if (r.IsEmptyElement) {
                r.Skip();
            }
            else {
                r.ReadStartElement();
                ReadEndElement();
            }
            return true;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.AddTarget"]/*' />
        protected void AddTarget(string id, object o) {
            if (id == null) {
                if (targetsWithoutIds == null) 
                    targetsWithoutIds = new ArrayList();
                if (o != null) 
                    targetsWithoutIds.Add(o);
            }
            else {
                if (targets == null) targets = new Hashtable();
                if (!targets.Contains(id))
                    targets.Add(id, o);
            }
        }



        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.AddFixup"]/*' />
        protected void AddFixup(Fixup fixup) {
            if (fixups == null) fixups = new ArrayList();
            fixups.Add(fixup);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.AddFixup2"]/*' />
        protected void AddFixup(CollectionFixup fixup) {
            if (collectionFixups == null) collectionFixups = new ArrayList();
            collectionFixups.Add(fixup);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.GetTarget"]/*' />
        protected object GetTarget(string id) {
            object target = targets != null ? targets[id] : null;
            if (target == null) {
                throw new InvalidOperationException(Res.GetString(Res.XmlInvalidHref, id));
            }
            Referenced(target);
            return target;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.Referenced"]/*' />
        protected void Referenced(object o) {
            if (o == null) return;
            if (referencedTargets == null) referencedTargets = new Hashtable();
            referencedTargets[o] = o;
        }

        void HandleUnreferencedObjects() {
            if (targets != null) {
                foreach (DictionaryEntry target in targets) {
                    if (referencedTargets == null || !referencedTargets.Contains(target.Value)) {
                        UnreferencedObject((string)target.Key, target.Value);
                    }
                }
            }
            if (targetsWithoutIds != null) {
                foreach (object o in targetsWithoutIds) {
                    if (referencedTargets == null || !referencedTargets.Contains(o)) {
                        UnreferencedObject(null, o);
                    }
                }
            }
        }

        void DoFixups() {
            if (fixups == null) return;
            for (int i = 0; i < fixups.Count; i++) {
                Fixup fixup = (Fixup)fixups[i];
                fixup.Callback(fixup);
            }

            if (collectionFixups == null) return;
            for (int i = 0; i < collectionFixups.Count; i++) {
                CollectionFixup collectionFixup = (CollectionFixup)collectionFixups[i];
                collectionFixup.Callback(collectionFixup.Collection, collectionFixup.CollectionItems);
            }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.FixupArrayRefs"]/*' />
        protected void FixupArrayRefs(object fixup) {
            Fixup f = (Fixup)fixup;
            Array array = (Array)f.Source;
            for (int i = 0; i < array.Length; i++) {
                string id = f.Ids[i];
                if (id == null) continue;
                object o = GetTarget(id);
                try {
                    array.SetValue(o, i);
                }
                catch (InvalidCastException) {
                    throw new InvalidOperationException(Res.GetString(Res.XmlInvalidArrayRef, id, o.GetType().FullName, i.ToString(CultureInfo.InvariantCulture)));
                }
            }
        }

        object ReadArray(string typeName, string typeNs) {
            SoapArrayInfo arrayInfo;
            Type fallbackElementType = null;
            if (soap12) {
                string itemType = r.GetAttribute(itemTypeID, soap12NsID);
                string arraySize = r.GetAttribute(arraySizeID, soap12NsID);
                Type arrayType = (Type)types[new XmlQualifiedName(typeName, typeNs)];
                // no indication that this is an array?
                if (itemType == null && arraySize == null && (arrayType == null || !arrayType.IsArray))
                    return null;

                arrayInfo = ParseSoap12ArrayType(itemType, arraySize);
                if (arrayType != null)
                    fallbackElementType = TypeScope.GetArrayElementType(arrayType, null);
            }
            else {
                string arrayType = r.GetAttribute(arrayTypeID, soapNsID);
                if (arrayType == null) 
                    return null;

                arrayInfo = ParseArrayType(arrayType);
            }

            if (arrayInfo.dimensions != 1) throw new InvalidOperationException(Res.GetString(Res.XmlInvalidArrayDimentions, CurrentTag()));

            // NOTE: don't use the array size that is specified since an evil client might pass
            // a number larger than the actual number of items in an attempt to harm the server.

            XmlQualifiedName qname;
            bool isPrimitive;
            Type elementType = null;
            XmlQualifiedName urTypeName = new XmlQualifiedName(urTypeID, schemaNsID);
            if (arrayInfo.qname.Length > 0) {
                qname = ToXmlQualifiedName(arrayInfo.qname, false);
                elementType = (Type)types[qname];
            }
            else
                qname = urTypeName;
            
            // try again if the best we could come up with was object
            if (soap12 && elementType == typeof(object))
                elementType = null;
            
            if (elementType == null) {
                if (!soap12) {
                    elementType = GetPrimitiveType(qname, true);
                    isPrimitive = true;
                }
                else {
                    // try it as a primitive
                    if (qname != urTypeName)
                        elementType = GetPrimitiveType(qname, false);
                    if (elementType != null) {
                        isPrimitive = true;
                    }
                    else {
                        // still nothing: go with fallback type or object
                        if (fallbackElementType == null) {
                            elementType = typeof(object);
                            isPrimitive = false;
                        }
                        else {
                            elementType = fallbackElementType;
                            XmlQualifiedName newQname = (XmlQualifiedName)typesReverse[elementType];
                            if (newQname == null) {
                                newQname = XmlSerializationWriter.GetPrimitiveTypeNameInternal(elementType);
                                isPrimitive = true;
                            }
                            else
                                isPrimitive = elementType.IsPrimitive;
                            if (newQname != null) qname = newQname;
                        }
                    }
                }
            }
            else
                isPrimitive = elementType.IsPrimitive;

            if (!soap12 && arrayInfo.jaggedDimensions > 0) {
                for (int i = 0; i < arrayInfo.jaggedDimensions; i++)
                    elementType = elementType.MakeArrayType();
            }

            if (r.IsEmptyElement) {
                r.Skip();
                return Array.CreateInstance(elementType, 0);
            }

            r.ReadStartElement();
            r.MoveToContent();

            int arrayLength = 0;
            Array array = null;

            if (elementType.IsValueType) {
                if (!isPrimitive && !elementType.IsEnum) {
                    throw new NotSupportedException(Res.GetString(Res.XmlRpcArrayOfValueTypes, elementType.FullName));
                }
                // 

                int whileIterations = 0;
                int readerCount = ReaderCount;
                while (r.NodeType != XmlNodeType.EndElement) {
                    array = EnsureArrayIndex(array, arrayLength, elementType);
                    array.SetValue(ReadReferencedElement(qname.Name, qname.Namespace), arrayLength);
                    arrayLength++;
                    r.MoveToContent();
                    CheckReaderCount(ref whileIterations, ref readerCount);
                }
                array = ShrinkArray(array, arrayLength, elementType, false);
            }
            else {
                string type;
                string typens;
                string[] ids = null;
                int idsLength = 0;

                int whileIterations = 0;
                int readerCount = ReaderCount;
                while (r.NodeType != XmlNodeType.EndElement) {
                    array = EnsureArrayIndex(array, arrayLength, elementType);
                    ids = (string[])EnsureArrayIndex(ids, idsLength, typeof(string));
                    // 
                    if (r.NamespaceURI.Length != 0){
                        type = r.LocalName;
                        if ((object)r.NamespaceURI == (object)soapNsID)
                            typens = XmlSchema.Namespace;
                        else
                            typens = r.NamespaceURI;
                    }
                    else {
                        type = qname.Name;
                        typens = qname.Namespace;                        
                    }
                    array.SetValue(ReadReferencingElement(type, typens, out ids[idsLength]), arrayLength);
                    arrayLength++;
                    idsLength++;
                    // 
                    r.MoveToContent();
                    CheckReaderCount(ref whileIterations, ref readerCount);
                }

                // special case for soap 1.2: try to get a better fit than object[] when no metadata is known
                // this applies in the doc/enc/bare case
                if (soap12 && elementType == typeof(object)) {
                    Type itemType = null;
                    for (int i = 0; i < arrayLength; i++) {
                        object currItem = array.GetValue(i);
                        if (currItem != null) {
                            Type currItemType = currItem.GetType();
                            if (currItemType.IsValueType) {
                                itemType = null;
                                break;
                            }
                            if (itemType == null || currItemType.IsAssignableFrom(itemType)) {
                                itemType = currItemType;
                            }
                            else if (!itemType.IsAssignableFrom(currItemType)) {
                                itemType = null;
                                break;
                            }
                        }
                    }
                    if (itemType != null)
                        elementType = itemType;
                }
                ids = (string[])ShrinkArray(ids, idsLength, typeof(string), false);
                array = ShrinkArray(array, arrayLength, elementType, false);
                Fixup fixupArray = new Fixup(array, new XmlSerializationFixupCallback(this.FixupArrayRefs), ids);
                AddFixup(fixupArray);
            }

            // 

            ReadEndElement();
            return array;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.InitCallbacks"]/*' />
        protected abstract void InitCallbacks();

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadReferencedElements"]/*' />
        protected void ReadReferencedElements() {
            r.MoveToContent();
            string dummy;
            int whileIterations = 0;
            int readerCount = ReaderCount;
            while (r.NodeType != XmlNodeType.EndElement && r.NodeType != XmlNodeType.None) {
                ReadReferencingElement(null, null, true, out dummy);
                r.MoveToContent();
                CheckReaderCount(ref whileIterations, ref readerCount);
            }
            DoFixups();

            HandleUnreferencedObjects();
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadReferencedElement"]/*' />
        protected object ReadReferencedElement() {
            return ReadReferencedElement(null, null);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadReferencedElement1"]/*' />
        protected object ReadReferencedElement(string name, string ns) {
            string dummy;
            return ReadReferencingElement(name, ns, out dummy);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadReferencingElement"]/*' />
        protected object ReadReferencingElement(out string fixupReference) {
            return ReadReferencingElement(null, null, out fixupReference);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadReferencingElement1"]/*' />
        protected object ReadReferencingElement(string name, string ns, out string fixupReference) {
            return ReadReferencingElement(name, ns, false, out fixupReference);
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadReferencingElement2"]/*' />
        protected object ReadReferencingElement(string name, string ns, bool elementCanBeType, out string fixupReference) {
            object o = null;

            if (callbacks == null) {
                callbacks = new Hashtable();
                types = new Hashtable();
                XmlQualifiedName urType = new XmlQualifiedName(urTypeID, r.NameTable.Add(XmlSchema.Namespace));
                types.Add(urType, typeof(object));
                typesReverse = new Hashtable();
                typesReverse.Add(typeof(object), urType);
                InitCallbacks();
            }

            r.MoveToContent();

            if (ReadReference(out fixupReference)) return null;

            if (ReadNull()) return null;

            string id = soap12 ? r.GetAttribute("id", Soap12.Encoding) : r.GetAttribute("id", null);

            if ((o = ReadArray(name, ns)) == null) {
                XmlQualifiedName typeId = GetXsiType();
                if (typeId == null) {
                    if (name == null)
                        typeId = new XmlQualifiedName(r.NameTable.Add(r.LocalName), r.NameTable.Add(r.NamespaceURI));
                    else
                        typeId = new XmlQualifiedName(r.NameTable.Add(name), r.NameTable.Add(ns));
                }
                XmlSerializationReadCallback callback = (XmlSerializationReadCallback)callbacks[typeId];
                if (callback != null) {
                    o = callback();
                }
                else
                    o = ReadTypedPrimitive(typeId, elementCanBeType);
            }

            AddTarget(id, o);

            return o;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.AddReadCallback"]/*' />
        protected void AddReadCallback(string name, string ns, Type type, XmlSerializationReadCallback read) {
            XmlQualifiedName typeName = new XmlQualifiedName(r.NameTable.Add(name), r.NameTable.Add(ns));
            callbacks[typeName] = read;
            types[typeName] = type;
            typesReverse[type] = typeName;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.ReadEndElement"]/*' />
        protected void ReadEndElement() {
            while (r.NodeType == XmlNodeType.Whitespace) r.Skip();
            if (r.NodeType == XmlNodeType.None) r.Skip();
            else r.ReadEndElement();
        }

        object ReadXmlNodes(bool elementCanBeType) {

            ArrayList xmlNodeList = new ArrayList();
            string elemLocalName = Reader.LocalName;
            string elemNs = Reader.NamespaceURI;
            string elemName = Reader.Name;
            string xsiTypeName = null;
            string xsiTypeNs = null;
            int skippableNodeCount = 0;
            int lineNumber = -1, linePosition=-1;
            XmlNode unknownNode = null;
            if(Reader.NodeType == XmlNodeType.Attribute){
                XmlAttribute attr = Document.CreateAttribute(elemName, elemNs);
                attr.Value = Reader.Value;
                unknownNode = attr;
            }
            else
                unknownNode = Document.CreateElement(elemName, elemNs);
            GetCurrentPosition(out lineNumber, out linePosition);
            XmlElement unknownElement = unknownNode as XmlElement;

            while (Reader.MoveToNextAttribute()) {
                if (IsXmlnsAttribute(Reader.Name) || (Reader.Name == "id" && (!soap12 || Reader.NamespaceURI == Soap12.Encoding)))
                    skippableNodeCount++;
                if ( (object)Reader.LocalName == (object)typeID &&
                     ( (object)Reader.NamespaceURI == (object)instanceNsID ||
                       (object)Reader.NamespaceURI == (object)instanceNs2000ID ||
                       (object)Reader.NamespaceURI == (object)instanceNs1999ID
                     )
                   ){
                    string value = Reader.Value;
                    int colon = value.LastIndexOf(':');
                    xsiTypeName = (colon >= 0) ? value.Substring(colon+1) : value;
                    xsiTypeNs = Reader.LookupNamespace((colon >= 0) ? value.Substring(0, colon) : "");
                }
                XmlAttribute xmlAttribute = (XmlAttribute)Document.ReadNode(r);
                xmlNodeList.Add(xmlAttribute);
                if (unknownElement != null) unknownElement.SetAttributeNode(xmlAttribute);
            }

            // If the node is referenced (or in case of paramStyle = bare) and if xsi:type is not
            // specified then the element name is used as the type name. Reveal this to the user
            // by adding an extra attribute node "xsi:type" with value as the element name.
            if(elementCanBeType && xsiTypeName == null){
                xsiTypeName = elemLocalName;
                xsiTypeNs = elemNs;
                XmlAttribute xsiTypeAttribute = Document.CreateAttribute(typeID, instanceNsID);
                xsiTypeAttribute.Value = elemName;
                xmlNodeList.Add(xsiTypeAttribute);
            }
            if( xsiTypeName == Soap.UrType &&
                ( (object)xsiTypeNs == (object)schemaNsID ||
                  (object)xsiTypeNs == (object)schemaNs1999ID ||
                  (object)xsiTypeNs == (object)schemaNs2000ID
                )
               )
                skippableNodeCount++;
            
            
            Reader.MoveToElement();
            if (Reader.IsEmptyElement) {
                Reader.Skip();
            }
            else {
                Reader.ReadStartElement();
                Reader.MoveToContent();
                int whileIterations = 0;
                int readerCount = ReaderCount;
                while (Reader.NodeType != System.Xml.XmlNodeType.EndElement) {
                    XmlNode xmlNode = Document.ReadNode(r);
                    xmlNodeList.Add(xmlNode);
                    if (unknownElement != null) unknownElement.AppendChild(xmlNode);
                    Reader.MoveToContent();
                    CheckReaderCount(ref whileIterations, ref readerCount);
                }
                ReadEndElement();

            }


            if(xmlNodeList.Count <= skippableNodeCount)
                return new object();

            XmlNode[] childNodes =  (XmlNode[])xmlNodeList.ToArray(typeof(XmlNode));

            UnknownNode(unknownNode, null, null);
            return childNodes;
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReader.CheckReaderCount"]/*' />
        protected void CheckReaderCount(ref int whileIterations, ref int readerCount)
        {
            if (checkDeserializeAdvances)
            {
                whileIterations++;
                if ((whileIterations & 0x80) == 0x80)
                {
                    if (readerCount == ReaderCount)
                        throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorReaderAdvance));
                    readerCount = ReaderCount;
                }
            }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="Fixup"]/*' />
        ///<internalonly/>
        protected class Fixup {
            XmlSerializationFixupCallback callback;
            object source;
            string[] ids;

            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="Fixup.Fixup1"]/*' />
            public Fixup(object o, XmlSerializationFixupCallback callback, int count) 
                : this (o, callback, new string[count]) {
            }

            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="Fixup.Fixup2"]/*' />
            public Fixup(object o, XmlSerializationFixupCallback callback, string[] ids) {
                this.callback = callback;
                this.Source = o;
                this.ids = ids;
            }

            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="Fixup.Callback"]/*' />
            public XmlSerializationFixupCallback Callback {
                get { return callback; }
            }

            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="Fixup.Source"]/*' />
            public object Source {
                get { return source; }
                set { source = value; }
            }

            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="Fixup.Ids"]/*' />
            public string[] Ids {
                get { return ids; }
            }
        }

        /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="CollectionFixup"]/*' />
        protected class CollectionFixup {
            XmlSerializationCollectionFixupCallback callback;
            object collection;
            object collectionItems;

            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="CollectionFixup.CollectionFixup"]/*' />
            public CollectionFixup(object collection, XmlSerializationCollectionFixupCallback callback, object collectionItems) {
                this.callback = callback;
                this.collection = collection;
                this.collectionItems = collectionItems;
            }

            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="CollectionFixup.Callback"]/*' />
            public XmlSerializationCollectionFixupCallback Callback {
                get { return callback; }
            }

            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="CollectionFixup.Collection"]/*' />
            public object Collection {
                get { return collection; }
            }

            /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="CollectionFixup.CollectionItems"]/*' />
            public object CollectionItems {
                get { return collectionItems; }
            }
        }
    }

    /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationFixupCallback"]/*' />
    ///<internalonly/>
    public delegate void XmlSerializationFixupCallback(object fixup);


    /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationCollectionFixupCallback"]/*' />
    ///<internalonly/>
    public delegate void XmlSerializationCollectionFixupCallback(object collection, object collectionItems);

    /// <include file='doc\XmlSerializationReader.uex' path='docs/doc[@for="XmlSerializationReadCallback"]/*' />
    ///<internalonly/>
    public delegate object XmlSerializationReadCallback();

    internal class XmlSerializationReaderCodeGen : XmlSerializationCodeGen {
        Hashtable idNames = new Hashtable();
        Hashtable enums;
        Hashtable createMethods = new Hashtable();
        int nextCreateMethodNumber = 0;
        int nextIdNumber = 0;
        int nextWhileLoopIndex = 0;

        internal Hashtable Enums {
            get {
                if (enums == null) {
                    enums = new Hashtable();
                }
                return enums;
            }
        }

        class CreateCollectionInfo {
            string name;
            TypeDesc td;

            internal CreateCollectionInfo(string name, TypeDesc td) {
                this.name = name;
                this.td = td;
            }
            internal string Name {
                get { return name; }
            }

            internal TypeDesc TypeDesc {
                get { return td; }
            }
        }
        class Member {
            string source;
            string arrayName;
            string arraySource;
            string choiceArrayName;
            string choiceSource;
            string choiceArraySource;
            MemberMapping mapping;
            bool isArray;
            bool isList;
            bool isNullable;
            bool multiRef;
            int fixupIndex = -1;
            string paramsReadSource;
            string checkSpecifiedSource;

            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arrayName, int i, MemberMapping mapping) 
                : this(outerClass, source, null, arrayName, i, mapping, false, null) {
            }
            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arrayName, int i, MemberMapping mapping, string choiceSource)
                : this(outerClass, source, null, arrayName, i, mapping, false, choiceSource) {
            }
            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arraySource, string arrayName, int i, MemberMapping mapping) 
                : this (outerClass, source, arraySource, arrayName, i, mapping, false, null) { 
            }
            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arraySource, string arrayName, int i, MemberMapping mapping, string choiceSource) 
                : this (outerClass, source, arraySource, arrayName, i, mapping, false, choiceSource) { 
            }
            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arrayName, int i, MemberMapping mapping, bool multiRef)
                : this(outerClass, source, null, arrayName, i, mapping, multiRef, null) {
            }
            internal Member(XmlSerializationReaderCodeGen outerClass, string source, string arraySource, string arrayName, int i, MemberMapping mapping, bool multiRef, string choiceSource) {
                this.source = source;
                this.arrayName = arrayName + "_" + i.ToString(CultureInfo.InvariantCulture);
                this.choiceArrayName = "choice_" + this.arrayName;
                this.choiceSource = choiceSource;
                ElementAccessor[] elements = mapping.Elements;

                if (mapping.TypeDesc.IsArrayLike) {
                    if (arraySource != null)
                        this.arraySource = arraySource;
                    else
                        this.arraySource = outerClass.GetArraySource(mapping.TypeDesc, this.arrayName, multiRef);
                    isArray = mapping.TypeDesc.IsArray;
                    isList = !isArray;
                    if (mapping.ChoiceIdentifier != null) {
                        this.choiceArraySource = outerClass.GetArraySource(mapping.TypeDesc, this.choiceArrayName, multiRef);

                        string a = choiceArrayName;
                        string c = "c" + a;
                        bool choiceUseReflection = mapping.ChoiceIdentifier.Mapping.TypeDesc.UseReflection;
                        string choiceTypeFullName = mapping.ChoiceIdentifier.Mapping.TypeDesc.CSharpName;
                        string castString = choiceUseReflection?"":"(" + choiceTypeFullName + "[])";
                        
                        string init = a + " = " + castString +
                            "EnsureArrayIndex(" + a + ", " + c + ", " + outerClass.RaCodeGen.GetStringForTypeof(choiceTypeFullName, choiceUseReflection) + ");";
                        this.choiceArraySource = init + outerClass.RaCodeGen.GetStringForArrayMember(a,  c + "++", mapping.ChoiceIdentifier.Mapping.TypeDesc);
                    }
                    else {
                        this.choiceArraySource = this.choiceSource;
                    }
                }
                else {
                    this.arraySource = arraySource == null ? source : arraySource;
                    this.choiceArraySource = this.choiceSource;
                }
                this.mapping = mapping;
            }

            internal MemberMapping Mapping {
                get { return mapping; }
            }

            internal string Source {
                get { return source; }
            }

            internal string ArrayName {
                get { return arrayName; }
            }

            internal string ArraySource {
                get { return arraySource; }
            }

            internal bool IsList {
                get { return isList; }
            }

            internal bool IsArrayLike {
                get { return (isArray || isList); }
            }

            internal bool IsNullable {
                get { return isNullable; }
                set { isNullable = value; }
            }

            internal bool MultiRef {
                get { return multiRef; }
                set { multiRef = value; }
            }

            internal int FixupIndex {
                get { return fixupIndex; }
                set { fixupIndex = value; }
            }

            internal string ParamsReadSource {
                get { return paramsReadSource; }
                set { paramsReadSource = value; }
            }
            
            internal string CheckSpecifiedSource {
                get { return checkSpecifiedSource; }
                set { checkSpecifiedSource = value; }
            }

            internal string ChoiceSource {
                get { return choiceSource; }
            }
            internal string ChoiceArrayName {
                get { return choiceArrayName; }
            }
            internal string ChoiceArraySource {
                get { return choiceArraySource; }
            }
        }

        internal XmlSerializationReaderCodeGen(IndentedWriter writer, TypeScope[] scopes, string access, string className) : base(writer, scopes, access, className) {
        }

        internal void GenerateBegin() {
            Writer.Write(Access);
            Writer.Write(" class ");
            Writer.Write(ClassName);
            Writer.Write(" : ");
            Writer.Write(typeof(XmlSerializationReader).FullName);
            Writer.WriteLine(" {");
            Writer.Indent++;
            foreach (TypeScope scope in Scopes) {
                foreach (TypeMapping mapping in scope.TypeMappings) {
                    if (mapping is StructMapping || mapping is EnumMapping || mapping is NullableMapping) 
                        MethodNames.Add(mapping, NextMethodName(mapping.TypeDesc.Name));
                }
                RaCodeGen.WriteReflectionInit(scope);
            }
            // pre-generate read methods only for the encoded soap
            foreach (TypeScope scope in Scopes) {
                foreach (TypeMapping mapping in scope.TypeMappings) {
                    if (!mapping.IsSoap)
                        continue;
                    if (mapping is StructMapping)
                        WriteStructMethod((StructMapping)mapping);
                    else if (mapping is EnumMapping)
                        WriteEnumMethod((EnumMapping)mapping);
                    else if (mapping is NullableMapping) {
                        WriteNullableMethod((NullableMapping)mapping);
                    }
                }
            }
        }

        internal override void GenerateMethod(TypeMapping mapping) {
            if (GeneratedMethods.Contains(mapping))
                return;

            GeneratedMethods[mapping] = mapping;
            if (mapping is StructMapping) {
                WriteStructMethod((StructMapping)mapping);
            }
            else if (mapping is EnumMapping) {
                WriteEnumMethod((EnumMapping)mapping);
            }
            else if (mapping is NullableMapping) {
                WriteNullableMethod((NullableMapping)mapping);
            }
        }

        internal void GenerateEnd() {
            GenerateEnd(new string[0], new XmlMapping[0], new Type[0]);
        }
        internal void GenerateEnd(string[] methods, XmlMapping[] xmlMappings, Type[] types) {
            GenerateReferencedMethods();
            GenerateInitCallbacksMethod();

            foreach (CreateCollectionInfo c in createMethods.Values) {
                WriteCreateCollectionMethod(c);
            }

            Writer.WriteLine();
            foreach (string idName in idNames.Values) {
                Writer.Write("string ");
                Writer.Write(idName);
                Writer.WriteLine(";");
            }                

            Writer.WriteLine();
            Writer.WriteLine("protected override void InitIDs() {");
            Writer.Indent++;
            foreach (string id in idNames.Keys) {
                // 
                string idName = (string)idNames[id];
                Writer.Write(idName);
                Writer.Write(" = Reader.NameTable.Add(");
                WriteQuotedCSharpString(id);
                Writer.WriteLine(");");
            }
            Writer.Indent--;
            Writer.WriteLine("}");

            Writer.Indent--;
            Writer.WriteLine("}");
        }

        internal string GenerateElement(XmlMapping xmlMapping) {
            if (!xmlMapping.IsReadable)
                return null;
            if (!xmlMapping.GenerateSerializer) 
                throw new ArgumentException(Res.GetString(Res.XmlInternalError), "xmlMapping");
            if (xmlMapping is XmlTypeMapping)
                return GenerateTypeElement((XmlTypeMapping)xmlMapping);
            else if (xmlMapping is XmlMembersMapping)
                return GenerateMembersElement((XmlMembersMapping)xmlMapping);
            else
                throw new ArgumentException(Res.GetString(Res.XmlInternalError), "xmlMapping");
        }

        void WriteIsStartTag(string name, string ns) {
            Writer.Write("if (Reader.IsStartElement(");
            WriteID(name);
            Writer.Write(", ");
            WriteID(ns);
            Writer.WriteLine(")) {");
            Writer.Indent++;
        }

        void WriteUnknownNode(string func, string node, ElementAccessor e, bool anyIfs) {
            if (anyIfs) {
                Writer.WriteLine("else {");
                Writer.Indent++;
            }
            Writer.Write(func);
            Writer.Write("(");
            Writer.Write(node);
            if (e != null) {
                Writer.Write(", ");
                string expectedElement = e.Form == XmlSchemaForm.Qualified ? e.Namespace : "";
                expectedElement += ":";
                expectedElement += e.Name;
                ReflectionAwareCodeGen.WriteQuotedCSharpString(Writer, expectedElement);
            }
            Writer.WriteLine(");");
            if (anyIfs) {
                Writer.Indent--;
                Writer.WriteLine("}");
            }
        }

        void GenerateInitCallbacksMethod() {
            Writer.WriteLine();
            Writer.WriteLine("protected override void InitCallbacks() {");
            Writer.Indent++;

            string dummyArrayMethodName = NextMethodName("Array");
            bool needDummyArrayMethod = false;
            foreach (TypeScope scope in Scopes) {
                foreach (TypeMapping mapping in scope.TypeMappings) {
                    if (mapping.IsSoap && 
                        (mapping is StructMapping || mapping is EnumMapping || mapping is ArrayMapping || mapping is NullableMapping) &&
                        !mapping.TypeDesc.IsRoot) {

                        string methodName;
                        if (mapping is ArrayMapping) {
                            methodName = dummyArrayMethodName;
                            needDummyArrayMethod = true;
                        }
                        else
                            methodName = (string)MethodNames[mapping];

                        Writer.Write("AddReadCallback(");
                        WriteID(mapping.TypeName);
                        Writer.Write(", ");
                        WriteID(mapping.Namespace);
                        Writer.Write(", ");
                        Writer.Write(RaCodeGen.GetStringForTypeof(mapping.TypeDesc.CSharpName,mapping.TypeDesc.UseReflection));
                        Writer.Write(", new ");
                        Writer.Write(typeof(XmlSerializationReadCallback).FullName);
                        Writer.Write("(this.");
                        Writer.Write(methodName);
                        Writer.WriteLine("));");
                    }
                }
            }

            Writer.Indent--;
            Writer.WriteLine("}");

            if (needDummyArrayMethod) {
                Writer.WriteLine();
                Writer.Write("object ");
                Writer.Write(dummyArrayMethodName);
                Writer.WriteLine("() {");
                Writer.Indent++;
                Writer.WriteLine("// dummy array method");
                Writer.WriteLine("UnknownNode(null);");
                Writer.WriteLine("return null;");
                Writer.Indent--;
                Writer.WriteLine("}");
            }
        }

       
        string GenerateMembersElement(XmlMembersMapping xmlMembersMapping) {
            if (xmlMembersMapping.Accessor.IsSoap)
                return GenerateEncodedMembersElement(xmlMembersMapping);
            else
                return GenerateLiteralMembersElement(xmlMembersMapping);
        }

        string GetChoiceIdentifierSource(MemberMapping[] mappings, MemberMapping member) {
            string choiceSource = null;
            if (member.ChoiceIdentifier != null) {
                for (int j = 0; j < mappings.Length; j++) {
                    if (mappings[j].Name == member.ChoiceIdentifier.MemberName) {
                        choiceSource = "p[" + j.ToString(CultureInfo.InvariantCulture) + "]";
                        break;
                    }
                }
                #if DEBUG
                    // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                    if (choiceSource == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "Can not find " + member.ChoiceIdentifier.MemberName + " in the members mapping."));
                #endif

            }
            return choiceSource;
        }

        string GetChoiceIdentifierSource(MemberMapping mapping, string parent, TypeDesc parentTypeDesc) {
            if (mapping.ChoiceIdentifier == null) return "";
            CodeIdentifier.CheckValidIdentifier(mapping.ChoiceIdentifier.MemberName);
            return RaCodeGen.GetStringForMember(parent,  mapping.ChoiceIdentifier.MemberName, parentTypeDesc);
        }

        string GenerateLiteralMembersElement(XmlMembersMapping xmlMembersMapping) {
            ElementAccessor element = xmlMembersMapping.Accessor;
            MemberMapping[] mappings = ((MembersMapping)element.Mapping).Members;
            bool hasWrapperElement = ((MembersMapping)element.Mapping).HasWrapperElement;
            string methodName = NextMethodName(element.Name);
            Writer.WriteLine();
            Writer.Write("public object[] ");
            Writer.Write(methodName);
            Writer.WriteLine("() {");
            Writer.Indent++;
            Writer.WriteLine("Reader.MoveToContent();");

            Writer.Write("object[] p = new object[");
            Writer.Write(mappings.Length.ToString(CultureInfo.InvariantCulture));
            Writer.WriteLine("];");
            InitializeValueTypes("p", mappings);

            int wrapperLoopIndex = 0;
            if (hasWrapperElement) {
                wrapperLoopIndex = WriteWhileNotLoopStart();
                Writer.Indent++;
                WriteIsStartTag(element.Name, element.Form == XmlSchemaForm.Qualified ? element.Namespace : "");
            }

            Member anyText = null;
            Member anyElement = null;
            Member anyAttribute = null; 

            ArrayList membersList = new ArrayList();
            ArrayList textOrArrayMembersList = new ArrayList();
            ArrayList attributeMembersList = new ArrayList();
            
            for (int i = 0; i < mappings.Length; i++) {
                MemberMapping mapping = mappings[i];
                string source = "p[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                string arraySource = source;
                if (mapping.Xmlns != null) {
                    arraySource = "((" + mapping.TypeDesc.CSharpName + ")" + source + ")";
                }
                string choiceSource = GetChoiceIdentifierSource(mappings, mapping);
                Member member = new Member(this, source, arraySource, "a", i, mapping, choiceSource);
                Member anyMember = new Member(this, source, null, "a", i, mapping, choiceSource);
                if (!mapping.IsSequence)
                    member.ParamsReadSource = "paramsRead[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                if (mapping.CheckSpecified == SpecifiedAccessor.ReadWrite) {
                    string nameSpecified = mapping.Name + "Specified";
                    for (int j = 0; j < mappings.Length; j++) {
                        if (mappings[j].Name == nameSpecified) {
                            member.CheckSpecifiedSource = "p[" + j.ToString(CultureInfo.InvariantCulture) + "]";
                            break;
                        }
                    }
                }
                bool foundAnyElement = false;
                if (mapping.Text != null) anyText = anyMember;
                if (mapping.Attribute != null && mapping.Attribute.Any)
                    anyAttribute = anyMember;
                if (mapping.Attribute != null || mapping.Xmlns != null)
                    attributeMembersList.Add(member);
                else if (mapping.Text != null)
                    textOrArrayMembersList.Add(member);

                if (!mapping.IsSequence) {
                    for (int j = 0; j < mapping.Elements.Length; j++) {
                        if (mapping.Elements[j].Any && mapping.Elements[j].Name.Length == 0) {
                            anyElement = anyMember;
                            if (mapping.Attribute == null && mapping.Text == null)
                                textOrArrayMembersList.Add(anyMember);
                            foundAnyElement = true;
                            break;
                        }
                    }
                }
                if (mapping.Attribute != null || mapping.Text != null || foundAnyElement)
                    membersList.Add(anyMember);
                else if (mapping.TypeDesc.IsArrayLike && !(mapping.Elements.Length == 1 && mapping.Elements[0].Mapping is ArrayMapping)) {
                    membersList.Add(anyMember);
                    textOrArrayMembersList.Add(anyMember);
                }
                else {
                    if (mapping.TypeDesc.IsArrayLike && !mapping.TypeDesc.IsArray)
                        member.ParamsReadSource = null; // collection
                    membersList.Add(member);
                }
            }
            Member[] members = (Member[]) membersList.ToArray(typeof(Member));
            Member[] textOrArrayMembers = (Member[]) textOrArrayMembersList.ToArray(typeof(Member));

            if (members.Length > 0 && members[0].Mapping.IsReturnValue) Writer.WriteLine("IsReturnValue = true;");
            
            WriteParamsRead(mappings.Length);

            if (attributeMembersList.Count > 0) {
                Member[] attributeMembers = (Member[]) attributeMembersList.ToArray(typeof(Member));
                WriteMemberBegin(attributeMembers);
                WriteAttributes(attributeMembers, anyAttribute, "UnknownNode", "(object)p");
                WriteMemberEnd(attributeMembers);
                Writer.WriteLine("Reader.MoveToElement();");
            }

            WriteMemberBegin(textOrArrayMembers);

            if (hasWrapperElement) {
                Writer.WriteLine("if (Reader.IsEmptyElement) { Reader.Skip(); Reader.MoveToContent(); continue; }");
                Writer.WriteLine("Reader.ReadStartElement();");
            }
            if (IsSequence(members)) {
                Writer.WriteLine("int state = 0;");
            }
            int loopIndex = WriteWhileNotLoopStart();
            Writer.Indent++;

            string unknownNode = "UnknownNode((object)p, " + ExpectedElements(members) + ");";
            WriteMemberElements(members, unknownNode, unknownNode, anyElement, anyText, null);

            Writer.WriteLine("Reader.MoveToContent();");
            WriteWhileLoopEnd(loopIndex);

            WriteMemberEnd(textOrArrayMembers);

            if (hasWrapperElement) {
                Writer.WriteLine("ReadEndElement();");

                Writer.Indent--;
                Writer.WriteLine("}");

                WriteUnknownNode("UnknownNode", "null", element, true);

                Writer.WriteLine("Reader.MoveToContent();");
                WriteWhileLoopEnd(wrapperLoopIndex);
            }
            
            Writer.WriteLine("return p;");
            Writer.Indent--;
            Writer.WriteLine("}");
            
            return methodName;
        }
        
        void InitializeValueTypes(string arrayName, MemberMapping[] mappings) {
            for (int i = 0; i < mappings.Length; i++) {
                if (!mappings[i].TypeDesc.IsValueType)
                    continue;
                Writer.Write(arrayName);
                Writer.Write("[");
                Writer.Write(i.ToString(CultureInfo.InvariantCulture));
                Writer.Write("] = ");

                if (mappings[i].TypeDesc.IsOptionalValue && mappings[i].TypeDesc.BaseTypeDesc.UseReflection) {
                    Writer.Write("null");
                }
                else {
                    Writer.Write(RaCodeGen.GetStringForCreateInstance(mappings[i].TypeDesc.CSharpName, mappings[i].TypeDesc.UseReflection, false, false));
                }
                Writer.WriteLine(";");
            }
        }
        
        string GenerateEncodedMembersElement(XmlMembersMapping xmlMembersMapping) {
            ElementAccessor element = xmlMembersMapping.Accessor;
            MembersMapping membersMapping = (MembersMapping)element.Mapping;
            MemberMapping[] mappings = membersMapping.Members;
            bool hasWrapperElement = membersMapping.HasWrapperElement;
            bool writeAccessors = membersMapping.WriteAccessors;
            string methodName = NextMethodName(element.Name);
            Writer.WriteLine();
            Writer.Write("public object[] ");
            Writer.Write(methodName);
            Writer.WriteLine("() {");
            Writer.Indent++;

            Writer.WriteLine("Reader.MoveToContent();");

            Writer.Write("object[] p = new object[");
            Writer.Write(mappings.Length.ToString(CultureInfo.InvariantCulture));
            Writer.WriteLine("];");
            InitializeValueTypes("p", mappings);

            if (hasWrapperElement) {
                WriteReadNonRoots();

                if (membersMapping.ValidateRpcWrapperElement) {
                    Writer.Write("if (!");
                    WriteXmlNodeEqual("Reader", element.Name, element.Form == XmlSchemaForm.Qualified ? element.Namespace : "");
                    Writer.WriteLine(") throw CreateUnknownNodeException();");
                }
                Writer.WriteLine("bool isEmptyWrapper = Reader.IsEmptyElement;");
                Writer.WriteLine("Reader.ReadStartElement();");
            }

            Member[] members = new Member[mappings.Length];
            for (int i = 0; i < mappings.Length; i++) {
                MemberMapping mapping = mappings[i];
                string source = "p[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                string arraySource = source;
                if (mapping.Xmlns != null) {
                    arraySource = "((" + mapping.TypeDesc.CSharpName + ")" + source + ")";
                }
                Member member = new Member(this,source, arraySource, "a", i, mapping);
                if (!mapping.IsSequence)
                    member.ParamsReadSource = "paramsRead[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                members[i] = member;

                if (mapping.CheckSpecified == SpecifiedAccessor.ReadWrite) {
                    string nameSpecified = mapping.Name + "Specified";
                    for (int j = 0; j < mappings.Length; j++) {
                        if (mappings[j].Name == nameSpecified) {
                            member.CheckSpecifiedSource = "p[" + j.ToString(CultureInfo.InvariantCulture) + "]";
                            break;
                        }
                    }
                }

            }

            string fixupMethodName = "fixup_" + methodName;
            bool anyFixups = WriteMemberFixupBegin(members, fixupMethodName, "p");

            if (members.Length > 0 && members[0].Mapping.IsReturnValue) Writer.WriteLine("IsReturnValue = true;");

            string checkTypeHrefSource = (!hasWrapperElement && !writeAccessors) ? "hrefList" : null;
            if (checkTypeHrefSource != null)
                WriteInitCheckTypeHrefList(checkTypeHrefSource);
                     
            WriteParamsRead(mappings.Length);
            int loopIndex = WriteWhileNotLoopStart();
            Writer.Indent++;

            string unrecognizedElementSource = checkTypeHrefSource == null ? "UnknownNode((object)p);" : "if (Reader.GetAttribute(\"id\", null) != null) { ReadReferencedElement(); } else { UnknownNode((object)p); }";
            WriteMemberElements(members, unrecognizedElementSource, "UnknownNode((object)p);", null, null, checkTypeHrefSource);
            Writer.WriteLine("Reader.MoveToContent();");

            WriteWhileLoopEnd(loopIndex);

            if (hasWrapperElement)
                Writer.WriteLine("if (!isEmptyWrapper) ReadEndElement();");

            if (checkTypeHrefSource != null)
                WriteHandleHrefList(members, checkTypeHrefSource);

            Writer.WriteLine("ReadReferencedElements();");
            Writer.WriteLine("return p;");

            Writer.Indent--;
            Writer.WriteLine("}");

            if (anyFixups) WriteFixupMethod(fixupMethodName, members, "object[]", false, false, "p");
          
            return methodName;
        }

        void WriteCreateCollection(TypeDesc td, string source) {
            bool useReflection = td.UseReflection;
            string item = (td.ArrayElementTypeDesc == null ? "object" : td.ArrayElementTypeDesc.CSharpName) + "[]";
            bool arrayElementUseReflection = td.ArrayElementTypeDesc == null?false:td.ArrayElementTypeDesc.UseReflection;
            
            //cannot call WriteArrayLocalDecl since 'ci' is always
            //array and 'td' corresponds to 'c'
            if (arrayElementUseReflection)
                item = typeof(Array).FullName;
            Writer.Write(item);
            Writer.Write(" ");
            Writer.Write("ci =");
            Writer.Write("("+item+")");
            Writer.Write(source);
            Writer.WriteLine(";");

            Writer.WriteLine("for (int i = 0; i < ci.Length; i++) {");
            Writer.Indent++;
            Writer.Write(RaCodeGen.GetStringForMethod("c", td.CSharpName,"Add",useReflection));
            
            //cannot call GetStringForArrayMember since 'ci' is always
            //array and 'td' corresponds to 'c'
            if (!arrayElementUseReflection)
                Writer.Write( "ci[i]");
            else 
                Writer.Write(RaCodeGen.GetReflectionVariable(typeof(Array).FullName, "0") + "[ci , i]");

            
            if (useReflection) Writer.WriteLine("}");
            Writer.WriteLine(");");
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        string GenerateTypeElement(XmlTypeMapping xmlTypeMapping) {
            ElementAccessor element = xmlTypeMapping.Accessor;
            TypeMapping mapping = element.Mapping;
            string methodName = NextMethodName(element.Name);
            Writer.WriteLine();
            Writer.Write("public object ");
            Writer.Write(methodName);
            Writer.WriteLine("() {");
            Writer.Indent++;
            Writer.WriteLine("object o = null;");
            MemberMapping member = new MemberMapping();
            member.TypeDesc = mapping.TypeDesc;
            //member.ReadOnly = !mapping.TypeDesc.HasDefaultConstructor;
            member.Elements = new ElementAccessor[] { element };
            Member[] members = new Member[] { new Member(this,"o", "o", "a", 0, member) };
            Writer.WriteLine("Reader.MoveToContent();");
            string unknownNode = "UnknownNode(null, " + ExpectedElements(members) + ");";
            WriteMemberElements(members, "throw CreateUnknownNodeException();", unknownNode, element.Any ? members[0] : null,  null, null);
            if (element.IsSoap) {
                Writer.WriteLine("Referenced(o);");
                Writer.WriteLine("ReadReferencedElements();");
            }
            Writer.WriteLine("return (object)o;");
            Writer.Indent--;
            Writer.WriteLine("}");
            return methodName;
        }
        
        string NextMethodName(string name) {
            return "Read" + (++NextMethodNumber).ToString(CultureInfo.InvariantCulture) + "_" + CodeIdentifier.MakeValidInternal(name);
        }
        
        string NextIdName(string name) {
            return "id" + (++nextIdNumber).ToString(CultureInfo.InvariantCulture) + "_" + CodeIdentifier.MakeValidInternal(name);
        }

        void WritePrimitive(TypeMapping mapping, string source) {
            if (mapping is EnumMapping) {
                string enumMethodName = ReferenceMapping(mapping);
                if (enumMethodName == null) throw new InvalidOperationException(Res.GetString(Res.XmlMissingMethodEnum, mapping.TypeDesc.Name));
                if (mapping.IsSoap) {
                    // SOAP methods are not strongly-typed (the return object), so we need to add a cast
                    Writer.Write("(");
                    Writer.Write(mapping.TypeDesc.CSharpName);
                    Writer.Write(")");
                }
                Writer.Write(enumMethodName);
                Writer.Write("(");
                if (!mapping.IsSoap) Writer.Write(source);
                Writer.Write(")");
            }
            else if (mapping.TypeDesc == StringTypeDesc) {
                Writer.Write(source);
            }
            else if (mapping.TypeDesc.FormatterName == "String") {
                if (mapping.TypeDesc.CollapseWhitespace) {
                    Writer.Write("CollapseWhitespace(");
                    Writer.Write(source);
                    Writer.Write(")");
                }
                else {
                    Writer.Write(source);
                }
            }
            else {
                if (!mapping.TypeDesc.HasCustomFormatter) {
                    Writer.Write(typeof(XmlConvert).FullName);
                    Writer.Write(".");
                }
                Writer.Write("To");
                Writer.Write(mapping.TypeDesc.FormatterName);
                Writer.Write("(");
                Writer.Write(source);
                Writer.Write(")");
            }
        }

        string MakeUnique(EnumMapping mapping, string name) {
            string uniqueName = name;
            object m = Enums[uniqueName];
            if (m != null) {
                if (m == mapping) {
                    // we already have created the hashtable
                    return null;
                }
                int i = 0;
                while (m != null) {
                    i++;
                    uniqueName = name + i.ToString(CultureInfo.InvariantCulture);
                    m = Enums[uniqueName];
                }
            }
            Enums.Add(uniqueName, mapping);
            return uniqueName;
        }

        string WriteHashtable(EnumMapping mapping, string typeName) {

            CodeIdentifier.CheckValidIdentifier(typeName);
            string propName = MakeUnique(mapping, typeName + "Values");
            if (propName == null) return CodeIdentifier.GetCSharpName(typeName);
            string memberName = MakeUnique(mapping, "_" + propName);
            propName = CodeIdentifier.GetCSharpName(propName);

            Writer.WriteLine();
            Writer.Write(typeof(Hashtable).FullName);
            Writer.Write(" ");
            Writer.Write(memberName);
            Writer.WriteLine(";");
            Writer.WriteLine();

            Writer.Write("internal ");
            Writer.Write(typeof(Hashtable).FullName);
            Writer.Write(" ");
            Writer.Write(propName);
            Writer.WriteLine(" {");
            Writer.Indent++;

            Writer.WriteLine("get {");
            Writer.Indent++;

            Writer.Write("if ((object)");
            Writer.Write(memberName);
            Writer.WriteLine(" == null) {");
            Writer.Indent++;

            Writer.Write(typeof(Hashtable).FullName);
            Writer.Write(" h = new ");
            Writer.Write(typeof(Hashtable).FullName);
            Writer.WriteLine("();");

            ConstantMapping[] constants = mapping.Constants;

            for (int i = 0; i < constants.Length; i++) {
                Writer.Write("h.Add(");
                WriteQuotedCSharpString(constants[i].XmlName);
                if (!mapping.TypeDesc.UseReflection){
                    Writer.Write(", (long)");
                    Writer.Write(mapping.TypeDesc.CSharpName);
                    Writer.Write(".@");
                    CodeIdentifier.CheckValidIdentifier(constants[i].Name);
                    Writer.Write(constants[i].Name);
                }
                else{
                    Writer.Write(", ");
                    Writer.Write(constants[i].Value.ToString(CultureInfo.InvariantCulture)+"L");
                }

                Writer.WriteLine(");");
            }

            Writer.Write(memberName);
            Writer.WriteLine(" = h;");

            Writer.Indent--;
            Writer.WriteLine("}");

            Writer.Write("return ");
            Writer.Write(memberName);
            Writer.WriteLine(";");

            Writer.Indent--;
            Writer.WriteLine("}");

            Writer.Indent--;
            Writer.WriteLine("}");

            return propName;
        }

        void WriteEnumMethod(EnumMapping mapping) {
            string tableName = null;
            if (mapping.IsFlags)
                tableName = WriteHashtable(mapping, mapping.TypeDesc.Name);

            string methodName = (string)MethodNames[mapping];
            Writer.WriteLine();
            bool useReflection = mapping.TypeDesc.UseReflection;
            string fullTypeName = mapping.TypeDesc.CSharpName;

            if (mapping.IsSoap) {
                Writer.Write("object");
                Writer.Write(" ");
                Writer.Write(methodName);
                Writer.WriteLine("() {");
                Writer.Indent++;
                Writer.WriteLine("string s = Reader.ReadElementString();");
            }
            else {
                Writer.Write(useReflection?"object":fullTypeName);
                Writer.Write(" ");
                Writer.Write(methodName);
                Writer.WriteLine("(string s) {");
                Writer.Indent++;
            }

            ConstantMapping[] constants = mapping.Constants;
            if (mapping.IsFlags) {
                if (useReflection){
                    Writer.Write("return ");
                    Writer.Write(typeof(Enum).FullName);
                    Writer.Write(".ToObject(");
                    Writer.Write(RaCodeGen.GetStringForTypeof(fullTypeName, useReflection));
                    Writer.Write(", ToEnum(s, ");
                    Writer.Write(tableName);
                    Writer.Write(", ");
                    WriteQuotedCSharpString(fullTypeName);
                    Writer.WriteLine("));");
                }
                else{
                    Writer.Write("return (");
                    Writer.Write(fullTypeName);
                    Writer.Write(")ToEnum(s, ");
                    Writer.Write(tableName);
                    Writer.Write(", ");
                    WriteQuotedCSharpString(fullTypeName);
                    Writer.WriteLine(");");
                }
            }
            else {
                Writer.WriteLine("switch (s) {");
                Writer.Indent++;
                Hashtable cases = new Hashtable();
                for (int i = 0; i < constants.Length; i++) {
                    ConstantMapping c = constants[i];

                    CodeIdentifier.CheckValidIdentifier(c.Name);
                    if (cases[c.XmlName] == null) {
                        Writer.Write("case ");
                        WriteQuotedCSharpString(c.XmlName);
                        Writer.Write(": return ");
                        Writer.Write(RaCodeGen.GetStringForEnumMember(fullTypeName, c.Name, useReflection));
                        Writer.WriteLine(";");
                        cases[c.XmlName] = c.XmlName;
                    }
                }
                
                Writer.Write("default: throw CreateUnknownConstantException(s, ");
                Writer.Write(RaCodeGen.GetStringForTypeof(fullTypeName, useReflection));
                Writer.WriteLine(");");
                Writer.Indent--;
                Writer.WriteLine("}");
            }

            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteDerivedTypes(StructMapping mapping, bool isTypedReturn, string returnTypeName) {

            for (StructMapping derived = mapping.DerivedMappings; derived != null; derived = derived.NextDerivedMapping) {
                Writer.Write("else if (");
                WriteQNameEqual("xsiType", derived.TypeName, derived.Namespace);
                Writer.WriteLine(")");
                Writer.Indent++;

                string methodName = ReferenceMapping(derived);
                #if DEBUG
                    // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                    if (methodName == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorMethod, derived.TypeDesc.Name));
                #endif

                Writer.Write("return ");
                if (derived.TypeDesc.UseReflection && isTypedReturn)
                    Writer.Write("(" + returnTypeName + ")");
                Writer.Write(methodName);
                Writer.Write("(");
                if (derived.TypeDesc.IsNullable)
                    Writer.Write("isNullable, ");
                Writer.WriteLine("false);");

                Writer.Indent--;

                WriteDerivedTypes(derived, isTypedReturn, returnTypeName);
            }
        }

        void WriteEnumAndArrayTypes() {
            foreach (TypeScope scope in Scopes) {
                foreach (Mapping m in scope.TypeMappings) {
                    if (m.IsSoap)
                        continue;
                    if (m is EnumMapping) {
                        EnumMapping mapping = (EnumMapping)m;
                        Writer.Write("else if (");
                        WriteQNameEqual("xsiType", mapping.TypeName, mapping.Namespace);
                        Writer.WriteLine(") {");
                        Writer.Indent++;
                        Writer.WriteLine("Reader.ReadStartElement();");
                        string methodName = ReferenceMapping(mapping);
                        #if DEBUG
                            // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                            if (methodName == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorMethod, mapping.TypeDesc.Name));
                        #endif
                        Writer.Write("object e = ");
                        Writer.Write(methodName);
                        Writer.WriteLine("(CollapseWhitespace(Reader.ReadString()));");
                        Writer.WriteLine("ReadEndElement();");
                        Writer.WriteLine("return e;");
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                    else if (m is ArrayMapping) {
                        ArrayMapping mapping = (ArrayMapping) m;
                        if (mapping.TypeDesc.HasDefaultConstructor) {
                            Writer.Write("else if (");
                            WriteQNameEqual("xsiType", mapping.TypeName, mapping.Namespace);
                            Writer.WriteLine(") {");
                            Writer.Indent++;
                            MemberMapping memberMapping = new MemberMapping();
                            memberMapping.TypeDesc = mapping.TypeDesc;
                            memberMapping.Elements = mapping.Elements;
                            Member member = new Member(this,"a", "z", 0, memberMapping);

                            TypeDesc td = mapping.TypeDesc;
                            string fullTypeName = mapping.TypeDesc.CSharpName;
                            if (td.UseReflection){
                                if (td.IsArray)
                                    Writer.Write(typeof(Array).FullName);
                                else
                                    Writer.Write("object");
                            }
                            else
                                Writer.Write(fullTypeName);
                            Writer.Write(" a = ");
                            if (mapping.TypeDesc.IsValueType) {
                                Writer.Write(RaCodeGen.GetStringForCreateInstance(fullTypeName, td.UseReflection, false, false));
                                Writer.WriteLine(";");
                            }
                            else
                                Writer.WriteLine("null;");

                            WriteArray(member.Source, member.ArrayName, mapping, false, false, -1);
                            Writer.WriteLine("return a;");
                            Writer.Indent--;
                            Writer.WriteLine("}");
                        }
                    }
                }
            }
        }

        void WriteNullableMethod(NullableMapping nullableMapping) {
            string methodName = (string)MethodNames[nullableMapping];
            bool useReflection = nullableMapping.BaseMapping.TypeDesc.UseReflection;
            string typeName = useReflection ? "object" : nullableMapping.TypeDesc.CSharpName;
            Writer.WriteLine();

            Writer.Write(typeName);
            Writer.Write(" ");
            Writer.Write(methodName);
            Writer.WriteLine("(bool checkType) {");
            Writer.Indent++;

            Writer.Write(typeName);
            Writer.Write(" o = ");

            if (useReflection) {
                Writer.Write("null");
            }
            else {
                Writer.Write("default(");
                Writer.Write(typeName);
                Writer.Write(")");
            }
            Writer.WriteLine(";");

            Writer.WriteLine("if (ReadNull())");
            Writer.Indent++;

            Writer.WriteLine("return o;");
            Writer.Indent--;

            ElementAccessor element = new ElementAccessor();
            element.Mapping = nullableMapping.BaseMapping;
            element.Any = false;
            element.IsNullable = nullableMapping.BaseMapping.TypeDesc.IsNullable;

            WriteElement("o", null, null, element, null, null, false, false, -1, -1);
            Writer.WriteLine("return o;");

            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteStructMethod(StructMapping structMapping) {
            if (structMapping.IsSoap)
                WriteEncodedStructMethod(structMapping);
            else
                WriteLiteralStructMethod(structMapping);
        }

        void WriteLiteralStructMethod(StructMapping structMapping) {
            string methodName = (string)MethodNames[structMapping];
            bool useReflection = structMapping.TypeDesc.UseReflection;
            string typeName = useReflection ? "object" : structMapping.TypeDesc.CSharpName;
            Writer.WriteLine();
            Writer.Write(typeName);
            Writer.Write(" ");
            Writer.Write(methodName);
            Writer.Write("(");
            if (structMapping.TypeDesc.IsNullable)
                Writer.Write("bool isNullable, ");
            Writer.WriteLine("bool checkType) {");
            Writer.Indent++;

            Writer.Write(typeof(XmlQualifiedName).FullName);
            Writer.WriteLine(" xsiType = checkType ? GetXsiType() : null;");
            Writer.WriteLine("bool isNull = false;");
            if (structMapping.TypeDesc.IsNullable)
                Writer.WriteLine("if (isNullable) isNull = ReadNull();");

            Writer.WriteLine("if (checkType) {");
            if (structMapping.TypeDesc.IsRoot) {
                Writer.Indent++;
                Writer.WriteLine("if (isNull) {"); 
                Writer.Indent++;
                Writer.WriteLine("if (xsiType != null) return (" + typeName + ")ReadTypedNull(xsiType);"); 
                Writer.Write("else return ");
                if (structMapping.TypeDesc.IsValueType) {
                    Writer.Write(RaCodeGen.GetStringForCreateInstance(structMapping.TypeDesc.CSharpName, useReflection, false, false));
                    Writer.WriteLine(";");
                }
                else
                    Writer.WriteLine("null;");

                Writer.Indent--;
                Writer.WriteLine("}"); 
            }
            Writer.Write("if (xsiType == null");
            if (!structMapping.TypeDesc.IsRoot) {
                Writer.Write(" || ");
                WriteQNameEqual("xsiType", structMapping.TypeName, structMapping.Namespace);
            }
            Writer.WriteLine(") {");
            if (structMapping.TypeDesc.IsRoot) {
                Writer.Indent++;
                Writer.WriteLine("return ReadTypedPrimitive(new System.Xml.XmlQualifiedName(\"" + Soap.UrType + "\", \"" + XmlSchema.Namespace + "\"));");
                Writer.Indent--;
            }
            Writer.WriteLine("}");
            WriteDerivedTypes(structMapping, !useReflection && !structMapping.TypeDesc.IsRoot, typeName);
            if (structMapping.TypeDesc.IsRoot) WriteEnumAndArrayTypes();
            Writer.WriteLine("else");
            Writer.Indent++;
            if (structMapping.TypeDesc.IsRoot)
                Writer.Write("return ReadTypedPrimitive((");
            else
                Writer.Write("throw CreateUnknownTypeException((");
            Writer.Write(typeof(XmlQualifiedName).FullName);
            Writer.WriteLine(")xsiType);");
            Writer.Indent--;
            Writer.WriteLine("}");

            if (structMapping.TypeDesc.IsNullable)
                Writer.WriteLine("if (isNull) return null;");

            if (structMapping.TypeDesc.IsAbstract) {
                Writer.Write("throw CreateAbstractTypeException(");
                WriteQuotedCSharpString(structMapping.TypeName);
                Writer.Write(", ");
                WriteQuotedCSharpString(structMapping.Namespace);
                Writer.WriteLine(");");
            }
            else {
                if (structMapping.TypeDesc.Type != null && typeof(XmlSchemaObject).IsAssignableFrom(structMapping.TypeDesc.Type)) {
                    Writer.WriteLine("DecodeName = false;");
                }
                WriteCreateMapping(structMapping, "o");
                
                MemberMapping[] mappings = TypeScope.GetSettableMembers(structMapping);
                
                Member anyText = null;
                Member anyElement = null;
                Member anyAttribute = null; 
                bool isSequence = structMapping.HasExplicitSequence();

                ArrayList arraysToDeclareList = new ArrayList(mappings.Length);
                ArrayList arraysToSetList = new ArrayList(mappings.Length);
                ArrayList allMembersList = new ArrayList(mappings.Length);

                for (int i = 0; i < mappings.Length; i++) {
                    MemberMapping mapping = mappings[i];
                    CodeIdentifier.CheckValidIdentifier(mapping.Name);
                    string source = RaCodeGen.GetStringForMember("o", mapping.Name, structMapping.TypeDesc);
                    Member member = new Member(this, source, "a", i, mapping, GetChoiceIdentifierSource(mapping, "o", structMapping.TypeDesc));
                    if (!mapping.IsSequence)
                        member.ParamsReadSource = "paramsRead[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                    member.IsNullable = mapping.TypeDesc.IsNullable;
                    if (mapping.CheckSpecified == SpecifiedAccessor.ReadWrite)
                        member.CheckSpecifiedSource = RaCodeGen.GetStringForMember("o", mapping.Name + "Specified", structMapping.TypeDesc);
                    if (mapping.Text != null)
                        anyText = member;
                    if (mapping.Attribute != null && mapping.Attribute.Any)
                        anyAttribute = member;
                    if (!isSequence) {
                        // find anyElement if present.
                        for (int j = 0; j < mapping.Elements.Length; j++) {
                            if (mapping.Elements[j].Any && (mapping.Elements[j].Name == null || mapping.Elements[j].Name.Length == 0)) {
                                anyElement = member;
                                break;
                            }
                        }
                    }
                    else if (mapping.IsParticle && !mapping.IsSequence) {
                        StructMapping declaringMapping;
                        structMapping.FindDeclaringMapping(mapping, out declaringMapping, structMapping.TypeName);
                        throw new InvalidOperationException(Res.GetString(Res.XmlSequenceHierarchy, structMapping.TypeDesc.FullName, mapping.Name, declaringMapping.TypeDesc.FullName, "Order"));
                    }
                    if (mapping.Attribute == null && mapping.Elements.Length == 1 && mapping.Elements[0].Mapping is ArrayMapping) {
                        Member arrayMember = new Member(this, source, source, "a", i, mapping, GetChoiceIdentifierSource(mapping, "o", structMapping.TypeDesc));
                        arrayMember.CheckSpecifiedSource = member.CheckSpecifiedSource;
                        allMembersList.Add(arrayMember);
                    }
                    else {
                        allMembersList.Add(member);
                    }

                    if (mapping.TypeDesc.IsArrayLike) {
                        arraysToDeclareList.Add(member);
                        if (mapping.TypeDesc.IsArrayLike && !(mapping.Elements.Length == 1 && mapping.Elements[0].Mapping is ArrayMapping)) {
                            member.ParamsReadSource = null; // flat arrays -- don't want to count params read.
                            if (member != anyText && member != anyElement) {
                                arraysToSetList.Add(member);
                            }
                        }
                        else if (!mapping.TypeDesc.IsArray) {
                            member.ParamsReadSource = null; // collection
                        }
                    }
                }
                if (anyElement != null) arraysToSetList.Add(anyElement);
                if (anyText != null && anyText != anyElement) arraysToSetList.Add(anyText);

                Member[] arraysToDeclare = (Member[]) arraysToDeclareList.ToArray(typeof(Member));
                Member[] arraysToSet = (Member[]) arraysToSetList.ToArray(typeof(Member));
                Member[] allMembers = (Member[]) allMembersList.ToArray(typeof(Member));

                WriteMemberBegin(arraysToDeclare);
                WriteParamsRead(mappings.Length);

                WriteAttributes(allMembers, anyAttribute, "UnknownNode", "(object)o");
                if (anyAttribute != null)
                    WriteMemberEnd(arraysToDeclare);

                Writer.WriteLine("Reader.MoveToElement();");

                Writer.WriteLine("if (Reader.IsEmptyElement) {");
                Writer.Indent++;
                Writer.WriteLine("Reader.Skip();");
                WriteMemberEnd(arraysToSet);
                Writer.WriteLine("return o;");
                Writer.Indent--;
                Writer.WriteLine("}");

                Writer.WriteLine("Reader.ReadStartElement();");
                if (IsSequence(allMembers)) {
                    Writer.WriteLine("int state = 0;");
                }
                int loopIndex = WriteWhileNotLoopStart();
                Writer.Indent++;
                string unknownNode = "UnknownNode((object)o, " + ExpectedElements(allMembers) + ");";
                WriteMemberElements(allMembers, unknownNode, unknownNode, anyElement, anyText, null);
                Writer.WriteLine("Reader.MoveToContent();");

                WriteWhileLoopEnd(loopIndex);
                WriteMemberEnd(arraysToSet);

                Writer.WriteLine("ReadEndElement();");
                Writer.WriteLine("return o;");
            }
            Writer.Indent--;
            Writer.WriteLine("}");
        }        

        void WriteEncodedStructMethod(StructMapping structMapping) {
            if(structMapping.TypeDesc.IsRoot)
                return;
            bool useReflection = structMapping.TypeDesc.UseReflection;
            string methodName = (string)MethodNames[structMapping];
            Writer.WriteLine();
            Writer.Write("object");
            Writer.Write(" ");
            Writer.Write(methodName);
            Writer.Write("(");
            Writer.WriteLine(") {");
            Writer.Indent++;

            Member[] members;
            bool anyFixups;
            string fixupMethodName;

            if (structMapping.TypeDesc.IsAbstract) {
                Writer.Write("throw CreateAbstractTypeException(");
                WriteQuotedCSharpString(structMapping.TypeName);
                Writer.Write(", ");
                WriteQuotedCSharpString(structMapping.Namespace);
                Writer.WriteLine(");");
                members = new Member[0];
                anyFixups = false;
                fixupMethodName = null;
            }
            else {
                WriteCreateMapping(structMapping, "o");

                MemberMapping[] mappings = TypeScope.GetSettableMembers(structMapping);
                members = new Member[mappings.Length];
                for (int i = 0; i < mappings.Length; i++) {
                    MemberMapping mapping = mappings[i];
                    CodeIdentifier.CheckValidIdentifier(mapping.Name);
                    string source = RaCodeGen.GetStringForMember("o", mapping.Name, structMapping.TypeDesc);
                    Member member = new Member(this,source, source, "a", i, mapping, GetChoiceIdentifierSource(mapping, "o", structMapping.TypeDesc));
                    if (mapping.CheckSpecified == SpecifiedAccessor.ReadWrite)
                        member.CheckSpecifiedSource = RaCodeGen.GetStringForMember("o", mapping.Name + "Specified", structMapping.TypeDesc);
                    if (!mapping.IsSequence)
                        member.ParamsReadSource = "paramsRead[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                    members[i] = member;
                }

                fixupMethodName = "fixup_" + methodName;
                anyFixups = WriteMemberFixupBegin(members, fixupMethodName, "o");
                
                // we're able to not do WriteMemberBegin here because we don't allow arrays as attributes
                
                WriteParamsRead(mappings.Length);
                WriteAttributes(members, null, "UnknownNode", "(object)o");
                Writer.WriteLine("Reader.MoveToElement();");

                Writer.WriteLine("if (Reader.IsEmptyElement) { Reader.Skip(); return o; }");
                Writer.WriteLine("Reader.ReadStartElement();");

                int loopIndex = WriteWhileNotLoopStart();
                Writer.Indent++;

                WriteMemberElements(members, "UnknownNode((object)o);", "UnknownNode((object)o);", null, null, null);
                Writer.WriteLine("Reader.MoveToContent();");

                WriteWhileLoopEnd(loopIndex);

                Writer.WriteLine("ReadEndElement();");
                Writer.WriteLine("return o;");
            }
            Writer.Indent--;
            Writer.WriteLine("}");

            if (anyFixups) WriteFixupMethod(fixupMethodName, members, structMapping.TypeDesc.CSharpName, structMapping.TypeDesc.UseReflection, true, "o");
        }

        void WriteFixupMethod(string fixupMethodName, Member[] members, string typeName, bool useReflection, bool typed, string source) {
            Writer.WriteLine();
            Writer.Write("void ");
            Writer.Write(fixupMethodName);
            Writer.WriteLine("(object objFixup) {");
            Writer.Indent++;
            Writer.WriteLine("Fixup fixup = (Fixup)objFixup;");
            WriteLocalDecl(typeName, source, "fixup.Source", useReflection);
            Writer.WriteLine("string[] ids = fixup.Ids;");

            for (int i = 0; i < members.Length; i++) {
                Member member = members[i];
                if (member.MultiRef) {
                    string fixupIndex = member.FixupIndex.ToString(CultureInfo.InvariantCulture);
                    Writer.Write("if (ids[");
                    Writer.Write(fixupIndex);
                    Writer.WriteLine("] != null) {");
                    Writer.Indent++;

                    string memberSource = /*member.IsList ? source + ".Add(" :*/ member.ArraySource;

                    string targetSource = "GetTarget(ids[" + fixupIndex + "])";
                    TypeDesc td = member.Mapping.TypeDesc;
                    if (td.IsCollection || td.IsEnumerable) {
                        WriteAddCollectionFixup(td, member.Mapping.ReadOnly, memberSource, targetSource);
                    }
                    else {
                        if (typed) {
                            Writer.WriteLine("try {");
                            Writer.Indent++;
                            WriteSourceBeginTyped(memberSource, member.Mapping.TypeDesc);
                        }
                        else
                            WriteSourceBegin(memberSource);

                        Writer.Write(targetSource);
                        WriteSourceEnd(memberSource);
                        Writer.WriteLine(";");

                        if (member.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite && member.CheckSpecifiedSource != null && member.CheckSpecifiedSource.Length > 0) { 
                            Writer.Write(member.CheckSpecifiedSource);
                            Writer.WriteLine(" = true;");
                        }

                        if (typed) {
                            WriteCatchCastException(member.Mapping.TypeDesc, targetSource, "ids[" + fixupIndex + "]");
                        }
                    }
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
            }
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteAddCollectionFixup(TypeDesc typeDesc, bool readOnly, string memberSource, string targetSource) {
            Writer.WriteLine("// get array of the collection items");
            bool useReflection = typeDesc.UseReflection;
            CreateCollectionInfo create = (CreateCollectionInfo)createMethods[typeDesc];
            if (create == null) {
                string createName = "create" + (++nextCreateMethodNumber).ToString(CultureInfo.InvariantCulture) + "_" + typeDesc.Name;
                create = new CreateCollectionInfo(createName, typeDesc);
                createMethods.Add(typeDesc, create);
            }

            Writer.Write("if ((object)(");
            Writer.Write(memberSource);
            Writer.WriteLine(") == null) {");
            Writer.Indent++;

            if (readOnly) {
                Writer.Write("throw CreateReadOnlyCollectionException(");
                WriteQuotedCSharpString(typeDesc.CSharpName);
                Writer.WriteLine(");");
            }
            else {
                Writer.Write(memberSource);
                Writer.Write(" = ");
                Writer.Write(RaCodeGen.GetStringForCreateInstance(typeDesc.CSharpName, typeDesc.UseReflection, typeDesc.CannotNew, true));
                Writer.WriteLine(";");
            }

            Writer.Indent--;
            Writer.WriteLine("}");
        
            Writer.Write("CollectionFixup collectionFixup = new CollectionFixup(");
            Writer.Write(memberSource);
            Writer.Write(", ");
            Writer.Write("new ");
            Writer.Write(typeof(XmlSerializationCollectionFixupCallback).FullName);
            Writer.Write("(this.");
            Writer.Write(create.Name);
            Writer.Write("), ");
            Writer.Write(targetSource);
            Writer.WriteLine(");");
            Writer.WriteLine("AddFixup(collectionFixup);");
        }

        void WriteCreateCollectionMethod(CreateCollectionInfo c) {
            Writer.Write("void ");
            Writer.Write(c.Name);
            Writer.WriteLine("(object collection, object collectionItems) {");
            Writer.Indent++;

            Writer.WriteLine("if (collectionItems == null) return;");
            Writer.WriteLine("if (collection == null) return;");

            TypeDesc td = c.TypeDesc;
            bool useReflection = td.UseReflection;
            string fullTypeName = td.CSharpName;
            WriteLocalDecl(fullTypeName, "c", "collection", useReflection);

            WriteCreateCollection(td, "collectionItems");

            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteQNameEqual(string source, string name, string ns) {
            Writer.Write("((object) ((");
            Writer.Write(typeof(XmlQualifiedName).FullName);
            Writer.Write(")");
            Writer.Write(source);
            Writer.Write(").Name == (object)");
            WriteID(name);
            Writer.Write(" && (object) ((");
            Writer.Write(typeof(XmlQualifiedName).FullName);
            Writer.Write(")");
            Writer.Write(source);
            Writer.Write(").Namespace == (object)");
            WriteID(ns);
            Writer.Write(")");
        }

        void WriteXmlNodeEqual(string source, string name, string ns) {
            Writer.Write("(");
            if (name != null && name.Length > 0) {
                Writer.Write("(object) ");
                Writer.Write(source);
                Writer.Write(".LocalName == (object)");
                WriteID(name);
                Writer.Write(" && ");
            }
            Writer.Write("(object) ");
            Writer.Write(source);
            Writer.Write(".NamespaceURI == (object)");
            WriteID(ns);
            Writer.Write(")");
        }

        void WriteID(string name) {
            if (name == null) {
                //Writer.Write("null");
                //return;
                name = "";
            }
            string idName = (string)idNames[name];
            if (idName == null) {
                idName = NextIdName(name);
                idNames.Add(name, idName);
            }
            Writer.Write(idName);
        }

        void WriteAttributes(Member[] members, Member anyAttribute, string elseCall, string firstParam) {
            int count = 0;
            Member xmlnsMember = null;
            ArrayList attributes = new ArrayList();
            
            Writer.WriteLine("while (Reader.MoveToNextAttribute()) {");
            Writer.Indent++;

            for (int i = 0; i < members.Length; i++) {
                Member member = (Member)members[i];
                if (member.Mapping.Xmlns != null) {
                    xmlnsMember = member;
                    continue;
                }
                if (member.Mapping.Ignore)
                    continue;
                AttributeAccessor attribute = member.Mapping.Attribute;

                if (attribute == null) continue;
                if (attribute.Any) continue;

                attributes.Add(attribute);

                if (count++ > 0)
                    Writer.Write("else ");

                Writer.Write("if (");
                if (member.ParamsReadSource != null) {
                    Writer.Write("!");
                    Writer.Write(member.ParamsReadSource);
                    Writer.Write(" && ");
                }

                if (attribute.IsSpecialXmlNamespace) {
                    WriteXmlNodeEqual("Reader", attribute.Name, XmlReservedNs.NsXml);
                }
                else
                    WriteXmlNodeEqual("Reader", attribute.Name, attribute.Form == XmlSchemaForm.Qualified ? attribute.Namespace : "");
                Writer.WriteLine(") {");
                Writer.Indent++;

                WriteAttribute(member);
                Writer.Indent--;
                Writer.WriteLine("}");
            }

            if (count > 0)
                Writer.Write("else ");

            if (xmlnsMember != null) {
                Writer.WriteLine("if (IsXmlnsAttribute(Reader.Name)) {");
                Writer.Indent++;

                Writer.Write("if (");
                Writer.Write(xmlnsMember.Source);
                Writer.Write(" == null) ");
                Writer.Write(xmlnsMember.Source);
                Writer.Write(" = new ");
                Writer.Write(xmlnsMember.Mapping.TypeDesc.CSharpName);
                Writer.WriteLine("();");

                //Writer.Write(xmlnsMember.ArraySource);
                Writer.Write("(("+xmlnsMember.Mapping.TypeDesc.CSharpName+")"+ xmlnsMember.ArraySource+")");
                Writer.WriteLine(".Add(Reader.Name.Length == 5 ? \"\" : Reader.LocalName, Reader.Value);");

                Writer.Indent--;
                Writer.WriteLine("}");

                Writer.WriteLine("else {");
                Writer.Indent++;
            }
            else {
                Writer.WriteLine("if (!IsXmlnsAttribute(Reader.Name)) {");
                Writer.Indent++;
            }
            if (anyAttribute != null) {
                Writer.Write(typeof(XmlAttribute).FullName);
                Writer.Write(" attr = ");
                Writer.Write("(");
                Writer.Write(typeof(XmlAttribute).FullName);
                Writer.WriteLine(") Document.ReadNode(Reader);");
                Writer.WriteLine("ParseWsdlArrayType(attr);");
                WriteAttribute(anyAttribute);
            }
            else {
                Writer.Write(elseCall);
                Writer.Write("(");
                Writer.Write(firstParam);
                if (attributes.Count > 0) {
                    Writer.Write(", ");
                    string qnames = "";

                    for (int i = 0; i < attributes.Count; i++) {
                        AttributeAccessor attribute = (AttributeAccessor)attributes[i];
                        if (i > 0)
                            qnames += ", ";
                        qnames += attribute.IsSpecialXmlNamespace ? XmlReservedNs.NsXml : (attribute.Form == XmlSchemaForm.Qualified ? attribute.Namespace : "") + ":" + attribute.Name;
                    }
                    WriteQuotedCSharpString(qnames);
                }
                Writer.WriteLine(");");
            }
            Writer.Indent--;
            Writer.WriteLine("}");

            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteAttribute(Member member) {

            AttributeAccessor attribute = member.Mapping.Attribute;

            if (attribute.Mapping is SpecialMapping) {
                SpecialMapping special = (SpecialMapping)attribute.Mapping;
                    
                if (special.TypeDesc.Kind == TypeKind.Attribute) {
                    WriteSourceBegin(member.ArraySource);
                    Writer.Write("attr");
                    WriteSourceEnd(member.ArraySource);
                    Writer.WriteLine(";");
                }
                else if (special.TypeDesc.CanBeAttributeValue) {
                    Writer.Write("if (attr is ");
                    Writer.Write(typeof(XmlAttribute).FullName);
                    Writer.WriteLine(") {");
                    Writer.Indent++;
                    WriteSourceBegin(member.ArraySource);
                    Writer.Write("(");
                    Writer.Write(typeof(XmlAttribute).FullName);
                    Writer.Write(")attr");
                    WriteSourceEnd(member.ArraySource);
                    Writer.WriteLine(";");
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                else
                    throw new InvalidOperationException(Res.GetString(Res.XmlInternalError));
            }
            else {
                if (attribute.IsList) {
                    Writer.WriteLine("string listValues = Reader.Value;");
                    Writer.WriteLine("string[] vals = listValues.Split(null);");
                    Writer.WriteLine("for (int i = 0; i < vals.Length; i++) {");
                    Writer.Indent++;

                    string attributeSource = GetArraySource(member.Mapping.TypeDesc, member.ArrayName);

                    WriteSourceBegin(attributeSource);
                    WritePrimitive(attribute.Mapping, "vals[i]");
                    WriteSourceEnd(attributeSource);
                    Writer.WriteLine(";");
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                else {
                    WriteSourceBegin(member.ArraySource);
                    WritePrimitive(attribute.Mapping, attribute.IsList ? "vals[i]" : "Reader.Value");
                    WriteSourceEnd(member.ArraySource);
                    Writer.WriteLine(";");
                }
            }
            if (member.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite && member.CheckSpecifiedSource != null && member.CheckSpecifiedSource.Length > 0) { 
                Writer.Write(member.CheckSpecifiedSource);
                Writer.WriteLine(" = true;");
            }
            if (member.ParamsReadSource != null) {
                Writer.Write(member.ParamsReadSource);
                Writer.WriteLine(" = true;");
            }
        }

        bool WriteMemberFixupBegin(Member[] members, string fixupMethodName, string source) {
            int fixupCount = 0;
            for (int i = 0; i < members.Length; i++) {
                Member member = (Member)members[i];
                if (member.Mapping.Elements.Length == 0)
                    continue;

                TypeMapping mapping = member.Mapping.Elements[0].Mapping;
                if (mapping is StructMapping || mapping is ArrayMapping || mapping is PrimitiveMapping || mapping is NullableMapping) {
                    member.MultiRef = true;
                    member.FixupIndex = fixupCount++;
                }
            }

            if (fixupCount > 0) {
                Writer.Write("Fixup fixup = new Fixup(");
                Writer.Write(source);
                Writer.Write(", ");
                Writer.Write("new ");
                Writer.Write(typeof(XmlSerializationFixupCallback).FullName);
                Writer.Write("(this.");
                Writer.Write(fixupMethodName);
                Writer.Write("), ");
                Writer.Write(fixupCount.ToString(CultureInfo.InvariantCulture));
                Writer.WriteLine(");");
                Writer.WriteLine("AddFixup(fixup);");
                return true;
            }
            return false;
        }

        void WriteMemberBegin(Member[] members) {

            for (int i = 0; i < members.Length; i++) {
                Member member = (Member)members[i];

                if (member.IsArrayLike) {
                    string a = member.ArrayName;
                    string c = "c" + a;

                    TypeDesc typeDesc = member.Mapping.TypeDesc;
                    string typeDescFullName = typeDesc.CSharpName;
                
                    if (member.Mapping.TypeDesc.IsArray) {
                        WriteArrayLocalDecl(typeDesc.CSharpName,
                                            a, "null", typeDesc);
                        Writer.Write("int ");
                        Writer.Write(c);
                        Writer.WriteLine(" = 0;");

                        if (member.Mapping.ChoiceIdentifier != null) {
                            WriteArrayLocalDecl(member.Mapping.ChoiceIdentifier.Mapping.TypeDesc.CSharpName+"[]",
                                                member.ChoiceArrayName, "null",
                                                member.Mapping.ChoiceIdentifier.Mapping.TypeDesc);
                            Writer.Write("int c");
                            Writer.Write(member.ChoiceArrayName);
                            Writer.WriteLine(" = 0;");

                        }
                    }
                    else {
                        bool useReflection = typeDesc.UseReflection;
                        if (member.Source[member.Source.Length - 1] == '(' || member.Source[member.Source.Length - 1] == '{') {
                            WriteCreateInstance(typeDescFullName, a, useReflection, typeDesc.CannotNew); 
                            Writer.Write(member.Source);
                            Writer.Write(a);
                            if (member.Source[member.Source.Length - 1] == '{')
                                Writer.WriteLine("});");
                            else
                                Writer.WriteLine(");");
                        }
                        else {
                            if (member.IsList && !member.Mapping.ReadOnly && member.Mapping.TypeDesc.IsNullable) {
                                // we need to new the Collections and ArrayLists
                                Writer.Write("if ((object)(");
                                Writer.Write(member.Source);
                                Writer.Write(") == null) ");
                                if (!member.Mapping.TypeDesc.HasDefaultConstructor) {
                                    Writer.Write("throw CreateReadOnlyCollectionException(");
                                    WriteQuotedCSharpString(member.Mapping.TypeDesc.CSharpName);
                                    Writer.WriteLine(");");
                                }
                                else {
                                    Writer.Write(member.Source);
                                    Writer.Write(" = ");
                                    Writer.Write(RaCodeGen.GetStringForCreateInstance(typeDescFullName, useReflection, typeDesc.CannotNew, true));
                                    Writer.WriteLine(";");
                                }
                            }
                            WriteLocalDecl(typeDescFullName, a, member.Source, useReflection);
                        }
                    }
                }
            }
        }

        string ExpectedElements(Member[] members) {
            if (IsSequence(members))
                return "null";
            string qnames = string.Empty;
            bool firstElement = true;
            for (int i = 0; i < members.Length; i++) {
                Member member = (Member)members[i];
                if (member.Mapping.Xmlns != null)
                    continue;
                if (member.Mapping.Ignore)
                    continue;
                if (member.Mapping.IsText || member.Mapping.IsAttribute)
                    continue;
                
                ElementAccessor[] elements = member.Mapping.Elements;

                for (int j = 0; j < elements.Length; j++) {
                    ElementAccessor e = elements[j];
                    string ns = e.Form == XmlSchemaForm.Qualified ? e.Namespace : "";
                    if (e.Any && (e.Name == null || e.Name.Length == 0)) continue;

                    if (!firstElement)
                        qnames += ", ";
                    qnames += ns + ":" + e.Name;
                    firstElement = false;
                }
            }
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            ReflectionAwareCodeGen.WriteQuotedCSharpString(new IndentedWriter(writer, true), qnames);
            return writer.ToString();
        }

        void WriteMemberElements(Member[] members, string elementElseString, string elseString, Member anyElement, Member anyText, string checkTypeHrefsSource) {
            bool checkType = (checkTypeHrefsSource != null && checkTypeHrefsSource.Length > 0);

            if (anyText != null) {
                Writer.WriteLine("string tmp = null;");
            }

            Writer.Write("if (Reader.NodeType == ");
            Writer.Write(typeof(XmlNodeType).FullName);
            Writer.WriteLine(".Element) {");
            Writer.Indent++;

            if (checkType) {
                WriteIfNotSoapRoot(elementElseString + " continue;");
                WriteMemberElementsCheckType(checkTypeHrefsSource);
            }
            else {
                WriteMemberElementsIf(members, anyElement, elementElseString, null);
            }

            Writer.Indent--;
            Writer.WriteLine("}");

            if (anyText != null)
                WriteMemberText(anyText, elseString);

            Writer.WriteLine("else {");
            Writer.Indent++;
            Writer.WriteLine(elseString);
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteMemberText(Member anyText, string elseString) {
            Writer.Write("else if (Reader.NodeType == ");
            Writer.Write(typeof(XmlNodeType).FullName);
            Writer.WriteLine(".Text || ");
            Writer.Write("Reader.NodeType == ");
            Writer.Write(typeof(XmlNodeType).FullName);
            Writer.WriteLine(".CDATA || ");
            Writer.Write("Reader.NodeType == ");
            Writer.Write(typeof(XmlNodeType).FullName);
            Writer.WriteLine(".Whitespace || ");
            Writer.Write("Reader.NodeType == ");
            Writer.Write(typeof(XmlNodeType).FullName);
            Writer.WriteLine(".SignificantWhitespace) {");
            Writer.Indent++;

            if (anyText != null) {
                WriteText(anyText);
            }
            else {
                Writer.Write(elseString);
                Writer.WriteLine(";");
            }
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteText(Member member) {

            TextAccessor text = member.Mapping.Text;

            if (text.Mapping is SpecialMapping) {
                SpecialMapping special = (SpecialMapping)text.Mapping;
                WriteSourceBeginTyped(member.ArraySource, special.TypeDesc);
                switch (special.TypeDesc.Kind) {
                    case TypeKind.Node:
                        Writer.Write("Document.CreateTextNode(Reader.ReadString())");
                        break;
                    default:
                        throw new InvalidOperationException(Res.GetString(Res.XmlInternalError));
                }
                WriteSourceEnd(member.ArraySource);
            }
            else {
                if (member.IsArrayLike) {
                    WriteSourceBegin(member.ArraySource);
                    if (text.Mapping.TypeDesc.CollapseWhitespace) {
                        Writer.Write("CollapseWhitespace(Reader.ReadString())");
                    }
                    else {
                        Writer.Write("Reader.ReadString()");
                    }
                }
                else {
                    if (text.Mapping.TypeDesc == StringTypeDesc || text.Mapping.TypeDesc.FormatterName == "String") {
                        Writer.Write("tmp = ReadString(tmp, ");
                        if (text.Mapping.TypeDesc.CollapseWhitespace)
                            Writer.WriteLine("true);");
                        else 
                            Writer.WriteLine("false);");
                        
                        WriteSourceBegin(member.ArraySource);
                        Writer.Write("tmp");
                    }
                    else {
                        WriteSourceBegin(member.ArraySource);
                        WritePrimitive(text.Mapping, "Reader.ReadString()");
                    }
                }
                WriteSourceEnd(member.ArraySource);
            }

            Writer.WriteLine(";");
        }

        void WriteMemberElementsCheckType(string checkTypeHrefsSource) {
            Writer.WriteLine("string refElemId = null;");
            Writer.WriteLine("object refElem = ReadReferencingElement(null, null, true, out refElemId);");
            
            Writer.WriteLine("if (refElemId != null) {");
            Writer.Indent++;
            Writer.Write(checkTypeHrefsSource);
            Writer.WriteLine(".Add(refElemId);");
            Writer.Write(checkTypeHrefsSource);
            Writer.WriteLine("IsObject.Add(false);");
            Writer.Indent--;
            Writer.WriteLine("}");
            Writer.WriteLine("else if (refElem != null) {");
            Writer.Indent++;
            Writer.Write(checkTypeHrefsSource);
            Writer.WriteLine(".Add(refElem);");
            Writer.Write(checkTypeHrefsSource);
            Writer.WriteLine("IsObject.Add(true);");
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteMemberElementsElse(Member anyElement, string elementElseString) {
            if (anyElement != null) {
                ElementAccessor[] elements = anyElement.Mapping.Elements;
                for (int i = 0; i < elements.Length; i++) {
                    ElementAccessor element = elements[i];
                    if (element.Any && element.Name.Length == 0) {
                        WriteElement(anyElement.ArraySource, anyElement.ArrayName, anyElement.ChoiceArraySource, element, anyElement.Mapping.ChoiceIdentifier, anyElement.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite ? anyElement.CheckSpecifiedSource : null, false, false, -1, i);
                        break;
                    }
                }
            }
            else {
                Writer.WriteLine(elementElseString);
            }
        }

        bool IsSequence(Member[] members) {
            for (int i = 0; i < members.Length; i++) {
                if (members[i].Mapping.IsParticle && members[i].Mapping.IsSequence)
                    return true;
            }
            return false;
        }
        void WriteMemberElementsIf(Member[] members, Member anyElement, string elementElseString, string checkTypeSource) {
            bool checkType = checkTypeSource != null && checkTypeSource.Length > 0;
            //int count = checkType ? 1 : 0;
            int count = 0;

            bool isSequence = IsSequence(members);
            if (isSequence) {
                Writer.WriteLine("switch (state) {");
            }
            int cases = 0;

            for (int i = 0; i < members.Length; i++) {
                Member member = (Member)members[i];
                if (member.Mapping.Xmlns != null)
                    continue;
                if (member.Mapping.Ignore)
                    continue;
                if (isSequence && (member.Mapping.IsText || member.Mapping.IsAttribute))
                    continue;

                bool firstElement = true;
                ChoiceIdentifierAccessor choice = member.Mapping.ChoiceIdentifier;
                ElementAccessor[] elements = member.Mapping.Elements;

                for (int j = 0; j < elements.Length; j++) {
                    ElementAccessor e = elements[j];
                    string ns = e.Form == XmlSchemaForm.Qualified ? e.Namespace : "";
                    if (!isSequence && e.Any && (e.Name == null || e.Name.Length == 0)) continue;
                    if (!firstElement || (!isSequence && count > 0)) {
                        Writer.Write("else ");
                    }
                    else if (isSequence) {
                        Writer.Write("case ");
                        Writer.Write(cases.ToString(CultureInfo.InvariantCulture));
                        Writer.WriteLine(":");
                        Writer.Indent++;
                    }
                    count++;
                    firstElement = false;
                    Writer.Write("if (");
                    if (member.ParamsReadSource != null) {
                        Writer.Write("!");
                        Writer.Write(member.ParamsReadSource);
                        Writer.Write(" && ");
                    }
                    if (checkType) {
                        if (e.Mapping is NullableMapping) {
                            TypeDesc td = ((NullableMapping)e.Mapping).BaseMapping.TypeDesc;
                            Writer.Write(RaCodeGen.GetStringForTypeof(td.CSharpName, td.UseReflection));
                        }
                        else {
                            Writer.Write(RaCodeGen.GetStringForTypeof(e.Mapping.TypeDesc.CSharpName, e.Mapping.TypeDesc.UseReflection));
                        }
                        Writer.Write(".IsAssignableFrom(");
                        Writer.Write(checkTypeSource);
                        Writer.Write("Type)");
                    }
                    else {
                        if (member.Mapping.IsReturnValue)
                            Writer.Write("(IsReturnValue || ");
                        if (isSequence && e.Any && e.AnyNamespaces == null) {
                            Writer.Write("true");
                        }
                        else {
                            WriteXmlNodeEqual("Reader", e.Name, ns);
                        }
                        if (member.Mapping.IsReturnValue)
                            Writer.Write(")");
                    }
                    Writer.WriteLine(") {");
                    Writer.Indent++;
                    if (checkType) {
                        if (e.Mapping.TypeDesc.IsValueType || e.Mapping is NullableMapping) {
                            Writer.Write("if (");
                            Writer.Write(checkTypeSource);
                            Writer.WriteLine(" != null) {");
                            Writer.Indent++;
                        }
                        if (e.Mapping is NullableMapping) {
                            WriteSourceBegin(member.ArraySource);
                            TypeDesc td = ((NullableMapping)e.Mapping).BaseMapping.TypeDesc;
                            Writer.Write(RaCodeGen.GetStringForCreateInstance(e.Mapping.TypeDesc.CSharpName, e.Mapping.TypeDesc.UseReflection, false, true, "(" + td.CSharpName + ")" + checkTypeSource));
                        }
                        else {
                            WriteSourceBeginTyped(member.ArraySource, e.Mapping.TypeDesc);
                            Writer.Write(checkTypeSource); 
                        }
                        WriteSourceEnd(member.ArraySource);
                        Writer.WriteLine(";");
                        if (e.Mapping.TypeDesc.IsValueType) {
                            Writer.Indent--;
                            Writer.WriteLine("}");
                        }
                        if (member.FixupIndex >= 0) {
                            Writer.Write("fixup.Ids[");
                            Writer.Write(member.FixupIndex.ToString(CultureInfo.InvariantCulture));
                            Writer.Write("] = ");
                            Writer.Write(checkTypeSource);
                            Writer.WriteLine("Id;");
                        }
                    }
                    else {
                        WriteElement(member.ArraySource, member.ArrayName, member.ChoiceArraySource, e, choice, member.Mapping.CheckSpecified == SpecifiedAccessor.ReadWrite ? member.CheckSpecifiedSource : null, member.IsList && member.Mapping.TypeDesc.IsNullable, member.Mapping.ReadOnly, member.FixupIndex, j);
                    }
                    if (member.Mapping.IsReturnValue)
                        Writer.WriteLine("IsReturnValue = false;");
                    if (member.ParamsReadSource != null) {
                        Writer.Write(member.ParamsReadSource);
                        Writer.WriteLine(" = true;");
                    }
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                if (isSequence) {
                    if (member.IsArrayLike) {
                        Writer.WriteLine("else {");
                        Writer.Indent++;
                    }
                    cases++;
                    Writer.Write("state = ");
                    Writer.Write(cases.ToString(CultureInfo.InvariantCulture));
                    Writer.WriteLine(";");
                    if (member.IsArrayLike) {
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                    Writer.WriteLine("break;");
                    Writer.Indent--;
                }
            }
            if (count > 0) {
                if (isSequence)
                    Writer.WriteLine("default:");
                else 
                    Writer.WriteLine("else {");
                Writer.Indent++;
            }
            WriteMemberElementsElse(anyElement, elementElseString);
            if (count > 0) {
                if (isSequence) {
                    Writer.WriteLine("break;");
                }
                Writer.Indent--;
                Writer.WriteLine("}");
            }
        }

        string GetArraySource(TypeDesc typeDesc, string arrayName) {
            return GetArraySource(typeDesc, arrayName, false);
        }
        string GetArraySource(TypeDesc typeDesc, string arrayName, bool multiRef) {
            string a = arrayName;
            string c = "c" + a;
            string init = "";

            if (multiRef) {
                init = "soap = (System.Object[])EnsureArrayIndex(soap, " + c + "+2, typeof(System.Object)); ";
            }
            bool useReflection = typeDesc.UseReflection;
            if (typeDesc.IsArray) {
                string arrayTypeFullName = typeDesc.ArrayElementTypeDesc.CSharpName;
                bool arrayUseReflection = typeDesc.ArrayElementTypeDesc.UseReflection;
                string castString = useReflection?"":"(" + arrayTypeFullName + "[])";
                init = init + a + " = " + castString +
                    "EnsureArrayIndex(" + a + ", " + c + ", "+ RaCodeGen.GetStringForTypeof(arrayTypeFullName, arrayUseReflection) + ");";
                string arraySource = RaCodeGen.GetStringForArrayMember(a, c+"++", typeDesc);
                if (multiRef) {
                    init = init + " soap[1] = " + a + ";";
                    init = init + " if (ReadReference(out soap[" + c + "+2])) " + arraySource + " = null; else ";
                }
                return init + arraySource;
            }
            else {
                return RaCodeGen.GetStringForMethod(arrayName,typeDesc.CSharpName,"Add",useReflection);
                
            }
        }


        void WriteMemberEnd(Member[] members) {
            WriteMemberEnd(members, false);
        }

        void WriteMemberEnd(Member[] members, bool soapRefs) {
            for (int i = 0; i < members.Length; i++) {
                Member member = (Member)members[i];

                if (member.IsArrayLike) {

                    TypeDesc typeDesc = member.Mapping.TypeDesc;

                    if (typeDesc.IsArray) {

                        WriteSourceBegin(member.Source);

                        if (soapRefs)
                            Writer.Write(" soap[1] = ");

                        string a = member.ArrayName;
                        string c = "c" + a;
                        
                        bool arrayUseReflection = typeDesc.ArrayElementTypeDesc.UseReflection;
                        string arrayTypeFullName = typeDesc.ArrayElementTypeDesc.CSharpName;
                        if (!arrayUseReflection)
                            Writer.Write("(" +arrayTypeFullName +"[])");
                        Writer.Write("ShrinkArray(");
                        Writer.Write(a);
                        Writer.Write(", ");
                        Writer.Write(c);
                        Writer.Write(", ");
                        Writer.Write(RaCodeGen.GetStringForTypeof(arrayTypeFullName, arrayUseReflection));
                        Writer.Write(", ");
                        WriteBooleanValue(member.IsNullable);
                        Writer.Write(")");
                        WriteSourceEnd(member.Source);
                        Writer.WriteLine(";");

                        if (member.Mapping.ChoiceIdentifier != null) {
                            WriteSourceBegin(member.ChoiceSource);
                            a = member.ChoiceArrayName;
                            c = "c" + a;
                        
                            bool choiceUseReflection = member.Mapping.ChoiceIdentifier.Mapping.TypeDesc.UseReflection;
                            string choiceTypeName = member.Mapping.ChoiceIdentifier.Mapping.TypeDesc.CSharpName;
                            if(!choiceUseReflection)
                                Writer.Write("(" +choiceTypeName+"[])");
                            Writer.Write("ShrinkArray(");
                            Writer.Write(a);
                            Writer.Write(", ");
                            Writer.Write(c);
                            Writer.Write(", ");
                            Writer.Write(RaCodeGen.GetStringForTypeof(choiceTypeName, choiceUseReflection));
                            Writer.Write(", ");
                            WriteBooleanValue(member.IsNullable);
                            Writer.Write(")");
                            WriteSourceEnd(member.ChoiceSource);
                            Writer.WriteLine(";");
                        }

                    }
                    else if (typeDesc.IsValueType) {
                        Writer.Write(member.Source);
                        Writer.Write(" = ");
                        Writer.Write(member.ArrayName);
                        Writer.WriteLine(";");
                    }
                }
            }
        }

        void WriteSourceBeginTyped(string source, TypeDesc typeDesc) {
            WriteSourceBegin(source);
            if (typeDesc != null && !typeDesc.UseReflection) {
                Writer.Write("(");
                Writer.Write(typeDesc.CSharpName);
                Writer.Write(")");
            }
        }

        void WriteSourceBegin(string source) {
            Writer.Write(source);
            if (source[source.Length - 1] != '(' && source[source.Length - 1] != '{')
                Writer.Write(" = ");
        }
        
        void WriteSourceEnd(string source) {
            // source could be of the form "var", "arrayVar[i]",
            // "collection.Add(" or "methodInfo.Invoke(collection, new object[] {"
            if (source[source.Length - 1] == '(' )
                Writer.Write(")");
            else if( source[source.Length - 1] == '{')
                Writer.Write("})");
        }

        void WriteArray(string source, string arrayName, ArrayMapping arrayMapping, bool readOnly, bool isNullable, int fixupIndex) {
            if (arrayMapping.IsSoap) {
                Writer.Write("object rre = ");
                Writer.Write(fixupIndex >= 0 ? "ReadReferencingElement" : "ReadReferencedElement");
                Writer.Write("(");
                WriteID(arrayMapping.TypeName);
                Writer.Write(", ");
                WriteID(arrayMapping.Namespace);
                if (fixupIndex >= 0) {
                    Writer.Write(", ");
                    Writer.Write("out fixup.Ids[");
                    Writer.Write((fixupIndex).ToString(CultureInfo.InvariantCulture));
                    Writer.Write("]");
                }
                Writer.WriteLine(");");

                TypeDesc td = arrayMapping.TypeDesc;
                if (td.IsEnumerable || td.IsCollection) {
                    Writer.WriteLine("if (rre != null) {");
                    Writer.Indent++;
                    WriteAddCollectionFixup(td, readOnly, source, "rre");
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                else {
                    Writer.WriteLine("try {");
                    Writer.Indent++;
                    WriteSourceBeginTyped(source, arrayMapping.TypeDesc);
                    Writer.Write("rre");
                    WriteSourceEnd(source);
                    Writer.WriteLine(";");
                    WriteCatchCastException(arrayMapping.TypeDesc, "rre", null);
                }
            }
            else {
                Writer.WriteLine("if (!ReadNull()) {");
                Writer.Indent++;

                MemberMapping memberMapping = new MemberMapping();
                memberMapping.Elements = arrayMapping.Elements;
                memberMapping.TypeDesc = arrayMapping.TypeDesc;
                memberMapping.ReadOnly = readOnly;
                Member member = new Member(this, source, arrayName, 0, memberMapping, false);
                member.IsNullable = false;//Note, [....]: IsNullable is set to false since null condition (xsi:nil) is already handled by 'ReadNull()'

                Member[] members = new Member[] { member };
                WriteMemberBegin(members);

                if (readOnly) {
                    Writer.Write("if (((object)(");
                    Writer.Write(member.ArrayName);
                    Writer.Write(") == null) || ");
                }
                else {
                    Writer.Write("if (");
                }
                Writer.WriteLine("(Reader.IsEmptyElement)) {");
                Writer.Indent++;
                Writer.WriteLine("Reader.Skip();");
                Writer.Indent--;
                Writer.WriteLine("}");
                Writer.WriteLine("else {");
                Writer.Indent++;

                Writer.WriteLine("Reader.ReadStartElement();");
                int loopIndex = WriteWhileNotLoopStart();
                Writer.Indent++;

                string unknownNode = "UnknownNode(null, " + ExpectedElements(members) + ");";
                WriteMemberElements(members, unknownNode, unknownNode, null, null, null);
                Writer.WriteLine("Reader.MoveToContent();");

                WriteWhileLoopEnd(loopIndex);
                Writer.Indent--;
                Writer.WriteLine("ReadEndElement();");
                Writer.WriteLine("}");

                WriteMemberEnd(members, false);

                Writer.Indent--;
                Writer.WriteLine("}");
                if (isNullable) {
                    Writer.WriteLine("else {");
                    Writer.Indent++;
                    member.IsNullable = true;
                    WriteMemberBegin(members);
                    WriteMemberEnd(members);
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
            }
        }
        
        void WriteElement(string source, string arrayName, string choiceSource, ElementAccessor element, ChoiceIdentifierAccessor choice, string checkSpecified, bool checkForNull, bool readOnly, int fixupIndex, int elementIndex) {
            if (checkSpecified != null && checkSpecified.Length > 0) {
                Writer.Write(checkSpecified);
                Writer.WriteLine(" = true;");
            }

            if (element.Mapping is ArrayMapping) {
                WriteArray(source, arrayName, (ArrayMapping)element.Mapping, readOnly, element.IsNullable, fixupIndex);
            }
            else if (element.Mapping is NullableMapping) {
                string methodName = ReferenceMapping(element.Mapping);
#if DEBUG
                // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                if (methodName == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorMethod, element.Mapping.TypeDesc.Name));
#endif
                WriteSourceBegin(source);
                Writer.Write(methodName);
                Writer.Write("(true)");
                WriteSourceEnd(source);
                Writer.WriteLine(";");
            }
            else if (!element.Mapping.IsSoap && (element.Mapping is PrimitiveMapping)) {
                if (element.IsNullable) {
                    Writer.WriteLine("if (ReadNull()) {");
                    Writer.Indent++;
                    WriteSourceBegin(source);
                    if (element.Mapping.TypeDesc.IsValueType) {
                        Writer.Write(RaCodeGen.GetStringForCreateInstance(element.Mapping.TypeDesc.CSharpName, element.Mapping.TypeDesc.UseReflection, false, false));
                    }
                    else {
                        Writer.Write("null");
                    }
                    WriteSourceEnd(source);
                    Writer.WriteLine(";");
                    Writer.Indent--;
                    Writer.WriteLine("}");
                    Writer.Write("else ");
                }
                if (element.Default != null && element.Default != DBNull.Value && element.Mapping.TypeDesc.IsValueType) {
                    Writer.WriteLine("if (Reader.IsEmptyElement) {");
                    Writer.Indent++;
                    Writer.WriteLine("Reader.Skip();");
                    Writer.Indent--;
                    Writer.WriteLine("}");
                    Writer.WriteLine("else {");
                }
                else {
                    Writer.WriteLine("{");
                }
                Writer.Indent++;

                WriteSourceBegin(source);
                if (element.Mapping.TypeDesc == QnameTypeDesc)
                    Writer.Write("ReadElementQualifiedName()");
                else {
                    string readFunc; 
                    switch (element.Mapping.TypeDesc.FormatterName) {
                    case "ByteArrayBase64":
                    case "ByteArrayHex": 
                        readFunc = "false";
                        break;
                    default:              
                        readFunc = "Reader.ReadElementString()";
                        break;
                    }
                    WritePrimitive(element.Mapping, readFunc);
                }

                WriteSourceEnd(source);
                Writer.WriteLine(";");
                Writer.Indent--;
                Writer.WriteLine("}");
            }
            else if (element.Mapping is StructMapping || (element.Mapping.IsSoap && element.Mapping is PrimitiveMapping)) {
                TypeMapping mapping = element.Mapping;
                if (mapping.IsSoap) {
                    Writer.Write("object rre = ");
                    Writer.Write(fixupIndex >= 0 ? "ReadReferencingElement" : "ReadReferencedElement");
                    Writer.Write("(");
                    WriteID(mapping.TypeName);
                    Writer.Write(", ");
                    WriteID(mapping.Namespace);

                    if (fixupIndex >= 0) {
                        Writer.Write(", out fixup.Ids[");
                        Writer.Write((fixupIndex).ToString(CultureInfo.InvariantCulture));
                        Writer.Write("]");
                    }
                    Writer.Write(")");
                    WriteSourceEnd(source);
                    Writer.WriteLine(";");

                    if (mapping.TypeDesc.IsValueType) {
                        Writer.WriteLine("if (rre != null) {");
                        Writer.Indent++;
                    }

                    Writer.WriteLine("try {");
                    Writer.Indent++;
                    WriteSourceBeginTyped(source, mapping.TypeDesc);
                    Writer.Write("rre");
                    WriteSourceEnd(source);
                    Writer.WriteLine(";");
                    WriteCatchCastException(mapping.TypeDesc, "rre", null);
                    Writer.Write("Referenced(");
                    Writer.Write(source);
                    Writer.WriteLine(");");
                    if (mapping.TypeDesc.IsValueType) {
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                }
                else {
                    string methodName = ReferenceMapping(mapping);
#if DEBUG
                        // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                        if (methodName == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorMethod, mapping.TypeDesc.Name));
#endif

                    if (checkForNull) {
                        Writer.Write("if ((object)(");
                        Writer.Write(arrayName);
                        Writer.Write(") == null) Reader.Skip(); else ");
                    }
                    WriteSourceBegin(source);
                    Writer.Write(methodName);
                    Writer.Write("(");
                    if (mapping.TypeDesc.IsNullable) {
                        WriteBooleanValue(element.IsNullable);
                        Writer.Write(", ");
                    }
                    Writer.Write("true");
                    Writer.Write(")");
                    WriteSourceEnd(source);
                    Writer.WriteLine(";");
                }
            }
            else if (element.Mapping is SpecialMapping) {
                SpecialMapping special = (SpecialMapping)element.Mapping;
                switch (special.TypeDesc.Kind) {
                case TypeKind.Node:
                    bool isDoc = special.TypeDesc.FullName == typeof(XmlDocument).FullName;
                    WriteSourceBeginTyped(source, special.TypeDesc);
                    Writer.Write(isDoc ? "ReadXmlDocument(" : "ReadXmlNode(");
                    Writer.Write(element.Any ? "false" : "true");
                    Writer.Write(")");
                    WriteSourceEnd(source);
                    Writer.WriteLine(";");
                    break;
                case TypeKind.Serializable:
                    SerializableMapping sm = (SerializableMapping)element.Mapping;
                    // check to see if we need to do the derivation
                    if (sm.DerivedMappings != null) {
                        Writer.Write(typeof(XmlQualifiedName).FullName);
                        Writer.WriteLine(" tser = GetXsiType();");
                        Writer.Write("if (tser == null");
                        Writer.Write(" || ");
                        WriteQNameEqual("tser", sm.XsiType.Name, sm.XsiType.Namespace);

                        Writer.WriteLine(") {");
                        Writer.Indent++;
                    }
                    WriteSourceBeginTyped(source, sm.TypeDesc);
                    Writer.Write("ReadSerializable(( ");
                    Writer.Write(typeof(IXmlSerializable).FullName);
                    Writer.Write(")");
                    Writer.Write(RaCodeGen.GetStringForCreateInstance(sm.TypeDesc.CSharpName, sm.TypeDesc.UseReflection, sm.TypeDesc.CannotNew, false));
                    bool isWrappedAny = !element.Any && IsWildcard(sm);
                    if (isWrappedAny) {
                        Writer.WriteLine(", true");
                    }
                    Writer.Write(")");
                    WriteSourceEnd(source);
                    Writer.WriteLine(";");
                    if (sm.DerivedMappings != null) {
                        Writer.Indent--;
                        Writer.WriteLine("}");
                        WriteDerivedSerializable(sm, sm, source, isWrappedAny);
                        WriteUnknownNode("UnknownNode", "null", null, true);
                    }
                    break;
                default:
                    throw new InvalidOperationException(Res.GetString(Res.XmlInternalError));
                }
            }
            else {
                throw new InvalidOperationException(Res.GetString(Res.XmlInternalError));
            }
            if (choice != null) {
                #if DEBUG
                    // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                    if (choiceSource == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "need parent for the " + source));
                #endif

                string enumTypeName = choice.Mapping.TypeDesc.CSharpName;
                Writer.Write(choiceSource);
                Writer.Write(" = ");
                CodeIdentifier.CheckValidIdentifier(choice.MemberIds[elementIndex]);
                Writer.Write(RaCodeGen.GetStringForEnumMember(enumTypeName, choice.MemberIds[elementIndex], choice.Mapping.TypeDesc.UseReflection));
                Writer.WriteLine(";");
            }
        }

        void WriteDerivedSerializable(SerializableMapping head, SerializableMapping mapping, string source, bool isWrappedAny) {
            if (mapping == null)
                return;
            for (SerializableMapping derived = mapping.DerivedMappings; derived != null; derived = derived.NextDerivedMapping) {
                Writer.Write("else if (tser == null");
                Writer.Write(" || ");
                WriteQNameEqual("tser", derived.XsiType.Name, derived.XsiType.Namespace);

                Writer.WriteLine(") {");
                Writer.Indent++;

                if (derived.Type != null) {
                    if (head.Type.IsAssignableFrom(derived.Type)) {
                        WriteSourceBeginTyped(source, head.TypeDesc);
                        Writer.Write("ReadSerializable(( ");
                        Writer.Write(typeof(IXmlSerializable).FullName);
                        Writer.Write(")");
                        Writer.Write(RaCodeGen.GetStringForCreateInstance(derived.TypeDesc.CSharpName, derived.TypeDesc.UseReflection, derived.TypeDesc.CannotNew, false));
                        if (isWrappedAny) {
                            Writer.WriteLine(", true");
                        }
                        Writer.Write(")");
                        WriteSourceEnd(source);
                        Writer.WriteLine(";");
                    }
                    else {
                        Writer.Write("throw CreateBadDerivationException(");
                        WriteQuotedCSharpString(derived.XsiType.Name);
                        Writer.Write(", ");
                        WriteQuotedCSharpString(derived.XsiType.Namespace);
                        Writer.Write(", ");
                        WriteQuotedCSharpString(head.XsiType.Name);
                            Writer.Write(", ");
                        WriteQuotedCSharpString(head.XsiType.Namespace);
                        Writer.Write(", ");
                        WriteQuotedCSharpString(derived.Type.FullName);
                        Writer.Write(", ");
                        WriteQuotedCSharpString(head.Type.FullName);
                        Writer.WriteLine(");");
                    }
                }
                else {
                    Writer.WriteLine("// " + "missing real mapping for " + derived.XsiType);
                    Writer.Write("throw CreateMissingIXmlSerializableType(");
                    WriteQuotedCSharpString(derived.XsiType.Name);
                    Writer.Write(", ");
                    WriteQuotedCSharpString(derived.XsiType.Namespace);
                    Writer.Write(", ");
                    WriteQuotedCSharpString(head.Type.FullName);
                    Writer.WriteLine(");");
                }

                Writer.Indent--;
                Writer.WriteLine("}");

                WriteDerivedSerializable(head, derived, source, isWrappedAny);
            }
        }

        int WriteWhileNotLoopStart()
        {
            Writer.WriteLine("Reader.MoveToContent();");
            int loopIndex = WriteWhileLoopStartCheck();
            Writer.Write("while (Reader.NodeType != ");
            Writer.Write(typeof(XmlNodeType).FullName);
            Writer.Write(".EndElement && Reader.NodeType != ");
            Writer.Write(typeof(XmlNodeType).FullName);
            Writer.WriteLine(".None) {");
            return loopIndex;
        }

        void WriteWhileLoopEnd(int loopIndex)
        {
            WriteWhileLoopEndCheck(loopIndex);
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        int WriteWhileLoopStartCheck()
        {
            Writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "int whileIterations{0} = 0;", nextWhileLoopIndex));
            Writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "int readerCount{0} = ReaderCount;", nextWhileLoopIndex));
            return nextWhileLoopIndex++;
        }

        void WriteWhileLoopEndCheck(int loopIndex)
        {
            Writer.WriteLine(String.Format(CultureInfo.InvariantCulture, "CheckReaderCount(ref whileIterations{0}, ref readerCount{1});", loopIndex, loopIndex));
        }

        void WriteParamsRead(int length) {
            Writer.Write("bool[] paramsRead = new bool[");
            Writer.Write(length.ToString(CultureInfo.InvariantCulture));
            Writer.WriteLine("];");
        }
        
        void WriteReadNonRoots() {
            Writer.WriteLine("Reader.MoveToContent();");
            int loopIndex = WriteWhileLoopStartCheck();
            Writer.Write("while (Reader.NodeType == ");
            Writer.Write(typeof(XmlNodeType).FullName);
            Writer.WriteLine(".Element) {");
            Writer.Indent++;
            Writer.Write("string root = Reader.GetAttribute(\"root\", \"");
            Writer.Write(Soap.Encoding);
            Writer.WriteLine("\");");
            Writer.Write("if (root == null || ");
            Writer.Write(typeof(XmlConvert).FullName);
            Writer.WriteLine(".ToBoolean(root)) break;");
            Writer.WriteLine("ReadReferencedElement();");
            Writer.WriteLine("Reader.MoveToContent();");
            WriteWhileLoopEnd(loopIndex);
        }

        void WriteBooleanValue(bool value) {
            Writer.Write(value ? "true" : "false");
        }

        void WriteInitCheckTypeHrefList(string source) {
            Writer.Write(typeof(ArrayList).FullName);
            Writer.Write(" ");
            Writer.Write(source);
            Writer.Write(" = new ");
            Writer.Write(typeof(ArrayList).FullName);
            Writer.WriteLine("();");

            Writer.Write(typeof(ArrayList).FullName);
            Writer.Write(" ");
            Writer.Write(source);
            Writer.Write("IsObject = new ");
            Writer.Write(typeof(ArrayList).FullName);
            Writer.WriteLine("();");
        }
        
        void WriteHandleHrefList(Member[] members, string listSource) {
            Writer.WriteLine("int isObjectIndex = 0;");
            Writer.Write("foreach (object obj in ");
            Writer.Write(listSource);
            Writer.WriteLine(") {");
            Writer.Indent++;
            Writer.WriteLine("bool isReferenced = true;");
            Writer.Write("bool isObject = (bool)");
            Writer.Write(listSource);
            Writer.WriteLine("IsObject[isObjectIndex++];");
            Writer.WriteLine("object refObj = isObject ? obj : GetTarget((string)obj);");
            Writer.WriteLine("if (refObj == null) continue;");
            Writer.Write(typeof(Type).FullName);
            Writer.WriteLine(" refObjType = refObj.GetType();");
            Writer.WriteLine("string refObjId = null;");

            WriteMemberElementsIf(members, null, "isReferenced = false;", "refObj");

            Writer.WriteLine("if (isObject && isReferenced) Referenced(refObj); // need to mark this obj as ref'd since we didn't do GetTarget");
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteIfNotSoapRoot(string source) {
            Writer.Write("if (Reader.GetAttribute(\"root\", \"");
            Writer.Write(Soap.Encoding);
            Writer.WriteLine("\") == \"0\") {");
            Writer.Indent++;
            Writer.WriteLine(source);
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteCreateMapping(TypeMapping mapping, string local) {
            string fullTypeName = mapping.TypeDesc.CSharpName;
            bool useReflection = mapping.TypeDesc.UseReflection;
            bool ctorInaccessible = mapping.TypeDesc.CannotNew;

            Writer.Write(useReflection ? "object" : fullTypeName);
            Writer.Write(" ");
            Writer.Write(local);
            Writer.WriteLine(";");

            if (ctorInaccessible) {
                Writer.WriteLine("try {");
                Writer.Indent++;
            }
            Writer.Write(local);
            Writer.Write(" = ");
            Writer.Write(RaCodeGen.GetStringForCreateInstance(fullTypeName, useReflection, mapping.TypeDesc.CannotNew, true));
            Writer.WriteLine(";");
            if (ctorInaccessible) {
                WriteCatchException(typeof(MissingMethodException));
                Writer.Indent++;
                Writer.Write("throw CreateInaccessibleConstructorException(");
                WriteQuotedCSharpString(fullTypeName);
                Writer.WriteLine(");");

                WriteCatchException(typeof(SecurityException));
                Writer.Indent++;

                Writer.Write("throw CreateCtorHasSecurityException(");
                WriteQuotedCSharpString(fullTypeName);
                Writer.WriteLine(");");

                Writer.Indent--;
                Writer.WriteLine("}");
            }
        }

        void WriteCatchException(Type exceptionType) {
            Writer.Indent--;
            Writer.WriteLine("}");
            Writer.Write("catch (");
            Writer.Write(exceptionType.FullName);
            Writer.WriteLine(") {");
        }

        void WriteCatchCastException(TypeDesc typeDesc, string source, string id) {
            WriteCatchException(typeof(InvalidCastException));
            Writer.Indent++;
            Writer.Write("throw CreateInvalidCastException(");
            Writer.Write(RaCodeGen.GetStringForTypeof(typeDesc.CSharpName, typeDesc.UseReflection));
            Writer.Write(", ");
            Writer.Write(source);
            if (id == null)
                Writer.WriteLine(", null);");
            else {
                Writer.Write(", (string)");
                Writer.Write(id);
                Writer.WriteLine(");");
            }
            Writer.Indent--;
            Writer.WriteLine("}");
        }
        void WriteArrayLocalDecl( string typeName, string variableName, string initValue, TypeDesc arrayTypeDesc) {
            RaCodeGen.WriteArrayLocalDecl(typeName, variableName, initValue, arrayTypeDesc);
        }
        void WriteCreateInstance(string escapedName, string source, bool useReflection, bool ctorInaccessible){
            RaCodeGen.WriteCreateInstance(escapedName, source, useReflection, ctorInaccessible);
        }
        void WriteLocalDecl(string typeFullName, string variableName, string initValue, bool useReflection) {
            RaCodeGen.WriteLocalDecl(typeFullName, variableName, initValue, useReflection);
        }
    }
}
