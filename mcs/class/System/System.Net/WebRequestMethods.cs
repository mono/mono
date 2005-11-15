//
// System.Net.WebRequestMethods.cs
//
// Author:
// 	Carlos Alberto Cortez (calberto.oortez@gmail.com)
//
// (c) Copyright 2005 Novell, Inc. (http://www.ximian.com)
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

#if NET_2_0

namespace System.Net 
{
	public static class WebRequestMethods
	{
		public static class File
		{
			public const string DownloadFile = "GET";
			public const string UploadFile = "PUT";
		}

		public static class Ftp
		{
			public const string AppendFile = "APPE";
			public const string DeleteFile = "DELE";
			public const string DownloadFile = "RETR";
			public const string GetFileSize = "SIZE";
			public const string GetDateTimestamps = "MDTM";
			public const string ListDirectory = "NLST";
			public const string ListDirectoryDetails = "LIST";
			public const string MakeDirectory = "MKD";
			public const string PrintWorkingDirectory = "PWD";
			public const string RemoveDirectory = "RMD";
			public const string Rename = "RENAME";
			public const string UploadFile = "STOR";
			public const string UploadFileWithUniqueName = "STUR";
		}

		public static class Http
		{
			public const string Connect = "CONNECT";
			public const string Get = "GET";
			public const string Head = "HEAD";
			public const string MkCol = "MKCOL";
			public const string Post = "POST";
			public const string Put = "PUT";
		}
	}
}

#endif

