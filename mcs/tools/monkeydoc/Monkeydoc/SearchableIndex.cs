//
//
// SearchableIndex.cs: Index that uses Lucene to search through the docs 
//
// Author: Mario Sopena
//

using System;
using System.IO;
using System.Collections;
// Lucene imports
using Mono.Lucene.Net.Index;
using Mono.Lucene.Net.Documents;
using Mono.Lucene.Net.Analysis;
using Mono.Lucene.Net.Analysis.Standard;
using Mono.Lucene.Net.Search;
using Mono.Lucene.Net.QueryParsers;

namespace MonkeyDoc
{
	public class SearchableIndex 
	{
		const int maxSearchCount = 30;

		IndexSearcher searcher;
		string dir;
		public string Dir {
			get { 
				if (dir == null) dir = "search_index";
				return dir;
			}
			set { dir = value; }
		}
		public ArrayList Results;
	
		public static SearchableIndex Load (string dir) {
			SearchableIndex s = new SearchableIndex ();
			s.dir = dir;
			s.Results = new ArrayList (20);
			try {
				s.searcher = new IndexSearcher (dir);
			} catch (IOException) {
				Console.WriteLine ("Index nonexistent or in bad format");
				return null;
			}
			return s;
		}
		
		//
		// Search the index with term
		//

		public Result Search (string term)
		{
			return Search (term, maxSearchCount);
		}

		public Result Search (string term, int count)
		{
			return Search (term, count, 0);
		}

		public Result Search (string term, int count, int start) {
			try {
				term = term.ToLower ();
				Term htTerm = new Term ("hottext", term);
				Query qq1 = new FuzzyQuery (htTerm);
				Query qq2 = new TermQuery (htTerm);
				qq2.SetBoost (10f);
				Query qq3 = new PrefixQuery (htTerm);
				qq3.SetBoost (10f);
				DisjunctionMaxQuery q1 = new DisjunctionMaxQuery (0f);
				q1.Add (qq1);
				q1.Add (qq2);
				q1.Add (qq3);
				Query q2 = new TermQuery (new Term ("text", term));
				q2.SetBoost (3f);
				Query q3 = new TermQuery (new Term ("examples", term));
				q3.SetBoost (3f);
				DisjunctionMaxQuery q = new DisjunctionMaxQuery (0f);

				q.Add (q1);
				q.Add (q2);
				q.Add (q3);
			
				TopDocs top = SearchInternal (q, count, start);
				Result r = new Result (term, searcher, top.ScoreDocs);
				Results.Add (r);
				return r;
			} catch (IOException) {
				Console.WriteLine ("No index in {0}", dir);
				return null;
			}
		}

		TopDocs SearchInternal (Query q, int count, int start)
		{
			// Easy path that doesn't involve creating a Collector ourselves
			// watch for Lucene.NET improvement on that (like searcher.SearchAfter)
			if (start == 0)
				return searcher.Search (q, count);

			var weight = searcher.CreateWeight (q); // TODO: reuse weight instead of query
			var collector = TopScoreDocCollector.create (start + count + 1, weight.ScoresDocsOutOfOrder());
			searcher.Search (q, collector);

			return collector.TopDocs (start, count);
		}

		public Result FastSearch (string term, int number)
		{
			try {
				term = term.ToLower ();
				Query q1 = new TermQuery (new Term ("hottext", term));
				Query q2 = new PrefixQuery (new Term ("hottext", term));
				q2.SetBoost (0.5f);
				DisjunctionMaxQuery q = new DisjunctionMaxQuery (0f);
				q.Add (q1);
				q.Add (q2);
				TopDocs top = searcher.Search (q, number);
				return new Result (term, searcher, top.ScoreDocs);
			} catch (IOException) {
				Console.WriteLine ("No index in {0}", dir);
				return null;
			}
		}
	
		Query Parse (string term, string field, bool fuzzy)
		{
			QueryParser parser = new QueryParser (Mono.Lucene.Net.Util.Version.LUCENE_CURRENT,
			                                      field,
			                                      new StandardAnalyzer (Mono.Lucene.Net.Util.Version.LUCENE_CURRENT));
			return parser.Parse (term);
		}
	}
	//
	// An object representing the search term with the results
	// 
	public class Result {
		string term;
		Searcher searcher;
		ScoreDoc[] docs;

		public string Term {
			get { return term;}
		}

		public int Count {
			get { return docs.Length; }
		}

		public Document this [int i] {
			get { return searcher.Doc (docs[i].doc); }
		}
	
		public string GetTitle (int i) 
		{
			Document d = this[i];
			return d == null ? string.Empty : d.Get ("title");
		}

		public string GetUrl (int i)
		{
			Document d = this[i];
			return d == null ? string.Empty : d.Get ("url");
		}

		public string GetFullTitle (int i)
		{
			Document d = this[i];
			return d == null ? string.Empty : d.Get ("fulltitle");
		}

		public float Score (int i)
		{
			return docs[i].score;
		}

		public Result (string Term, Searcher searcher, ScoreDoc[] docs) 
		{
			this.term = Term;
			this.searcher = searcher;
			this.docs = docs;
		}
	}
}

