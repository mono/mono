//
// DecimalFormatterTest.cs - NUnit Test Cases for System.DecimalFormatter
//
// Author:
//     Patrick Kalkman  kalkman@cistron.nl
//
// (C) 2003 Patrick Kalkman
// 
using NUnit.Framework;
using System;
using System.Threading;
using System.Globalization;

namespace MonoTests.System
{
	public class FormatString
	{
		private int testnumber;
		private decimal number;
		private string format;
		private string expectedresult;

		public FormatString (int TestNumber, decimal Number, string Format, string ExpectedResult)
		{
			testnumber = TestNumber;
			number = Number;
			format = Format;
			expectedresult = ExpectedResult;
		}

		public int TestNumber
		{
			get { return testnumber; }
			set { testnumber = value; }
		}

		public decimal Number
		{
			get { return number; }
			set { number = value; }
		}

		public string Format
		{
			get { return format; }
			set { format = value; }
		}

		public string ExpectedResult
		{
			get { return expectedresult; }
			set { expectedresult = value; }
		}
	}

	[TestFixture]
	public class DecimalFormatterTest
	{
		CultureInfo old_culture;

		[SetUp]
		public void Setup ()
		{
			old_culture = Thread.CurrentThread.CurrentCulture;

			CultureInfo EnUs = new CultureInfo ("en-US", false);
			EnUs.NumberFormat.CurrencyNegativePattern = 0; // -1 = (1)
			EnUs.NumberFormat.CurrencyDecimalSeparator = ".";
			EnUs.NumberFormat.NumberGroupSeparator = ",";
			EnUs.NumberFormat.NumberNegativePattern = 1; // -1 = -1

			//Set this culture for the current thread.
			Thread.CurrentThread.CurrentCulture = EnUs;
		}

		[TearDown]
		public void TearDown ()
		{
			Thread.CurrentThread.CurrentCulture = old_culture;
		}

		[Test]
		public void TestFormatStrings ()
		{
			// Test all the formatstrings in the FormatTest array. 
			// If a test fails the "DecF #" equals the index of the array.
			foreach (FormatString fTest in FormatTest) {
				Assert.AreEqual (fTest.ExpectedResult, fTest.Number.ToString (fTest.Format), "DecF #" + fTest.TestNumber);
			}
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void TestToDecimal ()
		{
			decimal x = 1.0000001m;
			string Result = x.ToString ("D2"); //To Decimal is for integral types only. 
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void TestToHex ()
		{
			decimal x = 1.0000001m;
			string Result = x.ToString ("X2"); //To Hex is for integral types only. 
		}

		// Computer generated format array.
		FormatString [] FormatTest = new FormatString [] {
			new FormatString (0, 1.0034m, "C", "$1.00"),
			new FormatString (1, 1.0034m, "C0", "$1"),
			new FormatString (2, 1.0034m, "C1", "$1.0"),
			new FormatString (3, 1.0034m, "C2", "$1.00"),
			new FormatString (4, 1.0034m, "C3", "$1.003"),
			new FormatString (5, 1.0034m, "C4", "$1.0034"),
			new FormatString (6, 1.0034m, "C5", "$1.00340"),
			new FormatString (7, 1.0034m, "C6", "$1.003400"),
			new FormatString (8, 1.0034m, "C7", "$1.0034000"),
			new FormatString (9, 1.0034m, "C8", "$1.00340000"),
			new FormatString (10, 1.0034m, "C9", "$1.003400000"),
			new FormatString (11, 1.0034m, "E", "1.003400E+000"),
			new FormatString (12, 1.0034m, "E0", "1E+000"),
			new FormatString (13, 1.0034m, "E1", "1.0E+000"),
			new FormatString (14, 1.0034m, "E2", "1.00E+000"),
			new FormatString (15, 1.0034m, "E3", "1.003E+000"),
			new FormatString (16, 1.0034m, "E4", "1.0034E+000"),
			new FormatString (17, 1.0034m, "E5", "1.00340E+000"),
			new FormatString (18, 1.0034m, "E6", "1.003400E+000"),
			new FormatString (19, 1.0034m, "E7", "1.0034000E+000"),
			new FormatString (20, 1.0034m, "E8", "1.00340000E+000"),
			new FormatString (21, 1.0034m, "E9", "1.003400000E+000"),
			new FormatString (22, 1.0034m, "F", "1.00"),
			new FormatString (23, 1.0034m, "F0", "1"),
			new FormatString (24, 1.0034m, "F1", "1.0"),
			new FormatString (25, 1.0034m, "F2", "1.00"),
			new FormatString (26, 1.0034m, "F3", "1.003"),
			new FormatString (27, 1.0034m, "F4", "1.0034"),
			new FormatString (28, 1.0034m, "F5", "1.00340"),
			new FormatString (29, 1.0034m, "F6", "1.003400"),
			new FormatString (30, 1.0034m, "F7", "1.0034000"),
			new FormatString (31, 1.0034m, "F8", "1.00340000"),
			new FormatString (32, 1.0034m, "F9", "1.003400000"),
			new FormatString (33, 1.0034m, "G", "1.0034"),
			new FormatString (34, 1.0034m, "G0", "1.0034"),
			new FormatString (35, 1.0034m, "G1", "1"),
			new FormatString (36, 1.0034m, "G2", "1"),
			new FormatString (37, 1.0034m, "G3", "1"),
			new FormatString (38, 1.0034m, "G4", "1.003"),
			new FormatString (39, 1.0034m, "G5", "1.0034"),
			new FormatString (40, 1.0034m, "G6", "1.0034"),
			new FormatString (41, 1.0034m, "G7", "1.0034"),
			new FormatString (42, 1.0034m, "G8", "1.0034"),
			new FormatString (43, 1.0034m, "G9", "1.0034"),
			new FormatString (44, 1.0034m, "N", "1.00"),
			new FormatString (45, 1.0034m, "N0", "1"),
			new FormatString (46, 1.0034m, "N1", "1.0"),
			new FormatString (47, 1.0034m, "N2", "1.00"),
			new FormatString (48, 1.0034m, "N3", "1.003"),
			new FormatString (49, 1.0034m, "N4", "1.0034"),
			new FormatString (50, 1.0034m, "N5", "1.00340"),
			new FormatString (51, 1.0034m, "N6", "1.003400"),
			new FormatString (52, 1.0034m, "N7", "1.0034000"),
			new FormatString (53, 1.0034m, "N8", "1.00340000"),
			new FormatString (54, 1.0034m, "N9", "1.003400000"),
			new FormatString (55, 1.0034m, "P", "100.34 %"),
			new FormatString (56, 1.0034m, "P0", "100 %"),
			new FormatString (57, 1.0034m, "P1", "100.3 %"),
			new FormatString (58, 1.0034m, "P2", "100.34 %"),
			new FormatString (59, 1.0034m, "P3", "100.340 %"),
			new FormatString (60, 1.0034m, "P4", "100.3400 %"),
			new FormatString (61, 1.0034m, "P5", "100.34000 %"),
			new FormatString (62, 1.0034m, "P6", "100.340000 %"),
			new FormatString (63, 1.0034m, "P7", "100.3400000 %"),
			new FormatString (64, 1.0034m, "P8", "100.34000000 %"),
			new FormatString (65, 1.0034m, "P9", "100.340000000 %"),
			new FormatString (66, 343433.223m, "C", "$343,433.22"),
			new FormatString (67, 343433.223m, "C0", "$343,433"),
			new FormatString (68, 343433.223m, "C1", "$343,433.2"),
			new FormatString (69, 343433.223m, "C2", "$343,433.22"),
			new FormatString (70, 343433.223m, "C3", "$343,433.223"),
			new FormatString (71, 343433.223m, "C4", "$343,433.2230"),
			new FormatString (72, 343433.223m, "C5", "$343,433.22300"),
			new FormatString (73, 343433.223m, "C6", "$343,433.223000"),
			new FormatString (74, 343433.223m, "C7", "$343,433.2230000"),
			new FormatString (75, 343433.223m, "C8", "$343,433.22300000"),
			new FormatString (76, 343433.223m, "C9", "$343,433.223000000"),
			new FormatString (77, 343433.223m, "E", "3.434332E+005"),
			new FormatString (78, 343433.223m, "E0", "3E+005"),
			new FormatString (79, 343433.223m, "E1", "3.4E+005"),
			new FormatString (80, 343433.223m, "E2", "3.43E+005"),
			new FormatString (81, 343433.223m, "E3", "3.434E+005"),
			new FormatString (82, 343433.223m, "E4", "3.4343E+005"),
			new FormatString (83, 343433.223m, "E5", "3.43433E+005"),
			new FormatString (84, 343433.223m, "E6", "3.434332E+005"),
			new FormatString (85, 343433.223m, "E7", "3.4343322E+005"),
			new FormatString (86, 343433.223m, "E8", "3.43433223E+005"),
			new FormatString (87, 343433.223m, "E9", "3.434332230E+005"),
			new FormatString (88, 343433.223m, "F", "343433.22"),
			new FormatString (89, 343433.223m, "F0", "343433"),
			new FormatString (90, 343433.223m, "F1", "343433.2"),
			new FormatString (91, 343433.223m, "F2", "343433.22"),
			new FormatString (92, 343433.223m, "F3", "343433.223"),
			new FormatString (93, 343433.223m, "F4", "343433.2230"),
			new FormatString (94, 343433.223m, "F5", "343433.22300"),
			new FormatString (95, 343433.223m, "F6", "343433.223000"),
			new FormatString (96, 343433.223m, "F7", "343433.2230000"),
			new FormatString (97, 343433.223m, "F8", "343433.22300000"),
			new FormatString (98, 343433.223m, "F9", "343433.223000000"),
			new FormatString (99, 343433.223m, "G", "343433.223"),
			new FormatString (100, 343433.223m, "G0", "343433.223"),
			new FormatString (101, 343433.223m, "G1", "3E+05"),
			new FormatString (102, 343433.223m, "G2", "3.4E+05"),
			new FormatString (103, 343433.223m, "G3", "3.43E+05"),
			new FormatString (104, 343433.223m, "G4", "3.434E+05"),
			new FormatString (105, 343433.223m, "G5", "3.4343E+05"),
			new FormatString (106, 343433.223m, "G6", "343433"),
			new FormatString (107, 343433.223m, "G7", "343433.2"),
			new FormatString (108, 343433.223m, "G8", "343433.22"),
			new FormatString (109, 343433.223m, "G9", "343433.223"),
			new FormatString (110, 343433.223m, "N", "343,433.22"),
			new FormatString (111, 343433.223m, "N0", "343,433"),
			new FormatString (112, 343433.223m, "N1", "343,433.2"),
			new FormatString (113, 343433.223m, "N2", "343,433.22"),
			new FormatString (114, 343433.223m, "N3", "343,433.223"),
			new FormatString (115, 343433.223m, "N4", "343,433.2230"),
			new FormatString (116, 343433.223m, "N5", "343,433.22300"),
			new FormatString (117, 343433.223m, "N6", "343,433.223000"),
			new FormatString (118, 343433.223m, "N7", "343,433.2230000"),
			new FormatString (119, 343433.223m, "N8", "343,433.22300000"),
			new FormatString (120, 343433.223m, "N9", "343,433.223000000"),
			new FormatString (121, 343433.223m, "P", "34,343,322.30 %"),
			new FormatString (122, 343433.223m, "P0", "34,343,322 %"),
			new FormatString (123, 343433.223m, "P1", "34,343,322.3 %"),
			new FormatString (124, 343433.223m, "P2", "34,343,322.30 %"),
			new FormatString (125, 343433.223m, "P3", "34,343,322.300 %"),
			new FormatString (126, 343433.223m, "P4", "34,343,322.3000 %"),
			new FormatString (127, 343433.223m, "P5", "34,343,322.30000 %"),
			new FormatString (128, 343433.223m, "P6", "34,343,322.300000 %"),
			new FormatString (129, 343433.223m, "P7", "34,343,322.3000000 %"),
			new FormatString (130, 343433.223m, "P8", "34,343,322.30000000 %"),
			new FormatString (131, 343433.223m, "P9", "34,343,322.300000000 %"),
			new FormatString (132, -1.9292929332m, "C", "($1.93)"),
			new FormatString (133, -1.9292929332m, "C0", "($2)"),
			new FormatString (134, -1.9292929332m, "C1", "($1.9)"),
			new FormatString (135, -1.9292929332m, "C2", "($1.93)"),
			new FormatString (136, -1.9292929332m, "C3", "($1.929)"),
			new FormatString (137, -1.9292929332m, "C4", "($1.9293)"),
			new FormatString (138, -1.9292929332m, "C5", "($1.92929)"),
			new FormatString (139, -1.9292929332m, "C6", "($1.929293)"),
			new FormatString (140, -1.9292929332m, "C7", "($1.9292929)"),
			new FormatString (141, -1.9292929332m, "C8", "($1.92929293)"),
			new FormatString (142, -1.9292929332m, "C9", "($1.929292933)"),
			new FormatString (143, -1.9292929332m, "E", "-1.929293E+000"),
			new FormatString (144, -1.9292929332m, "E0", "-2E+000"),
			new FormatString (145, -1.9292929332m, "E1", "-1.9E+000"),
			new FormatString (146, -1.9292929332m, "E2", "-1.93E+000"),
			new FormatString (147, -1.9292929332m, "E3", "-1.929E+000"),
			new FormatString (148, -1.9292929332m, "E4", "-1.9293E+000"),
			new FormatString (149, -1.9292929332m, "E5", "-1.92929E+000"),
			new FormatString (150, -1.9292929332m, "E6", "-1.929293E+000"),
			new FormatString (151, -1.9292929332m, "E7", "-1.9292929E+000"),
			new FormatString (152, -1.9292929332m, "E8", "-1.92929293E+000"),
			new FormatString (153, -1.9292929332m, "E9", "-1.929292933E+000"),
			new FormatString (154, -1.9292929332m, "F", "-1.93"),
			new FormatString (155, -1.9292929332m, "F0", "-2"),
			new FormatString (156, -1.9292929332m, "F1", "-1.9"),
			new FormatString (157, -1.9292929332m, "F2", "-1.93"),
			new FormatString (158, -1.9292929332m, "F3", "-1.929"),
			new FormatString (159, -1.9292929332m, "F4", "-1.9293"),
			new FormatString (160, -1.9292929332m, "F5", "-1.92929"),
			new FormatString (161, -1.9292929332m, "F6", "-1.929293"),
			new FormatString (162, -1.9292929332m, "F7", "-1.9292929"),
			new FormatString (163, -1.9292929332m, "F8", "-1.92929293"),
			new FormatString (164, -1.9292929332m, "F9", "-1.929292933"),
			new FormatString (165, -1.9292929332m, "G", "-1.9292929332"),
			new FormatString (166, -1.9292929332m, "G0", "-1.9292929332"),
			new FormatString (167, -1.9292929332m, "G1", "-2"),
			new FormatString (168, -1.9292929332m, "G2", "-1.9"),
			new FormatString (169, -1.9292929332m, "G3", "-1.93"),
			new FormatString (170, -1.9292929332m, "G4", "-1.929"),
			new FormatString (171, -1.9292929332m, "G5", "-1.9293"),
			new FormatString (172, -1.9292929332m, "G6", "-1.92929"),
			new FormatString (173, -1.9292929332m, "G7", "-1.929293"),
			new FormatString (174, -1.9292929332m, "G8", "-1.9292929"),
			new FormatString (175, -1.9292929332m, "G9", "-1.92929293"),
			new FormatString (176, -1.9292929332m, "N", "-1.93"),
			new FormatString (177, -1.9292929332m, "N0", "-2"),
			new FormatString (178, -1.9292929332m, "N1", "-1.9"),
			new FormatString (179, -1.9292929332m, "N2", "-1.93"),
			new FormatString (180, -1.9292929332m, "N3", "-1.929"),
			new FormatString (181, -1.9292929332m, "N4", "-1.9293"),
			new FormatString (182, -1.9292929332m, "N5", "-1.92929"),
			new FormatString (183, -1.9292929332m, "N6", "-1.929293"),
			new FormatString (184, -1.9292929332m, "N7", "-1.9292929"),
			new FormatString (185, -1.9292929332m, "N8", "-1.92929293"),
			new FormatString (186, -1.9292929332m, "N9", "-1.929292933"),
			new FormatString (187, -1.9292929332m, "P", "-192.93 %"),
			new FormatString (188, -1.9292929332m, "P0", "-193 %"),
			new FormatString (189, -1.9292929332m, "P1", "-192.9 %"),
			new FormatString (190, -1.9292929332m, "P2", "-192.93 %"),
			new FormatString (191, -1.9292929332m, "P3", "-192.929 %"),
			new FormatString (192, -1.9292929332m, "P4", "-192.9293 %"),
			new FormatString (193, -1.9292929332m, "P5", "-192.92929 %"),
			new FormatString (194, -1.9292929332m, "P6", "-192.929293 %"),
			new FormatString (195, -1.9292929332m, "P7", "-192.9292933 %"),
			new FormatString (196, -1.9292929332m, "P8", "-192.92929332 %"),
			new FormatString (197, -1.9292929332m, "P9", "-192.929293320 %"),
			new FormatString (198, 67234234.23434343434341111m, "C", "$67,234,234.23"),
			new FormatString (199, 67234234.23434343434341111m, "C0", "$67,234,234"),
			new FormatString (200, 67234234.23434343434341111m, "C1", "$67,234,234.2"),
			new FormatString (201, 67234234.23434343434341111m, "C2", "$67,234,234.23"),
			new FormatString (202, 67234234.23434343434341111m, "C3", "$67,234,234.234"),
			new FormatString (203, 67234234.23434343434341111m, "C4", "$67,234,234.2343"),
			new FormatString (204, 67234234.23434343434341111m, "C5", "$67,234,234.23434"),
			new FormatString (205, 67234234.23434343434341111m, "C6", "$67,234,234.234343"),
			new FormatString (206, 67234234.23434343434341111m, "C7", "$67,234,234.2343434"),
			new FormatString (207, 67234234.23434343434341111m, "C8", "$67,234,234.23434343"),
			new FormatString (208, 67234234.23434343434341111m, "C9", "$67,234,234.234343434"),
			new FormatString (209, 67234234.23434343434341111m, "E", "6.723423E+007"),
			new FormatString (210, 67234234.23434343434341111m, "E0", "7E+007"),
			new FormatString (211, 67234234.23434343434341111m, "E1", "6.7E+007"),
			new FormatString (212, 67234234.23434343434341111m, "E2", "6.72E+007"),
			new FormatString (213, 67234234.23434343434341111m, "E3", "6.723E+007"),
			new FormatString (214, 67234234.23434343434341111m, "E4", "6.7234E+007"),
			new FormatString (215, 67234234.23434343434341111m, "E5", "6.72342E+007"),
			new FormatString (216, 67234234.23434343434341111m, "E6", "6.723423E+007"),
			new FormatString (217, 67234234.23434343434341111m, "E7", "6.7234234E+007"),
			new FormatString (218, 67234234.23434343434341111m, "E8", "6.72342342E+007"),
			new FormatString (219, 67234234.23434343434341111m, "E9", "6.723423423E+007"),
			new FormatString (220, 67234234.23434343434341111m, "F", "67234234.23"),
			new FormatString (221, 67234234.23434343434341111m, "F0", "67234234"),
			new FormatString (222, 67234234.23434343434341111m, "F1", "67234234.2"),
			new FormatString (223, 67234234.23434343434341111m, "F2", "67234234.23"),
			new FormatString (224, 67234234.23434343434341111m, "F3", "67234234.234"),
			new FormatString (225, 67234234.23434343434341111m, "F4", "67234234.2343"),
			new FormatString (226, 67234234.23434343434341111m, "F5", "67234234.23434"),
			new FormatString (227, 67234234.23434343434341111m, "F6", "67234234.234343"),
			new FormatString (228, 67234234.23434343434341111m, "F7", "67234234.2343434"),
			new FormatString (229, 67234234.23434343434341111m, "F8", "67234234.23434343"),
			new FormatString (230, 67234234.23434343434341111m, "F9", "67234234.234343434"),
			new FormatString (231, 67234234.23434343434341111m, "G", "67234234.23434343434341111"),
			new FormatString (232, 67234234.23434343434341111m, "G0", "67234234.23434343434341111"),
			new FormatString (233, 67234234.23434343434341111m, "G1", "7E+07"),
			new FormatString (234, 67234234.23434343434341111m, "G2", "6.7E+07"),
			new FormatString (235, 67234234.23434343434341111m, "G3", "6.72E+07"),
			new FormatString (236, 67234234.23434343434341111m, "G4", "6.723E+07"),
			new FormatString (237, 67234234.23434343434341111m, "G5", "6.7234E+07"),
			new FormatString (238, 67234234.23434343434341111m, "G6", "6.72342E+07"),
			new FormatString (239, 67234234.23434343434341111m, "G7", "6.723423E+07"),
			new FormatString (240, 67234234.23434343434341111m, "G8", "67234234"),
			new FormatString (241, 67234234.23434343434341111m, "G9", "67234234.2"),
			new FormatString (242, 67234234.23434343434341111m, "N", "67,234,234.23"),
			new FormatString (243, 67234234.23434343434341111m, "N0", "67,234,234"),
			new FormatString (244, 67234234.23434343434341111m, "N1", "67,234,234.2"),
			new FormatString (245, 67234234.23434343434341111m, "N2", "67,234,234.23"),
			new FormatString (246, 67234234.23434343434341111m, "N3", "67,234,234.234"),
			new FormatString (247, 67234234.23434343434341111m, "N4", "67,234,234.2343"),
			new FormatString (248, 67234234.23434343434341111m, "N5", "67,234,234.23434"),
			new FormatString (249, 67234234.23434343434341111m, "N6", "67,234,234.234343"),
			new FormatString (250, 67234234.23434343434341111m, "N7", "67,234,234.2343434"),
			new FormatString (251, 67234234.23434343434341111m, "N8", "67,234,234.23434343"),
			new FormatString (252, 67234234.23434343434341111m, "N9", "67,234,234.234343434"),
			new FormatString (253, 67234234.23434343434341111m, "P", "6,723,423,423.43 %"),
			new FormatString (254, 67234234.23434343434341111m, "P0", "6,723,423,423 %"),
			new FormatString (255, 67234234.23434343434341111m, "P1", "6,723,423,423.4 %"),
			new FormatString (256, 67234234.23434343434341111m, "P2", "6,723,423,423.43 %"),
			new FormatString (257, 67234234.23434343434341111m, "P3", "6,723,423,423.434 %"),
			new FormatString (258, 67234234.23434343434341111m, "P4", "6,723,423,423.4343 %"),
			new FormatString (259, 67234234.23434343434341111m, "P5", "6,723,423,423.43434 %"),
			new FormatString (260, 67234234.23434343434341111m, "P6", "6,723,423,423.434343 %"),
			new FormatString (261, 67234234.23434343434341111m, "P7", "6,723,423,423.4343434 %"),
			new FormatString (262, 67234234.23434343434341111m, "P8", "6,723,423,423.43434343 %"),
			new FormatString (263, 67234234.23434343434341111m, "P9", "6,723,423,423.434343434 %")
		};
	}
}
