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
using IndexReader = Lucene.Net.Index.IndexReader;
using Term = Lucene.Net.Index.Term;
using Directory = Lucene.Net.Store.Directory;
using FSDirectory = Lucene.Net.Store.FSDirectory;
namespace Lucene.Net.Demo
{
	
	class DeleteFiles
	{
		[STAThread]
		public static void  Main(System.String[] args)
		{
			try
			{
				Directory directory = FSDirectory.GetDirectory("demo index", false);
				IndexReader reader = IndexReader.Open(directory);
				
				//       Term term = new Term("path", "pizza");
				//       int deleted = reader.delete(term);
				
				//       System.out.println("deleted " + deleted +
				// 			 " documents containing " + term);
				
				for (int i = 0; i < reader.MaxDoc(); i++)
					reader.Delete(i);
				
				reader.Close();
				directory.Close();
			}
			catch (System.Exception e)
			{
				System.Console.Out.WriteLine(" caught a " + e.GetType() + "\n with message: " + e.Message);
			}
		}
	}
}