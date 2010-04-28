//Copyright 2010 Microsoft Corporation
//
//Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. 
//You may obtain a copy of the License at 
//
//http://www.apache.org/licenses/LICENSE-2.0 
//
//Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
//"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and limitations under the License.


namespace System.Data.Services.Client
{
    #region Namespaces.

    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    #endregion Namespaces.

    internal static class ReflectionUtil
    {
        #region Static information on sequence methods
        private static readonly Dictionary<MethodInfo, SequenceMethod> s_methodMap;
        private static readonly Dictionary<SequenceMethod, MethodInfo> s_inverseMap;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static ReflectionUtil()
        {
            Dictionary<String, SequenceMethod> map = new Dictionary<string, SequenceMethod>(EqualityComparer<string>.Default);

            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Double>>)->Double", SequenceMethod.SumDoubleSelector);
            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Double>>>)->Nullable`1<Double>", SequenceMethod.SumNullableDoubleSelector);
            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Decimal>>)->Decimal", SequenceMethod.SumDecimalSelector);
            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Decimal>>>)->Nullable`1<Decimal>", SequenceMethod.SumNullableDecimalSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Int32>>)->Double", SequenceMethod.AverageIntSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Int32>>>)->Nullable`1<Double>", SequenceMethod.AverageNullableIntSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Single>>)->Single", SequenceMethod.AverageSingleSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Single>>>)->Nullable`1<Single>", SequenceMethod.AverageNullableSingleSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Int64>>)->Double", SequenceMethod.AverageLongSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Int64>>>)->Nullable`1<Double>", SequenceMethod.AverageNullableLongSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Double>>)->Double", SequenceMethod.AverageDoubleSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Double>>>)->Nullable`1<Double>", SequenceMethod.AverageNullableDoubleSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Decimal>>)->Decimal", SequenceMethod.AverageDecimalSelector);
            map.Add(@"Average(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Decimal>>>)->Nullable`1<Decimal>", SequenceMethod.AverageNullableDecimalSelector);
            map.Add(@"Aggregate(IQueryable`1<T0>, Expression`1<Func`3<T0, T0, T0>>)->T0", SequenceMethod.Aggregate);
            map.Add(@"Aggregate(IQueryable`1<T0>, T1, Expression`1<Func`3<T1, T0, T1>>)->T1", SequenceMethod.AggregateSeed);
            map.Add(@"Aggregate(IQueryable`1<T0>, T1, Expression`1<Func`3<T1, T0, T1>>, Expression`1<Func`2<T1, T2>>)->T2", SequenceMethod.AggregateSeedSelector);
            map.Add(@"AsQueryable(IEnumerable`1<T0>)->IQueryable`1<T0>", SequenceMethod.AsQueryableGeneric);
            map.Add(@"Where(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->IQueryable`1<T0>", SequenceMethod.Where);
            map.Add(@"Where(IQueryable`1<T0>, Expression`1<Func`3<T0, Int32, Boolean>>)->IQueryable`1<T0>", SequenceMethod.WhereOrdinal);
            map.Add(@"OfType(IQueryable)->IQueryable`1<T0>", SequenceMethod.OfType);
            map.Add(@"Cast(IQueryable)->IQueryable`1<T0>", SequenceMethod.Cast);
            map.Add(@"Select(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>)->IQueryable`1<T1>", SequenceMethod.Select);
            map.Add(@"Select(IQueryable`1<T0>, Expression`1<Func`3<T0, Int32, T1>>)->IQueryable`1<T1>", SequenceMethod.SelectOrdinal);
            map.Add(@"SelectMany(IQueryable`1<T0>, Expression`1<Func`2<T0, IEnumerable`1<T1>>>)->IQueryable`1<T1>", SequenceMethod.SelectMany);
            map.Add(@"SelectMany(IQueryable`1<T0>, Expression`1<Func`3<T0, Int32, IEnumerable`1<T1>>>)->IQueryable`1<T1>", SequenceMethod.SelectManyOrdinal);
            map.Add(@"SelectMany(IQueryable`1<T0>, Expression`1<Func`3<T0, Int32, IEnumerable`1<T1>>>, Expression`1<Func`3<T0, T1, T2>>)->IQueryable`1<T2>", SequenceMethod.SelectManyOrdinalResultSelector);
            map.Add(@"SelectMany(IQueryable`1<T0>, Expression`1<Func`2<T0, IEnumerable`1<T1>>>, Expression`1<Func`3<T0, T1, T2>>)->IQueryable`1<T2>", SequenceMethod.SelectManyResultSelector);
            map.Add(@"Join(IQueryable`1<T0>, IEnumerable`1<T1>, Expression`1<Func`2<T0, T2>>, Expression`1<Func`2<T1, T2>>, Expression`1<Func`3<T0, T1, T3>>)->IQueryable`1<T3>", SequenceMethod.Join);
            map.Add(@"Join(IQueryable`1<T0>, IEnumerable`1<T1>, Expression`1<Func`2<T0, T2>>, Expression`1<Func`2<T1, T2>>, Expression`1<Func`3<T0, T1, T3>>, IEqualityComparer`1<T2>)->IQueryable`1<T3>", SequenceMethod.JoinComparer);
            map.Add(@"GroupJoin(IQueryable`1<T0>, IEnumerable`1<T1>, Expression`1<Func`2<T0, T2>>, Expression`1<Func`2<T1, T2>>, Expression`1<Func`3<T0, IEnumerable`1<T1>, T3>>)->IQueryable`1<T3>", SequenceMethod.GroupJoin);
            map.Add(@"GroupJoin(IQueryable`1<T0>, IEnumerable`1<T1>, Expression`1<Func`2<T0, T2>>, Expression`1<Func`2<T1, T2>>, Expression`1<Func`3<T0, IEnumerable`1<T1>, T3>>, IEqualityComparer`1<T2>)->IQueryable`1<T3>", SequenceMethod.GroupJoinComparer);
            map.Add(@"OrderBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>)->IOrderedQueryable`1<T0>", SequenceMethod.OrderBy);
            map.Add(@"OrderBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, IComparer`1<T1>)->IOrderedQueryable`1<T0>", SequenceMethod.OrderByComparer);
            map.Add(@"OrderByDescending(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>)->IOrderedQueryable`1<T0>", SequenceMethod.OrderByDescending);
            map.Add(@"OrderByDescending(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, IComparer`1<T1>)->IOrderedQueryable`1<T0>", SequenceMethod.OrderByDescendingComparer);
            map.Add(@"ThenBy(IOrderedQueryable`1<T0>, Expression`1<Func`2<T0, T1>>)->IOrderedQueryable`1<T0>", SequenceMethod.ThenBy);
            map.Add(@"ThenBy(IOrderedQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, IComparer`1<T1>)->IOrderedQueryable`1<T0>", SequenceMethod.ThenByComparer);
            map.Add(@"ThenByDescending(IOrderedQueryable`1<T0>, Expression`1<Func`2<T0, T1>>)->IOrderedQueryable`1<T0>", SequenceMethod.ThenByDescending);
            map.Add(@"ThenByDescending(IOrderedQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, IComparer`1<T1>)->IOrderedQueryable`1<T0>", SequenceMethod.ThenByDescendingComparer);
            map.Add(@"Take(IQueryable`1<T0>, Int32)->IQueryable`1<T0>", SequenceMethod.Take);
            map.Add(@"TakeWhile(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->IQueryable`1<T0>", SequenceMethod.TakeWhile);
            map.Add(@"TakeWhile(IQueryable`1<T0>, Expression`1<Func`3<T0, Int32, Boolean>>)->IQueryable`1<T0>", SequenceMethod.TakeWhileOrdinal);
            map.Add(@"Skip(IQueryable`1<T0>, Int32)->IQueryable`1<T0>", SequenceMethod.Skip);
            map.Add(@"SkipWhile(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->IQueryable`1<T0>", SequenceMethod.SkipWhile);
            map.Add(@"SkipWhile(IQueryable`1<T0>, Expression`1<Func`3<T0, Int32, Boolean>>)->IQueryable`1<T0>", SequenceMethod.SkipWhileOrdinal);
            map.Add(@"GroupBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>)->IQueryable`1<IGrouping`2<T1, T0>>", SequenceMethod.GroupBy);
            map.Add(@"GroupBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, Expression`1<Func`2<T0, T2>>)->IQueryable`1<IGrouping`2<T1, T2>>", SequenceMethod.GroupByElementSelector);
            map.Add(@"GroupBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, IEqualityComparer`1<T1>)->IQueryable`1<IGrouping`2<T1, T0>>", SequenceMethod.GroupByComparer);
            map.Add(@"GroupBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, Expression`1<Func`2<T0, T2>>, IEqualityComparer`1<T1>)->IQueryable`1<IGrouping`2<T1, T2>>", SequenceMethod.GroupByElementSelectorComparer);
            map.Add(@"GroupBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, Expression`1<Func`2<T0, T2>>, Expression`1<Func`3<T1, IEnumerable`1<T2>, T3>>)->IQueryable`1<T3>", SequenceMethod.GroupByElementSelectorResultSelector);
            map.Add(@"GroupBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, Expression`1<Func`3<T1, IEnumerable`1<T0>, T2>>)->IQueryable`1<T2>", SequenceMethod.GroupByResultSelector);
            map.Add(@"GroupBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, Expression`1<Func`3<T1, IEnumerable`1<T0>, T2>>, IEqualityComparer`1<T1>)->IQueryable`1<T2>", SequenceMethod.GroupByResultSelectorComparer);
            map.Add(@"GroupBy(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>, Expression`1<Func`2<T0, T2>>, Expression`1<Func`3<T1, IEnumerable`1<T2>, T3>>, IEqualityComparer`1<T1>)->IQueryable`1<T3>", SequenceMethod.GroupByElementSelectorResultSelectorComparer);
            map.Add(@"Distinct(IQueryable`1<T0>)->IQueryable`1<T0>", SequenceMethod.Distinct);
            map.Add(@"Distinct(IQueryable`1<T0>, IEqualityComparer`1<T0>)->IQueryable`1<T0>", SequenceMethod.DistinctComparer);
            map.Add(@"Concat(IQueryable`1<T0>, IEnumerable`1<T0>)->IQueryable`1<T0>", SequenceMethod.Concat);
            map.Add(@"Union(IQueryable`1<T0>, IEnumerable`1<T0>)->IQueryable`1<T0>", SequenceMethod.Union);
            map.Add(@"Union(IQueryable`1<T0>, IEnumerable`1<T0>, IEqualityComparer`1<T0>)->IQueryable`1<T0>", SequenceMethod.UnionComparer);
            map.Add(@"Intersect(IQueryable`1<T0>, IEnumerable`1<T0>)->IQueryable`1<T0>", SequenceMethod.Intersect);
            map.Add(@"Intersect(IQueryable`1<T0>, IEnumerable`1<T0>, IEqualityComparer`1<T0>)->IQueryable`1<T0>", SequenceMethod.IntersectComparer);
            map.Add(@"Except(IQueryable`1<T0>, IEnumerable`1<T0>)->IQueryable`1<T0>", SequenceMethod.Except);
            map.Add(@"Except(IQueryable`1<T0>, IEnumerable`1<T0>, IEqualityComparer`1<T0>)->IQueryable`1<T0>", SequenceMethod.ExceptComparer);
            map.Add(@"First(IQueryable`1<T0>)->T0", SequenceMethod.First);
            map.Add(@"First(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->T0", SequenceMethod.FirstPredicate);
            map.Add(@"FirstOrDefault(IQueryable`1<T0>)->T0", SequenceMethod.FirstOrDefault);
            map.Add(@"FirstOrDefault(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->T0", SequenceMethod.FirstOrDefaultPredicate);
            map.Add(@"Last(IQueryable`1<T0>)->T0", SequenceMethod.Last);
            map.Add(@"Last(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->T0", SequenceMethod.LastPredicate);
            map.Add(@"LastOrDefault(IQueryable`1<T0>)->T0", SequenceMethod.LastOrDefault);
            map.Add(@"LastOrDefault(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->T0", SequenceMethod.LastOrDefaultPredicate);
            map.Add(@"Single(IQueryable`1<T0>)->T0", SequenceMethod.Single);
            map.Add(@"Single(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->T0", SequenceMethod.SinglePredicate);
            map.Add(@"SingleOrDefault(IQueryable`1<T0>)->T0", SequenceMethod.SingleOrDefault);
            map.Add(@"SingleOrDefault(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->T0", SequenceMethod.SingleOrDefaultPredicate);
            map.Add(@"ElementAt(IQueryable`1<T0>, Int32)->T0", SequenceMethod.ElementAt);
            map.Add(@"ElementAtOrDefault(IQueryable`1<T0>, Int32)->T0", SequenceMethod.ElementAtOrDefault);
            map.Add(@"DefaultIfEmpty(IQueryable`1<T0>)->IQueryable`1<T0>", SequenceMethod.DefaultIfEmpty);
            map.Add(@"DefaultIfEmpty(IQueryable`1<T0>, T0)->IQueryable`1<T0>", SequenceMethod.DefaultIfEmptyValue);
            map.Add(@"Contains(IQueryable`1<T0>, T0)->Boolean", SequenceMethod.Contains);
            map.Add(@"Contains(IQueryable`1<T0>, T0, IEqualityComparer`1<T0>)->Boolean", SequenceMethod.ContainsComparer);
            map.Add(@"Reverse(IQueryable`1<T0>)->IQueryable`1<T0>", SequenceMethod.Reverse);
            map.Add(@"SequenceEqual(IQueryable`1<T0>, IEnumerable`1<T0>)->Boolean", SequenceMethod.SequenceEqual);
            map.Add(@"SequenceEqual(IQueryable`1<T0>, IEnumerable`1<T0>, IEqualityComparer`1<T0>)->Boolean", SequenceMethod.SequenceEqualComparer);
            map.Add(@"Any(IQueryable`1<T0>)->Boolean", SequenceMethod.Any);
            map.Add(@"Any(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->Boolean", SequenceMethod.AnyPredicate);
            map.Add(@"All(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->Boolean", SequenceMethod.All);
            map.Add(@"Count(IQueryable`1<T0>)->Int32", SequenceMethod.Count);
            map.Add(@"Count(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->Int32", SequenceMethod.CountPredicate);
            map.Add(@"LongCount(IQueryable`1<T0>)->Int64", SequenceMethod.LongCount);
            map.Add(@"LongCount(IQueryable`1<T0>, Expression`1<Func`2<T0, Boolean>>)->Int64", SequenceMethod.LongCountPredicate);
            map.Add(@"Min(IQueryable`1<T0>)->T0", SequenceMethod.Min);
            map.Add(@"Min(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>)->T1", SequenceMethod.MinSelector);
            map.Add(@"Max(IQueryable`1<T0>)->T0", SequenceMethod.Max);
            map.Add(@"Max(IQueryable`1<T0>, Expression`1<Func`2<T0, T1>>)->T1", SequenceMethod.MaxSelector);
            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Int32>>)->Int32", SequenceMethod.SumIntSelector);
            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Int32>>>)->Nullable`1<Int32>", SequenceMethod.SumNullableIntSelector);
            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Int64>>)->Int64", SequenceMethod.SumLongSelector);
            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Int64>>>)->Nullable`1<Int64>", SequenceMethod.SumNullableLongSelector);
            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Single>>)->Single", SequenceMethod.SumSingleSelector);
            map.Add(@"Sum(IQueryable`1<T0>, Expression`1<Func`2<T0, Nullable`1<Single>>>)->Nullable`1<Single>", SequenceMethod.SumNullableSingleSelector);
            map.Add(@"AsQueryable(IEnumerable)->IQueryable", SequenceMethod.AsQueryable);
            map.Add(@"Sum(IQueryable`1<Int32>)->Int32", SequenceMethod.SumInt);
            map.Add(@"Sum(IQueryable`1<Nullable`1<Int32>>)->Nullable`1<Int32>", SequenceMethod.SumNullableInt);
            map.Add(@"Sum(IQueryable`1<Int64>)->Int64", SequenceMethod.SumLong);
            map.Add(@"Sum(IQueryable`1<Nullable`1<Int64>>)->Nullable`1<Int64>", SequenceMethod.SumNullableLong);
            map.Add(@"Sum(IQueryable`1<Single>)->Single", SequenceMethod.SumSingle);
            map.Add(@"Sum(IQueryable`1<Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.SumNullableSingle);
            map.Add(@"Sum(IQueryable`1<Double>)->Double", SequenceMethod.SumDouble);
            map.Add(@"Sum(IQueryable`1<Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.SumNullableDouble);
            map.Add(@"Sum(IQueryable`1<Decimal>)->Decimal", SequenceMethod.SumDecimal);
            map.Add(@"Sum(IQueryable`1<Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.SumNullableDecimal);
            map.Add(@"Average(IQueryable`1<Int32>)->Double", SequenceMethod.AverageInt);
            map.Add(@"Average(IQueryable`1<Nullable`1<Int32>>)->Nullable`1<Double>", SequenceMethod.AverageNullableInt);
            map.Add(@"Average(IQueryable`1<Int64>)->Double", SequenceMethod.AverageLong);
            map.Add(@"Average(IQueryable`1<Nullable`1<Int64>>)->Nullable`1<Double>", SequenceMethod.AverageNullableLong);
            map.Add(@"Average(IQueryable`1<Single>)->Single", SequenceMethod.AverageSingle);
            map.Add(@"Average(IQueryable`1<Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.AverageNullableSingle);
            map.Add(@"Average(IQueryable`1<Double>)->Double", SequenceMethod.AverageDouble);
            map.Add(@"Average(IQueryable`1<Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.AverageNullableDouble);
            map.Add(@"Average(IQueryable`1<Decimal>)->Decimal", SequenceMethod.AverageDecimal);
            map.Add(@"Average(IQueryable`1<Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.AverageNullableDecimal);
            map.Add(@"First(IEnumerable`1<T0>)->T0", SequenceMethod.First);
            map.Add(@"First(IEnumerable`1<T0>, Func`2<T0, Boolean>)->T0", SequenceMethod.FirstPredicate);
            map.Add(@"FirstOrDefault(IEnumerable`1<T0>)->T0", SequenceMethod.FirstOrDefault);
            map.Add(@"FirstOrDefault(IEnumerable`1<T0>, Func`2<T0, Boolean>)->T0", SequenceMethod.FirstOrDefaultPredicate);
            map.Add(@"Last(IEnumerable`1<T0>)->T0", SequenceMethod.Last);
            map.Add(@"Last(IEnumerable`1<T0>, Func`2<T0, Boolean>)->T0", SequenceMethod.LastPredicate);
            map.Add(@"LastOrDefault(IEnumerable`1<T0>)->T0", SequenceMethod.LastOrDefault);
            map.Add(@"LastOrDefault(IEnumerable`1<T0>, Func`2<T0, Boolean>)->T0", SequenceMethod.LastOrDefaultPredicate);
            map.Add(@"Single(IEnumerable`1<T0>)->T0", SequenceMethod.Single);
            map.Add(@"Single(IEnumerable`1<T0>, Func`2<T0, Boolean>)->T0", SequenceMethod.SinglePredicate);
            map.Add(@"SingleOrDefault(IEnumerable`1<T0>)->T0", SequenceMethod.SingleOrDefault);
            map.Add(@"SingleOrDefault(IEnumerable`1<T0>, Func`2<T0, Boolean>)->T0", SequenceMethod.SingleOrDefaultPredicate);
            map.Add(@"ElementAt(IEnumerable`1<T0>, Int32)->T0", SequenceMethod.ElementAt);
            map.Add(@"ElementAtOrDefault(IEnumerable`1<T0>, Int32)->T0", SequenceMethod.ElementAtOrDefault);
            map.Add(@"Repeat(T0, Int32)->IEnumerable`1<T0>", SequenceMethod.NotSupported);
            map.Add(@"Empty()->IEnumerable`1<T0>", SequenceMethod.Empty);
            map.Add(@"Any(IEnumerable`1<T0>)->Boolean", SequenceMethod.Any);
            map.Add(@"Any(IEnumerable`1<T0>, Func`2<T0, Boolean>)->Boolean", SequenceMethod.AnyPredicate);
            map.Add(@"All(IEnumerable`1<T0>, Func`2<T0, Boolean>)->Boolean", SequenceMethod.All);
            map.Add(@"Count(IEnumerable`1<T0>)->Int32", SequenceMethod.Count);
            map.Add(@"Count(IEnumerable`1<T0>, Func`2<T0, Boolean>)->Int32", SequenceMethod.CountPredicate);
            map.Add(@"LongCount(IEnumerable`1<T0>)->Int64", SequenceMethod.LongCount);
            map.Add(@"LongCount(IEnumerable`1<T0>, Func`2<T0, Boolean>)->Int64", SequenceMethod.LongCountPredicate);
            map.Add(@"Contains(IEnumerable`1<T0>, T0)->Boolean", SequenceMethod.Contains);
            map.Add(@"Contains(IEnumerable`1<T0>, T0, IEqualityComparer`1<T0>)->Boolean", SequenceMethod.ContainsComparer);
            map.Add(@"Aggregate(IEnumerable`1<T0>, Func`3<T0, T0, T0>)->T0", SequenceMethod.Aggregate);
            map.Add(@"Aggregate(IEnumerable`1<T0>, T1, Func`3<T1, T0, T1>)->T1", SequenceMethod.AggregateSeed);
            map.Add(@"Aggregate(IEnumerable`1<T0>, T1, Func`3<T1, T0, T1>, Func`2<T1, T2>)->T2", SequenceMethod.AggregateSeedSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Int32>)->Int32", SequenceMethod.SumIntSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Int32>>)->Nullable`1<Int32>", SequenceMethod.SumNullableIntSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Int64>)->Int64", SequenceMethod.SumLongSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Int64>>)->Nullable`1<Int64>", SequenceMethod.SumNullableLongSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Single>)->Single", SequenceMethod.SumSingleSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.SumNullableSingleSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Double>)->Double", SequenceMethod.SumDoubleSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.SumNullableDoubleSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Decimal>)->Decimal", SequenceMethod.SumDecimalSelector);
            map.Add(@"Sum(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.SumNullableDecimalSelector);
            map.Add(@"Min(IEnumerable`1<T0>)->T0", SequenceMethod.Min);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Int32>)->Int32", SequenceMethod.MinIntSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Int32>>)->Nullable`1<Int32>", SequenceMethod.MinNullableIntSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Int64>)->Int64", SequenceMethod.MinLongSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Int64>>)->Nullable`1<Int64>", SequenceMethod.MinNullableLongSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Single>)->Single", SequenceMethod.MinSingleSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.MinNullableSingleSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Double>)->Double", SequenceMethod.MinDoubleSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.MinNullableDoubleSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Decimal>)->Decimal", SequenceMethod.MinDecimalSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.MinNullableDecimalSelector);
            map.Add(@"Min(IEnumerable`1<T0>, Func`2<T0, T1>)->T1", SequenceMethod.MinSelector);
            map.Add(@"Max(IEnumerable`1<T0>)->T0", SequenceMethod.Max);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Int32>)->Int32", SequenceMethod.MaxIntSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Int32>>)->Nullable`1<Int32>", SequenceMethod.MaxNullableIntSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Int64>)->Int64", SequenceMethod.MaxLongSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Int64>>)->Nullable`1<Int64>", SequenceMethod.MaxNullableLongSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Single>)->Single", SequenceMethod.MaxSingleSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.MaxNullableSingleSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Double>)->Double", SequenceMethod.MaxDoubleSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.MaxNullableDoubleSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Decimal>)->Decimal", SequenceMethod.MaxDecimalSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.MaxNullableDecimalSelector);
            map.Add(@"Max(IEnumerable`1<T0>, Func`2<T0, T1>)->T1", SequenceMethod.MaxSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Int32>)->Double", SequenceMethod.AverageIntSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Int32>>)->Nullable`1<Double>", SequenceMethod.AverageNullableIntSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Int64>)->Double", SequenceMethod.AverageLongSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Int64>>)->Nullable`1<Double>", SequenceMethod.AverageNullableLongSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Single>)->Single", SequenceMethod.AverageSingleSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.AverageNullableSingleSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Double>)->Double", SequenceMethod.AverageDoubleSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.AverageNullableDoubleSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Decimal>)->Decimal", SequenceMethod.AverageDecimalSelector);
            map.Add(@"Average(IEnumerable`1<T0>, Func`2<T0, Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.AverageNullableDecimalSelector);
            map.Add(@"Where(IEnumerable`1<T0>, Func`2<T0, Boolean>)->IEnumerable`1<T0>", SequenceMethod.Where);
            map.Add(@"Where(IEnumerable`1<T0>, Func`3<T0, Int32, Boolean>)->IEnumerable`1<T0>", SequenceMethod.WhereOrdinal);
            map.Add(@"Select(IEnumerable`1<T0>, Func`2<T0, T1>)->IEnumerable`1<T1>", SequenceMethod.Select);
            map.Add(@"Select(IEnumerable`1<T0>, Func`3<T0, Int32, T1>)->IEnumerable`1<T1>", SequenceMethod.SelectOrdinal);
            map.Add(@"SelectMany(IEnumerable`1<T0>, Func`2<T0, IEnumerable`1<T1>>)->IEnumerable`1<T1>", SequenceMethod.SelectMany);
            map.Add(@"SelectMany(IEnumerable`1<T0>, Func`3<T0, Int32, IEnumerable`1<T1>>)->IEnumerable`1<T1>", SequenceMethod.SelectManyOrdinal);
            map.Add(@"SelectMany(IEnumerable`1<T0>, Func`3<T0, Int32, IEnumerable`1<T1>>, Func`3<T0, T1, T2>)->IEnumerable`1<T2>", SequenceMethod.SelectManyOrdinalResultSelector);
            map.Add(@"SelectMany(IEnumerable`1<T0>, Func`2<T0, IEnumerable`1<T1>>, Func`3<T0, T1, T2>)->IEnumerable`1<T2>", SequenceMethod.SelectManyResultSelector);
            map.Add(@"Take(IEnumerable`1<T0>, Int32)->IEnumerable`1<T0>", SequenceMethod.Take);
            map.Add(@"TakeWhile(IEnumerable`1<T0>, Func`2<T0, Boolean>)->IEnumerable`1<T0>", SequenceMethod.TakeWhile);
            map.Add(@"TakeWhile(IEnumerable`1<T0>, Func`3<T0, Int32, Boolean>)->IEnumerable`1<T0>", SequenceMethod.TakeWhileOrdinal);
            map.Add(@"Skip(IEnumerable`1<T0>, Int32)->IEnumerable`1<T0>", SequenceMethod.Skip);
            map.Add(@"SkipWhile(IEnumerable`1<T0>, Func`2<T0, Boolean>)->IEnumerable`1<T0>", SequenceMethod.SkipWhile);
            map.Add(@"SkipWhile(IEnumerable`1<T0>, Func`3<T0, Int32, Boolean>)->IEnumerable`1<T0>", SequenceMethod.SkipWhileOrdinal);
            map.Add(@"Join(IEnumerable`1<T0>, IEnumerable`1<T1>, Func`2<T0, T2>, Func`2<T1, T2>, Func`3<T0, T1, T3>)->IEnumerable`1<T3>", SequenceMethod.Join);
            map.Add(@"Join(IEnumerable`1<T0>, IEnumerable`1<T1>, Func`2<T0, T2>, Func`2<T1, T2>, Func`3<T0, T1, T3>, IEqualityComparer`1<T2>)->IEnumerable`1<T3>", SequenceMethod.JoinComparer);
            map.Add(@"GroupJoin(IEnumerable`1<T0>, IEnumerable`1<T1>, Func`2<T0, T2>, Func`2<T1, T2>, Func`3<T0, IEnumerable`1<T1>, T3>)->IEnumerable`1<T3>", SequenceMethod.GroupJoin);
            map.Add(@"GroupJoin(IEnumerable`1<T0>, IEnumerable`1<T1>, Func`2<T0, T2>, Func`2<T1, T2>, Func`3<T0, IEnumerable`1<T1>, T3>, IEqualityComparer`1<T2>)->IEnumerable`1<T3>", SequenceMethod.GroupJoinComparer);
            map.Add(@"OrderBy(IEnumerable`1<T0>, Func`2<T0, T1>)->IOrderedEnumerable`1<T0>", SequenceMethod.OrderBy);
            map.Add(@"OrderBy(IEnumerable`1<T0>, Func`2<T0, T1>, IComparer`1<T1>)->IOrderedEnumerable`1<T0>", SequenceMethod.OrderByComparer);
            map.Add(@"OrderByDescending(IEnumerable`1<T0>, Func`2<T0, T1>)->IOrderedEnumerable`1<T0>", SequenceMethod.OrderByDescending);
            map.Add(@"OrderByDescending(IEnumerable`1<T0>, Func`2<T0, T1>, IComparer`1<T1>)->IOrderedEnumerable`1<T0>", SequenceMethod.OrderByDescendingComparer);
            map.Add(@"ThenBy(IOrderedEnumerable`1<T0>, Func`2<T0, T1>)->IOrderedEnumerable`1<T0>", SequenceMethod.ThenBy);
            map.Add(@"ThenBy(IOrderedEnumerable`1<T0>, Func`2<T0, T1>, IComparer`1<T1>)->IOrderedEnumerable`1<T0>", SequenceMethod.ThenByComparer);
            map.Add(@"ThenByDescending(IOrderedEnumerable`1<T0>, Func`2<T0, T1>)->IOrderedEnumerable`1<T0>", SequenceMethod.ThenByDescending);
            map.Add(@"ThenByDescending(IOrderedEnumerable`1<T0>, Func`2<T0, T1>, IComparer`1<T1>)->IOrderedEnumerable`1<T0>", SequenceMethod.ThenByDescendingComparer);
            map.Add(@"GroupBy(IEnumerable`1<T0>, Func`2<T0, T1>)->IEnumerable`1<IGrouping`2<T1, T0>>", SequenceMethod.GroupBy);
            map.Add(@"GroupBy(IEnumerable`1<T0>, Func`2<T0, T1>, IEqualityComparer`1<T1>)->IEnumerable`1<IGrouping`2<T1, T0>>", SequenceMethod.GroupByComparer);
            map.Add(@"GroupBy(IEnumerable`1<T0>, Func`2<T0, T1>, Func`2<T0, T2>)->IEnumerable`1<IGrouping`2<T1, T2>>", SequenceMethod.GroupByElementSelector);
            map.Add(@"GroupBy(IEnumerable`1<T0>, Func`2<T0, T1>, Func`2<T0, T2>, IEqualityComparer`1<T1>)->IEnumerable`1<IGrouping`2<T1, T2>>", SequenceMethod.GroupByElementSelectorComparer);
            map.Add(@"GroupBy(IEnumerable`1<T0>, Func`2<T0, T1>, Func`3<T1, IEnumerable`1<T0>, T2>)->IEnumerable`1<T2>", SequenceMethod.GroupByResultSelector);
            map.Add(@"GroupBy(IEnumerable`1<T0>, Func`2<T0, T1>, Func`2<T0, T2>, Func`3<T1, IEnumerable`1<T2>, T3>)->IEnumerable`1<T3>", SequenceMethod.GroupByElementSelectorResultSelector);
            map.Add(@"GroupBy(IEnumerable`1<T0>, Func`2<T0, T1>, Func`3<T1, IEnumerable`1<T0>, T2>, IEqualityComparer`1<T1>)->IEnumerable`1<T2>", SequenceMethod.GroupByResultSelectorComparer);
            map.Add(@"GroupBy(IEnumerable`1<T0>, Func`2<T0, T1>, Func`2<T0, T2>, Func`3<T1, IEnumerable`1<T2>, T3>, IEqualityComparer`1<T1>)->IEnumerable`1<T3>", SequenceMethod.GroupByElementSelectorResultSelectorComparer);
            map.Add(@"Concat(IEnumerable`1<T0>, IEnumerable`1<T0>)->IEnumerable`1<T0>", SequenceMethod.Concat);
            map.Add(@"Distinct(IEnumerable`1<T0>)->IEnumerable`1<T0>", SequenceMethod.Distinct);
            map.Add(@"Distinct(IEnumerable`1<T0>, IEqualityComparer`1<T0>)->IEnumerable`1<T0>", SequenceMethod.DistinctComparer);
            map.Add(@"Union(IEnumerable`1<T0>, IEnumerable`1<T0>)->IEnumerable`1<T0>", SequenceMethod.Union);
            map.Add(@"Union(IEnumerable`1<T0>, IEnumerable`1<T0>, IEqualityComparer`1<T0>)->IEnumerable`1<T0>", SequenceMethod.UnionComparer);
            map.Add(@"Intersect(IEnumerable`1<T0>, IEnumerable`1<T0>)->IEnumerable`1<T0>", SequenceMethod.Intersect);
            map.Add(@"Intersect(IEnumerable`1<T0>, IEnumerable`1<T0>, IEqualityComparer`1<T0>)->IEnumerable`1<T0>", SequenceMethod.IntersectComparer);
            map.Add(@"Except(IEnumerable`1<T0>, IEnumerable`1<T0>)->IEnumerable`1<T0>", SequenceMethod.Except);
            map.Add(@"Except(IEnumerable`1<T0>, IEnumerable`1<T0>, IEqualityComparer`1<T0>)->IEnumerable`1<T0>", SequenceMethod.ExceptComparer);
            map.Add(@"Reverse(IEnumerable`1<T0>)->IEnumerable`1<T0>", SequenceMethod.Reverse);
            map.Add(@"SequenceEqual(IEnumerable`1<T0>, IEnumerable`1<T0>)->Boolean", SequenceMethod.SequenceEqual);
            map.Add(@"SequenceEqual(IEnumerable`1<T0>, IEnumerable`1<T0>, IEqualityComparer`1<T0>)->Boolean", SequenceMethod.SequenceEqualComparer);
            map.Add(@"AsEnumerable(IEnumerable`1<T0>)->IEnumerable`1<T0>", SequenceMethod.AsEnumerable);
            map.Add(@"ToArray(IEnumerable`1<T0>)->TSource[]", SequenceMethod.NotSupported);
            map.Add(@"ToList(IEnumerable`1<T0>)->List`1<T0>", SequenceMethod.ToList);
            map.Add(@"ToDictionary(IEnumerable`1<T0>, Func`2<T0, T1>)->Dictionary`2<T1, T0>", SequenceMethod.NotSupported);
            map.Add(@"ToDictionary(IEnumerable`1<T0>, Func`2<T0, T1>, IEqualityComparer`1<T1>)->Dictionary`2<T1, T0>", SequenceMethod.NotSupported);
            map.Add(@"ToDictionary(IEnumerable`1<T0>, Func`2<T0, T1>, Func`2<T0, T2>)->Dictionary`2<T1, T2>", SequenceMethod.NotSupported);
            map.Add(@"ToDictionary(IEnumerable`1<T0>, Func`2<T0, T1>, Func`2<T0, T2>, IEqualityComparer`1<T1>)->Dictionary`2<T1, T2>", SequenceMethod.NotSupported);
            map.Add(@"ToLookup(IEnumerable`1<T0>, Func`2<T0, T1>)->ILookup`2<T1, T0>", SequenceMethod.NotSupported);
            map.Add(@"ToLookup(IEnumerable`1<T0>, Func`2<T0, T1>, IEqualityComparer`1<T1>)->ILookup`2<T1, T0>", SequenceMethod.NotSupported);
            map.Add(@"ToLookup(IEnumerable`1<T0>, Func`2<T0, T1>, Func`2<T0, T2>)->ILookup`2<T1, T2>", SequenceMethod.NotSupported);
            map.Add(@"ToLookup(IEnumerable`1<T0>, Func`2<T0, T1>, Func`2<T0, T2>, IEqualityComparer`1<T1>)->ILookup`2<T1, T2>", SequenceMethod.NotSupported);
            map.Add(@"DefaultIfEmpty(IEnumerable`1<T0>)->IEnumerable`1<T0>", SequenceMethod.DefaultIfEmpty);
            map.Add(@"DefaultIfEmpty(IEnumerable`1<T0>, T0)->IEnumerable`1<T0>", SequenceMethod.DefaultIfEmptyValue);
            map.Add(@"OfType(IEnumerable)->IEnumerable`1<T0>", SequenceMethod.OfType);
            map.Add(@"Cast(IEnumerable)->IEnumerable`1<T0>", SequenceMethod.Cast);
            map.Add(@"Range(Int32, Int32)->IEnumerable`1<Int32>", SequenceMethod.NotSupported);
            map.Add(@"Sum(IEnumerable`1<Int32>)->Int32", SequenceMethod.SumInt);
            map.Add(@"Sum(IEnumerable`1<Nullable`1<Int32>>)->Nullable`1<Int32>", SequenceMethod.SumNullableInt);
            map.Add(@"Sum(IEnumerable`1<Int64>)->Int64", SequenceMethod.SumLong);
            map.Add(@"Sum(IEnumerable`1<Nullable`1<Int64>>)->Nullable`1<Int64>", SequenceMethod.SumNullableLong);
            map.Add(@"Sum(IEnumerable`1<Single>)->Single", SequenceMethod.SumSingle);
            map.Add(@"Sum(IEnumerable`1<Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.SumNullableSingle);
            map.Add(@"Sum(IEnumerable`1<Double>)->Double", SequenceMethod.SumDouble);
            map.Add(@"Sum(IEnumerable`1<Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.SumNullableDouble);
            map.Add(@"Sum(IEnumerable`1<Decimal>)->Decimal", SequenceMethod.SumDecimal);
            map.Add(@"Sum(IEnumerable`1<Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.SumNullableDecimal);
            map.Add(@"Min(IEnumerable`1<Int32>)->Int32", SequenceMethod.MinInt);
            map.Add(@"Min(IEnumerable`1<Nullable`1<Int32>>)->Nullable`1<Int32>", SequenceMethod.MinNullableInt);
            map.Add(@"Min(IEnumerable`1<Int64>)->Int64", SequenceMethod.MinLong);
            map.Add(@"Min(IEnumerable`1<Nullable`1<Int64>>)->Nullable`1<Int64>", SequenceMethod.MinNullableLong);
            map.Add(@"Min(IEnumerable`1<Single>)->Single", SequenceMethod.MinSingle);
            map.Add(@"Min(IEnumerable`1<Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.MinNullableSingle);
            map.Add(@"Min(IEnumerable`1<Double>)->Double", SequenceMethod.MinDouble);
            map.Add(@"Min(IEnumerable`1<Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.MinNullableDouble);
            map.Add(@"Min(IEnumerable`1<Decimal>)->Decimal", SequenceMethod.MinDecimal);
            map.Add(@"Min(IEnumerable`1<Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.MinNullableDecimal);
            map.Add(@"Max(IEnumerable`1<Int32>)->Int32", SequenceMethod.MaxInt);
            map.Add(@"Max(IEnumerable`1<Nullable`1<Int32>>)->Nullable`1<Int32>", SequenceMethod.MaxNullableInt);
            map.Add(@"Max(IEnumerable`1<Int64>)->Int64", SequenceMethod.MaxLong);
            map.Add(@"Max(IEnumerable`1<Nullable`1<Int64>>)->Nullable`1<Int64>", SequenceMethod.MaxNullableLong);
            map.Add(@"Max(IEnumerable`1<Double>)->Double", SequenceMethod.MaxDouble);
            map.Add(@"Max(IEnumerable`1<Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.MaxNullableDouble);
            map.Add(@"Max(IEnumerable`1<Single>)->Single", SequenceMethod.MaxSingle);
            map.Add(@"Max(IEnumerable`1<Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.MaxNullableSingle);
            map.Add(@"Max(IEnumerable`1<Decimal>)->Decimal", SequenceMethod.MaxDecimal);
            map.Add(@"Max(IEnumerable`1<Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.MaxNullableDecimal);
            map.Add(@"Average(IEnumerable`1<Int32>)->Double", SequenceMethod.AverageInt);
            map.Add(@"Average(IEnumerable`1<Nullable`1<Int32>>)->Nullable`1<Double>", SequenceMethod.AverageNullableInt);
            map.Add(@"Average(IEnumerable`1<Int64>)->Double", SequenceMethod.AverageLong);
            map.Add(@"Average(IEnumerable`1<Nullable`1<Int64>>)->Nullable`1<Double>", SequenceMethod.AverageNullableLong);
            map.Add(@"Average(IEnumerable`1<Single>)->Single", SequenceMethod.AverageSingle);
            map.Add(@"Average(IEnumerable`1<Nullable`1<Single>>)->Nullable`1<Single>", SequenceMethod.AverageNullableSingle);
            map.Add(@"Average(IEnumerable`1<Double>)->Double", SequenceMethod.AverageDouble);
            map.Add(@"Average(IEnumerable`1<Nullable`1<Double>>)->Nullable`1<Double>", SequenceMethod.AverageNullableDouble);
            map.Add(@"Average(IEnumerable`1<Decimal>)->Decimal", SequenceMethod.AverageDecimal);
            map.Add(@"Average(IEnumerable`1<Nullable`1<Decimal>>)->Nullable`1<Decimal>", SequenceMethod.AverageNullableDecimal);

            s_methodMap = new Dictionary<MethodInfo, SequenceMethod>(EqualityComparer<MethodInfo>.Default);
            s_inverseMap = new Dictionary<SequenceMethod, MethodInfo>(EqualityComparer<SequenceMethod>.Default);
            foreach (MethodInfo method in GetAllLinqOperators())
            {
                SequenceMethod sequenceMethod;
                string canonicalMethod = GetCanonicalMethodDescription(method);
                if (map.TryGetValue(canonicalMethod, out sequenceMethod))
                {
                    s_methodMap.Add(method, sequenceMethod);
                    s_inverseMap[sequenceMethod] = method;
                }
            }
        }
        #endregion

        internal static bool TryIdentifySequenceMethod(MethodInfo method, out SequenceMethod sequenceMethod)
        {
            method = method.IsGenericMethod ? method.GetGenericMethodDefinition() :
                method;
            return s_methodMap.TryGetValue(method, out sequenceMethod);
        }

        internal static bool IsSequenceMethod(MethodInfo method, SequenceMethod sequenceMethod)
        {
            bool result;
            SequenceMethod foundSequenceMethod;
            if (ReflectionUtil.TryIdentifySequenceMethod(method, out foundSequenceMethod))
            {
                result = foundSequenceMethod == sequenceMethod;
            }
            else
            {
                result = false;
            }

            return result;
        }

#if false
        internal static bool TryIdentifySequenceMethod(Expression expression, bool unwrapLambda, out SequenceMethod sequenceMethod)
        {
            if (expression.NodeType == ExpressionType.Lambda && unwrapLambda)
            {
                expression = ((LambdaExpression)expression).Body;
            }

            if (expression.NodeType == ExpressionType.Call)
            {
                MethodCallExpression methodCall = (MethodCallExpression)expression;
                return ReflectionUtil.TryIdentifySequenceMethod(methodCall.Method, out sequenceMethod);
            }

            sequenceMethod = default(SequenceMethod);
            return false;
        }

        internal static bool TryLookupMethod(SequenceMethod sequenceMethod, out MethodInfo method)
        {
            return s_inverseMap.TryGetValue(sequenceMethod, out method);
        }
#endif

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.MSInternal", "CA908:AvoidTypesThatRequireJitCompilationInPrecompiledAssemblies")]
        internal static string GetCanonicalMethodDescription(MethodInfo method)
        {
            Dictionary<Type, int> genericArgumentOrdinals = null;
            if (method.IsGenericMethodDefinition)
            {
                genericArgumentOrdinals = method.GetGenericArguments()
                    .Where(t => t.IsGenericParameter)
                    .Select((t, i) => new KeyValuePair<Type, int>(t, i))
                    .ToDictionary(r => r.Key, r => r.Value);
            }

            StringBuilder description = new StringBuilder();
            description.Append(method.Name).Append("(");

            bool first = true;
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                if (first) { first = false; }
                else { description.Append(", "); }
                AppendCanonicalTypeDescription(parameter.ParameterType, genericArgumentOrdinals, description);
            }

            description.Append(")");

            if (null != method.ReturnType)
            {
                description.Append("->");
                AppendCanonicalTypeDescription(method.ReturnType, genericArgumentOrdinals, description);
            }

            return description.ToString();
        }

        private static void AppendCanonicalTypeDescription(Type type, Dictionary<Type, int> genericArgumentOrdinals, StringBuilder description)
        {
            int ordinal;

            if (null != genericArgumentOrdinals && genericArgumentOrdinals.TryGetValue(type, out ordinal))
            {
                description.Append("T").Append(ordinal.ToString(CultureInfo.InvariantCulture));
                return;
            }

            description.Append(type.Name);

            if (type.IsGenericType)
            {
                description.Append("<");
                bool first = true;
                foreach (Type genericArgument in type.GetGenericArguments())
                {
                    if (first) { first = false; }
                    else { description.Append(", "); }
                    AppendCanonicalTypeDescription(genericArgument, genericArgumentOrdinals, description);
                }
                description.Append(">");
            }
        }

        internal static IEnumerable<MethodInfo> GetAllLinqOperators()
        {
            return typeof(Queryable).GetMethods(BindingFlags.Static | BindingFlags.Public).Concat(
                typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public));
        }
    }

    internal enum SequenceMethod
    {
        Where,
        WhereOrdinal,
        OfType,
        Cast,
        Select,
        SelectOrdinal,
        SelectMany,
        SelectManyOrdinal,
        SelectManyResultSelector,
        SelectManyOrdinalResultSelector,
        Join,
        JoinComparer,
        GroupJoin,
        GroupJoinComparer,
        OrderBy,
        OrderByComparer,
        OrderByDescending,
        OrderByDescendingComparer,
        ThenBy,
        ThenByComparer,
        ThenByDescending,
        ThenByDescendingComparer,
        Take,
        TakeWhile,
        TakeWhileOrdinal,
        Skip,
        SkipWhile,
        SkipWhileOrdinal,
        GroupBy,
        GroupByComparer,
        GroupByElementSelector,
        GroupByElementSelectorComparer,
        GroupByResultSelector,
        GroupByResultSelectorComparer,
        GroupByElementSelectorResultSelector,
        GroupByElementSelectorResultSelectorComparer,
        Distinct,
        DistinctComparer,
        Concat,
        Union,
        UnionComparer,
        Intersect,
        IntersectComparer,
        Except,
        ExceptComparer,
        First,
        FirstPredicate,
        FirstOrDefault,
        FirstOrDefaultPredicate,
        Last,
        LastPredicate,
        LastOrDefault,
        LastOrDefaultPredicate,
        Single,
        SinglePredicate,
        SingleOrDefault,
        SingleOrDefaultPredicate,
        ElementAt,
        ElementAtOrDefault,
        DefaultIfEmpty,
        DefaultIfEmptyValue,
        Contains,
        ContainsComparer,
        Reverse,
        Empty,
        SequenceEqual,
        SequenceEqualComparer,

        Any,
        AnyPredicate,
        All,

        Count,
        CountPredicate,
        LongCount,
        LongCountPredicate,

        Min,
        MinSelector,
        Max,
        MaxSelector,

        MinInt,
        MinNullableInt,
        MinLong,
        MinNullableLong,
        MinDouble,
        MinNullableDouble,
        MinDecimal,
        MinNullableDecimal,
        MinSingle,
        MinNullableSingle,
        MinIntSelector,
        MinNullableIntSelector,
        MinLongSelector,
        MinNullableLongSelector,
        MinDoubleSelector,
        MinNullableDoubleSelector,
        MinDecimalSelector,
        MinNullableDecimalSelector,
        MinSingleSelector,
        MinNullableSingleSelector,

        MaxInt,
        MaxNullableInt,
        MaxLong,
        MaxNullableLong,
        MaxDouble,
        MaxNullableDouble,
        MaxDecimal,
        MaxNullableDecimal,
        MaxSingle,
        MaxNullableSingle,
        MaxIntSelector,
        MaxNullableIntSelector,
        MaxLongSelector,
        MaxNullableLongSelector,
        MaxDoubleSelector,
        MaxNullableDoubleSelector,
        MaxDecimalSelector,
        MaxNullableDecimalSelector,
        MaxSingleSelector,
        MaxNullableSingleSelector,

        SumInt,
        SumNullableInt,
        SumLong,
        SumNullableLong,
        SumDouble,
        SumNullableDouble,
        SumDecimal,
        SumNullableDecimal,
        SumSingle,
        SumNullableSingle,
        SumIntSelector,
        SumNullableIntSelector,
        SumLongSelector,
        SumNullableLongSelector,
        SumDoubleSelector,
        SumNullableDoubleSelector,
        SumDecimalSelector,
        SumNullableDecimalSelector,
        SumSingleSelector,
        SumNullableSingleSelector,

        AverageInt,
        AverageNullableInt,
        AverageLong,
        AverageNullableLong,
        AverageDouble,
        AverageNullableDouble,
        AverageDecimal,
        AverageNullableDecimal,
        AverageSingle,
        AverageNullableSingle,
        AverageIntSelector,
        AverageNullableIntSelector,
        AverageLongSelector,
        AverageNullableLongSelector,
        AverageDoubleSelector,
        AverageNullableDoubleSelector,
        AverageDecimalSelector,
        AverageNullableDecimalSelector,
        AverageSingleSelector,
        AverageNullableSingleSelector,

        Aggregate,
        AggregateSeed,
        AggregateSeedSelector,

        AsQueryable,
        AsQueryableGeneric,
        AsEnumerable,

        ToList,

        NotSupported,
    }
}
