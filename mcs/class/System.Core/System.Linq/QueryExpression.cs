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
//        Alejandro Serrano "Serras" (trupill@yahoo.es)
//

using System;
using System.Collections.Generic;

namespace System.Linq
{
        [System.Runtime.CompilerServices.Extension]
        public static class QueryExpression
        {
                #region Count
                
                [System.Runtime.CompilerServices.Extension]
                public static int Count<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        if (source is ICollection<T>)
                                return ((ICollection<T>)source).Count;
                        else {
                                int counter = 0;
                                foreach (T element in source)
                                        counter++;
                                return counter;
                        }
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static int Count<T> (
                        IEnumerable<T> source,
                        Func<T, bool> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        foreach (T element in source)
                                if (selector(element))
                                        counter++;
                        
                        return counter;
                }
                
                #endregion
                
                #region LongCount
                
                [System.Runtime.CompilerServices.Extension]
                public static long LongCount<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long counter = 0;
                        foreach (T element in source)
                                counter++;
                        return counter;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static long LongCount<T> (
                        IEnumerable<T> source,
                        Func<T, bool> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long counter = 0;
                        foreach (T element in source)
                                if (selector(element))
                                        counter++;
                        
                        return counter;
                }
                
                #endregion
                
                #region Sum
                
                [System.Runtime.CompilerServices.Extension]
                public static int Sum (
                        IEnumerable<int> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        int sum = 0;
                        foreach (int element in source)
                                sum += element;
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static int Sum<T> (
                        IEnumerable<T> source,
                        Func<T, int> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int sum = 0;
                        foreach (T element in source)
                                sum += selector (element);
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static int? Sum (
                        IEnumerable<int?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        int? sum = 0;
                        foreach (int? element in source)
                                if (element.HasValue)
                                        sum += element.Value;
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static int? Sum<T> (
                        IEnumerable<T> source,
                        Func<T, int?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int? sum = 0;
                        foreach (T element in source) {
                                int? item = selector (element);
                                if (item.HasValue)
                                        sum += item.Value;
                        }
                        
                        return sum;
                }

                [System.Runtime.CompilerServices.Extension]
                public static long Sum (
                        IEnumerable<long> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        foreach (long element in source)
                                sum += element;
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static long Sum<T> (
                        IEnumerable<T> source,
                        Func<T, long> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        foreach (T element in source)
                                sum += selector (element);
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static long? Sum (
                        IEnumerable<long?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        long? sum = 0;
                        foreach (long? element in source)
                                if (element.HasValue)
                                        sum += element.Value;
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static long? Sum<T> (
                        IEnumerable<T> source,
                        Func<T, long?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long? sum = 0;
                        foreach (T element in source) {
                                long? item = selector (element);
                                if (item.HasValue)
                                        sum += item.Value;
                        }
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double Sum (
                        IEnumerable<double> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        double sum = 0;
                        foreach (double element in source)
                                sum += element;
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double Sum<T> (
                        IEnumerable<T> source,
                        Func<T, double> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double sum = 0;
                        foreach (T element in source)
                                sum += selector (element);
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Sum (
                        IEnumerable<double?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        double? sum = 0;
                        foreach (double? element in source)
                                if (element.HasValue)
                                        sum += element.Value;
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Sum<T> (
                        IEnumerable<T> source,
                        Func<T, double?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double? sum = 0;
                        foreach (T element in source) {
                                double? item = selector (element);
                                if (item.HasValue)
                                        sum += item.Value;
                        }
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal Sum (
                        IEnumerable<decimal> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        decimal sum = 0;
                        foreach (decimal element in source)
                                sum += element;
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal Sum<T> (
                        IEnumerable<T> source,
                        Func<T, decimal> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal sum = 0;
                        foreach (T element in source)
                                sum += selector (element);
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal? Sum (
                        IEnumerable<decimal?> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        decimal? sum = 0;
                        foreach (decimal? element in source)
                                if (element.HasValue)
                                        sum += element.Value;
                        
                        return sum;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal? Sum<T> (
                        IEnumerable<T> source,
                        Func<T, decimal?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal? sum = 0;
                        foreach (T element in source) {
                                decimal? item = selector (element);
                                if (item.HasValue)
                                        sum += item.Value;
                        }
                        
                        return sum;
                }
                
                #endregion
                
                #region Min
                
                [System.Runtime.CompilerServices.Extension]
                public static int Min (
                        IEnumerable<int> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static int? Min (
                        IEnumerable<int?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static long Min (
                        IEnumerable<long> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static long? Min (
                        IEnumerable<long?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double Min (
                        IEnumerable<double> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Min (
                        IEnumerable<double?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal Min (
                        IEnumerable<decimal> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal? Min (
                        IEnumerable<decimal?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static T Min<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool notAssigned = true;
                        T minimum = default (T);
                        int counter = 0;
                        foreach (T element in source) {
                                if (notAssigned) {
                                        minimum = element;
                                        notAssigned = false;
                                }
                                else {
                                        int comparison;
                                        if (element is IComparable<T>)
                                                comparison = ((IComparable<T>)element).CompareTo (minimum);
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
                
                [System.Runtime.CompilerServices.Extension]
                public static int Min<T> (
                        IEnumerable<T> source,
                        Func<T, int> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int minimum = int.MaxValue;
                        int counter = 0;
                        foreach (T item in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static int? Min<T> (
                        IEnumerable<T> source,
                        Func<T, int?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        int? minimum = int.MaxValue;
                        foreach (T item in source) {
                                int? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static long Min<T> (
                        IEnumerable<T> source,
                        Func<T, long> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long minimum = long.MaxValue;
                        int counter = 0;
                        foreach (T item in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static long? Min<T> (
                        IEnumerable<T> source,
                        Func<T, long?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long? minimum = long.MaxValue;
                        foreach (T item in source) {
                                long? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double Min<T> (
                        IEnumerable<T> source,
                        Func<T, double> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double minimum = double.MaxValue;
                        int counter = 0;
                        foreach (T item in source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Min<T> (
                        IEnumerable<T> source,
                        Func<T, double?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        double? minimum = double.MaxValue;
                        foreach (T item in source) {
                                double? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal Min<T> (
                        IEnumerable<T> source,
                        Func<T, decimal> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal minimum = decimal.MaxValue;
                        int counter = 0;
                        foreach (T item in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal? Min<T> (
                        IEnumerable<T> source,
                        Func<T, decimal?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        decimal? minimum = decimal.MaxValue;
                        foreach (T item in source) {
                                decimal? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element < minimum)
                                                minimum = element;
                                }
                        }
                        return (onlyNull ? null : minimum);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static S Min<T, S> (
                        IEnumerable<T> source,
                        Func<T, S> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool notAssigned = true;
                        S minimum = default (S);
                        int counter = 0;
                        foreach (T item in source) {
                                S element = selector (item);
                                if (notAssigned) {
                                        minimum = element;
                                        notAssigned = false;
                                }
                                else {
                                        int comparison;
                                        if (element is IComparable<S>)
                                                comparison = ((IComparable<S>)element).CompareTo (minimum);
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
                
                [System.Runtime.CompilerServices.Extension]
                public static int Max (
                        IEnumerable<int> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static int? Max (
                        IEnumerable<int?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static long Max (
                        IEnumerable<long> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static long? Max (
                        IEnumerable<long?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double Max (
                        IEnumerable<double> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Max (
                        IEnumerable<double?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal Max (
                        IEnumerable<decimal> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal? Max (
                        IEnumerable<decimal?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static T Max<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool notAssigned = true;
                        T maximum = default (T);
                        int counter = 0;
                        foreach (T element in source) {
                                if (notAssigned) {
                                        maximum = element;
                                        notAssigned = false;
                                }
                                else {
                                        int comparison;
                                        if (element is IComparable<T>)
                                                comparison = ((IComparable<T>)element).CompareTo (maximum);
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
                
                [System.Runtime.CompilerServices.Extension]
                public static int Max<T> (
                        IEnumerable<T> source,
                        Func<T, int> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int maximum = int.MinValue;
                        int counter = 0;
                        foreach (T item in source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static int? Max<T> (
                        IEnumerable<T> source,
                        Func<T, int?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        int? maximum = int.MinValue;
                        foreach (T item in source) {
                                int? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static long Max<T> (
                        IEnumerable<T> source,
                        Func<T, long> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long maximum = long.MinValue;
                        int counter = 0;
                        foreach (T item in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static long? Max<T> (
                        IEnumerable<T> source,
                        Func<T, long?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long? maximum = long.MinValue;
                        foreach (T item in source) {
                                long? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double Max<T> (
                        IEnumerable<T> source,
                        Func<T, double> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double maximum = double.MinValue;
                        int counter = 0;
                        foreach (T item in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Max<T> (
                        IEnumerable<T> source,
                        Func<T, double?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        double? maximum = double.MinValue;
                        foreach (T item in source) {
                                double? element = selector(item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal Max<T> (
                        IEnumerable<T> source,
                        Func<T, decimal> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal maximum = decimal.MinValue;
                        int counter = 0;
                        foreach (T item in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal? Max<T> (
                        IEnumerable<T> source,
                        Func<T, decimal?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        decimal? maximum = decimal.MinValue;
                        foreach (T item in source) {
                                decimal? element = selector(item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        if (element > maximum)
                                                maximum = element;
                                }
                        }
                        return (onlyNull ? null : maximum);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static S Max<T, S> (
                        IEnumerable<T> source,
                        Func<T, S> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool notAssigned = true;
                        S maximum = default (S);
                        int counter = 0;
                        foreach (T item in source)
                        {
                                S element = selector (item);
                                if (notAssigned)  {
                                        maximum = element;
                                        notAssigned = false;
                                }
                                else  {
                                        int comparison;
                                        if (element is IComparable<S>)
                                                comparison = ((IComparable<S>)element).CompareTo (maximum);
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double Average (
                        IEnumerable<int> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Average (
                        IEnumerable<int?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double Average (
                        IEnumerable<long> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Average (
                        IEnumerable<long?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double Average (
                        IEnumerable<double> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Average (
                        IEnumerable<double?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal Average (
                        IEnumerable<decimal> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal? Average (
                        IEnumerable<decimal?> source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static double Average<T> (
                        IEnumerable<T> source,
                        Func<T, int> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        long counter = 0;
                        foreach (T item in source) {
                                sum += selector (item);
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return (double)sum / (double)counter;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Average<T> (
                        IEnumerable<T> source,
                        Func<T, int?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long sum = 0;
                        long counter = 0;
                        foreach (T item in source) {
                                int? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (double?)sum / (double?)counter);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double Average<T> (
                        IEnumerable<T> source,
                        Func<T, long> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        long sum = 0;
                        long counter = 0;
                        foreach (T item in source) {
                                sum += selector (item);
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException();
                        else
                                return (double)sum / (double)counter;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Average<T> (
                        IEnumerable<T> source,
                        Func<T, long?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        long sum = 0;
                        long counter = 0;
                        foreach (T item in source) {
                                long? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (double?)sum/(double?)counter);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double Average<T> (
                        IEnumerable<T> source,
                        Func<T, double> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        double sum = 0;
                        double counter = 0;
                        foreach (T item in source) {
                                sum += selector (item);
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return sum / counter;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static double? Average<T> (
                        IEnumerable<T> source,
                        Func<T, double?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        double sum = 0;
                        double counter = 0;
                        foreach (T item in source) {
                                double? element = selector (item);
                                if (element.HasValue) {
                                        onlyNull = false;
                                        sum += element.Value;
                                        counter++;
                                }
                        }
                        return (onlyNull ? null : (double?)(sum/counter));
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal Average<T> (
                        IEnumerable<T> source,
                        Func<T, decimal> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        decimal sum = 0;
                        decimal counter = 0;
                        foreach (T item in source) {
                                sum += selector(item);
                                counter++;
                        }
                        
                        if (counter == 0)
                                throw new InvalidOperationException ();
                        else
                                return sum / counter;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static decimal? Average<T> (
                        IEnumerable<T> source,
                        Func<T, decimal?> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        bool onlyNull = true;
                        decimal sum = 0;
                        decimal counter = 0;
                        foreach (T item in source) {
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
                
                [Obsolete ("Use Aggregate instead")]
                [System.Runtime.CompilerServices.Extension]
                public static T Fold<T> (
                        IEnumerable<T> source,
                        Func<T, T, T> func)
                {
                        return Fold<T> (source, func);
                }
                
                [Obsolete ("Use Aggregate instead")]
                [System.Runtime.CompilerServices.Extension]
                public static U Fold<T, U> (
                        IEnumerable<T> source,
                        U seed,
                        Func<U, T, U> func)
                {
                        return Fold<T, U> (source, seed, func);
                }
                
                #endregion
                
                #region Aggregate
                
                [System.Runtime.CompilerServices.Extension]
                public static T Aggregate<T> (
                        IEnumerable<T> source,
                        Func<T, T, T> func)
                {
                        if (source == null || func == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        T folded = default (T);
                        
                        foreach (T element in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static U Aggregate<T, U> (
                        IEnumerable<T> source,
                        U seed,
                        Func<U, T, U> func)
                {
                        if (source == null || func == null)
                                throw new ArgumentNullException ();
                        
                        U folded = seed;
                        foreach (T element in source)
                                folded = func (folded, element);
                        return folded;
                }
                
                #endregion

                #region Concat
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Concat<T> (
                        IEnumerable<T> first,
                        IEnumerable<T> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in first)
                                yield return element;
                        foreach (T element in second)
                                yield return element;
                }
                
                #endregion

                #region ToSequence
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> ToSequence<T> (
                        IEnumerable<T> source)
                {
                        return (IEnumerable<T>)source;
                }
                
                #endregion
                
                #region ToArray
                
                [System.Runtime.CompilerServices.Extension]
                public static T[] ToArray<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        List<T> list = new List<T> (source);
                        return list.ToArray ();
                }
                
                #endregion
                
                #region ToList
                
                [System.Runtime.CompilerServices.Extension]
                public static List<T> ToList<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        return new List<T> (source);
                }
                
                #endregion
                
                #region ToDictionary
                
                [System.Runtime.CompilerServices.Extension]
                public static Dictionary<K, T> ToDictionary<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector)
                {
                        return ToDictionary<T, K> (source, keySelector, null);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static Dictionary<K, T> ToDictionary<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, T> dictionary = new Dictionary<K, T> (comparer ?? EqualityComparer<K>.Default);
                        foreach (T element in source) {
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
                public static Dictionary<K, E> ToDictionary<T, K, E> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        Func<T, E> elementSelector)
                {
                        return ToDictionary<T, K, E> (source, keySelector, elementSelector, null);
                }
                                
                [System.Runtime.CompilerServices.Extension]
                public static Dictionary<K, E> ToDictionary<T, K, E> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        Func<T, E> elementSelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null || elementSelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, E> dictionary = new Dictionary<K, E>(comparer ?? EqualityComparer<K>.Default);
                        foreach (T element in source)
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
                public static Lookup<K, T> ToLookup<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector)
                {
                        return ToLookup<T, K> (source, keySelector, null);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static Lookup<K, T> ToLookup<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, List<T>> dictionary = new Dictionary<K, List<T>> (comparer ?? EqualityComparer<K>.Default);
                        foreach (T element in source) {
                                K key = keySelector (element);
                                if (key == null)
                                        throw new ArgumentNullException ();
                                if (!dictionary.ContainsKey (key))
                                        dictionary.Add (key, new List<T> ());
                                dictionary[key].Add (element);
                        }
                        return new Lookup<K, T> (dictionary);
                }

                [System.Runtime.CompilerServices.Extension]
                public static Lookup<K, E> ToLookup<T, K, E> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        Func<T, E> elementSelector)
                {
                        return ToLookup<T, K, E> (source, keySelector, elementSelector, null);
                }
                                
                [System.Runtime.CompilerServices.Extension]
                public static Lookup<K, E> ToLookup<T, K, E> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        Func<T, E> elementSelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null || elementSelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, List<E>> dictionary = new Dictionary<K, List<E>>(comparer ?? EqualityComparer<K>.Default);
                        foreach (T element in source)
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
                
                #region OfType
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> OfType<T> (
                        System.Collections.IEnumerable source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (object element in source)
                                if (element is T)
                                        yield return (T)element;
                }
                
                #endregion
                
                #region Cast
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Cast<T> (
                        System.Collections.IEnumerable source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (object element in source)
                                yield return (T)element;
                }
                
                #endregion

                #region First
                
                [System.Runtime.CompilerServices.Extension]
                public static T First<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source)
                                return element;
                        
                        throw new InvalidOperationException ();
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static T First<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source) {
                                if (predicate (element))
                                        return element;
                        }
                        
                        throw new InvalidOperationException ();
                }
                
                #endregion
                
                #region FirstOrDefault
                
                [System.Runtime.CompilerServices.Extension]
                public static T FirstOrDefault<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source)
                                return element;
                        
                        return default (T);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static T FirstOrDefault<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source) {
                                if (predicate (element))
                                        return element;
                        }
                        
                        return default (T);
                }
                
                #endregion
                
                #region Last
                
                [System.Runtime.CompilerServices.Extension]
                public static T Last<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool noElements = true;
                        T lastElement = default (T);
                        foreach (T element in source)
                        {
                                if (noElements) noElements = false;
                                lastElement = element;
                        }
                        
                        if (!noElements)
                                return lastElement;
                        else
                                throw new InvalidOperationException();
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static T Last<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        bool noElements = true;
                        T lastElement = default (T);
                        foreach (T element in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static T LastOrDefault<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        T lastElement = default (T);
                        foreach (T element in source)
                                lastElement = element;
                        
                        return lastElement;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static T LastOrDefault<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        T lastElement = default (T);
                        foreach (T element in source) {
                                if (predicate (element))
                                        lastElement = element;
                        }
                        
                        return lastElement;
                }
                
                #endregion
                
                #region Single
                
                [System.Runtime.CompilerServices.Extension]
                public static T Single<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool otherElement = false;
                        T singleElement = default (T);
                        foreach (T element in source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static T Single<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        bool otherElement = false;
                        T singleElement = default (T);
                        foreach (T element in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static T SingleOrDefault<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool otherElement = false;
                        T singleElement = default (T);
                        foreach (T element in source)
                        {
                                if (otherElement) throw new InvalidOperationException ();
                                if (!otherElement) otherElement = true;
                                singleElement = element;
                        }
                        
                        return singleElement;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static T SingleOrDefault<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        bool otherElement = false;
                        T singleElement = default (T);
                        foreach (T element in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static T ElementAt<T> (
                        IEnumerable<T> source,
                        int index)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        if (index < 0)
                                throw new ArgumentOutOfRangeException ();
                        
                        if (source is IList<T>)
                                return ((IList<T>)source)[index];
                        else {
                                int counter = 0;
                                foreach (T element in source) {
                                        if (counter == index)
                                                return element;
                                        counter++;
                                }
                                throw new ArgumentOutOfRangeException();
                        }
                }
                
                #endregion
                
                #region ElementAtOrDefault
                
                [System.Runtime.CompilerServices.Extension]
                public static T ElementAtOrDefault<T> (
                        IEnumerable<T> source,
                        int index)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        if (index < 0)
                                return default(T);
                        
                        if (source is IList<T>)
                        {
                                if (((IList<T>)source).Count >= index)
                                        return default(T);
                                else
                                        return ((IList<T>)source)[index];
                        }
                        else {
                                int counter = 0;
                                foreach (T element in source) {
                                        if (counter == index)
                                                return element;
                                        counter++;
                                }
                                return default (T);
                        }
                }
                
                #endregion
                
                #region DefaultIfEmpty
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> DefaultIfEmpty<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool noYield = true;
                        foreach (T item in source)
                        {
                                noYield = false;
                                yield return item;
                        }
                        
                        if (noYield)
                                yield return default (T);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> DefaultIfEmpty<T> (
                        IEnumerable<T> source,
                        T defaultValue)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        bool noYield = true;
                        foreach (T item in source)
                        {
                                noYield = false;
                                yield return item;
                        }
                        
                        if (noYield)
                                yield return defaultValue;
                }
                
                #endregion

                #region EqualAll
                
                [System.Runtime.CompilerServices.Extension]
                public static bool EqualAll<T> (
                        IEnumerable<T> first,
                        IEnumerable<T> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();
                        
                        List<T> firstList = new List<T> (first);
                        List<T> secondList = new List<T> (second);
                        
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
                
                #region Repeat
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Repeat<T> (
                        T element, int count)
                {
                        if (count < 0)
                                throw new ArgumentOutOfRangeException ();
                        
                        for (int i = 0; i < count; i++)
                                yield return element;
                }
                
                #endregion

                #region Empty
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Empty<T> ()
                {
                        return new List<T> ();
                }
                
                #endregion
                
                /** A NOTE ON IMPLEMENTATION REGARDING NULL KEYS
                 * 
                 *  GroupBy specification states that null-key values
                 *  are allowed. However, all implementations of 
                 *  Dictionary<K, T> ban null keys.
                 *  Because of this, a small trick has to be done:
                 *  a special List<T> variable is created in order to
                 *  be filled with this null-key values.
                 *  Also, groups must be yielded in the order their
                 *  keys were found for first time, so we need to keep
                 *  a record on when the null-key values appeared
                 *  (that is nullCounter variable).
                 *  Then, when results are iterated and yielded, we
                 *  mantain a counter and if null-key values were
                 *  found, they are yielded in the order needed.
                 *  Because K can be a valuetype, compilers expose a
                 *  restriction on null values, that's why default(T)
                 *  is used. However, default(T) is null for
                 *  reference types, and values with selectors that
                 *  return value types can't return null. **/
                
                #region GroupBy
                
                private static List<T> ContainsGroup<K, T>(
                        Dictionary<K, List<T>> items, K key, IEqualityComparer<K> comparer)
                {
                        IEqualityComparer<K> comparerInUse = (comparer ?? EqualityComparer<K>.Default);
                        foreach (KeyValuePair<K, List<T>> value in items) {
                                if (comparerInUse.Equals(value.Key, key))
                                    return value.Value;
                        }
                        return null;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<IGrouping<K, T>> GroupBy<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector)
                {
                        return GroupBy<T, K> (source, keySelector, null);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<IGrouping<K, T>> GroupBy<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, List<T>> groups = new Dictionary<K, List<T>> ();
                        List<T> nullList = new List<T> ();
                        int counter = 0;
                        int nullCounter = -1;
                        
                        foreach (T element in source) {
                                K key = keySelector (element);
                                if (key == null) {
                                        nullList.Add (element);
                                        if (nullCounter == -1) {
                                                nullCounter = counter;
                                                counter++;
                                        }
                                }
                                else {
                                        List<T> group = ContainsGroup<K, T> (groups, key, comparer);
                                        if (group == null) {
                                                group = new List<T> ();
                                                groups.Add (key, group);
                                                counter++;
                                        }
                                        group.Add (element);
                                }
                        }
                        
                        counter = 0;
                        foreach (KeyValuePair<K, List<T>> group in groups) {
                                if (counter == nullCounter) {
                                        Grouping<K, T> nullGroup = new Grouping<K, T> (default (K), nullList);
                                        yield return nullGroup;
                                        counter++;
                                }
                                Grouping<K, T> grouping = new Grouping<K, T> (group.Key, group.Value);
                                yield return grouping;
                                counter++;
                        }
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<IGrouping<K, E>> GroupBy<T, K, E> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        Func<T, E> elementSelector)
                {
                        return GroupBy<T, K, E> (source, keySelector, elementSelector);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<IGrouping<K, E>> GroupBy<T, K, E> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        Func<T, E> elementSelector,
                        IEqualityComparer<K> comparer)
                {
                        if (source == null || keySelector == null || elementSelector == null)
                                throw new ArgumentNullException ();
                        
                        Dictionary<K, List<E>> groups = new Dictionary<K, List<E>> ();
                        List<E> nullList = new List<E> ();
                        int counter = 0;
                        int nullCounter = -1;

                        foreach (T item in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static OrderedSequence<T> OrderBy<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector)
                {
                        return OrderBy<T, K> (source, keySelector, null);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static OrderedSequence<T> OrderBy<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        IComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        return new InternalOrderedSequence<T, K> (
                                source, keySelector, (comparer ?? Comparer<K>.Default), false, null);
                }
                
                #endregion
                
                #region OrderByDescending
                
                [System.Runtime.CompilerServices.Extension]
                public static OrderedSequence<T> OrderByDescending<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector)
                {
                        return OrderByDescending<T, K> (source, keySelector, null);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static OrderedSequence<T> OrderByDescending<T, K> (
                        IEnumerable<T> source,
                        Func<T, K> keySelector,
                        IComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        return new InternalOrderedSequence<T, K> (
                                source, keySelector, (comparer ?? Comparer<K>.Default), true, null);
                }
                
                #endregion
                
                #region ThenBy
                
                [System.Runtime.CompilerServices.Extension]
                public static OrderedSequence<T> ThenBy<T, K> (
                        OrderedSequence<T> source,
                        Func<T, K> keySelector)
                {
                        return ThenBy<T, K> (source, keySelector, null);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static OrderedSequence<T> ThenBy<T, K> (
                        OrderedSequence<T> source,
                        Func<T, K> keySelector,
                        IComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        return new InternalOrderedSequence<T, K> (
                                source, keySelector, (comparer ?? Comparer<K>.Default), false, source);
                }
                
                #endregion
                
                #region ThenByDescending
                
                [System.Runtime.CompilerServices.Extension]
                public static OrderedSequence<T> ThenByDescending<T, K> (
                        OrderedSequence<T> source,
                        Func<T, K> keySelector)
                {
                        return ThenByDescending<T, K> (source, keySelector, null);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static OrderedSequence<T> ThenByDescending<T, K> (
                        OrderedSequence<T> source,
                        Func<T, K> keySelector,
                        IComparer<K> comparer)
                {
                        if (source == null || keySelector == null)
                                throw new ArgumentNullException ();
                        
                        return new InternalOrderedSequence<T, K> (
                                source, keySelector, (comparer ?? Comparer<K>.Default), true, source);
                }
                
                #endregion
                
                #region Reverse

                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Reverse<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        List<T> list = new List<T> (source);
                        list.Reverse ();
                        return list;
                }
                
                #endregion

                #region Take
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Take<T> (
                        IEnumerable<T> source,
                        int count)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        if (count <= 0)
                                yield break;
                        else {
                                int counter = 0;
                                foreach (T element in source) {
                                        yield return element;
                                        counter++;
                                        if (counter == count)
                                                yield break;
                                }
                        }
                }
                
                #endregion
                
                #region Skip
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Skip<T> (
                        IEnumerable<T> source,
                        int count)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        bool yield = false;
                        if (count <= 0)
                                yield = true;

                        foreach (T element in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> TakeWhile<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source) {
                                if (predicate (element))
                                        yield return element;
                                else
                                        yield break;
                        }
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> TakeWhile<T> (
                        IEnumerable<T> source,
                        Func<T, int, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        foreach (T element in source)
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
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> SkipWhile<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        bool yield = false;
                        
                        foreach (T element in source) {
                                if (yield)
                                        yield return element;
                                else
                                        if (!predicate (element)) {
                                                yield return element;
                                                yield = true;
                                        }
                        }
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> SkipWhile<T> (
                        IEnumerable<T> source,
                        Func<T, int, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException();
                        
                        int counter = 0;
                        bool yield = false;
                        
                        foreach (T element in source) {
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
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<S> Select<T, S> (
                        IEnumerable<T> source,
                        Func<T, S> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source)
                                yield return selector (element);
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<S> Select<T, S> (
                        IEnumerable<T> source,
                        Func<T, int, S> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        foreach (T element in source) {
                                yield return selector (element, counter);
                                counter++;
                        }
                }
                
                #endregion
                
                #region SelectMany
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<S> SelectMany<T, S> (
                        IEnumerable<T> source,
                        Func<T, IEnumerable<S>> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source)
                                foreach (S item in selector (element))
                                        yield return item;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<S> SelectMany<T, S> (
                        IEnumerable<T> source,
                        Func<T, int, IEnumerable<S>> selector)
                {
                        if (source == null || selector == null)
                                throw new ArgumentNullException ();
                        
                        int counter = 0;
                        foreach (T element in source) {
                                foreach (S item in selector (element, counter))
                                        yield return item;
                                counter++;
                        }
                }
                
                #endregion

                #region Any
                
                [System.Runtime.CompilerServices.Extension]
                public static bool Any<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source)
                                return true;
                        return false;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static bool Any<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source)
                                if (predicate(element))
                                        return true;
                        return false;
                }
                
                #endregion
                
                #region All
                
                [System.Runtime.CompilerServices.Extension]
                public static bool All<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source)
                                if (!predicate(element))
                                        return false;
                        return true;
                }
                
                #endregion
                
                #region Contains
                
                [System.Runtime.CompilerServices.Extension]
                public static bool Contains<T> (
                        IEnumerable<T> source,
                        T value)
                {
                        if (source is ICollection<T>) {
                                ICollection<T> collection = (ICollection<T>)source;
                                return collection.Contains(value);
                        }
                        else {
                                foreach (T element in source)
                                        if (Equals(element, value))
                                                return true;
                                return false;
                        }
                }
                
                #endregion

                #region Where
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Where<T> (
                        IEnumerable<T> source,
                        Func<T, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();
                        
                        foreach (T element in source)
                                if (predicate (element))
                                        yield return element;
                }
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Where<T> (
                        IEnumerable<T> source,
                        Func<T, int, bool> predicate)
                {
                        if (source == null || predicate == null)
                                throw new ArgumentNullException ();

                        int counter = 0;
                        foreach (T element in source) {
                                if (predicate (element, counter))
                                        yield return element;
                                counter++;
                        }
                }
                
                #endregion

                #region Distinct
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Distinct<T> (
                        IEnumerable<T> source)
                {
                        if (source == null)
                                throw new ArgumentNullException ();
                        
                        List<T> items = new List<T> ();
                        foreach (T element in source) {
                                if (IndexOf (items, element) == -1) {
                                        items.Add (element);
                                        yield return element;
                                }
                        }
                }
                
                #endregion
                
                #region Union
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Union<T> (
                        IEnumerable<T> first,
                        IEnumerable<T> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();
                        
                        List<T> items = new List<T> ();
                        foreach (T element in first)  {
                                if (IndexOf (items, element) == -1) {
                                        items.Add (element);
                                        yield return element;
                                }
                        }
                        foreach (T element in second)  {
                                if (IndexOf (items, element) == -1) {
                                        items.Add (element);
                                        yield return element;
                                }
                        }
                }
                
                #endregion
                
                #region Intersect
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Intersect<T> (
                        IEnumerable<T> first,
                        IEnumerable<T> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();

                        List<T> items = new List<T> (Distinct (first));
                        bool[] marked = new bool [items.Count];
                        for (int i = 0; i < marked.Length; i++)
                                marked[i] = false;
                        
                        foreach (T element in second) {
                                int index = IndexOf (items, element);
                                if (index != -1)
                                        marked [index] = true;
                        }
                        for (int i = 0; i < marked.Length; i++) {
                                if (marked [i])
                                        yield return items [i];
                        }
                }
                
                #endregion
                
                #region Except
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<T> Except<T> (
                        IEnumerable<T> first,
                        IEnumerable<T> second)
                {
                        if (first == null || second == null)
                                throw new ArgumentNullException ();

                        List<T> items = new List<T> (Distinct (first));
                        foreach (T element in second) {
                                int index = IndexOf (items, element);
                                if (index == -1)
                                        items.Add (element);
                                else
                                        items.RemoveAt (index);
                        }
                        foreach (T item in items)
                                yield return item;
                }
                
                #endregion
                
                # region Join
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<V> Join<T, U, K, V> (
                        IEnumerable<T> outer,
                        IEnumerable<U> inner,
                        Func<T, K> outerKeySelector,
                        Func<U, K> innerKeySelector,
                        Func<T, U, V> resultSelector)
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
                        
                        foreach (T element in outer)
                        {
                                K outerKey = outerKeySelector (element);
                                if (innerKeys.Contains (outerKey))
                                {
                                        foreach (U innerElement in innerKeys [outerKey])
                                                yield return resultSelector (element, innerElement);
                                }
                        }
                }
                
                # endregion
                
                # region GroupJoin
                
                [System.Runtime.CompilerServices.Extension]
                public static IEnumerable<V> GroupJoin<T, U, K, V> (
                        IEnumerable<T> outer,
                        IEnumerable<U> inner,
                        Func<T, K> outerKeySelector,
                        Func<U, K> innerKeySelector,
                        Func<T, IEnumerable<U>, V> resultSelector)
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
                        
                        foreach (T element in outer)
                        {
                                K outerKey = outerKeySelector (element);
                                if (innerKeys.Contains (outerKey))
                                        yield return resultSelector (element, innerKeys [outerKey]);
                        }
                }
                
                # endregion

                // These methods are not included in the
                // .NET Standard Query Operators Specification,
                // but they provide additional useful commands
                
                #region Compare
                
                [System.Runtime.CompilerServices.Extension]
                private static bool Equals<T> (
                        T first, T second)
                {
                        // Mostly, values in Enumerable<T> 
                        // sequences need to be compared using
                        // Equals and GetHashCode
                        
                        if (first == null || second == null)
                                return (first == null && second == null);
                        else
                                return ((first.Equals (second) ||
                                         first.GetHashCode () == second.GetHashCode ()));
                }
                
                #endregion

                #region IndexOf
                
                [System.Runtime.CompilerServices.Extension]
                public static int IndexOf<T>(
                        IEnumerable<T> source,
                        T item)
                {
                        int counter = 0;
                        foreach (T element in source) {
                                if (Equals(element, item))
                                        return counter;
                                counter++;
                        }
                        // The item was not found
                        return -1;
                }
                
                #endregion
        }
}
