using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Reflection;
using System.Data.Linq.Mapping;
using System.Reflection.Emit;

namespace DbLinq.Data.Linq.Mapping
{
	static class LambdaMetaAccessor
	{
		//This will go away with C# 4.0 ActionExpression
		static Delegate MakeSetter(MemberInfo member, Type memberType, Type declaringType)
		{
			Type delegateType = typeof(Action<,>).MakeGenericType(declaringType, memberType);

			switch (member.MemberType)
			{
				case MemberTypes.Property: {
                    MethodInfo method = ((PropertyInfo)member).GetSetMethod();
                    if (method != null)
                        return Delegate.CreateDelegate(delegateType, method);
                    var ca = member.GetCustomAttributes(typeof(ColumnAttribute), true)[0] as ColumnAttribute;
                    member = declaringType.GetField(ca.Storage,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    goto case MemberTypes.Field;
                }
				case MemberTypes.Field:
					{
						DynamicMethod m = new DynamicMethod("setter", 
                            typeof(void), 
                            new Type[] { declaringType, memberType },
                            true);
						ILGenerator cg = m.GetILGenerator();
					
						// arg0.<field> = arg1
						cg.Emit(OpCodes.Ldarg_0);
						cg.Emit(OpCodes.Ldarg_1);
						cg.Emit(OpCodes.Stfld, (FieldInfo)member);
						cg.Emit(OpCodes.Ret);
					
						return m.CreateDelegate(delegateType);
					}
				case MemberTypes.Method:
					 return Delegate.CreateDelegate(delegateType, (MethodInfo)member);
				default:
					throw new InvalidOperationException();
			}
		}

		public static MetaAccessor Create(MemberInfo member, Type declaringType)
		{
			Type memberType;
			switch (member.MemberType)
			{
				case MemberTypes.Property:
					memberType = ((PropertyInfo)member).PropertyType;
					break;
				case MemberTypes.Field:
					memberType = ((FieldInfo)member).FieldType;
					break;
				case MemberTypes.Method:
					memberType = ((MethodInfo)member).ReturnType;
					break;
				default:
					throw new InvalidOperationException();
			}
			Type accessorType = typeof(LambdaMetaAccessor<,>).MakeGenericType(declaringType, memberType);
			
			ParameterExpression p = Expression.Parameter(declaringType, "e");
			return (MetaAccessor)Activator.CreateInstance(accessorType, new object[]{ 
				Expression.Lambda(Expression.MakeMemberAccess(p, member), p).Compile(),
				MakeSetter(member, memberType, declaringType) }
			);
		}
	}

	class LambdaMetaAccessor<TEntity, TMember> : MetaAccessor<TEntity, TMember>
	{
		Func<TEntity, TMember> _Accessor;
		Action<TEntity, TMember> _Setter;

		public LambdaMetaAccessor(Func<TEntity, TMember> accessor, Action<TEntity, TMember> setter)
		{
			_Accessor = accessor;
			_Setter = setter;
		}

		//
		// Summary:
		//     Specifies the strongly typed value.
		//
		// Parameters:
		//   instance:
		//     The instance from which to get the value.
		public override TMember GetValue(TEntity instance)
		{
			return _Accessor(instance);
		}

		//
		// Summary:
		//     Specifies an instance on which to set the strongly typed value.
		//
		// Parameters:
		//   instance:
		//     The instance into which to set the value.
		//
		//   value:
		//     The strongly typed value to set.
		public override void SetValue(ref TEntity instance, TMember value)
		{
			_Setter(instance, value);
		}
	}
}
