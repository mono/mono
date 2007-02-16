// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
//
// Authors:
//      Alejandro Serrano "Serras" (trupill@yahoo.es)
//      Marek Safar (marek.safar@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Linq
{
        public static class Queryable
        {
                #region Count
                
                public static int Count<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        if (source is ICollection<TSource>)
                                return ((ICollection<TSource>)source).Count;
                        
                        int counter = 0;
                        foreach (TSource element in source)
                                counter++;
                        return counter;
                }
                
                public static int Count<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        foreach (TSource element in source)
                                if (selector(element))
                                        counter++;
                        
                        return counter;
                }
                
                #endregion
                
                #region LongCount
                
                public static long LongCount<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long counter = 0;
                        foreach (TSource element in source)
                                counter++;
                        return counter;
                }
                
                public static long LongCount<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long counter = 0;
                        foreach (TSource element in source)
                                if (selector(element))
                                        counter++;
                        
                        return counter;
                }
                
                #endregion
                
                #region Sum
                
                public static int Sum (this IQueryable<int> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        int sum = 0;
                        foreach (int element in source)
                                sum += element;
                        
                        return sum;
                }
                
                public static int Sum<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, int> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int sum = 0;
                        foreach (TSource element in source)
                                sum += selector (element);
                        
                        return sum;
                }
                
                public static int? Sum (this IQueryable<int?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        int? sum = 0;
                        foreach (int? element in source)
                                if (element.HasValue)
                                        sum += element.Value;
                        
                        return sum;
                }
                
                public static int? Sum<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, int?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int? sum = 0;
                        foreach (TSource element in source) {
                                int? item = selector (element);
                                if (item.HasValue)
                                        sum += item.Value;
                        }
                        
                        return sum;
                }

                public static long Sum (this IQueryable<long> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        foreach (long element in source)
                                sum += element;
                        
                        return sum;
                }
                
                public static long Sum<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, long> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        foreach (TSource element in source)
                                sum += selector (element);
                        
                        return sum;
                }
                
                public static long? Sum (this IQueryable<long?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long? sum = 0;
                        foreach (long? element in source)
                                if (element.HasValue)
                                        sum += element.Value;
                        
                        return sum;
                }
                
                public static long? Sum<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, long?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long? sum = 0;
                        foreach (TSource element in source) {
                                long? item = selector (element);
                                if (item.HasValue)
                                        sum += item.Value;
                        }
                        
                        return sum;
                }
                
                public static double Sum (this IQueryable<double> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        double sum = 0;
                        foreach (double element in source)
                                sum += element;
                        
                        return sum;
                }
                
                public static double Sum<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, double> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double sum = 0;
                        foreach (TSource element in source)
                                sum += selector (element);
                        
                        return sum;
                }
                
                public static double? Sum (this IQueryable<double?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        double? sum = 0;
                        foreach (double? element in source)
                                if (element.HasValue)
                                        sum += element.Value;
                        
                        return sum;
                }
                
                public static double? Sum<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, double?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double? sum = 0;
                        foreach (TSource element in source) {
                                double? item = selector (element);
                                if (item.HasValue)
                                        sum += item.Value;
                        }
                        
                        return sum;
                }
                
                public static decimal Sum (this IQueryable<decimal> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        decimal sum = 0;
                        foreach (decimal element in source)
                                sum += element;
                        
                        return sum;
                }
                
                public static decimal Sum<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, decimal> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal sum = 0;
                        foreach (TSource element in source)
                                sum += selector (element);
                        
                        return sum;
                }
                
                public static decimal? Sum (this IQueryable<decimal?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        decimal? sum = 0;
                        foreach (decimal? element in source)
                                if (element.HasValue)
                                        sum += element.Value;
                        
                        return sum;
                }
                
                public static decimal? Sum<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, decimal?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal? sum = 0;
                        foreach (TSource element in source) {
                                decimal? item = selector (element);
                                if (item.HasValue)
                                        sum += item.Value;
                        }
                        
                        return sum;
                }
                
                #endregion
                
                #region Min
                
                public static int Min (this IQueryable<int> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        int minimum = int.MaxValue;
                        int counter = 0;
                        foreach (int element in source) {
                                if (element < minimum)
                                        minimum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                public static int? Min (this IQueryable<int?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        int? minimum = int.MaxValue;
                        foreach (int? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                public static long Min (this IQueryable<long> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long minimum = long.MaxValue;
                        int counter = 0;
                        foreach (long element in source) {
                                if (element < minimum)
                                        minimum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                public static long? Min (this IQueryable<long?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long? minimum = long.MaxValue;
                        foreach (long? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                public static double Min (this IQueryable<double> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        double minimum = double.MaxValue;
                        int counter = 0;
                        foreach (double element in source) {
                                if (element < minimum)
                                        minimum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                public static double? Min (this IQueryable<double?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        double? minimum = double.MaxValue;
                        foreach (double? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                public static decimal Min (this IQueryable<decimal> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        decimal minimum = decimal.MaxValue;
                        int counter = 0;
                        foreach (decimal element in source) {
                                if (element < minimum)
                                        minimum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                public static decimal? Min (this IQueryable<decimal?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        decimal? minimum = decimal.MaxValue;
                        foreach (decimal? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                public static TSource Min<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool notAssigned = true;
                        TSource minimum = default (TSource);
                        int counter = 0;
                        foreach (TSource element in source) {
                                if (notAssigned) {
                                        minimum = element;
                                        notAssigned = false;
                                }
                                else {
                                        int comparison;
                                        if (element is IComparable<TSource>)
                                                comparison = ((IComparable<TSource>)element).CompareTo (minimum);
                                        else if (element is System.IComparable)
                                                comparison = ((System.IComparable)element).CompareTo (minimum);
                                        else
                                                throw new ArgumentNullException();
                                        
                                        if (comparison < 0)
                                                minimum = element;
                                }
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                public static int Min<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, int> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int minimum = int.MaxValue;
                        int counter = 0;
                        foreach (TSource item in source) {
                                int element = selector (item);
                                if (element < minimum)
                                        minimum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                public static int? Min<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, int?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        int? minimum = int.MaxValue;
                        foreach (TSource item in source) {
                                int? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                public static long Min<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, long> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long minimum = long.MaxValue;
                        int counter = 0;
                        foreach (TSource item in source) {
                                long element = selector (item);
                                if (element < minimum)
                                        minimum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                public static long? Min<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, long?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long? minimum = long.MaxValue;
                        foreach (TSource item in source) {
                                long? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                public static double Min<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, double> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double minimum = double.MaxValue;
                        int counter = 0;
                        foreach (TSource item in source)
                        {
                                double element = selector (item);
                                if (element < minimum)
                                        minimum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                public static double? Min<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, double?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        double? minimum = double.MaxValue;
                        foreach (TSource item in source) {
                                double? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                public static decimal Min<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, decimal> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal minimum = decimal.MaxValue;
                        int counter = 0;
                        foreach (TSource item in source) {
                                decimal element = selector (item);
                                if (element < minimum)
                                        minimum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                public static decimal? Min<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, decimal?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        decimal? minimum = decimal.MaxValue;
                        foreach (TSource item in source) {
                                decimal? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                public static TResult Min<TSource, TResult> (
                        this IQueryable<TSource> source,
                        Func<TSource, TResult> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool notAssigned = true;
                        TResult minimum = default (TResult);
                        int counter = 0;
                        foreach (TSource item in source) {
                                TResult element = selector (item);
                                if (notAssigned) {
                                        minimum = element;
                                        notAssigned = false;
                                }
                                else {
                                        int comparison;
                                        if (element is IComparable<TResult>)
                                                comparison = ((IComparable<TResult>)element).CompareTo (minimum);
                                        else if (element is System.IComparable)
                                                comparison = ((System.IComparable)element).CompareTo (minimum);
                                        else
                                                throw new ArgumentNullException ();
                                        
                                        if (comparison < 0)
                                                minimum = element;
                                }
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return minimum;
                }
                
                #endregion
                
                #region Max
                
                public static int Max (this IQueryable<int> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        int maximum = int.MinValue;
                        int counter = 0;
                        foreach (int element in source) {
                                if (element > maximum)
                                        maximum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                
                public static int? Max (this IQueryable<int?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        int? maximum = int.MinValue;
                        foreach (int? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                public static long Max (this IQueryable<long> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long maximum = long.MinValue;
                        int counter = 0;
                        foreach (long element in source) {
                                if (element > maximum)
                                        maximum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                
                public static long? Max (this IQueryable<long?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long? maximum = long.MinValue;
                        foreach (long? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                public static double Max (
                        this IQueryable<double> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        double maximum = double.MinValue;
                        int counter = 0;
                        foreach (double element in source) {
                                if (element > maximum)
                                        maximum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                
                public static double? Max (
                        this IQueryable<double?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        double? maximum = double.MinValue;
                        foreach (double? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                public static decimal Max (
                        this IQueryable<decimal> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        decimal maximum = decimal.MinValue;
                        int counter = 0;
                        foreach (decimal element in source) {
                                if (element > maximum)
                                        maximum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                
                public static decimal? Max (
                        this IQueryable<decimal?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        decimal? maximum = decimal.MinValue;
                        foreach (decimal? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                public static TSource Max<TSource> (
                        this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool notAssigned = true;
                        TSource maximum = default (TSource);
                        int counter = 0;
                        foreach (TSource element in source) {
                                if (notAssigned) {
                                        maximum = element;
                                        notAssigned = false;
                                }
                                else {
                                        int comparison;
                                        if (element is IComparable<TSource>)
                                                comparison = ((IComparable<TSource>)element).CompareTo (maximum);
                                        else if (element is System.IComparable)
                                                comparison = ((System.IComparable)element).CompareTo (maximum);
                                        else
                                                throw new ArgumentNullException();
                                        
                                        if (comparison > 0)
                                                maximum = element;
                                }
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                
                public static int Max<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, int> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int maximum = int.MinValue;
                        int counter = 0;
                        foreach (TSource item in source)
                        {
                                int element = selector (item);
                                if (element > maximum)
                                        maximum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                
                public static int? Max<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, int?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        int? maximum = int.MinValue;
                        foreach (TSource item in source) {
                                int? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                public static long Max<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, long> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long maximum = long.MinValue;
                        int counter = 0;
                        foreach (TSource item in source) {
                                long element = selector (item);
                                if (element > maximum)
                                        maximum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                
                public static long? Max<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, long?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long? maximum = long.MinValue;
                        foreach (TSource item in source) {
                                long? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                public static double Max<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, double> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double maximum = double.MinValue;
                        int counter = 0;
                        foreach (TSource item in source) {
                                double element = selector (item);
                                if (element > maximum)
                                        maximum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                
                public static double? Max<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, double?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        double? maximum = double.MinValue;
                        foreach (TSource item in source) {
                                double? element = selector(item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                public static decimal Max<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, decimal> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal maximum = decimal.MinValue;
                        int counter = 0;
                        foreach (TSource item in source) {
                                decimal element = selector(item);
                                if (element > maximum)
                                        maximum = element;
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                
                public static decimal? Max<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, decimal?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        decimal? maximum = decimal.MinValue;
                        foreach (TSource item in source) {
                                decimal? element = selector(item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                public static TResult Max<TSource, TResult> (
                        this IQueryable<TSource> source,
                        Func<TSource, TResult> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool notAssigned = true;
                        TResult maximum = default (TResult);
                        int counter = 0;
                        foreach (TSource item in source)
                        {
                                TResult element = selector (item);
                                if (notAssigned)  {
                                        maximum = element;
                                        notAssigned = false;
                                }
                                else  {
                                        int comparison;
                                        if (element is IComparable<TResult>)
                                                comparison = ((IComparable<TResult>)element).CompareTo (maximum);
                                        else if (element is System.IComparable)
                                                comparison = ((System.IComparable)element).CompareTo (maximum);
                                        else
                                                throw new ArgumentNullException();
                                        
                                        if (comparison > 0)
                                                maximum = element;
                                }
                                       counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return maximum;
                }
                                
                #endregion
        
                #region Average
                
                public static double Average (this IQueryable<int> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        long counter = 0;
                        foreach (int element in source) {
                                sum += element;                        
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return (double)sum / (double)counter;
                }
                
                public static double? Average (this IQueryable<int?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long sum = 0;
                        long counter = 0;
                        foreach (int? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (double?)sum / (double?)counter);
                }
                
                public static double Average (this IQueryable<long> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        long counter = 0;
                        foreach (long element in source) {
                                sum += element;                        
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return (double)sum / (double)counter;
                }
                
                public static double? Average (this IQueryable<long?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long sum = 0;
                        long counter = 0;
                        foreach (long? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (double?)sum / (double?)counter);
                }
                
                public static double Average (this IQueryable<double> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        double sum = 0;
                        double counter = 0;
                        foreach (double element in source) {
                                sum += element;                        
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return sum / counter;
                }
                
                public static double? Average (this IQueryable<double?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        double sum = 0;
                        double counter = 0;
                        foreach (double? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (double?)(sum / counter));
                }
                
                public static decimal Average (this IQueryable<decimal> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        decimal sum = 0;
                        decimal counter = 0;
                        foreach (decimal element in source) {
                                sum += element;                        
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return sum / counter;
                }
                
                public static decimal? Average (this IQueryable<decimal?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        decimal sum = 0;
                        decimal counter = 0;
                        foreach (decimal? element in source) {
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (decimal?)(sum / counter));
                }
                
                public static double Average<TSource> (this IQueryable<TSource> source,
                        Func<TSource, int> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        long counter = 0;
                        foreach (TSource item in source) {
                                sum += selector (item);
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return (double)sum / (double)counter;
                }
                
                public static double? Average<TSource> (this IQueryable<TSource> source,
                        Func<TSource, int?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long sum = 0;
                        long counter = 0;
                        foreach (TSource item in source) {
                                int? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (double?)sum / (double?)counter);
                }
                
                public static double Average<TSource> (this IQueryable<TSource> source,
                        Func<TSource, long> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        long counter = 0;
                        foreach (TSource item in source) {
                                sum += selector (item);
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException();
                        else
                                return (double)sum / (double)counter;
                }
                
                public static double? Average<TSource> (this IQueryable<TSource> source,
                        Func<TSource, long?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long sum = 0;
                        long counter = 0;
                        foreach (TSource item in source) {
                                long? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (double?)sum/(double?)counter);
                }
                
                public static double Average<TSource> (this IQueryable<TSource> source,
                        Func<TSource, double> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double sum = 0;
                        double counter = 0;
                        foreach (TSource item in source) {
                                sum += selector (item);
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return sum / counter;
                }
                
                public static double? Average<TSource> (this IQueryable<TSource> source,
                        Func<TSource, double?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        double sum = 0;
                        double counter = 0;
                        foreach (TSource item in source) {
                                double? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (double?)(sum/counter));
                }
                
                public static decimal Average<TSource> (this IQueryable<TSource> source,
                        Func<TSource, decimal> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal sum = 0;
                        decimal counter = 0;
                        foreach (TSource item in source) {
                                sum += selector(item);
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return sum / counter;
                }
                
                public static decimal? Average<TSource> (this IQueryable<TSource> source,
                        Func<TSource, decimal?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        decimal sum = 0;
                        decimal counter = 0;
                        foreach (TSource item in source) {
                                decimal? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (decimal?)(sum/counter));
                }
                
                #endregion
                
                #region Fold
/*                
                [Obsolete ("Use Aggregate instead")]
                [System.Runtime.CompilerServices.Extension]
                public static TSource Fold<TSource> (
                        IQueryable<TSource> source,
                        Func<TSource, TSource, TSource> func)
                {
                        return Fold<TSource> (source, func);
                }
                
                [Obsolete ("Use Aggregate instead")]
                [System.Runtime.CompilerServices.Extension]
                public static U Fold<TSource, U> (
                        IQueryable<TSource> source,
                        U seed,
                        Func<U, TSource, U> func)
                {
                        return Fold<TSource, U> (source, seed, func);
                }
*/                
                #endregion
                
                #region Aggregate
                
                public static TSource Aggregate<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, TSource, TSource> func)
                {
                        if (source == null || func == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        TSource folded = default (TSource);
                        
                        foreach (TSource element in source) {
                                if (counter == 0)
                                        folded = element;
                                else
                                        folded = func (folded, element);
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return folded;
                }
                
                public static U Aggregate<TSource, U> (
                        this IQueryable<TSource> source,
                        U seed,
                        Func<U, TSource, U> func)
                {
                        if (source == null || func == null)
                                throw new ArgumentNullException ();
                        
                        U folded = seed;
                        foreach (TSource element in source)
                                folded = func (folded, element);
                        return folded;
                }
                
                #endregion

                #region Concat
                
                public static IEnumerable<TSource> Concat<TSource> (
                        this IQueryable<TSource> first,
                        IEnumerable<TSource> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in first)
                                yield return element;
                        foreach (TSource element in second)
                                yield return element;
                }
                
                #endregion
/*
                #region ToSequence
                
                [System.Runtime.CompilerServices.Extension]
                public static IQueryable<TSource> ToSequence<TSource> (
                        IQueryable<TSource> source)
                {
                        return (IQueryable<TSource>)source;
                }
               
                #endregion
                
                #region ToArray
                
                [System.Runtime.CompilerServices.Extension]
                public static TSource[] ToArray<TSource> (
                        IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        List<TSource> list = new List<TSource> (source);
                        return list.ToArray ();
                }
                
                #endregion
                
                #region ToList
                
                [System.Runtime.CompilerServices.Extension]
                public static List<TSource> ToList<TSource> (
                        IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        return new List<TSource> (source);
                }
                
                #endregion
                
                #region ToDictionary
                
                [System.Runtime.CompilerServices.Extension]
                public static Dictionary<K, TSource> ToDictionary<TSource, K> (
                        IQueryable<TSource> source,
                        Func<TSource, K> keySelector)
                {
                        return ToDictionary<TSource, K> (source, keySelector, null);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static Dictionary<K, TSource> ToDictionary<TSource, K> (
                        IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, TSource> dictionary = new Dictionary<K, TSource> (comparer ?? EqualityComparer<K>.Default);
                        foreach (TSource element in source) {
                                K key = keySelector (element);
                                if (key == null)
                                        throw new ArgumentNullException ();
                                else if (dictionary.ContainsKey (key))
                                        throw new ArgumentException ();
                                else
                                        dictionary.Add (key, element);
                        }
                        return dictionary;
                }

                [System.Runtime.CompilerServices.Extension]
                public static Dictionary<K, E> ToDictionary<TSource, K, E> (
                        IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        Func<TSource, E> elementSelector)
                {
                        return ToDictionary<TSource, K, E> (source, keySelector, elementSelector, null);
                }
                                
                [System.Runtime.CompilerServices.Extension]
                public static Dictionary<K, E> ToDictionary<TSource, K, E> (
                        IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        Func<TSource, E> elementSelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null || elementSelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, E> dictionary = new Dictionary<K, E>(comparer ?? EqualityComparer<K>.Default);
                        foreach (TSource element in source)
                        {
                                K key = keySelector (element);
                                if (key == null)
                                        throw new ArgumentNullException ();
                                else if (dictionary.ContainsKey (key))
                                        throw new ArgumentException ();
                                else
                                        dictionary.Add(key, elementSelector (element));
                        }
                        return dictionary;
                }
                
                #endregion
                
                #region ToLookup
                
                [System.Runtime.CompilerServices.Extension]
                public static Lookup<K, TSource> ToLookup<TSource, K> (
                        IQueryable<TSource> source,
                        Func<TSource, K> keySelector)
                {
                        return ToLookup<TSource, K> (source, keySelector, null);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static Lookup<K, TSource> ToLookup<TSource, K> (
                        IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, List<TSource>> dictionary = new Dictionary<K, List<TSource>> (comparer ?? EqualityComparer<K>.Default);
                        foreach (TSource element in source) {
                                K key = keySelector (element);
                                if (key == null)
                                        throw new ArgumentNullException ();
                                if (!dictionary.ContainsKey (key))
                                        dictionary.Add (key, new List<TSource> ());
                                dictionary[key].Add (element);
                        }
                        return new Lookup<K, TSource> (dictionary);
                }

                [System.Runtime.CompilerServices.Extension]
                public static Lookup<K, E> ToLookup<TSource, K, E> (
                        IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        Func<TSource, E> elementSelector)
                {
                        return ToLookup<TSource, K, E> (source, keySelector, elementSelector, null);
                }
                                
                [System.Runtime.CompilerServices.Extension]
                public static Lookup<K, E> ToLookup<TSource, K, E> (
                        IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        Func<TSource, E> elementSelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null || elementSelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, List<E>> dictionary = new Dictionary<K, List<E>>(comparer ?? EqualityComparer<K>.Default);
                        foreach (TSource element in source)
                        {
                                K key = keySelector (element);
                                if (key == null)
                                        throw new ArgumentNullException ();
                                if (!dictionary.ContainsKey (key))
                                        dictionary.Add (key, new List<E> ());
                                dictionary[key].Add (elementSelector (element));
                        }
                        return new Lookup<K, E> (dictionary);
                }
                
                #endregion
  */              
                #region OfType
                
                public static IEnumerable<TResult> OfType<TResult> (this IQueryable source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (object element in source)
                                if (element is TResult)
                                        yield return (TResult)element;
                }
                
                #endregion
                
                #region Cast
                
                public static IEnumerable<TResult> Cast<TResult> (this IQueryable source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (object element in source)
                                yield return (TResult)element;
                }
                
                #endregion

                #region First
                
                public static TSource First<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source)
                                return element;
                        
                        throw new InvalidOperationException ();
                }
                
                public static TSource First<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source) {
                                if (predicate (element))
                                        return element;
                        }
                        
                        throw new InvalidOperationException ();
                }
                
                #endregion
                
                #region FirstOrDefault
                
                public static TSource FirstOrDefault<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source)
                                return element;
                        
                        return default (TSource);
                }
                
                public static TSource FirstOrDefault<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source) {
                                if (predicate (element))
                                        return element;
                        }
                        
                        return default (TSource);
                }
                
                #endregion
                
                #region Last
                
                public static TSource Last<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool noElements = true;
                        TSource lastElement = default (TSource);
                        foreach (TSource element in source)
                        {
                                if (noElements) noElements = false;
                                lastElement = element;
                        }
                        
                        if (!noElements)
                                return lastElement;
                        else
                                throw new InvalidOperationException();
                }
                
                public static TSource Last<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        bool noElements = true;
                        TSource lastElement = default (TSource);
                        foreach (TSource element in source) {
                                if (predicate (element))
                                {
                                        if (noElements) noElements = false;
                                        lastElement = element;
                                }
                        }
                        
                        if (!noElements)
                                return lastElement;
                        else
                                throw new InvalidOperationException ();
                }
                
                #endregion
                
                #region LastOrDefault
                
                public static TSource LastOrDefault<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        TSource lastElement = default (TSource);
                        foreach (TSource element in source)
                                lastElement = element;
                        
                        return lastElement;
                }
                
                public static TSource LastOrDefault<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        TSource lastElement = default (TSource);
                        foreach (TSource element in source) {
                                if (predicate (element))
                                        lastElement = element;
                        }
                        
                        return lastElement;
                }
                
                #endregion
                
                #region Single
                
                public static TSource Single<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool otherElement = false;
                        TSource singleElement = default (TSource);
                        foreach (TSource element in source)
                        {
                                if (otherElement) throw new InvalidOperationException ();
                                if (!otherElement) otherElement = true;
                                singleElement = element;
                        }
                        
                        if (otherElement)
                                return singleElement;
                        else
                                throw new InvalidOperationException();
                }
                
                public static TSource Single<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        bool otherElement = false;
                        TSource singleElement = default (TSource);
                        foreach (TSource element in source) {
                                if (predicate (element))
                                {
                                        if (otherElement) throw new InvalidOperationException ();
                                        if (!otherElement) otherElement = true;
                                        singleElement = element;
                                }
                        }
                        
                        if (otherElement)
                                return singleElement;
                        else
                                throw new InvalidOperationException ();
                }
                
                #endregion
                
                #region SingleOrDefault
                
                public static TSource SingleOrDefault<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool otherElement = false;
                        TSource singleElement = default (TSource);
                        foreach (TSource element in source)
                        {
                                if (otherElement) throw new InvalidOperationException ();
                                if (!otherElement) otherElement = true;
                                singleElement = element;
                        }
                        
                        return singleElement;
                }
                
                public static TSource SingleOrDefault<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        bool otherElement = false;
                        TSource singleElement = default (TSource);
                        foreach (TSource element in source) {
                                if (predicate (element))
                                {
                                        if (otherElement) throw new InvalidOperationException ();
                                        if (!otherElement) otherElement = true;
                                        singleElement = element;
                                }
                        }
                        
                        return singleElement;
                }
                
                #endregion
                
                #region ElementAt
                
                public static TSource ElementAt<TSource> (
                        this IQueryable<TSource> source,
                        int index)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        if (index < 0)
                                throw new ArgumentOutOfRangeException ();
                        
                        if (source is IList<TSource>)
                                return ((IList<TSource>)source)[index];
                        else {
                                int counter = 0;
                                foreach (TSource element in source) {
                                        if (counter == index)
                                                return element;
                                        counter++;
                                }
                                throw new ArgumentOutOfRangeException();
                        }
                }
                
                #endregion
                
                #region ElementAtOrDefault
                
                public static TSource ElementAtOrDefault<TSource> (
                        this IQueryable<TSource> source,
                        int index)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        if (index < 0)
                                return default(TSource);
                        
                        if (source is IList<TSource>)
                        {
                                if (((IList<TSource>)source).Count >= index)
                                        return default(TSource);
                                else
                                        return ((IList<TSource>)source)[index];
                        }
                        else {
                                int counter = 0;
                                foreach (TSource element in source) {
                                        if (counter == index)
                                                return element;
                                        counter++;
                                }
                                return default (TSource);
                        }
                }
                
                #endregion
                
                #region DefaultIfEmpty
                
                public static IEnumerable<TSource> DefaultIfEmpty<TSource> (
                        this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool noYield = true;
                        foreach (TSource item in source)
                        {
                                noYield = false;
                                yield return item;
                        }
                        
                        if (noYield)
                                yield return default (TSource);
                }
                
                public static IEnumerable<TSource> DefaultIfEmpty<TSource> (
                        this IQueryable<TSource> source,
                        TSource defaultValue)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool noYield = true;
                        foreach (TSource item in source)
                        {
                                noYield = false;
                                yield return item;
                        }
                        
                        if (noYield)
                                yield return defaultValue;
                }
                
                #endregion

/*                #region EqualAll
                
                [System.Runtime.CompilerServices.Extension]
                public static bool EqualAll<TSource> (
                        IQueryable<TSource> first,
                        IQueryable<TSource> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();
                        
                        List<TSource> firstList = new List<TSource> (first);
                        List<TSource> secondList = new List<TSource> (second);
                        
                        if (firstList.Count != firstList.Count)
                                return false;
                        
                        for (int i = 0; i < firstList.Count; i++) {
                                if (!System.Object.Equals (firstList [i], secondList [i]))
                                        return false;
                        }
                        // If no pair of elements is different, then everything is equal
                        return true;
                }
                
                #endregion

                #region Range
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<int> Range (
                        int start, int count)
                {
                        if (count < 0 || (start + count - 1) > int.MaxValue)
                                throw new ArgumentOutOfRangeException ();
                        
                        for (int i = start; i < (start + count - 1); i++)
                                yield return i;
                }
                
                #endregion
*/                
                #region Repeat
                
                public static IEnumerable<TSource> Repeat<TSource> (this TSource element, int count)
                {
                        if (count < 0)
                                throw new ArgumentOutOfRangeException ();
                        
                        for (int i = 0; i < count; i++)
                                yield return element;
                }
                
                #endregion
                
                /** A NOTE ON IMPLEMENTATION REGARDING NULL KEYS
                 * 
                 *  GroupBy specification states that null-key values
                 *  are allowed. However, all implementations of 
                 *  Dictionary<K, TSource> ban null keys.
                 *  Because of this, a small trick has to be done:
                 *  a special List<TSource> variable is created in order to
                 *  be filled with this null-key values.
                 *  Also, groups must be yielded in the order their
                 *  keys were found for first time, so we need to keep
                 *  a record on when the null-key values appeared
                 *  (that is nullCounter variable).
                 *  Then, when results are iterated and yielded, we
                 *  mantain a counter and if null-key values were
                 *  found, they are yielded in the order needed.
                 *  Because K can be a valuetype, compilers expose a
                 *  restriction on null values, that's why default(TSource)
                 *  is used. However, default(TSource) is null for
                 *  reference types, and values with selectors that
                 *  return value types can't return null. **/
                
                #region GroupBy
                
                private static List<TSource> ContainsGroup<K, TSource>(
                        Dictionary<K, List<TSource>> items, K key, IEqualityComparer<K> comparer)
                {
                        IEqualityComparer<K> comparerInUse = (comparer ?? EqualityComparer<K>.Default);
                        foreach (KeyValuePair<K, List<TSource>> value in items) {
                                if (comparerInUse.Equals(value.Key, key))
                                    return value.Value;
                        }
                        return null;
                }
                
                public static IQueryable<IGrouping<K, TSource>> GroupBy<TSource, K> (
                        this IQueryable<TSource> source,
                        Func<TSource, K> keySelector)
                {
                        throw new NotImplementedException ();
                        //return GroupBy<TSource, K> (source, keySelector, null);
                }
/*                
                [System.Runtime.CompilerServices.Extension]
                public static IQueryable<IGrouping<K, TSource>> GroupBy<TSource, K> (
                        IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, List<TSource>> groups = new Dictionary<K, List<TSource>> ();
                        List<TSource> nullList = new List<TSource> ();
                        int counter = 0;
                        int nullCounter = -1;
                        
                        foreach (TSource element in source) {
                                K key = keySelector (element);
                                if (key == null) {
                                        nullList.Add (element);
                                        if (nullCounter == -1) {
                                                nullCounter = counter;
                                                counter++;
                                        }
                                }
                                else {
                                        List<TSource> group = ContainsGroup<K, TSource> (groups, key, comparer);
                                        if (group == null) {
                                                group = new List<TSource> ();
                                                groups.Add (key, group);
                                                counter++;
                                        }
                                        group.Add (element);
                                }
                        }
                        
                        counter = 0;
                        foreach (KeyValuePair<K, List<TSource>> group in groups) {
                                if (counter == nullCounter) {
                                        Grouping<K, TSource> nullGroup = new Grouping<K, TSource> (default (K), nullList);
                                        yield return nullGroup;
                                        counter++;
                                }
                                Grouping<K, TSource> grouping = new Grouping<K, TSource> (group.Key, group.Value);
                                yield return grouping;
                                counter++;
                        }
                }
*/                

                public static IQueryable<IGrouping<K, E>> GroupBy<TSource, K, E> (
                        this IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        Func<TSource, E> elementSelector)
                {
                        return GroupBy<TSource, K, E> (source, keySelector, elementSelector);
                }
                
                public static IEnumerable<IGrouping<K, E>> GroupBy<TSource, K, E> (
                        this IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        Func<TSource, E> elementSelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null || elementSelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, List<E>> groups = new Dictionary<K, List<E>> ();
                        List<E> nullList = new List<E> ();
                        int counter = 0;
                        int nullCounter = -1;

                        foreach (TSource item in source) {
                                K key = keySelector (item);
                                E element = elementSelector (item);
                                if (key == null) {
                                        nullList.Add(element);
                                        if (nullCounter == -1) {
                                                nullCounter = counter;
                                                counter++;
                                        }
                                }
                                else {
                                        List<E> group = ContainsGroup<K, E> (groups, key, comparer);
                                        if (group == null) {
                                                group = new List<E> ();
                                                groups.Add (key, group);
                                                counter++;
                                        }
                                        group.Add (element);
                                }
                        }
                        
                        counter = 0;
                        foreach (KeyValuePair<K, List<E>> group in groups) {
                                if (counter == nullCounter) {
                                        Grouping<K, E> nullGroup = new Grouping<K, E> (default (K), nullList);
                                        yield return nullGroup;
                                        counter++;
                                }
                                Grouping<K, E> grouping = new Grouping<K, E> (group.Key, group.Value);
                                yield return grouping;
                                counter++;
                        }
                }
                
                #endregion

                #region OrderBy
                
                public static OrderedSequence<TSource> OrderBy<TSource, K> (
                        this IQueryable<TSource> source,
                        Func<TSource, K> keySelector)
                {
                        return OrderBy<TSource, K> (source, keySelector, null);
                }
                
                public static OrderedSequence<TSource> OrderBy<TSource, K> (
                        this IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        IComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        return new InternalOrderedSequence<TSource, K> (
                                source, keySelector, (comparer ?? Comparer<K>.Default), false, null);
                }
                
                #endregion
                
                #region OrderByDescending
                
                public static OrderedSequence<TSource> OrderByDescending<TSource, K> (
                        this IQueryable<TSource> source,
                        Func<TSource, K> keySelector)
                {
                        return OrderByDescending<TSource, K> (source, keySelector, null);
                }
                
                public static OrderedSequence<TSource> OrderByDescending<TSource, K> (
                        this IQueryable<TSource> source,
                        Func<TSource, K> keySelector,
                        IComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        return new InternalOrderedSequence<TSource, K> (
                                source, keySelector, (comparer ?? Comparer<K>.Default), true, null);
                }
                
                #endregion
                
                #region ThenBy
                
                public static OrderedSequence<TSource> ThenBy<TSource, K> (
                        this OrderedSequence<TSource> source,
                        Func<TSource, K> keySelector)
                {
                        return ThenBy<TSource, K> (source, keySelector, null);
                }
                
                public static OrderedSequence<TSource> ThenBy<TSource, K> (
                        this OrderedSequence<TSource> source,
                        Func<TSource, K> keySelector,
                        IComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        return new InternalOrderedSequence<TSource, K> (
                                source, keySelector, (comparer ?? Comparer<K>.Default), false, source);
                }
                
                #endregion
                
                #region ThenByDescending
                
                public static OrderedSequence<TSource> ThenByDescending<TSource, K> (
                        this OrderedSequence<TSource> source,
                        Func<TSource, K> keySelector)
                {
                        return ThenByDescending<TSource, K> (source, keySelector, null);
                }
                
                public static OrderedSequence<TSource> ThenByDescending<TSource, K> (
                        this OrderedSequence<TSource> source,
                        Func<TSource, K> keySelector,
                        IComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        return new InternalOrderedSequence<TSource, K> (
                                source, keySelector, (comparer ?? Comparer<K>.Default), true, source);
                }
                
                #endregion
                
                #region Reverse

                public static IQueryable<TSource> Reverse<TSource> (
                        this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        //List<TSource> list = new List<TSource> (source);
                        //list.Reverse ();
                        //return list;
                        throw new NotImplementedException ();
                }
                
                #endregion

                #region Take
                
                public static IEnumerable<TSource> Take<TSource> (
                        this IQueryable<TSource> source,
                        int count)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        if (count <= 0)
                                yield break;
                        else {
                                int counter = 0;
                                foreach (TSource element in source) {
                                        yield return element;
                                        counter++;
                                        if (counter == count)
                                                yield break;
                                }
                        }
                }
                
                #endregion
                
                #region Skip
                
                public static IEnumerable<TSource> Skip<TSource> (
                        this IQueryable<TSource> source,
                        int count)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        bool yield = false;
                        if (count <= 0)
                                yield = true;

                        foreach (TSource element in source) {
                                if (yield)
                                        yield return element;
                                else {
                                        counter++;
                                        if (counter == count)
                                                yield = true;
                                }
                        }
                }
                
                #endregion
                
                #region TakeWhile
                
                public static IEnumerable<TSource> TakeWhile<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source) {
                                if (predicate (element))
                                        yield return element;
                                else
                                        yield break;
                        }
                }
                
                public static IEnumerable<TSource> TakeWhile<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, int, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        foreach (TSource element in source)
                        {
                                if (predicate (element, counter))
                                        yield return element;
                                else
                                        yield break;
                                counter++;
                        }
                }
                
                #endregion
                
                #region SkipWhile
                
                public static IEnumerable<TSource> SkipWhile<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        bool yield = false;
                        
                        foreach (TSource element in source) {
                                if (yield)
                                        yield return element;
                                else
                                        if (!predicate (element)) {
                                                yield return element;
                                                yield = true;
                                        }
                        }
                }
                
                public static IEnumerable<TSource> SkipWhile<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, int, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException();
                        
                        int counter = 0;
                        bool yield = false;
                        
                        foreach (TSource element in source) {
                                if (yield)
                                        yield return element;
                                else
                                        if (!predicate (element, counter)) {
                                                yield return element;
                                                yield = true;
                                        }
                                counter++;
                        }
                }
                
                #endregion

                #region Select
                
                public static IEnumerable<TResult> Select<TSource, TResult> (
                        this IQueryable<TSource> source,
                        Func<TSource, TResult> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source)
                                yield return selector (element);
                }
                
                public static IEnumerable<TResult> Select<TSource, TResult> (
                        this IQueryable<TSource> source,
                        Func<TSource, int, TResult> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        foreach (TSource element in source) {
                                yield return selector (element, counter);
                                counter++;
                        }
                }
                
                #endregion
                
                #region SelectMany
                
                public static IEnumerable<TResult> SelectMany<TSource, TResult> (
                        this IQueryable<TSource> source,
                        Func<TSource, IQueryable<TResult>> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source)
                                foreach (TResult item in selector (element))
                                        yield return item;
                }
                
                public static IEnumerable<TResult> SelectMany<TSource, TResult> (
                        this IQueryable<TSource> source,
                        Func<TSource, int, IQueryable<TResult>> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        foreach (TSource element in source) {
                                foreach (TResult item in selector (element, counter))
                                        yield return item;
                                counter++;
                        }
                }
                
                #endregion

                #region Any
                
                public static bool Any<TSource> (this IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source)
                                return true;
                        return false;
                }
                
                public static bool Any<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source)
                                if (predicate(element))
                                        return true;
                        return false;
                }
                
                #endregion
                
                #region All
                
                public static bool All<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source)
                                if (!predicate(element))
                                        return false;
                        return true;
                }
                
                #endregion
                
                #region Contains
                
                public static bool Contains<TSource> (this IQueryable<TSource> source, TSource value)
                {
                        if (source is ICollection<TSource>) {
                                ICollection<TSource> collection = (ICollection<TSource>)source;
                                return collection.Contains(value);
                        }
                        else {
                                foreach (TSource element in source)
                                        if (Equals(element, value))
                                                return true;
                                return false;
                        }
                }
                
                #endregion

                #region Where
                
                public static IEnumerable<TSource> Where<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (TSource element in source)
                                if (predicate (element))
                                        yield return element;
                }
                
                public static IEnumerable<TSource> Where<TSource> (
                        this IQueryable<TSource> source,
                        Func<TSource, int, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();

                        int counter = 0;
                        foreach (TSource element in source) {
                                if (predicate (element, counter))
                                        yield return element;
                                counter++;
                        }
                }
                
                #endregion

                #region Distinct
                
/*                [System.Runtime.CompilerServices.Extension]
                public static IQueryable<TSource> Distinct<TSource> (
                        IQueryable<TSource> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        List<TSource> items = new List<TSource> ();
                        foreach (TSource element in source) {
                                if (IndexOf (items, element) == -1) {
                                        items.Add (element);
                                        yield return element;
                                }
                        }
                }*/
                
                #endregion
                
                #region Union
                
/*                [System.Runtime.CompilerServices.Extension]
                public static IQueryable<TSource> Union<TSource> (
                        IQueryable<TSource> first,
                        IQueryable<TSource> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();
                        
                        List<TSource> items = new List<TSource> ();
                        foreach (TSource element in first)  {
                                if (IndexOf (items, element) == -1) {
                                        items.Add (element);
                                        yield return element;
                                }
                        }
                        foreach (TSource element in second)  {
                                if (IndexOf (items, element) == -1) {
                                        items.Add (element);
                                        yield return element;
                                }
                        }
                }*/
                
                #endregion
                
                #region Intersect
                
/*                [System.Runtime.CompilerServices.Extension]
                public static IQueryable<TSource> Intersect<TSource> (
                        IQueryable<TSource> first,
                        IQueryable<TSource> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();

                        List<TSource> items = new List<TSource> (Distinct (first));
                        bool[] marked = new bool [items.Count];
                        for (int i = 0; i < marked.Length; i++)
                                marked[i] = false;
                        
                        foreach (TSource element in second) {
                                int index = IndexOf (items, element);
                                if (index != -1)
                                        marked [index] = true;
                        }
                        for (int i = 0; i < marked.Length; i++) {
                                if (marked [i])
                                        yield return items [i];
                        }
                }*/
                
                #endregion
                
                #region Except
                
                /*[System.Runtime.CompilerServices.Extension]
                public static IQueryable<TSource> Except<TSource> (
                        IQueryable<TSource> first,
                        IQueryable<TSource> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();

                        List<TSource> items = new List<TSource> (Distinct (first));
                        foreach (TSource element in second) {
                                int index = IndexOf (items, element);
                                if (index == -1)
                                        items.Add (element);
                                else
                                        items.RemoveAt (index);
                        }
                        foreach (TSource item in items)
                                yield return item;
                }*/
                
                #endregion
                
                # region Join
                
                public static IEnumerable<V> Join<TSource, U, K, V> (
                        this IQueryable<TSource> outer,
                        IQueryable<U> inner,
                        Func<TSource, K> outerKeySelector,
                        Func<U, K> innerKeySelector,
                        Func<TSource, U, V> resultSelector)
                {
                        if (outer == null || inner == null || outerKeySelector == null || 
                                innerKeySelector == null || resultSelector == null)
                                throw new ArgumentNullException ();
                        
                        /*Lookup<K, U> innerKeys = ToLookup<U, K> (inner, innerKeySelector);                       
                        /*Dictionary<K, List<U>> innerKeys = new Dictionary<K, List<U>> ();
                        foreach (U element in inner)
                        {
                                K innerKey = innerKeySelector (element);
                                if (!innerKeys.ContainsKey (innerKey))
                                        innerKeys.Add (innerKey, new List<U> ());
                                innerKeys[innerKey].Add (element);
                        }*/
                        /*
                        foreach (TSource element in outer)
                        {
                                K outerKey = outerKeySelector (element);
                                if (innerKeys.Contains (outerKey))
                                {
                                        foreach (U innerElement in innerKeys [outerKey])
                                                yield return resultSelector (element, innerElement);
                                }
                        }*/
                        throw new NotImplementedException ();
                }
                
                # endregion
                
                # region GroupJoin
                
                /*[System.Runtime.CompilerServices.Extension]
                public static IQueryable<V> GroupJoin<TSource, U, K, V> (
                        IQueryable<TSource> outer,
                        IQueryable<U> inner,
                        Func<TSource, K> outerKeySelector,
                        Func<U, K> innerKeySelector,
                        Func<TSource, IQueryable<U>, V> resultSelector)
                {
                        if (outer == null || inner == null || outerKeySelector == null || 
                                innerKeySelector == null || resultSelector == null)
                                throw new ArgumentNullException ();
                        
                        Lookup<K, U> innerKeys = ToLookup<U, K> (inner, innerKeySelector);
                        /*Dictionary<K, List<U>> innerKeys = new Dictionary<K, List<U>> ();
                        foreach (U element in inner)
                        {
                                K innerKey = innerKeySelector (element);
                                if (!innerKeys.ContainsKey (innerKey))
                                        innerKeys.Add (innerKey, new List<U> ());
                                innerKeys[innerKey].Add (element);
                        }*/
                        
                        /*foreach (TSource element in outer)
                        {
                                K outerKey = outerKeySelector (element);
                                if (innerKeys.Contains (outerKey))
                                        yield return resultSelector (element, innerKeys [outerKey]);
                        }
                }*/
                
                # endregion

                // These methods are not included in the
                // .NET Standard Query Operators Specification,
                // but they provide additional useful commands
                
                #region Compare
                
                private static bool Equals<TSource> (
                        this TSource first, TSource second)
                {
                        // Mostly, values in Enumerable<TSource> 
                        // sequences need to be compared using
                        // Equals and GetHashCode
                        
                        if (first == null || second == null)
                                return (first == null && second == null);
                        else
                                return ((first.Equals (second) ||
                                         first.GetHashCode () == second.GetHashCode ()));
                }
                
                #endregion
/*
                #region IndexOf
                
                [System.Runtime.CompilerServices.Extension]
                public static int IndexOf<TSource>(
                        IQueryable<TSource> source,
                        TSource item)
                {
                        int counter = 0;
                        foreach (TSource element in source) {
                                if (Equals(element, item))
                                        return counter;
                                counter++;
                        }
                        // The item was not found
                        return -1;
                }
                
                #endregion
*/
        }
}
