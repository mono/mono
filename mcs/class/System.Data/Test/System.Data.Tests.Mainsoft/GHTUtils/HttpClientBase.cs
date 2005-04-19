// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Oren Gurfinkel   <oreng@mainsoft.com>
//   Ofer Borstein
// 
// Copyright (c) 2004 Mainsoft Co.
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
using System.IO;
using System.Net;
using GHTUtils;
using GHTUtils.Base;


namespace CHttpClient
{
	/// <summary>
	/// Summary description for HttpClientBase.
	/// </summary>
	public class HttpClientBase : GHTBase
	{
		protected string _testingUrl = "";
		protected string _proxyAddress = "";
		public HttpClientBase()
		{
			_testingUrl = "http://localhost/httpapp/";
			_proxyAddress = "localhost";
		}

		protected string TCUrl(string page)
		{
			return _testingUrl + page;
		}

		//===========================================================
		// HTTP request/response routines
		//===========================================================

		protected HttpStatusCode HttpRequestStatusCode(string url)
		{
			return HttpRequestStatusCode(CreateRequest(url));
		}
		protected HttpStatusCode HttpRequestStatusCode(HttpWebRequest _request)
		{
			System.Net.HttpWebResponse _response;
			_response = (HttpWebResponse)_request.GetResponse();
			
			HttpStatusCode c = _response.StatusCode;
			_response.Close();

			return c;
		}

		protected string HttpRequestString(string url)
		{
			return HttpRequestString(CreateRequest(url));
		}
		protected string HttpRequestString(HttpWebRequest _request)
		{
			System.Net.HttpWebResponse _response;
			_response = (HttpWebResponse)_request.GetResponse();

			Stream s = _response.GetResponseStream();
			TextReader r = new StreamReader(s);

			string str = r.ReadToEnd();
			_response.Close();

			return str;
		}

		protected HttpWebRequest CreateRequest(string url)
		{
			return (HttpWebRequest)System.Net.WebRequest.Create(url);
		}

		//===========================================================
		// Upload/Download Data utilities
		//===========================================================

		protected bool ValidateFile(string filename)
		{
			FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
			byte [] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, buffer.Length);
			fs.Close();

			return ValidateData(buffer);
		}

		protected bool ValidateData(byte [] sdata)
		{
			for (int i=0; i < sdata.Length; i++)
			{
				if (sdata[i] != (byte)(i % 256))
					return false;
			}
			return true;
		}

		protected byte [] GenerateData(int size)
		{
			byte [] sdata = new byte[size];
			for (int i=0; i < sdata.Length; i++)
			{
				sdata[i] = (byte)(i % 256);
			}

			return sdata;
		}
		
		//===========================================================
		// Test case execution routines
		//===========================================================

		protected delegate bool TestCaseDelegate();
		protected void ExecuteTestCase(string name, TestCaseDelegate f)
		{
			Exception exp = null;

			//sub test start
			try
			{
				BeginCase(name);
				Compare( f(), true );
			} 
			catch(Exception ex){exp = ex;}
			finally{EndCase(exp); exp = null;}
			//sub test end
		}

	}
}
