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
// Novell.Directory.Ldap.LdapObjectClassSchema.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap.Utilclass;

namespace Novell.Directory.Ldap
{
	
	/// <summary>  The schema definition of an object class in a directory server.
	/// 
	/// The LdapObjectClassSchema class represents the definition of an object
	/// class.  It is used to query the syntax of an object class.
	/// 
	/// </summary>
	/// <seealso cref="LdapSchemaElement">
	/// </seealso>
	/// <seealso cref="LdapSchema">
	/// </seealso>
	public class LdapObjectClassSchema:LdapSchemaElement
	{
		/// <summary> Returns the object classes from which this one derives.
		/// 
		/// </summary>
		/// <returns> The object classes superior to this class.
		/// </returns>
		virtual public System.String[] Superiors
		{
			get
			{
				return superiors;
			}
			
		}
		/// <summary> Returns a list of attributes required for an entry with this object
		/// class.
		/// 
		/// </summary>
		/// <returns> The list of required attributes defined for this class.
		/// </returns>
		virtual public System.String[] RequiredAttributes
		{
			get
			{
				return required;
			}
			
		}
		/// <summary> Returns a list of optional attributes but not required of an entry
		/// with this object class.
		/// 
		/// </summary>
		/// <returns> The list of optional attributes defined for this class.
		/// </returns>
		virtual public System.String[] OptionalAttributes
		{
			get
			{
				return optional;
			}
			
		}
		/// <summary> Returns the type of object class.
		/// 
		/// The getType method returns one of the following constants defined in
		/// LdapObjectClassSchema:
		/// <ul>
		/// <li>ABSTRACT</li>
		/// <li>AUXILIARY</li>
		/// <li>STRUCTURAL</li>
		/// </ul>
		/// See the LdapSchemaElement.getQualifier method for information on
		/// obtaining the X-NDS flags.
		/// 
		/// </summary>
		/// <returns> The type of object class.
		/// </returns>
		virtual public int Type
		{
			get
			{
				return type;
			}
			
		}
		internal System.String[] superiors;
		internal System.String[] required;
		internal System.String[] optional;
		internal int type = - 1;
		
		/// <summary> This class definition defines an abstract schema class.
		/// 
		/// This is equivalent to setting the Novell eDirectory effective class
		/// flag to true.
		/// </summary>
		public const int ABSTRACT = 0;
		
		/// <summary> This class definition defines a structural schema class.
		/// 
		/// This is equivalent to setting the Novell eDirectory effective class
		/// flag to true.
		/// </summary>
		public const int STRUCTURAL = 1;
		
		/// <summary> This class definition defines an auxiliary schema class.</summary>
		public const int AUXILIARY = 2;
		
		/// <summary> Constructs an object class definition for adding to or deleting from
		/// a directory's schema.
		/// 
		/// </summary>
		/// <param name="names">    Name(s) of the object class.
		/// 
		/// </param>
		/// <param name="oid">      Object Identifer of the object class - in
		/// dotted-decimal format.
		/// 
		/// </param>
		/// <param name="description">   Optional description of the object class.
		/// 
		/// </param>
		/// <param name="superiors">     The object classes from which this one derives.
		/// 
		/// </param>
		/// <param name="required">      A list of attributes required
		/// for an entry with this object class.
		/// 
		/// </param>
		/// <param name="optional">      A list of attributes acceptable but not required
		/// for an entry with this object class.
		/// 
		/// </param>
		/// <param name="type">          One of ABSTRACT, AUXILIARY, or STRUCTURAL. These
		/// constants are defined in LdapObjectClassSchema.
		/// 
		/// </param>
		/// <param name="obsolete">      true if this object is obsolete
		/// 
		/// </param>
		public LdapObjectClassSchema(System.String[] names, System.String oid, System.String[] superiors, System.String description, System.String[] required, System.String[] optional, int type, bool obsolete):base(LdapSchema.schemaTypeNames[LdapSchema.OBJECT_CLASS])
		{
			base.names = new System.String[names.Length];
			names.CopyTo(base.names, 0);
			base.oid = oid;
			base.description = description;
			this.type = type;
			this.obsolete = obsolete;
			if (superiors != null)
			{
				this.superiors = new System.String[superiors.Length];
				superiors.CopyTo(this.superiors, 0);
			}
			if (required != null)
			{
				this.required = new System.String[required.Length];
				required.CopyTo(this.required, 0);
			}
			if (optional != null)
			{
				this.optional = new System.String[optional.Length];
				optional.CopyTo(this.optional, 0);
			}
			base.Value = formatString();
			return ;
		}
		
		
		
		/// <summary> Constructs an object class definition from the raw string value
		/// returned from a directory query for "objectClasses".
		/// 
		/// </summary>
		/// <param name="raw">     The raw string value returned from a directory
		/// query for "objectClasses".
		/// </param>
		public LdapObjectClassSchema(System.String raw):base(LdapSchema.schemaTypeNames[LdapSchema.OBJECT_CLASS])
		{
			try
			{
				SchemaParser parser = new SchemaParser(raw);
				
				if (parser.Names != null)
				{
					base.names = new System.String[parser.Names.Length];
					parser.Names.CopyTo(base.names, 0);
				}
				
				if ((System.Object) parser.ID != null)
					base.oid = parser.ID;
				if ((System.Object) parser.Description != null)
					base.description = parser.Description;
				base.obsolete = parser.Obsolete;
				if (parser.Required != null)
				{
					required = new System.String[parser.Required.Length];
					parser.Required.CopyTo(required, 0);
				}
				if (parser.Optional != null)
				{
					optional = new System.String[parser.Optional.Length];
					parser.Optional.CopyTo(optional, 0);
				}
				if (parser.Superiors != null)
				{
					superiors = new System.String[parser.Superiors.Length];
					parser.Superiors.CopyTo(superiors, 0);
				}
				type = parser.Type;
				System.Collections.IEnumerator qualifiers = parser.Qualifiers;
				AttributeQualifier attrQualifier;
				while (qualifiers.MoveNext())
				{
					attrQualifier = (AttributeQualifier) qualifiers.Current;
					setQualifier(attrQualifier.Name, attrQualifier.Values);
				}
				base.Value = formatString();
			}
			catch (System.IO.IOException e)
			{
			}
			return ;
		}
		
		/// <summary> Returns a string in a format suitable for directly adding to a
		/// directory, as a value of the particular schema element class.
		/// 
		/// </summary>
		/// <returns> A string representation of the class' definition.
		/// </returns>
		protected internal override System.String formatString()
		{
			
			System.Text.StringBuilder valueBuffer = new System.Text.StringBuilder("( ");
			System.String token;
			System.String[] strArray;
			
			if ((System.Object) (token = ID) != null)
			{
				valueBuffer.Append(token);
			}
			strArray = Names;
			if (strArray != null)
			{
				valueBuffer.Append(" NAME ");
				if (strArray.Length == 1)
				{
					valueBuffer.Append("'" + strArray[0] + "'");
				}
				else
				{
					valueBuffer.Append("( ");
					
					for (int i = 0; i < strArray.Length; i++)
					{
						valueBuffer.Append(" '" + strArray[i] + "'");
					}
					valueBuffer.Append(" )");
				}
			}
			if ((System.Object) (token = Description) != null)
			{
				valueBuffer.Append(" DESC ");
				valueBuffer.Append("'" + token + "'");
			}
			if (Obsolete)
			{
				valueBuffer.Append(" OBSOLETE");
			}
			if ((strArray = Superiors) != null)
			{
				valueBuffer.Append(" SUP ");
				if (strArray.Length > 1)
					valueBuffer.Append("( ");
				for (int i = 0; i < strArray.Length; i++)
				{
					if (i > 0)
						valueBuffer.Append(" $ ");
					valueBuffer.Append(strArray[i]);
				}
				if (strArray.Length > 1)
					valueBuffer.Append(" )");
			}
			if (Type != - 1)
			{
				if (Type == LdapObjectClassSchema.ABSTRACT)
					valueBuffer.Append(" ABSTRACT");
				else if (Type == LdapObjectClassSchema.AUXILIARY)
					valueBuffer.Append(" AUXILIARY");
				else if (Type == LdapObjectClassSchema.STRUCTURAL)
					valueBuffer.Append(" STRUCTURAL");
			}
			if ((strArray = RequiredAttributes) != null)
			{
				valueBuffer.Append(" MUST ");
				if (strArray.Length > 1)
					valueBuffer.Append("( ");
				for (int i = 0; i < strArray.Length; i++)
				{
					if (i > 0)
						valueBuffer.Append(" $ ");
					valueBuffer.Append(strArray[i]);
				}
				if (strArray.Length > 1)
					valueBuffer.Append(" )");
			}
			if ((strArray = OptionalAttributes) != null)
			{
				valueBuffer.Append(" MAY ");
				if (strArray.Length > 1)
					valueBuffer.Append("( ");
				for (int i = 0; i < strArray.Length; i++)
				{
					if (i > 0)
						valueBuffer.Append(" $ ");
					valueBuffer.Append(strArray[i]);
				}
				if (strArray.Length > 1)
					valueBuffer.Append(" )");
			}
			System.Collections.IEnumerator en;
			if ((en = QualifierNames) != null)
			{
				System.String qualName;
				System.String[] qualValue;
				while (en.MoveNext())
				{
					qualName = ((System.String) en.Current);
					valueBuffer.Append(" " + qualName + " ");
					if ((qualValue = getQualifier(qualName)) != null)
					{
						if (qualValue.Length > 1)
							valueBuffer.Append("( ");
						for (int i = 0; i < qualValue.Length; i++)
						{
							if (i > 0)
								valueBuffer.Append(" ");
							valueBuffer.Append("'" + qualValue[i] + "'");
						}
						if (qualValue.Length > 1)
							valueBuffer.Append(" )");
					}
				}
			}
			valueBuffer.Append(" )");
			return valueBuffer.ToString();
		}
	}
}
