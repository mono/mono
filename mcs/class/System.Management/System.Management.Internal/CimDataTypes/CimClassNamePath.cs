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
    /// Includes a CimName, NamespacePath, and a Host
    /// </summary>
    internal class CimClassNamePath : CimObjectPath
    {
        #region Members

        private CimName _className = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an empty CimClassNamePath
        /// </summary>
        public CimClassNamePath()
        {
        }

        /// <summary>
        /// Creates a CimClassNamePath with a class name
        /// </summary>
        /// <param name="className"></param>
        public CimClassNamePath(CimName className)
        {
            ClassName = className;
        }

        #endregion

        #region Properties and Indexers

        /// <summary>
        /// Name of the class
        /// </summary>
        public CimName ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        public override bool IsSet
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region Methods and Operators

        #endregion


    }
}
