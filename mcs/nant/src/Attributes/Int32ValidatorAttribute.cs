// NAnt - A .NET build tool
// Copyright (C) 2001 Gerry Shaw
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//
// Gerry Shaw (gerry_shaw@yahoo.com)

namespace SourceForge.NAnt {

    using System;
    using System.Reflection;

    /// <summary>Indicates that field should be able to be converted into a Int32 within the given range.</summary>
    [AttributeUsage(AttributeTargets.Field, Inherited=true)]
    public class Int32ValidatorAttribute : ValidatorAttribute {

        int _minValue = Int32.MinValue;
        int _maxValue = Int32.MaxValue;

        public Int32ValidatorAttribute() {
        }

        public Int32ValidatorAttribute(int minValue, int maxValue) {
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public int MinValue {
            get { return _minValue; }
            set { _minValue = value; }
        }

        public int MaxValue {
            get { return _maxValue; }
            set { _maxValue = value; }
        }

        public override string Validate(object value) {
            string errorMessage = null;
            try {
                Int32 intValue = Convert.ToInt32(value);
                if (intValue < MinValue || intValue > MaxValue) {
                    errorMessage = String.Format("Cannot resolve '{0}' to integer between '{1}' and '{2}'.", value.ToString(), MinValue, MaxValue);
                }
            } catch (Exception) {
                errorMessage = String.Format("Cannot resolve '{0}' to integer value.", value.ToString());
            }
            return errorMessage;
        }
    }
}