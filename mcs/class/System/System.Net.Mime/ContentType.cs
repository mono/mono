//
// System.Net.Mail.ContentType.cs
//
// Author:
//	Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2004
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

using System.Collections.Specialized;

namespace System.Net.Mime {
	public class ContentType
	{
		#region Fields

		string contentType;
		string boundary;
		string charset;
		string mediaType;
		string name;

		#endregion // Fields

		#region Constructors

		public ContentType ()
		{
		}
	
		[MonoTODO ("Parse content type")]
		public ContentType (string contentType)
		{
			this.contentType = contentType;
		}

		#endregion // Constructors

		#region Properties

		public string Boundary {
			get { return boundary; }
			set { 
				contentType = null;
				boundary = value; 
			}
		}

		public string CharSet {
			get { return charset; }
			set { 
				contentType = null;
				charset = value; 
			}
		}

		public string MediaType {
			get { return mediaType; }
			set {
				contentType = null;
				mediaType = value; 
			}
		}

		public string Name {
			get { return name; }
			set {
				contentType = null;
				name = value;
			}
		}

		[MonoTODO]
		public StringDictionary Parameters {
			get { 	
				throw new NotImplementedException ();
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO ("Fix this")]
		public override string ToString ()
		{
			if (contentType != null)
				return contentType;
			return String.Format ("{0}; Charset={1}; Name={2}", MediaType, CharSet, Name);
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
