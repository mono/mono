// 
// System.Web.Services.Protocols.UrlEncodedParameterWriter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.IO;
using System.Text;
using System.Web.Services;
using System.Web;
using System.Reflection;

namespace System.Web.Services.Protocols {
	public abstract class UrlEncodedParameterWriter : MimeParameterWriter {

		Encoding requestEncoding;
		ParameterInfo[] parameters;
		
		#region Constructors

		protected UrlEncodedParameterWriter () 
		{
		}
		
		#endregion // Constructors

		#region Properties 

		public override Encoding RequestEncoding {
			get { return requestEncoding; }
			set { requestEncoding = value; }
		}

		#endregion // Properties

		#region Methods

		protected void Encode (TextWriter writer, object[] values)
		{
			for (int n=0; n<values.Length; n++)
			{
				if (n>0) writer.Write ("&");
				Encode (writer, parameters[n].Name, values[n]);
			}
		}

		protected void Encode (TextWriter writer, string name, object value)
		{
			if (requestEncoding != null)
			{
				writer.Write (HttpUtility.UrlEncode (name, requestEncoding));
				writer.Write ("=");
				writer.Write (HttpUtility.UrlEncode (ObjToString (value), requestEncoding));
			}
			else
			{
				writer.Write (HttpUtility.UrlEncode (name));
				writer.Write ("=");
				writer.Write (HttpUtility.UrlEncode (ObjToString (value)));
			}
				
		}

		public override object GetInitializer (LogicalMethodInfo methodInfo)
		{
			if (methodInfo.OutParameters.Length > 0) return null;
			else return methodInfo.Parameters;
		}

		public override void Initialize (object initializer)
		{
			parameters = (ParameterInfo[]) initializer;
		}

		#endregion // Methods
	}
}
