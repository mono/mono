//
// System.ComponentModel.PropertyTabAttribute
//
// Authors:
//  Martin Willemoes Hansen (mwh@sysrq.dk)
//  Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
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

namespace System.ComponentModel
{
	[AttributeUsage(AttributeTargets.All)]
	public class PropertyTabAttribute : Attribute
	{

		private Type[] tabs;
		private PropertyTabScope[] scopes;


		public PropertyTabAttribute()
		{
			tabs = null;
			scopes = null;
		}

		public PropertyTabAttribute (string tabClassName)
		{
			string[] tabArray = {tabClassName};
			this.InitializeArrays (tabArray, null);
		}

		public PropertyTabAttribute (Type tabClass)
		{
			Type[] tabArray = {tabClass};
			this.InitializeArrays (tabArray, null);
		}

		public PropertyTabAttribute (string tabClassName, PropertyTabScope tabScope)
		{
			string[] tabArray = {tabClassName};
			PropertyTabScope[] scopeArray = {tabScope};
			this.InitializeArrays (tabArray, scopeArray);
		}

		public PropertyTabAttribute (Type tabClass, PropertyTabScope tabScope)
		{
			Type[] tabArray = {tabClass};
			PropertyTabScope[] scopeArray = {tabScope};
			this.InitializeArrays (tabArray, scopeArray);
		}

		public Type[] TabClasses {
			get { return tabs; }
		}

		public PropertyTabScope[] TabScopes {
			get { return scopes; }
		}

		public override bool Equals (object other)
		{
			if (!(other is PropertyTabAttribute))
				return false;
			if (other == this)
				return true;
			return ((PropertyTabAttribute) other).TabClasses == tabs &&
				((PropertyTabAttribute) other).TabScopes == scopes;
		}

		public bool Equals (PropertyTabAttribute other)
		{
			return this.Equals ((object) other);
		}

		public override int GetHashCode()
		{
			// FIXME check if other Hashcode is needed
			return base.GetHashCode ();
		}

		protected string[] TabClassNames {
			get {
				// FIXME untested, maybe wrong
				string[] tabClassName = (string[]) (Array.CreateInstance (typeof (string), tabs.Length));
				for (int x = 0; x < tabs.Length; x++)
					tabClassName[x] = tabs[x].AssemblyQualifiedName;
				return tabClassName;
			}
		}

		protected void InitializeArrays (string[] tabClassNames, PropertyTabScope[] tabScopes)
		{
			// FIXME untested, maybe wrong
			Type[] tabClasses = (Type[]) (Array.CreateInstance (typeof (Type), tabClassNames.Length));
			for (int x = 0; x < tabClassNames.Length; x++)
				tabClasses[x] = Type.GetType (tabClassNames[x], false);

			tabs = tabClasses;
			scopes = tabScopes;
		}

		protected void InitializeArrays (Type[] tabClasses, PropertyTabScope[] tabScopes)
		{
			// FIXME untested, maybe wrong
			tabs = tabClasses;
			scopes = tabScopes;
		}
	}
}
