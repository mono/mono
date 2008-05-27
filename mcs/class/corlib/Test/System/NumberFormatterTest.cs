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

using NUnit.Framework;

namespace MonoTests.System
{
	[TestFixture]
	public class NumberFormatterTest : Assertion
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
			AssertEquals ("#01", "0", 0.ToString ("D", _nfi));
			AssertEquals ("#02", "0", 0.ToString ("d", _nfi));
			AssertEquals ("#03", "-2147483648", Int32.MinValue.ToString ("D", _nfi));
			AssertEquals ("#04", "-2147483648", Int32.MinValue.ToString ("d", _nfi));
			AssertEquals ("#05", "2147483647", Int32.MaxValue.ToString ("D", _nfi));
			AssertEquals ("#06", "2147483647", Int32.MaxValue.ToString ("d", _nfi));
		}

		[Test]
		public void Test00001 ()
		{
			AssertEquals ("#01", "D ", 0.ToString ("D ", _nfi));
			AssertEquals ("#02", " D", 0.ToString (" D", _nfi));
			AssertEquals ("#03", " D ", 0.ToString (" D ", _nfi));
		}

		[Test]
		public void Test00002 ()
		{
			AssertEquals ("#01", "-D ", (-1).ToString ("D ", _nfi));
			AssertEquals ("#02", "- D", (-1).ToString (" D", _nfi));
			AssertEquals ("#03", "- D ", (-1).ToString (" D ", _nfi));
		}

		[Test]
		public void Test00003 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("D0", _nfi));
			AssertEquals ("#02", "0000000000", 0.ToString ("D10", _nfi));
			AssertEquals ("#03", "00000000000", 0.ToString ("D11", _nfi));
			AssertEquals ("#04", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("D99", _nfi));
			AssertEquals ("#05", "D100", 0.ToString ("D100", _nfi));
		}

		[Test]
		public void Test00004 ()
		{
			AssertEquals ("#01", "2147483647", Int32.MaxValue.ToString ("D0", _nfi));
			AssertEquals ("#02", "2147483647", Int32.MaxValue.ToString ("D10", _nfi));
			AssertEquals ("#03", "02147483647", Int32.MaxValue.ToString ("D11", _nfi));
			AssertEquals ("#04", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002147483647", Int32.MaxValue.ToString ("D99", _nfi));
			AssertEquals ("#05", "D12147483647", Int32.MaxValue.ToString ("D100", _nfi));
		}

		[Test]
		public void Test00005 ()
		{
			AssertEquals ("#01", "-2147483648", Int32.MinValue.ToString ("D0", _nfi));
			AssertEquals ("#02", "-2147483648", Int32.MinValue.ToString ("D10", _nfi));
			AssertEquals ("#03", "-02147483648", Int32.MinValue.ToString ("D11", _nfi));
			AssertEquals ("#04", "-000000000000000000000000000000000000000000000000000000000000000000000000000000000000000002147483648", Int32.MinValue.ToString ("D99", _nfi));
			AssertEquals ("#05", "-D12147483648", Int32.MinValue.ToString ("D100", _nfi));
		}

		[Test]
		public void Test00006 ()
		{
			AssertEquals ("#01", "DF", 0.ToString ("DF", _nfi));
			AssertEquals ("#02", "D0F", 0.ToString ("D0F", _nfi));
			AssertEquals ("#03", "D0xF", 0.ToString ("D0xF", _nfi));
		}

		[Test]
		public void Test00007 ()
		{
			AssertEquals ("#01", "DF", Int32.MaxValue.ToString ("DF", _nfi));
			AssertEquals ("#02", "D2147483647F", Int32.MaxValue.ToString ("D0F", _nfi));
			AssertEquals ("#03", "D2147483647xF", Int32.MaxValue.ToString ("D0xF", _nfi));
		}

		[Test]
		public void Test00008 ()
		{
			AssertEquals ("#01", "-DF", Int32.MinValue.ToString ("DF", _nfi));
			AssertEquals ("#02", "-D2147483648F", Int32.MinValue.ToString ("D0F", _nfi));
			AssertEquals ("#03", "-D2147483648xF", Int32.MinValue.ToString ("D0xF", _nfi));
		}

		[Test]
		public void Test00009 ()
		{
			AssertEquals ("#01", "00000000000", 0.ToString ("D0000000000000000000000000000000000000011", _nfi));
			AssertEquals ("#02", "02147483647", Int32.MaxValue.ToString ("D0000000000000000000000000000000000000011", _nfi));
			AssertEquals ("#03", "-02147483648", Int32.MinValue.ToString ("D0000000000000000000000000000000000000011", _nfi));
		}

		[Test]
		public void Test00010 ()
		{
			AssertEquals ("#01", "+D", 0.ToString ("+D", _nfi));
			AssertEquals ("#02", "D+", 0.ToString ("D+", _nfi));
			AssertEquals ("#03", "+D+", 0.ToString ("+D+", _nfi));
		}
		
		[Test]
		public void Test00011 ()
		{
			AssertEquals ("#01", "+D", Int32.MaxValue.ToString ("+D", _nfi));
			AssertEquals ("#02", "D+", Int32.MaxValue.ToString ("D+", _nfi));
			AssertEquals ("#03", "+D+", Int32.MaxValue.ToString ("+D+", _nfi));
		}

		[Test]
		public void Test00012 ()
		{
			AssertEquals ("#01", "-+D", Int32.MinValue.ToString ("+D", _nfi));
			AssertEquals ("#02", "-D+", Int32.MinValue.ToString ("D+", _nfi));
			AssertEquals ("#03", "-+D+", Int32.MinValue.ToString ("+D+", _nfi));
		}

		[Test]
		public void Test00013 ()
		{
			AssertEquals ("#01", "-D", 0.ToString ("-D", _nfi));
			AssertEquals ("#02", "D-", 0.ToString ("D-", _nfi));
			AssertEquals ("#03", "-D-", 0.ToString ("-D-", _nfi));
		}
		
		[Test]
		public void Test00014 ()
		{
			AssertEquals ("#01", "-D", Int32.MaxValue.ToString ("-D", _nfi));
			AssertEquals ("#02", "D-", Int32.MaxValue.ToString ("D-", _nfi));
			AssertEquals ("#03", "-D-", Int32.MaxValue.ToString ("-D-", _nfi));
		}

		[Test]
		public void Test00015 ()
		{
			AssertEquals ("#01", "--D", Int32.MinValue.ToString ("-D", _nfi));
			AssertEquals ("#02", "-D-", Int32.MinValue.ToString ("D-", _nfi));
			AssertEquals ("#03", "--D-", Int32.MinValue.ToString ("-D-", _nfi));
		}

		[Test]
		public void Test00016 ()
		{
			AssertEquals ("#01", "D+0", 0.ToString ("D+0", _nfi));
			AssertEquals ("#02", "D+2147483647", Int32.MaxValue.ToString ("D+0", _nfi));
			AssertEquals ("#03", "-D+2147483648", Int32.MinValue.ToString ("D+0", _nfi));
		}

		[Test]
		public void Test00017 ()
		{
			AssertEquals ("#01", "D+9", 0.ToString ("D+9", _nfi));
			AssertEquals ("#02", "D+9", Int32.MaxValue.ToString ("D+9", _nfi));
			AssertEquals ("#03", "-D+9", Int32.MinValue.ToString ("D+9", _nfi));
		}

		[Test]
		public void Test00018 ()
		{
			AssertEquals ("#01", "D-9", 0.ToString ("D-9", _nfi));
			AssertEquals ("#02", "D-9", Int32.MaxValue.ToString ("D-9", _nfi));
			AssertEquals ("#03", "-D-9", Int32.MinValue.ToString ("D-9", _nfi));
		}

		[Test]
		public void Test00019 ()
		{
			AssertEquals ("#01", "D0", 0.ToString ("D0,", _nfi));
			AssertEquals ("#02", "D2147484", Int32.MaxValue.ToString ("D0,", _nfi));
			AssertEquals ("#03", "-D2147484", Int32.MinValue.ToString ("D0,", _nfi));
		}

		[Test]
		public void Test00020 ()
		{
			AssertEquals ("#01", "D0", 0.ToString ("D0.", _nfi));
			AssertEquals ("#02", "D2147483647", Int32.MaxValue.ToString ("D0.", _nfi));
			AssertEquals ("#03", "-D2147483648", Int32.MinValue.ToString ("D0.", _nfi));
		}

		[Test]
		public void Test00021 ()
		{
			AssertEquals ("#01", "D0.0", 0.ToString ("D0.0", _nfi));
			AssertEquals ("#02", "D2147483647.0", Int32.MaxValue.ToString ("D0.0", _nfi));
			AssertEquals ("#03", "-D2147483648.0", Int32.MinValue.ToString ("D0.0", _nfi));
		}

		[Test]
		public void Test00022 ()
		{
			AssertEquals ("#01", "D09", 0.ToString ("D0.9", _nfi));
			AssertEquals ("#02", "D21474836479", Int32.MaxValue.ToString ("D0.9", _nfi));
			AssertEquals ("#03", "-D21474836489", Int32.MinValue.ToString ("D0.9", _nfi));
		}

		// Test01000- Int32 and E
		[Test]
		public void Test01000 ()
		{
			AssertEquals ("#01", "0.000000E+000", 0.ToString ("E", _nfi));
			AssertEquals ("#02", "0.000000e+000", 0.ToString ("e", _nfi));
			AssertEquals ("#03", "-2.147484E+009", Int32.MinValue.ToString ("E", _nfi));
			AssertEquals ("#04", "-2.147484e+009", Int32.MinValue.ToString ("e", _nfi));
			AssertEquals ("#05", "2.147484E+009", Int32.MaxValue.ToString ("E", _nfi));
			AssertEquals ("#06", "2.147484e+009", Int32.MaxValue.ToString ("e", _nfi));
		}

		[Test]
		public void Test01001 ()
		{
			AssertEquals ("#01", "E ", 0.ToString ("E ", _nfi));
			AssertEquals ("#02", " E", 0.ToString (" E", _nfi));
			AssertEquals ("#03", " E ", 0.ToString (" E ", _nfi));
		}

		[Test]
		public void Test01002 ()
		{
			AssertEquals ("#01", "-E ", (-1).ToString ("E ", _nfi));
			AssertEquals ("#02", "- E", (-1).ToString (" E", _nfi));
			AssertEquals ("#03", "- E ", (-1).ToString (" E ", _nfi));
		}

		[Test]
		public void Test01003 ()
		{
			AssertEquals ("#01", "0E+000", 0.ToString ("E0", _nfi));
			AssertEquals ("#02", "0.000000000E+000", 0.ToString ("E9", _nfi));
			AssertEquals ("#03", "0.0000000000E+000", 0.ToString ("E10", _nfi));
			AssertEquals ("#04", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000E+000", 0.ToString ("E99", _nfi));
			AssertEquals ("#05", "E100", 0.ToString ("E100", _nfi));
		}

		[Test]
		public void Test01004 ()
		{
			AssertEquals ("#01", "2E+009", Int32.MaxValue.ToString ("E0", _nfi));
			AssertEquals ("#02", "2.147483647E+009", Int32.MaxValue.ToString ("E9", _nfi));
			AssertEquals ("#03", "2.1474836470E+009", Int32.MaxValue.ToString ("E10", _nfi));
			AssertEquals ("#04", "2.147483647000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000E+009", Int32.MaxValue.ToString ("E99", _nfi));
			AssertEquals ("#05", "E12147483647", Int32.MaxValue.ToString ("E100", _nfi));
		}

		[Test]
		public void Test01005 ()
		{
			AssertEquals ("#01", "-2E+009", Int32.MinValue.ToString ("E0", _nfi));
			AssertEquals ("#02", "-2.147483648E+009", Int32.MinValue.ToString ("E9", _nfi));
			AssertEquals ("#03", "-2.1474836480E+009", Int32.MinValue.ToString ("E10", _nfi));
			AssertEquals ("#04", "-2.147483648000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000E+009", Int32.MinValue.ToString ("E99", _nfi));
			AssertEquals ("#05", "-E12147483648", Int32.MinValue.ToString ("E100", _nfi));
		}

		[Test]
		public void Test01006 ()
		{
			AssertEquals ("#01", "EF", 0.ToString ("EF", _nfi));
			AssertEquals ("#02", "E0F", 0.ToString ("E0F", _nfi));
			AssertEquals ("#03", "E0xF", 0.ToString ("E0xF", _nfi));
		}

		[Test]
		public void Test01007 ()
		{
			AssertEquals ("#01", "EF", Int32.MaxValue.ToString ("EF", _nfi));
			AssertEquals ("#02", "E0F", Int32.MaxValue.ToString ("E0F", _nfi));
			AssertEquals ("#03", "E0xF", Int32.MaxValue.ToString ("E0xF", _nfi));
		}

		[Test]
		public void Test01008 ()
		{
			AssertEquals ("#01", "-EF", Int32.MinValue.ToString ("EF", _nfi));
			AssertEquals ("#02", "E0F", Int32.MinValue.ToString ("E0F", _nfi));
			AssertEquals ("#03", "E0xF", Int32.MinValue.ToString ("E0xF", _nfi));
		}

		[Test]
		public void Test01009 ()
		{
			AssertEquals ("#01", "0.0000000000E+000", 0.ToString ("E0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#02", "2.1474836470E+009", Int32.MaxValue.ToString ("E0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#03", "-2.1474836480E+009", Int32.MinValue.ToString ("E0000000000000000000000000000000000000010", _nfi));
		}

		[Test]
		public void Test01010 ()
		{
			AssertEquals ("#01", "+E", 0.ToString ("+E", _nfi));
			AssertEquals ("#02", "E+", 0.ToString ("E+", _nfi));
			AssertEquals ("#03", "+E+", 0.ToString ("+E+", _nfi));
		}
		
		[Test]
		public void Test01011 ()
		{
			AssertEquals ("#01", "+E", Int32.MaxValue.ToString ("+E", _nfi));
			AssertEquals ("#02", "E+", Int32.MaxValue.ToString ("E+", _nfi));
			AssertEquals ("#03", "+E+", Int32.MaxValue.ToString ("+E+", _nfi));
		}

		[Test]
		public void Test01012 ()
		{
			AssertEquals ("#01", "-+E", Int32.MinValue.ToString ("+E", _nfi));
			AssertEquals ("#02", "-E+", Int32.MinValue.ToString ("E+", _nfi));
			AssertEquals ("#03", "-+E+", Int32.MinValue.ToString ("+E+", _nfi));
		}

		[Test]
		public void Test01013 ()
		{
			AssertEquals ("#01", "-E", 0.ToString ("-E", _nfi));
			AssertEquals ("#02", "E-", 0.ToString ("E-", _nfi));
			AssertEquals ("#03", "-E-", 0.ToString ("-E-", _nfi));
		}
		
		[Test]
		public void Test01014 ()
		{
			AssertEquals ("#01", "-E", Int32.MaxValue.ToString ("-E", _nfi));
			AssertEquals ("#02", "E-", Int32.MaxValue.ToString ("E-", _nfi));
			AssertEquals ("#03", "-E-", Int32.MaxValue.ToString ("-E-", _nfi));
		}

		[Test]
		public void Test01015 ()
		{
			AssertEquals ("#01", "--E", Int32.MinValue.ToString ("-E", _nfi));
			AssertEquals ("#02", "-E-", Int32.MinValue.ToString ("E-", _nfi));
			AssertEquals ("#03", "--E-", Int32.MinValue.ToString ("-E-", _nfi));
		}

		[Test]
		public void Test01016 ()
		{
			AssertEquals ("#01", "E+0", 0.ToString ("E+0", _nfi));
			AssertEquals ("#02", "E+0", Int32.MaxValue.ToString ("E+0", _nfi));
			AssertEquals ("#03", "E+0", Int32.MinValue.ToString ("E+0", _nfi));
		}

		[Test]
		public void Test01017 ()
		{
			AssertEquals ("#01", "E+9", 0.ToString ("E+9", _nfi));
			AssertEquals ("#02", "E+9", Int32.MaxValue.ToString ("E+9", _nfi));
			AssertEquals ("#03", "-E+9", Int32.MinValue.ToString ("E+9", _nfi));
		}

		[Test]
		public void Test01018 ()
		{
			AssertEquals ("#01", "E-9", 0.ToString ("E-9", _nfi));
			AssertEquals ("#02", "E-9", Int32.MaxValue.ToString ("E-9", _nfi));
			AssertEquals ("#03", "-E-9", Int32.MinValue.ToString ("E-9", _nfi));
		}

		[Test]
		public void Test01019 ()
		{
			AssertEquals ("#01", "E0", 0.ToString ("E0,", _nfi));
			AssertEquals ("#02", "E0", Int32.MaxValue.ToString ("E0,", _nfi));
			AssertEquals ("#03", "E0", Int32.MinValue.ToString ("E0,", _nfi));
		}

		[Test]
		public void Test01020 ()
		{
			AssertEquals ("#01", "E0", 0.ToString ("E0.", _nfi));
			AssertEquals ("#02", "E0", Int32.MaxValue.ToString ("E0.", _nfi));
			AssertEquals ("#03", "E0", Int32.MinValue.ToString ("E0.", _nfi));
		}

		[Test]
		public void Test01021 ()
		{
			AssertEquals ("#01", "E0.0", 0.ToString ("E0.0", _nfi));
			AssertEquals ("#02", "E10.2", Int32.MaxValue.ToString ("E0.0", _nfi));
			AssertEquals ("#03", "-E10.2", Int32.MinValue.ToString ("E0.0", _nfi));
		}

		[Test]
		public void Test01022 ()
		{
			AssertEquals ("#01", "E09", 0.ToString ("E0.9", _nfi));
			AssertEquals ("#02", "E09", Int32.MaxValue.ToString ("E0.9", _nfi));
			AssertEquals ("#03", "E09", Int32.MinValue.ToString ("E0.9", _nfi));
		}

		[Test]
		public void Test01023 ()
		{
			AssertEquals ("#01", "9.999999E+007", 99999990.ToString ("E", _nfi));
			AssertEquals ("#02", "9.999999E+007", 99999991.ToString ("E", _nfi));
			AssertEquals ("#03", "9.999999E+007", 99999992.ToString ("E", _nfi));
			AssertEquals ("#04", "9.999999E+007", 99999993.ToString ("E", _nfi));
			AssertEquals ("#05", "9.999999E+007", 99999994.ToString ("E", _nfi));
			AssertEquals ("#06", "1.000000E+008", 99999995.ToString ("E", _nfi));
			AssertEquals ("#07", "1.000000E+008", 99999996.ToString ("E", _nfi));
			AssertEquals ("#08", "1.000000E+008", 99999997.ToString ("E", _nfi));
			AssertEquals ("#09", "1.000000E+008", 99999998.ToString ("E", _nfi));
			AssertEquals ("#10", "1.000000E+008", 99999999.ToString ("E", _nfi));
		}

		[Test]
		public void Test01024 ()
		{
			AssertEquals ("#01", "-9.999999E+007", (-99999990).ToString ("E", _nfi));
			AssertEquals ("#02", "-9.999999E+007", (-99999991).ToString ("E", _nfi));
			AssertEquals ("#03", "-9.999999E+007", (-99999992).ToString ("E", _nfi));
			AssertEquals ("#04", "-9.999999E+007", (-99999993).ToString ("E", _nfi));
			AssertEquals ("#05", "-9.999999E+007", (-99999994).ToString ("E", _nfi));
			AssertEquals ("#06", "-1.000000E+008", (-99999995).ToString ("E", _nfi));
			AssertEquals ("#07", "-1.000000E+008", (-99999996).ToString ("E", _nfi));
			AssertEquals ("#08", "-1.000000E+008", (-99999997).ToString ("E", _nfi));
			AssertEquals ("#09", "-1.000000E+008", (-99999998).ToString ("E", _nfi));
			AssertEquals ("#10", "-1.000000E+008", (-99999999).ToString ("E", _nfi));
		}

		[Test]
		public void Test01025 ()
		{
			AssertEquals ("#01", "9.999998E+007", 99999980.ToString ("E", _nfi));
			AssertEquals ("#02", "9.999998E+007", 99999981.ToString ("E", _nfi));
			AssertEquals ("#03", "9.999998E+007", 99999982.ToString ("E", _nfi));
			AssertEquals ("#04", "9.999998E+007", 99999983.ToString ("E", _nfi));
			AssertEquals ("#05", "9.999998E+007", 99999984.ToString ("E", _nfi));
			AssertEquals ("#06", "9.999999E+007", 99999985.ToString ("E", _nfi));
			AssertEquals ("#07", "9.999999E+007", 99999986.ToString ("E", _nfi));
			AssertEquals ("#08", "9.999999E+007", 99999987.ToString ("E", _nfi));
			AssertEquals ("#09", "9.999999E+007", 99999988.ToString ("E", _nfi));
			AssertEquals ("#10", "9.999999E+007", 99999989.ToString ("E", _nfi));
		}

		[Test]
		public void Test01026 ()
		{
			AssertEquals ("#01", "-9.999998E+007", (-99999980).ToString ("E", _nfi));
			AssertEquals ("#02", "-9.999998E+007", (-99999981).ToString ("E", _nfi));
			AssertEquals ("#03", "-9.999998E+007", (-99999982).ToString ("E", _nfi));
			AssertEquals ("#04", "-9.999998E+007", (-99999983).ToString ("E", _nfi));
			AssertEquals ("#05", "-9.999998E+007", (-99999984).ToString ("E", _nfi));
			AssertEquals ("#06", "-9.999999E+007", (-99999985).ToString ("E", _nfi));
			AssertEquals ("#07", "-9.999999E+007", (-99999986).ToString ("E", _nfi));
			AssertEquals ("#08", "-9.999999E+007", (-99999987).ToString ("E", _nfi));
			AssertEquals ("#09", "-9.999999E+007", (-99999988).ToString ("E", _nfi));
			AssertEquals ("#10", "-9.999999E+007", (-99999989).ToString ("E", _nfi));
		}

		[Test]
		public void Test01027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			AssertEquals ("#01", "-1#000000E+008", (-99999999).ToString ("E", nfi));
		}

		[Test]
		public void Test01028 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			AssertEquals ("#01", "1.000000E-000", 1.ToString ("E", nfi));
			AssertEquals ("#02", "0.000000E-000", 0.ToString ("E", nfi));
			AssertEquals ("#03", "+1.000000E-000", (-1).ToString ("E", nfi));
		}

		// Test02000- Int32 and F
		[Test]
		public void Test02000 ()
		{
			AssertEquals ("#01", "0.00", 0.ToString ("F", _nfi));
			AssertEquals ("#02", "0.00", 0.ToString ("f", _nfi));
			AssertEquals ("#03", "-2147483648.00", Int32.MinValue.ToString ("F", _nfi));
			AssertEquals ("#04", "-2147483648.00", Int32.MinValue.ToString ("f", _nfi));
			AssertEquals ("#05", "2147483647.00", Int32.MaxValue.ToString ("F", _nfi));
			AssertEquals ("#06", "2147483647.00", Int32.MaxValue.ToString ("f", _nfi));
		}

		[Test]
		public void Test02001 ()
		{
			AssertEquals ("#01", "F ", 0.ToString ("F ", _nfi));
			AssertEquals ("#02", " F", 0.ToString (" F", _nfi));
			AssertEquals ("#03", " F ", 0.ToString (" F ", _nfi));
		}

		[Test]
		public void Test02002 ()
		{
			AssertEquals ("#01", "-F ", (-1).ToString ("F ", _nfi));
			AssertEquals ("#02", "- F", (-1).ToString (" F", _nfi));
			AssertEquals ("#03", "- F ", (-1).ToString (" F ", _nfi));
		}

		[Test]
		public void Test02003 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("F0", _nfi));
			AssertEquals ("#02", "0.000000000", 0.ToString ("F9", _nfi));
			AssertEquals ("#03", "0.0000000000", 0.ToString ("F10", _nfi));
			AssertEquals ("#04", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("F99", _nfi));
			AssertEquals ("#05", "F100", 0.ToString ("F100", _nfi));
		}

		[Test]
		public void Test02004 ()
		{
			AssertEquals ("#01", "2147483647", Int32.MaxValue.ToString ("F0", _nfi));
			AssertEquals ("#02", "2147483647.000000000", Int32.MaxValue.ToString ("F9", _nfi));
			AssertEquals ("#03", "2147483647.0000000000", Int32.MaxValue.ToString ("F10", _nfi));
			AssertEquals ("#04", "2147483647.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Int32.MaxValue.ToString ("F99", _nfi));
			AssertEquals ("#05", "F12147483647", Int32.MaxValue.ToString ("F100", _nfi));
		}

		[Test]
		public void Test02005 ()
		{
			AssertEquals ("#01", "-2147483648", Int32.MinValue.ToString ("F0", _nfi));
			AssertEquals ("#02", "-2147483648.000000000", Int32.MinValue.ToString ("F9", _nfi));
			AssertEquals ("#03", "-2147483648.0000000000", Int32.MinValue.ToString ("F10", _nfi));
			AssertEquals ("#04", "-2147483648.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Int32.MinValue.ToString ("F99", _nfi));
			AssertEquals ("#05", "-F12147483648", Int32.MinValue.ToString ("F100", _nfi));
		}

		[Test]
		public void Test02006 ()
		{
			AssertEquals ("#01", "FF", 0.ToString ("FF", _nfi));
			AssertEquals ("#02", "F0F", 0.ToString ("F0F", _nfi));
			AssertEquals ("#03", "F0xF", 0.ToString ("F0xF", _nfi));
		}

		[Test]
		public void Test02007 ()
		{
			AssertEquals ("#01", "FF", Int32.MaxValue.ToString ("FF", _nfi));
			AssertEquals ("#02", "F2147483647F", Int32.MaxValue.ToString ("F0F", _nfi));
			AssertEquals ("#03", "F2147483647xF", Int32.MaxValue.ToString ("F0xF", _nfi));
		}

		[Test]
		public void Test02008 ()
		{
			AssertEquals ("#01", "-FF", Int32.MinValue.ToString ("FF", _nfi));
			AssertEquals ("#02", "-F2147483648F", Int32.MinValue.ToString ("F0F", _nfi));
			AssertEquals ("#03", "-F2147483648xF", Int32.MinValue.ToString ("F0xF", _nfi));
		}

		[Test]
		public void Test02009 ()
		{
			AssertEquals ("#01", "0.0000000000", 0.ToString ("F0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#02", "2147483647.0000000000", Int32.MaxValue.ToString ("F0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#03", "-2147483648.0000000000", Int32.MinValue.ToString ("F0000000000000000000000000000000000000010", _nfi));
		}

		[Test]
		public void Test02010 ()
		{
			AssertEquals ("#01", "+F", 0.ToString ("+F", _nfi));
			AssertEquals ("#02", "F+", 0.ToString ("F+", _nfi));
			AssertEquals ("#03", "+F+", 0.ToString ("+F+", _nfi));
		}
		
		[Test]
		public void Test02011 ()
		{
			AssertEquals ("#01", "+F", Int32.MaxValue.ToString ("+F", _nfi));
			AssertEquals ("#02", "F+", Int32.MaxValue.ToString ("F+", _nfi));
			AssertEquals ("#03", "+F+", Int32.MaxValue.ToString ("+F+", _nfi));
		}

		[Test]
		public void Test02012 ()
		{
			AssertEquals ("#01", "-+F", Int32.MinValue.ToString ("+F", _nfi));
			AssertEquals ("#02", "-F+", Int32.MinValue.ToString ("F+", _nfi));
			AssertEquals ("#03", "-+F+", Int32.MinValue.ToString ("+F+", _nfi));
		}

		[Test]
		public void Test02013 ()
		{
			AssertEquals ("#01", "-F", 0.ToString ("-F", _nfi));
			AssertEquals ("#02", "F-", 0.ToString ("F-", _nfi));
			AssertEquals ("#03", "-F-", 0.ToString ("-F-", _nfi));
		}
		
		[Test]
		public void Test02014 ()
		{
			AssertEquals ("#01", "-F", Int32.MaxValue.ToString ("-F", _nfi));
			AssertEquals ("#02", "F-", Int32.MaxValue.ToString ("F-", _nfi));
			AssertEquals ("#03", "-F-", Int32.MaxValue.ToString ("-F-", _nfi));
		}

		[Test]
		public void Test02015 ()
		{
			AssertEquals ("#01", "--F", Int32.MinValue.ToString ("-F", _nfi));
			AssertEquals ("#02", "-F-", Int32.MinValue.ToString ("F-", _nfi));
			AssertEquals ("#03", "--F-", Int32.MinValue.ToString ("-F-", _nfi));
		}

		[Test]
		public void Test02016 ()
		{
			AssertEquals ("#01", "F+0", 0.ToString ("F+0", _nfi));
			AssertEquals ("#02", "F+2147483647", Int32.MaxValue.ToString ("F+0", _nfi));
			AssertEquals ("#03", "-F+2147483648", Int32.MinValue.ToString ("F+0", _nfi));
		}

		[Test]
		public void Test02017 ()
		{
			AssertEquals ("#01", "F+9", 0.ToString ("F+9", _nfi));
			AssertEquals ("#02", "F+9", Int32.MaxValue.ToString ("F+9", _nfi));
			AssertEquals ("#03", "-F+9", Int32.MinValue.ToString ("F+9", _nfi));
		}

		[Test]
		public void Test02018 ()
		{
			AssertEquals ("#01", "F-9", 0.ToString ("F-9", _nfi));
			AssertEquals ("#02", "F-9", Int32.MaxValue.ToString ("F-9", _nfi));
			AssertEquals ("#03", "-F-9", Int32.MinValue.ToString ("F-9", _nfi));
		}

		[Test]
		public void Test02019 ()
		{
			AssertEquals ("#01", "F0", 0.ToString ("F0,", _nfi));
			AssertEquals ("#02", "F2147484", Int32.MaxValue.ToString ("F0,", _nfi));
			AssertEquals ("#03", "-F2147484", Int32.MinValue.ToString ("F0,", _nfi));
		}

		[Test]
		public void Test02020 ()
		{
			AssertEquals ("#01", "F0", 0.ToString ("F0.", _nfi));
			AssertEquals ("#02", "F2147483647", Int32.MaxValue.ToString ("F0.", _nfi));
			AssertEquals ("#03", "-F2147483648", Int32.MinValue.ToString ("F0.", _nfi));
		}

		[Test]
		public void Test02021 ()
		{
			AssertEquals ("#01", "F0.0", 0.ToString ("F0.0", _nfi));
			AssertEquals ("#02", "F2147483647.0", Int32.MaxValue.ToString ("F0.0", _nfi));
			AssertEquals ("#03", "-F2147483648.0", Int32.MinValue.ToString ("F0.0", _nfi));
		}

		[Test]
		public void Test02022 ()
		{
			AssertEquals ("#01", "F09", 0.ToString ("F0.9", _nfi));
			AssertEquals ("#02", "F21474836479", Int32.MaxValue.ToString ("F0.9", _nfi));
			AssertEquals ("#03", "-F21474836489", Int32.MinValue.ToString ("F0.9", _nfi));
		}

		[Test]
		public void Test02023 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalDigits = 0;
			AssertEquals ("#01", "0", 0.ToString ("F", nfi));
			nfi.NumberDecimalDigits = 1;
			AssertEquals ("#02", "0.0", 0.ToString ("F", nfi));
			nfi.NumberDecimalDigits = 99;
			AssertEquals ("#03", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("F", nfi));
		}

		[Test]
		public void Test02024 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "";
			AssertEquals ("#01", "2147483648.00", Int32.MinValue.ToString ("F", nfi));
			nfi.NegativeSign = "-";
			AssertEquals ("#02", "-2147483648.00", Int32.MinValue.ToString ("F", nfi));
			nfi.NegativeSign = "+";
			AssertEquals ("#03", "+2147483648.00", Int32.MinValue.ToString ("F", nfi));
			nfi.NegativeSign = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			AssertEquals ("#04", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ2147483648.00", Int32.MinValue.ToString ("F", nfi));
		}

		[Test]
		public void Test02025 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "-";
			nfi.PositiveSign = "+";
			AssertEquals ("#01", "-1.00", (-1).ToString ("F", nfi));
			AssertEquals ("#02", "0.00", 0.ToString ("F", nfi));
			AssertEquals ("#03", "1.00",1.ToString ("F", nfi));
		}

		[Test]
		public void Test02026 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";
			AssertEquals ("#01", "+1.00", (-1).ToString ("F", nfi));
			AssertEquals ("#02", "0.00", 0.ToString ("F", nfi));
			AssertEquals ("#03", "1.00",1.ToString ("F", nfi));
		}

		[Test]
		public void Test02027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			AssertEquals ("#01", "1#00",1.ToString ("F", nfi));
		}

		// Test03000 - Int32 and G
		[Test]
		public void Test03000 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("G", _nfi));
			AssertEquals ("#02", "0", 0.ToString ("g", _nfi));
			AssertEquals ("#03", "-2147483648", Int32.MinValue.ToString ("G", _nfi));
			AssertEquals ("#04", "-2147483648", Int32.MinValue.ToString ("g", _nfi));
			AssertEquals ("#05", "2147483647", Int32.MaxValue.ToString ("G", _nfi));
			AssertEquals ("#06", "2147483647", Int32.MaxValue.ToString ("g", _nfi));
		}

		[Test]
		public void Test03001 ()
		{
			AssertEquals ("#01", "G ", 0.ToString ("G ", _nfi));
			AssertEquals ("#02", " G", 0.ToString (" G", _nfi));
			AssertEquals ("#03", " G ", 0.ToString (" G ", _nfi));
		}

		[Test]
		public void Test03002 ()
		{
			AssertEquals ("#01", "-G ", (-1).ToString ("G ", _nfi));
			AssertEquals ("#02", "- G", (-1).ToString (" G", _nfi));
			AssertEquals ("#03", "- G ", (-1).ToString (" G ", _nfi));
		}

		[Test]
		public void Test03003 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("G0", _nfi));
			AssertEquals ("#02", "0", 0.ToString ("G9", _nfi));
			AssertEquals ("#03", "0", 0.ToString ("G10", _nfi));
			AssertEquals ("#04", "0", 0.ToString ("G99", _nfi));
			AssertEquals ("#05", "G100", 0.ToString ("G100", _nfi));
		}

		[Test]
		public void Test03004 ()
		{
			AssertEquals ("#01", "2147483647", Int32.MaxValue.ToString ("G0", _nfi));
			AssertEquals ("#02", "2.14748365E+09", Int32.MaxValue.ToString ("G9", _nfi));
			AssertEquals ("#03", "2147483647", Int32.MaxValue.ToString ("G10", _nfi));
			AssertEquals ("#04", "2147483647", Int32.MaxValue.ToString ("G99", _nfi));
			AssertEquals ("#05", "G12147483647", Int32.MaxValue.ToString ("G100", _nfi));
		}

		[Test]
		public void Test03005 ()
		{
			AssertEquals ("#01", "-2147483648", Int32.MinValue.ToString ("G0", _nfi));
			AssertEquals ("#02", "-2.14748365E+09", Int32.MinValue.ToString ("G9", _nfi));
			AssertEquals ("#03", "-2147483648", Int32.MinValue.ToString ("G10", _nfi));
			AssertEquals ("#04", "-2147483648", Int32.MinValue.ToString ("G99", _nfi));
			AssertEquals ("#05", "-G12147483648", Int32.MinValue.ToString ("G100", _nfi));
		}

		[Test]
		public void Test03006 ()
		{
			AssertEquals ("#01", "GF", 0.ToString ("GF", _nfi));
			AssertEquals ("#02", "G0F", 0.ToString ("G0F", _nfi));
			AssertEquals ("#03", "G0xF", 0.ToString ("G0xF", _nfi));
		}

		[Test]
		public void Test03007 ()
		{
			AssertEquals ("#01", "GF", Int32.MaxValue.ToString ("GF", _nfi));
			AssertEquals ("#02", "G2147483647F", Int32.MaxValue.ToString ("G0F", _nfi));
			AssertEquals ("#03", "G2147483647xF", Int32.MaxValue.ToString ("G0xF", _nfi));
		}

		[Test]
		public void Test03008 ()
		{
			AssertEquals ("#01", "-GF", Int32.MinValue.ToString ("GF", _nfi));
			AssertEquals ("#02", "-G2147483648F", Int32.MinValue.ToString ("G0F", _nfi));
			AssertEquals ("#03", "-G2147483648xF", Int32.MinValue.ToString ("G0xF", _nfi));
		}

		[Test]
		public void Test03009 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("G0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#02", "2147483647", Int32.MaxValue.ToString ("G0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#03", "-2147483648", Int32.MinValue.ToString ("G0000000000000000000000000000000000000010", _nfi));
		}

		[Test]
		public void Test03010 ()
		{
			AssertEquals ("#01", "+G", 0.ToString ("+G", _nfi));
			AssertEquals ("#02", "G+", 0.ToString ("G+", _nfi));
			AssertEquals ("#03", "+G+", 0.ToString ("+G+", _nfi));
		}
		
		[Test]
		public void Test03011 ()
		{
			AssertEquals ("#01", "+G", Int32.MaxValue.ToString ("+G", _nfi));
			AssertEquals ("#02", "G+", Int32.MaxValue.ToString ("G+", _nfi));
			AssertEquals ("#03", "+G+", Int32.MaxValue.ToString ("+G+", _nfi));
		}

		[Test]
		public void Test03012 ()
		{
			AssertEquals ("#01", "-+G", Int32.MinValue.ToString ("+G", _nfi));
			AssertEquals ("#02", "-G+", Int32.MinValue.ToString ("G+", _nfi));
			AssertEquals ("#03", "-+G+", Int32.MinValue.ToString ("+G+", _nfi));
		}

		[Test]
		public void Test03013 ()
		{
			AssertEquals ("#01", "-G", 0.ToString ("-G", _nfi));
			AssertEquals ("#02", "G-", 0.ToString ("G-", _nfi));
			AssertEquals ("#03", "-G-", 0.ToString ("-G-", _nfi));
		}
		
		[Test]
		public void Test03014 ()
		{
			AssertEquals ("#01", "-G", Int32.MaxValue.ToString ("-G", _nfi));
			AssertEquals ("#02", "G-", Int32.MaxValue.ToString ("G-", _nfi));
			AssertEquals ("#03", "-G-", Int32.MaxValue.ToString ("-G-", _nfi));
		}

		[Test]
		public void Test03015 ()
		{
			AssertEquals ("#01", "--G", Int32.MinValue.ToString ("-G", _nfi));
			AssertEquals ("#02", "-G-", Int32.MinValue.ToString ("G-", _nfi));
			AssertEquals ("#03", "--G-", Int32.MinValue.ToString ("-G-", _nfi));
		}

		[Test]
		public void Test03016 ()
		{
			AssertEquals ("#01", "G+0", 0.ToString ("G+0", _nfi));
			AssertEquals ("#02", "G+2147483647", Int32.MaxValue.ToString ("G+0", _nfi));
			AssertEquals ("#03", "-G+2147483648", Int32.MinValue.ToString ("G+0", _nfi));
		}

		[Test]
		public void Test03017 ()
		{
			AssertEquals ("#01", "G+9", 0.ToString ("G+9", _nfi));
			AssertEquals ("#02", "G+9", Int32.MaxValue.ToString ("G+9", _nfi));
			AssertEquals ("#03", "-G+9", Int32.MinValue.ToString ("G+9", _nfi));
		}

		[Test]
		public void Test03018 ()
		{
			AssertEquals ("#01", "G-9", 0.ToString ("G-9", _nfi));
			AssertEquals ("#02", "G-9", Int32.MaxValue.ToString ("G-9", _nfi));
			AssertEquals ("#03", "-G-9", Int32.MinValue.ToString ("G-9", _nfi));
		}

		[Test]
		public void Test03019 ()
		{
			AssertEquals ("#01", "G0", 0.ToString ("G0,", _nfi));
			AssertEquals ("#02", "G2147484", Int32.MaxValue.ToString ("G0,", _nfi));
			AssertEquals ("#03", "-G2147484", Int32.MinValue.ToString ("G0,", _nfi));
		}

		[Test]
		public void Test03020 ()
		{
			AssertEquals ("#01", "G0", 0.ToString ("G0.", _nfi));
			AssertEquals ("#02", "G2147483647", Int32.MaxValue.ToString ("G0.", _nfi));
			AssertEquals ("#03", "-G2147483648", Int32.MinValue.ToString ("G0.", _nfi));
		}

		[Test]
		public void Test03021 ()
		{
			AssertEquals ("#01", "G0.0", 0.ToString ("G0.0", _nfi));
			AssertEquals ("#02", "G2147483647.0", Int32.MaxValue.ToString ("G0.0", _nfi));
			AssertEquals ("#03", "-G2147483648.0", Int32.MinValue.ToString ("G0.0", _nfi));
		}

		[Test]
		public void Test03022 ()
		{
			AssertEquals ("#01", "G09", 0.ToString ("G0.9", _nfi));
			AssertEquals ("#02", "G21474836479", Int32.MaxValue.ToString ("G0.9", _nfi));
			AssertEquals ("#03", "-G21474836489", Int32.MinValue.ToString ("G0.9", _nfi));
		}

		[Test]
		public void Test03023 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalDigits = 0;
			AssertEquals ("#01", "0", 0.ToString ("G", nfi));
			nfi.NumberDecimalDigits = 1;
			AssertEquals ("#02", "0", 0.ToString ("G", nfi));
			nfi.NumberDecimalDigits = 99;
			AssertEquals ("#03", "0", 0.ToString ("G", nfi));
		}

		[Test]
		public void Test03024 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "";
			AssertEquals ("#01", "2147483648", Int32.MinValue.ToString ("G", nfi));
			nfi.NegativeSign = "-";
			AssertEquals ("#02", "-2147483648", Int32.MinValue.ToString ("G", nfi));
			nfi.NegativeSign = "+";
			AssertEquals ("#03", "+2147483648", Int32.MinValue.ToString ("G", nfi));
			nfi.NegativeSign = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			AssertEquals ("#04", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ2147483648", Int32.MinValue.ToString ("G", nfi));
		}

		[Test]
		public void Test03025 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "-";
			nfi.PositiveSign = "+";
			AssertEquals ("#01", "-1", (-1).ToString ("G", nfi));
			AssertEquals ("#02", "0", 0.ToString ("G", nfi));
			AssertEquals ("#03", "1",1.ToString ("G", nfi));
		}

		[Test]
		public void Test03026 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";
			AssertEquals ("#01", "+1", (-1).ToString ("G", nfi));
			AssertEquals ("#02", "0", 0.ToString ("G", nfi));
			AssertEquals ("#03", "1",1.ToString ("G", nfi));
		}

		[Test]
		public void Test03027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			AssertEquals ("#01", "1#2E+02",123.ToString ("G2", nfi));
		}

		// Test04000 - Int32 and N
		[Test]
		public void Test04000 ()
		{
			AssertEquals ("#01", "0.00", 0.ToString ("N", _nfi));
			AssertEquals ("#02", "0.00", 0.ToString ("n", _nfi));
			AssertEquals ("#03", "-2,147,483,648.00", Int32.MinValue.ToString ("N", _nfi));
			AssertEquals ("#04", "-2,147,483,648.00", Int32.MinValue.ToString ("n", _nfi));
			AssertEquals ("#05", "2,147,483,647.00", Int32.MaxValue.ToString ("N", _nfi));
			AssertEquals ("#06", "2,147,483,647.00", Int32.MaxValue.ToString ("n", _nfi));
		}

		[Test]
		public void Test04001 ()
		{
			AssertEquals ("#01", "N ", 0.ToString ("N ", _nfi));
			AssertEquals ("#02", " N", 0.ToString (" N", _nfi));
			AssertEquals ("#03", " N ", 0.ToString (" N ", _nfi));
		}

		[Test]
		public void Test04002 ()
		{
			AssertEquals ("#01", "-N ", (-1).ToString ("N ", _nfi));
			AssertEquals ("#02", "- N", (-1).ToString (" N", _nfi));
			AssertEquals ("#03", "- N ", (-1).ToString (" N ", _nfi));
		}

		[Test]
		public void Test04003 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("N0", _nfi));
			AssertEquals ("#02", "0.000000000", 0.ToString ("N9", _nfi));
			AssertEquals ("#03", "0.0000000000", 0.ToString ("N10", _nfi));
			AssertEquals ("#04", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("N99", _nfi));
			AssertEquals ("#05", "N100", 0.ToString ("N100", _nfi));
		}

		[Test]
		public void Test04004 ()
		{
			AssertEquals ("#01", "2,147,483,647", Int32.MaxValue.ToString ("N0", _nfi));
			AssertEquals ("#02", "2,147,483,647.000000000", Int32.MaxValue.ToString ("N9", _nfi));
			AssertEquals ("#03", "2,147,483,647.0000000000", Int32.MaxValue.ToString ("N10", _nfi));
			AssertEquals ("#04", "2,147,483,647.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Int32.MaxValue.ToString ("N99", _nfi));
			AssertEquals ("#05", "N12147483647", Int32.MaxValue.ToString ("N100", _nfi));
		}

		[Test]
		public void Test04005 ()
		{
			AssertEquals ("#01", "-2,147,483,648", Int32.MinValue.ToString ("N0", _nfi));
			AssertEquals ("#02", "-2,147,483,648.000000000", Int32.MinValue.ToString ("N9", _nfi));
			AssertEquals ("#03", "-2,147,483,648.0000000000", Int32.MinValue.ToString ("N10", _nfi));
			AssertEquals ("#04", "-2,147,483,648.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Int32.MinValue.ToString ("N99", _nfi));
			AssertEquals ("#05", "-N12147483648", Int32.MinValue.ToString ("N100", _nfi));
		}

		[Test]
		public void Test04006 ()
		{
			AssertEquals ("#01", "NF", 0.ToString ("NF", _nfi));
			AssertEquals ("#02", "N0F", 0.ToString ("N0F", _nfi));
			AssertEquals ("#03", "N0xF", 0.ToString ("N0xF", _nfi));
		}

		[Test]
		public void Test04007 ()
		{
			AssertEquals ("#01", "NF", Int32.MaxValue.ToString ("NF", _nfi));
			AssertEquals ("#02", "N2147483647F", Int32.MaxValue.ToString ("N0F", _nfi));
			AssertEquals ("#03", "N2147483647xF", Int32.MaxValue.ToString ("N0xF", _nfi));
		}

		[Test]
		public void Test04008 ()
		{
			AssertEquals ("#01", "-NF", Int32.MinValue.ToString ("NF", _nfi));
			AssertEquals ("#02", "-N2147483648F", Int32.MinValue.ToString ("N0F", _nfi));
			AssertEquals ("#03", "-N2147483648xF", Int32.MinValue.ToString ("N0xF", _nfi));
		}

		[Test]
		public void Test04009 ()
		{
			AssertEquals ("#01", "0.0000000000", 0.ToString ("N0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#02", "2,147,483,647.0000000000", Int32.MaxValue.ToString ("N0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#03", "-2,147,483,648.0000000000", Int32.MinValue.ToString ("N0000000000000000000000000000000000000010", _nfi));
		}

		[Test]
		public void Test04010 ()
		{
			AssertEquals ("#01", "+N", 0.ToString ("+N", _nfi));
			AssertEquals ("#02", "N+", 0.ToString ("N+", _nfi));
			AssertEquals ("#03", "+N+", 0.ToString ("+N+", _nfi));
		}
		
		[Test]
		public void Test04011 ()
		{
			AssertEquals ("#01", "+N", Int32.MaxValue.ToString ("+N", _nfi));
			AssertEquals ("#02", "N+", Int32.MaxValue.ToString ("N+", _nfi));
			AssertEquals ("#03", "+N+", Int32.MaxValue.ToString ("+N+", _nfi));
		}

		[Test]
		public void Test04012 ()
		{
			AssertEquals ("#01", "-+N", Int32.MinValue.ToString ("+N", _nfi));
			AssertEquals ("#02", "-N+", Int32.MinValue.ToString ("N+", _nfi));
			AssertEquals ("#03", "-+N+", Int32.MinValue.ToString ("+N+", _nfi));
		}

		[Test]
		public void Test04013 ()
		{
			AssertEquals ("#01", "-N", 0.ToString ("-N", _nfi));
			AssertEquals ("#02", "N-", 0.ToString ("N-", _nfi));
			AssertEquals ("#03", "-N-", 0.ToString ("-N-", _nfi));
		}
		
		[Test]
		public void Test04014 ()
		{
			AssertEquals ("#01", "-N", Int32.MaxValue.ToString ("-N", _nfi));
			AssertEquals ("#02", "N-", Int32.MaxValue.ToString ("N-", _nfi));
			AssertEquals ("#03", "-N-", Int32.MaxValue.ToString ("-N-", _nfi));
		}

		[Test]
		public void Test04015 ()
		{
			AssertEquals ("#01", "--N", Int32.MinValue.ToString ("-N", _nfi));
			AssertEquals ("#02", "-N-", Int32.MinValue.ToString ("N-", _nfi));
			AssertEquals ("#03", "--N-", Int32.MinValue.ToString ("-N-", _nfi));
		}

		[Test]
		public void Test04016 ()
		{
			AssertEquals ("#01", "N+0", 0.ToString ("N+0", _nfi));
			AssertEquals ("#02", "N+2147483647", Int32.MaxValue.ToString ("N+0", _nfi));
			AssertEquals ("#03", "-N+2147483648", Int32.MinValue.ToString ("N+0", _nfi));
		}

		[Test]
		public void Test04017 ()
		{
			AssertEquals ("#01", "N+9", 0.ToString ("N+9", _nfi));
			AssertEquals ("#02", "N+9", Int32.MaxValue.ToString ("N+9", _nfi));
			AssertEquals ("#03", "-N+9", Int32.MinValue.ToString ("N+9", _nfi));
		}

		[Test]
		public void Test04018 ()
		{
			AssertEquals ("#01", "N-9", 0.ToString ("N-9", _nfi));
			AssertEquals ("#02", "N-9", Int32.MaxValue.ToString ("N-9", _nfi));
			AssertEquals ("#03", "-N-9", Int32.MinValue.ToString ("N-9", _nfi));
		}

		[Test]
		public void Test04019 ()
		{
			AssertEquals ("#01", "N0", 0.ToString ("N0,", _nfi));
			AssertEquals ("#02", "N2147484", Int32.MaxValue.ToString ("N0,", _nfi));
			AssertEquals ("#03", "-N2147484", Int32.MinValue.ToString ("N0,", _nfi));
		}

		[Test]
		public void Test04020 ()
		{
			AssertEquals ("#01", "N0", 0.ToString ("N0.", _nfi));
			AssertEquals ("#02", "N2147483647", Int32.MaxValue.ToString ("N0.", _nfi));
			AssertEquals ("#03", "-N2147483648", Int32.MinValue.ToString ("N0.", _nfi));
		}

		[Test]
		public void Test04021 ()
		{
			AssertEquals ("#01", "N0.0", 0.ToString ("N0.0", _nfi));
			AssertEquals ("#02", "N2147483647.0", Int32.MaxValue.ToString ("N0.0", _nfi));
			AssertEquals ("#03", "-N2147483648.0", Int32.MinValue.ToString ("N0.0", _nfi));
		}

		[Test]
		public void Test04022 ()
		{
			AssertEquals ("#01", "N09", 0.ToString ("N0.9", _nfi));
			AssertEquals ("#02", "N21474836479", Int32.MaxValue.ToString ("N0.9", _nfi));
			AssertEquals ("#03", "-N21474836489", Int32.MinValue.ToString ("N0.9", _nfi));
		}

		[Test]
		public void Test04023 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalDigits = 0;
			AssertEquals ("#01", "0", 0.ToString ("N", nfi));
			nfi.NumberDecimalDigits = 1;
			AssertEquals ("#02", "0.0", 0.ToString ("N", nfi));
			nfi.NumberDecimalDigits = 99;
			AssertEquals ("#03", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("N", nfi));
		}

		[Test]
		public void Test04024 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "";
			AssertEquals ("#01", "2,147,483,648.00", Int32.MinValue.ToString ("N", nfi));
			nfi.NegativeSign = "-";
			AssertEquals ("#02", "-2,147,483,648.00", Int32.MinValue.ToString ("N", nfi));
			nfi.NegativeSign = "+";
			AssertEquals ("#03", "+2,147,483,648.00", Int32.MinValue.ToString ("N", nfi));
			nfi.NegativeSign = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
			AssertEquals ("#04", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ2,147,483,648.00", Int32.MinValue.ToString ("N", nfi));
		}

		[Test]
		public void Test04025 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "-";
			nfi.PositiveSign = "+";
			AssertEquals ("#01", "-1.00", (-1).ToString ("N", nfi));
			AssertEquals ("#02", "0.00", 0.ToString ("N", nfi));
			AssertEquals ("#03", "1.00",1.ToString ("N", nfi));
		}

		[Test]
		public void Test04026 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";
			AssertEquals ("#01", "+1.00", (-1).ToString ("N", nfi));
			AssertEquals ("#02", "0.00", 0.ToString ("N", nfi));
			AssertEquals ("#03", "1.00",1.ToString ("N", nfi));
		}

		[Test]
		public void Test04027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			AssertEquals ("#01", "123#0",123.ToString ("N1", nfi));
		}

		[Test]
		public void Test04028 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberGroupSeparator = "-";
			AssertEquals ("#01", "-2-147-483-648.0",Int32.MinValue.ToString ("N1", nfi));
		}

		[Test]
		public void Test04029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberGroupSizes = new int [] {};
			AssertEquals ("#01", "-2147483648.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {0};
			AssertEquals ("#02", "-2147483648.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {1};
			AssertEquals ("#03", "-2,1,4,7,4,8,3,6,4,8.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {3};
			AssertEquals ("#04", "-2,147,483,648.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {9};
			AssertEquals ("#05", "-2,147483648.0",Int32.MinValue.ToString ("N1", nfi));
		}

		[Test]
		public void Test04030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberGroupSizes = new int [] {1,2};
			AssertEquals ("#01", "-2,14,74,83,64,8.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {1,2,3};
			AssertEquals ("#02", "-2,147,483,64,8.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {1,2,3,4};
			AssertEquals ("#03", "-2147,483,64,8.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {1,2,1,2,1,2,1};
			AssertEquals ("#04", "-2,14,7,48,3,64,8.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {1,0};
			AssertEquals ("#05", "-214748364,8.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {1,2,0};
			AssertEquals ("#06", "-2147483,64,8.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {1,2,3,0};
			AssertEquals ("#07", "-2147,483,64,8.0",Int32.MinValue.ToString ("N1", nfi));
			nfi.NumberGroupSizes = new int [] {1,2,3,4,0};
			AssertEquals ("#08", "-2147,483,64,8.0",Int32.MinValue.ToString ("N1", nfi));
		}

		[Test]
		public void Test04031 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "1234567890";
			AssertEquals ("#01", "12345678902,147,483,648.00", Int32.MinValue.ToString ("N", nfi));
		}

		// Test05000 - Int32 and P
		[Test]
		public void Test05000 ()
		{
			AssertEquals ("#01", "0.00 %", 0.ToString ("P", _nfi));
			AssertEquals ("#02", "0.00 %", 0.ToString ("p", _nfi));
			AssertEquals ("#03", "-214,748,364,800.00 %", Int32.MinValue.ToString ("P", _nfi));
			AssertEquals ("#04", "-214,748,364,800.00 %", Int32.MinValue.ToString ("p", _nfi));
			AssertEquals ("#05", "214,748,364,700.00 %", Int32.MaxValue.ToString ("P", _nfi));
			AssertEquals ("#06", "214,748,364,700.00 %", Int32.MaxValue.ToString ("p", _nfi));
		}

		[Test]
		public void Test05001 ()
		{
			AssertEquals ("#01", "P ", 0.ToString ("P ", _nfi));
			AssertEquals ("#02", " P", 0.ToString (" P", _nfi));
			AssertEquals ("#03", " P ", 0.ToString (" P ", _nfi));
		}

		[Test]
		public void Test05002 ()
		{
			AssertEquals ("#01", "-P ", (-1).ToString ("P ", _nfi));
			AssertEquals ("#02", "- P", (-1).ToString (" P", _nfi));
			AssertEquals ("#03", "- P ", (-1).ToString (" P ", _nfi));
		}

		[Test]
		public void Test05003 ()
		{
			AssertEquals ("#01", "0 %", 0.ToString ("P0", _nfi));
			AssertEquals ("#02", "0.000000000 %", 0.ToString ("P9", _nfi));
			AssertEquals ("#03", "0.0000000000 %", 0.ToString ("P10", _nfi));
			AssertEquals ("#04", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", 0.ToString ("P99", _nfi));
			AssertEquals ("#05", "P100", 0.ToString ("P100", _nfi));
		}

		[Test]
		public void Test05004 ()
		{
			AssertEquals ("#01", "214,748,364,700 %", Int32.MaxValue.ToString ("P0", _nfi));
			AssertEquals ("#02", "214,748,364,700.000000000 %", Int32.MaxValue.ToString ("P9", _nfi));
			AssertEquals ("#03", "214,748,364,700.0000000000 %", Int32.MaxValue.ToString ("P10", _nfi));
			AssertEquals ("#04", "214,748,364,700.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", Int32.MaxValue.ToString ("P99", _nfi));
			AssertEquals ("#05", "P12147483647", Int32.MaxValue.ToString ("P100", _nfi));
		}

		[Test]
		public void Test05005 ()
		{
			AssertEquals ("#01", "-214,748,364,800 %", Int32.MinValue.ToString ("P0", _nfi));
			AssertEquals ("#02", "-214,748,364,800.000000000 %", Int32.MinValue.ToString ("P9", _nfi));
			AssertEquals ("#03", "-214,748,364,800.0000000000 %", Int32.MinValue.ToString ("P10", _nfi));
			AssertEquals ("#04", "-214,748,364,800.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", Int32.MinValue.ToString ("P99", _nfi));
			AssertEquals ("#05", "-P12147483648", Int32.MinValue.ToString ("P100", _nfi));
		}

		[Test]
		public void Test05006 ()
		{
			AssertEquals ("#01", "PF", 0.ToString ("PF", _nfi));
			AssertEquals ("#02", "P0F", 0.ToString ("P0F", _nfi));
			AssertEquals ("#03", "P0xF", 0.ToString ("P0xF", _nfi));
		}

		[Test]
		public void Test05007 ()
		{
			AssertEquals ("#01", "PF", Int32.MaxValue.ToString ("PF", _nfi));
			AssertEquals ("#02", "P2147483647F", Int32.MaxValue.ToString ("P0F", _nfi));
			AssertEquals ("#03", "P2147483647xF", Int32.MaxValue.ToString ("P0xF", _nfi));
		}

		[Test]
		public void Test05008 ()
		{
			AssertEquals ("#01", "-PF", Int32.MinValue.ToString ("PF", _nfi));
			AssertEquals ("#02", "-P2147483648F", Int32.MinValue.ToString ("P0F", _nfi));
			AssertEquals ("#03", "-P2147483648xF", Int32.MinValue.ToString ("P0xF", _nfi));
		}

		[Test]
		public void Test05009 ()
		{
			AssertEquals ("#01", "0.0000000000 %", 0.ToString ("P0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#02", "214,748,364,700.0000000000 %", Int32.MaxValue.ToString ("P0000000000000000000000000000000000000010", _nfi));
			AssertEquals ("#03", "-214,748,364,800.0000000000 %", Int32.MinValue.ToString ("P0000000000000000000000000000000000000010", _nfi));
		}

		[Test]
		public void Test05010 ()
		{
			AssertEquals ("#01", "+P", 0.ToString ("+P", _nfi));
			AssertEquals ("#02", "P+", 0.ToString ("P+", _nfi));
			AssertEquals ("#03", "+P+", 0.ToString ("+P+", _nfi));
		}
		
		[Test]
		public void Test05011 ()
		{
			AssertEquals ("#01", "+P", Int32.MaxValue.ToString ("+P", _nfi));
			AssertEquals ("#02", "P+", Int32.MaxValue.ToString ("P+", _nfi));
			AssertEquals ("#03", "+P+", Int32.MaxValue.ToString ("+P+", _nfi));
		}

		[Test]
		public void Test05012 ()
		{
			AssertEquals ("#01", "-+P", Int32.MinValue.ToString ("+P", _nfi));
			AssertEquals ("#02", "-P+", Int32.MinValue.ToString ("P+", _nfi));
			AssertEquals ("#03", "-+P+", Int32.MinValue.ToString ("+P+", _nfi));
		}

		[Test]
		public void Test05013 ()
		{
			AssertEquals ("#01", "-P", 0.ToString ("-P", _nfi));
			AssertEquals ("#02", "P-", 0.ToString ("P-", _nfi));
			AssertEquals ("#03", "-P-", 0.ToString ("-P-", _nfi));
		}
		
		[Test]
		public void Test05014 ()
		{
			AssertEquals ("#01", "-P", Int32.MaxValue.ToString ("-P", _nfi));
			AssertEquals ("#02", "P-", Int32.MaxValue.ToString ("P-", _nfi));
			AssertEquals ("#03", "-P-", Int32.MaxValue.ToString ("-P-", _nfi));
		}

		[Test]
		public void Test05015 ()
		{
			AssertEquals ("#01", "--P", Int32.MinValue.ToString ("-P", _nfi));
			AssertEquals ("#02", "-P-", Int32.MinValue.ToString ("P-", _nfi));
			AssertEquals ("#03", "--P-", Int32.MinValue.ToString ("-P-", _nfi));
		}

		[Test]
		public void Test05016 ()
		{
			AssertEquals ("#01", "P+0", 0.ToString ("P+0", _nfi));
			AssertEquals ("#02", "P+2147483647", Int32.MaxValue.ToString ("P+0", _nfi));
			AssertEquals ("#03", "-P+2147483648", Int32.MinValue.ToString ("P+0", _nfi));
		}

		[Test]
		public void Test05017 ()
		{
			AssertEquals ("#01", "P+9", 0.ToString ("P+9", _nfi));
			AssertEquals ("#02", "P+9", Int32.MaxValue.ToString ("P+9", _nfi));
			AssertEquals ("#03", "-P+9", Int32.MinValue.ToString ("P+9", _nfi));
		}

		[Test]
		public void Test05018 ()
		{
			AssertEquals ("#01", "P-9", 0.ToString ("P-9", _nfi));
			AssertEquals ("#02", "P-9", Int32.MaxValue.ToString ("P-9", _nfi));
			AssertEquals ("#03", "-P-9", Int32.MinValue.ToString ("P-9", _nfi));
		}

		[Test]
		public void Test05019 ()
		{
			AssertEquals ("#01", "P0", 0.ToString ("P0,", _nfi));
			AssertEquals ("#02", "P2147484", Int32.MaxValue.ToString ("P0,", _nfi));
			AssertEquals ("#03", "-P2147484", Int32.MinValue.ToString ("P0,", _nfi));
		}

		[Test]
		public void Test05020 ()
		{
			AssertEquals ("#01", "P0", 0.ToString ("P0.", _nfi));
			AssertEquals ("#02", "P2147483647", Int32.MaxValue.ToString ("P0.", _nfi));
			AssertEquals ("#03", "-P2147483648", Int32.MinValue.ToString ("P0.", _nfi));
		}

		[Test]
		public void Test05021 ()
		{
			AssertEquals ("#01", "P0.0", 0.ToString ("P0.0", _nfi));
			AssertEquals ("#02", "P2147483647.0", Int32.MaxValue.ToString ("P0.0", _nfi));
			AssertEquals ("#03", "-P2147483648.0", Int32.MinValue.ToString ("P0.0", _nfi));
		}

		[Test]
		public void Test05022 ()
		{
			AssertEquals ("#01", "P09", 0.ToString ("P0.9", _nfi));
			AssertEquals ("#02", "P21474836479", Int32.MaxValue.ToString ("P0.9", _nfi));
			AssertEquals ("#03", "-P21474836489", Int32.MinValue.ToString ("P0.9", _nfi));
		}

		[Test]
		public void Test05023 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentDecimalDigits = 0;
			AssertEquals ("#01", "0 %", 0.ToString ("P", nfi));
			nfi.PercentDecimalDigits = 1;
			AssertEquals ("#02", "0.0 %", 0.ToString ("P", nfi));
			nfi.PercentDecimalDigits = 99;
			AssertEquals ("#03", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", 0.ToString ("P", nfi));
		}

		[Test]
		public void Test05024 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "";
			AssertEquals ("#01", "214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi));
			nfi.NegativeSign = "-";
			AssertEquals ("#02", "-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi));
			nfi.NegativeSign = "+";
			AssertEquals ("#03", "+214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi));
			nfi.NegativeSign = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMPOPQRSTUVWXYZ";
			AssertEquals ("#04", "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMPOPQRSTUVWXYZ214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi));
		}

		[Test]
		public void Test05025 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "-";
			nfi.PositiveSign = "+";
			AssertEquals ("#01", "-100.00 %", (-1).ToString ("P", nfi));
			AssertEquals ("#02", "0.00 %", 0.ToString ("P", nfi));
			AssertEquals ("#03", "100.00 %",1.ToString ("P", nfi));
		}

		[Test]
		public void Test05026 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";
			AssertEquals ("#01", "+100.00 %", (-1).ToString ("P", nfi));
			AssertEquals ("#02", "0.00 %", 0.ToString ("P", nfi));
			AssertEquals ("#03", "100.00 %",1.ToString ("P", nfi));
		}

		[Test]
		public void Test05027 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentDecimalSeparator = "#";
			AssertEquals ("#01", "12,300#0 %",123.ToString ("P1", nfi));
		}

		[Test]
		public void Test05028 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentGroupSeparator = "-";
			AssertEquals ("#01", "-214-748-364-800.0 %",Int32.MinValue.ToString ("P1", nfi));
		}

		[Test]
		public void Test05029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentGroupSizes = new int [] {};
			AssertEquals ("#01", "-214748364800.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {0};
			AssertEquals ("#02", "-214748364800.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {1};
			AssertEquals ("#03", "-2,1,4,7,4,8,3,6,4,8,0,0.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {3};
			AssertEquals ("#04", "-214,748,364,800.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {9};
			AssertEquals ("#05", "-214,748364800.0 %",Int32.MinValue.ToString ("P1", nfi));
		}

		[Test]
		public void Test05030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentGroupSizes = new int [] {1,2};
			AssertEquals ("#01", "-2,14,74,83,64,80,0.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {1,2,3};
			AssertEquals ("#02", "-214,748,364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {1,2,3,4};
			AssertEquals ("#03", "-21,4748,364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {1,2,1,2,1,2,1};
			AssertEquals ("#04", "-2,1,4,74,8,36,4,80,0.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {1,0};
			AssertEquals ("#05", "-21474836480,0.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {1,2,0};
			AssertEquals ("#06", "-214748364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {1,2,3,0};
			AssertEquals ("#07", "-214748,364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi));
			nfi.PercentGroupSizes = new int [] {1,2,3,4,0};
			AssertEquals ("#08", "-21,4748,364,80,0.0 %",Int32.MinValue.ToString ("P1", nfi));
		}

		[Test]
		public void Test05031 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "1234567890";
			AssertEquals ("#01", "1234567890214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi));
		}

		[Test]
		public void Test05032 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentNegativePattern = 0;
			AssertEquals ("#01", "-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi));
			AssertEquals ("#02", "214,748,364,700.00 %", Int32.MaxValue.ToString ("P", nfi));
			AssertEquals ("#03", "0.00 %", 0.ToString ("P", nfi));
		}

		[Test]
		public void Test05033 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentNegativePattern = 1;
			AssertEquals ("#01", "-214,748,364,800.00%", Int32.MinValue.ToString ("P", nfi));
			AssertEquals ("#02", "214,748,364,700.00 %", Int32.MaxValue.ToString ("P", nfi));
			AssertEquals ("#03", "0.00 %", 0.ToString ("P", nfi));
		}

		[Test]
		public void Test05034 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentNegativePattern = 2;
			AssertEquals ("#01", "-%214,748,364,800.00", Int32.MinValue.ToString ("P", nfi));
			AssertEquals ("#02", "214,748,364,700.00 %", Int32.MaxValue.ToString ("P", nfi));
			AssertEquals ("#03", "0.00 %", 0.ToString ("P", nfi));
		}

		[Test]
		public void Test05035 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentPositivePattern = 0;
			AssertEquals ("#01", "-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi));
			AssertEquals ("#02", "214,748,364,700.00 %", Int32.MaxValue.ToString ("P", nfi));
			AssertEquals ("#03", "0.00 %", 0.ToString ("P", nfi));
		}

		[Test]
		public void Test05036 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentPositivePattern = 1;
			AssertEquals ("#01", "-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi));
			AssertEquals ("#02", "214,748,364,700.00%", Int32.MaxValue.ToString ("P", nfi));
			AssertEquals ("#03", "0.00%", 0.ToString ("P", nfi));
		}

		[Test]
		public void Test05037 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentPositivePattern = 2;
			AssertEquals ("#01", "-214,748,364,800.00 %", Int32.MinValue.ToString ("P", nfi));
			AssertEquals ("#02", "%214,748,364,700.00", Int32.MaxValue.ToString ("P", nfi));
			AssertEquals ("#03", "%0.00", 0.ToString ("P", nfi));
		}

		// Test06000 - Int32 and R
		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Test06000 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("R", _nfi));
		}

		// Test07000- Int32 and X
		[Test]
		public void Test07000 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("X", _nfi));
			AssertEquals ("#02", "0", 0.ToString ("x", _nfi));
			AssertEquals ("#03", "80000000", Int32.MinValue.ToString ("X", _nfi));
			AssertEquals ("#04", "80000000", Int32.MinValue.ToString ("x", _nfi));
			AssertEquals ("#05", "7FFFFFFF", Int32.MaxValue.ToString ("X", _nfi));
			AssertEquals ("#06", "7fffffff", Int32.MaxValue.ToString ("x", _nfi));
		}

		[Test]
		public void Test07001 ()
		{
			AssertEquals ("#01", "X ", 0.ToString ("X ", _nfi));
			AssertEquals ("#02", " X", 0.ToString (" X", _nfi));
			AssertEquals ("#03", " X ", 0.ToString (" X ", _nfi));
		}

		[Test]
		public void Test07002 ()
		{
			AssertEquals ("#01", "-X ", (-1).ToString ("X ", _nfi));
			AssertEquals ("#02", "- X", (-1).ToString (" X", _nfi));
			AssertEquals ("#03", "- X ", (-1).ToString (" X ", _nfi));
		}

		[Test]
		public void Test07003 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("X0", _nfi));
			AssertEquals ("#02", "0000000000", 0.ToString ("X10", _nfi));
			AssertEquals ("#03", "00000000000", 0.ToString ("X11", _nfi));
			AssertEquals ("#04", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.ToString ("X99", _nfi));
			AssertEquals ("#05", "X100", 0.ToString ("X100", _nfi));
		}

		[Test]
		public void Test07004 ()
		{
			AssertEquals ("#01", "7FFFFFFF", Int32.MaxValue.ToString ("X0", _nfi));
			AssertEquals ("#02", "007FFFFFFF", Int32.MaxValue.ToString ("X10", _nfi));
			AssertEquals ("#03", "0007FFFFFFF", Int32.MaxValue.ToString ("X11", _nfi));
			AssertEquals ("#04", "00000000000000000000000000000000000000000000000000000000000000000000000000000000000000000007FFFFFFF", Int32.MaxValue.ToString ("X99", _nfi));
			AssertEquals ("#05", "X12147483647", Int32.MaxValue.ToString ("X100", _nfi));
		}

		[Test]
		public void Test07005 ()
		{
			AssertEquals ("#01", "80000000", Int32.MinValue.ToString ("X0", _nfi));
			AssertEquals ("#02", "0080000000", Int32.MinValue.ToString ("X10", _nfi));
			AssertEquals ("#03", "00080000000", Int32.MinValue.ToString ("X11", _nfi));
			AssertEquals ("#04", "000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000080000000", Int32.MinValue.ToString ("X99", _nfi));
			AssertEquals ("#05", "-X12147483648", Int32.MinValue.ToString ("X100", _nfi));
		}

		[Test]
		public void Test07006 ()
		{
			AssertEquals ("#01", "XF", 0.ToString ("XF", _nfi));
			AssertEquals ("#02", "X0F", 0.ToString ("X0F", _nfi));
			AssertEquals ("#03", "X0xF", 0.ToString ("X0xF", _nfi));
		}

		[Test]
		public void Test07007 ()
		{
			AssertEquals ("#01", "XF", Int32.MaxValue.ToString ("XF", _nfi));
			AssertEquals ("#02", "X2147483647F", Int32.MaxValue.ToString ("X0F", _nfi));
			AssertEquals ("#03", "X2147483647xF", Int32.MaxValue.ToString ("X0xF", _nfi));
		}

		[Test]
		public void Test07008 ()
		{
			AssertEquals ("#01", "-XF", Int32.MinValue.ToString ("XF", _nfi));
			AssertEquals ("#02", "-X2147483648F", Int32.MinValue.ToString ("X0F", _nfi));
			AssertEquals ("#03", "-X2147483648xF", Int32.MinValue.ToString ("X0xF", _nfi));
		}

		[Test]
		public void Test07009 ()
		{
			AssertEquals ("#01", "00000000000", 0.ToString ("X0000000000000000000000000000000000000011", _nfi));
			AssertEquals ("#02", "0007FFFFFFF", Int32.MaxValue.ToString ("X0000000000000000000000000000000000000011", _nfi));
			AssertEquals ("#03", "00080000000", Int32.MinValue.ToString ("X0000000000000000000000000000000000000011", _nfi));
		}

		[Test]
		public void Test07010 ()
		{
			AssertEquals ("#01", "+X", 0.ToString ("+X", _nfi));
			AssertEquals ("#02", "X+", 0.ToString ("X+", _nfi));
			AssertEquals ("#03", "+X+", 0.ToString ("+X+", _nfi));
		}
		
		[Test]
		public void Test07011 ()
		{
			AssertEquals ("#01", "+X", Int32.MaxValue.ToString ("+X", _nfi));
			AssertEquals ("#02", "X+", Int32.MaxValue.ToString ("X+", _nfi));
			AssertEquals ("#03", "+X+", Int32.MaxValue.ToString ("+X+", _nfi));
		}

		[Test]
		public void Test07012 ()
		{
			AssertEquals ("#01", "-+X", Int32.MinValue.ToString ("+X", _nfi));
			AssertEquals ("#02", "-X+", Int32.MinValue.ToString ("X+", _nfi));
			AssertEquals ("#03", "-+X+", Int32.MinValue.ToString ("+X+", _nfi));
		}

		[Test]
		public void Test07013 ()
		{
			AssertEquals ("#01", "-X", 0.ToString ("-X", _nfi));
			AssertEquals ("#02", "X-", 0.ToString ("X-", _nfi));
			AssertEquals ("#03", "-X-", 0.ToString ("-X-", _nfi));
		}
		
		[Test]
		public void Test07014 ()
		{
			AssertEquals ("#01", "-X", Int32.MaxValue.ToString ("-X", _nfi));
			AssertEquals ("#02", "X-", Int32.MaxValue.ToString ("X-", _nfi));
			AssertEquals ("#03", "-X-", Int32.MaxValue.ToString ("-X-", _nfi));
		}

		[Test]
		public void Test07015 ()
		{
			AssertEquals ("#01", "--X", Int32.MinValue.ToString ("-X", _nfi));
			AssertEquals ("#02", "-X-", Int32.MinValue.ToString ("X-", _nfi));
			AssertEquals ("#03", "--X-", Int32.MinValue.ToString ("-X-", _nfi));
		}

		[Test]
		public void Test07016 ()
		{
			AssertEquals ("#01", "X+0", 0.ToString ("X+0", _nfi));
			AssertEquals ("#02", "X+2147483647", Int32.MaxValue.ToString ("X+0", _nfi));
			AssertEquals ("#03", "-X+2147483648", Int32.MinValue.ToString ("X+0", _nfi));
		}

		[Test]
		public void Test07017 ()
		{
			AssertEquals ("#01", "X+9", 0.ToString ("X+9", _nfi));
			AssertEquals ("#02", "X+9", Int32.MaxValue.ToString ("X+9", _nfi));
			AssertEquals ("#03", "-X+9", Int32.MinValue.ToString ("X+9", _nfi));
		}

		[Test]
		public void Test07018 ()
		{
			AssertEquals ("#01", "X-9", 0.ToString ("X-9", _nfi));
			AssertEquals ("#02", "X-9", Int32.MaxValue.ToString ("X-9", _nfi));
			AssertEquals ("#03", "-X-9", Int32.MinValue.ToString ("X-9", _nfi));
		}

		[Test]
		public void Test07019 ()
		{
			AssertEquals ("#01", "X0", 0.ToString ("X0,", _nfi));
			AssertEquals ("#02", "X2147484", Int32.MaxValue.ToString ("X0,", _nfi));
			AssertEquals ("#03", "-X2147484", Int32.MinValue.ToString ("X0,", _nfi));
		}

		[Test]
		public void Test07020 ()
		{
			AssertEquals ("#01", "X0", 0.ToString ("X0.", _nfi));
			AssertEquals ("#02", "X2147483647", Int32.MaxValue.ToString ("X0.", _nfi));
			AssertEquals ("#03", "-X2147483648", Int32.MinValue.ToString ("X0.", _nfi));
		}

		[Test]
		public void Test07021 ()
		{
			AssertEquals ("#01", "X0.0", 0.ToString ("X0.0", _nfi));
			AssertEquals ("#02", "X2147483647.0", Int32.MaxValue.ToString ("X0.0", _nfi));
			AssertEquals ("#03", "-X2147483648.0", Int32.MinValue.ToString ("X0.0", _nfi));
		}

		[Test]
		public void Test07022 ()
		{
			AssertEquals ("#01", "X09", 0.ToString ("X0.9", _nfi));
			AssertEquals ("#02", "X21474836479", Int32.MaxValue.ToString ("X0.9", _nfi));
			AssertEquals ("#03", "-X21474836489", Int32.MinValue.ToString ("X0.9", _nfi));
		}

		[Test]
		public void Test08000 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("0", _nfi));
			AssertEquals ("#02", "2147483647", Int32.MaxValue.ToString ("0", _nfi));
			AssertEquals ("#03", "-2147483648", Int32.MinValue.ToString ("0", _nfi));
		}

		// Test08000 - Int32 and Custom
		[Test]
		public void Test08001 ()
		{
			AssertEquals ("#01", "00000000000", 0.ToString ("00000000000", _nfi));
			AssertEquals ("#02", "02147483647", Int32.MaxValue.ToString ("00000000000", _nfi));
			AssertEquals ("#03", "-02147483648", Int32.MinValue.ToString ("00000000000", _nfi));
		}

		[Test]
		public void Test08002 ()
		{
			AssertEquals ("#01", " 00000000000 ", 0.ToString (" 00000000000 ", _nfi));
			AssertEquals ("#02", " 02147483647 ", Int32.MaxValue.ToString (" 00000000000 ", _nfi));
			AssertEquals ("#03", "- 02147483648 ", Int32.MinValue.ToString (" 00000000000 ", _nfi));
		}

		[Test]
		public void Test08003 ()
		{
			AssertEquals ("#01", "", 0.ToString ("#", _nfi));
			AssertEquals ("#02", "2147483647", Int32.MaxValue.ToString ("#", _nfi));
			AssertEquals ("#03", "-2147483648", Int32.MinValue.ToString ("#", _nfi));
		}

		[Test]
		public void Test08004 ()
		{
			AssertEquals ("#01", "", 0.ToString ("##########", _nfi));
			AssertEquals ("#02", "2147483647", Int32.MaxValue.ToString ("##########", _nfi));
			AssertEquals ("#03", "-2147483648", Int32.MinValue.ToString ("##########", _nfi));
		}

		[Test]
		public void Test08005 ()
		{
			AssertEquals ("#01", "  ", 0.ToString (" ########## ", _nfi));
			AssertEquals ("#02", " 2147483647 ", Int32.MaxValue.ToString (" ########## ", _nfi));
			AssertEquals ("#03", "- 2147483648 ", Int32.MinValue.ToString (" ########## ", _nfi));
		}

		[Test]
		public void Test08006 ()
		{
			AssertEquals ("#01", "", 0.ToString (".", _nfi));
			AssertEquals ("#02", "", Int32.MaxValue.ToString (".", _nfi));
			AssertEquals ("#03", "-", Int32.MinValue.ToString (".", _nfi));
		}

		[Test]
		public void Test08007 ()
		{
			AssertEquals ("#01", "00000000000", 0.ToString ("00000000000.", _nfi));
			AssertEquals ("#02", "02147483647", Int32.MaxValue.ToString ("00000000000.", _nfi));
			AssertEquals ("#03", "-02147483648", Int32.MinValue.ToString ("00000000000.", _nfi));
		}

		[Test]
		public void Test08008 ()
		{
			AssertEquals ("#01", ".00000000000", 0.ToString (".00000000000", _nfi));
			AssertEquals ("#02", "2147483647.00000000000", Int32.MaxValue.ToString (".00000000000", _nfi));
			AssertEquals ("#03", "-2147483648.00000000000", Int32.MinValue.ToString (".00000000000", _nfi));
		}

		[Test]
		public void Test08009 ()
		{
			AssertEquals ("#01", "00000000000.00000000000", 0.ToString ("00000000000.00000000000", _nfi));
			AssertEquals ("#02", "02147483647.00000000000", Int32.MaxValue.ToString ("00000000000.00000000000", _nfi));
			AssertEquals ("#03", "-02147483648.00000000000", Int32.MinValue.ToString ("00000000000.00000000000", _nfi));
		}

		[Test]
		public void Test08010 ()
		{
			AssertEquals ("#01", "00.0000000000", 0.ToString ("00.0.00.000.0000", _nfi));
			AssertEquals ("#02", "01.0000000000", 1.ToString ("00.0.00.000.0000", _nfi));
			AssertEquals ("#03", "-01.0000000000", (-1).ToString ("00.0.00.000.0000", _nfi));
		}

		[Test]
		public void Test08011 ()
		{
			AssertEquals ("#01", "", 0.ToString ("##.#.##.###.####", _nfi));
			AssertEquals ("#02", "1", 1.ToString ("##.#.##.###.####", _nfi));
			AssertEquals ("#03", "-1", (-1).ToString ("##.#.##.###.####", _nfi));
		}

		[Test]
		public void Test08012 ()
		{
			AssertEquals ("#01", "00", 0.ToString ("0#.#.##.###.####", _nfi));
			AssertEquals ("#02", "01", 1.ToString ("0#.#.##.###.####", _nfi));
			AssertEquals ("#03", "-01", (-1).ToString ("0#.#.##.###.####", _nfi));
		}

		[Test]
		public void Test08013 ()
		{
			AssertEquals ("#01", "0", 0.ToString ("#0.#.##.###.####", _nfi));
			AssertEquals ("#02", "1", 1.ToString ("#0.#.##.###.####", _nfi));
			AssertEquals ("#03", "-1", (-1).ToString ("#0.#.##.###.####", _nfi));
		}

		[Test]
		public void Test08014 ()
		{
			AssertEquals ("#01", ".0000000000", 0.ToString ("##.#.##.###.###0", _nfi));
			AssertEquals ("#02", "1.0000000000", 1.ToString ("##.#.##.###.###0", _nfi));
			AssertEquals ("#03", "-1.0000000000", (-1).ToString ("##.#.##.###.###0", _nfi));
		}

		[Test]
		public void Test08015 ()
		{
			AssertEquals ("#01", ".000000000", 0.ToString ("##.#.##.###.##0#", _nfi));
			AssertEquals ("#02", "1.000000000", 1.ToString ("##.#.##.###.##0#", _nfi));
			AssertEquals ("#03", "-1.000000000", (-1).ToString ("##.#.##.###.##0#", _nfi));
		}

		[Test]
		public void Test08016 ()
		{
			AssertEquals ("#01", ".000000000", 0.ToString ("##.#.##.##0.##0#", _nfi));
			AssertEquals ("#02", "1.000000000", 1.ToString ("##.#.##.##0.##0#", _nfi));
			AssertEquals ("#03", "-1.000000000", (-1).ToString ("##.#.##.##0.##0#", _nfi));
		}

		[Test]
		public void Test08017 ()
		{
			AssertEquals ("#01", "0.000000000", 0.ToString ("#0.#.##.##0.##0#", _nfi));
			AssertEquals ("#02", "1.000000000", 1.ToString ("#0.#.##.##0.##0#", _nfi));
			AssertEquals ("#03", "-1.000000000", (-1).ToString ("#0.#.##.##0.##0#", _nfi));
		}

		[Test]
		public void Test08018 ()
		{
			AssertEquals ("#01", "-0002147484", Int32.MinValue.ToString ("0000000000,", _nfi));
			AssertEquals ("#02", "-0000002147", Int32.MinValue.ToString ("0000000000,,", _nfi));
			AssertEquals ("#03", "-0000000002", Int32.MinValue.ToString ("0000000000,,,", _nfi));
			AssertEquals ("#04", "0000000000", Int32.MinValue.ToString ("0000000000,,,,", _nfi));
			AssertEquals ("#05", "0000000000", Int32.MinValue.ToString ("0000000000,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,", _nfi));
		}

		[Test]
		public void Test08019 ()
		{
			AssertEquals ("#01", "-2147483648", Int32.MinValue.ToString (",0000000000", _nfi));
		}

		[Test]
		public void Test08020 ()
		{
			AssertEquals ("#01", "-0002147484", Int32.MinValue.ToString (",0000000000,", _nfi));
		}

		[Test]
		public void Test08021 ()
		{
			AssertEquals ("#01", "-02,147,483,648", Int32.MinValue.ToString ("0,0000000000", _nfi));
		}

		[Test]
		public void Test08022 ()
		{
			AssertEquals ("#01", "-02,147,483,648", Int32.MinValue.ToString ("0000000000,0", _nfi));
		}

		[Test]
		public void Test08023 ()
		{
			AssertEquals ("#01", "-02,147,483,648", Int32.MinValue.ToString ("0,0,0,0,0,0,0,0,0,0,0", _nfi));
		}

		[Test]
		public void Test08024 ()
		{
			AssertEquals ("#01", "-02,147,483,648", Int32.MinValue.ToString (",0,0,0,0,0,0,0,0,0,0,0", _nfi));
		}

		[Test]
		public void Test08025 ()
		{
			AssertEquals ("#01", "-00,002,147,484", Int32.MinValue.ToString ("0,0,0,0,0,0,0,0,0,0,0,", _nfi));
		}

		[Test]
		public void Test08026 ()
		{
			AssertEquals ("#01", "-00,002,147,484", Int32.MinValue.ToString (",0,0,0,0,0,0,0,0,0,0,0,", _nfi));
		}

		[Test]
		public void Test08027 ()
		{
			AssertEquals ("#01", "-", Int32.MinValue.ToString (",", _nfi));
		}

		[Test]
		public void Test08028 ()
		{
			AssertEquals ("#01", "-2147483648", Int32.MinValue.ToString (",##########", _nfi));
		}

		[Test]
		public void Test08029 ()
		{
			AssertEquals ("#01", "-2147484", Int32.MinValue.ToString (",##########,", _nfi));
		}

		[Test]
		public void Test08030 ()
		{
			AssertEquals ("#01", "-2,147,483,648", Int32.MinValue.ToString ("#,##########", _nfi));
		}

		[Test]
		public void Test08031 ()
		{
			AssertEquals ("#01", "-2,147,483,648", Int32.MinValue.ToString ("##########,#", _nfi));
		}

		[Test]
		public void Test08032 ()
		{
			AssertEquals ("#01", "-2,147,483,648", Int32.MinValue.ToString ("#,#,#,#,#,#,#,#,#,#,#", _nfi));
		}

		[Test]
		public void Test08033 ()
		{
			AssertEquals ("#01", "-2,147,483,648", Int32.MinValue.ToString (",#,#,#,#,#,#,#,#,#,#,#", _nfi));
		}

		[Test]
		public void Test08034 ()
		{
			AssertEquals ("#01", "-2,147,484", Int32.MinValue.ToString ("#,#,#,#,#,#,#,#,#,#,#,", _nfi));
		}

		[Test]
		public void Test08035 ()
		{
			AssertEquals ("#01", "-2,147,484", Int32.MinValue.ToString (",#,#,#,#,#,#,#,#,#,#,#,", _nfi));
		}

		[Test]
		public void Test08036 ()
		{
			AssertEquals ("#01", "-1", (-1000).ToString ("##########,", _nfi));
		}

		[Test]
		public void Test08037 ()
		{
			AssertEquals ("#01", "", (-100).ToString ("##########,", _nfi));
		}

		[Test]
		public void Test08038 ()
		{
			AssertEquals ("#01", "-%", Int32.MinValue.ToString ("%", _nfi));
		}

		[Test]
		public void Test08039 ()
		{
			AssertEquals ("#01", "-214748364800%", Int32.MinValue.ToString ("0%", _nfi));
		}

		[Test]
		public void Test08040 ()
		{
			AssertEquals ("#01", "-%214748364800", Int32.MinValue.ToString ("%0", _nfi));
		}

		[Test]
		public void Test08041 ()
		{
			AssertEquals ("#01", "-%21474836480000%", Int32.MinValue.ToString ("%0%", _nfi));
		}

		[Test]
		public void Test08042 ()
		{
			AssertEquals ("#01", "- % 21474836480000 % ", Int32.MinValue.ToString (" % 0 % ", _nfi));
		}

		[Test]
		public void Test08043 ()
		{
			AssertEquals ("#01", "-214748365%", Int32.MinValue.ToString ("0%,", _nfi));
		}

		[Test]
		public void Test08044 ()
		{
			AssertEquals ("#01", "-214748365%", Int32.MinValue.ToString ("0,%", _nfi));
		}

		[Test]
		public void Test08045 ()
		{
			AssertEquals ("#01", "-%214748364800", Int32.MinValue.ToString (",%0", _nfi));
		}

		[Test]
		public void Test08046 ()
		{
			AssertEquals ("#01", "-%214748364800", Int32.MinValue.ToString ("%,0", _nfi));
		}

		[Test]
		public void Test08047 ()
		{
			AssertEquals ("#01", "-2147483648%%%%%%", Int32.MinValue.ToString ("0,,,,%%%%%%", _nfi));
		}

		[Test]
		public void Test08048 ()
		{
			AssertEquals ("#01", "-2147483648%%%%%%", Int32.MinValue.ToString ("0%%%%%%,,,,", _nfi));
		}

		[Test]
		public void Test08049 ()
		{
			AssertEquals ("#01", "-%%%%%%2147483648", Int32.MinValue.ToString ("%%%%%%0,,,,", _nfi));
		}

		[Test]
		public void Test08050 ()
		{
			AssertEquals ("#01", "E+0", Int32.MinValue.ToString ("E+0", _nfi));
			AssertEquals ("#02", "e+0", Int32.MinValue.ToString ("e+0", _nfi));
			AssertEquals ("#03", "E0", Int32.MinValue.ToString ("E-0", _nfi));
			AssertEquals ("#04", "e0", Int32.MinValue.ToString ("e-0", _nfi));
		}

		[Test]
		public void Test08051 ()
		{
			AssertEquals ("#01", "-2E+9", Int32.MinValue.ToString ("0E+0", _nfi));
			AssertEquals ("#02", "-2e+9", Int32.MinValue.ToString ("0e+0", _nfi));
			AssertEquals ("#03", "-2E9", Int32.MinValue.ToString ("0E-0", _nfi));
			AssertEquals ("#04", "-2e9", Int32.MinValue.ToString ("0e-0", _nfi));
			AssertEquals ("#05", "-2E9", Int32.MinValue.ToString ("0E0", _nfi));
			AssertEquals ("#06", "-2e9", Int32.MinValue.ToString ("0e0", _nfi));
		}

		[Test]
		public void Test08052 ()
		{
			AssertEquals ("#01", "-2E+9", Int32.MinValue.ToString ("#E+0", _nfi));
			AssertEquals ("#02", "-2e+9", Int32.MinValue.ToString ("#e+0", _nfi));
			AssertEquals ("#03", "-2E9", Int32.MinValue.ToString ("#E-0", _nfi));
			AssertEquals ("#04", "-2e9", Int32.MinValue.ToString ("#e-0", _nfi));
			AssertEquals ("#05", "-2E9", Int32.MinValue.ToString ("#E0", _nfi));
			AssertEquals ("#06", "-2e9", Int32.MinValue.ToString ("#e0", _nfi));
		}

		[Test]
		public void Test08053 ()
		{
			AssertEquals ("#01", "-2147483648E+0", Int32.MinValue.ToString ("0000000000E+0", _nfi));
			AssertEquals ("#02", "-2147483648e+0", Int32.MinValue.ToString ("0000000000e+0", _nfi));
			AssertEquals ("#03", "-2147483648E0", Int32.MinValue.ToString ("0000000000E-0", _nfi));
			AssertEquals ("#04", "-2147483648e0", Int32.MinValue.ToString ("0000000000e-0", _nfi));
			AssertEquals ("#05", "-2147483648E0", Int32.MinValue.ToString ("0000000000E0", _nfi));
			AssertEquals ("#06", "-2147483648e0", Int32.MinValue.ToString ("0000000000e0", _nfi));
		}

		[Test]
		public void Test08054 ()
		{
			AssertEquals ("#01", "-21474836480E-1", Int32.MinValue.ToString ("00000000000E+0", _nfi));
			AssertEquals ("#02", "-21474836480e-1", Int32.MinValue.ToString ("00000000000e+0", _nfi));
			AssertEquals ("#03", "-21474836480E-1", Int32.MinValue.ToString ("00000000000E-0", _nfi));
			AssertEquals ("#04", "-21474836480e-1", Int32.MinValue.ToString ("00000000000e-0", _nfi));
			AssertEquals ("#05", "-21474836480E-1", Int32.MinValue.ToString ("00000000000E0", _nfi));
			AssertEquals ("#06", "-21474836480e-1", Int32.MinValue.ToString ("00000000000e0", _nfi));
		}

		[Test]
		public void Test08055 ()
		{
			AssertEquals ("#01", "-214748365E+1", Int32.MinValue.ToString ("000000000E+0", _nfi));
			AssertEquals ("#02", "-214748365e+1", Int32.MinValue.ToString ("000000000e+0", _nfi));
			AssertEquals ("#03", "-214748365E1", Int32.MinValue.ToString ("000000000E-0", _nfi));
			AssertEquals ("#04", "-214748365e1", Int32.MinValue.ToString ("000000000e-0", _nfi));
			AssertEquals ("#05", "-214748365E1", Int32.MinValue.ToString ("000000000E0", _nfi));
			AssertEquals ("#06", "-214748365e1", Int32.MinValue.ToString ("000000000e0", _nfi));
		}

		[Test]
		public void Test08056 ()
		{
			AssertEquals ("#01", "-21474836E+2", Int32.MinValue.ToString ("00000000E+0", _nfi));
			AssertEquals ("#02", "-21474836e+2", Int32.MinValue.ToString ("00000000e+0", _nfi));
			AssertEquals ("#03", "-21474836E2", Int32.MinValue.ToString ("00000000E-0", _nfi));
			AssertEquals ("#04", "-21474836e2", Int32.MinValue.ToString ("00000000e-0", _nfi));
			AssertEquals ("#05", "-21474836E2", Int32.MinValue.ToString ("00000000E0", _nfi));
			AssertEquals ("#06", "-21474836e2", Int32.MinValue.ToString ("00000000e0", _nfi));
		}

		[Test]
		public void Test08057 ()
		{
			AssertEquals ("#01", "-2147483648E+00", Int32.MinValue.ToString ("0000000000E+00", _nfi));
			AssertEquals ("#02", "-2147483648e+00", Int32.MinValue.ToString ("0000000000e+00", _nfi));
			AssertEquals ("#03", "-2147483648E00", Int32.MinValue.ToString ("0000000000E-00", _nfi));
			AssertEquals ("#04", "-2147483648e00", Int32.MinValue.ToString ("0000000000e-00", _nfi));
			AssertEquals ("#05", "-2147483648E00", Int32.MinValue.ToString ("0000000000E00", _nfi));
			AssertEquals ("#06", "-2147483648e00", Int32.MinValue.ToString ("0000000000e00", _nfi));
		}

		[Test]
		public void Test08058 ()
		{
			AssertEquals ("#01", "-2147483648E+02%", Int32.MinValue.ToString ("0000000000E+00%", _nfi));
			AssertEquals ("#02", "-2147483648e+02%", Int32.MinValue.ToString ("0000000000e+00%", _nfi));
			AssertEquals ("#03", "-2147483648E02%", Int32.MinValue.ToString ("0000000000E-00%", _nfi));
			AssertEquals ("#04", "-2147483648e02%", Int32.MinValue.ToString ("0000000000e-00%", _nfi));
			AssertEquals ("#05", "-2147483648E02%", Int32.MinValue.ToString ("0000000000E00%", _nfi));
			AssertEquals ("#06", "-2147483648e02%", Int32.MinValue.ToString ("0000000000e00%", _nfi));
		}

		[Test]
		public void Test08059 ()
		{
			AssertEquals ("#01", "-2147483648E+10%%%%%", Int32.MinValue.ToString ("0000000000E+00%%%%%", _nfi));
			AssertEquals ("#02", "-2147483648e+10%%%%%", Int32.MinValue.ToString ("0000000000e+00%%%%%", _nfi));
			AssertEquals ("#03", "-2147483648E10%%%%%", Int32.MinValue.ToString ("0000000000E-00%%%%%", _nfi));
			AssertEquals ("#04", "-2147483648e10%%%%%", Int32.MinValue.ToString ("0000000000e-00%%%%%", _nfi));
			AssertEquals ("#05", "-2147483648E10%%%%%", Int32.MinValue.ToString ("0000000000E00%%%%%", _nfi));
			AssertEquals ("#06", "-2147483648e10%%%%%", Int32.MinValue.ToString ("0000000000e00%%%%%", _nfi));
		}

		[Test]
		public void Test08060 ()
		{
			AssertEquals ("#01", "-2147483648E-03", Int32.MinValue.ToString ("0000000000E+00,", _nfi));
			AssertEquals ("#02", "-2147483648e-03", Int32.MinValue.ToString ("0000000000e+00,", _nfi));
			AssertEquals ("#03", "-2147483648E-03", Int32.MinValue.ToString ("0000000000E-00,", _nfi));
			AssertEquals ("#04", "-2147483648e-03", Int32.MinValue.ToString ("0000000000e-00,", _nfi));
			AssertEquals ("#05", "-2147483648E-03", Int32.MinValue.ToString ("0000000000E00,", _nfi));
			AssertEquals ("#06", "-2147483648e-03", Int32.MinValue.ToString ("0000000000e00,", _nfi));
		}

		[Test]
		public void Test08061 ()
		{
			AssertEquals ("#01", "-2147483648E-12", Int32.MinValue.ToString ("0000000000E+00,,,,", _nfi));
			AssertEquals ("#02", "-2147483648e-12", Int32.MinValue.ToString ("0000000000e+00,,,,", _nfi));
			AssertEquals ("#03", "-2147483648E-12", Int32.MinValue.ToString ("0000000000E-00,,,,", _nfi));
			AssertEquals ("#04", "-2147483648e-12", Int32.MinValue.ToString ("0000000000e-00,,,,", _nfi));
			AssertEquals ("#05", "-2147483648E-12", Int32.MinValue.ToString ("0000000000E00,,,,", _nfi));
			AssertEquals ("#06", "-2147483648e-12", Int32.MinValue.ToString ("0000000000e00,,,,", _nfi));
		}

		[Test]
		public void Test08062 ()
		{
			AssertEquals ("#01", "-2147483648E-04%%%%", Int32.MinValue.ToString ("0000000000E+00,,,,%%%%", _nfi));
			AssertEquals ("#02", "-2147483648e-04%%%%", Int32.MinValue.ToString ("0000000000e+00,,,,%%%%", _nfi));
			AssertEquals ("#03", "-2147483648E-04%%%%", Int32.MinValue.ToString ("0000000000E-00,,,,%%%%", _nfi));
			AssertEquals ("#04", "-2147483648e-04%%%%", Int32.MinValue.ToString ("0000000000e-00,,,,%%%%", _nfi));
			AssertEquals ("#05", "-2147483648E-04%%%%", Int32.MinValue.ToString ("0000000000E00,,,,%%%%", _nfi));
			AssertEquals ("#06", "-2147483648e-04%%%%", Int32.MinValue.ToString ("0000000000e00,,,,%%%%", _nfi));
		}

		[Test]
		public void Test08063 ()
		{
			AssertEquals ("#01", "-2147483648E-07%%%%", Int32.MinValue.ToString ("0000000000,E+00,,,,%%%%", _nfi));
			AssertEquals ("#02", "-2147483648e-07%%%%", Int32.MinValue.ToString ("0000000000,e+00,,,,%%%%", _nfi));
			AssertEquals ("#03", "-2147483648E-07%%%%", Int32.MinValue.ToString ("0000000000,E-00,,,,%%%%", _nfi));
			AssertEquals ("#04", "-2147483648e-07%%%%", Int32.MinValue.ToString ("0000000000,e-00,,,,%%%%", _nfi));
			AssertEquals ("#05", "-2147483648E-07%%%%", Int32.MinValue.ToString ("0000000000,E00,,,,%%%%", _nfi));
			AssertEquals ("#06", "-2147483648e-07%%%%", Int32.MinValue.ToString ("0000000000,e00,,,,%%%%", _nfi));
		}

		[Test]
		public void Test08064 ()
		{
			AssertEquals ("#01", "-000,000,214,7E+48%%%%", Int32.MinValue.ToString ("0000000000,E,+00,,,,%%%%", _nfi));
			AssertEquals ("#02", "-000,000,214,7e+48%%%%", Int32.MinValue.ToString ("0000000000,e,+00,,,,%%%%", _nfi));
			AssertEquals ("#03", "-000,000,214,7E-48%%%%", Int32.MinValue.ToString ("0000000000,E,-00,,,,%%%%", _nfi));
			AssertEquals ("#04", "-000,000,214,7e-48%%%%", Int32.MinValue.ToString ("0000000000,e,-00,,,,%%%%", _nfi));
			AssertEquals ("#05", "-000,000,214,7E48%%%%", Int32.MinValue.ToString ("0000000000,E,00,,,,%%%%", _nfi));
			AssertEquals ("#06", "-000,000,214,7e48%%%%", Int32.MinValue.ToString ("0000000000,e,00,,,,%%%%", _nfi));
		}

		[Test]
		public void Test08065 ()
		{
			AssertEquals ("#01", "-000,000,214,7E+48%%%%", Int32.MinValue.ToString ("0000000000,E+,00,,,,%%%%", _nfi));
			AssertEquals ("#02", "-000,000,214,7e+48%%%%", Int32.MinValue.ToString ("0000000000,e+,00,,,,%%%%", _nfi));
			AssertEquals ("#03", "-000,000,214,7E-48%%%%", Int32.MinValue.ToString ("0000000000,E-,00,,,,%%%%", _nfi));
			AssertEquals ("#04", "-000,000,214,7e-48%%%%", Int32.MinValue.ToString ("0000000000,e-,00,,,,%%%%", _nfi));
		}

		[Test]
		public void Test08066 ()
		{
			AssertEquals ("#01", "-21,474,836,48E-50%%%%", Int32.MinValue.ToString ("0000000000,E+0,0,,,,%%%%", _nfi));
			AssertEquals ("#02", "-21,474,836,48e-50%%%%", Int32.MinValue.ToString ("0000000000,e+0,0,,,,%%%%", _nfi));
			AssertEquals ("#03", "-21,474,836,48E-50%%%%", Int32.MinValue.ToString ("0000000000,E-0,0,,,,%%%%", _nfi));
			AssertEquals ("#04", "-21,474,836,48e-50%%%%", Int32.MinValue.ToString ("0000000000,e-0,0,,,,%%%%", _nfi));
			AssertEquals ("#05", "-21,474,836,48E-50%%%%", Int32.MinValue.ToString ("0000000000,E0,0,,,,%%%%", _nfi));
			AssertEquals ("#06", "-21,474,836,48e-50%%%%", Int32.MinValue.ToString ("0000000000,e0,0,,,,%%%%", _nfi));
		}

		[Test]
		public void Test08067 ()
		{
			AssertEquals ("#01", "-2147483648E-01,%%%%", Int32.MinValue.ToString (@"0000000000E+00\,,,,%%%%", _nfi));
			AssertEquals ("#02", "-2147483648e-01,%%%%", Int32.MinValue.ToString (@"0000000000e+00\,,,,%%%%", _nfi));
			AssertEquals ("#03", "-2147483648E-01,%%%%", Int32.MinValue.ToString (@"0000000000E-00\,,,,%%%%", _nfi));
			AssertEquals ("#04", "-2147483648e-01,%%%%", Int32.MinValue.ToString (@"0000000000e-00\,,,,%%%%", _nfi));
			AssertEquals ("#05", "-2147483648E-01,%%%%", Int32.MinValue.ToString (@"0000000000E00\,,,,%%%%", _nfi));
			AssertEquals ("#06", "-2147483648e-01,%%%%", Int32.MinValue.ToString (@"0000000000e00\,,,,%%%%", _nfi));
		}

		[Test]
		public void Test08068 ()
		{
			AssertEquals ("#01", "-2147483648E+02,,%%%%", Int32.MinValue.ToString (@"0000000000E+00\,,,\,%%%%", _nfi));
			AssertEquals ("#02", "-2147483648e+02,,%%%%", Int32.MinValue.ToString (@"0000000000e+00\,,,\,%%%%", _nfi));
			AssertEquals ("#03", "-2147483648E02,,%%%%", Int32.MinValue.ToString (@"0000000000E-00\,,,\,%%%%", _nfi));
			AssertEquals ("#04", "-2147483648e02,,%%%%", Int32.MinValue.ToString (@"0000000000e-00\,,,\,%%%%", _nfi));
			AssertEquals ("#05", "-2147483648E02,,%%%%", Int32.MinValue.ToString (@"0000000000E00\,,,\,%%%%", _nfi));
			AssertEquals ("#06", "-2147483648e02,,%%%%", Int32.MinValue.ToString (@"0000000000e00\,,,\,%%%%", _nfi));
		}

		[Test]
		public void Test08069 ()
		{
			AssertEquals ("#01", "-2147483648E+00,,%%%%", Int32.MinValue.ToString (@"0000000000E+00\,,,\,\%%%%", _nfi));
			AssertEquals ("#02", "-2147483648e+00,,%%%%", Int32.MinValue.ToString (@"0000000000e+00\,,,\,\%%%%", _nfi));
			AssertEquals ("#03", "-2147483648E00,,%%%%", Int32.MinValue.ToString (@"0000000000E-00\,,,\,\%%%%", _nfi));
			AssertEquals ("#04", "-2147483648e00,,%%%%", Int32.MinValue.ToString (@"0000000000e-00\,,,\,\%%%%", _nfi));
			AssertEquals ("#05", "-2147483648E00,,%%%%", Int32.MinValue.ToString (@"0000000000E00\,,,\,\%%%%", _nfi));
			AssertEquals ("#06", "-2147483648e00,,%%%%", Int32.MinValue.ToString (@"0000000000e00\,,,\,\%%%%", _nfi));
		}

		[Test]
		public void Test08070 ()
		{
			AssertEquals ("#01", "-2147483648E-02,,%%%%", Int32.MinValue.ToString (@"0000000000E+00\,,,\,\%%%\%", _nfi));
			AssertEquals ("#02", "-2147483648e-02,,%%%%", Int32.MinValue.ToString (@"0000000000e+00\,,,\,\%%%\%", _nfi));
			AssertEquals ("#03", "-2147483648E-02,,%%%%", Int32.MinValue.ToString (@"0000000000E-00\,,,\,\%%%\%", _nfi));
			AssertEquals ("#04", "-2147483648e-02,,%%%%", Int32.MinValue.ToString (@"0000000000e-00\,,,\,\%%%\%", _nfi));
			AssertEquals ("#05", "-2147483648E-02,,%%%%", Int32.MinValue.ToString (@"0000000000E00\,,,\,\%%%\%", _nfi));
			AssertEquals ("#06", "-2147483648e-02,,%%%%", Int32.MinValue.ToString (@"0000000000e00\,,,\,\%%%\%", _nfi));
		}

		[Test]
		public void Test08071 ()
		{
			AssertEquals ("#01", @"-2147483648E-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000E+00\\,,,\\,\\%%%\\%", _nfi));
			AssertEquals ("#02", @"-2147483648e-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000e+00\\,,,\\,\\%%%\\%", _nfi));
			AssertEquals ("#03", @"-2147483648E-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000E-00\\,,,\\,\\%%%\\%", _nfi));
			AssertEquals ("#04", @"-2147483648e-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000e-00\\,,,\\,\\%%%\\%", _nfi));
			AssertEquals ("#05", @"-2147483648E-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000E00\\,,,\\,\\%%%\\%", _nfi));
			AssertEquals ("#06", @"-2147483648e-04\\\%%%\%", Int32.MinValue.ToString (@"0000000000e00\\,,,\\,\\%%%\\%", _nfi));
		}

		[Test]
		public void Test08072 ()
		{
			AssertEquals ("#01", @"-2147483648E+00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E+00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#02", @"-2147483648e+00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e+00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#03", @"-2147483648E00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E-00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#04", @"-2147483648e00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e-00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#05", @"-2147483648E00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#06", @"-2147483648e00\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e00\\,\,,\\\,\\%%%\\\%", _nfi));
		}

		[Test]
		public void Test08073 ()
		{
			AssertEquals ("#01", @"-0021474836E+48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\E+00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#02", @"-0021474836e+48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\e+00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#03", @"-0021474836E-48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\E-00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#04", @"-0021474836e-48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\e-00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#05", @"-0021474836E48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\E00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#06", @"-0021474836e48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000\e00\\,\,,\\\,\\%%%\\\%", _nfi));
		}

		[Test]
		public void Test08074 ()
		{
			AssertEquals ("#01", @"-0021474836E+48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E\+00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#02", @"-0021474836e+48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e\+00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#03", @"-0021474836E-48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000E\-00\\,\,,\\\,\\%%%\\\%", _nfi));
			AssertEquals ("#04", @"-0021474836e-48\,\,\%%%\%", Int32.MinValue.ToString (@"0000000000e\-00\\,\,,\\\,\\%%%\\\%", _nfi));
		}

		[Test]
		public void Test08075 ()
		{
			AssertEquals ("#01", "-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E+00,,,',%'%%%", _nfi));
			AssertEquals ("#02", "-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e+00,,,',%'%%%", _nfi));
			AssertEquals ("#03", "-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E-00,,,',%'%%%", _nfi));
			AssertEquals ("#04", "-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e-00,,,',%'%%%", _nfi));
			AssertEquals ("#05", "-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E00,,,',%'%%%", _nfi));
			AssertEquals ("#06", "-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e00,,,',%'%%%", _nfi));
		}

		[Test]
		public void Test08076 ()
		{
			AssertEquals ("#01", "-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E+00,,,\",%\"%%%", _nfi));
			AssertEquals ("#02", "-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e+00,,,\",%\"%%%", _nfi));
			AssertEquals ("#03", "-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E-00,,,\",%\"%%%", _nfi));
			AssertEquals ("#04", "-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e-00,,,\",%\"%%%", _nfi));
			AssertEquals ("#05", "-2147483648E-03,%%%%", Int32.MinValue.ToString ("0000000000E00,,,\",%\"%%%", _nfi));
			AssertEquals ("#06", "-2147483648e-03,%%%%", Int32.MinValue.ToString ("0000000000e00,,,\",%\"%%%", _nfi));
		}

		[Test]
		public void Test08077 ()
		{
			AssertEquals ("#01", "-", Int32.MinValue.ToString (";", _nfi));
			AssertEquals ("#02", "", Int32.MaxValue.ToString (";", _nfi));
			AssertEquals ("#03", "",0.ToString (";", _nfi));
		}

		[Test]
		public void Test08078 ()
		{
			AssertEquals ("#01", "-2,147,483,648", Int32.MinValue.ToString ("#,#;", _nfi));
			AssertEquals ("#02", "2,147,483,647", Int32.MaxValue.ToString ("#,#;", _nfi));
			AssertEquals ("#03", "", 0.ToString ("#,#;", _nfi));
		}

		[Test]
		public void Test08079 ()
		{
			AssertEquals ("#01", "2,147,483,648", Int32.MinValue.ToString (";#,#", _nfi));
			AssertEquals ("#02", "", Int32.MaxValue.ToString (";#,#", _nfi));
			AssertEquals ("#03", "", 0.ToString (";#,#", _nfi));
		}

		[Test]
		public void Test08080 ()
		{
			AssertEquals ("#01", "2,147,483,648", Int32.MinValue.ToString ("0000000000,.0000000000;#,#", _nfi));
			AssertEquals ("#02", "0002147483.6470000000", Int32.MaxValue.ToString ("0000000000,.0000000000;#,#", _nfi));
			AssertEquals ("#03", "0000000000.0000000000", 0.ToString ("0000000000,.0000000000;#,#", _nfi));
		}

		[Test]
		public void Test08081 ()
		{
			AssertEquals ("#01", "-", Int32.MinValue.ToString (";;", _nfi));
			AssertEquals ("#02", "", Int32.MaxValue.ToString (";;", _nfi));
			AssertEquals ("#03", "",0.ToString (";;", _nfi));
		}

		[Test]
		public void Test08082 ()
		{
			AssertEquals ("#01", "-", Int32.MinValue.ToString (";;0%", _nfi));
			AssertEquals ("#02", "", Int32.MaxValue.ToString (";;0%", _nfi));
			AssertEquals ("#03", "0%",0.ToString (";;0%", _nfi));
		}

		[Test]
		public void Test08083 ()
		{
			AssertEquals ("#01", "2147484", Int32.MinValue.ToString (";0,;0%", _nfi));
			AssertEquals ("#02", "", Int32.MaxValue.ToString (";0,;0%", _nfi));
			AssertEquals ("#03", "0%",0.ToString (";0,;0%", _nfi));
		}

		[Test]
		public void Test08084 ()
		{
			AssertEquals ("#01", "2147484", Int32.MinValue.ToString ("0E+0;0,;0%", _nfi));
			AssertEquals ("#02", "2E+9", Int32.MaxValue.ToString ("0E+0;0,;0%", _nfi));
			AssertEquals ("#03", "0%",0.ToString ("0E+0;0,;0%", _nfi));
		}

		[Test]
		public void Test08085 ()
		{
			AssertEquals ("#01", "214,748,364,80;0%", Int32.MinValue.ToString (@"0E+0;0,\;0%", _nfi));
			AssertEquals ("#02", "2E+9", Int32.MaxValue.ToString (@"0E+0;0,\;0%", _nfi));
			AssertEquals ("#03", "0E+0",0.ToString (@"0E+0;0,\;0%", _nfi));
		}

		[Test]
		public void Test08086 ()
		{
			AssertEquals ("#01", "214,748,364,80;0%", Int32.MinValue.ToString ("0E+0;0,\";\"0%", _nfi));
			AssertEquals ("#02", "2E+9", Int32.MaxValue.ToString ("0E+0;0,\";\"0%", _nfi));
			AssertEquals ("#03", "0E+0",0.ToString ("0E+0;0,\";\"0%", _nfi));
		}

		[Test]
		public void Test08087 ()
		{
			// MS.NET bug?
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "$$$";
			AssertEquals ("#01", "-0000000000$$$2147483648", Int32.MinValue.ToString ("0000000000$$$0000000000", nfi));
		}

		[Test]
		public void Test08088 ()
		{
			// MS.NET bug?
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo.Clone() as NumberFormatInfo;
			nfi.NumberGroupSeparator = "$$$";
			AssertEquals ("#01", "-0000000000$$$2147483648", Int32.MinValue.ToString ("0000000000$$$0000000000", nfi));
		}

		[Test]
		public void Test08089 ()
		{
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo.Clone() as NumberFormatInfo;
			nfi.NumberGroupSizes = new int[] {3,2,1,0};
			AssertEquals ("#01", "-00000000002147,4,83,648", Int32.MinValue.ToString ("0000000000,0000000000", nfi));
		}

		[Test]
		public void Test08090 ()
		{
			// MS.NET bug?
			NumberFormatInfo nfi = NumberFormatInfo.InvariantInfo.Clone() as NumberFormatInfo;
			nfi.PercentSymbol = "$$$";
			AssertEquals ("#01", "-0000000000$$$2147483648", Int32.MinValue.ToString ("0000000000$$$0000000000", nfi));
		}

		[Test]
		public void Test08091 ()
		{
			// MS.NET bug?
			AssertEquals ("#01", "B2147", Int32.MinValue.ToString ("A0,;B0,,;C0,,,;D0,,,,;E0,,,,,", _nfi));
			AssertEquals ("#02", "A2147484", Int32.MaxValue.ToString ("A0,;B0,,;C0,,,;D0,,,,;E0,,,,,", _nfi));
			AssertEquals ("#03", "C0", 0.ToString ("A0,;B0,,;C0,,,;D0,,,,;E0,,,,,", _nfi));
		}

		// Test10000- Double and D
		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Test10000 ()
		{
			AssertEquals ("#01", "0", 0.0.ToString ("D", _nfi));
		}

		// Test11000- Double and E
		[Test]
		public void Test11000 ()
		{
			AssertEquals ("#01", "0.000000E+000", 0.0.ToString ("E", _nfi));
			AssertEquals ("#02", "0.000000e+000", 0.0.ToString ("e", _nfi));
			AssertEquals ("#03", "-1.797693E+308", Double.MinValue.ToString ("E", _nfi));
			AssertEquals ("#04", "-1.797693e+308", Double.MinValue.ToString ("e", _nfi));
			AssertEquals ("#05", "1.797693E+308", Double.MaxValue.ToString ("E", _nfi));
			AssertEquals ("#06", "1.797693e+308", Double.MaxValue.ToString ("e", _nfi));
		}

		[Test]
		public void Test11001 ()
		{
			AssertEquals ("#01", "E ", 0.0.ToString ("E ", _nfi));
			AssertEquals ("#02", " E", 0.0.ToString (" E", _nfi));
			AssertEquals ("#03", " E ", 0.0.ToString (" E ", _nfi));
		}

		[Test]
		public void Test11002 ()
		{
			AssertEquals ("#01", "-E ", (-1.0).ToString ("E ", _nfi));
			AssertEquals ("#02", "- E", (-1.0).ToString (" E", _nfi));
			AssertEquals ("#03", "- E ", (-1.0).ToString (" E ", _nfi));
		}

		[Test]
		public void Test11003 ()
		{
			AssertEquals ("#01", "0E+000", 0.0.ToString ("E0", _nfi));
			AssertEquals ("#02", "0.0000000000000000E+000", 0.0.ToString ("E16", _nfi));
			AssertEquals ("#03", "0.00000000000000000E+000", 0.0.ToString ("E17", _nfi));
			AssertEquals ("#04", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000E+000", 0.0.ToString ("E99", _nfi));
			AssertEquals ("#05", "E100", 0.0.ToString ("E100", _nfi));
		}

		[Test]
		public void Test11004 ()
		{
			AssertEquals ("#01", "2E+308", Double.MaxValue.ToString ("E0", _nfi));
			AssertEquals ("#02", "1.7976931348623157E+308", Double.MaxValue.ToString ("E16", _nfi));
			AssertEquals ("#03", "1.79769313486231570E+308", Double.MaxValue.ToString ("E17", _nfi));
			AssertEquals ("#04", "1.797693134862315700000000000000000000000000000000000000000000000000000000000000000000000000000000000E+308", Double.MaxValue.ToString ("E99", _nfi));
			AssertEquals ("#05", "E1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("E100", _nfi));
		}

		[Test]
		public void Test11005 ()
		{
			AssertEquals ("#01", "-2E+308", Double.MinValue.ToString ("E0", _nfi));
			AssertEquals ("#02", "-1.7976931348623157E+308", Double.MinValue.ToString ("E16", _nfi));
			AssertEquals ("#03", "-1.79769313486231570E+308", Double.MinValue.ToString ("E17", _nfi));
			AssertEquals ("#04", "-1.797693134862315700000000000000000000000000000000000000000000000000000000000000000000000000000000000E+308", Double.MinValue.ToString ("E99", _nfi));
			AssertEquals ("#05", "-E1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("E100", _nfi));
		}

		[Test]
		public void Test11006 ()
		{
			AssertEquals ("#01", "EF", 0.0.ToString ("EF", _nfi));
			AssertEquals ("#02", "E0F", 0.0.ToString ("E0F", _nfi));
			AssertEquals ("#03", "E0xF", 0.0.ToString ("E0xF", _nfi));
		}

		[Test]
		public void Test11007 ()
		{
			AssertEquals ("#01", "EF", Double.MaxValue.ToString ("EF", _nfi));
			AssertEquals ("#02", "E0F", Double.MaxValue.ToString ("E0F", _nfi));
			AssertEquals ("#03", "E0xF", Double.MaxValue.ToString ("E0xF", _nfi));
		}

		[Test]
		public void Test11008 ()
		{
			AssertEquals ("#01", "-EF", Double.MinValue.ToString ("EF", _nfi));
			AssertEquals ("#02", "E0F", Double.MinValue.ToString ("E0F", _nfi));
			AssertEquals ("#03", "E0xF", Double.MinValue.ToString ("E0xF", _nfi));
		}

		[Test]
		public void Test11009 ()
		{
			AssertEquals ("#01", "0.00000000000000000E+000", 0.0.ToString ("E0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#02", "1.79769313486231570E+308", Double.MaxValue.ToString ("E0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#03", "-1.79769313486231570E+308", Double.MinValue.ToString ("E0000000000000000000000000000000000000017", _nfi));
		}

		[Test]
		public void Test11010 ()
		{
			AssertEquals ("#01", "+E", 0.0.ToString ("+E", _nfi));
			AssertEquals ("#02", "E+", 0.0.ToString ("E+", _nfi));
			AssertEquals ("#03", "+E+", 0.0.ToString ("+E+", _nfi));
		}
		
		[Test]
		public void Test11011 ()
		{
			AssertEquals ("#01", "+E", Double.MaxValue.ToString ("+E", _nfi));
			AssertEquals ("#02", "E+", Double.MaxValue.ToString ("E+", _nfi));
			AssertEquals ("#03", "+E+", Double.MaxValue.ToString ("+E+", _nfi));
		}

		[Test]
		public void Test11012 ()
		{
			AssertEquals ("#01", "-+E", Double.MinValue.ToString ("+E", _nfi));
			AssertEquals ("#02", "-E+", Double.MinValue.ToString ("E+", _nfi));
			AssertEquals ("#03", "-+E+", Double.MinValue.ToString ("+E+", _nfi));
		}

		[Test]
		public void Test11013 ()
		{
			AssertEquals ("#01", "-E", 0.0.ToString ("-E", _nfi));
			AssertEquals ("#02", "E-", 0.0.ToString ("E-", _nfi));
			AssertEquals ("#03", "-E-", 0.0.ToString ("-E-", _nfi));
		}
		
		[Test]
		public void Test11014 ()
		{
			AssertEquals ("#01", "-E", Double.MaxValue.ToString ("-E", _nfi));
			AssertEquals ("#02", "E-", Double.MaxValue.ToString ("E-", _nfi));
			AssertEquals ("#03", "-E-", Double.MaxValue.ToString ("-E-", _nfi));
		}

		[Test]
		public void Test11015 ()
		{
			AssertEquals ("#01", "--E", Double.MinValue.ToString ("-E", _nfi));
			AssertEquals ("#02", "-E-", Double.MinValue.ToString ("E-", _nfi));
			AssertEquals ("#03", "--E-", Double.MinValue.ToString ("-E-", _nfi));
		}

		[Test]
		public void Test11016 ()
		{
			AssertEquals ("#01", "E+0", 0.0.ToString ("E+0", _nfi));
			AssertEquals ("#02", "E+0", Double.MaxValue.ToString ("E+0", _nfi));
			AssertEquals ("#03", "E+0", Double.MinValue.ToString ("E+0", _nfi));
		}

		[Test]
		public void Test11017 ()
		{
			AssertEquals ("#01", "E+9", 0.0.ToString ("E+9", _nfi));
			AssertEquals ("#02", "E+9", Double.MaxValue.ToString ("E+9", _nfi));
			AssertEquals ("#03", "-E+9", Double.MinValue.ToString ("E+9", _nfi));
		}

		[Test]
		public void Test11018 ()
		{
			AssertEquals ("#01", "E-9", 0.0.ToString ("E-9", _nfi));
			AssertEquals ("#02", "E-9", Double.MaxValue.ToString ("E-9", _nfi));
			AssertEquals ("#03", "-E-9", Double.MinValue.ToString ("E-9", _nfi));
		}

		[Test]
		public void Test11019 ()
		{
			AssertEquals ("#01", "E0", 0.0.ToString ("E0,", _nfi));
			AssertEquals ("#02", "E0", Double.MaxValue.ToString ("E0,", _nfi));
			AssertEquals ("#03", "E0", Double.MinValue.ToString ("E0,", _nfi));
		}

		[Test]
		public void Test11020 ()
		{
			AssertEquals ("#01", "E0", 0.0.ToString ("E0.", _nfi));
			AssertEquals ("#02", "E0", Double.MaxValue.ToString ("E0.", _nfi));
			AssertEquals ("#03", "E0", Double.MinValue.ToString ("E0.", _nfi));
		}

		[Test]
		public void Test11021 ()
		{
			AssertEquals ("#01", "E0.0", 0.0.ToString ("E0.0", _nfi));
			AssertEquals ("#02", "E309.2", Double.MaxValue.ToString ("E0.0", _nfi));
			AssertEquals ("#03", "-E309.2", Double.MinValue.ToString ("E0.0", _nfi));
		}

		[Test]
		public void Test11022 ()
		{
			AssertEquals ("#01", "E09", 0.0.ToString ("E0.9", _nfi));
			AssertEquals ("#02", "E09", Double.MaxValue.ToString ("E0.9", _nfi));
			AssertEquals ("#03", "E09", Double.MinValue.ToString ("E0.9", _nfi));
		}

		[Test]
		public void Test11023 ()
		{
			AssertEquals ("#01", "1.1E+000", 1.05.ToString ("E1", _nfi));
			AssertEquals ("#02", "1.2E+000", 1.15.ToString ("E1", _nfi));
			AssertEquals ("#03", "1.3E+000", 1.25.ToString ("E1", _nfi));
			AssertEquals ("#04", "1.4E+000", 1.35.ToString ("E1", _nfi));
			AssertEquals ("#05", "1.5E+000", 1.45.ToString ("E1", _nfi));
			AssertEquals ("#06", "1.6E+000", 1.55.ToString ("E1", _nfi));
			AssertEquals ("#07", "1.7E+000", 1.65.ToString ("E1", _nfi));
			AssertEquals ("#08", "1.8E+000", 1.75.ToString ("E1", _nfi));
			AssertEquals ("#09", "1.9E+000", 1.85.ToString ("E1", _nfi));
			AssertEquals ("#10", "2.0E+000", 1.95.ToString ("E1", _nfi));
		}

		[Test]
		public void Test11024 ()
		{
			AssertEquals ("#01", "1.01E+000", 1.005.ToString ("E2", _nfi));
			AssertEquals ("#02", "1.02E+000", 1.015.ToString ("E2", _nfi));
			AssertEquals ("#03", "1.03E+000", 1.025.ToString ("E2", _nfi));
			AssertEquals ("#04", "1.04E+000", 1.035.ToString ("E2", _nfi));
			AssertEquals ("#05", "1.05E+000", 1.045.ToString ("E2", _nfi));
			AssertEquals ("#06", "1.06E+000", 1.055.ToString ("E2", _nfi));
			AssertEquals ("#07", "1.07E+000", 1.065.ToString ("E2", _nfi));
			AssertEquals ("#08", "1.08E+000", 1.075.ToString ("E2", _nfi));
			AssertEquals ("#09", "1.09E+000", 1.085.ToString ("E2", _nfi));
			AssertEquals ("#10", "1.10E+000", 1.095.ToString ("E2", _nfi));
		}

		[Test]
		public void Test11025 ()
		{
			AssertEquals ("#01", "1.00000000000001E+000", 1.000000000000005.ToString ("E14", _nfi));
			AssertEquals ("#02", "1.00000000000002E+000", 1.000000000000015.ToString ("E14", _nfi));
			AssertEquals ("#03", "1.00000000000003E+000", 1.000000000000025.ToString ("E14", _nfi));
			AssertEquals ("#04", "1.00000000000004E+000", 1.000000000000035.ToString ("E14", _nfi));
			AssertEquals ("#05", "1.00000000000005E+000", 1.000000000000045.ToString ("E14", _nfi));
			AssertEquals ("#06", "1.00000000000006E+000", 1.000000000000055.ToString ("E14", _nfi));
			AssertEquals ("#07", "1.00000000000007E+000", 1.000000000000065.ToString ("E14", _nfi));
			AssertEquals ("#08", "1.00000000000008E+000", 1.000000000000075.ToString ("E14", _nfi));
			AssertEquals ("#09", "1.00000000000009E+000", 1.000000000000085.ToString ("E14", _nfi));
			AssertEquals ("#10", "1.00000000000010E+000", 1.000000000000095.ToString ("E14", _nfi));
		}

		[Test]
		public void Test11026 ()
		{
			AssertEquals ("#01", "1.000000000000000E+000", 1.0000000000000005.ToString ("E15", _nfi));
			AssertEquals ("#02", "1.000000000000002E+000", 1.0000000000000015.ToString ("E15", _nfi));
			AssertEquals ("#03", "1.000000000000002E+000", 1.0000000000000025.ToString ("E15", _nfi));
			AssertEquals ("#04", "1.000000000000004E+000", 1.0000000000000035.ToString ("E15", _nfi));
			AssertEquals ("#05", "1.000000000000004E+000", 1.0000000000000045.ToString ("E15", _nfi));
			AssertEquals ("#06", "1.000000000000006E+000", 1.0000000000000055.ToString ("E15", _nfi));
			AssertEquals ("#07", "1.000000000000006E+000", 1.0000000000000065.ToString ("E15", _nfi));
			AssertEquals ("#08", "1.000000000000008E+000", 1.0000000000000075.ToString ("E15", _nfi));
			AssertEquals ("#09", "1.000000000000008E+000", 1.0000000000000085.ToString ("E15", _nfi));
			AssertEquals ("#10", "1.000000000000010E+000", 1.0000000000000095.ToString ("E15", _nfi));
		}

		[Test]
		public void Test11027 ()
		{
			AssertEquals ("#01", "1.0000000000000000E+000", 1.00000000000000005.ToString ("E16", _nfi));
			AssertEquals ("#02", "1.0000000000000002E+000", 1.00000000000000015.ToString ("E16", _nfi));
			AssertEquals ("#03", "1.0000000000000002E+000", 1.00000000000000025.ToString ("E16", _nfi));
			AssertEquals ("#04", "1.0000000000000004E+000", 1.00000000000000035.ToString ("E16", _nfi));
			AssertEquals ("#05", "1.0000000000000004E+000", 1.00000000000000045.ToString ("E16", _nfi));
			AssertEquals ("#06", "1.0000000000000004E+000", 1.00000000000000055.ToString ("E16", _nfi));
			AssertEquals ("#07", "1.0000000000000007E+000", 1.00000000000000065.ToString ("E16", _nfi));
			AssertEquals ("#08", "1.0000000000000007E+000", 1.00000000000000075.ToString ("E16", _nfi));
			AssertEquals ("#09", "1.0000000000000009E+000", 1.00000000000000085.ToString ("E16", _nfi));
			AssertEquals ("#10", "1.0000000000000009E+000", 1.00000000000000095.ToString ("E16", _nfi));
		}

		[Test]
		public void Test11028 ()
		{
			AssertEquals ("#01", "1.00000000000000000E+000", 1.000000000000000005.ToString ("E17", _nfi));
			AssertEquals ("#02", "1.00000000000000000E+000", 1.000000000000000015.ToString ("E17", _nfi));
			AssertEquals ("#03", "1.00000000000000000E+000", 1.000000000000000025.ToString ("E17", _nfi));
			AssertEquals ("#04", "1.00000000000000000E+000", 1.000000000000000035.ToString ("E17", _nfi));
			AssertEquals ("#05", "1.00000000000000000E+000", 1.000000000000000045.ToString ("E17", _nfi));
			AssertEquals ("#06", "1.00000000000000000E+000", 1.000000000000000055.ToString ("E17", _nfi));
			AssertEquals ("#07", "1.00000000000000000E+000", 1.000000000000000065.ToString ("E17", _nfi));
			AssertEquals ("#08", "1.00000000000000000E+000", 1.000000000000000075.ToString ("E17", _nfi));
			AssertEquals ("#09", "1.00000000000000000E+000", 1.000000000000000085.ToString ("E17", _nfi));
			AssertEquals ("#10", "1.00000000000000000E+000", 1.000000000000000095.ToString ("E17", _nfi));
		}

		[Test]
		public void Test11029 ()
		{
			AssertEquals ("#01", "1E+000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E0"));
			AssertEquals ("#02", "1.2345678901234567E+000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E16"));
			AssertEquals ("#03", "1.23456789012345670E+000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E17"));
			AssertEquals ("#04", "1.234567890123456700000000000000000000000000000000000000000000000000000000000000000000000000000000000E+000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E99"));
			AssertEquals ("#04", "E101", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("E100"));
		}

		[Test]
		public void Test11030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			AssertEquals ("#01", "-1#000000E+008", (-99999999.9).ToString ("E", nfi));
		}

		[Test]
		public void Test11031 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			AssertEquals ("#01", "1.000000E-000", 1.0.ToString ("E", nfi));
			AssertEquals ("#02", "0.000000E-000", 0.0.ToString ("E", nfi));
			AssertEquals ("#03", "+1.000000E-000", (-1.0).ToString ("E", nfi));
		}

		[Test]
		public void TestNaNToString ()
		{
			AssertEquals ("#01", "Infinity", Double.PositiveInfinity.ToString());
			AssertEquals ("#02", "-Infinity", Double.NegativeInfinity.ToString());
			AssertEquals ("#03", "NaN", Double.NaN.ToString());
			AssertEquals ("#04", "Infinity", Single.PositiveInfinity.ToString());
			AssertEquals ("#05", "-Infinity", Single.NegativeInfinity.ToString());
			AssertEquals ("#06", "NaN", Single.NaN.ToString());

			AssertEquals ("#07", "Infinity", Double.PositiveInfinity.ToString("R"));
			AssertEquals ("#08", "-Infinity", Double.NegativeInfinity.ToString("R"));
			AssertEquals ("#09", "NaN", Double.NaN.ToString("R"));
			AssertEquals ("#10", "Infinity", Single.PositiveInfinity.ToString("R"));
			AssertEquals ("#11", "-Infinity", Single.NegativeInfinity.ToString("R"));
			AssertEquals ("#12", "NaN", Single.NaN.ToString("R"));
		}

		[Test]
		public void Test11032 ()
		{
			AssertEquals ("#01", "Infinity", (Double.MaxValue / 0.0).ToString ("E99", _nfi)); 
			AssertEquals ("#02", "-Infinity", (Double.MinValue / 0.0).ToString ("E99", _nfi)); 
			AssertEquals ("#03", "NaN", (0.0 / 0.0).ToString ("E99", _nfi)); 
		}

		// Test12000- Double and F
		[Test]
		public void Test12000 ()
		{
			AssertEquals ("#01", "0.00", 0.0.ToString ("F", _nfi));
			AssertEquals ("#02", "0.00", 0.0.ToString ("f", _nfi));
			AssertEquals ("#03", "-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00", Double.MinValue.ToString ("F", _nfi));
			AssertEquals ("#04", "-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00", Double.MinValue.ToString ("f", _nfi));
			AssertEquals ("#05", "179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00", Double.MaxValue.ToString ("F", _nfi));
			AssertEquals ("#06", "179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00", Double.MaxValue.ToString ("f", _nfi));
		}

		[Test]
		public void Test12001 ()
		{
			AssertEquals ("#01", "F ", 0.0.ToString ("F ", _nfi));
			AssertEquals ("#02", " F", 0.0.ToString (" F", _nfi));
			AssertEquals ("#03", " F ", 0.0.ToString (" F ", _nfi));
		}

		[Test]
		public void Test12002 ()
		{
			AssertEquals ("#01", "-F ", (-1.0).ToString ("F ", _nfi));
			AssertEquals ("#02", "- F", (-1.0).ToString (" F", _nfi));
			AssertEquals ("#03", "- F ", (-1.0).ToString (" F ", _nfi));
		}

		[Test]
		public void Test12003 ()
		{
			AssertEquals ("#01", "0", 0.0.ToString ("F0", _nfi));
			AssertEquals ("#02", "0.0000000000000000", 0.0.ToString ("F16", _nfi));
			AssertEquals ("#03", "0.00000000000000000", 0.0.ToString ("F17", _nfi));
			AssertEquals ("#04", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.0.ToString ("F99", _nfi));
			AssertEquals ("#05", "F100", 0.0.ToString ("F100", _nfi));
		}

		[Test]
		public void Test12004 ()
		{
			AssertEquals ("#01", "179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F0", _nfi));
			AssertEquals ("#02", "179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0000000000000000", Double.MaxValue.ToString ("F16", _nfi));
			AssertEquals ("#03", "179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00000000000000000", Double.MaxValue.ToString ("F17", _nfi));
			AssertEquals ("#04", "179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F99", _nfi));
			AssertEquals ("#05", "F1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F100", _nfi));
		}

		[Test]
		public void Test12005 ()
		{
			AssertEquals ("#01", "-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F0", _nfi));
			AssertEquals ("#02", "-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0000000000000000", Double.MinValue.ToString ("F16", _nfi));
			AssertEquals ("#03", "-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00000000000000000", Double.MinValue.ToString ("F17", _nfi));
			AssertEquals ("#04", "-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F99", _nfi));
			AssertEquals ("#05", "-F1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F100", _nfi));
		}

		[Test]
		public void Test12006 ()
		{
			AssertEquals ("#01", "FF", 0.0.ToString ("FF", _nfi));
			AssertEquals ("#02", "F0F", 0.0.ToString ("F0F", _nfi));
			AssertEquals ("#03", "F0xF", 0.0.ToString ("F0xF", _nfi));
		}

		[Test]
		public void Test12007 ()
		{
			AssertEquals ("#01", "FF", Double.MaxValue.ToString ("FF", _nfi));
			AssertEquals ("#02", "F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MaxValue.ToString ("F0F", _nfi));
			AssertEquals ("#03", "F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MaxValue.ToString ("F0xF", _nfi));
		}

		[Test]
		public void Test12008 ()
		{
			AssertEquals ("#01", "-FF", Double.MinValue.ToString ("FF", _nfi));
			AssertEquals ("#02", "-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MinValue.ToString ("F0F", _nfi));
			AssertEquals ("#03", "-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MinValue.ToString ("F0xF", _nfi));
		}

		[Test]
		public void Test12009 ()
		{
			AssertEquals ("#01", "0.00000000000000000", 0.0.ToString ("F0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#02", "179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00000000000000000", Double.MaxValue.ToString ("F0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#03", "-179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.00000000000000000", Double.MinValue.ToString ("F0000000000000000000000000000000000000017", _nfi));
		}

		[Test]
		public void Test12010 ()
		{
			AssertEquals ("#01", "+F", 0.0.ToString ("+F", _nfi));
			AssertEquals ("#02", "F+", 0.0.ToString ("F+", _nfi));
			AssertEquals ("#03", "+F+", 0.0.ToString ("+F+", _nfi));
		}
		
		[Test]
		public void Test12011 ()
		{
			AssertEquals ("#01", "+F", Double.MaxValue.ToString ("+F", _nfi));
			AssertEquals ("#02", "F+", Double.MaxValue.ToString ("F+", _nfi));
			AssertEquals ("#03", "+F+", Double.MaxValue.ToString ("+F+", _nfi));
		}

		[Test]
		public void Test12012 ()
		{
			AssertEquals ("#01", "-+F", Double.MinValue.ToString ("+F", _nfi));
			AssertEquals ("#02", "-F+", Double.MinValue.ToString ("F+", _nfi));
			AssertEquals ("#03", "-+F+", Double.MinValue.ToString ("+F+", _nfi));
		}

		[Test]
		public void Test12013 ()
		{
			AssertEquals ("#01", "-F", 0.0.ToString ("-F", _nfi));
			AssertEquals ("#02", "F-", 0.0.ToString ("F-", _nfi));
			AssertEquals ("#03", "-F-", 0.0.ToString ("-F-", _nfi));
		}
		
		[Test]
		public void Test12014 ()
		{
			AssertEquals ("#01", "-F", Double.MaxValue.ToString ("-F", _nfi));
			AssertEquals ("#02", "F-", Double.MaxValue.ToString ("F-", _nfi));
			AssertEquals ("#03", "-F-", Double.MaxValue.ToString ("-F-", _nfi));
		}

		[Test]
		public void Test12015 ()
		{
			AssertEquals ("#01", "--F", Double.MinValue.ToString ("-F", _nfi));
			AssertEquals ("#02", "-F-", Double.MinValue.ToString ("F-", _nfi));
			AssertEquals ("#03", "--F-", Double.MinValue.ToString ("-F-", _nfi));
		}

		[Test]
		public void Test12016 ()
		{
			AssertEquals ("#01", "F+0", 0.0.ToString ("F+0", _nfi));
			AssertEquals ("#02", "F+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F+0", _nfi));
			AssertEquals ("#03", "-F+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F+0", _nfi));
		}

		[Test]
		public void Test12017 ()
		{
			AssertEquals ("#01", "F+9", 0.0.ToString ("F+9", _nfi));
			AssertEquals ("#02", "F+9", Double.MaxValue.ToString ("F+9", _nfi));
			AssertEquals ("#03", "-F+9", Double.MinValue.ToString ("F+9", _nfi));
		}

		[Test]
		public void Test12018 ()
		{
			AssertEquals ("#01", "F-9", 0.0.ToString ("F-9", _nfi));
			AssertEquals ("#02", "F-9", Double.MaxValue.ToString ("F-9", _nfi));
			AssertEquals ("#03", "-F-9", Double.MinValue.ToString ("F-9", _nfi));
		}

		[Test]
		public void Test12019 ()
		{
			AssertEquals ("#01", "F0", 0.0.ToString ("F0,", _nfi));
			AssertEquals ("#02", "F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F0,", _nfi));
			AssertEquals ("#03", "-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F0,", _nfi));
		}

		[Test]
		public void Test12020 ()
		{
			AssertEquals ("#01", "F0", 0.0.ToString ("F0.", _nfi));
			AssertEquals ("#02", "F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("F0.", _nfi));
			AssertEquals ("#03", "-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("F0.", _nfi));
		}

		[Test]
		public void Test12021 ()
		{
			AssertEquals ("#01", "F0.0", 0.0.ToString ("F0.0", _nfi));
			AssertEquals ("#02", "F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MaxValue.ToString ("F0.0", _nfi));
			AssertEquals ("#03", "-F179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MinValue.ToString ("F0.0", _nfi));
		}

		[Test]
		public void Test12022 ()
		{
			AssertEquals ("#01", "F09", 0.0.ToString ("F0.9", _nfi));
			AssertEquals ("#02", "F1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MaxValue.ToString ("F0.9", _nfi));
			AssertEquals ("#03", "-F1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MinValue.ToString ("F0.9", _nfi));
		}

		[Test]
		public void Test12023 ()
		{
			AssertEquals ("#01", "1.1", 1.05.ToString ("F1", _nfi));
			AssertEquals ("#02", "1.2", 1.15.ToString ("F1", _nfi));
			AssertEquals ("#03", "1.3", 1.25.ToString ("F1", _nfi));
			AssertEquals ("#04", "1.4", 1.35.ToString ("F1", _nfi));
			AssertEquals ("#05", "1.5", 1.45.ToString ("F1", _nfi));
			AssertEquals ("#06", "1.6", 1.55.ToString ("F1", _nfi));
			AssertEquals ("#07", "1.7", 1.65.ToString ("F1", _nfi));
			AssertEquals ("#08", "1.8", 1.75.ToString ("F1", _nfi));
			AssertEquals ("#09", "1.9", 1.85.ToString ("F1", _nfi));
			AssertEquals ("#10", "2.0", 1.95.ToString ("F1", _nfi));
		}

		[Test]
		public void Test12024 ()
		{
			AssertEquals ("#01", "1.01", 1.005.ToString ("F2", _nfi));
			AssertEquals ("#02", "1.02", 1.015.ToString ("F2", _nfi));
			AssertEquals ("#03", "1.03", 1.025.ToString ("F2", _nfi));
			AssertEquals ("#04", "1.04", 1.035.ToString ("F2", _nfi));
			AssertEquals ("#05", "1.05", 1.045.ToString ("F2", _nfi));
			AssertEquals ("#06", "1.06", 1.055.ToString ("F2", _nfi));
			AssertEquals ("#07", "1.07", 1.065.ToString ("F2", _nfi));
			AssertEquals ("#08", "1.08", 1.075.ToString ("F2", _nfi));
			AssertEquals ("#09", "1.09", 1.085.ToString ("F2", _nfi));
			AssertEquals ("#10", "1.10", 1.095.ToString ("F2", _nfi));
		}

		[Test]
		public void Test12025 ()
		{
			AssertEquals ("#01", "1.00000000000001", 1.000000000000005.ToString ("F14", _nfi));
			AssertEquals ("#02", "1.00000000000002", 1.000000000000015.ToString ("F14", _nfi));
			AssertEquals ("#03", "1.00000000000003", 1.000000000000025.ToString ("F14", _nfi));
			AssertEquals ("#04", "1.00000000000004", 1.000000000000035.ToString ("F14", _nfi));
			AssertEquals ("#05", "1.00000000000005", 1.000000000000045.ToString ("F14", _nfi));
			AssertEquals ("#06", "1.00000000000006", 1.000000000000055.ToString ("F14", _nfi));
			AssertEquals ("#07", "1.00000000000007", 1.000000000000065.ToString ("F14", _nfi));
			AssertEquals ("#08", "1.00000000000008", 1.000000000000075.ToString ("F14", _nfi));
			AssertEquals ("#09", "1.00000000000009", 1.000000000000085.ToString ("F14", _nfi));
			AssertEquals ("#10", "1.00000000000010", 1.000000000000095.ToString ("F14", _nfi));
		}

		[Test]
		public void Test12026 ()
		{
			AssertEquals ("#01", "1.000000000000000", 1.0000000000000005.ToString ("F15", _nfi));
			AssertEquals ("#02", "1.000000000000000", 1.0000000000000015.ToString ("F15", _nfi));
			AssertEquals ("#03", "1.000000000000000", 1.0000000000000025.ToString ("F15", _nfi));
			AssertEquals ("#04", "1.000000000000000", 1.0000000000000035.ToString ("F15", _nfi));
			AssertEquals ("#05", "1.000000000000000", 1.0000000000000045.ToString ("F15", _nfi));
			AssertEquals ("#06", "1.000000000000010", 1.0000000000000055.ToString ("F15", _nfi));
			AssertEquals ("#07", "1.000000000000010", 1.0000000000000065.ToString ("F15", _nfi));
			AssertEquals ("#08", "1.000000000000010", 1.0000000000000075.ToString ("F15", _nfi));
			AssertEquals ("#09", "1.000000000000010", 1.0000000000000085.ToString ("F15", _nfi));
			AssertEquals ("#10", "1.000000000000010", 1.0000000000000095.ToString ("F15", _nfi));
		}

		[Test]
		public void Test12027 ()
		{
			AssertEquals ("#01", "1.0000000000000000", 1.00000000000000005.ToString ("F16", _nfi));
			AssertEquals ("#02", "1.0000000000000000", 1.00000000000000015.ToString ("F16", _nfi));
			AssertEquals ("#03", "1.0000000000000000", 1.00000000000000025.ToString ("F16", _nfi));
			AssertEquals ("#04", "1.0000000000000000", 1.00000000000000035.ToString ("F16", _nfi));
			AssertEquals ("#05", "1.0000000000000000", 1.00000000000000045.ToString ("F16", _nfi));
			AssertEquals ("#06", "1.0000000000000000", 1.00000000000000055.ToString ("F16", _nfi));
			AssertEquals ("#07", "1.0000000000000000", 1.00000000000000065.ToString ("F16", _nfi));
			AssertEquals ("#08", "1.0000000000000000", 1.00000000000000075.ToString ("F16", _nfi));
			AssertEquals ("#09", "1.0000000000000000", 1.00000000000000085.ToString ("F16", _nfi));
			AssertEquals ("#10", "1.0000000000000000", 1.00000000000000095.ToString ("F16", _nfi));
		}

		[Test]
		public void Test12028 ()
		{
			AssertEquals ("#01", "1", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F0", _nfi));
			AssertEquals ("#02", "1.234567890123", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F12", _nfi));
			AssertEquals ("#03", "1.2345678901235", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F13", _nfi));
			AssertEquals ("#04", "1.23456789012346", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F14", _nfi));
			AssertEquals ("#05", "1.234567890123460", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F15", _nfi));
			AssertEquals ("#06", "1.234567890123460000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F99", _nfi));
			AssertEquals ("#07", "F101", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("F100", _nfi));
		}

		[Test]
		public void Test12029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			AssertEquals ("#01", "-99999999#90", (-99999999.9).ToString ("F", nfi));
		}

		[Test]
		public void Test12030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			AssertEquals ("#01", "1.00", 1.0.ToString ("F", nfi));
			AssertEquals ("#02", "0.00", 0.0.ToString ("F", nfi));
			AssertEquals ("#03", "+1.00", (-1.0).ToString ("F", nfi));
		}

		[Test]
		public void Test12031 ()
		{
			AssertEquals ("#01", "Infinity", (Double.MaxValue / 0.0).ToString ("F99", _nfi)); 
			AssertEquals ("#02", "-Infinity", (Double.MinValue / 0.0).ToString ("F99", _nfi)); 
			AssertEquals ("#03", "NaN", (0.0 / 0.0).ToString ("F99", _nfi)); 
		}

		// Test13000- Double and G
		[Test]
		public void Test13000 ()
		{
			AssertEquals ("#01", "0", 0.0.ToString ("G", _nfi));
			AssertEquals ("#01.1", "0", (-0.0).ToString ("G", _nfi));
			AssertEquals ("#02", "0", 0.0.ToString ("g", _nfi));
			AssertEquals ("#03", "-1.79769313486232E+308", Double.MinValue.ToString ("G", _nfi));
			AssertEquals ("#04", "-1.79769313486232e+308", Double.MinValue.ToString ("g", _nfi));
			AssertEquals ("#05", "1.79769313486232E+308", Double.MaxValue.ToString ("G", _nfi));
			AssertEquals ("#06", "1.79769313486232e+308", Double.MaxValue.ToString ("g", _nfi));
		}

		[Test]
		public void Test13001 ()
		{
			AssertEquals ("#01", "G ", 0.0.ToString ("G ", _nfi));
			AssertEquals ("#02", " G", 0.0.ToString (" G", _nfi));
			AssertEquals ("#03", " G ", 0.0.ToString (" G ", _nfi));
		}

		[Test]
		public void Test13002 ()
		{
			AssertEquals ("#01", "-G ", (-1.0).ToString ("G ", _nfi));
			AssertEquals ("#02", "- G", (-1.0).ToString (" G", _nfi));
			AssertEquals ("#03", "- G ", (-1.0).ToString (" G ", _nfi));
		}

		[Test]
		public void Test13003 ()
		{
			AssertEquals ("#01", "0", 0.0.ToString ("G0", _nfi));
			AssertEquals ("#02", "0", 0.0.ToString ("G16", _nfi));
			AssertEquals ("#03", "0", 0.0.ToString ("G17", _nfi));
			AssertEquals ("#04", "0", 0.0.ToString ("G99", _nfi));
			AssertEquals ("#05", "G100", 0.0.ToString ("G100", _nfi));
		}

		[Test]
		public void Test13004 ()
		{
			AssertEquals ("#01", "1.79769313486232E+308", Double.MaxValue.ToString ("G0", _nfi));
			AssertEquals ("#02", "1.797693134862316E+308", Double.MaxValue.ToString ("G16", _nfi));
			AssertEquals ("#03", "1.7976931348623157E+308", Double.MaxValue.ToString ("G17", _nfi));
			AssertEquals ("#04", "1.7976931348623157E+308", Double.MaxValue.ToString ("G99", _nfi));
			AssertEquals ("#05", "G1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("G100", _nfi));
		}

		[Test]
		public void Test13005 ()
		{
			AssertEquals ("#01", "-1.79769313486232E+308", Double.MinValue.ToString ("G0", _nfi));
			AssertEquals ("#02", "-1.797693134862316E+308", Double.MinValue.ToString ("G16", _nfi));
			AssertEquals ("#03", "-1.7976931348623157E+308", Double.MinValue.ToString ("G17", _nfi));
			AssertEquals ("#04", "-1.7976931348623157E+308", Double.MinValue.ToString ("G99", _nfi));
			AssertEquals ("#05", "-G1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("G100", _nfi));
		}

		[Test]
		public void Test13006 ()
		{
			AssertEquals ("#01", "GF", 0.0.ToString ("GF", _nfi));
			AssertEquals ("#02", "G0F", 0.0.ToString ("G0F", _nfi));
			AssertEquals ("#03", "G0xF", 0.0.ToString ("G0xF", _nfi));
		}

		[Test]
		public void Test13007 ()
		{
			AssertEquals ("#01", "GF", Double.MaxValue.ToString ("GF", _nfi));
			AssertEquals ("#02", "G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MaxValue.ToString ("G0F", _nfi));
			AssertEquals ("#03", "G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MaxValue.ToString ("G0xF", _nfi));
		}

		[Test]
		public void Test13008 ()
		{
			AssertEquals ("#01", "-GF", Double.MinValue.ToString ("GF", _nfi));
			AssertEquals ("#02", "-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MinValue.ToString ("G0F", _nfi));
			AssertEquals ("#03", "-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MinValue.ToString ("G0xF", _nfi));
		}

		[Test]
		public void Test13009 ()
		{
			AssertEquals ("#01", "0", 0.0.ToString ("G0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#02", "1.7976931348623157E+308", Double.MaxValue.ToString ("G0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#03", "-1.7976931348623157E+308", Double.MinValue.ToString ("G0000000000000000000000000000000000000017", _nfi));
		}

		[Test]
		public void Test13010 ()
		{
			AssertEquals ("#01", "+G", 0.0.ToString ("+G", _nfi));
			AssertEquals ("#02", "G+", 0.0.ToString ("G+", _nfi));
			AssertEquals ("#03", "+G+", 0.0.ToString ("+G+", _nfi));
		}
		
		[Test]
		public void Test13011 ()
		{
			AssertEquals ("#01", "+G", Double.MaxValue.ToString ("+G", _nfi));
			AssertEquals ("#02", "G+", Double.MaxValue.ToString ("G+", _nfi));
			AssertEquals ("#03", "+G+", Double.MaxValue.ToString ("+G+", _nfi));
		}

		[Test]
		public void Test13012 ()
		{
			AssertEquals ("#01", "-+G", Double.MinValue.ToString ("+G", _nfi));
			AssertEquals ("#02", "-G+", Double.MinValue.ToString ("G+", _nfi));
			AssertEquals ("#03", "-+G+", Double.MinValue.ToString ("+G+", _nfi));
		}

		[Test]
		public void Test13013 ()
		{
			AssertEquals ("#01", "-G", 0.0.ToString ("-G", _nfi));
			AssertEquals ("#02", "G-", 0.0.ToString ("G-", _nfi));
			AssertEquals ("#03", "-G-", 0.0.ToString ("-G-", _nfi));
		}
		
		[Test]
		public void Test13014 ()
		{
			AssertEquals ("#01", "-G", Double.MaxValue.ToString ("-G", _nfi));
			AssertEquals ("#02", "G-", Double.MaxValue.ToString ("G-", _nfi));
			AssertEquals ("#03", "-G-", Double.MaxValue.ToString ("-G-", _nfi));
		}

		[Test]
		public void Test13015 ()
		{
			AssertEquals ("#01", "--G", Double.MinValue.ToString ("-G", _nfi));
			AssertEquals ("#02", "-G-", Double.MinValue.ToString ("G-", _nfi));
			AssertEquals ("#03", "--G-", Double.MinValue.ToString ("-G-", _nfi));
		}

		[Test]
		public void Test13016 ()
		{
			AssertEquals ("#01", "G+0", 0.0.ToString ("G+0", _nfi));
			AssertEquals ("#02", "G+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("G+0", _nfi));
			AssertEquals ("#03", "-G+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("G+0", _nfi));
		}

		[Test]
		public void Test13017 ()
		{
			AssertEquals ("#01", "G+9", 0.0.ToString ("G+9", _nfi));
			AssertEquals ("#02", "G+9", Double.MaxValue.ToString ("G+9", _nfi));
			AssertEquals ("#03", "-G+9", Double.MinValue.ToString ("G+9", _nfi));
		}

		[Test]
		public void Test13018 ()
		{
			AssertEquals ("#01", "G-9", 0.0.ToString ("G-9", _nfi));
			AssertEquals ("#02", "G-9", Double.MaxValue.ToString ("G-9", _nfi));
			AssertEquals ("#03", "-G-9", Double.MinValue.ToString ("G-9", _nfi));
		}

		[Test]
		public void Test13019 ()
		{
			AssertEquals ("#01", "G0", 0.0.ToString ("G0,", _nfi));
			AssertEquals ("#02", "G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("G0,", _nfi));
			AssertEquals ("#03", "-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("G0,", _nfi));
		}

		[Test]
		public void Test13020 ()
		{
			AssertEquals ("#01", "G0", 0.0.ToString ("G0.", _nfi));
			AssertEquals ("#02", "G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("G0.", _nfi));
			AssertEquals ("#03", "-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("G0.", _nfi));
		}

		[Test]
		public void Test13021 ()
		{
			AssertEquals ("#01", "G0.0", 0.0.ToString ("G0.0", _nfi));
			AssertEquals ("#02", "G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MaxValue.ToString ("G0.0", _nfi));
			AssertEquals ("#03", "-G179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MinValue.ToString ("G0.0", _nfi));
		}

		[Test]
		public void Test13022 ()
		{
			AssertEquals ("#01", "G09", 0.0.ToString ("G0.9", _nfi));
			AssertEquals ("#02", "G1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MaxValue.ToString ("G0.9", _nfi));
			AssertEquals ("#03", "-G1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MinValue.ToString ("G0.9", _nfi));
		}

		[Test]
		public void Test13023 ()
		{
			AssertEquals ("#01", "0.5", 0.5.ToString ("G1", _nfi));
			AssertEquals ("#02", "2", 1.5.ToString ("G1", _nfi));
			AssertEquals ("#03", "3", 2.5.ToString ("G1", _nfi));
			AssertEquals ("#04", "4", 3.5.ToString ("G1", _nfi));
			AssertEquals ("#05", "5", 4.5.ToString ("G1", _nfi));
			AssertEquals ("#06", "6", 5.5.ToString ("G1", _nfi));
			AssertEquals ("#07", "7", 6.5.ToString ("G1", _nfi));
			AssertEquals ("#08", "8", 7.5.ToString ("G1", _nfi));
			AssertEquals ("#09", "9", 8.5.ToString ("G1", _nfi));
			AssertEquals ("#10", "1E+01", 9.5.ToString ("G1", _nfi));
		}

		[Test]
		public void Test13024_CarryPropagation () 
		{
			Double d = 1.15;
			AssertEquals ("#01", "1", d.ToString ("G1", _nfi));
			// NumberStore converts 1.15 into 1.14999...91 (1 in index 17)
			// so the call to NumberToString doesn't result in 1.2 but in 1.1
			// which seems "somewhat" normal considering the #17 results,
			AssertEquals ("#02", "1.2", d.ToString ("G2", _nfi));
			AssertEquals ("#03", "1.15", d.ToString ("G3", _nfi));
			AssertEquals ("#04", "1.15", d.ToString ("G4", _nfi));
			AssertEquals ("#05", "1.15", d.ToString ("G5", _nfi));
			AssertEquals ("#06", "1.15", d.ToString ("G6", _nfi));
			AssertEquals ("#07", "1.15", d.ToString ("G7", _nfi));
			AssertEquals ("#08", "1.15", d.ToString ("G8", _nfi));
			AssertEquals ("#09", "1.15", d.ToString ("G9", _nfi));
			AssertEquals ("#10", "1.15", d.ToString ("G10", _nfi));
			AssertEquals ("#11", "1.15", d.ToString ("G11", _nfi));
			AssertEquals ("#12", "1.15", d.ToString ("G12", _nfi));
			AssertEquals ("#13", "1.15", d.ToString ("G13", _nfi));
			AssertEquals ("#14", "1.15", d.ToString ("G14", _nfi));
			AssertEquals ("#15", "1.15", d.ToString ("G15", _nfi));
			AssertEquals ("#16", "1.15", d.ToString ("G16", _nfi));
			AssertEquals ("#17", "1.1499999999999999", d.ToString ("G17", _nfi));
		}

		[Test]
		public void Test13024 ()
		{
			AssertEquals ("#01", "1.1", 1.05.ToString ("G2", _nfi));
			AssertEquals ("#02", "1.2", 1.15.ToString ("G2", _nfi));
			AssertEquals ("#03", "1.3", 1.25.ToString ("G2", _nfi));
			AssertEquals ("#04", "1.4", 1.35.ToString ("G2", _nfi));
			AssertEquals ("#05", "1.5", 1.45.ToString ("G2", _nfi));
			AssertEquals ("#06", "1.6", 1.55.ToString ("G2", _nfi));
			AssertEquals ("#07", "1.7", 1.65.ToString ("G2", _nfi));
			AssertEquals ("#08", "1.8", 1.75.ToString ("G2", _nfi));
			AssertEquals ("#09", "1.9", 1.85.ToString ("G2", _nfi));
			AssertEquals ("#10", "2", 1.95.ToString ("G2", _nfi));
		}

		[Test]
		public void Test13025 ()
		{
			AssertEquals ("#01", "10", 10.05.ToString ("G2", _nfi));
			AssertEquals ("#02", "10", 10.15.ToString ("G2", _nfi));
			AssertEquals ("#03", "10", 10.25.ToString ("G2", _nfi));
			AssertEquals ("#04", "10", 10.35.ToString ("G2", _nfi));
			AssertEquals ("#05", "10", 10.45.ToString ("G2", _nfi));
			AssertEquals ("#06", "11", 10.55.ToString ("G2", _nfi));
			AssertEquals ("#07", "11", 10.65.ToString ("G2", _nfi));
			AssertEquals ("#08", "11", 10.75.ToString ("G2", _nfi));
			AssertEquals ("#09", "11", 10.85.ToString ("G2", _nfi));
			AssertEquals ("#10", "11", 10.95.ToString ("G2", _nfi));
		}

		[Test]
		public void Test13026 ()
		{
			AssertEquals ("#01", "1.00000000000001", 1.000000000000005.ToString ("G15", _nfi));
			AssertEquals ("#02", "1.00000000000002", 1.000000000000015.ToString ("G15", _nfi));
			AssertEquals ("#03", "1.00000000000003", 1.000000000000025.ToString ("G15", _nfi));
			AssertEquals ("#04", "1.00000000000004", 1.000000000000035.ToString ("G15", _nfi));
			AssertEquals ("#05", "1.00000000000005", 1.000000000000045.ToString ("G15", _nfi));
			AssertEquals ("#06", "1.00000000000006", 1.000000000000055.ToString ("G15", _nfi));
			AssertEquals ("#07", "1.00000000000007", 1.000000000000065.ToString ("G15", _nfi));
			AssertEquals ("#08", "1.00000000000008", 1.000000000000075.ToString ("G15", _nfi));
			AssertEquals ("#09", "1.00000000000009", 1.000000000000085.ToString ("G15", _nfi));
			AssertEquals ("#10", "1.0000000000001", 1.000000000000095.ToString ("G15", _nfi));
		}

		[Test]
		public void Test13027 ()
		{
			AssertEquals ("#01", "1", 1.0000000000000005.ToString ("G16", _nfi));
			AssertEquals ("#02", "1.000000000000002", 1.0000000000000015.ToString ("G16", _nfi));
			AssertEquals ("#03", "1.000000000000002", 1.0000000000000025.ToString ("G16", _nfi));
			AssertEquals ("#04", "1.000000000000004", 1.0000000000000035.ToString ("G16", _nfi));
			AssertEquals ("#05", "1.000000000000004", 1.0000000000000045.ToString ("G16", _nfi));
			AssertEquals ("#06", "1.000000000000006", 1.0000000000000055.ToString ("G16", _nfi));
			AssertEquals ("#07", "1.000000000000006", 1.0000000000000065.ToString ("G16", _nfi));
			AssertEquals ("#08", "1.000000000000008", 1.0000000000000075.ToString ("G16", _nfi));
			AssertEquals ("#09", "1.000000000000008", 1.0000000000000085.ToString ("G16", _nfi));
			AssertEquals ("#10", "1.00000000000001", 1.0000000000000095.ToString ("G16", _nfi));
		}

		[Test]
		public void Test13028 ()
		{
			AssertEquals ("#01", "1", 1.00000000000000005.ToString ("G17", _nfi));
			AssertEquals ("#02", "1.0000000000000002", 1.00000000000000015.ToString ("G17", _nfi));
			AssertEquals ("#03", "1.0000000000000002", 1.00000000000000025.ToString ("G17", _nfi));
			AssertEquals ("#04", "1.0000000000000004", 1.00000000000000035.ToString ("G17", _nfi));
			AssertEquals ("#05", "1.0000000000000004", 1.00000000000000045.ToString ("G17", _nfi));
			AssertEquals ("#06", "1.0000000000000004", 1.00000000000000055.ToString ("G17", _nfi));
			AssertEquals ("#07", "1.0000000000000007", 1.00000000000000065.ToString ("G17", _nfi));
			AssertEquals ("#08", "1.0000000000000007", 1.00000000000000075.ToString ("G17", _nfi));
			AssertEquals ("#09", "1.0000000000000009", 1.00000000000000085.ToString ("G17", _nfi));
			AssertEquals ("#10", "1.0000000000000009", 1.00000000000000095.ToString ("G17", _nfi));
		}
		
		[Test]
		public void Test13029 ()
		{
			AssertEquals ("#01", "1", 1.000000000000000005.ToString ("G18", _nfi));
			AssertEquals ("#02", "1", 1.000000000000000015.ToString ("G18", _nfi));
			AssertEquals ("#03", "1", 1.000000000000000025.ToString ("G18", _nfi));
			AssertEquals ("#04", "1", 1.000000000000000035.ToString ("G18", _nfi));
			AssertEquals ("#05", "1", 1.000000000000000045.ToString ("G18", _nfi));
			AssertEquals ("#06", "1", 1.000000000000000055.ToString ("G18", _nfi));
			AssertEquals ("#07", "1", 1.000000000000000065.ToString ("G18", _nfi));
			AssertEquals ("#08", "1", 1.000000000000000075.ToString ("G18", _nfi));
			AssertEquals ("#09", "1", 1.000000000000000085.ToString ("G18", _nfi));
			AssertEquals ("#10", "1", 1.000000000000000095.ToString ("G18", _nfi));
		}

		[Test]
		public void Test13030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			AssertEquals ("#01", "-99999999#9", (-99999999.9).ToString ("G", nfi));
		}

		[Test]
		public void Test13031 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			AssertEquals ("#01", "1", 1.0.ToString ("G", nfi));
			AssertEquals ("#02", "0", 0.0.ToString ("G", nfi));
			AssertEquals ("#03", "+1", (-1.0).ToString ("G", nfi));
		}

		[Test]
		public void Test13032 ()
		{
			AssertEquals ("#01", "Infinity", (Double.MaxValue / 0.0).ToString ("G99", _nfi)); 
			AssertEquals ("#02", "-Infinity", (Double.MinValue / 0.0).ToString ("G99", _nfi)); 
			AssertEquals ("#03", "NaN", (0.0 / 0.0).ToString ("G99", _nfi)); 
		}

		[Test]
		public void Test13033 ()
		{
			AssertEquals ("#01", "0.0001", 0.0001.ToString ("G", _nfi));
			AssertEquals ("#02", "1E-05", 0.00001.ToString ("G", _nfi));
			AssertEquals ("#03", "0.0001", 0.0001.ToString ("G0", _nfi));
			AssertEquals ("#04", "1E-05", 0.00001.ToString ("G0", _nfi));
			AssertEquals ("#05", "100000000000000", 100000000000000.0.ToString ("G", _nfi));
			AssertEquals ("#06", "1E+15", 1000000000000000.0.ToString ("G", _nfi));
			AssertEquals ("#07", "1000000000000000", 1000000000000000.0.ToString ("G16", _nfi));
		}

		// Test14000- Double and N
		[Test]
		public void Test14000 ()
		{
			AssertEquals ("#01", "0.00", 0.0.ToString ("N", _nfi));
			AssertEquals ("#02", "0.00", 0.0.ToString ("n", _nfi));
			AssertEquals ("#03", "-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00", Double.MinValue.ToString ("N", _nfi));
			AssertEquals ("#04", "-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00", Double.MinValue.ToString ("n", _nfi));
			AssertEquals ("#05", "179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00", Double.MaxValue.ToString ("N", _nfi));
			AssertEquals ("#06", "179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00", Double.MaxValue.ToString ("n", _nfi));
		}

		[Test]
		public void Test14001 ()
		{
			AssertEquals ("#01", "N ", 0.0.ToString ("N ", _nfi));
			AssertEquals ("#02", " N", 0.0.ToString (" N", _nfi));
			AssertEquals ("#03", " N ", 0.0.ToString (" N ", _nfi));
		}

		[Test]
		public void Test14002 ()
		{
			AssertEquals ("#01", "-N ", (-1.0).ToString ("N ", _nfi));
			AssertEquals ("#02", "- N", (-1.0).ToString (" N", _nfi));
			AssertEquals ("#03", "- N ", (-1.0).ToString (" N ", _nfi));
		}

		[Test]
		public void Test14003 ()
		{
			AssertEquals ("#01", "0", 0.0.ToString ("N0", _nfi));
			AssertEquals ("#02", "0.0000000000000000", 0.0.ToString ("N16", _nfi));
			AssertEquals ("#03", "0.00000000000000000", 0.0.ToString ("N17", _nfi));
			AssertEquals ("#04", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 0.0.ToString ("N99", _nfi));
			AssertEquals ("#05", "N100", 0.0.ToString ("N100", _nfi));
		}

		[Test]
		public void Test14004 ()
		{
			AssertEquals ("#01", "179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000", Double.MaxValue.ToString ("N0", _nfi));
			AssertEquals ("#02", "179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.0000000000000000", Double.MaxValue.ToString ("N16", _nfi));
			AssertEquals ("#03", "179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000", Double.MaxValue.ToString ("N17", _nfi));
			AssertEquals ("#04", "179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N99", _nfi));
			AssertEquals ("#05", "N1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N100", _nfi));
		}

		[Test]
		public void Test14005 ()
		{
			AssertEquals ("#01", "-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000", Double.MinValue.ToString ("N0", _nfi));
			AssertEquals ("#02", "-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.0000000000000000", Double.MinValue.ToString ("N16", _nfi));
			AssertEquals ("#03", "-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000", Double.MinValue.ToString ("N17", _nfi));
			AssertEquals ("#04", "-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N99", _nfi));
			AssertEquals ("#05", "-N1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N100", _nfi));
		}

		[Test]
		public void Test14006 ()
		{
			AssertEquals ("#01", "NF", 0.0.ToString ("NF", _nfi));
			AssertEquals ("#02", "N0F", 0.0.ToString ("N0F", _nfi));
			AssertEquals ("#03", "N0xF", 0.0.ToString ("N0xF", _nfi));
		}

		[Test]
		public void Test14007 ()
		{
			AssertEquals ("#01", "NF", Double.MaxValue.ToString ("NF", _nfi));
			AssertEquals ("#02", "N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MaxValue.ToString ("N0F", _nfi));
			AssertEquals ("#03", "N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MaxValue.ToString ("N0xF", _nfi));
		}

		[Test]
		public void Test14008 ()
		{
			AssertEquals ("#01", "-NF", Double.MinValue.ToString ("NF", _nfi));
			AssertEquals ("#02", "-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MinValue.ToString ("N0F", _nfi));
			AssertEquals ("#03", "-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MinValue.ToString ("N0xF", _nfi));
		}

		[Test]
		public void Test14009 ()
		{
			AssertEquals ("#01", "0.00000000000000000", 0.0.ToString ("N0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#02", "179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000", Double.MaxValue.ToString ("N0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#03", "-179,769,313,486,232,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000", Double.MinValue.ToString ("N0000000000000000000000000000000000000017", _nfi));
		}

		[Test]
		public void Test14010 ()
		{
			AssertEquals ("#01", "+N", 0.0.ToString ("+N", _nfi));
			AssertEquals ("#02", "N+", 0.0.ToString ("N+", _nfi));
			AssertEquals ("#03", "+N+", 0.0.ToString ("+N+", _nfi));
		}
		
		[Test]
		public void Test14011 ()
		{
			AssertEquals ("#01", "+N", Double.MaxValue.ToString ("+N", _nfi));
			AssertEquals ("#02", "N+", Double.MaxValue.ToString ("N+", _nfi));
			AssertEquals ("#03", "+N+", Double.MaxValue.ToString ("+N+", _nfi));
		}

		[Test]
		public void Test14012 ()
		{
			AssertEquals ("#01", "-+N", Double.MinValue.ToString ("+N", _nfi));
			AssertEquals ("#02", "-N+", Double.MinValue.ToString ("N+", _nfi));
			AssertEquals ("#03", "-+N+", Double.MinValue.ToString ("+N+", _nfi));
		}

		[Test]
		public void Test14013 ()
		{
			AssertEquals ("#01", "-N", 0.0.ToString ("-N", _nfi));
			AssertEquals ("#02", "N-", 0.0.ToString ("N-", _nfi));
			AssertEquals ("#03", "-N-", 0.0.ToString ("-N-", _nfi));
		}
		
		[Test]
		public void Test14014 ()
		{
			AssertEquals ("#01", "-N", Double.MaxValue.ToString ("-N", _nfi));
			AssertEquals ("#02", "N-", Double.MaxValue.ToString ("N-", _nfi));
			AssertEquals ("#03", "-N-", Double.MaxValue.ToString ("-N-", _nfi));
		}

		[Test]
		public void Test14015 ()
		{
			AssertEquals ("#01", "--N", Double.MinValue.ToString ("-N", _nfi));
			AssertEquals ("#02", "-N-", Double.MinValue.ToString ("N-", _nfi));
			AssertEquals ("#03", "--N-", Double.MinValue.ToString ("-N-", _nfi));
		}

		[Test]
		public void Test14016 ()
		{
			AssertEquals ("#01", "N+0", 0.0.ToString ("N+0", _nfi));
			AssertEquals ("#02", "N+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N+0", _nfi));
			AssertEquals ("#03", "-N+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N+0", _nfi));
		}

		[Test]
		public void Test14017 ()
		{
			AssertEquals ("#01", "N+9", 0.0.ToString ("N+9", _nfi));
			AssertEquals ("#02", "N+9", Double.MaxValue.ToString ("N+9", _nfi));
			AssertEquals ("#03", "-N+9", Double.MinValue.ToString ("N+9", _nfi));
		}

		[Test]
		public void Test14018 ()
		{
			AssertEquals ("#01", "N-9", 0.0.ToString ("N-9", _nfi));
			AssertEquals ("#02", "N-9", Double.MaxValue.ToString ("N-9", _nfi));
			AssertEquals ("#03", "-N-9", Double.MinValue.ToString ("N-9", _nfi));
		}

		[Test]
		public void Test14019 ()
		{
			AssertEquals ("#01", "N0", 0.0.ToString ("N0,", _nfi));
			AssertEquals ("#02", "N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N0,", _nfi));
			AssertEquals ("#03", "-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N0,", _nfi));
		}

		[Test]
		public void Test14020 ()
		{
			AssertEquals ("#01", "N0", 0.0.ToString ("N0.", _nfi));
			AssertEquals ("#02", "N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("N0.", _nfi));
			AssertEquals ("#03", "-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("N0.", _nfi));
		}

		[Test]
		public void Test14021 ()
		{
			AssertEquals ("#01", "N0.0", 0.0.ToString ("N0.0", _nfi));
			AssertEquals ("#02", "N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MaxValue.ToString ("N0.0", _nfi));
			AssertEquals ("#03", "-N179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MinValue.ToString ("N0.0", _nfi));
		}

		[Test]
		public void Test14022 ()
		{
			AssertEquals ("#01", "N09", 0.0.ToString ("N0.9", _nfi));
			AssertEquals ("#02", "N1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MaxValue.ToString ("N0.9", _nfi));
			AssertEquals ("#03", "-N1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MinValue.ToString ("N0.9", _nfi));
		}

		[Test]
		public void Test14023 ()
		{
			AssertEquals ("#01", "999.1", 999.05.ToString ("N1", _nfi));
			AssertEquals ("#02", "999.2", 999.15.ToString ("N1", _nfi));
			AssertEquals ("#03", "999.3", 999.25.ToString ("N1", _nfi));
			AssertEquals ("#04", "999.4", 999.35.ToString ("N1", _nfi));
			AssertEquals ("#05", "999.5", 999.45.ToString ("N1", _nfi));
			AssertEquals ("#06", "999.6", 999.55.ToString ("N1", _nfi));
			AssertEquals ("#07", "999.7", 999.65.ToString ("N1", _nfi));
			AssertEquals ("#08", "999.8", 999.75.ToString ("N1", _nfi));
			AssertEquals ("#09", "999.9", 999.85.ToString ("N1", _nfi));
			AssertEquals ("#10", "1,000.0", 999.95.ToString ("N1", _nfi));
		}

		[Test]
		public void Test14024 ()
		{
			AssertEquals ("#01", "999.91", 999.905.ToString ("N2", _nfi));
			AssertEquals ("#02", "999.92", 999.915.ToString ("N2", _nfi));
			AssertEquals ("#03", "999.93", 999.925.ToString ("N2", _nfi));
			AssertEquals ("#04", "999.94", 999.935.ToString ("N2", _nfi));
			AssertEquals ("#05", "999.95", 999.945.ToString ("N2", _nfi));
			AssertEquals ("#06", "999.96", 999.955.ToString ("N2", _nfi));
			AssertEquals ("#07", "999.97", 999.965.ToString ("N2", _nfi));
			AssertEquals ("#08", "999.98", 999.975.ToString ("N2", _nfi));
			AssertEquals ("#09", "999.99", 999.985.ToString ("N2", _nfi));
			AssertEquals ("#10", "1,000.00", 999.995.ToString ("N2", _nfi));
		}

		[Test]
		public void Test14025 ()
		{
			AssertEquals ("#01", "999.99999999991", 999.999999999905.ToString ("N11", _nfi));
			AssertEquals ("#02", "999.99999999992", 999.999999999915.ToString ("N11", _nfi));
			AssertEquals ("#03", "999.99999999993", 999.999999999925.ToString ("N11", _nfi));
			AssertEquals ("#04", "999.99999999994", 999.999999999935.ToString ("N11", _nfi));
			AssertEquals ("#05", "999.99999999995", 999.999999999945.ToString ("N11", _nfi));
			AssertEquals ("#06", "999.99999999996", 999.999999999955.ToString ("N11", _nfi));
			AssertEquals ("#07", "999.99999999997", 999.999999999965.ToString ("N11", _nfi));
			AssertEquals ("#08", "999.99999999998", 999.999999999975.ToString ("N11", _nfi));
			AssertEquals ("#09", "999.99999999999", 999.999999999985.ToString ("N11", _nfi));
			AssertEquals ("#10", "1,000.00000000000", 999.999999999995.ToString ("N11", _nfi));
		}

		[Test]
		public void Test14026 ()
		{
			AssertEquals ("#01", "999.999999999990", 999.9999999999905.ToString ("N12", _nfi));
			AssertEquals ("#02", "999.999999999991", 999.9999999999915.ToString ("N12", _nfi));
			AssertEquals ("#03", "999.999999999993", 999.9999999999925.ToString ("N12", _nfi));
			AssertEquals ("#04", "999.999999999994", 999.9999999999935.ToString ("N12", _nfi));
			AssertEquals ("#05", "999.999999999995", 999.9999999999945.ToString ("N12", _nfi));
			AssertEquals ("#06", "999.999999999995", 999.9999999999955.ToString ("N12", _nfi));
			AssertEquals ("#07", "999.999999999996", 999.9999999999965.ToString ("N12", _nfi));
			AssertEquals ("#08", "999.999999999998", 999.9999999999975.ToString ("N12", _nfi));
			AssertEquals ("#09", "999.999999999999", 999.9999999999985.ToString ("N12", _nfi));
			AssertEquals ("#10", "1,000.000000000000", 999.9999999999995.ToString ("N12", _nfi));
		}

		[Test]
		public void Test14027 ()
		{
			AssertEquals ("#01", "999.9999999999990", 999.99999999999905.ToString ("N13", _nfi));
			AssertEquals ("#02", "999.9999999999990", 999.99999999999915.ToString ("N13", _nfi));
			AssertEquals ("#03", "999.9999999999990", 999.99999999999925.ToString ("N13", _nfi));
			AssertEquals ("#04", "999.9999999999990", 999.99999999999935.ToString ("N13", _nfi));
			AssertEquals ("#05", "999.9999999999990", 999.99999999999945.ToString ("N13", _nfi));
			AssertEquals ("#06", "1,000.0000000000000", 999.99999999999955.ToString ("N13", _nfi));
			AssertEquals ("#07", "1,000.0000000000000", 999.99999999999965.ToString ("N13", _nfi));
			AssertEquals ("#08", "1,000.0000000000000", 999.99999999999975.ToString ("N13", _nfi));
			AssertEquals ("#09", "1,000.0000000000000", 999.99999999999985.ToString ("N13", _nfi));
			AssertEquals ("#10", "1,000.0000000000000", 999.99999999999995.ToString ("N13", _nfi));
		}

		[Test]
		public void Test14028 ()
		{
			AssertEquals ("#01", "1", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N0", _nfi));
			AssertEquals ("#02", "1.234567890123", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N12", _nfi));
			AssertEquals ("#03", "1.2345678901235", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N13", _nfi));
			AssertEquals ("#04", "1.23456789012346", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N14", _nfi));
			AssertEquals ("#05", "1.234567890123460", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N15", _nfi));
			AssertEquals ("#06", "1.234567890123460000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N99", _nfi));
			AssertEquals ("#07", "N101", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N100", _nfi));
		}

		[Test]
		public void Test14029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NumberDecimalSeparator = "#";
			AssertEquals ("#01", "-99,999,999#90", (-99999999.9).ToString ("N", nfi));
		}

		[Test]
		public void Test14030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			AssertEquals ("#01", "1,000.00", 1000.0.ToString ("N", nfi));
			AssertEquals ("#02", "0.00", 0.0.ToString ("N", nfi));
			AssertEquals ("#03", "+1,000.00", (-1000.0).ToString ("N", nfi));
		}

		[Test]
		public void Test14031 ()
		{
			AssertEquals ("#01", "Infinity", (Double.MaxValue / 0.0).ToString ("N99", _nfi)); 
			AssertEquals ("#02", "-Infinity", (Double.MinValue / 0.0).ToString ("N99", _nfi)); 
			AssertEquals ("#03", "NaN", (0.0 / 0.0).ToString ("N99", _nfi)); 
		}

		// Test15000- Double and P
		[Test]
		public void Test15000 ()
		{
			AssertEquals ("#01", "0.00 %", 0.0.ToString ("P", _nfi));
			AssertEquals ("#02", "0.00 %", 0.0.ToString ("p", _nfi));
			AssertEquals ("#03", "-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00 %", Double.MinValue.ToString ("P", _nfi));
			AssertEquals ("#04", "-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00 %", Double.MinValue.ToString ("p", _nfi));
			AssertEquals ("#05", "17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00 %", Double.MaxValue.ToString ("P", _nfi));
			AssertEquals ("#06", "17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00 %", Double.MaxValue.ToString ("p", _nfi));
		}

		[Test]
		public void Test15001 ()
		{
			AssertEquals ("#01", "P ", 0.0.ToString ("P ", _nfi));
			AssertEquals ("#02", " P", 0.0.ToString (" P", _nfi));
			AssertEquals ("#03", " P ", 0.0.ToString (" P ", _nfi));
		}

		[Test]
		public void Test15002 ()
		{
			AssertEquals ("#01", "-P ", (-1.0).ToString ("P ", _nfi));
			AssertEquals ("#02", "- P", (-1.0).ToString (" P", _nfi));
			AssertEquals ("#03", "- P ", (-1.0).ToString (" P ", _nfi));
		}

		[Test]
		public void Test15003 ()
		{
			AssertEquals ("#01", "0 %", 0.0.ToString ("P0", _nfi));
			AssertEquals ("#02", "0.0000000000000000 %", 0.0.ToString ("P16", _nfi));
			AssertEquals ("#03", "0.00000000000000000 %", 0.0.ToString ("P17", _nfi));
			AssertEquals ("#04", "0.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", 0.0.ToString ("P99", _nfi));
			AssertEquals ("#05", "P100", 0.0.ToString ("P100", _nfi));
		}

		[Test]
		public void Test15004 ()
		{
			AssertEquals ("#01", "17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000 %", Double.MaxValue.ToString ("P0", _nfi));
			AssertEquals ("#02", "17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.0000000000000000 %", Double.MaxValue.ToString ("P16", _nfi));
			AssertEquals ("#03", "17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000 %", Double.MaxValue.ToString ("P17", _nfi));
			AssertEquals ("#04", "17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", Double.MaxValue.ToString ("P99", _nfi));
			AssertEquals ("#05", "P1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P100", _nfi));
		}

		[Test]
		public void Test15005 ()
		{
			AssertEquals ("#01", "-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000 %", Double.MinValue.ToString ("P0", _nfi));
			AssertEquals ("#02", "-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.0000000000000000 %", Double.MinValue.ToString ("P16", _nfi));
			AssertEquals ("#03", "-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000 %", Double.MinValue.ToString ("P17", _nfi));
			AssertEquals ("#04", "-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 %", Double.MinValue.ToString ("P99", _nfi));
			AssertEquals ("#05", "P1179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P100", _nfi));
		}

		[Test]
		public void Test15006 ()
		{
			AssertEquals ("#01", "PF", 0.0.ToString ("PF", _nfi));
			AssertEquals ("#02", "P0F", 0.0.ToString ("P0F", _nfi));
			AssertEquals ("#03", "P0xF", 0.0.ToString ("P0xF", _nfi));
		}

		[Test]
		public void Test15007 ()
		{
			AssertEquals ("#01", "PF", Double.MaxValue.ToString ("PF", _nfi));
			AssertEquals ("#02", "P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MaxValue.ToString ("P0F", _nfi));
			AssertEquals ("#03", "P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MaxValue.ToString ("P0xF", _nfi));
		}

		[Test]
		public void Test15008 ()
		{
			AssertEquals ("#01", "-PF", Double.MinValue.ToString ("PF", _nfi));
			AssertEquals ("#02", "-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000F", Double.MinValue.ToString ("P0F", _nfi));
			AssertEquals ("#03", "-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000xF", Double.MinValue.ToString ("P0xF", _nfi));
		}

		[Test]
		public void Test15009 ()
		{
			AssertEquals ("#01", "0.00000000000000000 %", 0.0.ToString ("P0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#02", "17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000 %", Double.MaxValue.ToString ("P0000000000000000000000000000000000000017", _nfi));
			AssertEquals ("#03", "-17,976,931,348,623,200,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000,000.00000000000000000 %", Double.MinValue.ToString ("P0000000000000000000000000000000000000017", _nfi));
		}

		[Test]
		public void Test15010 ()
		{
			AssertEquals ("#01", "+P", 0.0.ToString ("+P", _nfi));
			AssertEquals ("#02", "P+", 0.0.ToString ("P+", _nfi));
			AssertEquals ("#03", "+P+", 0.0.ToString ("+P+", _nfi));
		}
		
		[Test]
		public void Test15011 ()
		{
			AssertEquals ("#01", "+P", Double.MaxValue.ToString ("+P", _nfi));
			AssertEquals ("#02", "P+", Double.MaxValue.ToString ("P+", _nfi));
			AssertEquals ("#03", "+P+", Double.MaxValue.ToString ("+P+", _nfi));
		}

		[Test]
		public void Test15012 ()
		{
			AssertEquals ("#01", "-+P", Double.MinValue.ToString ("+P", _nfi));
			AssertEquals ("#02", "-P+", Double.MinValue.ToString ("P+", _nfi));
			AssertEquals ("#03", "-+P+", Double.MinValue.ToString ("+P+", _nfi));
		}

		[Test]
		public void Test15013 ()
		{
			AssertEquals ("#01", "-P", 0.0.ToString ("-P", _nfi));
			AssertEquals ("#02", "P-", 0.0.ToString ("P-", _nfi));
			AssertEquals ("#03", "-P-", 0.0.ToString ("-P-", _nfi));
		}
		
		[Test]
		public void Test15014 ()
		{
			AssertEquals ("#01", "-P", Double.MaxValue.ToString ("-P", _nfi));
			AssertEquals ("#02", "P-", Double.MaxValue.ToString ("P-", _nfi));
			AssertEquals ("#03", "-P-", Double.MaxValue.ToString ("-P-", _nfi));
		}

		[Test]
		public void Test15015 ()
		{
			AssertEquals ("#01", "--P", Double.MinValue.ToString ("-P", _nfi));
			AssertEquals ("#02", "-P-", Double.MinValue.ToString ("P-", _nfi));
			AssertEquals ("#03", "--P-", Double.MinValue.ToString ("-P-", _nfi));
		}

		[Test]
		public void Test15016 ()
		{
			AssertEquals ("#01", "P+0", 0.0.ToString ("P+0", _nfi));
			AssertEquals ("#02", "P+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P+0", _nfi));
			AssertEquals ("#03", "-P+179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("P+0", _nfi));
		}

		[Test]
		public void Test15017 ()
		{
			AssertEquals ("#01", "P+9", 0.0.ToString ("P+9", _nfi));
			AssertEquals ("#02", "P+9", Double.MaxValue.ToString ("P+9", _nfi));
			AssertEquals ("#03", "-P+9", Double.MinValue.ToString ("P+9", _nfi));
		}

		[Test]
		public void Test15018 ()
		{
			AssertEquals ("#01", "P-9", 0.0.ToString ("P-9", _nfi));
			AssertEquals ("#02", "P-9", Double.MaxValue.ToString ("P-9", _nfi));
			AssertEquals ("#03", "-P-9", Double.MinValue.ToString ("P-9", _nfi));
		}

		[Test]
		public void Test15019 ()
		{
			AssertEquals ("#01", "P0", 0.0.ToString ("P0,", _nfi));
			AssertEquals ("#02", "P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P0,", _nfi));
			AssertEquals ("#03", "-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("P0,", _nfi));
		}

		[Test]
		public void Test15020 ()
		{
			AssertEquals ("#01", "P0", 0.0.ToString ("P0.", _nfi));
			AssertEquals ("#02", "P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MaxValue.ToString ("P0.", _nfi));
			AssertEquals ("#03", "-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000", Double.MinValue.ToString ("P0.", _nfi));
		}

		[Test]
		public void Test15021 ()
		{
			AssertEquals ("#01", "P0.0", 0.0.ToString ("P0.0", _nfi));
			AssertEquals ("#02", "P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MaxValue.ToString ("P0.0", _nfi));
			AssertEquals ("#03", "-P179769313486232000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000.0", Double.MinValue.ToString ("P0.0", _nfi));
		}

		[Test]
		public void Test15022 ()
		{
			AssertEquals ("#01", "P09", 0.0.ToString ("P0.9", _nfi));
			AssertEquals ("#02", "P1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MaxValue.ToString ("P0.9", _nfi));
			AssertEquals ("#03", "-P1797693134862320000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000009", Double.MinValue.ToString ("P0.9", _nfi));
		}

		[Test]
		public void Test15023 ()
		{
			AssertEquals ("#01", "999.1 %", 9.9905.ToString ("P1", _nfi));
			AssertEquals ("#02", "999.2 %", 9.9915.ToString ("P1", _nfi));
			AssertEquals ("#03", "999.3 %", 9.9925.ToString ("P1", _nfi));
			AssertEquals ("#04", "999.4 %", 9.9935.ToString ("P1", _nfi));
			AssertEquals ("#05", "999.5 %", 9.9945.ToString ("P1", _nfi));
			AssertEquals ("#06", "999.6 %", 9.9955.ToString ("P1", _nfi));
			AssertEquals ("#07", "999.7 %", 9.9965.ToString ("P1", _nfi));
			AssertEquals ("#08", "999.8 %", 9.9975.ToString ("P1", _nfi));
			AssertEquals ("#09", "999.9 %", 9.9985.ToString ("P1", _nfi));
			AssertEquals ("#10", "1,000.0 %", 9.9995.ToString ("P1", _nfi));
		}

		[Test]
		public void Test15024 ()
		{
			AssertEquals ("#01", "999.91 %", 9.99905.ToString ("P2", _nfi));
			AssertEquals ("#02", "999.92 %", 9.99915.ToString ("P2", _nfi));
			AssertEquals ("#03", "999.93 %", 9.99925.ToString ("P2", _nfi));
			AssertEquals ("#04", "999.94 %", 9.99935.ToString ("P2", _nfi));
			AssertEquals ("#05", "999.95 %", 9.99945.ToString ("P2", _nfi));
			AssertEquals ("#06", "999.96 %", 9.99955.ToString ("P2", _nfi));
			AssertEquals ("#07", "999.97 %", 9.99965.ToString ("P2", _nfi));
			AssertEquals ("#08", "999.98 %", 9.99975.ToString ("P2", _nfi));
			AssertEquals ("#09", "999.99 %", 9.99985.ToString ("P2", _nfi));
			AssertEquals ("#10", "1,000.00 %", 9.99995.ToString ("P2", _nfi));
		}

		[Test]
		public void Test15025 ()
		{
			AssertEquals ("#01", "999.99999999991 %", 9.99999999999905.ToString ("P11", _nfi));
			AssertEquals ("#02", "999.99999999992 %", 9.99999999999915.ToString ("P11", _nfi));
			AssertEquals ("#03", "999.99999999993 %", 9.99999999999925.ToString ("P11", _nfi));
			AssertEquals ("#04", "999.99999999994 %", 9.99999999999935.ToString ("P11", _nfi));
			AssertEquals ("#05", "999.99999999995 %", 9.99999999999945.ToString ("P11", _nfi));
			AssertEquals ("#06", "999.99999999996 %", 9.99999999999955.ToString ("P11", _nfi));
			AssertEquals ("#07", "999.99999999997 %", 9.99999999999965.ToString ("P11", _nfi));
			AssertEquals ("#08", "999.99999999998 %", 9.99999999999975.ToString ("P11", _nfi));
			AssertEquals ("#09", "999.99999999999 %", 9.99999999999985.ToString ("P11", _nfi));
			AssertEquals ("#10", "1,000.00000000000 %", 9.99999999999995.ToString ("P11", _nfi));
		}

		[Test]
		public void Test15026 ()
		{
			AssertEquals ("#01", "999.999999999991 %", 9.999999999999905.ToString ("P12", _nfi));
			AssertEquals ("#02", "999.999999999991 %", 9.999999999999915.ToString ("P12", _nfi));
			AssertEquals ("#03", "999.999999999993 %", 9.999999999999925.ToString ("P12", _nfi));
			AssertEquals ("#04", "999.999999999993 %", 9.999999999999935.ToString ("P12", _nfi));
			AssertEquals ("#05", "999.999999999994 %", 9.999999999999945.ToString ("P12", _nfi));
			AssertEquals ("#06", "999.999999999996 %", 9.999999999999955.ToString ("P12", _nfi));
			AssertEquals ("#07", "999.999999999996 %", 9.999999999999965.ToString ("P12", _nfi));
			AssertEquals ("#08", "999.999999999998 %", 9.999999999999975.ToString ("P12", _nfi));
			AssertEquals ("#09", "999.999999999999 %", 9.999999999999985.ToString ("P12", _nfi));
			AssertEquals ("#10", "999.999999999999 %", 9.999999999999995.ToString ("P12", _nfi));
		}

		[Test]
		public void Test15027 ()
		{
			AssertEquals ("#01", "999.9999999999990 %", 9.9999999999999905.ToString ("P13", _nfi));
			AssertEquals ("#02", "999.9999999999990 %", 9.9999999999999915.ToString ("P13", _nfi));
			AssertEquals ("#03", "999.9999999999990 %", 9.9999999999999925.ToString ("P13", _nfi));
			AssertEquals ("#04", "999.9999999999990 %", 9.9999999999999935.ToString ("P13", _nfi));
			AssertEquals ("#05", "999.9999999999990 %", 9.9999999999999945.ToString ("P13", _nfi));
			AssertEquals ("#06", "999.9999999999990 %", 9.9999999999999955.ToString ("P13", _nfi));
			AssertEquals ("#07", "1,000.0000000000000 %", 9.9999999999999965.ToString ("P13", _nfi));
			AssertEquals ("#08", "1,000.0000000000000 %", 9.9999999999999975.ToString ("P13", _nfi));
			AssertEquals ("#09", "1,000.0000000000000 %", 9.9999999999999985.ToString ("P13", _nfi));
			AssertEquals ("#10", "1,000.0000000000000 %", 9.9999999999999995.ToString ("P13", _nfi));
		}

		[Test]
		public void Test15028 ()
		{
			AssertEquals ("#01", "1", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N0", _nfi));
			AssertEquals ("#02", "1.234567890123", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N12", _nfi));
			AssertEquals ("#03", "1.2345678901235", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N13", _nfi));
			AssertEquals ("#04", "1.23456789012346", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N14", _nfi));
			AssertEquals ("#05", "1.234567890123460", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N15", _nfi));
			AssertEquals ("#06", "1.234567890123460000000000000000000000000000000000000000000000000000000000000000000000000000000000000", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N99", _nfi));
			AssertEquals ("#07", "N101", 1.234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789.ToString ("N100"));
		}

		[Test]
		public void Test15029 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.PercentDecimalSeparator = "#";
			AssertEquals ("#01", "-9,999,999,990#00 %", (-99999999.9).ToString ("P", nfi));
		}

		[Test]
		public void Test15030 ()
		{
			NumberFormatInfo nfi = _nfi.Clone() as NumberFormatInfo;
			nfi.NegativeSign = "+";
			nfi.PositiveSign = "-";

			AssertEquals ("#01", "1,000.00 %", 10.0.ToString ("P", nfi));
			AssertEquals ("#02", "0.00 %", 0.0.ToString ("P", nfi));
			AssertEquals ("#03", "+1,000.00 %", (-10.0).ToString ("P", nfi));
		}

		[Test]
		public void Test15031 ()
		{
			AssertEquals ("#01", "Infinity", (Double.MaxValue / 0.0).ToString ("N99", _nfi)); 
			AssertEquals ("#02", "-Infinity", (Double.MinValue / 0.0).ToString ("N99", _nfi)); 
			AssertEquals ("#03", "NaN", (0.0 / 0.0).ToString ("N99", _nfi)); 
		}

		// TestRoundtrip for double and single
		[Test]
		public void TestRoundtrip()
		{
			AssertEquals ("#01", "1.2345678901234567", 1.2345678901234567890.ToString ("R", _nfi));
			AssertEquals ("#02", "1.2345678901234567", 1.2345678901234567890.ToString ("r", _nfi));
			AssertEquals ("#03", "1.2345678901234567", 1.2345678901234567890.ToString ("R0", _nfi));
			AssertEquals ("#04", "1.2345678901234567", 1.2345678901234567890.ToString ("r0", _nfi));
			AssertEquals ("#05", "1.2345678901234567", 1.2345678901234567890.ToString ("R99", _nfi));
			AssertEquals ("#06", "1.2345678901234567", 1.2345678901234567890.ToString ("r99", _nfi));
			AssertEquals ("#07", "-1.7976931348623157E+308", Double.MinValue.ToString ("R"));
			AssertEquals ("#08", "1.7976931348623157E+308", Double.MaxValue.ToString ("R"));
			AssertEquals ("#09", "-1.7976931348623147E+308", (-1.7976931348623147E+308).ToString("R"));
			AssertEquals ("#10", "-3.40282347E+38", Single.MinValue.ToString("R"));
			AssertEquals ("#11", "3.40282347E+38", Single.MaxValue.ToString("R"));
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
				AssertEquals ("Iter#" + exp, val, rndTripVal);
            }
		}

		// Test17000 - Double and X
		[Test]
		[ExpectedException (typeof (FormatException))]
		public void Test17000 ()
		{
			AssertEquals ("#01", "", 0.0.ToString ("X99", _nfi)); 
		}
	}
}
