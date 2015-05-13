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
    /// Holds an collection of CimMethod objects
    /// </summary>
    internal class CimMethodList : BaseDataTypeList<CimMethod>
    {

        #region Constructors
        /// <summary>
        /// Creates an empty CimMethodList
        /// </summary>
        public CimMethodList()
        {

        }

        /// <summary>
        /// Creates a new CimMethodList with the given CimMethods
        /// </summary>
        /// <param name="methods"></param>
        public CimMethodList(params CimMethod[] methods)
            : base(methods)
        {
        }
        #endregion

        #region Properties
       
        /// <summary>
        /// Gets a CimMethod based on the name
        /// </summary>
        /// <param name="name">Name of the CimMethod</param>
        /// <returns>CimMethod or null if not found</returns>
        public CimMethod this[string name]
        {
            get { return FindItem(new CimName(name)); }
        }
        /// <summary>
        /// Gets a CimMethod based on the name
        /// </summary>
        /// <param name="name">Name of the CimMethod</param>
        /// <returns>CimMethod or null if not found</returns>
        public CimMethod this[CimName name]
        {
            get { return FindItem(name); }
        }



        #endregion

        #region Methods
        
        /// <summary>
        /// Removes a CimMethod from the collection, based the the name
        /// </summary>
        /// <param name="name">Name of the method to remove</param>
        public bool Remove(string name)
        {
            return Remove(new CimName(name));

        }


        /// <summary>
        /// Removes a CimMethod from the collection, based the the name
        /// </summary>
        /// <param name="name">Name of the method to remove</param>        
        public bool Remove(CimName name)
        {
            CimMethod prop = FindItem(name);
            if (prop != null)
            {
                items.Remove(prop);
                return true;
            }
            return false;



        }

        private CimMethod FindItem(CimName name)
        {
            //In the future, use predicates to do the search
            //this._items.Find(Predicate<CimMethod> match)
            //Maybe do this in the base class?

            foreach (CimMethod curProp in items)
            {
                if (curProp.Name == name)
                    return curProp;
            }
            
            return null;

        }

        #endregion
    }
}
