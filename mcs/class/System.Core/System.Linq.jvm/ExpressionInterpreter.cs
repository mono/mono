using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections.Specialized;
using System.Linq.Expressions;

namespace System.Linq.jvm
{
    class ExpressionInterpreter : ExpressionVisitor
    {
 
        LambdaExpression _exp;

        object[] _args;

        object _value = null;

        public object Value
        {
            get { return _value; }
        }

        public ExpressionInterpreter(object[] args)
        {
            _args = args;
        }


        public void Run(LambdaExpression exp)
        {
            _exp = exp;
            Visit(exp.Body);
        }

		protected override void Visit (Expression expression)
		{
			if (expression == null) {
				return;
			}
			if (expression.NodeType == ExpressionType.Power) {
				VisitBinary ((BinaryExpression) expression);
			}
			else {
				base.Visit (expression);
			}
		}

        private void VisitCoalesce(BinaryExpression binary)
        {
            Visit(binary.Left);
            if (_value == null)
            {
                Visit(binary.Right);
            }
        }

        private void VisitAndAlso(BinaryExpression binary)
        {
            object right = null;
            object left = null;

            Visit(binary.Left);
            right = _value;

            if (right == null || ((bool)right))
            {
                Visit(binary.Right);
                left = _value;
            }

            _value = Math.And(right, left);
            
        }

        private void VisitOrElse(BinaryExpression binary)
        {
            object right = null;
            object left = null;

            Visit(binary.Left);
            right = _value;

            if (right == null || !((bool)right))
            {
                Visit(binary.Right);
                left = _value;
            }
            _value = Math.Or(right, left);
        }

        private void VisitCommonBinary(BinaryExpression binary)
        {
            try
            {
                Visit(binary.Left);
                object left = _value;
                Visit(binary.Right);
                object right = _value;

                if (binary.Method != null)
                {
                    _value = binary.Method.Invoke(null, new object[] { left, right });
                    return;
                }

                TypeCode tc = Type.GetTypeCode(binary.Type);

                switch (binary.NodeType)
                {
                    case ExpressionType.ArrayIndex:
                        _value = ((Array)left).GetValue((int)right);
                        return;
                    case ExpressionType.Equal:
						if (typeof(ValueType).IsAssignableFrom(binary.Right.Type))
							_value = ValueType.Equals (left, right);
						else
							_value = (left == right);
                        return;
                    case ExpressionType.NotEqual:
                        _value = !left.Equals(right);
                        return;
                    case ExpressionType.LessThan:
                        _value = (Comparer.Default.Compare(left, right) < 0);
                        return;
                    case ExpressionType.LessThanOrEqual:
                        _value = (Comparer.Default.Compare(left, right) <= 0);
                        return;
                    case ExpressionType.GreaterThan:
                        _value = (Comparer.Default.Compare(left, right) > 0);
                        return;
                    case ExpressionType.GreaterThanOrEqual:
                        _value = (Comparer.Default.Compare(left, right) >= 0);
                        return;
                    case ExpressionType.RightShift:
                        _value = Math.RightShift(left, Convert.ToInt32(right), tc);
                        return;
                    case ExpressionType.LeftShift:
                        _value = Math.LeftShift(left, Convert.ToInt32(right), tc);
                        return;
                    default:
                        _value = Math.Evaluate(left, right, binary.Type, binary.NodeType);
                        break;

                }
            }
            catch (OverflowException)
            {
                throw;
            }
            catch (Exception e)
            {

                throw new NotImplementedException(
                    string.Format(
                    "Interpriter for BinaryExpression with NodeType {0} is not implimented",
                    binary.NodeType),
                    e);
            }
        }

        protected override void VisitBinary(BinaryExpression binary)
        {
            switch (binary.NodeType)
            {
                case ExpressionType.AndAlso:
                    VisitAndAlso(binary);
                    return;
                case ExpressionType.OrElse:
                    VisitOrElse(binary);
                    return;
                case ExpressionType.Coalesce:
                    VisitCoalesce(binary);
                    return;
                default:
                    VisitCommonBinary(binary);
                    break;
            }
        }

        protected override void VisitUnary(UnaryExpression unary)
        {
			if (unary.NodeType == ExpressionType.Quote) 
			{
				_value = unary.Operand;
				return;
			}
            Visit(unary.Operand);
            object o = _value;

            if (unary.Method != null)
            {
                _value = unary.Method.Invoke(null, new object[] { o });
                return;
            }

            switch (unary.NodeType)
            {
                case ExpressionType.TypeAs:
                    if (o == null || !Math.IsType(unary.Type, o))
                    {
                        _value = null;
                    }
                    return;
                case ExpressionType.ArrayLength:
                    _value = ((Array)o).Length;
                    return;
                case ExpressionType.Negate:
                    _value = Math.Negete(o, Type.GetTypeCode(unary.Type));
                    return;
                case ExpressionType.NegateChecked:
                    _value = Math.NegeteChecked(o, Type.GetTypeCode(unary.Type));
                    return;
                case ExpressionType.Not:
					if (unary.Type == typeof(bool))
						_value = ! Convert.ToBoolean(o);
					else
						_value = ~Convert.ToInt32(o);
                    return;
                case ExpressionType.UnaryPlus:
                    _value = o;
                    return;
                case ExpressionType.Convert:
					_value = Math.ConvertToTypeUnchecked (o, unary.Operand.Type, unary.Type);					
                    return;
                case ExpressionType.ConvertChecked:
                    _value = Math.ConvertToTypeChecked(o,unary.Operand.Type, unary.Type);
                    return;				
            }
            throw new NotImplementedException(
                string.Format(
                "Interpriter for UnaryExpression with NodeType {0} is not implimented",
                unary.NodeType));

        }

        protected override void VisitNew(NewExpression nex)
        {
            if (nex.Constructor == null)
            {
                _value = System.Activator.CreateInstance(nex.Type);
            }
            else
            {
                object[] parameters = VisitListExpressions(nex.Arguments);
                _value = nex.Constructor.Invoke(parameters);
            }
        }

        protected override void VisitTypeIs(TypeBinaryExpression type)
        {
            Visit(type.Expression);
            _value = Math.IsType(type.TypeOperand, _value);
        }

        private void VisitMemberInfo(MemberInfo mi)
        {
            object o = _value;
            switch (mi.MemberType)
            {
                case MemberTypes.Field:
                    _value = ((FieldInfo)mi).GetValue(o);
                    return;
                case MemberTypes.Property:
                    _value = ((PropertyInfo)mi).GetValue(o, null);
                    return;
            }

            throw new NotImplementedException(
                string.Format(
                "Interpriter for MemberInfo with MemberType {0} is not implimented",
                mi.MemberType));
        }

        protected override void VisitMemberAccess(MemberExpression member)
        {
            Visit(member.Expression);
            VisitMemberInfo(member.Member);
        }


        protected override void VisitNewArray(NewArrayExpression newArray)
        {
            switch (newArray.NodeType)
            {
                case ExpressionType.NewArrayInit:
                    VisitNewArrayInit(newArray);
                    return;
                case ExpressionType.NewArrayBounds:
                    VisitNewArrayBounds(newArray);
                    return;
            }
            throw new NotImplementedException(
               string.Format(
               "Interpriter for VisitNewArray with NodeType {0} is not implimented",
               newArray.NodeType));
        }

        private void VisitNewArrayBounds(NewArrayExpression newArray)
        {
            int[] lengths = new int[newArray.Expressions.Count];
            for (int i = 0; i < lengths.Length; i++)
            {
                Visit(newArray.Expressions[i]);
                lengths[i] = (int)_value;
            }
            _value = Array.CreateInstance(newArray.Type.GetElementType(), lengths);

        }

        private void VisitNewArrayInit(NewArrayExpression newArray)
        {
            Array arr = Array.CreateInstance(
                        newArray.Type.GetElementType(),
                        newArray.Expressions.Count);
            for (int i = 0; i < arr.Length; i++)
            {
                Visit(newArray.Expressions[i]);
                arr.SetValue(_value, i);
            }
            _value = arr;
        }

        protected override void VisitConditional(ConditionalExpression conditional)
        {
            Visit(conditional.Test);
            if ((bool)_value)
            {
                Visit(conditional.IfTrue);
            }
            else
            {
                Visit(conditional.IfFalse);
            }
        }

        protected override void VisitMethodCall(MethodCallExpression call)
        {
            if (call.Object != null)
            {
                Visit(call.Object);
            }
            object callObject = _value;
            object[] arguments = VisitListExpressions(call.Arguments);
            _value = call.Method.Invoke(callObject, arguments);
        }

        protected override void VisitParameter(ParameterExpression parameter)
        {
            for (int i = 0; i < _exp.Parameters.Count; i++)
            {
                if (_exp.Parameters[i].Name.Equals(parameter.Name))
                {
                    _value = _args[i];
                    return;
                }
            }
            _value = null;
        }

        protected override void VisitConstant(ConstantExpression constant)
        {
            _value = constant.Value;
        }

        protected override void VisitInvocation(InvocationExpression invocation)
        {
            Visit(invocation.Expression);
            object o = _value;
            object[] arguments = VisitListExpressions(invocation.Arguments);
            _value = ((Delegate)o).DynamicInvoke(arguments);
        }

        protected override void VisitMemberListBinding(MemberListBinding binding)
        {
            object o = _value;
            try
            {
                VisitMemberInfo(binding.Member);
                base.VisitMemberListBinding(binding);
            }
            finally
            {
                _value = o;
            }
        }

        protected override void VisitElementInitializer(ElementInit initializer)
        {
            object o = _value;
            try
            {
                object[] arguments =
                    VisitListExpressions(initializer.Arguments);
                initializer.AddMethod.Invoke(o, arguments);

            }
            finally
            {
                _value = o;
            }
        }

        protected override void VisitMemberMemberBinding(MemberMemberBinding binding)
        {
            object o = _value;
            try
            {
                VisitMemberInfo(binding.Member);
                base.VisitMemberMemberBinding(binding);
            }
            finally
            {
                _value = o;
            }
        }

        protected override void VisitMemberAssignment(MemberAssignment assignment)
        {
            object o = _value;
            try
            {
                Visit(assignment.Expression);
                switch (assignment.Member.MemberType)
                {
                    case MemberTypes.Field:
                        ((FieldInfo)assignment.Member).SetValue(o, _value);
                        return;
                    case MemberTypes.Property:
                        ((PropertyInfo)assignment.Member).SetValue(o, _value, null);
                        return;
                }
                throw new NotImplementedException(
                    string.Format(
                    "Interpriter for MemberExpression with MemberType {0} is not implimented",
                    assignment.Member.MemberType));
            }
            finally
            {
                _value = o;
            }
        }

		protected override void VisitLambda (LambdaExpression lambda) {
			_value = lambda.Compile ();
		}

        private object[] VisitListExpressions(ReadOnlyCollection<Expression> collection)
        {
            object o = _value;
            try
            {
                object[] results = new object[collection.Count];
                for (int i = 0; i < results.Length; i++)
                {
                    Visit(collection[i]);
                    results[i] = _value;
                }

                return results;
            }
            finally
            {
                _value = o;
            }
        }

    }

}
