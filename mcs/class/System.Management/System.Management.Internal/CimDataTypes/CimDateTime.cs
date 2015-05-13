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


namespace System.Management.Internal.CimDataTypes
{
    /// <summary>
    /// 
    /// </summary>
    class CimDateTime
    {
        private DateTime _dateTime;

        //Do we even need this class? Or can we just use System.DateTime?
        //Is there an advantage to using a custom type?
        //Perhaps a Conversion method or two? string -> DateTime, DateTime -> string

        #region Constructors
        public CimDateTime():this(new DateTime(0))
        {
            
        }
        // FIX ME: We need to handle this string so that _dateTime is never null
        //public CimDateTime(string datetime)
        //{
        //}
        public CimDateTime(DateTime dateTime)
        {
            _dateTime = dateTime;
        }

        #endregion

        #region Properties
        public int Day
        {
            get { return _dateTime.Day; }
            set 
            {
                
            }
        }
        #endregion

        #region Methods

        #endregion

    }
}
