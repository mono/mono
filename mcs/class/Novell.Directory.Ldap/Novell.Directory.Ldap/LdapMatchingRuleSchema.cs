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
// Novell.Directory.Ldap.LdapMatchingRuleSchema.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using SchemaParser = Novell.Directory.Ldap.Utilclass.SchemaParser;

namespace Novell.Directory.Ldap
{
	/// <summary>  The schematic definition of a particular matching rule
	/// in a particular Directory Server.
	/// 
	/// The LdapMatchingRuleSchema class represents the definition of a mathcing
	/// rule.  It is used to query matching rule syntax, and to add or delete a
	/// matching rule definition in a directory.
	/// 
	/// Novell eDirectory does not currently allow matching rules to be added
	/// or deleted from the schema.
	/// 
	/// </summary>
	/// <seealso cref="LdapAttributeSchema">
	/// </seealso>
	/// <seealso cref="LdapSchemaElement">
	/// </seealso>
	/// <seealso cref="LdapSchema">
	/// </seealso>
	public class LdapMatchingRuleSchema:LdapSchemaElement
	{
		/// <summary> Returns the OIDs of the attributes to which this rule applies.
		/// 
		/// </summary>
		/// <returns> The OIDs of the attributes to which this matching rule applies.
		/// </returns>
		virtual public System.String[] Attributes
		{
			get
			{
				return attributes;
			}
			
		}
		/// <summary> Returns the OID of the syntax that this matching rule is valid for.
		/// 
		/// </summary>
		/// <returns> The OID of the syntax that this matching rule is valid for.
		/// </returns>
		virtual public System.String SyntaxString
		{
			get
			{
				return syntaxString;
			}
			
		}
		private System.String syntaxString;
		private System.String[] attributes;
		/// <summary> Constructs a matching rule definition for adding to or deleting from
		/// a directory.
		/// 
		/// </summary>
		/// <param name="names">      The names of the attribute.
		/// 
		/// </param>
		/// <param name="oid">        Object Identifier of the attribute - in
		/// dotted-decimal format.
		/// 
		/// </param>
		/// <param name="description">  Optional description of the attribute.
		/// 
		/// </param>
		/// <param name="attributes">   The OIDs of attributes to which the rule applies.
		/// This parameter may be null. All attributes added to
		/// this array must use the same syntax.
		/// 
		/// </param>
		/// <param name="obsolete">     true if this matching rule is obsolete.
		/// 
		/// 
		/// </param>
		/// <param name="syntaxString">  The unique object identifer of the syntax of the
		/// attribute, in dotted numerical format.
		/// 
		/// </param>
		public LdapMatchingRuleSchema(System.String[] names, System.String oid, System.String description, System.String[] attributes, bool obsolete, System.String syntaxString):base(LdapSchema.schemaTypeNames[LdapSchema.MATCHING])
		{
			base.names = new System.String[names.Length];
			names.CopyTo(base.names, 0);
			base.oid = oid;
			base.description = description;
			base.obsolete = obsolete;
			this.attributes = new System.String[attributes.Length];
			attributes.CopyTo(this.attributes, 0);
			this.syntaxString = syntaxString;
			base.Value = formatString();
			return ;
		}
		
		
		/// <summary> Constructs a matching rule definition from the raw string values
		/// returned from a schema query for "matchingRule" and for
		/// "matchingRuleUse" for the same rule.
		/// 
		/// </summary>
		/// <param name="rawMatchingRule">   The raw string value returned on a directory
		/// query for "matchingRule".
		/// 
		/// </param>
		/// <param name="rawMatchingRuleUse"> The raw string value returned on a directory
		/// query for "matchingRuleUse".
		/// </param>
		public LdapMatchingRuleSchema(System.String rawMatchingRule, System.String rawMatchingRuleUse):base(LdapSchema.schemaTypeNames[LdapSchema.MATCHING])
		{
			try
			{
				SchemaParser matchParser = new SchemaParser(rawMatchingRule);
				base.names = new System.String[matchParser.Names.Length];
				matchParser.Names.CopyTo(base.names, 0);
				base.oid = matchParser.ID;
				base.description = matchParser.Description;
				base.obsolete = matchParser.Obsolete;
				this.syntaxString = matchParser.Syntax;
				if ((System.Object) rawMatchingRuleUse != null)
				{
					SchemaParser matchUseParser = new SchemaParser(rawMatchingRuleUse);
					this.attributes = matchUseParser.Applies;
				}
				base.Value = formatString();
			}
			catch (System.IO.IOException e)
			{
			}
			return ;
		}
		
		/// <summary> Returns a string in a format suitable for directly adding to a
		/// directory, as a value of the particular schema element attribute.
		/// 
		/// </summary>
		/// <returns> A string representation of the attribute's definition.
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
			if ((System.Object) (token = SyntaxString) != null)
			{
				valueBuffer.Append(" SYNTAX ");
				valueBuffer.Append(token);
			}
			valueBuffer.Append(" )");
			return valueBuffer.ToString();
		}
	}
}
