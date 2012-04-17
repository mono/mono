/*
  Copyright (C) 2010-2012 Jeroen Frijters

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
		}

		public virtual MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] names, out object state)
		{
			throw new InvalidOperationException();
		}

		public virtual FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}

		public virtual object ChangeType(object value, Type type, CultureInfo culture)
		{
			throw new InvalidOperationException();
		}

		public virtual void ReorderArgumentArray(ref object[] args, object state)
		{
			throw new InvalidOperationException();
		}

		public abstract MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers);
		public abstract PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers);
	}

	sealed class DefaultBinder : Binder
	{
		public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
		{
			int matchCount = 0;
			foreach (MethodBase method in match)
			{
				if (MatchParameterTypes(method.GetParameters(), types))
				{
					match[matchCount++] = method;
				}
			}

			if (matchCount == 0)
			{
				return null;
			}

			if (matchCount == 1)
			{
				return match[0];
			}

			MethodBase bestMatch = match[0];
			bool ambiguous = false;
			for (int i = 1; i < matchCount; i++)
			{
				bestMatch = SelectBestMatch(bestMatch, match[i], types, ref ambiguous);
			}
			if (ambiguous)
			{
				throw new AmbiguousMatchException();
			}
			return bestMatch;
		}

		private static bool MatchParameterTypes(ParameterInfo[] parameters, Type[] types)
		{
			if (parameters.Length != types.Length)
			{
				return false;
			}
			for (int i = 0; i < parameters.Length; i++)
			{
				Type sourceType = types[i];
				Type targetType = parameters[i].ParameterType;
				if (sourceType != targetType
					&& !targetType.IsAssignableFrom(sourceType)
					&& !IsAllowedPrimitiveConversion(sourceType, targetType))
				{
					return false;
				}
			}
			return true;
		}

		private static MethodBase SelectBestMatch(MethodBase mb1, MethodBase mb2, Type[] types, ref bool ambiguous)
		{
			switch (MatchSignatures(mb1.MethodSignature, mb2.MethodSignature, types))
			{
				case 1:
					return mb1;
				case 2:
					return mb2;
			}

			if (mb1.MethodSignature.MatchParameterTypes(mb2.MethodSignature))
			{
				int depth1 = GetInheritanceDepth(mb1.DeclaringType);
				int depth2 = GetInheritanceDepth(mb2.DeclaringType);
				if (depth1 > depth2)
				{
					return mb1;
				}
				else if (depth1 < depth2)
				{
					return mb2;
				}
			}

			ambiguous = true;
			return mb1;
		}

		private static int GetInheritanceDepth(Type type)
		{
			int depth = 0;
			while (type != null)
			{
				depth++;
				type = type.BaseType;
			}
			return depth;
		}

		private static int MatchSignatures(MethodSignature sig1, MethodSignature sig2, Type[] types)
		{
			for (int i = 0; i < sig1.GetParameterCount(); i++)
			{
				Type type1 = sig1.GetParameterType(i);
				Type type2 = sig2.GetParameterType(i);
				if (type1 != type2)
				{
					return MatchTypes(type1, type2, types[i]);
				}
			}
			return 0;
		}

		private static int MatchSignatures(PropertySignature sig1, PropertySignature sig2, Type[] types)
		{
			for (int i = 0; i < sig1.ParameterCount; i++)
			{
				Type type1 = sig1.GetParameter(i);
				Type type2 = sig2.GetParameter(i);
				if (type1 != type2)
				{
					return MatchTypes(type1, type2, types[i]);
				}
			}
			return 0;
		}

		private static int MatchTypes(Type type1, Type type2, Type type)
		{
			if (type1 == type)
			{
				return 1;
			}
			if (type2 == type)
			{
				return 2;
			}
			bool conv = type1.IsAssignableFrom(type2);
			return conv == type2.IsAssignableFrom(type1) ? 0 : conv ? 2 : 1;
		}

		private static bool IsAllowedPrimitiveConversion(Type source, Type target)
		{
			// we need to check for primitives, because GetTypeCode will return the underlying type for enums
			if (!source.IsPrimitive || !target.IsPrimitive)
			{
				return false;
			}
			TypeCode sourceType = Type.GetTypeCode(source);
			TypeCode targetType = Type.GetTypeCode(target);
			switch (sourceType)
			{
				case TypeCode.Char:
					switch (targetType)
					{
						case TypeCode.UInt16:
						case TypeCode.UInt32:
						case TypeCode.Int32:
						case TypeCode.UInt64:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				case TypeCode.Byte:
					switch (targetType)
					{
						case TypeCode.Char:
						case TypeCode.UInt16:
						case TypeCode.Int16:
						case TypeCode.UInt32:
						case TypeCode.Int32:
						case TypeCode.UInt64:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				case TypeCode.SByte:
					switch (targetType)
					{
						case TypeCode.Int16:
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				case TypeCode.UInt16:
					switch (targetType)
					{
						case TypeCode.UInt32:
						case TypeCode.Int32:
						case TypeCode.UInt64:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				case TypeCode.Int16:
					switch (targetType)
					{
						case TypeCode.Int32:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				case TypeCode.UInt32:
					switch (targetType)
					{
						case TypeCode.UInt64:
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				case TypeCode.Int32:
					switch (targetType)
					{
						case TypeCode.Int64:
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				case TypeCode.UInt64:
					switch (targetType)
					{
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				case TypeCode.Int64:
					switch (targetType)
					{
						case TypeCode.Single:
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				case TypeCode.Single:
					switch (targetType)
					{
						case TypeCode.Double:
							return true;
						default:
							return false;
					}
				default:
					return false;
			}
		}

		public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes, ParameterModifier[] modifiers)
		{
			int matchCount = 0;
			foreach (PropertyInfo property in match)
			{
				if (indexes == null || MatchParameterTypes(property.GetIndexParameters(), indexes))
				{
					if (returnType != null)
					{
						if (property.PropertyType.IsPrimitive)
						{
							if (!IsAllowedPrimitiveConversion(returnType, property.PropertyType))
							{
								continue;
							}
						}
						else
						{
							if (!property.PropertyType.IsAssignableFrom(returnType))
							{
								continue;
							}
						}
					}
					match[matchCount++] = property;
				}
			}

			if (matchCount == 0)
			{
				return null;
			}

			if (matchCount == 1)
			{
				return match[0];
			}

			PropertyInfo bestMatch = match[0];
			bool ambiguous = false;
			for (int i = 1; i < matchCount; i++)
			{
				int best = MatchTypes(bestMatch.PropertyType, match[i].PropertyType, returnType);
				if (best == 0 && indexes != null)
				{
					best = MatchSignatures(bestMatch.PropertySignature, match[i].PropertySignature, indexes);
				}
				if (best == 0)
				{
					int depth1 = GetInheritanceDepth(bestMatch.DeclaringType);
					int depth2 = GetInheritanceDepth(match[i].DeclaringType);
					if (bestMatch.Name == match[i].Name && depth1 != depth2)
					{
						if (depth1 > depth2)
						{
							best = 1;
						}
						else
						{
							best = 2;
						}
					}
					else
					{
						ambiguous = true;
					}
				}
				if (best == 2)
				{
					ambiguous = false;
					bestMatch = match[i];
				}
			}
			if (ambiguous)
			{
				throw new AmbiguousMatchException();
			}
			return bestMatch;
		}
	}
}
