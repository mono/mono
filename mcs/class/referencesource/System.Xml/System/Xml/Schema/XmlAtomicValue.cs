//------------------------------------------------------------------------------
// <copyright file="XmlAtomicValue.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner> 
//------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.XPath;
using System.Diagnostics;

namespace System.Xml.Schema {

    /// <summary>
    /// This class contains a (CLR Object, XmlType) pair that represents an instance of an Xml atomic value.
    /// It is optimized to avoid boxing.
    /// </summary>
    public sealed class XmlAtomicValue : XPathItem, ICloneable {
        private XmlSchemaType xmlType;
        private object objVal;
        private TypeCode clrType;
        private Union unionVal;
        private NamespacePrefixForQName nsPrefix;

        [StructLayout(LayoutKind.Explicit, Size=8)]
        private struct Union {
            [FieldOffset(0)] public bool boolVal;
            [FieldOffset(0)] public double dblVal;
            [FieldOffset(0)] public long i64Val;
            [FieldOffset(0)] public int i32Val;
            [FieldOffset(0)] public DateTime dtVal;
        }

        class NamespacePrefixForQName : IXmlNamespaceResolver {
            public string prefix;
            public string ns;

            public NamespacePrefixForQName(string prefix, string ns) {
                this.ns = ns;
                this.prefix = prefix;
            }
            public string LookupNamespace(string prefix) {
                if (prefix == this.prefix) {
                    return ns;
                }
                return null;
            }

            public string LookupPrefix(string namespaceName) {
                if (ns == namespaceName) {
                    return prefix;
                }
                return null;
            }

            public IDictionary<string, string> GetNamespacesInScope(XmlNamespaceScope scope) {
                Dictionary<string, string> dict = new Dictionary<string, string>(1);
                dict[prefix] = ns;
                return dict;
            }
        }

        //-----------------------------------------------
        // XmlAtomicValue constructors and methods
        //-----------------------------------------------

        internal XmlAtomicValue(XmlSchemaType xmlType, bool value) {
            if (xmlType == null) throw new ArgumentNullException ("xmlType");
            this.xmlType = xmlType;
            this.clrType = TypeCode.Boolean;
            this.unionVal.boolVal = value;
        }

        internal XmlAtomicValue(XmlSchemaType xmlType, DateTime value) {
            if (xmlType == null) throw new ArgumentNullException ("xmlType");
            this.xmlType = xmlType;
            this.clrType = TypeCode.DateTime;
            this.unionVal.dtVal = value;
        }

        internal XmlAtomicValue(XmlSchemaType xmlType, double value) {
            if (xmlType == null) throw new ArgumentNullException ("xmlType");
            this.xmlType = xmlType;
            this.clrType = TypeCode.Double;
            this.unionVal.dblVal = value;
        }

        internal XmlAtomicValue(XmlSchemaType xmlType, int value) {
            if (xmlType == null) throw new ArgumentNullException ("xmlType");
            this.xmlType = xmlType;
            this.clrType = TypeCode.Int32;
            this.unionVal.i32Val = value;
        }

        internal XmlAtomicValue(XmlSchemaType xmlType, long value) {
            if (xmlType == null) throw new ArgumentNullException ("xmlType");
            this.xmlType = xmlType;
            this.clrType = TypeCode.Int64;
            this.unionVal.i64Val = value;
        }

        internal XmlAtomicValue(XmlSchemaType xmlType, string value) {
            if (value == null) throw new ArgumentNullException ("value");
            if (xmlType == null) throw new ArgumentNullException ("xmlType");
            this.xmlType = xmlType;
            this.objVal = value;
        }

        internal XmlAtomicValue(XmlSchemaType xmlType, string value, IXmlNamespaceResolver nsResolver) {
            if (value == null) throw new ArgumentNullException ("value");
            if (xmlType == null) throw new ArgumentNullException ("xmlType");
            this.xmlType = xmlType;
            this.objVal = value;
            if (nsResolver != null && (this.xmlType.TypeCode == XmlTypeCode.QName || this.xmlType.TypeCode == XmlTypeCode.Notation) ) {
                string prefix = GetPrefixFromQName(value);
                this.nsPrefix = new NamespacePrefixForQName(prefix, nsResolver.LookupNamespace(prefix));
            }
        }

        internal XmlAtomicValue(XmlSchemaType xmlType, object value) {
            if (value == null) throw new ArgumentNullException ("value");
            if (xmlType == null) throw new ArgumentNullException ("xmlType");
            this.xmlType = xmlType;
            this.objVal = value;
        }

        internal XmlAtomicValue(XmlSchemaType xmlType, object value, IXmlNamespaceResolver nsResolver) {
            if (value == null) throw new ArgumentNullException("value");
            if (xmlType == null) throw new ArgumentNullException("xmlType");
            this.xmlType = xmlType;
            this.objVal = value;
            
            if (nsResolver != null && (this.xmlType.TypeCode == XmlTypeCode.QName || this.xmlType.TypeCode == XmlTypeCode.Notation) ) { //Its a qualifiedName
                XmlQualifiedName qname = this.objVal as XmlQualifiedName;
                Debug.Assert(qname != null); //string representation is handled in a different overload
                string ns = qname.Namespace;
                this.nsPrefix = new NamespacePrefixForQName(nsResolver.LookupPrefix(ns), ns);    
            }
        }

        /// <summary>
        /// Since XmlAtomicValue is immutable, clone simply returns this.
        /// </summary>
        public XmlAtomicValue Clone() {
            return this;
        }


        //-----------------------------------------------
        // ICloneable methods
        //-----------------------------------------------

        /// <summary>
        /// Since XmlAtomicValue is immutable, clone simply returns this.
        /// </summary>
        object ICloneable.Clone() {
            return this;
        }


        //-----------------------------------------------
        // XPathItem methods
        //-----------------------------------------------

        public override bool IsNode {
            get { return false; }
        }

        public override XmlSchemaType XmlType {
            get { return this.xmlType; }
        }

        public override Type ValueType {
            get { return this.xmlType.Datatype.ValueType; }
        }

        public override object TypedValue {
            get {
                XmlValueConverter valueConverter = this.xmlType.ValueConverter;

                if (this.objVal == null) {
                    switch (this.clrType) {
                        case TypeCode.Boolean: return valueConverter.ChangeType(this.unionVal.boolVal, ValueType);
                        case TypeCode.Int32: return valueConverter.ChangeType(this.unionVal.i32Val, ValueType);
                        case TypeCode.Int64: return valueConverter.ChangeType(this.unionVal.i64Val, ValueType);
                        case TypeCode.Double: return valueConverter.ChangeType(this.unionVal.dblVal, ValueType);
                        case TypeCode.DateTime: return valueConverter.ChangeType(this.unionVal.dtVal, ValueType);
                        default: Debug.Assert(false, "Should never get here"); break;
                    }
                }
                return valueConverter.ChangeType(this.objVal, ValueType, this.nsPrefix);
            }
        }

        public override bool ValueAsBoolean {
            get {
                XmlValueConverter valueConverter = this.xmlType.ValueConverter;

                if (this.objVal == null) {
                    switch (this.clrType) {
                        case TypeCode.Boolean: return this.unionVal.boolVal;
                        case TypeCode.Int32: return valueConverter.ToBoolean(this.unionVal.i32Val);
                        case TypeCode.Int64: return valueConverter.ToBoolean(this.unionVal.i64Val);
                        case TypeCode.Double: return valueConverter.ToBoolean(this.unionVal.dblVal);
                        case TypeCode.DateTime: return valueConverter.ToBoolean(this.unionVal.dtVal);
                        default: Debug.Assert(false, "Should never get here"); break;
                    }
                }

                return valueConverter.ToBoolean(this.objVal);
            }
        }

        public override DateTime ValueAsDateTime {
            get {
                XmlValueConverter valueConverter = this.xmlType.ValueConverter;

                if (this.objVal == null) {
                    switch (this.clrType) {
                        case TypeCode.Boolean: return valueConverter.ToDateTime(this.unionVal.boolVal);
                        case TypeCode.Int32: return valueConverter.ToDateTime(this.unionVal.i32Val);
                        case TypeCode.Int64: return valueConverter.ToDateTime(this.unionVal.i64Val);
                        case TypeCode.Double: return valueConverter.ToDateTime(this.unionVal.dblVal);
                        case TypeCode.DateTime: return this.unionVal.dtVal;
                        default: Debug.Assert(false, "Should never get here"); break;
                    }
                }

                return valueConverter.ToDateTime(this.objVal);
            }
        }


        public override double ValueAsDouble {
            get {
                XmlValueConverter valueConverter = this.xmlType.ValueConverter;

                if (this.objVal == null) {
                    switch (this.clrType) {
                        case TypeCode.Boolean: return valueConverter.ToDouble(this.unionVal.boolVal);
                        case TypeCode.Int32: return valueConverter.ToDouble(this.unionVal.i32Val);
                        case TypeCode.Int64: return valueConverter.ToDouble(this.unionVal.i64Val);
                        case TypeCode.Double: return this.unionVal.dblVal;
                        case TypeCode.DateTime: return valueConverter.ToDouble(this.unionVal.dtVal);
                        default: Debug.Assert(false, "Should never get here"); break;
                    }
                }

                return valueConverter.ToDouble(this.objVal);
            }
        }

        public override int ValueAsInt {
            get {
                XmlValueConverter valueConverter = this.xmlType.ValueConverter;

                if (this.objVal == null) {
                    switch (this.clrType) {
                        case TypeCode.Boolean: return valueConverter.ToInt32(this.unionVal.boolVal);
                        case TypeCode.Int32: return this.unionVal.i32Val;
                        case TypeCode.Int64: return valueConverter.ToInt32(this.unionVal.i64Val);
                        case TypeCode.Double: return valueConverter.ToInt32(this.unionVal.dblVal);
                        case TypeCode.DateTime: return valueConverter.ToInt32(this.unionVal.dtVal);
                        default: Debug.Assert(false, "Should never get here"); break;
                    }
                }

                return valueConverter.ToInt32(this.objVal);
            }
        }

        public override long ValueAsLong {
            get {
                XmlValueConverter valueConverter = this.xmlType.ValueConverter;

                if (this.objVal == null) {
                    switch (this.clrType) {
                        case TypeCode.Boolean: return valueConverter.ToInt64(this.unionVal.boolVal);
                        case TypeCode.Int32: return valueConverter.ToInt64(this.unionVal.i32Val);
                        case TypeCode.Int64: return this.unionVal.i64Val;
                        case TypeCode.Double: return valueConverter.ToInt64(this.unionVal.dblVal);
                        case TypeCode.DateTime: return valueConverter.ToInt64(this.unionVal.dtVal);
                        default: Debug.Assert(false, "Should never get here"); break;
                    }
                }

                return valueConverter.ToInt64(this.objVal);
            }
        }

        public override object ValueAs(Type type, IXmlNamespaceResolver nsResolver) {
            XmlValueConverter valueConverter = this.xmlType.ValueConverter;

            if (type == typeof(XPathItem) || type == typeof(XmlAtomicValue))
                return this;

            if (this.objVal == null) {
                switch (this.clrType) {
                    case TypeCode.Boolean: return valueConverter.ChangeType(this.unionVal.boolVal, type);
                    case TypeCode.Int32: return valueConverter.ChangeType(this.unionVal.i32Val, type);
                    case TypeCode.Int64: return valueConverter.ChangeType(this.unionVal.i64Val, type);
                    case TypeCode.Double: return valueConverter.ChangeType(this.unionVal.dblVal, type);
                    case TypeCode.DateTime: return valueConverter.ChangeType(this.unionVal.dtVal, type);
                    default: Debug.Assert(false, "Should never get here"); break;
                }
            }

            return valueConverter.ChangeType(this.objVal, type, nsResolver);
        }

        public override string Value {
            get {
                XmlValueConverter valueConverter = this.xmlType.ValueConverter;

                if (this.objVal == null) {
                    switch (this.clrType) {
                        case TypeCode.Boolean: return valueConverter.ToString(this.unionVal.boolVal);
                        case TypeCode.Int32: return valueConverter.ToString(this.unionVal.i32Val);
                        case TypeCode.Int64: return valueConverter.ToString(this.unionVal.i64Val);
                        case TypeCode.Double: return valueConverter.ToString(this.unionVal.dblVal);
                        case TypeCode.DateTime: return valueConverter.ToString(this.unionVal.dtVal);
                        default: Debug.Assert(false, "Should never get here"); break;
                    }
                }
                return valueConverter.ToString(this.objVal, this.nsPrefix);
            }
        }

        public override string ToString() {
            return Value;
        }

        private string GetPrefixFromQName(string value) {
            int colonOffset;
            int len = ValidateNames.ParseQName(value, 0, out colonOffset);

            if (len == 0 || len != value.Length) {
                return null;
            }
            if (colonOffset != 0) {
                return value.Substring(0, colonOffset);
            }
            else {
                return string.Empty;
            }
        }
    }
}

