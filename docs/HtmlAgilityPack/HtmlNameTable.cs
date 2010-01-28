// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
using System.Xml;

namespace HtmlAgilityPack
{
    internal class HtmlNameTable : XmlNameTable
    {
        #region Fields

        private NameTable _nametable = new NameTable();

        #endregion

        #region Public Methods

        public override string Add(string array)
        {
            return _nametable.Add(array);
        }

        public override string Add(char[] array, int offset, int length)
        {
            return _nametable.Add(array, offset, length);
        }

        public override string Get(string array)
        {
            return _nametable.Get(array);
        }

        public override string Get(char[] array, int offset, int length)
        {
            return _nametable.Get(array, offset, length);
        }

        #endregion

        #region Internal Methods

        internal string GetOrAdd(string array)
        {
            string s = Get(array);
            if (s == null)
            {
                return Add(array);
            }
            return s;
        }

        #endregion
    }
}