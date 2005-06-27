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
// Novell.Directory.Ldap.LdapSyntaxSchema.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using SchemaParser = Novell.Directory.Ldap.Utilclass.SchemaParser;
using AttributeQualifier = Novell.Directory.Ldap.Utilclass.AttributeQualifier;

namespace Novell.Directory.Ldap
{
	
	/// <summary> Represents a syntax definition in the directory schema.
	/// 
	/// The LdapSyntaxSchema class represents the definition of a syntax.  It is
	/// used to discover the known set of syntaxes in effect for the subschema. 
	/// 
	/// Although this extends LdapSchemaElement, it does not use the name or
	/// obsolete members. Therefore, calls to the getName method always return
	/// null and to the isObsolete method always returns false. There is also no
	/// matching getSyntaxNames method in LdapSchema. Note also that adding and
	/// removing syntaxes is not typically a supported feature of Ldap servers.
	/// </summary>
	
	public class LdapSyntaxSchema:LdapSchemaElement
	{
		
		/// <summary> Constructs a syntax for adding to or deleting from the schema.
		/// 
		/// Adding and removing syntaxes is not typically a supported
		/// feature of Ldap servers. Novell eDirectory does not allow syntaxes to
		/// be added or removed.
		/// 
		/// </summary>
		/// <param name="oid">        The unique object identifier of the syntax - in
		/// dotted numerical format.
		/// 
		/// </param>
		/// <param name="description">An optional description of the syntax.
		/// </param>
		public LdapSyntaxSchema(System.String oid, System.String description):base(LdapSchema.schemaTypeNames[LdapSchema.SYNTAX])
		{
			base.oid = oid;
			base.description = description;
			base.Value = formatString();
			return ;
		}
		
		/// <summary> Constructs a syntax from the raw string value returned on a schema
		/// query for LdapSyntaxes.
		/// 
		/// </summary>
		/// <param name="raw">          The raw string value returned from a schema
		/// query for ldapSyntaxes.
		/// </param>
		public LdapSyntaxSchema(System.String raw):base(LdapSchema.schemaTypeNames[LdapSchema.SYNTAX])
		{
			try
			{
				SchemaParser parser = new SchemaParser(raw);
				
				if ((System.Object) parser.ID != null)
					base.oid = parser.ID;
				if ((System.Object) parser.Description != null)
					base.description = parser.Description;
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
				throw new System.SystemException(e.ToString());
			}
			return ;
		}
		
		/// <summary> Returns a string in a format suitable for directly adding to a
		/// directory, as a value of the particular schema element class.
		/// 
		/// </summary>
		/// <returns> A string representation of the syntax's definition.
		/// </returns>
		protected internal override System.String formatString()
		{
			System.Text.StringBuilder valueBuffer = new System.Text.StringBuilder("( ");
			System.String token;
			
			if ((System.Object) (token = ID) != null)
			{
				valueBuffer.Append(token);
			}
			if ((System.Object) (token = Description) != null)
			{
				valueBuffer.Append(" DESC ");
				valueBuffer.Append("'" + token + "'");
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
						{
							valueBuffer.Append("( ");
							for (int i = 0; i < qualValue.Length; i++)
							{
								if (i > 0)
								{
									valueBuffer.Append(" ");
								}
								valueBuffer.Append("'" + qualValue[i] + "'");
							}
							if (qualValue.Length > 1)
							{
								valueBuffer.Append(" )");
							}
						}
					}
				}
			}
			valueBuffer.Append(" )");
			return valueBuffer.ToString();
		}
	}
}
