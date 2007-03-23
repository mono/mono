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
            //TODO:
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
            //TODO:
            return type;
        }

        internal static bool IsNullableType(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");
            if (type.IsGenericType)
            {
                //TODO: check this...
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
            {
                method = ExpressionUtil.GetMethod("op_Addition", new Type[] { left.type, right.type });
            }

            if (method != null)
            {
                if (method.ReturnType == null) // it returns a void
                    throw new ArgumentException();
                if (!method.IsStatic)
                    throw new ArgumentException();
                ParameterInfo[] pars = method.GetParameters();
                if (pars.Length != 2)
                    throw new ArgumentException();
            }

            return new BinaryExpression(ExpressionType.Add, left, right, method, method.ReturnType);
        }

        public static BinaryExpression Add(Expression left, Expression right)
        {
            return Add(left, right, null);
        }

        public static BinaryExpression AddChecked(Expression left, Expression right, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression AddChecked(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression And(Expression left, Expression right, MethodInfo method)
        {
            CheckLeftRight(left, right);

            throw new NotImplementedException();
        }

        public static BinaryExpression And(Expression left, Expression right)
        {
            return And(left, right, null);
        }

        public static BinaryExpression AndAlso(Expression left, Expression right, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression AndAlso(Expression left, Expression right)
        {
            return AndAlso(left, right, null);
        }

        public static MethodCallExpression ArrayIndex(Expression array, IEnumerable<Expression> indexes)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression ArrayIndex(Expression array, Expression index)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression ArrayIndex(Expression array, params Expression[] indexes)
        {
            throw new NotImplementedException();
        }

        public static UnaryExpression ArrayLength(Expression array)
        {
            throw new NotImplementedException();
        }

        public static MemberAssignment Bind(MemberInfo member, Expression expression)
        {
            throw new NotImplementedException();
        }

        public static MemberAssignment Bind(MethodInfo propertyAccessor, Expression expression)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression Call(MethodInfo method, params Expression[] arguments)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, params Expression[] arguments)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression Call(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression Call(Expression instance, string methodName, Type[] typeArguments, params Expression[] arguments)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression Call(Type type, string methodName, Type[] typeArguments, params Expression[] arguments)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression CallVirtual(Expression instance, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression CallVirtual(Expression instance, MethodInfo method, params Expression[] arguments)
        {
            throw new NotImplementedException();
        }

        public static MethodCallExpression CallVirtual(Expression instance, MethodInfo method, IEnumerable<Expression> arguments)
        {
            throw new NotImplementedException();
        }

        public static UnaryExpression Cast(Expression expression, Type type)
        {
            throw new NotImplementedException();
        }

        public static UnaryExpression Cast(Expression expression, Type type, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression Coalesce(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression Coalesce(Expression left, Expression right, MethodInfo conversion)
        {
            throw new NotImplementedException();
        }

        public static ConditionalExpression Condition(Expression test, Expression ifTrue, Expression ifFalse)
        {
            throw new NotImplementedException();
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

        public static UnaryExpression Convert(Expression expression, Type type)
        {
            throw new NotImplementedException();
        }

        public static UnaryExpression Convert(Expression expression, Type type, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static UnaryExpression ConvertChecked(Expression expression, Type type)
        {
            throw new NotImplementedException();
        }

        public static UnaryExpression ConvertChecked(Expression expression, Type type, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression Divide(Expression left, Expression right)
        {
            return Divide(left, right);
        }

        public static BinaryExpression Divide(Expression left, Expression right, MethodInfo method)
        {
            CheckLeftRight(left, right);

            throw new NotImplementedException();
        }

        public static BinaryExpression Equal(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression Equal(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression ExclusiveOr(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression ExclusiveOr(Expression left, Expression right, MethodInfo method)
        {
            throw new NotImplementedException();
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

        public static BinaryExpression GreaterThan(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression GreaterThan(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression GreaterThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static InvocationExpression Invoke(Expression expression, IEnumerable<Expression> arguments)
        {
            throw new NotImplementedException();
        }

        public static InvocationExpression Invoke(Expression expression, params Expression[] arguments)
        {
            throw new NotImplementedException();
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, params ParameterExpression[] parameters)
        {
            throw new NotImplementedException();
        }

        public static Expression<TDelegate> Lambda<TDelegate>(Expression body, IEnumerable<ParameterExpression> parameters)
        {
            throw new NotImplementedException();
        }

        public static LambdaExpression Lambda(Expression body, params ParameterExpression[] parameters)
        {
            throw new NotImplementedException();
        }

        public static LambdaExpression Lambda(Type delegateType, Expression body, params ParameterExpression[] parameters)
        {
            throw new NotImplementedException();
        }

        public static LambdaExpression Lambda(Type delegateType, Expression body, IEnumerable<ParameterExpression> parameters)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression LeftShift(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression LeftShift(Expression left, Expression right, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression LessThan(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression LessThan(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression LessThanOrEqual(Expression left, Expression right)
        {
            throw new NotImplementedException();
        }

        public static BinaryExpression LessThanOrEqual(Expression left, Expression right, bool liftToNull, MethodInfo method)
        {
            throw new NotImplementedException();
        }

        public static LiftExpression Lift(Expression expression, IEnumerable<ParameterExpression> parameters, IEnumerable<Expression> arguments)
        {
            throw new NotImplementedException();
        }

        public static LiftExpression Lift(Expression expression, ParameterExpression parameter, Expression argument)
        {
            throw new NotImplementedException();
        }

        public static LiftExpression LiftEqual(Expression expression, IEnumerable<ParameterExpression> parameters, IEnumerable<Expression> arguments)
        {
            throw new NotImplementedException();
        }

        public static LiftExpression LiftFalse(Expression expression, IEnumerable<ParameterExpression> parameters, IEnumerable<Expression> arguments)
        {
            throw new NotImplementedException();
        }

        public static LiftExpression LiftFalse(Expression expression, ParameterExpression parameter, Expression argument)
        {
            throw new NotImplementedException();
        }

        public static LiftExpression LiftNotEqual(Expression expression, IEnumerable<ParameterExpression> parameters, IEnumerable<Expression> arguments)
        {
            throw new NotImplementedException();
        }

        public static LiftExpression LiftTrue(Expression expression, IEnumerable<ParameterExpression> parameters, IEnumerable<Expression> arguments)
        {
            throw new NotImplementedException();
        }

        public static LiftExpression LiftTrue(Expression expression, ParameterExpression parameter, Expression argument)
        {
            throw new NotImplementedException();
        }

        public static MemberListBinding ListBind(MemberInfo member, params Expression[] initializers)
        {
            throw new NotImplementedException();
        }

        public static MemberListBinding ListBind(MemberInfo member, IEnumerable<Expression> initializers)
        {
            throw new NotImplementedException();
        }

        public static MemberListBinding ListBind(MethodInfo propertyAccessor, IEnumerable<Expression> initializers)
        {
            throw new NotImplementedException();
        }

        public static ListInitExpression ListInit(NewExpression newExpression, params Expression[] initializers)
        {
            if (initializers == null)
                throw new ArgumentNullException("inizializers");

            return ListInit(newExpression, ExpressionUtil.GetReadOnlyCollection (initializers));
        }

        public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers)
        {
            if (newExpression == null)
                throw new ArgumentNullException("newExpression");
            if (initializers == null)
                throw new ArgumentNullException("inizializers");

            return new ListInitExpression(newExpression, ExpressionUtil.GetReadOnlyCollection(initializers));
        }

        public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings)
        {
            if (newExpression == null)
                throw new ArgumentNullException("newExpression");

            if (bindings == null)
                throw new ArgumentNullException("bindings");

            return new MemberInitExpression(newExpression, ExpressionUtil.GetReadOnlyCollection(bindings));
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

        public static UnaryExpression Quote(Expression expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            //TODO: is this right?
            return new UnaryExpression(ExpressionType.Quote, expression, expression.GetType());
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