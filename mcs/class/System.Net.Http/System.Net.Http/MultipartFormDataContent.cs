//
// MultipartFormDataContent.cs
//
// Authors:
//	Marek Safar  <marek.safar@gmail.com>
//
// Copyright (C) 2012 Xamarin Inc (http://www.xamarin.com)
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

using System.Net.Http.Headers;

namespace System.Net.Http
{
	public class MultipartFormDataContent : MultipartContent
	{
		public MultipartFormDataContent ()
			: base ("form-data")
		{
		}

		public MultipartFormDataContent (string boundary)
			: base ("form-data", boundary)
		{
		}
		
		public override void Add (HttpContent content)
		{
			base.Add (content);
			AddContentDisposition (content, null, null);
		}

		public void Add (HttpContent content, string name)
		{
			base.Add (content);
			
			if (string.IsNullOrWhiteSpace (name))
				throw new ArgumentException ("name");
			
			AddContentDisposition (content, name, null);
		}
		
		public void Add (HttpContent content, string name, string fileName)
		{
			base.Add (content);
			
			if (string.IsNullOrWhiteSpace (name))
				throw new ArgumentException ("name");
			
			if (string.IsNullOrWhiteSpace (fileName))
				throw new ArgumentException ("fileName");
			
			AddContentDisposition (content, name, fileName);
		}
		
		void AddContentDisposition (HttpContent content, string name, string fileName)
		{
			var headers = content.Headers;
			if (headers.ContentDisposition != null)
				return;
			
			headers.ContentDisposition = new ContentDispositionHeaderValue ("form-data") {
				Name = name,
				FileName = fileName,
				FileNameStar = fileName
			};
		}
	}
}
