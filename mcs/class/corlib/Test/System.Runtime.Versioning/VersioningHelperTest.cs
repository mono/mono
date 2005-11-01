//
// VersioningHelperTest.cs -
//	Unit tests for System.Runtime.Versioning.VersioningHelper
//
// Author:
//      Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.Runtime.Versioning;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Versioning {

	[TestFixture]
	public class VersioningHelperTest {

		[Test]
		public void Name_Null ()
		{
			string s = VersioningHelper.MakeVersionSafeName (null, ResourceScope.AppDomain, ResourceScope.AppDomain);
			Assert.AreEqual (String.Empty, s, "Result");
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void From_Invalid ()
		{
			ResourceScope bad = (ResourceScope) Int32.MinValue;
			string s = VersioningHelper.MakeVersionSafeName (null, bad, ResourceScope.None);
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void To_Invalid ()
		{
			ResourceScope bad = (ResourceScope) Int32.MinValue;
			string s = VersioningHelper.MakeVersionSafeName (null, ResourceScope.AppDomain, bad);
		}

		[Test]
		public void Type_Null ()
		{
			string s = VersioningHelper.MakeVersionSafeName (null, ResourceScope.AppDomain, ResourceScope.AppDomain, null);
			Assert.AreEqual (String.Empty, s, "Result");
		}

		// ResourceScope is encoded 6 bits for "from" and 6 bits for "to"
		static int[] ValidValues = { 65, 66, 68, 130, 131, 132, 133, 134, 135, 136, 137, 138, 139, 140, 141, 142, 
			143, 260, 261, 262, 263, 264, 265, 266, 267, 268, 269, 270, 271, 1089, 1090, 1092, 1105, 1106, 
			1108, 1121, 1122, 1124, 1137, 1138, 1140, 1154, 1155, 1156, 1157, 1158, 1159, 1160, 1161, 1162,
			1163, 1164, 1165, 1166, 1167, 1170, 1171, 1172, 1173, 1174, 1175, 1176, 1177, 1178, 1179, 1180,
			1181, 1182, 1183, 1186, 1187, 1188, 1189, 1190, 1191, 1192, 1193, 1194, 1195, 1196, 1197, 1198,
			1199, 1202, 1203, 1204, 1205, 1206, 1207, 1208, 1209, 1210, 1211, 1212, 1213, 1214, 1215, 1284,
			1285, 1286, 1287, 1288, 1289, 1290, 1291, 1292, 1293, 1294, 1295, 1300, 1301, 1302, 1303, 1304,
			1305, 1306, 1307, 1308, 1309, 1310, 1311, 1316, 1317, 1318, 1319, 1320, 1321, 1322, 1323, 1324, 
			1325, 1326, 1327, 1332, 1333, 1334, 1335, 1336, 1337, 1338, 1339, 1340, 1341, 1342, 1343, 2113, 
			2114, 2116, 2145, 2146, 2148, 2178, 2179, 2180, 2181, 2182, 2183, 2184, 2185, 2186, 2187, 2188, 
			2189, 2190, 2191, 2210, 2211, 2212, 2213, 2214, 2215, 2216, 2217, 2218, 2219, 2220, 2221, 2222, 
			2223, 2308, 2309, 2310, 2311, 2312, 2313, 2314, 2315, 2316, 2317, 2318, 2319, 2340, 2341, 2342,
			2343, 2344, 2345, 2346, 2347, 2348, 2349, 2350, 2351 };

		private bool IsValid (int n)
		{
			return (Array.BinarySearch (ValidValues, n) >= 0);
		}

		[Test]
		// note: this one can be _very_ slow under a debugger due to the high number of exceptions
		public void MakeVersionSafeName ()
		{
			for (int i = 0; i < 64; i++) {
				ResourceScope from = (ResourceScope) i;
				for (int j = 0; j < 64; j++) {
					ResourceScope to = (ResourceScope) j;
					bool valid = IsValid ((i << 6) | j);
					string from_to = String.Format ("From ({2}) {0} to ({3}) {1}", from, to, (int)from, (int)to);
					try {
						string s = VersioningHelper.MakeVersionSafeName ("a", from, to);
						Assert.IsTrue (valid, from_to);
					}
					catch (ArgumentException) {
						Assert.IsFalse (valid, from_to);
					}
				}
			}
		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void MakeVersionSafeName_SingleUnWorking ()
		{
			VersioningHelper.MakeVersionSafeName ("a", ResourceScope.None, ResourceScope.None);
		}

		[Test]
		public void ConvertTo_AppDomain ()
		{
			// if appdomain is present in "from" then the ouput is unchanged
			Assert.AreEqual ("a", VersioningHelper.MakeVersionSafeName ("a", ResourceScope.AppDomain, ResourceScope.Library), "1a");
			Assert.AreEqual ("a", VersioningHelper.MakeVersionSafeName ("a", ResourceScope.AppDomain, ResourceScope.AppDomain), "1b");
			Assert.AreEqual ("a", VersioningHelper.MakeVersionSafeName ("a", ResourceScope.AppDomain, ResourceScope.AppDomain | ResourceScope.Machine), "1c");

			// if "from" doesn't have appdomain then the appdomain id is appended
			Assert.IsTrue (VersioningHelper.MakeVersionSafeName ("a", ResourceScope.Machine, ResourceScope.AppDomain).StartsWith ("a_"), "2");
		}

		[Test]
		public void ConvertTo_Process ()
		{
			// if process is present in "from" then the ouput is unchanged
			Assert.AreEqual ("a", VersioningHelper.MakeVersionSafeName ("a", ResourceScope.Process, ResourceScope.Library), "1a");
			Assert.AreEqual ("a", VersioningHelper.MakeVersionSafeName ("a", ResourceScope.Process, ResourceScope.Process), "1b");
			Assert.AreEqual ("a", VersioningHelper.MakeVersionSafeName ("a", ResourceScope.Process, ResourceScope.Process | ResourceScope.Library), "1c");

			// if "from" doesn't have process then the process id is appended
			Assert.IsTrue (VersioningHelper.MakeVersionSafeName ("a", ResourceScope.Machine, ResourceScope.Process).StartsWith ("a_"), "2");
		}

		[Test]
		public void ConvertTo_AppDomain_Process ()
		{
			// if the appdomain is present then the output is unchanged (process isn't appended)
			Assert.AreEqual ("a", VersioningHelper.MakeVersionSafeName ("a", ResourceScope.AppDomain, ResourceScope.AppDomain | ResourceScope.Process), "1a");
			// if the process is present then appdomain is appended to the output
			Assert.IsTrue (VersioningHelper.MakeVersionSafeName ("a", ResourceScope.Process, ResourceScope.AppDomain | ResourceScope.Process).StartsWith ("a_"), "1b");

			// if "from" doesn't have process nor appdomain then both id are appended
			string s = VersioningHelper.MakeVersionSafeName ("a", ResourceScope.Machine, ResourceScope.AppDomain);
			int p1 = s.IndexOf ("_");
			Assert.IsTrue ((p1 > 0), "first _");
			int p2 = s.LastIndexOf ("_");
			Assert.IsTrue (((p2 > 0) && (p2 != p1)), "second _");
		}
	}
}

#endif
