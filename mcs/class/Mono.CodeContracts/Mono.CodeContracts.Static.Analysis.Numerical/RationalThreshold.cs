namespace Mono.CodeContracts.Static.Analysis.Numerical {
        class RationalThreshold : Threshold<Rational> {
                public RationalThreshold (int size) : base (size)
                {
                }

                protected override Rational MinusInfinity { get { return Rational.MinusInfinity; } }

                protected override Rational PlusInfinity { get { return Rational.PlusInfinity; } }

                protected override Rational Zero { get { return Rational.Zero; } }

                protected override bool LessThan (Rational a, Rational b)
                {
                        return a < b;
                }
        }
}