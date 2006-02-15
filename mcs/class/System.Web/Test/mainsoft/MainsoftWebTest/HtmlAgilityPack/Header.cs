// HtmlAgilityPack V1.0 - Simon Mourier <simonm@microsoft.com>
using System;
using System.Collections;

namespace HtmlAgilityPack
{
	internal class NameValuePair
	{
		internal readonly string Name;
		internal string Value;

		internal NameValuePair()
		{
		}

		internal NameValuePair(string name):
			this()
		{
			Name = name;
		}

		internal NameValuePair(string name, string value):
			this(name)
		{
			Value = value;
		}
	}

	internal class NameValuePairList
	{
		internal readonly string Text;
		private ArrayList _allPairs;
		private Hashtable _pairsWithName;

		internal NameValuePairList():
			this(null)
		{
		}

		internal NameValuePairList(string text)
		{
			Text = text;
			_allPairs = new ArrayList();
			_pairsWithName = new Hashtable();

			Parse(text);
		}

		internal string GetNameValuePairValue(string name)
		{
			if (name==null)
				throw new ArgumentNullException();
			ArrayList al = GetNameValuePairs(name);
			if (al==null)
				return null;

			// return first item
			NameValuePair nvp = al[0] as NameValuePair;
			return nvp.Value;
		}

		internal ArrayList GetNameValuePairs(string name)
		{
			if (name==null)
				return _allPairs;
			return _pairsWithName[name] as ArrayList;
		}

		private void Parse(string text)
		{
			_allPairs.Clear();
			_pairsWithName.Clear();
			if (text==null)
				return;

			string[] p = text.Split(';');
			if (p==null)
				return;
			foreach(string pv in p)
			{
				if (pv.Length==0)
					continue;
				string[] onep = pv.Split(new char[]{'='}, 2);
				if (onep==null)
					continue;
				NameValuePair nvp = new NameValuePair(onep[0].Trim().ToLower());
				if (onep.Length<2)
					nvp.Value = "";
				else
					nvp.Value = onep[1];

				_allPairs.Add(nvp);

				// index by name
				ArrayList al = _pairsWithName[nvp.Name] as ArrayList;
				if (al==null)
				{
					al = new ArrayList();
					_pairsWithName[nvp.Name] = al;
				}
				al.Add(nvp);
			}
		}

		internal static string GetNameValuePairsValue(string text, string name)
		{
			NameValuePairList l = new NameValuePairList(text);
			return l.GetNameValuePairValue(name);
		}
	}
}
