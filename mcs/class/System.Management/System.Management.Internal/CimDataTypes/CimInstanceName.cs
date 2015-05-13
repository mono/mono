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
    internal class CimInstanceName : ICimObjectName
    {
        /* <!ELEMENT INSTANCENAME (KEYBINDING*|KEYVALUE?|VALUE.REFERENCE?)>
         * <!ATTLIST INSTANCENAME
         *      %ClassName;>
         * 
         * <INSTANCENAME CLASSNAME="OMC_UnitaryComputerSystem">
         *      <KEYBINDING NAME="CreationClassName">
         *          <KEYVALUE VALUETYPE="string">OMC_UnitaryComputerSystem</KEYVALUE>
         *      </KEYBINDING>
         *      <KEYBINDING NAME="Name">
         *          <KEYVALUE VALUETYPE="string">d1850.cim.lab.novell.com</KEYVALUE>
         *      </KEYBINDING>
         *  </INSTANCENAME>
         * */                
        CimKeyBindingList _keyBindings = null;
        private CimName _className;

        #region Constructors
        public CimInstanceName(CimName className)
        {
            ClassName = className;
        }
        public CimInstanceName(string className)
            : this(new CimName(className))
        {
        }

        #endregion

        #region Properties
                
        /// <summary>
        /// Gets or set the value
        /// </summary>
        public CimKeyBindingList KeyBindings
        {
            get 
            {
                if (_keyBindings == null)
                    _keyBindings = new CimKeyBindingList();
                return _keyBindings; 
            }
            set { _keyBindings = value; }
        }

        /// <summary>
        /// Name of the class
        /// </summary>
        public CimName ClassName
        {
            get
            {
                if (_className == null)
                    _className = new CimName(string.Empty);

                return _className;
            }
            set { _className = value; }
        }

        /// <summary>
        /// Returns true if the class name and key bindings are set.
        /// </summary>
        public bool IsSet
        {
            get { return (ClassName.IsSet && KeyBindings.IsSet); }
        }
        #endregion


        #region Methods
        public override string ToString()
        {
            string retval = this.ClassName.ToString();        

            retval += " [";
            for (int i = 0; i < KeyBindings.Count; ++i)
            {
                retval += " " + KeyBindings[i].Name + "=" + KeyBindings[i].ToString();
            }
            retval += "]";

            return retval;
        }
        

        #endregion
    }


    
}
