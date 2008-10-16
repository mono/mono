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
using Monodoc.Lucene.Net.Index;
using Term = Monodoc.Lucene.Net.Index.Term;
using TermDocs = Monodoc.Lucene.Net.Index.TermDocs;
using TermEnum = Monodoc.Lucene.Net.Index.TermEnum;
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> Expert: The default cache implementation, storing all values in memory.
	/// A WeakHashMap is used for storage.
	/// 
	/// <p>Created: May 19, 2004 4:40:36 PM
	/// 
	/// </summary>
	/// <author>   Tim Jones (Nacimiento Software)
	/// </author>
	/// <since>   lucene 1.4
	/// </since>
    /// <version>  $Id: FieldCacheImpl.java,v 1.3.2.1 2004/09/30 19:10:26 dnaber Exp $
    /// </version>
	class FieldCacheImpl : FieldCache
	{
		
		/// <summary>Expert: Every key in the internal cache is of this type. </summary>
		internal class Entry
		{
			internal System.String field; // which Field
			internal int type; // which SortField type
			internal System.Object custom; // which custom comparator
			
			/// <summary>Creates one of these objects. </summary>
			internal Entry(System.String field, int type)
			{
                this.field = String.Intern(field);
                this.type = type;
                this.custom = null;
            }
			
			/// <summary>Creates one of these objects for a custom comparator. </summary>
			internal Entry(System.String field, System.Object custom)
			{
                this.field = String.Intern(field);
                this.type = SortField.CUSTOM;
                this.custom = custom;
            }
			
			/// <summary>Two of these are equal iff they reference the same field and type. </summary>
			public  override bool Equals(System.Object o)
			{
				if (o is Entry)
				{
					Entry other = (Entry) o;
					if ((System.Object) other.field == (System.Object) field && other.type == type)
					{
						if (other.custom == null)
						{
							if (custom == null)
								return true;
						}
						else if (other.custom.Equals(custom))
						{
							return true;
						}
					}
				}
				return false;
			}
			
			/// <summary>Composes a hashcode based on the field and type. </summary>
			public override int GetHashCode()
			{
				return field.GetHashCode() ^ type ^ (custom == null ? 0 : custom.GetHashCode());
			}
		}
		
		
		/// <summary>The internal cache. Maps Entry to array of interpreted term values. *</summary>
		internal System.Collections.IDictionary cache = new System.Collections.Hashtable();
		
		/// <summary>See if an object is in the cache. </summary>
		internal virtual System.Object Lookup(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field, int type)
		{
			Entry entry = new Entry(field, type);
			lock (this)
			{
                System.Collections.Hashtable readerCache = (System.Collections.Hashtable) cache[reader];
                if (readerCache == null)
                    return null;
                return readerCache[entry];
            }
		}
		
		/// <summary>See if a custom object is in the cache. </summary>
		internal virtual System.Object Lookup(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field, System.Object comparer)
		{
			Entry entry = new Entry(field, comparer);
			lock (this)
			{
                System.Collections.Hashtable readerCache = (System.Collections.Hashtable) cache[reader];
                if (readerCache == null)
                    return null;
                return readerCache[entry];
            }
		}
		
		/// <summary>Put an object into the cache. </summary>
		internal virtual System.Object Store(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field, int type, System.Object value_Renamed)
		{
			Entry entry = new Entry(field, type);
			lock (this)
			{
                System.Collections.Hashtable readerCache = (System.Collections.Hashtable) cache[reader];
                if (readerCache == null)
                {
                    readerCache = new System.Collections.Hashtable();
                    cache[reader] = readerCache;
                }
                System.Object tempObject;
                tempObject = readerCache[entry];
                readerCache[entry] = value_Renamed;
                return tempObject;
            }
		}
		
		/// <summary>Put a custom object into the cache. </summary>
		internal virtual System.Object Store(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field, System.Object comparer, System.Object value_Renamed)
		{
			Entry entry = new Entry(field, comparer);
			lock (this)
			{
                System.Collections.Hashtable readerCache = (System.Collections.Hashtable) cache[reader];
                if (readerCache == null)
                {
                    readerCache = new System.Collections.Hashtable();
                    cache[reader] = readerCache;
                }
                System.Object tempObject;
                tempObject = readerCache[entry];
                readerCache[entry] = value_Renamed;
                return tempObject;
            }
		}
		
		// inherit javadocs
		public virtual int[] GetInts(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field)
		{
			field = String.Intern(field);
			System.Object ret = Lookup(reader, field, SortField.INT);
			if (ret == null)
			{
				int[] retArray = new int[reader.MaxDoc()];
				if (retArray.Length > 0)
				{
					TermDocs termDocs = reader.TermDocs();
					TermEnum termEnum = reader.Terms(new Term(field, ""));
					try
					{
						if (termEnum.Term() == null)
						{
							throw new System.SystemException("no terms in Field " + field);
						}
						do 
						{
							Term term = termEnum.Term();
							if ((System.Object) term.Field() != (System.Object) field)
								break;
							int termval = System.Int32.Parse(term.Text());
							termDocs.Seek(termEnum);
							while (termDocs.Next())
							{
								retArray[termDocs.Doc()] = termval;
							}
						}
						while (termEnum.Next());
					}
					finally
					{
						termDocs.Close();
						termEnum.Close();
					}
				}
				Store(reader, field, SortField.INT, retArray);
				return retArray;
			}
			return (int[]) ret;
		}
		
		// inherit javadocs
		public virtual float[] GetFloats(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field)
		{
			field = String.Intern(field);
			System.Object ret = Lookup(reader, field, SortField.FLOAT);
			if (ret == null)
			{
				float[] retArray = new float[reader.MaxDoc()];
				if (retArray.Length > 0)
				{
					TermDocs termDocs = reader.TermDocs();
					TermEnum termEnum = reader.Terms(new Term(field, ""));
                    try
                    {
                        if (termEnum.Term() == null)
                        {
                            throw new System.SystemException("no terms in Field " + field);
                        }
                        do 
                        {
                            Term term = termEnum.Term();
                            if ((System.Object) term.Field() != (System.Object) field)
                                break;
                            float termval;
                            try
                            {
                                termval = SupportClass.Single.Parse(term.Text());
                            }
                            catch (Exception e)
                            {
                                termval = 0;
                            }
                            termDocs.Seek(termEnum);
                            while (termDocs.Next())
                            {
                                retArray[termDocs.Doc()] = termval;
                            }
                        }
                        while (termEnum.Next());
                    }
                    finally
                    {
                        termDocs.Close();
                        termEnum.Close();
                    }
				}
				Store(reader, field, SortField.FLOAT, retArray);
				return retArray;
			}
			return (float[]) ret;
		}
		
		// inherit javadocs
		public virtual System.String[] GetStrings(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field)
		{
			field = String.Intern(field);
			System.Object ret = Lookup(reader, field, SortField.STRING);
			if (ret == null)
			{
				System.String[] retArray = new System.String[reader.MaxDoc()];
				if (retArray.Length > 0)
				{
					TermDocs termDocs = reader.TermDocs();
					TermEnum termEnum = reader.Terms(new Term(field, ""));
					try
					{
						if (termEnum.Term() == null)
						{
							throw new System.SystemException("no terms in Field " + field);
						}
						do 
						{
							Term term = termEnum.Term();
							if ((System.Object) term.Field() != (System.Object) field)
								break;
							System.String termval = term.Text();
							termDocs.Seek(termEnum);
							while (termDocs.Next())
							{
								retArray[termDocs.Doc()] = termval;
							}
						}
						while (termEnum.Next());
					}
					finally
					{
						termDocs.Close();
						termEnum.Close();
					}
				}
				Store(reader, field, SortField.STRING, retArray);
				return retArray;
			}
			return (System.String[]) ret;
		}
		
		// inherit javadocs
		public virtual StringIndex GetStringIndex(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field)
		{
			field = String.Intern(field);
			System.Object ret = Lookup(reader, field, Monodoc.Lucene.Net.Search.FieldCache_Fields.STRING_INDEX);
			if (ret == null)
			{
				int[] retArray = new int[reader.MaxDoc()];
				System.String[] mterms = new System.String[reader.MaxDoc() + 1];
				if (retArray.Length > 0)
				{
					TermDocs termDocs = reader.TermDocs();
					TermEnum termEnum = reader.Terms(new Term(field, ""));
					int t = 0; // current term number
					
					// an entry for documents that have no terms in this Field
					// should a document with no terms be at top or bottom?
					// this puts them at the top - if it is changed, FieldDocSortedHitQueue
					// needs to change as well.
					mterms[t++] = null;
					
					try
					{
						if (termEnum.Term() == null)
						{
							throw new System.SystemException("no terms in Field " + field);
						}
						do 
						{
							Term term = termEnum.Term();
							if ((System.Object) term.Field() != (System.Object) field)
								break;
							
							// store term text
							// we expect that there is at most one term per document
							if (t >= mterms.Length)
								throw new System.SystemException("there are more terms than documents in Field \"" + field + "\"");
							mterms[t] = term.Text();
							
							termDocs.Seek(termEnum);
							while (termDocs.Next())
							{
								retArray[termDocs.Doc()] = t;
							}
							
							t++;
						}
						while (termEnum.Next());
					}
					finally
					{
						termDocs.Close();
						termEnum.Close();
					}
					
					if (t == 0)
					{
						// if there are no terms, make the term array
						// have a single null entry
						mterms = new System.String[1];
					}
					else if (t < mterms.Length)
					{
						// if there are less terms than documents,
						// trim off the dead array space
						System.String[] terms = new System.String[t];
						Array.Copy(mterms, 0, terms, 0, t);
						mterms = terms;
					}
				}
				StringIndex value_Renamed = new StringIndex(retArray, mterms);
				Store(reader, field, Monodoc.Lucene.Net.Search.FieldCache_Fields.STRING_INDEX, value_Renamed);
				return value_Renamed;
			}
			return (StringIndex) ret;
		}
		
		/// <summary>The pattern used to detect integer values in a Field </summary>
		/// <summary>removed for java 1.3 compatibility
		/// protected static final Pattern pIntegers = Pattern.compile ("[0-9\\-]+");
		/// 
		/// </summary>
		
		/// <summary>The pattern used to detect float values in a Field </summary>
		/// <summary> removed for java 1.3 compatibility
		/// protected static final Object pFloats = Pattern.compile ("[0-9+\\-\\.eEfFdD]+");
		/// </summary>
		
		// inherit javadocs
		public virtual System.Object GetAuto(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field)
		{
			field = String.Intern(field);
			System.Object ret = Lookup(reader, field, SortField.AUTO);
			if (ret == null)
			{
				TermEnum enumerator = reader.Terms(new Term(field, ""));
				try
				{
					Term term = enumerator.Term();
					if (term == null)
					{
						throw new System.SystemException("no terms in Field " + field + " - cannot determine sort type");
					}
					if ((System.Object) term.Field() == (System.Object) field)
					{
						System.String termtext = term.Text().Trim();
						
						/// <summary> Java 1.4 level code:
						/// if (pIntegers.matcher(termtext).matches())
						/// return IntegerSortedHitQueue.comparator (reader, enumerator, Field);
						/// else if (pFloats.matcher(termtext).matches())
						/// return FloatSortedHitQueue.comparator (reader, enumerator, Field);
						/// </summary>
						
						// Java 1.3 level code:
						try
						{
							System.Int32.Parse(termtext);
							ret = GetInts(reader, field);
						}
						catch (System.FormatException nfe1)
						{
							try
							{
								System.Single.Parse(termtext);
								ret = GetFloats(reader, field);
							}
							catch (System.FormatException nfe2)
							{
								ret = GetStringIndex(reader, field);
							}
						}
						if (ret != null)
						{
							Store(reader, field, SortField.AUTO, ret);
						}
					}
					else
					{
						throw new System.SystemException("Field \"" + field + "\" does not appear to be indexed");
					}
				}
				finally
				{
					enumerator.Close();
				}
			}
			return ret;
		}
		
		// inherit javadocs
		public virtual System.IComparable[] GetCustom(Monodoc.Lucene.Net.Index.IndexReader reader, System.String field, SortComparator comparator)
		{
			field = String.Intern(field);
			System.Object ret = Lookup(reader, field, comparator);
			if (ret == null)
			{
				System.IComparable[] retArray = new System.IComparable[reader.MaxDoc()];
				if (retArray.Length > 0)
				{
					TermDocs termDocs = reader.TermDocs();
					TermEnum termEnum = reader.Terms(new Term(field, ""));
					try
					{
						if (termEnum.Term() == null)
						{
							throw new System.SystemException("no terms in Field " + field);
						}
						do 
						{
							Term term = termEnum.Term();
							if ((System.Object) term.Field() != (System.Object) field)
								break;
							System.IComparable termval = comparator.GetComparable(term.Text());
							termDocs.Seek(termEnum);
							while (termDocs.Next())
							{
								retArray[termDocs.Doc()] = termval;
							}
						}
						while (termEnum.Next());
					}
					finally
					{
						termDocs.Close();
						termEnum.Close();
					}
				}
				Store(reader, field, SortField.CUSTOM, retArray);
				return retArray;
			}
			return (System.IComparable[]) ret;
		}
	}
}