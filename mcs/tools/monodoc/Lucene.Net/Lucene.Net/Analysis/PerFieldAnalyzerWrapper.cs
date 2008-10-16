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
namespace Monodoc.Lucene.Net.Analysis
{
	
	/// <summary> This analyzer is used to facilitate scenarios where different
	/// fields require different analysis techniques.  Use {@link #addAnalyzer}
	/// to add a non-default analyzer on a Field name basis.
	/// See TestPerFieldAnalyzerWrapper.java for example usage.
	/// </summary>
	public class PerFieldAnalyzerWrapper:Analyzer
	{
		private Analyzer defaultAnalyzer;
		private System.Collections.IDictionary analyzerMap = new System.Collections.Hashtable();
		
		
		/// <summary> Constructs with default analyzer.
		/// 
		/// </summary>
		/// <param name="defaultAnalyzer">Any fields not specifically
		/// defined to use a different analyzer will use the one provided here.
		/// </param>
		public PerFieldAnalyzerWrapper(Analyzer defaultAnalyzer)
		{
			this.defaultAnalyzer = defaultAnalyzer;
		}
		
		/// <summary> Defines an analyzer to use for the specified Field.
		/// 
		/// </summary>
		/// <param name="fieldName">Field name requiring a non-default analyzer.
		/// </param>
		/// <param name="analyzer">non-default analyzer to use for Field
		/// </param>
		public virtual void  AddAnalyzer(System.String fieldName, Analyzer analyzer)
		{
			analyzerMap[fieldName] = analyzer;
		}

		public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
		{
			Analyzer analyzer = (Analyzer) analyzerMap[fieldName];
			if (analyzer == null)
			{
				analyzer = defaultAnalyzer;
			}
			
			return analyzer.TokenStream(fieldName, reader);
		}
	}
}