namespace Mono.CodeContracts.Static.Analysis.Numerical {
    interface IIntervalEnvironment<TVar, TExpr, TInterval, TNumeric> 
        where TInterval : IntervalBase<TInterval, TNumeric> {

        IntervalContextBase<TInterval, TNumeric> Context { get; }

        TInterval Eval (TExpr expr);
        TInterval Eval (TVar expr);

        bool TryGetValue (TVar rightVar, out TInterval intv);
    }
}