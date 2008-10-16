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
using Analyzer = Monodoc.Lucene.Net.Analysis.Analyzer;
using BooleanQuery = Monodoc.Lucene.Net.Search.BooleanQuery;
using Query = Monodoc.Lucene.Net.Search.Query;
namespace Monodoc.Lucene.Net.QueryParsers
{
	
	/// <summary> A QueryParser which constructs queries to search multiple fields.
	/// 
	/// </summary>
	/// <author>  <a href="mailto:kelvin@relevanz.com">Kelvin Tan</a>
	/// </author>
	/// <version>  $Revision: 1.4 $
	/// </version>
	public class MultiFieldQueryParser : QueryParser
	{
		public const int NORMAL_FIELD = 0;
		public const int REQUIRED_FIELD = 1;
		public const int PROHIBITED_FIELD = 2;
		
		public MultiFieldQueryParser(QueryParserTokenManager tm):base(tm)
		{
		}
		
		public MultiFieldQueryParser(CharStream stream):base(stream)
		{
		}
		
		public MultiFieldQueryParser(System.String f, Analyzer a):base(f, a)
		{
		}
		
		/// <summary> <p>
		/// Parses a query which searches on the fields specified.
		/// <p>
		/// If x fields are specified, this effectively constructs:
		/// <pre>
		/// <code>
		/// (field1:query) (field2:query) (field3:query)...(fieldx:query)
		/// </code>
		/// </pre>
		/// 
		/// </summary>
		/// <param name="query">Query string to parse
		/// </param>
		/// <param name="fields">Fields to search on
		/// </param>
		/// <param name="analyzer">Analyzer to use
		/// </param>
		/// <throws>  ParseException if query parsing fails </throws>
		/// <throws>  TokenMgrError if query parsing fails </throws>
		public static Query Parse(System.String query, System.String[] fields, Analyzer analyzer)
		{
			BooleanQuery bQuery = new BooleanQuery();
			for (int i = 0; i < fields.Length; i++)
			{
				Query q = Parse(query, fields[i], analyzer);
				bQuery.Add(q, false, false);
			}
			return bQuery;
		}
		
		/// <summary> <p>
		/// Parses a query, searching on the fields specified.
		/// Use this if you need to specify certain fields as required,
		/// and others as prohibited.
		/// <p><pre>
		/// Usage:
		/// <code>
		/// String[] fields = {"filename", "contents", "description"};
		/// int[] flags = {MultiFieldQueryParser.NORMAL FIELD,
		/// MultiFieldQueryParser.REQUIRED FIELD,
		/// MultiFieldQueryParser.PROHIBITED FIELD,};
		/// parse(query, fields, flags, analyzer);
		/// </code>
		/// </pre>
		/// <p>
		/// The code above would construct a query:
		/// <pre>
		/// <code>
		/// (filename:query) +(contents:query) -(description:query)
		/// </code>
		/// </pre>
		/// 
		/// </summary>
		/// <param name="query">Query string to parse
		/// </param>
		/// <param name="fields">Fields to search on
		/// </param>
		/// <param name="flags">Flags describing the fields
		/// </param>
		/// <param name="analyzer">Analyzer to use
		/// </param>
		/// <throws>  ParseException if query parsing fails </throws>
		/// <throws>  TokenMgrError if query parsing fails </throws>
		public static Query Parse(System.String query, System.String[] fields, int[] flags, Analyzer analyzer)
		{
			BooleanQuery bQuery = new BooleanQuery();
			for (int i = 0; i < fields.Length; i++)
			{
				Query q = Parse(query, fields[i], analyzer);
				int flag = flags[i];
				switch (flag)
				{
					
					case REQUIRED_FIELD: 
						bQuery.Add(q, true, false);
						break;
					
					case PROHIBITED_FIELD: 
						bQuery.Add(q, false, true);
						break;
					
					default: 
						bQuery.Add(q, false, false);
						break;
					
				}
			}
			return bQuery;
		}
	}
}