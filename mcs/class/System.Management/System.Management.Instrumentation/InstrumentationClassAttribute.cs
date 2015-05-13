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
using System.Runtime;

namespace System.Management.Instrumentation
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class InstrumentationClassAttribute : Attribute
	{
		private InstrumentationType instrumentationType;

		private string managedBaseClassName;

		public InstrumentationType InstrumentationType
		{
			[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
			get
			{
				return this.instrumentationType;
			}
		}

		public string ManagedBaseClassName
		{
			get
			{
				if (this.managedBaseClassName == null || this.managedBaseClassName.Length == 0)
				{
					return null;
				}
				else
				{
					return this.managedBaseClassName;
				}
			}
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public InstrumentationClassAttribute(InstrumentationType instrumentationType)
		{
			this.instrumentationType = instrumentationType;
		}

		[TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
		public InstrumentationClassAttribute(InstrumentationType instrumentationType, string managedBaseClassName)
		{
			this.instrumentationType = instrumentationType;
			this.managedBaseClassName = managedBaseClassName;
		}

		internal static InstrumentationClassAttribute GetAttribute(Type type)
		{
			if (type == typeof(BaseEvent) || type == typeof(Instance))
			{
				return null;
			}
			else
			{
				object[] customAttributes = type.GetCustomAttributes(typeof(InstrumentationClassAttribute), true);
				if ((int)customAttributes.Length <= 0)
				{
					return null;
				}
				else
				{
					return (InstrumentationClassAttribute)customAttributes[0];
				}
			}
		}

		internal static Type GetBaseInstrumentationType(Type type)
		{
			if (InstrumentationClassAttribute.GetAttribute(type.BaseType) == null)
			{
				return null;
			}
			else
			{
				return type.BaseType;
			}
		}
	}
}