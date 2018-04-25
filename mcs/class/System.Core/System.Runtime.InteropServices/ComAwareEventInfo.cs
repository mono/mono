//
// ComAwareEventInfo.cs
//
// Authors:
//	Alexander Köplinger <alexander.koeplinger@xamarin.com>
//
// Copyright (C) 2016 Xamarin Inc (http://www.xamarin.com)
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

namespace System.Runtime.InteropServices
{
	public class ComAwareEventInfo : EventInfo
	{
		[MonoTODO]
		public override EventAttributes Attributes
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override Type DeclaringType
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public override string Name
		{
			get
			{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public ComAwareEventInfo (Type type, string eventName)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void AddEventHandler (object target, Delegate handler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override void RemoveEventHandler (object target, Delegate handler)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override MethodInfo GetAddMethod (bool nonPublic)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override MethodInfo GetRaiseMethod (bool nonPublic)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override MethodInfo GetRemoveMethod (bool nonPublic)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object[] GetCustomAttributes (bool inherit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Type ReflectedType
		{
			get { throw new NotImplementedException (); }
		}
	}
}
