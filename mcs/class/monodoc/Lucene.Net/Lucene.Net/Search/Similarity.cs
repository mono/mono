/*
 * Copyright 2004 The Apache Software Foundation
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using Field = Monodoc.Lucene.Net.Documents.Field;
using Monodoc.Lucene.Net.Index;
using IndexWriter = Monodoc.Lucene.Net.Index.IndexWriter;
using Term = Monodoc.Lucene.Net.Index.Term;
namespace Monodoc.Lucene.Net.Search
{
	
	
	/// <summary>Expert: Scoring API.
	/// <p>Subclasses implement search scoring.
	/// 
	/// <p>The score of query <code>q</code> for document <code>d</code> is defined
	/// in terms of these methods as follows:
	/// 
	/// <table cellpadding="0" cellspacing="0" border="0">
	/// <tr>
	/// <td valign="middle" align="right" rowspan="2">score(q,d) =<br></td>
	/// <td valign="middle" align="center">
	/// <big><big><big><big><big>&Sigma;</big></big></big></big></big></td>
	/// <td valign="middle"><small>
	/// {@link #Tf(int) tf}(t in d) *
	/// {@link #Idf(Term,Searcher) idf}(t) *
	/// {@link Field#getBoost getBoost}(t.Field in d) *
	/// {@link #LengthNorm(String,int) lengthNorm}(t.Field in d)
	/// </small></td>
	/// <td valign="middle" rowspan="2">&nbsp;*
	/// {@link #Coord(int,int) coord}(q,d) *
	/// {@link #QueryNorm(float) queryNorm}(q)
	/// </td>
	/// </tr>
	/// <tr>
	/// <td valign="top" align="right">
	/// <small>t in q</small>
	/// </td>
	/// </tr>
	/// </table>
	/// 
	/// </summary>
	/// <seealso cref="#SetDefault(Similarity)">
	/// </seealso>
	/// <seealso cref="IndexWriter#SetSimilarity(Similarity)">
	/// </seealso>
	/// <seealso cref="Searcher#SetSimilarity(Similarity)">
	/// </seealso>
	public abstract class Similarity
	{
		/// <summary>The Similarity implementation used by default. </summary>
		private static Similarity defaultImpl = new DefaultSimilarity();
		
		/// <summary>Set the default Similarity implementation used by indexing and search
		/// code.
		/// 
		/// </summary>
		/// <seealso cref="Searcher#SetSimilarity(Similarity)">
		/// </seealso>
		/// <seealso cref="IndexWriter#SetSimilarity(Similarity)">
		/// </seealso>
		public static void  SetDefault(Similarity similarity)
		{
			Similarity.defaultImpl = similarity;
		}
		
		/// <summary>Return the default Similarity implementation used by indexing and search
		/// code.
		/// 
		/// <p>This is initially an instance of {@link DefaultSimilarity}.
		/// 
		/// </summary>
		/// <seealso cref="Searcher#SetSimilarity(Similarity)">
		/// </seealso>
		/// <seealso cref="IndexWriter#SetSimilarity(Similarity)">
		/// </seealso>
		public static Similarity GetDefault()
		{
			return Similarity.defaultImpl;
		}
		
		/// <summary>Cache of decoded bytes. </summary>
		private static readonly float[] NORM_TABLE = new float[256];
		
		/// <summary>Decodes a normalization factor stored in an index.</summary>
		/// <seealso cref="#EncodeNorm(float)">
		/// </seealso>
		public static float DecodeNorm(byte b)
		{
			return NORM_TABLE[b & 0xFF];
		}
		
		/// <summary>Computes the normalization value for a Field given the total number of
		/// terms contained in a Field.  These values, together with Field boosts, are
		/// stored in an index and multipled into scores for hits on each Field by the
		/// search code.
		/// 
		/// <p>Matches in longer fields are less precise, so implemenations of this
		/// method usually return smaller values when <code>numTokens</code> is large,
		/// and larger values when <code>numTokens</code> is small.
		/// 
		/// <p>That these values are computed under {@link
		/// IndexWriter#AddDocument(Document)} and stored then using
		/// {#encodeNorm(float)}.  Thus they have limited precision, and documents
		/// must be re-indexed if this method is altered.
		/// 
		/// </summary>
		/// <param name="fieldName">the name of the Field
		/// </param>
		/// <param name="numTokens">the total number of tokens contained in fields named
		/// <i>fieldName</i> of <i>doc</i>.
		/// </param>
		/// <returns> a normalization factor for hits on this Field of this document
		/// 
		/// </returns>
		/// <seealso cref="Field#SetBoost(float)">
		/// </seealso>
		public abstract float LengthNorm(System.String fieldName, int numTokens);
		
		/// <summary>Computes the normalization value for a query given the sum of the squared
		/// weights of each of the query terms.  This value is then multipled into the
		/// weight of each query term.
		/// 
		/// <p>This does not affect ranking, but rather just attempts to make scores
		/// from different queries comparable.
		/// 
		/// </summary>
		/// <param name="sumOfSquaredWeights">the sum of the squares of query term weights
		/// </param>
		/// <returns> a normalization factor for query weights
		/// </returns>
		public abstract float QueryNorm(float sumOfSquaredWeights);
		
		/// <summary>Encodes a normalization factor for storage in an index.
		/// 
		/// <p>The encoding uses a five-bit exponent and three-bit mantissa, thus
		/// representing values from around 7x10^9 to 2x10^-9 with about one
		/// significant decimal digit of accuracy.  Zero is also represented.
		/// Negative numbers are rounded up to zero.  Values too large to represent
		/// are rounded down to the largest representable value.  Positive values too
		/// small to represent are rounded up to the smallest positive representable
		/// value.
		/// 
		/// </summary>
		/// <seealso cref="Field#SetBoost(float)">
		/// </seealso>
		public static byte EncodeNorm(float f)
		{
			return FloatToByte(f);
		}
		
		private static float ByteToFloat(byte b)
		{
			if (b == 0)
			// zero is a special case
				return 0.0f;
			int mantissa = b & 7;
			int exponent = (b >> 3) & 31;
			int bits = ((exponent + (63 - 15)) << 24) | (mantissa << 21);
            return BitConverter.ToSingle(BitConverter.GetBytes(bits), 0);
		}
		
		private static byte FloatToByte(float f)
		{
			if (f < 0.0f)
			// round negatives up to zero
				f = 0.0f;
			
			if (f == 0.0f)
			// zero is a special case
				return 0;
			
			int bits = BitConverter.ToInt32(BitConverter.GetBytes(f), 0); // parse float into parts
			int mantissa = (bits & 0xffffff) >> 21;
			int exponent = (((bits >> 24) & 0x7f) - 63) + 15;
			
			if (exponent > 31)
			{
				// overflow: use max value
				exponent = 31;
				mantissa = 7;
			}
			
			if (exponent < 0)
			{
				// underflow: use min value
				exponent = 0;
				mantissa = 1;
			}
			
			return (byte) ((exponent << 3) | mantissa); // pack into a byte
		}
		
		
		/// <summary>Computes a score factor based on a term or phrase's frequency in a
		/// document.  This value is multiplied by the {@link #Idf(Term, Searcher)}
		/// factor for each term in the query and these products are then summed to
		/// form the initial score for a document.
		/// 
		/// <p>Terms and phrases repeated in a document indicate the topic of the
		/// document, so implementations of this method usually return larger values
		/// when <code>freq</code> is large, and smaller values when <code>freq</code>
		/// is small.
		/// 
		/// <p>The default implementation calls {@link #Tf(float)}.
		/// 
		/// </summary>
		/// <param name="freq">the frequency of a term within a document
		/// </param>
		/// <returns> a score factor based on a term's within-document frequency
		/// </returns>
		public virtual float Tf(int freq)
		{
			return Tf((float) freq);
		}
		
		/// <summary>Computes the amount of a sloppy phrase match, based on an edit distance.
		/// This value is summed for each sloppy phrase match in a document to form
		/// the frequency that is passed to {@link #Tf(float)}.
		/// 
		/// <p>A phrase match with a small edit distance to a document passage more
		/// closely matches the document, so implementations of this method usually
		/// return larger values when the edit distance is small and smaller values
		/// when it is large.
		/// 
		/// </summary>
		/// <seealso cref="PhraseQuery#SetSlop(int)">
		/// </seealso>
		/// <param name="distance">the edit distance of this sloppy phrase match
		/// </param>
		/// <returns> the frequency increment for this match
		/// </returns>
		public abstract float SloppyFreq(int distance);
		
		/// <summary>Computes a score factor based on a term or phrase's frequency in a
		/// document.  This value is multiplied by the {@link #Idf(Term, Searcher)}
		/// factor for each term in the query and these products are then summed to
		/// form the initial score for a document.
		/// 
		/// <p>Terms and phrases repeated in a document indicate the topic of the
		/// document, so implemenations of this method usually return larger values
		/// when <code>freq</code> is large, and smaller values when <code>freq</code>
		/// is small.
		/// 
		/// </summary>
		/// <param name="freq">the frequency of a term within a document
		/// </param>
		/// <returns> a score factor based on a term's within-document frequency
		/// </returns>
		public abstract float Tf(float freq);
		
		/// <summary>Computes a score factor for a simple term.
		/// 
		/// <p>The default implementation is:<pre>
		/// return idf(searcher.docFreq(term), searcher.maxDoc());
		/// </pre>
		/// 
		/// Note that {@link Searcher#MaxDoc()} is used instead of
		/// {@link Monodoc.Lucene.Net.Index.IndexReader#NumDocs()} because it is proportional to
		/// {@link Searcher#DocFreq(Term)} , i.e., when one is inaccurate,
		/// so is the other, and in the same direction.
		/// 
		/// </summary>
		/// <param name="term">the term in question
		/// </param>
		/// <param name="searcher">the document collection being searched
		/// </param>
		/// <returns> a score factor for the term
		/// </returns>
		public virtual float Idf(Term term, Searcher searcher)
		{
			return Idf(searcher.DocFreq(term), searcher.MaxDoc());
		}
		
		/// <summary>Computes a score factor for a phrase.
		/// 
		/// <p>The default implementation sums the {@link #Idf(Term,Searcher)} factor
		/// for each term in the phrase.
		/// 
		/// </summary>
		/// <param name="terms">the terms in the phrase
		/// </param>
		/// <param name="searcher">the document collection being searched
		/// </param>
		/// <returns> a score factor for the phrase
		/// </returns>
		public virtual float Idf(System.Collections.ICollection terms, Searcher searcher)
		{
			float idf = 0.0f;
			System.Collections.IEnumerator i = terms.GetEnumerator();
			while (i.MoveNext())
			{
				idf += Idf((Term) i.Current, searcher);
			}
			return idf;
		}
		
		/// <summary>Computes a score factor based on a term's document frequency (the number
		/// of documents which contain the term).  This value is multiplied by the
		/// {@link #Tf(int)} factor for each term in the query and these products are
		/// then summed to form the initial score for a document.
		/// 
		/// <p>Terms that occur in fewer documents are better indicators of topic, so
		/// implemenations of this method usually return larger values for rare terms,
		/// and smaller values for common terms.
		/// 
		/// </summary>
		/// <param name="docFreq">the number of documents which contain the term
		/// </param>
		/// <param name="numDocs">the total number of documents in the collection
		/// </param>
		/// <returns> a score factor based on the term's document frequency
		/// </returns>
		public abstract float Idf(int docFreq, int numDocs);
		
		/// <summary>Computes a score factor based on the fraction of all query terms that a
		/// document contains.  This value is multiplied into scores.
		/// 
		/// <p>The presence of a large portion of the query terms indicates a better
		/// match with the query, so implemenations of this method usually return
		/// larger values when the ratio between these parameters is large and smaller
		/// values when the ratio between them is small.
		/// 
		/// </summary>
		/// <param name="overlap">the number of query terms matched in the document
		/// </param>
		/// <param name="maxOverlap">the total number of terms in the query
		/// </param>
		/// <returns> a score factor based on term overlap with the query
		/// </returns>
		public abstract float Coord(int overlap, int maxOverlap);
		static Similarity()
		{
			{
				for (int i = 0; i < 256; i++)
					NORM_TABLE[i] = ByteToFloat((byte) i);
			}
		}
	}
}