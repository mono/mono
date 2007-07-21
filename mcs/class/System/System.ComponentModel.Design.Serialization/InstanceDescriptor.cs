//
// System.ComponentModel.Design.Serialization.InstanceDescriptor.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Reflection;
using System.Security.Permissions;

namespace System.ComponentModel.Design.Serialization
{
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	public sealed class InstanceDescriptor {

		private MemberInfo member;
		private ICollection arguments;
		private bool isComplete;

		public InstanceDescriptor (MemberInfo member, ICollection arguments)
			: this (member, arguments, true)
		{
		}

		public InstanceDescriptor(MemberInfo member, ICollection arguments, bool isComplete)
		{
			this.isComplete = isComplete;
			ValidateMember (member, arguments);
			this.member = member;
			this.arguments = arguments;
		}

		private void ValidateMember (MemberInfo member, ICollection arguments)
		{
			if (member == null)
				return;

			switch (member.MemberType) {
			// According to docs only these types are allowed, but the docs do
			// state what happens for other types
			case MemberTypes.Constructor:
				ConstructorInfo CI = (ConstructorInfo) member;
				if (arguments == null) // null counts as no arguments
					if (CI.GetParameters().Length != 0)
						throw new ArgumentException ("Invalid number of arguments for this constructor");
				if (arguments.Count != CI.GetParameters().Length)
					throw new ArgumentException ("Invalid number of arguments for this constructor");
				break;
			case MemberTypes.Method:
				MethodInfo MI = (MethodInfo) member;
				if (!MI.IsStatic)
					throw new ArgumentException ("InstanceDescriptor only describes static (VB.Net: shared) members", "member");
				if (arguments == null) // null counts as no arguments
					if (MI.GetParameters().Length != 0)
						throw new ArgumentException ("Invalid number of arguments for this method", "arguments");
				if (arguments.Count != MI.GetParameters().Length)
					throw new ArgumentException ("Invalid number of arguments for this method");
				break;
			case MemberTypes.Field:
				FieldInfo FI = (FieldInfo) member;
				if (!FI.IsStatic)
					throw new ArgumentException ("Parameter must be static");
				if (arguments != null && arguments.Count != 0) // null counts as no arguments
					throw new ArgumentException ("Field members do not take any arguments");
				break;
			case MemberTypes.Property:
				PropertyInfo PI = (PropertyInfo) member;
				if (!(PI.CanRead))
					throw new ArgumentException ("Parameter must be readable");
				MethodInfo PIM = PI.GetGetMethod();
				if (!PIM.IsStatic)
					throw new ArgumentException ("Parameter must be static");
				break;
			}
		}

		public ICollection Arguments {
			get { 
				// It seems MS does not return null even if we specified null as parameter (but does not cause an exception)
				if (arguments == null)
					return new object[0];
				return arguments;
			}
		}

		public bool IsComplete {
			get { return isComplete; }
		}

		public MemberInfo MemberInfo {
			get { return member; }
		}

		public object Invoke()
		{
			if (member == null)
				return null;

			object[] parsearguments;
			if (arguments == null)
				parsearguments = new object[0];
			else {
				parsearguments = new object[arguments.Count];
				arguments.CopyTo (parsearguments, 0);
			}

			//MemberInfo member;
			switch (member.MemberType) {
			case MemberTypes.Constructor:
				ConstructorInfo CI = (ConstructorInfo) member;
				return CI.Invoke (parsearguments);

			case MemberTypes.Method:
				MethodInfo MI = (MethodInfo) member;
				return MI.Invoke (null, parsearguments);

			case MemberTypes.Field:
				FieldInfo FI = (FieldInfo) member;
				return FI.GetValue (null);

			case MemberTypes.Property:
				PropertyInfo PI = (PropertyInfo) member;
				return PI.GetValue (null, parsearguments);
			}
			return null;
		}
	}
}
