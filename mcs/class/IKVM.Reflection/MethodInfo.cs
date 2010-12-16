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
using System;
using System.Text;

namespace IKVM.Reflection
{
	public abstract class MethodInfo : MethodBase, IGenericContext, IGenericBinder
	{
		public sealed override MemberTypes MemberType
		{
			get { return MemberTypes.Method; }
		}

		public abstract Type ReturnType { get; }
		public abstract ParameterInfo ReturnParameter { get; }

		public virtual MethodInfo MakeGenericMethod(params Type[] typeArguments)
		{
			throw new NotSupportedException(this.GetType().FullName);
		}

		public virtual MethodInfo GetGenericMethodDefinition()
		{
			throw new NotSupportedException(this.GetType().FullName);
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(this.ReturnType.Name).Append(' ').Append(this.Name);
			string sep;
			if (this.IsGenericMethod)
			{
				sb.Append('[');
				sep = "";
				foreach (Type arg in GetGenericArguments())
				{
					sb.Append(sep).Append(arg);
					sep = ", ";
				}
				sb.Append(']');
			}
			sb.Append('(');
			sep = "";
			foreach (ParameterInfo arg in GetParameters())
			{
				sb.Append(sep).Append(arg.ParameterType);
				sep = ", ";
			}
			sb.Append(')');
			return sb.ToString();
		}

		internal bool IsNewSlot
		{
			get { return (this.Attributes & MethodAttributes.NewSlot) != 0; }
		}

		public MethodInfo GetBaseDefinition()
		{
			MethodInfo match = this;
			if (match.IsVirtual)
			{
				for (Type type = this.DeclaringType.BaseType; type != null && !match.IsNewSlot; type = type.BaseType)
				{
					MethodInfo method = type.FindMethod(this.Name, this.MethodSignature) as MethodInfo;
					if (method != null && method.IsVirtual)
					{
						match = method;
					}
				}
			}
			return match;
		}

		Type IGenericContext.GetGenericTypeArgument(int index)
		{
			return this.DeclaringType.GetGenericTypeArgument(index);
		}

		Type IGenericContext.GetGenericMethodArgument(int index)
		{
			return GetGenericMethodArgument(index);
		}

		internal virtual Type GetGenericMethodArgument(int index)
		{
			throw new InvalidOperationException();
		}

		internal virtual int GetGenericMethodArgumentCount()
		{
			throw new InvalidOperationException();
		}

		internal override MethodInfo GetMethodOnTypeDefinition()
		{
			return this;
		}

		Type IGenericBinder.BindTypeParameter(Type type)
		{
			return this.DeclaringType.GetGenericTypeArgument(type.GenericParameterPosition);
		}

		Type IGenericBinder.BindMethodParameter(Type type)
		{
			return GetGenericMethodArgument(type.GenericParameterPosition);
		}

		internal override MethodBase BindTypeParameters(Type type)
		{
			return new GenericMethodInstance(this.DeclaringType.BindTypeParameters(type), this, null);
		}

		// This method is used by ILGenerator and exists to allow ArrayMethod to override it,
		// because ArrayMethod doesn't have a working MethodAttributes property, so it needs
		// to base the result of this on the CallingConvention.
		internal virtual bool HasThis
		{
			get { return !IsStatic; }
		}
	}
}
