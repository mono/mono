//
// SingleFormatterTest.cs - NUnit Test Cases for System.SingleFormatter
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

namespace MonoTests.System {
	
	[TestFixture]
	public class SingleFormatterTest 
	{
		[SetUp]
		public void GetReady() 
		{
			CultureInfo EnUs = new CultureInfo ("en-us");
			EnUs.NumberFormat.CurrencyNegativePattern = 0; // -1 = (1)
			EnUs.NumberFormat.CurrencyDecimalSeparator = ".";
			EnUs.NumberFormat.NumberGroupSeparator = ",";
			EnUs.NumberFormat.NumberNegativePattern = 1; // -1 = -1
			
			//Set this culture for the current thread.
			Thread.CurrentThread.CurrentCulture = EnUs;
		}
		
		[TearDown]
		public void Clean() {}
		
		[Test]
		[ExpectedException(typeof(FormatException))]
		public void TestToDecimal()
		{
			Single x = 1.0000001F;
			string Result = x.ToString ("D2"); //To Decimal is for integral types only. 
		}
		
		[Test]
		[ExpectedException(typeof(FormatException))]
		public void TestToHex()
		{
			Single x = 1.212121F;
			string Result = x.ToString ("X2"); //To Hex is for integral types only. 
		}

		[Test]
		[ExpectedException(typeof(FormatException))]
		public void TestToUnknown()
		{
			Single x = 1.212121F;
			string Result = x.ToString ("L2"); //Invalid format. 
		}
		
		private void FormatStringTest(int TestNumber, float Number, string Format, string ExpectedResult)
		{
			Assertion.AssertEquals ("SngF #" + TestNumber, ExpectedResult, Number.ToString(Format));                                
		}
		
		[Test]
		public void TestFormatStrings()
		{
			 FormatStringTest (0, 121212F, "C", "$121,212.00");  
			 FormatStringTest (1, 121212F, "C0", "$121,212");  
			 FormatStringTest (2, 121212F, "C1", "$121,212.0");  
			 FormatStringTest (3, 121212F, "C2", "$121,212.00");  
			 FormatStringTest (4, 121212F, "C3", "$121,212.000");  
			 FormatStringTest (5, 121212F, "C4", "$121,212.0000");  
			 FormatStringTest (6, 121212F, "C5", "$121,212.00000");  
			 FormatStringTest (7, 121212F, "C6", "$121,212.000000");  
			 FormatStringTest (8, 121212F, "C7", "$121,212.0000000");  
			 FormatStringTest (9, 121212F, "C8", "$121,212.00000000");  
			 FormatStringTest (10, 121212F, "C9", "$121,212.000000000");  
			 FormatStringTest (11, 121212F, "C67", "$121,212.0000000000000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (12, 121212F, "E", "1.212120E+005");  
			 FormatStringTest (13, 121212F, "E0", "1E+005");  
			 FormatStringTest (14, 121212F, "E1", "1.2E+005");  
			 FormatStringTest (15, 121212F, "E2", "1.21E+005");  
			 FormatStringTest (16, 121212F, "E3", "1.212E+005");  
			 FormatStringTest (17, 121212F, "E4", "1.2121E+005");  
			 FormatStringTest (18, 121212F, "E5", "1.21212E+005");  
			 FormatStringTest (19, 121212F, "E6", "1.212120E+005");  
			 FormatStringTest (20, 121212F, "E7", "1.2121200E+005");  
			 FormatStringTest (21, 121212F, "E8", "1.21212000E+005");  
			 FormatStringTest (22, 121212F, "E9", "1.212120000E+005");  
			 FormatStringTest (23, 121212F, "E67", "1.2121200000000000000000000000000000000000000000000000000000000000000E+005");  
			 FormatStringTest (24, 121212F, "F", "121212.00");  
			 FormatStringTest (25, 121212F, "F0", "121212");  
			 FormatStringTest (26, 121212F, "F1", "121212.0");  
			 FormatStringTest (27, 121212F, "F2", "121212.00");  
			 FormatStringTest (28, 121212F, "F3", "121212.000");  
			 FormatStringTest (29, 121212F, "F4", "121212.0000");  
			 FormatStringTest (30, 121212F, "F5", "121212.00000");  
			 FormatStringTest (31, 121212F, "F6", "121212.000000");  
			 FormatStringTest (32, 121212F, "F7", "121212.0000000");  
			 FormatStringTest (33, 121212F, "F8", "121212.00000000");  
			 FormatStringTest (34, 121212F, "F9", "121212.000000000");  
			 FormatStringTest (35, 121212F, "F67", "121212.0000000000000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (36, 121212F, "G", "121212");  
			 FormatStringTest (37, 121212F, "G0", "121212");  
			 FormatStringTest (38, 121212F, "G1", "1E+05");  
			 FormatStringTest (39, 121212F, "G2", "1.2E+05");  
			 FormatStringTest (40, 121212F, "G3", "1.21E+05");  
			 FormatStringTest (41, 121212F, "G4", "1.212E+05");  
			 FormatStringTest (42, 121212F, "G5", "1.2121E+05");  
			 FormatStringTest (43, 121212F, "G6", "121212");  
			 FormatStringTest (44, 121212F, "G7", "121212");  
			 FormatStringTest (45, 121212F, "G8", "121212");  
			 FormatStringTest (46, 121212F, "G9", "121212");  
			 FormatStringTest (47, 121212F, "G67", "121212");  
			 FormatStringTest (48, 121212F, "N", "121,212.00");  
			 FormatStringTest (49, 121212F, "N0", "121,212");  
			 FormatStringTest (50, 121212F, "N1", "121,212.0");  
			 FormatStringTest (51, 121212F, "N2", "121,212.00");  
			 FormatStringTest (52, 121212F, "N3", "121,212.000");  
			 FormatStringTest (53, 121212F, "N4", "121,212.0000");  
			 FormatStringTest (54, 121212F, "N5", "121,212.00000");  
			 FormatStringTest (55, 121212F, "N6", "121,212.000000");  
			 FormatStringTest (56, 121212F, "N7", "121,212.0000000");  
			 FormatStringTest (57, 121212F, "N8", "121,212.00000000");  
			 FormatStringTest (58, 121212F, "N9", "121,212.000000000");  
			 FormatStringTest (59, 121212F, "N67", "121,212.0000000000000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (60, 121212F, "P", "12,121,200.00 %");  
			 FormatStringTest (61, 121212F, "P0", "12,121,200 %");  
			 FormatStringTest (62, 121212F, "P1", "12,121,200.0 %");  
			 FormatStringTest (63, 121212F, "P2", "12,121,200.00 %");  
			 FormatStringTest (64, 121212F, "P3", "12,121,200.000 %");  
			 FormatStringTest (65, 121212F, "P4", "12,121,200.0000 %");  
			 FormatStringTest (66, 121212F, "P5", "12,121,200.00000 %");  
			 FormatStringTest (67, 121212F, "P6", "12,121,200.000000 %");  
			 FormatStringTest (68, 121212F, "P7", "12,121,200.0000000 %");  
			 FormatStringTest (69, 121212F, "P8", "12,121,200.00000000 %");  
			 FormatStringTest (70, 121212F, "P9", "12,121,200.000000000 %");  
			 FormatStringTest (71, 121212F, "P67", "12,121,200.0000000000000000000000000000000000000000000000000000000000000000000 %");  
			 FormatStringTest (72, 3.402823E+38F, "C", "$340,282,300,000,000,000,000,000,000,000,000,000,000.00");  
			 FormatStringTest (73, 3.402823E+38F, "C0", "$340,282,300,000,000,000,000,000,000,000,000,000,000");  
			 FormatStringTest (74, 3.402823E+38F, "C1", "$340,282,300,000,000,000,000,000,000,000,000,000,000.0");  
			 FormatStringTest (75, 3.402823E+38F, "C2", "$340,282,300,000,000,000,000,000,000,000,000,000,000.00");  
			 FormatStringTest (76, 3.402823E+38F, "C3", "$340,282,300,000,000,000,000,000,000,000,000,000,000.000");  
			 FormatStringTest (77, 3.402823E+38F, "C4", "$340,282,300,000,000,000,000,000,000,000,000,000,000.0000");  
			 FormatStringTest (78, 3.402823E+38F, "C5", "$340,282,300,000,000,000,000,000,000,000,000,000,000.00000");  
			 FormatStringTest (79, 3.402823E+38F, "C6", "$340,282,300,000,000,000,000,000,000,000,000,000,000.000000");  
			 FormatStringTest (80, 3.402823E+38F, "C7", "$340,282,300,000,000,000,000,000,000,000,000,000,000.0000000");  
			 FormatStringTest (81, 3.402823E+38F, "C8", "$340,282,300,000,000,000,000,000,000,000,000,000,000.00000000");  
			 FormatStringTest (82, 3.402823E+38F, "C9", "$340,282,300,000,000,000,000,000,000,000,000,000,000.000000000");  
			 FormatStringTest (83, 3.402823E+38F, "C67", "$340,282,300,000,000,000,000,000,000,000,000,000,000.0000000000000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (84, 3.402823E+38F, "E", "3.402823E+038");  
			 FormatStringTest (85, 3.402823E+38F, "E0", "3E+038");  
			 FormatStringTest (86, 3.402823E+38F, "E1", "3.4E+038");  
			 FormatStringTest (87, 3.402823E+38F, "E2", "3.40E+038");  
			 FormatStringTest (88, 3.402823E+38F, "E3", "3.403E+038");  
			 FormatStringTest (89, 3.402823E+38F, "E4", "3.4028E+038");  
			 FormatStringTest (90, 3.402823E+38F, "E5", "3.40282E+038");  
			 FormatStringTest (91, 3.402823E+38F, "E6", "3.402823E+038");  
			 FormatStringTest (92, 3.402823E+38F, "E7", "3.4028231E+038");  
			 FormatStringTest (93, 3.402823E+38F, "E8", "3.40282306E+038");  
			 FormatStringTest (94, 3.402823E+38F, "E9", "3.402823060E+038");  
			 FormatStringTest (95, 3.402823E+38F, "E67", "3.4028230600000000000000000000000000000000000000000000000000000000000E+038");  
			 FormatStringTest (96, 3.402823E+38F, "F", "340282300000000000000000000000000000000.00");  
			 FormatStringTest (97, 3.402823E+38F, "F0", "340282300000000000000000000000000000000");  
			 FormatStringTest (98, 3.402823E+38F, "F1", "340282300000000000000000000000000000000.0");  
			 FormatStringTest (99, 3.402823E+38F, "F2", "340282300000000000000000000000000000000.00");  
			 FormatStringTest (100, 3.402823E+38F, "F3", "340282300000000000000000000000000000000.000");  
			 FormatStringTest (101, 3.402823E+38F, "F4", "340282300000000000000000000000000000000.0000");  
			 FormatStringTest (102, 3.402823E+38F, "F5", "340282300000000000000000000000000000000.00000");  
			 FormatStringTest (103, 3.402823E+38F, "F6", "340282300000000000000000000000000000000.000000");  
			 FormatStringTest (104, 3.402823E+38F, "F7", "340282300000000000000000000000000000000.0000000");  
			 FormatStringTest (105, 3.402823E+38F, "F8", "340282300000000000000000000000000000000.00000000");  
			 FormatStringTest (106, 3.402823E+38F, "F9", "340282300000000000000000000000000000000.000000000");  
			 FormatStringTest (107, 3.402823E+38F, "F67", "340282300000000000000000000000000000000.0000000000000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (108, 3.402823E+38F, "G", "3.402823E+38");  
			 FormatStringTest (109, 3.402823E+38F, "G0", "3.402823E+38");  
			 FormatStringTest (110, 3.402823E+38F, "G1", "3E+38");  
			 FormatStringTest (111, 3.402823E+38F, "G2", "3.4E+38");  
			 FormatStringTest (112, 3.402823E+38F, "G3", "3.4E+38");  
			 FormatStringTest (113, 3.402823E+38F, "G4", "3.403E+38");  
			 FormatStringTest (114, 3.402823E+38F, "G5", "3.4028E+38");  
			 FormatStringTest (115, 3.402823E+38F, "G6", "3.40282E+38");  
			 FormatStringTest (116, 3.402823E+38F, "G7", "3.402823E+38");  
			 FormatStringTest (117, 3.402823E+38F, "G8", "3.4028231E+38");  
			 FormatStringTest (118, 3.402823E+38F, "G9", "3.40282306E+38");  
			 FormatStringTest (119, 3.402823E+38F, "G67", "340282306000000000000000000000000000000");  
			 FormatStringTest (120, 3.402823E+38F, "N", "340,282,300,000,000,000,000,000,000,000,000,000,000.00");  
			 FormatStringTest (121, 3.402823E+38F, "N0", "340,282,300,000,000,000,000,000,000,000,000,000,000");  
			 FormatStringTest (122, 3.402823E+38F, "N1", "340,282,300,000,000,000,000,000,000,000,000,000,000.0");  
			 FormatStringTest (123, 3.402823E+38F, "N2", "340,282,300,000,000,000,000,000,000,000,000,000,000.00");  
			 FormatStringTest (124, 3.402823E+38F, "N3", "340,282,300,000,000,000,000,000,000,000,000,000,000.000");  
			 FormatStringTest (125, 3.402823E+38F, "N4", "340,282,300,000,000,000,000,000,000,000,000,000,000.0000");  
			 FormatStringTest (126, 3.402823E+38F, "N5", "340,282,300,000,000,000,000,000,000,000,000,000,000.00000");  
			 FormatStringTest (127, 3.402823E+38F, "N6", "340,282,300,000,000,000,000,000,000,000,000,000,000.000000");  
			 FormatStringTest (128, 3.402823E+38F, "N7", "340,282,300,000,000,000,000,000,000,000,000,000,000.0000000");  
			 FormatStringTest (129, 3.402823E+38F, "N8", "340,282,300,000,000,000,000,000,000,000,000,000,000.00000000");  
			 FormatStringTest (130, 3.402823E+38F, "N9", "340,282,300,000,000,000,000,000,000,000,000,000,000.000000000");  
			 FormatStringTest (131, 3.402823E+38F, "N67", "340,282,300,000,000,000,000,000,000,000,000,000,000.0000000000000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (132, 3.402823E+38F, "P", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.00 %");  
			 FormatStringTest (133, 3.402823E+38F, "P0", "34,028,230,000,000,000,000,000,000,000,000,000,000,000 %");  
			 FormatStringTest (134, 3.402823E+38F, "P1", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.0 %");  
			 FormatStringTest (135, 3.402823E+38F, "P2", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.00 %");  
			 FormatStringTest (136, 3.402823E+38F, "P3", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.000 %");  
			 FormatStringTest (137, 3.402823E+38F, "P4", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.0000 %");  
			 FormatStringTest (138, 3.402823E+38F, "P5", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.00000 %");  
			 FormatStringTest (139, 3.402823E+38F, "P6", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.000000 %");  
			 FormatStringTest (140, 3.402823E+38F, "P7", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.0000000 %");  
			 FormatStringTest (141, 3.402823E+38F, "P8", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.00000000 %");  
			 FormatStringTest (142, 3.402823E+38F, "P9", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.000000000 %");  
			 FormatStringTest (143, 3.402823E+38F, "P67", "34,028,230,000,000,000,000,000,000,000,000,000,000,000.0000000000000000000000000000000000000000000000000000000000000000000 %");  
			 FormatStringTest (144, -3.402823E+38F, "C", "($340,282,300,000,000,000,000,000,000,000,000,000,000.00)");  
			 FormatStringTest (145, -3.402823E+38F, "C0", "($340,282,300,000,000,000,000,000,000,000,000,000,000)");  
			 FormatStringTest (146, -3.402823E+38F, "C1", "($340,282,300,000,000,000,000,000,000,000,000,000,000.0)");  
			 FormatStringTest (147, -3.402823E+38F, "C2", "($340,282,300,000,000,000,000,000,000,000,000,000,000.00)");  
			 FormatStringTest (148, -3.402823E+38F, "C3", "($340,282,300,000,000,000,000,000,000,000,000,000,000.000)");  
			 FormatStringTest (149, -3.402823E+38F, "C4", "($340,282,300,000,000,000,000,000,000,000,000,000,000.0000)");  
			 FormatStringTest (150, -3.402823E+38F, "C5", "($340,282,300,000,000,000,000,000,000,000,000,000,000.00000)");  
			 FormatStringTest (151, -3.402823E+38F, "C6", "($340,282,300,000,000,000,000,000,000,000,000,000,000.000000)");  
			 FormatStringTest (152, -3.402823E+38F, "C7", "($340,282,300,000,000,000,000,000,000,000,000,000,000.0000000)");  
			 FormatStringTest (153, -3.402823E+38F, "C8", "($340,282,300,000,000,000,000,000,000,000,000,000,000.00000000)");  
			 FormatStringTest (154, -3.402823E+38F, "C9", "($340,282,300,000,000,000,000,000,000,000,000,000,000.000000000)");  
			 FormatStringTest (155, -3.402823E+38F, "C67", "($340,282,300,000,000,000,000,000,000,000,000,000,000.0000000000000000000000000000000000000000000000000000000000000000000)");  
			 FormatStringTest (156, -3.402823E+38F, "E", "-3.402823E+038");  
			 FormatStringTest (157, -3.402823E+38F, "E0", "-3E+038");  
			 FormatStringTest (158, -3.402823E+38F, "E1", "-3.4E+038");  
			 FormatStringTest (159, -3.402823E+38F, "E2", "-3.40E+038");  
			 FormatStringTest (160, -3.402823E+38F, "E3", "-3.403E+038");  
			 FormatStringTest (161, -3.402823E+38F, "E4", "-3.4028E+038");  
			 FormatStringTest (162, -3.402823E+38F, "E5", "-3.40282E+038");  
			 FormatStringTest (163, -3.402823E+38F, "E6", "-3.402823E+038");  
			 FormatStringTest (164, -3.402823E+38F, "E7", "-3.4028231E+038");  
			 FormatStringTest (165, -3.402823E+38F, "E8", "-3.40282306E+038");  
			 FormatStringTest (166, -3.402823E+38F, "E9", "-3.402823060E+038");  
			 FormatStringTest (167, -3.402823E+38F, "E67", "-3.4028230600000000000000000000000000000000000000000000000000000000000E+038");  
			 FormatStringTest (168, -3.402823E+38F, "F", "-340282300000000000000000000000000000000.00");  
			 FormatStringTest (169, -3.402823E+38F, "F0", "-340282300000000000000000000000000000000");  
			 FormatStringTest (170, -3.402823E+38F, "F1", "-340282300000000000000000000000000000000.0");  
			 FormatStringTest (171, -3.402823E+38F, "F2", "-340282300000000000000000000000000000000.00");  
			 FormatStringTest (172, -3.402823E+38F, "F3", "-340282300000000000000000000000000000000.000");  
			 FormatStringTest (173, -3.402823E+38F, "F4", "-340282300000000000000000000000000000000.0000");  
			 FormatStringTest (174, -3.402823E+38F, "F5", "-340282300000000000000000000000000000000.00000");  
			 FormatStringTest (175, -3.402823E+38F, "F6", "-340282300000000000000000000000000000000.000000");  
			 FormatStringTest (176, -3.402823E+38F, "F7", "-340282300000000000000000000000000000000.0000000");  
			 FormatStringTest (177, -3.402823E+38F, "F8", "-340282300000000000000000000000000000000.00000000");  
			 FormatStringTest (178, -3.402823E+38F, "F9", "-340282300000000000000000000000000000000.000000000");  
			 FormatStringTest (179, -3.402823E+38F, "F67", "-340282300000000000000000000000000000000.0000000000000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (180, -3.402823E+38F, "G", "-3.402823E+38");  
			 FormatStringTest (181, -3.402823E+38F, "G0", "-3.402823E+38");  
			 FormatStringTest (182, -3.402823E+38F, "G1", "-3E+38");  
			 FormatStringTest (183, -3.402823E+38F, "G2", "-3.4E+38");  
			 FormatStringTest (184, -3.402823E+38F, "G3", "-3.4E+38");  
			 FormatStringTest (185, -3.402823E+38F, "G4", "-3.403E+38");  
			 FormatStringTest (186, -3.402823E+38F, "G5", "-3.4028E+38");  
			 FormatStringTest (187, -3.402823E+38F, "G6", "-3.40282E+38");  
			 FormatStringTest (188, -3.402823E+38F, "G7", "-3.402823E+38");  
			 FormatStringTest (189, -3.402823E+38F, "G8", "-3.4028231E+38");  
			 FormatStringTest (190, -3.402823E+38F, "G9", "-3.40282306E+38");  
			 FormatStringTest (191, -3.402823E+38F, "G67", "-340282306000000000000000000000000000000");  
			 FormatStringTest (192, -3.402823E+38F, "N", "-340,282,300,000,000,000,000,000,000,000,000,000,000.00");  
			 FormatStringTest (193, -3.402823E+38F, "N0", "-340,282,300,000,000,000,000,000,000,000,000,000,000");  
			 FormatStringTest (194, -3.402823E+38F, "N1", "-340,282,300,000,000,000,000,000,000,000,000,000,000.0");  
			 FormatStringTest (195, -3.402823E+38F, "N2", "-340,282,300,000,000,000,000,000,000,000,000,000,000.00");  
			 FormatStringTest (196, -3.402823E+38F, "N3", "-340,282,300,000,000,000,000,000,000,000,000,000,000.000");  
			 FormatStringTest (197, -3.402823E+38F, "N4", "-340,282,300,000,000,000,000,000,000,000,000,000,000.0000");  
			 FormatStringTest (198, -3.402823E+38F, "N5", "-340,282,300,000,000,000,000,000,000,000,000,000,000.00000");  
			 FormatStringTest (199, -3.402823E+38F, "N6", "-340,282,300,000,000,000,000,000,000,000,000,000,000.000000");  
			 FormatStringTest (200, -3.402823E+38F, "N7", "-340,282,300,000,000,000,000,000,000,000,000,000,000.0000000");  
			 FormatStringTest (201, -3.402823E+38F, "N8", "-340,282,300,000,000,000,000,000,000,000,000,000,000.00000000");  
			 FormatStringTest (202, -3.402823E+38F, "N9", "-340,282,300,000,000,000,000,000,000,000,000,000,000.000000000");  
			 FormatStringTest (203, -3.402823E+38F, "N67", "-340,282,300,000,000,000,000,000,000,000,000,000,000.0000000000000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (204, -3.402823E+38F, "P", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.00 %");  
			 FormatStringTest (205, -3.402823E+38F, "P0", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000 %");  
			 FormatStringTest (206, -3.402823E+38F, "P1", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.0 %");  
			 FormatStringTest (207, -3.402823E+38F, "P2", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.00 %");  
			 FormatStringTest (208, -3.402823E+38F, "P3", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.000 %");  
			 FormatStringTest (209, -3.402823E+38F, "P4", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.0000 %");  
			 FormatStringTest (210, -3.402823E+38F, "P5", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.00000 %");  
			 FormatStringTest (211, -3.402823E+38F, "P6", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.000000 %");  
			 FormatStringTest (212, -3.402823E+38F, "P7", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.0000000 %");  
			 FormatStringTest (213, -3.402823E+38F, "P8", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.00000000 %");  
			 FormatStringTest (214, -3.402823E+38F, "P9", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.000000000 %");  
			 FormatStringTest (215, -3.402823E+38F, "P67", "-34,028,230,000,000,000,000,000,000,000,000,000,000,000.0000000000000000000000000000000000000000000000000000000000000000000 %");  
			 FormatStringTest (216, 1E-10F, "C", "$0.00");  
			 FormatStringTest (217, 1E-10F, "C0", "$0");  
			 FormatStringTest (218, 1E-10F, "C1", "$0.0");  
			 FormatStringTest (219, 1E-10F, "C2", "$0.00");  
			 FormatStringTest (220, 1E-10F, "C3", "$0.000");  
			 FormatStringTest (221, 1E-10F, "C4", "$0.0000");  
			 FormatStringTest (222, 1E-10F, "C5", "$0.00000");  
			 FormatStringTest (223, 1E-10F, "C6", "$0.000000");  
			 FormatStringTest (224, 1E-10F, "C7", "$0.0000000");  
			 FormatStringTest (225, 1E-10F, "C8", "$0.00000000");  
			 FormatStringTest (226, 1E-10F, "C9", "$0.000000000");  
			 FormatStringTest (227, 1E-10F, "C67", "$0.0000000001000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (228, 1E-10F, "E", "1.000000E-010");  
			 FormatStringTest (229, 1E-10F, "E0", "1E-010");  
			 FormatStringTest (230, 1E-10F, "E1", "1.0E-010");  
			 FormatStringTest (231, 1E-10F, "E2", "1.00E-010");  
			 FormatStringTest (232, 1E-10F, "E3", "1.000E-010");  
			 FormatStringTest (233, 1E-10F, "E4", "1.0000E-010");  
			 FormatStringTest (234, 1E-10F, "E5", "1.00000E-010");  
			 FormatStringTest (235, 1E-10F, "E6", "1.000000E-010");  
			 FormatStringTest (236, 1E-10F, "E7", "1.0000000E-010");  
			 FormatStringTest (237, 1E-10F, "E8", "1.00000001E-010");  
			 FormatStringTest (238, 1E-10F, "E9", "1.000000010E-010");  
			 FormatStringTest (239, 1E-10F, "E67", "1.0000000100000000000000000000000000000000000000000000000000000000000E-010");  
			 FormatStringTest (240, 1E-10F, "F", "0.00");  
			 FormatStringTest (241, 1E-10F, "F0", "0");  
			 FormatStringTest (242, 1E-10F, "F1", "0.0");  
			 FormatStringTest (243, 1E-10F, "F2", "0.00");  
			 FormatStringTest (244, 1E-10F, "F3", "0.000");  
			 FormatStringTest (245, 1E-10F, "F4", "0.0000");  
			 FormatStringTest (246, 1E-10F, "F5", "0.00000");  
			 FormatStringTest (247, 1E-10F, "F6", "0.000000");  
			 FormatStringTest (248, 1E-10F, "F7", "0.0000000");  
			 FormatStringTest (249, 1E-10F, "F8", "0.00000000");  
			 FormatStringTest (250, 1E-10F, "F9", "0.000000000");  
			 FormatStringTest (251, 1E-10F, "F67", "0.0000000001000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (252, 1E-10F, "G", "1E-10");  
			 FormatStringTest (253, 1E-10F, "G0", "1E-10");  
			 FormatStringTest (254, 1E-10F, "G1", "1E-10");  
			 FormatStringTest (255, 1E-10F, "G2", "1E-10");  
			 FormatStringTest (256, 1E-10F, "G3", "1E-10");  
			 FormatStringTest (257, 1E-10F, "G4", "1E-10");  
			 FormatStringTest (258, 1E-10F, "G5", "1E-10");  
			 FormatStringTest (259, 1E-10F, "G6", "1E-10");  
			 FormatStringTest (260, 1E-10F, "G7", "1E-10");  
			 FormatStringTest (261, 1E-10F, "G8", "1E-10");  
			 FormatStringTest (262, 1E-10F, "G9", "1.00000001E-10");  
			 FormatStringTest (263, 1E-10F, "G67", "1.00000001E-10");  
			 FormatStringTest (264, 1E-10F, "N", "0.00");  
			 FormatStringTest (265, 1E-10F, "N0", "0");  
			 FormatStringTest (266, 1E-10F, "N1", "0.0");  
			 FormatStringTest (267, 1E-10F, "N2", "0.00");  
			 FormatStringTest (268, 1E-10F, "N3", "0.000");  
			 FormatStringTest (269, 1E-10F, "N4", "0.0000");  
			 FormatStringTest (270, 1E-10F, "N5", "0.00000");  
			 FormatStringTest (271, 1E-10F, "N6", "0.000000");  
			 FormatStringTest (272, 1E-10F, "N7", "0.0000000");  
			 FormatStringTest (273, 1E-10F, "N8", "0.00000000");  
			 FormatStringTest (274, 1E-10F, "N9", "0.000000000");  
			 FormatStringTest (275, 1E-10F, "N67", "0.0000000001000000000000000000000000000000000000000000000000000000000");  
			 FormatStringTest (276, 1E-10F, "P", "0.00 %");  
			 FormatStringTest (277, 1E-10F, "P0", "0 %");  
			 FormatStringTest (278, 1E-10F, "P1", "0.0 %");  
			 FormatStringTest (279, 1E-10F, "P2", "0.00 %");  
			 FormatStringTest (280, 1E-10F, "P3", "0.000 %");  
			 FormatStringTest (281, 1E-10F, "P4", "0.0000 %");  
			 FormatStringTest (282, 1E-10F, "P5", "0.00000 %");  
			 FormatStringTest (283, 1E-10F, "P6", "0.000000 %");  
			 FormatStringTest (284, 1E-10F, "P7", "0.0000000 %");  
			 FormatStringTest (285, 1E-10F, "P8", "0.00000001 %");  
			 FormatStringTest (286, 1E-10F, "P9", "0.000000010 %");  
			 FormatStringTest (287, 1E-10F, "P67", "0.0000000100000000000000000000000000000000000000000000000000000000000 %");  
		}
	}
}
		

