//
// Microsoft.Web.Services.Converters.DateTimeConverter
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System;
using System.Web;
using Microsoft.Web.Services;

namespace Microsoft.Web.Services.Converters
{
	public class DateTimeConverter : JavaScriptConverter
	{
		public DateTimeConverter ()
		{
		}

		public override object Deserialize (string s, Type t)
		{
			return JavaScriptObjectDeserializer.Deserialize (s, t);
		}

		protected override string GetClientTypeName (Type serverType)
		{
			return "Date";
		}

		protected override void Initialize ()
		{
		}

		public override string Serialize (object o)
		{
			if (o == null || o.GetType () != typeof (DateTime))
				throw new ArgumentException ("Value does not fall within the expected range.");

			DateTime dt = (DateTime)o;

			return String.Format ("new Date({0},{1},{2},{3},{4},{5})",
					      dt.Year, dt.Month - 1, dt.Day, dt.Hour, dt.Minute, dt.Second);
		}

		Type[] supportedTypes;
		protected override Type[] SupportedTypes {
			get {
				if (supportedTypes == null) {
					supportedTypes = new Type[1];
					supportedTypes[0] = typeof (DateTime);
				}

				return supportedTypes;
			}
		}
	}
}

#endif
