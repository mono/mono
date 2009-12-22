// HtmlAgilityPack V1.0 - Simon Mourier <simon underscore mourier at hotmail dot com>
namespace HtmlAgilityPack
{
    /// <summary>
    /// Represents a base class for fragments in a mixed code document.
    /// </summary>
    public abstract class MixedCodeDocumentFragment
    {
        #region Fields

        internal MixedCodeDocument Doc;
        private string _fragmentText;
        internal int Index;
        internal int Length;
        private int _line;
        internal int _lineposition;
        internal MixedCodeDocumentFragmentType _type;

        #endregion

        #region Constructors

        internal MixedCodeDocumentFragment(MixedCodeDocument doc, MixedCodeDocumentFragmentType type)
        {
            Doc = doc;
            _type = type;
            switch (type)
            {
                case MixedCodeDocumentFragmentType.Text:
                    Doc._textfragments.Append(this);
                    break;

                case MixedCodeDocumentFragmentType.Code:
                    Doc._codefragments.Append(this);
                    break;
            }
            Doc._fragments.Append(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the fragement text.
        /// </summary>
        public string FragmentText
        {
            get
            {
                if (_fragmentText == null)
                {
                    _fragmentText = Doc._text.Substring(Index, Length);
                }
                return FragmentText;
            }
            internal set { _fragmentText = value; }
        }

        /// <summary>
        /// Gets the type of fragment.
        /// </summary>
        public MixedCodeDocumentFragmentType FragmentType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the line number of the fragment.
        /// </summary>
        public int Line
        {
            get { return _line; }
            internal set { _line = value; }
        }

        /// <summary>
        /// Gets the line position (column) of the fragment.
        /// </summary>
        public int LinePosition
        {
            get { return _lineposition; }
        }

        /// <summary>
        /// Gets the fragment position in the document's stream.
        /// </summary>
        public int StreamPosition
        {
            get { return Index; }
        }

        #endregion
    }
}