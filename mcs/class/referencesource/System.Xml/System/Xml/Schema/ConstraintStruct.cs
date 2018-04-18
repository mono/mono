//------------------------------------------------------------------------------
// <copyright file="ConstraintStruct.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Schema {
    using System;
    using System.Text;
    using System.Collections;
    using System.Globalization;
    using System.Diagnostics;
    using System.Xml.XPath;
    using MS.Internal.Xml.XPath;

    internal sealed class ConstraintStruct {
        // for each constraint
        internal CompiledIdentityConstraint constraint;     // pointer to constraint
        internal SelectorActiveAxis axisSelector;
        internal ArrayList  axisFields;                     // Add tableDim * LocatedActiveAxis in a loop
        internal Hashtable  qualifiedTable;                 // Checking confliction
        internal Hashtable  keyrefTable;                    // several keyref tables having connections to this one is possible
        private int tableDim;                               // dimension of table = numbers of fields;

        internal int TableDim {
            get { return this.tableDim; }
        }

        internal ConstraintStruct (CompiledIdentityConstraint constraint) {
            this.constraint = constraint;
            this.tableDim = constraint.Fields.Length;
            this.axisFields = new ArrayList();              // empty fields
            this.axisSelector = new SelectorActiveAxis (constraint.Selector, this);
            if (this.constraint.Role != CompiledIdentityConstraint.ConstraintRole.Keyref) {
                this.qualifiedTable = new Hashtable();
            }
        }

    } 

    // ActiveAxis plus the location plus the state of matching in the constraint table : only for field
    internal class LocatedActiveAxis : ActiveAxis {
        private int         column;                     // the column in the table (the field sequence)
        internal bool       isMatched;                  // if it's matched, then fill value in the validator later
        internal KeySequence Ks;                        // associated with a keysequence it will fills in

        internal int Column {
            get { return this.column; }
        }

        internal LocatedActiveAxis (Asttree astfield, KeySequence ks, int column) : base (astfield) {
            this.Ks = ks;
            this.column = column;
            this.isMatched = false;
        }

        internal void Reactivate(KeySequence ks) {
            Reactivate();
            this.Ks = ks;
        }
        
    }

    // exist for optimization purpose
    // ActiveAxis plus
    // 1. overload endelement function from parent to return result
    // 2. combine locatedactiveaxis and keysequence more closely
    // 3. enable locatedactiveaxis reusing (the most important optimization point)
    // 4. enable ks adding to hashtable right after moving out selector node (to enable 3)
    // 5. will modify locatedactiveaxis class accordingly
    // 6. taking care of updating ConstraintStruct.axisFields
    // 7. remove constraintTable from ConstraintStruct
    // 8. still need centralized locatedactiveaxis for movetoattribute purpose
    internal class SelectorActiveAxis : ActiveAxis {
        private ConstraintStruct cs;            // pointer of constraintstruct, to enable 6
        private ArrayList KSs;                  // stack of KSStruct, will not become less 
        private int KSpointer = 0;              // indicate current stack top (next available element);
        
        public bool EmptyStack {
            get { return KSpointer == 0; }
        }
        
        public int lastDepth {
            get { return (KSpointer == 0) ? -1 : ((KSStruct) KSs[KSpointer - 1]).depth; } 
        }
        
        public SelectorActiveAxis(Asttree axisTree, ConstraintStruct cs) : base(axisTree) {
            this.KSs = new ArrayList();
            this.cs = cs;
        }
        
        public override bool EndElement(string localname, string URN) {
            base.EndElement(localname, URN);
            if (KSpointer > 0 && this.CurrentDepth == lastDepth) {
                return true;
                // next step PopPS, and insert into hash
            }
            return false;
        }
        
        // update constraintStruct.axisFields as well, if it's new LocatedActiveAxis
        public int PushKS (int errline, int errcol) {
            // new KeySequence each time
            KeySequence ks = new KeySequence(cs.TableDim, errline, errcol);

            // needs to clear KSStruct before using
            KSStruct kss;
            if (KSpointer < KSs.Count) {
                // reuse, clear up KSs.KSpointer
                kss = (KSStruct) KSs[KSpointer];
                kss.ks = ks;
                // reactivate LocatedActiveAxis
                for (int i = 0; i < cs.TableDim; i ++) {
                    kss.fields[i].Reactivate(ks);               // reassociate key sequence
                }
            }
            else { // "==", new
                kss = new KSStruct(ks, cs.TableDim);
                for (int i = 0; i < cs.TableDim; i ++) {
                    kss.fields[i] = new LocatedActiveAxis (cs.constraint.Fields[i], ks, i);
                    cs.axisFields.Add (kss.fields[i]);          // new, add to axisFields
                }
                KSs.Add(kss);
            }
            
            kss.depth = this.CurrentDepth - 1;
            
            return (KSpointer ++);
        }
    
        public KeySequence PopKS () {
            return ((KSStruct)KSs[-- KSpointer]).ks;
        }
        
    }
    
    internal class KSStruct {
        public int depth;                       // depth of selector when it matches
        public KeySequence ks;                  // ks of selector when it matches and assigned -- needs to new each time
        public LocatedActiveAxis[] fields;      // array of fields activeaxis when it matches and assigned
        
        public KSStruct(KeySequence ks, int dim) {
            this.ks = ks;
            this.fields = new LocatedActiveAxis[dim];
        }
    }
    
    internal class TypedObject {

        private class DecimalStruct {
            bool isDecimal = false;         // rare case it will be used...
            decimal[] dvalue;               // to accelerate equals operation.  array <-> list

            public bool IsDecimal {
                get { return this.isDecimal; }
                set { this.isDecimal = value; }
            }

            public decimal[] Dvalue {
                get { return this.dvalue; }
            }

            public DecimalStruct () {
                this.dvalue = new decimal[1];
            }
            //list
            public DecimalStruct (int dim) {
                this.dvalue = new decimal[dim];
            }
        }

        DecimalStruct dstruct = null; 
        object ovalue;
        string svalue;      // only for output
        XmlSchemaDatatype xsdtype;
        int dim = 1; 
        bool isList = false;

        public int Dim {
            get { return this.dim; }
        }

        public bool IsList {
            get { return this.isList; }
        }

        public bool IsDecimal {
            get { 
                Debug.Assert (this.dstruct != null);
                return this.dstruct.IsDecimal; 
            }
        }
        public decimal[] Dvalue {
            get {
                Debug.Assert (this.dstruct != null);
                return this.dstruct.Dvalue; 
            }
        }
        
        public object Value {
            get {return ovalue; }
            set {ovalue = value; }
        }

        public XmlSchemaDatatype Type {
            get {return xsdtype; }
            set {xsdtype = value; }
        }

        public TypedObject (object obj, string svalue, XmlSchemaDatatype xsdtype) {
            this.ovalue = obj;
            this.svalue = svalue;
            this.xsdtype = xsdtype;
            if (xsdtype.Variety == XmlSchemaDatatypeVariety.List ||
                xsdtype is Datatype_base64Binary ||
                xsdtype is Datatype_hexBinary) {
                this.isList = true;
                this.dim = ((Array)obj).Length;
            }
        }

        public override string ToString() {
            // only for exception
            return this.svalue;
        }

        public void SetDecimal () {

            if (this.dstruct != null) {
                return; 
            }
        
            // Debug.Assert(!this.IsDecimal);
            switch(xsdtype.TypeCode) {
                case XmlTypeCode.Byte:
                case XmlTypeCode.UnsignedByte:
                case XmlTypeCode.Short:
                case XmlTypeCode.UnsignedShort:
                case XmlTypeCode.Int:
                case XmlTypeCode.UnsignedInt:
                case XmlTypeCode.Long:
                case XmlTypeCode.UnsignedLong:
                case XmlTypeCode.Decimal:
                case XmlTypeCode.Integer:
                case XmlTypeCode.PositiveInteger:
                case XmlTypeCode.NonNegativeInteger:
                case XmlTypeCode.NegativeInteger:
                case XmlTypeCode.NonPositiveInteger:

                    if (this.isList) {
                        this.dstruct = new DecimalStruct(this.dim);
                        for (int i = 0; i < this.dim; i ++) {
                            this.dstruct.Dvalue[i] = Convert.ToDecimal (((Array) this.ovalue).GetValue(i),NumberFormatInfo.InvariantInfo);
                        }
                    }
                    else { //not list
                        this.dstruct = new DecimalStruct();
                        //possibility of list of length 1.
                        this.dstruct.Dvalue[0] = Convert.ToDecimal (this.ovalue, NumberFormatInfo.InvariantInfo);
                    }
                    this.dstruct.IsDecimal = true;
                    break;

                default:
                    if (this.isList) {
                        this.dstruct = new DecimalStruct(this.dim);
                    }
                    else {
                        this.dstruct = new DecimalStruct();
                    }
                    break;

            }
        }

        private bool ListDValueEquals (TypedObject other) {
            for (int i = 0; i < this.Dim; i ++) {
                if (this.Dvalue[i] != other.Dvalue[i]) {
                    return false;
                }                
            }
            return true;
        }

        public bool Equals (TypedObject other) {
            // ? one is list with one member, another is not list -- still might be equal
            if (this.Dim != other.Dim) {
                return false;
            }

            if (this.Type != other.Type) {
                //Check if types are comparable
                if (! (this.Type.IsComparable(other.Type)) ) {       
                    return false;
                }
                other.SetDecimal(); // can't use cast and other.Type.IsEqual (value1, value2)
                this.SetDecimal();
                if (this.IsDecimal && other.IsDecimal) { //Both are decimal / derived types 
                    return this.ListDValueEquals(other);
                }
            }

            // not-Decimal derivation or type equal
            if (this.IsList) {
                if (other.IsList) { //Both are lists and values are XmlAtomicValue[] or clrvalue[]. So use Datatype_List.Compare
                    return this.Type.Compare(this.Value, other.Value) == 0;
                }
                else { //this is a list and other is a single value
                    Array arr1 = this.Value as System.Array;
                    XmlAtomicValue[] atomicValues1 = arr1 as XmlAtomicValue[];
                    if (atomicValues1 != null) { // this is a list of union
                        return atomicValues1.Length == 1 && atomicValues1.GetValue(0).Equals(other.Value);
                    }
                    else {
                        return arr1.Length == 1 && arr1.GetValue(0).Equals(other.Value);
                    }
                }
            }
            else if (other.IsList) {
                Array arr2 = other.Value as System.Array;
                XmlAtomicValue[] atomicValues2 = arr2 as XmlAtomicValue[];
                if (atomicValues2 != null) { // other is a list of union
                    return atomicValues2.Length == 1 && atomicValues2.GetValue(0).Equals(this.Value);
                }
                else {
                    return arr2.Length == 1 && arr2.GetValue(0).Equals(this.Value);
                }
            }
            else { //Both are not lists
                return this.Value.Equals(other.Value);
            }
        }
    }

    internal class KeySequence {
        TypedObject[] ks;
        int dim;
        int hashcode = -1;
        int posline, poscol;            // for error reporting

        internal KeySequence (int dim, int line, int col) {
            Debug.Assert(dim > 0);
            this.dim = dim;
            this.ks = new TypedObject[dim];
            this.posline = line;
            this.poscol = col;
        }

        public int PosLine {
            get { return this.posline; }
        }

        public int PosCol {
            get { return this.poscol; }
        }

        public KeySequence(TypedObject[] ks) {
            this.ks = ks;
            this.dim = ks.Length;
            this.posline = this.poscol = 0;
        }

        public object this[int index] {
            get {
                object result = ks[index];
                return result;
            }
            set {
                ks[index] = (TypedObject) value;
            } 
        }

        // return true if no null field
        internal bool IsQualified() {
            for (int i = 0; i < this.ks.Length; ++i) {
                if ((this.ks[i] == null) || (this.ks[i].Value == null)) return false;
            }
            return true;
        }

        // it's not directly suit for hashtable, because it's always calculating address
        public override int GetHashCode() {
            if (hashcode != -1) {
                return hashcode;
            }
            hashcode = 0;  // indicate it's changed. even the calculated hashcode below is 0
            for (int i = 0; i < this.ks.Length; i ++) {
                // extract its primitive value to calculate hashcode
                // decimal is handled differently to enable among different CLR types
                this.ks[i].SetDecimal();
					if (this.ks[i].IsDecimal) {
						for (int j = 0 ; j < this.ks[i].Dim ; j ++) {
							hashcode += this.ks[i].Dvalue[j].GetHashCode();
						}
					}
					// 
					else {
						Array arr = this.ks[i].Value as System.Array;
						if (arr != null) {
							XmlAtomicValue[] atomicValues = arr as XmlAtomicValue[];
							if (atomicValues != null) {
								for (int j = 0 ; j < atomicValues.Length ; j ++) {
									hashcode += ((XmlAtomicValue)atomicValues.GetValue(j)).TypedValue.GetHashCode();
								}
							}
							else {
								for (int j = 0 ; j < ((Array) this.ks[i].Value).Length ; j ++) {
									hashcode += ((Array) this.ks[i].Value).GetValue(j).GetHashCode();
								}
							}
						}
						else { //not a list
							hashcode += this.ks[i].Value.GetHashCode();
						}
					}
            }
            return hashcode;
        }

        // considering about derived type
        public override bool Equals(object other) {
            // each key sequence member can have different type
            KeySequence keySequence = (KeySequence)other;
            for (int i = 0; i < this.ks.Length; i++) {
                if (!this.ks[i].Equals(keySequence.ks[i])) {
                    return false;
                }
            }
            return true;
		}

        public override string ToString() {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.ks[0].ToString());
            for (int i = 1; i < this.ks.Length; i++) {
                sb.Append(" ");
                sb.Append(this.ks[i].ToString());
            }
            return sb.ToString();
		}
    }

}
