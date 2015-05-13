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
    /// Abstract class that contains a NamespacePath and a Host
    /// </summary>
    internal abstract class CimObjectPath
    {
        /* <!ELEMENT OBJECTPATH (INSTANCEPATH|CLASSPATH)> 
         * */
        private CimNamespacePath _namespacePath = null;

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the Namespacepath
        /// </summary>
        internal CimNamespacePath NamespacePath
        {
            get
            {
                if (_namespacePath == null)
                    _namespacePath = new CimNamespacePath();

                return _namespacePath;
            }
            set { _namespacePath = value; }
        }

        /// <summary>
        /// Gets or sets the CimHost
        /// </summary>
        public CimName Host
        {
            get { return NamespacePath.Host; }
            set { NamespacePath.Host = value; }
        }

        /// <summary>
        /// Gets or sets the Namespace
        /// </summary>
        public CimName Namespace
        {
            get { return NamespacePath.Namespace; }
            set { NamespacePath.Namespace = value; }
        }

        /// <summary>
        /// Returns true if the host name is not set
        /// </summary>
        public bool IsLocalPath
        {
            get { return ! Host.IsSet; }
        }

        public abstract bool IsSet
        {
            get;
        }
        #endregion
    }


    //internal abstract class CimLocalObjectPath
    //{
    //    CimName _localNamespacePath = null;

    //    #region Properties and Indexers

    //    /// <summary>
    //    /// Gets or sets the Namespacepath
    //    /// </summary>
    //    public CimName Namespace
    //    {
    //        get
    //        {
    //            if (_localNamespacePath == null)
    //                _localNamespacePath = string.Empty;
    //            return _localNamespacePath;
    //        }
    //        set { _localNamespacePath = value; }
    //    }

    //    public abstract bool IsSet
    //    {
    //        get;
    //    }
    //    #endregion
    //}


    /// <summary>
    /// 
    /// </summary>
    //internal class CimInstancePath : CimObjectPath
    //{
    //    /* <!ELEMENT INSTANCEPATH (NAMESPACEPATH,INSTANCENAME)>
    //     * */
    //    private CimInstanceName _instanceName = null;

    //    #region Constructors

    //    #endregion

    //    #region Properties and Indexers
    //    /// <summary>
    //    /// Gets or sets the InstanceName
    //    /// </summary>
    //    public CimInstanceName InstanceName
    //    {
    //        get
    //        {
    //            if (_instanceName == null)
    //                _instanceName = new CimInstanceName(null);
    //            return _instanceName;
    //        }
    //        set { _instanceName = value; }
    //    }

    //    public override bool IsSet
    //    {
    //        get { return (NamespacePath.IsSet && InstanceName.IsSet); }
    //    }
    //    #endregion

    //    #region Methods

    //    #endregion
    //}

    //internal class CimNamespace : CimName
    //{
    //    public CimName(string name):base(name)
    //    {            
    //    }

    //    /// <summary>
    //    /// Implicitly convert from a string to CimLocalNamespacePath.
    //    /// </summary>
    //    /// <param name="value">String to convert</param>
    //    /// <returns>CimLocalNamespacePath</returns>
    //    public static implicit operator CimName(string value)
    //    {
    //        //This implicit conversion only works in Mono 1.1.16 and later
    //        return new CimName(value);
    //    }
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //internal class CimNamespacePath
    //{
    //    /* <!ELEMENT NAMESPACEPATH (HOST,LOCALNAMESPACEPATH)> 
    //     * */
    //    private string _host = null;
    //    //private CimLocalNamespacePath _localNamespacePath;
    //    private string _namespacePath = null;

    //    #region Constructors
    //    public CimNamespacePath()
    //    {

    //    }
    //    #endregion

    //    #region Properties and Indexers
    //    /// <summary>
    //    /// Gets or sets the CimHost
    //    /// </summary>
    //    public string Host
    //    {
    //        get
    //        {
    //            if (_host == null)
    //                _host = string.Empty;
    //            return _host;
    //        }
    //        set { _host = value; }
    //    }
    //    /// <summary>
    //    /// Gets or sets the LocalNamespacePath
    //    /// </summary>
    //    public string Namespace
    //    {
    //        get
    //        {
    //            if (_namespacePath == null)
    //                _namespacePath = string.Empty; ;
    //            return _namespacePath;
    //        }
    //        set { _namespacePath = value; }
    //    }

    //    public bool IsSet
    //    {
    //        get { return ((Host != string.Empty) && (Namespace != string.Empty)); }
    //    }
    //    #endregion

    //    #region Methods

    //    #endregion
    //}


    /// <summary>
    /// 
    /// </summary>
    //internal class CimClassPath : CimObjectPath
    //{
    //    /* <!ELEMENT CLASSPATH (NAMESPACEPATH,CLASSNAME)>
    //     * */



    //    private CimName _className = null;

    //    #region Constructors

    //    #endregion

    //    #region Properties and Indexers

    //    /// <summary>
    //    /// Gets or sets the name of the class
    //    /// </summary>
    //    public CimName ClassName
    //    {
    //        get
    //        {
    //            if (_className == null)
    //                _className = new CimName(null);
    //            return _className;
    //        }
    //        set { _className = value; }
    //    }

    //    public override bool IsSet
    //    {
    //        get { return (NamespacePath.IsSet && ClassName.IsSet); }
    //    }
    //    #endregion

    //    #region Methods

    //    #endregion
    //}

    /// <summary>
    /// 
    /// </summary>
    //internal class CimLocalInstancePath : CimLocalObjectPath
    //{
    //    /* <!ELEMENT LOCALINSTANCEPATH (LOCALNAMESPACEPATH,INSTANCENAME)>
    //     * */
    //    CimInstanceName _instanceName = null;

    //    public CimInstanceName InstanceName
    //    {
    //        get
    //        {
    //            if (_instanceName == null)
    //                _instanceName = new CimInstanceName(null);
    //            return _instanceName;
    //        }
    //        set { _instanceName = value; }
    //    }

    //    public override bool IsSet
    //    {
    //        get { return ((Namespace != string.Empty) && (InstanceName.IsSet)); }
    //    }
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //internal class CimLocalClassPath : CimLocalObjectPath
    //{
    //    //<!ELEMENT LOCALCLASSPATH (LOCALNAMESPACEPATH, CLASSNAME)>
    //    CimName _name = null;

    //    public CimName ClassName
    //    {
    //        get
    //        {
    //            if (_name == null)
    //                _name = new CimName(null);
    //            return _name;
    //        }
    //        set { _name = value; }
    //    }

    //    public override bool IsSet
    //    {
    //        get { return ((Namespace != string.Empty) && (ClassName.IsSet)); }
    //    }
    //}


}
