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
using System.Reflection;
using System.Runtime;

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
	public class ManagedNameAttribute : Attribute
	{
		private string name;

		public string Name
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.name;
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public ManagedNameAttribute(string name)
		{
			this.name = name;
		}

		internal static string GetBaseClassName(Type type)
		{
			InstrumentationClassAttribute attribute = InstrumentationClassAttribute.GetAttribute(type);
			string managedBaseClassName = attribute.ManagedBaseClassName;
			if (managedBaseClassName == null)
			{
				InstrumentationClassAttribute instrumentationClassAttribute = InstrumentationClassAttribute.GetAttribute(type.BaseType);
				if (instrumentationClassAttribute == null)
				{
					InstrumentationType instrumentationType = attribute.InstrumentationType;
					switch (instrumentationType)
					{
						case InstrumentationType.Instance:
						{
							return null;
						}
						case InstrumentationType.Event:
						{
							return "__ExtrinsicEvent";
						}
						case InstrumentationType.Abstract:
						{
							return null;
						}
					}
				}
				return ManagedNameAttribute.GetMemberName(type.BaseType);
			}
			else
			{
				return managedBaseClassName;
			}
		}

		internal static string GetMemberName(MemberInfo member)
		{
			object[] customAttributes = member.GetCustomAttributes(typeof(ManagedNameAttribute), false);
			if ((int)customAttributes.Length > 0)
			{
				ManagedNameAttribute managedNameAttribute = (ManagedNameAttribute)customAttributes[0];
				if (managedNameAttribute.name != null && managedNameAttribute.name.Length != 0)
				{
					return managedNameAttribute.name;
				}
			}
			return member.Name;
		}
	}
}