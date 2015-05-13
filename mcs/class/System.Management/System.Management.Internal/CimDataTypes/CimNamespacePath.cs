//
// AssemblyRef
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Management.Internal
{
    internal class CimNamespacePath
    {
        /* <!ELEMENT NAMESPACEPATH (HOST,LOCALNAMESPACEPATH)> 
         * */
        private CimName _host = null;
        //private CimLocalNamespacePath _localNamespacePath;
        private CimName _namespacePath = null;

        #region Constructors
        public CimNamespacePath()
        {

        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the CimHost
        /// </summary>
        public CimName Host
        {
            get
            {
                if (_host == null)
                    _host = new CimName(null);

                return _host;
            }
            set { _host = value; }
        }
        /// <summary>
        /// Gets or sets the LocalNamespacePath
        /// </summary>
        public CimName Namespace
        {
            get
            {
                if (_namespacePath == null)
                    _namespacePath = new CimName(null);

                return _namespacePath;
            }
            set { _namespacePath = value; }
        }

        public bool IsSet
        {
            get { return ((Host != string.Empty) && (Namespace != string.Empty)); }
        }
        #endregion

        #region Methods

        #endregion
    }
}