//
// System.Web.HttpResponseTest.cs - Unit tests for System.Web.HttpResponse
//
// Author:
//      Miguel de Icaza  <miguel@ximian.com>
//
// Copyright (C) 2005-2009 Novell, Inc (http://www.novell.com)
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

using System.Text;
using System.Web;
using System;
using System.Collections;
using System.Collections.Specialized;
using NUnit.Framework;

namespace MonoTests.Common
{

    public class FakeHttpWorkerRequest : HttpWorkerRequest
    {
	const string appPath = "/";
        string queryString;
        public Hashtable KnownResponseHeaders;
        public Hashtable UnknownResponseHeaders;
        public int return_kind;

        public FakeHttpWorkerRequest ()
            : this (1)
        {
    		// for Mono
    		AppDomain.CurrentDomain.SetData (".appVPath", appPath);
        }

        public FakeHttpWorkerRequest (int re)
        {
            KnownResponseHeaders = CollectionsUtil.CreateCaseInsensitiveHashtable ();
            UnknownResponseHeaders = CollectionsUtil.CreateCaseInsensitiveHashtable ();
            return_kind = re;
            queryString = String.Empty;
        }

        public override string GetUriPath ()
        {
            return "/fake";
        }

	public override string GetFilePath ()
	{
	    return GetUriPath ();
	}

        public override string GetQueryString ()
        {
            return queryString;
        }

        public void SetQueryString (string queryString)
        {
            this.queryString = queryString;
        }

        public override string GetRawUrl ()
        {
#if NET_4_0
	    return "/GetRawUrl";
#else
            return "GetRawUrl";
#endif
        }

        public override string GetHttpVerbName ()
        {
            return "GET";
        }

        public override string GetHttpVersion ()
        {
            if (return_kind == 1)
                return "HTTP/1.0";
            else
                return "HTTP/1.1";
        }

        public override string GetRemoteAddress ()
        {
            return "__GetRemoteAddress";
        }

        public override int GetRemotePort ()
        {
            return 1010;
        }

        public override string GetLocalAddress ()
        {
            return "GetLocalAddress";
        }

        public override string GetAppPath ()
        {
            return appPath;
        }

        public override int GetLocalPort ()
        {
            return 2020;
        }

	public override string MapPath (string virtualPath)
	{
#if TARGET_DOTNET
		return "c:\\fakefile";
#else
		return "/fakefile";
#endif
	}

        public bool status_sent;
        public int status_code;
        public string status_string;

        public override void SendStatus (int s, string x)
        {
            status_sent = true;
            status_code = s;
            status_string = x;
        }

        void AddHeader (Hashtable table, string header_name, object header)
        {
            object o = table[header_name];
            if (o == null)
                table.Add (header_name, header);
            else {
                ArrayList al = o as ArrayList;
                if (al == null) {
                    al = new ArrayList ();
                    al.Add (o);
                    table[header_name] = al;
                } else
                    al = o as ArrayList;

                al.Add (header);
            }
        }

        bool headers_sent;
        public override void SendKnownResponseHeader (int x, string j)
        {
            string header_name = HttpWorkerRequest.GetKnownRequestHeaderName (x);
            AddHeader (KnownResponseHeaders, header_name, new KnownResponseHeader (x, j));
            headers_sent = true;
        }

        public override void SendUnknownResponseHeader (string a, string b)
        {
            AddHeader (UnknownResponseHeaders, a, new UnknownResponseHeader (a, b));
            headers_sent = true;
        }

        bool data_sent;
        public byte[] data;
        public int data_len;
        public int total = 0;

        public override void SendResponseFromMemory (byte[] arr, int x)
        {
            data_sent = true;
            data = new byte[x];
            for (int i = 0; i < x; i++)
                data[i] = arr[i];
            data_len = x;
            total += data_len;
        }

        public override void SendResponseFromFile (string a, long b, long c)
        {
            data_sent = true;
        }

        public override void SendResponseFromFile (IntPtr a, long b, long c)
        {
            data_sent = true;
        }

        public override void FlushResponse (bool x)
        {
        }

        public override void EndOfRequest ()
        {
        }

        public override string GetKnownRequestHeader (int index)
        {
            return null;
        }

        public bool OutputProduced
        {
            get
            {
                return headers_sent || data_sent;
            }
        }
    }
}
