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
    /// Holds an collection of CimProperty objects
    /// </summary>
    internal class CimPropertyList : BaseDataTypeList<CimProperty>    
    {

        #region Constructors
        /// <summary>
        /// Creates an empty CimPropertyList
        /// </summary>
        public CimPropertyList()
        {

        }

        /// <summary>
        /// Creates a new CimPropertyList with the given CimProperties
        /// </summary>
        /// <param name="properties"></param>
        public CimPropertyList(params CimProperty[] properties)
            : base(properties)
        {
        }
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets a CimProperty based on the name
        /// </summary>
        /// <param name="name">Name of the CimProperty</param>
        /// <returns>CimProperty or null if not found</returns>
        public CimProperty this[string name]
        {
            get { return FindItem(new CimName(name)); }
        }
        /// <summary>
        /// Gets a CimProperty based on the name
        /// </summary>
        /// <param name="name">Name of the CimProperty</param>
        /// <returns>CimProperty or null if not found</returns>
        public CimProperty this[CimName name]
        {
            get { return FindItem(name); }
        }

        /// <summary>
        /// Returns true if the list has at least one key property
        /// </summary>
        public bool HasKeyProperty
        {
            get
            {
            	//Changed for MONO
            	for (int i = 0; i < this.Count; i++)
                {
            		if (this[i].IsKeyProperty)
                        return true;
                }                

                return false;
            }
        }

        #endregion

        #region Methods
        
        
        /// <summary>
        /// Removes a CimProperty from the collection, based the the name
        /// </summary>
        /// <param name="name">Name of the property to remove</param>
        public bool Remove(string name)
        {
            return Remove(new CimName(name));

        }


        /// <summary>
        /// Removes a CimProperty from the collection, based the the name
        /// </summary>
        /// <param name="name">Name of the property to remove</param>        
        public bool Remove(CimName name)
        {
            CimProperty prop = FindItem(name);
            if (prop != null)
            {
                items.Remove(prop);
                return true;
            }
            return false;
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private CimProperty FindItem(CimName name)
        {
            foreach (CimProperty curItem in items)
            {
                if (curItem.Name == name)
                    return curItem;
            }
            return null;          
        }

        public bool Contains(CimName propertyName)
        {
            return (FindItem(propertyName) != null);
        }

      
        
        #region Equals, operator== , operator!=
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CimPropertyList))
            {
                return false;
            }
            return (this == (CimPropertyList)obj);
        }
        /// <summary>
        /// Shallow compare two CimPropertyLists
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns>Returns true if both lists have the same elements with the same names</returns>
        public static bool operator ==(CimPropertyList list1, CimPropertyList list2)
        {

            if (((object)list1 == null) || ((object)list2 == null))
            {
                if (((object)list1 == null) && ((object)list2 == null))
                {
                    return true;
                }
                return false;
            }
            if (list1.Count != list2.Count)
                return false;
            
            //Changing to for loops for MONO
            //check that all A's exist in B
            for(int i = 0; i < list1.Count; i++)
            {
            	if (list2.FindItem(list1[i].Name) == null)
                    return false;
            }
            //check that all B's exist in A
            for(int i = 0; i < list2.Count; i++)
            {
            	if (list1.FindItem(list2[i].Name) == null)
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Shallow compare of two CimPropertyLists
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns>Returns true if the lists do not have the same elements</returns>
        public static bool operator !=(CimPropertyList list1, CimPropertyList list2)
        {
            return !(list1 == list2);
        }

        #endregion
       
        #region Operator <,>,<=,>=
        /*  
        /// <summary>
        /// Determines whether a list is a subset of another list (shallow compare)
        /// </summary>
        /// <param name="list1">Subset list</param>
        /// <param name="list2">Superset list</param>
        /// <returns>Returns true if list1 is a subset of list2</returns>
        public static bool operator <(CimPropertyList list1, CimPropertyList list2)
        {
            if (!(list1 <= list2))
                return false;
            //list1 is a subset of list2
            return !(list1 == list2);//return true if the two lists are not equal
        }
        public static bool operator >(CimPropertyList list1, CimPropertyList list2)
        {
            return (list2 < list1);
        }
         * */
        /// <summary>
        /// Determines whether a list is a subset of another list (shallow compare)
        /// </summary>
        /// <param name="list1">Subset list</param>
        /// <param name="list2">Superset list</param>
        /// <returns>Returns true if list1 is a subset of list2</returns>
        public static bool operator <=(CimPropertyList list1, CimPropertyList list2)
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
            for (int i = 0; i < list1.Count; ++i)
            {
            	if (list2.FindItem(list1[i].Name) == null)
                    return false;
            }

            return true;
        }
        /// <summary>
        /// Determines whether a list is a superset of another list (shallow compare)
        /// </summary>
        /// <param name="list1">superset list</param>
        /// <param name="list2">subset list</param>
        /// <returns>Returns true if list1 is a superset of list2</returns>
        public static bool operator >=(CimPropertyList list1, CimPropertyList list2)
        {
            return (list2 <= list1);
        }
        #endregion

        /// <summary>
        /// Performes a deep compare of two CimPropertyLists
        /// </summary>
        /// <param name="list">CimPropertyList to compare</param>
        /// <returns>Returns true if the lists have the same properties and values</returns>
        public bool IsEqualTo(CimPropertyList list)
        {
            if (this != list)//do shallow compare first
                return false;

            //changing to for loop for MONO
            for(int i = 0; i < this.Count; i++)
            {
            	CimProperty p = list.FindItem(this[i].Name);
            	if ((p == null)||(p != this[i]))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Finds all key properties in the list
        /// </summary>
        /// <returns>A list of all the key properties</returns>
        public CimPropertyList GetKeyProperties()
        {
            CimPropertyList list = new CimPropertyList();
            //Changing to for loop for MONO
            for (int i = 0; i < this.Count; ++i)
            {
            	if (this[i].IsKeyProperty)
            		list.Add(this[i]);
            }

            return list;
        }

        /// <summary>
        /// Finds all required properties in the list
        /// </summary>
        /// <returns>A list of all the required properties</returns>
        public CimPropertyList GetRequiredProperties()
        {
            CimPropertyList list = new CimPropertyList();
            //Changing to for loop for MONO
            for (int i = 0; i < this.Count; ++i)
            {
                if (this[i].IsRequiredProperty)
                    list.Add(this[i]);
            }

            return list;
        }
        #endregion

    }
}
