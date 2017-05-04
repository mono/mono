//
// System.Configuration.ConfigurationSettings.cs
//
// Author:
//       Martin Baulig <mabaul@microsoft.com>
//
// Copyright (c) 2017 Xamarin, Inc.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
#if (XML_DEP)
using System.Xml;
using System.Xml.XPath;
#endif

#pragma warning disable 618

namespace System.Configuration {
	public sealed class ConfigurationSettings {
		const string EXCEPTION_MESSAGE = "System.Configuration.ConfigurationSettings is not supported on the current platform.";

		private ConfigurationSettings ()
		{
		}

		[Obsolete ("This method is obsolete, it has been replaced by System.Configuration!System.Configuration.ConfigurationManager.GetSection")]
		public static object GetConfig (string sectionName)
		{
			throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
		}

		[Obsolete ("This property is obsolete.  Please use System.Configuration.ConfigurationManager.AppSettings")]
		public static NameValueCollection AppSettings
		{
			get {
				throw new PlatformNotSupportedException (EXCEPTION_MESSAGE);
			}
		}
	}
}


