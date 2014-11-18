using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="namespace", Target="System.Numerics", Justification="[....] - by design")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", Scope="assembly", Target="System.Numerics", Justification="[....] - by design")]

[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.SR.GetObject(System.String):System.Object")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "System.Numerics.BigInteger.ObjectInvariant():System.Void")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.SR.#GetString(System.String,System.Boolean&)")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.SR.#get_Resources()")]

[module: SuppressMessage("Microsoft.Usage","CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="System.Numerics.BigInteger.#op_UnaryPlus(System.Numerics.BigInteger)")]
[module: SuppressMessage("Microsoft.Usage","CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="System.Numerics.BigInteger.#op_RightShift(System.Numerics.BigInteger,System.Int32)")]
[module: SuppressMessage("Microsoft.Usage","CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="System.Numerics.BigInteger.#op_OnesComplement(System.Numerics.BigInteger)")]
[module: SuppressMessage("Microsoft.Usage","CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="System.Numerics.BigInteger.#op_LeftShift(System.Numerics.BigInteger,System.Int32)")]
[module: SuppressMessage("Microsoft.Usage","CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="System.Numerics.BigInteger.#op_Increment(System.Numerics.BigInteger)")]
[module: SuppressMessage("Microsoft.Usage","CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="System.Numerics.BigInteger.#op_ExclusiveOr(System.Numerics.BigInteger,System.Numerics.BigInteger)")]
[module: SuppressMessage("Microsoft.Usage","CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="System.Numerics.BigInteger.#op_Decrement(System.Numerics.BigInteger)")]
[module: SuppressMessage("Microsoft.Usage","CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="System.Numerics.BigInteger.#op_BitwiseOr(System.Numerics.BigInteger,System.Numerics.BigInteger)")]
[module: SuppressMessage("Microsoft.Usage","CA2225:OperatorOverloadsHaveNamedAlternates", Scope="member", Target="System.Numerics.BigInteger.#op_BitwiseAnd(System.Numerics.BigInteger,System.Numerics.BigInteger)")]

[module: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.Numerics.BigIntegerBuilder.ObjectInvariant():System.Void")]
