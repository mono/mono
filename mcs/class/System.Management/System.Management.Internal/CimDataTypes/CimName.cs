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
    /// The CIMName entity describes the name of a CIM Element (Class, Instance, Method, Property, Qualifier or Parameter). The value MUST be a legal CIM element name. 
    /// CimName is a case insensitive string. A CimName should not include characters that define path delimiters, such as .,:;\/ unless the name is a path like 'root/cimv2'
    /// </summary>
    internal class CimName : IComparable, ICimObjectName
    {
        /*
         * <!ENTITY % CIMName "NAME CDATA #REQUIRED">
         * 
         * CimName encapsulates a Cim Name, which is simply a string, with the
         * special semantic of being case-insensitive and preserving.
         */
        private string _value;
        //private static char[] invalidChars = @".,:;".ToCharArray();

        #region Constructors
        /// <summary>
        /// Creates a CimName object and sets the value of the object
        /// </summary>
        /// <param name="name"></param>
        public CimName(string name)
        {
            Value = name;
        }
        #endregion

        #region Properties
       
        /// <summary>
        /// Sets the value of the CimName object. CimName should not include path delimiting characters, such as .,:;\/
        /// </summary>
        private string Value
        {
            get { return _value; }
            set 
            {
                if (value == string.Empty)
                    _value = null;      // Essentially set IsSet to false
                else
                {
                    // Can't enforce this because a namespace has a path like root/cimv2
                    //if (value.IndexOfAny(invalidChars) >= 0)//The string has at least one of these chars
                    //    throw new Exception("Invalid chars in CimName. Invalid chars are \"" + invalidChars + "\"");
                    //else
                        _value = value;
                }
            }
        }

        /// <summary>
        /// Returns the prefix of the CimName. "CIM_NFS" returns "CIM"
        /// </summary>
        public CimName Schema
        {
            get
            {
                int idx = this.ToString().IndexOf('_');

                if (idx < 0)
                    return string.Empty;
                else
                    return this.ToString().Substring(0, idx);
            }
        }

        /// <summary>
        /// Returns true is the value of the name is not null
        /// </summary>
        public bool IsSet
        {
            get { return (Value != null); }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the value of the CimName object.
        /// </summary>
        /// <returns>CimName.Name</returns>
        public override string ToString()
        {
            // Guaranteed not to return null.
            if (Value == null)
                return string.Empty;

            return Value;
        }

        /// <summary>
        /// Compares the values of two CimName objects
        /// </summary>
        /// <param name="obj">object to compare</param>
        /// <returns>True if the values of the CimName objects are equal</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj is CimName)
            {                
                return (AreEqual(this.Value, ((CimName)obj).Value));
            }
            else if (obj is string)
            {
                return AreEqual(this.ToString(), (string)obj);
            }
            
            throw new InvalidCastException("Can only compare CimName objects to other CimName objects or strings");
        }
        
        private static bool AreEqual(string name1, string name2)
        {
            if ((name1 == null) &&
                (name2 == null))
            {
                return true;
            }

            if ((name1 == null) ||
                (name2 == null))
            {
                // Since it didn't return true above, then they are not equal
                return false;
            }

            return (name1.ToLower() == name2.ToLower());                
        }

        /// <summary>
        /// Returns the hashcode of the underlying lowercase string.
        /// </summary>
        /// <returns>Hashscode of the string</returns>
        public override int GetHashCode()
        {
            return _value.ToLower().GetHashCode();
        }

        #endregion

        #region Operators

        /// <summary>
        /// Compare a CimName object and an object
        /// </summary>
        /// <param name="cimName">CimName object</param>
        /// <param name="obj">object value</param>
        /// <returns>Returns true if the values are the same</returns>
        public static bool operator ==(CimName cimName, object obj)
        {
            if ((object)cimName == null)
            {
                if (obj == null)
                    return true;
                else
                    return false;
            }
            else
                return cimName.Equals(obj);          
        }

        

        /// <summary>
        /// Compare a CimName object and an object
        /// </summary>
        /// <param name="cimName">CimName object</param>
        /// <param name="obj">string value</param>
        /// <returns>Returns true if the values are not the same</returns>        
        public static bool operator !=(CimName cimName, object obj)
        {
            if ((object)cimName == null)
            {
                if (obj != null)
                    return true;
                else
                    return false;
            }
            else
                return !cimName.Equals(obj);  
        }

        /// <summary>
        /// Implicitly convert from a string to CimName.
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>CimName</returns>
        public static implicit operator CimName(string value)
        {
            return new CimName(value);
        }
        #endregion


        #region IComparable Members

        /// <summary>
        /// Sortable by the string Name member
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            string val = ((CimName)obj).Value.ToLower();
            return this.Value.ToLower().CompareTo(val);
        }

        #endregion

        
    }
}
