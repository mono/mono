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


        public static ListInitExpression ListInit(NewExpression newExpression, params Expression[] initializers)
        {
            if (initializers == null)
                throw new ArgumentNullException("inizializers");

            return ListInit(newExpression, initializers.ToReadOnlyCollection<Expression>());
        }

        public static ListInitExpression ListInit(NewExpression newExpression, IEnumerable<Expression> initializers)
        {
            if (newExpression == null)
                throw new ArgumentNullException("newExpression");
            if (initializers == null)
                throw new ArgumentNullException("inizializers");

            return new ListInitExpression(newExpression, initializers.ToReadOnlyCollection<Expression>());
        }

        public static MemberInitExpression MemberInit(NewExpression newExpression, IEnumerable<MemberBinding> bindings)
        {
            if (newExpression == null)
                throw new ArgumentNullException("newExpression");

            if (bindings == null)
                throw new ArgumentNullException("bindings");

            return new MemberInitExpression(newExpression, bindings.ToReadOnlyCollection<MemberBinding>());
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