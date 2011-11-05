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
using ReaderUtil = Mono.Lucene.Net.Util.ReaderUtil;
using Explanation = Mono.Lucene.Net.Search.Explanation;

namespace Mono.Lucene.Net.Search.Function
{
	
	/// <summary>This class wraps another ValueSource, but protects
	/// against accidental double RAM usage in FieldCache when
	/// a composite reader is passed to {@link #getValues}.
	/// 
	/// <p/><b>NOTE</b>: this class adds a CPU penalty to every
	/// lookup, as it must resolve the incoming document to the
	/// right sub-reader using a binary search.<p/>
	/// 
	/// </summary>
	/// <deprecated> This class is temporary, to ease the
	/// migration to segment-based searching. Please change your
	/// code to not pass composite readers to these APIs. 
	/// </deprecated>
    [Obsolete("This class is temporary, to ease the migration to segment-based searching. Please change your code to not pass composite readers to these APIs. ")]
	[Serializable]
	public sealed class MultiValueSource:ValueSource
	{
		
		internal ValueSource other;
		public MultiValueSource(ValueSource other)
		{
			this.other = other;
		}
		
		public override DocValues GetValues(IndexReader reader)
		{
			
			IndexReader[] subReaders = reader.GetSequentialSubReaders();
			if (subReaders != null)
			{
				// This is a composite reader
				return new MultiDocValues(this, subReaders);
			}
			else
			{
				// Already an atomic reader -- just delegate
				return other.GetValues(reader);
			}
		}
		
		public override System.String Description()
		{
			return other.Description();
		}
		
		public  override bool Equals(System.Object o)
		{
			if (o is MultiValueSource)
			{
				return ((MultiValueSource) o).other.Equals(other);
			}
			else
			{
				return false;
			}
		}
		
		public override int GetHashCode()
		{
			return 31 * other.GetHashCode();
		}
		
		private sealed class MultiDocValues:DocValues
		{
			private void  InitBlock(MultiValueSource enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private MultiValueSource enclosingInstance;
			public MultiValueSource Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			
			internal DocValues[] docValues;
			internal int[] docStarts;
			
			internal MultiDocValues(MultiValueSource enclosingInstance, IndexReader[] subReaders)
			{
				InitBlock(enclosingInstance);
				docValues = new DocValues[subReaders.Length];
				docStarts = new int[subReaders.Length];
				int base_Renamed = 0;
				for (int i = 0; i < subReaders.Length; i++)
				{
					docValues[i] = Enclosing_Instance.other.GetValues(subReaders[i]);
					docStarts[i] = base_Renamed;
					base_Renamed += subReaders[i].MaxDoc();
				}
			}
			
			public override float FloatVal(int doc)
			{
				int n = ReaderUtil.SubIndex(doc, docStarts);
				return docValues[n].FloatVal(doc - docStarts[n]);
			}
			
			public override int IntVal(int doc)
			{
				int n = ReaderUtil.SubIndex(doc, docStarts);
				return docValues[n].IntVal(doc - docStarts[n]);
			}
			
			public override long LongVal(int doc)
			{
				int n = ReaderUtil.SubIndex(doc, docStarts);
				return docValues[n].LongVal(doc - docStarts[n]);
			}
			
			public override double DoubleVal(int doc)
			{
				int n = ReaderUtil.SubIndex(doc, docStarts);
				return docValues[n].DoubleVal(doc - docStarts[n]);
			}
			
			public override System.String StrVal(int doc)
			{
				int n = ReaderUtil.SubIndex(doc, docStarts);
				return docValues[n].StrVal(doc - docStarts[n]);
			}
			
			public override System.String ToString(int doc)
			{
				int n = ReaderUtil.SubIndex(doc, docStarts);
				return docValues[n].ToString(doc - docStarts[n]);
			}
			
			public override Explanation Explain(int doc)
			{
				int n = ReaderUtil.SubIndex(doc, docStarts);
				return docValues[n].Explain(doc - docStarts[n]);
			}
		}
	}
}
