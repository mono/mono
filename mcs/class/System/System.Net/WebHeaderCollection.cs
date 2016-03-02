//
// System.Net.WebHeaderCollection
//
// Authors:
// 	Lawrence Pit (loz@cable.a2000.nl)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//      Miguel de Icaza (miguel@novell.com)
//	Marek Safar (marek.safar@gmail.com)
//
// Copyright 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright 2007 Novell, Inc. (http://www.novell.com)
// Copyright (C) 2011 Xamarin Inc (http://www.xamarin.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
    
// See RFC 2068 par 4.2 Message Headers
    
namespace System.Net 
{
	public partial class WebHeaderCollection : NameValueCollection, ISerializable
	{
		internal string [] GetValues_internal (string header)
		{
			if (header == null)
				throw new ArgumentNullException ("header");

			string [] values = InnerCollection.GetValues (header);
			if (values == null || values.Length == 0)
				return null;

			return values;
		}

		internal string ToStringMultiValue ()
		{
			StringBuilder sb = new StringBuilder();

			int count = InnerCollection.Count;
			for (int i = 0; i < count ; i++) {
				string key = GetKey (i);
				string[] values = GetValues (i);
				if (values != null) {
					foreach (string v in values) {
						sb.Append (key)
						  .Append (": ")
						  .Append (v)
						  .Append ("\r\n");
					}
				} else {
					sb.Append (key)
					  .Append (": ")
					  .Append (Get (i))
					  .Append ("\r\n");
				}
			}
			return sb.Append("\r\n").ToString();
		}

		// With this we don't check for invalid characters in header. See bug #55994.
		internal void SetInternal (string header)
		{
			int pos = header.IndexOf (':');
			if (pos == -1)
				throw new ArgumentException ("no colon found", "header");				

			SetAddVerified (header.Substring (0, pos).Trim (), header.Substring (pos + 1).Trim ());
		}

		internal void RemoveAndAdd (string name, string value)
		{
			if (value == null)
				value = String.Empty;
			else
				value = value.Trim ();

			InnerCollection.Remove (name);
			InnerCollection.Set (name, value);
		}

		internal static bool IsHeaderValue (string value)
		{
			// TEXT any 8 bit value except CTL's (0-31 and 127)
			//      but including \r\n space and \t
			//      after a newline at least one space or \t must follow
			//      certain header fields allow comments ()
				
			int len = value.Length;
			for (int i = 0; i < len; i++) {			
				char c = value [i];
				if (c == 127)
					return false;
				if (c < 0x20 && (c != '\r' && c != '\n' && c != '\t'))
					return false;
				if (c == '\n' && ++i < len) {
					c = value [i];
					if (c != ' ' && c != '\t')
						return false;
				}
			}
			
			return true;
		}
	}
}
