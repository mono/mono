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
namespace Monodoc.Lucene.Net.Index
{
    /// <summary>A Term represents a word from text.  This is the unit of search.  It is
    /// composed of two elements, the text of the word, as a string, and the name of
    /// the Field that the text occured in, an interned string.
    /// Note that terms may represent more than words from text fields, but also
    /// things like dates, email addresses, urls, etc.  
    /// </summary>
    [Serializable]
	public sealed class Term : System.IComparable
	{
		internal System.String field;
		public /*internal*/ System.String text;
		
		/// <summary>Constructs a Term with the given Field and text. </summary>
		public Term(System.String fld, System.String txt) : this(fld, txt, true)
		{
		}
		internal Term(System.String fld, System.String txt, bool intern)
		{
			field = intern ? String.Intern(fld) : fld; // Field names are interned
			text = txt; // unless already known to be
		}
		
		/// <summary>Returns the Field of this term, an interned string.   The Field indicates
		/// the part of a document which this term came from. 
		/// </summary>
		public System.String Field()
		{
			return field;
		}
		
		/// <summary>Returns the text of this term.  In the case of words, this is simply the
		/// text of the word.  In the case of dates and other types, this is an
		/// encoding of the object as a string.  
		/// </summary>
		public System.String Text()
		{
			return text;
		}
		
		/// <summary>Compares two terms, returning true iff they have the same
		/// Field and text. 
		/// </summary>
		public  override bool Equals(System.Object o)
		{
			if (o == null)
				return false;
			Term other = (Term) o;
			return (System.Object) field == (System.Object) other.field && text.Equals(other.text);
		}
		
		/// <summary>Combines the hashCode() of the Field and the text. </summary>
		public override int GetHashCode()
		{
			return field.GetHashCode() + text.GetHashCode();
		}
		
		public int CompareTo(System.Object other)
		{
			return CompareTo((Term) other);
		}
		
		/// <summary>Compares two terms, returning an integer which is less than zero iff this
		/// term belongs after the argument, equal zero iff this term is equal to the
		/// argument, and greater than zero iff this term belongs after the argument.
		/// The ordering of terms is first by Field, then by text.
		/// </summary>
		public int CompareTo(Term other)
		{
			if ((System.Object) field == (System.Object) other.field)
			// fields are interned
				return String.CompareOrdinal(text, other.text);
			else
				return String.CompareOrdinal(field, other.field);
		}
		
		/// <summary>Resets the Field and text of a Term. </summary>
		internal void  Set(System.String fld, System.String txt)
		{
			field = fld;
			text = txt;
		}
		
		public override System.String ToString()
		{
			return field + ":" + text;
		}
		
		private void  ReadObject(System.IO.BinaryReader in_Renamed)
		{
			// This function is private and is never been called, so this may not be a port issue. // {{Aroush}}
			// in_Renamed.defaultReadObject(); // {{Aroush}} >> 'java.io.ObjectInputStream.defaultReadObject()'
			field = String.Intern(field);
		}
		
		// {{Aroush: Or is this what we want (vs. the above)?!!
		private void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("field", field);
			info.AddValue("text", text);
		}
		// Aroush}}
	}
}