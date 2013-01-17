//
//
// SearchableIndex.cs: Index that uses Lucene to search through the docs 
//
// Author: Mario Sopena
//

using System;
using System.IO;
using System.Collections.Generic;
// Lucene imports
using Lucene.Net.Index;
using Lucene.Net.Documents;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers;
using Lucene.Net.Store;

namespace Monodoc
{
	public class SearchableIndex 
	{
		const int maxSearchCount = 30;

		IndexSearcher searcher;
		string dir;

		public string Dir {
			get { 
				if (dir == null)
					dir = "search_index";
				return dir;
			}
			set { dir = value; }
		}

		public static SearchableIndex Load (string dir)
		{
			SearchableIndex s = new SearchableIndex ();
			s.dir = dir;
			try {
				//s.searcher = new IndexSearcher (dir);
				// TODO: parametrize that depending if we run on the desktop (low footprint) or the server (use RAMDirectory for instance)
				s.searcher = new IndexSearcher (FSDirectory.Open (dir));
			} catch (IOException) {
				Console.WriteLine ("Index nonexistent or in bad format");
				return null;
			}
			return s;
		}
		
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
				qq2.Boost = 10f;
				Query qq3 = new PrefixQuery (htTerm);
				qq3.Boost = 10f;
				DisjunctionMaxQuery q1 = new DisjunctionMaxQuery (0f);
				q1.Add (qq1);
				q1.Add (qq2);
				q1.Add (qq3);
				Query q2 = new TermQuery (new Term ("text", term));
				q2.Boost = 3f;
				Query q3 = new TermQuery (new Term ("examples", term));
				q3.Boost = 3f;
				DisjunctionMaxQuery q = new DisjunctionMaxQuery (0f);

				q.Add (q1);
				q.Add (q2);
				q.Add (q3);
			
				TopDocs top = SearchInternal (q, count, start);
				Result r = new Result (term, searcher, top.ScoreDocs);
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
			var collector = TopScoreDocCollector.Create (start + count + 1, false);
			searcher.Search (q, collector);

			return collector.TopDocs (start, count);
		}

		public Result FastSearch (string term, int number)
		{
			try {
				term = term.ToLower ();
				Query q1 = new TermQuery (new Term ("hottext", term));
				Query q2 = new PrefixQuery (new Term ("hottext", term));
				q2.Boost = 0.5f;
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
			get { return searcher.Doc (docs[i].Doc); }
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
			return docs[i].Score;
		}

		public Result (string Term, Searcher searcher, ScoreDoc[] docs) 
		{
			this.term = Term;
			this.searcher = searcher;
			this.docs = docs;
		}
	}
}

