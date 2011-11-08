/**
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace Mono.Lucene.Net.Index
{
    /// <summary>
    /// Base class for enumerating all but deleted docs.
    /// 
    /// <p/>NOTE: this class is meant only to be used internally
    /// by Lucene; it's only public so it can be shared across
    /// packages.  This means the API is freely subject to
    /// change, and, the class could be removed entirely, in any
    /// Lucene release.  Use directly at your own risk! */
    /// </summary>
    public abstract class AbstractAllTermDocs : TermDocs
    {
        protected int maxDoc;
        protected int doc = -1;

        protected AbstractAllTermDocs(int maxDoc)
        {
            this.maxDoc = maxDoc;
        }

        public void Seek(Term term)
        {
            if (term == null)
            {
                doc = -1;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void Seek(TermEnum termEnum)
        {
            throw new NotSupportedException();
        }

        public int Doc()
        {
            return doc;
        }

        public int Freq()
        {
            return 1;
        }

        public bool Next()
        {
            return SkipTo(doc + 1);
        }

        public int Read(int[] docs, int[] freqs)
        {
            int length = docs.Length;
            int i = 0;
            while (i < length && doc < maxDoc)
            {
                if (!IsDeleted(doc))
                {
                    docs[i] = doc;
                    freqs[i] = 1;
                    ++i;
                }
                doc++;
            }
            return i;
        }

        public bool SkipTo(int target)
        {
            doc = target;
            while (doc < maxDoc)
            {
                if (!IsDeleted(doc))
                {
                    return true;
                }
                doc++;
            }
            return false;
        }

        public void Close()
        {
        }

        public abstract bool IsDeleted(int doc);
    }
}
