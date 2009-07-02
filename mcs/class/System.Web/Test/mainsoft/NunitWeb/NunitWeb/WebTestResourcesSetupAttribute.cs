//
// WebTestResourcesSetupAttribute.cs
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2009 Novell, Inc (http://novell.com/)
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

namespace MonoTests.SystemWeb.Framework
{
	[AttributeUsage (AttributeTargets.Assembly, AllowMultiple=false)]
	public class WebTestResourcesSetupAttribute : Attribute
	{
		public delegate void SetupHandler ();
		
		Type type;
		string methodName;
		
		public Type Type {
			get { return type; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				
				type = value;
			}
		}

		public string MethodName {
			get { return methodName; }
			set {
				if (value == null || value.Length == 0)
					throw new ArgumentNullException ("value");
				methodName = value;
			}
		}

		public SetupHandler Handler {
			get { return FindHandler (Type, MethodName); }
		}
		
		public WebTestResourcesSetupAttribute (Type type, string methodName)
		{
			Type = type;
			MethodName = methodName;
		}

		SetupHandler FindHandler (Type type, string methodName)
		{
			SetupHandler ret;
			MethodInfo mi = type.GetMethod (methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, null, Type.EmptyTypes, null);
			
			if (mi == null)
				throw new InvalidOperationException ("Method '" + methodName + "' cannot be found in type '" + type + "'.");

			if (mi.IsAbstract ||
#if NET_2_0
			    mi.IsGenericMethodDefinition ||
#endif
			    mi.ReturnType != typeof (void) || mi.GetParameters ().Length > 0)
				throw new InvalidOperationException ("Method '" + methodName + "' must return void and take no parameters.");

#if NET_2_0
			ret = Delegate.CreateDelegate (typeof (SetupHandler), null, mi, false) as SetupHandler;
#else
			ret = Delegate.CreateDelegate (typeof (SetupHandler), mi) as SetupHandler;
#endif
			if (ret == null)
				throw new InvalidOperationException ("Failed to create a delegate to method '" + methodName + "'.");

			return ret;
		}
	}
}
