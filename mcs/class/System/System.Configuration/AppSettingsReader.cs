//
// System.Configuration.AppSettingsReader
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
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

using System.Reflection;
using System.Collections.Specialized;

#pragma warning disable 618

namespace System.Configuration
{
	public class AppSettingsReader
	{
		NameValueCollection appSettings;

		public AppSettingsReader ()
		{
			appSettings = ConfigurationSettings.AppSettings;
		}

		public object GetValue (string key, Type type)
		{
			if (key == null)
				throw new ArgumentNullException ("key");

			if (type == null)
				throw new ArgumentNullException ("type");

			string value = appSettings [key];
			if (value == null)
				throw new InvalidOperationException ("'" + key + "' could not be found.");

			if (type == typeof (string))
				return value;
			
			MethodInfo parse = type.GetMethod ("Parse", new Type [] {typeof (string)});
			if (parse == null)
				throw new InvalidOperationException ("Type " + type + " does not have a Parse method");

			object result = null;
			try {
				result = parse.Invoke (null, new object [] {value});
			} catch (Exception e) {
				throw new InvalidOperationException ("Parse error.", e);
			}

			return result;
		}
	}
}

