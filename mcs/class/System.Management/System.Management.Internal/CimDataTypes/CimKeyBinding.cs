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
    /// The KeyBinding defines a single key property value binding.
    /// </summary>
    internal class CimKeyBinding : IComparable    
    {
        /* <!ELEMENT KEYBINDING (KEYVALUE|VALUE.REFERENCE)>
         * <!ATTLIST KEYBINDING
         *      %CIMName;>
         * 
         * <KEYBINDING NAME="CreationClassName">
         *      <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>
         * </KEYBINDING>
         * */
        #region Members
        
        public enum RefType { KeyValue, ValueReference };
        private CimName _name = null;
        private object _value = null;
        RefType _type;
        
        #endregion

        #region Constructors
        public CimKeyBinding(CimName name)
        {
            Name = name;
        }
        public CimKeyBinding(CimName name, CimKeyValue keyValue)
            : this(name)
        {
            Value = keyValue;
        }
        public CimKeyBinding(CimName name, CimValueReference reference)
            : this(name)
        {
            Value = reference;
        }

        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the name of the KeyBinding
        /// </summary>
        public CimName Name
        {
            get 
            {
                if (_name == null)
                    _name = new CimName(null);

                return _name; 
            }
            set { _name = value; }
        }
        /// <summary>
        /// Gets or sets the KeyValue
        /// </summary>
        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (_value is CimKeyValue)
                {
                    _type = RefType.KeyValue;
                }
                else if (_value is CimValueReference)
                {
                    _type = RefType.ValueReference;
                }
                else
                {
                    throw new Exception("Invalid type for KeyBinding");
                }
            }
        }
        /// <summary>
        /// Gets the Type of the value
        /// </summary>
        public RefType Type
        {
            get { return _type; }
        }

        public bool IsSet
        {
            get { return (Value == null); }
        }
        #endregion

        #region Methods and Operators
        public bool ShallowCompare(CimKeyBinding keybinding)
        {
            return (this.Name == keybinding.Name);
        }

        #region Equals, operator== , operator!=
        /// <summary>
        /// Compares two CimKeyBinding objects
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>Returns true if equals</returns>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CimKeyBinding))
            {
                return false;
            }
            return (this == (CimKeyBinding)obj);
        }
        /// <summary>
        /// Compares two CimKeyBinding objects
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns>Returns true if they are equal</returns>
        public static bool operator ==(CimKeyBinding val1, CimKeyBinding val2)
        {

            //Add code here
            if (((object)val1 == null) || ((object)val2 == null))
            {
                if (((object)val1 == null) && ((object)val2 == null))
                {
                    return true;
                }
                return false;
            }

            if (val1.Type != val2.Type)
            {
                return false;
            }
            if (val1.Type == RefType.KeyValue)
            {
                return ((CimKeyValue)val1.Value).Equals(val2.Value);       // this should call the overriden method in the class         
            }
            else
            {
                return ((CimValueReference)val1.Value).Equals(val2.Value);
            }

        }
        public static bool operator !=(CimKeyBinding val1, CimKeyBinding val2)
        {
            return !(val1 == val2);
        }

        public override string ToString()
        {
            string str = string.Empty;
            switch (Type)
            {
                case RefType.KeyValue:
                    str = ((CimKeyValue)Value).Value;
                    break;
                case RefType.ValueReference:
                    str = ((CimValueReference)Value).ToString();
                    break;
                default:
                    throw new Exception("Invalid type for KeyBinding");                 

            }
            return str;
        }
        #endregion

        
        #endregion

        #region IComparable Members
        /// <summary>
        /// Sortable by CimName Name member
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
           
            return this.Name.CompareTo(((CimKeyBinding)obj).Name);
        }

        #endregion
    }
}
