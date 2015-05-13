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
    /// A list of CimKeyBinding objects
    /// </summary>
    internal class CimKeyBindingList : BaseDataTypeList<CimKeyBinding>
    {
        #region Members

        #endregion

        #region Constructors

        #endregion

        #region Properties and Indexers

        /// <summary>
        /// Gets a CimKeyBinding based on the name
        /// </summary>
        /// <param name="name">Name of the CimKeyBinding</param>
        /// <returns>CimKeyBinding or null if not found</returns>
        public CimKeyBinding this[string name]
        {
            get { return FindItem(new CimName(name)); }
        }
        /// <summary>
        /// Gets a CimKeyBinding based on the name
        /// </summary>
        /// <param name="name">Name of the CimKeyBinding</param>
        /// <returns>CimKeyBinding or null if not found</returns>
        public CimKeyBinding this[CimName name]
        {
            get { return FindItem(name); }
        }
        #endregion

        
        #region Methods and Operators
        private CimKeyBinding FindItem(CimName name)
        {
            foreach (CimKeyBinding curItem in items)
            {
                if (curItem.Name == name)
                    return curItem;
            }
            return null;
        }
        /// <summary>
        /// Compares the names of the KeyBindings in the list. 
        /// </summary>
        /// <param name="list">CimKeyBindingList to compare with</param>
        /// <returns>True if the lists have the same count and same items</returns>
        public bool ShallowEquals(CimKeyBindingList list)
        {
            if (this.Count != list.Count)
                return false;           
            
            //Changed for MONO
            for (int i =0; i < this.Count; ++i)
            {
            	if (list.FindItem(this[i].Name) == null)
                    return false;  
            }
            for (int i =0; i < list.Count; ++i)
            {
            	if (this.FindItem(list[i].Name) == null)
                    return false;  
            }
            
            return true;
        }

        #region Equals, operator== , operator!=
        /// <summary>
        /// Returns true if the two CimKeyBindingLists have the same key bindings
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CimKeyBindingList))
            {
                return false;
            }
            return (this == (CimKeyBindingList)obj);
        }

        /// <summary>
        /// Returns true if the two CimKeyBindingLists have the same key bindings. 
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator ==(CimKeyBindingList val1, CimKeyBindingList val2)
        {
            if (((object)val1 == null) || ((object)val2 == null))
            {
                if (((object)val1 == null) && ((object)val2 == null))
                {
                    return true;
                }
                return false;
            }
            return val1.ShallowEquals(val2);

            //throw new Exception("Not implemented yet");
            //if (val1.Count != val2.Count)
            //    return false;

            //for (int i = 0; i < val1.Count; i++)
            //{
            //    if ((CimKeyBinding)val1[i] != (CimKeyBinding)val2[i])
            //        return false;
            //}

            //return true;
        }

        /// <summary>
        /// Returns true if the two CimKeyBindingLists do not have the same key bindings
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns></returns>
        public static bool operator !=(CimKeyBindingList val1, CimKeyBindingList val2)
        {
            return !(val1 == val2);
        }
        #endregion
        
        #endregion
        
    }
}
