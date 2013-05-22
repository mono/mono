//
// System.Configuration.TimeSpanValidatorAttribute.cs
//
// Authors:
//  Lluis Sanchez Gual (lluis@novell.com)
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
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
//

namespace System.Configuration
{
	[AttributeUsage (AttributeTargets.Property)]
	public sealed class TimeSpanValidatorAttribute: ConfigurationValidatorAttribute
	{
		bool excludeRange = false;
		string maxValueString = TimeSpanMaxValue;
		string minValueString = TimeSpanMinValue;
		
		public const string TimeSpanMaxValue = "10675199.02:48:05.4775807";
		public const string TimeSpanMinValue = "-10675199.02:48:05.4775808";
		
		ConfigurationValidatorBase instance;
		
		public string MaxValueString {
			get { return maxValueString; }
			set { maxValueString = value; instance = null; }
		}
		
		public string MinValueString {
			get { return minValueString; }
			set { minValueString = value; instance = null; }
		}
		
		public TimeSpan MaxValue {
			get { return TimeSpan.Parse (maxValueString); }
		}
		
		public TimeSpan MinValue {
			get { return TimeSpan.Parse (minValueString); }
		}
		
		public bool ExcludeRange {
			get { return excludeRange; }
			set { excludeRange = value; instance = null; }
		}
		
		public override ConfigurationValidatorBase ValidatorInstance {
			get {
				if (instance == null)
					instance = new TimeSpanValidator (MinValue, MaxValue, excludeRange);
				return instance;
			}
		}
	}
}

