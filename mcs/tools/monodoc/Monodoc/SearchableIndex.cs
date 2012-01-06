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

namespace Monodoc
{

//TODO: where do I call searcher.close()
public class SearchableIndex 
{
	const int maxSearchCount = 100;

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

	public Result Search (string term, int number) {
		try {
			Query q1 = Parse (term, "hottext", true);
			Query q2 = Parse (term, "text", false);
			q2.SetBoost (0.7f);
			Query q3 = Parse (term, "examples", false);
			q3.SetBoost (0.5f);
			BooleanQuery q = new BooleanQuery();
			q.Add (q1, BooleanClause.Occur.SHOULD);
			q.Add (q2, BooleanClause.Occur.SHOULD);
			q.Add (q3, BooleanClause.Occur.SHOULD);
			TopDocs top = searcher.Search (q, number);
			Result r = new Result (term, searcher, top.ScoreDocs);
			Results.Add (r);
			return r;
		} catch (IOException) {
			Console.WriteLine ("No index in {0}", dir);
			return null;
		}
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

