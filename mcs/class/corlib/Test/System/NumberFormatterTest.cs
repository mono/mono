//
// MonoTests.System.NumberFormatterTest
//
// Authors: 
//	akiramei (mei@work.email.ne.jp)
//
// (C) 2005 akiramei
//

using System;
using System.Globalization;
using System.Threading;
using System.Text;

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class NumberFormatterTest 
	{
		CultureInfo old_culture;
		NumberFormatInfo _nfi;

		[SetUp]
		public void SetUp ()
		{
			old_culture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = new CultureInfo ("en-US", false);
			_nfi = NumberFormatInfo.InvariantInfo.Clone () as NumberFormatInfo;
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = old_culture;
		}

		// Test00000- Int32 and D
		[Test]
		public void Test00000 ()
		{
			Assert.AreEqual ("0", 0.ToString ("D", _nfi), "#01");
			Assert.AreEqual ("0", 0.ToString ("d", _nfi), "#02");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("D", _nfi), "#03");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("d", _nfi), "#04");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("D", _nfi), "#05");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("d", _nfi), "#06");
		}

		[Test]
		public void Test00001 ()
		{
			Assert.AreEqual ("D ", 0.ToString ("D ", _nfi), "#01");
			Assert.AreEqual (" D", 0.ToString (" D", _nfi), "#02");
			Assert.AreEqual (" D ", 0.ToString (" D ", _nfi), "#03");
		}

		[Test]
		public void Test00002 ()
		{
			Assert.AreEqual ("-D ", (-1).ToString ("D ", _nfi), "#01");
			Assert.AreEqual ("- D", (-1).ToString (" D", _nfi), "#02");
			Assert.AreEqual ("- D ", (-1).ToString (" D ", _nfi), "#03");
		}

		[Test]
		public void Test00003 ()
		{
			Assert.AreEqual ("0", 0.ToString ("D0", _nfi), "#01");
			Assert.AreEqual ("0000000000", 0.ToString ("D10", _nfi), "#02");
			Assert.AreEqual ("00000000000", 0.ToString ("D11", _nfi), "#03");
			Assert.AreEqual ("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("D99", _nfi), "#04");
			Assert.AreEqual ("D100", 0.ToString ("D100", _nfi), "#05");
		}

		[Test]
		public void Test00004 ()
		{
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("D0", _nfi), "#01");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("D10", _nfi), "#02");
			Assert.AreEqual ("02147483647", Int32.MaxValue.ToString ("D11", _nfi), "#03");
			Assert.AreEqual ("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002147483647", Int32.MaxValue.ToString ("D99", _nfi), "#04");
			Assert.AreEqual ("D12147483647", Int32.MaxValue.ToString ("D100", _nfi), "#05");
		}

		[Test]
		public void Test00005 ()
		{
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("D0", _nfi), "#01");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("D10", _nfi), "#02");
			Assert.AreEqual ("-02147483648", Int32.MinValue.ToString ("D11", _nfi), "#03");
			Assert.AreEqual ("-000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002147483648", Int32.MinValue.ToString ("D99", _nfi), "#04");
			Assert.AreEqual ("-D12147483648", Int32.MinValue.ToString ("D100", _nfi), "#05");
		}

		[Test]
		public void Test00006 ()
		{
			Assert.AreEqual ("DF", 0.ToString ("DF", _nfi), "#01");
			Assert.AreEqual ("D0F", 0.ToString ("D0F", _nfi), "#02");
			Assert.AreEqual ("D0xF", 0.ToString ("D0xF", _nfi), "#03");
		}

		[Test]
		public void Test00007 ()
		{
			Assert.AreEqual ("DF", Int32.MaxValue.ToString ("DF", _nfi), "#01");
			Assert.AreEqual ("D2147483647F", Int32.MaxValue.ToString ("D0F", _nfi), "#02");
			Assert.AreEqual ("D2147483647xF", Int32.MaxValue.ToString ("D0xF", _nfi), "#03");
		}

		[Test]
		public void Test00008 ()
		{
			Assert.AreEqual ("-DF", Int32.MinValue.ToString ("DF", _nfi), "#01");
			Assert.AreEqual ("-D2147483648F", Int32.MinValue.ToString ("D0F", _nfi), "#02");
			Assert.AreEqual ("-D2147483648xF", Int32.MinValue.ToString ("D0xF", _nfi), "#03");
		}

		[Test]
		public void Test00009 ()
		{
			Assert.AreEqual ("00000000000", 0.ToString ("D0000000000000000000000000000000000000011", _nfi), "#01");
			Assert.AreEqual ("02147483647", Int32.MaxValue.ToString ("D0000000000000000000000000000000000000011", _nfi), "#02");
			Assert.AreEqual ("-02147483648", Int32.MinValue.ToString ("D0000000000000000000000000000000000000011", _nfi), "#03");
		}

		[Test]
		public void Test00010 ()
		{
			Assert.AreEqual ("+D", 0.ToString ("+D", _nfi), "#01");
			Assert.AreEqual ("D+", 0.ToString ("D+", _nfi), "#02");
			Assert.AreEqual ("+D+", 0.ToString ("+D+", _nfi), "#03");
		}
		
		[Test]
		public void Test00011 ()
		{
			Assert.AreEqual ("+D", Int32.MaxValue.ToString ("+D", _nfi), "#01");
			Assert.AreEqual ("D+", Int32.MaxValue.ToString ("D+", _nfi), "#02");
			Assert.AreEqual ("+D+", Int32.MaxValue.ToString ("+D+", _nfi), "#03");
		}

		[Test]
		public void Test00012 ()
		{
			Assert.AreEqual ("-+D", Int32.MinValue.ToString ("+D", _nfi), "#01");
			Assert.AreEqual ("-D+", Int32.MinValue.ToString ("D+", _nfi), "#02");
			Assert.AreEqual ("-+D+", Int32.MinValue.ToString ("+D+", _nfi), "#03");
		}

		[Test]
		public void Test00013 ()
		{
			Assert.AreEqual ("-D", 0.ToString ("-D", _nfi), "#01");
			Assert.AreEqual ("D-", 0.ToString ("D-", _nfi), "#02");
			Assert.AreEqual ("-D-", 0.ToString ("-D-", _nfi), "#03");
		}
		
		[Test]
		public void Test00014 ()
		{
			Assert.AreEqual ("-D", Int32.MaxValue.ToString ("-D", _nfi), "#01");
			Assert.AreEqual ("D-", Int32.MaxValue.ToString ("D-", _nfi), "#02");
			Assert.AreEqual ("-D-", Int32.MaxValue.ToString ("-D-", _nfi), "#03");
		}

		[Test]
		public void Test00015 ()
		{
			Assert.AreEqual ("--D", Int32.MinValue.ToString ("-D", _nfi), "#01");
			Assert.AreEqual ("-D-", Int32.MinValue.ToString ("D-", _nfi), "#02");
			Assert.AreEqual ("--D-", Int32.MinValue.ToString ("-D-", _nfi), "#03");
		}

		[Test]
		public void Test00016 ()
		{
			Assert.AreEqual ("D+0", 0.ToString ("D+0", _nfi), "#01");
			Assert.AreEqual ("D+2147483647", Int32.MaxValue.ToString ("D+0", _nfi), "#02");
			Assert.AreEqual ("-D+2147483648", Int32.MinValue.ToString ("D+0", _nfi), "#03");
		}

		[Test]
		public void Test00017 ()
		{
			Assert.AreEqual ("D+9", 0.ToString ("D+9", _nfi), "#01");
			Assert.AreEqual ("D+9", Int32.MaxValue.ToString ("D+9", _nfi), "#02");
			Assert.AreEqual ("-D+9", Int32.MinValue.ToString ("D+9", _nfi), "#03");
		}

		[Test]
		public void Test00018 ()
		{
			Assert.AreEqual ("D-9", 0.ToString ("D-9", _nfi), "#01");
			Assert.AreEqual ("D-9", Int32.MaxValue.ToString ("D-9", _nfi), "#02");
			Assert.AreEqual ("-D-9", Int32.MinValue.ToString ("D-9", _nfi), "#03");
		}

		[Test]
		public void Test00019 ()
		{
			Assert.AreEqual ("D0", 0.ToString ("D0,", _nfi), "#01");
			Assert.AreEqual ("D2147484", Int32.MaxValue.ToString ("D0,", _nfi), "#02");
			Assert.AreEqual ("-D2147484", Int32.MinValue.ToString ("D0,", _nfi), "#03");
		}

		[Test]
		public void Test00020 ()
		{
			Assert.AreEqual ("D0", 0.ToString ("D0.", _nfi), "#01");
			Assert.AreEqual ("D2147483647", Int32.MaxValue.ToString ("D0.", _nfi), "#02");
			Assert.AreEqual ("-D2147483648", Int32.MinValue.ToString ("D0.", _nfi), "#03");
		}

		[Test]
		public void Test00021 ()
		{
			Assert.AreEqual ("D0.0", 0.ToString ("D0.0", _nfi), "#01");
			Assert.AreEqual ("D2147483647.0", Int32.MaxValue.ToString ("D0.0", _nfi), "#02");
			Assert.AreEqual ("-D2147483648.0", Int32.MinValue.ToString ("D0.0", _nfi), "#03");
		}

		[Test]
		public void Test00022 ()
		{
			Assert.AreEqual ("D09", 0.ToString ("D0.9", _nfi), "#01");
			Assert.AreEqual ("D21474836479", Int32.MaxValue.ToString ("D0.9", _nfi), "#02");
			Assert.AreEqual ("-D21474836489", Int32.MinValue.ToString ("D0.9", _nfi), "#03");
		}

		// Test01000- Int32 and E
		[Test]
		public void Test01000 ()
		{
			Assert.AreEqual ("0.000000E+000", 0.ToString ("E", _nfi), "#01");
			Assert.AreEqual ("0.000000e+000", 0.ToString ("e", _nfi), "#02");
			Assert.AreEqual ("-2.147484E+009", Int32.MinValue.ToString ("E", _nfi), "#03");
			Assert.AreEqual ("-2.147484e+009", Int32.MinValue.ToString ("e", _nfi), "#04");
			Assert.AreEqual ("2.147484E+009", Int32.MaxValue.ToString ("E", _nfi), "#05");
			Assert.AreEqual ("2.147484e+009", Int32.MaxValue.ToString ("e", _nfi), "#06");
		}

		[Test]
		public void Test01001 ()
		{
			Assert.AreEqual ("E ", 0.ToString ("E ", _nfi), "#01");
			Assert.AreEqual (" E", 0.ToString (" E", _nfi), "#02");
			Assert.AreEqual (" E ", 0.ToString (" E ", _nfi), "#03");
		}

		[Test]
		public void Test01002 ()
		{
			Assert.AreEqual ("-E ", (-1).ToString ("E ", _nfi), "#01");
			Assert.AreEqual ("- E", (-1).ToString (" E", _nfi), "#02");
			Assert.AreEqual ("- E ", (-1).ToString (" E ", _nfi), "#03");
		}

		[Test]
		public void Test01003 ()
		{
			Assert.AreEqual ("0E+000", 0.ToString ("E0", _nfi), "#01");
			Assert.AreEqual ("0.000000000E+000", 0.ToString ("E9", _nfi), "#02");
			Assert.AreEqual ("0.0000000000E+000", 0.ToString ("E10", _nfi), "#03");
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000E+000", 0.ToString ("E99", _nfi), "#04");
			Assert.AreEqual ("E100", 0.ToString ("E100", _nfi), "#05");
		}

		[Test]
		public void Test01004 ()
		{
			Assert.AreEqual ("2E+009", Int32.MaxValue.ToString ("E0", _nfi), "#01");
			Assert.AreEqual ("2.147483647E+009", Int32.MaxValue.ToString ("E9", _nfi), "#02");
			Assert.AreEqual ("2.1474836470E+009", Int32.MaxValue.ToString ("E10", _nfi), "#03");
			Assert.AreEqual ("2.147483647000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000E+009", Int32.MaxValue.ToString ("E99", _nfi), "#04");
			Assert.AreEqual ("E12147483647", Int32.MaxValue.ToString ("E100", _nfi), "#05");
		}

		[Test]
		public void Test01005 ()
		{
			Assert.AreEqual ("-2E+009", Int32.MinValue.ToString ("E0", _nfi), "#01");
			Assert.AreEqual ("-2.147483648E+009", Int32.MinValue.ToString ("E9", _nfi), "#02");
			Assert.AreEqual ("-2.1474836480E+009", Int32.MinValue.ToString ("E10", _nfi), "#03");
			Assert.AreEqual ("-2.147483648000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000E+009", Int32.MinValue.ToString ("E99", _nfi), "#04");
			Assert.AreEqual ("-E12147483648", Int32.MinValue.ToString ("E100", _nfi), "#05");
		}

		[Test]
		public void Test01006 ()
		{
			Assert.AreEqual ("EF", 0.ToString ("EF", _nfi), "#01");
			Assert.AreEqual ("E0F", 0.ToString ("E0F", _nfi), "#02");
			Assert.AreEqual ("E0xF", 0.ToString ("E0xF", _nfi), "#03");
		}

		[Test]
		public void Test01007 ()
		{
			Assert.AreEqual ("EF", Int32.MaxValue.ToString ("EF", _nfi), "#01");
			Assert.AreEqual ("E0F", Int32.MaxValue.ToString ("E0F", _nfi), "#02");
			Assert.AreEqual ("E0xF", Int32.MaxValue.ToString ("E0xF", _nfi), "#03");
		}

		[Test]
		public void Test01008 ()
		{
			Assert.AreEqual ("-EF", Int32.MinValue.ToString ("EF", _nfi), "#01");
			Assert.AreEqual ("E0F", Int32.MinValue.ToString ("E0F", _nfi), "#02");
			Assert.AreEqual ("E0xF", Int32.MinValue.ToString ("E0xF", _nfi), "#03");
		}

		[Test]
		public void Test01009 ()
		{
			Assert.AreEqual ("0.0000000000E+000", 0.ToString ("E0000000000000000000000000000000000000010", _nfi), "#01");
			Assert.AreEqual ("2.1474836470E+009", Int32.MaxValue.ToString ("E0000000000000000000000000000000000000010", _nfi), "#02");
			Assert.AreEqual ("-2.1474836480E+009", Int32.MinValue.ToString ("E0000000000000000000000000000000000000010", _nfi), "#03");
		}

		[Test]
		public void Test01010 ()
		{
			Assert.AreEqual ("+E", 0.ToString ("+E", _nfi), "#01");
			Assert.AreEqual ("E+", 0.ToString ("E+", _nfi), "#02");
			Assert.AreEqual ("+E+", 0.ToString ("+E+", _nfi), "#03");
		}
		
		[Test]
		public void Test01011 ()
		{
			Assert.AreEqual ("+E", Int32.MaxValue.ToString ("+E", _nfi), "#01");
			Assert.AreEqual ("E+", Int32.MaxValue.ToString ("E+", _nfi), "#02");
			Assert.AreEqual ("+E+", Int32.MaxValue.ToString ("+E+", _nfi), "#03");
		}

		[Test]
		public void Test01012 ()
		{
			Assert.AreEqual ("-+E", Int32.MinValue.ToString ("+E", _nfi), "#01");
			Assert.AreEqual ("-E+", Int32.MinValue.ToString ("E+", _nfi), "#02");
			Assert.AreEqual ("-+E+", Int32.MinValue.ToString ("+E+", _nfi), "#03");
		}

		[Test]
		public void Test01013 ()
		{
			Assert.AreEqual ("-E", 0.ToString ("-E", _nfi), "#01");
			Assert.AreEqual ("E-", 0.ToString ("E-", _nfi), "#02");
			Assert.AreEqual ("-E-", 0.ToString ("-E-", _nfi), "#03");
		}
		
		[Test]
		public void Test01014 ()
		{
			Assert.AreEqual ("-E", Int32.MaxValue.ToString ("-E", _nfi), "#01");
			Assert.AreEqual ("E-", Int32.MaxValue.ToString ("E-", _nfi), "#02");
			Assert.AreEqual ("-E-", Int32.MaxValue.ToString ("-E-", _nfi), "#03");
		}

		[Test]
		public void Test01015 ()
		{
			Assert.AreEqual ("--E", Int32.MinValue.ToString ("-E", _nfi), "#01");
			Assert.AreEqual ("-E-", Int32.MinValue.ToString ("E-", _nfi), "#02");
			Assert.AreEqual ("--E-", Int32.MinValue.ToString ("-E-", _nfi), "#03");
		}

		[Test]
		public void Test01016 ()
		{
			Assert.AreEqual ("E+0", 0.ToString ("E+0", _nfi), "#01");
			Assert.AreEqual ("E+0", Int32.MaxValue.ToString ("E+0", _nfi), "#02");
			Assert.AreEqual ("E+0", Int32.MinValue.ToString ("E+0", _nfi), "#03");
		}

		[Test]
		public void Test01017 ()
		{
			Assert.AreEqual ("E+9", 0.ToString ("E+9", _nfi), "#01");
			Assert.AreEqual ("E+9", Int32.MaxValue.ToString ("E+9", _nfi), "#02");
			Assert.AreEqual ("-E+9", Int32.MinValue.ToString ("E+9", _nfi), "#03");
		}

		[Test]
		public void Test01018 ()
		{
			Assert.AreEqual ("E-9", 0.ToString ("E-9", _nfi), "#01");
			Assert.AreEqual ("E-9", Int32.MaxValue.ToString ("E-9", _nfi), "#02");
			Assert.AreEqual ("-E-9", Int32.MinValue.ToString ("E-9", _nfi), "#03");
		}

		[Test]
		public void Test01019 ()
		{
			Assert.AreEqual ("E0", 0.ToString ("E0,", _nfi), "#01");
			Assert.AreEqual ("E0", Int32.MaxValue.ToString ("E0,", _nfi), "#02");
			Assert.AreEqual ("E0", Int32.MinValue.ToString ("E0,", _nfi), "#03");
		}

		[Test]
		public void Test01020 ()
		{
			Assert.AreEqual ("E0", 0.ToString ("E0.", _nfi), "#01");
			Assert.AreEqual ("E0", Int32.MaxValue.ToString ("E0.", _nfi), "#02");
			Assert.AreEqual ("E0", Int32.MinValue.ToString ("E0.", _nfi), "#03");
		}

		[Test]
		public void Test01021 ()
		{
			Assert.AreEqual ("E0.0", 0.ToString ("E0.0", _nfi), "#01");
			Assert.AreEqual ("E10.2", Int32.MaxValue.ToString ("E0.0", _nfi), "#02");
			Assert.AreEqual ("-E10.2", Int32.MinValue.ToString ("E0.0", _nfi), "#03");
		}

		[Test]
		public void Test01022 ()
		{
			Assert.AreEqual ("E09", 0.ToString ("E0.9", _nfi), "#01");
			Assert.AreEqual ("E09", Int32.MaxValue.ToString ("E0.9", _nfi), "#02");
			Assert.AreEqual ("E09", Int32.MinValue.ToString ("E0.9", _nfi), "#03");
		}

		[Test]
		public void Test01023 ()
		{
			Assert.AreEqual ("9.999999E+007", 99999990.ToString ("E", _nfi), "#01");
			Assert.AreEqual ("9.999999E+007", 99999991.ToString ("E", _nfi), "#02");
			Assert.AreEqual ("9.999999E+007", 99999992.ToString ("E", _nfi), "#03");
			Assert.AreEqual ("9.999999E+007", 99999993.ToString ("E", _nfi), "#04");
			Assert.AreEqual ("9.999999E+007", 99999994.ToString ("E", _nfi), "#05");
			Assert.AreEqual ("1.000000E+008", 99999995.ToString ("E", _nfi), "#06");
			Assert.AreEqual ("1.000000E+008", 99999996.ToString ("E", _nfi), "#07");
			Assert.AreEqual ("1.000000E+008", 99999997.ToString ("E", _nfi), "#08");
			Assert.AreEqual ("1.000000E+008", 99999998.ToString ("E", _nfi), "#09");
			Assert.AreEqual ("1.000000E+008", 99999999.ToString ("E", _nfi), "#10");
		}

		[Test]
		public void Test01024 ()
		{
			Assert.AreEqual ("-9.999999E+007", (-99999990).ToString ("E", _nfi), "#01");
			Assert.AreEqual ("-9.999999E+007", (-99999991).ToString ("E", _nfi), "#02");
			Assert.AreEqual ("-9.999999E+007", (-99999992).ToString ("E", _nfi), "#03");
			Assert.AreEqual ("-9.999999E+007", (-99999993).ToString ("E", _nfi), "#04");
			Assert.AreEqual ("-9.999999E+007", (-99999994).ToString ("E", _nfi), "#05");
			Assert.AreEqual ("-1.000000E+008", (-99999995).ToString ("E", _nfi), "#06");
			Assert.AreEqual ("-1.000000E+008", (-99999996).ToString ("E", _nfi), "#07");
			Assert.AreEqual ("-1.000000E+008", (-99999997).ToString ("E", _nfi), "#08");
			Assert.AreEqual ("-1.000000E+008", (-99999998).ToString ("E", _nfi), "#09");
			Assert.AreEqual ("-1.000000E+008", (-99999999).ToString ("E", _nfi), "#10");
		}

		[Test]
		public void Test01025 ()
		{
			Assert.AreEqual ("9.999998E+007", 99999980.ToString ("E", _nfi), "#01");
			Assert.AreEqual ("9.999998E+007", 99999981.ToString ("E", _nfi), "#02");
			Assert.AreEqual ("9.999998E+007", 99999982.ToString ("E", _nfi), "#03");
			Assert.AreEqual ("9.999998E+007", 99999983.ToString ("E", _nfi), "#04");
			Assert.AreEqual ("9.999998E+007", 99999984.ToString ("E", _nfi), "#05");
			Assert.AreEqual ("9.999999E+007", 99999985.ToString ("E", _nfi), "#06");
			Assert.AreEqual ("9.999999E+007", 99999986.ToString ("E", _nfi), "#07");
			Assert.AreEqual ("9.999999E+007", 99999987.ToString ("E", _nfi), "#08");
			Assert.AreEqual ("9.999999E+007", 99999988.ToString ("E", _nfi), "#09");
			Assert.AreEqual ("9.999999E+007", 99999989.ToString ("E", _nfi), "#10");
		}

		[Test]
		public void Test01026 ()
		{
			Assert.AreEqual ("-9.999998E+007", (-99999980).ToString ("E", _nfi), "#01");
			Assert.AreEqual ("-9.999998E+007", (-99999981).ToString ("E", _nfi), "#02");
			Assert.AreEqual ("-9.999998E+007", (-99999982).ToString ("E", _nfi), "#03");
			Assert.AreEqual ("-9.999998E+007", (-99999983).ToString ("E", _nfi), "#04");
			Assert.AreEqual ("-9.999998E+007", (-99999984).ToString ("E", _nfi), "#05");
			Assert.AreEqual ("-9.999999E+007", (-99999985).ToString ("E", _nfi), "#06");
			Assert.AreEqual ("-9.999999E+007", (-99999986).ToString ("E", _nfi), "#07");
			Assert.AreEqual ("-9.999999E+007", (-99999987).ToString ("E", _nfi), "#08");
			Assert.AreEqual ("-9.999999E+007", (-99999988).ToString ("E", _nfi), "#09");
			Assert.AreEqual ("-9.999999E+007", (-99999989).ToString ("E", _nfi), "#10");
		}

		[Test]
		public void Test01027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			Assert.AreEqual ("-1#000000E+008", (-99999999).ToString ("E", nfi), "#01");
		}

		[Test]
		public void Test01028 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			Assert.AreEqual ("1.000000E-000", 1.ToString ("E", nfi), "#01");
			Assert.AreEqual ("0.000000E-000", 0.ToString ("E", nfi), "#02");
			Assert.AreEqual ("+1.000000E-000", (-1).ToString ("E", nfi), "#03");
		}

		// Test02000- Int32 and F
		[Test]
		public void Test02000 ()
		{
			Assert.AreEqual ("0.00", 0.ToString ("F", _nfi), "#01");
			Assert.AreEqual ("0.00", 0.ToString ("f", _nfi), "#02");
			Assert.AreEqual ("-2147483648.00", Int32.MinValue.ToString ("F", _nfi), "#03");
			Assert.AreEqual ("-2147483648.00", Int32.MinValue.ToString ("f", _nfi), "#04");
			Assert.AreEqual ("2147483647.00", Int32.MaxValue.ToString ("F", _nfi), "#05");
			Assert.AreEqual ("2147483647.00", Int32.MaxValue.ToString ("f", _nfi), "#06");
		}

		[Test]
		public void Test02001 ()
		{
			Assert.AreEqual ("F ", 0.ToString ("F ", _nfi), "#01");
			Assert.AreEqual (" F", 0.ToString (" F", _nfi), "#02");
			Assert.AreEqual (" F ", 0.ToString (" F ", _nfi), "#03");
		}

		[Test]
		public void Test02002 ()
		{
			Assert.AreEqual ("-F ", (-1).ToString ("F ", _nfi), "#01");
			Assert.AreEqual ("- F", (-1).ToString (" F", _nfi), "#02");
			Assert.AreEqual ("- F ", (-1).ToString (" F ", _nfi), "#03");
		}

		[Test]
		public void Test02003 ()
		{
			Assert.AreEqual ("0", 0.ToString ("F0", _nfi), "#01");
			Assert.AreEqual ("0.000000000", 0.ToString ("F9", _nfi), "#02");
			Assert.AreEqual ("0.0000000000", 0.ToString ("F10", _nfi), "#03");
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("F99", _nfi), "#04");
			Assert.AreEqual ("F100", 0.ToString ("F100", _nfi), "#05");
		}

		[Test]
		public void Test02004 ()
		{
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("F0", _nfi), "#01");
			Assert.AreEqual ("2147483647.000000000", Int32.MaxValue.ToString ("F9", _nfi), "#02");
			Assert.AreEqual ("2147483647.0000000000", Int32.MaxValue.ToString ("F10", _nfi), "#03");
			Assert.AreEqual ("2147483647.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Int32.MaxValue.ToString ("F99", _nfi), "#04");
			Assert.AreEqual ("F12147483647", Int32.MaxValue.ToString ("F100", _nfi), "#05");
		}

		[Test]
		public void Test02005 ()
		{
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("F0", _nfi), "#01");
			Assert.AreEqual ("-2147483648.000000000", Int32.MinValue.ToString ("F9", _nfi), "#02");
			Assert.AreEqual ("-2147483648.0000000000", Int32.MinValue.ToString ("F10", _nfi), "#03");
			Assert.AreEqual ("-2147483648.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Int32.MinValue.ToString ("F99", _nfi), "#04");
			Assert.AreEqual ("-F12147483648", Int32.MinValue.ToString ("F100", _nfi), "#05");
		}

		[Test]
		public void Test02006 ()
		{
			Assert.AreEqual ("FF", 0.ToString ("FF", _nfi), "#01");
			Assert.AreEqual ("F0F", 0.ToString ("F0F", _nfi), "#02");
			Assert.AreEqual ("F0xF", 0.ToString ("F0xF", _nfi), "#03");
		}

		[Test]
		public void Test02007 ()
		{
			Assert.AreEqual ("FF", Int32.MaxValue.ToString ("FF", _nfi), "#01");
			Assert.AreEqual ("F2147483647F", Int32.MaxValue.ToString ("F0F", _nfi), "#02");
			Assert.AreEqual ("F2147483647xF", Int32.MaxValue.ToString ("F0xF", _nfi), "#03");
		}

		[Test]
		public void Test02008 ()
		{
			Assert.AreEqual ("-FF", Int32.MinValue.ToString ("FF", _nfi), "#01");
			Assert.AreEqual ("-F2147483648F", Int32.MinValue.ToString ("F0F", _nfi), "#02");
			Assert.AreEqual ("-F2147483648xF", Int32.MinValue.ToString ("F0xF", _nfi), "#03");
		}

		[Test]
		public void Test02009 ()
		{
			Assert.AreEqual ("0.0000000000", 0.ToString ("F0000000000000000000000000000000000000010", _nfi), "#01");
			Assert.AreEqual ("2147483647.0000000000", Int32.MaxValue.ToString ("F0000000000000000000000000000000000000010", _nfi), "#02");
			Assert.AreEqual ("-2147483648.0000000000", Int32.MinValue.ToString ("F0000000000000000000000000000000000000010", _nfi), "#03");
		}

		[Test]
		public void Test02010 ()
		{
			Assert.AreEqual ("+F", 0.ToString ("+F", _nfi), "#01");
			Assert.AreEqual ("F+", 0.ToString ("F+", _nfi), "#02");
			Assert.AreEqual ("+F+", 0.ToString ("+F+", _nfi), "#03");
		}
		
		[Test]
		public void Test02011 ()
		{
			Assert.AreEqual ("+F", Int32.MaxValue.ToString ("+F", _nfi), "#01");
			Assert.AreEqual ("F+", Int32.MaxValue.ToString ("F+", _nfi), "#02");
			Assert.AreEqual ("+F+", Int32.MaxValue.ToString ("+F+", _nfi), "#03");
		}

		[Test]
		public void Test02012 ()
		{
			Assert.AreEqual ("-+F", Int32.MinValue.ToString ("+F", _nfi), "#01");
			Assert.AreEqual ("-F+", Int32.MinValue.ToString ("F+", _nfi), "#02");
			Assert.AreEqual ("-+F+", Int32.MinValue.ToString ("+F+", _nfi), "#03");
		}

		[Test]
		public void Test02013 ()
		{
			Assert.AreEqual ("-F", 0.ToString ("-F", _nfi), "#01");
			Assert.AreEqual ("F-", 0.ToString ("F-", _nfi), "#02");
			Assert.AreEqual ("-F-", 0.ToString ("-F-", _nfi), "#03");
		}
		
		[Test]
		public void Test02014 ()
		{
			Assert.AreEqual ("-F", Int32.MaxValue.ToString ("-F", _nfi), "#01");
			Assert.AreEqual ("F-", Int32.MaxValue.ToString ("F-", _nfi), "#02");
			Assert.AreEqual ("-F-", Int32.MaxValue.ToString ("-F-", _nfi), "#03");
		}

		[Test]
		public void Test02015 ()
		{
			Assert.AreEqual ("--F", Int32.MinValue.ToString ("-F", _nfi), "#01");
			Assert.AreEqual ("-F-", Int32.MinValue.ToString ("F-", _nfi), "#02");
			Assert.AreEqual ("--F-", Int32.MinValue.ToString ("-F-", _nfi), "#03");
		}

		[Test]
		public void Test02016 ()
		{
			Assert.AreEqual ("F+0", 0.ToString ("F+0", _nfi), "#01");
			Assert.AreEqual ("F+2147483647", Int32.MaxValue.ToString ("F+0", _nfi), "#02");
			Assert.AreEqual ("-F+2147483648", Int32.MinValue.ToString ("F+0", _nfi), "#03");
		}

		[Test]
		public void Test02017 ()
		{
			Assert.AreEqual ("F+9", 0.ToString ("F+9", _nfi), "#01");
			Assert.AreEqual ("F+9", Int32.MaxValue.ToString ("F+9", _nfi), "#02");
			Assert.AreEqual ("-F+9", Int32.MinValue.ToString ("F+9", _nfi), "#03");
		}

		[Test]
		public void Test02018 ()
		{
			Assert.AreEqual ("F-9", 0.ToString ("F-9", _nfi), "#01");
			Assert.AreEqual ("F-9", Int32.MaxValue.ToString ("F-9", _nfi), "#02");
			Assert.AreEqual ("-F-9", Int32.MinValue.ToString ("F-9", _nfi), "#03");
		}

		[Test]
		public void Test02019 ()
		{
			Assert.AreEqual ("F0", 0.ToString ("F0,", _nfi), "#01");
			Assert.AreEqual ("F2147484", Int32.MaxValue.ToString ("F0,", _nfi), "#02");
			Assert.AreEqual ("-F2147484", Int32.MinValue.ToString ("F0,", _nfi), "#03");
		}

		[Test]
		public void Test02020 ()
		{
			Assert.AreEqual ("F0", 0.ToString ("F0.", _nfi), "#01");
			Assert.AreEqual ("F2147483647", Int32.MaxValue.ToString ("F0.", _nfi), "#02");
			Assert.AreEqual ("-F2147483648", Int32.MinValue.ToString ("F0.", _nfi), "#03");
		}

		[Test]
		public void Test02021 ()
		{
			Assert.AreEqual ("F0.0", 0.ToString ("F0.0", _nfi), "#01");
			Assert.AreEqual ("F2147483647.0", Int32.MaxValue.ToString ("F0.0", _nfi), "#02");
			Assert.AreEqual ("-F2147483648.0", Int32.MinValue.ToString ("F0.0", _nfi), "#03");
		}

		[Test]
		public void Test02022 ()
		{
			Assert.AreEqual ("F09", 0.ToString ("F0.9", _nfi), "#01");
			Assert.AreEqual ("F21474836479", Int32.MaxValue.ToString ("F0.9", _nfi), "#02");
			Assert.AreEqual ("-F21474836489", Int32.MinValue.ToString ("F0.9", _nfi), "#03");
		}

		[Test]
		public void Test02023 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalDigits = 0;
			Assert.AreEqual ("0", 0.ToString ("F", nfi), "#01");
			nfi.NumberDecimalDigits = 1;
			Assert.AreEqual ("0.0", 0.ToString ("F", nfi), "#02");
			nfi.NumberDecimalDigits = 99;
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("F", nfi), "#03");
		}

		[Test]
		public void Test02024 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "";
			Assert.AreEqual ("2147483648.00", Int32.MinValue.ToString ("F", nfi), "#01");
			nfi.NegativeSign = "-";
			Assert.AreEqual ("-2147483648.00", Int32.MinValue.ToString ("F", nfi), "#02");
			nfi.NegativeSign = "+";
			Assert.AreEqual ("+2147483648.00", Int32.MinValue.ToString ("F", nfi), "#03");
			nfi.NegativeSign = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Assert.AreEqual ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ2147483648.00", Int32.MinValue.ToString ("F", nfi), "#04");
		}

		[Test]
		public void Test02025 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "-";
			nfi.PositiveSign = "+";
			Assert.AreEqual ("-1.00", (-1).ToString ("F", nfi), "#01");
			Assert.AreEqual ("0.00", 0.ToString ("F", nfi), "#02");
			Assert.AreEqual ("1.00",1.ToString ("F", nfi), "#03");
		}

		[Test]
		public void Test02026 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";
			Assert.AreEqual ("+1.00", (-1).ToString ("F", nfi), "#01");
			Assert.AreEqual ("0.00", 0.ToString ("F", nfi), "#02");
			Assert.AreEqual ("1.00",1.ToString ("F", nfi), "#03");
		}

		[Test]
		public void Test02027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			Assert.AreEqual ("1#00",1.ToString ("F", nfi), "#01");
		}

		// Test03000 - Int32 and G
		[Test]
		public void Test03000 ()
		{
			Assert.AreEqual ("0", 0.ToString ("G", _nfi), "#01");
			Assert.AreEqual ("0", 0.ToString ("g", _nfi), "#02");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("G", _nfi), "#03");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("g", _nfi), "#04");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("G", _nfi), "#05");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("g", _nfi), "#06");
		}

		[Test]
		public void Test03001 ()
		{
			Assert.AreEqual ("G ", 0.ToString ("G ", _nfi), "#01");
			Assert.AreEqual (" G", 0.ToString (" G", _nfi), "#02");
			Assert.AreEqual (" G ", 0.ToString (" G ", _nfi), "#03");
		}

		[Test]
		public void Test03002 ()
		{
			Assert.AreEqual ("-G ", (-1).ToString ("G ", _nfi), "#01");
			Assert.AreEqual ("- G", (-1).ToString (" G", _nfi), "#02");
			Assert.AreEqual ("- G ", (-1).ToString (" G ", _nfi), "#03");
		}

		[Test]
		public void Test03003 ()
		{
			Assert.AreEqual ("0", 0.ToString ("G0", _nfi), "#01");
			Assert.AreEqual ("0", 0.ToString ("G9", _nfi), "#02");
			Assert.AreEqual ("0", 0.ToString ("G10", _nfi), "#03");
			Assert.AreEqual ("0", 0.ToString ("G99", _nfi), "#04");
			Assert.AreEqual ("G100", 0.ToString ("G100", _nfi), "#05");
		}

		[Test]
		public void Test03004 ()
		{
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("G0", _nfi), "#01");
			Assert.AreEqual ("2.14748365E+09", Int32.MaxValue.ToString ("G9", _nfi), "#02");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("G10", _nfi), "#03");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("G99", _nfi), "#04");
			Assert.AreEqual ("G12147483647", Int32.MaxValue.ToString ("G100", _nfi), "#05");
		}

		[Test]
		public void Test03005 ()
		{
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("G0", _nfi), "#01");
			Assert.AreEqual ("-2.14748365E+09", Int32.MinValue.ToString ("G9", _nfi), "#02");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("G10", _nfi), "#03");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("G99", _nfi), "#04");
			Assert.AreEqual ("-G12147483648", Int32.MinValue.ToString ("G100", _nfi), "#05");
		}

		[Test]
		public void Test03006 ()
		{
			Assert.AreEqual ("GF", 0.ToString ("GF", _nfi), "#01");
			Assert.AreEqual ("G0F", 0.ToString ("G0F", _nfi), "#02");
			Assert.AreEqual ("G0xF", 0.ToString ("G0xF", _nfi), "#03");
		}

		[Test]
		public void Test03007 ()
		{
			Assert.AreEqual ("GF", Int32.MaxValue.ToString ("GF", _nfi), "#01");
			Assert.AreEqual ("G2147483647F", Int32.MaxValue.ToString ("G0F", _nfi), "#02");
			Assert.AreEqual ("G2147483647xF", Int32.MaxValue.ToString ("G0xF", _nfi), "#03");
		}

		[Test]
		public void Test03008 ()
		{
			Assert.AreEqual ("-GF", Int32.MinValue.ToString ("GF", _nfi), "#01");
			Assert.AreEqual ("-G2147483648F", Int32.MinValue.ToString ("G0F", _nfi), "#02");
			Assert.AreEqual ("-G2147483648xF", Int32.MinValue.ToString ("G0xF", _nfi), "#03");
		}

		[Test]
		public void Test03009 ()
		{
			Assert.AreEqual ("0", 0.ToString ("G0000000000000000000000000000000000000010", _nfi), "#01");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("G0000000000000000000000000000000000000010", _nfi), "#02");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("G0000000000000000000000000000000000000010", _nfi), "#03");
		}

		[Test]
		public void Test03010 ()
		{
			Assert.AreEqual ("+G", 0.ToString ("+G", _nfi), "#01");
			Assert.AreEqual ("G+", 0.ToString ("G+", _nfi), "#02");
			Assert.AreEqual ("+G+", 0.ToString ("+G+", _nfi), "#03");
		}
		
		[Test]
		public void Test03011 ()
		{
			Assert.AreEqual ("+G", Int32.MaxValue.ToString ("+G", _nfi), "#01");
			Assert.AreEqual ("G+", Int32.MaxValue.ToString ("G+", _nfi), "#02");
			Assert.AreEqual ("+G+", Int32.MaxValue.ToString ("+G+", _nfi), "#03");
		}

		[Test]
		public void Test03012 ()
		{
			Assert.AreEqual ("-+G", Int32.MinValue.ToString ("+G", _nfi), "#01");
			Assert.AreEqual ("-G+", Int32.MinValue.ToString ("G+", _nfi), "#02");
			Assert.AreEqual ("-+G+", Int32.MinValue.ToString ("+G+", _nfi), "#03");
		}

		[Test]
		public void Test03013 ()
		{
			Assert.AreEqual ("-G", 0.ToString ("-G", _nfi), "#01");
			Assert.AreEqual ("G-", 0.ToString ("G-", _nfi), "#02");
			Assert.AreEqual ("-G-", 0.ToString ("-G-", _nfi), "#03");
		}
		
		[Test]
		public void Test03014 ()
		{
			Assert.AreEqual ("-G", Int32.MaxValue.ToString ("-G", _nfi), "#01");
			Assert.AreEqual ("G-", Int32.MaxValue.ToString ("G-", _nfi), "#02");
			Assert.AreEqual ("-G-", Int32.MaxValue.ToString ("-G-", _nfi), "#03");
		}

		[Test]
		public void Test03015 ()
		{
			Assert.AreEqual ("--G", Int32.MinValue.ToString ("-G", _nfi), "#01");
			Assert.AreEqual ("-G-", Int32.MinValue.ToString ("G-", _nfi), "#02");
			Assert.AreEqual ("--G-", Int32.MinValue.ToString ("-G-", _nfi), "#03");
		}

		[Test]
		public void Test03016 ()
		{
			Assert.AreEqual ("G+0", 0.ToString ("G+0", _nfi), "#01");
			Assert.AreEqual ("G+2147483647", Int32.MaxValue.ToString ("G+0", _nfi), "#02");
			Assert.AreEqual ("-G+2147483648", Int32.MinValue.ToString ("G+0", _nfi), "#03");
		}

		[Test]
		public void Test03017 ()
		{
			Assert.AreEqual ("G+9", 0.ToString ("G+9", _nfi), "#01");
			Assert.AreEqual ("G+9", Int32.MaxValue.ToString ("G+9", _nfi), "#02");
			Assert.AreEqual ("-G+9", Int32.MinValue.ToString ("G+9", _nfi), "#03");
		}

		[Test]
		public void Test03018 ()
		{
			Assert.AreEqual ("G-9", 0.ToString ("G-9", _nfi), "#01");
			Assert.AreEqual ("G-9", Int32.MaxValue.ToString ("G-9", _nfi), "#02");
			Assert.AreEqual ("-G-9", Int32.MinValue.ToString ("G-9", _nfi), "#03");
		}

		[Test]
		public void Test03019 ()
		{
			Assert.AreEqual ("G0", 0.ToString ("G0,", _nfi), "#01");
			Assert.AreEqual ("G2147484", Int32.MaxValue.ToString ("G0,", _nfi), "#02");
			Assert.AreEqual ("-G2147484", Int32.MinValue.ToString ("G0,", _nfi), "#03");
		}

		[Test]
		public void Test03020 ()
		{
			Assert.AreEqual ("G0", 0.ToString ("G0.", _nfi), "#01");
			Assert.AreEqual ("G2147483647", Int32.MaxValue.ToString ("G0.", _nfi), "#02");
			Assert.AreEqual ("-G2147483648", Int32.MinValue.ToString ("G0.", _nfi), "#03");
		}

		[Test]
		public void Test03021 ()
		{
			Assert.AreEqual ("G0.0", 0.ToString ("G0.0", _nfi), "#01");
			Assert.AreEqual ("G2147483647.0", Int32.MaxValue.ToString ("G0.0", _nfi), "#02");
			Assert.AreEqual ("-G2147483648.0", Int32.MinValue.ToString ("G0.0", _nfi), "#03");
		}

		[Test]
		public void Test03022 ()
		{
			Assert.AreEqual ("G09", 0.ToString ("G0.9", _nfi), "#01");
			Assert.AreEqual ("G21474836479", Int32.MaxValue.ToString ("G0.9", _nfi), "#02");
			Assert.AreEqual ("-G21474836489", Int32.MinValue.ToString ("G0.9", _nfi), "#03");
		}

		[Test]
		public void Test03023 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalDigits = 0;
			Assert.AreEqual ("0", 0.ToString ("G", nfi), "#01");
			nfi.NumberDecimalDigits = 1;
			Assert.AreEqual ("0", 0.ToString ("G", nfi), "#02");
			nfi.NumberDecimalDigits = 99;
			Assert.AreEqual ("0", 0.ToString ("G", nfi), "#03");
		}

		[Test]
		public void Test03024 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "";
			Assert.AreEqual ("2147483648", Int32.MinValue.ToString ("G", nfi), "#01");
			nfi.NegativeSign = "-";
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("G", nfi), "#02");
			nfi.NegativeSign = "+";
			Assert.AreEqual ("+2147483648", Int32.MinValue.ToString ("G", nfi), "#03");
			nfi.NegativeSign = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Assert.AreEqual ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ2147483648", Int32.MinValue.ToString ("G", nfi), "#04");
		}

		[Test]
		public void Test03025 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "-";
			nfi.PositiveSign = "+";
			Assert.AreEqual ("-1", (-1).ToString ("G", nfi), "#01");
			Assert.AreEqual ("0", 0.ToString ("G", nfi), "#02");
			Assert.AreEqual ("1",1.ToString ("G", nfi), "#03");
		}

		[Test]
		public void Test03026 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";
			Assert.AreEqual ("+1", (-1).ToString ("G", nfi), "#01");
			Assert.AreEqual ("0", 0.ToString ("G", nfi), "#02");
			Assert.AreEqual ("1",1.ToString ("G", nfi), "#03");
		}

		[Test]
		public void Test03027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			Assert.AreEqual ("1#2E+02",123.ToString ("G2", nfi), "#01");
		}

		// Test04000 - Int32 and N
		[Test]
		public void Test04000 ()
		{
			Assert.AreEqual ("0.00", 0.ToString ("N", _nfi), "#01");
			Assert.AreEqual ("0.00", 0.ToString ("n", _nfi), "#02");
			Assert.AreEqual ("-2,147,483,648.00", Int32.MinValue.ToString ("N", _nfi), "#03");
			Assert.AreEqual ("-2,147,483,648.00", Int32.MinValue.ToString ("n", _nfi), "#04");
			Assert.AreEqual ("2,147,483,647.00", Int32.MaxValue.ToString ("N", _nfi), "#05");
			Assert.AreEqual ("2,147,483,647.00", Int32.MaxValue.ToString ("n", _nfi), "#06");
		}

		[Test]
		public void Test04001 ()
		{
			Assert.AreEqual ("N ", 0.ToString ("N ", _nfi), "#01");
			Assert.AreEqual (" N", 0.ToString (" N", _nfi), "#02");
			Assert.AreEqual (" N ", 0.ToString (" N ", _nfi), "#03");
		}

		[Test]
		public void Test04002 ()
		{
			Assert.AreEqual ("-N ", (-1).ToString ("N ", _nfi), "#01");
			Assert.AreEqual ("- N", (-1).ToString (" N", _nfi), "#02");
			Assert.AreEqual ("- N ", (-1).ToString (" N ", _nfi), "#03");
		}

		[Test]
		public void Test04003 ()
		{
			Assert.AreEqual ("0", 0.ToString ("N0", _nfi), "#01");
			Assert.AreEqual ("0.000000000", 0.ToString ("N9", _nfi), "#02");
			Assert.AreEqual ("0.0000000000", 0.ToString ("N10", _nfi), "#03");
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("N99", _nfi), "#04");
			Assert.AreEqual ("N100", 0.ToString ("N100", _nfi), "#05");
		}

		[Test]
		public void Test04004 ()
		{
			Assert.AreEqual ("2,147,483,647", Int32.MaxValue.ToString ("N0", _nfi), "#01");
			Assert.AreEqual ("2,147,483,647.000000000", Int32.MaxValue.ToString ("N9", _nfi), "#02");
			Assert.AreEqual ("2,147,483,647.0000000000", Int32.MaxValue.ToString ("N10", _nfi), "#03");
			Assert.AreEqual ("2,147,483,647.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Int32.MaxValue.ToString ("N99", _nfi), "#04");
			Assert.AreEqual ("N12147483647", Int32.MaxValue.ToString ("N100", _nfi), "#05");
		}

		[Test]
		public void Test04005 ()
		{
			Assert.AreEqual ("-2,147,483,648", Int32.MinValue.ToString ("N0", _nfi), "#01");
			Assert.AreEqual ("-2,147,483,648.000000000", Int32.MinValue.ToString ("N9", _nfi), "#02");
			Assert.AreEqual ("-2,147,483,648.0000000000", Int32.MinValue.ToString ("N10", _nfi), "#03");
			Assert.AreEqual ("-2,147,483,648.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Int32.MinValue.ToString ("N99", _nfi), "#04");
			Assert.AreEqual ("-N12147483648", Int32.MinValue.ToString ("N100", _nfi), "#05");
		}

		[Test]
		public void Test04006 ()
		{
			Assert.AreEqual ("NF", 0.ToString ("NF", _nfi), "#01");
			Assert.AreEqual ("N0F", 0.ToString ("N0F", _nfi), "#02");
			Assert.AreEqual ("N0xF", 0.ToString ("N0xF", _nfi), "#03");
		}

		[Test]
		public void Test04007 ()
		{
			Assert.AreEqual ("NF", Int32.MaxValue.ToString ("NF", _nfi), "#01");
			Assert.AreEqual ("N2147483647F", Int32.MaxValue.ToString ("N0F", _nfi), "#02");
			Assert.AreEqual ("N2147483647xF", Int32.MaxValue.ToString ("N0xF", _nfi), "#03");
		}

		[Test]
		public void Test04008 ()
		{
			Assert.AreEqual ("-NF", Int32.MinValue.ToString ("NF", _nfi), "#01");
			Assert.AreEqual ("-N2147483648F", Int32.MinValue.ToString ("N0F", _nfi), "#02");
			Assert.AreEqual ("-N2147483648xF", Int32.MinValue.ToString ("N0xF", _nfi), "#03");
		}

		[Test]
		public void Test04009 ()
		{
			Assert.AreEqual ("0.0000000000", 0.ToString ("N0000000000000000000000000000000000000010", _nfi), "#01");
			Assert.AreEqual ("2,147,483,647.0000000000", Int32.MaxValue.ToString ("N0000000000000000000000000000000000000010", _nfi), "#02");
			Assert.AreEqual ("-2,147,483,648.0000000000", Int32.MinValue.ToString ("N0000000000000000000000000000000000000010", _nfi), "#03");
		}

		[Test]
		public void Test04010 ()
		{
			Assert.AreEqual ("+N", 0.ToString ("+N", _nfi), "#01");
			Assert.AreEqual ("N+", 0.ToString ("N+", _nfi), "#02");
			Assert.AreEqual ("+N+", 0.ToString ("+N+", _nfi), "#03");
		}
		
		[Test]
		public void Test04011 ()
		{
			Assert.AreEqual ("+N", Int32.MaxValue.ToString ("+N", _nfi), "#01");
			Assert.AreEqual ("N+", Int32.MaxValue.ToString ("N+", _nfi), "#02");
			Assert.AreEqual ("+N+", Int32.MaxValue.ToString ("+N+", _nfi), "#03");
		}

		[Test]
		public void Test04012 ()
		{
			Assert.AreEqual ("-+N", Int32.MinValue.ToString ("+N", _nfi), "#01");
			Assert.AreEqual ("-N+", Int32.MinValue.ToString ("N+", _nfi), "#02");
			Assert.AreEqual ("-+N+", Int32.MinValue.ToString ("+N+", _nfi), "#03");
		}

		[Test]
		public void Test04013 ()
		{
			Assert.AreEqual ("-N", 0.ToString ("-N", _nfi), "#01");
			Assert.AreEqual ("N-", 0.ToString ("N-", _nfi), "#02");
			Assert.AreEqual ("-N-", 0.ToString ("-N-", _nfi), "#03");
		}
		
		[Test]
		public void Test04014 ()
		{
			Assert.AreEqual ("-N", Int32.MaxValue.ToString ("-N", _nfi), "#01");
			Assert.AreEqual ("N-", Int32.MaxValue.ToString ("N-", _nfi), "#02");
			Assert.AreEqual ("-N-", Int32.MaxValue.ToString ("-N-", _nfi), "#03");
		}

		[Test]
		public void Test04015 ()
		{
			Assert.AreEqual ("--N", Int32.MinValue.ToString ("-N", _nfi), "#01");
			Assert.AreEqual ("-N-", Int32.MinValue.ToString ("N-", _nfi), "#02");
			Assert.AreEqual ("--N-", Int32.MinValue.ToString ("-N-", _nfi), "#03");
		}

		[Test]
		public void Test04016 ()
		{
			Assert.AreEqual ("N+0", 0.ToString ("N+0", _nfi), "#01");
			Assert.AreEqual ("N+2147483647", Int32.MaxValue.ToString ("N+0", _nfi), "#02");
			Assert.AreEqual ("-N+2147483648", Int32.MinValue.ToString ("N+0", _nfi), "#03");
		}

		[Test]
		public void Test04017 ()
		{
			Assert.AreEqual ("N+9", 0.ToString ("N+9", _nfi), "#01");
			Assert.AreEqual ("N+9", Int32.MaxValue.ToString ("N+9", _nfi), "#02");
			Assert.AreEqual ("-N+9", Int32.MinValue.ToString ("N+9", _nfi), "#03");
		}

		[Test]
		public void Test04018 ()
		{
			Assert.AreEqual ("N-9", 0.ToString ("N-9", _nfi), "#01");
			Assert.AreEqual ("N-9", Int32.MaxValue.ToString ("N-9", _nfi), "#02");
			Assert.AreEqual ("-N-9", Int32.MinValue.ToString ("N-9", _nfi), "#03");
		}

		[Test]
		public void Test04019 ()
		{
			Assert.AreEqual ("N0", 0.ToString ("N0,", _nfi), "#01");
			Assert.AreEqual ("N2147484", Int32.MaxValue.ToString ("N0,", _nfi), "#02");
			Assert.AreEqual ("-N2147484", Int32.MinValue.ToString ("N0,", _nfi), "#03");
		}

		[Test]
		public void Test04020 ()
		{
			Assert.AreEqual ("N0", 0.ToString ("N0.", _nfi), "#01");
			Assert.AreEqual ("N2147483647", Int32.MaxValue.ToString ("N0.", _nfi), "#02");
			Assert.AreEqual ("-N2147483648", Int32.MinValue.ToString ("N0.", _nfi), "#03");
		}

		[Test]
		public void Test04021 ()
		{
			Assert.AreEqual ("N0.0", 0.ToString ("N0.0", _nfi), "#01");
			Assert.AreEqual ("N2147483647.0", Int32.MaxValue.ToString ("N0.0", _nfi), "#02");
			Assert.AreEqual ("-N2147483648.0", Int32.MinValue.ToString ("N0.0", _nfi), "#03");
		}

		[Test]
		public void Test04022 ()
		{
			Assert.AreEqual ("N09", 0.ToString ("N0.9", _nfi), "#01");
			Assert.AreEqual ("N21474836479", Int32.MaxValue.ToString ("N0.9", _nfi), "#02");
			Assert.AreEqual ("-N21474836489", Int32.MinValue.ToString ("N0.9", _nfi), "#03");
		}

		[Test]
		public void Test04023 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalDigits = 0;
			Assert.AreEqual ("0", 0.ToString ("N", nfi), "#01");
			nfi.NumberDecimalDigits = 1;
			Assert.AreEqual ("0.0", 0.ToString ("N", nfi), "#02");
			nfi.NumberDecimalDigits = 99;
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("N", nfi), "#03");
		}

		[Test]
		public void Test04024 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "";
			Assert.AreEqual ("2,147,483,648.00", Int32.MinValue.ToString ("N", nfi), "#01");
			nfi.NegativeSign = "-";
			Assert.AreEqual ("-2,147,483,648.00", Int32.MinValue.ToString ("N", nfi), "#02");
			nfi.NegativeSign = "+";
			Assert.AreEqual ("+2,147,483,648.00", Int32.MinValue.ToString ("N", nfi), "#03");
			nfi.NegativeSign = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Assert.AreEqual ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ2,147,483,648.00", Int32.MinValue.ToString ("N", nfi), "#04");
		}

		[Test]
		public void Test04025 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "-";
			nfi.PositiveSign = "+";
			Assert.AreEqual ("-1.00", (-1).ToString ("N", nfi), "#01");
			Assert.AreEqual ("0.00", 0.ToString ("N", nfi), "#02");
			Assert.AreEqual ("1.00",1.ToString ("N", nfi), "#03");
		}

		[Test]
		public void Test04026 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";
			Assert.AreEqual ("+1.00", (-1).ToString ("N", nfi), "#01");
			Assert.AreEqual ("0.00", 0.ToString ("N", nfi), "#02");
			Assert.AreEqual ("1.00",1.ToString ("N", nfi), "#03");
		}

		[Test]
		public void Test04027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			Assert.AreEqual ("123#0",123.ToString ("N1", nfi), "#01");
		}

		[Test]
		public void Test04028 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberGroupSeparator = "-";
			Assert.AreEqual ("-2-147-483-648.0",Int32.MinValue.ToString ("N1", nfi), "#01");
		}

		[Test]
		public void Test04029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberGroupSizes = new int [] {};
			Assert.AreEqual ("-2147483648.0",Int32.MinValue.ToString ("N1", nfi), "#01");
			nfi.NumberGroupSizes = new int [] {0};
			Assert.AreEqual ("-2147483648.0",Int32.MinValue.ToString ("N1", nfi), "#02");
			nfi.NumberGroupSizes = new int [] {1};
			Assert.AreEqual ("-2,1,4,7,4,8,3,6,4,8.0",Int32.MinValue.ToString ("N1", nfi), "#03");
			nfi.NumberGroupSizes = new int [] {3};
			Assert.AreEqual ("-2,147,483,648.0",Int32.MinValue.ToString ("N1", nfi), "#04");
			nfi.NumberGroupSizes = new int [] {9};
			Assert.AreEqual ("-2,147483648.0",Int32.MinValue.ToString ("N1", nfi), "#05");
		}

		[Test]
		public void Test04030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberGroupSizes = new int [] {1,2};
			Assert.AreEqual ("-2,14,74,83,64,8.0",Int32.MinValue.ToString ("N1", nfi), "#01");
			nfi.NumberGroupSizes = new int [] {1,2,3};
			Assert.AreEqual ("-2,147,483,64,8.0",Int32.MinValue.ToString ("N1", nfi), "#02");
			nfi.NumberGroupSizes = new int [] {1,2,3,4};
			Assert.AreEqual ("-2147,483,64,8.0",Int32.MinValue.ToString ("N1", nfi), "#03");
			nfi.NumberGroupSizes = new int [] {1,2,1,2,1,2,1};
			Assert.AreEqual ("-2,14,7,48,3,64,8.0",Int32.MinValue.ToString ("N1", nfi), "#04");
			nfi.NumberGroupSizes = new int [] {1,0};
			Assert.AreEqual ("-214748364,8.0",Int32.MinValue.ToString ("N1", nfi), "#05");
			nfi.NumberGroupSizes = new int [] {1,2,0};
			Assert.AreEqual ("-2147483,64,8.0",Int32.MinValue.ToString ("N1", nfi), "#06");
			nfi.NumberGroupSizes = new int [] {1,2,3,0};
			Assert.AreEqual ("-2147,483,64,8.0",Int32.MinValue.ToString ("N1", nfi), "#07");
			nfi.NumberGroupSizes = new int [] {1,2,3,4,0};
			Assert.AreEqual ("-2147,483,64,8.0",Int32.MinValue.ToString ("N1", nfi), "#08");
		}

		[Test]
		public void Test04031 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "1234567890";
			Assert.AreEqual ("12345678902,147,483,648.00", Int32.MinValue.ToString ("N", nfi), "#01");
		}

		// Test05000 - Int32 and P
		[Test]
		public void Test05000 ()
		{
			Assert.AreEqual ("0.00 %", 0.ToString ("P", _nfi), "#01");
			Assert.AreEqual ("0.00 %", 0.ToString ("p", _nfi), "#02");
			Assert.AreEqual ("-214,748,364,800.00 %", Int32.MinValue.ToString ("P", _nfi), "#03");
			Assert.AreEqual ("-214,748,364,800.00 %", Int32.MinValue.ToString ("p", _nfi), "#04");
			Assert.AreEqual ("214,748,364,700.00 %", Int32.MaxValue.ToString ("P", _nfi), "#05");
			Assert.AreEqual ("214,748,364,700.00 %", Int32.MaxValue.ToString ("p", _nfi), "#06");
		}

		[Test]
		public void Test05001 ()
		{
			Assert.AreEqual ("P ", 0.ToString ("P ", _nfi), "#01");
			Assert.AreEqual (" P", 0.ToString (" P", _nfi), "#02");
			Assert.AreEqual (" P ", 0.ToString (" P ", _nfi), "#03");
		}

		[Test]
		public void Test05002 ()
		{
			Assert.AreEqual ("-P ", (-1).ToString ("P ", _nfi), "#01");
			Assert.AreEqual ("- P", (-1).ToString (" P", _nfi), "#02");
			Assert.AreEqual ("- P ", (-1).ToString (" P ", _nfi), "#03");
		}

		[Test]
		public void Test05003 ()
		{
			Assert.AreEqual ("0 %", 0.ToString ("P0", _nfi), "#01");
			Assert.AreEqual ("0.000000000 %", 0.ToString ("P9", _nfi), "#02");
			Assert.AreEqual ("0.0000000000 %", 0.ToString ("P10", _nfi), "#03");
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", 0.ToString ("P99", _nfi), "#04");
			Assert.AreEqual ("P100", 0.ToString ("P100", _nfi), "#05");
		}

		[Test]
		public void Test05004 ()
		{
			Assert.AreEqual ("214,748,364,700 %", Int32.MaxValue.ToString ("P0", _nfi), "#01");
			Assert.AreEqual ("214,748,364,700.000000000 %", Int32.MaxValue.ToString ("P9", _nfi), "#02");
			Assert.AreEqual ("214,748,364,700.0000000000 %", Int32.MaxValue.ToString ("P10", _nfi), "#03");
			Assert.AreEqual ("214,748,364,700.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", Int32.MaxValue.ToString ("P99", _nfi), "#04");
			Assert.AreEqual ("P12147483647", Int32.MaxValue.ToString ("P100", _nfi), "#05");
		}

		[Test]
		public void Test05005 ()
		{
			Assert.AreEqual ("-214,748,364,800 %", Int32.MinValue.ToString ("P0", _nfi), "#01");
			Assert.AreEqual ("-214,748,364,800.000000000 %", Int32.MinValue.ToString ("P9", _nfi), "#02");
			Assert.AreEqual ("-214,748,364,800.0000000000 %", Int32.MinValue.ToString ("P10", _nfi), "#03");
			Assert.AreEqual ("-214,748,364,800.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", Int32.MinValue.ToString ("P99", _nfi), "#04");
			Assert.AreEqual ("-P12147483648", Int32.MinValue.ToString ("P100", _nfi), "#05");
		}

		[Test]
		public void Test05006 ()
		{
			Assert.AreEqual ("PF", 0.ToString ("PF", _nfi), "#01");
			Assert.AreEqual ("P0F", 0.ToString ("P0F", _nfi), "#02");
			Assert.AreEqual ("P0xF", 0.ToString ("P0xF", _nfi), "#03");
		}

		[Test]
		public void Test05007 ()
		{
			Assert.AreEqual ("PF", Int32.MaxValue.ToString ("PF", _nfi), "#01");
			Assert.AreEqual ("P2147483647F", Int32.MaxValue.ToString ("P0F", _nfi), "#02");
			Assert.AreEqual ("P2147483647xF", Int32.MaxValue.ToString ("P0xF", _nfi), "#03");
		}

		[Test]
		public void Test05008 ()
		{
			Assert.AreEqual ("-PF", Int32.MinValue.ToString ("PF", _nfi), "#01");
			Assert.AreEqual ("-P2147483648F", Int32.MinValue.ToString ("P0F", _nfi), "#02");
			Assert.AreEqual ("-P2147483648xF", Int32.MinValue.ToString ("P0xF", _nfi), "#03");
		}

		[Test]
		public void Test05009 ()
		{
			Assert.AreEqual ("0.0000000000 %", 0.ToString ("P0000000000000000000000000000000000000010", _nfi), "#01");
			Assert.AreEqual ("214,748,364,700.0000000000 %", Int32.MaxValue.ToString ("P0000000000000000000000000000000000000010", _nfi), "#02");
			Assert.AreEqual ("-214,748,364,800.0000000000 %", Int32.MinValue.ToString ("P0000000000000000000000000000000000000010", _nfi), "#03");
		}

		[Test]
		public void Test05010 ()
		{
			Assert.AreEqual ("+P", 0.ToString ("+P", _nfi), "#01");
			Assert.AreEqual ("P+", 0.ToString ("P+", _nfi), "#02");
			Assert.AreEqual ("+P+", 0.ToString ("+P+", _nfi), "#03");
		}
		
		[Test]
		public void Test05011 ()
		{
			Assert.AreEqual ("+P", Int32.MaxValue.ToString ("+P", _nfi), "#01");
			Assert.AreEqual ("P+", Int32.MaxValue.ToString ("P+", _nfi), "#02");
			Assert.AreEqual ("+P+", Int32.MaxValue.ToString ("+P+", _nfi), "#03");
		}

		[Test]
		public void Test05012 ()
		{
			Assert.AreEqual ("-+P", Int32.MinValue.ToString ("+P", _nfi), "#01");
			Assert.AreEqual ("-P+", Int32.MinValue.ToString ("P+", _nfi), "#02");
			Assert.AreEqual ("-+P+", Int32.MinValue.ToString ("+P+", _nfi), "#03");
		}

		[Test]
		public void Test05013 ()
		{
			Assert.AreEqual ("-P", 0.ToString ("-P", _nfi), "#01");
			Assert.AreEqual ("P-", 0.ToString ("P-", _nfi), "#02");
			Assert.AreEqual ("-P-", 0.ToString ("-P-", _nfi), "#03");
		}
		
		[Test]
		public void Test05014 ()
		{
			Assert.AreEqual ("-P", Int32.MaxValue.ToString ("-P", _nfi), "#01");
			Assert.AreEqual ("P-", Int32.MaxValue.ToString ("P-", _nfi), "#02");
			Assert.AreEqual ("-P-", Int32.MaxValue.ToString ("-P-", _nfi), "#03");
		}

		[Test]
		public void Test05015 ()
		{
			Assert.AreEqual ("--P", Int32.MinValue.ToString ("-P", _nfi), "#01");
			Assert.AreEqual ("-P-", Int32.MinValue.ToString ("P-", _nfi), "#02");
			Assert.AreEqual ("--P-", Int32.MinValue.ToString ("-P-", _nfi), "#03");
		}

		[Test]
		public void Test05016 ()
		{
			Assert.AreEqual ("P+0", 0.ToString ("P+0", _nfi), "#01");
			Assert.AreEqual ("P+2147483647", Int32.MaxValue.ToString ("P+0", _nfi), "#02");
			Assert.AreEqual ("-P+2147483648", Int32.MinValue.ToString ("P+0", _nfi), "#03");
		}

		[Test]
		public void Test05017 ()
		{
			Assert.AreEqual ("P+9", 0.ToString ("P+9", _nfi), "#01");
			Assert.AreEqual ("P+9", Int32.MaxValue.ToString ("P+9", _nfi), "#02");
			Assert.AreEqual ("-P+9", Int32.MinValue.ToString ("P+9", _nfi), "#03");
		}

		[Test]
		public void Test05018 ()
		{
			Assert.AreEqual ("P-9", 0.ToString ("P-9", _nfi), "#01");
			Assert.AreEqual ("P-9", Int32.MaxValue.ToString ("P-9", _nfi), "#02");
			Assert.AreEqual ("-P-9", Int32.MinValue.ToString ("P-9", _nfi), "#03");
		}

		[Test]
		public void Test05019 ()
		{
			Assert.AreEqual ("P0", 0.ToString ("P0,", _nfi), "#01");
			Assert.AreEqual ("P2147484", Int32.MaxValue.ToString ("P0,", _nfi), "#02");
			Assert.AreEqual ("-P2147484", Int32.MinValue.ToString ("P0,", _nfi), "#03");
		}

		[Test]
		public void Test05020 ()
		{
			Assert.AreEqual ("P0", 0.ToString ("P0.", _nfi), "#01");
			Assert.AreEqual ("P2147483647", Int32.MaxValue.ToString ("P0.", _nfi), "#02");
			Assert.AreEqual ("-P2147483648", Int32.MinValue.ToString ("P0.", _nfi), "#03");
		}

		[Test]
		public void Test05021 ()
		{
			Assert.AreEqual ("P0.0", 0.ToString ("P0.0", _nfi), "#01");
			Assert.AreEqual ("P2147483647.0", Int32.MaxValue.ToString ("P0.0", _nfi), "#02");
			Assert.AreEqual ("-P2147483648.0", Int32.MinValue.ToString ("P0.0", _nfi), "#03");
		}

		[Test]
		public void Test05022 ()
		{
			Assert.AreEqual ("P09", 0.ToString ("P0.9", _nfi), "#01");
			Assert.AreEqual ("P21474836479", Int32.MaxValue.ToString ("P0.9", _nfi), "#02");
			Assert.AreEqual ("-P21474836489", Int32.MinValue.ToString ("P0.9", _nfi), "#03");
		}

		[Test]
		public void Test05023 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentDecimalDigits = 0;
			Assert.AreEqual ("0 %", 0.ToString ("P", nfi), "#01");
			nfi.PercentDecimalDigits = 1;
			Assert.AreEqual ("0.0 %", 0.ToString ("P", nfi), "#02");
			nfi.PercentDecimalDigits = 99;
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", 0.ToString ("P", nfi), "#03");
		}

		[Test]
		public void Test05024 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "";
			Assert.AreEqual ("214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi), "#01");
			nfi.NegativeSign = "-";
			Assert.AreEqual ("-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi), "#02");
			nfi.NegativeSign = "+";
			Assert.AreEqual ("+214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi), "#03");
			nfi.NegativeSign = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMPOPQRSTUVWXYZ";
			Assert.AreEqual ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMPOPQRSTUVWXYZ214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi), "#04");
		}

		[Test]
		public void Test05025 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "-";
			nfi.PositiveSign = "+";
			Assert.AreEqual ("-100.00 %", (-1).ToString ("P", nfi), "#01");
			Assert.AreEqual ("0.00 %", 0.ToString ("P", nfi), "#02");
			Assert.AreEqual ("100.00 %",1.ToString ("P", nfi), "#03");
		}

		[Test]
		public void Test05026 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";
			Assert.AreEqual ("+100.00 %", (-1).ToString ("P", nfi), "#01");
			Assert.AreEqual ("0.00 %", 0.ToString ("P", nfi), "#02");
			Assert.AreEqual ("100.00 %",1.ToString ("P", nfi), "#03");
		}

		[Test]
		public void Test05027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentDecimalSeparator = "#";
			Assert.AreEqual ("12,300#0 %",123.ToString ("P1", nfi), "#01");
		}

		[Test]
		public void Test05028 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentGroupSeparator = "-";
			Assert.AreEqual ("-214-748-364-800.0 %",Int32.MinValue.ToString ("P1", nfi), "#01");
		}

		[Test]
		public void Test05029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentGroupSizes = new int [] {};
			Assert.AreEqual ("-214748364800.0 %",Int32.MinValue.ToString ("P1", nfi), "#01");
			nfi.PercentGroupSizes = new int [] {0};
			Assert.AreEqual ("-214748364800.0 %",Int32.MinValue.ToString ("P1", nfi), "#02");
			nfi.PercentGroupSizes = new int [] {1};
			Assert.AreEqual ("-2,1,4,7,4,8,3,6,4,8,0,0.0 %",Int32.MinValue.ToString ("P1", nfi), "#03");
			nfi.PercentGroupSizes = new int [] {3};
			Assert.AreEqual ("-214,748,364,800.0 %",Int32.MinValue.ToString ("P1", nfi), "#04");
			nfi.PercentGroupSizes = new int [] {9};
			Assert.AreEqual ("-214,748364800.0 %",Int32.MinValue.ToString ("P1", nfi), "#05");
		}

		[Test]
		public void Test05030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentGroupSizes = new int [] {1,2};
			Assert.AreEqual ("-2,14,74,83,64,80,0.0 %",Int32.MinValue.ToString ("P1", nfi), "#01");
			nfi.PercentGroupSizes = new int [] {1,2,3};
			Assert.AreEqual ("-214,748,364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi), "#02");
			nfi.PercentGroupSizes = new int [] {1,2,3,4};
			Assert.AreEqual ("-21,4748,364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi), "#03");
			nfi.PercentGroupSizes = new int [] {1,2,1,2,1,2,1};
			Assert.AreEqual ("-2,1,4,74,8,36,4,80,0.0 %",Int32.MinValue.ToString ("P1", nfi), "#04");
			nfi.PercentGroupSizes = new int [] {1,0};
			Assert.AreEqual ("-21474836480,0.0 %",Int32.MinValue.ToString ("P1", nfi), "#05");
			nfi.PercentGroupSizes = new int [] {1,2,0};
			Assert.AreEqual ("-214748364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi), "#06");
			nfi.PercentGroupSizes = new int [] {1,2,3,0};
			Assert.AreEqual ("-214748,364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi), "#07");
			nfi.PercentGroupSizes = new int [] {1,2,3,4,0};
			Assert.AreEqual ("-21,4748,364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi), "#08");
		}

		[Test]
		public void Test05031 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "1234567890";
			Assert.AreEqual ("1234567890214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi), "#01");
		}

		[Test]
		public void Test05032 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentNegativePattern = 0;
			Assert.AreEqual ("-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi), "#01");
			Assert.AreEqual ("214,748,364,700.00 %", Int32.MaxValue.ToString ("P", nfi), "#02");
			Assert.AreEqual ("0.00 %", 0.ToString ("P", nfi), "#03");
		}

		[Test]
		public void Test05033 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentNegativePattern = 1;
			Assert.AreEqual ("-214,748,364,800.00%", Int32.MinValue.ToString ("P", nfi), "#01");
			Assert.AreEqual ("214,748,364,700.00 %", Int32.MaxValue.ToString ("P", nfi), "#02");
			Assert.AreEqual ("0.00 %", 0.ToString ("P", nfi), "#03");
		}

		[Test]
		public void Test05034 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentNegativePattern = 2;
			Assert.AreEqual ("-%214,748,364,800.00", Int32.MinValue.ToString ("P", nfi), "#01");
			Assert.AreEqual ("214,748,364,700.00 %", Int32.MaxValue.ToString ("P", nfi), "#02");
			Assert.AreEqual ("0.00 %", 0.ToString ("P", nfi), "#03");
		}

		[Test]
		public void Test05035 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentPositivePattern = 0;
			Assert.AreEqual ("-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi), "#01");
			Assert.AreEqual ("214,748,364,700.00 %", Int32.MaxValue.ToString ("P", nfi), "#02");
			Assert.AreEqual ("0.00 %", 0.ToString ("P", nfi), "#03");
		}

		[Test]
		public void Test05036 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentPositivePattern = 1;
			Assert.AreEqual ("-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi), "#01");
			Assert.AreEqual ("214,748,364,700.00%", Int32.MaxValue.ToString ("P", nfi), "#02");
			Assert.AreEqual ("0.00%", 0.ToString ("P", nfi), "#03");
		}

		[Test]
		public void Test05037 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentPositivePattern = 2;
			Assert.AreEqual ("-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi), "#01");
			Assert.AreEqual ("%214,748,364,700.00", Int32.MaxValue.ToString ("P", nfi), "#02");
			Assert.AreEqual ("%0.00", 0.ToString ("P", nfi), "#03");
		}

		// Test06000 - Int32 and R
		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Test06000 ()
		{
			Assert.AreEqual ("0", 0.ToString ("R", _nfi), "#01");
		}

		// Test07000- Int32 and X
		[Test]
		public void Test07000 ()
		{
			Assert.AreEqual ("0", 0.ToString ("X", _nfi), "#01");
			Assert.AreEqual ("0", 0.ToString ("x", _nfi), "#02");
			Assert.AreEqual ("80000000", Int32.MinValue.ToString ("X", _nfi), "#03");
			Assert.AreEqual ("80000000", Int32.MinValue.ToString ("x", _nfi), "#04");
			Assert.AreEqual ("7FFFFFFF", Int32.MaxValue.ToString ("X", _nfi), "#05");
			Assert.AreEqual ("7fffffff", Int32.MaxValue.ToString ("x", _nfi), "#06");
		}

		[Test]
		public void Test07001 ()
		{
			Assert.AreEqual ("X ", 0.ToString ("X ", _nfi), "#01");
			Assert.AreEqual (" X", 0.ToString (" X", _nfi), "#02");
			Assert.AreEqual (" X ", 0.ToString (" X ", _nfi), "#03");
		}

		[Test]
		public void Test07002 ()
		{
			Assert.AreEqual ("-X ", (-1).ToString ("X ", _nfi), "#01");
			Assert.AreEqual ("- X", (-1).ToString (" X", _nfi), "#02");
			Assert.AreEqual ("- X ", (-1).ToString (" X ", _nfi), "#03");
		}

		[Test]
		public void Test07003 ()
		{
			Assert.AreEqual ("0", 0.ToString ("X0", _nfi), "#01");
			Assert.AreEqual ("0000000000", 0.ToString ("X10", _nfi), "#02");
			Assert.AreEqual ("00000000000", 0.ToString ("X11", _nfi), "#03");
			Assert.AreEqual ("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("X99", _nfi), "#04");
			Assert.AreEqual ("X100", 0.ToString ("X100", _nfi), "#05");
		}

		[Test]
		public void Test07004 ()
		{
			Assert.AreEqual ("7FFFFFFF", Int32.MaxValue.ToString ("X0", _nfi), "#01");
			Assert.AreEqual ("007FFFFFFF", Int32.MaxValue.ToString ("X10", _nfi), "#02");
			Assert.AreEqual ("0007FFFFFFF", Int32.MaxValue.ToString ("X11", _nfi), "#03");
			Assert.AreEqual ("00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000007FFFFFFF", Int32.MaxValue.ToString ("X99", _nfi), "#04");
			Assert.AreEqual ("X12147483647", Int32.MaxValue.ToString ("X100", _nfi), "#05");
		}

		[Test]
		public void Test07005 ()
		{
			Assert.AreEqual ("80000000", Int32.MinValue.ToString ("X0", _nfi), "#01");
			Assert.AreEqual ("0080000000", Int32.MinValue.ToString ("X10", _nfi), "#02");
			Assert.AreEqual ("00080000000", Int32.MinValue.ToString ("X11", _nfi), "#03");
			Assert.AreEqual ("000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000080000000", Int32.MinValue.ToString ("X99", _nfi), "#04");
			Assert.AreEqual ("-X12147483648", Int32.MinValue.ToString ("X100", _nfi), "#05");
		}

		[Test]
		public void Test07006 ()
		{
			Assert.AreEqual ("XF", 0.ToString ("XF", _nfi), "#01");
			Assert.AreEqual ("X0F", 0.ToString ("X0F", _nfi), "#02");
			Assert.AreEqual ("X0xF", 0.ToString ("X0xF", _nfi), "#03");
		}

		[Test]
		public void Test07007 ()
		{
			Assert.AreEqual ("XF", Int32.MaxValue.ToString ("XF", _nfi), "#01");
			Assert.AreEqual ("X2147483647F", Int32.MaxValue.ToString ("X0F", _nfi), "#02");
			Assert.AreEqual ("X2147483647xF", Int32.MaxValue.ToString ("X0xF", _nfi), "#03");
		}

		[Test]
		public void Test07008 ()
		{
			Assert.AreEqual ("-XF", Int32.MinValue.ToString ("XF", _nfi), "#01");
			Assert.AreEqual ("-X2147483648F", Int32.MinValue.ToString ("X0F", _nfi), "#02");
			Assert.AreEqual ("-X2147483648xF", Int32.MinValue.ToString ("X0xF", _nfi), "#03");
		}

		[Test]
		public void Test07009 ()
		{
			Assert.AreEqual ("00000000000", 0.ToString ("X0000000000000000000000000000000000000011", _nfi), "#01");
			Assert.AreEqual ("0007FFFFFFF", Int32.MaxValue.ToString ("X0000000000000000000000000000000000000011", _nfi), "#02");
			Assert.AreEqual ("00080000000", Int32.MinValue.ToString ("X0000000000000000000000000000000000000011", _nfi), "#03");
		}

		[Test]
		public void Test07010 ()
		{
			Assert.AreEqual ("+X", 0.ToString ("+X", _nfi), "#01");
			Assert.AreEqual ("X+", 0.ToString ("X+", _nfi), "#02");
			Assert.AreEqual ("+X+", 0.ToString ("+X+", _nfi), "#03");
		}
		
		[Test]
		public void Test07011 ()
		{
			Assert.AreEqual ("+X", Int32.MaxValue.ToString ("+X", _nfi), "#01");
			Assert.AreEqual ("X+", Int32.MaxValue.ToString ("X+", _nfi), "#02");
			Assert.AreEqual ("+X+", Int32.MaxValue.ToString ("+X+", _nfi), "#03");
		}

		[Test]
		public void Test07012 ()
		{
			Assert.AreEqual ("-+X", Int32.MinValue.ToString ("+X", _nfi), "#01");
			Assert.AreEqual ("-X+", Int32.MinValue.ToString ("X+", _nfi), "#02");
			Assert.AreEqual ("-+X+", Int32.MinValue.ToString ("+X+", _nfi), "#03");
		}

		[Test]
		public void Test07013 ()
		{
			Assert.AreEqual ("-X", 0.ToString ("-X", _nfi), "#01");
			Assert.AreEqual ("X-", 0.ToString ("X-", _nfi), "#02");
			Assert.AreEqual ("-X-", 0.ToString ("-X-", _nfi), "#03");
		}
		
		[Test]
		public void Test07014 ()
		{
			Assert.AreEqual ("-X", Int32.MaxValue.ToString ("-X", _nfi), "#01");
			Assert.AreEqual ("X-", Int32.MaxValue.ToString ("X-", _nfi), "#02");
			Assert.AreEqual ("-X-", Int32.MaxValue.ToString ("-X-", _nfi), "#03");
		}

		[Test]
		public void Test07015 ()
		{
			Assert.AreEqual ("--X", Int32.MinValue.ToString ("-X", _nfi), "#01");
			Assert.AreEqual ("-X-", Int32.MinValue.ToString ("X-", _nfi), "#02");
			Assert.AreEqual ("--X-", Int32.MinValue.ToString ("-X-", _nfi), "#03");
		}

		[Test]
		public void Test07016 ()
		{
			Assert.AreEqual ("X+0", 0.ToString ("X+0", _nfi), "#01");
			Assert.AreEqual ("X+2147483647", Int32.MaxValue.ToString ("X+0", _nfi), "#02");
			Assert.AreEqual ("-X+2147483648", Int32.MinValue.ToString ("X+0", _nfi), "#03");
		}

		[Test]
		public void Test07017 ()
		{
			Assert.AreEqual ("X+9", 0.ToString ("X+9", _nfi), "#01");
			Assert.AreEqual ("X+9", Int32.MaxValue.ToString ("X+9", _nfi), "#02");
			Assert.AreEqual ("-X+9", Int32.MinValue.ToString ("X+9", _nfi), "#03");
		}

		[Test]
		public void Test07018 ()
		{
			Assert.AreEqual ("X-9", 0.ToString ("X-9", _nfi), "#01");
			Assert.AreEqual ("X-9", Int32.MaxValue.ToString ("X-9", _nfi), "#02");
			Assert.AreEqual ("-X-9", Int32.MinValue.ToString ("X-9", _nfi), "#03");
		}

		[Test]
		public void Test07019 ()
		{
			Assert.AreEqual ("X0", 0.ToString ("X0,", _nfi), "#01");
			Assert.AreEqual ("X2147484", Int32.MaxValue.ToString ("X0,", _nfi), "#02");
			Assert.AreEqual ("-X2147484", Int32.MinValue.ToString ("X0,", _nfi), "#03");
		}

		[Test]
		public void Test07020 ()
		{
			Assert.AreEqual ("X0", 0.ToString ("X0.", _nfi), "#01");
			Assert.AreEqual ("X2147483647", Int32.MaxValue.ToString ("X0.", _nfi), "#02");
			Assert.AreEqual ("-X2147483648", Int32.MinValue.ToString ("X0.", _nfi), "#03");
		}

		[Test]
		public void Test07021 ()
		{
			Assert.AreEqual ("X0.0", 0.ToString ("X0.0", _nfi), "#01");
			Assert.AreEqual ("X2147483647.0", Int32.MaxValue.ToString ("X0.0", _nfi), "#02");
			Assert.AreEqual ("-X2147483648.0", Int32.MinValue.ToString ("X0.0", _nfi), "#03");
		}

		[Test]
		public void Test07022 ()
		{
			Assert.AreEqual ("X09", 0.ToString ("X0.9", _nfi), "#01");
			Assert.AreEqual ("X21474836479", Int32.MaxValue.ToString ("X0.9", _nfi), "#02");
			Assert.AreEqual ("-X21474836489", Int32.MinValue.ToString ("X0.9", _nfi), "#03");
		}

		[Test]
		public void Test08000 ()
		{
			Assert.AreEqual ("0", 0.ToString ("0", _nfi), "#01");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("0", _nfi), "#02");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("0", _nfi), "#03");
		}

		// Test08000 - Int32 and Custom
		[Test]
		public void Test08001 ()
		{
			Assert.AreEqual ("00000000000", 0.ToString ("00000000000", _nfi), "#01");
			Assert.AreEqual ("02147483647", Int32.MaxValue.ToString ("00000000000", _nfi), "#02");
			Assert.AreEqual ("-02147483648", Int32.MinValue.ToString ("00000000000", _nfi), "#03");
		}

		[Test]
		public void Test08002 ()
		{
			Assert.AreEqual (" 00000000000 ", 0.ToString (" 00000000000 ", _nfi), "#01");
			Assert.AreEqual (" 02147483647 ", Int32.MaxValue.ToString (" 00000000000 ", _nfi), "#02");
			Assert.AreEqual ("- 02147483648 ", Int32.MinValue.ToString (" 00000000000 ", _nfi), "#03");
		}

		[Test]
		public void Test08003 ()
		{
			Assert.AreEqual ("", 0.ToString ("#", _nfi), "#01");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("#", _nfi), "#02");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("#", _nfi), "#03");
		}

		[Test]
		public void Test08004 ()
		{
			Assert.AreEqual ("", 0.ToString ("##########", _nfi), "#01");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString ("##########", _nfi), "#02");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString ("##########", _nfi), "#03");
		}

		[Test]
		public void Test08005 ()
		{
			Assert.AreEqual ("  ", 0.ToString (" ########## ", _nfi), "#01");
			Assert.AreEqual (" 2147483647 ", Int32.MaxValue.ToString (" ########## ", _nfi), "#02");
			Assert.AreEqual ("- 2147483648 ", Int32.MinValue.ToString (" ########## ", _nfi), "#03");
		}

		[Test]
		public void Test08006 ()
		{
			Assert.AreEqual ("", 0.ToString (".", _nfi), "#01");
			Assert.AreEqual ("2147483647", Int32.MaxValue.ToString (".", _nfi), "#02");
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString (".", _nfi), "#03");
		}

		[Test]
		public void Test08007 ()
		{
			Assert.AreEqual ("00000000000", 0.ToString ("00000000000.", _nfi), "#01");
			Assert.AreEqual ("02147483647", Int32.MaxValue.ToString ("00000000000.", _nfi), "#02");
			Assert.AreEqual ("-02147483648", Int32.MinValue.ToString ("00000000000.", _nfi), "#03");
		}

		[Test]
		public void Test08008 ()
		{
			Assert.AreEqual (".00000000000", 0.ToString (".00000000000", _nfi), "#01");
			Assert.AreEqual ("2147483647.00000000000", Int32.MaxValue.ToString (".00000000000", _nfi), "#02");
			Assert.AreEqual ("-2147483648.00000000000", Int32.MinValue.ToString (".00000000000", _nfi), "#03");
		}

		[Test]
		public void Test08009 ()
		{
			Assert.AreEqual ("00000000000.00000000000", 0.ToString ("00000000000.00000000000", _nfi), "#01");
			Assert.AreEqual ("02147483647.00000000000", Int32.MaxValue.ToString ("00000000000.00000000000", _nfi), "#02");
			Assert.AreEqual ("-02147483648.00000000000", Int32.MinValue.ToString ("00000000000.00000000000", _nfi), "#03");
		}

		[Test]
		public void Test08010 ()
		{
			Assert.AreEqual ("00.0000000000", 0.ToString ("00.0.00.000.0000", _nfi), "#01");
			Assert.AreEqual ("01.0000000000", 1.ToString ("00.0.00.000.0000", _nfi), "#02");
			Assert.AreEqual ("-01.0000000000", (-1).ToString ("00.0.00.000.0000", _nfi), "#03");
		}

		[Test]
		public void Test08011 ()
		{
			Assert.AreEqual ("", 0.ToString ("##.#.##.###.####", _nfi), "#01");
			Assert.AreEqual ("1", 1.ToString ("##.#.##.###.####", _nfi), "#02");
			Assert.AreEqual ("-1", (-1).ToString ("##.#.##.###.####", _nfi), "#03");
		}

		[Test]
		public void Test08012 ()
		{
			Assert.AreEqual ("00", 0.ToString ("0#.#.##.###.####", _nfi), "#01");
			Assert.AreEqual ("01", 1.ToString ("0#.#.##.###.####", _nfi), "#02");
			Assert.AreEqual ("-01", (-1).ToString ("0#.#.##.###.####", _nfi), "#03");
		}

		[Test]
		public void Test08013 ()
		{
			Assert.AreEqual ("0", 0.ToString ("#0.#.##.###.####", _nfi), "#01");
			Assert.AreEqual ("1", 1.ToString ("#0.#.##.###.####", _nfi), "#02");
			Assert.AreEqual ("-1", (-1).ToString ("#0.#.##.###.####", _nfi), "#03");
		}

		[Test]
		public void Test08014 ()
		{
			Assert.AreEqual (".0000000000", 0.ToString ("##.#.##.###.###0", _nfi), "#01");
			Assert.AreEqual ("1.0000000000", 1.ToString ("##.#.##.###.###0", _nfi), "#02");
			Assert.AreEqual ("-1.0000000000", (-1).ToString ("##.#.##.###.###0", _nfi), "#03");
		}

		[Test]
		public void Test08015 ()
		{
			Assert.AreEqual (".000000000", 0.ToString ("##.#.##.###.##0#", _nfi), "#01");
			Assert.AreEqual ("1.000000000", 1.ToString ("##.#.##.###.##0#", _nfi), "#02");
			Assert.AreEqual ("-1.000000000", (-1).ToString ("##.#.##.###.##0#", _nfi), "#03");
		}

		[Test]
		public void Test08016 ()
		{
			Assert.AreEqual (".000000000", 0.ToString ("##.#.##.##0.##0#", _nfi), "#01");
			Assert.AreEqual ("1.000000000", 1.ToString ("##.#.##.##0.##0#", _nfi), "#02");
			Assert.AreEqual ("-1.000000000", (-1).ToString ("##.#.##.##0.##0#", _nfi), "#03");
		}

		[Test]
		public void Test08017 ()
		{
			Assert.AreEqual ("0.000000000", 0.ToString ("#0.#.##.##0.##0#", _nfi), "#01");
			Assert.AreEqual ("1.000000000", 1.ToString ("#0.#.##.##0.##0#", _nfi), "#02");
			Assert.AreEqual ("-1.000000000", (-1).ToString ("#0.#.##.##0.##0#", _nfi), "#03");
		}

		[Test]
		public void Test08018 ()
		{
			Assert.AreEqual ("-0002147484", Int32.MinValue.ToString ("0000000000,", _nfi), "#01");
			Assert.AreEqual ("-0000002147", Int32.MinValue.ToString ("0000000000,,", _nfi), "#02");
			Assert.AreEqual ("-0000000002", Int32.MinValue.ToString ("0000000000,,,", _nfi), "#03");
			Assert.AreEqual ("0000000000", Int32.MinValue.ToString ("0000000000,,,,", _nfi), "#04");
			Assert.AreEqual ("0000000000", Int32.MinValue.ToString ("0000000000,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,", _nfi), "#05");
		}

		[Test]
		public void Test08019 ()
		{
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString (",0000000000", _nfi), "#01");
		}

		[Test]
		public void Test08020 ()
		{
			Assert.AreEqual ("-0002147484", Int32.MinValue.ToString (",0000000000,", _nfi), "#01");
		}

		[Test]
		public void Test08021 ()
		{
			Assert.AreEqual ("-02,147,483,648", Int32.MinValue.ToString ("0,0000000000", _nfi), "#01");
		}

		[Test]
		public void Test08022 ()
		{
			Assert.AreEqual ("-02,147,483,648", Int32.MinValue.ToString ("0000000000,0", _nfi), "#01");
		}

		[Test]
		public void Test08023 ()
		{
			Assert.AreEqual ("-02,147,483,648", Int32.MinValue.ToString ("0,0,0,0,0,0,0,0,0,0,0", _nfi), "#01");
		}

		[Test]
		public void Test08024 ()
		{
			Assert.AreEqual ("-02,147,483,648", Int32.MinValue.ToString (",0,0,0,0,0,0,0,0,0,0,0", _nfi), "#01");
		}

		[Test]
		public void Test08025 ()
		{
			Assert.AreEqual ("-00,002,147,484", Int32.MinValue.ToString ("0,0,0,0,0,0,0,0,0,0,0,", _nfi), "#01");
		}

		[Test]
		public void Test08026 ()
		{
			Assert.AreEqual ("-00,002,147,484", Int32.MinValue.ToString (",0,0,0,0,0,0,0,0,0,0,0,", _nfi), "#01");
		}

		[Test]
		public void Test08027 ()
		{
			Assert.AreEqual ("-", Int32.MinValue.ToString (",", _nfi), "#01");
		}

		[Test]
		public void Test08028 ()
		{
			Assert.AreEqual ("-2147483648", Int32.MinValue.ToString (",##########", _nfi), "#01");
		}

		[Test]
		public void Test08029 ()
		{
			Assert.AreEqual ("-2147484", Int32.MinValue.ToString (",##########,", _nfi), "#01");
		}

		[Test]
		public void Test08030 ()
		{
			Assert.AreEqual ("-2,147,483,648", Int32.MinValue.ToString ("#,##########", _nfi), "#01");
		}

		[Test]
		public void Test08031 ()
		{
			Assert.AreEqual ("-2,147,483,648", Int32.MinValue.ToString ("##########,#", _nfi), "#01");
		}

		[Test]
		public void Test08032 ()
		{
			Assert.AreEqual ("-2,147,483,648", Int32.MinValue.ToString ("#,#,#,#,#,#,#,#,#,#,#", _nfi), "#01");
		}

		[Test]
		public void Test08033 ()
		{
			Assert.AreEqual ("-2,147,483,648", Int32.MinValue.ToString (",#,#,#,#,#,#,#,#,#,#,#", _nfi), "#01");
		}

		[Test]
		public void Test08034 ()
		{
			Assert.AreEqual ("-2,147,484", Int32.MinValue.ToString ("#,#,#,#,#,#,#,#,#,#,#,", _nfi), "#01");
		}

		[Test]
		public void Test08035 ()
		{
			Assert.AreEqual ("-2,147,484", Int32.MinValue.ToString (",#,#,#,#,#,#,#,#,#,#,#,", _nfi), "#01");
		}

		[Test]
		public void Test08036 ()
		{
			Assert.AreEqual ("-1", (-1000).ToString ("##########,", _nfi), "#01");
		}

		[Test]
		public void Test08037 ()
		{
			Assert.AreEqual ("", (-100).ToString ("##########,", _nfi), "#01");
		}

		[Test]
		public void Test08038 ()
		{
			Assert.AreEqual ("-%", Int32.MinValue.ToString ("%", _nfi), "#01");
		}

		[Test]
		public void Test08039 ()
		{
			Assert.AreEqual ("-214748364800%", Int32.MinValue.ToString ("0%", _nfi), "#01");
		}

		[Test]
		public void Test08040 ()
		{
			Assert.AreEqual ("-%214748364800", Int32.MinValue.ToString ("%0", _nfi), "#01");
		}

		[Test]
		public void Test08041 ()
		{
			Assert.AreEqual ("-%21474836480000%", Int32.MinValue.ToString ("%0%", _nfi), "#01");
		}

		[Test]
		public void Test08042 ()
		{
			Assert.AreEqual ("- % 21474836480000 % ", Int32.MinValue.ToString (" % 0 % ", _nfi), "#01");
		}

		[Test]
		public void Test08043 ()
		{
			Assert.AreEqual ("-214748365%", Int32.MinValue.ToString ("0%,", _nfi), "#01");
		}

		[Test]
		public void Test08044 ()
		{
			Assert.AreEqual ("-214748365%", Int32.MinValue.ToString ("0,%", _nfi), "#01");
		}

		[Test]
		public void Test08045 ()
		{
			Assert.AreEqual ("-%214748364800", Int32.MinValue.ToString (",%0", _nfi), "#01");
		}

		[Test]
		public void Test08046 ()
		{
			Assert.AreEqual ("-%214748364800", Int32.MinValue.ToString ("%,0", _nfi), "#01");
		}

		[Test]
		public void Test08047 ()
		{
			Assert.AreEqual ("-2147483648%%%%%%", Int32.MinValue.ToString ("0,,,,%%%%%%", _nfi), "#01");
		}

		[Test]
		public void Test08048 ()
		{
			Assert.AreEqual ("-2147483648%%%%%%", Int32.MinValue.ToString ("0%%%%%%,,,,", _nfi), "#01");
		}

		[Test]
		public void Test08049 ()
		{
			Assert.AreEqual ("-%%%%%%2147483648", Int32.MinValue.ToString ("%%%%%%0,,,,", _nfi), "#01");
		}

		[Test]
		public void Test08050 ()
		{
			Assert.AreEqual ("E+0", Int32.MinValue.ToString ("E+0", _nfi), "#01");
			Assert.AreEqual ("e+0", Int32.MinValue.ToString ("e+0", _nfi), "#02");
			Assert.AreEqual ("E0", Int32.MinValue.ToString ("E-0", _nfi), "#03");
			Assert.AreEqual ("e0", Int32.MinValue.ToString ("e-0", _nfi), "#04");
		}

		[Test]
		public void Test08051 ()
		{
			Assert.AreEqual ("-2E+9", Int32.MinValue.ToString ("0E+0", _nfi), "#01");
			Assert.AreEqual ("-2e+9", Int32.MinValue.ToString ("0e+0", _nfi), "#02");
			Assert.AreEqual ("-2E9", Int32.MinValue.ToString ("0E-0", _nfi), "#03");
			Assert.AreEqual ("-2e9", Int32.MinValue.ToString ("0e-0", _nfi), "#04");
			Assert.AreEqual ("-2E9", Int32.MinValue.ToString ("0E0", _nfi), "#05");
			Assert.AreEqual ("-2e9", Int32.MinValue.ToString ("0e0", _nfi), "#06");
		}

		[Test]
		public void Test08052 ()
		{
			Assert.AreEqual ("-2E+9", Int32.MinValue.ToString ("#E+0", _nfi), "#01");
			Assert.AreEqual ("-2e+9", Int32.MinValue.ToString ("#e+0", _nfi), "#02");
			Assert.AreEqual ("-2E9", Int32.MinValue.ToString ("#E-0", _nfi), "#03");
			Assert.AreEqual ("-2e9", Int32.MinValue.ToString ("#e-0", _nfi), "#04");
			Assert.AreEqual ("-2E9", Int32.MinValue.ToString ("#E0", _nfi), "#05");
			Assert.AreEqual ("-2e9", Int32.MinValue.ToString ("#e0", _nfi), "#06");
		}

		[Test]
		public void Test08053 ()
		{
			Assert.AreEqual ("-2147483648E+0", Int32.MinValue.ToString ("0000000000E+0", _nfi), "#01");
			Assert.AreEqual ("-2147483648e+0", Int32.MinValue.ToString ("0000000000e+0", _nfi), "#02");
			Assert.AreEqual ("-2147483648E0", Int32.MinValue.ToString ("0000000000E-0", _nfi), "#03");
			Assert.AreEqual ("-2147483648e0", Int32.MinValue.ToString ("0000000000e-0", _nfi), "#04");
			Assert.AreEqual ("-2147483648E0", Int32.MinValue.ToString ("0000000000E0", _nfi), "#05");
			Assert.AreEqual ("-2147483648e0", Int32.MinValue.ToString ("0000000000e0", _nfi), "#06");
		}

		[Test]
		public void Test08054 ()
		{
			Assert.AreEqual ("-21474836480E-1", Int32.MinValue.ToString ("00000000000E+0", _nfi), "#01");
			Assert.AreEqual ("-21474836480e-1", Int32.MinValue.ToString ("00000000000e+0", _nfi), "#02");
			Assert.AreEqual ("-21474836480E-1", Int32.MinValue.ToString ("00000000000E-0", _nfi), "#03");
			Assert.AreEqual ("-21474836480e-1", Int32.MinValue.ToString ("00000000000e-0", _nfi), "#04");
			Assert.AreEqual ("-21474836480E-1", Int32.MinValue.ToString ("00000000000E0", _nfi), "#05");
			Assert.AreEqual ("-21474836480e-1", Int32.MinValue.ToString ("00000000000e0", _nfi), "#06");
		}

		[Test]
		public void Test08055 ()
		{
			Assert.AreEqual ("-214748365E+1", Int32.MinValue.ToString ("000000000E+0", _nfi), "#01");
			Assert.AreEqual ("-214748365e+1", Int32.MinValue.ToString ("000000000e+0", _nfi), "#02");
			Assert.AreEqual ("-214748365E1", Int32.MinValue.ToString ("000000000E-0", _nfi), "#03");
			Assert.AreEqual ("-214748365e1", Int32.MinValue.ToString ("000000000e-0", _nfi), "#04");
			Assert.AreEqual ("-214748365E1", Int32.MinValue.ToString ("000000000E0", _nfi), "#05");
			Assert.AreEqual ("-214748365e1", Int32.MinValue.ToString ("000000000e0", _nfi), "#06");
		}

		[Test]
		public void Test08056 ()
		{
			Assert.AreEqual ("-21474836E+2", Int32.MinValue.ToString ("00000000E+0", _nfi), "#01");
			Assert.AreEqual ("-21474836e+2", Int32.MinValue.ToString ("00000000e+0", _nfi), "#02");
			Assert.AreEqual ("-21474836E2", Int32.MinValue.ToString ("00000000E-0", _nfi), "#03");
			Assert.AreEqual ("-21474836e2", Int32.MinValue.ToString ("00000000e-0", _nfi), "#04");
			Assert.AreEqual ("-21474836E2", Int32.MinValue.ToString ("00000000E0", _nfi), "#05");
			Assert.AreEqual ("-21474836e2", Int32.MinValue.ToString ("00000000e0", _nfi), "#06");
		}

		[Test]
		public void Test08057 ()
		{
			Assert.AreEqual ("-2147483648E+00", Int32.MinValue.ToString ("0000000000E+00", _nfi), "#01");
			Assert.AreEqual ("-2147483648e+00", Int32.MinValue.ToString ("0000000000e+00", _nfi), "#02");
			Assert.AreEqual ("-2147483648E00", Int32.MinValue.ToString ("0000000000E-00", _nfi), "#03");
			Assert.AreEqual ("-2147483648e00", Int32.MinValue.ToString ("0000000000e-00", _nfi), "#04");
			Assert.AreEqual ("-2147483648E00", Int32.MinValue.ToString ("0000000000E00", _nfi), "#05");
			Assert.AreEqual ("-2147483648e00", Int32.MinValue.ToString ("0000000000e00", _nfi), "#06");
		}

		[Test]
		public void Test08058 ()
		{
			Assert.AreEqual ("-2147483648E+02%", Int32.MinValue.ToString ("0000000000E+00%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e+02%", Int32.MinValue.ToString ("0000000000e+00%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E02%", Int32.MinValue.ToString ("0000000000E-00%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e02%", Int32.MinValue.ToString ("0000000000e-00%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E02%", Int32.MinValue.ToString ("0000000000E00%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e02%", Int32.MinValue.ToString ("0000000000e00%", _nfi), "#06");
		}

		[Test]
		public void Test08059 ()
		{
			Assert.AreEqual ("-2147483648E+10%%%%%", Int32.MinValue.ToString ("0000000000E+00%%%%%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e+10%%%%%", Int32.MinValue.ToString ("0000000000e+00%%%%%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E10%%%%%", Int32.MinValue.ToString ("0000000000E-00%%%%%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e10%%%%%", Int32.MinValue.ToString ("0000000000e-00%%%%%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E10%%%%%", Int32.MinValue.ToString ("0000000000E00%%%%%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e10%%%%%", Int32.MinValue.ToString ("0000000000e00%%%%%", _nfi), "#06");
		}

		[Test]
		public void Test08060 ()
		{
			Assert.AreEqual ("-2147483648E-03", Int32.MinValue.ToString ("0000000000E+00,", _nfi), "#01");
			Assert.AreEqual ("-2147483648e-03", Int32.MinValue.ToString ("0000000000e+00,", _nfi), "#02");
			Assert.AreEqual ("-2147483648E-03", Int32.MinValue.ToString ("0000000000E-00,", _nfi), "#03");
			Assert.AreEqual ("-2147483648e-03", Int32.MinValue.ToString ("0000000000e-00,", _nfi), "#04");
			Assert.AreEqual ("-2147483648E-03", Int32.MinValue.ToString ("0000000000E00,", _nfi), "#05");
			Assert.AreEqual ("-2147483648e-03", Int32.MinValue.ToString ("0000000000e00,", _nfi), "#06");
		}

		[Test]
		public void Test08061 ()
		{
			Assert.AreEqual ("-2147483648E-12", Int32.MinValue.ToString ("0000000000E+00,,,,", _nfi), "#01");
			Assert.AreEqual ("-2147483648e-12", Int32.MinValue.ToString ("0000000000e+00,,,,", _nfi), "#02");
			Assert.AreEqual ("-2147483648E-12", Int32.MinValue.ToString ("0000000000E-00,,,,", _nfi), "#03");
			Assert.AreEqual ("-2147483648e-12", Int32.MinValue.ToString ("0000000000e-00,,,,", _nfi), "#04");
			Assert.AreEqual ("-2147483648E-12", Int32.MinValue.ToString ("0000000000E00,,,,", _nfi), "#05");
			Assert.AreEqual ("-2147483648e-12", Int32.MinValue.ToString ("0000000000e00,,,,", _nfi), "#06");
		}

		[Test]
		public void Test08062 ()
		{
			Assert.AreEqual ("-2147483648E-04%%%%", Int32.MinValue.ToString ("0000000000E+00,,,,%%%%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e-04%%%%", Int32.MinValue.ToString ("0000000000e+00,,,,%%%%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E-04%%%%", Int32.MinValue.ToString ("0000000000E-00,,,,%%%%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e-04%%%%", Int32.MinValue.ToString ("0000000000e-00,,,,%%%%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E-04%%%%", Int32.MinValue.ToString ("0000000000E00,,,,%%%%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e-04%%%%", Int32.MinValue.ToString ("0000000000e00,,,,%%%%", _nfi), "#06");
		}

		[Test]
		public void Test08063 ()
		{
			Assert.AreEqual ("-2147483648E-07%%%%", Int32.MinValue.ToString ("0000000000,E+00,,,,%%%%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e-07%%%%", Int32.MinValue.ToString ("0000000000,e+00,,,,%%%%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E-07%%%%", Int32.MinValue.ToString ("0000000000,E-00,,,,%%%%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e-07%%%%", Int32.MinValue.ToString ("0000000000,e-00,,,,%%%%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E-07%%%%", Int32.MinValue.ToString ("0000000000,E00,,,,%%%%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e-07%%%%", Int32.MinValue.ToString ("0000000000,e00,,,,%%%%", _nfi), "#06");
		}

		[Test]
		public void Test08064 ()
		{
			Assert.AreEqual ("-000,000,214,7E+48%%%%", Int32.MinValue.ToString ("0000000000,E,+00,,,,%%%%", _nfi), "#01");
			Assert.AreEqual ("-000,000,214,7e+48%%%%", Int32.MinValue.ToString ("0000000000,e,+00,,,,%%%%", _nfi), "#02");
			Assert.AreEqual ("-000,000,214,7E-48%%%%", Int32.MinValue.ToString ("0000000000,E,-00,,,,%%%%", _nfi), "#03");
			Assert.AreEqual ("-000,000,214,7e-48%%%%", Int32.MinValue.ToString ("0000000000,e,-00,,,,%%%%", _nfi), "#04");
			Assert.AreEqual ("-000,000,214,7E48%%%%", Int32.MinValue.ToString ("0000000000,E,00,,,,%%%%", _nfi), "#05");
			Assert.AreEqual ("-000,000,214,7e48%%%%", Int32.MinValue.ToString ("0000000000,e,00,,,,%%%%", _nfi), "#06");
		}

		[Test]
		public void Test08065 ()
		{
			Assert.AreEqual ("-000,000,214,7E+48%%%%", Int32.MinValue.ToString ("0000000000,E+,00,,,,%%%%", _nfi), "#01");
			Assert.AreEqual ("-000,000,214,7e+48%%%%", Int32.MinValue.ToString ("0000000000,e+,00,,,,%%%%", _nfi), "#02");
			Assert.AreEqual ("-000,000,214,7E-48%%%%", Int32.MinValue.ToString ("0000000000,E-,00,,,,%%%%", _nfi), "#03");
			Assert.AreEqual ("-000,000,214,7e-48%%%%", Int32.MinValue.ToString ("0000000000,e-,00,,,,%%%%", _nfi), "#04");
		}

		[Test]
		public void Test08066 ()
		{
			Assert.AreEqual ("-21,474,836,48E-50%%%%", Int32.MinValue.ToString ("0000000000,E+0,0,,,,%%%%", _nfi), "#01");
			Assert.AreEqual ("-21,474,836,48e-50%%%%", Int32.MinValue.ToString ("0000000000,e+0,0,,,,%%%%", _nfi), "#02");
			Assert.AreEqual ("-21,474,836,48E-50%%%%", Int32.MinValue.ToString ("0000000000,E-0,0,,,,%%%%", _nfi), "#03");
			Assert.AreEqual ("-21,474,836,48e-50%%%%", Int32.MinValue.ToString ("0000000000,e-0,0,,,,%%%%", _nfi), "#04");
			Assert.AreEqual ("-21,474,836,48E-50%%%%", Int32.MinValue.ToString ("0000000000,E0,0,,,,%%%%", _nfi), "#05");
			Assert.AreEqual ("-21,474,836,48e-50%%%%", Int32.MinValue.ToString ("0000000000,e0,0,,,,%%%%", _nfi), "#06");
		}

		[Test]
		public void Test08067 ()
		{
			Assert.AreEqual ("-2147483648E-01,%%%%", Int32.MinValue.ToString (@"0000000000E+00\,,,,%%%%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e-01,%%%%", Int32.MinValue.ToString (@"0000000000e+00\,,,,%%%%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E-01,%%%%", Int32.MinValue.ToString (@"0000000000E-00\,,,,%%%%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e-01,%%%%", Int32.MinValue.ToString (@"0000000000e-00\,,,,%%%%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E-01,%%%%", Int32.MinValue.ToString (@"0000000000E00\,,,,%%%%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e-01,%%%%", Int32.MinValue.ToString (@"0000000000e00\,,,,%%%%", _nfi), "#06");
		}

		[Test]
		public void Test08068 ()
		{
			Assert.AreEqual ("-2147483648E+02,,%%%%", Int32.MinValue.ToString (@"0000000000E+00\,,,\,%%%%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e+02,,%%%%", Int32.MinValue.ToString (@"0000000000e+00\,,,\,%%%%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E02,,%%%%", Int32.MinValue.ToString (@"0000000000E-00\,,,\,%%%%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e02,,%%%%", Int32.MinValue.ToString (@"0000000000e-00\,,,\,%%%%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E02,,%%%%", Int32.MinValue.ToString (@"0000000000E00\,,,\,%%%%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e02,,%%%%", Int32.MinValue.ToString (@"0000000000e00\,,,\,%%%%", _nfi), "#06");
		}

		[Test]
		public void Test08069 ()
		{
			Assert.AreEqual ("-2147483648E+00,,%%%%", Int32.MinValue.ToString (@"0000000000E+00\,,,\,\%%%%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e+00,,%%%%", Int32.MinValue.ToString (@"0000000000e+00\,,,\,\%%%%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E00,,%%%%", Int32.MinValue.ToString (@"0000000000E-00\,,,\,\%%%%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e00,,%%%%", Int32.MinValue.ToString (@"0000000000e-00\,,,\,\%%%%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E00,,%%%%", Int32.MinValue.ToString (@"0000000000E00\,,,\,\%%%%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e00,,%%%%", Int32.MinValue.ToString (@"0000000000e00\,,,\,\%%%%", _nfi), "#06");
		}

		[Test]
		public void Test08070 ()
		{
			Assert.AreEqual ("-2147483648E-02,,%%%%", Int32.MinValue.ToString (@"0000000000E+00\,,,\,\%%%\%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e-02,,%%%%", Int32.MinValue.ToString (@"0000000000e+00\,,,\,\%%%\%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E-02,,%%%%", Int32.MinValue.ToString (@"0000000000E-00\,,,\,\%%%\%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e-02,,%%%%", Int32.MinValue.ToString (@"0000000000e-00\,,,\,\%%%\%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E-02,,%%%%", Int32.MinValue.ToString (@"0000000000E00\,,,\,\%%%\%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e-02,,%%%%", Int32.MinValue.ToString (@"0000000000e00\,,,\,\%%%\%", _nfi), "#06");
		}

		[Test]
		public void Test08071 ()
		{
			Assert.AreEqual (@"-2147483648E-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000E+00\\,,,\\,\\%%%\\%", _nfi), "#01");
			Assert.AreEqual (@"-2147483648e-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000e+00\\,,,\\,\\%%%\\%", _nfi), "#02");
			Assert.AreEqual (@"-2147483648E-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000E-00\\,,,\\,\\%%%\\%", _nfi), "#03");
			Assert.AreEqual (@"-2147483648e-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000e-00\\,,,\\,\\%%%\\%", _nfi), "#04");
			Assert.AreEqual (@"-2147483648E-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000E00\\,,,\\,\\%%%\\%", _nfi), "#05");
			Assert.AreEqual (@"-2147483648e-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000e00\\,,,\\,\\%%%\\%", _nfi), "#06");
		}

		[Test]
		public void Test08072 ()
		{
			Assert.AreEqual (@"-2147483648E+00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E+00\\,\,,\\\,\\%%%\\\%", _nfi), "#01");
			Assert.AreEqual (@"-2147483648e+00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e+00\\,\,,\\\,\\%%%\\\%", _nfi), "#02");
			Assert.AreEqual (@"-2147483648E00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E-00\\,\,,\\\,\\%%%\\\%", _nfi), "#03");
			Assert.AreEqual (@"-2147483648e00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e-00\\,\,,\\\,\\%%%\\\%", _nfi), "#04");
			Assert.AreEqual (@"-2147483648E00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E00\\,\,,\\\,\\%%%\\\%", _nfi), "#05");
			Assert.AreEqual (@"-2147483648e00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e00\\,\,,\\\,\\%%%\\\%", _nfi), "#06");
		}

		[Test]
		public void Test08073 ()
		{
			Assert.AreEqual (@"-0021474836E+48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\E+00\\,\,,\\\,\\%%%\\\%", _nfi), "#01");
			Assert.AreEqual (@"-0021474836e+48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\e+00\\,\,,\\\,\\%%%\\\%", _nfi), "#02");
			Assert.AreEqual (@"-0021474836E-48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\E-00\\,\,,\\\,\\%%%\\\%", _nfi), "#03");
			Assert.AreEqual (@"-0021474836e-48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\e-00\\,\,,\\\,\\%%%\\\%", _nfi), "#04");
			Assert.AreEqual (@"-0021474836E48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\E00\\,\,,\\\,\\%%%\\\%", _nfi), "#05");
			Assert.AreEqual (@"-0021474836e48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\e00\\,\,,\\\,\\%%%\\\%", _nfi), "#06");
		}

		[Test]
		public void Test08074 ()
		{
			Assert.AreEqual (@"-0021474836E+48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E\+00\\,\,,\\\,\\%%%\\\%", _nfi), "#01");
			Assert.AreEqual (@"-0021474836e+48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e\+00\\,\,,\\\,\\%%%\\\%", _nfi), "#02");
			Assert.AreEqual (@"-0021474836E-48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E\-00\\,\,,\\\,\\%%%\\\%", _nfi), "#03");
			Assert.AreEqual (@"-0021474836e-48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e\-00\\,\,,\\\,\\%%%\\\%", _nfi), "#04");
		}

		[Test]
		public void Test08075 ()
		{
			Assert.AreEqual ("-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E+00,,,',%'%%%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e+00,,,',%'%%%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E-00,,,',%'%%%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e-00,,,',%'%%%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E00,,,',%'%%%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e00,,,',%'%%%", _nfi), "#06");
		}

		[Test]
		public void Test08076 ()
		{
			Assert.AreEqual ("-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E+00,,,\",%\"%%%", _nfi), "#01");
			Assert.AreEqual ("-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e+00,,,\",%\"%%%", _nfi), "#02");
			Assert.AreEqual ("-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E-00,,,\",%\"%%%", _nfi), "#03");
			Assert.AreEqual ("-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e-00,,,\",%\"%%%", _nfi), "#04");
			Assert.AreEqual ("-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E00,,,\",%\"%%%", _nfi), "#05");
			Assert.AreEqual ("-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e00,,,\",%\"%%%", _nfi), "#06");
		}

		[Test]
		public void Test08077 ()
		{
			Assert.AreEqual ("-", Int32.MinValue.ToString (";", _nfi), "#01");
			Assert.AreEqual ("", Int32.MaxValue.ToString (";", _nfi), "#02");
			Assert.AreEqual ("",0.ToString (";", _nfi), "#03");
		}

		[Test]
		public void Test08078 ()
		{
			Assert.AreEqual ("-2,147,483,648", Int32.MinValue.ToString ("#,#;", _nfi), "#01");
			Assert.AreEqual ("2,147,483,647", Int32.MaxValue.ToString ("#,#;", _nfi), "#02");
			Assert.AreEqual ("", 0.ToString ("#,#;", _nfi), "#03");
		}

		[Test]
		public void Test08079 ()
		{
			Assert.AreEqual ("2,147,483,648", Int32.MinValue.ToString (";#,#", _nfi), "#01");
			Assert.AreEqual ("", Int32.MaxValue.ToString (";#,#", _nfi), "#02");
			Assert.AreEqual ("", 0.ToString (";#,#", _nfi), "#03");
		}

		[Test]
		public void Test08080 ()
		{
			Assert.AreEqual ("2,147,483,648", Int32.MinValue.ToString ("0000000000,.0000000000;#,#", _nfi), "#01");
			Assert.AreEqual ("0002147483.6470000000", Int32.MaxValue.ToString ("0000000000,.0000000000;#,#", _nfi), "#02");
			Assert.AreEqual ("0000000000.0000000000", 0.ToString ("0000000000,.0000000000;#,#", _nfi), "#03");
		}

		[Test]
		public void Test08081 ()
		{
			Assert.AreEqual ("-", Int32.MinValue.ToString (";;", _nfi), "#01");
			Assert.AreEqual ("", Int32.MaxValue.ToString (";;", _nfi), "#02");
			Assert.AreEqual ("",0.ToString (";;", _nfi), "#03");
		}

		[Test]
		public void Test08082 ()
		{
			Assert.AreEqual ("-", Int32.MinValue.ToString (";;0%", _nfi), "#01");
			Assert.AreEqual ("", Int32.MaxValue.ToString (";;0%", _nfi), "#02");
			Assert.AreEqual ("0%",0.ToString (";;0%", _nfi), "#03");
		}

		[Test]
		public void Test08083 ()
		{
			Assert.AreEqual ("2147484", Int32.MinValue.ToString (";0,;0%", _nfi), "#01");
			Assert.AreEqual ("", Int32.MaxValue.ToString (";0,;0%", _nfi), "#02");
			Assert.AreEqual ("0%",0.ToString (";0,;0%", _nfi), "#03");
		}

		[Test]
		public void Test08084 ()
		{
			Assert.AreEqual ("2147484", Int32.MinValue.ToString ("0E+0;0,;0%", _nfi), "#01");
			Assert.AreEqual ("2E+9", Int32.MaxValue.ToString ("0E+0;0,;0%", _nfi), "#02");
			Assert.AreEqual ("0%",0.ToString ("0E+0;0,;0%", _nfi), "#03");
		}

		[Test]
		public void Test08085 ()
		{
			Assert.AreEqual ("214,748,364,80;0%", Int32.MinValue.ToString (@"0E+0;0,\;0%", _nfi), "#01");
			Assert.AreEqual ("2E+9", Int32.MaxValue.ToString (@"0E+0;0,\;0%", _nfi), "#02");
			Assert.AreEqual ("0E+0",0.ToString (@"0E+0;0,\;0%", _nfi), "#03");
		}

		[Test]
		public void Test08086 ()
		{
			Assert.AreEqual ("214,748,364,80;0%", Int32.MinValue.ToString ("0E+0;0,\";\"0%", _nfi), "#01");
			Assert.AreEqual ("2E+9", Int32.MaxValue.ToString ("0E+0;0,\";\"0%", _nfi), "#02");
			Assert.AreEqual ("0E+0",0.ToString ("0E+0;0,\";\"0%", _nfi), "#03");
		}

		[Test]
		public void Test08087 ()
		{
			// MS.NET bug?
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "$$$";
			Assert.AreEqual ("-0000000000$$$2147483648", Int32.MinValue.ToString ("0000000000$$$0000000000", nfi), "#01");
		}

		[Test]
		public void Test08088 ()
		{
			// MS.NET bug?
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo.Clone() as NumberFormatInfo;
			nfi.NumberGroupSeparator = "$$$";
			Assert.AreEqual ("-0000000000$$$2147483648", Int32.MinValue.ToString ("0000000000$$$0000000000", nfi), "#01");
		}

		[Test]
		public void Test08089 ()
		{
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo.Clone() as NumberFormatInfo;
			nfi.NumberGroupSizes = new int[] {3,2,1,0};
			Assert.AreEqual ("-00000000002147,4,83,648", Int32.MinValue.ToString ("0000000000,0000000000", nfi), "#01");
		}

		[Test]
		public void Test08090 ()
		{
			// MS.NET bug?
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo.Clone() as NumberFormatInfo;
			nfi.PercentSymbol = "$$$";
			Assert.AreEqual ("-0000000000$$$2147483648", Int32.MinValue.ToString ("0000000000$$$0000000000", nfi), "#01");
		}

		[Test]
		public void Test08091 ()
		{
			// MS.NET bug?
			Assert.AreEqual ("B2147", Int32.MinValue.ToString ("A0,;B0,,;C0,,,;D0,,,,;E0,,,,,", _nfi), "#01");
			Assert.AreEqual ("A2147484", Int32.MaxValue.ToString ("A0,;B0,,;C0,,,;D0,,,,;E0,,,,,", _nfi), "#02");
			Assert.AreEqual ("C0", 0.ToString ("A0,;B0,,;C0,,,;D0,,,,;E0,,,,,", _nfi), "#03");
		}

		// Test10000- Double and D
		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Test10000 ()
		{
			Assert.AreEqual ("0", 0.0.ToString ("D", _nfi), "#01");
		}

		// Test11000- Double and E
		[Test]
		public void Test11000 ()
		{
			Assert.AreEqual ("0.000000E+000", 0.0.ToString ("E", _nfi), "#01");
			Assert.AreEqual ("0.000000e+000", 0.0.ToString ("e", _nfi), "#02");
			Assert.AreEqual ("-1.797693E+308", Double.MinValue.ToString ("E", _nfi), "#03");
			Assert.AreEqual ("-1.797693e+308", Double.MinValue.ToString ("e", _nfi), "#04");
			Assert.AreEqual ("1.797693E+308", Double.MaxValue.ToString ("E", _nfi), "#05");
			Assert.AreEqual ("1.797693e+308", Double.MaxValue.ToString ("e", _nfi), "#06");
		}

		[Test]
		public void Test11001 ()
		{
			Assert.AreEqual ("E ", 0.0.ToString ("E ", _nfi), "#01");
			Assert.AreEqual (" E", 0.0.ToString (" E", _nfi), "#02");
			Assert.AreEqual (" E ", 0.0.ToString (" E ", _nfi), "#03");
		}

		[Test]
		public void Test11002 ()
		{
			Assert.AreEqual ("-E ", (-1.0).ToString ("E ", _nfi), "#01");
			Assert.AreEqual ("- E", (-1.0).ToString (" E", _nfi), "#02");
			Assert.AreEqual ("- E ", (-1.0).ToString (" E ", _nfi), "#03");
		}

		[Test]
		public void Test11003 ()
		{
			Assert.AreEqual ("0E+000", 0.0.ToString ("E0", _nfi), "#01");
			Assert.AreEqual ("0.0000000000000000E+000", 0.0.ToString ("E16", _nfi), "#02");
			Assert.AreEqual ("0.00000000000000000E+000", 0.0.ToString ("E17", _nfi), "#03");
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000E+000", 0.0.ToString ("E99", _nfi), "#04");
			Assert.AreEqual ("E100", 0.0.ToString ("E100", _nfi), "#05");
		}

		[Test]
		public void Test11004 ()
		{
			Assert.AreEqual ("2E+308", Double.MaxValue.ToString ("E0", _nfi), "#01");
			Assert.AreEqual ("1.7976931348623157E+308", Double.MaxValue.ToString ("E16", _nfi), "#02");
			Assert.AreEqual ("1.79769313486231570E+308", Double.MaxValue.ToString ("E17", _nfi), "#03");
			Assert.AreEqual ("1.797693134862315700000000000000000000000000000000000000000000000000000000000000000000000000000000000E+308", Double.MaxValue.ToString ("E99", _nfi), "#04");
			Assert.AreEqual ("E1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("E100", _nfi), "#05");
		}

		[Test]
		public void Test11005 ()
		{
			Assert.AreEqual ("-2E+308", Double.MinValue.ToString ("E0", _nfi), "#01");
			Assert.AreEqual ("-1.7976931348623157E+308", Double.MinValue.ToString ("E16", _nfi), "#02");
			Assert.AreEqual ("-1.79769313486231570E+308", Double.MinValue.ToString ("E17", _nfi), "#03");
			Assert.AreEqual ("-1.797693134862315700000000000000000000000000000000000000000000000000000000000000000000000000000000000E+308", Double.MinValue.ToString ("E99", _nfi), "#04");
			Assert.AreEqual ("-E1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("E100", _nfi), "#05");
		}

		[Test]
		public void Test11006 ()
		{
			Assert.AreEqual ("EF", 0.0.ToString ("EF", _nfi), "#01");
			Assert.AreEqual ("E0F", 0.0.ToString ("E0F", _nfi), "#02");
			Assert.AreEqual ("E0xF", 0.0.ToString ("E0xF", _nfi), "#03");
		}

		[Test]
		public void Test11007 ()
		{
			Assert.AreEqual ("EF", Double.MaxValue.ToString ("EF", _nfi), "#01");
			Assert.AreEqual ("E0F", Double.MaxValue.ToString ("E0F", _nfi), "#02");
			Assert.AreEqual ("E0xF", Double.MaxValue.ToString ("E0xF", _nfi), "#03");
		}

		[Test]
		public void Test11008 ()
		{
			Assert.AreEqual ("-EF", Double.MinValue.ToString ("EF", _nfi), "#01");
			Assert.AreEqual ("E0F", Double.MinValue.ToString ("E0F", _nfi), "#02");
			Assert.AreEqual ("E0xF", Double.MinValue.ToString ("E0xF", _nfi), "#03");
		}

		[Test]
		public void Test11009 ()
		{
			Assert.AreEqual ("0.00000000000000000E+000", 0.0.ToString ("E0000000000000000000000000000000000000017", _nfi), "#01");
			Assert.AreEqual ("1.79769313486231570E+308", Double.MaxValue.ToString ("E0000000000000000000000000000000000000017", _nfi), "#02");
			Assert.AreEqual ("-1.79769313486231570E+308", Double.MinValue.ToString ("E0000000000000000000000000000000000000017", _nfi), "#03");
		}

		[Test]
		public void Test11010 ()
		{
			Assert.AreEqual ("+E", 0.0.ToString ("+E", _nfi), "#01");
			Assert.AreEqual ("E+", 0.0.ToString ("E+", _nfi), "#02");
			Assert.AreEqual ("+E+", 0.0.ToString ("+E+", _nfi), "#03");
		}
		
		[Test]
		public void Test11011 ()
		{
			Assert.AreEqual ("+E", Double.MaxValue.ToString ("+E", _nfi), "#01");
			Assert.AreEqual ("E+", Double.MaxValue.ToString ("E+", _nfi), "#02");
			Assert.AreEqual ("+E+", Double.MaxValue.ToString ("+E+", _nfi), "#03");
		}

		[Test]
		public void Test11012 ()
		{
			Assert.AreEqual ("-+E", Double.MinValue.ToString ("+E", _nfi), "#01");
			Assert.AreEqual ("-E+", Double.MinValue.ToString ("E+", _nfi), "#02");
			Assert.AreEqual ("-+E+", Double.MinValue.ToString ("+E+", _nfi), "#03");
		}

		[Test]
		public void Test11013 ()
		{
			Assert.AreEqual ("-E", 0.0.ToString ("-E", _nfi), "#01");
			Assert.AreEqual ("E-", 0.0.ToString ("E-", _nfi), "#02");
			Assert.AreEqual ("-E-", 0.0.ToString ("-E-", _nfi), "#03");
		}
		
		[Test]
		public void Test11014 ()
		{
			Assert.AreEqual ("-E", Double.MaxValue.ToString ("-E", _nfi), "#01");
			Assert.AreEqual ("E-", Double.MaxValue.ToString ("E-", _nfi), "#02");
			Assert.AreEqual ("-E-", Double.MaxValue.ToString ("-E-", _nfi), "#03");
		}

		[Test]
		public void Test11015 ()
		{
			Assert.AreEqual ("--E", Double.MinValue.ToString ("-E", _nfi), "#01");
			Assert.AreEqual ("-E-", Double.MinValue.ToString ("E-", _nfi), "#02");
			Assert.AreEqual ("--E-", Double.MinValue.ToString ("-E-", _nfi), "#03");
		}

		[Test]
		public void Test11016 ()
		{
			Assert.AreEqual ("E+0", 0.0.ToString ("E+0", _nfi), "#01");
			Assert.AreEqual ("E+0", Double.MaxValue.ToString ("E+0", _nfi), "#02");
			Assert.AreEqual ("E+0", Double.MinValue.ToString ("E+0", _nfi), "#03");
		}

		[Test]
		public void Test11017 ()
		{
			Assert.AreEqual ("E+9", 0.0.ToString ("E+9", _nfi), "#01");
			Assert.AreEqual ("E+9", Double.MaxValue.ToString ("E+9", _nfi), "#02");
			Assert.AreEqual ("-E+9", Double.MinValue.ToString ("E+9", _nfi), "#03");
		}

		[Test]
		public void Test11018 ()
		{
			Assert.AreEqual ("E-9", 0.0.ToString ("E-9", _nfi), "#01");
			Assert.AreEqual ("E-9", Double.MaxValue.ToString ("E-9", _nfi), "#02");
			Assert.AreEqual ("-E-9", Double.MinValue.ToString ("E-9", _nfi), "#03");
		}

		[Test]
		public void Test11019 ()
		{
			Assert.AreEqual ("E0", 0.0.ToString ("E0,", _nfi), "#01");
			Assert.AreEqual ("E0", Double.MaxValue.ToString ("E0,", _nfi), "#02");
			Assert.AreEqual ("E0", Double.MinValue.ToString ("E0,", _nfi), "#03");
		}

		[Test]
		public void Test11020 ()
		{
			Assert.AreEqual ("E0", 0.0.ToString ("E0.", _nfi), "#01");
			Assert.AreEqual ("E0", Double.MaxValue.ToString ("E0.", _nfi), "#02");
			Assert.AreEqual ("E0", Double.MinValue.ToString ("E0.", _nfi), "#03");
		}

		[Test]
		public void Test11021 ()
		{
			Assert.AreEqual ("E0.0", 0.0.ToString ("E0.0", _nfi), "#01");
			Assert.AreEqual ("E309.2", Double.MaxValue.ToString ("E0.0", _nfi), "#02");
			Assert.AreEqual ("-E309.2", Double.MinValue.ToString ("E0.0", _nfi), "#03");
		}

		[Test]
		public void Test11022 ()
		{
			Assert.AreEqual ("E09", 0.0.ToString ("E0.9", _nfi), "#01");
			Assert.AreEqual ("E09", Double.MaxValue.ToString ("E0.9", _nfi), "#02");
			Assert.AreEqual ("E09", Double.MinValue.ToString ("E0.9", _nfi), "#03");
		}

		[Test]
		public void Test11023 ()
		{
			Assert.AreEqual ("1.1E+000", 1.05.ToString ("E1", _nfi), "#01");
			Assert.AreEqual ("1.2E+000", 1.15.ToString ("E1", _nfi), "#02");
			Assert.AreEqual ("1.3E+000", 1.25.ToString ("E1", _nfi), "#03");
			Assert.AreEqual ("1.4E+000", 1.35.ToString ("E1", _nfi), "#04");
			Assert.AreEqual ("1.5E+000", 1.45.ToString ("E1", _nfi), "#05");
			Assert.AreEqual ("1.6E+000", 1.55.ToString ("E1", _nfi), "#06");
			Assert.AreEqual ("1.7E+000", 1.65.ToString ("E1", _nfi), "#07");
			Assert.AreEqual ("1.8E+000", 1.75.ToString ("E1", _nfi), "#08");
			Assert.AreEqual ("1.9E+000", 1.85.ToString ("E1", _nfi), "#09");
			Assert.AreEqual ("2.0E+000", 1.95.ToString ("E1", _nfi), "#10");
		}

		[Test]
		public void Test11024 ()
		{
			Assert.AreEqual ("1.01E+000", 1.005.ToString ("E2", _nfi), "#01");
			Assert.AreEqual ("1.02E+000", 1.015.ToString ("E2", _nfi), "#02");
			Assert.AreEqual ("1.03E+000", 1.025.ToString ("E2", _nfi), "#03");
			Assert.AreEqual ("1.04E+000", 1.035.ToString ("E2", _nfi), "#04");
			Assert.AreEqual ("1.05E+000", 1.045.ToString ("E2", _nfi), "#05");
			Assert.AreEqual ("1.06E+000", 1.055.ToString ("E2", _nfi), "#06");
			Assert.AreEqual ("1.07E+000", 1.065.ToString ("E2", _nfi), "#07");
			Assert.AreEqual ("1.08E+000", 1.075.ToString ("E2", _nfi), "#08");
			Assert.AreEqual ("1.09E+000", 1.085.ToString ("E2", _nfi), "#09");
			Assert.AreEqual ("1.10E+000", 1.095.ToString ("E2", _nfi), "#10");
		}

		[Test]
		public void Test11025 ()
		{
			Assert.AreEqual ("1.00000000000001E+000", 1.000000000000005.ToString ("E14", _nfi), "#01");
			Assert.AreEqual ("1.00000000000002E+000", 1.000000000000015.ToString ("E14", _nfi), "#02");
			Assert.AreEqual ("1.00000000000003E+000", 1.000000000000025.ToString ("E14", _nfi), "#03");
			Assert.AreEqual ("1.00000000000004E+000", 1.000000000000035.ToString ("E14", _nfi), "#04");
			Assert.AreEqual ("1.00000000000005E+000", 1.000000000000045.ToString ("E14", _nfi), "#05");
			Assert.AreEqual ("1.00000000000006E+000", 1.000000000000055.ToString ("E14", _nfi), "#06");
			Assert.AreEqual ("1.00000000000007E+000", 1.000000000000065.ToString ("E14", _nfi), "#07");
			Assert.AreEqual ("1.00000000000008E+000", 1.000000000000075.ToString ("E14", _nfi), "#08");
			Assert.AreEqual ("1.00000000000009E+000", 1.000000000000085.ToString ("E14", _nfi), "#09");
			Assert.AreEqual ("1.00000000000010E+000", 1.000000000000095.ToString ("E14", _nfi), "#10");
		}

		[Test]
		public void Test11026 ()
		{
			Assert.AreEqual ("1.000000000000000E+000", 1.0000000000000005.ToString ("E15", _nfi), "#01");
			Assert.AreEqual ("1.000000000000002E+000", 1.0000000000000015.ToString ("E15", _nfi), "#02");
			Assert.AreEqual ("1.000000000000002E+000", 1.0000000000000025.ToString ("E15", _nfi), "#03");
			Assert.AreEqual ("1.000000000000004E+000", 1.0000000000000035.ToString ("E15", _nfi), "#04");
			Assert.AreEqual ("1.000000000000004E+000", 1.0000000000000045.ToString ("E15", _nfi), "#05");
			Assert.AreEqual ("1.000000000000006E+000", 1.0000000000000055.ToString ("E15", _nfi), "#06");
			Assert.AreEqual ("1.000000000000006E+000", 1.0000000000000065.ToString ("E15", _nfi), "#07");
			Assert.AreEqual ("1.000000000000008E+000", 1.0000000000000075.ToString ("E15", _nfi), "#08");
			Assert.AreEqual ("1.000000000000008E+000", 1.0000000000000085.ToString ("E15", _nfi), "#09");
			Assert.AreEqual ("1.000000000000010E+000", 1.0000000000000095.ToString ("E15", _nfi), "#10");
		}

		[Test]
		public void Test11027 ()
		{
			Assert.AreEqual ("1.0000000000000000E+000", 1.00000000000000005.ToString ("E16", _nfi), "#01");
			Assert.AreEqual ("1.0000000000000002E+000", 1.00000000000000015.ToString ("E16", _nfi), "#02");
			Assert.AreEqual ("1.0000000000000002E+000", 1.00000000000000025.ToString ("E16", _nfi), "#03");
			Assert.AreEqual ("1.0000000000000004E+000", 1.00000000000000035.ToString ("E16", _nfi), "#04");
			Assert.AreEqual ("1.0000000000000004E+000", 1.00000000000000045.ToString ("E16", _nfi), "#05");
			Assert.AreEqual ("1.0000000000000004E+000", 1.00000000000000055.ToString ("E16", _nfi), "#06");
			Assert.AreEqual ("1.0000000000000007E+000", 1.00000000000000065.ToString ("E16", _nfi), "#07");
			Assert.AreEqual ("1.0000000000000007E+000", 1.00000000000000075.ToString ("E16", _nfi), "#08");
			Assert.AreEqual ("1.0000000000000009E+000", 1.00000000000000085.ToString ("E16", _nfi), "#09");
			Assert.AreEqual ("1.0000000000000009E+000", 1.00000000000000095.ToString ("E16", _nfi), "#10");
		}

		[Test]
		public void Test11028 ()
		{
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000005.ToString ("E17", _nfi), "#01");
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000015.ToString ("E17", _nfi), "#02");
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000025.ToString ("E17", _nfi), "#03");
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000035.ToString ("E17", _nfi), "#04");
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000045.ToString ("E17", _nfi), "#05");
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000055.ToString ("E17", _nfi), "#06");
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000065.ToString ("E17", _nfi), "#07");
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000075.ToString ("E17", _nfi), "#08");
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000085.ToString ("E17", _nfi), "#09");
			Assert.AreEqual ("1.00000000000000000E+000", 1.000000000000000095.ToString ("E17", _nfi), "#10");
		}

		[Test]
		public void Test11029 ()
		{
			Assert.AreEqual ("1E+000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E0"), "#01");
			Assert.AreEqual ("1.2345678901234567E+000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E16"), "#02");
			Assert.AreEqual ("1.23456789012345670E+000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E17"), "#03");
			Assert.AreEqual ("1.234567890123456700000000000000000000000000000000000000000000000000000000000000000000000000000000000E+000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E99"), "#04");
			Assert.AreEqual ("E101", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E100"), "#04");
		}

		[Test]
		public void Test11030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			Assert.AreEqual ("-1#000000E+008", (-99999999.9).ToString ("E", nfi), "#01");
		}

		[Test]
		public void Test11031 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			Assert.AreEqual ("1.000000E-000", 1.0.ToString ("E", nfi), "#01");
			Assert.AreEqual ("0.000000E-000", 0.0.ToString ("E", nfi), "#02");
			Assert.AreEqual ("+1.000000E-000", (-1.0).ToString ("E", nfi), "#03");
		}

		[Test]
		public void TestNaNToString ()
		{
			var nfi = CultureInfo.CurrentCulture.NumberFormat;
			Assert.AreEqual (nfi.PositiveInfinitySymbol, Double.PositiveInfinity.ToString(), "#01");
			Assert.AreEqual (nfi.NegativeInfinitySymbol, Double.NegativeInfinity.ToString(), "#02");
			Assert.AreEqual (nfi.NaNSymbol, Double.NaN.ToString(), "#03");
			Assert.AreEqual (nfi.PositiveInfinitySymbol, Single.PositiveInfinity.ToString(), "#04");
			Assert.AreEqual (nfi.NegativeInfinitySymbol, Single.NegativeInfinity.ToString(), "#05");
			Assert.AreEqual (nfi.NaNSymbol, Single.NaN.ToString(), "#06");

			Assert.AreEqual (nfi.PositiveInfinitySymbol, Double.PositiveInfinity.ToString("R"), "#07");
			Assert.AreEqual (nfi.NegativeInfinitySymbol, Double.NegativeInfinity.ToString("R"), "#08");
			Assert.AreEqual (nfi.NaNSymbol, Double.NaN.ToString("R"), "#09");
			Assert.AreEqual (nfi.PositiveInfinitySymbol, Single.PositiveInfinity.ToString("R"), "#10");
			Assert.AreEqual (nfi.NegativeInfinitySymbol, Single.NegativeInfinity.ToString("R"), "#11");
			Assert.AreEqual (nfi.NaNSymbol, Single.NaN.ToString("R"), "#12");
		}

		[Test]
		public void Test11032 ()
		{
			Assert.AreEqual ("Infinity", (Double.MaxValue / 0.0).ToString ("E99", _nfi) , "#01");
			Assert.AreEqual ("-Infinity", (Double.MinValue / 0.0).ToString ("E99", _nfi) , "#02");
			Assert.AreEqual ("NaN", (0.0 / 0.0).ToString ("E99", _nfi) , "#03");
		}

		// Test12000- Double and F
		[Test]
		public void Test12000 ()
		{
			Assert.AreEqual ("0.00", 0.0.ToString ("F", _nfi), "#01");
			Assert.AreEqual ("0.00", 0.0.ToString ("f", _nfi), "#02");
			Assert.AreEqual ("-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00", Double.MinValue.ToString ("F", _nfi), "#03");
			Assert.AreEqual ("-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00", Double.MinValue.ToString ("f", _nfi), "#04");
			Assert.AreEqual ("179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00", Double.MaxValue.ToString ("F", _nfi), "#05");
			Assert.AreEqual ("179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00", Double.MaxValue.ToString ("f", _nfi), "#06");
		}

		[Test]
		public void Test12001 ()
		{
			Assert.AreEqual ("F ", 0.0.ToString ("F ", _nfi), "#01");
			Assert.AreEqual (" F", 0.0.ToString (" F", _nfi), "#02");
			Assert.AreEqual (" F ", 0.0.ToString (" F ", _nfi), "#03");
		}

		[Test]
		public void Test12002 ()
		{
			Assert.AreEqual ("-F ", (-1.0).ToString ("F ", _nfi), "#01");
			Assert.AreEqual ("- F", (-1.0).ToString (" F", _nfi), "#02");
			Assert.AreEqual ("- F ", (-1.0).ToString (" F ", _nfi), "#03");
		}

		[Test]
		public void Test12003 ()
		{
			Assert.AreEqual ("0", 0.0.ToString ("F0", _nfi), "#01");
			Assert.AreEqual ("0.0000000000000000", 0.0.ToString ("F16", _nfi), "#02");
			Assert.AreEqual ("0.00000000000000000", 0.0.ToString ("F17", _nfi), "#03");
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.0.ToString ("F99", _nfi), "#04");
			Assert.AreEqual ("F100", 0.0.ToString ("F100", _nfi), "#05");
		}

		[Test]
		public void Test12004 ()
		{
			Assert.AreEqual ("179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F0", _nfi), "#01");
			Assert.AreEqual ("179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0000000000000000", Double.MaxValue.ToString ("F16", _nfi), "#02");
			Assert.AreEqual ("179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00000000000000000", Double.MaxValue.ToString ("F17", _nfi), "#03");
			Assert.AreEqual ("179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F99", _nfi), "#04");
			Assert.AreEqual ("F1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F100", _nfi), "#05");
		}

		[Test]
		public void Test12005 ()
		{
			Assert.AreEqual ("-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F0", _nfi), "#01");
			Assert.AreEqual ("-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0000000000000000", Double.MinValue.ToString ("F16", _nfi), "#02");
			Assert.AreEqual ("-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00000000000000000", Double.MinValue.ToString ("F17", _nfi), "#03");
			Assert.AreEqual ("-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F99", _nfi), "#04");
			Assert.AreEqual ("-F1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F100", _nfi), "#05");
		}

		[Test]
		public void Test12006 ()
		{
			Assert.AreEqual ("FF", 0.0.ToString ("FF", _nfi), "#01");
			Assert.AreEqual ("F0F", 0.0.ToString ("F0F", _nfi), "#02");
			Assert.AreEqual ("F0xF", 0.0.ToString ("F0xF", _nfi), "#03");
		}

		[Test]
		public void Test12007 ()
		{
			Assert.AreEqual ("FF", Double.MaxValue.ToString ("FF", _nfi), "#01");
			Assert.AreEqual ("F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MaxValue.ToString ("F0F", _nfi), "#02");
			Assert.AreEqual ("F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MaxValue.ToString ("F0xF", _nfi), "#03");
		}

		[Test]
		public void Test12008 ()
		{
			Assert.AreEqual ("-FF", Double.MinValue.ToString ("FF", _nfi), "#01");
			Assert.AreEqual ("-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MinValue.ToString ("F0F", _nfi), "#02");
			Assert.AreEqual ("-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MinValue.ToString ("F0xF", _nfi), "#03");
		}

		[Test]
		public void Test12009 ()
		{
			Assert.AreEqual ("0.00000000000000000", 0.0.ToString ("F0000000000000000000000000000000000000017", _nfi), "#01");
			Assert.AreEqual ("179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00000000000000000", Double.MaxValue.ToString ("F0000000000000000000000000000000000000017", _nfi), "#02");
			Assert.AreEqual ("-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00000000000000000", Double.MinValue.ToString ("F0000000000000000000000000000000000000017", _nfi), "#03");
		}

		[Test]
		public void Test12010 ()
		{
			Assert.AreEqual ("+F", 0.0.ToString ("+F", _nfi), "#01");
			Assert.AreEqual ("F+", 0.0.ToString ("F+", _nfi), "#02");
			Assert.AreEqual ("+F+", 0.0.ToString ("+F+", _nfi), "#03");
		}
		
		[Test]
		public void Test12011 ()
		{
			Assert.AreEqual ("+F", Double.MaxValue.ToString ("+F", _nfi), "#01");
			Assert.AreEqual ("F+", Double.MaxValue.ToString ("F+", _nfi), "#02");
			Assert.AreEqual ("+F+", Double.MaxValue.ToString ("+F+", _nfi), "#03");
		}

		[Test]
		public void Test12012 ()
		{
			Assert.AreEqual ("-+F", Double.MinValue.ToString ("+F", _nfi), "#01");
			Assert.AreEqual ("-F+", Double.MinValue.ToString ("F+", _nfi), "#02");
			Assert.AreEqual ("-+F+", Double.MinValue.ToString ("+F+", _nfi), "#03");
		}

		[Test]
		public void Test12013 ()
		{
			Assert.AreEqual ("-F", 0.0.ToString ("-F", _nfi), "#01");
			Assert.AreEqual ("F-", 0.0.ToString ("F-", _nfi), "#02");
			Assert.AreEqual ("-F-", 0.0.ToString ("-F-", _nfi), "#03");
		}
		
		[Test]
		public void Test12014 ()
		{
			Assert.AreEqual ("-F", Double.MaxValue.ToString ("-F", _nfi), "#01");
			Assert.AreEqual ("F-", Double.MaxValue.ToString ("F-", _nfi), "#02");
			Assert.AreEqual ("-F-", Double.MaxValue.ToString ("-F-", _nfi), "#03");
		}

		[Test]
		public void Test12015 ()
		{
			Assert.AreEqual ("--F", Double.MinValue.ToString ("-F", _nfi), "#01");
			Assert.AreEqual ("-F-", Double.MinValue.ToString ("F-", _nfi), "#02");
			Assert.AreEqual ("--F-", Double.MinValue.ToString ("-F-", _nfi), "#03");
		}

		[Test]
		public void Test12016 ()
		{
			Assert.AreEqual ("F+0", 0.0.ToString ("F+0", _nfi), "#01");
			Assert.AreEqual ("F+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F+0", _nfi), "#02");
			Assert.AreEqual ("-F+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F+0", _nfi), "#03");
		}

		[Test]
		public void Test12017 ()
		{
			Assert.AreEqual ("F+9", 0.0.ToString ("F+9", _nfi), "#01");
			Assert.AreEqual ("F+9", Double.MaxValue.ToString ("F+9", _nfi), "#02");
			Assert.AreEqual ("-F+9", Double.MinValue.ToString ("F+9", _nfi), "#03");
		}

		[Test]
		public void Test12018 ()
		{
			Assert.AreEqual ("F-9", 0.0.ToString ("F-9", _nfi), "#01");
			Assert.AreEqual ("F-9", Double.MaxValue.ToString ("F-9", _nfi), "#02");
			Assert.AreEqual ("-F-9", Double.MinValue.ToString ("F-9", _nfi), "#03");
		}

		[Test]
		public void Test12019 ()
		{
			Assert.AreEqual ("F0", 0.0.ToString ("F0,", _nfi), "#01");
			Assert.AreEqual ("F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F0,", _nfi), "#02");
			Assert.AreEqual ("-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F0,", _nfi), "#03");
		}

		[Test]
		public void Test12020 ()
		{
			Assert.AreEqual ("F0", 0.0.ToString ("F0.", _nfi), "#01");
			Assert.AreEqual ("F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F0.", _nfi), "#02");
			Assert.AreEqual ("-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F0.", _nfi), "#03");
		}

		[Test]
		public void Test12021 ()
		{
			Assert.AreEqual ("F0.0", 0.0.ToString ("F0.0", _nfi), "#01");
			Assert.AreEqual ("F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MaxValue.ToString ("F0.0", _nfi), "#02");
			Assert.AreEqual ("-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MinValue.ToString ("F0.0", _nfi), "#03");
		}

		[Test]
		public void Test12022 ()
		{
			Assert.AreEqual ("F09", 0.0.ToString ("F0.9", _nfi), "#01");
			Assert.AreEqual ("F1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MaxValue.ToString ("F0.9", _nfi), "#02");
			Assert.AreEqual ("-F1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MinValue.ToString ("F0.9", _nfi), "#03");
		}

		[Test]
		public void Test12023 ()
		{
			Assert.AreEqual ("1.1", 1.05.ToString ("F1", _nfi), "#01");
			Assert.AreEqual ("1.2", 1.15.ToString ("F1", _nfi), "#02");
			Assert.AreEqual ("1.3", 1.25.ToString ("F1", _nfi), "#03");
			Assert.AreEqual ("1.4", 1.35.ToString ("F1", _nfi), "#04");
			Assert.AreEqual ("1.5", 1.45.ToString ("F1", _nfi), "#05");
			Assert.AreEqual ("1.6", 1.55.ToString ("F1", _nfi), "#06");
			Assert.AreEqual ("1.7", 1.65.ToString ("F1", _nfi), "#07");
			Assert.AreEqual ("1.8", 1.75.ToString ("F1", _nfi), "#08");
			Assert.AreEqual ("1.9", 1.85.ToString ("F1", _nfi), "#09");
			Assert.AreEqual ("2.0", 1.95.ToString ("F1", _nfi), "#10");
		}

		[Test]
		public void Test12024 ()
		{
			Assert.AreEqual ("1.01", 1.005.ToString ("F2", _nfi), "#01");
			Assert.AreEqual ("1.02", 1.015.ToString ("F2", _nfi), "#02");
			Assert.AreEqual ("1.03", 1.025.ToString ("F2", _nfi), "#03");
			Assert.AreEqual ("1.04", 1.035.ToString ("F2", _nfi), "#04");
			Assert.AreEqual ("1.05", 1.045.ToString ("F2", _nfi), "#05");
			Assert.AreEqual ("1.06", 1.055.ToString ("F2", _nfi), "#06");
			Assert.AreEqual ("1.07", 1.065.ToString ("F2", _nfi), "#07");
			Assert.AreEqual ("1.08", 1.075.ToString ("F2", _nfi), "#08");
			Assert.AreEqual ("1.09", 1.085.ToString ("F2", _nfi), "#09");
			Assert.AreEqual ("1.10", 1.095.ToString ("F2", _nfi), "#10");
		}

		[Test]
		public void Test12025 ()
		{
			Assert.AreEqual ("1.00000000000001", 1.000000000000005.ToString ("F14", _nfi), "#01");
			Assert.AreEqual ("1.00000000000002", 1.000000000000015.ToString ("F14", _nfi), "#02");
			Assert.AreEqual ("1.00000000000003", 1.000000000000025.ToString ("F14", _nfi), "#03");
			Assert.AreEqual ("1.00000000000004", 1.000000000000035.ToString ("F14", _nfi), "#04");
			Assert.AreEqual ("1.00000000000005", 1.000000000000045.ToString ("F14", _nfi), "#05");
			Assert.AreEqual ("1.00000000000006", 1.000000000000055.ToString ("F14", _nfi), "#06");
			Assert.AreEqual ("1.00000000000007", 1.000000000000065.ToString ("F14", _nfi), "#07");
			Assert.AreEqual ("1.00000000000008", 1.000000000000075.ToString ("F14", _nfi), "#08");
			Assert.AreEqual ("1.00000000000009", 1.000000000000085.ToString ("F14", _nfi), "#09");
			Assert.AreEqual ("1.00000000000010", 1.000000000000095.ToString ("F14", _nfi), "#10");
		}

		[Test]
		public void Test12026 ()
		{
			Assert.AreEqual ("1.000000000000000", 1.0000000000000005.ToString ("F15", _nfi), "#01");
			Assert.AreEqual ("1.000000000000000", 1.0000000000000015.ToString ("F15", _nfi), "#02");
			Assert.AreEqual ("1.000000000000000", 1.0000000000000025.ToString ("F15", _nfi), "#03");
			Assert.AreEqual ("1.000000000000000", 1.0000000000000035.ToString ("F15", _nfi), "#04");
			Assert.AreEqual ("1.000000000000000", 1.0000000000000045.ToString ("F15", _nfi), "#05");
			Assert.AreEqual ("1.000000000000010", 1.0000000000000055.ToString ("F15", _nfi), "#06");
			Assert.AreEqual ("1.000000000000010", 1.0000000000000065.ToString ("F15", _nfi), "#07");
			Assert.AreEqual ("1.000000000000010", 1.0000000000000075.ToString ("F15", _nfi), "#08");
			Assert.AreEqual ("1.000000000000010", 1.0000000000000085.ToString ("F15", _nfi), "#09");
			Assert.AreEqual ("1.000000000000010", 1.0000000000000095.ToString ("F15", _nfi), "#10");
		}

		[Test]
		public void Test12027 ()
		{
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000005.ToString ("F16", _nfi), "#01");
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000015.ToString ("F16", _nfi), "#02");
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000025.ToString ("F16", _nfi), "#03");
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000035.ToString ("F16", _nfi), "#04");
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000045.ToString ("F16", _nfi), "#05");
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000055.ToString ("F16", _nfi), "#06");
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000065.ToString ("F16", _nfi), "#07");
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000075.ToString ("F16", _nfi), "#08");
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000085.ToString ("F16", _nfi), "#09");
			Assert.AreEqual ("1.0000000000000000", 1.00000000000000095.ToString ("F16", _nfi), "#10");
		}

		[Test]
		public void Test12028 ()
		{
			Assert.AreEqual ("1", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F0", _nfi), "#01");
			Assert.AreEqual ("1.234567890123", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F12", _nfi), "#02");
			Assert.AreEqual ("1.2345678901235", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F13", _nfi), "#03");
			Assert.AreEqual ("1.23456789012346", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F14", _nfi), "#04");
			Assert.AreEqual ("1.234567890123460", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F15", _nfi), "#05");
			Assert.AreEqual ("1.234567890123460000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F99", _nfi), "#06");
			Assert.AreEqual ("F101", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F100", _nfi), "#07");
		}

		[Test]
		public void Test12029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			Assert.AreEqual ("-99999999#90", (-99999999.9).ToString ("F", nfi), "#01");
		}

		[Test]
		public void Test12030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			Assert.AreEqual ("1.00", 1.0.ToString ("F", nfi), "#01");
			Assert.AreEqual ("0.00", 0.0.ToString ("F", nfi), "#02");
			Assert.AreEqual ("+1.00", (-1.0).ToString ("F", nfi), "#03");
		}

		[Test]
		public void Test12031 ()
		{
			Assert.AreEqual ("Infinity", (Double.MaxValue / 0.0).ToString ("F99", _nfi) , "#01");
			Assert.AreEqual ("-Infinity", (Double.MinValue / 0.0).ToString ("F99", _nfi) , "#02");
			Assert.AreEqual ("NaN", (0.0 / 0.0).ToString ("F99", _nfi) , "#03");
		}

		// Test13000- Double and G
		[Test]
		public void Test13000 ()
		{
			Assert.AreEqual ("0", 0.0.ToString ("G", _nfi), "#01");
			Assert.AreEqual ("0", (-0.0).ToString ("G", _nfi), "#01.1");
			Assert.AreEqual ("0", 0.0.ToString ("g", _nfi), "#02");
			Assert.AreEqual ("-1.79769313486232E+308", Double.MinValue.ToString ("G", _nfi), "#03");
			Assert.AreEqual ("-1.79769313486232e+308", Double.MinValue.ToString ("g", _nfi), "#04");
			Assert.AreEqual ("1.79769313486232E+308", Double.MaxValue.ToString ("G", _nfi), "#05");
			Assert.AreEqual ("1.79769313486232e+308", Double.MaxValue.ToString ("g", _nfi), "#06");
		}

		[Test]
		public void Test13001 ()
		{
			Assert.AreEqual ("G ", 0.0.ToString ("G ", _nfi), "#01");
			Assert.AreEqual (" G", 0.0.ToString (" G", _nfi), "#02");
			Assert.AreEqual (" G ", 0.0.ToString (" G ", _nfi), "#03");
		}

		[Test]
		public void Test13002 ()
		{
			Assert.AreEqual ("-G ", (-1.0).ToString ("G ", _nfi), "#01");
			Assert.AreEqual ("- G", (-1.0).ToString (" G", _nfi), "#02");
			Assert.AreEqual ("- G ", (-1.0).ToString (" G ", _nfi), "#03");
		}

		[Test]
		public void Test13003 ()
		{
			Assert.AreEqual ("0", 0.0.ToString ("G0", _nfi), "#01");
			Assert.AreEqual ("0", 0.0.ToString ("G16", _nfi), "#02");
			Assert.AreEqual ("0", 0.0.ToString ("G17", _nfi), "#03");
			Assert.AreEqual ("0", 0.0.ToString ("G99", _nfi), "#04");
			Assert.AreEqual ("G100", 0.0.ToString ("G100", _nfi), "#05");
		}

		[Test]
		public void Test13004 ()
		{
			Assert.AreEqual ("1.79769313486232E+308", Double.MaxValue.ToString ("G0", _nfi), "#01");
			Assert.AreEqual ("1.797693134862316E+308", Double.MaxValue.ToString ("G16", _nfi), "#02");
			Assert.AreEqual ("1.7976931348623157E+308", Double.MaxValue.ToString ("G17", _nfi), "#03");
			Assert.AreEqual ("1.7976931348623157E+308", Double.MaxValue.ToString ("G99", _nfi), "#04");
			Assert.AreEqual ("G1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("G100", _nfi), "#05");
		}

		[Test]
		public void Test13005 ()
		{
			Assert.AreEqual ("-1.79769313486232E+308", Double.MinValue.ToString ("G0", _nfi), "#01");
			Assert.AreEqual ("-1.797693134862316E+308", Double.MinValue.ToString ("G16", _nfi), "#02");
			Assert.AreEqual ("-1.7976931348623157E+308", Double.MinValue.ToString ("G17", _nfi), "#03");
			Assert.AreEqual ("-1.7976931348623157E+308", Double.MinValue.ToString ("G99", _nfi), "#04");
			Assert.AreEqual ("-G1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("G100", _nfi), "#05");
		}

		[Test]
		public void Test13006 ()
		{
			Assert.AreEqual ("GF", 0.0.ToString ("GF", _nfi), "#01");
			Assert.AreEqual ("G0F", 0.0.ToString ("G0F", _nfi), "#02");
			Assert.AreEqual ("G0xF", 0.0.ToString ("G0xF", _nfi), "#03");
		}

		[Test]
		public void Test13007 ()
		{
			Assert.AreEqual ("GF", Double.MaxValue.ToString ("GF", _nfi), "#01");
			Assert.AreEqual ("G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MaxValue.ToString ("G0F", _nfi), "#02");
			Assert.AreEqual ("G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MaxValue.ToString ("G0xF", _nfi), "#03");
		}

		[Test]
		public void Test13008 ()
		{
			Assert.AreEqual ("-GF", Double.MinValue.ToString ("GF", _nfi), "#01");
			Assert.AreEqual ("-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MinValue.ToString ("G0F", _nfi), "#02");
			Assert.AreEqual ("-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MinValue.ToString ("G0xF", _nfi), "#03");
		}

		[Test]
		public void Test13009 ()
		{
			Assert.AreEqual ("0", 0.0.ToString ("G0000000000000000000000000000000000000017", _nfi), "#01");
			Assert.AreEqual ("1.7976931348623157E+308", Double.MaxValue.ToString ("G0000000000000000000000000000000000000017", _nfi), "#02");
			Assert.AreEqual ("-1.7976931348623157E+308", Double.MinValue.ToString ("G0000000000000000000000000000000000000017", _nfi), "#03");
		}

		[Test]
		public void Test13010 ()
		{
			Assert.AreEqual ("+G", 0.0.ToString ("+G", _nfi), "#01");
			Assert.AreEqual ("G+", 0.0.ToString ("G+", _nfi), "#02");
			Assert.AreEqual ("+G+", 0.0.ToString ("+G+", _nfi), "#03");
		}
		
		[Test]
		public void Test13011 ()
		{
			Assert.AreEqual ("+G", Double.MaxValue.ToString ("+G", _nfi), "#01");
			Assert.AreEqual ("G+", Double.MaxValue.ToString ("G+", _nfi), "#02");
			Assert.AreEqual ("+G+", Double.MaxValue.ToString ("+G+", _nfi), "#03");
		}

		[Test]
		public void Test13012 ()
		{
			Assert.AreEqual ("-+G", Double.MinValue.ToString ("+G", _nfi), "#01");
			Assert.AreEqual ("-G+", Double.MinValue.ToString ("G+", _nfi), "#02");
			Assert.AreEqual ("-+G+", Double.MinValue.ToString ("+G+", _nfi), "#03");
		}

		[Test]
		public void Test13013 ()
		{
			Assert.AreEqual ("-G", 0.0.ToString ("-G", _nfi), "#01");
			Assert.AreEqual ("G-", 0.0.ToString ("G-", _nfi), "#02");
			Assert.AreEqual ("-G-", 0.0.ToString ("-G-", _nfi), "#03");
		}
		
		[Test]
		public void Test13014 ()
		{
			Assert.AreEqual ("-G", Double.MaxValue.ToString ("-G", _nfi), "#01");
			Assert.AreEqual ("G-", Double.MaxValue.ToString ("G-", _nfi), "#02");
			Assert.AreEqual ("-G-", Double.MaxValue.ToString ("-G-", _nfi), "#03");
		}

		[Test]
		public void Test13015 ()
		{
			Assert.AreEqual ("--G", Double.MinValue.ToString ("-G", _nfi), "#01");
			Assert.AreEqual ("-G-", Double.MinValue.ToString ("G-", _nfi), "#02");
			Assert.AreEqual ("--G-", Double.MinValue.ToString ("-G-", _nfi), "#03");
		}

		[Test]
		public void Test13016 ()
		{
			Assert.AreEqual ("G+0", 0.0.ToString ("G+0", _nfi), "#01");
			Assert.AreEqual ("G+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("G+0", _nfi), "#02");
			Assert.AreEqual ("-G+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("G+0", _nfi), "#03");
		}

		[Test]
		public void Test13017 ()
		{
			Assert.AreEqual ("G+9", 0.0.ToString ("G+9", _nfi), "#01");
			Assert.AreEqual ("G+9", Double.MaxValue.ToString ("G+9", _nfi), "#02");
			Assert.AreEqual ("-G+9", Double.MinValue.ToString ("G+9", _nfi), "#03");
		}

		[Test]
		public void Test13018 ()
		{
			Assert.AreEqual ("G-9", 0.0.ToString ("G-9", _nfi), "#01");
			Assert.AreEqual ("G-9", Double.MaxValue.ToString ("G-9", _nfi), "#02");
			Assert.AreEqual ("-G-9", Double.MinValue.ToString ("G-9", _nfi), "#03");
		}

		[Test]
		public void Test13019 ()
		{
			Assert.AreEqual ("G0", 0.0.ToString ("G0,", _nfi), "#01");
			Assert.AreEqual ("G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("G0,", _nfi), "#02");
			Assert.AreEqual ("-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("G0,", _nfi), "#03");
		}

		[Test]
		public void Test13020 ()
		{
			Assert.AreEqual ("G0", 0.0.ToString ("G0.", _nfi), "#01");
			Assert.AreEqual ("G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("G0.", _nfi), "#02");
			Assert.AreEqual ("-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("G0.", _nfi), "#03");
		}

		[Test]
		public void Test13021 ()
		{
			Assert.AreEqual ("G0.0", 0.0.ToString ("G0.0", _nfi), "#01");
			Assert.AreEqual ("G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MaxValue.ToString ("G0.0", _nfi), "#02");
			Assert.AreEqual ("-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MinValue.ToString ("G0.0", _nfi), "#03");
		}

		[Test]
		public void Test13022 ()
		{
			Assert.AreEqual ("G09", 0.0.ToString ("G0.9", _nfi), "#01");
			Assert.AreEqual ("G1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MaxValue.ToString ("G0.9", _nfi), "#02");
			Assert.AreEqual ("-G1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MinValue.ToString ("G0.9", _nfi), "#03");
		}

		[Test]
		public void Test13023 ()
		{
			Assert.AreEqual ("0.5", 0.5.ToString ("G1", _nfi), "#01");
			Assert.AreEqual ("2", 1.5.ToString ("G1", _nfi), "#02");
			Assert.AreEqual ("3", 2.5.ToString ("G1", _nfi), "#03");
			Assert.AreEqual ("4", 3.5.ToString ("G1", _nfi), "#04");
			Assert.AreEqual ("5", 4.5.ToString ("G1", _nfi), "#05");
			Assert.AreEqual ("6", 5.5.ToString ("G1", _nfi), "#06");
			Assert.AreEqual ("7", 6.5.ToString ("G1", _nfi), "#07");
			Assert.AreEqual ("8", 7.5.ToString ("G1", _nfi), "#08");
			Assert.AreEqual ("9", 8.5.ToString ("G1", _nfi), "#09");
			Assert.AreEqual ("1E+01", 9.5.ToString ("G1", _nfi), "#10");
		}

		[Test]
		public void Test13024_CarryPropagation () 
		{
			Double d = 1.15;
			Assert.AreEqual ("1", d.ToString ("G1", _nfi), "#01");
			// NumberStore converts 1.15 into 1.14999...91 (1 in index 17)
			// so the call to NumberToString doesn't result in 1.2 but in 1.1
			// which seems "somewhat" normal considering the #17 results,
			Assert.AreEqual ("1.2", d.ToString ("G2", _nfi), "#02");
			Assert.AreEqual ("1.15", d.ToString ("G3", _nfi), "#03");
			Assert.AreEqual ("1.15", d.ToString ("G4", _nfi), "#04");
			Assert.AreEqual ("1.15", d.ToString ("G5", _nfi), "#05");
			Assert.AreEqual ("1.15", d.ToString ("G6", _nfi), "#06");
			Assert.AreEqual ("1.15", d.ToString ("G7", _nfi), "#07");
			Assert.AreEqual ("1.15", d.ToString ("G8", _nfi), "#08");
			Assert.AreEqual ("1.15", d.ToString ("G9", _nfi), "#09");
			Assert.AreEqual ("1.15", d.ToString ("G10", _nfi), "#10");
			Assert.AreEqual ("1.15", d.ToString ("G11", _nfi), "#11");
			Assert.AreEqual ("1.15", d.ToString ("G12", _nfi), "#12");
			Assert.AreEqual ("1.15", d.ToString ("G13", _nfi), "#13");
			Assert.AreEqual ("1.15", d.ToString ("G14", _nfi), "#14");
			Assert.AreEqual ("1.15", d.ToString ("G15", _nfi), "#15");
			Assert.AreEqual ("1.15", d.ToString ("G16", _nfi), "#16");
			Assert.AreEqual ("1.1499999999999999", d.ToString ("G17", _nfi), "#17");
		}

		[Test]
		public void Test13024 ()
		{
			Assert.AreEqual ("1.1", 1.05.ToString ("G2", _nfi), "#01");
			Assert.AreEqual ("1.2", 1.15.ToString ("G2", _nfi), "#02");
			Assert.AreEqual ("1.3", 1.25.ToString ("G2", _nfi), "#03");
			Assert.AreEqual ("1.4", 1.35.ToString ("G2", _nfi), "#04");
			Assert.AreEqual ("1.5", 1.45.ToString ("G2", _nfi), "#05");
			Assert.AreEqual ("1.6", 1.55.ToString ("G2", _nfi), "#06");
			Assert.AreEqual ("1.7", 1.65.ToString ("G2", _nfi), "#07");
			Assert.AreEqual ("1.8", 1.75.ToString ("G2", _nfi), "#08");
			Assert.AreEqual ("1.9", 1.85.ToString ("G2", _nfi), "#09");
			Assert.AreEqual ("2", 1.95.ToString ("G2", _nfi), "#10");
		}

		[Test]
		public void Test13025 ()
		{
			Assert.AreEqual ("10", 10.05.ToString ("G2", _nfi), "#01");
			Assert.AreEqual ("10", 10.15.ToString ("G2", _nfi), "#02");
			Assert.AreEqual ("10", 10.25.ToString ("G2", _nfi), "#03");
			Assert.AreEqual ("10", 10.35.ToString ("G2", _nfi), "#04");
			Assert.AreEqual ("10", 10.45.ToString ("G2", _nfi), "#05");
			Assert.AreEqual ("11", 10.55.ToString ("G2", _nfi), "#06");
			Assert.AreEqual ("11", 10.65.ToString ("G2", _nfi), "#07");
			Assert.AreEqual ("11", 10.75.ToString ("G2", _nfi), "#08");
			Assert.AreEqual ("11", 10.85.ToString ("G2", _nfi), "#09");
			Assert.AreEqual ("11", 10.95.ToString ("G2", _nfi), "#10");
		}

		[Test]
		public void Test13026 ()
		{
			Assert.AreEqual ("1.00000000000001", 1.000000000000005.ToString ("G15", _nfi), "#01");
			Assert.AreEqual ("1.00000000000002", 1.000000000000015.ToString ("G15", _nfi), "#02");
			Assert.AreEqual ("1.00000000000003", 1.000000000000025.ToString ("G15", _nfi), "#03");
			Assert.AreEqual ("1.00000000000004", 1.000000000000035.ToString ("G15", _nfi), "#04");
			Assert.AreEqual ("1.00000000000005", 1.000000000000045.ToString ("G15", _nfi), "#05");
			Assert.AreEqual ("1.00000000000006", 1.000000000000055.ToString ("G15", _nfi), "#06");
			Assert.AreEqual ("1.00000000000007", 1.000000000000065.ToString ("G15", _nfi), "#07");
			Assert.AreEqual ("1.00000000000008", 1.000000000000075.ToString ("G15", _nfi), "#08");
			Assert.AreEqual ("1.00000000000009", 1.000000000000085.ToString ("G15", _nfi), "#09");
			Assert.AreEqual ("1.0000000000001", 1.000000000000095.ToString ("G15", _nfi), "#10");
		}

		[Test]
		public void Test13027 ()
		{
			Assert.AreEqual ("1", 1.0000000000000005.ToString ("G16", _nfi), "#01");
			Assert.AreEqual ("1.000000000000002", 1.0000000000000015.ToString ("G16", _nfi), "#02");
			Assert.AreEqual ("1.000000000000002", 1.0000000000000025.ToString ("G16", _nfi), "#03");
			Assert.AreEqual ("1.000000000000004", 1.0000000000000035.ToString ("G16", _nfi), "#04");
			Assert.AreEqual ("1.000000000000004", 1.0000000000000045.ToString ("G16", _nfi), "#05");
			Assert.AreEqual ("1.000000000000006", 1.0000000000000055.ToString ("G16", _nfi), "#06");
			Assert.AreEqual ("1.000000000000006", 1.0000000000000065.ToString ("G16", _nfi), "#07");
			Assert.AreEqual ("1.000000000000008", 1.0000000000000075.ToString ("G16", _nfi), "#08");
			Assert.AreEqual ("1.000000000000008", 1.0000000000000085.ToString ("G16", _nfi), "#09");
			Assert.AreEqual ("1.00000000000001", 1.0000000000000095.ToString ("G16", _nfi), "#10");
		}

		[Test]
		public void Test13028 ()
		{
			Assert.AreEqual ("1", 1.00000000000000005.ToString ("G17", _nfi), "#01");
			Assert.AreEqual ("1.0000000000000002", 1.00000000000000015.ToString ("G17", _nfi), "#02");
			Assert.AreEqual ("1.0000000000000002", 1.00000000000000025.ToString ("G17", _nfi), "#03");
			Assert.AreEqual ("1.0000000000000004", 1.00000000000000035.ToString ("G17", _nfi), "#04");
			Assert.AreEqual ("1.0000000000000004", 1.00000000000000045.ToString ("G17", _nfi), "#05");
			Assert.AreEqual ("1.0000000000000004", 1.00000000000000055.ToString ("G17", _nfi), "#06");
			Assert.AreEqual ("1.0000000000000007", 1.00000000000000065.ToString ("G17", _nfi), "#07");
			Assert.AreEqual ("1.0000000000000007", 1.00000000000000075.ToString ("G17", _nfi), "#08");
			Assert.AreEqual ("1.0000000000000009", 1.00000000000000085.ToString ("G17", _nfi), "#09");
			Assert.AreEqual ("1.0000000000000009", 1.00000000000000095.ToString ("G17", _nfi), "#10");
		}
		
		[Test]
		public void Test13029 ()
		{
			Assert.AreEqual ("1", 1.000000000000000005.ToString ("G18", _nfi), "#01");
			Assert.AreEqual ("1", 1.000000000000000015.ToString ("G18", _nfi), "#02");
			Assert.AreEqual ("1", 1.000000000000000025.ToString ("G18", _nfi), "#03");
			Assert.AreEqual ("1", 1.000000000000000035.ToString ("G18", _nfi), "#04");
			Assert.AreEqual ("1", 1.000000000000000045.ToString ("G18", _nfi), "#05");
			Assert.AreEqual ("1", 1.000000000000000055.ToString ("G18", _nfi), "#06");
			Assert.AreEqual ("1", 1.000000000000000065.ToString ("G18", _nfi), "#07");
			Assert.AreEqual ("1", 1.000000000000000075.ToString ("G18", _nfi), "#08");
			Assert.AreEqual ("1", 1.000000000000000085.ToString ("G18", _nfi), "#09");
			Assert.AreEqual ("1", 1.000000000000000095.ToString ("G18", _nfi), "#10");
		}

		[Test]
		public void Test13030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			Assert.AreEqual ("-99999999#9", (-99999999.9).ToString ("G", nfi), "#01");
		}

		[Test]
		public void Test13031 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			Assert.AreEqual ("1", 1.0.ToString ("G", nfi), "#01");
			Assert.AreEqual ("0", 0.0.ToString ("G", nfi), "#02");
			Assert.AreEqual ("+1", (-1.0).ToString ("G", nfi), "#03");
		}

		[Test]
		public void Test13032 ()
		{
			Assert.AreEqual ("Infinity", (Double.MaxValue / 0.0).ToString ("G99", _nfi) , "#01");
			Assert.AreEqual ("-Infinity", (Double.MinValue / 0.0).ToString ("G99", _nfi) , "#02");
			Assert.AreEqual ("NaN", (0.0 / 0.0).ToString ("G99", _nfi) , "#03");
		}

		[Test]
		public void Test13033 ()
		{
			Assert.AreEqual ("0.0001", 0.0001.ToString ("G", _nfi), "#01");
			Assert.AreEqual ("1E-05", 0.00001.ToString ("G", _nfi), "#02");
			Assert.AreEqual ("0.0001", 0.0001.ToString ("G0", _nfi), "#03");
			Assert.AreEqual ("1E-05", 0.00001.ToString ("G0", _nfi), "#04");
			Assert.AreEqual ("100000000000000", 100000000000000.0.ToString ("G", _nfi), "#05");
			Assert.AreEqual ("1E+15", 1000000000000000.0.ToString ("G", _nfi), "#06");
			Assert.AreEqual ("1000000000000000", 1000000000000000.0.ToString ("G16", _nfi), "#07");
		}

		// Test14000- Double and N
		[Test]
		public void Test14000 ()
		{
			Assert.AreEqual ("0.00", 0.0.ToString ("N", _nfi), "#01");
			Assert.AreEqual ("0.00", 0.0.ToString ("n", _nfi), "#02");
			Assert.AreEqual ("-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00", Double.MinValue.ToString ("N", _nfi), "#03");
			Assert.AreEqual ("-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00", Double.MinValue.ToString ("n", _nfi), "#04");
			Assert.AreEqual ("179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00", Double.MaxValue.ToString ("N", _nfi), "#05");
			Assert.AreEqual ("179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00", Double.MaxValue.ToString ("n", _nfi), "#06");
		}

		[Test]
		public void Test14001 ()
		{
			Assert.AreEqual ("N ", 0.0.ToString ("N ", _nfi), "#01");
			Assert.AreEqual (" N", 0.0.ToString (" N", _nfi), "#02");
			Assert.AreEqual (" N ", 0.0.ToString (" N ", _nfi), "#03");
		}

		[Test]
		public void Test14002 ()
		{
			Assert.AreEqual ("-N ", (-1.0).ToString ("N ", _nfi), "#01");
			Assert.AreEqual ("- N", (-1.0).ToString (" N", _nfi), "#02");
			Assert.AreEqual ("- N ", (-1.0).ToString (" N ", _nfi), "#03");
		}

		[Test]
		public void Test14003 ()
		{
			Assert.AreEqual ("0", 0.0.ToString ("N0", _nfi), "#01");
			Assert.AreEqual ("0.0000000000000000", 0.0.ToString ("N16", _nfi), "#02");
			Assert.AreEqual ("0.00000000000000000", 0.0.ToString ("N17", _nfi), "#03");
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.0.ToString ("N99", _nfi), "#04");
			Assert.AreEqual ("N100", 0.0.ToString ("N100", _nfi), "#05");
		}

		[Test]
		public void Test14004 ()
		{
			Assert.AreEqual ("179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000", Double.MaxValue.ToString ("N0", _nfi), "#01");
			Assert.AreEqual ("179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.0000000000000000", Double.MaxValue.ToString ("N16", _nfi), "#02");
			Assert.AreEqual ("179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000", Double.MaxValue.ToString ("N17", _nfi), "#03");
			Assert.AreEqual ("179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N99", _nfi), "#04");
			Assert.AreEqual ("N1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N100", _nfi), "#05");
		}

		[Test]
		public void Test14005 ()
		{
			Assert.AreEqual ("-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000", Double.MinValue.ToString ("N0", _nfi), "#01");
			Assert.AreEqual ("-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.0000000000000000", Double.MinValue.ToString ("N16", _nfi), "#02");
			Assert.AreEqual ("-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000", Double.MinValue.ToString ("N17", _nfi), "#03");
			Assert.AreEqual ("-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N99", _nfi), "#04");
			Assert.AreEqual ("-N1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N100", _nfi), "#05");
		}

		[Test]
		public void Test14006 ()
		{
			Assert.AreEqual ("NF", 0.0.ToString ("NF", _nfi), "#01");
			Assert.AreEqual ("N0F", 0.0.ToString ("N0F", _nfi), "#02");
			Assert.AreEqual ("N0xF", 0.0.ToString ("N0xF", _nfi), "#03");
		}

		[Test]
		public void Test14007 ()
		{
			Assert.AreEqual ("NF", Double.MaxValue.ToString ("NF", _nfi), "#01");
			Assert.AreEqual ("N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MaxValue.ToString ("N0F", _nfi), "#02");
			Assert.AreEqual ("N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MaxValue.ToString ("N0xF", _nfi), "#03");
		}

		[Test]
		public void Test14008 ()
		{
			Assert.AreEqual ("-NF", Double.MinValue.ToString ("NF", _nfi), "#01");
			Assert.AreEqual ("-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MinValue.ToString ("N0F", _nfi), "#02");
			Assert.AreEqual ("-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MinValue.ToString ("N0xF", _nfi), "#03");
		}

		[Test]
		public void Test14009 ()
		{
			Assert.AreEqual ("0.00000000000000000", 0.0.ToString ("N0000000000000000000000000000000000000017", _nfi), "#01");
			Assert.AreEqual ("179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000", Double.MaxValue.ToString ("N0000000000000000000000000000000000000017", _nfi), "#02");
			Assert.AreEqual ("-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000", Double.MinValue.ToString ("N0000000000000000000000000000000000000017", _nfi), "#03");
		}

		[Test]
		public void Test14010 ()
		{
			Assert.AreEqual ("+N", 0.0.ToString ("+N", _nfi), "#01");
			Assert.AreEqual ("N+", 0.0.ToString ("N+", _nfi), "#02");
			Assert.AreEqual ("+N+", 0.0.ToString ("+N+", _nfi), "#03");
		}
		
		[Test]
		public void Test14011 ()
		{
			Assert.AreEqual ("+N", Double.MaxValue.ToString ("+N", _nfi), "#01");
			Assert.AreEqual ("N+", Double.MaxValue.ToString ("N+", _nfi), "#02");
			Assert.AreEqual ("+N+", Double.MaxValue.ToString ("+N+", _nfi), "#03");
		}

		[Test]
		public void Test14012 ()
		{
			Assert.AreEqual ("-+N", Double.MinValue.ToString ("+N", _nfi), "#01");
			Assert.AreEqual ("-N+", Double.MinValue.ToString ("N+", _nfi), "#02");
			Assert.AreEqual ("-+N+", Double.MinValue.ToString ("+N+", _nfi), "#03");
		}

		[Test]
		public void Test14013 ()
		{
			Assert.AreEqual ("-N", 0.0.ToString ("-N", _nfi), "#01");
			Assert.AreEqual ("N-", 0.0.ToString ("N-", _nfi), "#02");
			Assert.AreEqual ("-N-", 0.0.ToString ("-N-", _nfi), "#03");
		}
		
		[Test]
		public void Test14014 ()
		{
			Assert.AreEqual ("-N", Double.MaxValue.ToString ("-N", _nfi), "#01");
			Assert.AreEqual ("N-", Double.MaxValue.ToString ("N-", _nfi), "#02");
			Assert.AreEqual ("-N-", Double.MaxValue.ToString ("-N-", _nfi), "#03");
		}

		[Test]
		public void Test14015 ()
		{
			Assert.AreEqual ("--N", Double.MinValue.ToString ("-N", _nfi), "#01");
			Assert.AreEqual ("-N-", Double.MinValue.ToString ("N-", _nfi), "#02");
			Assert.AreEqual ("--N-", Double.MinValue.ToString ("-N-", _nfi), "#03");
		}

		[Test]
		public void Test14016 ()
		{
			Assert.AreEqual ("N+0", 0.0.ToString ("N+0", _nfi), "#01");
			Assert.AreEqual ("N+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N+0", _nfi), "#02");
			Assert.AreEqual ("-N+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N+0", _nfi), "#03");
		}

		[Test]
		public void Test14017 ()
		{
			Assert.AreEqual ("N+9", 0.0.ToString ("N+9", _nfi), "#01");
			Assert.AreEqual ("N+9", Double.MaxValue.ToString ("N+9", _nfi), "#02");
			Assert.AreEqual ("-N+9", Double.MinValue.ToString ("N+9", _nfi), "#03");
		}

		[Test]
		public void Test14018 ()
		{
			Assert.AreEqual ("N-9", 0.0.ToString ("N-9", _nfi), "#01");
			Assert.AreEqual ("N-9", Double.MaxValue.ToString ("N-9", _nfi), "#02");
			Assert.AreEqual ("-N-9", Double.MinValue.ToString ("N-9", _nfi), "#03");
		}

		[Test]
		public void Test14019 ()
		{
			Assert.AreEqual ("N0", 0.0.ToString ("N0,", _nfi), "#01");
			Assert.AreEqual ("N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N0,", _nfi), "#02");
			Assert.AreEqual ("-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N0,", _nfi), "#03");
		}

		[Test]
		public void Test14020 ()
		{
			Assert.AreEqual ("N0", 0.0.ToString ("N0.", _nfi), "#01");
			Assert.AreEqual ("N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N0.", _nfi), "#02");
			Assert.AreEqual ("-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N0.", _nfi), "#03");
		}

		[Test]
		public void Test14021 ()
		{
			Assert.AreEqual ("N0.0", 0.0.ToString ("N0.0", _nfi), "#01");
			Assert.AreEqual ("N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MaxValue.ToString ("N0.0", _nfi), "#02");
			Assert.AreEqual ("-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MinValue.ToString ("N0.0", _nfi), "#03");
		}

		[Test]
		public void Test14022 ()
		{
			Assert.AreEqual ("N09", 0.0.ToString ("N0.9", _nfi), "#01");
			Assert.AreEqual ("N1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MaxValue.ToString ("N0.9", _nfi), "#02");
			Assert.AreEqual ("-N1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MinValue.ToString ("N0.9", _nfi), "#03");
		}

		[Test]
		public void Test14023 ()
		{
			Assert.AreEqual ("999.1", 999.05.ToString ("N1", _nfi), "#01");
			Assert.AreEqual ("999.2", 999.15.ToString ("N1", _nfi), "#02");
			Assert.AreEqual ("999.3", 999.25.ToString ("N1", _nfi), "#03");
			Assert.AreEqual ("999.4", 999.35.ToString ("N1", _nfi), "#04");
			Assert.AreEqual ("999.5", 999.45.ToString ("N1", _nfi), "#05");
			Assert.AreEqual ("999.6", 999.55.ToString ("N1", _nfi), "#06");
			Assert.AreEqual ("999.7", 999.65.ToString ("N1", _nfi), "#07");
			Assert.AreEqual ("999.8", 999.75.ToString ("N1", _nfi), "#08");
			Assert.AreEqual ("999.9", 999.85.ToString ("N1", _nfi), "#09");
			Assert.AreEqual ("1,000.0", 999.95.ToString ("N1", _nfi), "#10");
		}

		[Test]
		public void Test14024 ()
		{
			Assert.AreEqual ("999.91", 999.905.ToString ("N2", _nfi), "#01");
			Assert.AreEqual ("999.92", 999.915.ToString ("N2", _nfi), "#02");
			Assert.AreEqual ("999.93", 999.925.ToString ("N2", _nfi), "#03");
			Assert.AreEqual ("999.94", 999.935.ToString ("N2", _nfi), "#04");
			Assert.AreEqual ("999.95", 999.945.ToString ("N2", _nfi), "#05");
			Assert.AreEqual ("999.96", 999.955.ToString ("N2", _nfi), "#06");
			Assert.AreEqual ("999.97", 999.965.ToString ("N2", _nfi), "#07");
			Assert.AreEqual ("999.98", 999.975.ToString ("N2", _nfi), "#08");
			Assert.AreEqual ("999.99", 999.985.ToString ("N2", _nfi), "#09");
			Assert.AreEqual ("1,000.00", 999.995.ToString ("N2", _nfi), "#10");
		}

		[Test]
		public void Test14025 ()
		{
			Assert.AreEqual ("999.99999999991", 999.999999999905.ToString ("N11", _nfi), "#01");
			Assert.AreEqual ("999.99999999992", 999.999999999915.ToString ("N11", _nfi), "#02");
			Assert.AreEqual ("999.99999999993", 999.999999999925.ToString ("N11", _nfi), "#03");
			Assert.AreEqual ("999.99999999994", 999.999999999935.ToString ("N11", _nfi), "#04");
			Assert.AreEqual ("999.99999999995", 999.999999999945.ToString ("N11", _nfi), "#05");
			Assert.AreEqual ("999.99999999996", 999.999999999955.ToString ("N11", _nfi), "#06");
			Assert.AreEqual ("999.99999999997", 999.999999999965.ToString ("N11", _nfi), "#07");
			Assert.AreEqual ("999.99999999998", 999.999999999975.ToString ("N11", _nfi), "#08");
			Assert.AreEqual ("999.99999999999", 999.999999999985.ToString ("N11", _nfi), "#09");
			Assert.AreEqual ("1,000.00000000000", 999.999999999995.ToString ("N11", _nfi), "#10");
		}

		[Test]
		public void Test14026 ()
		{
			Assert.AreEqual ("999.999999999990", 999.9999999999905.ToString ("N12", _nfi), "#01");
			Assert.AreEqual ("999.999999999991", 999.9999999999915.ToString ("N12", _nfi), "#02");
			Assert.AreEqual ("999.999999999992", 999.9999999999925.ToString ("N12", _nfi), "#03");
			Assert.AreEqual ("999.999999999994", 999.9999999999935.ToString ("N12", _nfi), "#04");
			Assert.AreEqual ("999.999999999995", 999.9999999999945.ToString ("N12", _nfi), "#05");
			Assert.AreEqual ("999.999999999995", 999.9999999999955.ToString ("N12", _nfi), "#06");
			Assert.AreEqual ("999.999999999996", 999.9999999999965.ToString ("N12", _nfi), "#07");
			Assert.AreEqual ("999.999999999997", 999.9999999999975.ToString ("N12", _nfi), "#08");
			Assert.AreEqual ("999.999999999999", 999.9999999999985.ToString ("N12", _nfi), "#09");
			Assert.AreEqual ("1,000.000000000000", 999.9999999999995.ToString ("N12", _nfi), "#10");
		}

		[Test]
		public void Test14027 ()
		{
			Assert.AreEqual ("999.9999999999990", 999.99999999999905.ToString ("N13", _nfi), "#01");
			Assert.AreEqual ("999.9999999999990", 999.99999999999915.ToString ("N13", _nfi), "#02");
			Assert.AreEqual ("999.9999999999990", 999.99999999999925.ToString ("N13", _nfi), "#03");
			Assert.AreEqual ("999.9999999999990", 999.99999999999935.ToString ("N13", _nfi), "#04");
			Assert.AreEqual ("999.9999999999990", 999.99999999999945.ToString ("N13", _nfi), "#05");
			Assert.AreEqual ("1,000.0000000000000", 999.99999999999955.ToString ("N13", _nfi), "#06");
			Assert.AreEqual ("1,000.0000000000000", 999.99999999999965.ToString ("N13", _nfi), "#07");
			Assert.AreEqual ("1,000.0000000000000", 999.99999999999975.ToString ("N13", _nfi), "#08");
			Assert.AreEqual ("1,000.0000000000000", 999.99999999999985.ToString ("N13", _nfi), "#09");
			Assert.AreEqual ("1,000.0000000000000", 999.99999999999995.ToString ("N13", _nfi), "#10");
		}

		[Test]
		public void Test14028 ()
		{
			Assert.AreEqual ("1", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N0", _nfi), "#01");
			Assert.AreEqual ("1.234567890123", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N12", _nfi), "#02");
			Assert.AreEqual ("1.2345678901235", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N13", _nfi), "#03");
			Assert.AreEqual ("1.23456789012346", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N14", _nfi), "#04");
			Assert.AreEqual ("1.234567890123460", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N15", _nfi), "#05");
			Assert.AreEqual ("1.234567890123460000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N99", _nfi), "#06");
			Assert.AreEqual ("N101", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N100", _nfi), "#07");
		}

		[Test]
		public void Test14029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			Assert.AreEqual ("-99,999,999#90", (-99999999.9).ToString ("N", nfi), "#01");
		}

		[Test]
		public void Test14030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			Assert.AreEqual ("1,000.00", 1000.0.ToString ("N", nfi), "#01");
			Assert.AreEqual ("0.00", 0.0.ToString ("N", nfi), "#02");
			Assert.AreEqual ("+1,000.00", (-1000.0).ToString ("N", nfi), "#03");
		}

		[Test]
		public void Test14031 ()
		{
			Assert.AreEqual ("Infinity", (Double.MaxValue / 0.0).ToString ("N99", _nfi) , "#01");
			Assert.AreEqual ("-Infinity", (Double.MinValue / 0.0).ToString ("N99", _nfi) , "#02");
			Assert.AreEqual ("NaN", (0.0 / 0.0).ToString ("N99", _nfi) , "#03");
		}

		[Test (Description = "Bug #659061")]
		public void Test14032 ()
		{
			NumberFormatInfo nfi = _nfi.Clone () as NumberFormatInfo;
			int[] groups = new int [10];

			for (int i = 0; i < groups.Length; i++)
				groups [i] = 1;
			nfi.NumberGroupSizes = groups;
			Assert.AreEqual ("2,5,5,5,6,6,6.65", (2555666.65).ToString ("N", nfi), "#01");

			for (int i = 0; i < groups.Length; i++)
				groups [i] = 2;
			nfi.NumberGroupSizes = groups;
			Assert.AreEqual ("2,55,56,66.65", (2555666.65).ToString ("N", nfi), "#02");

			for (int i = 0; i < groups.Length; i++)
				groups [i] = 3;
			nfi.NumberGroupSizes = groups;
			Assert.AreEqual ("2,555,666.65", (2555666.65).ToString ("N", nfi), "#03");

			for (int i = 0; i < groups.Length; i++)
				groups [i] = 4;
			nfi.NumberGroupSizes = groups;
			Assert.AreEqual ("255,5666.65", (2555666.65).ToString ("N", nfi), "#04");

			for (int i = 0; i < groups.Length; i++)
				groups [i] = 5;
			nfi.NumberGroupSizes = groups;
			Assert.AreEqual ("25,55666.65", (2555666.65).ToString ("N", nfi), "#05");

			for (int i = 0; i < groups.Length; i++)
				groups [i] = 6;
			nfi.NumberGroupSizes = groups;
			Assert.AreEqual ("2,555666.65", (2555666.65).ToString ("N", nfi), "#06");

			for (int i = 0; i < groups.Length; i++)
				groups [i] = 7;
			nfi.NumberGroupSizes = groups;
			Assert.AreEqual ("2555666.65", (2555666.65).ToString ("N", nfi), "#07");

			for (int i = 0; i < groups.Length; i++)
				groups [i] = 8;
			nfi.NumberGroupSizes = groups;
			Assert.AreEqual ("2555666.65", (2555666.65).ToString ("N", nfi), "#08");
		}

		[Test]
		public void Test14033 ()
		{
			NumberFormatInfo nfi = _nfi.Clone () as NumberFormatInfo;
			int[] groups = new int [] { 1, 2, 3 }; 

			nfi.NumberGroupSizes = groups;
			Assert.AreEqual ("2,555,66,6.65", (2555666.65).ToString ("N", nfi), "#01");
		}

		// Test15000- Double and P
		[Test]
		public void Test15000 ()
		{
			Assert.AreEqual ("0.00 %", 0.0.ToString ("P", _nfi), "#01");
			Assert.AreEqual ("0.00 %", 0.0.ToString ("p", _nfi), "#02");
			Assert.AreEqual ("-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00 %", Double.MinValue.ToString ("P", _nfi), "#03");
			Assert.AreEqual ("-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00 %", Double.MinValue.ToString ("p", _nfi), "#04");
			Assert.AreEqual ("17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00 %", Double.MaxValue.ToString ("P", _nfi), "#05");
			Assert.AreEqual ("17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00 %", Double.MaxValue.ToString ("p", _nfi), "#06");
		}

		[Test]
		public void Test15001 ()
		{
			Assert.AreEqual ("P ", 0.0.ToString ("P ", _nfi), "#01");
			Assert.AreEqual (" P", 0.0.ToString (" P", _nfi), "#02");
			Assert.AreEqual (" P ", 0.0.ToString (" P ", _nfi), "#03");
		}

		[Test]
		public void Test15002 ()
		{
			Assert.AreEqual ("-P ", (-1.0).ToString ("P ", _nfi), "#01");
			Assert.AreEqual ("- P", (-1.0).ToString (" P", _nfi), "#02");
			Assert.AreEqual ("- P ", (-1.0).ToString (" P ", _nfi), "#03");
		}

		[Test]
		public void Test15003 ()
		{
			Assert.AreEqual ("0 %", 0.0.ToString ("P0", _nfi), "#01");
			Assert.AreEqual ("0.0000000000000000 %", 0.0.ToString ("P16", _nfi), "#02");
			Assert.AreEqual ("0.00000000000000000 %", 0.0.ToString ("P17", _nfi), "#03");
			Assert.AreEqual ("0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", 0.0.ToString ("P99", _nfi), "#04");
			Assert.AreEqual ("P100", 0.0.ToString ("P100", _nfi), "#05");
		}

		[Test]
		public void Test15004 ()
		{
			Assert.AreEqual ("17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000 %", Double.MaxValue.ToString ("P0", _nfi), "#01");
			Assert.AreEqual ("17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.0000000000000000 %", Double.MaxValue.ToString ("P16", _nfi), "#02");
			Assert.AreEqual ("17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000 %", Double.MaxValue.ToString ("P17", _nfi), "#03");
			Assert.AreEqual ("17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", Double.MaxValue.ToString ("P99", _nfi), "#04");
			Assert.AreEqual ("P1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P100", _nfi), "#05");
		}

		[Test]
		public void Test15005 ()
		{
			Assert.AreEqual ("-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000 %", Double.MinValue.ToString ("P0", _nfi), "#01");
			Assert.AreEqual ("-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.0000000000000000 %", Double.MinValue.ToString ("P16", _nfi), "#02");
			Assert.AreEqual ("-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000 %", Double.MinValue.ToString ("P17", _nfi), "#03");
			Assert.AreEqual ("-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", Double.MinValue.ToString ("P99", _nfi), "#04");
			Assert.AreEqual ("P1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P100", _nfi), "#05");
		}

		[Test]
		public void Test15006 ()
		{
			Assert.AreEqual ("PF", 0.0.ToString ("PF", _nfi), "#01");
			Assert.AreEqual ("P0F", 0.0.ToString ("P0F", _nfi), "#02");
			Assert.AreEqual ("P0xF", 0.0.ToString ("P0xF", _nfi), "#03");
		}

		[Test]
		public void Test15007 ()
		{
			Assert.AreEqual ("PF", Double.MaxValue.ToString ("PF", _nfi), "#01");
			Assert.AreEqual ("P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MaxValue.ToString ("P0F", _nfi), "#02");
			Assert.AreEqual ("P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MaxValue.ToString ("P0xF", _nfi), "#03");
		}

		[Test]
		public void Test15008 ()
		{
			Assert.AreEqual ("-PF", Double.MinValue.ToString ("PF", _nfi), "#01");
			Assert.AreEqual ("-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MinValue.ToString ("P0F", _nfi), "#02");
			Assert.AreEqual ("-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MinValue.ToString ("P0xF", _nfi), "#03");
		}

		[Test]
		public void Test15009 ()
		{
			Assert.AreEqual ("0.00000000000000000 %", 0.0.ToString ("P0000000000000000000000000000000000000017", _nfi), "#01");
			Assert.AreEqual ("17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000 %", Double.MaxValue.ToString ("P0000000000000000000000000000000000000017", _nfi), "#02");
			Assert.AreEqual ("-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000 %", Double.MinValue.ToString ("P0000000000000000000000000000000000000017", _nfi), "#03");
		}

		[Test]
		public void Test15010 ()
		{
			Assert.AreEqual ("+P", 0.0.ToString ("+P", _nfi), "#01");
			Assert.AreEqual ("P+", 0.0.ToString ("P+", _nfi), "#02");
			Assert.AreEqual ("+P+", 0.0.ToString ("+P+", _nfi), "#03");
		}
		
		[Test]
		public void Test15011 ()
		{
			Assert.AreEqual ("+P", Double.MaxValue.ToString ("+P", _nfi), "#01");
			Assert.AreEqual ("P+", Double.MaxValue.ToString ("P+", _nfi), "#02");
			Assert.AreEqual ("+P+", Double.MaxValue.ToString ("+P+", _nfi), "#03");
		}

		[Test]
		public void Test15012 ()
		{
			Assert.AreEqual ("-+P", Double.MinValue.ToString ("+P", _nfi), "#01");
			Assert.AreEqual ("-P+", Double.MinValue.ToString ("P+", _nfi), "#02");
			Assert.AreEqual ("-+P+", Double.MinValue.ToString ("+P+", _nfi), "#03");
		}

		[Test]
		public void Test15013 ()
		{
			Assert.AreEqual ("-P", 0.0.ToString ("-P", _nfi), "#01");
			Assert.AreEqual ("P-", 0.0.ToString ("P-", _nfi), "#02");
			Assert.AreEqual ("-P-", 0.0.ToString ("-P-", _nfi), "#03");
		}
		
		[Test]
		public void Test15014 ()
		{
			Assert.AreEqual ("-P", Double.MaxValue.ToString ("-P", _nfi), "#01");
			Assert.AreEqual ("P-", Double.MaxValue.ToString ("P-", _nfi), "#02");
			Assert.AreEqual ("-P-", Double.MaxValue.ToString ("-P-", _nfi), "#03");
		}

		[Test]
		public void Test15015 ()
		{
			Assert.AreEqual ("--P", Double.MinValue.ToString ("-P", _nfi), "#01");
			Assert.AreEqual ("-P-", Double.MinValue.ToString ("P-", _nfi), "#02");
			Assert.AreEqual ("--P-", Double.MinValue.ToString ("-P-", _nfi), "#03");
		}

		[Test]
		public void Test15016 ()
		{
			Assert.AreEqual ("P+0", 0.0.ToString ("P+0", _nfi), "#01");
			Assert.AreEqual ("P+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P+0", _nfi), "#02");
			Assert.AreEqual ("-P+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("P+0", _nfi), "#03");
		}

		[Test]
		public void Test15017 ()
		{
			Assert.AreEqual ("P+9", 0.0.ToString ("P+9", _nfi), "#01");
			Assert.AreEqual ("P+9", Double.MaxValue.ToString ("P+9", _nfi), "#02");
			Assert.AreEqual ("-P+9", Double.MinValue.ToString ("P+9", _nfi), "#03");
		}

		[Test]
		public void Test15018 ()
		{
			Assert.AreEqual ("P-9", 0.0.ToString ("P-9", _nfi), "#01");
			Assert.AreEqual ("P-9", Double.MaxValue.ToString ("P-9", _nfi), "#02");
			Assert.AreEqual ("-P-9", Double.MinValue.ToString ("P-9", _nfi), "#03");
		}

		[Test]
		public void Test15019 ()
		{
			Assert.AreEqual ("P0", 0.0.ToString ("P0,", _nfi), "#01");
			Assert.AreEqual ("P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P0,", _nfi), "#02");
			Assert.AreEqual ("-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("P0,", _nfi), "#03");
		}

		[Test]
		public void Test15020 ()
		{
			Assert.AreEqual ("P0", 0.0.ToString ("P0.", _nfi), "#01");
			Assert.AreEqual ("P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P0.", _nfi), "#02");
			Assert.AreEqual ("-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("P0.", _nfi), "#03");
		}

		[Test]
		public void Test15021 ()
		{
			Assert.AreEqual ("P0.0", 0.0.ToString ("P0.0", _nfi), "#01");
			Assert.AreEqual ("P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MaxValue.ToString ("P0.0", _nfi), "#02");
			Assert.AreEqual ("-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MinValue.ToString ("P0.0", _nfi), "#03");
		}

		[Test]
		public void Test15022 ()
		{
			Assert.AreEqual ("P09", 0.0.ToString ("P0.9", _nfi), "#01");
			Assert.AreEqual ("P1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MaxValue.ToString ("P0.9", _nfi), "#02");
			Assert.AreEqual ("-P1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MinValue.ToString ("P0.9", _nfi), "#03");
		}

		[Test]
		public void Test15023 ()
		{
			Assert.AreEqual ("999.1 %", 9.9905.ToString ("P1", _nfi), "#01");
			Assert.AreEqual ("999.2 %", 9.9915.ToString ("P1", _nfi), "#02");
			Assert.AreEqual ("999.3 %", 9.9925.ToString ("P1", _nfi), "#03");
			Assert.AreEqual ("999.4 %", 9.9935.ToString ("P1", _nfi), "#04");
			Assert.AreEqual ("999.5 %", 9.9945.ToString ("P1", _nfi), "#05");
			Assert.AreEqual ("999.6 %", 9.9955.ToString ("P1", _nfi), "#06");
			Assert.AreEqual ("999.7 %", 9.9965.ToString ("P1", _nfi), "#07");
			Assert.AreEqual ("999.8 %", 9.9975.ToString ("P1", _nfi), "#08");
			Assert.AreEqual ("999.9 %", 9.9985.ToString ("P1", _nfi), "#09");
			Assert.AreEqual ("1,000.0 %", 9.9995.ToString ("P1", _nfi), "#10");
		}

		[Test]
		public void Test15024 ()
		{
			Assert.AreEqual ("999.91 %", 9.99905.ToString ("P2", _nfi), "#01");
			Assert.AreEqual ("999.92 %", 9.99915.ToString ("P2", _nfi), "#02");
			Assert.AreEqual ("999.93 %", 9.99925.ToString ("P2", _nfi), "#03");
			Assert.AreEqual ("999.94 %", 9.99935.ToString ("P2", _nfi), "#04");
			Assert.AreEqual ("999.95 %", 9.99945.ToString ("P2", _nfi), "#05");
			Assert.AreEqual ("999.96 %", 9.99955.ToString ("P2", _nfi), "#06");
			Assert.AreEqual ("999.97 %", 9.99965.ToString ("P2", _nfi), "#07");
			Assert.AreEqual ("999.98 %", 9.99975.ToString ("P2", _nfi), "#08");
			Assert.AreEqual ("999.99 %", 9.99985.ToString ("P2", _nfi), "#09");
			Assert.AreEqual ("1,000.00 %", 9.99995.ToString ("P2", _nfi), "#10");
		}

		[Test]
		public void Test15025 ()
		{
			Assert.AreEqual ("999.99999999991 %", 9.99999999999905.ToString ("P11", _nfi), "#01");
			Assert.AreEqual ("999.99999999992 %", 9.99999999999915.ToString ("P11", _nfi), "#02");
			Assert.AreEqual ("999.99999999993 %", 9.99999999999925.ToString ("P11", _nfi), "#03");
			Assert.AreEqual ("999.99999999994 %", 9.99999999999935.ToString ("P11", _nfi), "#04");
			Assert.AreEqual ("999.99999999995 %", 9.99999999999945.ToString ("P11", _nfi), "#05");
			Assert.AreEqual ("999.99999999996 %", 9.99999999999955.ToString ("P11", _nfi), "#06");
			Assert.AreEqual ("999.99999999997 %", 9.99999999999965.ToString ("P11", _nfi), "#07");
			Assert.AreEqual ("999.99999999998 %", 9.99999999999975.ToString ("P11", _nfi), "#08");
			Assert.AreEqual ("999.99999999999 %", 9.99999999999985.ToString ("P11", _nfi), "#09");
			Assert.AreEqual ("1,000.00000000000 %", 9.99999999999995.ToString ("P11", _nfi), "#10");
		}

		[Test]
		public void Test15026 ()
		{
			Assert.AreEqual ("999.999999999991 %", 9.999999999999905.ToString ("P12", _nfi), "#01");
			Assert.AreEqual ("999.999999999991 %", 9.999999999999915.ToString ("P12", _nfi), "#02");
			Assert.AreEqual ("999.999999999993 %", 9.999999999999925.ToString ("P12", _nfi), "#03");
			Assert.AreEqual ("999.999999999993 %", 9.999999999999935.ToString ("P12", _nfi), "#04");
			Assert.AreEqual ("999.999999999994 %", 9.999999999999945.ToString ("P12", _nfi), "#05");
			Assert.AreEqual ("999.999999999996 %", 9.999999999999955.ToString ("P12", _nfi), "#06");
			Assert.AreEqual ("999.999999999996 %", 9.999999999999965.ToString ("P12", _nfi), "#07");
			Assert.AreEqual ("999.999999999998 %", 9.999999999999975.ToString ("P12", _nfi), "#08");
			Assert.AreEqual ("999.999999999999 %", 9.999999999999985.ToString ("P12", _nfi), "#09");
			Assert.AreEqual ("999.999999999999 %", 9.999999999999995.ToString ("P12", _nfi), "#10");
		}

		[Test]
		public void Test15027 ()
		{
			Assert.AreEqual ("999.9999999999990 %", 9.9999999999999905.ToString ("P13", _nfi), "#01");
			Assert.AreEqual ("999.9999999999990 %", 9.9999999999999915.ToString ("P13", _nfi), "#02");
			Assert.AreEqual ("999.9999999999990 %", 9.9999999999999925.ToString ("P13", _nfi), "#03");
			Assert.AreEqual ("999.9999999999990 %", 9.9999999999999935.ToString ("P13", _nfi), "#04");
			Assert.AreEqual ("999.9999999999990 %", 9.9999999999999945.ToString ("P13", _nfi), "#05");
			Assert.AreEqual ("999.9999999999990 %", 9.9999999999999955.ToString ("P13", _nfi), "#06");
			Assert.AreEqual ("1,000.0000000000000 %", 9.9999999999999965.ToString ("P13", _nfi), "#07");
			Assert.AreEqual ("1,000.0000000000000 %", 9.9999999999999975.ToString ("P13", _nfi), "#08");
			Assert.AreEqual ("1,000.0000000000000 %", 9.9999999999999985.ToString ("P13", _nfi), "#09");
			Assert.AreEqual ("1,000.0000000000000 %", 9.9999999999999995.ToString ("P13", _nfi), "#10");
		}

		[Test]
		public void Test15028 ()
		{
			Assert.AreEqual ("1", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N0", _nfi), "#01");
			Assert.AreEqual ("1.234567890123", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N12", _nfi), "#02");
			Assert.AreEqual ("1.2345678901235", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N13", _nfi), "#03");
			Assert.AreEqual ("1.23456789012346", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N14", _nfi), "#04");
			Assert.AreEqual ("1.234567890123460", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N15", _nfi), "#05");
			Assert.AreEqual ("1.234567890123460000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N99", _nfi), "#06");
			Assert.AreEqual ("N101", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N100"), "#07");
		}

		[Test]
		public void Test15029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentDecimalSeparator = "#";
			Assert.AreEqual ("-9,999,999,990#00 %", (-99999999.9).ToString ("P", nfi), "#01");
		}

		[Test]
		public void Test15030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			Assert.AreEqual ("1,000.00 %", 10.0.ToString ("P", nfi), "#01");
			Assert.AreEqual ("0.00 %", 0.0.ToString ("P", nfi), "#02");
			Assert.AreEqual ("+1,000.00 %", (-10.0).ToString ("P", nfi), "#03");
		}

		[Test]
		public void Test15031 ()
		{
			Assert.AreEqual ("Infinity", (Double.MaxValue / 0.0).ToString ("N99", _nfi) , "#01");
			Assert.AreEqual ("-Infinity", (Double.MinValue / 0.0).ToString ("N99", _nfi) , "#02");
			Assert.AreEqual ("NaN", (0.0 / 0.0).ToString ("N99", _nfi) , "#03");
		}

		// TestRoundtrip for double and single
		[Test]
		public void TestRoundtrip()
		{
			Assert.AreEqual ("1.2345678901234567", 1.2345678901234567890.ToString ("R", _nfi), "#01");
			Assert.AreEqual ("1.2345678901234567", 1.2345678901234567890.ToString ("r", _nfi), "#02");
			Assert.AreEqual ("1.2345678901234567", 1.2345678901234567890.ToString ("R0", _nfi), "#03");
			Assert.AreEqual ("1.2345678901234567", 1.2345678901234567890.ToString ("r0", _nfi), "#04");
			Assert.AreEqual ("1.2345678901234567", 1.2345678901234567890.ToString ("R99", _nfi), "#05");
			Assert.AreEqual ("1.2345678901234567", 1.2345678901234567890.ToString ("r99", _nfi), "#06");
			Assert.AreEqual ("-1.7976931348623157E+308", Double.MinValue.ToString ("R"), "#07");
			Assert.AreEqual ("1.7976931348623157E+308", Double.MaxValue.ToString ("R"), "#08");
			Assert.AreEqual ("-1.7976931348623147E+308", (-1.7976931348623147E+308).ToString("R"), "#09");
			Assert.AreEqual ("-3.40282347E+38", Single.MinValue.ToString("R"), "#10");
			Assert.AreEqual ("3.40282347E+38", Single.MaxValue.ToString("R"), "#11");
		}

		// Tests arithmetic overflow in double.ToString exposed by Bug #383531
		[Test]
		public void TestToStringOverflow()
		{
			// Test all the possible double exponents with the maximal mantissa
            long dblPattern = 0xfffffffffffff; // all 1s significand

            for (long exp = 0; exp < 4096; exp++) {
                double val = BitConverter.Int64BitsToDouble((long)(dblPattern | (exp << 52)));
                string strRes = val.ToString("R", NumberFormatInfo.InvariantInfo);
				double rndTripVal = Double.Parse(strRes);
				Assert.AreEqual (val, rndTripVal, "Iter#" + exp);
            }
		}

		// Test17000 - Double and X
		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Test17000 ()
		{
			Assert.AreEqual ("", 0.0.ToString ("X99", _nfi) , "#01");
		}

		[Test]
		public void Test18000 ()
		{
			string formatString = "p 00.0000\\';n 0000.00\\';0.#\\'";

			Assert.AreEqual ("p 08.3266'", 8.32663472.ToString (formatString, CultureInfo.InvariantCulture), "#1");
			Assert.AreEqual ("n 0001.13'", (-1.1345343).ToString (formatString, CultureInfo.InvariantCulture), "#2");
			Assert.AreEqual ("0'", 0.0.ToString (formatString, CultureInfo.InvariantCulture), "#3");
		}

		[Test]
		[Category ("MultiThreaded")]
		public void TestInvariantThreading ()
		{
			Thread[] th = new Thread[4];
			bool failed = false;

			for (int i = 0; i < th.Length; i++) {
				th [i] = new Thread (() => {
					for (int ii = 0; ii < 100; ++ii) {
						var headers = new StringBuilder ();
						headers.AppendFormat (CultureInfo.InvariantCulture, "{0} {1}", 100, "ok");
						if (headers.ToString () != "100 ok") {
							failed = true;
						}
					}
				});
				th [i].Start ();
			}

			foreach (Thread t in th) {
				t.Join ();
			}

			Assert.IsFalse (failed);
		}
	}
}
