// ***********************************************************************
// Copyright (c) 2008 Charlie Poole
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

namespace NUnit.Framework.Constraints
{
    /// <summary>
    /// The Tolerance class generalizes the notion of a tolerance
    /// within which an equality test succeeds. Normally, it is
    /// used with numeric types, but it can be used with any
    /// type that supports taking a difference between two 
    /// objects and comparing that difference to a value.
    /// </summary>
    public class Tolerance
    {
        private readonly ToleranceMode mode;
        private readonly object amount;

        private const string ModeMustFollowTolerance = "Tolerance amount must be specified before setting mode";
        private const string MultipleToleranceModes = "Tried to use multiple tolerance modes at the same time";
        private const string NumericToleranceRequired = "A numeric tolerance is required";

        /// <summary>
        /// Returns an empty Tolerance object, equivalent to
        /// specifying no tolerance. In most cases, it results
        /// in an exact match but for floats and doubles a
        /// default tolerance may be used.
        /// </summary>
        public static Tolerance Empty
        {
            get { return new Tolerance(0, ToleranceMode.None); }
        }

        /// <summary>
        /// Returns a zero Tolerance object, equivalent to 
        /// specifying an exact match.
        /// </summary>
        public static Tolerance Zero
        {
            get { return new Tolerance(0, ToleranceMode.Linear); }
        }

        /// <summary>
        /// Constructs a linear tolerance of a specdified amount
        /// </summary>
        public Tolerance(object amount) : this(amount, ToleranceMode.Linear) { }

        /// <summary>
        /// Constructs a tolerance given an amount and ToleranceMode
        /// </summary>
        private Tolerance(object amount, ToleranceMode mode)
        {
            this.amount = amount;
            this.mode = mode;
        }

        /// <summary>
        /// Gets the ToleranceMode for the current Tolerance
        /// </summary>
        public ToleranceMode Mode
        {
            get { return this.mode; }
        }
        

        /// <summary>
        /// Tests that the current Tolerance is linear with a 
        /// numeric value, throwing an exception if it is not.
        /// </summary>
        private void CheckLinearAndNumeric()
        {
            if (mode != ToleranceMode.Linear)
                throw new InvalidOperationException(mode == ToleranceMode.None
                    ? ModeMustFollowTolerance
                    : MultipleToleranceModes);

            if (!Numerics.IsNumericType(amount))
                throw new InvalidOperationException(NumericToleranceRequired);
        }

        /// <summary>
        /// Gets the value of the current Tolerance instance.
        /// </summary>
        public object Value
        {
            get { return this.amount; }
        }

        /// <summary>
        /// Returns a new tolerance, using the current amount as a percentage.
        /// </summary>
        public Tolerance Percent
        {
            get
            {
                CheckLinearAndNumeric();
                return new Tolerance(this.amount, ToleranceMode.Percent);
            }
        }

        /// <summary>
        /// Returns a new tolerance, using the current amount in Ulps.
        /// </summary>
        public Tolerance Ulps
        {
            get
            {
                CheckLinearAndNumeric();
                return new Tolerance(this.amount, ToleranceMode.Ulps);
            }
        }

        /// <summary>
        /// Returns a new tolerance with a TimeSpan as the amount, using 
        /// the current amount as a number of days.
        /// </summary>
        public Tolerance Days
        {
            get
            {
                CheckLinearAndNumeric();
                return new Tolerance(TimeSpan.FromDays(Convert.ToDouble(amount)));
            }
        }

        /// <summary>
        /// Returns a new tolerance with a TimeSpan as the amount, using 
        /// the current amount as a number of hours.
        /// </summary>
        public Tolerance Hours
        {
            get
            {
                CheckLinearAndNumeric();
                return new Tolerance(TimeSpan.FromHours(Convert.ToDouble(amount)));
            }
        }

        /// <summary>
        /// Returns a new tolerance with a TimeSpan as the amount, using 
        /// the current amount as a number of minutes.
        /// </summary>
        public Tolerance Minutes
        {
            get
            {
                CheckLinearAndNumeric();
                return new Tolerance(TimeSpan.FromMinutes(Convert.ToDouble(amount)));
            }
        }

        /// <summary>
        /// Returns a new tolerance with a TimeSpan as the amount, using 
        /// the current amount as a number of seconds.
        /// </summary>
        public Tolerance Seconds
        {
            get
            {
                CheckLinearAndNumeric();
                return new Tolerance(TimeSpan.FromSeconds(Convert.ToDouble(amount)));
            }
        }

        /// <summary>
        /// Returns a new tolerance with a TimeSpan as the amount, using 
        /// the current amount as a number of milliseconds.
        /// </summary>
        public Tolerance Milliseconds
        {
            get
            {
                CheckLinearAndNumeric();
                return new Tolerance(TimeSpan.FromMilliseconds(Convert.ToDouble(amount)));
            }
        }

        /// <summary>
        /// Returns a new tolerance with a TimeSpan as the amount, using 
        /// the current amount as a number of clock ticks.
        /// </summary>
        public Tolerance Ticks
        {
            get
            {
                CheckLinearAndNumeric();
                return new Tolerance(TimeSpan.FromTicks(Convert.ToInt64(amount)));
            }
        }

        /// <summary>
        /// Returns true if the current tolerance is empty.
        /// </summary>
        public bool IsEmpty
        {
            get { return mode == ToleranceMode.None; }
        }
    }
}