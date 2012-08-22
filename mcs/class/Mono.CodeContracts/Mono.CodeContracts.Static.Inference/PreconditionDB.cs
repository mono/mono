using System;
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Proving;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Inference
{
	class PreconditionDB
	{
		#region ComparisonOutputType enum
		
		private enum ComparisonOutputType
		{
			Top,
			LessEqual,
			Equal,
			GreaterEqual,
		}
		
		#endregion
		
		#region Fileds
		
		private readonly List<Tuple<BoxedExpression, string, ProofOutcome, List<ProofObligation>>> inferred;
		private Dictionary<BoxedExpression, List<ProofObligation>> preconditions;
		
		#endregion
		
		#region Constructor
		
		public PreconditionDB ()
		{
			this.inferred = new List<Tuple<BoxedExpression, string, ProofOutcome, List<ProofObligation>>> ();
		}
		
		#endregion
		
		#region Methods 
		
		public bool TryLookUp (ProofObligation obligation, BoxedExpression expression, out ProofOutcome outcome)
		{
			string s = ((object)expression).ToString ();
			foreach (Tuple<BoxedExpression, string, ProofOutcome, List<ProofObligation>> item in this.inferred) {
				if (item.Item2 == s) {
					item.Item4.Add (obligation);
					outcome = item.Item4;
					return true;
				}
			}
			outcome = ProofOutcome.Top;
			return false;
		}
		
		public List<uint> CausesFor (BoxedExpression expression)
		{
			if (this.preconditions == null || expression == null)
				return (List<uint>)null;
			List<ProofObligation> list;
			if (this.preconditions.TryGetValue (expression, out list))
				return Enumerable.ToList<uint> (Enumerable.Distinct<uint> ((IEnumerable<uint>)list.ConvertAll<uint> ((Converter<ProofObligation, uint>)(obl => obl.ID))));
			else
				return (List<uint>)null;
		}
		
		private static Dictionary<BoxedExpression, List<ProofObligation>> RemoveCoveringPremises (Dictionary<BoxedExpression, List<ProofObligation>> original)
		{
			Dictionary<BoxedExpression, List<ProofObligation>> dictionary1 = new Dictionary<BoxedExpression, List<ProofObligation>> ();
			return dictionary1;
		}
		
		private Dictionary<BoxedExpression, List<ProofObligation>> ComputeMinimalWithSameUpperBound (Dictionary<BoxedExpression, List<ProofObligation>> inferred)
		{
			Dictionary<BoxedExpression, List<ProofObligation>> dictionary = new Dictionary<BoxedExpression, List<ProofObligation>> ();
			return dictionary;
		}
		
		public void Add (ProofObligation obligation, BoxedExpression expression, ProofOutcome outcome)
		{
			this.inferred.Add (new Tuple<BoxedExpression, string, ProofOutcome, List<ProofObligation>> (expression, ((object)expression).ToString (), outcome, new List<ProofObligation> (){obligation}));
			this.preconditions = (Dictionary<BoxedExpression, List<ProofObligation>>)null;
		}
		
		private static List<ProofObligation> JoinAllProofObligation (List<Tuple<BoxedExpression, BoxedExpression, List<ProofObligation>>> list)
		{
			List<ProofObligation> _list = new List<ProofObligation> ();
			foreach (Tuple<BoxedExpression, BoxedExpression, List<ProofObligation>> item in list) {
				_list.AddRange ((IEnumerable<ProofObligation>)item.Item3);
			}
			return _list;
		}
		
		private static Dictionary<BoxedExpression, List<ProofObligation>> RemoveCoveringPremises (Dictionary<BoxedExpression, List<ProofObligation>> original)
		{
			Dictionary<BoxedExpression, List<ProofObligation>> dictionary_1 = new Dictionary<BoxedExpression, List<ProofObligation>> ();
			Dictionary<BoxedExpression, Tuple<BoxedExpression, List<ProofObligation>>> dictionary_2 = new Dictionary<BoxedExpression, Tuple<BoxedExpression, List<ProofObligation>>> (original.Count);
			foreach (KeyValuePair<BoxedExpression, List<ProofObligation>> pair in original) {
				BinaryOperator binaryOp;
				BoxedExpression _left, _right;
				if (pair.Key.IsBinaryExpression (out binaryOp, out _left, out _right) && binaryOp == BinaryOperator.LogicalOr) {
					Tuple<BoxedExpression, List<ProofObligation>> tuple;
					if (dictionary_2.TryGetValue (_right, out tuple)) {
						BoxedExpression _left_2, _right_2;
						if (tuple.Item1.IsBinaryExpression (out binaryOp, out _left_2, out _right_2) && binaryOp == BinaryOperator.LogicalOr) {
							dictionary_2.Remove (_right);
							if (_left.Negate ().Equals ((object)_left_2)) {
								DictionaryExtensions.AddOrUpdate<BoxedExpression, ProofObligation> ((IDictionary<BoxedExpression, List<ProofObligation>>)dictionary_1, _right, pair.Value);
							} else {
								DictionaryExtensions.AddOrUpdate<BoxedExpression, ProofObligation> ((IDictionary<BoxedExpression, List<ProofObligation>>)dictionary_1, pair.Key, pair.Value);
								DictionaryExtensions.AddOrUpdate<BoxedExpression, ProofObligation> ((IDictionary<BoxedExpression, List<ProofObligation>>)dictionary_1, tuple.Item1, tuple.Item2);
							}
						}
					} else {
						dictionary_2 [_right] = new Tuple<BoxedExpression, List<ProofObligation>> (pair.Key, pair.Value);
					}
				} else
					DictionaryExtensions.AddOrUpdate<BoxedExpression, ProofObligation> ((IDictionary<BoxedExpression, List<ProofObligation>>)dictionary_1, pair.Key, pair.Value);
			}
			foreach (KeyValuePair<BoxedExpression, Tuple<BoxedExpression, List<ProofObligation>>> pair in dictionary_2)
				DictionaryExtensions.AddOrUpdate<BoxedExpression, ProofObligation> ((IDictionary<BoxedExpression, List<ProofObligation>>)dictionary_1, pair.Value.Item1, pair.Value.Item2);
			return dictionary_1;
		}
		
		private static Dictionary<BoxedExpression, List<ProofObligation>> ProjectExpressions (List<Tuple<BoxedExpression, string, ProofOutcome, List<ProofObligation>>> elements)
		{
			Dictionary<BoxedExpression, List<ProofObligation>> dictionary = new Dictionary<BoxedExpression, List<ProofObligation>> ();
			foreach (Tuple<BoxedExpression, string, ProofOutcome, List<ProofObligation>> tuple in elements)
				dictionary.Add (tuple.Item1, new List<ProofObligation> ((IEnumerable<ProofObligation>)tuple.Item4));
			return dictionary;
		}

		public IEnumerable<KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>> GeneratePreconditions ()
		{
			if (this.preconditions == null)
				this.preconditions = PreconditionDB.RemoveCoveringPremises (this.ComputeMinimalWithSameUpperBound (PreconditionDB.RemoveStrongerPremises (PreconditionDB.ProjectExpressions (this.inferred))));
			return Enumerable.Distinct<KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>> (Enumerable.Select<KeyValuePair<BoxedExpression, List<ProofObligation>>, KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>> ((IEnumerable<KeyValuePair<BoxedExpression, List<ProofObligation>>>)this.preconditions, (Func<KeyValuePair<BoxedExpression, List<ProofObligation>>, KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>>>)(p => new KeyValuePair<BoxedExpression, IEnumerable<MinimalProofObligation>> (p.Key, Enumerable.Select<ProofObligation, MinimalProofObligation> ((IEnumerable<ProofObligation>)p.Value, (Func<ProofObligation, MinimalProofObligation>)(obl => obl.MinimalProofObligation))))), BoxedExpression.EqualityPairComparer);
		}
		
		private static Dictionary<BoxedExpression, List<ProofObligation>> RemoveStrongerPremises (Dictionary<BoxedExpression, List<ProofObligation>> inferred)
		{
			Dictionary<BoxedExpression, List<ProofObligation>> dictionary = new Dictionary<BoxedExpression, List<ProofObligation>> ();
			foreach (KeyValuePair<BoxedExpression, List<ProofObligation>> pair in inferred) {
				BinaryOperator binaryOp;
				BoxedExpression _left, _right;
				if (pair.Key.IsBinaryExpression (out binaryOp, out _left, out _right) && binaryOp == BinaryOperator.LogicalOr) {
					if (inferred.ContainsKey (_right)) {
						List<ProofObligation> list;
						if (dictionary.TryGetValue (_right, out list) || inferred.TryGetValue (_right, out list)) {
							list.AddRange ((IEnumerable<ProofObligation>)pair.Value);
							continue;
						} else
							continue;
					} else if (BoxedExpression.SimpleSyntacticEquality (_left, _right)) {
						DictionaryExtensions.AddOrUpdate<BoxedExpression, ProofObligation> ((IDictionary<BoxedExpression, List<ProofObligation>>)dictionary, _left, pair.Value);
						continue;
					} else if (PreconditionDB.Implies (_right, _left)) {
						DictionaryExtensions.AddOrUpdate<BoxedExpression, ProofObligation> ((IDictionary<BoxedExpression, List<ProofObligation>>)dictionary, _right, pair.Value);
						continue;
					} else {
						BoxedExpression grouped;
						if (PreconditionDB.CanMergeTogether (_right, _left, out grouped)) {
							DictionaryExtensions.AddOrUpdate<BoxedExpression, ProofObligation> ((IDictionary<BoxedExpression, List<ProofObligation>>)dictionary, grouped, pair.Value);
							continue;
						}
					}
				}
				DictionaryExtensions.AddOrUpdate<BoxedExpression, ProofObligation> ((IDictionary<BoxedExpression, List<ProofObligation>>)dictionary, pair.Key, pair.Value);
			}
			return dictionary;
		}
		
		private static bool Implies (BoxedExpression condition, BoxedExpression premise)
		{
			BinaryOperator binaryOp_1, binaryOp_2;
			BoxedExpression left_1, left_2, right_1, right_2;
			if (!condition.IsBinaryExpression (out binaryOp_1, out left_1, out right_1) || 
				!premise.IsBinaryExpression (out binaryOp_2, out left_2, out right_2) || 
				binaryOp_2 != BinaryOperator.Ceq ||
				(!left_1.Equals ((object)left_2)) ||
				!right_1.Equals ((object)right_2) && 
				(!left_1.Equals ((object)right_2) || !right_1.Equals ((object)left_2))) {
				return false;
			}
			return ArrayExtensions.Contains<BinaryOperator> (new BinaryOperator[5]
	      	{
	        	BinaryOperator.Ceq,
	        	BinaryOperator.Cge,
	        	BinaryOperator.Cge_Un,
	        	BinaryOperator.Cle,
	        	BinaryOperator.Cle_Un
	      	}, binaryOp_1);
		}
		
		private PreconditionDB.ComparisonOutputType SyntacticComparison (BoxedExpression left, BoxedExpression right)
		{
			if (left.Equals ((object)right)) {
				return PreconditionDB.ComparisonOutputType.Equal;
			}
			int num1, num2;
			if (BoxedExpressionExtensions.IsConstantInt (left, out num1) && BoxedExpressionExtensions.IsConstantInt (right, out num2)) {
				if (num1 == num2)
					return PreconditionDB.ComparisonOutputType.Equal;
				if (num1 < num2)
					return PreconditionDB.ComparisonOutputType.LessEqual;
				if (num1 > num2)
					return PreconditionDB.ComparisonOutputType.GreaterEqual;
			}
			BinaryOperator binaryOp_1, binaryOp_2;
			BoxedExpression left_1, left_2, right_1, right_2;
			bool flag_1 = left.IsBinaryExpression (out binaryOp_1, out left_1, out right_1);
			bool flag_2 = right.IsBinaryExpression (out binaryOp_2, out left_2, out right_2);
			if (flag_1 && flag_2 && binaryOp_1 == binaryOp_2) {
				if (binaryOp_1 == BinaryOperator.Add)
					return this.Join (this.SyntacticComparison (left_1, left_2), this.SyntacticComparison (right_1, right_2));
				else
					return PreconditionDB.ComparisonOutputType.Top;
			} else if (flag_1 && left_1.Equals ((object)right)) {
				if (binaryOp_1 == BinaryOperator.Add && BoxedExpressionExtensions.IsConstantInt (right_1, out num1)) {
					if (num1 == 0)
						return PreconditionDB.ComparisonOutputType.Equal;
					if (num1 > 0)
						return PreconditionDB.ComparisonOutputType.GreaterEqual;
					if (num1 < 0)
						return PreconditionDB.ComparisonOutputType.LessEqual;
				}
				return PreconditionDB.ComparisonOutputType.Top;
			} else {
				if (!flag_2 || !left_2.Equals ((object)left) || (binaryOp_2 != BinaryOperator.Add || !BoxedExpressionExtensions.IsConstantInt (right_2, out num2)))
					return PreconditionDB.ComparisonOutputType.Top;
				if (num2 == 0)
					return PreconditionDB.ComparisonOutputType.Equal;
				if (num2 > 0)
					return PreconditionDB.ComparisonOutputType.GreaterEqual;
				return num2 < 0 ? PreconditionDB.ComparisonOutputType.LessEqual : PreconditionDB.ComparisonOutputType.Top;
			}
			return PreconditionDB.ComparisonOutputType.Top;
		}
		
		private PreconditionDB.ComparisonOutputType Join (PreconditionDB.ComparisonOutputType compareLeft, PreconditionDB.ComparisonOutputType compareRight)
		{
			if (compareLeft == compareRight)
				return compareLeft;
			if (compareLeft == PreconditionDB.ComparisonOutputType.Equal)
				return compareRight;
			if (compareRight == PreconditionDB.ComparisonOutputType.Equal)
				return compareLeft;
			else
				return PreconditionDB.ComparisonOutputType.Top;
		}
		
		private bool TryGetSubsumingExpression (List<Tuple<BoxedExpression, BoxedExpression, List<ProofObligation>>> list, out int minIndex)
		{
			BoxedExpression right = list[0].Item1;
			minIndex = 0;
			for(int i = 1; i < list.Count; ++i)
			{
				BoxedExpression left = list[i].Item1;
				switch (this.SyntacticComparison(left, right))
				{
					case PreconditionDB.ComparisonOutputType.Top:
           				 	minIndex = -1;
            				return false;
					case PreconditionDB.ComparisonOutputType.GreaterEqual:
            				right = left;
            				minIndex = i;
            				break;
				}
			}
			return true;
		}
		
		private static bool CanMergeTogether (BoxedExpression condition, BoxedExpression premise, out BoxedExpression joined)
		{
			BinaryOperator binaryOp_1, binaryOp_2;
			BoxedExpression left_1, left_2, right_1, right_2;
			if (condition.IsBinaryExpression(out binaryOp_1, out left_1, out right_1) && premise.IsBinaryExpression(out binaryOp_2, out left_2, out right_2))
			{
				condition = premise = (BoxedExpression) null;
				BoxedExpression left_3 = BoxedExpressionExtensions.StripIfCastOfArrayLength(left_1);        
				BoxedExpression right_3 = BoxedExpressionExtensions.StripIfCastOfArrayLength(right_1);
        		BoxedExpression boxedExpression_1 = BoxedExpressionExtensions.StripIfCastOfArrayLength(left_2);
        		BoxedExpression boxedExpression_2 = BoxedExpressionExtensions.StripIfCastOfArrayLength(right_2);
				if (binaryOp_1 == BinaryOperator.Clt && binaryOp_2 == BinaryOperator.Ceq || binaryOp_1 == BinaryOperator.Ceq && binaryOp_2 == BinaryOperator.Clt)
        		{
		          	if (left_3.Equals((object) boxedExpression_1) && right_3.Equals((object) boxedExpression_2))
		          	{
		            	joined = BoxedExpression.Binary(BinaryOperator.Cle, left_3, right_3, (object) null);
		            	return true;
		          	}
		          	else if (left_3.Equals((object) boxedExpression_2) && right_3.Equals((object) boxedExpression_1))
		          	{
		            	joined = BoxedExpression.Binary(binaryOp_1 == BinaryOperator.Ceq ? BinaryOperator.Cge : BinaryOperator.Cle, left_3, right_3, (object) null);
		            	return true;
		          	}
        		}
				if (binaryOp_1 == BinaryOperator.Cgt && binaryOp_2 == BinaryOperator.Ceq || binaryOp_1 == BinaryOperator.Ceq && binaryOp_2 == BinaryOperator.Cgt)
        		{
		          if (left_3.Equals((object) boxedExpression_1) && right_3.Equals((object) boxedExpression_2))
		          {
		            joined = BoxedExpression.Binary(BinaryOperator.Cge, left_3, right_3, (object) null);
		            return true;
		          }
		          else if (left_3.Equals((object) boxedExpression_2) && right_3.Equals((object) boxedExpression_1))
		          {
		            joined = BoxedExpression.Binary(binaryOp_1 == BinaryOperator.Ceq ? BinaryOperator.Cle : BinaryOperator.Cge, left_3, right_3, (object) null);
		            return true;
		          }
		        }
			}
			return false;
		}
		
		private void ObjectInvariant ()
		{
		}
		
		#endregion
	}
}

