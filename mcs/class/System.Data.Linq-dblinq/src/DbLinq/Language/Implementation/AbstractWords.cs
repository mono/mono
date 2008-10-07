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
using System.Reflection;
using DbLinq.Language;

namespace DbLinq.Language.Implementation
{
#if MONO_STRICT
    internal
#else
    public
#endif
    abstract class AbstractWords : ILanguageWords
    {
        protected IDictionary<string, int> WordsWeights;
        protected IDictionary<string, string> SingularToPlural = new Dictionary<string, string>();
        protected IDictionary<string, string> PluralToSingular = new Dictionary<string, string>();

        public virtual string Singularize(string plural)
        {
            string singular;
            if (PluralToSingular.TryGetValue(plural, out singular))
                return singular;
            return ComputeSingular(plural);
        }

        public virtual string Pluralize(string singular)
        {
            string plural;
            if (SingularToPlural.TryGetValue(singular, out plural))
                return plural;
            return ComputePlural(singular);
        }

        protected abstract string ComputeSingular(string plural);
        protected abstract string ComputePlural(string singular);

        public abstract bool Supports(CultureInfo cultureInfo);
        public abstract void Load();

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
                        if (word.Length == 0 || word[0] == '#')
                            continue;
                        int count = 1;
                        while (word.StartsWith("+"))
                        {
                            count++;
                            word = word.Substring(1);
                        }

                        var singularPlural = word.Split(singularPluralSeparator, StringSplitOptions.RemoveEmptyEntries);
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

        protected virtual string GetStandard(string word)
        {
            return word;
        }

        protected int GetWeight(string word)
        {
            if (word.Length == 1) // a letter is always 1
                return 1;
            int weight;
            WordsWeights.TryGetValue(GetStandard(word.ToLower()), out weight);
            return weight;
        }

        protected bool Exists(string word)
        {
            return GetWeight(word) > 0;
        }

        private class Context
        {
            internal class Split
            {
                public string Magma;
                public IList<string> Words;
                public double Note;
            }

            public IDictionary<string, Split> Splits = new Dictionary<string, Split>();
        }

        public virtual IList<string> GetWords(string text)
        {
            var context = new Context();
            IList<string> words = new List<string>();
            int lastIndex = 0;
            for (int index = 0; index <= text.Length; index++)
            {
                if (index == text.Length || !char.IsLetterOrDigit(text[index]))
                {
                    GetMagmaWords(text.Substring(lastIndex, index - lastIndex), words, context);
                    lastIndex = index + 1;
                }
            }
            return words;
        }

        private void GetMagmaWords(string magma, IList<string> words, Context context)
        {
            foreach (var word in GetMagmaWords(magma, context))
                words.Add(word);
        }

        /// <summary>
        /// Extracts words from a "word magma" by splitting the string on every position and keep the best score.
        /// The method is recursive
        /// </summary>
        /// <param name="magma"></param>
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
                return bestLeft; // note that this may break the weight... for example toothpaste always win vs +++tooth +++paste
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

        private double ComputeWords(string magma, out IList<string> words, Context context)
        {
            Context.Split split;
            if (!context.Splits.TryGetValue(magma, out split))
            {
                split = new Context.Split
                            {
                                Magma = magma,
                                Words = GetMagmaWords(magma, context)
                            };
                split.Note = GetNote(split.Words);
                context.Splits[magma] = split;
            }
            words = split.Words;
            return split.Note;
        }

        /// <summary>
        /// Returns a note for a list of words, with the following rules:
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