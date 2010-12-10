//
// RangeAttribute.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
//

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
//
using System;
using System.ComponentModel;

namespace System.ComponentModel.DataAnnotations
{
	[AttributeUsage (AttributeTargets.Property|AttributeTargets.Field, AllowMultiple = false)]
	public class RangeAttribute : ValidationAttribute
	{
		Func <object, bool> comparer;
		TypeConverter cvt;

		public object Maximum { get; private set; }
		public object Minimum { get; private set; }
		public Type OperandType { get; private set; }

		IComparable MaximumComparable {
			get { return Maximum as IComparable; }
		}

		IComparable MinimumComparable {
			get { return Minimum as IComparable; }
		}
		
		RangeAttribute ()
			: base (GetDefaultErrorMessage)
		{
		}
		
		public RangeAttribute (double minimum, double maximum) : this ()
		{
			Minimum = minimum;
			Maximum = maximum;
			OperandType = typeof (double);
		}

		public RangeAttribute (int minimum, int maximum) : this ()
		{
			Minimum = minimum;
			Maximum = maximum;
			OperandType = typeof (int);
		}

		public RangeAttribute (Type type, string minimum, string maximum) : this ()
		{
#if !NET_4_0
			if (type == null)
				throw new ArgumentNullException ("type");
#endif
			OperandType = type;
			Minimum = minimum;
			Maximum = maximum;
#if !NET_4_0
			comparer = SetupComparer ();
#endif
		}

		static string GetDefaultErrorMessage ()
		{
			return "The field {0} must be between {1} and {2}.";
		}
		
		public override string FormatErrorMessage (string name)
		{
			if (comparer == null)
				comparer = SetupComparer ();

			return String.Format (ErrorMessageString, name, Minimum, Maximum);
		}

		// LAMESPEC: does not throw ValidationException when value is out of range
		public override bool IsValid (object value)
		{
			if (comparer == null)
				comparer = SetupComparer ();
			
			if (value == null)
				return true;

			string s = value as string;
			if (s != null && s.Length == 0)
				return true;
			
			try {
				if (comparer != null)
					return comparer (value);

				return false;
			} catch (FormatException) {
				return false;
			} catch (InvalidCastException) {
				return false;
			}
		}

		Func <object, bool> SetupComparer ()
		{
			Type ot = OperandType;

			object min = Minimum, max = Maximum;
#if NET_4_0
			if (min == null || max == null)
				throw new InvalidOperationException ("The minimum and maximum values must be set.");
#endif
			if (min is int)
				return new Func <object, bool> (CompareInt);

			if (min is double)
				return new Func <object, bool> (CompareDouble);
			
			if (ot == null)
				throw new InvalidOperationException ("The OperandType must be set when strings are used for minimum and maximum values.");
			
			if (!typeof(IComparable).IsAssignableFrom (ot)) {
#if NET_4_0
				string message = String.Format ("The type {0} must implement System.IComparable", ot.FullName);
				throw new InvalidOperationException (message);
#else
				throw new ArgumentException ("object");
#endif
			}
			
			string smin = min as string, smax = max as string;
			cvt = TypeDescriptor.GetConverter (ot);
			Minimum = cvt.ConvertFromString (smin);
			Maximum = cvt.ConvertFromString (smax);

			return new Func <object, bool> (CompareArbitrary);
		}

		bool CompareInt (object value)
		{
			int cv = Convert.ToInt32 (value);

			return MinimumComparable.CompareTo (cv) <= 0 && MaximumComparable.CompareTo (cv) >= 0;
		}

		bool CompareDouble (object value)
		{
			double cv = Convert.ToDouble (value);
			
			return MinimumComparable.CompareTo (cv) <= 0 && MaximumComparable.CompareTo (cv) >= 0;
		}

		bool CompareArbitrary (object value)
		{
			object cv;
			if (value != null && value.GetType () == OperandType)
				cv = value;
			else if (cvt != null)
				cv = cvt.ConvertFrom (value);
			else
				cv = null;
			
			return MinimumComparable.CompareTo (cv) <= 0 && MaximumComparable.CompareTo (cv) >= 0;
		}
	}
}
