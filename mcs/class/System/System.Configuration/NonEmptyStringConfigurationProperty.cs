//
// System.Configuration.NonEmptyStringConfigurationProperty.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0 && XML_DEP
#if XML_DEP
using System;

namespace System.Configuration
{
	public sealed class NonEmptyStringConfigurationProperty : ConfigurationProperty
	{
		NonEmptyStringFlags stringFlags;
		
		public NonEmptyStringConfigurationProperty (string name, string defaultValue, ConfigurationPropertyFlags flags)
			: base (name, typeof(string), defaultValue, flags)
		{
		}

		public NonEmptyStringConfigurationProperty (string name, string defaultValue, ConfigurationPropertyFlags flags, NonEmptyStringFlags nonEmptyStringFlags)
			: base (name, typeof(string), defaultValue, flags)
		{
			stringFlags = nonEmptyStringFlags;
		}

		protected internal override object ConvertFromString (string value)
		{
			return Check (value);
		}

		protected internal override string ConvertToString (object value)
		{
			return Check (value as string);
		}
		
		string Check (string s)
		{
			if (s == string.Empty || s == null)
				throw new ConfigurationException ("The property '" + Name + "' can't be empty");
			
			if (stringFlags == NonEmptyStringFlags.TrimWhitespace)
				return s.Trim ();
			else
				return s;
		}
	}
}
#endif
