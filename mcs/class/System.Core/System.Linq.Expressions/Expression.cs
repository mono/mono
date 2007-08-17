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
//
// Authors:
//        Marek Safar (marek.safar@seznam.cz)
//        Antonello Provenzano  <antonello@deveel.com>
//        Federico Di Gregorio <fog@initd.org>

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    public abstract class Expression
    {
        #region .ctor
        protected Expression (ExpressionType nodeType, Type type)
        {
            this.nodeType = nodeType;
            this.type = type;
        }
        #endregion
        
        #region Fields
        private Type type;
        private ExpressionType nodeType;
        #endregion
        
        #region Properties
        public Type Type {
            get { return type; }
        }

        public ExpressionType NodeType {
            get { return nodeType; }
        }
        #endregion

        #region Internal methods 
        internal virtual void BuildString (StringBuilder builder)
        {
            builder.Append ("[").Append (nodeType).Append ("]");
        }
        
        internal static Type GetNonNullableType(Type type)
        {
            // The Nullable<> class takes a single generic type so we can directly return
            // the first element of the array (if the type is nullable.)
            
            if (IsNullableType (type))
                return type.GetGenericArguments ()[0];
            else
                return type;
        }

        internal static bool IsNullableType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (type.IsGenericType) {
                Type genType = type.GetGenericTypeDefinition();
                return typeof(Nullable<>).IsAssignableFrom(genType);
            }

            return false;
        }
        #endregion
        
        #region Private support methods
        private static int IsWhat(Type type)
        {
            // This method return a "type code" that can be easily compared to a bitmask
            // to determine the "broad type" (integer, boolean, floating-point) of the given type.
            // It is used by the three methods below.

            if (IsNullableType (type))
                type = GetNonNullableType (type);
                
            switch (Type.GetTypeCode (type)) {
                case TypeCode.Byte:  case TypeCode.SByte:
                case TypeCode.Int16: case TypeCode.UInt16:
                case TypeCode.Int32: case TypeCode.UInt32:
                case TypeCode.Int64: case TypeCode.UInt64:
                return 1;
                
                case TypeCode.Boolean:
                return 2;
                
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                return 4;
                
                default:
                return 0;
            }
        }
        
        private static bool IsInteger (Type type)
        {
            return (IsWhat(type) & 1) != 0;
        }

        private static bool IsIntegerOrBool (Type type)
        {
            return (IsWhat(type) & 3) != 0;
        }

        private static bool IsNumeric (Type type)
        {
            return (IsWhat(type) & 5) != 0;        
        }
        
        private const BindingFlags opBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        private static MethodInfo GetUserDefinedBinaryOperator (Type leftType, Type rightType, string name)
        {
            Type[] types = new Type[2] { leftType, rightType };
            MethodInfo method;
            
            method  = leftType.GetMethod (name, opBindingFlags, null, types, null);
            if (method != null) return method;
                
            method = rightType.GetMethod (name, opBindingFlags, null, types, null);
            if (method != null) return method;

            if (method == null && IsNullableType (leftType) && IsNullableType (rightType))
                return GetUserDefinedBinaryOperator (GetNonNullableType (leftType), GetNonNullableType (rightType), name);
        
            return null;
        }

        private static BinaryExpression GetUserDefinedBinaryOperatorOrThrow (ExpressionType nodeType, string name,
                Expression left, Expression right)
        {
            MethodInfo method = GetUserDefinedBinaryOperator(left.type, right.type, name);

            if (method != null)
                return new BinaryExpression (nodeType, left, right, method, method.ReturnType);
            else
                throw new InvalidOperationException (String.Format (
                    "The binary operator Add is not defined for the types '{0}' and '{1}'.", left.type, right.type));

            // Note: here the code in ExpressionUtils has a series of checks to make sure that
            // the method is static, that its return type is not void and that the number of
            // parameters is 2 and they are of the right type, but we already know that! Or not?
        }
        
        private static MethodInfo FindMethod (Type type, string methodName, Type [] typeArgs, Expression [] args, BindingFlags flags)
        {
            MemberInfo[] members = type.FindMembers(MemberTypes.Method, flags,
                delegate(MemberInfo mi, object obj) { return mi.Name == (String)obj; },
                methodName);
            if (members.Length == 0)
                throw new InvalidOperationException (String.Format (
                    "No method '{0}' exists on type '{1}'.", methodName, type.FullName));

            MethodInfo methodDefinition = null;
            MethodInfo method = null;
            int methodCount = 1;        

            foreach (MemberInfo member in members) {
                MethodInfo mi = (MethodInfo)member;
                if (mi.IsGenericMethodDefinition) {
                    // If the generic method definition matches we save it away to be able to make the
                    // correct closed method later on.
                    Type[] genericArgs = mi.GetGenericArguments();
                    if (genericArgs.Length != typeArgs.Length) goto next;

                    methodDefinition = mi;
                    goto next;
                }
                
                // If there is a discrepancy between method's generic types and the given types or if
                // the method is open we simply discard it and go on.
                if ((mi.IsGenericMethod && (typeArgs == null || mi.ContainsGenericParameters))
                     || (!mi.IsGenericMethod && typeArgs != null))
                    goto next;
                    
                // If the method is a closed generic we try to match the generic types.
                if (mi.IsGenericMethod) {
                    Type[] genericArgs = mi.GetGenericArguments();
                    if (genericArgs.Length != typeArgs.Length) goto next;
                    for (int i=0 ; i < genericArgs.Length ; i++)
                        if (genericArgs[i] != typeArgs[i]) goto next;
                }
                
                // Finally we test for the method's parameters.
                ParameterInfo[] parameters = mi.GetParameters ();
                if (parameters.Length != args.Length) goto next;
                for (int i=0 ; i < parameters.Length ; i++)
                    if (parameters[i].ParameterType != args[i].type) goto next;

                method = mi;
                break;
                
             next:
                continue;
            }
            
            if (method != null)
                return method;
            else
                throw new InvalidOperationException(String.Format(
                    "No method '{0}' on type '{1}' is compatible with the supplied arguments.", methodName, type.FullName));
        }

        private static PropertyInfo GetProperty (MethodInfo mi)
        {
            // If the method has the hidebysig and specialname attributes it can be a property accessor;
            // if that's the case we try to extract the type of the property and then we use it and the
            // property name (derived from the method name) to find the right ProprtyInfo.
            
            if (mi.IsHideBySig && mi.IsSpecialName) {
                Type propertyType = null;
                if (mi.Name.StartsWith("set_")) {
                    ParameterInfo[] parameters = mi.GetParameters();
                    if (parameters.Length == 1)
                        propertyType = parameters[0].ParameterType;
                }
                else if (mi.Name.StartsWith("get_")) {
                    propertyType = mi.ReturnType;
                }
                
                if (propertyType != null) {
                    PropertyInfo pi = mi.DeclaringType.GetProperty(mi.Name.Substring(4),
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
                        null, propertyType, new Type[0], null);
                    if (pi != null) return pi;
                }
            }
            
            throw new ArgumentException (String.Format( 
                "The method '{0}.{1}' is not a property accessor", mi.DeclaringType.FullName, mi.Name));
        }
        
        private static void ValidateUserDefinedConditionalLogicOperator (ExpressionType nodeType, Type left, Type right, MethodInfo method)
        {
            // Conditional logic need the "definitely true" and "definitely false" operators.
            Type[] types = new Type[1] { left };
                        
            MethodInfo opTrue  = left.GetMethod ("op_True", opBindingFlags, null, types, null);
            MethodInfo opFalse = left.GetMethod ("op_False", opBindingFlags, null, types, null);
            
            if (opTrue == null || opFalse == null)
                throw new ArgumentException (String.Format (
                    "The user-defined operator method '{0}' for operator '{1}' must have associated boolean True and False operators.",
                    method.Name, nodeType));
        }
        
        private static void ValidateSettableFieldOrPropertyMember (MemberInfo member, out Type memberType)
        {
            if (member.MemberType == MemberTypes.Field) {
                memberType = typeof (FieldInfo);
            }
            else if (member.MemberType == MemberTypes.Property) {
                PropertyInfo pi = (PropertyInfo)member;
                if (!pi.CanWrite)
                    throw new ArgumentException (String.Format ("The property '{0}' has no 'set' accessor", pi));
                memberType = typeof (PropertyInfo);
            }
            else {
                throw new ArgumentException ("Argument must be either a FieldInfo or PropertyInfo");   
            }
        }
        #endregion
                
        #region ToString
        public override string ToString()
        {
            StringBuilder builder = new StringBuilder ();
            BuildString (builder);
            return builder.ToString ();
        }
        #endregion

        #region Add
        public static BinaryExpression Add(Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.Add, left, right, method, method.ReturnType);
            
            // Since both the expressions define the same numeric type we don't have
            // to look for the "op_Addition" method.
            if (left.type == right.type && IsNumeric (left.type))
                return new BinaryExpression(ExpressionType.Add, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.Add, "op_Addition", left, right);
        }

        public static BinaryExpression Add(Expression left, Expression right)
        {
            return Add(left, right, null);
        }
        #endregion
        
        #region AddChecked
        public static BinaryExpression AddChecked(Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.AddChecked, left, right, method, method.ReturnType);

            // Since both the expressions define the same numeric type we don't have
            // to look for the "op_Addition" method.
            if (left.type == right.type && IsNumeric (left.type))
                return new BinaryExpression(ExpressionType.AddChecked, left, right, left.type);

            method = GetUserDefinedBinaryOperator (left.type, right.type, "op_Addition");
            if (method == null)
                throw new InvalidOperationException(String.Format(
                    "The binary operator AddChecked is not defined for the types '{0}' and '{1}'.", left.type, right.type));
            
            Type retType = method.ReturnType;

            // Note: here the code did some very strange checks for bool (but note that bool does
            // not define an addition operator) and created nullables for value types (but the new
            // MS code does not do that). All that has been removed.

            return new BinaryExpression(ExpressionType.AddChecked, left, right, method, retType);
        }

        public static BinaryExpression AddChecked(Expression left, Expression right)
        {
            return AddChecked(left, right, null);
        }
        #endregion

        #region And
        public static BinaryExpression And(Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.And, left, right, method, method.ReturnType);
            
            // Since both the expressions define the same integer or boolean type we don't have
            // to look for the "op_BitwiseAnd" method.
            if (left.type == right.type && IsIntegerOrBool (left.type))
                return new BinaryExpression(ExpressionType.And, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.And, "op_BitwiseAnd", left, right);
        }

        public static BinaryExpression And(Expression left, Expression right)
        {
            return And(left, right, null);
        }
        #endregion
        
        #region AndAlso
        public static BinaryExpression AndAlso(Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            // Since both the expressions define the same boolean type we don't have
            // to look for the "op_BitwiseAnd" method.
            if (left.type == right.type && left.type == typeof(bool))
                return new BinaryExpression(ExpressionType.AndAlso, left, right, left.type);

            // Else we must validate the method to make sure it has companion "true" and "false" operators.
            if (method == null)
                method = GetUserDefinedBinaryOperator (left.type, right.type, "op_BitwiseAnd");
            if (method == null)
                throw new InvalidOperationException(String.Format(
                    "The binary operator AndAlso is not defined for the types '{0}' and '{1}'.", left.type, right.type));
            ValidateUserDefinedConditionalLogicOperator(ExpressionType.AndAlso, left.type, right.type, method);
            
            return new BinaryExpression(ExpressionType.AndAlso, left, right, method, method.ReturnType);
        }

        public static BinaryExpression AndAlso(Expression left, Expression right)
        {
            return AndAlso(left, right, null);
        }
        #endregion
        
        #region ArrayIndex
        public static BinaryExpression ArrayIndex(Expression array, Expression index)
        {
            if (array == null)
                throw new ArgumentNullException ("array");
            if (index == null)
                throw new ArgumentNullException ("index");
            if (!array.type.IsArray)
                throw new ArgumentException ("Argument must be array");
            if (index.type != typeof(int))
                throw new ArgumentException ("Argument for array index must be of type Int32");

            return new BinaryExpression(ExpressionType.ArrayIndex, array, index, array.type.GetElementType());
        }

        public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes)
        {
            return ArrayIndex(array, (IEnumerable<Expression>)indexes);
        }

        public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes)
        {
            if (array == null)
                throw new ArgumentNullException ("array");
            if (indexes == null)
                throw new ArgumentNullException ("indexes");
            if (!array.type.IsArray)
                throw new ArgumentException ("Argument must be array");

            // We'll need an array of typeof(Type) elements long as the array's rank later
            // and also a generic List to hold the indexes (ReadOnlyCollection wants that.)
            
            Type[] types = (Type[])Array.CreateInstance(typeof(Type), array.type.GetArrayRank());
            Expression[] indexesList = new Expression[array.type.GetArrayRank()];
            
            int rank = 0;
            foreach (Expression index in indexes) {
                if (index.type != typeof(int))
                    throw new ArgumentException ("Argument for array index must be of type Int32");
                if (rank == array.type.GetArrayRank())
                    throw new ArgumentException ("Incorrect number of indexes");

                types[rank] = index.type;
                indexesList[rank] = index;
                rank += 1;
            }
                
            // If the array's rank is equalto the number of given indexes we can go on and
            // look for a Get(Int32, ...) method with "rank" parameters to generate the
            // MethodCallExpression.

            MethodInfo method = array.type.GetMethod("Get",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);

            // This should not happen, but we check anyway.
            if (method == null)
                throw new InvalidOperationException(String.Format(
                    "The method Get(...) is not defined for the type '{0}'.", array.type));
    
            return new MethodCallExpression(ExpressionType.Call, method, array, new ReadOnlyCollection<Expression>(indexesList));
        }
        #endregion
        
        #region ArrayLength
        public static UnaryExpression ArrayLength(Expression array)
        {
            if (array == null)
                throw new ArgumentNullException ("array");
            if (!array.type.IsArray)
                throw new ArgumentException ("Argument must be array");

            return new UnaryExpression(ExpressionType.ArrayLength, array, typeof(Int32));        
        }
        #endregion
        
        #region Bind
        public static MemberAssignment Bind (MemberInfo member, Expression expression)
        {
            if (member == null)
                throw new ArgumentNullException ("member");
            if (expression == null)
                throw new ArgumentNullException ("expression");
                
            Type memberType;
            ValidateSettableFieldOrPropertyMember(member, out memberType);            
        
            return new MemberAssignment(member, expression);
        }

        public static MemberAssignment Bind (MethodInfo propertyAccessor, Expression expression)
        {
            if (propertyAccessor == null)
                throw new ArgumentNullException ("propertyAccessor");
            if (expression == null)
                throw new ArgumentNullException ("expression");

            return new MemberAssignment(GetProperty(propertyAccessor), expression);        
        }
        #endregion
        
        #region Call
        public static MethodCallExpression Call(Expression instance, MethodInfo method)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            if (instance == null && !method.IsStatic)
                throw new ArgumentNullException("instance");
                
            return Call(instance, method, (Expression[])null);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments)
        {
            return Call(instance, method, (IEnumerable<Expression>)arguments);
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
        {
            if (method == null)
                throw new ArgumentNullException("method");
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (instance == null && !method.IsStatic)
                throw new ArgumentNullException("instance");
                
            if (method.IsGenericMethodDefinition)
                    throw new ArgumentException();
            if (method.ContainsGenericParameters)
                    throw new ArgumentException();
            if (instance != null && !instance.type.IsAssignableFrom(method.DeclaringType))
                throw new ArgumentException();

            ReadOnlyCollection<Expression> roArgs = Enumerable.ToReadOnlyCollection<Expression>(arguments);

            ParameterInfo[] pars = method.GetParameters();
            if (Enumerable.Count<Expression>(arguments) != pars.Length)
                throw new ArgumentException();

            if (pars.Length > 0)
            {
                //TODO: validate the parameters against the arguments...
            }

            return new MethodCallExpression(ExpressionType.Call, method, instance, roArgs);
        }

        public static MethodCallExpression Call (Expression instance, string methodName, Type [] typeArguments, params Expression [] arguments)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            return Call (null, FindMethod (instance.type, methodName, typeArguments, arguments,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance),
                (IEnumerable<Expression>)arguments);        
        }
        
        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments)
        {
            return Call(null, method, (IEnumerable<Expression>)arguments);
        }

        public static MethodCallExpression Call (Type type, string methodName, Type [] typeArguments, params Expression [] arguments)
        {
            // FIXME: MS implementation does not check for type here and simply lets FindMethod() raise
            // a NullReferenceException. Shall we do the same or raise the correct exception here?
            //if (type == null)
            //    throw new ArgumentNullException("type");
            
            if (methodName == null)
                throw new ArgumentNullException("methodName");
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            // Note that we're looking for static methods only (this version of Call() doesn't take an instance).
            return Call (null, FindMethod (type, methodName, typeArguments, arguments,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static),
                (IEnumerable<Expression>)arguments);
        }
        #endregion

        // NOTE: CallVirtual is not implemented because it is already marked as Obsolete by MS.
        
        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse)
        {
            if (test == null)
                throw new ArgumentNullException("test");
            if (ifTrue == null)
                throw new ArgumentNullException("ifTrue");
            if (ifFalse == null)
                throw new ArgumentNullException("ifFalse");
            if (test.type != typeof(bool))
                throw new ArgumentException();
            if (ifTrue.type != ifFalse.type)
                throw new ArgumentException();

            return new ConditionalExpression(test, ifTrue, ifFalse, ifTrue.type);
        }

        public static ConstantExpression Constant(object value, Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (value == null && !IsNullableType(type))
                throw new ArgumentException("Argument types do not match");

            return new ConstantExpression(value, type);
        }

        public static ConstantExpression Constant(object value)
        {
            if (value != null)
                return new ConstantExpression(value, value.GetType());
            else
                return new ConstantExpression(null, typeof(object));
        }

        #region Divide
        public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.Divide, left, right, method, method.ReturnType);
            
            // Since both the expressions define the same numeric type we don't have
            // to look for the "op_Addition" method.
            if (left.type == right.type && IsNumeric (left.type))
                return new BinaryExpression(ExpressionType.Divide, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.Divide, "op_Division", left, right);
        }

        public static BinaryExpression Divide(Expression left, Expression right)
        {
            return Divide(left, right, null);
        }
        #endregion
        
        #region ExclusiveOr
        public static BinaryExpression ExclusiveOr (Expression left, Expression right, System.Reflection.MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.ExclusiveOr, left, right, method, method.ReturnType);
            
            // Since both the expressions define the same integer or boolean type we don't have
            // to look for the "op_BitwiseAnd" method.
            if (left.type == right.type && IsIntegerOrBool (left.type))
                return new BinaryExpression(ExpressionType.ExclusiveOr, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.ExclusiveOr, "op_ExclusiveOr", left, right);
        }
        
        public static BinaryExpression ExclusiveOr (Expression left, Expression right)
        {
            return ExclusiveOr (left, right, null);
        }
        #endregion
        
        public static MemberExpression Field(Expression expression, FieldInfo field)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            return new MemberExpression(expression, field, field.FieldType);
        }

        public static MemberExpression Field(Expression expression, string fieldName)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            FieldInfo field = expression.Type.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new ArgumentException();

            return Field(expression, field);
        }

        public static FuncletExpression Funclet(Funclet funclet, Type type)
        {
            if (funclet == null)
                throw new ArgumentNullException("funclet");
            if (type == null)
                throw new ArgumentNullException("type");

            return new FuncletExpression(funclet, type);
        }

        public static Type GetFuncType(params Type[] typeArgs)
        {
            if (typeArgs == null)
                throw new ArgumentNullException("typeArgs");
            if (typeArgs.Length > 5)
                throw new ArgumentException();

            return typeof(Func<,,,,>).MakeGenericType(typeArgs);
        }

        #region LeftShift
        public static BinaryExpression LeftShift (Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.LeftShift, left, right, method, method.ReturnType);
            
            // If the left side is any kind of integer and the right is int32 we don't have
            // to look for the "op_Addition" method.
            if (IsInteger(left.type) && right.type == typeof(Int32))
                return new BinaryExpression(ExpressionType.LeftShift, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.LeftShift, "op_LeftShift", left, right);
        }

        public static BinaryExpression LeftShift (Expression left, Expression right)
        {
            return LeftShift (left, right, null);
        }
        #endregion
        
        public static ListInitExpression ListInit(NewExpression newExpression, params Expression[] initializers)
        {
            if (initializers == null)
                throw new ArgumentNullException("inizializers");

            return ListInit(newExpression, Enumerable.ToReadOnlyCollection<Expression>(initializers));
        }

        public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers)
        {
            if (newExpression == null)
                throw new ArgumentNullException("newExpression");
            if (initializers == null)
                throw new ArgumentNullException("inizializers");

            return new ListInitExpression(newExpression, Enumerable.ToReadOnlyCollection<Expression>(initializers));
        }

        public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings)
        {
            if (newExpression == null)
                throw new ArgumentNullException("newExpression");

            if (bindings == null)
                throw new ArgumentNullException("bindings");

            return new MemberInitExpression(newExpression, Enumerable.ToReadOnlyCollection<MemberBinding>(bindings));
        }

        #region Modulo
        public static BinaryExpression Modulo (Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.Modulo, left, right, method, method.ReturnType);
            
            // Since both the expressions define the same integer or boolean type we don't have
            // to look for the "op_BitwiseAnd" method.
            if (left.type == right.type && IsNumeric (left.type))
                return new BinaryExpression(ExpressionType.Modulo, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.Modulo, "op_Modulus", left, right);
        
        }
        
        public static BinaryExpression Modulo (Expression left, Expression right)
        {
            return Modulo (left, right, null);        
        }
        #endregion
        
        #region Multiply
        public static BinaryExpression Multiply (Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.Multiply, left, right, method, method.ReturnType);
            
            // Since both the expressions define the same integer or boolean type we don't have
            // to look for the "op_BitwiseAnd" method.
            if (left.type == right.type && IsNumeric (left.type))
                return new BinaryExpression(ExpressionType.Multiply, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.Multiply, "op_Multiply", left, right);
        
        }
        
        public static BinaryExpression Multiply (Expression left, Expression right)
        {
            return Multiply (left, right, null);
        }
        #endregion
        
        #region MultiplyChecked
        public static BinaryExpression MultiplyChecked (Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.MultiplyChecked, left, right, method, method.ReturnType);

            // Since both the expressions define the same numeric type we don't have
            // to look for the "op_Addition" method.
            if (left.type == right.type && IsNumeric (left.type))
                return new BinaryExpression(ExpressionType.MultiplyChecked, left, right, left.type);

            method = GetUserDefinedBinaryOperator (left.type, right.type, "op_Multiply");
            if (method == null)
                throw new InvalidOperationException(String.Format(
                    "The binary operator MultiplyChecked is not defined for the types '{0}' and '{1}'.", left.type, right.type));
            
            Type retType = method.ReturnType;

            return new BinaryExpression(ExpressionType.MultiplyChecked, left, right, method, retType);
        }
        
        public static BinaryExpression MultiplyChecked (Expression left, Expression right)
        {
            return MultiplyChecked(left, right, null);
        }
        #endregion
        
        public static MemberExpression Property(Expression expression, PropertyInfo property)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            MethodInfo getMethod = property.GetGetMethod(true);
            if (getMethod == null)
                throw new ArgumentException(); // to access the property we need to have
                                               // a get method...

            return new MemberExpression(expression, property, property.PropertyType);
        }
        
        #region Or
        public static BinaryExpression Or (Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.Or, left, right, method, method.ReturnType);
            
            // Since both the expressions define the same integer or boolean type we don't have
            // to look for the "op_BitwiseOr" method.
            if (left.type == right.type && IsIntegerOrBool (left.type))
                return new BinaryExpression(ExpressionType.Or, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.Or, "op_BitwiseOr", left, right);
        
        }

        public static BinaryExpression Or (Expression left, Expression right)
        {
            return Or (left, right, null);
        }
        #endregion
        
        #region OrElse
        public static BinaryExpression OrElse (Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            // Since both the expressions define the same boolean type we don't have
            // to look for the "op_BitwiseOr" method.
            if (left.type == right.type && left.type == typeof(bool))
                return new BinaryExpression(ExpressionType.OrElse, left, right, left.type);

            // Else we must validate the method to make sure it has companion "true" and "false" operators.
            if (method == null)
                method = GetUserDefinedBinaryOperator (left.type, right.type, "op_BitwiseOr");
            if (method == null)
                throw new InvalidOperationException(String.Format(
                    "The binary operator OrElse is not defined for the types '{0}' and '{1}'.", left.type, right.type));
            ValidateUserDefinedConditionalLogicOperator(ExpressionType.OrElse, left.type, right.type, method);
            
            return new BinaryExpression(ExpressionType.OrElse, left, right, method, method.ReturnType);
        }
        
        public static BinaryExpression OrElse (Expression left, Expression right)
        {
            return OrElse(left, right, null);
        }
        #endregion
        
        public static UnaryExpression Quote(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            return new UnaryExpression(ExpressionType.Quote, expression, expression.GetType());
        }


        public static MemberExpression Property(Expression expression, string propertyName)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            PropertyInfo property = expression.Type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (property == null)
                throw new ArgumentException();

            return Property(expression, property);
        }

        public static MemberExpression PropertyOrField(Expression expression, string propertyOrFieldName)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            PropertyInfo property = expression.Type.GetProperty(propertyOrFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (property != null)
                return Property(expression, property);

            FieldInfo field = expression.Type.GetField(propertyOrFieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            if (field != null)
                return Field(expression, field);
                
            //TODO: should we return <null> here?
            // the name is not defined in the Type of the expression given...
            throw new ArgumentException();
        }

        #region RightShift
        public static BinaryExpression RightShift (Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.RightShift, left, right, method, method.ReturnType);
            
            // If the left side is any kind of integer and the right is int32 we don't have
            // to look for the "op_Addition" method.
            if (IsInteger(left.type) && right.type == typeof(Int32))
                return new BinaryExpression(ExpressionType.RightShift, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.RightShift, "op_RightShift", left, right);
        }

        public static BinaryExpression RightShift (Expression left, Expression right)
        {
            return RightShift (left, right, null);
        }
        #endregion

        #region Subtract
        public static BinaryExpression Subtract (Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.Subtract, left, right, method, method.ReturnType);
            
            // Since both the expressions define the same numeric type we don't have
            // to look for the "op_Addition" method.
            if (left.type == right.type && IsNumeric (left.type))
                return new BinaryExpression(ExpressionType.Subtract, left, right, left.type);

            // Else we try for a user-defined operator.
            return GetUserDefinedBinaryOperatorOrThrow (ExpressionType.Subtract, "op_Subtraction", left, right);        
        }
        
        public static BinaryExpression Subtract (Expression left, Expression right)
        {
            return Subtract (left, right, null);        
        }
        #endregion
        
        #region SubtractChecked
        public static BinaryExpression SubtractChecked (Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException ("left");
            if (right == null)
                throw new ArgumentNullException ("right");

            if (method != null)
                return new BinaryExpression(ExpressionType.SubtractChecked, left, right, method, method.ReturnType);

            // Since both the expressions define the same numeric type we don't have
            // to look for the "op_Addition" method.
            if (left.type == right.type && IsNumeric (left.type))
                return new BinaryExpression(ExpressionType.SubtractChecked, left, right, left.type);

            method = GetUserDefinedBinaryOperator (left.type, right.type, "op_Subtraction");
            if (method == null)
                throw new InvalidOperationException(String.Format(
                    "The binary operator AddChecked is not defined for the types '{0}' and '{1}'.", left.type, right.type));
            
            Type retType = method.ReturnType;

            // Note: here the code did some very strange checks for bool (but note that bool does
            // not define an addition operator) and created nullables for value types (but the new
            // MS code does not do that). All that has been removed.

            return new BinaryExpression(ExpressionType.SubtractChecked, left, right, method, retType);
        }
        
        public static BinaryExpression SubtractChecked (Expression left, Expression right)
        {
            return SubtractChecked (left, right, null);
        }
        #endregion

        #region TypeAs
        public static UnaryExpression TypeAs(Expression expression, Type type)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (type == null)
                throw new ArgumentNullException("type");

            return new UnaryExpression(ExpressionType.TypeAs, expression, type);
        }
        #endregion

        #region TypeIs
        public static TypeBinaryExpression TypeIs(Expression expression, Type type)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (type == null)
                throw new ArgumentNullException("type"); 
            
            return new TypeBinaryExpression(ExpressionType.TypeIs, expression, type, typeof(bool));
        }
        #endregion
    }
}
