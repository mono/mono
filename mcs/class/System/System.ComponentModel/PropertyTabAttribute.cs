//
// System.ComponentModel.PropertyTabAttribute
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//  Ivan N. Zlatev (contact@i-nz.net)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// (C) 2008 Novell, Inc
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

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public class PropertyTabAttribute : Attribute
	{

		private Type[] tabs;
		private PropertyTabScope[] scopes;


		public PropertyTabAttribute()
		{
			tabs = Type.EmptyTypes;
			scopes = new PropertyTabScope[0];
		}

		public PropertyTabAttribute (string tabClassName) : this (tabClassName, PropertyTabScope.Component)
		{
		}

		public PropertyTabAttribute (Type tabClass) : this (tabClass, PropertyTabScope.Component)
		{
		}

		public PropertyTabAttribute (string tabClassName, PropertyTabScope tabScope)
		{
			if (tabClassName == null)
				throw new ArgumentNullException ("tabClassName");
			this.InitializeArrays (new string[] { tabClassName }, new PropertyTabScope[] { tabScope });
		}

		public PropertyTabAttribute (Type tabClass, PropertyTabScope tabScope)
		{
			if (tabClass == null)
				throw new ArgumentNullException ("tabClass");
			this.InitializeArrays (new Type[] { tabClass }, new PropertyTabScope[] { tabScope });
		}

		public Type[] TabClasses {
			get { return tabs; }
		}

		public PropertyTabScope[] TabScopes {
			get { return scopes; }
		}

		protected string[] TabClassNames {
			get {
				string[] names = new string[tabs.Length];
				for (int i=0; i < tabs.Length; i++)
					names[i] = tabs[i].Name;
				return names;
			}
		}

		public override bool Equals (object other)
		{
			if (other is PropertyTabAttribute)
				return Equals ((PropertyTabAttribute)other);
			return false;
		}

		public bool Equals (PropertyTabAttribute other)
		{
			if (other != this) {
				if (other.TabClasses.Length != tabs.Length)
					return false;

				for (int i=0; i < tabs.Length; i++) {
					if (tabs[i] != other.TabClasses[i])
						return false;
				}
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}

		protected void InitializeArrays (string[] tabClassNames, PropertyTabScope[] tabScopes)
		{
			if (tabScopes == null)
				throw new ArgumentNullException ("tabScopes");
			if (tabClassNames == null)
				throw new ArgumentNullException ("tabClassNames");

			scopes = tabScopes;
			tabs = new Type[tabClassNames.Length];
			for (int i = 0; i < tabClassNames.Length; i++)
				tabs[i] = GetTypeFromName (tabClassNames[i]);
		}

		protected void InitializeArrays (Type[] tabClasses, PropertyTabScope[] tabScopes)
		{
			if (tabScopes == null)
				throw new ArgumentNullException ("tabScopes");
			if (tabClasses == null)
				throw new ArgumentNullException ("tabClasses");
			if (tabClasses.Length != tabScopes.Length)
				throw new ArgumentException ("tabClasses.Length != tabScopes.Length");

			tabs = tabClasses;
			scopes = tabScopes;
		}

		private Type GetTypeFromName (string typeName)
		{
			if (typeName == null)
				throw new ArgumentNullException ("typeName");

			int index = typeName.IndexOf (",");
			if (index != -1) {
				string typeNameOnly = typeName.Substring (0, index);
				string assemblyName = typeName.Substring (index + 1);
				Assembly assembly = Assembly.Load (assemblyName);
				if (assembly != null)
					return assembly.GetType (typeNameOnly, true);
			}
			return Type.GetType (typeName, true);
		}
	}
}
