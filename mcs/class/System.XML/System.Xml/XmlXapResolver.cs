//
// XmlXapResolver.cs
//
// Author:
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2009 Novell Inc.
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
using System.Reflection;

#if !MOBILE
namespace System.Xml
{
	public class XmlXapResolver : XmlResolver
	{
		static readonly MethodInfo method;
		static readonly PropertyInfo property;
		static XmlXapResolver ()
		{
			var at = Type.GetType ("System.Windows.Application, System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", true);
			method = at.GetMethod ("GetResourceStream", new Type [] {typeof (Uri)});
			var rt = Type.GetType ("System.Windows.Resources.StreamResourceInfo, System.Windows, Version=2.0.5.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e", true);
			property = rt.GetProperty ("Stream");
		}

		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			if (absoluteUri == null)
				throw new ArgumentNullException ("absoluteUri");
			if (absoluteUri.IsAbsoluteUri)
				throw new XmlException ("URI must be relative to the application XAP");
			if (!SupportsType (absoluteUri, ofObjectToReturn))
				throw new XmlException (String.Format ("Unsupported entity type '{0}'", ofObjectToReturn.GetType ()));
			
			try {
				var sri = method.Invoke (null, new object [] {absoluteUri});
				if (sri == null)
					throw new XmlException (String.Format ("Resource '{0}' not found", absoluteUri));
				return property.GetValue (sri, new object [0]);
			} catch (TargetInvocationException tie) {
				// throw the original exception
				throw tie.InnerException;
			}
		}
	}
}
#endif
