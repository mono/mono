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
using NUnit.Framework;
using SimpleAnalyzer = Lucene.Net.Analysis.SimpleAnalyzer;
using Document = Lucene.Net.Documents.Document;
using Field = Lucene.Net.Documents.Field;
using IndexWriter = Lucene.Net.Index.IndexWriter;
using Term = Lucene.Net.Index.Term;
using Lucene.Net.Search;
using Searchable = Lucene.Net.Search.Searchable;
using RAMDirectory = Lucene.Net.Store.RAMDirectory;
using English = Lucene.Net.Util.English;
namespace Lucene.Net.Search.Spans
{
	
	/// <summary> Tests basic search capabilities.
	/// 
	/// <p>Uses a collection of 1000 documents, each the english rendition of their
	/// document number.  For example, the document numbered 333 has text "three
	/// hundred thirty three".
	/// 
	/// <p>Tests are each a single query, and its hits are checked to ensure that
	/// all and only the correct documents are returned, thus providing end-to-end
	/// testing of the indexing and search code.
	/// 
	/// </summary>
	/// <author>  Doug Cutting
	/// </author>
	[TestFixture]
	public class TestBasics
	{
		private IndexSearcher searcher;
		
        [TestFixtureSetUp]
		public virtual void  SetUp()
		{
			RAMDirectory directory = new RAMDirectory();
			IndexWriter writer = new IndexWriter(directory, new SimpleAnalyzer(), true);
			//writer.infoStream = System.out;
			for (int i = 0; i < 1000; i++)
			{
				Document doc = new Document();
				doc.Add(Field.Text("Field", English.IntToEnglish(i)));
				writer.AddDocument(doc);
			}
			
			writer.Close();
			
			searcher = new IndexSearcher(directory);
		}
		
        [Test]
		public virtual void  TestTerm()
		{
			Query query = new TermQuery(new Term("Field", "seventy"));
			CheckHits(query, new int[]{70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 270, 271, 272, 273, 274, 275, 276, 277, 278, 279, 370, 371, 372, 373, 374, 375, 376, 377, 378, 379, 470, 471, 472, 473, 474, 475, 476, 477, 478, 479, 570, 571, 572, 573, 574, 575, 576, 577, 578, 579, 670, 671, 672, 673, 674, 675, 676, 677, 678, 679, 770, 771, 772, 773, 774, 775, 776, 777, 778, 779, 870, 871, 872, 873, 874, 875, 876, 877, 878, 879, 970, 971, 972, 973, 974, 975, 976, 977, 978, 979});
		}
		
        [Test]
		public virtual void  TestTerm2()
		{
			Query query = new TermQuery(new Term("Field", "seventish"));
			CheckHits(query, new int[]{});
		}
		
        [Test]
		public virtual void  TestPhrase()
		{
			PhraseQuery query = new PhraseQuery();
			query.Add(new Term("Field", "seventy"));
			query.Add(new Term("Field", "seven"));
			CheckHits(query, new int[]{77, 177, 277, 377, 477, 577, 677, 777, 877, 977});
		}
		
        [Test]
		public virtual void  TestPhrase2()
		{
			PhraseQuery query = new PhraseQuery();
			query.Add(new Term("Field", "seventish"));
			query.Add(new Term("Field", "sevenon"));
			CheckHits(query, new int[]{});
		}
		
        [Test]
		public virtual void  TestBoolean()
		{
			BooleanQuery query = new BooleanQuery();
			query.Add(new TermQuery(new Term("Field", "seventy")), true, false);
			query.Add(new TermQuery(new Term("Field", "seven")), true, false);
			CheckHits(query, new int[]{77, 777, 177, 277, 377, 477, 577, 677, 770, 771, 772, 773, 774, 775, 776, 778, 779, 877, 977});
		}
		
        [Test]
		public virtual void  TestBoolean2()
		{
			BooleanQuery query = new BooleanQuery();
			query.Add(new TermQuery(new Term("Field", "sevento")), true, false);
			query.Add(new TermQuery(new Term("Field", "sevenly")), true, false);
			CheckHits(query, new int[]{});
		}
		
        [Test]
		public virtual void  TestSpanNearExact()
		{
			SpanTermQuery term1 = new SpanTermQuery(new Term("Field", "seventy"));
			SpanTermQuery term2 = new SpanTermQuery(new Term("Field", "seven"));
			SpanNearQuery query = new SpanNearQuery(new SpanQuery[]{term1, term2}, 0, true);
			CheckHits(query, new int[]{77, 177, 277, 377, 477, 577, 677, 777, 877, 977});
			
			Assert.IsTrue(searcher.Explain(query, 77).GetValue() > 0.0f);
			Assert.IsTrue(searcher.Explain(query, 977).GetValue() > 0.0f);
		}
		
        [Test]
		public virtual void  TestSpanNearUnordered()
		{
			SpanTermQuery term1 = new SpanTermQuery(new Term("Field", "nine"));
			SpanTermQuery term2 = new SpanTermQuery(new Term("Field", "six"));
			SpanNearQuery query = new SpanNearQuery(new SpanQuery[]{term1, term2}, 4, false);
			
			CheckHits(query, new int[]{609, 629, 639, 649, 659, 669, 679, 689, 699, 906, 926, 936, 946, 956, 966, 976, 986, 996});
		}
		
        [Test]
		public virtual void  TestSpanNearOrdered()
		{
			SpanTermQuery term1 = new SpanTermQuery(new Term("Field", "nine"));
			SpanTermQuery term2 = new SpanTermQuery(new Term("Field", "six"));
			SpanNearQuery query = new SpanNearQuery(new SpanQuery[]{term1, term2}, 4, true);
			CheckHits(query, new int[]{906, 926, 936, 946, 956, 966, 976, 986, 996});
		}
		
        [Test]
		public virtual void  TestSpanNot()
		{
			SpanTermQuery term1 = new SpanTermQuery(new Term("Field", "eight"));
			SpanTermQuery term2 = new SpanTermQuery(new Term("Field", "one"));
			SpanNearQuery near = new SpanNearQuery(new SpanQuery[]{term1, term2}, 4, true);
			SpanTermQuery term3 = new SpanTermQuery(new Term("Field", "forty"));
			SpanNotQuery query = new SpanNotQuery(near, term3);
			
			CheckHits(query, new int[]{801, 821, 831, 851, 861, 871, 881, 891});
			
			Assert.IsTrue(searcher.Explain(query, 801).GetValue() > 0.0f);
			Assert.IsTrue(searcher.Explain(query, 891).GetValue() > 0.0f);
		}
		
        [Test]
		public virtual void  TestSpanFirst()
		{
			SpanTermQuery term1 = new SpanTermQuery(new Term("Field", "five"));
			SpanFirstQuery query = new SpanFirstQuery(term1, 1);
			
			CheckHits(query, new int[]{5, 500, 501, 502, 503, 504, 505, 506, 507, 508, 509, 510, 511, 512, 513, 514, 515, 516, 517, 518, 519, 520, 521, 522, 523, 524, 525, 526, 527, 528, 529, 530, 531, 532, 533, 534, 535, 536, 537, 538, 539, 540, 541, 542, 543, 544, 545, 546, 547, 548, 549, 550, 551, 552, 553, 554, 555, 556, 557, 558, 559, 560, 561, 562, 563, 564, 565, 566, 567, 568, 569, 570, 571, 572, 573, 574, 575, 576, 577, 578, 579, 580, 581, 582, 583, 584, 585, 586, 587, 588, 589, 590, 591, 592, 593, 594, 595, 596, 597, 598, 599});
			
			Assert.IsTrue(searcher.Explain(query, 5).GetValue() > 0.0f);
			Assert.IsTrue(searcher.Explain(query, 599).GetValue() > 0.0f);
		}
		
        [Test]
		public virtual void  TestSpanOr()
		{
			SpanTermQuery term1 = new SpanTermQuery(new Term("Field", "thirty"));
			SpanTermQuery term2 = new SpanTermQuery(new Term("Field", "three"));
			SpanNearQuery near1 = new SpanNearQuery(new SpanQuery[]{term1, term2}, 0, true);
			SpanTermQuery term3 = new SpanTermQuery(new Term("Field", "forty"));
			SpanTermQuery term4 = new SpanTermQuery(new Term("Field", "seven"));
			SpanNearQuery near2 = new SpanNearQuery(new SpanQuery[]{term3, term4}, 0, true);
			
			SpanOrQuery query = new SpanOrQuery(new SpanQuery[]{near1, near2});
			
			CheckHits(query, new int[]{33, 47, 133, 147, 233, 247, 333, 347, 433, 447, 533, 547, 633, 647, 733, 747, 833, 847, 933, 947});
			
			Assert.IsTrue(searcher.Explain(query, 33).GetValue() > 0.0f);
			Assert.IsTrue(searcher.Explain(query, 947).GetValue() > 0.0f);
		}
		
        [Test]
		public virtual void  TestSpanExactNested()
		{
			SpanTermQuery term1 = new SpanTermQuery(new Term("Field", "three"));
			SpanTermQuery term2 = new SpanTermQuery(new Term("Field", "hundred"));
			SpanNearQuery near1 = new SpanNearQuery(new SpanQuery[]{term1, term2}, 0, true);
			SpanTermQuery term3 = new SpanTermQuery(new Term("Field", "thirty"));
			SpanTermQuery term4 = new SpanTermQuery(new Term("Field", "three"));
			SpanNearQuery near2 = new SpanNearQuery(new SpanQuery[]{term3, term4}, 0, true);
			
			SpanNearQuery query = new SpanNearQuery(new SpanQuery[]{near1, near2}, 0, true);
			
			CheckHits(query, new int[]{333});
			
			Assert.IsTrue(searcher.Explain(query, 333).GetValue() > 0.0f);
		}
		
        [Test]
		public virtual void  TestSpanNearOr()
		{
			
			SpanTermQuery t1 = new SpanTermQuery(new Term("Field", "six"));
			SpanTermQuery t3 = new SpanTermQuery(new Term("Field", "seven"));
			
			SpanTermQuery t5 = new SpanTermQuery(new Term("Field", "seven"));
			SpanTermQuery t6 = new SpanTermQuery(new Term("Field", "six"));
			
			SpanOrQuery to1 = new SpanOrQuery(new SpanQuery[]{t1, t3});
			SpanOrQuery to2 = new SpanOrQuery(new SpanQuery[]{t5, t6});
			
			SpanNearQuery query = new SpanNearQuery(new SpanQuery[]{to1, to2}, 10, true);
			
			CheckHits(query, new int[]{606, 607, 626, 627, 636, 637, 646, 647, 656, 657, 666, 667, 676, 677, 686, 687, 696, 697, 706, 707, 726, 727, 736, 737, 746, 747, 756, 757, 766, 767, 776, 777, 786, 787, 796, 797});
		}
		
        [Test]
		public virtual void  TestSpanComplex1()
		{
			
			SpanTermQuery t1 = new SpanTermQuery(new Term("Field", "six"));
			SpanTermQuery t2 = new SpanTermQuery(new Term("Field", "hundred"));
			SpanNearQuery tt1 = new SpanNearQuery(new SpanQuery[]{t1, t2}, 0, true);
			
			SpanTermQuery t3 = new SpanTermQuery(new Term("Field", "seven"));
			SpanTermQuery t4 = new SpanTermQuery(new Term("Field", "hundred"));
			SpanNearQuery tt2 = new SpanNearQuery(new SpanQuery[]{t3, t4}, 0, true);
			
			SpanTermQuery t5 = new SpanTermQuery(new Term("Field", "seven"));
			SpanTermQuery t6 = new SpanTermQuery(new Term("Field", "six"));
			
			SpanOrQuery to1 = new SpanOrQuery(new SpanQuery[]{tt1, tt2});
			SpanOrQuery to2 = new SpanOrQuery(new SpanQuery[]{t5, t6});
			
			SpanNearQuery query = new SpanNearQuery(new SpanQuery[]{to1, to2}, 100, true);
			
			CheckHits(query, new int[]{606, 607, 626, 627, 636, 637, 646, 647, 656, 657, 666, 667, 676, 677, 686, 687, 696, 697, 706, 707, 726, 727, 736, 737, 746, 747, 756, 757, 766, 767, 776, 777, 786, 787, 796, 797});
		}
		
		
		private void  CheckHits(Query query, int[] results)
		{
			Lucene.Net.Search.CheckHits.CheckHits_(query, "Field", searcher, results, null);
		}
	}
}