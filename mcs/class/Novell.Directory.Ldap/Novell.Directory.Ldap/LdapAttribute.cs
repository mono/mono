/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.LdapAttribute.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using ArrayEnumeration = Novell.Directory.Ldap.Utilclass.ArrayEnumeration;
using Base64 = Novell.Directory.Ldap.Utilclass.Base64;

namespace Novell.Directory.Ldap
{
	/// <summary> The name and values of one attribute of a directory entry.
	/// 
	/// LdapAttribute objects are used when searching for, adding,
	/// modifying, and deleting attributes from the directory.
	/// LdapAttributes are often used in conjunction with an
	/// {@link LdapAttributeSet} when retrieving or adding multiple
	/// attributes to an entry.
	/// 
	/// 
	/// 
	/// </summary>
	/// <seealso cref="LdapEntry">
	/// </seealso>
	/// <seealso cref="LdapAttributeSet">
	/// </seealso>
	/// <seealso cref="LdapModification">
	/// </seealso>
	
	public class LdapAttribute : System.ICloneable, System.IComparable
	{
		class URLData
		{
			private void  InitBlock(LdapAttribute enclosingInstance)
			{
				this.enclosingInstance = enclosingInstance;
			}
			private LdapAttribute enclosingInstance;
			public LdapAttribute Enclosing_Instance
			{
				get
				{
					return enclosingInstance;
				}
				
			}
			private int length;
			private sbyte[] data;
			public URLData(LdapAttribute enclosingInstance, sbyte[] data, int length)
			{
				InitBlock(enclosingInstance);
				this.length = length;
				this.data = data;
				return ;
			}
			public int getLength()
			{
				return length;
			}
			public sbyte[] getData()
			{
				return data;
			}
		}
		/// <summary> Returns an enumerator for the values of the attribute in byte format.
		/// 
		/// </summary>
		/// <returns> The values of the attribute in byte format.
		///  Note: All string values will be UTF-8 encoded. To decode use the
		/// String constructor. Example: new String( byteArray, "UTF-8" );
		/// </returns>
		virtual public System.Collections.IEnumerator ByteValues
		{
			get
			{
				return new ArrayEnumeration(ByteValueArray);
			}
			
		}
		/// <summary> Returns an enumerator for the string values of an attribute.
		/// 
		/// </summary>
		/// <returns> The string values of an attribute.
		/// </returns>
		virtual public System.Collections.IEnumerator StringValues
		{
			get
			{
				return new ArrayEnumeration(StringValueArray);
			}
			
		}
		/// <summary> Returns the values of the attribute as an array of bytes.
		/// 
		/// </summary>
		/// <returns> The values as an array of bytes or an empty array if there are
		/// no values.
		/// </returns>
		[CLSCompliantAttribute(false)]
		virtual public sbyte[][] ByteValueArray
		{
			get
			{
				if (null == this.values)
					return new sbyte[0][];
				int size = this.values.Length;
				sbyte[][] bva = new sbyte[size][];
				// Deep copy so application cannot change values
				for (int i = 0, u = size; i < u; i++)
				{
					bva[i] = new sbyte[((sbyte[]) values[i]).Length];
					Array.Copy((System.Array) this.values[i], 0, (System.Array) bva[i], 0, bva[i].Length);
				}
				return bva;
			}
			
		}
		/// <summary> Returns the values of the attribute as an array of strings.
		/// 
		/// </summary>
		/// <returns> The values as an array of strings or an empty array if there are
		/// no values
		/// </returns>
		virtual public System.String[] StringValueArray
		{
			get
			{
				if (null == this.values)
					return new System.String[0];
				int size = values.Length;
				System.String[] sva = new System.String[size];
				for (int j = 0; j < size; j++)
				{
					try
					{
						System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
						char[] dchar = encoder.GetChars(SupportClass.ToByteArray((sbyte[])values[j]));
//						char[] dchar = encoder.GetChars((byte[])values[j]);
						sva[j] = new String(dchar);
//						sva[j] = new String((sbyte[]) values[j], "UTF-8");
					}
					catch (System.IO.IOException uee)
					{
						// Exception should NEVER get thrown but just in case it does ...
						throw new System.SystemException(uee.ToString());
					}
				}
				return sva;
			}
			
		}
		/// <summary> Returns the the first value of the attribute as a <code>String</code>.
		/// 
		/// </summary>
		/// <returns>  The UTF-8 encoded<code>String</code> value of the attribute's
		/// value.  If the value wasn't a UTF-8 encoded <code>String</code>
		/// to begin with the value of the returned <code>String</code> is
		/// non deterministic.
		/// 
		/// If <code>this</code> attribute has more than one value the
		/// first value is converted to a UTF-8 encoded <code>String</code>
		/// and returned. It should be noted, that the directory may
		/// return attribute values in any order, so that the first
		/// value may vary from one call to another.
		/// 
		/// If the attribute has no values <code>null</code> is returned
		/// </returns>
		virtual public System.String StringValue
		{
			get
			{
				System.String rval = null;
				if (this.values != null)
				{
					try
					{
						System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
						char[] dchar = encoder.GetChars(SupportClass.ToByteArray((sbyte[])this.values[0]));
//						char[] dchar = encoder.GetChars((byte[]) this.values[0]);
						rval = new String(dchar);
					}
					catch (System.IO.IOException use)
					{
						throw new System.SystemException(use.ToString());
					}
				}
				return rval;
			}
			
		}
		/// <summary> Returns the the first value of the attribute as a byte array.
		/// 
		/// </summary>
		/// <returns>  The binary value of <code>this</code> attribute or
		/// <code>null</code> if <code>this</code> attribute doesn't have a value.
		/// 
		/// If the attribute has no values <code>null</code> is returned
		/// </returns>
		[CLSCompliantAttribute(false)]
		virtual public sbyte[] ByteValue
		{
			get
			{
				sbyte[] bva = null;
				if (this.values != null)
				{
					// Deep copy so app can't change the value
					bva = new sbyte[((sbyte[]) values[0]).Length];
					Array.Copy((System.Array) this.values[0], 0, (System.Array) bva, 0, bva.Length);
				}
				return bva;
			}
			
		}
		/// <summary> Returns the language subtype of the attribute, if any.
		/// 
		/// For example, if the attribute name is cn;lang-ja;phonetic,
		/// this method returns the string, lang-ja.
		/// 
		/// </summary>
		/// <returns> The language subtype of the attribute or null if the attribute
		/// has none.
		/// </returns>
		virtual public System.String LangSubtype
		{
			get
			{
				if (subTypes != null)
				{
					for (int i = 0; i < subTypes.Length; i++)
					{
						if (subTypes[i].StartsWith("lang-"))
						{
							return subTypes[i];
						}
					}
				}
				return null;
			}
			
		}
		/// <summary> Returns the name of the attribute.
		/// 
		/// </summary>
		/// <returns> The name of the attribute.
		/// </returns>
		virtual public System.String Name
		{
			get
			{
				return name;
			}
			
		}
		/// <summary> Replaces all values with the specified value. This protected method is
		/// used by sub-classes of LdapSchemaElement because the value cannot be set
		/// with a contructor.
		/// </summary>
		virtual protected internal System.String Value
		{
			set
			{
				values = null;
				try
				{
					System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
					byte[] ibytes = encoder.GetBytes(value);
					sbyte[] sbytes=SupportClass.ToSByteArray(ibytes);

					this.add(sbytes);
				}
				catch (System.IO.IOException ue)
				{
					throw new System.SystemException(ue.ToString());
				}
				return ;
			}
			
		}
		private System.String name; // full attribute name
		private System.String baseName; // cn of cn;lang-ja;phonetic
		private System.String[] subTypes = null; // lang-ja of cn;lang-ja
		private System.Object[] values = null; // Array of byte[] attribute values
		
		/// <summary> Constructs an attribute with copies of all values of the input
		/// attribute.
		/// 
		/// </summary>
		/// <param name="attr"> An LdapAttribute to use as a template.
		/// 
		/// @throws IllegalArgumentException if attr is null
		/// </param>
		public LdapAttribute(LdapAttribute attr)
		{
			if (attr == null)
			{
				throw new System.ArgumentException("LdapAttribute class cannot be null");
			}
			// Do a deep copy of the LdapAttribute template
			this.name = attr.name;
			this.baseName = attr.baseName;
			if (null != attr.subTypes)
			{
				this.subTypes = new System.String[attr.subTypes.Length];
				Array.Copy((System.Array) attr.subTypes, 0, (System.Array) this.subTypes, 0, this.subTypes.Length);
			}
			// OK to just copy attributes, as the app only sees a deep copy of them
			if (null != attr.values)
			{
				this.values = new System.Object[attr.values.Length];
				Array.Copy((System.Array) attr.values, 0, (System.Array) this.values, 0, this.values.Length);
			}
			return ;
		}
		
		/// <summary> Constructs an attribute with no values.
		/// 
		/// </summary>
		/// <param name="attrName">Name of the attribute.
		/// 
		/// @throws IllegalArgumentException if attrName is null
		/// </param>
		public LdapAttribute(System.String attrName)
		{
			if ((System.Object) attrName == null)
			{
				throw new System.ArgumentException("Attribute name cannot be null");
			}
			this.name = attrName;
			this.baseName = LdapAttribute.getBaseName(attrName);
			this.subTypes = LdapAttribute.getSubtypes(attrName);
			return ;
		}
		
		/// <summary> Constructs an attribute with a byte-formatted value.
		/// 
		/// </summary>
		/// <param name="attrName">Name of the attribute.
		/// </param>
		/// <param name="attrBytes">Value of the attribute as raw bytes.
		/// 
		///  Note: If attrBytes represents a string it should be UTF-8 encoded.
		/// 
		/// @throws IllegalArgumentException if attrName or attrBytes is null
		/// </param>
		[CLSCompliantAttribute(false)]
		public LdapAttribute(System.String attrName, sbyte[] attrBytes):this(attrName)
		{
			if (attrBytes == null)
			{
				throw new System.ArgumentException("Attribute value cannot be null");
			}
			// Make our own copy of the byte array to prevent app from changing it
			sbyte[] tmp = new sbyte[attrBytes.Length];
			Array.Copy((System.Array) attrBytes, 0, (System.Array)tmp, 0, attrBytes.Length);
			this.add(tmp);
			return ;
		}
		
		/// <summary> Constructs an attribute with a single string value.
		/// 
		/// </summary>
		/// <param name="attrName">Name of the attribute.
		/// </param>
		/// <param name="attrString">Value of the attribute as a string.
		/// 
		/// @throws IllegalArgumentException if attrName or attrString is null
		/// </param>
		public LdapAttribute(System.String attrName, System.String attrString):this(attrName)
		{
			if ((System.Object) attrString == null)
			{
				throw new System.ArgumentException("Attribute value cannot be null");
			}
			try
			{
				System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
				byte[] ibytes = encoder.GetBytes(attrString);
				sbyte[] sbytes=SupportClass.ToSByteArray(ibytes);

				this.add(sbytes);
			}
			catch (System.IO.IOException e)
			{
				throw new System.SystemException(e.ToString());
			}
			return ;
		}
		
		/// <summary> Constructs an attribute with an array of string values.
		/// 
		/// </summary>
		/// <param name="attrName">Name of the attribute.
		/// </param>
		/// <param name="attrStrings">Array of values as strings.
		/// 
		/// @throws IllegalArgumentException if attrName, attrStrings, or a member
		/// of attrStrings is null
		/// </param>
		public LdapAttribute(System.String attrName, System.String[] attrStrings):this(attrName)
		{
			if (attrStrings == null)
			{
				throw new System.ArgumentException("Attribute values array cannot be null");
			}
			for (int i = 0, u = attrStrings.Length; i < u; i++)
			{
				try
				{
					if ((System.Object) attrStrings[i] == null)
					{
						throw new System.ArgumentException("Attribute value " + "at array index " + i + " cannot be null");
					}
					System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
					byte[] ibytes = encoder.GetBytes(attrStrings[i]);
					sbyte[] sbytes=SupportClass.ToSByteArray(ibytes);
					this.add(sbytes);
//					this.add(attrStrings[i].getBytes("UTF-8"));
				}
				catch (System.IO.IOException e)
				{
					throw new System.SystemException(e.ToString());
				}
			}
			return ;
		}
		
		/// <summary> Returns a clone of this LdapAttribute.
		/// 
		/// </summary>
		/// <returns> clone of this LdapAttribute.
		/// </returns>
		public System.Object Clone()
		{
			try
			{
				System.Object newObj = base.MemberwiseClone();
				if (values != null)
				{
					Array.Copy((System.Array) this.values, 0, (System.Array) ((LdapAttribute) newObj).values, 0, this.values.Length);
				}
				return newObj;
			}
			catch (System.Exception ce)
			{
				throw new System.SystemException("Internal error, cannot create clone");
			}
		}
		
		/// <summary> Adds a string value to the attribute.
		/// 
		/// </summary>
		/// <param name="attrString">Value of the attribute as a String.
		/// 
		/// @throws IllegalArgumentException if attrString is null
		/// </param>
		public virtual void  addValue(System.String attrString)
		{
			if ((System.Object) attrString == null)
			{
				throw new System.ArgumentException("Attribute value cannot be null");
			}
			try
			{
				System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
				byte[] ibytes = encoder.GetBytes(attrString);
				sbyte[] sbytes=SupportClass.ToSByteArray(ibytes);
				this.add(sbytes);
//				this.add(attrString.getBytes("UTF-8"));
			}
			catch (System.IO.IOException ue)
			{
				throw new System.SystemException(ue.ToString());
			}
			return ;
		}
		
		/// <summary> Adds a byte-formatted value to the attribute.
		/// 
		/// </summary>
		/// <param name="attrBytes">Value of the attribute as raw bytes.
		/// 
		///  Note: If attrBytes represents a string it should be UTF-8 encoded.
		/// 
		/// @throws IllegalArgumentException if attrBytes is null
		/// </param>
		[CLSCompliantAttribute(false)]
		public virtual void  addValue(sbyte[] attrBytes)
		{
			if (attrBytes == null)
			{
				throw new System.ArgumentException("Attribute value cannot be null");
			}
			this.add(attrBytes);
			return ;
		}
		
		/// <summary> Adds a base64 encoded value to the attribute.
		/// The value will be decoded and stored as bytes.  String
		/// data encoded as a base64 value must be UTF-8 characters.
		/// 
		/// </summary>
		/// <param name="attrString">The base64 value of the attribute as a String.
		/// 
		/// @throws IllegalArgumentException if attrString is null
		/// </param>
		public virtual void  addBase64Value(System.String attrString)
		{
			if ((System.Object) attrString == null)
			{
				throw new System.ArgumentException("Attribute value cannot be null");
			}
			
			this.add(Base64.decode(attrString));
			return ;
		}
		
		/// <summary> Adds a base64 encoded value to the attribute.
		/// The value will be decoded and stored as bytes.  Character
		/// data encoded as a base64 value must be UTF-8 characters.
		/// 
		/// </summary>
		/// <param name="attrString">The base64 value of the attribute as a StringBuffer.
		/// </param>
		/// <param name="start"> The start index of base64 encoded part, inclusive.
		/// </param>
		/// <param name="end"> The end index of base encoded part, exclusive.
		/// 
		/// @throws IllegalArgumentException if attrString is null
		/// </param>
		public virtual void  addBase64Value(System.Text.StringBuilder attrString, int start, int end)
		{
			if (attrString == null)
			{
				throw new System.ArgumentException("Attribute value cannot be null");
			}
			
			this.add(Base64.decode(attrString, start, end));
			
			return ;
		}
		
		/// <summary> Adds a base64 encoded value to the attribute.
		/// The value will be decoded and stored as bytes.  Character
		/// data encoded as a base64 value must be UTF-8 characters.
		/// 
		/// </summary>
		/// <param name="attrChars">The base64 value of the attribute as an array of
		/// characters.
		/// 
		/// @throws IllegalArgumentException if attrString is null
		/// </param>
		public virtual void  addBase64Value(char[] attrChars)
		{
			if (attrChars == null)
			{
				throw new System.ArgumentException("Attribute value cannot be null");
			}
			
			this.add(Base64.decode(attrChars));
			return ;
		}
		
		/// <summary> Adds a URL, indicating a file or other resource that contains
		/// the value of the attribute.
		/// 
		/// </summary>
		/// <param name="url">String value of a URL pointing to the resource containing
		/// the value of the attribute.
		/// 
		/// @throws IllegalArgumentException if url is null
		/// </param>
		public virtual void  addURLValue(System.String url)
		{
			if ((System.Object) url == null)
			{
				throw new System.ArgumentException("Attribute URL cannot be null");
			}
			addURLValue(new System.Uri(url));
			return ;
		}
		
		/// <summary> Adds a URL, indicating a file or other resource that contains
		/// the value of the attribute.
		/// 
		/// </summary>
		/// <param name="url">A URL class pointing to the resource containing the value
		/// of the attribute.
		/// 
		/// @throws IllegalArgumentException if url is null
		/// </param>
		public virtual void  addURLValue(System.Uri url)
		{
			// Class to encapsulate the data bytes and the length
			if (url == null)
			{
				throw new System.ArgumentException("Attribute URL cannot be null");
			}
			try
			{
				// Get InputStream from the URL
				System.IO.Stream in_Renamed = System.Net.WebRequest.Create(url).GetResponse().GetResponseStream();
				// Read the bytes into buffers and store the them in an arraylist
				System.Collections.ArrayList bufs = new System.Collections.ArrayList();
				sbyte[] buf = new sbyte[4096];
				int len, totalLength = 0;
				while ((len = SupportClass.ReadInput(in_Renamed, ref buf, 0, 4096)) != - 1)
				{
					bufs.Add(new URLData(this, buf, len));
					buf = new sbyte[4096];
					totalLength += len;
				}
				/*
				* Now that the length is known, allocate an array to hold all
				* the bytes of data and copy the data to that array, store
				* it in this LdapAttribute
				*/
				sbyte[] data = new sbyte[totalLength];
				int offset = 0; //
				for (int i = 0; i < bufs.Count; i++)
				{
					URLData b = (URLData) bufs[i];
					len = b.getLength();
					Array.Copy((System.Array) b.getData(), 0, (System.Array) data, offset, len);
					offset += len;
				}
				this.add(data);
			}
			catch (System.IO.IOException ue)
			{
				throw new System.SystemException(ue.ToString());
			}
			return ;
		}
		
		/// <summary> Returns the base name of the attribute.
		/// 
		/// For example, if the attribute name is cn;lang-ja;phonetic,
		/// this method returns cn.
		/// 
		/// </summary>
		/// <returns> The base name of the attribute.
		/// </returns>
		public virtual System.String getBaseName()
		{
			return baseName;
		}
		
		/// <summary> Returns the base name of the specified attribute name.
		/// 
		/// For example, if the attribute name is cn;lang-ja;phonetic,
		/// this method returns cn.
		/// 
		/// </summary>
		/// <param name="attrName">Name of the attribute from which to extract the
		/// base name.
		/// 
		/// </param>
		/// <returns> The base name of the attribute.
		/// 
		/// @throws IllegalArgumentException if attrName is null
		/// </returns>
		public static System.String getBaseName(System.String attrName)
		{
			if ((System.Object) attrName == null)
			{
				throw new System.ArgumentException("Attribute name cannot be null");
			}
			int idx = attrName.IndexOf((System.Char) ';');
			if (- 1 == idx)
			{
				return attrName;
			}
			return attrName.Substring(0, (idx) - (0));
		}
		
		/// <summary> Extracts the subtypes from the attribute name.
		/// 
		/// For example, if the attribute name is cn;lang-ja;phonetic,
		/// this method returns an array containing lang-ja and phonetic.
		/// 
		/// </summary>
		/// <returns> An array subtypes or null if the attribute has none.
		/// </returns>
		public virtual System.String[] getSubtypes()
		{
			return subTypes;
		}
		
		/// <summary> Extracts the subtypes from the specified attribute name.
		/// 
		/// For example, if the attribute name is cn;lang-ja;phonetic,
		/// this method returns an array containing lang-ja and phonetic.
		/// 
		/// </summary>
		/// <param name="attrName">  Name of the attribute from which to extract
		/// the subtypes.
		/// 
		/// </param>
		/// <returns> An array subtypes or null if the attribute has none.
		/// 
		/// @throws IllegalArgumentException if attrName is null
		/// </returns>
		public static System.String[] getSubtypes(System.String attrName)
		{
			if ((System.Object) attrName == null)
			{
				throw new System.ArgumentException("Attribute name cannot be null");
			}
			SupportClass.Tokenizer st = new SupportClass.Tokenizer(attrName, ";");
			System.String[] subTypes = null;
			int cnt = st.Count;
			if (cnt > 0)
			{
				st.NextToken(); // skip over basename
				subTypes = new System.String[cnt - 1];
				int i = 0;
				while (st.HasMoreTokens())
				{
					subTypes[i++] = st.NextToken();
				}
			}
			return subTypes;
		}
		
		/// <summary> Reports if the attribute name contains the specified subtype.
		/// 
		/// For example, if you check for the subtype lang-en and the
		/// attribute name is cn;lang-en, this method returns true.
		/// 
		/// </summary>
		/// <param name="subtype"> The single subtype to check for.
		/// 
		/// </param>
		/// <returns> True, if the attribute has the specified subtype;
		/// false, if it doesn't.
		/// 
		/// @throws IllegalArgumentException if subtype is null
		/// </returns>
		public virtual bool hasSubtype(System.String subtype)
		{
			if ((System.Object) subtype == null)
			{
				throw new System.ArgumentException("subtype cannot be null");
			}
			if (null != this.subTypes)
			{
				for (int i = 0; i < subTypes.Length; i++)
				{
					if (subTypes[i].ToUpper().Equals(subtype.ToUpper()))
						return true;
				}
			}
			return false;
		}
		
		/// <summary> Reports if the attribute name contains all the specified subtypes.
		/// 
		///  For example, if you check for the subtypes lang-en and phonetic
		/// and if the attribute name is cn;lang-en;phonetic, this method
		/// returns true. If the attribute name is cn;phonetic or cn;lang-en,
		/// this method returns false.
		/// 
		/// </summary>
		/// <param name="subtypes">  An array of subtypes to check for.
		/// 
		/// </param>
		/// <returns> True, if the attribute has all the specified subtypes;
		/// false, if it doesn't have all the subtypes.
		/// 
		/// @throws IllegalArgumentException if subtypes is null or if array member
		/// is null.
		/// </returns>
		public virtual bool hasSubtypes(System.String[] subtypes)
		{
			if (subtypes == null)
			{
				throw new System.ArgumentException("subtypes cannot be null");
			}
			for (int i = 0; i < subtypes.Length; i++)
			{
				for (int j = 0; j < subTypes.Length; j++)
				{
					if ((System.Object) subTypes[j] == null)
					{
						throw new System.ArgumentException("subtype " + "at array index " + i + " cannot be null");
					}
					if (subTypes[j].ToUpper().Equals(subtypes[i].ToUpper()))
					{
						goto gotSubType;
					}
				}
				return false;
gotSubType: ;
			}
			return true;
		}
		
		/// <summary> Removes a string value from the attribute.
		/// 
		/// </summary>
		/// <param name="attrString">  Value of the attribute as a string.
		/// 
		/// Note: Removing a value which is not present in the attribute has
		/// no effect.
		/// 
		/// @throws IllegalArgumentException if attrString is null
		/// </param>
		public virtual void  removeValue(System.String attrString)
		{
			if (null == (System.Object) attrString)
			{
				throw new System.ArgumentException("Attribute value cannot be null");
			}
			try
			{
				System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
				byte[] ibytes = encoder.GetBytes(attrString);
				sbyte[] sbytes=SupportClass.ToSByteArray(ibytes);
				this.removeValue(sbytes);
//				this.removeValue(attrString.getBytes("UTF-8"));
			}
			catch (System.IO.IOException uee)
			{
				// This should NEVER happen but just in case ...
				throw new System.SystemException(uee.ToString());
			}
			return ;
		}
		
		/// <summary> Removes a byte-formatted value from the attribute.
		/// 
		/// </summary>
		/// <param name="attrBytes">   Value of the attribute as raw bytes.
		///  Note: If attrBytes represents a string it should be UTF-8 encoded.
		/// Example: <code>String.getBytes("UTF-8");</code>
		/// 
		/// Note: Removing a value which is not present in the attribute has
		/// no effect.
		/// 
		/// @throws IllegalArgumentException if attrBytes is null
		/// </param>
		[CLSCompliantAttribute(false)]
		public virtual void  removeValue(sbyte[] attrBytes)
		{
			if (null == attrBytes)
			{
				throw new System.ArgumentException("Attribute value cannot be null");
			}
			for (int i = 0; i < this.values.Length; i++)
			{
				if (equals(attrBytes, (sbyte[]) this.values[i]))
				{
					if (0 == i && 1 == this.values.Length)
					{
						// Optimize if first element of a single valued attr
						this.values = null;
						return ;
					}
					if (this.values.Length == 1)
					{
						this.values = null;
					}
					else
					{
						int moved = this.values.Length - i - 1;
						System.Object[] tmp = new System.Object[this.values.Length - 1];
						if (i != 0)
						{
							Array.Copy((System.Array) values, 0, (System.Array) tmp, 0, i);
						}
						if (moved != 0)
						{
							Array.Copy((System.Array) values, i + 1, (System.Array) tmp, i, moved);
						}
						this.values = tmp;
						tmp = null;
					}
					break;
				}
			}
			return ;
		}
		
		/// <summary> Returns the number of values in the attribute.
		/// 
		/// </summary>
		/// <returns> The number of values in the attribute.
		/// </returns>
		public virtual int size()
		{
			return null == this.values?0:this.values.Length;
		}
		
		/// <summary> Compares this object with the specified object for order.
		/// 
		///  Ordering is determined by comparing attribute names (see
		/// {@link #getName() }) using the method compareTo() of the String class.
		/// 
		/// 
		/// </summary>
		/// <param name="attribute">  The LdapAttribute to be compared to this object.
		/// 
		/// </param>
		/// <returns>            Returns a negative integer, zero, or a positive
		/// integer as this object is less than, equal to, or greater than the
		/// specified object.
		/// </returns>
		public virtual int CompareTo(System.Object attribute)
		{
			
			return name.CompareTo(((LdapAttribute) attribute).name);
		}
		
		/// <summary> Adds an object to <code>this</code> object's list of attribute values
		/// 
		/// </summary>
		/// <param name="bytes">  Ultimately all of this attribute's values are treated
		/// as binary data so we simplify the process by requiring
		/// that all data added to our list is in binary form.
		/// 
		///  Note: If attrBytes represents a string it should be UTF-8 encoded.
		/// </param>
		private void  add(sbyte[] bytes)
		{
			if (null == this.values)
			{
				this.values = new System.Object[]{bytes};
			}
			else
			{
				// Duplicate attribute values not allowed
				for (int i = 0; i < this.values.Length; i++)
				{
					if (equals(bytes, (sbyte[]) this.values[i]))
					{
						return ; // Duplicate, don't add
					}
				}
				System.Object[] tmp = new System.Object[this.values.Length + 1];
				Array.Copy((System.Array) this.values, 0, (System.Array) tmp, 0, this.values.Length);
				tmp[this.values.Length] = bytes;
				this.values = tmp;
				tmp = null;
			}
			return ;
		}
		
		/// <summary> Returns true if the two specified arrays of bytes are equal to each
		/// another.  Matches the logic of Arrays.equals which is not available
		/// in jdk 1.1.x.
		/// 
		/// </summary>
		/// <param name="e1">the first array to be tested
		/// </param>
		/// <param name="e2">the second array to be tested
		/// </param>
		/// <returns> true if the two arrays are equal
		/// </returns>
		private bool equals(sbyte[] e1, sbyte[] e2)
		{
			// If same object, they compare true
			if (e1 == e2)
				return true;
			
			// If either but not both are null, they compare false
			if (e1 == null || e2 == null)
				return false;
			
			// If arrays have different length, they compare false
			int length = e1.Length;
			if (e2.Length != length)
				return false;
			
			// If any of the bytes are different, they compare false
			for (int i = 0; i < length; i++)
			{
				if (e1[i] != e2[i])
					return false;
			}
			
			return true;
		}
		
		/// <summary> Returns a string representation of this LdapAttribute
		/// 
		/// </summary>
		/// <returns> a string representation of this LdapAttribute
		/// </returns>
		public override System.String ToString()
		{
			System.Text.StringBuilder result = new System.Text.StringBuilder("LdapAttribute: ");
			try
			{
				result.Append("{type='" + name + "'");
				if (values != null)
				{
					result.Append(", ");
					if (values.Length == 1)
					{
						result.Append("value='");
					}
					else
					{
						result.Append("values='");
					}
					for (int i = 0; i < values.Length; i++)
					{
						if (i != 0)
						{
							result.Append("','");
						}
						if (((sbyte[]) values[i]).Length == 0)
						{
							continue;
						}
						System.Text.Encoding encoder = System.Text.Encoding.GetEncoding("utf-8"); 
//						char[] dchar = encoder.GetChars((byte[]) values[i]);
						char[] dchar = encoder.GetChars(SupportClass.ToByteArray((sbyte[])values[i]));
						System.String sval = new String(dchar);

//						System.String sval = new String((sbyte[]) values[i], "UTF-8");
						if (sval.Length == 0)
						{
							// didn't decode well, must be binary
							result.Append("<binary value, length:" + sval.Length);
							continue;
						}
						result.Append(sval);
					}
					result.Append("'");
				}
				result.Append("}");
			}
			catch (System.Exception e)
			{
				throw new System.SystemException(e.ToString());
			}
			return result.ToString();
		}
	}
}
