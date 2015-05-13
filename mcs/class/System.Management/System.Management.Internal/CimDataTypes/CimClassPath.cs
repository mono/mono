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
    /// <summary>
    /// Represent a CimClass as well as the host and namespace path to the CimClass
    /// </summary>
    internal class CimClassPath : CimObjectPath
    {
        /* <!ELEMENT CLASSPATH (NAMESPACEPATH,CLASSNAME)>
         * */

        //private CimName _className = null;
        private CimClass _class = null;

        /// <summary>
        /// Creates an empty CimClassPath
        /// </summary>
        public CimClassPath()
        {
        }

        /// <summary>
        /// Creates a CimClassPath object with the CimClass and the NamespacePath set
        /// </summary>
        /// <param name="mClass"></param>
        /// <param name="namespacepath"></param>
        public CimClassPath(CimClass mClass, CimNamespacePath namespacepath)
        {
            Class = mClass;
            NamespacePath = namespacepath;
        }
        #region Properties and Indexers


        ///// <summary>
        ///// Gets or sets the name of the class
        ///// </summary>
        //public CimName ClassName
        //{
        //    get
        //    {
        //        if (_className == null)
        //            _className = new CimName(null);
        //        return _className;
        //    }
        //    set { _className = value; }
        //}

        /// <summary>
        /// Gets or sets the name of the class
        /// </summary>
        public CimClass Class
        {
            get { return _class; }
            set { _class = value; }
        }

        /// <summary>
        /// Returns true if the Namespace and Class are both set
        /// </summary>
        public override bool IsSet
        {
            get { return (NamespacePath.IsSet && Class.IsSet); }
        }
        #endregion
    }
}