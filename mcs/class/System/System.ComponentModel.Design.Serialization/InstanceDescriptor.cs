//
// System.ComponentModel.Design.Serialization.InstanceDescriptor.cs
//
// Authors:
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2003 Martin Willemoes Hansen
// (C) 2003 Andreas Nahr
//

using System.Collections;
using System.Reflection;

namespace System.ComponentModel.Design.Serialization
{
	public sealed class InstanceDescriptor
	{

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
			if (member == null)
				throw new ArgumentNullException ("member", "MemberInfo must be valid");
			if (!IsMemberValid (member, arguments))
				throw new ArgumentException ("Only Constructor, Method, Field or Property members allowed", "member");
			this.member = member;
			this.arguments = arguments;
		}

		private bool IsMemberValid (MemberInfo member, ICollection arguments)
		{
			switch (member.MemberType) {
			// According to docs only these types are allowed
			case MemberTypes.Constructor:
				ConstructorInfo CI = (ConstructorInfo) member;
				if (!CI.IsStatic)
					throw new ArgumentException ("InstanceDescriptor only describes static (VB.Net: shared) members", "member");
				if (arguments == null) // null counts as no arguments
					if (CI.GetParameters().Length != 0)
						throw new ArgumentException ("Invalid number of arguments for this constructor", "arguments");
				if (arguments.Count != CI.GetParameters().Length)
					throw new ArgumentException ("Invalid number of arguments for this constructor", "arguments");
				return true;
			case MemberTypes.Method:
				MethodInfo MI = (MethodInfo) member;
				if (!MI.IsStatic)
					throw new ArgumentException ("InstanceDescriptor only describes static (VB.Net: shared) members", "member");
				if (arguments == null) // null counts as no arguments
					if (MI.GetParameters().Length != 0)
						throw new ArgumentException ("Invalid number of arguments for this method", "arguments");
				if (arguments.Count != MI.GetParameters().Length)
					throw new ArgumentException ("Invalid number of arguments for this method", "arguments");
				return true;
			case MemberTypes.Field:
				FieldInfo FI = (FieldInfo) member;
				if (!FI.IsStatic)
					throw new ArgumentException ("InstanceDescriptor only describes static (VB.Net: shared) members", "member");
				if (arguments == null) // null counts as no arguments
					return true;
				if (arguments.Count == 0)
					throw new ArgumentException ("Field members do not take any arguments", "arguments");
				return true;
			case MemberTypes.Property:
				PropertyInfo PI = (PropertyInfo) member;
				if (!(PI.CanRead))
					throw new ArgumentException ("That property cannot be read", "member");
				MethodInfo PIM = PI.GetGetMethod();
				if (!PIM.IsStatic)
					throw new ArgumentException ("InstanceDescriptor only describes static (VB.Net: shared) members", "member");
				if (arguments == null) // null counts as no arguments
					if (PIM.GetParameters().Length != 0)
						throw new ArgumentException ("Invalid number of arguments for this property", "arguments");
				if (arguments.Count != PIM.GetParameters().Length)
					throw new ArgumentException ("Invalid number of arguments for this property", "arguments");
				return true;
			}
			return false;
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
			object[] parsearguments;
			if (arguments == null)
				parsearguments = new object[0];
			else {
				parsearguments = new object[arguments.Count - 1];
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
