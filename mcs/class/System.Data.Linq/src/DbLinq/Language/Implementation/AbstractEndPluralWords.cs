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

using DbLinq.Language.Implementation;

namespace DbLinq.Language.Implementation
{
#if MONO_STRICT
    internal
#else
    public
#endif
    abstract class AbstractEndPluralWords : AbstractWords
    {
        protected class SingularPlural
        {
            public string Singular;
            public string Plural;
        }

        protected abstract SingularPlural[] SingularsPlurals { get; }

        /// <summary>
        /// using English heuristics, convert 'dogs' to 'dog',
        /// 'categories' to 'category',
        /// 'cat' remains unchanged.
        /// </summary>
        protected override string ComputeSingular(string plural)
        {
            if (plural.Length < 2)
                return plural;

            foreach (SingularPlural sp in SingularsPlurals)
            {
                string newWord = Try(plural, sp.Plural, sp.Singular);
                if (newWord != null)
                    return newWord;
            }

            return plural;
        }

        /// <summary>
        /// using English heuristics, convert 'dog' to 'dogs',
        /// 'bass' remains unchanged.
        /// </summary>
        protected override string ComputePlural(string singular)
        {
            if (singular.Length < 2)
                return singular;

            foreach (SingularPlural sp in SingularsPlurals)
            {
                string newWord = Try(singular, sp.Singular, sp.Plural);
                if (newWord != null)
                    return newWord;
            }

            return singular;
        }

        protected string Try(string word, string ending, string newEnding)
        {
            if (word.ToLower().EndsWith(ending))
            {
                string newWord = word.Substring(0, word.Length - ending.Length) + newEnding;
                if (Exists(newWord))
                    return newWord;
            }
            return null;
        }
    }
}