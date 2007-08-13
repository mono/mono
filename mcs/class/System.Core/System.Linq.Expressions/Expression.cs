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

        #region Internal support methods 
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
        private const BindingFlags opBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        private static MethodInfo GetUserDefinedBinaryOperator (Type leftType, Type rightType, string name)
        {
            Type[] types = new Type[2] { leftType, rightType };
                        
            MethodInfo method = leftType.GetMethod (name, opBindingFlags, null, types, null);
            if (method != null) return method;
                
            method = rightType.GetMethod (name, opBindingFlags, null, types, null);
            if (method != null) return method;

            if (method == null && IsNullableType(leftType) && IsNullableType(rightType))
                return GetUserDefinedBinaryOperator(GetNonNullableType(leftType), GetNonNullableType(rightType), name);
        
            return null;
        }

        private static BinaryExpression GetUserDefinedBinaryOperatorOrThrow (ExpressionType nodeType, string name,
                Expression left, Expression right)
        {
            MethodInfo method = GetUserDefinedBinaryOperator(left.type, right.type, name);

            if (method != null)
                return new BinaryExpression (nodeType, left, right, method, method.ReturnType);
            else
                throw new InvalidOperationException(String.Format(
                    "The binary operator Add is not defined for the types '{0}' and '{1}'.", left.type, right.type));

            // Note: here the code in ExpressionUtils has a series of checks to make sure that
            // the method is static, that its return type is not void and that the number of
            // parameters is 2 and they are of the right type, but we already know that! Or not?
        }
        
        private static void ValidateUserDefinedConditionalLogicOperator (ExpressionType nodeType, Type left, Type right, MethodInfo method)
        {
            // Conditional logic need the "definitely true" and "definitely false" operators.
            Type[] types = new Type[1] { left };
                        
            MethodInfo opTrue  = left.GetMethod ("op_True", opBindingFlags, null, types, null);
            MethodInfo opFalse = left.GetMethod ("op_False", opBindingFlags, null, types, null);
            
            if (opTrue == null || opFalse == null)
                throw new ArgumentException(String.Format(
                    "The user-defined operator method '{0}' for operator '{1}' must have associated boolean True and False operators.",
                    method.Name, nodeType));
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
            if (left.type == right.type && ExpressionUtil.IsNumber(left.type))
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
            if (left.type == right.type && ExpressionUtil.IsNumber(left.type))
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
            if (left.type == right.type && (ExpressionUtil.IsInteger(left.type) || left.type == typeof(bool)))
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

            // Since both the expressions define the same integer or boolean type we don't have
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
        public static MethodCallExpression ArrayIndex(Expression array, Expression index)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes)
        {
            throw new NotImplementedException();
        }
        #endregion
        
        public static MethodCallExpression Call(Expression instance, MethodInfo method)
        {
            return Call(instance, method, (Expression[])null);
        }

        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments)
        {
            return Call(null, method, Enumerable.ToReadOnlyCollection<Expression>(arguments));
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments)
        {
            return Call(instance, method, Enumerable.ToReadOnlyCollection<Expression>(arguments));
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (method == null)
                throw new ArgumentNullException("method");
            if (method.IsGenericMethodDefinition)
                    throw new ArgumentException();
            if (method.ContainsGenericParameters)
                    throw new ArgumentException();
            if (!method.IsStatic && instance == null)
                throw new ArgumentNullException("instance");
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

        public static BinaryExpression Divide(Expression left, Expression right)
        {
            return Divide(left, right, null);
        }

        public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            // sine both the expressions define the same numeric type we don't have 
            // to look for the "op_Division" method...
            if (left.type == right.type &&
                ExpressionUtil.IsNumber(left.type))
                return new BinaryExpression(ExpressionType.Divide, left, right, left.type);

            if (method == null)
                method = ExpressionUtil.GetOperatorMethod("op_Division", left.type, right.type);

            // ok if even op_Division is not defined we need to throw an exception...
            if (method == null)
                throw new InvalidOperationException();

            return new BinaryExpression(ExpressionType.Divide, left, right, method, method.ReturnType);
        }

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

        public static BinaryExpression LeftShift(Expression left, Expression right, MethodInfo method)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");

            // since the left expression is of an integer type and the right is of
            // an integer we don't have to look for the "op_LeftShift" method...
            if (ExpressionUtil.IsInteger(left.type) && right.type == typeof(int))
                return new BinaryExpression(ExpressionType.LeftShift, left, right, left.type);

            if (method == null)
                method = ExpressionUtil.GetOperatorMethod("op_LeftShift", left.type, right.type);

            // ok if even op_Division is not defined we need to throw an exception...
            if (method == null)
                throw new InvalidOperationException();

            return new BinaryExpression(ExpressionType.LeftShift, left, right, method, method.ReturnType);
        }

        public static BinaryExpression LeftShift(Expression left, Expression right)
        {
            return LeftShift(left, right, null);
        }

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


        public static TypeBinaryExpression TypeIs(Expression expression, Type type)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (type == null)
                throw new ArgumentNullException("type"); 
            
            return new TypeBinaryExpression(ExpressionType.TypeIs, expression, type, typeof(bool));
        }

        public static UnaryExpression TypeAs(Expression expression, Type type)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            if (type == null)
                throw new ArgumentNullException("type");

            return new UnaryExpression(ExpressionType.TypeAs, expression, type);
        }
    }
}
