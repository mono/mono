//	monostyle.cs
//
//	Adam Treat (manyoso@yahoo.com)
//	Ben Maurer (bmaurer@users.sf.net)
//	(C) 2002 Adam Treat
//	(C) 2003 Ben Maurer
//
//	This program is free software; you can redistribute it and/or modify
//	it under the terms of the GNU General Public License as published by
//	the Free Software Foundation; either version 2 of the License, or
//	(at your option) any later version.

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Specialized;

namespace Mono.Util {

	class MonoStyle {

		string file;
		StringCollection filebuffer;
		bool linespace = true;

		void Usage()
		{
			Console.Write (
				"monostyle -f file.cs -l <true|false> > output.cs\n\n" +
				"   -f || /-f || --file  file.cs        The csharp source file to parse.\n\n" +
				"   -l || /-l || --line  <true|false>   Specifies wether to use line spacing.\n\n");
		}

		public static void Main (string[] args)
		{
			MonoStyle style = new MonoStyle(args);
		}

		public MonoStyle (string[] args)
		{
			int argc = args.Length;
			for(int i = 0; i < argc; i++) {
				string arg = args[i];
				// The "/" switch is there for wine users, like me ;-)
				if(arg.StartsWith("-") || arg.StartsWith("/")) {
					switch(arg) {
						case "-l": case "/-l": case "--line":
						if((i + 1) >= argc) {
							Usage();
							return;
						}
						if (args[i++] == "false") {
							linespace = false;
						}
						continue;
						case "-f": case "/-f": case "--file":
						if((i + 1) >= argc) {
							Usage();
							return;
						}
						file = args[++i];
						continue;
						default:
						Usage();
						return;
					}
				}
			}
			if(file == null) {
				Usage();
				return;
			}
			filebuffer = new StringCollection();
			StreamReader sr = new StreamReader(file);
			FillBuffer(sr);
			FixMonoStyle();
			PrintToConsole();
		}

		public void FillBuffer(StreamReader sr)
		{
			sr.BaseStream.Seek(0, SeekOrigin.Begin);
			while (sr.Peek() > -1) {
				filebuffer.Add(sr.ReadLine());
			}
			sr.Close();
		}

		public void FixMonoStyle()
		{
			for (int i=0; i < filebuffer.Count; i++) {
				IsBadMonoStyle(filebuffer[i]);
			}
		}

		public void PrintToConsole()
		{
			for (int i=0; i < filebuffer.Count; i++) {
				Console.WriteLine(filebuffer[i]);
			}
		}

		public void IsBadMonoStyle(String str)
		{
			if (IsBadMonoType(str)) {
				FixHangingBrace(str);
			} else if(IsBadMonoFlow(str)) {
				FixHangingBrace(str);
			} else if(IsBadMonoFunction(str)) {
				FixEndBrace(str);
			} else if(IsBadMonoProperty(str)) {
				FixHangingBrace(str);
			} else {
			}
   		}

		public void FixHangingBrace(String str)
		{
			int strloc = filebuffer.IndexOf(str);
			int brcloc = FindHangingBrace(strloc);
			int diff = brcloc - strloc;
			if (brcloc > 0) {
				for (int i = 0; i < diff+1; i++) {
					filebuffer.RemoveAt(strloc);
				}
				filebuffer.Insert(strloc, str + " {");
				if (linespace) {
					filebuffer.Insert(strloc+1, "");
				}
			} else {}
		}

		public int FindHangingBrace(int strloc)
		{
			strloc++;
			bool found = false;
			while (!found) {
				try {
					string str = filebuffer[strloc++];
					found = IsHangingBrace(str);
					if (!found && !IsBlankLine(str)) {
						return -1;
					}
				} catch (Exception) {
					return -1;
				}
			}
			return strloc -1;
   		}

		public void FixEndBrace(String str)
		{
			int strloc = filebuffer.IndexOf(str);
			filebuffer.RemoveAt(strloc);
			filebuffer.Insert(strloc, RemoveEndBrace(str));
			filebuffer.Insert(strloc+1, AddHangingBrace(str));
		}

		public static bool IsBadMonoType(String str)
		{
			if ( IsType(str) && !EndWithBrace(str)) {
				return true;
			} else {
				return false;
			}
   		}

		public static bool IsBadMonoFlow(String str)
		{
			if (IsFlow(str) && !EndWithBrace(str)) {
				return true;
			} else {
				return false;
			}
   		}

		public static bool IsBadMonoFunction(String str)
		{
			if (IsFunction(str) && EndWithBrace(str)) {
				return true;
			} else {
				return false;
			}
		}

		public static bool IsBadMonoProperty(String str)
		{
			if (IsProperty(str) && !EndWithBrace(str)) {
				return true;
			} else {
				return false;
			}
		}

		public static bool IsType(String str)
		{
			if (	!IsComment(str) && (
					IsNameSpace(str) ||
					IsClass(str) ||
					IsStruct(str) ||
					IsEnum(str) )) {
				return true;
			} else {
				return false;
			}
   		}

		public static bool IsFlow(String str)
		{
			if (	!IsComment(str) && (
					IsIf(str) ||
					IsElse(str) ||
					IsElseIf(str) ||
					IsTry(str) ||
					IsCatch(str) ||
					IsFinally(str) ||
					IsFor(str) ||
					IsForEach(str) ||
					IsWhile(str) ||
					IsSwitch(str)
					)) {
				return true;
			} else {
				return false;
			}
   		}

		public static bool IsFunction(String str)
		{
			if (	Regex.IsMatch(str, @"^\s*(\w+)\s+(\w+).*\(+") &&
					!IsDeclaration(str) &&
					!IsComment(str) &&
					!IsType(str) &&
					!IsFlow(str) ) {
				return true;
			} else {
				return false;
			}
		}

		public static bool IsProperty(String str)
		{
			if (	Regex.IsMatch(str, @"^\s*(\w+)\s+(\w+).*") &&
					!IsDeclaration(str) &&
					!IsComment(str) &&
					!IsType(str) &&
					!IsFlow(str) &&
					!IsFunction(str) ) {
				return true;
			} else {
				return false;
			}
		}

		public static string RemoveEndBrace(String str)
		{
			Regex rg = new Regex(@"\{\s*$");
			return rg.Replace(str, "");
   		}

		public static string AddHangingBrace(String str)
		{
			Regex rg = new Regex(@"\S+\s*");
			string blank = rg.Replace(str,"");
			return blank + "{";
   		}

		public static bool IsDeclaration(String str)
		{
			return Regex.IsMatch(str, @"\;\s*$");
		}

		public static bool IsComment(String str)
		{
			return Regex.IsMatch(str, @"^(\s*\/+|\s*\*+|\s*\#+)");
   		}

		public static bool EndWithBrace(String str)
		{
			return Regex.IsMatch(str, @"\{\s*$");
   		}

		public static bool IsHangingBrace(String str)
		{
			return Regex.IsMatch(str, @"^\s*\{");
   		}

		public static bool IsBlankLine(String str)
		{
			return Regex.IsMatch(str, @"^\s*$");
   		}

		public static bool IsNameSpace(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+)namespace\s+");
   		}

		public static bool IsClass(String str)
		{
			return Regex.IsMatch(str, @"\s+class\s+");
   		}

		public static bool IsStruct(String str)
		{
			return Regex.IsMatch(str, @"\s+struct\s+");
   		}

		public static bool IsEnum(String str)
		{
			return Regex.IsMatch(str, @"\s+enum\s+");
   		}

		public static bool IsIf(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)if(\s+|\(+|$)");
   		}

		public static bool IsElse(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)else(\s+|\{+|$)");
   		}

		public static bool IsElseIf(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)else if(\s+|\(+|$)");
   		}

		public static bool IsTry(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)try(\s+|\(+|$)");
   		}

		public static bool IsCatch(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)catch(\s+|\(+|$)");
   		}

		public static bool IsFinally(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)finally(\s+|\{+|$)");
   		}

		public static bool IsFor(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)for(\s+|\(+|$)");
   		}

		public static bool IsForEach(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)foreach(\s+|\(+|$)");
   		}

		public static bool IsWhile(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)while(\s+|\(+|$)");
   		}

		public static bool IsSwitch(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)switch(\s+|\(+|$)");
   		}

		public static bool IsCase(String str)
		{
			return Regex.IsMatch(str, @"(^|\s+|\}+)case(\s+|\(+|$)");
   		}
	}
}
