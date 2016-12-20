// TextInfoTest.cs - NUnit Test Cases for the
// System.Globalization.TextInfo class
//
// Miguel de Icaza <miguel@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
//

using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace MonoTests.System.Globalization
{

[TestFixture]
public class TextInfoTest {

	[Test]
	public void TitleCase ()
	{
		TextInfo ti = new CultureInfo ("en-US", false).TextInfo;

		Assert.AreEqual (" The Dog", ti.ToTitleCase (" the dog"), "#1");
		Assert.AreEqual (" The Dude", ti.ToTitleCase (" The Dude"), "#2");
		Assert.AreEqual ("La Guerra Yla Paz", ti.ToTitleCase ("la Guerra yLa pAz"), "#3");
		Assert.AreEqual ("\tTab\tAnd\tPeace", ti.ToTitleCase ("\ttab\taNd\tpeaCE"), "#4");
		Assert.AreEqual ("This_Is\uFE58A\u0095String\u06D4With\uFE33Separators", ti.ToTitleCase ("this_is\uFE58a\u0095string\u06D4with\uFE33separators"), "#5");
	}

	[Test]
	public void ListSeparator ()
	{
		TextInfo ti;

		ti = new CultureInfo ("en-US", false).TextInfo;
		Assert.AreEqual (",", ti.ListSeparator, "#1");

		ti = CultureInfo.InvariantCulture.TextInfo;
		Assert.AreEqual (",", ti.ListSeparator, "#2");

		ti = new CultureInfo ("nl-BE", false).TextInfo;
		Assert.AreEqual (";", ti.ListSeparator, "#3");
	}

	[Test]
	public void TitleCase2 ()
	{
		foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
			Check (ci, "AB", "AB");
			Check (ci, "ab", "Ab");
			Check (ci, "aB", "Ab");
			Check (ci, "1Ab", "1Ab");
			Check (ci, "abc AB ab aB Ab ABc 1AB 1Ab 1ab 1aB",
				"Abc AB Ab Ab Ab Abc 1AB 1Ab 1Ab 1Ab");
			Check (ci, "LJ", "LJ");
			Check (ci, "lj", "Lj");
			Check (ci, "lJ", "Lj");
			Check (ci, "lj abc ljabc", "Lj Abc Ljabc");
			Check (ci, "ab", "Ab");

			// Some greek titlecase characters
			Check (ci, "\u01c4", "\u01c5");
			Check (ci, "\u01c5", "\u01c5");
			Check (ci, "\u01c6", "\u01c5");
			Check (ci, "\u01ca", "\u01cb");
			Check (ci, "\u01cb", "\u01cb");
			Check (ci, "\u01cc", "\u01cb");
			// Roman numbers are not converted unlike ToUpper().
			Check (ci, "\u2170", "\u2170");
			Check (ci, "\u24e9", "\u24e9");
		}
	}

	private void Check (CultureInfo ci, string src, string expected)
	{
		Assert.AreEqual (expected, ci.TextInfo.ToTitleCase (src), src + " at culture " + ci.Name);
	}

	private void CompareProperties (TextInfo t1, TextInfo t2, bool compareReadOnly)
	{
		Assert.AreEqual (t1.ANSICodePage, t2.ANSICodePage, "ANSICodePage");
		Assert.AreEqual (t1.EBCDICCodePage, t2.EBCDICCodePage, "EBCDICCodePage");
		Assert.AreEqual (t1.ListSeparator, t2.ListSeparator, "ListSeparator");
		Assert.AreEqual (t1.MacCodePage, t2.MacCodePage, "MacCodePage");
		Assert.AreEqual (t1.OEMCodePage, t2.OEMCodePage, "OEMCodePage");
		Assert.AreEqual (t1.CultureName, t2.CultureName, "CultureName");
		if (compareReadOnly)
			Assert.AreEqual (t1.IsReadOnly, t2.IsReadOnly, "IsReadOnly");
//FIXME		Assert.AreEqual (t1.IsRightToLeft, t2.IsRightToLeft, "IsRightToLeft");
		Assert.AreEqual (t1.LCID, t2.LCID, "LCID");
	}

	[Test]
	[Category ("NotWorking")] // OnDeserialization isn't completed
	public void SerializationRoundtrip ()
	{
		TextInfo enus = new CultureInfo ("en-US").TextInfo;
		BinaryFormatter bf = new BinaryFormatter ();
		MemoryStream ms = new MemoryStream ();
		bf.Serialize (ms, enus);

		ms.Position = 0;
		TextInfo clone = (TextInfo) bf.Deserialize (ms);
		CompareProperties (enus, clone, true);
	}

	[Test]
	public void Clone ()
	{
		TextInfo enus = new CultureInfo ("en-US").TextInfo;
		TextInfo clone = (TextInfo) enus.Clone ();
		CompareProperties (enus, clone, true);
	}

	[Test]
	public void Clone_ReadOnly ()
	{
		TextInfo enus = TextInfo.ReadOnly (new CultureInfo ("en-US").TextInfo);
		Assert.IsTrue (enus.IsReadOnly, "IsReadOnly-1");
		TextInfo clone = (TextInfo) enus.Clone ();
		Assert.IsFalse (clone.IsReadOnly, "IsReadOnly-2");
		CompareProperties (enus, clone, false);
		// cloned item is *NOT* read-only
	}

	[Test]
	public void ReadOnly ()
	{
		TextInfo enus = new CultureInfo ("en-US").TextInfo;
		Assert.IsFalse (enus.IsReadOnly, "IsReadOnly-1");
		TextInfo ro = TextInfo.ReadOnly (enus);
		Assert.IsTrue (ro.IsReadOnly, "IsReadOnly-2");
		CompareProperties (enus, ro, false);
	}

	[Test]
	public void IsRightToLeft ()
	{
		foreach (CultureInfo ci in CultureInfo.GetCultures (CultureTypes.AllCultures)) {
			switch (ci.LCID) {
			case 1:		// ar
			case 13:	// he
			case 32:	// ur
			case 41:	// fa
			case 0x63:	// ps
			case 90:	// syr
			case 101:	// div
			case 128:	// ur
			case 1025:	// ar-SA
			case 1037:	// he-IL
			case 1056:	// ur-PK
			case 1065:	// ra-IR
			case 1114:	// syr-SY
			case 1125:	// div-MV
			case 1152:	// ug-CN
			case 2049:	// ar-IQ
			case 2080:	// ur-IN
			case 2118:	// pa-Arab-PK
			case 3073:	// ar-EG
			case 4097:	// ar-LY
			case 5121:	// ar-DZ
			case 6145:	// ar-MA
			case 7169:	// ar-TN
			case 8193:	// ar-OM
			case 9217:	// ar-YE
			case 10241:	// ar-SY
			case 11265:	// ar-JO
			case 12289:	// ar-LB
			case 13313:	// ar-KW
			case 14337:	// ar-AE
			case 15361:	// ar-BH
			case 16385:	// ar-QA
			case 31814:	// pa-Arab
			case 0x463: // ps-AF
				Assert.IsTrue (ci.TextInfo.IsRightToLeft, ci.Name);
				break;
			default:
				Assert.IsFalse (ci.TextInfo.IsRightToLeft, ci.Name);
				break;
			}
		}
	}

	[Test]
	public void Deserialization ()
	{
		TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
		IDeserializationCallback dc = (ti as IDeserializationCallback);
		dc.OnDeserialization (null);
	}
}

}
