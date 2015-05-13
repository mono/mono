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
    /// The PROPERTY element defines a single (non-array) CIM Property that is not a reference. 
    /// It contains a single value of the Property.
    /// </summary>
    internal class CimProperty : CimClassMember
    {
        /* <!ELEMENT PROPERTY (QUALIFIER*,VALUE?)> 
         * <!ATTLIST PROPERTY 
         *      %CIMName; 
         *      %CIMType; #REQUIRED 
         *      %ClassOrigin; 
         *      %Propagated; 
         *      xml:lang NMTOKEN #IMPLIED>
         * */
        private string _value = null;
        

        #region Constructors
        
        /// <summary>
        /// Creates a new property with the given name and type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public CimProperty(CimName name, CimType type)
        {
            Name = name;
            Type = type;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the value of the property
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

        /// <summary>
        /// Returns true if the property has a key qualifier and the key qualifier is set to 'true'
        /// </summary>
        public bool IsKeyProperty
        {
            get
            {
            	//Changed for MONO
            	for (int i = 0; i < Qualifiers.Count; i++)
                {
            		CimQualifier curQual = Qualifiers[i];
					if ( (curQual.Name.ToString().Equals("key", StringComparison.OrdinalIgnoreCase)) &&
                         (curQual.Type == CimType.BOOLEAN) &&
						(curQual.Values[0].Equals("true", StringComparison.OrdinalIgnoreCase)) )                        
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns true if the property has a required qualifier and the required qualifier is set to 'true'
        /// </summary>
        public bool IsRequiredProperty
        {
            get
            {
                //Changed for MONO
                for (int i = 0; i < Qualifiers.Count; i++)
                {
                    CimQualifier curQual = Qualifiers[i];
					if ((curQual.Name.ToString().Equals("required", StringComparison.OrdinalIgnoreCase)) &&
                         (curQual.Type == CimType.BOOLEAN) &&
						(curQual.Values[0].Equals("true", StringComparison.OrdinalIgnoreCase)))
                        return true;
                }

                return false;
            }
        }
        
        /// <summary>
        /// Returns true if the property name, type, and value are set
        /// </summary>
        public bool IsSet
        {
            get { return (Name.IsSet && Type.IsSet && (_value != string.Empty)); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts the property to a CimKeyBinding
        /// </summary>
        /// <returns></returns>
        public CimKeyBinding ToCimKeyBinding()
        {
            CimKeyValue tmpCKV = new CimKeyValue();
            tmpCKV.ValueType = "string";
            tmpCKV.Value = this.Value;

            return new CimKeyBinding(this.Name, tmpCKV);
        }
        
        #region Equals, operator== , operator!=
        /// <summary>
        /// Deep compare of two CimProperty objects
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>returns true if the objects are equal</returns>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CimProperty))
            {
                return false;
            }
            return (this == (CimProperty)obj);
        }
        /// <summary>
        /// Deep compare of two CimProperty objects
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns>Returns true if the properties are equal</returns>
        public static bool operator ==(CimProperty val1, CimProperty val2)
        {

            if (((object)val1 == null) || ((object)val2 == null))
            {
                if (((object)val1 == null) && ((object)val2 == null))
                {
                    return true;
                }
                return false;
            }

            //Add code here
            if ((val1.Name != val2.Name) || (val1.Type != val2.Type) || (val1.ClassOrigin != val2.ClassOrigin))
                return false;
            if (val1.IsPropagated == val2.IsPropagated)
                return false;

            if (val1.Qualifiers != val2.Qualifiers)
                return false;
            if (val1.Value.ToLower() != val2.Value.ToLower())
                return false;
            return true;

        }
        /// <summary>
        /// Deep compare of two CimProperty objects
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns>Returns true if the properties are not equal</returns>
        public static bool operator !=(CimProperty val1, CimProperty val2)
        {
            return !(val1 == val2);
        }
        #endregion

        #endregion
    }
}
