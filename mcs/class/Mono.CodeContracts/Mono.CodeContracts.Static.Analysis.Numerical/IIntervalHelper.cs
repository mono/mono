using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
    internal interface IIntervalHelper<TInterval, TNumeric> {
        TInterval TopValue { get; }
        TInterval BottomValue { get; }
        TInterval Zero { get; }
        TInterval One { get; }
        TInterval Positive { get; }
        TInterval Negative { get; }

        TInterval For (long value);
        TInterval For (TNumeric value);
        TInterval For (TNumeric lower, TNumeric upper);

        bool IsGreaterThanZero (TNumeric value);
        bool IsGreaterEqualThanZero (TNumeric value);

        bool IsLessThanZero(TNumeric value);
        bool IsLessEqualThanZero(TNumeric value);

        bool IsLessThan (TNumeric a, TNumeric b);
        bool IsLessEqualThan (TNumeric a, TNumeric b);

        bool IsGreaterThan(TNumeric a, TNumeric b);
        bool IsGreaterEqualThan(TNumeric a, TNumeric b);

        bool IsZero (TNumeric value);
        bool IsNotZero (TNumeric value);

        bool IsPlusInfinity (TNumeric value);
        bool IsMinusInfinity (TNumeric value);

        bool AreEqual (TNumeric a, TNumeric b);

        TInterval Add (TInterval a, TInterval b);
        TInterval Sub (TInterval a, TInterval b);
        TInterval Div (TInterval a, TInterval b);
        TInterval Rem (TInterval a, TInterval b);
        TInterval Mul (TInterval a, TInterval b);
        
        TInterval Not (TInterval value);
        TInterval UnaryMinus (TInterval value);

        FlatDomain<bool> IsLessThan (TInterval a, TInterval b);
    }
}