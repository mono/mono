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
using System.Text;
using System.Management.Internal.Batch;


namespace System.Management.Internal
{
    internal class EnumerateClassNamesOpSettings : SingleRequest
    {
        CimName _className;
        bool _deepInheritance;

        #region Constructors
        public EnumerateClassNamesOpSettings()
            : this((CimName)null)
        {
        }

        public EnumerateClassNamesOpSettings(string className)
            : this(new CimName(className))
        {
        }

        public EnumerateClassNamesOpSettings(CimName className)
        {
            ReqType = RequestType.EnumerateClassNames;

            ClassName = className;
            DeepInheritance = false;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The ClassName input parameter defines the Class that is the basis for the enumeration.
        /// </summary>
        public CimName ClassName
        {
            get
            {
                if (_className == null)
                    _className = new CimName(null);

                return _className;
            }
            set { _className = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the DeepInheritance input parameter istrue, this specifies that the names of all subclasses of the specified Class should be returned (if the ClassName input parameter is absent, this implies that the names of all Classes in the target Namespace should be returned). If false, only the names of immediate child subclasses are returned (if theClassName input parameter is NULL, this implies that the names of all base Classes in the target Namespace should be returned). This definition of DeepInheritance applies only to the EnumerateClasses and EnumerateClassName operations.
        /// </summary>
        public bool DeepInheritance
        {
            get { return _deepInheritance; }
            set { _deepInheritance = value; }
        }
        #endregion
    }
}
