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
    /// 
    /// </summary>
    //internal class CimNamedInstance
    //{
    //    //<!ELEMENT VALUE.NAMEDINSTANCE (INSTANCENAME,INSTANCE)>

    //    // FIXME: Temporary solution for twiest 
    //    private CimInstanceName _name = null;
    //    private CimInstance _instance = null;


    //    #region Cosntructors
    //    public CimNamedInstance()
    //    {
    //    }
    //    public CimNamedInstance(CimInstanceName name, CimInstance instance)
    //    {
    //        Name = name;
    //        Instance = instance;
    //    }

    //    #endregion

    //    #region Properties and indexers
    //    public CimInstanceName Name
    //    {
    //        get 
    //        {
    //            if (_name == null)
    //                _name = new CimInstanceName(null);

    //            return _name; 
    //        }
    //        set { _name = value; }
    //    }
    //    public CimInstance Instance
    //    {
    //        get 
    //        {
    //            if (_instance == null)
    //                _instance = new CimInstance(null);

    //            return _instance; 
    //        }
    //        set { _instance = value; }
    //    }

    //    public bool IsSet
    //    {
    //        get { return (Name.IsSet && Instance.ClassName.IsSet); }
    //    }
    //    #endregion

    //    #region Methods

    //    #endregion
    //}

    //internal class CimNamedInstanceList : BaseDataTypeList<CimNamedInstance>
    //{
    //}
}
