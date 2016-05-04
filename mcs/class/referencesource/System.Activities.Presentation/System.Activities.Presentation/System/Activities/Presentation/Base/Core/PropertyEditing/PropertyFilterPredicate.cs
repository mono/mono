namespace System.Activities.Presentation.PropertyEditing {
    using System;
    using System.Globalization;
    using System.Runtime;
    using System.Activities.Presentation;

    /// <summary>
    /// Represents a predicate for search/filtering 
    /// </summary>
    [Fx.Tag.XamlVisible(false)]
    public class PropertyFilterPredicate
    {
        private string _matchText;

        /// <summary>
        /// Creates a PropertyFilterPredicate.
        /// </summary>
        /// <param name="matchText"></param>
        /// <exception cref="ArgumentNullException">When matchText is null</exception>
        public PropertyFilterPredicate(string matchText) {
            if (matchText == null)
                throw FxTrace.Exception.ArgumentNull("matchText");

            _matchText = matchText.ToUpper(CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Gets the string predicate
        /// </summary>
        protected string MatchText {
            get {
                return _matchText;
            }
        }

        /// <summary>
        /// Returns true if a case insensitive match of the predicate string is contained
        /// within the target string.
        /// </summary>
        /// <param name="target">The string filter</param>
        /// <returns></returns>
        public virtual bool Match(string target) {
            return target != null && target.ToUpper(CultureInfo.CurrentCulture).Contains(_matchText);
        }
    }
}
