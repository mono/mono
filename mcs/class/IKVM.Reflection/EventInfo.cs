/*
  Copyright (C) 2009 Jeroen Frijters

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

namespace IKVM.Reflection
{
	public abstract class EventInfo : MemberInfo
	{
		// prevent external subclasses
		internal EventInfo()
		{
		}

		public sealed override MemberTypes MemberType
		{
			get { return MemberTypes.Event; }
		}

		public abstract EventAttributes Attributes { get; }
		public abstract MethodInfo GetAddMethod(bool nonPublic);
		public abstract MethodInfo GetRaiseMethod(bool nonPublic);
		public abstract MethodInfo GetRemoveMethod(bool nonPublic);
		public abstract MethodInfo[] GetOtherMethods(bool nonPublic);
		public abstract MethodInfo[] __GetMethods();
		public abstract Type EventHandlerType { get; }
		internal abstract bool IsPublic { get; }
		internal abstract bool IsStatic { get; }

		public bool IsSpecialName
		{
			get { return (Attributes & EventAttributes.SpecialName) != 0; }
		}

		public MethodInfo GetAddMethod()
		{
			return GetAddMethod(false);
		}

		public MethodInfo GetRaiseMethod()
		{
			return GetRaiseMethod(false);
		}

		public MethodInfo GetRemoveMethod()
		{
			return GetRemoveMethod(false);
		}

		public MethodInfo[] GetOtherMethods()
		{
			return GetOtherMethods(false);
		}

		internal virtual EventInfo BindTypeParameters(Type type)
		{
			return new GenericEventInfo(this.DeclaringType.BindTypeParameters(type), this);
		}

		public override string ToString()
		{
			return this.DeclaringType.ToString() + " " + Name;
		}
	}
}
