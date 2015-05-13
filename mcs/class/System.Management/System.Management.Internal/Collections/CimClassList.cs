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
    /// A list of CimClass objects
    /// </summary>
    internal class CimClassList : BaseDataTypeList<CimClass>
    {
        #region Properties
        /// <summary>
        /// Gets a CimClass based on the name. Note: a new CimName is created based on name, and then the collection is searched.
        /// </summary>
        /// <param name="name">Name of the CimClass</param>
        /// <returns>CimClass or null if not found</returns>
        public CimClass this[string name]
        {
            get { return FindItem(new CimName(name)); }
        }
        /// <summary>
        /// Gets a CimClass based on the name
        /// </summary>
        /// <param name="name">Name of the CimClass</param>
        /// <returns>CimClass or null if not found</returns>
        public CimClass this[CimName name]
        {
            get { return FindItem(name); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Removes the specified CimClass from the list
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(string name)
        {
            return Remove(new CimName(name));
        }

        /// <summary>
        /// Removes the specified CimClass from the list
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool Remove(CimName name)
        {
            CimClass prop = FindItem(name);
            if (prop != null)
            {
                items.Remove(prop);
                return true;
            }
            return false;
        }

        private CimClass FindItem(CimName name)
        {
            foreach (CimClass curProp in items)
            {
                if (curProp.ClassName == name)
                    return curProp;
            }
            return null;
        }

        #endregion

    }
}
