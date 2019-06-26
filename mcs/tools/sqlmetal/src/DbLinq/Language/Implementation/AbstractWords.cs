#region MIT license
// 
// MIT license
//
// Copyright (c) 2007-2008 Jiri Moudry, Pascal Craponne
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DbLinq.Language.Implementation
{
    /// <summary>
    /// Offer base mechanisms for words based languages (== all)
    /// </summary>
#if !MONO_STRICT
    public
#endif
    abstract class AbstractWords : ILanguageWords
    {
        /// <summary>
        /// Words and corresponding weights
        /// </summary>
        protected IDictionary<string, int> WordsWeights;
        /// <summary>
        /// Plural forms for singular words (exceptions)
        /// </summary>
        protected IDictionary<string, string> SingularToPlural = new Dictionary<string, string>();
        /// <summary>
        /// Singular froms for plural words (exceptions)
        /// </summary>
        protected IDictionary<string, string> PluralToSingular = new Dictionary<string, string>();

        /// <summary>
        /// using English heuristics, convert 'dogs' to 'dog',
        /// 'categories' to 'category',
        /// 'cat' remains unchanged.
        /// </summary>
        /// <param name="plural"></param>
        /// <returns></returns>
        public virtual string Singularize(string plural)
        {
            string singular;
            if (PluralToSingular.TryGetValue(plural, out singular))
                return singular;
            return ComputeSingular(plural);
        }

        /// <summary>
        /// using English heuristics, convert 'dog' to 'dogs',
        /// 'bass' remains unchanged.
        /// </summary>
        /// <param name="singular"></param>
        /// <returns></returns>
        public virtual string Pluralize(string singular)
        {
            string plural;
            if (SingularToPlural.TryGetValue(singular, out plural))
                return plural;
            return ComputePlural(singular);
        }

        /// <summary>
        /// Computes the singular.
        /// </summary>
        /// <param name="plural">The plural.</param>
        /// <returns></returns>
        protected abstract string ComputeSingular(string plural);
        /// <summary>
        /// Computes the plural.
        /// </summary>
        /// <param name="singular">The singular.</param>
        /// <returns></returns>
        protected abstract string ComputePlural(string singular);

        /// <summary>
        /// Returns true if the required culture is supported
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public abstract bool Supports(CultureInfo cultureInfo);
        /// <summary>
        /// Loads the words (operation may be slow, so it is excluded from ctor)
        /// </summary>
        public abstract void Load();

        /// <summary>
        /// Loads the specified resource name.
        /// </summary>
        /// <param name="resourceName">Name of the resource.</param>
        public virtual void Load(string resourceName)
        {
            WordsWeights = new Dictionary<string, int>();
            var type = GetType();
            using (var resourceStream = type.Assembly.GetManifestResourceStream(type, resourceName))
            {
                using (var resourceReader = new StreamReader(resourceStream))
                {
                    var singularPluralSeparator = new[] { "=>" };
                    while (!resourceReader.EndOfStream)
                    {
                        string word = resourceReader.ReadLine().Trim().ToLower();
                        // comments start with a "#"
                        if (word.Length == 0 || word[0] == '#')
                            continue;
                        int count = 1;
                        // starting a word with a "+" adds weight to it
                        while (word.StartsWith("+"))
                        {
                            count++;
                            word = word.Substring(1);
                        }

                        var singularPlural = word.Split(singularPluralSeparator, StringSplitOptions.RemoveEmptyEntries);
                        // "a => b" declares a singular => plural form
                        if (singularPlural.Length > 1)
                        {
                            word = singularPlural[0].Trim();
                            var plural = singularPlural[1].Trim();
                            SingularToPlural[word] = plural;
                            PluralToSingular[plural] = word;
                        }

                        if (!WordsWeights.ContainsKey(word))
                            WordsWeights[word] = count;
                        else
                            WordsWeights[word] += count;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the standard form for word (removes mixed letters, for example).
        /// The goal is to make it usable from dictionary.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        protected virtual string GetStandard(string word)
        {
            return word;
        }

        /// <summary>
        /// Gets the weight for a given word.
        /// Actually based on dictionary info.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        protected int GetWeight(string word)
        {
            if (word.Length == 1) // a letter is always 1
                return 1;
            int weight;
            WordsWeights.TryGetValue(GetStandard(word.ToLower()), out weight);
            return weight;
        }

        /// <summary>
        /// Tells if the specified word exists in dictionary.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <returns></returns>
        protected bool Exists(string word)
        {
            return GetWeight(word) > 0;
        }

        /// <summary>
        /// Context is used to speedup words recognition
        /// </summary>
        private class Context
        {
            internal class Split
            {
                public IList<string> Words;
                public double Note;
            }

            public readonly IDictionary<string, Split> Splits = new Dictionary<string, Split>();
        }

        /// <summary>
        /// Extracts words from an undistinguishable letters magma
        /// for example "shipsperunit" --&gt; "ships" "per" "unit"
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public virtual IList<string> GetWords(string text)
        {
            //var context = new Context();
            //IList<string> words = new List<string>();
            //int lastIndex = 0;
            //for (int index = 0; index <= text.Length; index++)
            //{
            //    if (index == text.Length || !char.IsLetterOrDigit(text[index]))
            //    {
            //        var word = text.Substring(lastIndex, index - lastIndex);
            //        // if the word is empty, we skip it
            //        if (!string.IsNullOrEmpty(word))
            //            GetMagmaWords(word, words, context);
            //        lastIndex = index + 1;
            //    }
            //}
            //return words;
            var words = new List<string>();
            GetMagmaWords(text, words, new Context());
            return words;
        }

        /// <summary>
        /// Gets the magma words.
        /// </summary>
        /// <param name="magma">The magma.</param>
        /// <param name="words">The words.</param>
        /// <param name="context">The context.</param>
        private void GetMagmaWords(string magma, ICollection<string> words, Context context)
        {
            foreach (var word in GetMagmaWords(magma, context))
                words.Add(word);
        }

        /// <summary>
        /// Extracts words from a "word magma" by splitting the string on every position and keep the best score.
        /// The method is recursive
        /// </summary>
        /// <param name="magma">The magma.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private IList<string> GetMagmaWords(string magma, Context context)
        {
            var foundWords = new List<string>();
            if (magma.Length == 0)
                throw new ArgumentException("magma string must not be empty");
            // initalize matching
            IList<string> bestLeft = new[] { magma };
            IList<string> bestRight = new string[0];
            double bestNote = GetNote(bestLeft);
            if (bestNote > 0) // if we have something here, it is a full word, then don't look any further
                return bestLeft; // that this may break the weight... for example toothpaste always win vs +++tooth +++paste
            // split and try
            for (int i = 1; i <= magma.Length - 1; i++)
            {
                var left = magma.Substring(0, i);
                var right = magma.Substring(i);
                IList<string> leftWords, rightWords;
                double leftNote = ComputeWords(left, out leftWords, context);
                double rightNote = ComputeWords(right, out rightWords, context);
                double note = leftNote + rightNote;
                if (note >= bestNote) // >= means "longer words are better"
                {
                    bestNote = note;
                    bestLeft = leftWords;
                    bestRight = rightWords;
                }
            }
            foundWords.AddRange(bestLeft);
            foundWords.AddRange(bestRight);
            return foundWords;
        }

        /// <summary>
        /// Computes the words.
        /// </summary>
        /// <param name="magma">The magma.</param>
        /// <param name="words">The words.</param>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        private double ComputeWords(string magma, out IList<string> words, Context context)
        {
            Context.Split split;
            if (!context.Splits.TryGetValue(magma, out split))
            {
                split = new Context.Split
                            {
                                Words = GetMagmaWords(magma, context)
                            };
                split.Note = GetNote(split.Words);
                context.Splits[magma] = split;
            }
            words = split.Words;
            return split.Note;
        }

        /// <summary>
        /// Returns a value for a list of words, with the following rules:
        /// - fewer is better
        /// - popular is better
        /// </summary>
        /// <param name="words"></param>
        /// <returns></returns>
        public double GetNote(IList<string> words)
        {
            if (words.Count == 0)
                return 0;

            double totalWeight = 0;
            foreach (string word in words)
            {
                double weight = GetWeight(word);
                totalWeight += weight;
            }
            double averageWeight = totalWeight / words.Count;
            return averageWeight / words.Count
                   * 1000; // coz it's easier to read
        }
    }
}