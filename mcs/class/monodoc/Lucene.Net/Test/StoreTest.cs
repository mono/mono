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
using InputStream = Lucene.Net.Store.InputStream;
using OutputStream = Lucene.Net.Store.OutputStream;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
namespace Lucene.Net
{
	
	class StoreTest
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			try
			{
				Test(1000, true);
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
			}
		}
		
		public static void  Test(int count, bool ram)
		{
			System.Random gen = new System.Random((System.Int32) 1251971);
			int i;
			
			System.DateTime veryStart = System.DateTime.Now;
			System.DateTime start = System.DateTime.Now;
			
			Directory store;
			if (ram)
				store = new RAMDirectory();
			else
				store = FSDirectory.GetDirectory("test.store", true);
			
			int LENGTH_MASK = 0xFFF;
			
			for (i = 0; i < count; i++)
			{
				System.String name = i + ".dat";
				int length = gen.Next() & LENGTH_MASK;
				byte b = (byte) (gen.Next() & 0x7F);
				//System.out.println("filling " + name + " with " + length + " of " + b);
				
				OutputStream file = store.CreateFile(name);
				
				for (int j = 0; j < length; j++)
					file.WriteByte(b);
				
				file.Close();
			}
			
			store.Close();
			
			System.DateTime end = System.DateTime.Now;
			
			System.Console.Out.Write(end.Ticks - start.Ticks);
			System.Console.Out.WriteLine(" total milliseconds to create");
			
			gen = new System.Random((System.Int32) 1251971);
			start = System.DateTime.Now;
			
			if (!ram)
				store = FSDirectory.GetDirectory("test.store", false);
			
			for (i = 0; i < count; i++)
			{
				System.String name = i + ".dat";
				int length = gen.Next() & LENGTH_MASK;
				sbyte b = (sbyte) (gen.Next() & 0x7F);
				//System.out.println("reading " + name + " with " + length + " of " + b);
				
				InputStream file = store.OpenFile(name);
				
				if (file.Length() != length)
					throw new System.Exception("length incorrect");
				
				for (int j = 0; j < length; j++)
					if (file.ReadByte() != b)
						throw new System.Exception("contents incorrect");
				
				file.Close();
			}
			
			end = System.DateTime.Now;
			
			System.Console.Out.Write(end.Ticks - start.Ticks);
			System.Console.Out.WriteLine(" total milliseconds to read");
			
			gen = new System.Random((System.Int32) 1251971);
			start = System.DateTime.Now;
			
			for (i = 0; i < count; i++)
			{
				System.String name = i + ".dat";
				//System.out.println("deleting " + name);
				store.DeleteFile(name);
			}
			
			end = System.DateTime.Now;
			
			System.Console.Out.Write(end.Ticks - start.Ticks);
			System.Console.Out.WriteLine(" total milliseconds to delete");
			
			System.Console.Out.Write(end.Ticks - veryStart.Ticks);
			System.Console.Out.WriteLine(" total milliseconds");
			
			store.Close();
		}
	}
}