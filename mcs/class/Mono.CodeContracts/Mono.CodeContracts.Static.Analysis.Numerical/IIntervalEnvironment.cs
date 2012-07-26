namespace Mono.CodeContracts.Static.Analysis.Numerical {
    interface IIntervalEnvironment<TVar, TExpr, TInterval, TNumeric> where TInterval : IntervalBase<TInterval, TNumeric> {
        IntervalContextBase<TInterval, TNumeric> Context { get; }

        TInterval Evaluate (TExpr expr);
        TInterval Evaluate (TVar expr);

        bool TryGetValue (TVar rightVar, out TInterval intv);
    }
}