//
// assembly:	System_test
// namespace:	MonoTests.System.Text.RegularExpressions
// file:	PerlTest.cs
//
// Authors:	
//   Dan Lewis (dlewis@gmx.co.uk)
//   Martin Willemoes Hansen (mwh@sysrq.dk)
//   Gonzalo Paniagua Javier
//
// (c) 2002 Dan Lewis
// (c) 2003 Martin Willemoes Hansen
// (c) 2005 Novell, Inc.
//

using System;
using System.Text.RegularExpressions;

using NUnit.Framework;

namespace MonoTests.System.Text.RegularExpressions {
	[TestFixture]
	public class PerlTest {
		[Test]
		public void Trials ()
		{
			Console.WriteLine ("{0} trials", PerlTrials.trials.Length);
		}

		[Test]
		public void Trial0000 ()
		{
			RegexTrial t = PerlTrials.trials [0];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0000", t.Expected, actual);
		}

		[Test]
		public void Trial0001 ()
		{
			RegexTrial t = PerlTrials.trials [1];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0001", t.Expected, actual);
		}

		[Test]
		public void Trial0002 ()
		{
			RegexTrial t = PerlTrials.trials [2];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0002", t.Expected, actual);
		}

		[Test]
		public void Trial0003 ()
		{
			RegexTrial t = PerlTrials.trials [3];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0003", t.Expected, actual);
		}

		[Test]
		public void Trial0004 ()
		{
			RegexTrial t = PerlTrials.trials [4];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0004", t.Expected, actual);
		}

		[Test]
		public void Trial0005 ()
		{
			RegexTrial t = PerlTrials.trials [5];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0005", t.Expected, actual);
		}

		[Test]
		public void Trial0006 ()
		{
			RegexTrial t = PerlTrials.trials [6];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0006", t.Expected, actual);
		}

		[Test]
		public void Trial0007 ()
		{
			RegexTrial t = PerlTrials.trials [7];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0007", t.Expected, actual);
		}

		[Test]
		public void Trial0008 ()
		{
			RegexTrial t = PerlTrials.trials [8];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0008", t.Expected, actual);
		}

		[Test]
		public void Trial0009 ()
		{
			RegexTrial t = PerlTrials.trials [9];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0009", t.Expected, actual);
		}

		[Test]
		public void Trial0010 ()
		{
			RegexTrial t = PerlTrials.trials [10];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0010", t.Expected, actual);
		}

		[Test]
		public void Trial0011 ()
		{
			RegexTrial t = PerlTrials.trials [11];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0011", t.Expected, actual);
		}

		[Test]
		public void Trial0012 ()
		{
			RegexTrial t = PerlTrials.trials [12];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0012", t.Expected, actual);
		}

		[Test]
		public void Trial0013 ()
		{
			RegexTrial t = PerlTrials.trials [13];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0013", t.Expected, actual);
		}

		[Test]
		public void Trial0014 ()
		{
			RegexTrial t = PerlTrials.trials [14];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0014", t.Expected, actual);
		}

		[Test]
		public void Trial0015 ()
		{
			RegexTrial t = PerlTrials.trials [15];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0015", t.Expected, actual);
		}

		[Test]
		public void Trial0016 ()
		{
			RegexTrial t = PerlTrials.trials [16];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0016", t.Expected, actual);
		}

		[Test]
		public void Trial0017 ()
		{
			RegexTrial t = PerlTrials.trials [17];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0017", t.Expected, actual);
		}

		[Test]
		public void Trial0018 ()
		{
			RegexTrial t = PerlTrials.trials [18];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0018", t.Expected, actual);
		}

		[Test]
		public void Trial0019 ()
		{
			RegexTrial t = PerlTrials.trials [19];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0019", t.Expected, actual);
		}

		[Test]
		public void Trial0020 ()
		{
			RegexTrial t = PerlTrials.trials [20];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0020", t.Expected, actual);
		}

		[Test]
		public void Trial0021 ()
		{
			RegexTrial t = PerlTrials.trials [21];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0021", t.Expected, actual);
		}

		[Test]
		public void Trial0022 ()
		{
			RegexTrial t = PerlTrials.trials [22];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0022", t.Expected, actual);
		}

		[Test]
		public void Trial0023 ()
		{
			RegexTrial t = PerlTrials.trials [23];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0023", t.Expected, actual);
		}

		[Test]
		public void Trial0024 ()
		{
			RegexTrial t = PerlTrials.trials [24];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0024", t.Expected, actual);
		}

		[Test]
		public void Trial0025 ()
		{
			RegexTrial t = PerlTrials.trials [25];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0025", t.Expected, actual);
		}

		[Test]
		public void Trial0026 ()
		{
			RegexTrial t = PerlTrials.trials [26];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0026", t.Expected, actual);
		}

		[Test]
		public void Trial0027 ()
		{
			RegexTrial t = PerlTrials.trials [27];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0027", t.Expected, actual);
		}

		[Test]
		public void Trial0028 ()
		{
			RegexTrial t = PerlTrials.trials [28];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0028", t.Expected, actual);
		}

		[Test]
		public void Trial0029 ()
		{
			RegexTrial t = PerlTrials.trials [29];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0029", t.Expected, actual);
		}

		[Test]
		public void Trial0030 ()
		{
			RegexTrial t = PerlTrials.trials [30];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0030", t.Expected, actual);
		}

		[Test]
		public void Trial0031 ()
		{
			RegexTrial t = PerlTrials.trials [31];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0031", t.Expected, actual);
		}

		[Test]
		public void Trial0032 ()
		{
			RegexTrial t = PerlTrials.trials [32];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0032", t.Expected, actual);
		}

		[Test]
		public void Trial0033 ()
		{
			RegexTrial t = PerlTrials.trials [33];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0033", t.Expected, actual);
		}

		[Test]
		public void Trial0034 ()
		{
			RegexTrial t = PerlTrials.trials [34];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0034", t.Expected, actual);
		}

		[Test]
		public void Trial0035 ()
		{
			RegexTrial t = PerlTrials.trials [35];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0035", t.Expected, actual);
		}

		[Test]
		public void Trial0036 ()
		{
			RegexTrial t = PerlTrials.trials [36];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0036", t.Expected, actual);
		}

		[Test]
		public void Trial0037 ()
		{
			RegexTrial t = PerlTrials.trials [37];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0037", t.Expected, actual);
		}

		[Test]
		public void Trial0038 ()
		{
			RegexTrial t = PerlTrials.trials [38];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0038", t.Expected, actual);
		}

		[Test]
		public void Trial0039 ()
		{
			RegexTrial t = PerlTrials.trials [39];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0039", t.Expected, actual);
		}

		[Test]
		public void Trial0040 ()
		{
			RegexTrial t = PerlTrials.trials [40];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0040", t.Expected, actual);
		}

		[Test]
		public void Trial0041 ()
		{
			RegexTrial t = PerlTrials.trials [41];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0041", t.Expected, actual);
		}

		[Test]
		public void Trial0042 ()
		{
			RegexTrial t = PerlTrials.trials [42];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0042", t.Expected, actual);
		}

		[Test]
		public void Trial0043 ()
		{
			RegexTrial t = PerlTrials.trials [43];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0043", t.Expected, actual);
		}

		[Test]
		public void Trial0044 ()
		{
			RegexTrial t = PerlTrials.trials [44];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0044", t.Expected, actual);
		}

		[Test]
		public void Trial0045 ()
		{
			RegexTrial t = PerlTrials.trials [45];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0045", t.Expected, actual);
		}

		[Test]
		public void Trial0046 ()
		{
			RegexTrial t = PerlTrials.trials [46];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0046", t.Expected, actual);
		}

		[Test]
		public void Trial0047 ()
		{
			RegexTrial t = PerlTrials.trials [47];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0047", t.Expected, actual);
		}

		[Test]
		public void Trial0048 ()
		{
			RegexTrial t = PerlTrials.trials [48];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0048", t.Expected, actual);
		}

		[Test]
		public void Trial0049 ()
		{
			RegexTrial t = PerlTrials.trials [49];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0049", t.Expected, actual);
		}

		[Test]
		public void Trial0050 ()
		{
			RegexTrial t = PerlTrials.trials [50];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0050", t.Expected, actual);
		}

		[Test]
		public void Trial0051 ()
		{
			RegexTrial t = PerlTrials.trials [51];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0051", t.Expected, actual);
		}

		[Test]
		public void Trial0052 ()
		{
			RegexTrial t = PerlTrials.trials [52];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0052", t.Expected, actual);
		}

		[Test]
		public void Trial0053 ()
		{
			RegexTrial t = PerlTrials.trials [53];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0053", t.Expected, actual);
		}

		[Test]
		public void Trial0054 ()
		{
			RegexTrial t = PerlTrials.trials [54];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0054", t.Expected, actual);
		}

		[Test]
		public void Trial0055 ()
		{
			RegexTrial t = PerlTrials.trials [55];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0055", t.Expected, actual);
		}

		[Test]
		public void Trial0056 ()
		{
			RegexTrial t = PerlTrials.trials [56];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0056", t.Expected, actual);
		}

		[Test]
		public void Trial0057 ()
		{
			RegexTrial t = PerlTrials.trials [57];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0057", t.Expected, actual);
		}

		[Test]
		public void Trial0058 ()
		{
			RegexTrial t = PerlTrials.trials [58];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0058", t.Expected, actual);
		}

		[Test]
		public void Trial0059 ()
		{
			RegexTrial t = PerlTrials.trials [59];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0059", t.Expected, actual);
		}

		[Test]
		public void Trial0060 ()
		{
			RegexTrial t = PerlTrials.trials [60];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0060", t.Expected, actual);
		}

		[Test]
		public void Trial0061 ()
		{
			RegexTrial t = PerlTrials.trials [61];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0061", t.Expected, actual);
		}

		[Test]
		public void Trial0062 ()
		{
			RegexTrial t = PerlTrials.trials [62];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0062", t.Expected, actual);
		}

		[Test]
		public void Trial0063 ()
		{
			RegexTrial t = PerlTrials.trials [63];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0063", t.Expected, actual);
		}

		[Test]
		public void Trial0064 ()
		{
			RegexTrial t = PerlTrials.trials [64];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0064", t.Expected, actual);
		}

		[Test]
		public void Trial0065 ()
		{
			RegexTrial t = PerlTrials.trials [65];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0065", t.Expected, actual);
		}

		[Test]
		public void Trial0066 ()
		{
			RegexTrial t = PerlTrials.trials [66];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0066", t.Expected, actual);
		}

		[Test]
		public void Trial0067 ()
		{
			RegexTrial t = PerlTrials.trials [67];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0067", t.Expected, actual);
		}

		[Test]
		public void Trial0068 ()
		{
			RegexTrial t = PerlTrials.trials [68];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0068", t.Expected, actual);
		}

		[Test]
		public void Trial0069 ()
		{
			RegexTrial t = PerlTrials.trials [69];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0069", t.Expected, actual);
		}

		[Test]
		public void Trial0070 ()
		{
			RegexTrial t = PerlTrials.trials [70];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0070", t.Expected, actual);
		}

		[Test]
		public void Trial0071 ()
		{
			RegexTrial t = PerlTrials.trials [71];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0071", t.Expected, actual);
		}

		[Test]
		public void Trial0072 ()
		{
			RegexTrial t = PerlTrials.trials [72];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0072", t.Expected, actual);
		}

		[Test]
		public void Trial0073 ()
		{
			RegexTrial t = PerlTrials.trials [73];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0073", t.Expected, actual);
		}

		[Test]
		public void Trial0074 ()
		{
			RegexTrial t = PerlTrials.trials [74];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0074", t.Expected, actual);
		}

		[Test]
		public void Trial0075 ()
		{
			RegexTrial t = PerlTrials.trials [75];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0075", t.Expected, actual);
		}

		[Test]
		public void Trial0076 ()
		{
			RegexTrial t = PerlTrials.trials [76];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0076", t.Expected, actual);
		}

		[Test]
		public void Trial0077 ()
		{
			RegexTrial t = PerlTrials.trials [77];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0077", t.Expected, actual);
		}

		[Test]
		public void Trial0078 ()
		{
			RegexTrial t = PerlTrials.trials [78];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0078", t.Expected, actual);
		}

		[Test]
		public void Trial0079 ()
		{
			RegexTrial t = PerlTrials.trials [79];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0079", t.Expected, actual);
		}

		[Test]
		public void Trial0080 ()
		{
			RegexTrial t = PerlTrials.trials [80];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0080", t.Expected, actual);
		}

		[Test]
		public void Trial0081 ()
		{
			RegexTrial t = PerlTrials.trials [81];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0081", t.Expected, actual);
		}

		[Test]
		public void Trial0082 ()
		{
			RegexTrial t = PerlTrials.trials [82];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0082", t.Expected, actual);
		}

		[Test]
		public void Trial0083 ()
		{
			RegexTrial t = PerlTrials.trials [83];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0083", t.Expected, actual);
		}

		[Test]
		public void Trial0084 ()
		{
			RegexTrial t = PerlTrials.trials [84];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0084", t.Expected, actual);
		}

		[Test]
		public void Trial0085 ()
		{
			RegexTrial t = PerlTrials.trials [85];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0085", t.Expected, actual);
		}

		[Test]
		public void Trial0086 ()
		{
			RegexTrial t = PerlTrials.trials [86];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0086", t.Expected, actual);
		}

		[Test]
		public void Trial0087 ()
		{
			RegexTrial t = PerlTrials.trials [87];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0087", t.Expected, actual);
		}

		[Test]
		public void Trial0088 ()
		{
			RegexTrial t = PerlTrials.trials [88];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0088", t.Expected, actual);
		}

		[Test]
		public void Trial0089 ()
		{
			RegexTrial t = PerlTrials.trials [89];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0089", t.Expected, actual);
		}

		[Test]
		public void Trial0090 ()
		{
			RegexTrial t = PerlTrials.trials [90];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0090", t.Expected, actual);
		}

		[Test]
		public void Trial0091 ()
		{
			RegexTrial t = PerlTrials.trials [91];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0091", t.Expected, actual);
		}

		[Test]
		public void Trial0092 ()
		{
			RegexTrial t = PerlTrials.trials [92];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0092", t.Expected, actual);
		}

		[Test]
		public void Trial0093 ()
		{
			RegexTrial t = PerlTrials.trials [93];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0093", t.Expected, actual);
		}

		[Test]
		public void Trial0094 ()
		{
			RegexTrial t = PerlTrials.trials [94];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0094", t.Expected, actual);
		}

		[Test]
		public void Trial0095 ()
		{
			RegexTrial t = PerlTrials.trials [95];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0095", t.Expected, actual);
		}

		[Test]
		public void Trial0096 ()
		{
			RegexTrial t = PerlTrials.trials [96];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0096", t.Expected, actual);
		}

		[Test]
		public void Trial0097 ()
		{
			RegexTrial t = PerlTrials.trials [97];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0097", t.Expected, actual);
		}

		[Test]
		public void Trial0098 ()
		{
			RegexTrial t = PerlTrials.trials [98];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0098", t.Expected, actual);
		}

		[Test]
		public void Trial0099 ()
		{
			RegexTrial t = PerlTrials.trials [99];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0099", t.Expected, actual);
		}

		[Test]
		public void Trial0100 ()
		{
			RegexTrial t = PerlTrials.trials [100];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0100", t.Expected, actual);
		}

		[Test]
		public void Trial0101 ()
		{
			RegexTrial t = PerlTrials.trials [101];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0101", t.Expected, actual);
		}

		[Test]
		public void Trial0102 ()
		{
			RegexTrial t = PerlTrials.trials [102];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0102", t.Expected, actual);
		}

		[Test]
		public void Trial0103 ()
		{
			RegexTrial t = PerlTrials.trials [103];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0103", t.Expected, actual);
		}

		[Test]
		public void Trial0104 ()
		{
			RegexTrial t = PerlTrials.trials [104];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0104", t.Expected, actual);
		}

		[Test]
		public void Trial0105 ()
		{
			RegexTrial t = PerlTrials.trials [105];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0105", t.Expected, actual);
		}

		[Test]
		public void Trial0106 ()
		{
			RegexTrial t = PerlTrials.trials [106];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0106", t.Expected, actual);
		}

		[Test]
		public void Trial0107 ()
		{
			RegexTrial t = PerlTrials.trials [107];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0107", t.Expected, actual);
		}

		[Test]
		public void Trial0108 ()
		{
			RegexTrial t = PerlTrials.trials [108];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0108", t.Expected, actual);
		}

		[Test]
		public void Trial0109 ()
		{
			RegexTrial t = PerlTrials.trials [109];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0109", t.Expected, actual);
		}

		[Test]
		public void Trial0110 ()
		{
			RegexTrial t = PerlTrials.trials [110];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0110", t.Expected, actual);
		}

		[Test]
		public void Trial0111 ()
		{
			RegexTrial t = PerlTrials.trials [111];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0111", t.Expected, actual);
		}

		[Test]
		public void Trial0112 ()
		{
			RegexTrial t = PerlTrials.trials [112];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0112", t.Expected, actual);
		}

		[Test]
		public void Trial0113 ()
		{
			RegexTrial t = PerlTrials.trials [113];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0113", t.Expected, actual);
		}

		[Test]
		public void Trial0114 ()
		{
			RegexTrial t = PerlTrials.trials [114];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0114", t.Expected, actual);
		}

		[Test]
		public void Trial0115 ()
		{
			RegexTrial t = PerlTrials.trials [115];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0115", t.Expected, actual);
		}

		[Test]
		public void Trial0116 ()
		{
			RegexTrial t = PerlTrials.trials [116];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0116", t.Expected, actual);
		}

		[Test]
		public void Trial0117 ()
		{
			RegexTrial t = PerlTrials.trials [117];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0117", t.Expected, actual);
		}

		[Test]
		public void Trial0118 ()
		{
			RegexTrial t = PerlTrials.trials [118];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0118", t.Expected, actual);
		}

		[Test]
		public void Trial0119 ()
		{
			RegexTrial t = PerlTrials.trials [119];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0119", t.Expected, actual);
		}

		[Test]
		public void Trial0120 ()
		{
			RegexTrial t = PerlTrials.trials [120];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0120", t.Expected, actual);
		}

		[Test]
		public void Trial0121 ()
		{
			RegexTrial t = PerlTrials.trials [121];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0121", t.Expected, actual);
		}

		[Test]
		public void Trial0122 ()
		{
			RegexTrial t = PerlTrials.trials [122];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0122", t.Expected, actual);
		}

		[Test]
		public void Trial0123 ()
		{
			RegexTrial t = PerlTrials.trials [123];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0123", t.Expected, actual);
		}

		[Test]
		public void Trial0124 ()
		{
			RegexTrial t = PerlTrials.trials [124];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0124", t.Expected, actual);
		}

		[Test]
		public void Trial0125 ()
		{
			RegexTrial t = PerlTrials.trials [125];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0125", t.Expected, actual);
		}

		[Test]
		public void Trial0126 ()
		{
			RegexTrial t = PerlTrials.trials [126];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0126", t.Expected, actual);
		}

		[Test]
		public void Trial0127 ()
		{
			RegexTrial t = PerlTrials.trials [127];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0127", t.Expected, actual);
		}

		[Test]
		public void Trial0128 ()
		{
			RegexTrial t = PerlTrials.trials [128];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0128", t.Expected, actual);
		}

		[Test]
		public void Trial0129 ()
		{
			RegexTrial t = PerlTrials.trials [129];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0129", t.Expected, actual);
		}

		[Test]
		public void Trial0130 ()
		{
			RegexTrial t = PerlTrials.trials [130];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0130", t.Expected, actual);
		}

		[Test]
		public void Trial0131 ()
		{
			RegexTrial t = PerlTrials.trials [131];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0131", t.Expected, actual);
		}

		[Test]
		public void Trial0132 ()
		{
			RegexTrial t = PerlTrials.trials [132];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0132", t.Expected, actual);
		}

		[Test]
		public void Trial0133 ()
		{
			RegexTrial t = PerlTrials.trials [133];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0133", t.Expected, actual);
		}

		[Test]
		public void Trial0134 ()
		{
			RegexTrial t = PerlTrials.trials [134];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0134", t.Expected, actual);
		}

		[Test]
		public void Trial0135 ()
		{
			RegexTrial t = PerlTrials.trials [135];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0135", t.Expected, actual);
		}

		[Test]
		public void Trial0136 ()
		{
			RegexTrial t = PerlTrials.trials [136];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0136", t.Expected, actual);
		}

		[Test]
		public void Trial0137 ()
		{
			RegexTrial t = PerlTrials.trials [137];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0137", t.Expected, actual);
		}

		[Test]
		public void Trial0138 ()
		{
			RegexTrial t = PerlTrials.trials [138];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0138", t.Expected, actual);
		}

		[Test]
		public void Trial0139 ()
		{
			RegexTrial t = PerlTrials.trials [139];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0139", t.Expected, actual);
		}

		[Test]
		public void Trial0140 ()
		{
			RegexTrial t = PerlTrials.trials [140];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0140", t.Expected, actual);
		}

		[Test]
		public void Trial0141 ()
		{
			RegexTrial t = PerlTrials.trials [141];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0141", t.Expected, actual);
		}

		[Test]
		public void Trial0142 ()
		{
			RegexTrial t = PerlTrials.trials [142];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0142", t.Expected, actual);
		}

		[Test]
		public void Trial0143 ()
		{
			RegexTrial t = PerlTrials.trials [143];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0143", t.Expected, actual);
		}

		[Test]
		public void Trial0144 ()
		{
			RegexTrial t = PerlTrials.trials [144];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0144", t.Expected, actual);
		}

		[Test]
		public void Trial0145 ()
		{
			RegexTrial t = PerlTrials.trials [145];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0145", t.Expected, actual);
		}

		[Test]
		public void Trial0146 ()
		{
			RegexTrial t = PerlTrials.trials [146];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0146", t.Expected, actual);
		}

		[Test]
		public void Trial0147 ()
		{
			RegexTrial t = PerlTrials.trials [147];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0147", t.Expected, actual);
		}

		[Test]
		public void Trial0148 ()
		{
			RegexTrial t = PerlTrials.trials [148];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0148", t.Expected, actual);
		}

		[Test]
		public void Trial0149 ()
		{
			RegexTrial t = PerlTrials.trials [149];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0149", t.Expected, actual);
		}

		[Test]
		public void Trial0150 ()
		{
			RegexTrial t = PerlTrials.trials [150];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0150", t.Expected, actual);
		}

		[Test]
		public void Trial0151 ()
		{
			RegexTrial t = PerlTrials.trials [151];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0151", t.Expected, actual);
		}

		[Test]
		public void Trial0152 ()
		{
			RegexTrial t = PerlTrials.trials [152];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0152", t.Expected, actual);
		}

		[Test]
		public void Trial0153 ()
		{
			RegexTrial t = PerlTrials.trials [153];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0153", t.Expected, actual);
		}

		[Test]
		public void Trial0154 ()
		{
			RegexTrial t = PerlTrials.trials [154];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0154", t.Expected, actual);
		}

		[Test]
		public void Trial0155 ()
		{
			RegexTrial t = PerlTrials.trials [155];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0155", t.Expected, actual);
		}

		[Test]
		public void Trial0156 ()
		{
			RegexTrial t = PerlTrials.trials [156];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0156", t.Expected, actual);
		}

		[Test]
		public void Trial0157 ()
		{
			RegexTrial t = PerlTrials.trials [157];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0157", t.Expected, actual);
		}

		[Test]
		public void Trial0158 ()
		{
			RegexTrial t = PerlTrials.trials [158];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0158", t.Expected, actual);
		}

		[Test]
		public void Trial0159 ()
		{
			RegexTrial t = PerlTrials.trials [159];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0159", t.Expected, actual);
		}

		[Test]
		public void Trial0160 ()
		{
			RegexTrial t = PerlTrials.trials [160];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0160", t.Expected, actual);
		}

		[Test]
		public void Trial0161 ()
		{
			RegexTrial t = PerlTrials.trials [161];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0161", t.Expected, actual);
		}

		[Test]
		public void Trial0162 ()
		{
			RegexTrial t = PerlTrials.trials [162];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0162", t.Expected, actual);
		}

		[Test]
		public void Trial0163 ()
		{
			RegexTrial t = PerlTrials.trials [163];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0163", t.Expected, actual);
		}

		[Test]
		public void Trial0164 ()
		{
			RegexTrial t = PerlTrials.trials [164];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0164", t.Expected, actual);
		}

		[Test]
		public void Trial0165 ()
		{
			RegexTrial t = PerlTrials.trials [165];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0165", t.Expected, actual);
		}

		[Test]
		public void Trial0166 ()
		{
			RegexTrial t = PerlTrials.trials [166];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0166", t.Expected, actual);
		}

		[Test]
		public void Trial0167 ()
		{
			RegexTrial t = PerlTrials.trials [167];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0167", t.Expected, actual);
		}

		[Test]
		public void Trial0168 ()
		{
			RegexTrial t = PerlTrials.trials [168];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0168", t.Expected, actual);
		}

		[Test]
		public void Trial0169 ()
		{
			RegexTrial t = PerlTrials.trials [169];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0169", t.Expected, actual);
		}

		[Test]
		public void Trial0170 ()
		{
			RegexTrial t = PerlTrials.trials [170];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0170", t.Expected, actual);
		}

		[Test]
		public void Trial0171 ()
		{
			RegexTrial t = PerlTrials.trials [171];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0171", t.Expected, actual);
		}

		[Test]
		public void Trial0172 ()
		{
			RegexTrial t = PerlTrials.trials [172];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0172", t.Expected, actual);
		}

		[Test]
		public void Trial0173 ()
		{
			RegexTrial t = PerlTrials.trials [173];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0173", t.Expected, actual);
		}

		[Test]
		public void Trial0174 ()
		{
			RegexTrial t = PerlTrials.trials [174];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0174", t.Expected, actual);
		}

		[Test]
		public void Trial0175 ()
		{
			RegexTrial t = PerlTrials.trials [175];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0175", t.Expected, actual);
		}

		[Test]
		public void Trial0176 ()
		{
			RegexTrial t = PerlTrials.trials [176];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0176", t.Expected, actual);
		}

		[Test]
		public void Trial0177 ()
		{
			RegexTrial t = PerlTrials.trials [177];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0177", t.Expected, actual);
		}

		[Test]
		public void Trial0178 ()
		{
			RegexTrial t = PerlTrials.trials [178];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0178", t.Expected, actual);
		}

		[Test]
		public void Trial0179 ()
		{
			RegexTrial t = PerlTrials.trials [179];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0179", t.Expected, actual);
		}

		[Test]
		public void Trial0180 ()
		{
			RegexTrial t = PerlTrials.trials [180];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0180", t.Expected, actual);
		}

		[Test]
		public void Trial0181 ()
		{
			RegexTrial t = PerlTrials.trials [181];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0181", t.Expected, actual);
		}

		[Test]
		public void Trial0182 ()
		{
			RegexTrial t = PerlTrials.trials [182];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0182", t.Expected, actual);
		}

		[Test]
		public void Trial0183 ()
		{
			RegexTrial t = PerlTrials.trials [183];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0183", t.Expected, actual);
		}

		[Test]
		public void Trial0184 ()
		{
			RegexTrial t = PerlTrials.trials [184];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0184", t.Expected, actual);
		}

		[Test]
		public void Trial0185 ()
		{
			RegexTrial t = PerlTrials.trials [185];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0185", t.Expected, actual);
		}

		[Test]
		public void Trial0186 ()
		{
			RegexTrial t = PerlTrials.trials [186];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0186", t.Expected, actual);
		}

		[Test]
		public void Trial0187 ()
		{
			RegexTrial t = PerlTrials.trials [187];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0187", t.Expected, actual);
		}

		[Test]
		public void Trial0188 ()
		{
			RegexTrial t = PerlTrials.trials [188];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0188", t.Expected, actual);
		}

		[Test]
		public void Trial0189 ()
		{
			RegexTrial t = PerlTrials.trials [189];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0189", t.Expected, actual);
		}

		[Test]
		public void Trial0190 ()
		{
			RegexTrial t = PerlTrials.trials [190];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0190", t.Expected, actual);
		}

		[Test]
		public void Trial0191 ()
		{
			RegexTrial t = PerlTrials.trials [191];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0191", t.Expected, actual);
		}

		[Test]
		public void Trial0192 ()
		{
			RegexTrial t = PerlTrials.trials [192];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0192", t.Expected, actual);
		}

		[Test]
		public void Trial0193 ()
		{
			RegexTrial t = PerlTrials.trials [193];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0193", t.Expected, actual);
		}

		[Test]
		public void Trial0194 ()
		{
			RegexTrial t = PerlTrials.trials [194];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0194", t.Expected, actual);
		}

		[Test]
		public void Trial0195 ()
		{
			RegexTrial t = PerlTrials.trials [195];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0195", t.Expected, actual);
		}

		[Test]
		public void Trial0196 ()
		{
			RegexTrial t = PerlTrials.trials [196];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0196", t.Expected, actual);
		}

		[Test]
		public void Trial0197 ()
		{
			RegexTrial t = PerlTrials.trials [197];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0197", t.Expected, actual);
		}

		[Test]
		public void Trial0198 ()
		{
			RegexTrial t = PerlTrials.trials [198];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0198", t.Expected, actual);
		}

		[Test]
		public void Trial0199 ()
		{
			RegexTrial t = PerlTrials.trials [199];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0199", t.Expected, actual);
		}

		[Test]
		public void Trial0200 ()
		{
			RegexTrial t = PerlTrials.trials [200];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0200", t.Expected, actual);
		}

		[Test]
		public void Trial0201 ()
		{
			RegexTrial t = PerlTrials.trials [201];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0201", t.Expected, actual);
		}

		[Test]
		public void Trial0202 ()
		{
			RegexTrial t = PerlTrials.trials [202];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0202", t.Expected, actual);
		}

		[Test]
		public void Trial0203 ()
		{
			RegexTrial t = PerlTrials.trials [203];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0203", t.Expected, actual);
		}

		[Test]
		public void Trial0204 ()
		{
			RegexTrial t = PerlTrials.trials [204];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0204", t.Expected, actual);
		}

		[Test]
		public void Trial0205 ()
		{
			RegexTrial t = PerlTrials.trials [205];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0205", t.Expected, actual);
		}

		[Test]
		public void Trial0206 ()
		{
			RegexTrial t = PerlTrials.trials [206];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0206", t.Expected, actual);
		}

		[Test]
		public void Trial0207 ()
		{
			RegexTrial t = PerlTrials.trials [207];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0207", t.Expected, actual);
		}

		[Test]
		public void Trial0208 ()
		{
			RegexTrial t = PerlTrials.trials [208];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0208", t.Expected, actual);
		}

		[Test]
		public void Trial0209 ()
		{
			RegexTrial t = PerlTrials.trials [209];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0209", t.Expected, actual);
		}

		[Test]
		public void Trial0210 ()
		{
			RegexTrial t = PerlTrials.trials [210];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0210", t.Expected, actual);
		}

		[Test]
		public void Trial0211 ()
		{
			RegexTrial t = PerlTrials.trials [211];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0211", t.Expected, actual);
		}

		[Test]
		public void Trial0212 ()
		{
			RegexTrial t = PerlTrials.trials [212];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0212", t.Expected, actual);
		}

		[Test]
		public void Trial0213 ()
		{
			RegexTrial t = PerlTrials.trials [213];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0213", t.Expected, actual);
		}

		[Test]
		public void Trial0214 ()
		{
			RegexTrial t = PerlTrials.trials [214];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0214", t.Expected, actual);
		}

		[Test]
		public void Trial0215 ()
		{
			RegexTrial t = PerlTrials.trials [215];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0215", t.Expected, actual);
		}

		[Test]
		public void Trial0216 ()
		{
			RegexTrial t = PerlTrials.trials [216];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0216", t.Expected, actual);
		}

		[Test]
		public void Trial0217 ()
		{
			RegexTrial t = PerlTrials.trials [217];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0217", t.Expected, actual);
		}

		[Test]
		public void Trial0218 ()
		{
			RegexTrial t = PerlTrials.trials [218];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0218", t.Expected, actual);
		}

		[Test]
		public void Trial0219 ()
		{
			RegexTrial t = PerlTrials.trials [219];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0219", t.Expected, actual);
		}

		[Test]
		public void Trial0220 ()
		{
			RegexTrial t = PerlTrials.trials [220];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0220", t.Expected, actual);
		}

		[Test]
		public void Trial0221 ()
		{
			RegexTrial t = PerlTrials.trials [221];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0221", t.Expected, actual);
		}

		[Test]
		public void Trial0222 ()
		{
			RegexTrial t = PerlTrials.trials [222];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0222", t.Expected, actual);
		}

		[Test]
		public void Trial0223 ()
		{
			RegexTrial t = PerlTrials.trials [223];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0223", t.Expected, actual);
		}

		[Test]
		public void Trial0224 ()
		{
			RegexTrial t = PerlTrials.trials [224];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0224", t.Expected, actual);
		}

		[Test]
		public void Trial0225 ()
		{
			RegexTrial t = PerlTrials.trials [225];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0225", t.Expected, actual);
		}

		[Test]
		public void Trial0226 ()
		{
			RegexTrial t = PerlTrials.trials [226];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0226", t.Expected, actual);
		}

		[Test]
		public void Trial0227 ()
		{
			RegexTrial t = PerlTrials.trials [227];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0227", t.Expected, actual);
		}

		[Test]
		public void Trial0228 ()
		{
			RegexTrial t = PerlTrials.trials [228];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0228", t.Expected, actual);
		}

		[Test]
		public void Trial0229 ()
		{
			RegexTrial t = PerlTrials.trials [229];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0229", t.Expected, actual);
		}

		[Test]
		public void Trial0230 ()
		{
			RegexTrial t = PerlTrials.trials [230];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0230", t.Expected, actual);
		}

		[Test]
		public void Trial0231 ()
		{
			RegexTrial t = PerlTrials.trials [231];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0231", t.Expected, actual);
		}

		[Test]
		public void Trial0232 ()
		{
			RegexTrial t = PerlTrials.trials [232];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0232", t.Expected, actual);
		}

		[Test]
		public void Trial0233 ()
		{
			RegexTrial t = PerlTrials.trials [233];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0233", t.Expected, actual);
		}

		[Test]
		public void Trial0234 ()
		{
			RegexTrial t = PerlTrials.trials [234];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0234", t.Expected, actual);
		}

		[Test]
		public void Trial0235 ()
		{
			RegexTrial t = PerlTrials.trials [235];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0235", t.Expected, actual);
		}

		[Test]
		public void Trial0236 ()
		{
			RegexTrial t = PerlTrials.trials [236];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0236", t.Expected, actual);
		}

		[Test]
		public void Trial0237 ()
		{
			RegexTrial t = PerlTrials.trials [237];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0237", t.Expected, actual);
		}

		[Test]
		public void Trial0238 ()
		{
			RegexTrial t = PerlTrials.trials [238];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0238", t.Expected, actual);
		}

		[Test]
		public void Trial0239 ()
		{
			RegexTrial t = PerlTrials.trials [239];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0239", t.Expected, actual);
		}

		[Test]
		public void Trial0240 ()
		{
			RegexTrial t = PerlTrials.trials [240];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0240", t.Expected, actual);
		}

		[Test]
		public void Trial0241 ()
		{
			RegexTrial t = PerlTrials.trials [241];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0241", t.Expected, actual);
		}

		[Test]
		public void Trial0242 ()
		{
			RegexTrial t = PerlTrials.trials [242];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0242", t.Expected, actual);
		}

		[Test]
		public void Trial0243 ()
		{
			RegexTrial t = PerlTrials.trials [243];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0243", t.Expected, actual);
		}

		[Test]
		public void Trial0244 ()
		{
			RegexTrial t = PerlTrials.trials [244];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0244", t.Expected, actual);
		}

		[Test]
		public void Trial0245 ()
		{
			RegexTrial t = PerlTrials.trials [245];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0245", t.Expected, actual);
		}

		[Test]
		public void Trial0246 ()
		{
			RegexTrial t = PerlTrials.trials [246];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0246", t.Expected, actual);
		}

		[Test]
		public void Trial0247 ()
		{
			RegexTrial t = PerlTrials.trials [247];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0247", t.Expected, actual);
		}

		[Test]
		public void Trial0248 ()
		{
			RegexTrial t = PerlTrials.trials [248];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0248", t.Expected, actual);
		}

		[Test]
		public void Trial0249 ()
		{
			RegexTrial t = PerlTrials.trials [249];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0249", t.Expected, actual);
		}

		[Test]
		public void Trial0250 ()
		{
			RegexTrial t = PerlTrials.trials [250];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0250", t.Expected, actual);
		}

		[Test]
		public void Trial0251 ()
		{
			RegexTrial t = PerlTrials.trials [251];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0251", t.Expected, actual);
		}

		[Test]
		public void Trial0252 ()
		{
			RegexTrial t = PerlTrials.trials [252];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0252", t.Expected, actual);
		}

		[Test]
		public void Trial0253 ()
		{
			RegexTrial t = PerlTrials.trials [253];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0253", t.Expected, actual);
		}

		[Test]
		public void Trial0254 ()
		{
			RegexTrial t = PerlTrials.trials [254];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0254", t.Expected, actual);
		}

		[Test]
		public void Trial0255 ()
		{
			RegexTrial t = PerlTrials.trials [255];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0255", t.Expected, actual);
		}

		[Test]
		public void Trial0256 ()
		{
			RegexTrial t = PerlTrials.trials [256];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0256", t.Expected, actual);
		}

		[Test]
		public void Trial0257 ()
		{
			RegexTrial t = PerlTrials.trials [257];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0257", t.Expected, actual);
		}

		[Test]
		public void Trial0258 ()
		{
			RegexTrial t = PerlTrials.trials [258];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0258", t.Expected, actual);
		}

		[Test]
		public void Trial0259 ()
		{
			RegexTrial t = PerlTrials.trials [259];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0259", t.Expected, actual);
		}

		[Test]
		public void Trial0260 ()
		{
			RegexTrial t = PerlTrials.trials [260];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0260", t.Expected, actual);
		}

		[Test]
		public void Trial0261 ()
		{
			RegexTrial t = PerlTrials.trials [261];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0261", t.Expected, actual);
		}

		[Test]
		public void Trial0262 ()
		{
			RegexTrial t = PerlTrials.trials [262];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0262", t.Expected, actual);
		}

		[Test]
		public void Trial0263 ()
		{
			RegexTrial t = PerlTrials.trials [263];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0263", t.Expected, actual);
		}

		[Test]
		public void Trial0264 ()
		{
			RegexTrial t = PerlTrials.trials [264];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0264", t.Expected, actual);
		}

		[Test]
		public void Trial0265 ()
		{
			RegexTrial t = PerlTrials.trials [265];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0265", t.Expected, actual);
		}

		[Test]
		public void Trial0266 ()
		{
			RegexTrial t = PerlTrials.trials [266];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0266", t.Expected, actual);
		}

		[Test]
		public void Trial0267 ()
		{
			RegexTrial t = PerlTrials.trials [267];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0267", t.Expected, actual);
		}

		[Test]
		public void Trial0268 ()
		{
			RegexTrial t = PerlTrials.trials [268];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0268", t.Expected, actual);
		}

		[Test]
		public void Trial0269 ()
		{
			RegexTrial t = PerlTrials.trials [269];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0269", t.Expected, actual);
		}

		[Test]
		public void Trial0270 ()
		{
			RegexTrial t = PerlTrials.trials [270];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0270", t.Expected, actual);
		}

		[Test]
		public void Trial0271 ()
		{
			RegexTrial t = PerlTrials.trials [271];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0271", t.Expected, actual);
		}

		[Test]
		public void Trial0272 ()
		{
			RegexTrial t = PerlTrials.trials [272];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0272", t.Expected, actual);
		}

		[Test]
		public void Trial0273 ()
		{
			RegexTrial t = PerlTrials.trials [273];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0273", t.Expected, actual);
		}

		[Test]
		public void Trial0274 ()
		{
			RegexTrial t = PerlTrials.trials [274];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0274", t.Expected, actual);
		}

		[Test]
		public void Trial0275 ()
		{
			RegexTrial t = PerlTrials.trials [275];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0275", t.Expected, actual);
		}

		[Test]
		public void Trial0276 ()
		{
			RegexTrial t = PerlTrials.trials [276];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0276", t.Expected, actual);
		}

		[Test]
		public void Trial0277 ()
		{
			RegexTrial t = PerlTrials.trials [277];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0277", t.Expected, actual);
		}

		[Test]
		public void Trial0278 ()
		{
			RegexTrial t = PerlTrials.trials [278];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0278", t.Expected, actual);
		}

		[Test]
		public void Trial0279 ()
		{
			RegexTrial t = PerlTrials.trials [279];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0279", t.Expected, actual);
		}

		[Test]
		public void Trial0280 ()
		{
			RegexTrial t = PerlTrials.trials [280];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0280", t.Expected, actual);
		}

		[Test]
		public void Trial0281 ()
		{
			RegexTrial t = PerlTrials.trials [281];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0281", t.Expected, actual);
		}

		[Test]
		public void Trial0282 ()
		{
			RegexTrial t = PerlTrials.trials [282];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0282", t.Expected, actual);
		}

		[Test]
		public void Trial0283 ()
		{
			RegexTrial t = PerlTrials.trials [283];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0283", t.Expected, actual);
		}

		[Test]
		public void Trial0284 ()
		{
			RegexTrial t = PerlTrials.trials [284];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0284", t.Expected, actual);
		}

		[Test]
		public void Trial0285 ()
		{
			RegexTrial t = PerlTrials.trials [285];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0285", t.Expected, actual);
		}

		[Test]
		public void Trial0286 ()
		{
			RegexTrial t = PerlTrials.trials [286];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0286", t.Expected, actual);
		}

		[Test]
		public void Trial0287 ()
		{
			RegexTrial t = PerlTrials.trials [287];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0287", t.Expected, actual);
		}

		[Test]
		public void Trial0288 ()
		{
			RegexTrial t = PerlTrials.trials [288];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0288", t.Expected, actual);
		}

		[Test]
		public void Trial0289 ()
		{
			RegexTrial t = PerlTrials.trials [289];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0289", t.Expected, actual);
		}

		[Test]
		public void Trial0290 ()
		{
			RegexTrial t = PerlTrials.trials [290];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0290", t.Expected, actual);
		}

		[Test]
		public void Trial0291 ()
		{
			RegexTrial t = PerlTrials.trials [291];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0291", t.Expected, actual);
		}

		[Test]
		public void Trial0292 ()
		{
			RegexTrial t = PerlTrials.trials [292];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0292", t.Expected, actual);
		}

		[Test]
		public void Trial0293 ()
		{
			RegexTrial t = PerlTrials.trials [293];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0293", t.Expected, actual);
		}

		[Test]
		public void Trial0294 ()
		{
			RegexTrial t = PerlTrials.trials [294];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0294", t.Expected, actual);
		}

		[Test]
		public void Trial0295 ()
		{
			RegexTrial t = PerlTrials.trials [295];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0295", t.Expected, actual);
		}

		[Test]
		public void Trial0296 ()
		{
			RegexTrial t = PerlTrials.trials [296];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0296", t.Expected, actual);
		}

		[Test]
		public void Trial0297 ()
		{
			RegexTrial t = PerlTrials.trials [297];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0297", t.Expected, actual);
		}

		[Test]
		public void Trial0298 ()
		{
			RegexTrial t = PerlTrials.trials [298];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0298", t.Expected, actual);
		}

		[Test]
		public void Trial0299 ()
		{
			RegexTrial t = PerlTrials.trials [299];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0299", t.Expected, actual);
		}

		[Test]
		public void Trial0300 ()
		{
			RegexTrial t = PerlTrials.trials [300];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0300", t.Expected, actual);
		}

		[Test]
		public void Trial0301 ()
		{
			RegexTrial t = PerlTrials.trials [301];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0301", t.Expected, actual);
		}

		[Test]
		public void Trial0302 ()
		{
			RegexTrial t = PerlTrials.trials [302];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0302", t.Expected, actual);
		}

		[Test]
		public void Trial0303 ()
		{
			RegexTrial t = PerlTrials.trials [303];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0303", t.Expected, actual);
		}

		[Test]
		public void Trial0304 ()
		{
			RegexTrial t = PerlTrials.trials [304];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0304", t.Expected, actual);
		}

		[Test]
		public void Trial0305 ()
		{
			RegexTrial t = PerlTrials.trials [305];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0305", t.Expected, actual);
		}

		[Test]
		public void Trial0306 ()
		{
			RegexTrial t = PerlTrials.trials [306];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0306", t.Expected, actual);
		}

		[Test]
		public void Trial0307 ()
		{
			RegexTrial t = PerlTrials.trials [307];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0307", t.Expected, actual);
		}

		[Test]
		public void Trial0308 ()
		{
			RegexTrial t = PerlTrials.trials [308];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0308", t.Expected, actual);
		}

		[Test]
		public void Trial0309 ()
		{
			RegexTrial t = PerlTrials.trials [309];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0309", t.Expected, actual);
		}

		[Test]
		public void Trial0310 ()
		{
			RegexTrial t = PerlTrials.trials [310];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0310", t.Expected, actual);
		}

		[Test]
		public void Trial0311 ()
		{
			RegexTrial t = PerlTrials.trials [311];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0311", t.Expected, actual);
		}

		[Test]
		public void Trial0312 ()
		{
			RegexTrial t = PerlTrials.trials [312];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0312", t.Expected, actual);
		}

		[Test]
		public void Trial0313 ()
		{
			RegexTrial t = PerlTrials.trials [313];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0313", t.Expected, actual);
		}

		[Test]
		public void Trial0314 ()
		{
			RegexTrial t = PerlTrials.trials [314];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0314", t.Expected, actual);
		}

		[Test]
		public void Trial0315 ()
		{
			RegexTrial t = PerlTrials.trials [315];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0315", t.Expected, actual);
		}

		[Test]
		public void Trial0316 ()
		{
			RegexTrial t = PerlTrials.trials [316];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0316", t.Expected, actual);
		}

		[Test]
		public void Trial0317 ()
		{
			RegexTrial t = PerlTrials.trials [317];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0317", t.Expected, actual);
		}

		[Test]
		public void Trial0318 ()
		{
			RegexTrial t = PerlTrials.trials [318];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0318", t.Expected, actual);
		}

		[Test]
		public void Trial0319 ()
		{
			RegexTrial t = PerlTrials.trials [319];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0319", t.Expected, actual);
		}

		[Test]
		public void Trial0320 ()
		{
			RegexTrial t = PerlTrials.trials [320];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0320", t.Expected, actual);
		}

		[Test]
		public void Trial0321 ()
		{
			RegexTrial t = PerlTrials.trials [321];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0321", t.Expected, actual);
		}

		[Test]
		public void Trial0322 ()
		{
			RegexTrial t = PerlTrials.trials [322];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0322", t.Expected, actual);
		}

		[Test]
		public void Trial0323 ()
		{
			RegexTrial t = PerlTrials.trials [323];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0323", t.Expected, actual);
		}

		[Test]
		public void Trial0324 ()
		{
			RegexTrial t = PerlTrials.trials [324];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0324", t.Expected, actual);
		}

		[Test]
		public void Trial0325 ()
		{
			RegexTrial t = PerlTrials.trials [325];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0325", t.Expected, actual);
		}

		[Test]
		public void Trial0326 ()
		{
			RegexTrial t = PerlTrials.trials [326];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0326", t.Expected, actual);
		}

		[Test]
		public void Trial0327 ()
		{
			RegexTrial t = PerlTrials.trials [327];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0327", t.Expected, actual);
		}

		[Test]
		public void Trial0328 ()
		{
			RegexTrial t = PerlTrials.trials [328];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0328", t.Expected, actual);
		}

		[Test]
		public void Trial0329 ()
		{
			RegexTrial t = PerlTrials.trials [329];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0329", t.Expected, actual);
		}

		[Test]
		public void Trial0330 ()
		{
			RegexTrial t = PerlTrials.trials [330];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0330", t.Expected, actual);
		}

		[Test]
		public void Trial0331 ()
		{
			RegexTrial t = PerlTrials.trials [331];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0331", t.Expected, actual);
		}

		[Test]
		public void Trial0332 ()
		{
			RegexTrial t = PerlTrials.trials [332];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0332", t.Expected, actual);
		}

		[Test]
		public void Trial0333 ()
		{
			RegexTrial t = PerlTrials.trials [333];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0333", t.Expected, actual);
		}

		[Test]
		public void Trial0334 ()
		{
			RegexTrial t = PerlTrials.trials [334];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0334", t.Expected, actual);
		}

		[Test]
		public void Trial0335 ()
		{
			RegexTrial t = PerlTrials.trials [335];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0335", t.Expected, actual);
		}

		[Test]
		public void Trial0336 ()
		{
			RegexTrial t = PerlTrials.trials [336];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0336", t.Expected, actual);
		}

		[Test]
		public void Trial0337 ()
		{
			RegexTrial t = PerlTrials.trials [337];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0337", t.Expected, actual);
		}

		[Test]
		public void Trial0338 ()
		{
			RegexTrial t = PerlTrials.trials [338];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0338", t.Expected, actual);
		}

		[Test]
		public void Trial0339 ()
		{
			RegexTrial t = PerlTrials.trials [339];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0339", t.Expected, actual);
		}

		[Test]
		public void Trial0340 ()
		{
			RegexTrial t = PerlTrials.trials [340];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0340", t.Expected, actual);
		}

		[Test]
		public void Trial0341 ()
		{
			RegexTrial t = PerlTrials.trials [341];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0341", t.Expected, actual);
		}

		[Test]
		public void Trial0342 ()
		{
			RegexTrial t = PerlTrials.trials [342];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0342", t.Expected, actual);
		}

		[Test]
		public void Trial0343 ()
		{
			RegexTrial t = PerlTrials.trials [343];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0343", t.Expected, actual);
		}

		[Test]
		public void Trial0344 ()
		{
			RegexTrial t = PerlTrials.trials [344];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0344", t.Expected, actual);
		}

		[Test]
		public void Trial0345 ()
		{
			RegexTrial t = PerlTrials.trials [345];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0345", t.Expected, actual);
		}

		[Test]
		public void Trial0346 ()
		{
			RegexTrial t = PerlTrials.trials [346];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0346", t.Expected, actual);
		}

		[Test]
		public void Trial0347 ()
		{
			RegexTrial t = PerlTrials.trials [347];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0347", t.Expected, actual);
		}

		[Test]
		public void Trial0348 ()
		{
			RegexTrial t = PerlTrials.trials [348];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0348", t.Expected, actual);
		}

		[Test]
		public void Trial0349 ()
		{
			RegexTrial t = PerlTrials.trials [349];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0349", t.Expected, actual);
		}

		[Test]
		public void Trial0350 ()
		{
			RegexTrial t = PerlTrials.trials [350];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0350", t.Expected, actual);
		}

		[Test]
		public void Trial0351 ()
		{
			RegexTrial t = PerlTrials.trials [351];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0351", t.Expected, actual);
		}

		[Test]
		public void Trial0352 ()
		{
			RegexTrial t = PerlTrials.trials [352];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0352", t.Expected, actual);
		}

		[Test]
		public void Trial0353 ()
		{
			RegexTrial t = PerlTrials.trials [353];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0353", t.Expected, actual);
		}

		[Test]
		public void Trial0354 ()
		{
			RegexTrial t = PerlTrials.trials [354];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0354", t.Expected, actual);
		}

		[Test]
		public void Trial0355 ()
		{
			RegexTrial t = PerlTrials.trials [355];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0355", t.Expected, actual);
		}

		[Test]
		public void Trial0356 ()
		{
			RegexTrial t = PerlTrials.trials [356];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0356", t.Expected, actual);
		}

		[Test]
		public void Trial0357 ()
		{
			RegexTrial t = PerlTrials.trials [357];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0357", t.Expected, actual);
		}

		[Test]
		public void Trial0358 ()
		{
			RegexTrial t = PerlTrials.trials [358];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0358", t.Expected, actual);
		}

		[Test]
		public void Trial0359 ()
		{
			RegexTrial t = PerlTrials.trials [359];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0359", t.Expected, actual);
		}

		[Test]
		public void Trial0360 ()
		{
			RegexTrial t = PerlTrials.trials [360];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0360", t.Expected, actual);
		}

		[Test]
		public void Trial0361 ()
		{
			RegexTrial t = PerlTrials.trials [361];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0361", t.Expected, actual);
		}

		[Test]
		public void Trial0362 ()
		{
			RegexTrial t = PerlTrials.trials [362];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0362", t.Expected, actual);
		}

		[Test]
		public void Trial0363 ()
		{
			RegexTrial t = PerlTrials.trials [363];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0363", t.Expected, actual);
		}

		[Test]
		public void Trial0364 ()
		{
			RegexTrial t = PerlTrials.trials [364];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0364", t.Expected, actual);
		}

		[Test]
		public void Trial0365 ()
		{
			RegexTrial t = PerlTrials.trials [365];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0365", t.Expected, actual);
		}

		[Test]
		public void Trial0366 ()
		{
			RegexTrial t = PerlTrials.trials [366];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0366", t.Expected, actual);
		}

		[Test]
		public void Trial0367 ()
		{
			RegexTrial t = PerlTrials.trials [367];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0367", t.Expected, actual);
		}

		[Test]
		public void Trial0368 ()
		{
			RegexTrial t = PerlTrials.trials [368];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0368", t.Expected, actual);
		}

		[Test]
		public void Trial0369 ()
		{
			RegexTrial t = PerlTrials.trials [369];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0369", t.Expected, actual);
		}

		[Test]
		public void Trial0370 ()
		{
			RegexTrial t = PerlTrials.trials [370];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0370", t.Expected, actual);
		}

		[Test]
		public void Trial0371 ()
		{
			RegexTrial t = PerlTrials.trials [371];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0371", t.Expected, actual);
		}

		[Test]
		public void Trial0372 ()
		{
			RegexTrial t = PerlTrials.trials [372];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0372", t.Expected, actual);
		}

		[Test]
		public void Trial0373 ()
		{
			RegexTrial t = PerlTrials.trials [373];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0373", t.Expected, actual);
		}

		[Test]
		public void Trial0374 ()
		{
			RegexTrial t = PerlTrials.trials [374];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0374", t.Expected, actual);
		}

		[Test]
		public void Trial0375 ()
		{
			RegexTrial t = PerlTrials.trials [375];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0375", t.Expected, actual);
		}

		[Test]
		public void Trial0376 ()
		{
			RegexTrial t = PerlTrials.trials [376];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0376", t.Expected, actual);
		}

		[Test]
		public void Trial0377 ()
		{
			RegexTrial t = PerlTrials.trials [377];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0377", t.Expected, actual);
		}

		[Test]
		public void Trial0378 ()
		{
			RegexTrial t = PerlTrials.trials [378];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0378", t.Expected, actual);
		}

		[Test]
		public void Trial0379 ()
		{
			RegexTrial t = PerlTrials.trials [379];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0379", t.Expected, actual);
		}

		[Test]
		public void Trial0380 ()
		{
			RegexTrial t = PerlTrials.trials [380];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0380", t.Expected, actual);
		}

		[Test]
		public void Trial0381 ()
		{
			RegexTrial t = PerlTrials.trials [381];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0381", t.Expected, actual);
		}

		[Test]
		public void Trial0382 ()
		{
			RegexTrial t = PerlTrials.trials [382];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0382", t.Expected, actual);
		}

		[Test]
		public void Trial0383 ()
		{
			RegexTrial t = PerlTrials.trials [383];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0383", t.Expected, actual);
		}

		[Test]
		public void Trial0384 ()
		{
			RegexTrial t = PerlTrials.trials [384];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0384", t.Expected, actual);
		}

		[Test]
		public void Trial0385 ()
		{
			RegexTrial t = PerlTrials.trials [385];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0385", t.Expected, actual);
		}

		[Test]
		public void Trial0386 ()
		{
			RegexTrial t = PerlTrials.trials [386];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0386", t.Expected, actual);
		}

		[Test]
		public void Trial0387 ()
		{
			RegexTrial t = PerlTrials.trials [387];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0387", t.Expected, actual);
		}

		[Test]
		public void Trial0388 ()
		{
			RegexTrial t = PerlTrials.trials [388];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0388", t.Expected, actual);
		}

		[Test]
		public void Trial0389 ()
		{
			RegexTrial t = PerlTrials.trials [389];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0389", t.Expected, actual);
		}

		[Test]
		public void Trial0390 ()
		{
			RegexTrial t = PerlTrials.trials [390];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0390", t.Expected, actual);
		}

		[Test]
		public void Trial0391 ()
		{
			RegexTrial t = PerlTrials.trials [391];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0391", t.Expected, actual);
		}

		[Test]
		public void Trial0392 ()
		{
			RegexTrial t = PerlTrials.trials [392];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0392", t.Expected, actual);
		}

		[Test]
		public void Trial0393 ()
		{
			RegexTrial t = PerlTrials.trials [393];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0393", t.Expected, actual);
		}

		[Test]
		public void Trial0394 ()
		{
			RegexTrial t = PerlTrials.trials [394];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0394", t.Expected, actual);
		}

		[Test]
		public void Trial0395 ()
		{
			RegexTrial t = PerlTrials.trials [395];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0395", t.Expected, actual);
		}

		[Test]
		public void Trial0396 ()
		{
			RegexTrial t = PerlTrials.trials [396];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0396", t.Expected, actual);
		}

		[Test]
		public void Trial0397 ()
		{
			RegexTrial t = PerlTrials.trials [397];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0397", t.Expected, actual);
		}

		[Test]
		public void Trial0398 ()
		{
			RegexTrial t = PerlTrials.trials [398];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0398", t.Expected, actual);
		}

		[Test]
		public void Trial0399 ()
		{
			RegexTrial t = PerlTrials.trials [399];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0399", t.Expected, actual);
		}

		[Test]
		public void Trial0400 ()
		{
			RegexTrial t = PerlTrials.trials [400];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0400", t.Expected, actual);
		}

		[Test]
		public void Trial0401 ()
		{
			RegexTrial t = PerlTrials.trials [401];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0401", t.Expected, actual);
		}

		[Test]
		public void Trial0402 ()
		{
			RegexTrial t = PerlTrials.trials [402];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0402", t.Expected, actual);
		}

		[Test]
		public void Trial0403 ()
		{
			RegexTrial t = PerlTrials.trials [403];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0403", t.Expected, actual);
		}

		[Test]
		public void Trial0404 ()
		{
			RegexTrial t = PerlTrials.trials [404];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0404", t.Expected, actual);
		}

		[Test]
		public void Trial0405 ()
		{
			RegexTrial t = PerlTrials.trials [405];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0405", t.Expected, actual);
		}

		[Test]
		public void Trial0406 ()
		{
			RegexTrial t = PerlTrials.trials [406];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0406", t.Expected, actual);
		}

		[Test]
		public void Trial0407 ()
		{
			RegexTrial t = PerlTrials.trials [407];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0407", t.Expected, actual);
		}

		[Test]
		public void Trial0408 ()
		{
			RegexTrial t = PerlTrials.trials [408];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0408", t.Expected, actual);
		}

		[Test]
		public void Trial0409 ()
		{
			RegexTrial t = PerlTrials.trials [409];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0409", t.Expected, actual);
		}

		[Test]
		public void Trial0410 ()
		{
			RegexTrial t = PerlTrials.trials [410];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0410", t.Expected, actual);
		}

		[Test]
		public void Trial0411 ()
		{
			RegexTrial t = PerlTrials.trials [411];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0411", t.Expected, actual);
		}

		[Test]
		public void Trial0412 ()
		{
			RegexTrial t = PerlTrials.trials [412];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0412", t.Expected, actual);
		}

		[Test]
		public void Trial0413 ()
		{
			RegexTrial t = PerlTrials.trials [413];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0413", t.Expected, actual);
		}

		[Test]
		public void Trial0414 ()
		{
			RegexTrial t = PerlTrials.trials [414];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0414", t.Expected, actual);
		}

		[Test]
		public void Trial0415 ()
		{
			RegexTrial t = PerlTrials.trials [415];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0415", t.Expected, actual);
		}

		[Test]
		public void Trial0416 ()
		{
			RegexTrial t = PerlTrials.trials [416];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0416", t.Expected, actual);
		}

		[Test]
		public void Trial0417 ()
		{
			RegexTrial t = PerlTrials.trials [417];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0417", t.Expected, actual);
		}

		[Test]
		public void Trial0418 ()
		{
			RegexTrial t = PerlTrials.trials [418];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0418", t.Expected, actual);
		}

		[Test]
		public void Trial0419 ()
		{
			RegexTrial t = PerlTrials.trials [419];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0419", t.Expected, actual);
		}

		[Test]
		public void Trial0420 ()
		{
			RegexTrial t = PerlTrials.trials [420];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0420", t.Expected, actual);
		}

		[Test]
		public void Trial0421 ()
		{
			RegexTrial t = PerlTrials.trials [421];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0421", t.Expected, actual);
		}

		[Test]
		public void Trial0422 ()
		{
			RegexTrial t = PerlTrials.trials [422];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0422", t.Expected, actual);
		}

		[Test]
		public void Trial0423 ()
		{
			RegexTrial t = PerlTrials.trials [423];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0423", t.Expected, actual);
		}

		[Test]
		public void Trial0424 ()
		{
			RegexTrial t = PerlTrials.trials [424];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0424", t.Expected, actual);
		}

		[Test]
		public void Trial0425 ()
		{
			RegexTrial t = PerlTrials.trials [425];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0425", t.Expected, actual);
		}

		[Test]
		public void Trial0426 ()
		{
			RegexTrial t = PerlTrials.trials [426];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0426", t.Expected, actual);
		}

		[Test]
		public void Trial0427 ()
		{
			RegexTrial t = PerlTrials.trials [427];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0427", t.Expected, actual);
		}

		[Test]
		public void Trial0428 ()
		{
			RegexTrial t = PerlTrials.trials [428];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0428", t.Expected, actual);
		}

		[Test]
		public void Trial0429 ()
		{
			RegexTrial t = PerlTrials.trials [429];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0429", t.Expected, actual);
		}

		[Test]
		public void Trial0430 ()
		{
			RegexTrial t = PerlTrials.trials [430];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0430", t.Expected, actual);
		}

		[Test]
		public void Trial0431 ()
		{
			RegexTrial t = PerlTrials.trials [431];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0431", t.Expected, actual);
		}

		[Test]
		public void Trial0432 ()
		{
			RegexTrial t = PerlTrials.trials [432];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0432", t.Expected, actual);
		}

		[Test]
		public void Trial0433 ()
		{
			RegexTrial t = PerlTrials.trials [433];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0433", t.Expected, actual);
		}

		[Test]
		public void Trial0434 ()
		{
			RegexTrial t = PerlTrials.trials [434];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0434", t.Expected, actual);
		}

		[Test]
		public void Trial0435 ()
		{
			RegexTrial t = PerlTrials.trials [435];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0435", t.Expected, actual);
		}

		[Test]
		public void Trial0436 ()
		{
			RegexTrial t = PerlTrials.trials [436];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0436", t.Expected, actual);
		}

		[Test]
		public void Trial0437 ()
		{
			RegexTrial t = PerlTrials.trials [437];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0437", t.Expected, actual);
		}

		[Test]
		public void Trial0438 ()
		{
			RegexTrial t = PerlTrials.trials [438];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0438", t.Expected, actual);
		}

		[Test]
		public void Trial0439 ()
		{
			RegexTrial t = PerlTrials.trials [439];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0439", t.Expected, actual);
		}

		[Test]
		public void Trial0440 ()
		{
			RegexTrial t = PerlTrials.trials [440];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0440", t.Expected, actual);
		}

		[Test]
		public void Trial0441 ()
		{
			RegexTrial t = PerlTrials.trials [441];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0441", t.Expected, actual);
		}

		[Test]
		public void Trial0442 ()
		{
			RegexTrial t = PerlTrials.trials [442];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0442", t.Expected, actual);
		}

		[Test]
		public void Trial0443 ()
		{
			RegexTrial t = PerlTrials.trials [443];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0443", t.Expected, actual);
		}

		[Test]
		public void Trial0444 ()
		{
			RegexTrial t = PerlTrials.trials [444];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0444", t.Expected, actual);
		}

		[Test]
		public void Trial0445 ()
		{
			RegexTrial t = PerlTrials.trials [445];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0445", t.Expected, actual);
		}

		[Test]
		public void Trial0446 ()
		{
			RegexTrial t = PerlTrials.trials [446];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0446", t.Expected, actual);
		}

		[Test]
		public void Trial0447 ()
		{
			RegexTrial t = PerlTrials.trials [447];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0447", t.Expected, actual);
		}

		[Test]
		public void Trial0448 ()
		{
			RegexTrial t = PerlTrials.trials [448];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0448", t.Expected, actual);
		}

		[Test]
		public void Trial0449 ()
		{
			RegexTrial t = PerlTrials.trials [449];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0449", t.Expected, actual);
		}

		[Test]
		public void Trial0450 ()
		{
			RegexTrial t = PerlTrials.trials [450];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0450", t.Expected, actual);
		}

		[Test]
		public void Trial0451 ()
		{
			RegexTrial t = PerlTrials.trials [451];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0451", t.Expected, actual);
		}

		[Test]
		public void Trial0452 ()
		{
			RegexTrial t = PerlTrials.trials [452];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0452", t.Expected, actual);
		}

		[Test]
		public void Trial0453 ()
		{
			RegexTrial t = PerlTrials.trials [453];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0453", t.Expected, actual);
		}

		[Test]
		public void Trial0454 ()
		{
			RegexTrial t = PerlTrials.trials [454];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0454", t.Expected, actual);
		}

		[Test]
		public void Trial0455 ()
		{
			RegexTrial t = PerlTrials.trials [455];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0455", t.Expected, actual);
		}

		[Test]
		public void Trial0456 ()
		{
			RegexTrial t = PerlTrials.trials [456];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0456", t.Expected, actual);
		}

		[Test]
		public void Trial0457 ()
		{
			RegexTrial t = PerlTrials.trials [457];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0457", t.Expected, actual);
		}

		[Test]
		public void Trial0458 ()
		{
			RegexTrial t = PerlTrials.trials [458];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0458", t.Expected, actual);
		}

		[Test]
		public void Trial0459 ()
		{
			RegexTrial t = PerlTrials.trials [459];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0459", t.Expected, actual);
		}

		[Test]
		public void Trial0460 ()
		{
			RegexTrial t = PerlTrials.trials [460];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0460", t.Expected, actual);
		}

		[Test]
		public void Trial0461 ()
		{
			RegexTrial t = PerlTrials.trials [461];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0461", t.Expected, actual);
		}

		[Test]
		public void Trial0462 ()
		{
			RegexTrial t = PerlTrials.trials [462];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0462", t.Expected, actual);
		}

		[Test]
		public void Trial0463 ()
		{
			RegexTrial t = PerlTrials.trials [463];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0463", t.Expected, actual);
		}

		[Test]
		public void Trial0464 ()
		{
			RegexTrial t = PerlTrials.trials [464];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0464", t.Expected, actual);
		}

		[Test]
		public void Trial0465 ()
		{
			RegexTrial t = PerlTrials.trials [465];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0465", t.Expected, actual);
		}

		[Test]
		public void Trial0466 ()
		{
			RegexTrial t = PerlTrials.trials [466];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0466", t.Expected, actual);
		}

		[Test]
		public void Trial0467 ()
		{
			RegexTrial t = PerlTrials.trials [467];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0467", t.Expected, actual);
		}

		[Test]
		public void Trial0468 ()
		{
			RegexTrial t = PerlTrials.trials [468];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0468", t.Expected, actual);
		}

		[Test]
		public void Trial0469 ()
		{
			RegexTrial t = PerlTrials.trials [469];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0469", t.Expected, actual);
		}

		[Test]
		public void Trial0470 ()
		{
			RegexTrial t = PerlTrials.trials [470];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0470", t.Expected, actual);
		}

		[Test]
		public void Trial0471 ()
		{
			RegexTrial t = PerlTrials.trials [471];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0471", t.Expected, actual);
		}

		[Test]
		public void Trial0472 ()
		{
			RegexTrial t = PerlTrials.trials [472];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0472", t.Expected, actual);
		}

		[Test]
		public void Trial0473 ()
		{
			RegexTrial t = PerlTrials.trials [473];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0473", t.Expected, actual);
		}

		[Test]
		public void Trial0474 ()
		{
			RegexTrial t = PerlTrials.trials [474];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0474", t.Expected, actual);
		}

		[Test]
		public void Trial0475 ()
		{
			RegexTrial t = PerlTrials.trials [475];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0475", t.Expected, actual);
		}

		[Test]
		public void Trial0476 ()
		{
			RegexTrial t = PerlTrials.trials [476];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0476", t.Expected, actual);
		}

		[Test]
		public void Trial0477 ()
		{
			RegexTrial t = PerlTrials.trials [477];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0477", t.Expected, actual);
		}

		[Test]
		public void Trial0478 ()
		{
			RegexTrial t = PerlTrials.trials [478];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0478", t.Expected, actual);
		}

		[Test]
		public void Trial0479 ()
		{
			RegexTrial t = PerlTrials.trials [479];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0479", t.Expected, actual);
		}

		[Test]
		public void Trial0480 ()
		{
			RegexTrial t = PerlTrials.trials [480];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0480", t.Expected, actual);
		}

		[Test]
		public void Trial0481 ()
		{
			RegexTrial t = PerlTrials.trials [481];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0481", t.Expected, actual);
		}

		[Test]
		public void Trial0482 ()
		{
			RegexTrial t = PerlTrials.trials [482];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0482", t.Expected, actual);
		}

		[Test]
		public void Trial0483 ()
		{
			RegexTrial t = PerlTrials.trials [483];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0483", t.Expected, actual);
		}

		[Test]
		public void Trial0484 ()
		{
			RegexTrial t = PerlTrials.trials [484];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0484", t.Expected, actual);
		}

		[Test]
		public void Trial0485 ()
		{
			RegexTrial t = PerlTrials.trials [485];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0485", t.Expected, actual);
		}

		[Test]
		public void Trial0486 ()
		{
			RegexTrial t = PerlTrials.trials [486];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0486", t.Expected, actual);
		}

		[Test]
		public void Trial0487 ()
		{
			RegexTrial t = PerlTrials.trials [487];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0487", t.Expected, actual);
		}

		[Test]
		public void Trial0488 ()
		{
			RegexTrial t = PerlTrials.trials [488];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0488", t.Expected, actual);
		}

		[Test]
		public void Trial0489 ()
		{
			RegexTrial t = PerlTrials.trials [489];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0489", t.Expected, actual);
		}

		[Test]
		public void Trial0490 ()
		{
			RegexTrial t = PerlTrials.trials [490];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0490", t.Expected, actual);
		}

		[Test]
		public void Trial0491 ()
		{
			RegexTrial t = PerlTrials.trials [491];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0491", t.Expected, actual);
		}

		[Test]
		public void Trial0492 ()
		{
			RegexTrial t = PerlTrials.trials [492];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0492", t.Expected, actual);
		}

		[Test]
		public void Trial0493 ()
		{
			RegexTrial t = PerlTrials.trials [493];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0493", t.Expected, actual);
		}

		[Test]
		public void Trial0494 ()
		{
			RegexTrial t = PerlTrials.trials [494];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0494", t.Expected, actual);
		}

		[Test]
		public void Trial0495 ()
		{
			RegexTrial t = PerlTrials.trials [495];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0495", t.Expected, actual);
		}

		[Test]
		public void Trial0496 ()
		{
			RegexTrial t = PerlTrials.trials [496];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0496", t.Expected, actual);
		}

		[Test]
		public void Trial0497 ()
		{
			RegexTrial t = PerlTrials.trials [497];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0497", t.Expected, actual);
		}

		[Test]
		public void Trial0498 ()
		{
			RegexTrial t = PerlTrials.trials [498];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0498", t.Expected, actual);
		}

		[Test]
		public void Trial0499 ()
		{
			RegexTrial t = PerlTrials.trials [499];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0499", t.Expected, actual);
		}

		[Test]
		public void Trial0500 ()
		{
			RegexTrial t = PerlTrials.trials [500];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0500", t.Expected, actual);
		}

		[Test]
		public void Trial0501 ()
		{
			RegexTrial t = PerlTrials.trials [501];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0501", t.Expected, actual);
		}

		[Test]
		public void Trial0502 ()
		{
			RegexTrial t = PerlTrials.trials [502];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0502", t.Expected, actual);
		}

		[Test]
		public void Trial0503 ()
		{
			RegexTrial t = PerlTrials.trials [503];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0503", t.Expected, actual);
		}

		[Test]
		public void Trial0504 ()
		{
			RegexTrial t = PerlTrials.trials [504];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0504", t.Expected, actual);
		}

		[Test]
		public void Trial0505 ()
		{
			RegexTrial t = PerlTrials.trials [505];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0505", t.Expected, actual);
		}

		[Test]
		public void Trial0506 ()
		{
			RegexTrial t = PerlTrials.trials [506];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0506", t.Expected, actual);
		}

		[Test]
		public void Trial0507 ()
		{
			RegexTrial t = PerlTrials.trials [507];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0507", t.Expected, actual);
		}

		[Test]
		public void Trial0508 ()
		{
			RegexTrial t = PerlTrials.trials [508];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0508", t.Expected, actual);
		}

		[Test]
		public void Trial0509 ()
		{
			RegexTrial t = PerlTrials.trials [509];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0509", t.Expected, actual);
		}

		[Test]
		public void Trial0510 ()
		{
			RegexTrial t = PerlTrials.trials [510];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0510", t.Expected, actual);
		}

		[Test]
		public void Trial0511 ()
		{
			RegexTrial t = PerlTrials.trials [511];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0511", t.Expected, actual);
		}

		[Test]
		public void Trial0512 ()
		{
			RegexTrial t = PerlTrials.trials [512];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0512", t.Expected, actual);
		}

		[Test]
		public void Trial0513 ()
		{
			RegexTrial t = PerlTrials.trials [513];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0513", t.Expected, actual);
		}

		[Test]
		public void Trial0514 ()
		{
			RegexTrial t = PerlTrials.trials [514];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0514", t.Expected, actual);
		}

		[Test]
		public void Trial0515 ()
		{
			RegexTrial t = PerlTrials.trials [515];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0515", t.Expected, actual);
		}

		[Test]
		public void Trial0516 ()
		{
			RegexTrial t = PerlTrials.trials [516];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0516", t.Expected, actual);
		}

		[Test]
		public void Trial0517 ()
		{
			RegexTrial t = PerlTrials.trials [517];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0517", t.Expected, actual);
		}

		[Test]
		public void Trial0518 ()
		{
			RegexTrial t = PerlTrials.trials [518];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0518", t.Expected, actual);
		}

		[Test]
		public void Trial0519 ()
		{
			RegexTrial t = PerlTrials.trials [519];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0519", t.Expected, actual);
		}

		[Test]
		public void Trial0520 ()
		{
			RegexTrial t = PerlTrials.trials [520];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0520", t.Expected, actual);
		}

		[Test]
		public void Trial0521 ()
		{
			RegexTrial t = PerlTrials.trials [521];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0521", t.Expected, actual);
		}

		[Test]
		public void Trial0522 ()
		{
			RegexTrial t = PerlTrials.trials [522];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0522", t.Expected, actual);
		}

		[Test]
		public void Trial0523 ()
		{
			RegexTrial t = PerlTrials.trials [523];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0523", t.Expected, actual);
		}

		[Test]
		public void Trial0524 ()
		{
			RegexTrial t = PerlTrials.trials [524];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0524", t.Expected, actual);
		}

		[Test]
		public void Trial0525 ()
		{
			RegexTrial t = PerlTrials.trials [525];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0525", t.Expected, actual);
		}

		[Test]
		public void Trial0526 ()
		{
			RegexTrial t = PerlTrials.trials [526];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0526", t.Expected, actual);
		}

		[Test]
		public void Trial0527 ()
		{
			RegexTrial t = PerlTrials.trials [527];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0527", t.Expected, actual);
		}

		[Test]
		public void Trial0528 ()
		{
			RegexTrial t = PerlTrials.trials [528];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0528", t.Expected, actual);
		}

		[Test]
		public void Trial0529 ()
		{
			RegexTrial t = PerlTrials.trials [529];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0529", t.Expected, actual);
		}

		[Test]
		public void Trial0530 ()
		{
			RegexTrial t = PerlTrials.trials [530];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0530", t.Expected, actual);
		}

		[Test]
		public void Trial0531 ()
		{
			RegexTrial t = PerlTrials.trials [531];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0531", t.Expected, actual);
		}

		[Test]
		public void Trial0532 ()
		{
			RegexTrial t = PerlTrials.trials [532];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0532", t.Expected, actual);
		}

		[Test]
		public void Trial0533 ()
		{
			RegexTrial t = PerlTrials.trials [533];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0533", t.Expected, actual);
		}

		[Test]
		public void Trial0534 ()
		{
			RegexTrial t = PerlTrials.trials [534];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0534", t.Expected, actual);
		}

		[Test]
		public void Trial0535 ()
		{
			RegexTrial t = PerlTrials.trials [535];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0535", t.Expected, actual);
		}

		[Test]
		public void Trial0536 ()
		{
			RegexTrial t = PerlTrials.trials [536];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0536", t.Expected, actual);
		}

		[Test]
		public void Trial0537 ()
		{
			RegexTrial t = PerlTrials.trials [537];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0537", t.Expected, actual);
		}

		[Test]
		public void Trial0538 ()
		{
			RegexTrial t = PerlTrials.trials [538];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0538", t.Expected, actual);
		}

		[Test]
		public void Trial0539 ()
		{
			RegexTrial t = PerlTrials.trials [539];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0539", t.Expected, actual);
		}

		[Test]
		public void Trial0540 ()
		{
			RegexTrial t = PerlTrials.trials [540];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0540", t.Expected, actual);
		}

		[Test]
		public void Trial0541 ()
		{
			RegexTrial t = PerlTrials.trials [541];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0541", t.Expected, actual);
		}

		[Test]
		public void Trial0542 ()
		{
			RegexTrial t = PerlTrials.trials [542];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0542", t.Expected, actual);
		}

		[Test]
		public void Trial0543 ()
		{
			RegexTrial t = PerlTrials.trials [543];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0543", t.Expected, actual);
		}

		[Test]
		public void Trial0544 ()
		{
			RegexTrial t = PerlTrials.trials [544];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0544", t.Expected, actual);
		}

		[Test]
		public void Trial0545 ()
		{
			RegexTrial t = PerlTrials.trials [545];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0545", t.Expected, actual);
		}

		[Test]
		public void Trial0546 ()
		{
			RegexTrial t = PerlTrials.trials [546];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0546", t.Expected, actual);
		}

		[Test]
		public void Trial0547 ()
		{
			RegexTrial t = PerlTrials.trials [547];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0547", t.Expected, actual);
		}

		[Test]
		public void Trial0548 ()
		{
			RegexTrial t = PerlTrials.trials [548];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0548", t.Expected, actual);
		}

		[Test]
		public void Trial0549 ()
		{
			RegexTrial t = PerlTrials.trials [549];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0549", t.Expected, actual);
		}

		[Test]
		public void Trial0550 ()
		{
			RegexTrial t = PerlTrials.trials [550];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0550", t.Expected, actual);
		}

		[Test]
		public void Trial0551 ()
		{
			RegexTrial t = PerlTrials.trials [551];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0551", t.Expected, actual);
		}

		[Test]
		public void Trial0552 ()
		{
			RegexTrial t = PerlTrials.trials [552];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0552", t.Expected, actual);
		}

		[Test]
		public void Trial0553 ()
		{
			RegexTrial t = PerlTrials.trials [553];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0553", t.Expected, actual);
		}

		[Test]
		public void Trial0554 ()
		{
			RegexTrial t = PerlTrials.trials [554];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0554", t.Expected, actual);
		}

		[Test]
		public void Trial0555 ()
		{
			RegexTrial t = PerlTrials.trials [555];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0555", t.Expected, actual);
		}

		[Test]
		public void Trial0556 ()
		{
			RegexTrial t = PerlTrials.trials [556];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0556", t.Expected, actual);
		}

		[Test]
		public void Trial0557 ()
		{
			RegexTrial t = PerlTrials.trials [557];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0557", t.Expected, actual);
		}

		[Test]
		public void Trial0558 ()
		{
			RegexTrial t = PerlTrials.trials [558];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0558", t.Expected, actual);
		}

		[Test]
		public void Trial0559 ()
		{
			RegexTrial t = PerlTrials.trials [559];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0559", t.Expected, actual);
		}

		[Test]
		public void Trial0560 ()
		{
			RegexTrial t = PerlTrials.trials [560];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0560", t.Expected, actual);
		}

		[Test]
		public void Trial0561 ()
		{
			RegexTrial t = PerlTrials.trials [561];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0561", t.Expected, actual);
		}

		[Test]
		public void Trial0562 ()
		{
			RegexTrial t = PerlTrials.trials [562];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0562", t.Expected, actual);
		}

		[Test]
		public void Trial0563 ()
		{
			RegexTrial t = PerlTrials.trials [563];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0563", t.Expected, actual);
		}

		[Test]
		public void Trial0564 ()
		{
			RegexTrial t = PerlTrials.trials [564];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0564", t.Expected, actual);
		}

		[Test]
		public void Trial0565 ()
		{
			RegexTrial t = PerlTrials.trials [565];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0565", t.Expected, actual);
		}

		[Test]
		public void Trial0566 ()
		{
			RegexTrial t = PerlTrials.trials [566];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0566", t.Expected, actual);
		}

		[Test]
		public void Trial0567 ()
		{
			RegexTrial t = PerlTrials.trials [567];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0567", t.Expected, actual);
		}

		[Test]
		public void Trial0568 ()
		{
			RegexTrial t = PerlTrials.trials [568];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0568", t.Expected, actual);
		}

		[Test]
		public void Trial0569 ()
		{
			RegexTrial t = PerlTrials.trials [569];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0569", t.Expected, actual);
		}

		[Test]
		public void Trial0570 ()
		{
			RegexTrial t = PerlTrials.trials [570];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0570", t.Expected, actual);
		}

		[Test]
		public void Trial0571 ()
		{
			RegexTrial t = PerlTrials.trials [571];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0571", t.Expected, actual);
		}

		[Test]
		public void Trial0572 ()
		{
			RegexTrial t = PerlTrials.trials [572];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0572", t.Expected, actual);
		}

		[Test]
		public void Trial0573 ()
		{
			RegexTrial t = PerlTrials.trials [573];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0573", t.Expected, actual);
		}

		[Test]
		public void Trial0574 ()
		{
			RegexTrial t = PerlTrials.trials [574];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0574", t.Expected, actual);
		}

		[Test]
		public void Trial0575 ()
		{
			RegexTrial t = PerlTrials.trials [575];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0575", t.Expected, actual);
		}

		[Test]
		public void Trial0576 ()
		{
			RegexTrial t = PerlTrials.trials [576];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0576", t.Expected, actual);
		}

		[Test]
		public void Trial0577 ()
		{
			RegexTrial t = PerlTrials.trials [577];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0577", t.Expected, actual);
		}

		[Test]
		public void Trial0578 ()
		{
			RegexTrial t = PerlTrials.trials [578];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0578", t.Expected, actual);
		}

		[Test]
		public void Trial0579 ()
		{
			RegexTrial t = PerlTrials.trials [579];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0579", t.Expected, actual);
		}

		[Test]
		public void Trial0580 ()
		{
			RegexTrial t = PerlTrials.trials [580];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0580", t.Expected, actual);
		}

		[Test]
		public void Trial0581 ()
		{
			RegexTrial t = PerlTrials.trials [581];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0581", t.Expected, actual);
		}

		[Test]
		public void Trial0582 ()
		{
			RegexTrial t = PerlTrials.trials [582];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0582", t.Expected, actual);
		}

		[Test]
		public void Trial0583 ()
		{
			RegexTrial t = PerlTrials.trials [583];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0583", t.Expected, actual);
		}

		[Test]
		public void Trial0584 ()
		{
			RegexTrial t = PerlTrials.trials [584];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0584", t.Expected, actual);
		}

		[Test]
		public void Trial0585 ()
		{
			RegexTrial t = PerlTrials.trials [585];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0585", t.Expected, actual);
		}

		[Test]
		public void Trial0586 ()
		{
			RegexTrial t = PerlTrials.trials [586];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0586", t.Expected, actual);
		}

		[Test]
		public void Trial0587 ()
		{
			RegexTrial t = PerlTrials.trials [587];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0587", t.Expected, actual);
		}

		[Test]
		public void Trial0588 ()
		{
			RegexTrial t = PerlTrials.trials [588];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0588", t.Expected, actual);
		}

		[Test]
		public void Trial0589 ()
		{
			RegexTrial t = PerlTrials.trials [589];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0589", t.Expected, actual);
		}

		[Test]
		public void Trial0590 ()
		{
			RegexTrial t = PerlTrials.trials [590];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0590", t.Expected, actual);
		}

		[Test]
		public void Trial0591 ()
		{
			RegexTrial t = PerlTrials.trials [591];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0591", t.Expected, actual);
		}

		[Test]
		public void Trial0592 ()
		{
			RegexTrial t = PerlTrials.trials [592];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0592", t.Expected, actual);
		}

		[Test]
		public void Trial0593 ()
		{
			RegexTrial t = PerlTrials.trials [593];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0593", t.Expected, actual);
		}

		[Test]
		public void Trial0594 ()
		{
			RegexTrial t = PerlTrials.trials [594];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0594", t.Expected, actual);
		}

		[Test]
		public void Trial0595 ()
		{
			RegexTrial t = PerlTrials.trials [595];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0595", t.Expected, actual);
		}

		[Test]
		public void Trial0596 ()
		{
			RegexTrial t = PerlTrials.trials [596];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0596", t.Expected, actual);
		}

		[Test]
		public void Trial0597 ()
		{
			RegexTrial t = PerlTrials.trials [597];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0597", t.Expected, actual);
		}

		[Test]
		public void Trial0598 ()
		{
			RegexTrial t = PerlTrials.trials [598];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0598", t.Expected, actual);
		}

		[Test]
		public void Trial0599 ()
		{
			RegexTrial t = PerlTrials.trials [599];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0599", t.Expected, actual);
		}

		[Test]
		public void Trial0600 ()
		{
			RegexTrial t = PerlTrials.trials [600];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0600", t.Expected, actual);
		}

		[Test]
		public void Trial0601 ()
		{
			RegexTrial t = PerlTrials.trials [601];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0601", t.Expected, actual);
		}

		[Test]
		public void Trial0602 ()
		{
			RegexTrial t = PerlTrials.trials [602];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0602", t.Expected, actual);
		}

		[Test]
		public void Trial0603 ()
		{
			RegexTrial t = PerlTrials.trials [603];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0603", t.Expected, actual);
		}

		[Test]
		public void Trial0604 ()
		{
			RegexTrial t = PerlTrials.trials [604];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0604", t.Expected, actual);
		}

		[Test]
		public void Trial0605 ()
		{
			RegexTrial t = PerlTrials.trials [605];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0605", t.Expected, actual);
		}

		[Test]
		public void Trial0606 ()
		{
			RegexTrial t = PerlTrials.trials [606];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0606", t.Expected, actual);
		}

		[Test]
		public void Trial0607 ()
		{
			RegexTrial t = PerlTrials.trials [607];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0607", t.Expected, actual);
		}

		[Test]
		public void Trial0608 ()
		{
			RegexTrial t = PerlTrials.trials [608];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0608", t.Expected, actual);
		}

		[Test]
		public void Trial0609 ()
		{
			RegexTrial t = PerlTrials.trials [609];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0609", t.Expected, actual);
		}

		[Test]
		public void Trial0610 ()
		{
			RegexTrial t = PerlTrials.trials [610];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0610", t.Expected, actual);
		}

		[Test]
		public void Trial0611 ()
		{
			RegexTrial t = PerlTrials.trials [611];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0611", t.Expected, actual);
		}

		[Test]
		public void Trial0612 ()
		{
			RegexTrial t = PerlTrials.trials [612];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0612", t.Expected, actual);
		}

		[Test]
		public void Trial0613 ()
		{
			RegexTrial t = PerlTrials.trials [613];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0613", t.Expected, actual);
		}

		[Test]
		public void Trial0614 ()
		{
			RegexTrial t = PerlTrials.trials [614];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0614", t.Expected, actual);
		}

		[Test]
		public void Trial0615 ()
		{
			RegexTrial t = PerlTrials.trials [615];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0615", t.Expected, actual);
		}

		[Test]
		public void Trial0616 ()
		{
			RegexTrial t = PerlTrials.trials [616];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0616", t.Expected, actual);
		}

		[Test]
		public void Trial0617 ()
		{
			RegexTrial t = PerlTrials.trials [617];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0617", t.Expected, actual);
		}

		[Test]
		public void Trial0618 ()
		{
			RegexTrial t = PerlTrials.trials [618];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0618", t.Expected, actual);
		}

		[Test]
		public void Trial0619 ()
		{
			RegexTrial t = PerlTrials.trials [619];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0619", t.Expected, actual);
		}

		[Test]
		public void Trial0620 ()
		{
			RegexTrial t = PerlTrials.trials [620];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0620", t.Expected, actual);
		}

		[Test]
		public void Trial0621 ()
		{
			RegexTrial t = PerlTrials.trials [621];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0621", t.Expected, actual);
		}

		[Test]
		public void Trial0622 ()
		{
			RegexTrial t = PerlTrials.trials [622];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0622", t.Expected, actual);
		}

		[Test]
		public void Trial0623 ()
		{
			RegexTrial t = PerlTrials.trials [623];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0623", t.Expected, actual);
		}

		[Test]
		public void Trial0624 ()
		{
			RegexTrial t = PerlTrials.trials [624];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0624", t.Expected, actual);
		}

		[Test]
		public void Trial0625 ()
		{
			RegexTrial t = PerlTrials.trials [625];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0625", t.Expected, actual);
		}

		[Test]
		public void Trial0626 ()
		{
			RegexTrial t = PerlTrials.trials [626];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0626", t.Expected, actual);
		}

		[Test]
		public void Trial0627 ()
		{
			RegexTrial t = PerlTrials.trials [627];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0627", t.Expected, actual);
		}

		[Test]
		public void Trial0628 ()
		{
			RegexTrial t = PerlTrials.trials [628];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0628", t.Expected, actual);
		}

		[Test]
		public void Trial0629 ()
		{
			RegexTrial t = PerlTrials.trials [629];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0629", t.Expected, actual);
		}

		[Test]
		public void Trial0630 ()
		{
			RegexTrial t = PerlTrials.trials [630];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0630", t.Expected, actual);
		}

		[Test]
		public void Trial0631 ()
		{
			RegexTrial t = PerlTrials.trials [631];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0631", t.Expected, actual);
		}

		[Test]
		public void Trial0632 ()
		{
			RegexTrial t = PerlTrials.trials [632];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0632", t.Expected, actual);
		}

		[Test]
		public void Trial0633 ()
		{
			RegexTrial t = PerlTrials.trials [633];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0633", t.Expected, actual);
		}

		[Test]
		public void Trial0634 ()
		{
			RegexTrial t = PerlTrials.trials [634];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0634", t.Expected, actual);
		}

		[Test]
		public void Trial0635 ()
		{
			RegexTrial t = PerlTrials.trials [635];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0635", t.Expected, actual);
		}

		[Test]
		public void Trial0636 ()
		{
			RegexTrial t = PerlTrials.trials [636];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0636", t.Expected, actual);
		}

		[Test]
		public void Trial0637 ()
		{
			RegexTrial t = PerlTrials.trials [637];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0637", t.Expected, actual);
		}

		[Test]
		public void Trial0638 ()
		{
			RegexTrial t = PerlTrials.trials [638];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0638", t.Expected, actual);
		}

		[Test]
		public void Trial0639 ()
		{
			RegexTrial t = PerlTrials.trials [639];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0639", t.Expected, actual);
		}

		[Test]
		public void Trial0640 ()
		{
			RegexTrial t = PerlTrials.trials [640];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0640", t.Expected, actual);
		}

		[Test]
		public void Trial0641 ()
		{
			RegexTrial t = PerlTrials.trials [641];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0641", t.Expected, actual);
		}

		[Test]
		public void Trial0642 ()
		{
			RegexTrial t = PerlTrials.trials [642];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0642", t.Expected, actual);
		}

		[Test]
		public void Trial0643 ()
		{
			RegexTrial t = PerlTrials.trials [643];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0643", t.Expected, actual);
		}

		[Test]
		public void Trial0644 ()
		{
			RegexTrial t = PerlTrials.trials [644];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0644", t.Expected, actual);
		}

		[Test]
		public void Trial0645 ()
		{
			RegexTrial t = PerlTrials.trials [645];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0645", t.Expected, actual);
		}

		[Test]
		public void Trial0646 ()
		{
			RegexTrial t = PerlTrials.trials [646];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0646", t.Expected, actual);
		}

		[Test]
		public void Trial0647 ()
		{
			RegexTrial t = PerlTrials.trials [647];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0647", t.Expected, actual);
		}

		[Test]
		public void Trial0648 ()
		{
			RegexTrial t = PerlTrials.trials [648];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0648", t.Expected, actual);
		}

		[Test]
		public void Trial0649 ()
		{
			RegexTrial t = PerlTrials.trials [649];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0649", t.Expected, actual);
		}

		[Test]
		public void Trial0650 ()
		{
			RegexTrial t = PerlTrials.trials [650];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0650", t.Expected, actual);
		}

		[Test]
		public void Trial0651 ()
		{
			RegexTrial t = PerlTrials.trials [651];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0651", t.Expected, actual);
		}

		[Test]
		public void Trial0652 ()
		{
			RegexTrial t = PerlTrials.trials [652];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0652", t.Expected, actual);
		}

		[Test]
		public void Trial0653 ()
		{
			RegexTrial t = PerlTrials.trials [653];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0653", t.Expected, actual);
		}

		[Test]
		public void Trial0654 ()
		{
			RegexTrial t = PerlTrials.trials [654];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0654", t.Expected, actual);
		}

		[Test]
		public void Trial0655 ()
		{
			RegexTrial t = PerlTrials.trials [655];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0655", t.Expected, actual);
		}

		[Test]
		public void Trial0656 ()
		{
			RegexTrial t = PerlTrials.trials [656];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0656", t.Expected, actual);
		}

		[Test]
		public void Trial0657 ()
		{
			RegexTrial t = PerlTrials.trials [657];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0657", t.Expected, actual);
		}

		[Test]
		public void Trial0658 ()
		{
			RegexTrial t = PerlTrials.trials [658];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0658", t.Expected, actual);
		}

		[Test]
		public void Trial0659 ()
		{
			RegexTrial t = PerlTrials.trials [659];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0659", t.Expected, actual);
		}

		[Test]
		public void Trial0660 ()
		{
			RegexTrial t = PerlTrials.trials [660];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0660", t.Expected, actual);
		}

		[Test]
		public void Trial0661 ()
		{
			RegexTrial t = PerlTrials.trials [661];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0661", t.Expected, actual);
		}

		[Test]
		public void Trial0662 ()
		{
			RegexTrial t = PerlTrials.trials [662];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0662", t.Expected, actual);
		}

		[Test]
		public void Trial0663 ()
		{
			RegexTrial t = PerlTrials.trials [663];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0663", t.Expected, actual);
		}

		[Test]
		public void Trial0664 ()
		{
			RegexTrial t = PerlTrials.trials [664];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0664", t.Expected, actual);
		}

		[Test]
		public void Trial0665 ()
		{
			RegexTrial t = PerlTrials.trials [665];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0665", t.Expected, actual);
		}

		[Test]
		public void Trial0666 ()
		{
			RegexTrial t = PerlTrials.trials [666];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0666", t.Expected, actual);
		}

		[Test]
		public void Trial0667 ()
		{
			RegexTrial t = PerlTrials.trials [667];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0667", t.Expected, actual);
		}

		[Test]
		public void Trial0668 ()
		{
			RegexTrial t = PerlTrials.trials [668];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0668", t.Expected, actual);
		}

		[Test]
		public void Trial0669 ()
		{
			RegexTrial t = PerlTrials.trials [669];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0669", t.Expected, actual);
		}

		[Test]
		public void Trial0670 ()
		{
			RegexTrial t = PerlTrials.trials [670];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0670", t.Expected, actual);
		}

		[Test]
		public void Trial0671 ()
		{
			RegexTrial t = PerlTrials.trials [671];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0671", t.Expected, actual);
		}

		[Test]
		public void Trial0672 ()
		{
			RegexTrial t = PerlTrials.trials [672];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0672", t.Expected, actual);
		}

		[Test]
		public void Trial0673 ()
		{
			RegexTrial t = PerlTrials.trials [673];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0673", t.Expected, actual);
		}

		[Test]
		public void Trial0674 ()
		{
			RegexTrial t = PerlTrials.trials [674];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0674", t.Expected, actual);
		}

		[Test]
		public void Trial0675 ()
		{
			RegexTrial t = PerlTrials.trials [675];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0675", t.Expected, actual);
		}

		[Test]
		public void Trial0676 ()
		{
			RegexTrial t = PerlTrials.trials [676];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0676", t.Expected, actual);
		}

		[Test]
		public void Trial0677 ()
		{
			RegexTrial t = PerlTrials.trials [677];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0677", t.Expected, actual);
		}

		[Test]
		public void Trial0678 ()
		{
			RegexTrial t = PerlTrials.trials [678];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0678", t.Expected, actual);
		}

		[Test]
		public void Trial0679 ()
		{
			RegexTrial t = PerlTrials.trials [679];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0679", t.Expected, actual);
		}

		[Test]
		public void Trial0680 ()
		{
			RegexTrial t = PerlTrials.trials [680];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0680", t.Expected, actual);
		}

		[Test]
		public void Trial0681 ()
		{
			RegexTrial t = PerlTrials.trials [681];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0681", t.Expected, actual);
		}

		[Test]
		public void Trial0682 ()
		{
			RegexTrial t = PerlTrials.trials [682];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0682", t.Expected, actual);
		}

		[Test]
		public void Trial0683 ()
		{
			RegexTrial t = PerlTrials.trials [683];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0683", t.Expected, actual);
		}

		[Test]
		public void Trial0684 ()
		{
			RegexTrial t = PerlTrials.trials [684];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0684", t.Expected, actual);
		}

		[Test]
		public void Trial0685 ()
		{
			RegexTrial t = PerlTrials.trials [685];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0685", t.Expected, actual);
		}

		[Test]
		public void Trial0686 ()
		{
			RegexTrial t = PerlTrials.trials [686];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0686", t.Expected, actual);
		}

		[Test]
		public void Trial0687 ()
		{
			RegexTrial t = PerlTrials.trials [687];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0687", t.Expected, actual);
		}

		[Test]
		public void Trial0688 ()
		{
			RegexTrial t = PerlTrials.trials [688];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0688", t.Expected, actual);
		}

		[Test]
		public void Trial0689 ()
		{
			RegexTrial t = PerlTrials.trials [689];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0689", t.Expected, actual);
		}

		[Test]
		public void Trial0690 ()
		{
			RegexTrial t = PerlTrials.trials [690];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0690", t.Expected, actual);
		}

		[Test]
		public void Trial0691 ()
		{
			RegexTrial t = PerlTrials.trials [691];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0691", t.Expected, actual);
		}

		[Test]
		public void Trial0692 ()
		{
			RegexTrial t = PerlTrials.trials [692];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0692", t.Expected, actual);
		}

		[Test]
		public void Trial0693 ()
		{
			RegexTrial t = PerlTrials.trials [693];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0693", t.Expected, actual);
		}

		[Test]
		public void Trial0694 ()
		{
			RegexTrial t = PerlTrials.trials [694];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0694", t.Expected, actual);
		}

		[Test]
		public void Trial0695 ()
		{
			RegexTrial t = PerlTrials.trials [695];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0695", t.Expected, actual);
		}

		[Test]
		public void Trial0696 ()
		{
			RegexTrial t = PerlTrials.trials [696];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0696", t.Expected, actual);
		}

		[Test]
		public void Trial0697 ()
		{
			RegexTrial t = PerlTrials.trials [697];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0697", t.Expected, actual);
		}

		[Test]
		public void Trial0698 ()
		{
			RegexTrial t = PerlTrials.trials [698];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0698", t.Expected, actual);
		}

		[Test]
		public void Trial0699 ()
		{
			RegexTrial t = PerlTrials.trials [699];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0699", t.Expected, actual);
		}

		[Test]
		public void Trial0700 ()
		{
			RegexTrial t = PerlTrials.trials [700];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0700", t.Expected, actual);
		}

		[Test]
		public void Trial0701 ()
		{
			RegexTrial t = PerlTrials.trials [701];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0701", t.Expected, actual);
		}

		[Test]
		public void Trial0702 ()
		{
			RegexTrial t = PerlTrials.trials [702];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0702", t.Expected, actual);
		}

		[Test]
		public void Trial0703 ()
		{
			RegexTrial t = PerlTrials.trials [703];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0703", t.Expected, actual);
		}

		[Test]
		public void Trial0704 ()
		{
			RegexTrial t = PerlTrials.trials [704];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0704", t.Expected, actual);
		}

		[Test]
		public void Trial0705 ()
		{
			RegexTrial t = PerlTrials.trials [705];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0705", t.Expected, actual);
		}

		[Test]
		public void Trial0706 ()
		{
			RegexTrial t = PerlTrials.trials [706];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0706", t.Expected, actual);
		}

		[Test]
		public void Trial0707 ()
		{
			RegexTrial t = PerlTrials.trials [707];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0707", t.Expected, actual);
		}

		[Test]
		public void Trial0708 ()
		{
			RegexTrial t = PerlTrials.trials [708];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0708", t.Expected, actual);
		}

		[Test]
		public void Trial0709 ()
		{
			RegexTrial t = PerlTrials.trials [709];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0709", t.Expected, actual);
		}

		[Test]
		public void Trial0710 ()
		{
			RegexTrial t = PerlTrials.trials [710];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0710", t.Expected, actual);
		}

		[Test]
		public void Trial0711 ()
		{
			RegexTrial t = PerlTrials.trials [711];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0711", t.Expected, actual);
		}

		[Test]
		public void Trial0712 ()
		{
			RegexTrial t = PerlTrials.trials [712];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0712", t.Expected, actual);
		}

		[Test]
		public void Trial0713 ()
		{
			RegexTrial t = PerlTrials.trials [713];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0713", t.Expected, actual);
		}

		[Test]
		public void Trial0714 ()
		{
			RegexTrial t = PerlTrials.trials [714];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0714", t.Expected, actual);
		}

		[Test]
		public void Trial0715 ()
		{
			RegexTrial t = PerlTrials.trials [715];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0715", t.Expected, actual);
		}

		[Test]
		public void Trial0716 ()
		{
			RegexTrial t = PerlTrials.trials [716];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0716", t.Expected, actual);
		}

		[Test]
		public void Trial0717 ()
		{
			RegexTrial t = PerlTrials.trials [717];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0717", t.Expected, actual);
		}

		[Test]
		public void Trial0718 ()
		{
			RegexTrial t = PerlTrials.trials [718];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0718", t.Expected, actual);
		}

		[Test]
		public void Trial0719 ()
		{
			RegexTrial t = PerlTrials.trials [719];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0719", t.Expected, actual);
		}

		[Test]
		public void Trial0720 ()
		{
			RegexTrial t = PerlTrials.trials [720];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0720", t.Expected, actual);
		}

		[Test]
		public void Trial0721 ()
		{
			RegexTrial t = PerlTrials.trials [721];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0721", t.Expected, actual);
		}

		[Test]
		public void Trial0722 ()
		{
			RegexTrial t = PerlTrials.trials [722];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0722", t.Expected, actual);
		}

		[Test]
		public void Trial0723 ()
		{
			RegexTrial t = PerlTrials.trials [723];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0723", t.Expected, actual);
		}

		[Test]
		public void Trial0724 ()
		{
			RegexTrial t = PerlTrials.trials [724];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0724", t.Expected, actual);
		}

		[Test]
		public void Trial0725 ()
		{
			RegexTrial t = PerlTrials.trials [725];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0725", t.Expected, actual);
		}

		[Test]
		public void Trial0726 ()
		{
			RegexTrial t = PerlTrials.trials [726];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0726", t.Expected, actual);
		}

		[Test]
		public void Trial0727 ()
		{
			RegexTrial t = PerlTrials.trials [727];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0727", t.Expected, actual);
		}

		[Test]
		public void Trial0728 ()
		{
			RegexTrial t = PerlTrials.trials [728];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0728", t.Expected, actual);
		}

		[Test]
		public void Trial0729 ()
		{
			RegexTrial t = PerlTrials.trials [729];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0729", t.Expected, actual);
		}

		[Test]
		public void Trial0730 ()
		{
			RegexTrial t = PerlTrials.trials [730];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0730", t.Expected, actual);
		}

		[Test]
		public void Trial0731 ()
		{
			RegexTrial t = PerlTrials.trials [731];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0731", t.Expected, actual);
		}

		[Test]
		public void Trial0732 ()
		{
			RegexTrial t = PerlTrials.trials [732];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0732", t.Expected, actual);
		}

		[Test]
		public void Trial0733 ()
		{
			RegexTrial t = PerlTrials.trials [733];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0733", t.Expected, actual);
		}

		[Test]
		public void Trial0734 ()
		{
			RegexTrial t = PerlTrials.trials [734];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0734", t.Expected, actual);
		}

		[Test]
		public void Trial0735 ()
		{
			RegexTrial t = PerlTrials.trials [735];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0735", t.Expected, actual);
		}

		[Test]
		public void Trial0736 ()
		{
			RegexTrial t = PerlTrials.trials [736];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0736", t.Expected, actual);
		}

		[Test]
		public void Trial0737 ()
		{
			RegexTrial t = PerlTrials.trials [737];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0737", t.Expected, actual);
		}

		[Test]
		public void Trial0738 ()
		{
			RegexTrial t = PerlTrials.trials [738];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0738", t.Expected, actual);
		}

		[Test]
		public void Trial0739 ()
		{
			RegexTrial t = PerlTrials.trials [739];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0739", t.Expected, actual);
		}

		[Test]
		public void Trial0740 ()
		{
			RegexTrial t = PerlTrials.trials [740];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0740", t.Expected, actual);
		}

		[Test]
		public void Trial0741 ()
		{
			RegexTrial t = PerlTrials.trials [741];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0741", t.Expected, actual);
		}

		[Test]
		public void Trial0742 ()
		{
			RegexTrial t = PerlTrials.trials [742];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0742", t.Expected, actual);
		}

		[Test]
		public void Trial0743 ()
		{
			RegexTrial t = PerlTrials.trials [743];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0743", t.Expected, actual);
		}

		[Test]
		public void Trial0744 ()
		{
			RegexTrial t = PerlTrials.trials [744];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0744", t.Expected, actual);
		}

		[Test]
		public void Trial0745 ()
		{
			RegexTrial t = PerlTrials.trials [745];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0745", t.Expected, actual);
		}

		[Test]
		public void Trial0746 ()
		{
			RegexTrial t = PerlTrials.trials [746];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0746", t.Expected, actual);
		}

		[Test]
		public void Trial0747 ()
		{
			RegexTrial t = PerlTrials.trials [747];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0747", t.Expected, actual);
		}

		[Test]
		public void Trial0748 ()
		{
			RegexTrial t = PerlTrials.trials [748];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0748", t.Expected, actual);
		}

		[Test]
		public void Trial0749 ()
		{
			RegexTrial t = PerlTrials.trials [749];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0749", t.Expected, actual);
		}

		[Test]
		public void Trial0750 ()
		{
			RegexTrial t = PerlTrials.trials [750];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0750", t.Expected, actual);
		}

		[Test]
		public void Trial0751 ()
		{
			RegexTrial t = PerlTrials.trials [751];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0751", t.Expected, actual);
		}

		[Test]
		public void Trial0752 ()
		{
			RegexTrial t = PerlTrials.trials [752];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0752", t.Expected, actual);
		}

		[Test]
		public void Trial0753 ()
		{
			RegexTrial t = PerlTrials.trials [753];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0753", t.Expected, actual);
		}

		[Test]
		public void Trial0754 ()
		{
			RegexTrial t = PerlTrials.trials [754];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0754", t.Expected, actual);
		}

		[Test]
		public void Trial0755 ()
		{
			RegexTrial t = PerlTrials.trials [755];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0755", t.Expected, actual);
		}

		[Test]
		public void Trial0756 ()
		{
			RegexTrial t = PerlTrials.trials [756];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0756", t.Expected, actual);
		}

		[Test]
		public void Trial0757 ()
		{
			RegexTrial t = PerlTrials.trials [757];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0757", t.Expected, actual);
		}

		[Test]
		public void Trial0758 ()
		{
			RegexTrial t = PerlTrials.trials [758];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0758", t.Expected, actual);
		}

		[Test]
		public void Trial0759 ()
		{
			RegexTrial t = PerlTrials.trials [759];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0759", t.Expected, actual);
		}

		[Test]
		public void Trial0760 ()
		{
			RegexTrial t = PerlTrials.trials [760];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0760", t.Expected, actual);
		}

		[Test]
		public void Trial0761 ()
		{
			RegexTrial t = PerlTrials.trials [761];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0761", t.Expected, actual);
		}

		[Test]
		public void Trial0762 ()
		{
			RegexTrial t = PerlTrials.trials [762];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0762", t.Expected, actual);
		}

		[Test]
		public void Trial0763 ()
		{
			RegexTrial t = PerlTrials.trials [763];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0763", t.Expected, actual);
		}

		[Test]
		public void Trial0764 ()
		{
			RegexTrial t = PerlTrials.trials [764];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0764", t.Expected, actual);
		}

		[Test]
		public void Trial0765 ()
		{
			RegexTrial t = PerlTrials.trials [765];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0765", t.Expected, actual);
		}

		[Test]
		public void Trial0766 ()
		{
			RegexTrial t = PerlTrials.trials [766];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0766", t.Expected, actual);
		}

		[Test]
		public void Trial0767 ()
		{
			RegexTrial t = PerlTrials.trials [767];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0767", t.Expected, actual);
		}

		[Test]
		public void Trial0768 ()
		{
			RegexTrial t = PerlTrials.trials [768];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0768", t.Expected, actual);
		}

		[Test]
		public void Trial0769 ()
		{
			RegexTrial t = PerlTrials.trials [769];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0769", t.Expected, actual);
		}

		[Test]
		public void Trial0770 ()
		{
			RegexTrial t = PerlTrials.trials [770];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0770", t.Expected, actual);
		}

		[Test]
		public void Trial0771 ()
		{
			RegexTrial t = PerlTrials.trials [771];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0771", t.Expected, actual);
		}

		[Test]
		public void Trial0772 ()
		{
			RegexTrial t = PerlTrials.trials [772];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0772", t.Expected, actual);
		}

		[Test]
		public void Trial0773 ()
		{
			RegexTrial t = PerlTrials.trials [773];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0773", t.Expected, actual);
		}

		[Test]
		public void Trial0774 ()
		{
			RegexTrial t = PerlTrials.trials [774];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0774", t.Expected, actual);
		}

		[Test]
		public void Trial0775 ()
		{
			RegexTrial t = PerlTrials.trials [775];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0775", t.Expected, actual);
		}

		[Test]
		public void Trial0776 ()
		{
			RegexTrial t = PerlTrials.trials [776];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0776", t.Expected, actual);
		}

		[Test]
		public void Trial0777 ()
		{
			RegexTrial t = PerlTrials.trials [777];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0777", t.Expected, actual);
		}

		[Test]
		public void Trial0778 ()
		{
			RegexTrial t = PerlTrials.trials [778];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0778", t.Expected, actual);
		}

		[Test]
		public void Trial0779 ()
		{
			RegexTrial t = PerlTrials.trials [779];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0779", t.Expected, actual);
		}

		[Test]
		public void Trial0780 ()
		{
			RegexTrial t = PerlTrials.trials [780];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0780", t.Expected, actual);
		}

		[Test]
		public void Trial0781 ()
		{
			RegexTrial t = PerlTrials.trials [781];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0781", t.Expected, actual);
		}

		[Test]
		public void Trial0782 ()
		{
			RegexTrial t = PerlTrials.trials [782];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0782", t.Expected, actual);
		}

		[Test]
		public void Trial0783 ()
		{
			RegexTrial t = PerlTrials.trials [783];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0783", t.Expected, actual);
		}

		[Test]
		public void Trial0784 ()
		{
			RegexTrial t = PerlTrials.trials [784];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0784", t.Expected, actual);
		}

		[Test]
		public void Trial0785 ()
		{
			RegexTrial t = PerlTrials.trials [785];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0785", t.Expected, actual);
		}

		[Test]
		public void Trial0786 ()
		{
			RegexTrial t = PerlTrials.trials [786];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0786", t.Expected, actual);
		}

		[Test]
		public void Trial0787 ()
		{
			RegexTrial t = PerlTrials.trials [787];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0787", t.Expected, actual);
		}

		[Test]
		public void Trial0788 ()
		{
			RegexTrial t = PerlTrials.trials [788];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0788", t.Expected, actual);
		}

		[Test]
		public void Trial0789 ()
		{
			RegexTrial t = PerlTrials.trials [789];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0789", t.Expected, actual);
		}

		[Test]
		public void Trial0790 ()
		{
			RegexTrial t = PerlTrials.trials [790];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0790", t.Expected, actual);
		}

		[Test]
		public void Trial0791 ()
		{
			RegexTrial t = PerlTrials.trials [791];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0791", t.Expected, actual);
		}

		[Test]
		public void Trial0792 ()
		{
			RegexTrial t = PerlTrials.trials [792];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0792", t.Expected, actual);
		}

		[Test]
		public void Trial0793 ()
		{
			RegexTrial t = PerlTrials.trials [793];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0793", t.Expected, actual);
		}

		[Test]
		public void Trial0794 ()
		{
			RegexTrial t = PerlTrials.trials [794];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0794", t.Expected, actual);
		}

		[Test]
		public void Trial0795 ()
		{
			RegexTrial t = PerlTrials.trials [795];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0795", t.Expected, actual);
		}

		[Test]
		public void Trial0796 ()
		{
			RegexTrial t = PerlTrials.trials [796];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0796", t.Expected, actual);
		}

		[Test]
		public void Trial0797 ()
		{
			RegexTrial t = PerlTrials.trials [797];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0797", t.Expected, actual);
		}

		[Test]
		public void Trial0798 ()
		{
			RegexTrial t = PerlTrials.trials [798];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0798", t.Expected, actual);
		}

		[Test]
		public void Trial0799 ()
		{
			RegexTrial t = PerlTrials.trials [799];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0799", t.Expected, actual);
		}

		[Test]
		public void Trial0800 ()
		{
			RegexTrial t = PerlTrials.trials [800];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0800", t.Expected, actual);
		}

		[Test]
		public void Trial0801 ()
		{
			RegexTrial t = PerlTrials.trials [801];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0801", t.Expected, actual);
		}

		[Test]
		public void Trial0802 ()
		{
			RegexTrial t = PerlTrials.trials [802];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0802", t.Expected, actual);
		}

		[Test]
		public void Trial0803 ()
		{
			RegexTrial t = PerlTrials.trials [803];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0803", t.Expected, actual);
		}

		[Test]
		public void Trial0804 ()
		{
			RegexTrial t = PerlTrials.trials [804];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0804", t.Expected, actual);
		}

		[Test]
		public void Trial0805 ()
		{
			RegexTrial t = PerlTrials.trials [805];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0805", t.Expected, actual);
		}

		[Test]
		public void Trial0806 ()
		{
			RegexTrial t = PerlTrials.trials [806];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0806", t.Expected, actual);
		}

		[Test]
		public void Trial0807 ()
		{
			RegexTrial t = PerlTrials.trials [807];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0807", t.Expected, actual);
		}

		[Test]
		public void Trial0808 ()
		{
			RegexTrial t = PerlTrials.trials [808];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0808", t.Expected, actual);
		}

		[Test]
		public void Trial0809 ()
		{
			RegexTrial t = PerlTrials.trials [809];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0809", t.Expected, actual);
		}

		[Test]
		public void Trial0810 ()
		{
			RegexTrial t = PerlTrials.trials [810];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0810", t.Expected, actual);
		}

		[Test]
		public void Trial0811 ()
		{
			RegexTrial t = PerlTrials.trials [811];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0811", t.Expected, actual);
		}

		[Test]
		public void Trial0812 ()
		{
			RegexTrial t = PerlTrials.trials [812];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0812", t.Expected, actual);
		}

		[Test]
		public void Trial0813 ()
		{
			RegexTrial t = PerlTrials.trials [813];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0813", t.Expected, actual);
		}

		[Test]
		public void Trial0814 ()
		{
			RegexTrial t = PerlTrials.trials [814];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0814", t.Expected, actual);
		}

		[Test]
		public void Trial0815 ()
		{
			RegexTrial t = PerlTrials.trials [815];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0815", t.Expected, actual);
		}

		[Test]
		public void Trial0816 ()
		{
			RegexTrial t = PerlTrials.trials [816];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0816", t.Expected, actual);
		}

		[Test]
		public void Trial0817 ()
		{
			RegexTrial t = PerlTrials.trials [817];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0817", t.Expected, actual);
		}

		[Test]
		public void Trial0818 ()
		{
			RegexTrial t = PerlTrials.trials [818];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0818", t.Expected, actual);
		}

		[Test]
		public void Trial0819 ()
		{
			RegexTrial t = PerlTrials.trials [819];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0819", t.Expected, actual);
		}

		[Test]
		public void Trial0820 ()
		{
			RegexTrial t = PerlTrials.trials [820];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0820", t.Expected, actual);
		}

		[Test]
		public void Trial0821 ()
		{
			RegexTrial t = PerlTrials.trials [821];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0821", t.Expected, actual);
		}

		[Test]
		public void Trial0822 ()
		{
			RegexTrial t = PerlTrials.trials [822];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0822", t.Expected, actual);
		}

		[Test]
		public void Trial0823 ()
		{
			RegexTrial t = PerlTrials.trials [823];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0823", t.Expected, actual);
		}

		[Test]
		public void Trial0824 ()
		{
			RegexTrial t = PerlTrials.trials [824];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0824", t.Expected, actual);
		}

		[Test]
		public void Trial0825 ()
		{
			RegexTrial t = PerlTrials.trials [825];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0825", t.Expected, actual);
		}

		[Test]
		public void Trial0826 ()
		{
			RegexTrial t = PerlTrials.trials [826];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0826", t.Expected, actual);
		}

		[Test]
		public void Trial0827 ()
		{
			RegexTrial t = PerlTrials.trials [827];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0827", t.Expected, actual);
		}

		[Test]
		public void Trial0828 ()
		{
			RegexTrial t = PerlTrials.trials [828];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0828", t.Expected, actual);
		}

		[Test]
		public void Trial0829 ()
		{
			RegexTrial t = PerlTrials.trials [829];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0829", t.Expected, actual);
		}

		[Test]
		public void Trial0830 ()
		{
			RegexTrial t = PerlTrials.trials [830];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0830", t.Expected, actual);
		}

		[Test]
		public void Trial0831 ()
		{
			RegexTrial t = PerlTrials.trials [831];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0831", t.Expected, actual);
		}

		[Test]
		public void Trial0832 ()
		{
			RegexTrial t = PerlTrials.trials [832];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0832", t.Expected, actual);
		}

		[Test]
		public void Trial0833 ()
		{
			RegexTrial t = PerlTrials.trials [833];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0833", t.Expected, actual);
		}

		[Test]
		public void Trial0834 ()
		{
			RegexTrial t = PerlTrials.trials [834];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0834", t.Expected, actual);
		}

		[Test]
		public void Trial0835 ()
		{
			RegexTrial t = PerlTrials.trials [835];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0835", t.Expected, actual);
		}

		[Test]
		public void Trial0836 ()
		{
			RegexTrial t = PerlTrials.trials [836];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0836", t.Expected, actual);
		}

		[Test]
		public void Trial0837 ()
		{
			RegexTrial t = PerlTrials.trials [837];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0837", t.Expected, actual);
		}

		[Test]
		public void Trial0838 ()
		{
			RegexTrial t = PerlTrials.trials [838];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0838", t.Expected, actual);
		}

		[Test]
		public void Trial0839 ()
		{
			RegexTrial t = PerlTrials.trials [839];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0839", t.Expected, actual);
		}

		[Test]
		public void Trial0840 ()
		{
			RegexTrial t = PerlTrials.trials [840];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0840", t.Expected, actual);
		}

		[Test]
		public void Trial0841 ()
		{
			RegexTrial t = PerlTrials.trials [841];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0841", t.Expected, actual);
		}

		[Test]
		public void Trial0842 ()
		{
			RegexTrial t = PerlTrials.trials [842];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0842", t.Expected, actual);
		}

		[Test]
		public void Trial0843 ()
		{
			RegexTrial t = PerlTrials.trials [843];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0843", t.Expected, actual);
		}

		[Test]
		public void Trial0844 ()
		{
			RegexTrial t = PerlTrials.trials [844];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0844", t.Expected, actual);
		}

		[Test]
		public void Trial0845 ()
		{
			RegexTrial t = PerlTrials.trials [845];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0845", t.Expected, actual);
		}

		[Test]
		public void Trial0846 ()
		{
			RegexTrial t = PerlTrials.trials [846];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0846", t.Expected, actual);
		}

		[Test]
		public void Trial0847 ()
		{
			RegexTrial t = PerlTrials.trials [847];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0847", t.Expected, actual);
		}

		[Test]
		public void Trial0848 ()
		{
			RegexTrial t = PerlTrials.trials [848];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0848", t.Expected, actual);
		}

		[Test]
		public void Trial0849 ()
		{
			RegexTrial t = PerlTrials.trials [849];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0849", t.Expected, actual);
		}

		[Test]
		public void Trial0850 ()
		{
			RegexTrial t = PerlTrials.trials [850];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0850", t.Expected, actual);
		}

		[Test]
		public void Trial0851 ()
		{
			RegexTrial t = PerlTrials.trials [851];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0851", t.Expected, actual);
		}

		[Test]
		public void Trial0852 ()
		{
			RegexTrial t = PerlTrials.trials [852];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0852", t.Expected, actual);
		}

		[Test]
		public void Trial0853 ()
		{
			RegexTrial t = PerlTrials.trials [853];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0853", t.Expected, actual);
		}

		[Test]
		public void Trial0854 ()
		{
			RegexTrial t = PerlTrials.trials [854];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0854", t.Expected, actual);
		}

		[Test]
		public void Trial0855 ()
		{
			RegexTrial t = PerlTrials.trials [855];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0855", t.Expected, actual);
		}

		[Test]
		public void Trial0856 ()
		{
			RegexTrial t = PerlTrials.trials [856];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0856", t.Expected, actual);
		}

		[Test]
		public void Trial0857 ()
		{
			RegexTrial t = PerlTrials.trials [857];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0857", t.Expected, actual);
		}

		[Test]
		public void Trial0858 ()
		{
			RegexTrial t = PerlTrials.trials [858];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0858", t.Expected, actual);
		}

		[Test]
		public void Trial0859 ()
		{
			RegexTrial t = PerlTrials.trials [859];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0859", t.Expected, actual);
		}

		[Test]
		public void Trial0860 ()
		{
			RegexTrial t = PerlTrials.trials [860];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0860", t.Expected, actual);
		}

		[Test]
		public void Trial0861 ()
		{
			RegexTrial t = PerlTrials.trials [861];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0861", t.Expected, actual);
		}

		[Test]
		public void Trial0862 ()
		{
			RegexTrial t = PerlTrials.trials [862];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0862", t.Expected, actual);
		}

		[Test]
		public void Trial0863 ()
		{
			RegexTrial t = PerlTrials.trials [863];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0863", t.Expected, actual);
		}

		[Test]
		public void Trial0864 ()
		{
			RegexTrial t = PerlTrials.trials [864];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0864", t.Expected, actual);
		}

		[Test]
		public void Trial0865 ()
		{
			RegexTrial t = PerlTrials.trials [865];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0865", t.Expected, actual);
		}

		[Test]
		public void Trial0866 ()
		{
			RegexTrial t = PerlTrials.trials [866];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0866", t.Expected, actual);
		}

		[Test]
		public void Trial0867 ()
		{
			RegexTrial t = PerlTrials.trials [867];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0867", t.Expected, actual);
		}

		[Test]
		public void Trial0868 ()
		{
			RegexTrial t = PerlTrials.trials [868];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0868", t.Expected, actual);
		}

		[Test]
		public void Trial0869 ()
		{
			RegexTrial t = PerlTrials.trials [869];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0869", t.Expected, actual);
		}

		[Test]
		public void Trial0870 ()
		{
			RegexTrial t = PerlTrials.trials [870];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0870", t.Expected, actual);
		}

		[Test]
		public void Trial0871 ()
		{
			RegexTrial t = PerlTrials.trials [871];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0871", t.Expected, actual);
		}

		[Test]
		public void Trial0872 ()
		{
			RegexTrial t = PerlTrials.trials [872];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0872", t.Expected, actual);
		}

		[Test]
		public void Trial0873 ()
		{
			RegexTrial t = PerlTrials.trials [873];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0873", t.Expected, actual);
		}

		[Test]
		public void Trial0874 ()
		{
			RegexTrial t = PerlTrials.trials [874];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0874", t.Expected, actual);
		}

		[Test]
		public void Trial0875 ()
		{
			RegexTrial t = PerlTrials.trials [875];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0875", t.Expected, actual);
		}

		[Test]
		public void Trial0876 ()
		{
			RegexTrial t = PerlTrials.trials [876];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0876", t.Expected, actual);
		}

		[Test]
		public void Trial0877 ()
		{
			RegexTrial t = PerlTrials.trials [877];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0877", t.Expected, actual);
		}

		[Test]
		public void Trial0878 ()
		{
			RegexTrial t = PerlTrials.trials [878];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0878", t.Expected, actual);
		}

		[Test]
		public void Trial0879 ()
		{
			RegexTrial t = PerlTrials.trials [879];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0879", t.Expected, actual);
		}

		[Test]
		public void Trial0880 ()
		{
			RegexTrial t = PerlTrials.trials [880];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0880", t.Expected, actual);
		}

		[Test]
		public void Trial0881 ()
		{
			RegexTrial t = PerlTrials.trials [881];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0881", t.Expected, actual);
		}

		[Test]
		public void Trial0882 ()
		{
			RegexTrial t = PerlTrials.trials [882];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0882", t.Expected, actual);
		}

		[Test]
		public void Trial0883 ()
		{
			RegexTrial t = PerlTrials.trials [883];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0883", t.Expected, actual);
		}

		[Test]
		public void Trial0884 ()
		{
			RegexTrial t = PerlTrials.trials [884];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0884", t.Expected, actual);
		}

		[Test]
		public void Trial0885 ()
		{
			RegexTrial t = PerlTrials.trials [885];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0885", t.Expected, actual);
		}

		[Test]
		public void Trial0886 ()
		{
			RegexTrial t = PerlTrials.trials [886];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0886", t.Expected, actual);
		}

		[Test]
		public void Trial0887 ()
		{
			RegexTrial t = PerlTrials.trials [887];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0887", t.Expected, actual);
		}

		[Test]
		public void Trial0888 ()
		{
			RegexTrial t = PerlTrials.trials [888];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0888", t.Expected, actual);
		}

		[Test]
		public void Trial0889 ()
		{
			RegexTrial t = PerlTrials.trials [889];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0889", t.Expected, actual);
		}

		[Test]
		public void Trial0890 ()
		{
			RegexTrial t = PerlTrials.trials [890];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0890", t.Expected, actual);
		}

		[Test]
		public void Trial0891 ()
		{
			RegexTrial t = PerlTrials.trials [891];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0891", t.Expected, actual);
		}

		[Test]
		public void Trial0892 ()
		{
			RegexTrial t = PerlTrials.trials [892];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0892", t.Expected, actual);
		}

		[Test]
		public void Trial0893 ()
		{
			RegexTrial t = PerlTrials.trials [893];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0893", t.Expected, actual);
		}

		[Test]
		public void Trial0894 ()
		{
			RegexTrial t = PerlTrials.trials [894];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0894", t.Expected, actual);
		}

		[Test]
		public void Trial0895 ()
		{
			RegexTrial t = PerlTrials.trials [895];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0895", t.Expected, actual);
		}

		[Test]
		public void Trial0896 ()
		{
			RegexTrial t = PerlTrials.trials [896];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0896", t.Expected, actual);
		}

		[Test]
		public void Trial0897 ()
		{
			RegexTrial t = PerlTrials.trials [897];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0897", t.Expected, actual);
		}

		[Test]
		public void Trial0898 ()
		{
			RegexTrial t = PerlTrials.trials [898];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0898", t.Expected, actual);
		}

		[Test]
		public void Trial0899 ()
		{
			RegexTrial t = PerlTrials.trials [899];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0899", t.Expected, actual);
		}

		[Test]
		public void Trial0900 ()
		{
			RegexTrial t = PerlTrials.trials [900];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0900", t.Expected, actual);
		}

		[Test]
		public void Trial0901 ()
		{
			RegexTrial t = PerlTrials.trials [901];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0901", t.Expected, actual);
		}

		[Test]
		public void Trial0902 ()
		{
			RegexTrial t = PerlTrials.trials [902];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0902", t.Expected, actual);
		}

		[Test]
		public void Trial0903 ()
		{
			RegexTrial t = PerlTrials.trials [903];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0903", t.Expected, actual);
		}

		[Test]
		public void Trial0904 ()
		{
			RegexTrial t = PerlTrials.trials [904];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0904", t.Expected, actual);
		}

		[Test]
		public void Trial0905 ()
		{
			RegexTrial t = PerlTrials.trials [905];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0905", t.Expected, actual);
		}

		[Test]
		public void Trial0906 ()
		{
			RegexTrial t = PerlTrials.trials [906];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0906", t.Expected, actual);
		}

		[Test]
		public void Trial0907 ()
		{
			RegexTrial t = PerlTrials.trials [907];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0907", t.Expected, actual);
		}

		[Test]
		public void Trial0908 ()
		{
			RegexTrial t = PerlTrials.trials [908];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0908", t.Expected, actual);
		}

		[Test]
		public void Trial0909 ()
		{
			RegexTrial t = PerlTrials.trials [909];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0909", t.Expected, actual);
		}

		[Test]
		public void Trial0910 ()
		{
			RegexTrial t = PerlTrials.trials [910];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0910", t.Expected, actual);
		}

		[Test]
		public void Trial0911 ()
		{
			RegexTrial t = PerlTrials.trials [911];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0911", t.Expected, actual);
		}

		[Test]
		public void Trial0912 ()
		{
			RegexTrial t = PerlTrials.trials [912];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0912", t.Expected, actual);
		}

		[Test]
		public void Trial0913 ()
		{
			RegexTrial t = PerlTrials.trials [913];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0913", t.Expected, actual);
		}

		[Test]
		public void Trial0914 ()
		{
			RegexTrial t = PerlTrials.trials [914];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0914", t.Expected, actual);
		}

		[Test]
		public void Trial0915 ()
		{
			RegexTrial t = PerlTrials.trials [915];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0915", t.Expected, actual);
		}

		[Test]
		public void Trial0916 ()
		{
			RegexTrial t = PerlTrials.trials [916];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0916", t.Expected, actual);
		}

		[Test]
		public void Trial0917 ()
		{
			RegexTrial t = PerlTrials.trials [917];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0917", t.Expected, actual);
		}

		[Test]
		public void Trial0918 ()
		{
			RegexTrial t = PerlTrials.trials [918];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0918", t.Expected, actual);
		}

		[Test]
		public void Trial0919 ()
		{
			RegexTrial t = PerlTrials.trials [919];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0919", t.Expected, actual);
		}

		[Test]
		public void Trial0920 ()
		{
			RegexTrial t = PerlTrials.trials [920];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0920", t.Expected, actual);
		}

		[Test]
		public void Trial0921 ()
		{
			RegexTrial t = PerlTrials.trials [921];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0921", t.Expected, actual);
		}

		[Test]
		public void Trial0922 ()
		{
			RegexTrial t = PerlTrials.trials [922];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0922", t.Expected, actual);
		}

		[Test]
		public void Trial0923 ()
		{
			RegexTrial t = PerlTrials.trials [923];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0923", t.Expected, actual);
		}

		[Test]
		public void Trial0924 ()
		{
			RegexTrial t = PerlTrials.trials [924];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0924", t.Expected, actual);
		}

		[Test]
		public void Trial0925 ()
		{
			RegexTrial t = PerlTrials.trials [925];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0925", t.Expected, actual);
		}

		[Test]
		public void Trial0926 ()
		{
			RegexTrial t = PerlTrials.trials [926];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0926", t.Expected, actual);
		}

		[Test]
		public void Trial0927 ()
		{
			RegexTrial t = PerlTrials.trials [927];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0927", t.Expected, actual);
		}

		[Test]
		public void Trial0928 ()
		{
			RegexTrial t = PerlTrials.trials [928];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0928", t.Expected, actual);
		}

		[Test]
		public void Trial0929 ()
		{
			RegexTrial t = PerlTrials.trials [929];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0929", t.Expected, actual);
		}

		[Test]
		public void Trial0930 ()
		{
			RegexTrial t = PerlTrials.trials [930];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0930", t.Expected, actual);
		}

		[Test]
		public void Trial0931 ()
		{
			RegexTrial t = PerlTrials.trials [931];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0931", t.Expected, actual);
		}

		[Test]
		public void Trial0932 ()
		{
			RegexTrial t = PerlTrials.trials [932];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0932", t.Expected, actual);
		}

		[Test]
		public void Trial0933 ()
		{
			RegexTrial t = PerlTrials.trials [933];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0933", t.Expected, actual);
		}

		[Test]
		public void Trial0934 ()
		{
			RegexTrial t = PerlTrials.trials [934];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0934", t.Expected, actual);
		}

		[Test]
		public void Trial0935 ()
		{
			RegexTrial t = PerlTrials.trials [935];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0935", t.Expected, actual);
		}

		[Test]
		public void Trial0936 ()
		{
			RegexTrial t = PerlTrials.trials [936];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0936", t.Expected, actual);
		}

		[Test]
		public void Trial0937 ()
		{
			RegexTrial t = PerlTrials.trials [937];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0937", t.Expected, actual);
		}

		[Test]
		public void Trial0938 ()
		{
			RegexTrial t = PerlTrials.trials [938];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0938", t.Expected, actual);
		}

		[Test]
		public void Trial0939 ()
		{
			RegexTrial t = PerlTrials.trials [939];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0939", t.Expected, actual);
		}

		[Test]
		public void Trial0940 ()
		{
			RegexTrial t = PerlTrials.trials [940];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0940", t.Expected, actual);
		}

		[Test]
		public void Trial0941 ()
		{
			RegexTrial t = PerlTrials.trials [941];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0941", t.Expected, actual);
		}

		[Test]
		public void Trial0942 ()
		{
			RegexTrial t = PerlTrials.trials [942];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0942", t.Expected, actual);
		}

		[Test]
		public void Trial0943 ()
		{
			RegexTrial t = PerlTrials.trials [943];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0943", t.Expected, actual);
		}

		[Test]
		public void Trial0944 ()
		{
			RegexTrial t = PerlTrials.trials [944];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0944", t.Expected, actual);
		}

		[Test]
		public void Trial0945 ()
		{
			RegexTrial t = PerlTrials.trials [945];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0945", t.Expected, actual);
		}

		[Test]
		public void Trial0946 ()
		{
			RegexTrial t = PerlTrials.trials [946];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0946", t.Expected, actual);
		}

		[Test]
		public void Trial0947 ()
		{
			RegexTrial t = PerlTrials.trials [947];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0947", t.Expected, actual);
		}

		[Test]
		public void Trial0948 ()
		{
			RegexTrial t = PerlTrials.trials [948];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0948", t.Expected, actual);
		}

		[Test]
		public void Trial0949 ()
		{
			RegexTrial t = PerlTrials.trials [949];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0949", t.Expected, actual);
		}

		[Test]
		public void Trial0950 ()
		{
			RegexTrial t = PerlTrials.trials [950];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0950", t.Expected, actual);
		}

		[Test]
		public void Trial0951 ()
		{
			RegexTrial t = PerlTrials.trials [951];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0951", t.Expected, actual);
		}

		[Test]
		public void Trial0952 ()
		{
			RegexTrial t = PerlTrials.trials [952];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0952", t.Expected, actual);
		}

		[Test]
		public void Trial0953 ()
		{
			RegexTrial t = PerlTrials.trials [953];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0953", t.Expected, actual);
		}

		[Test]
		public void Trial0954 ()
		{
			RegexTrial t = PerlTrials.trials [954];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0954", t.Expected, actual);
		}

		[Test]
		public void Trial0955 ()
		{
			RegexTrial t = PerlTrials.trials [955];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0955", t.Expected, actual);
		}

		[Test]
		public void Trial0956 ()
		{
			RegexTrial t = PerlTrials.trials [956];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0956", t.Expected, actual);
		}

		[Test]
		public void Trial0957 ()
		{
			RegexTrial t = PerlTrials.trials [957];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0957", t.Expected, actual);
		}

		[Test]
		public void Trial0958 ()
		{
			RegexTrial t = PerlTrials.trials [958];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0958", t.Expected, actual);
		}

		[Test]
		public void Trial0959 ()
		{
			RegexTrial t = PerlTrials.trials [959];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0959", t.Expected, actual);
		}

		[Test]
		public void Trial0960 ()
		{
			RegexTrial t = PerlTrials.trials [960];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0960", t.Expected, actual);
		}

		[Test]
		public void Trial0961 ()
		{
			RegexTrial t = PerlTrials.trials [961];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0961", t.Expected, actual);
		}

		[Test]
		public void Trial0962 ()
		{
			RegexTrial t = PerlTrials.trials [962];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0962", t.Expected, actual);
		}

		[Test]
		public void Trial0963 ()
		{
			RegexTrial t = PerlTrials.trials [963];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0963", t.Expected, actual);
		}

		[Test]
		public void Trial0964 ()
		{
			RegexTrial t = PerlTrials.trials [964];
			string actual = t.Execute ();
			Assertion.AssertEquals ("#0964", t.Expected, actual);
		}
	}
}

