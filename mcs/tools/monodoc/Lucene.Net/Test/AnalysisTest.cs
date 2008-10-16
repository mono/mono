/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using Analyzer = Lucene.Net.Analysis.Analyzer;
using SimpleAnalyzer = Lucene.Net.Analysis.SimpleAnalyzer;
using Token = Lucene.Net.Analysis.Token;
using TokenStream = Lucene.Net.Analysis.TokenStream;
namespace Lucene.Net
{
	
	class AnalysisTest
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			try
			{
				Test("This is a test", true);
				// FIXME: OG: what's with this hard-coded file name??
				Test(new System.IO.FileInfo("words.txt"), false);
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
			}
		}
		
		internal static void  Test(System.IO.FileInfo file, bool verbose)
		{
			long bytes = file.Length;
			System.Console.Out.WriteLine(" Reading test file containing " + bytes + " bytes.");
			
			System.IO.FileStream is_Renamed = new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			System.IO.StreamReader ir = new System.IO.StreamReader(new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(is_Renamed, System.Text.Encoding.Default).CurrentEncoding);
			
			Test(ir, verbose, bytes);
			
			ir.Close();
		}
		
		internal static void  Test(System.String text, bool verbose)
		{
			System.Console.Out.WriteLine(" Tokenizing string: " + text);
			Test(new System.IO.StringReader(text), verbose, text.Length);
		}
		
		internal static void  Test(System.IO.TextReader reader, bool verbose, long bytes)
		{
			Analyzer analyzer = new SimpleAnalyzer();
			TokenStream stream = analyzer.TokenStream(null, reader);
			
			System.DateTime start = System.DateTime.Now;
			
			int count = 0;
			for (Token t = stream.Next(); t != null; t = stream.Next())
			{
				if (verbose)
				{
					System.Console.Out.WriteLine("Text=" + t.TermText() + " start=" + t.StartOffset() + " end=" + t.EndOffset());
				}
				count++;
			}
			
			System.DateTime end = System.DateTime.Now;
			
			long time = end.Ticks - start.Ticks;
			System.Console.Out.WriteLine(time + " milliseconds to extract " + count + " tokens");
			System.Console.Out.WriteLine((time * 1000.0) / count + " microseconds/token");
			System.Console.Out.WriteLine((bytes * 1000.0 * 60.0 * 60.0) / (time * 1000000.0) + " megabytes/hour");
		}
	}
}