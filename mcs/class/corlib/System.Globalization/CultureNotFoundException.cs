//
// System.Globalization.CultureNotFoundException.cs
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2010 Novell (http://www.novell.com)
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
#if NET_4_0 || MOONLIGHT

using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System.Globalization {
	[Serializable]
	[ComVisible (true)]
	public class CultureNotFoundException : ArgumentException, ISerializable {
		string invalid_culture_name;
		int? invalid_culture_id;

		public CultureNotFoundException () : base ("Culture not found")
		{
		}

		public CultureNotFoundException (string message) : base (message)
		{
		}

		protected CultureNotFoundException (SerializationInfo info, StreamingContext context)
				: base (info, context)
		{
			invalid_culture_name = (string) info.GetValue ("invalid_culture_name", typeof (string));
			invalid_culture_id = (int?) info.GetValue ("invalid_culture_id", typeof (int?));
		}

		public CultureNotFoundException (string message, Exception innerException)
				: base (message, innerException)
		{
		}

		public CultureNotFoundException (string paramName, string message)
				: base (message, paramName)
		{
		}

		public CultureNotFoundException (string message, int invalidCultureId, Exception innerException)
				: base (message, innerException)
		{
			invalid_culture_id = invalidCultureId;
		}

		public CultureNotFoundException (string paramName, int invalidCultureId, string message)
				: base (message, paramName)
		{
			invalid_culture_id = invalidCultureId;
		}

		public CultureNotFoundException (string message, string invalidCultureName, Exception innerException)
				: base (message, innerException)
		{
			invalid_culture_name = invalidCultureName;
		}

		public CultureNotFoundException (string paramName, string invalidCultureName, string message)
				: base (message, paramName)
		{
			invalid_culture_name = invalidCultureName;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException ("info");

			base.GetObjectData (info, context);
			info.AddValue ("invalid_culture_name", invalid_culture_name, typeof (string));
			info.AddValue ("invalid_culture_id", invalid_culture_id, typeof (int?));
		}

		public virtual int? InvalidCultureId {
			get { return invalid_culture_id; }
		}

		public virtual string InvalidCultureName {
			get { return invalid_culture_name; }
		}

		public override string Message {
			get {
				if (invalid_culture_name == null && invalid_culture_id.HasValue == false)
					return base.Message;

				if (invalid_culture_name != null)
					return String.Format ("Culture name {0} is invalid", invalid_culture_name);
				return String.Format ("Culture ID {0} is invalid", invalid_culture_id);
			}
		}
	}
}
#endif

