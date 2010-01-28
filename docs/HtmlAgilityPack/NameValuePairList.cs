// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System;
using System.Collections;

namespace HtmlAgilityPack
{
    internal class NameValuePairList
    {
        #region Fields

        internal readonly string Text;
        private ArrayList _allPairs;
        private Hashtable _pairsWithName;

        #endregion

        #region Constructors

        internal NameValuePairList() :
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

        #endregion

        #region Internal Methods

        internal static string GetNameValuePairsValue(string text, string name)
        {
            NameValuePairList l = new NameValuePairList(text);
            return l.GetNameValuePairValue(name);
        }

        internal ArrayList GetNameValuePairs(string name)
        {
            if (name == null)
                return _allPairs;
            return _pairsWithName[name] as ArrayList;
        }

        internal string GetNameValuePairValue(string name)
        {
            if (name == null)
                throw new ArgumentNullException();
            ArrayList al = GetNameValuePairs(name);
            if (al == null)
                return null;

            // return first item
            NameValuePair nvp = al[0] as NameValuePair;
            return nvp != null ? nvp.Value : string.Empty;
        }

        #endregion

        #region Private Methods

        private void Parse(string text)
        {
            _allPairs.Clear();
            _pairsWithName.Clear();
            if (text == null)
                return;

            string[] p = text.Split(';');
            foreach (string pv in p)
            {
                if (pv.Length == 0)
                    continue;
                string[] onep = pv.Split(new char[] {'='}, 2);
                if (onep.Length==0)
                    continue;
                NameValuePair nvp = new NameValuePair(onep[0].Trim().ToLower());

                nvp.Value = onep.Length < 2 ? "" : onep[1];

                _allPairs.Add(nvp);

                // index by name
                ArrayList al = _pairsWithName[nvp.Name] as ArrayList;
                if (al == null)
                {
                    al = new ArrayList();
                    _pairsWithName[nvp.Name] = al;
                }
                al.Add(nvp);
            }
        }

        #endregion
    }
}