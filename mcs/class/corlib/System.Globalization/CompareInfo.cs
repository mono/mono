//
// System.Globalization.CompareInfo.cs
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Dick Porter (dick@ximian.com)
//
// (C) Ximian, Inc. 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Reflection;
using System.Collections;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Mono.Globalization.Unicode;

namespace System.Globalization
{
	[Serializable]
#if !NET_2_1 || MONOTOUCH
#if !DISABLE_SECURITY
	[ComVisible (true)]
#endif
	public class CompareInfo : IDeserializationCallback {

		static readonly bool useManagedCollation =
		#if !MICRO_LIB
			Environment.internalGetEnvironmentVariable ("MONO_DISABLE_MANAGED_COLLATION")
			!= "yes" && MSCompatUnicodeTable.IsReady;
		#else
			false;
		#endif

		internal static bool UseManagedCollation {
			get { return useManagedCollation; }
		}
		
		void IDeserializationCallback.OnDeserialization(object sender)
		{
			#if !MICRO_LIB
			if (UseManagedCollation) {
				collator = new SimpleCollator (new CultureInfo (culture));
			} else 
			#endif
			{
				/* This will build the ICU collator, and store
				 * the pointer in ICU_collator
				 */
				try {
					this.construct_compareinfo (icu_name);
				} catch {
				//	ICU_collator=IntPtr.Zero;
				}
			}
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void construct_compareinfo (string locale);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void free_internal_collator ();

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int internal_compare (string str1, int offset1,
						     int length1, string str2,
						     int offset2, int length2,
						     CompareOptions options);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void assign_sortkey (object key, string source,
						    CompareOptions options);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int internal_index (string source, int sindex,
						   int count, char value,
						   CompareOptions options,
						   bool first);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern int internal_index (string source, int sindex,
						   int count, string value,
						   CompareOptions options,
						   bool first);

#else
	public class CompareInfo {
		internal static bool UseManagedCollation {
			get { return true; }
		}
#endif
		const CompareOptions ValidCompareOptions_NoStringSort =
			CompareOptions.None | CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace |
			CompareOptions.IgnoreSymbols | CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth |
#if NET_2_0
			CompareOptions.OrdinalIgnoreCase |
#endif
			CompareOptions.Ordinal;

		const CompareOptions ValidCompareOptions = ValidCompareOptions_NoStringSort | CompareOptions.StringSort;

		// Keep in synch with MonoCompareInfo in the runtime. 
		private int culture;
		[NonSerialized]
		private string icu_name;
//		[NonSerialized]
//		private IntPtr ICU_collator;

#pragma warning disable 169		
		private int win32LCID;	// Unused, but MS.NET serializes this
#if NET_2_0
		private string m_name; // Unused, but MS.NET serializes this
#endif
#pragma warning restore 169
#if !MICRO_LIB
		[NonSerialized]
		SimpleCollator collator;
#endif
		// Maps culture IDs to SimpleCollator objects
		private static Hashtable collators;

		[NonSerialized]
		// Protects access to 'collators'
		private static object monitor = new Object ();
		
		/* Hide the .ctor() */
		CompareInfo() {}
		
		internal CompareInfo (CultureInfo ci)
		{
			this.culture = ci.LCID;
			#if !MICRO_LIB
			if (UseManagedCollation) {
				lock (monitor) {
					if (collators == null)
						collators = new Hashtable ();
					collator = (SimpleCollator)collators [ci.LCID];
					if (collator == null) {
						collator = new SimpleCollator (ci);
						collators [ci.LCID] = collator;
					}
				}
			} else 
			#endif 
			{
#if !NET_2_1 || MONOTOUCH
				this.icu_name = ci.IcuName;
				this.construct_compareinfo (icu_name);
#endif
			}
		}

		~CompareInfo ()
		{
#if !NET_2_1 || MONOTOUCH
			free_internal_collator ();
#endif
		}
		
#if MICRO_LIB
private int string_invariant_compare_char(char c1, char c2, CompareOptions options)
		{
			if ((options & CompareOptions.Ordinal) > 0) 
				return c1 - c2;
				
			int result;
					
			if ((options & CompareOptions.IgnoreCase) > 0) {
				result = Char.ToLower(c1) - Char.ToLower(c2);
			} else {
				/*
				 * No options. Kana, symbol and spacing options don't
				 * apply to the invariant culture.
				 */
		
				/*
				 * FIXME: here we must use the information from c1type and c2type
				 * to find out the proper collation, even on the InvariantCulture, the
				 * sorting is not done by computing the unicode values, but their
				 * actual sort order.
				 */
				result = c1 - c2;
				}
			
			return ((result < 0) ? -1 : (result > 0) ? 1 : 0);
		}
		
private int internal_compare_micro (string str1, int offset1,
						     int len1, string str2,
						     int offset2, int len2,
						     CompareOptions options)
		{
			int length;
			int charcmp;
			string ustr1;
			string ustr2;
			int pos;
		
			if(len1 >= len2) {
				length=len1;
			} else {
				length=len2;
			}
		
			ustr1 = str1.Substring(offset1);
			ustr2 = str2.Substring(offset2);
		
			pos = 0;
		
			for (pos = 0; pos != length; pos++) {
				if (pos >= len1 || pos >= len2)
					break;
		
				charcmp = string_invariant_compare_char(ustr1[pos], ustr2[pos],
									options);
				if (charcmp != 0) {
					return(charcmp);
				}
			}
		
			/* the lesser wins, so if we have looped until length we just
			 * need to check the last char
			 */
			if (pos == length) {
				return(string_invariant_compare_char(ustr1[pos - 1],
								     ustr2[pos - 1], options));
			}
		
			/* Test if one of the strings has been compared to the end */
			if (pos >= len1) {
				if (pos >= len2) {
					return(0);
				} else {
					return(-1);
				}
			} else if (pos >= len2) {
				return(1);
			}
		
			/* if not, check our last char only.. (can this happen?) */
			return(string_invariant_compare_char(ustr1[pos], ustr2[pos], options));
		}
#endif

#if !NET_2_1 || MONOTOUCH
#if !MICRO_LIB
		private int internal_compare_managed (string str1, int offset1,
						int length1, string str2,
						int offset2, int length2,
						CompareOptions options)
		{
			return collator.Compare (str1, offset1, length1,
				str2, offset2, length2, options);
		}
#endif

		private int internal_compare_switch (string str1, int offset1,
						int length1, string str2,
						int offset2, int length2,
						CompareOptions options)
		{
			#if !MICRO_LIB
			return UseManagedCollation ?
				internal_compare_managed (str1, offset1, length1,
				str2, offset2, length2, options) :
				internal_compare (str1, offset1, length1,
				str2, offset2, length2, options);
			#else
			return internal_compare_micro (str1, offset1, length1,
				str2, offset2, length2, options);
			#endif
		}

#else
		private int internal_compare_switch (string str1, int offset1,
						int length1, string str2,
						int offset2, int length2,
						CompareOptions options)
		{
			return collator.Compare (str1, offset1, length1,
				str2, offset2, length2, options);
		}
#endif
		public virtual int Compare (string string1, string string2)
		{
			return Compare (string1, string2, CompareOptions.None);
		}

		public virtual int Compare (string string1, string string2,
					    CompareOptions options)
		{
			if ((options & ValidCompareOptions) != options)
				throw new ArgumentException ("options");

			if (string1 == null) {
				if (string2 == null)
					return 0;
				return -1;
			}
			if (string2 == null)
				return 1;

			/* Short cut... */
			if(string1.Length == 0 && string2.Length == 0)
				return(0);

			return(internal_compare_switch (string1, 0, string1.Length,
						 string2, 0, string2.Length,
						 options));
		}

		public virtual int Compare (string string1, int offset1,
					    string string2, int offset2)
		{
			return Compare (string1, offset1, string2, offset2, CompareOptions.None);
		}

		public virtual int Compare (string string1, int offset1,
					    string string2, int offset2,
					    CompareOptions options)
		{
			if ((options & ValidCompareOptions) != options)
				throw new ArgumentException ("options");

			if (string1 == null) {
				if (string2 == null)
					return 0;
				return -1;
			}
			if (string2 == null)
				return 1;

			/* Not in the spec, but ms does these short
			 * cuts before checking the offsets (breaking
			 * the offset >= string length specified check
			 * in the process...)
			 */
			if((string1.Length == 0 || offset1 == string1.Length) &&
				(string2.Length == 0 || offset2 == string2.Length))
				return(0);

			if(offset1 < 0 || offset2 < 0) {
				throw new ArgumentOutOfRangeException ("Offsets must not be less than zero");
			}
			
			if(offset1 > string1.Length) {
				throw new ArgumentOutOfRangeException ("Offset1 is greater than or equal to the length of string1");
			}
			
			if(offset2 > string2.Length) {
				throw new ArgumentOutOfRangeException ("Offset2 is greater than or equal to the length of string2");
			}
			
			return(internal_compare_switch (string1, offset1,
						 string1.Length-offset1,
						 string2, offset2,
						 string2.Length-offset2,
						 options));
		}

		public virtual int Compare (string string1, int offset1,
					    int length1, string string2,
					    int offset2, int length2)
		{
			return Compare (string1, offset1, length1, string2, offset2, length2, CompareOptions.None);
		}

		public virtual int Compare (string string1, int offset1,
					    int length1, string string2,
					    int offset2, int length2,
					    CompareOptions options)
		{
			if ((options & ValidCompareOptions) != options)
				throw new ArgumentException ("options");

			if (string1 == null) {
				if (string2 == null)
					return 0;
				return -1;
			}
			if (string2 == null)
				return 1;

			/* Not in the spec, but ms does these short
			 * cuts before checking the offsets (breaking
			 * the offset >= string length specified check
			 * in the process...)
			 */
			if((string1.Length == 0 ||
				offset1 == string1.Length ||
				length1 == 0) &&
				(string2.Length == 0 ||
				offset2 == string2.Length ||
				length2 == 0))
					return(0);

			if(offset1 < 0 || length1 < 0 ||
			   offset2 < 0 || length2 < 0) {
				throw new ArgumentOutOfRangeException ("Offsets and lengths must not be less than zero");
			}
			
			if(offset1 > string1.Length) {
				throw new ArgumentOutOfRangeException ("Offset1 is greater than or equal to the length of string1");
			}
			
			if(offset2 > string2.Length) {
				throw new ArgumentOutOfRangeException ("Offset2 is greater than or equal to the length of string2");
			}
			
			if(length1 > string1.Length-offset1) {
				throw new ArgumentOutOfRangeException ("Length1 is greater than the number of characters from offset1 to the end of string1");
			}
			
			if(length2 > string2.Length-offset2) {
				throw new ArgumentOutOfRangeException ("Length2 is greater than the number of characters from offset2 to the end of string2");
			}
			
			return(internal_compare_switch (string1, offset1, length1,
						 string2, offset2, length2,
						 options));
		}

		public override bool Equals(object value)
		{
			CompareInfo other=value as CompareInfo;
			if(other==null) {
				return(false);
			}
			
			return(other.culture==culture);
		}

		public static CompareInfo GetCompareInfo(int culture)
		{
			return(new CultureInfo (culture).CompareInfo);
		}

		public static CompareInfo GetCompareInfo(string name)
		{
			if(name == null) {
				throw new ArgumentNullException("name");
			}
			return(new CultureInfo (name).CompareInfo);
		}

		public static CompareInfo GetCompareInfo(int culture,
							 Assembly assembly)
		{
			/* The assembly parameter is supposedly there
			 * to allow some sort of compare algorithm
			 * versioning.
			 */
			if(assembly == null) {
				throw new ArgumentNullException("assembly");
			}
			if(assembly!=typeof (Object).Module.Assembly) {
				throw new ArgumentException ("Assembly is an invalid type");
			}
			return(GetCompareInfo (culture));
		}

		public static CompareInfo GetCompareInfo(string name,
							 Assembly assembly)
		{
			/* The assembly parameter is supposedly there
			 * to allow some sort of compare algorithm
			 * versioning.
			 */
			if(name == null) {
				throw new ArgumentNullException("name");
			}
			if(assembly == null) {
				throw new ArgumentNullException("assembly");
			}
			if(assembly!=typeof (Object).Module.Assembly) {
				throw new ArgumentException ("Assembly is an invalid type");
			}
			return(GetCompareInfo (name));
		}

		public override int GetHashCode()
		{
			return(LCID);
		}
		
		public virtual SortKey GetSortKey(string source)
		{
			return(GetSortKey (source, CompareOptions.None));
		}

		public virtual SortKey GetSortKey(string source,
						  CompareOptions options)
		{
#if NET_2_0
			switch (options) {
			case CompareOptions.Ordinal:
			case CompareOptions.OrdinalIgnoreCase:
				throw new ArgumentException ("Now allowed CompareOptions.", "options");
			}
#endif
#if !NET_2_1 || MONOTOUCH
#if !MICRO_LIB
			if (UseManagedCollation)
				return collator.GetSortKey (source, options);
#endif
			SortKey key=new SortKey (culture, source, options);

			/* Need to do the icall here instead of in the
			 * SortKey constructor, as we need access to
			 * this instance's collator.
			 */
			assign_sortkey (key, source, options);
			
			return(key);
#else
			return collator.GetSortKey (source, options);
#endif
		}

		public virtual int IndexOf (string source, char value)
		{
			return(IndexOf (source, value, 0, source.Length,
					CompareOptions.None));
		}

		public virtual int IndexOf (string source, string value)
		{
			return(IndexOf (source, value, 0, source.Length,
					CompareOptions.None));
		}

		public virtual int IndexOf (string source, char value,
					    CompareOptions options)
		{
			return(IndexOf (source, value, 0, source.Length,
					options));
		}

		public virtual int IndexOf (string source, char value,
					    int startIndex)
		{
			return(IndexOf (source, value, startIndex,
					source.Length - startIndex,
					CompareOptions.None));
		}
		
		public virtual int IndexOf (string source, string value,
					    CompareOptions options)
		{
			return(IndexOf (source, value, 0, source.Length,
					options));
		}

		public virtual int IndexOf (string source, string value,
					    int startIndex)
		{
			return(IndexOf (source, value, startIndex,
					source.Length - startIndex,
					CompareOptions.None));
		}

		public virtual int IndexOf (string source, char value,
					    int startIndex,
					    CompareOptions options)
		{
			return(IndexOf (source, value, startIndex,
					source.Length - startIndex, options));
		}

		public virtual int IndexOf (string source, char value,
					    int startIndex, int count)
		{
			return IndexOf (source, value, startIndex, count,
					CompareOptions.None);
		}

		public virtual int IndexOf (string source, string value,
					    int startIndex,
					    CompareOptions options)
		{
			return(IndexOf (source, value, startIndex,
					source.Length - startIndex, options));
		}

		public virtual int IndexOf (string source, string value,
					    int startIndex, int count)
		{
			return(IndexOf (source, value, startIndex, count,
					CompareOptions.None));
		}

#if !NET_2_1 || MONOTOUCH
#if !MICRO_LIB
		private int internal_index_managed (string s, int sindex,
			int count, char c, CompareOptions opt,
			bool first)
		{
			return first ?
				collator.IndexOf (s, c, sindex, count, opt) :
				collator.LastIndexOf (s, c, sindex, count, opt);
		}
#endif
		private int internal_index_switch (string s, int sindex,
			int count, char c, CompareOptions opt,
			bool first)
		{
			#if !MICRO_LIB
			// - forward IndexOf() icall is much faster than
			//   manged version, so always use icall. However,
			//   it does not work for OrdinalIgnoreCase, so
			//   do not avoid managed collator for that option.
			return UseManagedCollation && ! (first && opt == CompareOptions.Ordinal) ?
				internal_index_managed (s, sindex, count, c, opt, first) :
				internal_index (s, sindex, count, c, opt, first);
			#else
				return internal_index (s, sindex, count, c, opt, first);
			#endif
		}
#else
		private int internal_index_switch (string s, int sindex,
			int count, char c, CompareOptions opt,
			bool first)
		{
			return first ?
				collator.IndexOf (s, c, sindex, count, opt) :
				collator.LastIndexOf (s, c, sindex, count, opt);
		}
#endif

		public virtual int IndexOf (string source, char value,
					    int startIndex, int count,
					    CompareOptions options)
		{
			if(source==null) {
				throw new ArgumentNullException ("source");
			}
			if(startIndex<0) {
				throw new ArgumentOutOfRangeException ("startIndex");
			}
			if(count<0 || (source.Length - startIndex) < count) {
				throw new ArgumentOutOfRangeException ("count");
			}
			if ((options & ValidCompareOptions_NoStringSort) != options)
				throw new ArgumentException ("options");
			
			if(count==0) {
				return(-1);
			}

			if((options & CompareOptions.Ordinal)!=0) {
				for(int pos=startIndex;
				    pos < startIndex + count;
				    pos++) {
					if(source[pos]==value) {
						return(pos);
					}
				}
				return(-1);
			} else {
				return (internal_index_switch (source, startIndex,
							count, value, options,
							true));
			}
		}

#if !NET_2_1 || MONOTOUCH
#if !MICRO_LIB
		private int internal_index_managed (string s1, int sindex,
			int count, string s2, CompareOptions opt,
			bool first)
		{
			return first ?
				collator.IndexOf (s1, s2, sindex, count, opt) :
				collator.LastIndexOf (s1, s2, sindex, count, opt);
		}
#endif
		private int internal_index_switch (string s1, int sindex,
			int count, string s2, CompareOptions opt,
			bool first)
		{
			#if !MICRO_LIB
			// - forward IndexOf() icall is much faster than
			//   manged version, so always use icall. However,
			//   it does not work for OrdinalIgnoreCase, so
			//   do not avoid managed collator for that option.
			return UseManagedCollation && ! (first && opt == CompareOptions.Ordinal) ?
				internal_index_managed (s1, sindex, count, s2, opt, first) :
				internal_index (s1, sindex, count, s2, opt, first);
			#else
				return internal_index (s1, sindex, count, s2, opt, first);
			#endif
}
#else
		private int internal_index_switch (string s1, int sindex,
			int count, string s2, CompareOptions opt,
			bool first)
		{
			return first ?
				collator.IndexOf (s1, s2, sindex, count, opt) :
				collator.LastIndexOf (s1, s2, sindex, count, opt);
		}
#endif

		public virtual int IndexOf (string source, string value,
					    int startIndex, int count,
					    CompareOptions options)
		{
			if(source==null) {
				throw new ArgumentNullException ("source");
			}
			if(value==null) {
				throw new ArgumentNullException ("value");
			}
			if(startIndex<0) {
				throw new ArgumentOutOfRangeException ("startIndex");
			}
			if(count<0 || (source.Length - startIndex) < count) {
				throw new ArgumentOutOfRangeException ("count");
			}
			if ((options & ValidCompareOptions_NoStringSort) != options)
				throw new ArgumentException ("options");
			if(value.Length==0) {
				return(startIndex);
			}
			if(count==0) {
				return(-1);
			}

			return (internal_index_switch (source, startIndex, count,
						value, options, true));
		}

		public virtual bool IsPrefix(string source, string prefix)
		{
			return(IsPrefix (source, prefix, CompareOptions.None));
		}

		public virtual bool IsPrefix(string source, string prefix,
					     CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source");
			}
			if(prefix == null) {
				throw new ArgumentNullException("prefix");
			}
#if !MICRO_LIB
			if (UseManagedCollation)
				return collator.IsPrefix (source, prefix, options);
#endif
			if(source.Length < prefix.Length) {
				return(false);
			} else {
				return(Compare (source, 0, prefix.Length,
						prefix, 0, prefix.Length,
						options)==0);
			}
		}

		public virtual bool IsSuffix(string source, string suffix)
		{
			return(IsSuffix (source, suffix, CompareOptions.None));
		}

		public virtual bool IsSuffix(string source, string suffix,
					     CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source");
			}
			if(suffix == null) {
				throw new ArgumentNullException("suffix");
			}
#if !MICRO_LIB
			if (UseManagedCollation)
				return collator.IsSuffix (source, suffix, options);
#endif
			if(source.Length < suffix.Length) {
				return(false);
			} else {
				return(Compare (source,
						source.Length - suffix.Length,
						suffix.Length, suffix, 0,
						suffix.Length, options)==0);
			}
		}

		public virtual int LastIndexOf(string source, char value)
		{
			return(LastIndexOf (source, value, source.Length - 1,
					    source.Length, CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, string value)
		{
			return(LastIndexOf (source, value, source.Length - 1,
					    source.Length, CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, char value,
					       CompareOptions options)
		{
			return(LastIndexOf (source, value, source.Length - 1,
					    source.Length, options));
		}

		public virtual int LastIndexOf(string source, char value,
					       int startIndex)
		{
			return(LastIndexOf (source, value, startIndex,
					    startIndex + 1,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, string value,
					       CompareOptions options)
		{
			return(LastIndexOf (source, value, source.Length - 1,
					    source.Length, options));
		}

		public virtual int LastIndexOf(string source, string value,
					       int startIndex)
		{
			return(LastIndexOf (source, value, startIndex,
					    startIndex + 1,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, char value,
					       int startIndex,
					       CompareOptions options)
		{
			return(LastIndexOf (source, value, startIndex,
					    startIndex + 1,
					    options));
		}

		public virtual int LastIndexOf(string source, char value,
					       int startIndex, int count)
		{
			return(LastIndexOf (source, value, startIndex, count,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, string value,
					       int startIndex,
					       CompareOptions options)
		{
			return(LastIndexOf (source, value, startIndex,
					    startIndex + 1,
					    options));
		}

		public virtual int LastIndexOf(string source, string value,
					       int startIndex, int count)
		{
			return(LastIndexOf (source, value, startIndex, count,
					    CompareOptions.None));
		}

		public virtual int LastIndexOf(string source, char value,
					       int startIndex, int count,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source");
			}
			if(startIndex < 0) {
				throw new ArgumentOutOfRangeException ("startIndex");
			}
			if(count < 0 || (startIndex - count) < -1) {
				throw new ArgumentOutOfRangeException("count");
			}
			if ((options & ValidCompareOptions_NoStringSort) != options)
				throw new ArgumentException ("options");
			
			if(count==0) {
				return(-1);
			}

			if((options & CompareOptions.Ordinal)!=0) {
				for(int pos=startIndex;
				    pos > startIndex - count;
				    pos--) {
					if(source[pos]==value) {
						return(pos);
					}
				}
				return(-1);
			} else {
				return (internal_index_switch (source, startIndex,
							count, value, options,
							false));
			}
		}

		public virtual int LastIndexOf(string source, string value,
					       int startIndex, int count,
					       CompareOptions options)
		{
			if(source == null) {
				throw new ArgumentNullException("source");
			}
			if(value == null) {
				throw new ArgumentNullException("value");
			}
			if(startIndex < 0) {
				throw new ArgumentOutOfRangeException ("startIndex");
			}
			if(count < 0 || (startIndex - count) < -1) {
				throw new ArgumentOutOfRangeException("count");
			}
			if ((options & ValidCompareOptions_NoStringSort) != options)
				throw new ArgumentException ("options");
			if(count == 0) {
				return(-1);
			}

			int valuelen=value.Length;
			if(valuelen==0) {
				return(0);
			}

			return(internal_index_switch (source, startIndex, count,
					       value, options, false));
		}

#if NET_2_0 && !MICRO_LIB
		[ComVisible (false)]
		public static bool IsSortable (char ch)
		{
			return MSCompatUnicodeTable.IsSortable (ch);
		}

		[ComVisible (false)]
		public static bool IsSortable (string text)
		{
			return MSCompatUnicodeTable.IsSortable (text);
		}
#endif

		public override string ToString()
		{
			return("CompareInfo - "+culture);
		}

		/* LAMESPEC: not mentioned in the spec, but corcompare
		 * shows it.  Some documentation about what it does
		 * would be nice.
		 */
		public int LCID
		{
			get {
				return(culture);
			}
		}

#if NET_2_0
		[ComVisible (false)]
		public virtual string Name {
			get { return icu_name; }
		}
#endif
	}
}
