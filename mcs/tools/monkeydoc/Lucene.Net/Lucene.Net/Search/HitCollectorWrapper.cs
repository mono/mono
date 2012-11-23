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

using IndexReader = Mono.Lucene.Net.Index.IndexReader;

namespace Mono.Lucene.Net.Search
{
	
	/// <summary> Wrapper for ({@link HitCollector}) implementations, which simply re-bases the
	/// incoming docID before calling {@link HitCollector#collect}.
	/// 
	/// </summary>
	/// <deprecated> Please migrate custom HitCollectors to the new {@link Collector}
	/// class. This class will be removed when {@link HitCollector} is
	/// removed.
	/// </deprecated>
    [Obsolete("Please migrate custom HitCollectors to the new Collector class. This class will be removed when HitCollector is removed.")]
	public class HitCollectorWrapper:Collector
	{
		private HitCollector collector;
		private int base_Renamed = 0;
		private Scorer scorer = null;
		
		public HitCollectorWrapper(HitCollector collector)
		{
			this.collector = collector;
		}
		
		public override void  SetNextReader(IndexReader reader, int docBase)
		{
			base_Renamed = docBase;
		}
		
		public override void  Collect(int doc)
		{
			collector.Collect(doc + base_Renamed, scorer.Score());
		}
		
		public override void  SetScorer(Scorer scorer)
		{
			this.scorer = scorer;
		}
		
		public override bool AcceptsDocsOutOfOrder()
		{
			return false;
		}
	}
}
