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
using System.IO;
using System.Net;
using System.Text;

namespace System.Management.Internal.Net
{
    internal class CimomResponse
    {
        private HttpWebResponse httpResp;
        private HttpStatusCode statusCode;
        private string statusDescription;
        private string message;

        #region Delegates
        public delegate void CimomResponseHandler(int statusCode, string msg);
        public static event CimomResponseHandler OnCimomResponse = null;
        #endregion

        #region Constructors
        public CimomResponse(HttpWebResponse nHttpResp)
        {            
            httpResp = nHttpResp;
            statusCode = httpResp.StatusCode;
            statusDescription = httpResp.StatusDescription;

            StreamReader streamRead = new StreamReader(httpResp.GetResponseStream(), System.Text.UTF8Encoding.UTF8);
            message = streamRead.ReadToEnd();

            streamRead.Close();

            // Release the HttpResponse Resource.
            httpResp.Close();

            if (OnCimomResponse != null)
            {
                OnCimomResponse((int)this.StatusCode, Message);
            }
        }
        #endregion

        #region Properties and Indexers
        public HttpStatusCode StatusCode
        {
            get { return statusCode; }
        }

        public string StatusDescription
        {
            get { return statusDescription; }
        }

        public string Message
        {
            get { return message; }         
        }
        #endregion
    }
}
