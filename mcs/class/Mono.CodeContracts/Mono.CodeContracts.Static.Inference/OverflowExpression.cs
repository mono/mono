using System;
using System.Collections.Generic;
using Mono.CodeContracts.Inference.Interface;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Proving;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.ControlFlow;

namespace Mono.CodeContracts.Static.Inference 
{
	public class OverflowExpressionControl<Var> : IBoxedExpressionController
	{
		#region Fields
		
		private BoxedExpression LocalResult;
		private readonly Dictionary<BoxedExpression, OverflowExpressionControl<Var>.Condition> Expressions;
		private readonly IFactForOverflow<BoxedExpression> Overflow;
		private readonly APC PC;
		private readonly LazyEval<BoxedExpression> ZeroExp;
		
		#endregion
		
		#region Current Code Condition Enum
		
		private enum Condition
	    {
	      Overflow,
	      DoesNotOverflow
	    }
		
		#endregion
		
		#region Constructor
		
		public OverflowExpressionControl (APC PC, LazyEval<BoxedExpression> ZeroExp, IFactQuery<BoxedExpression, Var> facts)
		{
			this.LocalResult = null;
			this.PC = PC; 
			this.ZeroExp = ZeroExp;
			this.Overflow = (IFactForOverflow<BoxedExpression>) new FactForOverflow<Var>(facts);
		}
		
		#endregion
		
		#region Interface Methods
		
		private void Unary(UnaryOperator unaryOp, BoxedExpression arg, BoxedExpression source)
		{
			arg.Dispatch((IBoxedExpressionController) this);
      		if (this.LocalResult != null)
        	ReturnOkResult(BoxedExpression.Unary(unaryOperator, this.PartialResult));
      		ReturnNotOkResult(original);			
		}
		
		private void Binary(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			if(CheckComparison(binaryOp, left, right, source) || 
			   CheckDevidingByConstant(binaryOp, left, right, source)||
			   (CheckSubstraction(binaryOp, left, right, source) || CheckAddition(binaryOp, left, right, source)) || 
			   CheckMovingSubtractionOnComparison(binaryOp, left, right, source) || 
			   CheckSumRewriting(binaryOp, left, right, source) || 
			   CheckRemovingAddInComparison(binaryOp, left, right, source) || 
			   CheckMovingExpressionAroundComparison(binaryOp, left, right, source) || 
			   CheckAltAddAndSub(binaryOp, left, right, source) || 
			   CheckRemoveWeakInequalities(binaryOp, left, right, source))
        	return;
      		ReturnNotOkResult(source);
		}
		
		private void SizeOf<Type>(Type type, int size, BoxedExpression source);
		
		private void ArrayIndex(Type type, BoxedExpression array, BoxedExpression itemIndex, BoxedExpression source);
		
		private void Result<Type>(Type type, BoxedExpression source);
		
		private void ReturnedValue<Type>(Type type, BoxedExpression expression, BoxedExpression source);
		
		private void Variable(object var, PathElement[] path, BoxedExpression source);
		
		private void Constant<Type>(Type type, object value, BoxedExpression source);
		
		#endregion
		
		#region Checkers 
		
		private bool CheckComparison(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			 if (BinaryOperatorExtensions.IsComparisonBinaryOperator(binaryOp))
		     {
				BoxedExpression _left;
		     	left.Dispatch((IBoxedExpressionVisitor) this);
				
				if ((left1 = this.PartialResult) != null)
				{
					BoxedExpression _right;
					right.Dispatch((IBoxedExpressionVisitor) this);
          			if ((_right = this.LocalResult) != null)
			        {
			            ReturnOkResult(BoxedExpression.Binary(binaryOp, _left, _right, source.UnderlyingVariable));
			            return true;
			        }
				}
		     }
		     this.LocalResult = null;
		     return false;
		}
		
		private bool CheckDevidingByConstant(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			int i;
			
			if((BoxedExpressionExtensions.IsConstantInt(right, out i) && i != 0) && 
			  ((binaryOp == BinaryOperator.Rem || binaryOp == BinaryOpertor.Rem_Un)||
			 	binaryOp == BinaryOpertor.Div || binaryOp == BinaryOpertor.Div_Un))
			{
				BoxedExpression _left;
				left.Dispatch((IBoxedExpressionController) this);
		        if ((_left = this.LocalResult) != null)
		        {
		          ReturnOkResult(BoxedExpression.Binary(binaryOp, _left, right, source.UnderlyingVariable));
		          return true;
		        }
			}
			this.LocalResult = null;
      		return false;
		}
		
		private bool CheckSubstraction(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			if(binaryOp == BinaryOperator.Sub ||binaryOp == BinaryOperator.Sub_Ovf || binaryOp == BinaryOperator.Sub_Ovf_Un)
			{
				
				this.ReturnOkResult(left);
        		this.ReturnOkResult(right);
        		this.ReturnOkResult(original);
				return true;
			} 
			else
			{
				this.LocalResult = null;
				return false;
			}
		}
		
		private bool CheckAddition(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			if(binaryOp == BinaryOperator.Add ||binaryOp == BinaryOperator.Add_Ovf || binaryOp == BinaryOperator.Add_Ovf_Un)
			{
				this.ReturnOkResult(left);
        		this.ReturnOkResult(right);
        		this.ReturnOkResult(original);
        		return true;
			} 
			else
			{
				this.LocalResult = null;
				return false;
			}
		}
		
		private bool CheckMovingSubtractionOnComparison(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			int i;
      		BinaryOperator _binaryOp;
	      	BoxedExpression _left;
	      	BoxedExpression _right;
			
      		if (OperatorExtensions.IsComparisonBinaryOperator(binaryOp) && 
			    BoxedExpressionExtensions.IsConstantInt(right, out i) && 
			    (i == 0 && left.IsBinaryExpression(out binaryOp, out _left, out _right)) && 
			    (bop1 == BinaryOperator.Sub || binaryOp == BinaryOperator.Sub_Ovf || binaryOp == BinaryOperator.Sub_Ovf_Un))
			{
				_left.Dispatch((IBoxedExpressionController) this);
				BoxedExpression boxedExpression_1;
				if((boxedExpression_1 = this.LocalResult) != null)
				{
					ReturnOkResult(boxedExpression_1);
					_right.Dispatch((IBoxedExpressionController) this);
					BoxedExpression boxedExpression_2;
					
					if ((boxedExpression_2 = this.LocalResult) != null)
			        {
						ReturnOkResult(boxedExpression_2);
			            ReturnOkResult(BoxedExpression.Binary(_binaryOp, boxedExpression_1, boxedExpression_2, source.UnderlyingVariable));
			            return true;
			        }
				}
			}
      
      		this.LocalResult = (BoxedExpression) null;
      		return false;
		}
		
		private bool CheckSumRewriting(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			int i;
      		BinaryOperator _binaryOp;
	      	BoxedExpression _left;
	      	BoxedExpression _right;
			
			if ((binaryOp == BinaryOperator.Div || binaryOp == BinaryOperator.Div_Un) && 
			    left.IsBinaryExpression(out _binaryOp, out _left, out _right) && 
			    (_binaryOp == BinaryOperator.Add || _binaryOp == BinaryOperator.Add_Ovf || _binaryOp == BinaryOperator.Add_Ovf_Un) &&
			    (BoxedExpressionExtensions.IsConstantInt(right, out i) && i == 2))
			{
				_left.Dispatch((IBoxedExpressionController) this);
        		BoxedExpression boxedExpression_1;
				if ((boxedExpression_1 = this.LocalResult) != null)
				{
					_right.Dispatch((IBoxedExpressionController) this);
          			BoxedExpression boxedExpression_2;
					if ((boxedExpression_2 = this.LocalResult) != null)
					{
						_left = (BoxedExpression) null;
						BoxedExpression boxedExpression_3 = BoxedExpression.Binary(BinaryOperator.Sub, boxedExpression_2, boxedExpression_1, (object) null);
						if(!this.Overflow.Overflow(PC, boxedExpression_3))
						{
							return this.ReturnOkResult(BoxedExpression.Binary(BinaryOperator.Add, boxedExpression_1, BoxedExpression.Binary(binaryOp, boxedExpression_3, right, (object) null), (object) null));
						}
						BoxedExpression boxedExpression_4 = BoxedExpression.Binary(BinaryOperator.Sub, boxedExpression_1, boxedExpression_2, (object) null);
						if (!this.OverflowOracle.CanUnderflow(this.PC, boxedExpression4))
						{
							return this.ReturnOkResultt(BoxedExpression.Binary(BinaryOperator.Add, boxedExpression_2, BoxedExpression.Binary(binaryOp, boxedExpression_4, right, (object) null), (object) null));	
						}
					}
				}
			}
      		return this.NotifyNotOk(source);
		}
		
		private bool CheckRemovingAddInComparison(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			BinaryOperator _binaryOp;
			BoxedExpression _left;
	      	BoxedExpression _right;
			
			if(OperatorExtensions.IsComparisonBinarayOperator(binaryOp) && left.IsBinaryExpression(out _binaryOp,out _left, out _right) && (_binaryOp == BinaryOperator.Add || _binaryOp == BinaryOperator.Add_Ovf || _binaryOp == BinaryOperator.Add_Ovf_Un))
			{
				_left.Dispatch((IBoxedExpressionController) this);
				BoxedExpression boxedExpression_1;
				if ((boxedExpression_1 = this.LocalResult) != null)
				{
					_right.Dispatch((IBoxedExpressionController) this);
          			BoxedExpression boxedExpression_2;
					if ((boxedExpression_2 = this.LocalResult) != null)
					{
						right.Dispatch((IBoxedExpressionController) this);
						BoxedExpression _left_2;
						if ((_left_2 = this.LocalResult) != null)
						{
							_left = (BoxedExpression) null;
              				_right = (BoxedExpression) null;
							right = (BoxedExpression) null;
							BoxedExpression boxedExpression_3 = BoxedExpression.Binary(BinaryOperator.Sub, _left_2, boxedExpression_2, (object) null);
							if (!this.Overflow.Underflow(this.PC, boxedExpression3_))
							{
								return this.ReturnOkResult(BoxedExpression.Binary(binaryOp, boxedExpression_1, boxedExpression_3, (object) null));
							}
							BoxedExpression boxedExpression_4 = BoxedExpression.Binary(BinaryOperator.Sub, _left_2, boxedExpression_1, (object) null);
							if (!this.Overflow.Underflow(this.PC, boxedExpression_4))
							{
								return this.ReturnOkResult(BoxedExpression.Binary(binaryOp, boxedExpression_2, boxedExpression_4, (object) null));
							}
						}
					}
				}
			}
			return this.ReturnNotOkResult(source);
		}
		
		private bool CheckMovingExpressionAroundComparison(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			BinaryOperator _binaryOp;
		    BoxedExpression _left;
		    BoxedExpression _right;
			if (OperatorExtensions.IsComparisonBinaryOperator(binaryOp) && left.IsBinaryExpression(out _binaryOp, out _left, out _right) && (_binaryOp == BinaryOperator.Add || _binaryOp == BinaryOperator.Add_Ovf || _binaryOp == BinaryOperator.Add_Ovf_Un))
			{
				BoxedExpression _left_2 = BoxedExpression.Binary(BinaryOperator.Sub, left, right, (object) null);
				_left_2.Dispatch((IBoxedExpressionController) this);
				if (this.LocalResult != null)
				{
					return this.ReturnOkResult(BoxedExpression.Binary(binaryOp, _left_2, this.ZeroExp.Value, (object) null));
				}
			}
      		return this.ReturnNotOkResult(source);
		}
		
		private bool CheckAltAddAndSub(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			BinaryOperator _binaryOp;
			BoxedExpression _left;
			BoxedExpression _right;
			if ((binrayOp == BinaryOperator.Sub || binrayOp == BinaryOperator.Sub_Ovf || binaryOp == BinaryOperator.Sub_Ovf_Un) && left.IsBinaryExpression(out _binaryOp, out _left, out _right) && (binaryOp == BinaryOperator.Add || binaryOp == BinaryOperator.Add_Ovf || binaryOp == BinaryOperator.Add_Ovf_Un))
			{
				_left.Dispatch((IBoxedExpressionController) this);
				if (this.LocalResult != null)
				{
					_right.Dispatch((IBoxedExpressionController) this);
					if (this.PartialResult != null)
					{
						right.Dispatch((IBoxedExpressionController) this);
						if(this.LocalResult != null)
						{
							BoxedExpression boxedExpression_1 = BoxedExpression.Binary(BinaryOperator.Sub, _left, right, (object) null);
							if (!this.Overflow.Underflow(this.PC, boxedExpression_1))
							{
								BoxedExpression.Binary(BinaryOperator.Add, BoxedExpression_1);
								if (this.LocalResult != null) return this.ReturnOkResult(this.LocalResult);
							}
							BoxedExpression boxedExpression_2 = BoxedExpression.Binary(BinaryOperator.Sub, _right, rigt, (object) null);
              				if (!this.Overflow.Underflow(this.PC, boxedExpression_2))
							{
								BoxedExpression.Binary(BinaryOperator.Add, boxedExpression_2, _left, (object) null).Dispatch((IBoxedExpressionController) this);
                				if (this.LocalResult != null) return this.ReturnOkResult(this.LocalResult);
							}
						}
					}
				}
			}
			return this.ReturnNotOkResult(source);
		}
		
		private bool CheckRemoveWeakInequalities(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right, BoxedExpression source)
		{
			 if (OperatorExtensions.IsBooleanBinaryOperator(binaryOp))
		     {
		        if (binaryOp == BinaryOperator.Cle)
		          return this.TryRemoveWeakInequalitiesInternal(binaryOp, left, right);
		        if (binaryOp == BinaryOperator.Cge)
		          return this.TryRemoveWeakInequalitiesInternal(BinaryOperator.Cle, right, left);
		     }
		     return this.ReturnNotOk(original);
		}
		
		private bool TryRemoveWeakInequalitiesInternal(BinaryOperator binaryOp, BoxedExpression left, BoxedExpression right)
	    {
	      if (binaryOp == BinaryOperator.Cle)
	      {
	        left.Dispatch((IBoxedExpressionController) this);
	        BinaryOperator _binaryOp;
	        BoxedExpression _left;
	        BoxedExpression _right;
	        int i;
	        if (left.IsBinaryExpression(out _binaryOp, out _left, out _right) && (_binaryOp == BinaryOperator.Add || _binaryOp == BinaryOperator.Add_Ovf || _binaryOp == BinaryOperator.Add_Ovf_Un) && BoxedExpressionExtensions.IsConstantInt(_right, out i))
	        {
	          _left.Dispatch((IBoxedExpressionController) this);
	          if (this.LocalResult != null)
	          {
	            BoxedExpression.Binary(BinaryOperator.Clt, this.PartialResult, right, (object) null).Dispatch((IBoxedExpressionController) this);
	            if (this.LocalResult != null)
	              return this.NotifyOkAndSetResult(this.LocalResult);
	          }
	        }
	      }
	      return false;
	    }
		
		#endregion
		
		#region Checekers result returners
		
		private bool ReturnOkResult(BoxedExpression expression)
	    {
	      	this.LocalResult = expression;
	      	this.Expressions[expression] = (OverflowExpressionControl<Var>.Condition) 1;
	      	return true;
	    }
		
		private bool ReturnNotOkResult(BoxedExpression expression)
	    {
	       	this.LocalResult = null;
      		this.Expressions[exp] = (OverflowExpressionControl<Var>.State) 0;
      		return false;
	    }
		
		#endregion
		
	}
}

