//
// IntegerFormatterTest.cs
//
// Author:
//   Derek Holden  (dholden@draper.com)
//
// (C) Derek Holden  dholden@draper.com
//

//
// Simple test for IntegerFormatter. 
// 
// Pull out IntegerFormatter.cs and NumberFormatInfo.cs and compile w/ 
// csc /nowarn:1595 /nowarn:0168 /out:test.exe *.cs
//

using System;
using System.Globalization;

namespace System {

	public class Test {	       

		public static void Main() {

			bool checkShorts = true; // false;
			bool checkInts = true; // false;
			bool checkLongs = true; // false;
			bool checkUShorts = true; // false;
			bool checkUInts = true; // false;
			bool checkULongs = true; // false;
			bool checkBytes = true; // false;
			bool checkSBytes = true; // false;
								     
			string[] formats = { 
				"C", "C0", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10", "C99",
				"D", "D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "D10", "D99",
				"E", "E0", "E1", "E2", "E3", "E4", "E5", "E6", "E7", "E8", "E9", "E10", "E99",
				"F", "F0", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F99",
				"G", "G0", "G1", "G2", "G3", "G4", "G5", "G6", "G7", "G8", "G9", "G10", "G99",
				"N", "N0", "N1", "N2", "N3", "N4", "N5", "N6", "N7", "N8", "N9", "N10", "N99",
				"P", "P0", "P1", "P2", "P3", "P4", "P5", "P6", "P7", "P8", "P9", "P10", "P99",
				//"R", "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9", "R10", "R99",
				"X", "X0", "X1", "X2", "X3", "X4", "X5", "X6", "X7", "X8", "X9", "X10", "X99"
			};			

			short[] shortBoundries = { Int16.MinValue, Int16.MaxValue };
			int[] intBoundries = { Int32.MinValue, Int32.MaxValue };
			long[] longBoundries =  { Int64.MinValue, Int64.MaxValue };
			ushort[] ushortBoundries = { UInt16.MinValue, UInt16.MaxValue };
			uint[] uintBoundries = { UInt32.MinValue, UInt32.MaxValue };
			ulong[] ulongBoundries =  { UInt64.MinValue, UInt64.MaxValue };
			byte[] byteBoundries = { Byte.MinValue, Byte.MaxValue };
			sbyte[] sbyteBoundries = { SByte.MinValue, SByte.MaxValue };

			int count;
			string s1, s2;
			NumberFormatInfo nfi = new NumberFormatInfo();

			// shorts
			
			if (checkShorts) {			
				count = 0;
				Console.WriteLine ("Checking shorts...");
				for (short n = -1000; n < 1000; n++) {
					for (int idx = 0; idx < formats.Length; idx++) {
						s1 = n.ToString (formats[idx]);
						s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
						if (!s2.Equals (s1)) {
							Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
									   n, formats[idx], s1, s2);
							count++;
						}
					}
				}
				
				Console.WriteLine ("Checking boundries...");
				foreach (short n in shortBoundries) {
					for (int idx = 0; idx < formats.Length; idx++) {
						try {
							s1 = n.ToString (formats[idx]);
							s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
							if (!s2.Equals (s1)) {
								Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
										   n, formats[idx], s1, s2);
								count++;
							}
						} catch (Exception e) {
							Console.WriteLine ("Exception Caught! Number: {0} Format: {1}", n, formats[idx]);
							count++;
						}
					}
				}

				if (count == 0) 
					Console.WriteLine ("Passed all tests\n");
				else Console.WriteLine ("Failed on {0} tests\n", count);
			}

			// ints
			
			if (checkInts) {
				count = 0;
				Console.WriteLine ("Checking ints...");
				for (int n = -10000; n < 10000; n++) {
					for (int idx = 0; idx < formats.Length; idx++) {
						try {
							s1 = n.ToString (formats[idx]);
							s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
							if (!s2.Equals (s1)) {
								Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
										   n, formats[idx], s1, s2);
								count++;
							}
						} catch (Exception e) {
							Console.WriteLine ("Exception Caught! Number: {0} Format: {1}", n, formats[idx]);
						}
					}
				}

				Console.WriteLine ("Checking boundries...");
				foreach (int n in intBoundries) {
					for (int idx = 0; idx < formats.Length; idx++) {
						try {
							s1 = n.ToString (formats[idx]);
							s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
							if (!s2.Equals (s1)) {
								Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
										   n, formats[idx], s1, s2);
								count++;
							}
						} catch (Exception e) {
							Console.WriteLine ("Exception Caught! Number: {0} Format: {1}", n, formats[idx]);
							count++;
						}
					}
				}

				if (count == 0) 
					Console.WriteLine ("Passed all tests\n");
				else Console.WriteLine ("Failed on {0} tests\n", count);
			}

			// longs

			if (checkLongs) {
				count = 0;
				Console.WriteLine ("Checking longs...");
				for (long n = -10000; n < 10000; n++) {
					for (int idx = 0; idx < formats.Length; idx++) {
						s1 = n.ToString (formats[idx]);
						s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
						if (!s2.Equals (s1)) {
							Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
									   n, formats[idx], s1, s2);
							count++;
						}
					}
				}

				Console.WriteLine ("Checking boundries...");
				foreach (long n in longBoundries) {
					for (int idx = 0; idx < formats.Length; idx++) {
						try {
							s1 = n.ToString (formats[idx]);
							s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
							if (!s2.Equals (s1)) {
								Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
										   n, formats[idx], s1, s2);
								count++;
							}
						} catch (Exception e) {
							Console.WriteLine ("Exception Caught! Number: {0} Format: {1}", n, formats[idx]);
							count++;
						}
					}
				}

				if (count == 0) 
					Console.WriteLine ("Passed all tests\n");
				else Console.WriteLine ("Failed on {0} tests\n", count);
			}

			// ushorts

			if (checkUShorts) {
				count = 0;
				Console.WriteLine ("Checking ushorts...");
				for (ushort n = 0; n < 1000; n++) {
					for (int idx = 0; idx < formats.Length; idx++) {
						s1 = n.ToString (formats[idx]);
						s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
						if (!s2.Equals (s1)) {
							Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
									   n, formats[idx], s1, s2);
							count++;
						}
					}
				}

				Console.WriteLine ("Checking boundries...");
				foreach (ushort n in shortBoundries) {
					for (int idx = 0; idx < formats.Length; idx++) {
						try {
							s1 = n.ToString (formats[idx]);
							s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
							if (!s2.Equals (s1)) {
								Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
										   n, formats[idx], s1, s2);
								count++;
							}
						} catch (Exception e) {
							Console.WriteLine ("Exception Caught! Number: {0} Format: {1}", n, formats[idx]);
							count++;
						}
					}
				}

				if (count == 0) 
					Console.WriteLine ("Passed all tests\n");
				else Console.WriteLine ("Failed on {0} tests\n", count);
			}

			// uint

			if (checkUInts) {
				count = 0;
				Console.WriteLine ("Checking uints...");
				for (uint n = 0; n < 10000; n++) {
					for (int idx = 0; idx < formats.Length; idx++) {
						s1 = n.ToString (formats[idx]);
						s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
						if (!s2.Equals (s1)) {
							Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
									   n, formats[idx], s1, s2);
							count++;
						}
					}
				}

				Console.WriteLine ("Checking boundries...");
				foreach (uint n in uintBoundries) {
					for (int idx = 0; idx < formats.Length; idx++) {
						try {
							s1 = n.ToString (formats[idx]);
							s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
							if (!s2.Equals (s1)) {
								Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
										   n, formats[idx], s1, s2);
								count++;
							}
						} catch (Exception e) {
							Console.WriteLine ("Exception Caught! Number: {0} Format: {1}", n, formats[idx]);
							count++;
						}
					}
				}

				if (count == 0) 
					Console.WriteLine ("Passed all tests\n");
				else Console.WriteLine ("Failed on {0} tests\n", count);
			}

			// ulong
			
			if (checkULongs) {
				count = 0;
				Console.WriteLine ("Checking ulongs...");
				for (ulong n = 0; n < 10000; n++) {
					for (int idx = 0; idx < formats.Length; idx++) {
						s1 = n.ToString (formats[idx]);
						s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
						if (!s2.Equals (s1)) {
							Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
									   n, formats[idx], s1, s2);
							count++;
						}
					}
				}

				Console.WriteLine ("Checking boundries...");
				foreach (ulong n in ulongBoundries) {
					for (int idx = 0; idx < formats.Length; idx++) {
						try {
							s1 = n.ToString (formats[idx]);
							s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
							if (!s2.Equals (s1)) {
								Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
										   n, formats[idx], s1, s2);
								count++;
							}
						} catch (Exception e) {
							Console.WriteLine ("Exception Caught! Number: {0} Format: {1}", n, formats[idx]);
							count++;
						}
					}
				}

				if (count == 0) 
					Console.WriteLine ("Passed all tests\n");
				else Console.WriteLine ("Failed on {0} tests\n", count);
			}

			// bytes

			if (checkBytes) {
				count = 0;
				Console.WriteLine ("Checking bytes...");
				for (byte n = 0; n < 100; n++) {
					for (int idx = 0; idx < formats.Length; idx++) {
						s1 = n.ToString (formats[idx]);
						s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
						if (!s2.Equals (s1)) {
							Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
									   n, formats[idx], s1, s2);
							count++;
						}
					}
				}

				Console.WriteLine ("Checking boundries...");
				foreach (byte n in byteBoundries) {
					for (int idx = 0; idx < formats.Length; idx++) {
						try {
							s1 = n.ToString (formats[idx]);
							s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
							if (!s2.Equals (s1)) {
								Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
										   n, formats[idx], s1, s2);
								count++;
							}
						} catch (Exception e) {
							Console.WriteLine ("Exception Caught! Number: {0} Format: {1}", n, formats[idx]);
							count++;
						}
					}
				}

				if (count == 0) 
					Console.WriteLine ("Passed all tests\n");
				else Console.WriteLine ("Failed on {0} tests\n", count);
			}

			// sbytes

			if (checkSBytes) {
				count = 0;
				Console.WriteLine ("Checking sbytes...");
				for (sbyte n = -100; n < 100; n++) {
					for (int idx = 0; idx < formats.Length; idx++) {
						s1 = n.ToString (formats[idx]);
						s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
						if (!s2.Equals (s1)) {
							Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
									   n, formats[idx], s1, s2);
							count++;
						}
					}
				}

				Console.WriteLine ("Checking boundries...");
				foreach (sbyte n in sbyteBoundries) {
					for (int idx = 0; idx < formats.Length; idx++) {
						try {
							s1 = n.ToString (formats[idx]);
							s2 = IntegerFormatter.NumberToString (formats[idx], nfi, n);
							if (!s2.Equals (s1)) {
								Console.WriteLine ("Number: {0} Format: {1} MS: ({2}) Me: ({3})", 
										   n, formats[idx], s1, s2);
								count++;
							}
						} catch (Exception e) {
							Console.WriteLine ("Exception Caught! Number: {0} Format: {1}", n, formats[idx]);
							count++;
						}
					}
				}

				if (count == 0) 
					Console.WriteLine ("Passed all tests\n");
				else Console.WriteLine ("Failed on {0} tests\n", count); 
			}
		}		
	}
}
