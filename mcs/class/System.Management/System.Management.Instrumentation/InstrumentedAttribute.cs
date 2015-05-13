//
// AssemblyRef
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System.Collections;
using System.Management;
using System.Reflection;
using System.Runtime;
using System.Text.RegularExpressions;

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Assembly)]
	public class InstrumentedAttribute : Attribute
	{
		private string namespaceName;

		private string securityDescriptor;

		public string NamespaceName
		{
			get
			{
				if (this.namespaceName == null)
				{
					return string.Empty;
				}
				else
				{
					return this.namespaceName;
				}
			}
		}

		public string SecurityDescriptor
		{
			get
			{
				if (this.securityDescriptor == null || this.securityDescriptor.Length == 0)
				{
					return null;
				}
				else
				{
					return this.securityDescriptor;
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public InstrumentedAttribute() : this(null, null)
		{
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public InstrumentedAttribute(string namespaceName) : this(namespaceName, null)
		{
		}

		public InstrumentedAttribute(string namespaceName, string securityDescriptor)
		{
			if (namespaceName != null)
			{
				namespaceName = namespaceName.Replace('/', '\\');
			}
			if (namespaceName == null || namespaceName.Length == 0)
			{
				namespaceName = "root\\default";
			}
			bool flag = true;
			char[] chrArray = new char[1];
			chrArray[0] = '\\';
			string[] strArrays = namespaceName.Split(chrArray);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				if (str.Length == 0 || flag && string.Compare(str, "root", StringComparison.OrdinalIgnoreCase) != 0 || !Regex.Match(str, "^[a-z,A-Z]").Success || Regex.Match(str, "_$").Success || Regex.Match(str, "[^a-z,A-Z,0-9,_,\\u0080-\\uFFFF]").Success)
				{
					ManagementException.ThrowWithExtendedInfo(ManagementStatus.InvalidNamespace);
				}
				flag = false;
			}
			this.namespaceName = namespaceName;
			this.securityDescriptor = securityDescriptor;
		}

		internal static InstrumentedAttribute GetAttribute(Assembly assembly)
		{
			object[] customAttributes = assembly.GetCustomAttributes(typeof(InstrumentedAttribute), false);
			if ((int)customAttributes.Length <= 0)
			{
				return new InstrumentedAttribute();
			}
			else
			{
				return (InstrumentedAttribute)customAttributes[0];
			}
		}

		private static void GetInstrumentedParentTypes(ArrayList types, Type childType)
		{
			if (!types.Contains(childType))
			{
				Type baseInstrumentationType = InstrumentationClassAttribute.GetBaseInstrumentationType(childType);
				if (baseInstrumentationType != null)
				{
					InstrumentedAttribute.GetInstrumentedParentTypes(types, baseInstrumentationType);
				}
				types.Add(childType);
			}
		}

		internal static Type[] GetInstrumentedTypes(Assembly assembly)
		{
			ArrayList arrayLists = new ArrayList();
			Type[] types = assembly.GetTypes();
			for (int i = 0; i < (int)types.Length; i++)
			{
				Type type = types[i];
				if (InstrumentedAttribute.IsInstrumentationClass(type))
				{
					InstrumentedAttribute.GetInstrumentedParentTypes(arrayLists, type);
				}
			}
			return (Type[])arrayLists.ToArray(typeof(Type));
		}

		private static bool IsInstrumentationClass(Type type)
		{
			return null != InstrumentationClassAttribute.GetAttribute(type);
		}
	}
}