/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
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

namespace Mono.Lucene.Net.Analysis
{
	
	
	/// <summary> Works in conjunction with the SinkTokenizer to provide the ability to set aside tokens
	/// that have already been analyzed.  This is useful in situations where multiple fields share
	/// many common analysis steps and then go their separate ways.
	/// <p/>
	/// It is also useful for doing things like entity extraction or proper noun analysis as
	/// part of the analysis workflow and saving off those tokens for use in another field.
	/// 
	/// <pre>
	/// SinkTokenizer sink1 = new SinkTokenizer();
	/// SinkTokenizer sink2 = new SinkTokenizer();
	/// TokenStream source1 = new TeeTokenFilter(new TeeTokenFilter(new WhitespaceTokenizer(reader1), sink1), sink2);
	/// TokenStream source2 = new TeeTokenFilter(new TeeTokenFilter(new WhitespaceTokenizer(reader2), sink1), sink2);
	/// TokenStream final1 = new LowerCaseFilter(source1);
	/// TokenStream final2 = source2;
	/// TokenStream final3 = new EntityDetect(sink1);
	/// TokenStream final4 = new URLDetect(sink2);
	/// d.add(new Field("f1", final1));
	/// d.add(new Field("f2", final2));
	/// d.add(new Field("f3", final3));
	/// d.add(new Field("f4", final4));
	/// </pre>
	/// In this example, <code>sink1</code> and <code>sink2</code> will both get tokens from both
	/// <code>reader1</code> and <code>reader2</code> after whitespace tokenizer
	/// and now we can further wrap any of these in extra analysis, and more "sources" can be inserted if desired.
	/// It is important, that tees are consumed before sinks (in the above example, the field names must be
	/// less the sink's field names).
	/// Note, the EntityDetect and URLDetect TokenStreams are for the example and do not currently exist in Lucene
	/// <p/>
	/// 
	/// See <a href="http://issues.apache.org/jira/browse/LUCENE-1058">LUCENE-1058</a>.
	/// <p/>
	/// WARNING: {@link TeeTokenFilter} and {@link SinkTokenizer} only work with the old TokenStream API.
	/// If you switch to the new API, you need to use {@link TeeSinkTokenFilter} instead, which offers 
	/// the same functionality.
	/// </summary>
	/// <seealso cref="SinkTokenizer">
	/// </seealso>
	/// <deprecated> Use {@link TeeSinkTokenFilter} instead
	/// 
	/// </deprecated>
    [Obsolete("Use TeeSinkTokenFilter instead")]
	public class TeeTokenFilter:TokenFilter
	{
		internal SinkTokenizer sink;
		
		public TeeTokenFilter(TokenStream input, SinkTokenizer sink):base(input)
		{
			this.sink = sink;
		}

        [Obsolete("Mono.Lucene.Net-2.9.1. This method overrides obsolete member Mono.Lucene.Net.Analysis.TokenStream.Next(Mono.Lucene.Net.Analysis.Token)")]
		public override Token Next(Token reusableToken)
		{
			System.Diagnostics.Debug.Assert(reusableToken != null);
			Token nextToken = input.Next(reusableToken);
			sink.Add(nextToken);
			return nextToken;
		}
	}
}
