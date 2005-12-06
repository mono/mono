//
// dbcs-table-generator.cs : generates big5.table or gb2312-new.table
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc. http://www.novell.com
//
// where CP936.TXT and CP950.TXT is found at:
// http://ftp.unicode.org/Public/MAPPINGS/VENDORS/MICSFT/WINDOWS/CP950.TXT
//
// (don't use BIG5.TXT - it is obsoleted. GB2312.TXT does not exist anymore.)
//
using System;
using System.Globalization;
using System.IO;

public class DbcsTableGenerator
{
	public static void Main (string [] args)
	{
		if (args.Length == 1) {
			switch (args [0]) {
			case "BIG5":
				Main (new string [] {
					"CP950.TXT", "big5.table", "A1", "43"});
				return;
			case "GB2312":
				Main (new string [] {
					"CP936.TXT", "gb2312-new.table", "81", "5E"});
				return;
			}
		}

		if (args.Length < 3) {
			Console.Error.WriteLine (@"
usage1: dbcs-table-generator.exe BIG5

usage2: dbcs-table-generator.exe GB2312

usage3: dbcs-table-generator.exe CPxxx.TXT xxx.table from [len]

from: BIG5 (CP950) = A1, GB2312 (CP936) = 81
len: optional. length of array. BIG5: 43");
			return;
		}
		byte upper = byte.Parse (args [2], NumberStyles.HexNumber);

		int [] n2u = new int [0x10000];
		int [] u2n = new int [0x10000];

		StreamReader reader = new StreamReader (args [0]);
		int native_min = upper << 8 + 0x40;
		int native_max = 0x10000;
		int native_count = native_max - native_min;

		if (args.Length > 3) {
			native_count = int.Parse (args [3], NumberStyles.HexNumber) << 8;
			native_max = native_min + native_count;
			Console.Error.WriteLine ("adjusted as count = {0:X04} max = {1:X04}", native_count, native_max);
		}

		int map_count = 0;
		for (int line = 1; reader.Peek () > 0; line++) {
			string s = reader.ReadLine ();
			int idx = s.IndexOf ('#');
			if (idx >= 0)
				s = s.Substring (0, idx).Trim ();
			if (s.Length == 0)
				continue;
			idx = s.IndexOf ('\t');
			if (idx < 0)
				continue;
			if (s.Length < 2 || s [0] != '0' || s [1] != 'x')
				throw new ArgumentException ("Unexpected line at " + line + " : " + s);
			int native = int.Parse (s.Substring (2, idx - 2).Trim (), NumberStyles.HexNumber);
			if (native < native_min)
				continue;
			int ordinal = ((int) (native / 0x100 - upper)) * 191 +
				(native % 0x100 - 0x40);

			s = s.Substring (idx + 1).Trim ();
			if (s.Length < 2 || s [0] != '0' || s [1] != 'x')
				throw new ArgumentException ("Unexpected line at " + line + " : " + s);
			int uni = int.Parse (s.Substring (2), NumberStyles.HexNumber);
			u2n [uni] = native;
			n2u [ordinal] = uni;
			map_count++;
		}

		FileStream output = File.OpenWrite (args [1]);
		output.Seek (0, SeekOrigin.Begin);

		WriteInt32 (output, 1);
		WriteInt32 (output, native_count * 2);
		for (int i = 0; i < native_count; i++) {
			output.WriteByte ((byte) (n2u [i] % 256));
			output.WriteByte ((byte) (n2u [i] / 256));
		}

		int uni_max = 0x10000;
		WriteInt32 (output, 2);
		WriteInt32 (output, uni_max * 2);
		for (int i = 0; i < uni_max; i++) {
			output.WriteByte ((byte) (u2n [i] % 256));
			output.WriteByte ((byte) (u2n [i] / 256));
		}

		output.Close ();
	}

	static void WriteInt32 (Stream s, int value)
	{
		for (int i = 0; i < 4; i++) {
			s.WriteByte ((byte) (value % 0x100));
			value /= 0x100;
		}
	}
}

