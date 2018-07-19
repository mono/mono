//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Globalization;
    using System.Runtime;

    struct SequenceRange
    {
        // fields
        Int64 lower;
        Int64 upper;

        // constructors
        public SequenceRange(Int64 number)
            : this(number, number)
        {
        }

        public SequenceRange(Int64 lower, Int64 upper)
        {
            if (lower < 0)
            {
                throw Fx.AssertAndThrow("Argument lower cannot be negative.");
            }

            if (lower > upper)
            {
                throw Fx.AssertAndThrow("Argument upper cannot be less than argument lower.");
            }

            this.lower = lower;
            this.upper = upper;
        }

        // properties
        public Int64 Lower
        {
            get { return this.lower; }
        }

        public Int64 Upper
        {
            get { return this.upper; }
        }

        public static bool operator ==(SequenceRange a, SequenceRange b)
        {
            return (a.lower == b.lower) && (a.upper == b.upper);
        }

        public static bool operator !=(SequenceRange a, SequenceRange b)
        {
            return !(a == b);
        }

        public bool Contains(Int64 number)
        {
            return (number >= this.lower && number <= this.upper);
        }

        public bool Contains(SequenceRange range)
        {
            return (range.Lower >= this.lower && range.Upper <= this.upper);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj is SequenceRange)
            {
                return this == (SequenceRange)obj;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            Int64 hashCode = (this.upper ^ (this.upper - this.lower));
            return (int)((hashCode << 32) ^ (hashCode >> 32));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}", this.lower, this.upper);
        }
    }
}
