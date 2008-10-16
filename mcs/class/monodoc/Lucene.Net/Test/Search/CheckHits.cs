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

/* 20 May 2004:   Factored out of spans tests. Please leave this comment
until this class is evt. also used by tests in search package.
*/
using System;
using NUnit.Framework;
namespace Lucene.Net.Search
{
    public class CheckHits
	{
		public static void  CheckHits_(Query query, System.String defaultFieldName, Searcher searcher, int[] results, TestCase testCase)
		{
            Hits hits = searcher.Search(query);
			
            System.Collections.Hashtable correct = new System.Collections.Hashtable();
            for (int i = 0; i < results.Length; i++)
            {
                correct.Add((System.Int32) results[i], null);
            }
			
            System.Collections.Hashtable actual = new System.Collections.Hashtable();
            for (int i = 0; i < hits.Length(); i++)
            {
                actual.Add((System.Int32) hits.Id(i), null);
            }
			
            //Assert.AreEqual(correct, actual, query.ToString(defaultFieldName));
            if (correct.Count != 0)
            {
                System.Collections.IDictionaryEnumerator iter = correct.GetEnumerator();
                bool status = false;
                while (iter.MoveNext())
                {
                    status = actual.ContainsKey(iter.Key);
                    if (status == false)
                        break;
                }
                Assert.IsTrue(status, query.ToString(defaultFieldName));
            }
        }
		
		public static void  PrintDocNrs(Hits hits)
		{
			System.Console.Out.Write("new int[] {");
			for (int i = 0; i < hits.Length(); i++)
			{
				System.Console.Out.Write(hits.Id(i));
				if (i != hits.Length() - 1)
					System.Console.Out.Write(", ");
			}
			System.Console.Out.WriteLine("}");
		}
	}
}