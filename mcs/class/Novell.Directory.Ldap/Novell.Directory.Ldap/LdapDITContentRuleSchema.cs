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
// Novell.Directory.Ldap.LdapDITContentRuleSchema.cs
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
	
	/// <summary>  Represents a DIT (Directory Information Tree) content rule
	/// in a directory schema.
	/// 
	/// The LdapDITContentRuleSchema class is used to discover or modify
	/// additional auxiliary classes, mandatory and optional attributes, and
	/// restricted attributes in effect for an object class.
	/// </summary>
	public class LdapDITContentRuleSchema:LdapSchemaElement
	{
		/// <summary> Returns the list of allowed auxiliary classes.
		/// 
		/// </summary>
		/// <returns> The list of allowed auxiliary classes.
		/// </returns>
		virtual public System.String[] AuxiliaryClasses
		{
			get
			{
				return auxiliary;
			}
			
		}
		/// <summary> Returns the list of additional required attributes for an entry
		/// controlled by this content rule.
		/// 
		/// </summary>
		/// <returns> The list of additional required attributes.
		/// </returns>
		virtual public System.String[] RequiredAttributes
		{
			get
			{
				return required;
			}
			
		}
		/// <summary> Returns the list of additional optional attributes for an entry
		/// controlled by this content rule.
		/// 
		/// </summary>
		/// <returns> The list of additional optional attributes.
		/// </returns>
		virtual public System.String[] OptionalAttributes
		{
			get
			{
				return optional;
			}
			
		}
		/// <summary> Returns the list of precluded attributes for an entry controlled by
		/// this content rule.
		/// 
		/// </summary>
		/// <returns> The list of precluded attributes.
		/// </returns>
		virtual public System.String[] PrecludedAttributes
		{
			get
			{
				return precluded;
			}
			
		}
		private System.String[] auxiliary = new System.String[]{""};
		private System.String[] required = new System.String[]{""};
		private System.String[] optional = new System.String[]{""};
		private System.String[] precluded = new System.String[]{""};
		
		/// <summary> Constructs a DIT content rule for adding to or deleting from the
		/// schema.
		/// 
		/// </summary>
		/// <param name="names">       The names of the content rule.
		/// 
		/// </param>
		/// <param name="oid">        The unique object identifier of the content rule -
		/// in dotted numerical format.
		/// 
		/// </param>
		/// <param name="description">The optional description of the content rule.
		/// 
		/// </param>
		/// <param name="obsolete">   True if the content rule is obsolete.
		/// 
		/// </param>
		/// <param name="auxiliary">  A list of auxiliary object classes allowed for
		/// an entry to which this content rule applies.
		/// These may either be specified by name or
		/// numeric oid.
		/// 
		/// </param>
		/// <param name="required">   A list of attributes that an entry
		/// to which this content rule applies must
		/// contain in addition to its normal set of
		/// mandatory attributes. These attributes may be
		/// specified by either name or numeric oid.
		/// 
		/// </param>
		/// <param name="optional">   A list of attributes that an entry
		/// to which this content rule applies may contain
		/// in addition to its normal set of optional
		/// attributes. These attributes may be specified by
		/// either name or numeric oid.
		/// 
		/// </param>
		/// <param name="precluded">  A list, consisting of a subset of the optional
		/// attributes of the structural and
		/// auxiliary object classes which are precluded
		/// from an entry to which this content rule
		/// applies. These may be specified by either name
		/// or numeric oid.
		/// </param>
		public LdapDITContentRuleSchema(System.String[] names, System.String oid, System.String description, bool obsolete, System.String[] auxiliary, System.String[] required, System.String[] optional, System.String[] precluded):base(LdapSchema.schemaTypeNames[LdapSchema.DITCONTENT])
		{
			base.names = new System.String[names.Length];
			names.CopyTo(base.names, 0);
			base.oid = oid;
			base.description = description;
			base.obsolete = obsolete;
			this.auxiliary = auxiliary;
			this.required = required;
			this.optional = optional;
			this.precluded = precluded;
			base.Value = formatString();
			return ;
		}
		
		/// <summary> Constructs a DIT content rule from the raw string value returned from a
		/// schema query for dITContentRules.
		/// 
		/// </summary>
		/// <param name="raw">        The raw string value returned from a schema query
		/// for content rules.
		/// </param>
		public LdapDITContentRuleSchema(System.String raw):base(LdapSchema.schemaTypeNames[LdapSchema.DITCONTENT])
		{
			base.obsolete = false;
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
				if (parser.Auxiliary != null)
				{
					auxiliary = new System.String[parser.Auxiliary.Length];
					parser.Auxiliary.CopyTo(auxiliary, 0);
				}
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
				if (parser.Precluded != null)
				{
					precluded = new System.String[parser.Precluded.Length];
					parser.Precluded.CopyTo(precluded, 0);
				}
				base.obsolete = parser.Obsolete;
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
			if ((strArray = AuxiliaryClasses) != null)
			{
				valueBuffer.Append(" AUX ");
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
			if ((strArray = PrecludedAttributes) != null)
			{
				valueBuffer.Append(" NOT ");
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
