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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Brian O'Keefe (zer0keefie@gmail.com)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel;

namespace MonoTests.System.ComponentModel
{
	[TestFixture]
	public class PropertyFilterAttributeTest
	{
		public PropertyFilterAttributeTest()
		{
		}

		[Test]
		public void PropertyFilterAttributeFilterTest()
		{
			Assert.AreEqual (PropertyFilterOptions.All, PropertyFilterAttribute.Default.Filter, "Filter_#1");

			Assert.AreEqual (PropertyFilterOptions.None, new PropertyFilterAttribute(PropertyFilterOptions.None).Filter, "Filter_#2");
			Assert.AreEqual (PropertyFilterOptions.SetValues, new PropertyFilterAttribute (PropertyFilterOptions.SetValues).Filter, "Filter_#3");

			Assert.AreEqual (PropertyFilterOptions.Valid.GetHashCode(), new PropertyFilterAttribute (PropertyFilterOptions.Valid).GetHashCode(), "Filter_#4");
		}

		private static PropertyFilterAttribute [] CreateAllAttributeOptions()
		{
			// This iterates over all possible combinations 
			PropertyFilterAttribute [] opts = new PropertyFilterAttribute [16];
			for (int i = 0; i < 16; i++) {
				// Note: This is certainly not an ideal technique for this, but it saves space
				opts [i] = new PropertyFilterAttribute ((PropertyFilterOptions)i);
			}
			return opts;
		}

		private static readonly PropertyFilterAttribute [] AllAttributeOptions = CreateAllAttributeOptions ();

		private static void ValidateFilterValues(PropertyFilterAttribute test, bool[] matchResults, int equalsResult, string message)
		{
			for (int i = 0; i < 16; i++) {
				Assert.AreEqual(matchResults[i], test.Match(AllAttributeOptions[i]),
						message + " - Match - Iteration " + i + ": " + 
						Enum.GetName(typeof(PropertyFilterOptions), (PropertyFilterOptions)i));
				Assert.AreEqual (equalsResult == i, test.Equals (AllAttributeOptions [i]),
						message + " - Equals - Iteration " + i + ": " +
						Enum.GetName (typeof (PropertyFilterOptions), (PropertyFilterOptions)i));
			}
		}

		[Test]
		public void PropertyFilterAttributeOptionsNoneTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.None);

			bool [] matches = new bool [] {
				true, true, true, true, true, true, true, true,
				true, true, true, true, true, true, true, true,
			};

			ValidateFilterValues (all, matches, (int)PropertyFilterOptions.None, "None");
		}

		[Test]
		public void PropertyFilterAttributeOptionsInvalidTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.Invalid);

			bool [] matches = new bool [] {
				false, true, false, true, false, true, false, true,
				false, true, false, true, false, true, false, true,
			};

			ValidateFilterValues (all, matches, (int)PropertyFilterOptions.Invalid, "Invalid");
		}

		[Test]
		public void PropertyFilterAttributeOptionsSetValuesTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.SetValues);

			bool [] matches = new bool [] {
				false, false, true, true, false, false, true, true,
				false, false, true, true, false, false, true, true,
			};

			ValidateFilterValues (all, matches, (int)PropertyFilterOptions.SetValues, "SetValues");
		}

		[Test]
		public void PropertyFilterAttributeOptionsUnsetValuesTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.UnsetValues);

			bool [] matches = new bool [] {
				false, false, false, false, true, true, true, true,
				false, false, false, false, true, true, true, true,
			};

			ValidateFilterValues (all, matches, (int)PropertyFilterOptions.UnsetValues, "UnsetValues");
		}

		[Test]
		public void PropertyFilterAttributeOptionsValidTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.Valid);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, false, false,
				true, true, true, true, true, true, true, true,
			};

			ValidateFilterValues (all, matches, (int)PropertyFilterOptions.Valid, "Valid");
		}

		[Test]
		public void PropertyFilterAttributeOptionsAllTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.All);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, false, false,
				false, false, false, false, false, false, false, true,
			};

			ValidateFilterValues (all, matches, (int)PropertyFilterOptions.All, "All");
		}

		[Test]
		public void PropertyFilterAttributeOptionsInvalidSetValuesTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.Invalid | PropertyFilterOptions.SetValues);

			bool [] matches = new bool [] {
				false, false, false, true, false, false, false, true,
				false, false, false, true, false, false, false, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.Invalid | PropertyFilterOptions.SetValues), "Invalid|SetValues");
		}

		[Test]
		public void PropertyFilterAttributeOptionsInvalidUnsetValuesTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.Invalid | PropertyFilterOptions.UnsetValues);

			bool [] matches = new bool [] {
				false, false, false, false, false, true, false, true,
				false, false, false, false, false, true, false, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.Invalid | PropertyFilterOptions.UnsetValues), "Invalid|UnsetValues");
		}

		[Test]
		public void PropertyFilterAttributeOptionsInvalidValidTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.Invalid | PropertyFilterOptions.Valid);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, false, false,
				false, true, false, true, false, true, false, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.Invalid | PropertyFilterOptions.Valid), "Invalid|Valid");
		}

		[Test]
		public void PropertyFilterAttributeOptionsSetValuesUnsetValuesTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, true, true,
				false, false, false, false, false, false, true, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues), "SetValues|UnsetValues");
		}

		[Test]
		public void PropertyFilterAttributeOptionsSetValuesValidTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.SetValues | PropertyFilterOptions.Valid);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, false, false,
				false, false, true, true, false, false, true, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.SetValues | PropertyFilterOptions.Valid), "SetValues|Valid");
		}

		[Test]
		public void PropertyFilterAttributeOptionsUnsetValuesValidTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.UnsetValues | PropertyFilterOptions.Valid);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, false, false,
				false, false, false, false, true, true, true, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.UnsetValues | PropertyFilterOptions.Valid), "UnsetValues|Valid");
		}

		[Test]
		public void PropertyFilterAttributeOptionsInvalidSetValuesUnsetValuesTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.Invalid | PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, false, true,
				false, false, false, false, false, false, false, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.Invalid | PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues), "Invalid|SetValues|UnsetValues");
		}

		[Test]
		public void PropertyFilterAttributeOptionsInvalidSetValuesValidTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.Invalid | PropertyFilterOptions.SetValues | PropertyFilterOptions.Valid);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, false, false,
				false, false, false, true, false, false, false, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.Invalid | PropertyFilterOptions.SetValues | PropertyFilterOptions.Valid), "Invalid|SetValues|Valid");
		}

		[Test]
		public void PropertyFilterAttributeOptionsInvalidUnsetValuesValidTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.Invalid | PropertyFilterOptions.UnsetValues | PropertyFilterOptions.Valid);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, false, false,
				false, false, false, false, false, true, false, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.Invalid | PropertyFilterOptions.UnsetValues | PropertyFilterOptions.Valid), "Invalid|UnsetValues|Valid");
		}

		[Test]
		public void PropertyFilterAttributeOptionsSetValuesUnsetValuesValidTest()
		{
			PropertyFilterAttribute all = new PropertyFilterAttribute (PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues | PropertyFilterOptions.Valid);

			bool [] matches = new bool [] {
				false, false, false, false, false, false, false, false,
				false, false, false, false, false, false, true, true,
			};

			ValidateFilterValues (all, matches, (int)(PropertyFilterOptions.SetValues | PropertyFilterOptions.UnsetValues | PropertyFilterOptions.Valid), "SetValues|UnsetValues|Valid");
		}
	}

}
