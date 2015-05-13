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
using System.Management.Internal.BaseDataTypes;

namespace System.Management.Internal
{
    /// <summary>
    /// The INSTANCE element defines a single CIM Instance of a CIM Class
    /// </summary>
    internal class CimInstance : CimObject
    {
        /* <!ELEMENT INSTANCE (QUALIFIER*,(PROPERTY|PROPERTY.ARRAY|PROPERTY.REFERENCE)*)>
         * <!ATTLIST INSTANCE 
         *      %ClassName;
         *      xml:lang   NMTOKEN  #IMPLIED>
         * */


        CimInstanceName _name = null;

        #region Constructors

        /// <summary>
        /// Creates a CimInstance object with 
        /// </summary>
        /// <param name="className"></param>
        public CimInstance(string className)
            : this(new CimName(className))
        {
        }

        public CimInstance(CimName className)
        {
            ClassName = className;
        }
        #endregion

        #region Properties
        /// <summary>
        /// InstanceName of the instance
        /// </summary>
        public CimInstanceName InstanceName
        {
            get
            {
                if (_name == null)
                    _name = new CimInstanceName(string.Empty);

                return _name;
            }
            set { _name = value; }
        }

        /// <summary>
        /// Gets or sets the name of the CimClass object
        /// </summary>
        public override CimName ClassName
        {
            get { return InstanceName.ClassName; }
            set { InstanceName.ClassName = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Checks the properties of a CimInstance object against the keyProperties list
        /// </summary>
        /// <param name="keyProperties">list of key properties to check</param>
        /// <returns>Returns true if all the key properties are set</returns>
        public bool AreKeyPropertiesSet(CimPropertyList keyProperties)
        {
            //This method is only called in one place, in CimInstanceForm. Should this be in GUI library somewhere
        	//Changed for MONO
        	for (int i = 0; i < keyProperties.Count; ++i)
            {
        		CimProperty curKeyProp = keyProperties[i];
                if (! this.Properties.Contains(curKeyProp.Name))
                    return false;   // This instance is missing a key property

                if (this.Properties[curKeyProp.Name].Value == string.Empty)
                    return false;   // The key property isn't set
            }  
            // if it makes it to here then all key properties are set.
            return true;
        }

        /// <summary>
        /// Returns the string with the name and key property values of the instance. It requires the class
        /// definition in order to find the required properties, and the instance itself doesn't have the needed
        /// qualifiers attached to the properties.
        /// </summary>
        /// <param name="classDef">class definition of the instance</param>
        /// <returns></returns>
        public string ToString(CimClass classDef)
        {
            string retval = this.ClassName.ToString();

            //In case they pass in the wrong class def 
            if (classDef.ClassName != this.ClassName)
            {
                return retval;
            }

            retval += " [";

            //Changed for MONO
            CimPropertyList keyprops = classDef.Properties.GetKeyProperties();
            for (int i = 0; i < keyprops.Count; ++i)
            {
                CimProperty keyprop = keyprops[i];
                CimProperty localProp = this.Properties[keyprop.Name];
                if (localProp != null)
                {
                    retval += " " + localProp.Name + "=" + localProp.Value;
                }
                else
                {
                    retval += " " + keyprop.Name + "=" + "{NOT SET}";
                }
            }
            retval += "]";

            return retval;
        }

        /// <summary>
        /// Compares the members of CimInstance (ClassName, Qualifiers, Properties)
        /// </summary>
        /// <param name="obj">CimInstance to compare to</param>
        /// <returns>Returns true if the two CimInstances have the same class anme, qualifiers, and properties</returns>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CimInstance))
            {
                return false;
            }
            return (this == (CimInstance)obj);

           
        }

        /// <summary>
        /// Compares the members of CimInstance (ClassName, Qualifiers, Properties)
        /// </summary>
        /// <param name="instance1"></param>
        /// <param name="instance2"></param>
        /// <returns>Returns true if the two CimInstances have the same class anme, qualifiers, and properties</returns>
        public static bool operator==(CimInstance instance1, CimInstance instance2)
        {
            if (((object)instance1 == null) || ((object)instance2 == null))
            {
                if (((object)instance1 == null) && ((object)instance2 == null))
                {
                    return true;
                }
                return false;
            }

            if (instance1.ClassName != instance2.ClassName)
            {
                return false;
            }
            if (instance1.Qualifiers != instance2.Qualifiers)
            {
                return false;
            }
            if (instance1.Properties != instance2.Properties)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Compares the members of CimInstance (ClassName, Qualifiers, Properties)
        /// </summary>
        /// <param name="instance1"></param>
        /// <param name="instance2"></param>
        /// <returns>Returns true if the two CimInstances have the same class anme, qualifiers, and properties</returns>
        public static bool operator!=(CimInstance instance1, CimInstance instance2)
        {
            return !(instance1 == instance2);
        }
        #endregion

    }

}
