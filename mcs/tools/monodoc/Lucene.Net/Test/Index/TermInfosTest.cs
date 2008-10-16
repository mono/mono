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
using Directory = Lucene.Net.Store.Directory;
using FSDirectory = Lucene.Net.Store.FSDirectory;
namespace Lucene.Net.Index
{
	class TermInfosTest
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			try
			{
				Test();
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
			}
		}
		
		// FIXME: OG: remove hard-coded file names
		public static void  Test()
		{
			
			System.IO.FileInfo file = new System.IO.FileInfo("words.txt");
			System.Console.Out.WriteLine(" reading word file containing " + file.Length + " bytes");
			
			System.DateTime start = System.DateTime.Now;
			
			System.Collections.ArrayList keys = System.Collections.ArrayList.Synchronized(new System.Collections.ArrayList(10));
			System.IO.FileStream ws = new System.IO.FileStream(file.FullName, System.IO.FileMode.Open, System.IO.FileAccess.Read);
			System.IO.StreamReader wr = new System.IO.StreamReader(new System.IO.StreamReader(ws, System.Text.Encoding.Default).BaseStream, new System.IO.StreamReader(ws, System.Text.Encoding.Default).CurrentEncoding);
			
			for (System.String key = wr.ReadLine(); key != null; key = wr.ReadLine())
				keys.Add(new Term("word", key));
			wr.Close();
			
			System.DateTime end = System.DateTime.Now;
			
			System.Console.Out.Write(end.Ticks - start.Ticks);
			System.Console.Out.WriteLine(" milliseconds to read " + keys.Count + " words");
			
			start = System.DateTime.Now;
			
			System.Random gen = new System.Random((System.Int32) 1251971);
			long fp = (gen.Next() & 0xF) + 1;
			long pp = (gen.Next() & 0xF) + 1;
			int[] docFreqs = new int[keys.Count];
			long[] freqPointers = new long[keys.Count];
			long[] proxPointers = new long[keys.Count];
			for (int i = 0; i < keys.Count; i++)
			{
				docFreqs[i] = (gen.Next() & 0xF) + 1;
				freqPointers[i] = fp;
				proxPointers[i] = pp;
				fp += (gen.Next() & 0xF) + 1;
				;
				pp += (gen.Next() & 0xF) + 1;
				;
			}
			
			end = System.DateTime.Now;
			
			System.Console.Out.Write(end.Ticks - start.Ticks);
			System.Console.Out.WriteLine(" milliseconds to generate values");
			
			start = System.DateTime.Now;
			
			Directory store = FSDirectory.GetDirectory("test.store", true);
			FieldInfos fis = new FieldInfos();
			
			TermInfosWriter writer = new TermInfosWriter(store, "words", fis);
			fis.Add("word", false);
			
			for (int i = 0; i < keys.Count; i++)
				writer.Add((Term) keys[i], new TermInfo(docFreqs[i], freqPointers[i], proxPointers[i]));
			
			writer.Close();
			
			end = System.DateTime.Now;
			
			System.Console.Out.Write(end.Ticks - start.Ticks);
			System.Console.Out.WriteLine(" milliseconds to write table");
			
			System.Console.Out.WriteLine(" table occupies " + store.FileLength("words.tis") + " bytes");
			
			start = System.DateTime.Now;
			
			TermInfosReader reader = new TermInfosReader(store, "words", fis);
			
			end = System.DateTime.Now;
			
			System.Console.Out.Write(end.Ticks - start.Ticks);
			System.Console.Out.WriteLine(" milliseconds to open table");
			
			start = System.DateTime.Now;
			
			SegmentTermEnum enumerator = reader.Terms();
			for (int i = 0; i < keys.Count; i++)
			{
				enumerator.Next();
				Term key = (Term) keys[i];
				if (!key.Equals(enumerator.Term()))
				{
					throw new System.Exception("wrong term: " + enumerator.Term() + ", expected: " + key + " at " + i);
				}
				TermInfo ti = enumerator.TermInfo();
				if (ti.docFreq != docFreqs[i])
					throw new System.Exception("wrong value: " + System.Convert.ToString(ti.docFreq, 16) + ", expected: " + System.Convert.ToString(docFreqs[i], 16) + " at " + i);
				if (ti.freqPointer != freqPointers[i])
					throw new System.Exception("wrong value: " + System.Convert.ToString(ti.freqPointer, 16) + ", expected: " + System.Convert.ToString(freqPointers[i], 16) + " at " + i);
				if (ti.proxPointer != proxPointers[i])
					throw new System.Exception("wrong value: " + System.Convert.ToString(ti.proxPointer, 16) + ", expected: " + System.Convert.ToString(proxPointers[i], 16) + " at " + i);
			}
			
			end = System.DateTime.Now;
			
			System.Console.Out.Write(end.Ticks - start.Ticks);
			System.Console.Out.WriteLine(" milliseconds to iterate over " + keys.Count + " words");
			
			start = System.DateTime.Now;
			
			for (int i = 0; i < keys.Count; i++)
			{
				Term key = (Term) keys[i];
				TermInfo ti = reader.Get(key);
				if (ti.docFreq != docFreqs[i])
					throw new System.Exception("wrong value: " + System.Convert.ToString(ti.docFreq, 16) + ", expected: " + System.Convert.ToString(docFreqs[i], 16) + " at " + i);
				if (ti.freqPointer != freqPointers[i])
					throw new System.Exception("wrong value: " + System.Convert.ToString(ti.freqPointer, 16) + ", expected: " + System.Convert.ToString(freqPointers[i], 16) + " at " + i);
				if (ti.proxPointer != proxPointers[i])
					throw new System.Exception("wrong value: " + System.Convert.ToString(ti.proxPointer, 16) + ", expected: " + System.Convert.ToString(proxPointers[i], 16) + " at " + i);
			}
			
			end = System.DateTime.Now;
			
			System.Console.Out.Write((end.Ticks - start.Ticks) / (float) keys.Count);
			System.Console.Out.WriteLine(" average milliseconds per lookup");
			
			TermEnum e = reader.Terms(new Term("word", "azz"));
			System.Console.Out.WriteLine("Word after azz is " + e.Term().text);
			
			reader.Close();
			
			store.Close();
		}
	}
}