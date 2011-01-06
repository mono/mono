/*
  Copyright (C) 2010 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Globalization;

namespace IKVM.Reflection
{
	public abstract class Binder
	{
		protected Binder()
		{
			throw new NotSupportedException();
		}

		public abstract MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state);
		public abstract FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture);
		public abstract object ChangeType(object value, Type type, CultureInfo culture);
		public abstract void ReorderArgumentArray(ref object[] args, object state);
		public abstract MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers);
		public abstract PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers);
	}
}
