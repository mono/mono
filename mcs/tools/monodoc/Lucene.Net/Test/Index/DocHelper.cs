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
/// <summary> Created by IntelliJ IDEA.
/// User: Grant Ingersoll
/// Date: Feb 2, 2004
/// Time: 6:16:12 PM
/// $Id: DocHelper.java,v 1.1 2004/02/20 20:14:55 cutting Exp $
/// Copyright 2004.  Center For Natural Language Processing
/// </summary>
using System;
using Analyzer = Lucene.Net.Analysis.Analyzer;
using WhitespaceAnalyzer = Lucene.Net.Analysis.WhitespaceAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using Similarity = Lucene.Net.Search.Similarity;
using Directory = Lucene.Net.Store.Directory;
namespace Lucene.Net.Index
{
	
	/// <summary> 
	/// 
	/// 
	/// </summary>
	class DocHelper
	{
		public const System.String FIELD_1_TEXT = "Field one text";
		public const System.String TEXT_FIELD_1_KEY = "textField1";
		public static Field textField1;
		
		public const System.String FIELD_2_TEXT = "Field Field Field two text";
		public static readonly int[] FIELD_2_FREQS = new int[]{3, 1, 1};
		public const System.String TEXT_FIELD_2_KEY = "textField2";
		public static Field textField2;
		
		public const System.String KEYWORD_TEXT = "Keyword";
		public const System.String KEYWORD_FIELD_KEY = "keyField";
		public static Field keyField;
		
		public const System.String UNINDEXED_FIELD_TEXT = "unindexed Field text";
		public const System.String UNINDEXED_FIELD_KEY = "unIndField";
		public static Field unIndField;
		
		public const System.String UNSTORED_1_FIELD_TEXT = "unstored Field text";
		public const System.String UNSTORED_FIELD_1_KEY = "unStoredField1";
		public static Field unStoredField1;
		
		public const System.String UNSTORED_2_FIELD_TEXT = "unstored Field text";
		public const System.String UNSTORED_FIELD_2_KEY = "unStoredField2";
		public static Field unStoredField2;
		
		//  public static Set fieldNamesSet = null;
		//  public static Set fieldValuesSet = null;
		public static System.Collections.IDictionary nameValues = null;
		
		/// <summary> Adds the fields above to a document </summary>
		/// <param name="doc">The document to write
		/// </param>
		public static void  SetupDoc(Document doc)
		{
			doc.Add(textField1);
			doc.Add(textField2);
			doc.Add(keyField);
			doc.Add(unIndField);
			doc.Add(unStoredField1);
			doc.Add(unStoredField2);
		}
		/// <summary> Writes the document to the directory using a segment named "test"</summary>
		/// <param name="">dir
		/// </param>
		/// <param name="">doc
		/// </param>
		public static void  WriteDoc(Directory dir, Document doc)
		{
			
			WriteDoc(dir, "test", doc);
		}
		/// <summary> Writes the document to the directory in the given segment</summary>
		/// <param name="">dir
		/// </param>
		/// <param name="">segment
		/// </param>
		/// <param name="">doc
		/// </param>
		public static void  WriteDoc(Directory dir, System.String segment, Document doc)
		{
			Analyzer analyzer = new WhitespaceAnalyzer();
			Similarity similarity = Similarity.GetDefault();
			WriteDoc(dir, analyzer, similarity, segment, doc);
		}
		/// <summary> Writes the document to the directory segment named "test" using the specified analyzer and similarity</summary>
		/// <param name="">dir
		/// </param>
		/// <param name="">analyzer
		/// </param>
		/// <param name="">similarity
		/// </param>
		/// <param name="">doc
		/// </param>
		public static void  WriteDoc(Directory dir, Analyzer analyzer, Similarity similarity, Document doc)
		{
			WriteDoc(dir, analyzer, similarity, "test", doc);
		}
		/// <summary> Writes the document to the directory segment using the analyzer and the similarity score</summary>
		/// <param name="">dir
		/// </param>
		/// <param name="">analyzer
		/// </param>
		/// <param name="">similarity
		/// </param>
		/// <param name="">segment
		/// </param>
		/// <param name="">doc
		/// </param>
		public static void  WriteDoc(Directory dir, Analyzer analyzer, Similarity similarity, System.String segment, Document doc)
		{
			DocumentWriter writer = new DocumentWriter(dir, analyzer, similarity, 50);
			try
			{
				writer.AddDocument(segment, doc);
			}
			catch (System.IO.IOException e)
			{
				System.Console.Error.WriteLine(e.StackTrace);
			}
		}
		
		public static int NumFields(Document doc)
		{
            int result = 0;
            foreach (Field field in doc.Fields())
            {
                System.Object generatedAux = field;
                result++;
            }
            return result;
		}
		static DocHelper()
		{
			textField1 = Field.Text(TEXT_FIELD_1_KEY, FIELD_1_TEXT, false);
			textField2 = Field.Text(TEXT_FIELD_2_KEY, FIELD_2_TEXT, true);
			keyField = Field.Keyword(KEYWORD_FIELD_KEY, KEYWORD_TEXT);
			unIndField = Field.UnIndexed(UNINDEXED_FIELD_KEY, UNINDEXED_FIELD_TEXT);
			unStoredField1 = Field.UnStored(UNSTORED_FIELD_1_KEY, UNSTORED_1_FIELD_TEXT, false);
			unStoredField2 = Field.UnStored(UNSTORED_FIELD_2_KEY, UNSTORED_2_FIELD_TEXT, true);
			{
				
				nameValues = new System.Collections.Hashtable();
				nameValues[TEXT_FIELD_1_KEY] = FIELD_1_TEXT;
				nameValues[TEXT_FIELD_2_KEY] = FIELD_2_TEXT;
				nameValues[KEYWORD_FIELD_KEY] = KEYWORD_TEXT;
				nameValues[UNINDEXED_FIELD_KEY] = UNINDEXED_FIELD_TEXT;
				nameValues[UNSTORED_FIELD_1_KEY] = UNSTORED_1_FIELD_TEXT;
				nameValues[UNSTORED_FIELD_2_KEY] = UNSTORED_2_FIELD_TEXT;
			}
		}
	}
	/*
	fieldNamesSet = new HashSet();
	fieldNamesSet.add(TEXT_FIELD_1_KEY);
	fieldNamesSet.add(TEXT_FIELD_2_KEY);
	fieldNamesSet.add(KEYWORD_FIELD_KEY);
	fieldNamesSet.add(UNINDEXED_FIELD_KEY);
	fieldNamesSet.add(UNSTORED_FIELD_1_KEY);
	fieldNamesSet.add(UNSTORED_FIELD_2_KEY);
	fieldValuesSet = new HashSet();
	fieldValuesSet.add(FIELD_1_TEXT);
	fieldValuesSet.add(FIELD_2_TEXT);
	fieldValuesSet.add(KEYWORD_TEXT);
	fieldValuesSet.add(UNINDEXED_FIELD_TEXT);
	fieldValuesSet.add(UNSTORED_1_FIELD_TEXT);
	fieldValuesSet.add(UNSTORED_2_FIELD_TEXT);*/
}