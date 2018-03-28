//---------------------------------------------------------------------
// <copyright file="GlobalSuppressions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <summary>global code analysis suppressions</summary>
//---------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;

// They recommend something along the lines of 'TypedBaseDataTable' instead of 'TypedTableBase', this can't be changed at this point
[module: SuppressMessage("Microsoft.Naming","CA1710:IdentifiersShouldHaveCorrectSuffix", Scope="type", Target="System.Data.TypedTableBase`1")]

// Select<TRow, S>(...) should have something more meaningfull than just 'S'
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="S", Scope="member", Target="System.Data.EnumerableRowCollectionExtensions.#Select`2(System.Data.EnumerableRowCollection`1<!!0>,System.Func`2<!!0,!!1>)")]
[module: SuppressMessage("Microsoft.Naming","CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId="T", Scope="member", Target="System.Data.EnumerableRowCollectionExtensions.#Select`2(System.Data.EnumerableRowCollection`1<!!0>,System.Func`2<!!0,!!1>)")]
[module: SuppressMessage("Microsoft.Naming","CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId="S", Scope="member", Target="System.Data.TypedTableBaseExtensions.#Select`2(System.Data.TypedTableBase`1<!!0>,System.Func`2<!!0,!!1>)")]
[module: SuppressMessage("Microsoft.Naming","CA1715:IdentifiersShouldHaveCorrectPrefix", MessageId="T", Scope="member", Target="System.Data.TypedTableBaseExtensions.#Select`2(System.Data.TypedTableBase`1<!!0>,System.Func`2<!!0,!!1>)")]

// Violations in the generated Resource file; can't prevent these from being generated...
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.Data.DataSetRes.#GetObject(System.String)")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.Data.DataSetRes.#GetString(System.String,System.Boolean&)")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.Data.DataSetRes.#get_Resources()")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.Data.DataSetExtensions.Error.#ArgumentNull(System.String)")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.Data.DataSetExtensions.Error.#ArgumentOutOfRange(System.String)")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.Data.DataSetExtensions.Error.#NotImplemented()", Justification="")]
[module: SuppressMessage("Microsoft.Performance","CA1811:AvoidUncalledPrivateCode", Scope="member", Target="System.Data.DataSetExtensions.Error.#NotSupported()")]
