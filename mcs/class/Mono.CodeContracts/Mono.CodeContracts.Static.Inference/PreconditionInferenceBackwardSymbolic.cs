using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Inference.Interface;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.Proving;
using Mono.CodeContracts.Static.Analysis.Drivers;
using Mono.CodeContracts.Static.ControlFlow;

namespace Mono.CodeContracts.Static.Inference
{
	public class PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions> : IPreconditionInference where Type : IEquatable<Type> where Expression : IEquatable<Expression> where Variable : IEquatable<Variable> where LogOptions : IFrameworkLogOptions
	{
		private const int TIMEOUT = 2;
	    private readonly IFactQuery<BoxedExpression, Variable> Facts;
	    private readonly IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions> MDriver;
	    private readonly TimeOutChecker timeout;

		public PreconditionInferenceBackwardSymbolic(IFactQuery<BoxedExpression, Variable> facts, IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions> mdriver)
	    {
	      this.Facts = facts;
	      this.MDriver = mdriver;
	      this.timeout = new TimeOutChecker(2, false);
	    }

		public bool TryInferPrecondition(ProofObligation obl, ICodeFixesManager codefixesManager, out InferredPreconditions preConditions)
	    {
	      Func<BoxedExpression, SimpleInferredPrecondition> func = (Func<BoxedExpression, SimpleInferredPrecondition>) null;
	      PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.\u003C\u003Ec__DisplayClass7 cDisplayClass7 = new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.\u003C\u003Ec__DisplayClass7();
	      cDisplayClass7.obl = obl;
	      cDisplayClass7.\u003C\u003E4__this = this;
	      preConditions = (InferredPreconditions) null;
	      if (this.timeout.HasAlreadyTimeOut)
	        return false;
	      PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.BackwardsPropagation backwardsPropagation = new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.BackwardsPropagation(cDisplayClass7.obl, cDisplayClass7.obl.PC, codefixesManager, this.Facts, this.MDriver, this.timeout);
	      codefixesManager.TrySuggestOffByOneFix<Variable>(cDisplayClass7.obl, cDisplayClass7.obl.PC, cDisplayClass7.obl.ObligationName == "ArrayUpperBoundAccess", cDisplayClass7.obl.Condition, new Func<Variable, FList<PathElement>>((object) cDisplayClass7, __methodptr(\u003CTryInferPrecondition\u003Eb__0)), new Func<BoxedExpression, bool>((object) cDisplayClass7, __methodptr(\u003CTryInferPrecondition\u003Eb__1)), this.Facts);
	      if (backwardsPropagation.TryInferPrecondition(cDisplayClass7.obl.PC, new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition((BoxedExpression) null, cDisplayClass7.obl.ConditionForPreconditionInference, (List<BoxedExpression>) null)))
	      {
	        List<BoxedExpression> list1 = backwardsPropagation.InferredPrecondition(this.Facts);
	        if (list1 == null)
	        {
	          preConditions = (InferredPreconditions) null;
	        }
	        else
	        {
	          IValueContextData<Local, Parameter, Method, Field, Type, Variable> valueContext = this.MDriver.Context.ValueContext;
	          List<BoxedExpression> list2 = list1;
	          if (PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.CS\u0024\u003C\u003E9__CachedAnonymousMethodDelegate5 == null)
	          {
	            PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.CS\u0024\u003C\u003E9__CachedAnonymousMethodDelegate5 = new Func<BoxedExpression, bool>((object) null, __methodptr(\u003CTryInferPrecondition\u003Eb__2));
	          }
	          Func<BoxedExpression, bool> predicate = PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.CS\u0024\u003C\u003E9__CachedAnonymousMethodDelegate5;
	          IEnumerable<BoxedExpression> source = Enumerable.Where<BoxedExpression>((IEnumerable<BoxedExpression>) list2, predicate);
	          if (func == null)
	          {
	            func = new Func<BoxedExpression, SimpleInferredPrecondition>((object) cDisplayClass7, __methodptr(\u003CTryInferPrecondition\u003Eb__3));
	          }
	          Func<BoxedExpression, SimpleInferredPrecondition> selector = func;
	          IEnumerable<SimpleInferredPrecondition> enumerable = Enumerable.Select<BoxedExpression, SimpleInferredPrecondition>(source, selector);
	          preConditions = new InferredPreconditions((IEnumerable<IInferredPrecondition>) enumerable);
	        }
	      }
	      if ((preConditions == null || !Enumerable.Any<IInferredPrecondition>((IEnumerable<IInferredPrecondition>) preConditions)) && (!backwardsPropagation.AlreadyFoundAFix && backwardsPropagation.TestFix != null))
	        codefixesManager.TrySuggestTestStrengthening(cDisplayClass7.obl, backwardsPropagation.TestFix.pc, backwardsPropagation.TestFix.Fix);
	      if (preConditions != null)
	        return Enumerable.Any<IInferredPrecondition>((IEnumerable<IInferredPrecondition>) preConditions);
	      else
	        return false;
    	}

		private class BackwardsPropagation : IEdgeVisit<APC, Local, Parameter, Method, Field, Type, Variable, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition>, IVisitMSIL<APC, Local, Parameter, Method, Field, Type, Variable, Variable, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition>, IVisitSynthIL<APC, Method, Type, Variable, Variable, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition>, IVisitExprIL<APC, Type, Variable, Variable, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition>
	    {
	      private const int MaxDepth = 400;
	      private readonly APC pcCondition;
	      private readonly PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.InvariantDB invariants;
	      private readonly IFactQuery<BoxedExpression, Variable> facts;
	      private readonly IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions> Mdriver;
	      private readonly ExpressionReader<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable> ExpressionReader;
	      private readonly SimpleSatisfyProcedure<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> SatisfyProcedure;
	      private readonly ICFG CFG;
	      private readonly Set<APC> underVisit;
	      private readonly TimeOutChecker timeout;
	      private readonly ICodeFixesManager CodeFixes;
	      private readonly ProofObligation obl;

	      internal PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.BackwardsPropagation.AdditionalTest TestFix { get; private set; }

	      internal bool AlreadyFoundAFix { get; private set; }

	      private bool TraceInference
	      {
	        get
	        {
	          return this.Mdriver.Options.TraceInference;
	        }
	      }

	      public BackwardsPropagation(ProofObligation obl, APC pcCondition, ICodeFixesManager codefixesManager, IFactQuery<BoxedExpression, Variable> facts, IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions> mdriver, TimeOutChecker timeout)
	      {
	        this.obl = obl;
	        this.pcCondition = pcCondition;
	        this.facts = facts;
	        this.Mdriver = mdriver;
	        this.CFG = this.Mdriver.StackLayer.Decoder.Context.MethodContext.CFG;
	        this.invariants = new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.InvariantDB(this.CFG.Entry);
	        this.underVisit = new Set<APC>();
	        this.timeout = timeout;
	        this.CodeFixes = codefixesManager;
	        this.ExpressionReader = new ExpressionReader<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>();
	        this.SatisfyProcedure = new SimpleSatisfyProcedure<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>(mdriver.MetaDataDecoder);
	        this.TestFix = (PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.BackwardsPropagation.AdditionalTest) null;
	        this.AlreadyFoundAFix = false;
	      }

   

	      private BoxedExpression Converter(APC pc, BoxedExpression exp)
	      {
	        ExpressionInPreState expressionInPreState = PreconditionSuggestion.ExpressionInPreState<Local, Parameter, Method, Field, Property, Event, Type, Variable, Expression, Attribute, Assembly>(exp, this.Mdriver.Context, this.Mdriver.MetaDataDecoder, pc, false, false);
	        if (expressionInPreState != null)
	          return expressionInPreState.expr;
	        else
	          return (BoxedExpression) null;
	      }

	      public List<BoxedExpression> InferredPrecondition(IFactQuery<BoxedExpression, Variable> facts)
	      {
	        List<BoxedExpression> list = new List<BoxedExpression>();
	        foreach (PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition precondition1 in this.invariants.PreconditionAtEntryPoint)
	        {
	          if (!precondition1.IsNone)
	          {
	            PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition precondition2 = precondition1.Convert((Converter<BoxedExpression, BoxedExpression>) (exp => this.Converter(this.CFG.EntryAfterRequires, exp)));
	            if (!precondition2.IsNone)
	            {
	              Func<BoxedExpression, ProofOutcome> IsImpliedByPreconditions = (Func<BoxedExpression, ProofOutcome>) (exp => facts.IsTrue(this.CFG.EntryAfterRequires, exp));
	              if (ListExtensions.AddIfNotNull<BoxedExpression>(list, precondition2.SimplifyPremises(IsImpliedByPreconditions).SimplifyCondition(this.SatisfyProcedure).ToBoxedExpression(this.Mdriver.MetaDataDecoder)))
	                continue;
	            }
	            PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition precondition3 = precondition1.Convert((Converter<BoxedExpression, BoxedExpression>) (exp => this.Converter(this.pcCondition, exp)));
	            if (!precondition3.IsNone)
	            {
	              Func<BoxedExpression, ProofOutcome> IsImpliedByPreconditions = (Func<BoxedExpression, ProofOutcome>) (exp => facts.IsTrue(this.CFG.EntryAfterRequires, exp));
	              ListExtensions.AddIfNotNull<BoxedExpression>(list, precondition3.SimplifyPremises(IsImpliedByPreconditions).SimplifyCondition(this.SatisfyProcedure).ToBoxedExpression(this.Mdriver.MetaDataDecoder));
	            }
	          }
	        }
	        return list;
	      }

	      public bool TryInferPrecondition(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        if (this.timeout.HasAlreadyTimeOut)
	          return false;
	        try
	        {
	          this.timeout.Start();
	          this.Visit(pc, pre, 0);
	          return this.invariants.PreconditionAtEntryPoint.Count != 0;
	        }
	        catch (TimeoutExceptionFixpointComputation ex)
	        {
	          return false;
	        }
	        finally
	        {
	          this.timeout.Stop();
	        }
	      }

	      private void Visit(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre, int depth)
	      {
	        if (this.Checks(pc, pre, depth))
	          return;
	        PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition newPrecondition;
	        APC apc1 = this.VisitBlock(pc, pre, out newPrecondition);
	        if (newPrecondition.IsNone || this.Checks(apc1, newPrecondition, depth))
	          return;
	        pre = newPrecondition;
	        pc = apc1;
	        foreach (APC apc2 in this.CFG.Predecessors(apc1))
	        {
	          if (!this.Mdriver.BasicFacts.IsUnreachable(apc2))
	          {
	            if (this.underVisit.Contains(apc1))
	              break;
	            if (this.CFG.IsForwardBackEdgeTarget(pc))
	            {
	              this.underVisit.Add(apc1);
	              this.Visit(apc2, this.Mdriver.BackwardTransfer<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.BackwardsPropagation>(pc, apc2, pre, this), depth + 1);
	              this.underVisit.Remove(apc1);
	            }
	            else
	            {
	              PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre1 = this.Mdriver.BackwardTransfer<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.BackwardsPropagation>(pc, apc2, pre, this);
	              this.Visit(apc2, pre1, depth + 1);
	            }
	          }
	        }
	      }

	      private bool Checks(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre, int depth)
	      {
	        this.timeout.CheckTimeOut("Precondition (Backwards) computation");
	        if (depth >= 400 || pre.IsNone)
	          return true;
	        if (!this.CFG.Entry.Equals(pc))
	          return false;
	        this.invariants.Add(pc, pre);
	        return true;
	      }

	      private APC VisitBlock(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition preCondition, out PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition newPrecondition)
	      {
	        APC that = pc.FirstInBlock();
	        IEnumerable<APC> source = this.CFG.Predecessors(pc);
	        newPrecondition = preCondition;
	        for (; Enumerable.Count<APC>(source) == 1; {
	          APC apc;
	          source = this.CFG.Predecessors(apc);
	        }
	        )
	        {
	          if (pc.Equals(that))
	            return that;
	          if (this.Mdriver.BasicFacts.IsUnreachable(pc))
	          {
	            newPrecondition = PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	            return pc;
	          }
	          else
	          {
	            apc = Enumerable.First<APC>(source);
	            newPrecondition = this.Mdriver.BackwardTransfer<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.BackwardsPropagation>(pc, apc, newPrecondition, this);
	            pc = apc;
	          }
	        }
	        return pc;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Rename(APC from, APC to, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre, IFunctionalMap<Variable, Variable> renaming)
	      {
	        List<BoxedExpression> Premises = pre.HasEmptyPremises ? (List<BoxedExpression>) null : ListExtensions.ConvertAllDroppingNulls<BoxedExpression, BoxedExpression>(pre.Premises, (Converter<BoxedExpression, BoxedExpression>) (exp => BoxedExpressionExtensions.Rename<Variable>(exp, renaming, (Func<Variable, BoxedExpression>) null)));
	        if (Premises != null && Premises.Count < pre.Premises.Count)
	          return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	        Func<Variable, BoxedExpression> converter = (Func<Variable, BoxedExpression>) (v => BoxedExpression.Convert<Type, Variable, Expression>(this.Mdriver.Context.ExpressionContext.Refine(to, v), this.Mdriver.ExpressionDecoder, int.MaxValue, true, true, (Func<Variable, FList<PathElement>>) null));
	        BoxedExpression boxedExpression = BoxedExpressionExtensions.Rename<Variable>(pre.Condition, renaming, converter);
	        List<BoxedExpression> knownFacts = pre.KnownFacts != null ? pre.KnownFacts.ConvertAll<BoxedExpression>((Converter<BoxedExpression, BoxedExpression>) (exp => BoxedExpressionExtensions.Rename<Variable>(exp, renaming, (Func<Variable, BoxedExpression>) null))) : (List<BoxedExpression>) null;
	        if (boxedExpression != null)
	        {
	          switch (this.facts.IsTrue(to, boxedExpression))
	          {
	            case ProofOutcome.Top:
	              return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(Premises, boxedExpression, knownFacts);
	            case ProofOutcome.Bottom:
	              return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	            case ProofOutcome.True:
	              BinaryOperator bop1;
	              BoxedExpression left1;
	              BoxedExpression right1;
	              BinaryOperator bop2;
	              BoxedExpression left2;
	              BoxedExpression right2;
	              if (Premises != null && Premises.Count == 1 && (boxedExpression.IsBinaryExpression(out bop1, out left1, out right1) && Premises[0].IsBinaryExpression(out bop2, out left2, out right2)))
	              {
	                this.Normalize(ref bop1, ref left1, ref right1);
	                this.Normalize(ref bop2, ref left2, ref right2);
	                if (bop1 == BinaryOperator.Clt && (bop2 == BinaryOperator.Clt || bop2 == BinaryOperator.Cle) && left1.Equals((object) left2))
	                  return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(new List<BoxedExpression>(), BoxedExpression.Binary(BinaryOperator.Cle, right2, right1, (object) null), pre.KnownFacts);
	              }
	              return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	            case ProofOutcome.False:
	              Func<Variable, BoxedExpression> VariableName = (Func<Variable, BoxedExpression>) (v =>
	              {
	                FList<PathElement> local_0 = this.Mdriver.Context.ValueContext.AccessPathList(from, v, true, false);
	                if (local_0 != null)
	                  return BoxedExpression.Var((object) v, local_0);
	                else
	                  return (BoxedExpression) null;
	              });
	              this.AlreadyFoundAFix = this.AlreadyFoundAFix || this.CodeFixes.TrySuggestConstantInitializationFix<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Variable>(this.obl, (Func<APC>) (() => APCExtensions.GetFirstPredecessorWithSourceContext(to, this.Mdriver.CFG)), pre.Condition, boxedExpression, this.Mdriver.MetaDataDecoder, (Func<Variable, BoxedExpression>) (v => converter(renaming[v])), VariableName);
	              return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	          }
	        }
	        return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	      }

	      private void Normalize(ref BinaryOperator bop, ref BoxedExpression left, ref BoxedExpression right)
	      {
	        if (bop != BinaryOperator.Cgt && bop != BinaryOperator.Cge)
	          return;
	        bop = bop == BinaryOperator.Cgt ? BinaryOperator.Clt : BinaryOperator.Cle;
	        BoxedExpression boxedExpression = right;
	        right = left;
	        left = boxedExpression;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Arglist(APC pc, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition BranchCond(APC pc, APC target, BranchOperator bop, Variable value1, Variable value2, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition BranchTrue(APC pc, APC target, Variable cond, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition BranchFalse(APC pc, APC target, Variable cond, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Branch(APC pc, APC target, bool leave, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Break(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Call<TypeList, ArgList>(APC pc, Method method, bool tail, bool virt, TypeList extraVarargs, Variable dest, ArgList args, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre) where TypeList : IIndexable<Type> where ArgList : IIndexable<Variable>
	      {
	        if (!this.AlreadyFoundAFix && pre.Condition != null)
	        {
	          Func<BoxedExpression, BoxedExpression> ExtraSimplification = (Func<BoxedExpression, BoxedExpression>) (exp => new SimpleSatisfyProcedure<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>(this.Mdriver.MetaDataDecoder).ApplySimpleArithmeticRules(exp));
	          BoxedExpression condition = BoxedExpressionExtensions.Simplify<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>(pre.Condition, this.Mdriver.MetaDataDecoder, (Func<BoxedExpression, bool>) null, ExtraSimplification);
	          if (this.CodeFixes.TrySuggestFixForMethodCallReturnValue<Variable, ArgList>(this.obl, pc, dest, args, condition, (Func<bool>) (() => this.Mdriver.MetaDataDecoder.IsStatic(method)), (Func<string>) (() => this.Mdriver.MetaDataDecoder.FullName(method)), (Func<Variable, FList<PathElement>>) (var => this.Mdriver.Context.ValueContext.AccessPathList(pc, var, true, true)), (Func<Variable, FList<PathElement>, BoxedExpression>) ((var, accessPath) => (BoxedExpression) this.MakeMethodCallExpression(method, var, accessPath)), (Func<Variable, BoxedExpression>) (var => this.MakeZeroExp(pc, var))))
	            this.AlreadyFoundAFix = true;
	        }
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Calli<TypeList, ArgList>(APC pc, Type returnType, TypeList argTypes, bool tail, bool instance, Variable dest, Variable fp, ArgList args, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre) where TypeList : IIndexable<Type> where ArgList : IIndexable<Variable>
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ckfinite(APC pc, Variable dest, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Cpblk(APC pc, bool @volatile, Variable destaddr, Variable srcaddr, Variable len, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Endfilter(APC pc, Variable decision, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Endfinally(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Initblk(APC pc, bool @volatile, Variable destaddr, Variable value, Variable len, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Jmp(APC pc, Method method, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldarg(APC pc, Parameter argument, bool isOld, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldarga(APC pc, Parameter argument, bool isOld, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldftn(APC pc, Method method, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldind(APC pc, Type type, bool @volatile, Variable dest, Variable ptr, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldloc(APC pc, Local local, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldloca(APC pc, Local local, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Localloc(APC pc, Variable dest, Variable size, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Nop(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Pop(APC pc, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Return(APC pc, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Starg(APC pc, Parameter argument, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Stind(APC pc, Type type, bool @volatile, Variable ptr, Variable value, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Stloc(APC pc, Local local, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Switch(APC pc, Type type, IEnumerable<Pair<object, APC>> cases, Variable value, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Box(APC pc, Type type, Variable dest, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition ConstrainedCallvirt<TypeList, ArgList>(APC pc, Method method, bool tail, Type constraint, TypeList extraVarargs, Variable dest, ArgList args, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre) where TypeList : IIndexable<Type> where ArgList : IIndexable<Variable>
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Castclass(APC pc, Type type, Variable dest, Variable obj, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Cpobj(APC pc, Type type, Variable destptr, Variable srcptr, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Initobj(APC pc, Type type, Variable ptr, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldelem(APC pc, Type type, Variable dest, Variable array, Variable index, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldelema(APC pc, Type type, bool @readonly, Variable dest, Variable array, Variable index, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldfld(APC pc, Field field, bool @volatile, Variable dest, Variable obj, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldflda(APC pc, Field field, Variable dest, Variable obj, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldlen(APC pc, Variable dest, Variable array, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldsfld(APC pc, Field field, bool @volatile, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldsflda(APC pc, Field field, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldtypetoken(APC pc, Type type, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldfieldtoken(APC pc, Field field, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldmethodtoken(APC pc, Method method, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldvirtftn(APC pc, Method method, Variable dest, Variable obj, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Mkrefany(APC pc, Type type, Variable dest, Variable obj, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Newarray<ArgList>(APC pc, Type type, Variable dest, ArgList len, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre) where ArgList : IIndexable<Variable>
	      {
	        ReaderInfo<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable> readerInfo = new ReaderInfo<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>(pc, this.Mdriver.Context, this.Mdriver.MetaDataDecoder);
	        Func<Variable, BoxedExpression> Converter = (Func<Variable, BoxedExpression>) (v => BoxedExpression.Convert<Type, Variable, Expression>(this.Mdriver.Context.ExpressionContext.Refine(pc.Post(), v), this.Mdriver.ExpressionDecoder, int.MaxValue, true, true, (Func<Variable, FList<PathElement>>) null));
	        Func<APC> definitionPC = (Func<APC>) (() =>
	        {
	          APC local_0;
	          if (!this.Mdriver.AdditionalSyntacticInformation.VariableDefinitions.TryGetValue(len[0], out local_0))
	            local_0 = APCExtensions.GetFirstPredecessorWithSourceContext(pc, this.Mdriver.CFG);
	          return local_0;
	        });
	        if (pre.Condition != null && this.CodeFixes.TrySuggestLargerAllocation<Variable>(this.obl, definitionPC, pc, pre.Condition, dest, len[0], Converter, this.facts))
	          this.AlreadyFoundAFix = true;
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Newobj<ArgList>(APC pc, Method ctor, Variable dest, ArgList args, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre) where ArgList : IIndexable<Variable>
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Refanytype(APC pc, Variable dest, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Refanyval(APC pc, Type type, Variable dest, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Rethrow(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Stelem(APC pc, Type type, Variable array, Variable index, Variable value, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        bool isForAll;
	        BoxedExpression boundVar;
	        BoxedExpression lower;
	        BoxedExpression upper;
	        BoxedExpression body;
	        BoxedExpression.ArrayIndexExpression<Type> res;
	        if (pre.Condition.IsQuantifiedExpression(out isForAll, out boundVar, out lower, out upper, out body) && isForAll && (BoxedExpressionExtensions.TryFindArrayExp<Type>(body, boundVar, out res) && array.Equals(res.Array.UnderlyingVariable)))
	        {
	          BoxedExpression upperBound = BoxedExpression.Convert<Type, Variable, Expression>(this.Mdriver.Context.ExpressionContext.Refine(pc, index), this.Mdriver.ExpressionDecoder, int.MaxValue, true, true, (Func<Variable, FList<PathElement>>) null);
	          ForAllIndexedExpression indexedExpression = new ForAllIndexedExpression(boundVar, lower, upperBound, body);
	          if (this.facts.IsTrue(pc, (BoxedExpression) indexedExpression) == ProofOutcome.True)
	          {
	            BoxedExpression Condition = body.Substitute((BoxedExpression) res, BoxedExpression.Convert<Type, Variable, Expression>(this.Mdriver.Context.ExpressionContext.Refine(pc, value), this.Mdriver.ExpressionDecoder, int.MaxValue, true, true, (Func<Variable, FList<PathElement>>) null));
	            if (Condition != null)
	              return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(pre.Premises, Condition, (List<BoxedExpression>) null);
	          }
	        }
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Stfld(APC pc, Field field, bool @volatile, Variable obj, Variable value, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Stsfld(APC pc, Field field, bool @volatile, Variable value, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Throw(APC pc, Variable exn, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Unbox(APC pc, Type type, Variable dest, Variable obj, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Unboxany(APC pc, Type type, Variable dest, Variable obj, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Entry(APC pc, Method method, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Assume(APC pc, string tag, Variable condition, object provenance, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        if (!(tag == "true") && !(tag == "false"))
	          return pre;
	        BoxedExpression boxedExpression1 = BoxedExpression.Convert<Type, Variable, Expression>(this.Mdriver.Context.ExpressionContext.Refine(pc, condition), this.Mdriver.ExpressionDecoder, int.MaxValue, true, true, (Func<Variable, FList<PathElement>>) null);
	        if (boxedExpression1 == null)
	          return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	        FlatDomain<Type> type = this.Mdriver.Context.ValueContext.GetType(pc, condition);
	        if (!type.IsNormal || !type.Value.Equals(this.Mdriver.MetaDataDecoder.System_Boolean))
	          boxedExpression1 = BoxedExpressionsExtensions.MakeNotEqualToZero<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>(boxedExpression1, type, this.Mdriver.MetaDataDecoder);
	        if (tag == "false")
	          boxedExpression1 = boxedExpression1.Negate();
	        Func<APC> pc1 = (Func<APC>) (() =>
	        {
	          APC local_0;
	          if (this.Mdriver.AdditionalSyntacticInformation.VariableDefinitions.TryGetValue(condition, out local_0))
	            return local_0;
	          else
	            return APCExtensions.GetFirstPredecessorWithSourceContext(pc, this.Mdriver.CFG);
	        });
	        ReaderInfo<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable> info = new ReaderInfo<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>(pc, this.Mdriver.Context, this.Mdriver.MetaDataDecoder);
	        BoxedExpression boxedExpression2 = this.ExpressionReader.Visit(pre.Condition, info, (Func<object, FList<PathElement>>) null);
	        if (boxedExpression2 != null)
	        {
	          if (this.CodeFixes.TrySuggestTestFix<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly>(this.obl, pc1, boxedExpression1, boxedExpression2, this.Mdriver.MetaDataDecoder, (Func<BoxedExpression, bool>) (be => BoxedExpressionExtensions.IsArrayLength<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>(be, pc, this.Mdriver.Context, this.Mdriver.MetaDataDecoder))))
	            this.AlreadyFoundAFix = true;
	          else if (this.TestFix == null && Enumerable.Any<BoxedExpression>(Enumerable.Intersect<BoxedExpression>((IEnumerable<BoxedExpression>) BoxedExpressionExtensions.Variables(boxedExpression2), (IEnumerable<BoxedExpression>) BoxedExpressionExtensions.Variables(boxedExpression1))))
	          {
	            APC withSourceContext;
	            if (!this.Mdriver.AdditionalSyntacticInformation.VariableDefinitions.TryGetValue(condition, out withSourceContext))
	              withSourceContext = APCExtensions.GetFirstPredecessorWithSourceContext(pc, this.Mdriver.CFG);
	            this.TestFix = new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.BackwardsPropagation.AdditionalTest(withSourceContext, boxedExpression2);
	          }
	        }
	        switch (this.facts.IsTrue(pc, boxedExpression1))
	        {
	          case ProofOutcome.Top:
	            if (pre.KnownFacts != null)
	            {
	              foreach (BoxedExpression elem in pre.KnownFacts)
	              {
	                if (elem != null)
	                {
	                  FList<BoxedExpression> posAssumptions = FList<BoxedExpression>.Cons(elem, FList<BoxedExpression>.Empty);
	                  if (this.facts.IsTrueImply(pc, posAssumptions, FList<BoxedExpression>.Empty, boxedExpression1) == ProofOutcome.True)
	                    return pre.AddKnownFact(boxedExpression1);
	                }
	              }
	            }
	            return pre.AddPremise(boxedExpression1);
	          case ProofOutcome.Bottom:
	          case ProofOutcome.False:
	            return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	          case ProofOutcome.True:
	            return pre.AddKnownFact(boxedExpression1);
	          default:
	            return pre;
	        }
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Assert(APC pc, string tag, Variable condition, object provenance, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldstack(APC pc, int offset, Variable dest, Variable source, bool isOld, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldstacka(APC pc, int offset, Variable dest, Variable source, Type origParamType, bool isOld, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldresult(APC pc, Type type, Variable dest, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition BeginOld(APC pc, APC matchingEnd, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition EndOld(APC pc, APC matchingBegin, Type type, Variable dest, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Binary(APC pc, BinaryOperator op, Variable dest, Variable s1, Variable s2, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Isinst(APC pc, Type type, Variable dest, Variable obj, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldconst(APC pc, object constant, Type type, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Ldnull(APC pc, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Sizeof(APC pc, Type type, Variable dest, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        return pre;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Unary(APC pc, UnaryOperator op, bool overflow, bool unsigned, Variable dest, Variable source, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        BinaryOperator bop;
	        BoxedExpression left;
	        BoxedExpression right;
	        if (!pre.IsNone && pre.Condition.IsBinaryExpression(out bop, out left, out right) && (bop == BinaryOperator.Clt && this.AreTheSame(pc, dest, right)))
	        {
	          FList<PathElement> path = this.Mdriver.Context.ValueContext.AccessPathList(this.Mdriver.CFG.Post(this.Mdriver.CFG.Post(pc)), dest, true, true);
	          if (path != null)
	          {
	            BoxedExpression dest1 = BoxedExpression.Var((object) dest, path);
	            BoxedExpression newInitialization = BoxedExpression.Binary(BinaryOperator.Sub, right, BoxedExpression.Const<Local, Parameter, Field, Property, Event, Method, Type, Attribute, Assembly>((object) 1, this.Mdriver.MetaDataDecoder.System_Int32, this.Mdriver.MetaDataDecoder), (object) null);
	            this.AlreadyFoundAFix = this.AlreadyFoundAFix | this.CodeFixes.TrySuggestConstantInititalizationFix(this.obl, pc, dest1, BoxedExpression.Convert<Type, Variable, Expression>(this.Mdriver.Context.ExpressionContext.Refine(pc, source), this.Mdriver.ExpressionDecoder, int.MaxValue, true, true, (Func<Variable, FList<PathElement>>) null), newInitialization, pre.Condition);
	          }
	        }
	        return pre;
	      }

	      private bool AreTheSame(APC pc, Variable dest, BoxedExpression right)
	      {
	        if (right.UnderlyingVariable != null)
	        {
	          if (dest.Equals(right.UnderlyingVariable))
	            return true;
	          BoxedExpression boxedExpression = BoxedExpression.Convert<Type, Variable, Expression>(this.Mdriver.Context.ExpressionContext.Refine(this.Mdriver.CFG.Post(pc), dest), this.Mdriver.ExpressionDecoder, int.MaxValue, true, true, (Func<Variable, FList<PathElement>>) null);
	          UnaryOperator uop;
	          BoxedExpression left;
	          if (boxedExpression != null && boxedExpression.UnderlyingVariable != null && boxedExpression.UnderlyingVariable.Equals(right.UnderlyingVariable) || boxedExpression.IsUnaryExpression(out uop, out left) && OperatorExtensions.IsConversionOperator(uop) && right.UnderlyingVariable.Equals(left.UnderlyingVariable))
	            return true;
	        }
	        return false;
	      }

	      private BoxedExpression.BinaryExpressionMethodCall MakeMethodCallExpression(Method method, Variable var, FList<PathElement> accessPath)
	      {
	        return new BoxedExpression.BinaryExpressionMethodCall(BinaryOperator.Add, BoxedExpression.Var((object) var, accessPath, (object) this.Mdriver.MetaDataDecoder.System_Object), this.Mdriver.MetaDataDecoder.Name(method), (object) null);
	      }

	      private BoxedExpression MakeZeroExp(APC pc, Variable dest)
	      {
	        FlatDomain<Type> type = this.Mdriver.Context.ValueContext.GetType(this.Mdriver.Context.MethodContext.CFG.Post(pc), dest);
	        return !type.IsNormal ? BoxedExpression.Const<Local, Parameter, Field, Property, Event, Method, Type, Attribute, Assembly>((object) null, this.Mdriver.MetaDataDecoder.System_Object, this.Mdriver.MetaDataDecoder) : BoxedExpression.Const<Local, Parameter, Field, Property, Event, Method, Type, Attribute, Assembly>(!this.Mdriver.MetaDataDecoder.IsStruct(type.Value) ? (object) null : (this.Mdriver.MetaDataDecoder.System_Boolean.Equals(type.Value) ? (object) false : (object) 0), type.Value, this.Mdriver.MetaDataDecoder);
	      }

	      [Conditional("DEBUG")]
	      protected void BreakHere(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre, string instr = null)
	      {
	        if (!this.TraceInference)
	          return;
	        Console.WriteLine("Visiting: {0}-{1} with {2}", (object) pc.ToString(), (object) instr, (object) pre.ToString());
	      }

	      [Conditional("DEBUG")]
	      protected void Trace(string s, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        if (!this.TraceInference)
	          return;
	        Console.WriteLine(s + ": " + pre.ToString());
	      }

	      internal class AdditionalTest
	      {
	        public readonly APC pc;
	        public readonly BoxedExpression Fix;

	        public AdditionalTest(APC pc, BoxedExpression Fix)
	        {
	          this.pc = pc;
	          this.Fix = Fix;
	        }

	        public override string ToString()
	        {
	          return string.Format("{0}:{1}", (object) this.pc, (object) this.Fix);
	        }
	      }
	    }

	    private struct Precondition
	    {
	      public readonly List<BoxedExpression> Premises;
	      public readonly BoxedExpression Condition;
	      public readonly List<BoxedExpression> KnownFacts;
	      private readonly PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.State state;

	      public static PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition None
	      {
	        get
	        {
	          return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(true);
	        }
	      }

	      public bool IsNone
	      {
	        get
	        {
	          return this.state == 1;
	        }
	      }

	      public bool HasEmptyPremises
	      {
	        get
	        {
	          if (this.IsNone)
	            return false;
	          if (this.Premises != null)
	            return this.Premises.Count == 0;
	          else
	            return true;
	        }
	      }

	      public Precondition(BoxedExpression Premise, BoxedExpression Condition, List<BoxedExpression> knownFacts = null)
	      {
	        // ISSUE: explicit reference operation
	        // ISSUE: variable of a reference type
	        PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition& local = @this;
	        List<BoxedExpression> Premises;
	        if (Premise == null)
	          Premises = (List<BoxedExpression>) null;
	        else
	          Premises = new List<BoxedExpression>()
	          {
	            Premise
	          };
	        BoxedExpression Condition1 = Condition;
	        List<BoxedExpression> knownFacts1 = knownFacts;
	        // ISSUE: explicit reference operation
	        ^local = new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(Premises, Condition1, knownFacts1);
	      }

	      public Precondition(List<BoxedExpression> Premises, BoxedExpression Condition, List<BoxedExpression> knownFacts = null)
	      {
	        this.Premises = Premises;
	        this.Condition = Condition;
	        this.KnownFacts = knownFacts == null ? new List<BoxedExpression>() : knownFacts;
	        this.state = (PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.State) 0;
	      }

	      public Precondition(bool dummy)
	      {
	        this.Premises = (List<BoxedExpression>) null;
	        this.Condition = (BoxedExpression) null;
	        this.KnownFacts = (List<BoxedExpression>) null;
	        this.state = (PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.State) 1;
	      }

	      public static bool operator ==(PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition left, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition right)
	      {
	        if ((ValueType) left.state == (ValueType) right.state)
	        {
	          switch ((int) left.state)
	          {
	            case 0:
	              if (left.Condition == right.Condition)
	                return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.ListEqual(left.Premises, right.Premises);
	              else
	                return false;
	            case 1:
	              return true;
	          }
	        }
	        return false;
	      }

	      public static bool operator !=(PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition left, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition right)
	      {
	        return !(left == right);
	      }

	      private static bool ListEqual(List<BoxedExpression> left, List<BoxedExpression> right)
	      {
	        if (left == null && right == null)
	          return true;
	        if (left == null != (right == null))
	          return false;
	        foreach (BoxedExpression boxedExpression in left)
	        {
	          if (!right.Contains(boxedExpression))
	            return false;
	        }
	        foreach (BoxedExpression boxedExpression in right)
	        {
	          if (!left.Contains(boxedExpression))
	            return false;
	        }
	        return true;
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition AddPremise(BoxedExpression premise)
	      {
	        if (this.Premises == null)
	          return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(premise, this.Condition, this.KnownFacts);
	        if (this.Premises.Contains(premise))
	          return this;
	        UnaryOperator uop;
	        BoxedExpression left;
	        if (premise.IsUnaryExpression(out uop, out left) && uop == UnaryOperator.Not)
	        {
	          object innerVar = left.UnderlyingVariable;
	          if (innerVar != null && this.Premises.Exists((Predicate<BoxedExpression>) (exp =>
	          {
	            Variable local_0;
	            if (exp != null && BoxedExpressionExtensions.TryGetFrameworkVariable<Variable>(exp, out local_0))
	              return local_0.Equals(innerVar);
	            else
	              return false;
	          })))
	            return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	        }
	        return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(new List<BoxedExpression>((IEnumerable<BoxedExpression>) this.Premises)
	        {
	          premise
	        }, this.Condition, this.KnownFacts);
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition AddKnownFact(BoxedExpression knownFact)
	      {
	        List<BoxedExpression> Premises = this.Premises;
	        BoxedExpression Condition = this.Condition;
	        List<BoxedExpression> knownFacts;
	        if (this.KnownFacts == null)
	          knownFacts = (List<BoxedExpression>) null;
	        else
	          knownFacts = new List<BoxedExpression>((IEnumerable<BoxedExpression>) this.KnownFacts)
	          {
	            knownFact
	          };
	        return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(Premises, Condition, knownFacts);
	      }

	      public BoxedExpression ToBoxedExpression(IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> mdDecoder)
	      {
	        switch ((int) this.state)
	        {
	          case 0:
	            BoxedExpression right = this.Condition;
	            if (this.Premises != null && this.Premises.Count > 0)
	              right = BoxedExpression.Binary(BinaryOperator.LogicalOr, BoxedExpressionsExtensions.Concatenate((IEnumerable<BoxedExpression>) this.Premises.ConvertAll<BoxedExpression>((Converter<BoxedExpression, BoxedExpression>) (exp => exp.Negate())), BinaryOperator.LogicalOr), right, (object) null);
	            return right;
	          case 1:
	            return BoxedExpression.ConstBool<Local, Parameter, Field, Property, Event, Method, Type, Attribute, Assembly>((object) true, mdDecoder);
	          default:
	            return (BoxedExpression) null;
	        }
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition SimplifyPremises(Func<BoxedExpression, ProofOutcome> IsImpliedByPreconditions)
	      {
	        if (this.IsNone || this.HasEmptyPremises)
	          return this;
	        List<BoxedExpression> Premises = new List<BoxedExpression>(this.Premises.Count);
	        foreach (BoxedExpression boxedExpression in this.Premises)
	        {
	          if (boxedExpression != null && IsImpliedByPreconditions(boxedExpression) == ProofOutcome.Top)
	            Premises.Add(boxedExpression);
	        }
	        Premises.Reverse();
	        return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(Premises, this.Condition, (List<BoxedExpression>) null);
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition SimplifyCondition(SimpleSatisfyProcedure<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> satisfy)
	      {
	        if (this.IsNone || this.HasEmptyPremises)
	          return this;
	        BoxedExpression Condition = satisfy.ApplySimpleArithmeticRules(this.Condition);
	        if (Condition == this.Condition)
	          return this;
	        else
	          return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(this.Premises, Condition, (List<BoxedExpression>) null);
	      }

	      public PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition Convert(Converter<BoxedExpression, BoxedExpression> converter)
	      {
	        if (this.IsNone)
	          return this;
	        BoxedExpression Condition = converter(this.Condition);
	        if (Condition == null)
	          return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	        if (this.Premises == null)
	          return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(this.Premises, Condition, (List<BoxedExpression>) null);
	        List<BoxedExpression> Premises = new List<BoxedExpression>(this.Premises.Count);
	        foreach (BoxedExpression input in this.Premises)
	        {
	          BoxedExpression boxedExpression = converter(input);
	          if (boxedExpression == null)
	            return PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition.None;
	          Premises.Add(boxedExpression);
	        }
	        return new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(Premises, Condition, (List<BoxedExpression>) null);
	      }

	      public override bool Equals(object obj)
	      {
	        if (obj == null || !(obj is PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition))
	          return false;
	        else
	          return this == (PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition) obj;
	      }

	      public override int GetHashCode()
	      {
	        if (this.state == 1)
	          return 0;
	        else
	          return (this.Premises != null ? this.Premises.GetHashCode() : 0) + (this.Condition != null ? this.Condition.GetHashCode() : 0);
	      }

	      public override string ToString()
	      {
	        return string.Format("{0} ==> {1} {2}", (object) this.ToString(this.Premises), this.Condition != null ? (object) ((object) this.Condition).ToString() : (object) string.Empty, this.KnownFacts == null || this.KnownFacts.Count <= 0 ? (object) string.Empty : (object) ("( knwon facts: " + this.ToString(this.KnownFacts) + " )"));
	      }

	      private string ToString(List<BoxedExpression> exps)
	      {
	        if (exps == null || exps.Count == 0)
	          return "true";
	        StringBuilder stringBuilder = new StringBuilder();
	        foreach (BoxedExpression boxedExpression in exps)
	        {
	          if (boxedExpression != null)
	            stringBuilder.Append(" " + ((object) boxedExpression).ToString());
	        }
	        return ((object) stringBuilder).ToString();
	      }

	      private enum State
	      {
	        Normal,
	        None,
	      }
	    }

	    private struct InvariantDB
	    {
	      private readonly Dictionary<APC, Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition>> Invariants;
	      private readonly APC entry;

	      public Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition> PreconditionAtEntryPoint
	      {
	        get
	        {
	          return this[this.entry];
	        }
	      }

	      public Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition> this[APC pc]
	      {
	        get
	        {
	          Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition> set;
	          if (this.Invariants.TryGetValue(pc, out set))
	            return set;
	          else
	            return new Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition>();
	        }
	      }

	      public InvariantDB(APC entry)
	      {
	        this.entry = entry;
	        this.Invariants = new Dictionary<APC, Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition>>();
	      }

	      public void Add(APC pc, PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition pre)
	      {
	        bool flag1 = false;
	        Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition> set1;
	        if (this.Invariants.TryGetValue(pc, out set1))
	        {
	          Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition> set2 = new Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition>();
	          foreach (PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition a in set1)
	          {
	            if (a.Condition.Equals((object) pre.Condition))
	            {
	              flag1 = true;
	              if (a.HasEmptyPremises || pre.HasEmptyPremises)
	              {
	                set2.Add(new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition((BoxedExpression) null, pre.Condition, (List<BoxedExpression>) null));
	              }
	              else
	              {
	                List<BoxedExpression> Premises = new List<BoxedExpression>();
	                List<BoxedExpression> list = pre.Premises.ConvertAll<BoxedExpression>((Converter<BoxedExpression, BoxedExpression>) (exp =>
	                {
	                  if (exp == null)
	                    return (BoxedExpression) null;
	                  else
	                    return exp.Negate();
	                }));
	                foreach (BoxedExpression boxedExpression in a.Premises)
	                {
	                  if (!list.Contains(boxedExpression))
	                    Premises.Add(boxedExpression);
	                }
	                if (Premises.Count <= a.Premises.Count)
	                {
	                  bool flag2 = true;
	                  foreach (BoxedExpression boxedExpression in Premises)
	                  {
	                    if (!a.Premises.Contains(boxedExpression))
	                    {
	                      flag2 = false;
	                      break;
	                    }
	                  }
	                  if (flag2)
	                  {
	                    set2.Add(new PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition(Premises, pre.Condition, (List<BoxedExpression>) null));
	                    continue;
	                  }
	                }
	                set2.Add(a);
	                set2.Add(pre);
	              }
	            }
	            else
	              set2.Add(a);
	          }
	          if (!flag1)
	            set2.Add(pre);
	          this.Invariants[pc] = set2;
	        }
	        else
	          this.Invariants[pc] = new Set<PreconditionInferenceBackwardSymbolic<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, LogOptions>.Precondition>()
	          {
	            pre
	          };
	      }

	      public override string ToString()
	      {
	        return this.Invariants.ToString();
	      }

		private void ObjectInvariant()
	    {
	    }
	}
}


