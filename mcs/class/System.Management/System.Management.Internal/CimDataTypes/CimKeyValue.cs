/******************************************************************************
* The MIT License
* Copyright (c) 2007 Novell Inc.,  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest (twiest@novell.com)
//		Rusty Howell  (rhowell@novell.com)
//
// (C)  Novell Inc.


using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Internal
{
    /// <summary>
    /// 
    /// </summary>
    internal class CimKeyValue
    {
        //public enum KeyValueType {STRING,BOOLEAN,NUMERIC};//don't need this

        /* <!ELEMENT KEYVALUE (#PCDATA)>
         * <!ATTLIST KEYVALUE
         *      VALUETYPE    (string|boolean|numeric)  "string"
         *      %CIMType;    #IMPLIED>
         * 
         * <KEYBINDING NAME="CreationClassName">
         *      <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>
         * </KEYBINDING>
         * */

        #region Members

        private string _valueType;
        private NullableCimType _type;
        private string _value;

        #endregion

        #region Constructors
        public CimKeyValue()
        {
        }

        public CimKeyValue(string valueType, string value)
        {
            ValueType = valueType;
            Value = value;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the ValueType: Valid types are "string", "boolean", or "numeric"
        /// </summary>
        public string ValueType
        {
            get { return _valueType; }
            set
            {
                switch (value.ToLower())
                {
                    case "string":
                    case "boolean":
                    case "numeric":
                        _valueType = value.ToLower();
                        break;
                    default:
                        throw new Exception("Invalid KeyValue type");
                }

            }
        }

        /// <summary>
        /// Gets or sets the CimType for the data.
        /// </summary>
        public NullableCimType Type
        {
            get
            {
                if (_type == null)
                    _type = new NullableCimType();

                return _type;
            }
            set { _type = value; }
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value
        {
            get
            {
                if (_value == null)
                    return string.Empty;

                return _value;
            }
            set { _value = value; }
        }
        #endregion

        #region Methods and Operators

        #region Equals, operator== , operator!=
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CimKeyValue))
            {
                return false;
            }
            return (this == (CimKeyValue)obj);
        }
        public static bool operator ==(CimKeyValue val1, CimKeyValue val2)
        {
            if (((object)val1 == null) || ((object)val2 == null))
            {
                if (((object)val1 == null) && ((object)val2 == null))
                {
                    return true;
                }
                return false;
            }
            if (val1.ValueType != val2.ValueType)
            {
                return false;
            }
            if (val1.Value.ToLower() != val2.Value.ToLower())
            {
                return false;
            }
            return true;

        }
        public static bool operator !=(CimKeyValue val1, CimKeyValue val2)
        {
            return !(val1 == val2);
        }
        #endregion
        
        #endregion

    }
}
