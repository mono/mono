// ***********************************************************************
// Copyright (c) 2009 Charlie Poole
//
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
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.IO;
using System.Collections;
using System.Reflection;
#if CLR_2_0 || CLR_4_0
using System.Collections.Generic;
#endif

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// NUnitEqualityComparer encapsulates NUnit's handling of
    /// equality tests between objects.
    /// </summary>
    public class NUnitEqualityComparer
    {
        #region Static and Instance Fields

        /// <summary>
        /// If true, all string comparisons will ignore case
        /// </summary>
        private bool caseInsensitive;

        /// <summary>
        /// If true, arrays will be treated as collections, allowing
        /// those of different dimensions to be compared
        /// </summary>
        private bool compareAsCollection;

        /// <summary>
        /// Comparison objects used in comparisons for some constraints.
        /// </summary>
        private EqualityAdapterList externalComparers = new EqualityAdapterList();
            
        /// <summary>
        /// List of points at which a failure occured.
        /// </summary>
        private FailurePointList failurePoints;

        /// <summary>
        /// RecursionDetector used to check for recursion when
        /// evaluating self-referencing enumerables.
        /// </summary>
        private RecursionDetector recursionDetector;

        private static readonly int BUFFER_SIZE = 4096;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the default NUnitEqualityComparer
        /// </summary>
        public static NUnitEqualityComparer Default
        {
            get { return new NUnitEqualityComparer(); }
        }
        /// <summary>
        /// Gets and sets a flag indicating whether case should
        /// be ignored in determining equality.
        /// </summary>
        public bool IgnoreCase
        {
            get { return caseInsensitive; }
            set { caseInsensitive = value; }
        }

        /// <summary>
        /// Gets and sets a flag indicating that arrays should be
        /// compared as collections, without regard to their shape.
        /// </summary>
        public bool CompareAsCollection
        {
            get { return compareAsCollection; }
            set { compareAsCollection = value; }
        }

        /// <summary>
        /// Gets the list of external comparers to be used to
        /// test for equality. They are applied to members of
        /// collections, in place of NUnit's own logic.
        /// </summary>
#if CLR_2_0 || CLR_4_0
        public IList<EqualityAdapter> ExternalComparers
#else
        public IList ExternalComparers
#endif
        {
            get { return externalComparers; }
        }

        /// <summary>
        /// Gets the list of failure points for the last Match performed.
        /// The list consists of objects to be interpreted by the caller.
        /// This generally means that the caller may only make use of
        /// objects it has placed on the list at a particular depthy.
        /// </summary>
#if CLR_2_0 || CLR_4_0
        public IList<FailurePoint> FailurePoints
#else
        public IList FailurePoints
#endif
        {
            get { return failurePoints; }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Compares two objects for equality within a tolerance, setting
        /// the tolerance to the actual tolerance used if an empty
        /// tolerance is supplied.
        /// </summary>
        public bool AreEqual(object expected, object actual, ref Tolerance tolerance)
        {
            this.failurePoints = new FailurePointList();
            this.recursionDetector = new RecursionDetector();

            return ObjectsEqual(expected, actual, ref tolerance);
        }

        #endregion

        #region Helper Methods

        private bool ObjectsEqual(object expected, object actual, ref Tolerance tolerance)
        {
            if (expected == null && actual == null)
                return true;

            if (expected == null || actual == null)
                return false;

            if (object.ReferenceEquals(expected, actual))
                return true;

            Type xType = expected.GetType();
            Type yType = actual.GetType();

            EqualityAdapter externalComparer = GetExternalComparer(expected, actual);
            if (externalComparer != null)
                return externalComparer.AreEqual(expected, actual);

            if (xType.IsArray && yType.IsArray && !compareAsCollection)
                return ArraysEqual((Array)expected, (Array)actual, ref tolerance);

            if (expected is IDictionary && actual is IDictionary)
                return DictionariesEqual((IDictionary)expected, (IDictionary)actual, ref tolerance);

            if (expected is IEnumerable && actual is IEnumerable && !(expected is string && actual is string))
                return EnumerablesEqual((IEnumerable)expected, (IEnumerable)actual, ref tolerance);

            if (expected is string && actual is string)
                return StringsEqual((string)expected, (string)actual);

            if (expected is Stream && actual is Stream)
                return StreamsEqual((Stream)expected, (Stream)actual);

            if (expected is DirectoryInfo && actual is DirectoryInfo)
                return DirectoriesEqual((DirectoryInfo)expected, (DirectoryInfo)actual);

            if (Numerics.IsNumericType(expected) && Numerics.IsNumericType(actual))
                return Numerics.AreEqual(expected, actual, ref tolerance);

            if (tolerance != null && tolerance.Value is TimeSpan)
            {
                TimeSpan amount = (TimeSpan)tolerance.Value;

                if (expected is DateTime && actual is DateTime)
                    return ((DateTime)expected - (DateTime)actual).Duration() <= amount;

                if (expected is TimeSpan && actual is TimeSpan)
                    return ((TimeSpan)expected - (TimeSpan)actual).Duration() <= amount;
            }

#if (CLR_2_0 || CLR_4_0) && !NETCF
            if (FirstImplementsIEquatableOfSecond(xType, yType))
                return InvokeFirstIEquatableEqualsSecond(expected, actual);
            else if (FirstImplementsIEquatableOfSecond(yType, xType))
                return InvokeFirstIEquatableEqualsSecond(actual, expected);
#endif
            
            return expected.Equals(actual);
        }

#if (CLR_2_0 || CLR_4_0) && !NETCF
        private static bool FirstImplementsIEquatableOfSecond(Type first, Type second)
        {
            Type[] equatableArguments = GetEquatableGenericArguments(first);

            foreach (var xEquatableArgument in equatableArguments)
                if (xEquatableArgument.Equals(second))
                    return true;

            return false;
        }

        private static Type[] GetEquatableGenericArguments(Type type)
        {
            foreach (Type @interface in type.GetInterfaces())
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition().Equals(typeof(IEquatable<>)))
                    return @interface.GetGenericArguments();

            return new Type[0];
        }

        private static bool InvokeFirstIEquatableEqualsSecond(object first, object second)
        {
            MethodInfo equals = typeof(IEquatable<>).MakeGenericType(second.GetType()).GetMethod("Equals");

            return (bool)equals.Invoke(first, new object[] { second });
        }
#endif
        
        private EqualityAdapter GetExternalComparer(object x, object y)
        {
            foreach (EqualityAdapter adapter in externalComparers)
                if (adapter.CanCompare(x, y))
                    return adapter;

            return null;
        }

        /// <summary>
        /// Helper method to compare two arrays
        /// </summary>
        private bool ArraysEqual(Array expected, Array actual, ref Tolerance tolerance)
        {
            int rank = expected.Rank;

            if (rank != actual.Rank)
                return false;

            for (int r = 1; r < rank; r++)
                if (expected.GetLength(r) != actual.GetLength(r))
                    return false;

            return EnumerablesEqual((IEnumerable)expected, (IEnumerable)actual, ref tolerance);
        }

        private bool DictionariesEqual(IDictionary expected, IDictionary actual, ref Tolerance tolerance)
        {
            if (expected.Count != actual.Count)
                return false;
 
            CollectionTally tally = new CollectionTally(this, expected.Keys);
            if (!tally.TryRemove(actual.Keys) || tally.Count > 0)
                return false;

            foreach (object key in expected.Keys)
                if (!ObjectsEqual(expected[key], actual[key], ref tolerance))
                    return false;
 
            return true;
        }

        private bool StringsEqual(string expected, string actual)
        {
            string s1 = caseInsensitive ? expected.ToLower() : expected;
            string s2 = caseInsensitive ? actual.ToLower() : actual;

            return s1.Equals(s2);
        }

        private bool EnumerablesEqual(IEnumerable expected, IEnumerable actual, ref Tolerance tolerance)
        {
            if (recursionDetector.CheckRecursion(expected, actual))
                return false;

            IEnumerator expectedEnum = expected.GetEnumerator();
            IEnumerator actualEnum = actual.GetEnumerator();

            int count;
            for (count = 0; ; count++)
            {
                bool expectedHasData = expectedEnum.MoveNext();
                bool actualHasData = actualEnum.MoveNext();

                if (!expectedHasData && !actualHasData)
                    return true;

                if (expectedHasData != actualHasData ||
                    !ObjectsEqual(expectedEnum.Current, actualEnum.Current, ref tolerance))
                {
                    FailurePoint fp = new FailurePoint();
                    fp.Position = count;
                    fp.ExpectedHasData = expectedHasData;
                    if (expectedHasData)
                        fp.ExpectedValue = expectedEnum.Current;
                    fp.ActualHasData = actualHasData;
                    if (actualHasData)
                        fp.ActualValue = actualEnum.Current;
                    failurePoints.Insert(0, fp);
                    return false;
                }
            }
        }

        /// <summary>
        /// Method to compare two DirectoryInfo objects
        /// </summary>
        /// <param name="expected">first directory to compare</param>
        /// <param name="actual">second directory to compare</param>
        /// <returns>true if equivalent, false if not</returns>
        private static bool DirectoriesEqual(DirectoryInfo expected, DirectoryInfo actual)
        {
            // Do quick compares first
            if (expected.Attributes != actual.Attributes ||
                expected.CreationTime != actual.CreationTime ||
                expected.LastAccessTime != actual.LastAccessTime)
            {
                return false;
            }

            // TODO: Find a cleaner way to do this
            return new SamePathConstraint(expected.FullName).Matches(actual.FullName);
        }

        private bool StreamsEqual(Stream expected, Stream actual)
        {
            if (expected == actual) return true;

            if (!expected.CanRead)
                throw new ArgumentException("Stream is not readable", "expected");
            if (!actual.CanRead)
                throw new ArgumentException("Stream is not readable", "actual");
            if (!expected.CanSeek)
                throw new ArgumentException("Stream is not seekable", "expected");
            if (!actual.CanSeek)
                throw new ArgumentException("Stream is not seekable", "actual");

            if (expected.Length != actual.Length) return false;

            byte[] bufferExpected = new byte[BUFFER_SIZE];
            byte[] bufferActual = new byte[BUFFER_SIZE];

            BinaryReader binaryReaderExpected = new BinaryReader(expected);
            BinaryReader binaryReaderActual = new BinaryReader(actual);

            long expectedPosition = expected.Position;
            long actualPosition = actual.Position;

            try
            {
                binaryReaderExpected.BaseStream.Seek(0, SeekOrigin.Begin);
                binaryReaderActual.BaseStream.Seek(0, SeekOrigin.Begin);

                for (long readByte = 0; readByte < expected.Length; readByte += BUFFER_SIZE)
                {
                    binaryReaderExpected.Read(bufferExpected, 0, BUFFER_SIZE);
                    binaryReaderActual.Read(bufferActual, 0, BUFFER_SIZE);

                    for (int count = 0; count < BUFFER_SIZE; ++count)
                    {
                        if (bufferExpected[count] != bufferActual[count])
                        {
                            FailurePoint fp = new FailurePoint();
                            fp.Position = (int)readByte + count;
                            failurePoints.Insert(0, fp);
                            return false;
                        }
                    }
                }
            }
            finally
            {
                expected.Position = expectedPosition;
                actual.Position = actualPosition;
            }

            return true;
        }
		
        #endregion

        #region Nested RecursionDetector class

        /// <summary>
        /// RecursionDetector detects when a comparison
        /// between two enumerables has reached a point
        /// where the same objects that were previously
        /// compared are again being compared. This allows
        /// the caller to stop the comparison if desired.
        /// </summary>
        class RecursionDetector
        {
#if CLR_2_0 || CLR_4_0
            readonly Dictionary<UnorderedReferencePair, object> table = new Dictionary<UnorderedReferencePair, object>();
#else
            readonly Hashtable table = new Hashtable();
#endif

            /// <summary>
            /// Check whether two objects have previously
            /// been compared, returning true if they have.
            /// The two objects are remembered, so that a
            /// second call will always return true.
            /// </summary>
            public bool CheckRecursion(IEnumerable expected, IEnumerable actual)
            {
                UnorderedReferencePair pair = new UnorderedReferencePair(expected, actual);

                if (ContainsPair(pair))
                    return true;

                table.Add(pair, null);
                return false;
            }

            private bool ContainsPair(UnorderedReferencePair pair)
            {
#if CLR_2_0 || CLR_4_0
                return table.ContainsKey(pair);
#else
                return table.Contains(pair);
#endif
            }

#if CLR_2_0 || CLR_4_0
            class UnorderedReferencePair : IEquatable<UnorderedReferencePair>
#else
            class UnorderedReferencePair
#endif
            {
                private readonly object first;
                private readonly object second;

                public UnorderedReferencePair(object first, object second)
                {
                    this.first = first;
                    this.second = second;
                }

                public bool Equals(UnorderedReferencePair other)
                {
                    return (Equals(first, other.first) && Equals(second, other.second)) ||
                           (Equals(first, other.second) && Equals(second, other.first));
                }

                public override bool Equals(object obj)
                {
                    if (ReferenceEquals(null, obj)) return false;
                    return obj is UnorderedReferencePair && Equals((UnorderedReferencePair)obj);
                }

                public override int GetHashCode()
                {
                    unchecked
                    {
                        return ((first != null ? first.GetHashCode() : 0) * 397) ^ ((second != null ? second.GetHashCode() : 0) * 397);
                    }
                }
            }
        }

        #endregion
    }
}