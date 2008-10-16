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
using Monodoc.Lucene.Net.Index;
using Monodoc.Lucene.Net.Documents;
using Monodoc.Lucene.Net.Analysis;
using Monodoc.Lucene.Net.Analysis.Standard;
using Monodoc.Lucene.Net.Search;
using Monodoc.Lucene.Net.QueryParsers;

namespace Monodoc
{

//TODO: where do I call searcher.close()
public class SearchableIndex 
{
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
	public Result Search (string term) {
		try {
			Query q1 = QueryParser.Parse (term, "hottext", new StandardAnalyzer ());
			Query q2 = QueryParser.Parse (term, "text", new StandardAnalyzer ());
			q2.SetBoost (0.7f);
			Query q3 = QueryParser.Parse (term, "examples", new StandardAnalyzer ());
			q3.SetBoost (0.5f);
			BooleanQuery q = new BooleanQuery();
			q.Add (q1, false, false);
			q.Add (q2, false, false);
			q.Add (q3, false, false);
			Hits hits = searcher.Search(q);
			Result r = new Result (term, hits);
			Results.Add (r);
			return r;
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
	public string Term {
		get { return term;}
	}
	public Hits hits;

	public int Count {
		get { return hits.Length(); }
	}
	public Document this [int i] {
		get { return hits.Doc (i); }
	}
	
	public string GetTitle (int i) 
	{
		Document d = hits.Doc (i);
		if (d == null)
			return "";
		else
			return d.Get ("title");
	}
	public string GetUrl (int i)
	{
		Document d = hits.Doc (i);
		if (d == null)
			return "";
		else
			return d.Get ("url");
		
	}
	public float Score (int i)
	{
		return hits.Score (i);
	}
	public Result (string Term, Hits hits) 
	{
		this.term = Term;
		this.hits = hits;
	}
}
}

