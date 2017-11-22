//------------------------------------------------------------------------------
// <copyright file="XmlSerializationWriter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization {

    using System;
    using System.IO;
    using System.Collections;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Xml.Schema;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.CodeDom.Compiler;
    using System.Globalization;
    using System.Text;
    using System.Threading;
    using System.Security.Permissions;
    using System.Runtime.Versioning;
   
    /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter"]/*' />
    ///<internalonly/>
    public abstract class XmlSerializationWriter : XmlSerializationGeneratedCode {
        XmlWriter w;
        XmlSerializerNamespaces namespaces;
        int tempNamespacePrefix;
        Hashtable usedPrefixes;
        Hashtable references;
        string idBase;
        int nextId;
        Hashtable typeEntries;
        ArrayList referencesToWrite;
        Hashtable objectsInUse;
        string aliasBase = "q";
        bool soap12;
        bool escapeName = true;

        // this method must be called before any generated serialization methods are called
        internal void Init(XmlWriter w, XmlSerializerNamespaces namespaces, string encodingStyle, string idBase, TempAssembly tempAssembly) {
            this.w = w;
            this.namespaces = namespaces;
            this.soap12 = (encodingStyle == Soap12.Encoding);
            this.idBase = idBase;
            Init(tempAssembly);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.EscapeName"]/*' />
        protected bool EscapeName {
            get {
                return escapeName;
            }
            set {
                escapeName = value;
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.Writer"]/*' />
        protected XmlWriter Writer {
            get {
                return w;
            }
            set {
                w = value;
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.Namespaces"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected ArrayList Namespaces {
            get {
                return namespaces == null ? null : namespaces.NamespaceList;
            }
            set {
                if (value == null) {
                    namespaces = null;
                }
                else {
                    XmlQualifiedName[] qnames = (XmlQualifiedName[])value.ToArray(typeof(XmlQualifiedName));
                    namespaces = new XmlSerializerNamespaces(qnames);
                }
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromByteArrayBase64"]/*' />
        protected static byte[] FromByteArrayBase64(byte[] value) {
            // Unlike other "From" functions that one is just a place holder for automatic code generation.
            // The reason is performance and memory consumption for (potentially) big 64base-encoded chunks
            // And it is assumed that the caller generates the code that will distinguish between byte[] and string return types
            //
            return value;
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.ResolveDynamicAssembly"]/*' />
        ///<internalonly/>
        protected static Assembly ResolveDynamicAssembly(string assemblyFullName){
            return DynamicAssemblies.Get(assemblyFullName);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromByteArrayHex"]/*' />
        protected static string FromByteArrayHex(byte[] value) {
            return XmlCustomFormatter.FromByteArrayHex(value);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromDateTime"]/*' />
        protected static string FromDateTime(DateTime value) {
            return XmlCustomFormatter.FromDateTime(value);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromDate"]/*' />
        protected static string FromDate(DateTime value) {
            return XmlCustomFormatter.FromDate(value);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromTime"]/*' />
        protected static string FromTime(DateTime value) {
            return XmlCustomFormatter.FromTime(value);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromChar"]/*' />
        protected static string FromChar(char value) {
            return XmlCustomFormatter.FromChar(value);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromEnum"]/*' />
        protected static string FromEnum(long value, string[] values, long[] ids) {
            return XmlCustomFormatter.FromEnum(value, values, ids, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromEnum1"]/*' />
        protected static string FromEnum(long value, string[] values, long[] ids, string typeName) {
            return XmlCustomFormatter.FromEnum(value, values, ids, typeName);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromXmlName"]/*' />
        protected static string FromXmlName(string name) {
            return XmlCustomFormatter.FromXmlName(name);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromXmlNCName"]/*' />
        protected static string FromXmlNCName(string ncName) {
            return XmlCustomFormatter.FromXmlNCName(ncName);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromXmlNmToken"]/*' />
        protected static string FromXmlNmToken(string nmToken) {
            return XmlCustomFormatter.FromXmlNmToken(nmToken);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromXmlNmTokens"]/*' />
        protected static string FromXmlNmTokens(string nmTokens) {
            return XmlCustomFormatter.FromXmlNmTokens(nmTokens);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteXsiType"]/*' />
        protected void WriteXsiType(string name, string ns) {
            WriteAttribute("type", XmlSchema.InstanceNamespace, GetQualifiedName(name, ns));
        }

        XmlQualifiedName GetPrimitiveTypeName(Type type) {
            return GetPrimitiveTypeName(type, true);
        }

        XmlQualifiedName GetPrimitiveTypeName(Type type, bool throwIfUnknown) {
            XmlQualifiedName qname = GetPrimitiveTypeNameInternal(type);
            if (throwIfUnknown && qname == null)
                throw CreateUnknownTypeException(type);
            return qname;
        }

        internal static XmlQualifiedName GetPrimitiveTypeNameInternal(Type type) {
            string typeName;
            string typeNs = XmlSchema.Namespace;
            
            switch (Type.GetTypeCode(type)) {
            case TypeCode.String: typeName = "string"; break;
            case TypeCode.Int32: typeName = "int"; break;
            case TypeCode.Boolean: typeName = "boolean"; break;
            case TypeCode.Int16: typeName = "short"; break;
            case TypeCode.Int64: typeName = "long"; break;
            case TypeCode.Single: typeName = "float"; break;
            case TypeCode.Double: typeName = "double"; break;
            case TypeCode.Decimal: typeName = "decimal"; break;
            case TypeCode.DateTime: typeName = "dateTime"; break;
            case TypeCode.Byte: typeName = "unsignedByte"; break;
            case TypeCode.SByte: typeName = "byte"; break;
            case TypeCode.UInt16: typeName = "unsignedShort"; break;
            case TypeCode.UInt32: typeName = "unsignedInt"; break;
            case TypeCode.UInt64: typeName = "unsignedLong"; break;
            case TypeCode.Char: 
                typeName = "char"; 
                typeNs = UrtTypes.Namespace;
                break;
            default:
                    if (type == typeof(XmlQualifiedName)) typeName = "QName";
                    else if (type == typeof(byte[])) typeName = "base64Binary";
                    else if (type == typeof(TimeSpan) && LocalAppContextSwitches.EnableTimeSpanSerialization)
                        typeName = "TimeSpan";
                    else if (type == typeof(Guid)) {
                        typeName = "guid";
                        typeNs = UrtTypes.Namespace;
                    }
                    else if (type == typeof(XmlNode[])) {
                        typeName = Soap.UrType;
                    }
                    else
                        return null;
                break;
            }
            return new XmlQualifiedName(typeName, typeNs);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteTypedPrimitive"]/*' />
        protected void WriteTypedPrimitive(string name, string ns, object o, bool xsiType) {
            string value = null;
            string type;
            string typeNs = XmlSchema.Namespace;
            bool writeRaw = true;
            bool writeDirect = false;
            Type t = o.GetType();
            bool wroteStartElement = false;

            switch (Type.GetTypeCode(t)) {
            case TypeCode.String:
                value = (string)o;
                type = "string";
                writeRaw = false;
                break;
            case TypeCode.Int32:
                value = XmlConvert.ToString((int)o);
                type = "int";
                break;
            case TypeCode.Boolean:
                value = XmlConvert.ToString((bool)o);
                type = "boolean";
                break;
            case TypeCode.Int16:
                value = XmlConvert.ToString((short)o);
                type = "short";
                break;
            case TypeCode.Int64:
                value = XmlConvert.ToString((long)o);
                type = "long";
                break;
            case TypeCode.Single:
                value = XmlConvert.ToString((float)o);
                type = "float";
                break;
            case TypeCode.Double:
                value = XmlConvert.ToString((double)o);
                type = "double";
                break;
            case TypeCode.Decimal:
                value = XmlConvert.ToString((decimal)o);
                type = "decimal";
                break;
            case TypeCode.DateTime:
                value = FromDateTime((DateTime)o);
                type = "dateTime";
                break;
            case TypeCode.Char:
                value = FromChar((char)o);
                type = "char";
                typeNs = UrtTypes.Namespace;
                break;
            case TypeCode.Byte:
                value = XmlConvert.ToString((byte)o);
                type = "unsignedByte";
                break;
            case TypeCode.SByte:
                value = XmlConvert.ToString((sbyte)o);
                type = "byte";
                break;
            case TypeCode.UInt16:
                value = XmlConvert.ToString((UInt16)o);
                type = "unsignedShort";
                break;
            case TypeCode.UInt32:
                value = XmlConvert.ToString((UInt32)o);
                type = "unsignedInt";
                break;
            case TypeCode.UInt64:
                value = XmlConvert.ToString((UInt64)o);
                type = "unsignedLong";
                break;

            default:
                if (t == typeof(XmlQualifiedName)) {
                    type = "QName";
                    // need to write start element ahead of time to establish context
                    // for ns definitions by FromXmlQualifiedName
                    wroteStartElement = true;
                    if (name == null)
                        w.WriteStartElement(type, typeNs);
                    else
                        w.WriteStartElement(name, ns);
                    value = FromXmlQualifiedName((XmlQualifiedName)o, false);
                }
                else if (t == typeof(byte[])) {
                    value = String.Empty;                    
                    writeDirect = true;
                    type = "base64Binary";
                }
                else if (t == typeof(Guid)) {
                    value = XmlConvert.ToString((Guid)o);
                    type = "guid";
                    typeNs = UrtTypes.Namespace;
                }
                else if (t == typeof(TimeSpan) && LocalAppContextSwitches.EnableTimeSpanSerialization) {
                    value = XmlConvert.ToString((TimeSpan)o);
                    type = "TimeSpan";
                }
                else if (typeof(XmlNode[]).IsAssignableFrom(t)){
                    if (name == null)
                        w.WriteStartElement(Soap.UrType, XmlSchema.Namespace);
                    else
                        w.WriteStartElement(name, ns);

                    XmlNode[] xmlNodes = (XmlNode[])o;
                    for (int i=0;i<xmlNodes.Length;i++){
                        if (xmlNodes[i] == null)
                            continue;
                        xmlNodes[i].WriteTo(w);
                    }
                    w.WriteEndElement();
                    return;
                }
                else
                    throw CreateUnknownTypeException(t);
                break;
            }
            if (!wroteStartElement) {
                if (name == null)
                    w.WriteStartElement(type, typeNs);
                else
                    w.WriteStartElement(name, ns);
            }

            if (xsiType) WriteXsiType(type, typeNs);

            if (value == null) {
                w.WriteAttributeString("nil", XmlSchema.InstanceNamespace, "true");
            }
            else if (writeDirect) {
                // only one type currently writes directly to XML stream
                XmlCustomFormatter.WriteArrayBase64(w, (byte[])o, 0,((byte[])o).Length);
            }
            else if(writeRaw) {
                w.WriteRaw(value);
            }
            else
                w.WriteString(value);
            w.WriteEndElement();
        }

        string GetQualifiedName(string name, string ns) {
            if (ns == null || ns.Length == 0) return name;
            string prefix = w.LookupPrefix(ns);
            if (prefix == null) {
                if (ns == XmlReservedNs.NsXml) {
                    prefix = "xml";
                }
                else {
                    prefix = NextPrefix();
                    WriteAttribute("xmlns", prefix, null, ns);
                }
            }
            else if (prefix.Length == 0) {
                return name;
            }
            return prefix + ":" + name;
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromXmlQualifiedName"]/*' />
        protected string FromXmlQualifiedName(XmlQualifiedName xmlQualifiedName) {
            return FromXmlQualifiedName(xmlQualifiedName, true);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.FromXmlQualifiedName"]/*' />
        protected string FromXmlQualifiedName(XmlQualifiedName xmlQualifiedName, bool ignoreEmpty) {
            if (xmlQualifiedName == null) return null;
            if (xmlQualifiedName.IsEmpty && ignoreEmpty) return null;
            return GetQualifiedName(EscapeName ? XmlConvert.EncodeLocalName(xmlQualifiedName.Name) : xmlQualifiedName.Name, xmlQualifiedName.Namespace);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteStartElement"]/*' />
        protected void WriteStartElement(string name) {
            WriteStartElement(name, null, null, false, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteStartElement1"]/*' />
        protected void WriteStartElement(string name, string ns) {
            WriteStartElement(name, ns, null, false, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteStartElement4"]/*' />
        protected void WriteStartElement(string name, string ns, bool writePrefixed) {
            WriteStartElement(name, ns, null, writePrefixed, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteStartElement2"]/*' />
        protected void WriteStartElement(string name, string ns, object o) {
            WriteStartElement(name, ns, o, false, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteStartElement3"]/*' />
        protected void WriteStartElement(string name, string ns, object o, bool writePrefixed) {
            WriteStartElement(name, ns, o, writePrefixed, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteStartElement5"]/*' />
        protected void WriteStartElement(string name, string ns, object o, bool writePrefixed, XmlSerializerNamespaces xmlns) {
            if (o != null && objectsInUse != null) {
                if (objectsInUse.ContainsKey(o)) throw new InvalidOperationException(Res.GetString(Res.XmlCircularReference, o.GetType().FullName));
                objectsInUse.Add(o, o);
            }

            string prefix = null;
            bool needEmptyDefaultNamespace = false;
            if (namespaces != null) {
                foreach(string alias in namespaces.Namespaces.Keys) {
                    string aliasNs = (string)namespaces.Namespaces[alias];

                    if (alias.Length > 0 && aliasNs == ns)
                        prefix = alias;
                    if (alias.Length == 0) {
                        if (aliasNs == null || aliasNs.Length == 0)
                            needEmptyDefaultNamespace = true;
                        if (ns != aliasNs)
                            writePrefixed = true;
                    }
                }
                usedPrefixes = ListUsedPrefixes(namespaces.Namespaces, aliasBase);
            }
            if (writePrefixed && prefix == null && ns != null && ns.Length > 0) {
                prefix = w.LookupPrefix(ns);
                if (prefix == null || prefix.Length == 0) {
                    prefix = NextPrefix();
                }
            }
            if (prefix == null && xmlns != null) {
                prefix = xmlns.LookupPrefix(ns);
            }
            if (needEmptyDefaultNamespace && prefix == null && ns != null && ns.Length != 0)
                prefix = NextPrefix();
            w.WriteStartElement(prefix, name, ns);
            if (namespaces != null) {
                foreach(string alias in namespaces.Namespaces.Keys) {
                    string aliasNs = (string)namespaces.Namespaces[alias];
                    if (alias.Length == 0 && (aliasNs == null || aliasNs.Length == 0))
                        continue;
                    if (aliasNs == null || aliasNs.Length == 0) {
                        if (alias.Length > 0)
                            throw new InvalidOperationException(Res.GetString(Res.XmlInvalidXmlns, alias));
                        WriteAttribute("xmlns", alias, null, aliasNs);
                    }
                    else {
                        if (w.LookupPrefix(aliasNs) == null) {
                            // write the default namespace declaration only if we have not written it already, over wise we just ignore one provided by the user
                            if (prefix == null && alias.Length == 0)
                                break;
                            WriteAttribute("xmlns", alias, null, aliasNs);
                        }
                    }
                }
            }
            WriteNamespaceDeclarations(xmlns);
        }

        Hashtable ListUsedPrefixes(Hashtable nsList, string prefix) {
            Hashtable qnIndexes = new Hashtable();
            int prefixLength = prefix.Length;
            const string MaxInt32 = "2147483647";
            foreach(string alias in namespaces.Namespaces.Keys) {
                string name;
                if (alias.Length > prefixLength) {
                    name = alias;
                    int nameLength = name.Length;
                    if (name.Length > prefixLength && name.Length <= prefixLength + MaxInt32.Length && name.StartsWith(prefix, StringComparison.Ordinal)) {
                        bool numeric = true;
                        for (int j = prefixLength; j < name.Length; j++) {
                            if (!Char.IsDigit(name, j)) {
                                numeric = false;
                                break;
                            }
                        }
                        if (numeric) {
                            Int64 index = Int64.Parse(name.Substring(prefixLength), CultureInfo.InvariantCulture);
                            if (index <= Int32.MaxValue) {
                                Int32 newIndex = (Int32)index;
                                if (!qnIndexes.ContainsKey(newIndex)) {
                                    qnIndexes.Add(newIndex, newIndex);
                                }
                            }
                        }
                    }
                }
            }
            if (qnIndexes.Count > 0) {
                return qnIndexes;
            }
            return null;
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullTagEncoded"]/*' />
        protected void WriteNullTagEncoded(string name) {
            WriteNullTagEncoded(name, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullTagEncoded1"]/*' />
        protected void WriteNullTagEncoded(string name, string ns) {
            if (name == null || name.Length == 0)
                return;
            WriteStartElement(name, ns, null, true);
            w.WriteAttributeString("nil", XmlSchema.InstanceNamespace, "true");
            w.WriteEndElement();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullTagLiteral"]/*' />
        protected void WriteNullTagLiteral(string name) {
            WriteNullTagLiteral(name, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullTag1"]/*' />
        protected void WriteNullTagLiteral(string name, string ns) {
            if (name == null || name.Length == 0)
                return;
            WriteStartElement(name, ns, null, false);
            w.WriteAttributeString("nil", XmlSchema.InstanceNamespace, "true");
            w.WriteEndElement();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteEmptyTag"]/*' />
        protected void WriteEmptyTag(string name) {
            WriteEmptyTag(name, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteEmptyTag1"]/*' />
        protected void WriteEmptyTag(string name, string ns) {
            if (name == null || name.Length == 0)
                return;
            WriteStartElement(name, ns, null, false);
            w.WriteEndElement();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteEndElement"]/*' />
        protected void WriteEndElement() {
            w.WriteEndElement();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteEndElement1"]/*' />
        protected void WriteEndElement(object o) {
            w.WriteEndElement();

            if (o != null && objectsInUse != null) {
#if DEBUG
                    // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                    if (!objectsInUse.ContainsKey(o)) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "missing stack object of type " + o.GetType().FullName));
#endif

                objectsInUse.Remove(o);
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteSerializable"]/*' />
        protected void WriteSerializable(IXmlSerializable serializable, string name, string ns, bool isNullable) {
            WriteSerializable(serializable, name, ns, isNullable, true);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteSerializable1"]/*' />
        protected void WriteSerializable(IXmlSerializable serializable, string name, string ns, bool isNullable, bool wrapped) {
            if (serializable == null) {
                if (isNullable) WriteNullTagLiteral(name, ns);
                return;
            }
            if (wrapped) {
                w.WriteStartElement(name, ns);
            }
            serializable.WriteXml(w);
            if (wrapped) {
                w.WriteEndElement();
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullableStringEncoded"]/*' />
        protected void WriteNullableStringEncoded(string name, string ns, string value, XmlQualifiedName xsiType) {
            if (value == null)
                WriteNullTagEncoded(name, ns);
            else
                WriteElementString(name, ns, value, xsiType);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullableStringLiteral"]/*' />
        protected void WriteNullableStringLiteral(string name, string ns, string value) {
            if (value == null)
                WriteNullTagLiteral(name, ns);
            else
                WriteElementString(name, ns, value, null);
        }
        

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullableStringEncodedRaw"]/*' />
        protected void WriteNullableStringEncodedRaw(string name, string ns, string value, XmlQualifiedName xsiType) {
            if (value == null)
                WriteNullTagEncoded(name, ns);
            else
                WriteElementStringRaw(name, ns, value, xsiType);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullableStringEncodedRaw1"]/*' />
        protected void WriteNullableStringEncodedRaw(string name, string ns, byte[] value, XmlQualifiedName xsiType) {
            if (value == null)
                WriteNullTagEncoded(name, ns);
            else
                WriteElementStringRaw(name, ns, value, xsiType);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullableStringLiteralRaw"]/*' />
        protected void WriteNullableStringLiteralRaw(string name, string ns, string value) {
            if (value == null)
                WriteNullTagLiteral(name, ns);
            else
                WriteElementStringRaw(name, ns, value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullableStringLiteralRaw1"]/*' />
        protected void WriteNullableStringLiteralRaw(string name, string ns, byte[] value) {
            if (value == null)
                WriteNullTagLiteral(name, ns);
            else
                WriteElementStringRaw(name, ns, value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullableQualifiedNameEncoded"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void WriteNullableQualifiedNameEncoded(string name, string ns, XmlQualifiedName value, XmlQualifiedName xsiType) {
            if (value == null)
                WriteNullTagEncoded(name, ns);
            else
                WriteElementQualifiedName(name, ns, value, xsiType);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNullableQualifiedNameLiteral"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void WriteNullableQualifiedNameLiteral(string name, string ns, XmlQualifiedName value) {
            if (value == null)
                WriteNullTagLiteral(name, ns);
            else
                WriteElementQualifiedName(name, ns, value, null);
        }

        
        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementEncoded"]/*' />
        protected void WriteElementEncoded(XmlNode node, string name, string ns, bool isNullable, bool any) {
            if (node == null) {
                if (isNullable) WriteNullTagEncoded(name, ns);
                return;
            }
            WriteElement(node, name, ns, isNullable, any);
        }
        
        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementLiteral"]/*' />
        protected void WriteElementLiteral(XmlNode node, string name, string ns, bool isNullable, bool any) {
            if (node == null) {
                if (isNullable) WriteNullTagLiteral(name, ns);
                return;
            }
            WriteElement(node, name, ns, isNullable, any);
        }
        
        private void WriteElement(XmlNode node, string name, string ns, bool isNullable, bool any) {
            if (typeof(XmlAttribute).IsAssignableFrom(node.GetType()))
                throw new InvalidOperationException(Res.GetString(Res.XmlNoAttributeHere));
            if (node is XmlDocument) {
                node = ((XmlDocument)node).DocumentElement;
                if (node == null) {
                    if (isNullable) WriteNullTagEncoded(name, ns);
                    return;
                }
            }
            if (any) {
                if (node is XmlElement && name != null && name.Length > 0) {
                    // need to check against schema
                    if (node.LocalName != name || node.NamespaceURI != ns)
                        throw new InvalidOperationException(Res.GetString(Res.XmlElementNameMismatch, node.LocalName, node.NamespaceURI, name, ns));
                }
            }
            else
                w.WriteStartElement(name, ns);

            node.WriteTo(w);

            if (!any)
                w.WriteEndElement();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.CreateUnknownTypeException"]/*' />
        protected Exception CreateUnknownTypeException(object o) {
            return CreateUnknownTypeException(o.GetType());
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.CreateUnknownTypeException1"]/*' />
        protected Exception CreateUnknownTypeException(Type type) {
            if (typeof(IXmlSerializable).IsAssignableFrom(type)) return new InvalidOperationException(Res.GetString(Res.XmlInvalidSerializable, type.FullName));
            TypeDesc typeDesc = new TypeScope().GetTypeDesc(type);
            if (!typeDesc.IsStructLike) return new InvalidOperationException(Res.GetString(Res.XmlInvalidUseOfType, type.FullName));
            return new InvalidOperationException(Res.GetString(Res.XmlUnxpectedType, type.FullName));
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.CreateMismatchChoiceException"]/*' />
        protected Exception CreateMismatchChoiceException(string value, string elementName, string enumValue) {
            // Value of {0} mismatches the type of {1}, you need to set it to {2}.
            return new InvalidOperationException(Res.GetString(Res.XmlChoiceMismatchChoiceException, elementName, value, enumValue));
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.CreateUnknownAnyElementException"]/*' />
        protected Exception CreateUnknownAnyElementException(string name, string ns) {
            return new InvalidOperationException(Res.GetString(Res.XmlUnknownAnyElement, name, ns));
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.CreateInvalidChoiceIdentifierValueException"]/*' />
        protected Exception CreateInvalidChoiceIdentifierValueException(string type, string identifier) {
            return new InvalidOperationException(Res.GetString(Res.XmlInvalidChoiceIdentifierValue, type, identifier));
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.CreateChoiceIdentifierValueException"]/*' />
        protected Exception CreateChoiceIdentifierValueException(string value, string identifier, string name, string ns) {
            // XmlChoiceIdentifierMismatch=Value '{0}' of the choice identifier '{1}' does not match element '{2}' from namespace '{3}'.
            return new InvalidOperationException(Res.GetString(Res.XmlChoiceIdentifierMismatch, value, identifier, name, ns));
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.CreateInvalidEnumValueException"]/*' />
        protected Exception CreateInvalidEnumValueException(object value, string typeName) {
            return new InvalidOperationException(Res.GetString(Res.XmlUnknownConstant, value, typeName));
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.CreateInvalidAnyTypeException"]/*' />
        protected Exception CreateInvalidAnyTypeException(object o) {
            return CreateInvalidAnyTypeException(o.GetType());
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.CreateInvalidAnyTypeException1"]/*' />
        protected Exception CreateInvalidAnyTypeException(Type type) {
            return new InvalidOperationException(Res.GetString(Res.XmlIllegalAnyElement, type.FullName));
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteReferencingElement"]/*' />
        protected void WriteReferencingElement(string n, string ns, object o) {
            WriteReferencingElement(n, ns, o, false);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteReferencingElement1"]/*' />
        protected void WriteReferencingElement(string n, string ns, object o, bool isNullable) {
            if (o == null) {
                if (isNullable) WriteNullTagEncoded(n, ns);
                return;
            }
            WriteStartElement(n, ns, null, true);
            if (soap12)
                w.WriteAttributeString("ref", Soap12.Encoding, GetId(o, true));
            else
                w.WriteAttributeString("href", "#" + GetId(o, true));
            w.WriteEndElement();
        }

        bool IsIdDefined(object o) {
            if (references != null) return references.Contains(o);
            else return false;
        }

        string GetId(object o, bool addToReferencesList) {
            if (references == null) {
                references = new Hashtable();
                referencesToWrite = new ArrayList();
            }
            string id = (string)references[o];
            if (id == null) {
                id = idBase + "id" + (++nextId).ToString(CultureInfo.InvariantCulture);
                references.Add(o, id);
                if (addToReferencesList) referencesToWrite.Add(o);
            }
            return id;
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteId"]/*' />
        protected void WriteId(object o) {
            WriteId(o, true);
        }

        void WriteId(object o, bool addToReferencesList) {
            if (soap12)
                w.WriteAttributeString("id", Soap12.Encoding, GetId(o, addToReferencesList));
            else
                w.WriteAttributeString("id", GetId(o, addToReferencesList));
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteXmlAttribute1"]/*' />
        protected void WriteXmlAttribute(XmlNode node) {
            WriteXmlAttribute(node, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteXmlAttribute2"]/*' />
        protected void WriteXmlAttribute(XmlNode node, object container) {
            XmlAttribute attr = node as XmlAttribute;
            if (attr == null) throw new InvalidOperationException(Res.GetString(Res.XmlNeedAttributeHere));
            if (attr.Value != null) {
                if (attr.NamespaceURI == Wsdl.Namespace && attr.LocalName == Wsdl.ArrayType) {
                    string dims;
                    XmlQualifiedName qname = TypeScope.ParseWsdlArrayType(attr.Value, out dims, (container is XmlSchemaObject) ? (XmlSchemaObject)container : null);

                    string value = FromXmlQualifiedName(qname, true) + dims;

                    //<xsd:attribute xmlns:q3="s0" wsdl:arrayType="q3:FoosBase[]" xmlns:q4="http://schemas.xmlsoap.org/soap/encoding/" ref="q4:arrayType" />
                    WriteAttribute(Wsdl.ArrayType, Wsdl.Namespace, value);
                }
                else {
                    WriteAttribute(attr.Name, attr.NamespaceURI, attr.Value);
                }
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteAttribute"]/*' />
        protected void WriteAttribute(string localName, string ns, string value) {
            if (value == null) return;
            if (localName == "xmlns" || localName.StartsWith("xmlns:", StringComparison.Ordinal)) {
                ;
            }
            else {
                int colon = localName.IndexOf(':');
                if (colon < 0) {
                    if (ns == XmlReservedNs.NsXml) {
                        string prefix = w.LookupPrefix(ns);
                        if (prefix == null || prefix.Length == 0)
                            prefix = "xml";
                        w.WriteAttributeString(prefix, localName, ns, value);
                    }
                    else {
                        w.WriteAttributeString(localName, ns, value);
                    }
                }
                else {
                    string prefix = localName.Substring(0, colon);
                    w.WriteAttributeString(prefix, localName.Substring(colon+1), ns, value);
                }
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteAttribute0"]/*' />
        protected void WriteAttribute(string localName, string ns, byte[] value) {
            if (value == null) return;
            if (localName == "xmlns" || localName.StartsWith("xmlns:", StringComparison.Ordinal)) {
                ;
            }
            else {
                int colon = localName.IndexOf(':');
                if (colon < 0) {
                    if (ns == XmlReservedNs.NsXml) {
                        string prefix = w.LookupPrefix(ns);
                        if (prefix == null || prefix.Length == 0)
                            prefix = "xml";
                        w.WriteStartAttribute("xml", localName, ns);
                    }
                    else {
                        w.WriteStartAttribute(null, localName, ns);
                    }
                }
                else {
                    string prefix = localName.Substring(0, colon);
                    prefix = w.LookupPrefix(ns);
                    w.WriteStartAttribute(prefix, localName.Substring(colon+1), ns);
                }
                XmlCustomFormatter.WriteArrayBase64(w, value, 0, value.Length);
                w.WriteEndAttribute();
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteAttribute1"]/*' />
        protected void WriteAttribute(string localName, string value) {
            if (value == null) return;
            w.WriteAttributeString(localName, null, value);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteAttribute01"]/*' />
        protected void WriteAttribute(string localName, byte[] value) {
            if (value == null) return;

            w.WriteStartAttribute(null, localName, (string)null);
            XmlCustomFormatter.WriteArrayBase64(w, value, 0, value.Length);
            w.WriteEndAttribute();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteAttribute2"]/*' />
        protected void WriteAttribute(string prefix, string localName, string ns, string value) {
            if (value == null) return;
            w.WriteAttributeString(prefix, localName, null, value);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteValue"]/*' />
        protected void WriteValue(string value) {
            if (value == null) return;
            w.WriteString(value);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteValue01"]/*' />
        protected void WriteValue(byte[] value) {
            if (value == null) return;
            XmlCustomFormatter.WriteArrayBase64(w, value, 0, value.Length);
        }
        
        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteStartDocument"]/*' />
        protected void WriteStartDocument() {
            if (w.WriteState == WriteState.Start) {
                w.WriteStartDocument();
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementString"]/*' />
        protected void WriteElementString(String localName, String value) {
            WriteElementString(localName, null, value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementString1"]/*' />
        protected void WriteElementString(String localName, String ns, String value) {
            WriteElementString(localName, ns, value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementString2"]/*' />
        protected void WriteElementString(String localName, String value, XmlQualifiedName xsiType) {
            WriteElementString(localName, null, value, xsiType);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementString3"]/*' />
        protected void WriteElementString(String localName, String ns, String value, XmlQualifiedName xsiType) {
            if (value == null) return;
            if (xsiType == null)
                w.WriteElementString(localName, ns, value);
            else {
                w.WriteStartElement(localName, ns);
                WriteXsiType(xsiType.Name, xsiType.Namespace);
                w.WriteString(value);
                w.WriteEndElement();
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementStringRaw"]/*' />
        protected void WriteElementStringRaw(String localName, String value) {
            WriteElementStringRaw(localName,null,value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementStringRaw0"]/*' />
        protected void WriteElementStringRaw(String localName, byte[] value) {
            WriteElementStringRaw(localName,null,value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementStringRaw1"]/*' />
        protected void WriteElementStringRaw(String localName, String ns, String value) {
            WriteElementStringRaw(localName, ns, value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementStringRaw01"]/*' />
        protected void WriteElementStringRaw(String localName, String ns, byte[] value) {
            WriteElementStringRaw(localName, ns, value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementStringRaw2"]/*' />
        protected void WriteElementStringRaw(String localName, String value, XmlQualifiedName xsiType) {
            WriteElementStringRaw(localName,null,value, xsiType);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementStringRaw02"]/*' />
        protected void WriteElementStringRaw(String localName, byte[] value, XmlQualifiedName xsiType) {
            WriteElementStringRaw(localName, null, value, xsiType);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementStringRaw3"]/*' />
        protected void WriteElementStringRaw(String localName, String ns, String value, XmlQualifiedName xsiType) {
            if (value == null) return;
            w.WriteStartElement(localName, ns);
            if (xsiType != null)
                WriteXsiType(xsiType.Name, xsiType.Namespace);
            w.WriteRaw(value);
            w.WriteEndElement();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementStringRaw03"]/*' />
        protected void WriteElementStringRaw(String localName, String ns, byte[] value, XmlQualifiedName xsiType) {
            if (value == null) return;
            w.WriteStartElement(localName, ns);
            if (xsiType != null)
                WriteXsiType(xsiType.Name, xsiType.Namespace);
            XmlCustomFormatter.WriteArrayBase64(w, value, 0, value.Length);
            w.WriteEndElement();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteRpcResult"]/*' />
        protected void WriteRpcResult(string name, string ns) {
            if (!soap12) return;
            WriteElementQualifiedName(Soap12.RpcResult, Soap12.RpcNamespace, new XmlQualifiedName(name, ns), null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementQualifiedName"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void WriteElementQualifiedName(String localName, XmlQualifiedName value) {
            WriteElementQualifiedName(localName,null,value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementQualifiedName1"]/*' />
        protected void WriteElementQualifiedName(string localName, XmlQualifiedName value, XmlQualifiedName xsiType) {
            WriteElementQualifiedName(localName, null, value, xsiType);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementQualifiedName2"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected void WriteElementQualifiedName(String localName, String ns, XmlQualifiedName value) {
            WriteElementQualifiedName(localName, ns, value, null);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteElementQualifiedName3"]/*' />
        protected void WriteElementQualifiedName(string localName, string ns, XmlQualifiedName value, XmlQualifiedName xsiType) {
            if (value == null) return;
            if (value.Namespace == null || value.Namespace.Length == 0) {
                WriteStartElement(localName, ns, null, true);
                WriteAttribute("xmlns", "");
            }
            else
                w.WriteStartElement(localName, ns);
            if (xsiType != null)
                WriteXsiType(xsiType.Name, xsiType.Namespace);
            w.WriteString(FromXmlQualifiedName(value, false));
            w.WriteEndElement();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.AddWriteCallback"]/*' />
        protected void AddWriteCallback(Type type, string typeName, string typeNs, XmlSerializationWriteCallback callback) {
            TypeEntry entry = new TypeEntry();
            entry.typeName = typeName;
            entry.typeNs = typeNs;
            entry.type = type;
            entry.callback = callback;
            typeEntries[type] = entry;
        }

        void WriteArray(string name, string ns, object o, Type type) {
            Type elementType = TypeScope.GetArrayElementType(type, null);
            string typeName;
            string typeNs;
            
            StringBuilder arrayDims = new StringBuilder();
            if (!soap12) {
                while ((elementType.IsArray || typeof(IEnumerable).IsAssignableFrom(elementType)) && GetPrimitiveTypeName(elementType, false) == null) {
                    elementType = TypeScope.GetArrayElementType(elementType, null);
                    arrayDims.Append("[]");
                }
            }
            
            if (elementType == typeof(object)) {
                typeName = Soap.UrType;
                typeNs = XmlSchema.Namespace;
            }
            else {
                TypeEntry entry = GetTypeEntry(elementType);
                if (entry != null) {
                    typeName = entry.typeName;
                    typeNs = entry.typeNs;
                }
                else if (soap12) {
                    XmlQualifiedName qualName = GetPrimitiveTypeName(elementType, false);
                    if (qualName != null) {
                        typeName = qualName.Name;
                        typeNs = qualName.Namespace;
                    }
                    else {
                        Type elementBaseType = elementType.BaseType;
                        while (elementBaseType != null) {
                            entry = GetTypeEntry(elementBaseType);
                            if (entry != null) break;
                            elementBaseType = elementBaseType.BaseType;
                        }
                        if (entry != null) {
                            typeName = entry.typeName;
                            typeNs = entry.typeNs;
                        }
                        else {
                            typeName = Soap.UrType;
                            typeNs = XmlSchema.Namespace;
                        }
                    }
                }
                else {
                    XmlQualifiedName qualName = GetPrimitiveTypeName(elementType);
                    typeName = qualName.Name;
                    typeNs = qualName.Namespace;
                }
            }
            
            if (arrayDims.Length > 0)
                typeName = typeName + arrayDims.ToString();
            
            if (soap12 && name != null && name.Length > 0)
                WriteStartElement(name, ns, null, false);
            else
                WriteStartElement(Soap.Array, Soap.Encoding, null, true);
            
            WriteId(o, false);

            if (type.IsArray) {
                Array a = (Array)o;
                int arrayLength = a.Length;
                if (soap12) {
                    w.WriteAttributeString("itemType", Soap12.Encoding, GetQualifiedName(typeName, typeNs));
                    w.WriteAttributeString("arraySize", Soap12.Encoding, arrayLength.ToString(CultureInfo.InvariantCulture));
                }
                else {
                    w.WriteAttributeString("arrayType", Soap.Encoding, GetQualifiedName(typeName, typeNs) + "[" + arrayLength.ToString(CultureInfo.InvariantCulture) + "]");
                }
                for (int i = 0; i < arrayLength; i++) {
                    WritePotentiallyReferencingElement("Item", "", a.GetValue(i), elementType, false, true);
                }
            }
            else {
#if DEBUG
                    // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                    if (!typeof(IEnumerable).IsAssignableFrom(type)) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "not array like type " + type.FullName));
#endif

                int arrayLength = typeof(ICollection).IsAssignableFrom(type) ? ((ICollection)o).Count : -1;
                if (soap12) {
                    w.WriteAttributeString("itemType", Soap12.Encoding, GetQualifiedName(typeName, typeNs));
                    if (arrayLength >= 0)
                        w.WriteAttributeString("arraySize", Soap12.Encoding, arrayLength.ToString(CultureInfo.InvariantCulture));
                }
                else {
                    string brackets = arrayLength >= 0 ? "[" + arrayLength + "]" : "[]";
                    w.WriteAttributeString("arrayType", Soap.Encoding, GetQualifiedName(typeName, typeNs) + brackets);
                }
                IEnumerator e = ((IEnumerable)o).GetEnumerator();
                if (e != null) {
                    while (e.MoveNext()) {
                        WritePotentiallyReferencingElement("Item", "", e.Current, elementType, false, true);
                    }
                }
            }
            w.WriteEndElement();
        }
        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WritePotentiallyReferencingElement"]/*' />
        protected void WritePotentiallyReferencingElement(string n, string ns, object o) {
            WritePotentiallyReferencingElement(n, ns, o, null, false, false);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WritePotentiallyReferencingElement1"]/*' />
        protected void WritePotentiallyReferencingElement(string n, string ns, object o, Type ambientType) {
            WritePotentiallyReferencingElement(n, ns, o, ambientType, false, false);
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WritePotentiallyReferencingElement2"]/*' />
        protected void WritePotentiallyReferencingElement(string n, string ns, object o, Type ambientType, bool suppressReference) {
            WritePotentiallyReferencingElement(n, ns, o, ambientType, suppressReference, false);
        }
        
        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WritePotentiallyReferencingElement3"]/*' />
        protected void WritePotentiallyReferencingElement(string n, string ns, object o, Type ambientType, bool suppressReference, bool isNullable) {
            if (o == null) {
                if (isNullable) WriteNullTagEncoded(n, ns);
                return;
            }
            Type t = o.GetType();
            if (Convert.GetTypeCode(o) == TypeCode.Object && !(o is Guid) && (t != typeof(XmlQualifiedName)) && !(o is XmlNode[]) && (t != typeof(byte[]))) {
                if ((suppressReference || soap12) && !IsIdDefined(o)) {
                    WriteReferencedElement(n, ns, o, ambientType);
                }
                else {
                    if (n == null) {
                        TypeEntry entry = GetTypeEntry(t);
                        WriteReferencingElement(entry.typeName, entry.typeNs, o, isNullable);
                    }
                    else
                        WriteReferencingElement(n, ns, o, isNullable);
                }
            }
            else {
                // Enums always write xsi:type, so don't write it again here.
                bool needXsiType = t != ambientType && !t.IsEnum;
                TypeEntry entry = GetTypeEntry(t);
                if (entry != null) {
                    if (n == null)
                        WriteStartElement(entry.typeName, entry.typeNs, null, true);
                    else
                        WriteStartElement(n, ns, null, true);
                    
                    if (needXsiType) WriteXsiType(entry.typeName, entry.typeNs);
                    entry.callback(o);
                    w.WriteEndElement();
                }
                else {
                    WriteTypedPrimitive(n, ns, o, needXsiType);
                }
            }
        }

        
        void WriteReferencedElement(object o, Type ambientType) {
            WriteReferencedElement(null, null, o, ambientType);
        }

        void WriteReferencedElement(string name, string ns, object o, Type ambientType) {
            if (name == null) name = String.Empty;
            Type t = o.GetType();
            if (t.IsArray || typeof(IEnumerable).IsAssignableFrom(t)) {
                WriteArray(name, ns, o, t);
            }
            else {
                TypeEntry entry = GetTypeEntry(t);
                if (entry == null) throw CreateUnknownTypeException(t);
                WriteStartElement(name.Length == 0 ? entry.typeName : name, ns == null ? entry.typeNs : ns, null, true);
                WriteId(o, false);
                if (ambientType != t) WriteXsiType(entry.typeName, entry.typeNs);
                entry.callback(o);
                w.WriteEndElement();
            }
        }

        TypeEntry GetTypeEntry(Type t) {
            if (typeEntries == null) {
                typeEntries = new Hashtable();
                InitCallbacks();
            }
            return (TypeEntry)typeEntries[t];
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.InitCallbacks"]/*' />
        protected abstract void InitCallbacks();

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteReferencedElements"]/*' />
        protected void WriteReferencedElements() {
            if (referencesToWrite == null) return;

            for (int i = 0; i < referencesToWrite.Count; i++) {
                WriteReferencedElement(referencesToWrite[i], null);
            }
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.TopLevelElement"]/*' />
        protected void TopLevelElement() {
            objectsInUse = new Hashtable();
        }

        /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriter.WriteNamespaceDeclarations"]/*' />
        ///<internalonly/>
        protected void WriteNamespaceDeclarations(XmlSerializerNamespaces xmlns) {
            if (xmlns != null) {
                foreach (DictionaryEntry entry in xmlns.Namespaces) {
                    string prefix = (string)entry.Key;
                    string ns = (string)entry.Value;
                    if (namespaces != null) {
                        string oldNs = namespaces.Namespaces[prefix] as string;
                        if (oldNs != null && oldNs != ns) {
                            throw new InvalidOperationException(Res.GetString(Res.XmlDuplicateNs, prefix, ns));
                        }
                    }
                    string oldPrefix = (ns == null || ns.Length == 0) ? null : Writer.LookupPrefix(ns);

                    if (oldPrefix == null || oldPrefix != prefix) {
                        WriteAttribute("xmlns", prefix, null, ns);
                    }
                }
            }
            namespaces = null;
        }

        string NextPrefix() {
            if (usedPrefixes == null) {
                return aliasBase + (++tempNamespacePrefix);
            }
            while (usedPrefixes.ContainsKey(++tempNamespacePrefix)) {;}
            return aliasBase + tempNamespacePrefix;
        }

        internal class TypeEntry {
            internal XmlSerializationWriteCallback callback;
            internal string typeNs;
            internal string typeName;
            internal Type type;
        }
    }


    /// <include file='doc\XmlSerializationWriter.uex' path='docs/doc[@for="XmlSerializationWriteCallback"]/*' />
    ///<internalonly/>
    public delegate void XmlSerializationWriteCallback(object o);

    internal class XmlSerializationWriterCodeGen : XmlSerializationCodeGen {

        internal XmlSerializationWriterCodeGen(IndentedWriter writer, TypeScope[] scopes, string access, string className) : base(writer, scopes, access, className){
        }

        internal void GenerateBegin() {
            Writer.Write(Access);
            Writer.Write(" class ");
            Writer.Write(ClassName);
            Writer.Write(" : ");
            Writer.Write(typeof(XmlSerializationWriter).FullName);
            Writer.WriteLine(" {");
            Writer.Indent++;

            foreach (TypeScope scope in Scopes) {
                foreach (TypeMapping mapping in scope.TypeMappings) {
                    if (mapping is StructMapping || mapping is EnumMapping){
                        MethodNames.Add(mapping, NextMethodName(mapping.TypeDesc.Name));
                    }
                }
                RaCodeGen.WriteReflectionInit(scope);
            }

            // pre-generate write methods only for the encoded soap
            foreach (TypeScope scope in Scopes) {
                foreach (TypeMapping mapping in scope.TypeMappings) {
                    if (!mapping.IsSoap)
                        continue;

                    if (mapping is StructMapping)
                        WriteStructMethod((StructMapping)mapping);
                    else if (mapping is EnumMapping)
                        WriteEnumMethod((EnumMapping)mapping);
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
        }
        internal void GenerateEnd() {
            GenerateReferencedMethods();
            GenerateInitCallbacksMethod();
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        internal string GenerateElement(XmlMapping xmlMapping) {
            if (!xmlMapping.IsWriteable)
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

        void GenerateInitCallbacksMethod() {
            Writer.WriteLine();
            Writer.WriteLine("protected override void InitCallbacks() {");
            Writer.Indent++;

            foreach (TypeScope scope in Scopes) {
                foreach (TypeMapping typeMapping in scope.TypeMappings) {
                    if (typeMapping.IsSoap && 
                        (typeMapping is StructMapping || typeMapping is EnumMapping) && 
                        !typeMapping.TypeDesc.IsRoot) {
                        
                        string methodName = (string)MethodNames[typeMapping];
                        Writer.Write("AddWriteCallback(");
                        Writer.Write(RaCodeGen.GetStringForTypeof(typeMapping.TypeDesc.CSharpName, typeMapping.TypeDesc.UseReflection));
                        Writer.Write(", ");
                        WriteQuotedCSharpString(typeMapping.TypeName);
                        Writer.Write(", ");
                        WriteQuotedCSharpString(typeMapping.Namespace);
                        Writer.Write(", new ");
                        Writer.Write(typeof(XmlSerializationWriteCallback).FullName);
                        Writer.Write("(this.");
                        Writer.Write(methodName);
                        Writer.WriteLine("));");
                    }
                }
            }
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteQualifiedNameElement(string name, string ns, object defaultValue, string source, bool nullable, bool IsSoap, TypeMapping mapping) {
            bool hasDefault = defaultValue != null && defaultValue != DBNull.Value;
            if (hasDefault) {
                WriteCheckDefault(source, defaultValue, nullable);
                Writer.WriteLine(" {");
                Writer.Indent++;
            }
            string suffix = IsSoap ? "Encoded" : "Literal";
            Writer.Write(nullable ? ("WriteNullableQualifiedName" + suffix): "WriteElementQualifiedName");
            Writer.Write("(");
            WriteQuotedCSharpString(name);
            if (ns != null) {
                Writer.Write(", ");
                WriteQuotedCSharpString(ns);
            }
            Writer.Write(", ");
            Writer.Write(source);

            if (IsSoap) {
                Writer.Write(", new System.Xml.XmlQualifiedName(");
                WriteQuotedCSharpString(mapping.TypeName);
                Writer.Write(", ");
                WriteQuotedCSharpString(mapping.Namespace);
                Writer.Write(")");
            }

            Writer.WriteLine(");");

            if (hasDefault) {
                Writer.Indent--;
                Writer.WriteLine("}");
            }
        }

        void WriteEnumValue(EnumMapping mapping, string source) {
            string methodName = ReferenceMapping(mapping);

            #if DEBUG
                // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                if (methodName == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorMethod, mapping.TypeDesc.Name) + Environment.StackTrace);
            #endif

            Writer.Write(methodName);
            Writer.Write("(");
            Writer.Write(source);
            Writer.Write(")");
        }

        void WritePrimitiveValue(TypeDesc typeDesc, string source, bool isElement) {
            if (typeDesc == StringTypeDesc || typeDesc.FormatterName == "String") {
                Writer.Write(source);
            }
            else {
                if (!typeDesc.HasCustomFormatter) {
                    Writer.Write(typeof(XmlConvert).FullName);
                    Writer.Write(".ToString((");
                    Writer.Write(typeDesc.CSharpName);
                    Writer.Write(")");
                    Writer.Write(source);
                    Writer.Write(")");
                } 
                else {
                    Writer.Write("From");
                    Writer.Write(typeDesc.FormatterName);
                    Writer.Write("(");
                    Writer.Write(source);
                    Writer.Write(")");
                }
            }
        }

        void WritePrimitive(string method, string name, string ns, object defaultValue, string source, TypeMapping mapping, bool writeXsiType, bool isElement, bool isNullable) {
            TypeDesc typeDesc = mapping.TypeDesc;
            bool hasDefault = defaultValue != null && defaultValue != DBNull.Value && mapping.TypeDesc.HasDefaultSupport;
            if (hasDefault) {
                if (mapping is EnumMapping) {
                    #if DEBUG
                        // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                        if (defaultValue.GetType() != typeof(string)) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, name + " has invalid default type " + defaultValue.GetType().Name));
                    #endif

                    Writer.Write("if (");
                    if (mapping.TypeDesc.UseReflection)
                        Writer.Write(RaCodeGen.GetStringForEnumLongValue(source, mapping.TypeDesc.UseReflection));
                    else
                        Writer.Write(source);
                    Writer.Write(" != ");
                    if (((EnumMapping)mapping).IsFlags) {
                        Writer.Write("(");
                        string[] values = ((string)defaultValue).Split(null);
                        for (int i = 0; i < values.Length; i++) {
                            if (values[i] == null || values[i].Length == 0) 
                                continue;
                            if (i > 0) 
                                Writer.WriteLine(" | ");
                            Writer.Write(RaCodeGen.GetStringForEnumCompare((EnumMapping)mapping, values[i], mapping.TypeDesc.UseReflection));
                        }
                        Writer.Write(")");
                    }
                    else {
                        Writer.Write(RaCodeGen.GetStringForEnumCompare((EnumMapping)mapping, (string)defaultValue, mapping.TypeDesc.UseReflection));
                    }
                    Writer.Write(")");
                }
                else {
                    WriteCheckDefault(source, defaultValue, isNullable);
                }
                Writer.WriteLine(" {");
                Writer.Indent++;
            }
            Writer.Write(method);
            Writer.Write("(");
            WriteQuotedCSharpString(name);
            if (ns != null) {
                Writer.Write(", ");
                WriteQuotedCSharpString(ns);
            }
            Writer.Write(", ");

            if (mapping is EnumMapping) {
                WriteEnumValue((EnumMapping)mapping, source);
            }
            else {
                WritePrimitiveValue(typeDesc, source, isElement);
            }

            if (writeXsiType) {
                Writer.Write(", new System.Xml.XmlQualifiedName(");
                WriteQuotedCSharpString(mapping.TypeName);
                Writer.Write(", ");
                WriteQuotedCSharpString(mapping.Namespace);
                Writer.Write(")");
            }

            Writer.WriteLine(");");

            if (hasDefault) {
                Writer.Indent--;
                Writer.WriteLine("}");
            }
        }

        void WriteTag(string methodName, string name, string ns) {
            Writer.Write(methodName);
            Writer.Write("(");
            WriteQuotedCSharpString(name);
            Writer.Write(", ");
            if (ns == null) {
                Writer.Write("null");
            }
            else {
                WriteQuotedCSharpString(ns);
            }
            Writer.WriteLine(");");
        }

        void WriteTag(string methodName, string name, string ns, bool writePrefixed) {
            Writer.Write(methodName);
            Writer.Write("(");
            WriteQuotedCSharpString(name);
            Writer.Write(", ");
            if (ns == null) {
                Writer.Write("null");
            }
            else {
                WriteQuotedCSharpString(ns);
            }
            Writer.Write(", null, ");
            if (writePrefixed)
                Writer.Write("true");
            else
                Writer.Write("false");
            Writer.WriteLine(");");
        }

        void WriteStartElement(string name, string ns, bool writePrefixed) {
            WriteTag("WriteStartElement", name, ns, writePrefixed);
        }

        void WriteEndElement() {
            Writer.WriteLine("WriteEndElement();");
        }
        void WriteEndElement(string source) {
            Writer.Write("WriteEndElement(");
            Writer.Write(source);
            Writer.WriteLine(");");
        }

        void WriteEncodedNullTag(string name, string ns) {
            WriteTag("WriteNullTagEncoded", name, ns);
        }

        void WriteLiteralNullTag(string name, string ns) {
            WriteTag("WriteNullTagLiteral", name, ns);
        }

        void WriteEmptyTag(string name, string ns) {
            WriteTag("WriteEmptyTag", name, ns);
        }

        string GenerateMembersElement(XmlMembersMapping xmlMembersMapping) {
            ElementAccessor element = xmlMembersMapping.Accessor;
            MembersMapping mapping = (MembersMapping)element.Mapping;
            bool hasWrapperElement = mapping.HasWrapperElement;
            bool writeAccessors = mapping.WriteAccessors;
            bool isRpc = xmlMembersMapping.IsSoap && writeAccessors;
            string methodName = NextMethodName(element.Name);
            Writer.WriteLine();
            Writer.Write("public void ");
            Writer.Write(methodName);
            Writer.WriteLine("(object[] p) {");
            Writer.Indent++;

            Writer.WriteLine("WriteStartDocument();");

            if (!mapping.IsSoap) {
                Writer.WriteLine("TopLevelElement();");
            }

            // in the top-level method add check for the parameters length, 
            // because visual basic does not have a concept of an <out> parameter it uses <ByRef> instead
            // so sometime we think that we have more parameters then supplied
            Writer.WriteLine("int pLength = p.Length;");

            if (hasWrapperElement) {
                WriteStartElement(element.Name, (element.Form == XmlSchemaForm.Qualified ? element.Namespace : ""), mapping.IsSoap);

                int xmlnsMember = FindXmlnsIndex(mapping.Members);
                if (xmlnsMember >= 0) {
                    MemberMapping member = mapping.Members[xmlnsMember];
                    string source = "((" + typeof(XmlSerializerNamespaces).FullName + ")p[" + xmlnsMember.ToString(CultureInfo.InvariantCulture) + "])";

                    Writer.Write("if (pLength > ");
                    Writer.Write(xmlnsMember.ToString(CultureInfo.InvariantCulture));
                    Writer.WriteLine(") {");
                    Writer.Indent++;
                    WriteNamespaces(source);
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }

                for (int i = 0; i < mapping.Members.Length; i++) {
                    MemberMapping member = mapping.Members[i];
                    
                    if (member.Attribute != null && !member.Ignore) {
                        string source = "p[" + i.ToString(CultureInfo.InvariantCulture) + "]";

                        string specifiedSource = null;
                        int specifiedPosition = 0;
                        if (member.CheckSpecified != SpecifiedAccessor.None) {
                            string memberNameSpecified = member.Name + "Specified";
                            for (int j = 0; j < mapping.Members.Length; j++) {
                                if (mapping.Members[j].Name == memberNameSpecified) {
                                    specifiedSource = "((bool) p[" + j.ToString(CultureInfo.InvariantCulture) + "])";
                                    specifiedPosition = j;
                                    break;
                                }
                            }
                        }

                        Writer.Write("if (pLength > ");
                        Writer.Write(i.ToString(CultureInfo.InvariantCulture));
                        Writer.WriteLine(") {");
                        Writer.Indent++;

                        if (specifiedSource != null) {
                            Writer.Write("if (pLength <= ");
                            Writer.Write(specifiedPosition.ToString(CultureInfo.InvariantCulture));
                            Writer.Write(" || ");
                            Writer.Write(specifiedSource);
                            Writer.WriteLine(") {");
                            Writer.Indent++;
                        }

                        WriteMember(source, member.Attribute, member.TypeDesc, "p");

                        if (specifiedSource != null) {
                            Writer.Indent--;
                            Writer.WriteLine("}");
                        }
                        
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                }
            }
               
            for (int i = 0; i < mapping.Members.Length; i++) {
                MemberMapping member = mapping.Members[i];
                if (member.Xmlns != null)
                    continue;
                if (member.Ignore)
                    continue;
                    
                string specifiedSource = null;
                int specifiedPosition = 0;
                if (member.CheckSpecified != SpecifiedAccessor.None) {
                    string memberNameSpecified = member.Name + "Specified";

                    for (int j = 0; j < mapping.Members.Length; j++) {
                        if (mapping.Members[j].Name == memberNameSpecified) {
                            specifiedSource = "((bool) p[" + j.ToString(CultureInfo.InvariantCulture) + "])";
                            specifiedPosition = j;
                            break;
                        }
                    }
                }

                Writer.Write("if (pLength > ");
                Writer.Write(i.ToString(CultureInfo.InvariantCulture));
                Writer.WriteLine(") {");
                Writer.Indent++;
               
                if (specifiedSource != null) {
                    Writer.Write("if (pLength <= ");
                    Writer.Write(specifiedPosition.ToString(CultureInfo.InvariantCulture));
                    Writer.Write(" || ");
                    Writer.Write(specifiedSource);
                    Writer.WriteLine(") {");
                    Writer.Indent++;
                }

                string source = "p[" + i.ToString(CultureInfo.InvariantCulture) + "]";
                string enumSource = null;
                if (member.ChoiceIdentifier != null) {
                    for (int j = 0; j < mapping.Members.Length; j++) {
                        if (mapping.Members[j].Name == member.ChoiceIdentifier.MemberName) {
                            if (member.ChoiceIdentifier.Mapping.TypeDesc.UseReflection)
                                enumSource = "p[" + j.ToString(CultureInfo.InvariantCulture) + "]";
                            else
                                enumSource = "((" + mapping.Members[j].TypeDesc.CSharpName + ")p[" + j.ToString(CultureInfo.InvariantCulture) + "]" + ")";
                            break;
                        }
                    }

                    #if DEBUG
                        // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                        if (enumSource == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorDetails, "Can not find " + member.ChoiceIdentifier.MemberName + " in the members mapping."));
                    #endif

                }
                
                if (isRpc && member.IsReturnValue && member.Elements.Length > 0) {
                    Writer.Write("WriteRpcResult(");
                    WriteQuotedCSharpString(member.Elements[0].Name);
                    Writer.Write(", ");
                    WriteQuotedCSharpString("");
                    Writer.WriteLine(");");
                }

                // override writeAccessors choice when we've written a wrapper element
                WriteMember(source, enumSource, member.ElementsSortedByDerivation, member.Text, member.ChoiceIdentifier, member.TypeDesc, writeAccessors || hasWrapperElement);

                if (specifiedSource != null) {
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                
                Writer.Indent--;
                Writer.WriteLine("}");
            }

            if (hasWrapperElement) {
                WriteEndElement();
            }
            if (element.IsSoap) {
                if (!hasWrapperElement && !writeAccessors) {
                    // doc/bare case -- allow extra members
                    Writer.Write("if (pLength > ");
                    Writer.Write(mapping.Members.Length.ToString(CultureInfo.InvariantCulture));
                    Writer.WriteLine(") {");
                    Writer.Indent++;
                    
                    WriteExtraMembers(mapping.Members.Length.ToString(CultureInfo.InvariantCulture), "pLength");
                
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                Writer.WriteLine("WriteReferencedElements();");
            }
            Writer.Indent--;
            Writer.WriteLine("}");
            return methodName;
        }
        
        string GenerateTypeElement(XmlTypeMapping xmlTypeMapping) {
            ElementAccessor element = xmlTypeMapping.Accessor;
            TypeMapping mapping = element.Mapping;
            string methodName = NextMethodName(element.Name);
            Writer.WriteLine();
            Writer.Write("public void ");
            Writer.Write(methodName);
            Writer.WriteLine("(object o) {");
            Writer.Indent++;

            Writer.WriteLine("WriteStartDocument();");

            Writer.WriteLine("if (o == null) {");
            Writer.Indent++;
            if (element.IsNullable){
                if(mapping.IsSoap)
                    WriteEncodedNullTag(element.Name, (element.Form == XmlSchemaForm.Qualified ? element.Namespace : ""));
                else
                    WriteLiteralNullTag(element.Name, (element.Form == XmlSchemaForm.Qualified ? element.Namespace : ""));
            }
            else
                WriteEmptyTag(element.Name, (element.Form == XmlSchemaForm.Qualified ? element.Namespace : ""));
            Writer.WriteLine("return;");
            Writer.Indent--;
            Writer.WriteLine("}");

            if (!mapping.IsSoap && !mapping.TypeDesc.IsValueType && !mapping.TypeDesc.Type.IsPrimitive) {
                Writer.WriteLine("TopLevelElement();");
            }

            WriteMember("o", null, new ElementAccessor[] { element }, null, null, mapping.TypeDesc, !element.IsSoap);

            if (mapping.IsSoap) {
                Writer.WriteLine("WriteReferencedElements();");
            }
            Writer.Indent--;
            Writer.WriteLine("}");
            return methodName;
        }

        string NextMethodName(string name) {
            return "Write" + (++NextMethodNumber).ToString(null, NumberFormatInfo.InvariantInfo) + "_" + CodeIdentifier.MakeValidInternal(name);
        }
        
        void WriteEnumMethod(EnumMapping mapping) {
            string methodName = (string)MethodNames[mapping];
            Writer.WriteLine();
            string fullTypeName = mapping.TypeDesc.CSharpName;
            if (mapping.IsSoap) {
                Writer.Write("void ");
                Writer.Write(methodName);
                Writer.WriteLine("(object e) {");
                WriteLocalDecl(fullTypeName, "v", "e", mapping.TypeDesc.UseReflection);
            }
            else {
                Writer.Write("string ");
                Writer.Write(methodName);
                Writer.Write("(");
                Writer.Write(mapping.TypeDesc.UseReflection? "object" : fullTypeName );
                Writer.WriteLine(" v) {");
            }
            Writer.Indent++;
            Writer.WriteLine("string s = null;");
            ConstantMapping[] constants = mapping.Constants;

            if (constants.Length > 0) {
                Hashtable values = new Hashtable();
                if (mapping.TypeDesc.UseReflection)
                    Writer.WriteLine("switch ("+RaCodeGen.GetStringForEnumLongValue("v", mapping.TypeDesc.UseReflection)+" ){");
                else
                    Writer.WriteLine("switch (v) {");
                Writer.Indent++;
                for (int i = 0; i < constants.Length; i++) {
                    ConstantMapping c = constants[i];
                    if (values[c.Value] == null) {
                        WriteEnumCase(fullTypeName, c, mapping.TypeDesc.UseReflection);
                        Writer.Write("s = ");
                        WriteQuotedCSharpString(c.XmlName);
                        Writer.WriteLine("; break;");
                        values.Add(c.Value, c.Value);
                    }
                }
                

                if (mapping.IsFlags) {
                    Writer.Write("default: s = FromEnum(");
                    Writer.Write(RaCodeGen.GetStringForEnumLongValue("v", mapping.TypeDesc.UseReflection));
                    Writer.Write(", new string[] {");
                    Writer.Indent++;
                    for (int i = 0; i < constants.Length; i++) {
                        ConstantMapping c = constants[i];
                        if (i > 0)
                            Writer.WriteLine(", ");
                        WriteQuotedCSharpString(c.XmlName);
                    }
                    Writer.Write("}, new ");
                    Writer.Write(typeof(long).FullName);
                    Writer.Write("[] {");

                    for (int i = 0; i < constants.Length; i++) {
                        ConstantMapping c = constants[i];
                        if (i > 0)
                            Writer.WriteLine(", ");
                        Writer.Write("(long)");
                        if (mapping.TypeDesc.UseReflection)
                            Writer.Write(c.Value.ToString(CultureInfo.InvariantCulture));
                        else {
                            Writer.Write(fullTypeName);
                            Writer.Write(".@");
                            CodeIdentifier.CheckValidIdentifier(c.Name);
                            Writer.Write(c.Name);
                        }
                    }
                    Writer.Indent--;
                    Writer.Write("}, ");
                    WriteQuotedCSharpString(mapping.TypeDesc.FullName);
                    Writer.WriteLine("); break;");
                }
                else {
                    Writer.Write("default: throw CreateInvalidEnumValueException(");
                    Writer.Write(RaCodeGen.GetStringForEnumLongValue("v", mapping.TypeDesc.UseReflection));
                    Writer.Write(".ToString(System.Globalization.CultureInfo.InvariantCulture), ");
                    WriteQuotedCSharpString(mapping.TypeDesc.FullName);
                    Writer.WriteLine(");");
                }
                Writer.Indent--;
                Writer.WriteLine("}");
            }
            if (mapping.IsSoap) {
                Writer.Write("WriteXsiType(");
                WriteQuotedCSharpString(mapping.TypeName);
                Writer.Write(", ");
                WriteQuotedCSharpString(mapping.Namespace);
                Writer.WriteLine(");");
                Writer.WriteLine("Writer.WriteString(s);");
            }
            else {
                Writer.WriteLine("return s;");
            }
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteDerivedTypes(StructMapping mapping) {
            for (StructMapping derived = mapping.DerivedMappings; derived != null; derived = derived.NextDerivedMapping) {
                string fullTypeName = derived.TypeDesc.CSharpName;
                Writer.Write("else if (");
                WriteTypeCompare("t", fullTypeName, derived.TypeDesc.UseReflection);
                Writer.WriteLine(") {");
                Writer.Indent++;

                string methodName = ReferenceMapping(derived);

                #if DEBUG
                    // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                    if (methodName == null) throw new InvalidOperationException("deriaved from " + mapping.TypeDesc.FullName + ", " + Res.GetString(Res.XmlInternalErrorMethod, derived.TypeDesc.Name) + Environment.StackTrace);
                #endif

                Writer.Write(methodName);
                Writer.Write("(n, ns,");
                if(!derived.TypeDesc.UseReflection) Writer.Write("("+fullTypeName+")");
                Writer.Write("o");
                if (derived.TypeDesc.IsNullable)
                    Writer.Write(", isNullable");
                Writer.Write(", true");
                Writer.WriteLine(");");
                Writer.WriteLine("return;");
                Writer.Indent--;
                Writer.WriteLine("}");

                WriteDerivedTypes(derived);
            }
        }

        void WriteEnumAndArrayTypes() {
            foreach (TypeScope scope in Scopes) {
                foreach (Mapping m in scope.TypeMappings) {
                    if (m is EnumMapping && !m.IsSoap) {
                        EnumMapping mapping = (EnumMapping)m;
                        string fullTypeName = mapping.TypeDesc.CSharpName;
                        Writer.Write("else if (");
                        WriteTypeCompare("t",fullTypeName, mapping.TypeDesc.UseReflection);
                        Writer.WriteLine(") {");
                        Writer.Indent++;

                        string methodName = ReferenceMapping(mapping);

                        #if DEBUG
                            // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                            if (methodName == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorMethod, mapping.TypeDesc.Name) + Environment.StackTrace);
                        #endif
                        Writer.WriteLine("Writer.WriteStartElement(n, ns);");
                        Writer.Write("WriteXsiType(");
                        WriteQuotedCSharpString(mapping.TypeName);
                        Writer.Write(", ");
                        WriteQuotedCSharpString(mapping.Namespace);
                        Writer.WriteLine(");");
                        Writer.Write("Writer.WriteString(");
                        Writer.Write(methodName);
                        Writer.Write("(");
                        if (!mapping.TypeDesc.UseReflection) Writer.Write("("+fullTypeName+")");
                        Writer.WriteLine("o));");
                        Writer.WriteLine("Writer.WriteEndElement();");
                        Writer.WriteLine("return;");
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                    else if (m is ArrayMapping && !m.IsSoap) {
                        ArrayMapping mapping = m as ArrayMapping;
                        if (mapping == null || m.IsSoap) continue;
                        string fullTypeName = mapping.TypeDesc.CSharpName;
                        Writer.Write("else if (");
                        if (mapping.TypeDesc.IsArray)
                            WriteArrayTypeCompare("t", fullTypeName, mapping.TypeDesc.ArrayElementTypeDesc.CSharpName, mapping.TypeDesc.UseReflection);
                        else
                            WriteTypeCompare("t", fullTypeName, mapping.TypeDesc.UseReflection);
                        Writer.WriteLine(") {");
                        Writer.Indent++;

                        Writer.WriteLine("Writer.WriteStartElement(n, ns);");
                        Writer.Write("WriteXsiType(");
                        WriteQuotedCSharpString(mapping.TypeName);
                        Writer.Write(", ");
                        WriteQuotedCSharpString(mapping.Namespace);
                        Writer.WriteLine(");");

                        WriteMember("o", null, mapping.ElementsSortedByDerivation, null, null, mapping.TypeDesc, true);

                        Writer.WriteLine("Writer.WriteEndElement();");
                        Writer.WriteLine("return;");
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                }
            }
        }
        
        void WriteStructMethod(StructMapping mapping) {
            if (mapping.IsSoap && mapping.TypeDesc.IsRoot) return;
            string methodName = (string)MethodNames[mapping];

            Writer.WriteLine();
            Writer.Write("void ");
            Writer.Write(methodName);

            string fullTypeName = mapping.TypeDesc.CSharpName;

            if (mapping.IsSoap) {
                Writer.WriteLine("(object s) {");
                Writer.Indent++;
                WriteLocalDecl(fullTypeName, "o", "s", mapping.TypeDesc.UseReflection);
            }
            else {
                Writer.Write("(string n, string ns, ");
                Writer.Write(mapping.TypeDesc.UseReflection ? "object" : fullTypeName);
                Writer.Write(" o");
                if (mapping.TypeDesc.IsNullable)
                    Writer.Write(", bool isNullable");
                Writer.WriteLine(", bool needType) {");
                Writer.Indent++;
                if (mapping.TypeDesc.IsNullable) {
                    Writer.WriteLine("if ((object)o == null) {");
                    Writer.Indent++;
                    Writer.WriteLine("if (isNullable) WriteNullTagLiteral(n, ns);");
                    Writer.WriteLine("return;");
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                Writer.WriteLine("if (!needType) {");
                Writer.Indent++;

                Writer.Write(typeof(Type).FullName);
                Writer.WriteLine(" t = o.GetType();");
                Writer.Write("if (");
                WriteTypeCompare("t", fullTypeName, mapping.TypeDesc.UseReflection);
                Writer.WriteLine(") {");
                Writer.WriteLine("}");
                WriteDerivedTypes(mapping);
                if (mapping.TypeDesc.IsRoot)
                    WriteEnumAndArrayTypes();
                Writer.WriteLine("else {");

                Writer.Indent++;
                if (mapping.TypeDesc.IsRoot) {
                    Writer.WriteLine("WriteTypedPrimitive(n, ns, o, true);");
                    Writer.WriteLine("return;");
                }
                else {
                    Writer.WriteLine("throw CreateUnknownTypeException(o);");
                }
                Writer.Indent--;
                Writer.WriteLine("}");
                Writer.Indent--;
                Writer.WriteLine("}");
            }

            if (!mapping.TypeDesc.IsAbstract) {
                if (mapping.TypeDesc.Type != null && typeof(XmlSchemaObject).IsAssignableFrom(mapping.TypeDesc.Type)) {
                    Writer.WriteLine("EscapeName = false;");
                }

                string xmlnsSource = null;
                MemberMapping[] members = TypeScope.GetAllMembers(mapping);
                int xmlnsMember = FindXmlnsIndex(members);
                if (xmlnsMember >= 0) {
                    MemberMapping member = members[xmlnsMember];
                    CodeIdentifier.CheckValidIdentifier(member.Name);
                    xmlnsSource = RaCodeGen.GetStringForMember("o", member.Name, mapping.TypeDesc);
                    if (mapping.TypeDesc.UseReflection) {
                        xmlnsSource = "(("+member.TypeDesc.CSharpName+")"+xmlnsSource+")";
                    }
                }

                if (!mapping.IsSoap) {
                    Writer.Write("WriteStartElement(n, ns, o, false, ");
                    if (xmlnsSource == null)
                        Writer.Write("null");
                    else 
                        Writer.Write(xmlnsSource);

                    Writer.WriteLine(");");
                    if (!mapping.TypeDesc.IsRoot) {
                        Writer.Write("if (needType) WriteXsiType(");
                        WriteQuotedCSharpString(mapping.TypeName);
                        Writer.Write(", ");
                        WriteQuotedCSharpString(mapping.Namespace);
                        Writer.WriteLine(");");
                    }
                }
                else if (xmlnsSource != null) {
                    WriteNamespaces(xmlnsSource);
                }
                for (int i = 0; i < members.Length; i++) {
                    MemberMapping m = members[i];
                    if (m.Attribute != null) {
                        CodeIdentifier.CheckValidIdentifier(m.Name);
                        if (m.CheckShouldPersist) {
                            Writer.Write("if (");
                            string methodInvoke = RaCodeGen.GetStringForMethodInvoke("o", fullTypeName, "ShouldSerialize"+m.Name, mapping.TypeDesc.UseReflection);
                            if (mapping.TypeDesc.UseReflection) methodInvoke = "(("+typeof(bool).FullName+")"+methodInvoke+")";
                            Writer.Write(methodInvoke);
                            Writer.WriteLine(") {");
                            Writer.Indent++;
                        }
                        if (m.CheckSpecified != SpecifiedAccessor.None) {
                            Writer.Write("if (");
                            string memberGet = RaCodeGen.GetStringForMember("o", m.Name+"Specified", mapping.TypeDesc);
                            if(mapping.TypeDesc.UseReflection) memberGet = "(("+typeof(bool).FullName+")"+ memberGet+")";
                            Writer.Write(memberGet);
                            Writer.WriteLine(") {");
                            Writer.Indent++;
                        }
                        WriteMember(RaCodeGen.GetStringForMember("o", m.Name, mapping.TypeDesc), m.Attribute, m.TypeDesc, "o");

                        if (m.CheckSpecified != SpecifiedAccessor.None) {
                            Writer.Indent--;
                            Writer.WriteLine("}");
                        }
                        if (m.CheckShouldPersist) {
                            Writer.Indent--;
                            Writer.WriteLine("}");
                        }
                    }
                }
                
                for (int i = 0; i < members.Length; i++) {
                    MemberMapping m = members[i];
                    if (m.Xmlns != null)
                        continue;
                    CodeIdentifier.CheckValidIdentifier(m.Name);
                    bool checkShouldPersist = m.CheckShouldPersist && (m.Elements.Length > 0 || m.Text != null);

                    if (checkShouldPersist) {
                        Writer.Write("if (");
                        string methodInvoke = RaCodeGen.GetStringForMethodInvoke("o", fullTypeName, "ShouldSerialize"+m.Name, mapping.TypeDesc.UseReflection);
                        if (mapping.TypeDesc.UseReflection) methodInvoke = "(("+typeof(bool).FullName+")"+methodInvoke+")";
                        Writer.Write(methodInvoke);
                        Writer.WriteLine(") {");
                        Writer.Indent++;
                    }
                    if (m.CheckSpecified != SpecifiedAccessor.None) {
                        Writer.Write("if (");
                        string memberGet = RaCodeGen.GetStringForMember("o", m.Name+"Specified", mapping.TypeDesc);
                        if(mapping.TypeDesc.UseReflection) memberGet = "(("+typeof(bool).FullName+")"+ memberGet+")";
                        Writer.Write(memberGet);
                        Writer.WriteLine(") {");
                        Writer.Indent++;
                    }

                    string choiceSource = null;
                    if (m.ChoiceIdentifier != null){
                        CodeIdentifier.CheckValidIdentifier(m.ChoiceIdentifier.MemberName);
                        choiceSource = RaCodeGen.GetStringForMember("o", m.ChoiceIdentifier.MemberName, mapping.TypeDesc);
                    }
                    WriteMember(RaCodeGen.GetStringForMember("o", m.Name, mapping.TypeDesc), choiceSource, m.ElementsSortedByDerivation, m.Text, m.ChoiceIdentifier, m.TypeDesc, true);

                    if (m.CheckSpecified != SpecifiedAccessor.None) {
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                    if (checkShouldPersist) {
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                }
                if (!mapping.IsSoap) {
                    WriteEndElement("o");
                }
            }
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        bool CanOptimizeWriteListSequence(TypeDesc listElementTypeDesc) {

            // check to see if we can write values of the attribute sequentially
            // currently we have only one data type (XmlQualifiedName) that we can not write "inline", 
            // because we need to output xmlns:qx="..." for each of the qnames

            return (listElementTypeDesc != null && listElementTypeDesc != QnameTypeDesc);
        }

        void WriteMember(string source, AttributeAccessor attribute, TypeDesc memberTypeDesc, string parent) {
            if (memberTypeDesc.IsAbstract) return;
            if (memberTypeDesc.IsArrayLike) {
                Writer.WriteLine("{");
                Writer.Indent++;
                string fullTypeName = memberTypeDesc.CSharpName;
                WriteArrayLocalDecl(fullTypeName, "a", source, memberTypeDesc);
                if (memberTypeDesc.IsNullable) {
                    Writer.WriteLine("if (a != null) {");
                    Writer.Indent++;
                }
                if (attribute.IsList) {
                    if (CanOptimizeWriteListSequence(memberTypeDesc.ArrayElementTypeDesc)) {
                        Writer.Write("Writer.WriteStartAttribute(null, ");
                        WriteQuotedCSharpString(attribute.Name);
                        Writer.Write(", ");
                        string ns = attribute.Form == XmlSchemaForm.Qualified ? attribute.Namespace : String.Empty;
                        if (ns != null) {
                            WriteQuotedCSharpString(ns);
                        }
                        else {
                            Writer.Write("null");
                        }
                        Writer.WriteLine(");");
                    }
                    else {
                        Writer.Write(typeof(StringBuilder).FullName);
                        Writer.Write(" sb = new ");
                        Writer.Write(typeof(StringBuilder).FullName);
                        Writer.WriteLine("();");
                    }
                }
                TypeDesc arrayElementTypeDesc = memberTypeDesc.ArrayElementTypeDesc;

                if (memberTypeDesc.IsEnumerable) {
                    Writer.Write(" e = ");
                    Writer.Write(typeof(IEnumerator).FullName);
                    if (memberTypeDesc.IsPrivateImplementation) {
                        Writer.Write("((");
                        Writer.Write(typeof(IEnumerable).FullName);
                        Writer.WriteLine(").GetEnumerator();");
                    }
                    else if(memberTypeDesc.IsGenericInterface) {
                        if (memberTypeDesc.UseReflection) {
                            // we use wildcard method name for generic GetEnumerator method, so we cannot use GetStringForMethodInvoke call here
                            Writer.Write("(");
                            Writer.Write(typeof(IEnumerator).FullName);
                            Writer.Write(")");
                            Writer.Write(RaCodeGen.GetReflectionVariable(memberTypeDesc.CSharpName, "System.Collections.Generic.IEnumerable*"));
                            Writer.WriteLine(".Invoke(a, new object[0]);");
                        }
                        else {
                            Writer.Write("((System.Collections.Generic.IEnumerable<");
                            Writer.Write(arrayElementTypeDesc.CSharpName);
                            Writer.WriteLine(">)a).GetEnumerator();");
                        }
                    }
                    else {
                        if (memberTypeDesc.UseReflection) {
                            Writer.Write("(");
                            Writer.Write(typeof(IEnumerator).FullName);
                            Writer.Write(")");
                        }
                        Writer.Write(RaCodeGen.GetStringForMethodInvoke("a", memberTypeDesc.CSharpName, "GetEnumerator", memberTypeDesc.UseReflection));
                        Writer.WriteLine(";");
                    }
                    Writer.WriteLine("if (e != null)");
                    Writer.WriteLine("while (e.MoveNext()) {");
                    Writer.Indent++;

                    string arrayTypeFullName = arrayElementTypeDesc.CSharpName;
                    WriteLocalDecl(arrayTypeFullName, "ai", "e.Current", arrayElementTypeDesc.UseReflection);
                }
                else {
                    Writer.Write("for (int i = 0; i < ");
                    if (memberTypeDesc.IsArray) {
                        Writer.WriteLine("a.Length; i++) {");
                    }
                    else {
                        Writer.Write("((");
                        Writer.Write(typeof(ICollection).FullName);
                        Writer.WriteLine(")a).Count; i++) {");
                    }
                    Writer.Indent++;
                    string arrayTypeFullName = arrayElementTypeDesc.CSharpName;
                    WriteLocalDecl(arrayTypeFullName, "ai", RaCodeGen.GetStringForArrayMember("a", "i", memberTypeDesc), arrayElementTypeDesc.UseReflection);
                }
                if (attribute.IsList) {
                    // check to see if we can write values of the attribute sequentially
                    if (CanOptimizeWriteListSequence(memberTypeDesc.ArrayElementTypeDesc)) {
                        Writer.WriteLine("if (i != 0) Writer.WriteString(\" \");");
                        Writer.Write("WriteValue(");
                    }
                    else {
                        Writer.WriteLine("if (i != 0) sb.Append(\" \");");
                        Writer.Write("sb.Append(");
                    }
                    if (attribute.Mapping is EnumMapping)
                        WriteEnumValue((EnumMapping)attribute.Mapping, "ai");
                    else
                        WritePrimitiveValue(arrayElementTypeDesc, "ai", true);
                    Writer.WriteLine(");");
                }
                else {
                    WriteAttribute("ai", attribute, parent);
                }
                Writer.Indent--;
                Writer.WriteLine("}");
                if (attribute.IsList) {
                    // check to see if we can write values of the attribute sequentially
                    if (CanOptimizeWriteListSequence(memberTypeDesc.ArrayElementTypeDesc)) {
                        Writer.WriteLine("Writer.WriteEndAttribute();");
                    }
                    else {
                        Writer.WriteLine("if (sb.Length != 0) {");
                        Writer.Indent++;
                    
                        Writer.Write("WriteAttribute(");
                        WriteQuotedCSharpString(attribute.Name);
                        Writer.Write(", ");
                        string ns = attribute.Form == XmlSchemaForm.Qualified ? attribute.Namespace : String.Empty;
                        if (ns != null) {
                            WriteQuotedCSharpString(ns);
                            Writer.Write(", ");
                        }
                        Writer.WriteLine("sb.ToString());");
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                }

                if (memberTypeDesc.IsNullable) {
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                Writer.Indent--;
                Writer.WriteLine("}");
            }
            else {
                WriteAttribute(source, attribute, parent);
            }
        }

        void WriteAttribute(string source, AttributeAccessor attribute, string parent) {
            if (attribute.Mapping is SpecialMapping) {
                SpecialMapping special = (SpecialMapping)attribute.Mapping;
                if (special.TypeDesc.Kind == TypeKind.Attribute || special.TypeDesc.CanBeAttributeValue) {
                    Writer.Write("WriteXmlAttribute(");
                    Writer.Write(source);
                    Writer.Write(", ");
                    Writer.Write(parent);
                    Writer.WriteLine(");");
                }
                else
                    throw new InvalidOperationException(Res.GetString(Res.XmlInternalError));
            }
            else {
                TypeDesc typeDesc = attribute.Mapping.TypeDesc;
                if (!typeDesc.UseReflection) source = "(("+typeDesc.CSharpName+")"+source+")";
                WritePrimitive("WriteAttribute", attribute.Name, attribute.Form == XmlSchemaForm.Qualified ? attribute.Namespace : "", attribute.Default, source, attribute.Mapping, false, false, false);
            }
        }
        
        void WriteMember(string source, string choiceSource, ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, TypeDesc memberTypeDesc, bool writeAccessors) {
            if (memberTypeDesc.IsArrayLike && 
                !(elements.Length == 1 && elements[0].Mapping is ArrayMapping))
                WriteArray(source, choiceSource, elements, text, choice, memberTypeDesc);
            else
                WriteElements(source, choiceSource, elements, text, choice, "a", writeAccessors, memberTypeDesc.IsNullable);
        }
        

        void WriteArray(string source, string choiceSource, ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, TypeDesc arrayTypeDesc) {
            if (elements.Length == 0 && text == null) return;
            Writer.WriteLine("{");
            Writer.Indent++;
            string arrayTypeName = arrayTypeDesc.CSharpName;
            WriteArrayLocalDecl(arrayTypeName, "a", source, arrayTypeDesc);
            if (arrayTypeDesc.IsNullable) {
                Writer.WriteLine("if (a != null) {");
                Writer.Indent++;
            }

            if (choice != null) {
                bool choiceUseReflection = choice.Mapping.TypeDesc.UseReflection;
                string choiceFullName = choice.Mapping.TypeDesc.CSharpName;
                WriteArrayLocalDecl(choiceFullName+"[]", "c", choiceSource, choice.Mapping.TypeDesc);
                // write check for the choice identifier array
                Writer.WriteLine("if (c == null || c.Length < a.Length) {");
                Writer.Indent++;
                Writer.Write("throw CreateInvalidChoiceIdentifierValueException(");
                WriteQuotedCSharpString(choice.Mapping.TypeDesc.FullName);
                Writer.Write(", ");
                WriteQuotedCSharpString(choice.MemberName);
                Writer.Write(");");
                Writer.Indent--;
                Writer.WriteLine("}");
            }

            WriteArrayItems(elements, text, choice, arrayTypeDesc, "a", "c");
            if (arrayTypeDesc.IsNullable) {
                Writer.Indent--;
                Writer.WriteLine("}");
            }
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteArrayItems(ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, TypeDesc arrayTypeDesc, string arrayName, string choiceName) {
            TypeDesc arrayElementTypeDesc = arrayTypeDesc.ArrayElementTypeDesc;

            if (arrayTypeDesc.IsEnumerable) {
                Writer.Write(typeof(IEnumerator).FullName);
                Writer.Write(" e = ");
                if (arrayTypeDesc.IsPrivateImplementation) {
                    Writer.Write("((");
                    Writer.Write(typeof(IEnumerable).FullName);
                    Writer.Write(")");
                    Writer.Write(arrayName);
                    Writer.WriteLine(").GetEnumerator();");
                }
                else if(arrayTypeDesc.IsGenericInterface) {
                    if (arrayTypeDesc.UseReflection) {
                        // we use wildcard method name for generic GetEnumerator method, so we cannot use GetStringForMethodInvoke call here
                        Writer.Write("(");
                        Writer.Write(typeof(IEnumerator).FullName);
                        Writer.Write(")");
                        Writer.Write(RaCodeGen.GetReflectionVariable(arrayTypeDesc.CSharpName, "System.Collections.Generic.IEnumerable*"));
                        Writer.Write(".Invoke(");
                        Writer.Write(arrayName);
                        Writer.WriteLine(", new object[0]);");
                    }
                    else {
                        Writer.Write("((System.Collections.Generic.IEnumerable<");
                        Writer.Write(arrayElementTypeDesc.CSharpName);
                        Writer.Write(">)");
                        Writer.Write(arrayName);
                        Writer.WriteLine(").GetEnumerator();");
                    }
                }
                else {
                    if (arrayTypeDesc.UseReflection) {
                        Writer.Write("(");
                        Writer.Write(typeof(IEnumerator).FullName);
                        Writer.Write(")");
                    }
                    Writer.Write(RaCodeGen.GetStringForMethodInvoke(arrayName, arrayTypeDesc.CSharpName, "GetEnumerator", arrayTypeDesc.UseReflection));
                    Writer.WriteLine(";");
                }
                Writer.WriteLine("if (e != null)");
                Writer.WriteLine("while (e.MoveNext()) {");
                Writer.Indent++;
                string arrayTypeFullName = arrayElementTypeDesc.CSharpName;
                WriteLocalDecl(arrayTypeFullName, arrayName+"i", "e.Current", arrayElementTypeDesc.UseReflection);
                WriteElements(arrayName + "i", choiceName + "i", elements, text, choice, arrayName + "a", true, true);
            }
            else {
                Writer.Write("for (int i");
                Writer.Write(arrayName);
                Writer.Write(" = 0; i");
                Writer.Write(arrayName);
                Writer.Write(" < ");
                if (arrayTypeDesc.IsArray) {
                    Writer.Write(arrayName);
                    Writer.Write(".Length");
                }
                else {
                    Writer.Write("((");
                    Writer.Write(typeof(ICollection).FullName);
                    Writer.Write(")");
                    Writer.Write(arrayName);
                    Writer.Write(").Count");
                }
                Writer.Write("; i");
                Writer.Write(arrayName);
                Writer.WriteLine("++) {");
                Writer.Indent++;
                int count = elements.Length + (text == null ? 0 : 1);
                if (count > 1) {
                    string arrayTypeFullName = arrayElementTypeDesc.CSharpName;
                    WriteLocalDecl(arrayTypeFullName, arrayName+"i", RaCodeGen.GetStringForArrayMember(arrayName, "i"+arrayName, arrayTypeDesc), arrayElementTypeDesc.UseReflection);
                    if (choice != null) {
                        string choiceFullName = choice.Mapping.TypeDesc.CSharpName;
                        WriteLocalDecl(choiceFullName, choiceName+"i", RaCodeGen.GetStringForArrayMember(choiceName, "i"+arrayName, choice.Mapping.TypeDesc), choice.Mapping.TypeDesc.UseReflection);
                    }
                    WriteElements(arrayName + "i", choiceName + "i", elements, text, choice, arrayName + "a", true, arrayElementTypeDesc.IsNullable);
                }
                else {
                    WriteElements(RaCodeGen.GetStringForArrayMember(arrayName , "i" + arrayName, arrayTypeDesc), elements, text, choice, arrayName + "a", true, arrayElementTypeDesc.IsNullable);
                }
            }
            Writer.Indent--;
            Writer.WriteLine("}");
        }
        
        void WriteElements(string source, ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, string arrayName, bool writeAccessors, bool isNullable) {
            WriteElements(source, null, elements, text, choice, arrayName, writeAccessors, isNullable);
        }

        void WriteElements(string source, string enumSource, ElementAccessor[] elements, TextAccessor text, ChoiceIdentifierAccessor choice, string arrayName, bool writeAccessors, bool isNullable) {
            if (elements.Length == 0 && text == null) return;
            if (elements.Length == 1 && text == null) {
                TypeDesc td = elements[0].IsUnbounded ? elements[0].Mapping.TypeDesc.CreateArrayTypeDesc() : elements[0].Mapping.TypeDesc;
                if (!elements[0].Any && !elements[0].Mapping.TypeDesc.UseReflection && !elements[0].Mapping.TypeDesc.IsOptionalValue)
                    source = "(("+td.CSharpName+")"+ source+")";
                WriteElement(source, elements[0], arrayName, writeAccessors);
            }
            else {
                if (isNullable && choice == null) {
                    Writer.Write("if ((object)(");
                    Writer.Write(source);
                    Writer.Write(") != null)");
                }
                Writer.WriteLine("{");
                Writer.Indent++;
                int anyCount = 0;
                ArrayList namedAnys = new ArrayList();
                ElementAccessor unnamedAny = null; // can only have one
                bool wroteFirstIf = false;
                string enumTypeName = choice == null ? null : choice.Mapping.TypeDesc.FullName;

                for (int i = 0; i < elements.Length; i++) {
                    ElementAccessor element = elements[i];

                    if (element.Any) {
                        anyCount++;
                        if (element.Name != null && element.Name.Length > 0)
                            namedAnys.Add(element);
                        else if (unnamedAny == null)
                            unnamedAny = element;
                    }
                    else if (choice != null) {
                        bool useReflection = element.Mapping.TypeDesc.UseReflection;
                        string fullTypeName = element.Mapping.TypeDesc.CSharpName;
                        bool enumUseReflection = choice.Mapping.TypeDesc.UseReflection;
                        string enumFullName = (enumUseReflection?"":enumTypeName + ".@") + FindChoiceEnumValue(element, (EnumMapping)choice.Mapping, enumUseReflection);

                        if (wroteFirstIf) Writer.Write("else ");
                        else wroteFirstIf = true;
                        Writer.Write("if (");
                        Writer.Write(enumUseReflection?RaCodeGen.GetStringForEnumLongValue(enumSource, enumUseReflection):enumSource);
                        Writer.Write(" == ");
                        Writer.Write(enumFullName);
                        if (isNullable && !element.IsNullable) {
                            Writer.Write(" && ((object)(");
                            Writer.Write(source);
                            Writer.Write(") != null)");
                        }
                        Writer.WriteLine(") {");
                        Writer.Indent++;

                        WriteChoiceTypeCheck(source, fullTypeName, useReflection, choice, enumFullName, element.Mapping.TypeDesc);

                        string castedSource = source;
                        if (!useReflection)
                            castedSource = "(("+fullTypeName+")"+ source+")";
                        WriteElement(element.Any ? source : castedSource, element, arrayName, writeAccessors);
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                    else {
                        bool useReflection = element.Mapping.TypeDesc.UseReflection;
                        TypeDesc td = element.IsUnbounded ? element.Mapping.TypeDesc.CreateArrayTypeDesc() : element.Mapping.TypeDesc;
                        string fullTypeName = td.CSharpName;
                        if (wroteFirstIf) Writer.Write("else ");
                        else wroteFirstIf = true;
                        Writer.Write("if (");
                        WriteInstanceOf(source, fullTypeName, useReflection);
                        Writer.WriteLine(") {");
                        Writer.Indent++;
                        string castedSource = source;
                        if (!useReflection)
                            castedSource = "(("+fullTypeName+")"+ source+")";
                        WriteElement(element.Any ? source : castedSource, element, arrayName, writeAccessors);
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                }
                if (anyCount > 0) {
                    if (elements.Length - anyCount > 0) Writer.Write("else ");
                    
                    string fullTypeName = typeof(XmlElement).FullName;

                    Writer.Write("if (");
                    Writer.Write(source);
                    Writer.Write(" is ");
                    Writer.Write(fullTypeName);
                    Writer.WriteLine(") {");
                    Writer.Indent++;

                    Writer.Write(fullTypeName);
                    Writer.Write(" elem = (");
                    Writer.Write(fullTypeName);
                    Writer.Write(")");
                    Writer.Write(source);
                    Writer.WriteLine(";");
                    
                    int c = 0;

                    foreach (ElementAccessor element in namedAnys) {
                        if (c++ > 0) Writer.Write("else ");

                        string enumFullName = null;

                        bool useReflection = element.Mapping.TypeDesc.UseReflection;
                        if (choice != null) {
                            bool enumUseReflection = choice.Mapping.TypeDesc.UseReflection;
                            enumFullName = (enumUseReflection?"":enumTypeName + ".@") + FindChoiceEnumValue(element, (EnumMapping)choice.Mapping, enumUseReflection);
                            Writer.Write("if (");
                            Writer.Write(enumUseReflection?RaCodeGen.GetStringForEnumLongValue(enumSource, enumUseReflection):enumSource);
                            Writer.Write(" == ");
                            Writer.Write(enumFullName);
                            if (isNullable && !element.IsNullable) {
                                Writer.Write(" && ((object)(");
                                Writer.Write(source);
                                Writer.Write(") != null)");
                            }
                            Writer.WriteLine(") {");
                            Writer.Indent++;
                        }
                        Writer.Write("if (elem.Name == ");
                        WriteQuotedCSharpString(element.Name);
                        Writer.Write(" && elem.NamespaceURI == ");
                        WriteQuotedCSharpString(element.Namespace);
                        Writer.WriteLine(") {");
                        Writer.Indent++;
                        WriteElement("elem", element, arrayName, writeAccessors);

                        if (choice != null) {
                            Writer.Indent--;
                            Writer.WriteLine("}");
                            Writer.WriteLine("else {");
                            Writer.Indent++;

                            Writer.WriteLine("// throw Value '{0}' of the choice identifier '{1}' does not match element '{2}' from namespace '{3}'.");
                            
                            Writer.Write("throw CreateChoiceIdentifierValueException(");
                            WriteQuotedCSharpString(enumFullName);
                            Writer.Write(", ");
                            WriteQuotedCSharpString(choice.MemberName);
                            Writer.WriteLine(", elem.Name, elem.NamespaceURI);");
                            Writer.Indent--;
                            Writer.WriteLine("}");
                        }
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                    if (c > 0) {
                        Writer.WriteLine("else {");
                        Writer.Indent++;
                    }
                    if (unnamedAny != null) {
                        WriteElement("elem", unnamedAny, arrayName, writeAccessors);
                    }
                    else {
                        Writer.WriteLine("throw CreateUnknownAnyElementException(elem.Name, elem.NamespaceURI);");
                    }
                    if (c > 0) {
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                if (text != null) {
                    bool useReflection = text.Mapping.TypeDesc.UseReflection;
                    string fullTypeName = text.Mapping.TypeDesc.CSharpName;
                    if (elements.Length > 0) {
                        Writer.Write("else ");
                        Writer.Write("if (");
                        WriteInstanceOf(source, fullTypeName, useReflection);
                        Writer.WriteLine(") {");
                        Writer.Indent++;
                        string castedSource = source;
                        if (!useReflection)
                            castedSource = "(("+fullTypeName+")"+ source+")";
                        WriteText(castedSource, text);
                        Writer.Indent--;
                        Writer.WriteLine("}");
                    }
                    else {
                        string castedSource = source;
                        if (!useReflection)
                            castedSource = "(("+fullTypeName+")"+ source+")";
                        WriteText(castedSource, text);
                    }
                }
                if (elements.Length > 0) {
                    Writer.Write("else ");

                    if (isNullable) {
                        Writer.Write(" if ((object)(");
                        Writer.Write(source);
                        Writer.Write(") != null)");
                    }

                    Writer.WriteLine("{");
                    Writer.Indent++;

                    Writer.Write("throw CreateUnknownTypeException(");
                    Writer.Write(source);
                    Writer.WriteLine(");");

                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                Writer.Indent--;
                Writer.WriteLine("}");
            }
        }

        void WriteText(string source, TextAccessor text) {
            if (text.Mapping is PrimitiveMapping) {
                PrimitiveMapping mapping = (PrimitiveMapping)text.Mapping;
                Writer.Write("WriteValue(");
                if (text.Mapping is EnumMapping) {
                    WriteEnumValue((EnumMapping)text.Mapping, source);
                }
                else {
                    WritePrimitiveValue(mapping.TypeDesc, source, false);
                }
                Writer.WriteLine(");");
            }
            else if (text.Mapping is SpecialMapping) {
                SpecialMapping mapping = (SpecialMapping)text.Mapping;
                switch (mapping.TypeDesc.Kind) {
                    case TypeKind.Node:
                        Writer.Write(source);
                        Writer.WriteLine(".WriteTo(Writer);");
                        break;
                    default:
                        throw new InvalidOperationException(Res.GetString(Res.XmlInternalError));
                }
            }
        }
        
        void WriteElement(string source, ElementAccessor element, string arrayName, bool writeAccessor) {
            string name = writeAccessor ? element.Name : element.Mapping.TypeName;
            string ns = element.Any && element.Name.Length == 0 ? null : (element.Form == XmlSchemaForm.Qualified ? (writeAccessor ? element.Namespace : element.Mapping.Namespace) : "");
            if (element.Mapping is NullableMapping) {
                Writer.Write("if ("); 
                Writer.Write(source);
                Writer.WriteLine(" != null) {");
                Writer.Indent++;
                string fullTypeName = element.Mapping.TypeDesc.BaseTypeDesc.CSharpName;
                string castedSource = source;
                if (!element.Mapping.TypeDesc.BaseTypeDesc.UseReflection)
                    castedSource = "(("+fullTypeName+")"+ source+")";
                ElementAccessor e = element.Clone();
                e.Mapping = ((NullableMapping)element.Mapping).BaseMapping;
                WriteElement(e.Any ? source : castedSource, e, arrayName, writeAccessor);
                Writer.Indent--;
                Writer.WriteLine("}");
                if (element.IsNullable) {
                    Writer.WriteLine("else {");
                    Writer.Indent++;
                    WriteLiteralNullTag(element.Name, element.Form == XmlSchemaForm.Qualified ? element.Namespace : "");
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
            }
            else if (element.Mapping is ArrayMapping) {
                ArrayMapping mapping = (ArrayMapping)element.Mapping;
                if (mapping.IsSoap) {
                    Writer.Write("WritePotentiallyReferencingElement(");
                    WriteQuotedCSharpString(name);
                    Writer.Write(", ");
                    WriteQuotedCSharpString(ns);
                    Writer.Write(", ");
                    Writer.Write(source);
                    if (!writeAccessor) {
                        Writer.Write(", ");
                        Writer.Write(RaCodeGen.GetStringForTypeof(mapping.TypeDesc.CSharpName, mapping.TypeDesc.UseReflection));
                        Writer.Write(", true, ");
                    }
                    else {
                        Writer.Write(", null, false, ");
                    }
                    WriteValue(element.IsNullable);
                    Writer.WriteLine(");");
                }
                else if (element.IsUnbounded) {
                    TypeDesc td = mapping.TypeDesc.CreateArrayTypeDesc();
                    string fullTypeName = td.CSharpName;
                    string elementArrayName = "el" + arrayName;
                    string arrayIndex = "c" + elementArrayName;
                    Writer.WriteLine("{");
                    Writer.Indent++;
                    WriteArrayLocalDecl(fullTypeName, elementArrayName, source, mapping.TypeDesc);
                    if (element.IsNullable) {
                        WriteNullCheckBegin(elementArrayName, element);
                    }
                    else {
                        if (mapping.TypeDesc.IsNullable) {
                            Writer.Write("if (");
                            Writer.Write(elementArrayName);
                            Writer.Write(" != null)");
                        }
                        Writer.WriteLine("{");
                        Writer.Indent++;
                    }

                    Writer.Write("for (int ");
                    Writer.Write(arrayIndex);
                    Writer.Write(" = 0; ");
                    Writer.Write(arrayIndex);
                    Writer.Write(" < ");

                    if (td.IsArray) {
                        Writer.Write(elementArrayName);
                        Writer.Write(".Length");
                    }
                    else {
                        Writer.Write("((");
                        Writer.Write(typeof(ICollection).FullName);
                        Writer.Write(")");
                        Writer.Write(elementArrayName);
                        Writer.Write(").Count");
                    }
                    Writer.Write("; ");
                    Writer.Write(arrayIndex);
                    Writer.WriteLine("++) {");
                    Writer.Indent++;

                    element.IsUnbounded = false;
                    WriteElement(elementArrayName + "[" + arrayIndex + "]", element, arrayName, writeAccessor);
                    element.IsUnbounded = true;

                    Writer.Indent--;
                    Writer.WriteLine("}");

                    Writer.Indent--;
                    Writer.WriteLine("}");
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
                else {
                    string fullTypeName = mapping.TypeDesc.CSharpName;
                    Writer.WriteLine("{");
                    Writer.Indent++;
                    WriteArrayLocalDecl(fullTypeName, arrayName, source, mapping.TypeDesc);
                    if (element.IsNullable) {
                        WriteNullCheckBegin(arrayName, element);
                    }
                    else {
                        if (mapping.TypeDesc.IsNullable) {
                            Writer.Write("if (");
                            Writer.Write(arrayName);
                            Writer.Write(" != null)");
                        }
                        Writer.WriteLine("{");
                        Writer.Indent++;
                    }
                    WriteStartElement(name, ns, false);
                    WriteArrayItems(mapping.ElementsSortedByDerivation, null, null, mapping.TypeDesc, arrayName, null);
                    WriteEndElement();
                    Writer.Indent--;
                    Writer.WriteLine("}");
                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
            }
            else if (element.Mapping is EnumMapping) {
                if (element.Mapping.IsSoap) {
                    string methodName = (string)MethodNames[element.Mapping];
                    Writer.Write("Writer.WriteStartElement(");
                    WriteQuotedCSharpString(name);
                    Writer.Write(", ");
                    WriteQuotedCSharpString(ns);
                    Writer.WriteLine(");");
                    Writer.Write(methodName);
                    Writer.Write("(");
                    Writer.Write(source);
                    Writer.WriteLine(");");
                    WriteEndElement();
                }
                else {
                    WritePrimitive("WriteElementString", name, ns, element.Default, source, element.Mapping, false, true, element.IsNullable);
                }
            }
            else if (element.Mapping is PrimitiveMapping) {
                PrimitiveMapping mapping = (PrimitiveMapping)element.Mapping;
                if (mapping.TypeDesc == QnameTypeDesc)
                    WriteQualifiedNameElement(name, ns, element.Default, source, element.IsNullable, mapping.IsSoap, mapping);
                else {
                    string suffixNullable = mapping.IsSoap ? "Encoded" : "Literal";
                    string suffixRaw = mapping.TypeDesc.XmlEncodingNotRequired?"Raw":"";
                    WritePrimitive(element.IsNullable ? ("WriteNullableString" + suffixNullable + suffixRaw) : ("WriteElementString" + suffixRaw),
                                   name, ns, element.Default, source, mapping, mapping.IsSoap, true, element.IsNullable);
                }
            }
            else if (element.Mapping is StructMapping) {
                StructMapping mapping = (StructMapping)element.Mapping;

                if (mapping.IsSoap) {
                    Writer.Write("WritePotentiallyReferencingElement(");
                    WriteQuotedCSharpString(name);
                    Writer.Write(", ");
                    WriteQuotedCSharpString(ns);
                    Writer.Write(", ");
                    Writer.Write(source);
                    if (!writeAccessor) {
                        Writer.Write(", ");
                        Writer.Write(RaCodeGen.GetStringForTypeof(mapping.TypeDesc.CSharpName, mapping.TypeDesc.UseReflection));
                        Writer.Write(", true, ");
                    }
                    else {
                        Writer.Write(", null, false, ");
                    }
                    WriteValue(element.IsNullable);
                }
                else {
                    string methodName = ReferenceMapping(mapping);

                    #if DEBUG
                        // use exception in the place of Debug.Assert to avoid throwing asserts from a server process such as aspnet_ewp.exe
                        if (methodName == null) throw new InvalidOperationException(Res.GetString(Res.XmlInternalErrorMethod, mapping.TypeDesc.Name) + Environment.StackTrace);
                    #endif
                    Writer.Write(methodName);
                    Writer.Write("(");
                    WriteQuotedCSharpString(name);
                    Writer.Write(", ");
                    if (ns == null)
                        Writer.Write("null");
                    else {
                        WriteQuotedCSharpString(ns);
                    }
                    Writer.Write(", ");
                    Writer.Write(source);
                    if (mapping.TypeDesc.IsNullable) {
                        Writer.Write(", ");
                        WriteValue(element.IsNullable);
                    }
                    Writer.Write(", false");
                }
                Writer.WriteLine(");");
            }
            else if (element.Mapping is SpecialMapping) {
                SpecialMapping mapping = (SpecialMapping)element.Mapping;
                bool useReflection = mapping.TypeDesc.UseReflection;
                TypeDesc td = mapping.TypeDesc;
                string fullTypeName = td.CSharpName;


                if (element.Mapping is SerializableMapping) {
                    WriteElementCall("WriteSerializable", typeof(IXmlSerializable), source, name, ns, element.IsNullable, !element.Any);
                }
                else {
                    // XmlNode, XmlElement
                    Writer.Write("if ((");
                    Writer.Write(source);
                    Writer.Write(") is ");
                    Writer.Write(typeof(XmlNode).FullName);
                    Writer.Write(" || ");
                    Writer.Write(source);
                    Writer.Write(" == null");
                    Writer.WriteLine(") {");
                    Writer.Indent++;

                    WriteElementCall("WriteElementLiteral", typeof(XmlNode), source, name, ns, element.IsNullable, element.Any);

                    Writer.Indent--;
                    Writer.WriteLine("}");
                    Writer.WriteLine("else {");
                    Writer.Indent++;

                    Writer.Write("throw CreateInvalidAnyTypeException(");
                    Writer.Write(source);
                    Writer.WriteLine(");");

                    Writer.Indent--;
                    Writer.WriteLine("}");
                }
            }
            else {
                throw new InvalidOperationException(Res.GetString(Res.XmlInternalError));
            }
        }

        void WriteElementCall(string func, Type cast, string source, string name, string ns, bool isNullable, bool isAny) {
            Writer.Write(func);
            Writer.Write("((");
            Writer.Write(cast.FullName);
            Writer.Write(")");
            Writer.Write(source);
            Writer.Write(", ");
            WriteQuotedCSharpString(name);
            Writer.Write(", ");
            WriteQuotedCSharpString(ns);
            Writer.Write(", ");
            WriteValue(isNullable);
            Writer.Write(", ");
            WriteValue(isAny);
            Writer.WriteLine(");");
        }

        void WriteCheckDefault(string source, object value, bool isNullable) {
            Writer.Write("if (");

            if (value is string && ((string)value).Length == 0) {
                // special case for string compare
                Writer.Write("(");
                Writer.Write(source);
                if (isNullable)
                    Writer.Write(" == null) || (");
                else 
                    Writer.Write(" != null) && (");
                Writer.Write(source);
                Writer.Write(".Length != 0)");
            }
            else {
                Writer.Write(source);
                Writer.Write(" != ");
                WriteValue(value);
            }
            Writer.Write(")");
        }

        void WriteChoiceTypeCheck(string source, string fullTypeName, bool useReflection, ChoiceIdentifierAccessor choice, string enumName, TypeDesc typeDesc) {

            Writer.Write("if (((object)");
            Writer.Write(source);
            Writer.Write(") != null && !(");
            WriteInstanceOf(source, fullTypeName, useReflection);
            Writer.Write(")) throw CreateMismatchChoiceException(");
            WriteQuotedCSharpString(typeDesc.FullName);
            Writer.Write(", ");
            WriteQuotedCSharpString(choice.MemberName);
            Writer.Write(", ");
            WriteQuotedCSharpString(enumName);
            Writer.WriteLine(");");
        }

        void WriteNullCheckBegin(string source, ElementAccessor element) {
            Writer.Write("if ((object)(");
            Writer.Write(source);
            Writer.WriteLine(") == null) {");
            Writer.Indent++;
            WriteLiteralNullTag(element.Name, element.Form == XmlSchemaForm.Qualified ? element.Namespace : "");
            Writer.Indent--;
            Writer.WriteLine("}");
            Writer.WriteLine("else {");
            Writer.Indent++;
        }

        void WriteValue(object value) {
            if (value == null) {
                Writer.Write("null");
            }
            else {
                Type type = value.GetType();

                switch (Type.GetTypeCode(type)) {
                case TypeCode.String:
                    {
                        string s = (string)value;
                        WriteQuotedCSharpString(s);
                    }
                    break;
                case TypeCode.Char:
                    {
                        Writer.Write('\'');
                        char ch = (char)value;
                        if (ch == '\'') 
                            Writer.Write("\'");
                        else
                            Writer.Write(ch);
                        Writer.Write('\'');
                    }
                    break;
                case TypeCode.Int32:
                    Writer.Write(((Int32)value).ToString(null, NumberFormatInfo.InvariantInfo));
                    break;
                case TypeCode.Double:
                    Writer.Write(((Double)value).ToString("R", NumberFormatInfo.InvariantInfo));
                    break;
                case TypeCode.Boolean:
                    Writer.Write((bool)value ? "true" : "false");
                    break;
                case TypeCode.Int16:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Byte:
                case TypeCode.SByte:
                    Writer.Write("(");
                    Writer.Write(type.FullName);
                    Writer.Write(")");
                    Writer.Write("(");
                    Writer.Write(Convert.ToString(value, NumberFormatInfo.InvariantInfo));
                    Writer.Write(")");
                    break;
                case TypeCode.Single:
                    Writer.Write(((Single)value).ToString("R", NumberFormatInfo.InvariantInfo));
                    Writer.Write("f");
                    break;
                case TypeCode.Decimal:
                    Writer.Write(((Decimal)value).ToString(null, NumberFormatInfo.InvariantInfo));
                    Writer.Write("m");
                    break;
                case TypeCode.DateTime:
                    Writer.Write(" new ");
                    Writer.Write(type.FullName);
                    Writer.Write("(");
                    Writer.Write(((DateTime)value).Ticks.ToString(CultureInfo.InvariantCulture));
                    Writer.Write(")");
                    break;
                default:
                    if (type.IsEnum) {
                        Writer.Write(((int)value).ToString(null, NumberFormatInfo.InvariantInfo));
                    }
                    else if(type == typeof(TimeSpan) && LocalAppContextSwitches.EnableTimeSpanSerialization) {
                            Writer.Write(" new ");
                            Writer.Write(type.FullName);
                            Writer.Write("(");
                            Writer.Write(((TimeSpan)value).Ticks.ToString(CultureInfo.InvariantCulture));
                            Writer.Write(")");
                     }
                    else {
                        throw new InvalidOperationException(Res.GetString(Res.XmlUnsupportedDefaultType, type.FullName));
                    }
                    break;
                }
            }
        }

        void WriteNamespaces(string source) {
            Writer.Write("WriteNamespaceDeclarations(");
            Writer.Write(source);
            Writer.WriteLine(");");
        }

        int FindXmlnsIndex(MemberMapping[] members) {
            for (int i = 0; i < members.Length; i++) {
                if (members[i].Xmlns == null)
                    continue;
                return i;
            }
            return -1;
        }

        void WriteExtraMembers(string loopStartSource, string loopEndSource) {
            Writer.Write("for (int i = ");
            Writer.Write(loopStartSource);
            Writer.Write("; i < ");
            Writer.Write(loopEndSource);
            Writer.WriteLine("; i++) {");
            Writer.Indent++;
            Writer.WriteLine("if (p[i] != null) {");
            Writer.Indent++;
            Writer.WriteLine("WritePotentiallyReferencingElement(null, null, p[i], p[i].GetType(), true, false);");
            Writer.Indent--;
            Writer.WriteLine("}");
            Writer.Indent--;
            Writer.WriteLine("}");
        }

        void WriteLocalDecl(string typeName, string variableName, string initValue, bool useReflection) {
            RaCodeGen.WriteLocalDecl(typeName, variableName, initValue, useReflection);
        }

        void WriteArrayLocalDecl(string typeName, string variableName, string initValue, TypeDesc arrayTypeDesc) {
            RaCodeGen.WriteArrayLocalDecl(typeName,  variableName,  initValue,  arrayTypeDesc);
        }
        void WriteTypeCompare(string variable, string escapedTypeName, bool useReflection){
            RaCodeGen.WriteTypeCompare(variable, escapedTypeName, useReflection);
        }
        void WriteInstanceOf(string source, string escapedTypeName, bool useReflection){
            RaCodeGen.WriteInstanceOf(source, escapedTypeName, useReflection);
        }
        void WriteArrayTypeCompare(string variable, string escapedTypeName, string elementTypeName, bool useReflection){
            RaCodeGen.WriteArrayTypeCompare(variable, escapedTypeName, elementTypeName, useReflection);
        }
        
        void WriteEnumCase(string fullTypeName, ConstantMapping c, bool useReflection){
            RaCodeGen.WriteEnumCase(fullTypeName, c, useReflection);
        }
        
        string FindChoiceEnumValue(ElementAccessor element, EnumMapping choiceMapping, bool useReflection) {
            string enumValue = null;

            for (int i = 0; i < choiceMapping.Constants.Length; i++) {
                string xmlName = choiceMapping.Constants[i].XmlName;

                if (element.Any && element.Name.Length == 0) {
                    if (xmlName == "##any:") {
                        if (useReflection)
                            enumValue = choiceMapping.Constants[i].Value.ToString(CultureInfo.InvariantCulture);
                        else
                            enumValue = choiceMapping.Constants[i].Name;
                        break;
                    }
                    continue;
                }
                int colon = xmlName.LastIndexOf(':');
                string choiceNs = colon < 0 ? choiceMapping.Namespace : xmlName.Substring(0, colon);
                string choiceName = colon < 0 ? xmlName : xmlName.Substring(colon+1);

                if (element.Name == choiceName) {
                    if ((element.Form == XmlSchemaForm.Unqualified && string.IsNullOrEmpty(choiceNs)) || element.Namespace == choiceNs) {
                        if (useReflection)
                            enumValue = choiceMapping.Constants[i].Value.ToString(CultureInfo.InvariantCulture);
                        else
                            enumValue = choiceMapping.Constants[i].Name;
                        break;
                    }
                }
            }
            if (enumValue == null || enumValue.Length == 0) {
                if (element.Any && element.Name.Length == 0) {
                    // Type {0} is missing enumeration value '##any' for XmlAnyElementAttribute.
                    throw new InvalidOperationException(Res.GetString(Res.XmlChoiceMissingAnyValue, choiceMapping.TypeDesc.FullName));
                }
                // Type {0} is missing value for '{1}'.
                throw new InvalidOperationException(Res.GetString(Res.XmlChoiceMissingValue, choiceMapping.TypeDesc.FullName, element.Namespace + ":" + element.Name, element.Name, element.Namespace));
            }
            if(!useReflection)
                CodeIdentifier.CheckValidIdentifier(enumValue);
            return enumValue;
        }
    }
    
    internal static class DynamicAssemblies {
        static ArrayList assembliesInConfig = new ArrayList();
        static volatile Hashtable nameToAssemblyMap = new Hashtable();
        static volatile Hashtable assemblyToNameMap = new Hashtable();
        static Hashtable tableIsTypeDynamic = Hashtable.Synchronized(new Hashtable());
        static volatile FileIOPermission fileIOPermission;
        static FileIOPermission UnrestrictedFileIOPermission {
            get {
                if (fileIOPermission == null) {
                    fileIOPermission = new FileIOPermission(PermissionState.Unrestricted);
                }
                return fileIOPermission;
            }
        }

        // SxS: This method does not take any resource name and does not expose any resources to the caller.
        // It's OK to suppress the SxS warning.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        [ResourceExposure(ResourceScope.None)]
        internal static bool IsTypeDynamic(Type type) {
            object oIsTypeDynamic = tableIsTypeDynamic[type];
            if (oIsTypeDynamic == null) {
                UnrestrictedFileIOPermission.Assert();
                Assembly assembly = type.Assembly;
                bool isTypeDynamic = assembly.IsDynamic || string.IsNullOrEmpty(assembly.Location);
                if (!isTypeDynamic)
                {
                    if (type.IsArray)
                    {
                        isTypeDynamic = IsTypeDynamic(type.GetElementType());
                    }
                    else if (type.IsGenericType)
                    {
                        Type[] parameterTypes = type.GetGenericArguments();
                        if (parameterTypes != null)
                        {
                            for (int i = 0; i < parameterTypes.Length; i++)
                            {
                                Type parameterType = parameterTypes[i];
                                if (!(parameterType == null || parameterType.IsGenericParameter))
                                {
                                    isTypeDynamic = IsTypeDynamic(parameterType);
                                    if (isTypeDynamic)
                                        break;
                                }
                            }
                        }
                    }
                }
                tableIsTypeDynamic[type] = oIsTypeDynamic = isTypeDynamic;
            }
            return (bool)oIsTypeDynamic;
        }


        internal static bool IsTypeDynamic(Type[] arguments) {
            foreach (Type t in arguments){
                if (DynamicAssemblies.IsTypeDynamic(t)) {
                    return true;
                }
            }
            return false;
        }

        internal static void Add(Assembly a) {
            lock (nameToAssemblyMap) {
                if (assemblyToNameMap[a] != null) {
                    //already added
                    return;
                }
                Assembly oldAssembly = nameToAssemblyMap[a.FullName] as Assembly;
                string key = null;
                if (oldAssembly == null) {
                    key = a.FullName;
                }
                else if(oldAssembly != a) {
                    //more than one assembly with same name
                    key = a.FullName+", "+nameToAssemblyMap.Count;
                }
                if (key != null) {
                    nameToAssemblyMap.Add(key, a);
                    assemblyToNameMap.Add(a, key);
                }
            }
        }
        internal static Assembly Get(string fullName){
            return nameToAssemblyMap!=null?(Assembly)nameToAssemblyMap[fullName]:null;
        }
        internal static string GetName(Assembly a){
            return assemblyToNameMap!=null?(string)assemblyToNameMap[a]:null;
        }
    }
    internal class ReflectionAwareCodeGen {
        private const string hexDigits = "0123456789ABCDEF";
        const string arrayMemberKey = "0";
        // reflectionVariables holds mapping between a reflection entity
        // referenced in the generated code (such as TypeInfo,
        // FieldInfo) and the variable which represent the entity (and
        // initialized before).
        // The types of reflection entity and corresponding key is
        // given below.
        // ----------------------------------------------------------------------------------
        // Entity           Key
        // ----------------------------------------------------------------------------------
        // Assembly         assembly.FullName
        // Type             CodeIdentifier.EscapedKeywords(type.FullName)
        // Field            fieldName+":"+CodeIdentifier.EscapedKeywords(containingType.FullName>)
        // Property         propertyName+":"+CodeIdentifier.EscapedKeywords(containingType.FullName)
        // ArrayAccessor    "0:"+CodeIdentifier.EscapedKeywords(typeof(Array).FullName)
        // MyCollectionAccessor     "0:"+CodeIdentifier.EscapedKeywords(typeof(MyCollection).FullName)
        // ----------------------------------------------------------------------------------
        Hashtable reflectionVariables = null;
        int nextReflectionVariableNumber = 0;
        IndentedWriter writer;
        internal ReflectionAwareCodeGen(IndentedWriter writer){
            this.writer = writer;
        }
        
        internal void WriteReflectionInit(TypeScope scope){
            foreach (Type type in scope.Types) {
                TypeDesc typeDesc = scope.GetTypeDesc(type);
                if (typeDesc.UseReflection)
                    WriteTypeInfo(scope, typeDesc, type);
            }
        }
        
        string WriteTypeInfo(TypeScope scope, TypeDesc typeDesc, Type type){
            InitTheFirstTime();
            string typeFullName = typeDesc.CSharpName;
            string typeVariable = (string)reflectionVariables[typeFullName];
            if (typeVariable != null)
                return typeVariable;

            if (type.IsArray)
            {
                typeVariable = GenerateVariableName("array", typeDesc.CSharpName);
                TypeDesc elementTypeDesc = typeDesc.ArrayElementTypeDesc;
                if (elementTypeDesc.UseReflection)
                {
                    string elementTypeVariable = WriteTypeInfo(scope, elementTypeDesc, scope.GetTypeFromTypeDesc(elementTypeDesc));
                    writer.WriteLine("static "+typeof(Type).FullName+" "+typeVariable +" = " + elementTypeVariable + ".MakeArrayType();");
                }
                else
                {
                    string assemblyVariable = WriteAssemblyInfo(type);
                    writer.Write("static "+typeof(Type).FullName+" "+typeVariable +" = "+assemblyVariable+".GetType(");
                    WriteQuotedCSharpString(type.FullName);
                    writer.WriteLine(");");
                }
            }
            else
            {
                typeVariable = GenerateVariableName("type", typeDesc.CSharpName);

                Type parameterType = Nullable.GetUnderlyingType(type);
                if (parameterType != null)
                {
                    string parameterTypeVariable = WriteTypeInfo(scope, scope.GetTypeDesc(parameterType), parameterType);
                    writer.WriteLine("static "+typeof(Type).FullName+" "+typeVariable +" = typeof(System.Nullable<>).MakeGenericType(new " + typeof(Type).FullName + "[] {"+parameterTypeVariable+"});");
                }
                else
                {
                    string assemblyVariable = WriteAssemblyInfo(type);
                    writer.Write("static "+typeof(Type).FullName+" "+typeVariable +" = "+assemblyVariable+".GetType(");
                    WriteQuotedCSharpString(type.FullName);
                    writer.WriteLine(");");
                }
            }
            
            reflectionVariables.Add(typeFullName, typeVariable);

            TypeMapping mapping = scope.GetTypeMappingFromTypeDesc(typeDesc);
            if (mapping != null)
                WriteMappingInfo(mapping, typeVariable, type);
            if (typeDesc.IsCollection || typeDesc.IsEnumerable){// Arrays use the generic item_Array
                TypeDesc elementTypeDesc = typeDesc.ArrayElementTypeDesc;
                if (elementTypeDesc.UseReflection)
                    WriteTypeInfo(scope, elementTypeDesc, scope.GetTypeFromTypeDesc(elementTypeDesc));
                WriteCollectionInfo(typeVariable, typeDesc, type);
            }
            return typeVariable;
        }

        void InitTheFirstTime(){
            if (reflectionVariables == null){
                reflectionVariables = new Hashtable();
                writer.Write(String.Format(CultureInfo.InvariantCulture, helperClassesForUseReflection,
                    "object", "string", typeof(Type).FullName,
                    typeof(FieldInfo).FullName, typeof(PropertyInfo).FullName,
                    typeof(MemberInfo).FullName, typeof(MemberTypes).FullName));

                WriteDefaultIndexerInit(typeof(IList), typeof(Array).FullName, false, false);
            }
        }
        
        void WriteMappingInfo(TypeMapping mapping, string typeVariable, Type type){
            string typeFullName = mapping.TypeDesc.CSharpName;
            if(mapping is StructMapping){
                StructMapping structMapping = mapping as StructMapping;
                for (int i = 0; i < structMapping.Members.Length; i++) {
                    MemberMapping member = structMapping.Members[i]; 
                    string memberVariable = WriteMemberInfo(type, typeFullName, typeVariable, member.Name);
                    if (member.CheckShouldPersist){
                        string memberName = "ShouldSerialize"+member.Name;
                        memberVariable = WriteMethodInfo(typeFullName, typeVariable, memberName, false);
                    }
                    if (member.CheckSpecified != SpecifiedAccessor.None) {
                        string memberName = member.Name+"Specified";
                        memberVariable = WriteMemberInfo(type, typeFullName, typeVariable, memberName);
                    }
                    if (member.ChoiceIdentifier != null){
                        string memberName = member.ChoiceIdentifier.MemberName;
                        memberVariable = WriteMemberInfo(type, typeFullName, typeVariable, memberName);
                    }
                }
            }
            else if (mapping is EnumMapping){
                FieldInfo[] enumFields = type.GetFields();
                for (int i = 0; i < enumFields.Length; i++) {
                    WriteMemberInfo(type, typeFullName, typeVariable, enumFields[i].Name);
                }
            }
        }
        void WriteCollectionInfo(string typeVariable, TypeDesc typeDesc, Type type){
            string typeFullName = CodeIdentifier.GetCSharpName(type);
            string elementTypeFullName = typeDesc.ArrayElementTypeDesc.CSharpName;
            bool elementUseReflection = typeDesc.ArrayElementTypeDesc.UseReflection;
            if (typeDesc.IsCollection) {
                WriteDefaultIndexerInit(type, typeFullName, typeDesc.UseReflection, elementUseReflection);
            }
            else if (typeDesc.IsEnumerable) {
                if (typeDesc.IsGenericInterface) {
                    WriteMethodInfo(typeFullName, typeVariable, "System.Collections.Generic.IEnumerable*", true);
                }
                else if (!typeDesc.IsPrivateImplementation) {
                    WriteMethodInfo(typeFullName, typeVariable, "GetEnumerator", true);
                }
            }
            WriteMethodInfo(typeFullName, typeVariable, "Add", false, GetStringForTypeof(elementTypeFullName, elementUseReflection));
        }

        string WriteAssemblyInfo(Type type){
            string assemblyFullName = type.Assembly.FullName;
            string assemblyVariable = (string)reflectionVariables[assemblyFullName];
            if ( assemblyVariable == null){
                int iComma = assemblyFullName.IndexOf(',');
                string assemblyName = (iComma>-1)?assemblyFullName.Substring(0, iComma):assemblyFullName;
                assemblyVariable = GenerateVariableName("assembly", assemblyName);
                //writer.WriteLine("static "+ typeof(Assembly).FullName+" "+assemblyVariable+" = "+typeof(Assembly).FullName+".Load(");
                writer.Write("static "+ typeof(Assembly).FullName+" "+assemblyVariable+" = "+"ResolveDynamicAssembly(");
                WriteQuotedCSharpString(DynamicAssemblies.GetName(type.Assembly)/*assemblyFullName*/);
                writer.WriteLine(");");
                reflectionVariables.Add(assemblyFullName, assemblyVariable);
            }
            return assemblyVariable;
        }

        string WriteMemberInfo(Type type, string escapedName, string typeVariable, string memberName){
            MemberInfo[] memberInfos = type.GetMember(memberName);
            for (int i = 0; i < memberInfos.Length; i++) {
                MemberTypes memberType = memberInfos[i].MemberType;
                if (memberType == MemberTypes.Property){
                    string propVariable = GenerateVariableName("prop", memberName);
                    writer.Write("static XSPropInfo "+propVariable+" = new XSPropInfo("+typeVariable+", ");
                    WriteQuotedCSharpString(memberName);
                    writer.WriteLine(");");
                    reflectionVariables.Add(memberName+":"+escapedName, propVariable);
                    return propVariable;
                }
                else if (memberType == MemberTypes.Field){
                    string fieldVariable = GenerateVariableName("field", memberName);
                    writer.Write("static XSFieldInfo "+fieldVariable+" = new XSFieldInfo("+typeVariable+", ");
                    WriteQuotedCSharpString(memberName);
                    writer.WriteLine(");");
                    reflectionVariables.Add(memberName+":"+escapedName, fieldVariable);
                    return fieldVariable;
                }
            }
            throw new InvalidOperationException(Res.GetString(Res.XmlSerializerUnsupportedType, memberInfos[0].ToString()));
        }

        string WriteMethodInfo(string escapedName, string typeVariable, string memberName, bool isNonPublic, params string[] paramTypes){
            string methodVariable = GenerateVariableName("method", memberName);
            writer.Write("static "+typeof(MethodInfo).FullName+" "+methodVariable+" = "+typeVariable+".GetMethod(");
            WriteQuotedCSharpString(memberName);
            writer.Write(", ");

            string bindingFlags = typeof(BindingFlags).FullName;
            writer.Write(bindingFlags); 
            writer.Write(".Public | "); 
            writer.Write(bindingFlags); 
            writer.Write(".Instance | "); 
            writer.Write(bindingFlags); 
            writer.Write(".Static"); 

            if (isNonPublic) {
                writer.Write(" | "); 
                writer.Write(bindingFlags); 
                writer.Write(".NonPublic"); 
            }
            writer.Write(", null, "); 
            writer.Write("new "+typeof(Type).FullName+"[] { ");
            for(int i=0;i<paramTypes.Length;i++){
                writer.Write(paramTypes[i]);
                if(i < (paramTypes.Length-1))
                    writer.Write(", ");
            }
            writer.WriteLine("}, null);");
            reflectionVariables.Add(memberName+":"+escapedName, methodVariable);
            return methodVariable;
        }

        string WriteDefaultIndexerInit(Type type, string escapedName, bool collectionUseReflection, bool elementUseReflection){
            string itemVariable = GenerateVariableName("item", escapedName);
            PropertyInfo defaultIndexer = TypeScope.GetDefaultIndexer(type, null);
            writer.Write("static XSArrayInfo ");
            writer.Write(itemVariable);
            writer.Write("= new XSArrayInfo(");
            writer.Write(GetStringForTypeof(CodeIdentifier.GetCSharpName(type), collectionUseReflection));
            writer.Write(".GetProperty(");
            WriteQuotedCSharpString(defaultIndexer.Name);
            writer.Write(",");
            //defaultIndexer.PropertyType is same as TypeDesc.ElementTypeDesc
            writer.Write(GetStringForTypeof(CodeIdentifier.GetCSharpName(defaultIndexer.PropertyType), elementUseReflection));
            writer.Write(",new ");
            writer.Write(typeof(Type[]).FullName);
            writer.WriteLine("{typeof(int)}));");
            reflectionVariables.Add(arrayMemberKey+":" + escapedName, itemVariable);
            return itemVariable;
        }
        
        private string GenerateVariableName(string prefix, string fullName){
            ++nextReflectionVariableNumber;
            return prefix+nextReflectionVariableNumber+"_"+
                CodeIdentifier.MakeValidInternal(fullName.Replace('.','_'));
        }
        internal string GetReflectionVariable(string typeFullName, string memberName){
            string key;
            if(memberName == null)
                key = typeFullName;
            else
                key = memberName+":"+typeFullName;
            return (string)reflectionVariables[key];
        }
        

        internal string GetStringForMethodInvoke(string obj, string escapedTypeName, string methodName, bool useReflection, params string[] args){
            StringBuilder sb = new StringBuilder();
            if (useReflection){
                sb.Append(GetReflectionVariable(escapedTypeName, methodName));
                sb.Append(".Invoke(");
                sb.Append(obj);
                sb.Append(", new object[] {");
            }
            else{
                sb.Append(obj);
                sb.Append(".@");
                sb.Append(methodName);
                sb.Append("(");
            }
            for(int i=0;i<args.Length;i++){
                if(i != 0)
                    sb.Append(", ");
                sb.Append(args[i]);
            }
            if (useReflection)
                sb.Append( "})");
            else
                sb.Append( ")");
            return sb.ToString();
        }

        internal string GetStringForEnumCompare(EnumMapping mapping, string memberName, bool useReflection){
            if(!useReflection){
                CodeIdentifier.CheckValidIdentifier(memberName);
                return mapping.TypeDesc.CSharpName+".@"+memberName;
            }
            string memberAccess = GetStringForEnumMember(mapping.TypeDesc.CSharpName, memberName, useReflection);
            return GetStringForEnumLongValue(memberAccess, useReflection);
        }
        internal string GetStringForEnumLongValue(string variable, bool useReflection){
            if (useReflection)
                return typeof(Convert).FullName+".ToInt64("+variable+")";
            return "(("+typeof(long).FullName+")"+variable+")";
        }
        
        internal string GetStringForTypeof(string typeFullName, bool useReflection){
            if (useReflection){
                return GetReflectionVariable(typeFullName, null);
            }
            else{
                return "typeof("+typeFullName+")";
            }
        }
        internal string GetStringForMember(string obj, string memberName, TypeDesc typeDesc){
            if (!typeDesc.UseReflection)
                return obj+".@"+memberName;

            TypeDesc saveTypeDesc = typeDesc;
            while(typeDesc!=null){
                string typeFullName = typeDesc.CSharpName;
                string memberInfoName = GetReflectionVariable(typeFullName, memberName);
                if(memberInfoName != null)
                    return memberInfoName+"["+obj+"]";
                // member may be part of the basetype 
                typeDesc = typeDesc.BaseTypeDesc;
                if (typeDesc != null && !typeDesc.UseReflection)
                    return "(("+typeDesc.CSharpName+")"+obj+").@"+memberName;
            }
            //throw GetReflectionVariableException(saveTypeDesc.CSharpName,memberName); 
            // NOTE, Microsoft:Must never happen. If it does let the code
            // gen continue to help debugging what's gone wrong.
            // Eventually the compilation will fail.
            return "["+obj+"]";
        }
        /*
        Exception GetReflectionVariableException(string typeFullName, string memberName){
            string key;
            if(memberName == null)
                key = typeFullName;
            else
                key = memberName+":"+typeFullName;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach(object varAvail in reflectionVariables.Keys){
                sb.Append(varAvail.ToString());
                sb.Append("\n");
            }
            return new Exception("No reflection variable for " + key + "\nAvailable keys\n"+sb.ToString());
        }*/
        
        internal string GetStringForEnumMember(string typeFullName, string memberName, bool useReflection){
            if(!useReflection)
                return typeFullName+".@"+memberName;
            
            string memberInfoName = GetReflectionVariable(typeFullName, memberName);
            return memberInfoName+"[null]";
        }
        internal string GetStringForArrayMember(string arrayName, string subscript, TypeDesc arrayTypeDesc){
            if (!arrayTypeDesc.UseReflection){
                return arrayName+"["+subscript+"]";
            }
            string typeFullName = arrayTypeDesc.IsCollection ? arrayTypeDesc.CSharpName : typeof(Array).FullName;
            string arrayInfo = GetReflectionVariable(typeFullName, arrayMemberKey);
            return arrayInfo + "["+arrayName + ", "+subscript+"]";
            
        }
        internal string GetStringForMethod(string obj, string typeFullName, string memberName, bool useReflection){
            if(!useReflection)
                return obj+"."+memberName+"(";
            
            string memberInfoName = GetReflectionVariable(typeFullName, memberName);
            return memberInfoName+".Invoke("+obj+", new object[]{";
        }
        internal string GetStringForCreateInstance(string escapedTypeName, bool useReflection, bool ctorInaccessible, bool cast) {
            return GetStringForCreateInstance(escapedTypeName, useReflection, ctorInaccessible, cast, string.Empty);
        }

        internal string GetStringForCreateInstance(string escapedTypeName, bool useReflection, bool ctorInaccessible, bool cast, string arg) {
            if (!useReflection && !ctorInaccessible)
                return "new " + escapedTypeName + "(" + arg + ")";
            return GetStringForCreateInstance(GetStringForTypeof(escapedTypeName, useReflection), cast && !useReflection ? escapedTypeName : null, ctorInaccessible, arg);
        }

        internal string GetStringForCreateInstance(string type, string cast, bool nonPublic, string arg) {
            StringBuilder createInstance = new StringBuilder();
            if (cast != null && cast.Length > 0) {
                createInstance.Append("(");
                createInstance.Append(cast);
                createInstance.Append(")");
            }
            createInstance.Append(typeof(Activator).FullName);
            createInstance.Append(".CreateInstance(");
            createInstance.Append(type);
            createInstance.Append(", ");
            string bindingFlags = typeof(BindingFlags).FullName;
            createInstance.Append(bindingFlags); 
            createInstance.Append(".Instance | "); 
            createInstance.Append(bindingFlags); 
            createInstance.Append(".Public | "); 
            createInstance.Append(bindingFlags); 
            createInstance.Append(".CreateInstance"); 

            if (nonPublic) {
                createInstance.Append(" | "); 
                createInstance.Append(bindingFlags); 
                createInstance.Append(".NonPublic"); 
            }
            if (arg == null || arg.Length == 0) {
                createInstance.Append(", null, new object[0], null)");
            }
            else {
                createInstance.Append(", null, new object[] { ");
                createInstance.Append(arg);
                createInstance.Append(" }, null)"); 
            }
            return createInstance.ToString();
        }
        
        internal void WriteLocalDecl(string typeFullName, string variableName, string initValue, bool useReflection) {
            if (useReflection)
                typeFullName = "object";
            writer.Write(typeFullName);
            writer.Write(" ");
            writer.Write(variableName);
            if (initValue != null){
                writer.Write(" = ");
                if(!useReflection &&  initValue != "null"){
                    writer.Write("("+typeFullName+")");
                }
                writer.Write(initValue);
            }
            writer.WriteLine(";");
        }

        internal void WriteCreateInstance(string escapedName, string source, bool useReflection, bool ctorInaccessible) {
            writer.Write(useReflection ? "object" : escapedName);
            writer.Write(" ");
            writer.Write(source);
            writer.Write(" = ");
            writer.Write(GetStringForCreateInstance(escapedName, useReflection, ctorInaccessible, !useReflection && ctorInaccessible));
            writer.WriteLine(";");
        }
        internal void WriteInstanceOf(string source, string escapedTypeName, bool useReflection){
            if(!useReflection){
                writer.Write(source);
                writer.Write(" is ");
                writer.Write(escapedTypeName);
                return;
            }
            writer.Write(GetReflectionVariable(escapedTypeName,null));
            writer.Write(".IsAssignableFrom(");
            writer.Write(source);
            writer.Write(".GetType())");
            
        }

        internal void WriteArrayLocalDecl( string typeName, string variableName, string initValue, TypeDesc arrayTypeDesc) {
            if (arrayTypeDesc.UseReflection){
                if(arrayTypeDesc.IsEnumerable)
                    typeName = typeof(IEnumerable).FullName;
                else if (arrayTypeDesc.IsCollection)
                    typeName = typeof(ICollection).FullName;
                else
                    typeName = typeof(Array).FullName;
            }
            writer.Write(typeName);
            writer.Write(" ");
            writer.Write(variableName);
            if (initValue != null){
                writer.Write(" = ");
                if (initValue != "null")
                    writer.Write("("+typeName+")");
                writer.Write(initValue);
            }
            writer.WriteLine(";");
        }
        internal void WriteEnumCase(string fullTypeName, ConstantMapping c, bool useReflection) {
            writer.Write("case ");
            if (useReflection){
                writer.Write(c.Value.ToString(CultureInfo.InvariantCulture));
            }
            else{
                writer.Write(fullTypeName);
                writer.Write(".@");
                CodeIdentifier.CheckValidIdentifier(c.Name);
                writer.Write(c.Name);
            }
            writer.Write(": ");
        }
        internal void WriteTypeCompare(string variable, string escapedTypeName, bool useReflection){
            writer.Write(variable);
            writer.Write(" == ");
            writer.Write(GetStringForTypeof(escapedTypeName, useReflection));
        }
        internal void WriteArrayTypeCompare(string variable, string escapedTypeName, string elementTypeName, bool useReflection){
            if(!useReflection){
                writer.Write(variable);
                writer.Write(" == typeof(");
                writer.Write(escapedTypeName);
                writer.Write(")");
                return;
            }
            writer.Write(variable);
            writer.Write(".IsArray ");
            writer.Write(" && ");
            WriteTypeCompare(variable+".GetElementType()", elementTypeName, useReflection);
        }

        internal static void WriteQuotedCSharpString(IndentedWriter writer, string value) {
            if (value == null) {
                writer.Write("null");
                return;
            }
            writer.Write("@\"");
            foreach (char ch in value) {
                if (ch < 32) {
                    
                    if (ch == '\r')
                        writer.Write("\\r");
                    else if (ch == '\n')
                        writer.Write("\\n");
                    else if (ch == '\t')
                        writer.Write("\\t");
                    else {
                        byte b = (byte)ch;
                        writer.Write("\\x");
                        writer.Write(hexDigits[b >> 4]);
                        writer.Write(hexDigits[b & 0xF]);
                    }
                }
                else if (ch == '\"') {
                    writer.Write("\"\"");
                }
                else {
                    writer.Write(ch);
                }
            }
            writer.Write("\"");
        }

        internal void WriteQuotedCSharpString(string value) {
            WriteQuotedCSharpString(writer, value);
        }
        
        static string helperClassesForUseReflection = @"
    sealed class XSFieldInfo {{
       {3} fieldInfo;
        public XSFieldInfo({2} t, {1} memberName){{
            fieldInfo = t.GetField(memberName);
        }}
        public {0} this[{0} o] {{
            get {{
                return fieldInfo.GetValue(o);
            }}
            set {{
                fieldInfo.SetValue(o, value);
            }}
        }}

    }}
    sealed class XSPropInfo {{
        {4} propInfo;
        public XSPropInfo({2} t, {1} memberName){{
            propInfo = t.GetProperty(memberName);
        }}
        public {0} this[{0} o] {{
            get {{
                return propInfo.GetValue(o, null);
            }}
            set {{
                propInfo.SetValue(o, value, null);
            }}
        }}
    }}
    sealed class XSArrayInfo {{
        {4} propInfo;
        public XSArrayInfo({4} propInfo){{
            this.propInfo = propInfo;
        }}
        public {0} this[{0} a, int i] {{
            get {{
                return propInfo.GetValue(a, new {0}[]{{i}});
            }}
            set {{
                propInfo.SetValue(a, value, new {0}[]{{i}});
            }}
        }}
    }}
";    
    }
   
}
