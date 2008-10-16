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
namespace Monodoc.Lucene.Net.Search
{
	
	/// <summary> Stores information about how to sort documents by terms in an individual
	/// Field.  Fields must be indexed in order to sort by them.
	/// 
	/// <p>Created: Feb 11, 2004 1:25:29 PM
	/// 
	/// </summary>
	/// <author>   Tim Jones (Nacimiento Software)
	/// </author>
	/// <since>   lucene 1.4
	/// </since>
	/// <version>  $Id: SortField.java,v 1.9 2004/05/24 22:51:42 tjones Exp $
	/// </version>
	/// <seealso cref="Sort">
	/// </seealso>
	[Serializable]
	public class SortField
	{
		
		/// <summary>Sort by document score (relevancy).  Sort values are Float and higher
		/// values are at the front. 
		/// </summary>
		public const int SCORE = 0;
		
		/// <summary>Sort by document number (index order).  Sort values are Integer and lower
		/// values are at the front. 
		/// </summary>
		public const int DOC = 1;
		
		/// <summary>Guess type of sort based on Field contents.  A regular expression is used
		/// to look at the first term indexed for the Field and determine if it
		/// represents an integer number, a floating point number, or just arbitrary
		/// string characters. 
		/// </summary>
		public const int AUTO = 2;
		
		/// <summary>Sort using term values as Strings.  Sort values are String and lower
		/// values are at the front. 
		/// </summary>
		public const int STRING = 3;
		
		/// <summary>Sort using term values as encoded Integers.  Sort values are Integer and
		/// lower values are at the front. 
		/// </summary>
		public const int INT = 4;
		
		/// <summary>Sort using term values as encoded Floats.  Sort values are Float and
		/// lower values are at the front. 
		/// </summary>
		public const int FLOAT = 5;
		
		/// <summary>Sort using a custom Comparator.  Sort values are any Comparable and
		/// sorting is done according to natural order. 
		/// </summary>
		public const int CUSTOM = 9;
		
		// IMPLEMENTATION NOTE: the FieldCache.STRING_INDEX is in the same "namespace"
		// as the above static int values.  Any new values must not have the same value
		// as FieldCache.STRING_INDEX.
		
		
		/// <summary>Represents sorting by document score (relevancy). </summary>
		public static readonly SortField FIELD_SCORE = new SortField(null, SCORE);
		
		/// <summary>Represents sorting by document number (index order). </summary>
		public static readonly SortField FIELD_DOC = new SortField(null, DOC);
		
		
		private System.String field;
		private int type = AUTO; // defaults to determining type dynamically
		private System.Globalization.CultureInfo locale; // defaults to "natural order" (no Locale)
		internal bool reverse = false; // defaults to natural order
		private SortComparatorSource factory;
		
		/// <summary>Creates a sort by terms in the given Field where the type of term value
		/// is determined dynamically ({@link #AUTO AUTO}).
		/// </summary>
		/// <param name="Field">Name of Field to sort by, cannot be <code>null</code>.
		/// </param>
		public SortField(System.String field)
		{
			this.field = String.Intern(field);
		}
		
		/// <summary>Creates a sort, possibly in reverse, by terms in the given Field where
		/// the type of term value is determined dynamically ({@link #AUTO AUTO}).
		/// </summary>
		/// <param name="Field">Name of Field to sort by, cannot be <code>null</code>.
		/// </param>
		/// <param name="reverse">True if natural order should be reversed.
		/// </param>
		public SortField(System.String field, bool reverse)
		{
			this.field = String.Intern(field);
			this.reverse = reverse;
		}
		
		/// <summary>Creates a sort by terms in the given Field with the type of term
		/// values explicitly given.
		/// </summary>
		/// <param name="Field"> Name of Field to sort by.  Can be <code>null</code> if
		/// <code>type</code> is SCORE or DOC.
		/// </param>
		/// <param name="type">  Type of values in the terms.
		/// </param>
		public SortField(System.String field, int type)
		{
			this.field = (field != null)?String.Intern(field):field;
			this.type = type;
		}
		
		/// <summary>Creates a sort, possibly in reverse, by terms in the given Field with the
		/// type of term values explicitly given.
		/// </summary>
		/// <param name="Field"> Name of Field to sort by.  Can be <code>null</code> if
		/// <code>type</code> is SCORE or DOC.
		/// </param>
		/// <param name="type">  Type of values in the terms.
		/// </param>
		/// <param name="reverse">True if natural order should be reversed.
		/// </param>
		public SortField(System.String field, int type, bool reverse)
		{
			this.field = (field != null) ? String.Intern(field) : field;
			this.type = type;
			this.reverse = reverse;
		}
		
		/// <summary>Creates a sort by terms in the given Field sorted
		/// according to the given locale.
		/// </summary>
		/// <param name="Field"> Name of Field to sort by, cannot be <code>null</code>.
		/// </param>
		/// <param name="locale">Locale of values in the Field.
		/// </param>
		public SortField(System.String field, System.Globalization.CultureInfo locale)
		{
			this.field = String.Intern(field);
			this.type = STRING;
			this.locale = locale;
		}
		
		/// <summary>Creates a sort, possibly in reverse, by terms in the given Field sorted
		/// according to the given locale.
		/// </summary>
		/// <param name="Field"> Name of Field to sort by, cannot be <code>null</code>.
		/// </param>
		/// <param name="locale">Locale of values in the Field.
		/// </param>
		public SortField(System.String field, System.Globalization.CultureInfo locale, bool reverse)
		{
			this.field = String.Intern(field);
			this.type = STRING;
			this.locale = locale;
			this.reverse = reverse;
		}
		
		/// <summary>Creates a sort with a custom comparison function.</summary>
		/// <param name="Field">Name of Field to sort by; cannot be <code>null</code>.
		/// </param>
		/// <param name="comparator">Returns a comparator for sorting hits.
		/// </param>
		public SortField(System.String field, SortComparatorSource comparator)
		{
			this.field = (field != null)?String.Intern(field):field;
			this.type = CUSTOM;
			this.factory = comparator;
		}
		
		/// <summary>Creates a sort, possibly in reverse, with a custom comparison function.</summary>
		/// <param name="Field">Name of Field to sort by; cannot be <code>null</code>.
		/// </param>
		/// <param name="comparator">Returns a comparator for sorting hits.
		/// </param>
		/// <param name="reverse">True if natural order should be reversed.
		/// </param>
		public SortField(System.String field, SortComparatorSource comparator, bool reverse)
		{
			this.field = (field != null)?String.Intern(field):field;
			this.type = CUSTOM;
			this.reverse = reverse;
			this.factory = comparator;
		}
		
		/// <summary>Returns the name of the Field.  Could return <code>null</code>
		/// if the sort is by SCORE or DOC.
		/// </summary>
		/// <returns> Name of Field, possibly <code>null</code>.
		/// </returns>
		public virtual System.String GetField()
		{
			return field;
		}
		
		/// <summary>Returns the type of contents in the Field.</summary>
		/// <returns> One of the constants SCORE, DOC, AUTO, STRING, INT or FLOAT.
		/// </returns>
		new public virtual int GetType()
		{
			return type;
		}
		
		/// <summary>Returns the Locale by which term values are interpreted.
		/// May return <code>null</code> if no Locale was specified.
		/// </summary>
		/// <returns> Locale, or <code>null</code>.
		/// </returns>
		public virtual System.Globalization.CultureInfo GetLocale()
		{
			return locale;
		}
		
		/// <summary>Returns whether the sort should be reversed.</summary>
		/// <returns>  True if natural order should be reversed.
		/// </returns>
		public virtual bool GetReverse()
		{
			return reverse;
		}
		
		public virtual SortComparatorSource GetFactory()
		{
			return factory;
		}
		
		public override System.String ToString()
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			switch (type)
			{
				
				case SCORE:  buffer.Append("<score>");
					break;
				
				
				case DOC:  buffer.Append("<doc>");
					break;
				
				
				case CUSTOM:  
					buffer.Append("<custom:\"" + field + "\": " + factory + ">");
					break;
				
				
				default:  buffer.Append("\"" + field + "\"");
					break;
				
			}
			
			if (locale != null)
				buffer.Append("(" + locale + ")");
			if (reverse)
				buffer.Append('!');
			
			return buffer.ToString();
		}
	}
}