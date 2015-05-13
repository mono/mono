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
using System.Collections;
using System.Text;
using System.Management.Internal.BaseDataTypes;

namespace System.Management.Internal
{
    /// <summary>
    /// Holds an collection of CimQualifiers objects
    /// </summary>
    internal class CimQualifierList : BaseDataTypeList<CimQualifier>
    {

        #region Constructors
        /// <summary>
        /// Creates an empty CimQualifierList
        /// </summary>
        public CimQualifierList()
        {

        }

        /// <summary>
        /// Creates a new CimQualifierList with the given CimQualifiers
        /// </summary>
        /// <param name="qualifiers"></param>
        public CimQualifierList(params CimQualifier[] qualifiers)
            : base(qualifiers)
        {
        }
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets a CimQualifier based on the name. Note: a new CimName is created base on name, and then the collection is searched.
        /// </summary>
        /// <param name="name">Name of the CimQualifier</param>
        /// <returns>CimQualifier or null if not found</returns>
        public CimQualifier this[string name]
        {
            get { return FindItem(new CimName(name)); }
        }
        /// <summary>
        /// Gets a CimQualifier based on the name
        /// </summary>
        /// <param name="name">Name of the CimQualifier</param>
        /// <returns>CimQualifier or null if not found</returns>
        public CimQualifier this[CimName name]
        {
            get { return FindItem(name); }
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Removes a CimQualifier from the collection, based the the name
        /// </summary>
        /// <param name="name">Name of the qualifier to remove</param>
        public bool Remove(string name)
        {
            return Remove(new CimName(name));

        }


        /// <summary>
        /// Removes a CimQualifier from the collection, based the the name
        /// </summary>
        /// <param name="name">Name of the qualifier to remove</param>        
        public bool Remove(CimName name)
        {
            CimQualifier prop = FindItem(name);
            if (prop != null)
            {
                items.Remove(prop);
                return true;
            }
            return false;
        }

        private CimQualifier FindItem(CimName name)
        {
            foreach (CimQualifier curProp in items)
            {
                if (curProp.Name == name)
                    return curProp;
            }
            return null;

        }

        
        #region Equals, operator== , operator!=
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CimQualifierList))
            {
                return false;
            }
            return (this == (CimQualifierList)obj);
        }
        /// <summary>
        /// Shallow compare of two lists
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator ==(CimQualifierList val1, CimQualifierList val2)
        {

            if (((object)val1 == null) || ((object)val2 == null))
            {
                if (((object)val1 == null) && ((object)val2 == null))
                {
                    return true;
                }
                return false;
            }
            if (val1.Count != val2.Count)
                return false;

            //Changing to for loop for MONO
            for(int i = 0; i < val1.Count;++i)
            {
            	if (val2.FindItem(val1[i].Name) == null)
                    return false;
            }
            for(int i = 0; i < val2.Count;++i)
            {
            	if (val1.FindItem(val2[i].Name) == null)
                    return false;
            }
            return true;

        }

        /// <summary>
        /// Shallow compare of two lists
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator !=(CimQualifierList val1, CimQualifierList val2)
        {
            return !(val1 == val2);
        }
        #endregion

        /// <summary>
        /// Shallow compare of two lists
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator <=(CimQualifierList list1, CimQualifierList list2)
        {
            //return ((list1 < list2) || (list1 == list2));
            if (((object)list1 == null) || ((object)list2 == null))
            {
                if (((object)list1 == null) && ((object)list2 == null))
                {
                    return true;
                }
                return false;
            }
            if (list1.Count > list2.Count)
                return false;

            //Changing to for loop for MONO
            for(int i = 0; i < list1.Count; ++i)
            {
            	if (list2.FindItem(list1[i].Name) == null)
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Shallow compare of two lists
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator >=(CimQualifierList val1, CimQualifierList val2)
        {
            return (val2 <= val1);
        }
        /// <summary>
        /// Performs a deep compare
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool IsEqualTo(CimQualifierList list)
        {
            return true;
        }
        #endregion
    }
}

