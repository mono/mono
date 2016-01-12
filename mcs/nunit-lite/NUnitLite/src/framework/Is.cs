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
using System.Collections;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
    /// <summary>
    /// Helper class with properties and methods that supply
    /// a number of constraints used in Asserts.
    /// </summary>
    public class Is
    {
        #region Not

        /// <summary>
        /// Returns a ConstraintExpression that negates any
        /// following constraint.
        /// </summary>
        public static ConstraintExpression Not
        {
            get { return new ConstraintExpression().Not; }
        }

        #endregion

        #region All

        /// <summary>
        /// Returns a ConstraintExpression, which will apply
        /// the following constraint to all members of a collection,
        /// succeeding if all of them succeed.
        /// </summary>
        public static ConstraintExpression All
        {
            get { return new ConstraintExpression().All; }
        }

        #endregion

        #region Null

        /// <summary>
        /// Returns a constraint that tests for null
        /// </summary>
        public static NullConstraint Null
        {
            get { return new NullConstraint(); }
        }

        #endregion

        #region True

        /// <summary>
        /// Returns a constraint that tests for True
        /// </summary>
        public static TrueConstraint True
        {
            get { return new TrueConstraint(); }
        }

        #endregion

        #region False

        /// <summary>
        /// Returns a constraint that tests for False
        /// </summary>
        public static FalseConstraint False
        {
            get { return new FalseConstraint(); }
        }

        #endregion

        #region Positive
 
        /// <summary>
        /// Returns a constraint that tests for a positive value
        /// </summary>
        public static GreaterThanConstraint Positive
        {
            get { return new GreaterThanConstraint(0); }
        }
 
        #endregion
 
        #region Negative
 
        /// <summary>
        /// Returns a constraint that tests for a negative value
        /// </summary>
        public static LessThanConstraint Negative
        {
            get { return new LessThanConstraint(0); }
        }
 
        #endregion

        #region NaN

        /// <summary>
        /// Returns a constraint that tests for NaN
        /// </summary>
        public static NaNConstraint NaN
        {
            get { return new NaNConstraint(); }
        }

        #endregion

        #region Empty

        /// <summary>
        /// Returns a constraint that tests for empty
        /// </summary>
        public static EmptyConstraint Empty
        {
            get { return new EmptyConstraint(); }
        }

        #endregion

        #region Unique

        /// <summary>
        /// Returns a constraint that tests whether a collection 
        /// contains all unique items.
        /// </summary>
        public static UniqueItemsConstraint Unique
        {
            get { return new UniqueItemsConstraint(); }
        }

        #endregion

        #region BinarySerializable

#if !NETCF && !SILVERLIGHT
        /// <summary>
        /// Returns a constraint that tests whether an object graph is serializable in binary format.
        /// </summary>
        public static BinarySerializableConstraint BinarySerializable
        {
            get { return new BinarySerializableConstraint(); }
        }
#endif

        #endregion

        #region XmlSerializable

#if !SILVERLIGHT
        /// <summary>
        /// Returns a constraint that tests whether an object graph is serializable in xml format.
        /// </summary>
        public static XmlSerializableConstraint XmlSerializable
        {
            get { return new XmlSerializableConstraint(); }
        }
#endif

        #endregion

        #region EqualTo

        /// <summary>
        /// Returns a constraint that tests two items for equality
        /// </summary>
        public static EqualConstraint EqualTo(object expected)
        {
            return new EqualConstraint(expected);
        }

        #endregion

        #region SameAs

        /// <summary>
        /// Returns a constraint that tests that two references are the same object
        /// </summary>
        public static SameAsConstraint SameAs(object expected)
        {
            return new SameAsConstraint(expected);
        }

        #endregion

        #region GreaterThan

        /// <summary>
        /// Returns a constraint that tests whether the
        /// actual value is greater than the suppled argument
        /// </summary>
        public static GreaterThanConstraint GreaterThan(object expected)
        {
            return new GreaterThanConstraint(expected);
        }

        #endregion

        #region GreaterThanOrEqualTo

        /// <summary>
        /// Returns a constraint that tests whether the
        /// actual value is greater than or equal to the suppled argument
        /// </summary>
        public static GreaterThanOrEqualConstraint GreaterThanOrEqualTo(object expected)
        {
            return new GreaterThanOrEqualConstraint(expected);
        }

        /// <summary>
        /// Returns a constraint that tests whether the
        /// actual value is greater than or equal to the suppled argument
        /// </summary>
        public static GreaterThanOrEqualConstraint AtLeast(object expected)
        {
            return new GreaterThanOrEqualConstraint(expected);
        }

        #endregion

        #region LessThan

        /// <summary>
        /// Returns a constraint that tests whether the
        /// actual value is less than the suppled argument
        /// </summary>
        public static LessThanConstraint LessThan(object expected)
        {
            return new LessThanConstraint(expected);
        }

        #endregion

        #region LessThanOrEqualTo

        /// <summary>
        /// Returns a constraint that tests whether the
        /// actual value is less than or equal to the suppled argument
        /// </summary>
        public static LessThanOrEqualConstraint LessThanOrEqualTo(object expected)
        {
            return new LessThanOrEqualConstraint(expected);
        }

        /// <summary>
        /// Returns a constraint that tests whether the
        /// actual value is less than or equal to the suppled argument
        /// </summary>
        public static LessThanOrEqualConstraint AtMost(object expected)
        {
            return new LessThanOrEqualConstraint(expected);
        }

        #endregion

        #region TypeOf

        /// <summary>
        /// Returns a constraint that tests whether the actual
        /// value is of the exact type supplied as an argument.
        /// </summary>
        public static ExactTypeConstraint TypeOf(Type expectedType)
        {
            return new ExactTypeConstraint(expectedType);
        }

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Returns a constraint that tests whether the actual
        /// value is of the exact type supplied as an argument.
        /// </summary>
        public static ExactTypeConstraint TypeOf<T>()
        {
            return new ExactTypeConstraint(typeof(T));
        }
#endif

        #endregion

        #region InstanceOf

        /// <summary>
        /// Returns a constraint that tests whether the actual value
        /// is of the type supplied as an argument or a derived type.
        /// </summary>
        public static InstanceOfTypeConstraint InstanceOf(Type expectedType)
        {
            return new InstanceOfTypeConstraint(expectedType);
        }

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Returns a constraint that tests whether the actual value
        /// is of the type supplied as an argument or a derived type.
        /// </summary>
        public static InstanceOfTypeConstraint InstanceOf<T>()
        {
            return new InstanceOfTypeConstraint(typeof(T));
        }
#endif

        #endregion

        #region AssignableFrom

        /// <summary>
        /// Returns a constraint that tests whether the actual value
        /// is assignable from the type supplied as an argument.
        /// </summary>
        public static AssignableFromConstraint AssignableFrom(Type expectedType)
        {
            return new AssignableFromConstraint(expectedType);
        }

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Returns a constraint that tests whether the actual value
        /// is assignable from the type supplied as an argument.
        /// </summary>
        public static AssignableFromConstraint AssignableFrom<T>()
        {
            return new AssignableFromConstraint(typeof(T));
        }
#endif

        #endregion

        #region AssignableTo

        /// <summary>
        /// Returns a constraint that tests whether the actual value
        /// is assignable from the type supplied as an argument.
        /// </summary>
        public static AssignableToConstraint AssignableTo(Type expectedType)
        {
            return new AssignableToConstraint(expectedType);
        }

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Returns a constraint that tests whether the actual value
        /// is assignable from the type supplied as an argument.
        /// </summary>
        public static AssignableToConstraint AssignableTo<T>()
        {
            return new AssignableToConstraint(typeof(T));
        }
#endif

        #endregion

        #region EquivalentTo

        /// <summary>
        /// Returns a constraint that tests whether the actual value
        /// is a collection containing the same elements as the 
        /// collection supplied as an argument.
        /// </summary>
        public static CollectionEquivalentConstraint EquivalentTo(IEnumerable expected)
        {
            return new CollectionEquivalentConstraint(expected);
        }

        #endregion

        #region SubsetOf

        /// <summary>
        /// Returns a constraint that tests whether the actual value
        /// is a subset of the collection supplied as an argument.
        /// </summary>
        public static CollectionSubsetConstraint SubsetOf(IEnumerable expected)
        {
            return new CollectionSubsetConstraint(expected);
        }

        #endregion

        #region Ordered

        /// <summary>
        /// Returns a constraint that tests whether a collection is ordered
        /// </summary>
        public static CollectionOrderedConstraint Ordered
        {
            get { return new CollectionOrderedConstraint(); }
        }

        #endregion

        #region StringContaining

        /// <summary>
        /// Returns a constraint that succeeds if the actual
        /// value contains the substring supplied as an argument.
        /// </summary>
        public static SubstringConstraint StringContaining(string expected)
        {
            return new SubstringConstraint(expected);
        }

        #endregion

        #region StringStarting

        /// <summary>
        /// Returns a constraint that succeeds if the actual
        /// value starts with the substring supplied as an argument.
        /// </summary>
        public static StartsWithConstraint StringStarting(string expected)
        {
            return new StartsWithConstraint(expected);
        }

        #endregion

        #region StringEnding

        /// <summary>
        /// Returns a constraint that succeeds if the actual
        /// value ends with the substring supplied as an argument.
        /// </summary>
        public static EndsWithConstraint StringEnding(string expected)
        {
            return new EndsWithConstraint(expected);
        }

        #endregion

        #region StringMatching

#if !NETCF
        /// <summary>
        /// Returns a constraint that succeeds if the actual
        /// value matches the regular expression supplied as an argument.
        /// </summary>
        public static RegexConstraint StringMatching(string pattern)
        {
            return new RegexConstraint(pattern);
        }
#endif

        #endregion

        #region SamePath

        /// <summary>
        /// Returns a constraint that tests whether the path provided 
        /// is the same as an expected path after canonicalization.
        /// </summary>
        public static SamePathConstraint SamePath(string expected)
        {
            return new SamePathConstraint(expected);
        }

        #endregion

        #region SubPath

        /// <summary>
        /// Returns a constraint that tests whether the path provided 
        /// is under an expected path after canonicalization.
        /// </summary>
        public static SubPathConstraint SubPath(string expected)
        {
            return new SubPathConstraint(expected);
        }

        #endregion

        #region SamePathOrUnder

        /// <summary>
        /// Returns a constraint that tests whether the path provided 
        /// is the same path or under an expected path after canonicalization.
        /// </summary>
        public static SamePathOrUnderConstraint SamePathOrUnder(string expected)
        {
            return new SamePathOrUnderConstraint(expected);
        }

        #endregion

        #region InRange

#if CLR_2_0 || CLR_4_0
        /// <summary>
        /// Returns a constraint that tests whether the actual value falls 
        /// within a specified range.
        /// </summary>
        public static RangeConstraint<T> InRange<T>(T from, T to) where T : IComparable<T>
        {
            return new RangeConstraint<T>(from, to);
        }
#else
        /// <summary>
        /// Returns a constraint that tests whether the actual value falls 
        /// within a specified range.
        /// </summary>
        public static RangeConstraint InRange(IComparable from, IComparable to)
        {
            return new RangeConstraint(from, to);
        }
#endif

        #endregion

    }
}
