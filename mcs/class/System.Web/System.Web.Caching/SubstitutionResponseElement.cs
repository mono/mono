//
// Authors:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2010 Novell, Inc (http://novell.com/)
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
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Web;

namespace System.Web.Caching
{
	[Serializable]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Unrestricted)]
	public class SubstitutionResponseElement : ResponseElement
	{
		string typeName;
		string methodName;
		
		public HttpResponseSubstitutionCallback Callback {
			get;
			private set;
		}
		
		public SubstitutionResponseElement (HttpResponseSubstitutionCallback callback)
		{
			if (callback == null)
				throw new ArgumentNullException ("callback");

			this.Callback = callback;

			MethodInfo mi = callback.Method;
			this.typeName = mi.DeclaringType.AssemblyQualifiedName;
			this.methodName = mi.Name;
		}

		[OnDeserialized]
		void ObjectDeserialized (StreamingContext context)
		{
			Type type = Type.GetType (typeName, true);
			Callback = Delegate.CreateDelegate (typeof (HttpResponseSubstitutionCallback), type, methodName, false, true) as HttpResponseSubstitutionCallback;
		}
	}
}
