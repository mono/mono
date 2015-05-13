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
using System.Security.Cryptography.X509Certificates;

namespace System.Management.Internal.Net
{
    internal class CimomRequest
    {        
        private HttpWebRequest httpReq;
        private CimName nameSpaceValue;
        private string message = string.Empty;
        private static Random rand = new Random();

        #region Delegates 
        public delegate void CimomRequestHandler(string msg);
        public static event CimomRequestHandler OnCimomRequest = null;
        #endregion

        #region Constructors
        public CimomRequest(string uri, ICertificatePolicy certificatePolicy)
        {
            // Build a CimomRequest off of a new HttpRequest
            httpReq = (HttpWebRequest)WebRequest.Create(uri);
            //httpReq.KeepAlive = false;

            // The namespace needs a 2 digit number between 00 and 99
            nameSpaceValue = rand.Next(10).ToString() + rand.Next(10).ToString();

            // Setup the http request
            httpReq.PreAuthenticate = false;            
            httpReq.ProtocolVersion = HttpVersion.Version11;
            httpReq.Method = "M-POST";
            httpReq.ContentType = "application/xml;charset=\"UTF-8\"";
            httpReq.Accept = "text/xml,application/xml";


            // I get an "Unknown Extension URI" when I try to use this URL :(
            //Headers.Add("Man", "http://www.dmtf.org/standards/documents/WBEM/DSP200.html;ns=" + nameSpaceValue);

            // Add the Http/CIM headers
            Headers.Add("Man", "http://www.dmtf.org/cim/mapping/http/v1.0;ns=" + nameSpaceValue);
            Headers.Add(nameSpaceValue.ToString() + "-CIMProtocolVersion", "1.0");
            
            if (certificatePolicy == null)
                ServicePointManager.CertificatePolicy = new AcceptAllCertificatePolicy();
            else
                ServicePointManager.CertificatePolicy = certificatePolicy;
        }
        #endregion

        #region Properties and Indexers
        public ICredentials Credentials
        {
            get { return httpReq.Credentials; }
            set { httpReq.Credentials = value; }
        }

        public WebHeaderCollection Headers
        {
            get { return httpReq.Headers; }
        }

        public CimName NameSpaceValue
        {
            get { return nameSpaceValue; }
        }

        public long ContentLength
        {
            get { return message.Length; }            
        }

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        #endregion

        #region Internal Classes
        internal class AcceptAllCertificatePolicy : ICertificatePolicy
        {
            public bool CheckValidationResult(ServicePoint sPoint,
                X509Certificate cert, WebRequest wRequest, int certProb)
            {
                // Always accept
                return true;
            }
        }
        #endregion

        #region Methods
        public CimomResponse GetResponse()
        {
            if (OnCimomRequest != null)
            {
                OnCimomRequest(Message);
            }

            // ------------- Convert the message to a byte array, put it into the send stream, and send it ------------- 

            // Tell it how long the message is 
            // (not sure if this is necessary as commenting it out has no effect).
            httpReq.ContentLength = Message.Length;

            // Put the message in the send buffer.
            UTF8Encoding utf8 = new UTF8Encoding();
            byte[] messageInBytes = utf8.GetBytes(Message);

            Stream requestStream = httpReq.GetRequestStream();
            requestStream.Write(messageInBytes, 0, messageInBytes.Length);
            requestStream.Close();

            return new CimomResponse((HttpWebResponse)httpReq.GetResponse());
        }
        #endregion 
    }
}
