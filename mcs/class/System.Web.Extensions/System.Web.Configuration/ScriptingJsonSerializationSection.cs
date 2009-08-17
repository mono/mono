//
// ScriptingJsonSerializationSection.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace System.Web.Configuration
{
	public sealed class ScriptingJsonSerializationSection : ConfigurationSection
	{
		[ConfigurationPropertyAttribute ("converters", IsKey = true)]
		public ConvertersCollection Converters {
			get {
				return (ConvertersCollection) base ["converters"];
			}
		}

#if NET_3_5
		[ConfigurationPropertyAttribute ("maxJsonLength", DefaultValue = 2097152)]
#else
		[ConfigurationPropertyAttribute ("maxJsonLength", DefaultValue = 102400)]
#endif
		public int MaxJsonLength {
			get {
				return (int) this ["maxJsonLength"];
			}
			set {
				this ["maxJsonLength"] = value;
			}
		}

		[ConfigurationPropertyAttribute ("recursionLimit", DefaultValue = 100)]
		public int RecursionLimit {
			get {
				return (int) this ["recursionLimit"];
			}
			set {
				this ["recursionLimit"] = value;
			}
		}
	}
}
