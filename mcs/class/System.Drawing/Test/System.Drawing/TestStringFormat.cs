//
// StringFormat class testing unit
//
// Authors:
// 	 Jordi Mas i Hernàndez (jordi@ximian.com)
//	 Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2004 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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
using System.Drawing;
using System.Drawing.Text;
using System.Security.Permissions;
using NUnit.Framework;

namespace MonoTests.System.Drawing{

	[TestFixture]	
	public class StringFormatTest {

		private void CheckDefaults (StringFormat sf)
		{
			Assert.AreEqual (StringAlignment.Near, sf.Alignment, "Alignment");
			Assert.AreEqual (0, sf.DigitSubstitutionLanguage, "DigitSubstitutionLanguage");
			Assert.AreEqual (StringDigitSubstitute.User, sf.DigitSubstitutionMethod, "DigitSubstitutionMethod");
			Assert.AreEqual ((StringFormatFlags) 0, sf.FormatFlags, "FormatFlags");
			Assert.AreEqual (HotkeyPrefix.None, sf.HotkeyPrefix, "HotkeyPrefix");
			Assert.AreEqual (StringAlignment.Near, sf.LineAlignment, "LineAlignment");
			Assert.AreEqual (StringTrimming.Character, sf.Trimming, "Trimming");
		}

		[Test]
		public void Default ()
		{
			using (StringFormat sf = new StringFormat ()) {
				CheckDefaults (sf);
				Assert.AreEqual ("[StringFormat, FormatFlags=0]", sf.ToString (), "ToString");
				// check setters validations
				sf.FormatFlags = (StringFormatFlags) Int32.MinValue;
				Assert.AreEqual ((StringFormatFlags) Int32.MinValue, sf.FormatFlags, "Min-FormatFlags");
				Assert.AreEqual ("[StringFormat, FormatFlags=-2147483648]", sf.ToString (), "ToString-2");
			}
		}

		[Test]
		public void Default_Dispose ()
		{
			StringFormat sf = new StringFormat ();
			sf.Dispose ();
			Assert.Throws<ArgumentException> (() => sf.ToString ());
		}

		[Test]
		public void ctor_StringFormat_Null ()
		{
			Assert.Throws<ArgumentNullException> (() => new StringFormat (null));
		}

		[Test]
		public void ctor_StringFormat ()
		{
			using (StringFormat sf = new StringFormat (StringFormat.GenericTypographic)) {
				CheckTypographic (sf);
			}
		}

		[Test]
		public void ctor_StringFormatFlags ()
		{
			using (StringFormat sf = new StringFormat ((StringFormatFlags)Int32.MinValue)) {
				Assert.AreEqual ((StringFormatFlags) Int32.MinValue, sf.FormatFlags, "FormatFlags");
			}
		}

		[Test]
		public void ctor_StringFormatFlags_Int32 ()
		{
			using (StringFormat sf = new StringFormat ((StringFormatFlags) Int32.MinValue, Int32.MinValue)) {
				Assert.AreEqual (0, sf.DigitSubstitutionLanguage, "DigitSubstitutionLanguage");
				Assert.AreEqual ((StringFormatFlags) Int32.MinValue, sf.FormatFlags, "FormatFlags");
			}
		}

		[Test]
		public void GenericDefault ()
		{
			CheckDefaults (StringFormat.GenericDefault);
		}

		[Test]
		public void GenericDefault_Dispose ()
		{
			StringFormat.GenericDefault.Dispose ();
			CheckDefaults (StringFormat.GenericDefault);
		}

		[Test]
		public void GenericDefault_Local_Dispose ()
		{
			StringFormat sf = StringFormat.GenericDefault;
			sf.Dispose (); // can't be cached
			Assert.Throws<ArgumentException> (() => CheckDefaults (sf));
		}

		private void CheckTypographic (StringFormat sf)
		{
			Assert.AreEqual (StringAlignment.Near, sf.Alignment, "Alignment");
			Assert.AreEqual (0, sf.DigitSubstitutionLanguage, "DigitSubstitutionLanguage");
			Assert.AreEqual (StringDigitSubstitute.User, sf.DigitSubstitutionMethod, "DigitSubstitutionMethod");
			Assert.AreEqual (StringFormatFlags.FitBlackBox | StringFormatFlags.LineLimit | StringFormatFlags.NoClip, sf.FormatFlags, "FormatFlags");
			Assert.AreEqual (HotkeyPrefix.None, sf.HotkeyPrefix, "HotkeyPrefix");
			Assert.AreEqual (StringAlignment.Near, sf.LineAlignment, "LineAlignment");
			Assert.AreEqual (StringTrimming.None, sf.Trimming, "Trimming");
		}

		[Test]
		public void GenericTypographic ()
		{
			StringFormat sf = StringFormat.GenericTypographic;
			CheckTypographic (sf);
			Assert.AreEqual ("[StringFormat, FormatFlags=FitBlackBox, LineLimit, NoClip]", sf.ToString (), "ToString");
		}

		[Test]
		public void GenericTypographic_Dispose ()
		{
			StringFormat.GenericTypographic.Dispose ();
			CheckTypographic (StringFormat.GenericTypographic);
		}

		[Test]
		public void GenericTypographic_Local_Dispose ()
		{
			StringFormat sf = StringFormat.GenericTypographic;
			sf.Dispose (); // can't be cached
			Assert.Throws<ArgumentException> (() => CheckTypographic (sf));
		}

		[Test]
		public void Alignment_All ()
		{
			using (StringFormat sf = new StringFormat ()) {
				foreach (StringAlignment sa in Enum.GetValues (typeof (StringAlignment))) {
					sf.Alignment = sa;
					Assert.AreEqual (sa, sf.Alignment, sa.ToString ());
				}
			}
		}

		[Test]
		public void Alignment_Invalid ()
		{
			using (StringFormat sf = new StringFormat ()) {
				Assert.Throws<InvalidEnumArgumentException> (() => sf.Alignment = (StringAlignment) Int32.MinValue);
			}
		}

		[Test]
		public void HotkeyPrefix_All ()
		{
			using (StringFormat sf = new StringFormat ()) {
				foreach (HotkeyPrefix hp in Enum.GetValues (typeof (HotkeyPrefix))) {
					sf.HotkeyPrefix = hp;
					Assert.AreEqual (hp, sf.HotkeyPrefix, hp.ToString ());
				}
			}
		}

		[Test]
		public void HotkeyPrefix_Invalid ()
		{
			using (StringFormat sf = new StringFormat ()) {
				Assert.Throws<InvalidEnumArgumentException> (() => sf.HotkeyPrefix = (HotkeyPrefix) Int32.MinValue);
			}
		}

		[Test]
		public void LineAlignment_All ()
		{
			using (StringFormat sf = new StringFormat ()) {
				foreach (StringAlignment sa in Enum.GetValues (typeof (StringAlignment))) {
					sf.LineAlignment = sa;
					Assert.AreEqual (sa, sf.LineAlignment, sa.ToString ());
				}
			}
		}

		[Test]
		public void LineAlignment_Invalid ()
		{
			using (StringFormat sf = new StringFormat ()) {
				Assert.Throws<InvalidEnumArgumentException> (() => sf.LineAlignment = (StringAlignment) Int32.MinValue);
			}
		}

		[Test]
		public void Trimming_All ()
		{
			using (StringFormat sf = new StringFormat ()) {
				foreach (StringTrimming st in Enum.GetValues (typeof (StringTrimming))) {
					sf.Trimming = st;
					Assert.AreEqual (st, sf.Trimming, st.ToString ());
				}
			}
		}

		[Test]
		public void Trimming_Invalid ()
		{
			using (StringFormat sf = new StringFormat ()) {
				Assert.Throws<InvalidEnumArgumentException> (() => sf.Trimming = (StringTrimming) Int32.MinValue);
			}
		}

		[Test]
		public void Clone() 
		{
			using (StringFormat sf = new StringFormat ()) {
				using (StringFormat clone = (StringFormat) sf.Clone ()) {
					CheckDefaults (clone);
				}
			}
		}

		[Test]
		public void Clone_Complex ()
		{
			using (StringFormat sf = new StringFormat ()) {
				CharacterRange[] ranges = new CharacterRange [2];
				ranges[0].First = 1;
				ranges[0].Length = 2;
				ranges[1].First = 3;
				ranges[1].Length = 4;
				sf.SetMeasurableCharacterRanges (ranges);

				float[] stops = new float [2];
				stops [0] = 6.0f;
				stops [1] = 7.0f;
				sf.SetTabStops (5.0f, stops);

				using (StringFormat clone = (StringFormat) sf.Clone ()) {
					CheckDefaults (clone);

					float first;
					float[] cloned_stops = clone.GetTabStops (out first);
					Assert.AreEqual (5.0f, first, "first");
					Assert.AreEqual (6.0f, cloned_stops[0], "cloned_stops[0]");
					Assert.AreEqual (7.0f, cloned_stops[1], "cloned_stops[1]");
				}
			}
		}
			
		[Test]
		public void TestFormatFlags() 
		{
			using (StringFormat smf = new StringFormat ()) {
				smf.FormatFlags = StringFormatFlags.DisplayFormatControl;
				Assert.AreEqual (StringFormatFlags.DisplayFormatControl, smf.FormatFlags);
			}
		}		
		
		[Test]
		public void TabsStops() 
		{
			using (StringFormat smf = new StringFormat ()) {
				float firstTabOffset;
				float[] tabsSrc = { 100, 200, 300, 400 };
				float[] tabStops;

				smf.SetTabStops (200, tabsSrc);
				tabStops = smf.GetTabStops (out firstTabOffset);

				Assert.AreEqual (200, firstTabOffset);
				Assert.AreEqual (tabsSrc.Length, tabStops.Length);
				Assert.AreEqual (tabsSrc[0], tabStops[0]);
				Assert.AreEqual (tabsSrc[1], tabStops[1]);
				Assert.AreEqual (tabsSrc[2], tabStops[2]);
				Assert.AreEqual (tabsSrc[3], tabStops[3]);
			}
		}

		[Test]
		public void SetTabStops_Null ()
		{
			using (StringFormat sf = new StringFormat ()) {
				Assert.Throws<NullReferenceException> (() => sf.SetTabStops (Single.NaN, null));
			}
		}

		[Test]
		public void SetDigitSubstitution ()
		{
			using (StringFormat sf = new StringFormat ()) {
				sf.SetDigitSubstitution (Int32.MinValue, (StringDigitSubstitute) Int32.MinValue);
				Assert.AreEqual (0, sf.DigitSubstitutionLanguage, "DigitSubstitutionLanguage");
				Assert.AreEqual ((StringDigitSubstitute) Int32.MinValue, sf.DigitSubstitutionMethod, "DigitSubstitutionMethod");
			}
		}

		[Test]
		public void SetMeasurableCharacterRanges_Null ()
		{
			using (StringFormat sf = new StringFormat ()) {
				Assert.Throws<NullReferenceException> (() => sf.SetMeasurableCharacterRanges (null));
			}
		}

		[Test]
		public void SetMeasurableCharacterRanges_Empty ()
		{
			using (StringFormat sf = new StringFormat ()) {
				CharacterRange[] range = new CharacterRange[0];
				sf.SetMeasurableCharacterRanges (range);
			}
		}

		[Test]
		public void SetMeasurableCharacterRanges_Max ()
		{
			using (StringFormat sf = new StringFormat ()) {
				CharacterRange[] range = new CharacterRange[32];
				sf.SetMeasurableCharacterRanges (range);
			}
		}

		[Test]
		public void SetMeasurableCharacterRanges_TooBig ()
		{
			using (StringFormat sf = new StringFormat ()) {
				CharacterRange[] range = new CharacterRange[33];
				Assert.Throws<OverflowException> (() => sf.SetMeasurableCharacterRanges (range));
			}
		}
	}
}
