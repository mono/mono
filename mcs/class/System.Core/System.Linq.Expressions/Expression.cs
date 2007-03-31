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
//

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;

namespace System.Linq.Expressions
{
    public abstract class Expression
    {
        #region .ctor
        protected Expression(ExpressionType nodeType, Type type)
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
        public Type Type
        {
            get { return type; }
        }

        public ExpressionType NodeType
        {
            get { return nodeType; }
        }
        #endregion

        #region Private Static Methods
        private static void CheckLeftRight(Expression left, Expression right)
        {
            if (left == null)
                throw new ArgumentNullException("left");
            if (right == null)
                throw new ArgumentNullException("right");
        }

        #endregion

        #region Internal Methods
        internal virtual void BuildString(StringBuilder builder)
        {
            builder.Append("[" + nodeType + "]");
        }
        #endregion

        #region Public Methods
        public override string ToString()
        {
            //TODO: check this...
            StringBuilder builder = new StringBuilder();
            BuildString(builder);
            return builder.ToString();
        }
        #endregion

        #region Internal Static Methos
        internal static Type GetNonNullableType(Type type)
        {
            if (IsNullableType(type))
            {
                //TODO: check this... should we return just the first argument?
                Type[] argTypes = type.GetGenericArguments();
                return argTypes[0];
            }

            return type;
        }

        internal static bool IsNullableType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            if (type.IsGenericType)
            {
                Type genType = type.GetGenericTypeDefinition();
                return typeof(Nullable<>).IsAssignableFrom(genType);
            }

            return false;
        }
        #endregion

        #region Public Static Methods
        public static BinaryExpression Add(Expression left, Expression right, MethodInfo method)
        {
            CheckLeftRight(left, right);

            // sine both the expressions define the same numeric type we don't have
            // to look for the "op_Addition" method...
            if (left.type == right.type &&
                ExpressionUtil.IsNumber (left.type))
                return new BinaryExpression(ExpressionType.Add, left, right, left.type);

            if (method == null)
                method = ExpressionUtil.GetOperatorMethod("op_Addition", left.type, right.type);

            // ok if even op_Addition is not defined we need to throw an exception...
            if (method == null)
                throw new InvalidOperationException();

            return new BinaryExpression(ExpressionType.Add, left, right, method, method.ReturnType);
        }

        public static BinaryExpression Add(Expression left, Expression right)
        {
            return Add(left, right, null);
        }

        public static BinaryExpression AddChecked(Expression left, Expression right, MethodInfo method)
        {
            CheckLeftRight(left, right);

            // sine both the expressions define the same numeric type we don't have
            // to look for the "op_Addition" method...
            if (left.type == right.type &&
                ExpressionUtil.IsNumber(left.type))
                return new BinaryExpression(ExpressionType.AddChecked, left, right, left.type);

            if (method == null)
            {
                // in the case of a non-specified method we have to 'check'
                method = ExpressionUtil.GetOperatorMethod("op_Addition", left.type, right.type);
                if (method == null)
                    throw new InvalidOperationException();

                if (method.ReturnType.IsValueType && !IsNullableType(method.ReturnType))
                {
                    Type retType = method.ReturnType;
                    if (retType != typeof(bool))
                    {
                        // in case the returned type is not a boolean
                        // we want to use a nullable version of the type...
                        if (IsNullableType(retType))
                        {
                            Type[] genTypes = retType.GetGenericArguments();
                            if (genTypes.Length > 1) //TODO: should we just ignore it if is it an array greater
                                                     //      than 1?
                                throw new InvalidOperationException();
                            retType = genTypes[0];
                        }

                        retType = ExpressionUtil.GetNullable(retType);
                    }
                    return new BinaryExpression(ExpressionType.AddChecked, left, right, method, retType);
                }
            }

            return new BinaryExpression(ExpressionType.AddChecked, left, right, method, method.ReturnType);
        }

        public static BinaryExpression AddChecked(Expression left, Expression right)
        {
            return AddChecked(left, right, null);
        }

        public static BinaryExpression And(Expression left, Expression right, MethodInfo method)
        {
            CheckLeftRight(left, right);

            // sine both the expressions define the same numeric type or is a boolean
            // we don't have to look for the "op_BitwiseAnd" method...
            if (left.type == right.type &&
                (ExpressionUtil.IsInteger(left.type) || left.type == typeof(bool)))
                return new BinaryExpression(ExpressionType.AddChecked, left, right, left.type);

            if (method == null)
                method = ExpressionUtil.GetOperatorMethod("op_BitwiseAnd", left.type, right.type);

            // ok if even op_BitwiseAnd is not defined we need to throw an exception...
            if (method == null)
                throw new InvalidOperationException();

            return new BinaryExpression(ExpressionType.And, left, right, method, method.ReturnType);
        }

        public static BinaryExpression And(Expression left, Expression right)
        {
            return And(left, right, null);
        }

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

        public static ConstantExpression Constant(object value)
        {
            Type valueType = null;
            if (value != null)
                valueType = value.GetType();
            return Constant(value, valueType);
        }

        public static ConstantExpression Constant(object value, Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (value == null && !IsNullableType(type))
                throw new ArgumentException();

            return new ConstantExpression(value, type);
        }

        public static BinaryExpression Divide(Expression left, Expression right)
        {
            return Divide(left, right, null);
        }

        public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method)
        {
            CheckLeftRight(left, right);

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
            CheckLeftRight(left, right);

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
        #endregion
    }
}
