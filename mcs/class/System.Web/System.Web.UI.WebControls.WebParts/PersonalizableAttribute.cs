//
// System.Web.Compilation.PersonalizableAttribute
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
#if NET_2_0
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace System.Web.UI.WebControls.WebParts
{
	[AttributeUsageAttribute(AttributeTargets.Property)] 
	public sealed class PersonalizableAttribute : Attribute
	{
		public static readonly PersonalizableAttribute Default;
		public static readonly PersonalizableAttribute NotPersonalizable;
		public static readonly PersonalizableAttribute Personalizable;
		public static readonly PersonalizableAttribute SharedPersonalizable;
		public static readonly PersonalizableAttribute UserPersonalizable;

		bool isPersonalizable;
		bool isSensitive;
		PersonalizationScope scope;
		
		static PersonalizableAttribute ()
		{
			Default = new PersonalizableAttribute (false);
			NotPersonalizable = Default;
			Personalizable = new PersonalizableAttribute (PersonalizationScope.User, false);
			SharedPersonalizable = new PersonalizableAttribute (PersonalizationScope.Shared, false);
			UserPersonalizable = new PersonalizableAttribute (PersonalizationScope.User, false);
		}
		
		public PersonalizableAttribute () : this (true)
		{
		}

		public PersonalizableAttribute (bool isPersonalizable)
		{
			this.isPersonalizable = isPersonalizable;
			this.scope = PersonalizationScope.User;
			this.isSensitive = false;
		}

		public PersonalizableAttribute (PersonalizationScope scope) : this (scope, false)
		{
		}

		public PersonalizableAttribute (PersonalizationScope scope, bool isSensitive)
		{
			this.isPersonalizable = true;
			this.scope = scope;
			this.isSensitive = isSensitive;
		}

		public bool IsPersonalizable {
			get { return isPersonalizable; }
		}

		public bool IsSensitive {
			get { return isSensitive; }
		}

		public PersonalizationScope Scope {
			get { return scope; }
		}

		public override bool Equals (object obj)
		{
			PersonalizableAttribute attr = obj as PersonalizableAttribute;
			if (attr == null)
				return false;

			return (this.isPersonalizable == attr.IsPersonalizable &&
				this.isSensitive == attr.IsSensitive &&
				this.scope == attr.Scope);
		}
		
		public override int GetHashCode ()
		{
			return (this.isPersonalizable.GetHashCode () ^
				this.isSensitive.GetHashCode () ^
				this.scope.GetHashCode ());
		}

		public static ICollection GetPersonalizableProperties (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			
			PropertyInfo[] properties = type.GetProperties ();
			if (properties == null || properties.Length == 0)
				return new PropertyInfo [0];
			List <PropertyInfo> ret = null;
			
			foreach (PropertyInfo pi in properties)
				if (PropertyQualifies (pi)) {
					if (ret == null)
						ret = new List <PropertyInfo> ();
					ret.Add (pi);
				}
			return ret;
		}

		static bool PropertyQualifies (PropertyInfo pi)
		{
			object[] attributes = pi.GetCustomAttributes (false);
			if (attributes == null || attributes.Length == 0)
				return false;

			PersonalizableAttribute attr;
			MethodInfo mi;
			foreach (object a in attributes) {
				attr = a as PersonalizableAttribute;
				if (attr == null || !attr.IsPersonalizable)
					continue;
				mi = pi.GetSetMethod (false);
				if (mi == null)
					throw new HttpException ("A public property on the type is marked as personalizable but is read-only.");
				return true;
			}

			return false;
		}

		public override bool IsDefaultAttribute ()
		{
			return PersonalizableAttribute.Equals (this, Default);
		}

		public override bool Match (object obj)
		{
			PersonalizableAttribute attr = obj as PersonalizableAttribute;
			if (obj == null)
				return false;
			return (this.isPersonalizable == attr.IsPersonalizable);
		}
	}
}
#endif
