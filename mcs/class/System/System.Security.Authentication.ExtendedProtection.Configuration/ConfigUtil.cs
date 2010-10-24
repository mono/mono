//
// ConfigUtil.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006,2010 Novell, Inc.  http://www.novell.com
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
#if NET_4_0 && CONFIGURATION_DEP

extern alias PrebuiltSystem;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

using TypeConverter = PrebuiltSystem::System.ComponentModel.TypeConverter;

namespace System.Security.Authentication.ExtendedProtection.Configuration
{
	internal static class ConfigUtil
	{
		// ugh, we cannot use extension methods yet.
		internal static T GetCustomAttribute<T> (MemberInfo m, bool inherit)
		{
			var atts = m.GetCustomAttributes (typeof (T), false);
			return atts.Length > 0 ? (T) atts [0] : default (T);
		}

		internal static ConfigurationProperty BuildProperty (Type t, string name)
		{
			var mi = t.GetProperty (name);

			var a = GetCustomAttribute<ConfigurationPropertyAttribute> (mi, false);
			var tca = GetCustomAttribute<TypeConverterAttribute> (mi, false);
			var va = GetCustomAttribute<ConfigurationValidatorAttribute> (mi, false);

			return new ConfigurationProperty (a.Name, mi.PropertyType, a.DefaultValue, tca != null ? (TypeConverter) Activator.CreateInstance (Type.GetType (tca.ConverterTypeName)) : null, va != null ? va.ValidatorInstance : null, a.Options);
		}
	}
}

#endif
