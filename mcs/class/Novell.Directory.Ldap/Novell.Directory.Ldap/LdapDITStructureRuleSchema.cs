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
// Novell.Directory.Ldap.LdapDITStructureRuleSchema.cs
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
	
	/// <summary> Represents the definition of a specific DIT (Directory Information Tree)
	/// structure rule in the directory schema.
	/// 
	/// The LdapDITStructureRuleSchema class represents the definition of a DIT
	/// Structure Rule.  It is used to discover or modify which
	/// object classes a particular object class may be subordinate to in the DIT.
	/// 
	/// </summary>
	
	public class LdapDITStructureRuleSchema:LdapSchemaElement
	{
		/// <summary> Returns the rule ID for this structure rule.
		/// 
		/// The getRuleID method returns an integer rather than a dotted
		/// decimal OID. Objects of this class do not have an OID,
		/// thus getID can return null. 
		/// 
		/// 
		/// </summary>
		/// <returns> The rule ID for this structure rule.
		/// </returns>
		virtual public int RuleID
		{
			
			
			get
			{
				return ruleID;
			}
			
		}
		/// <summary> Returns the NameForm that this structure rule controls.
		/// 
		/// You can get the actual object class that this structure rule controls
		/// by calling the getNameForm.getObjectClass method.
		/// 
		/// </summary>
		/// <returns> The NameForm that this structure rule controls.
		/// </returns>
		virtual public System.String NameForm
		{
			get
			{
				return nameForm;
			}
			
		}
		/// <summary> Returns a list of all structure rules that are superior to this
		/// structure rule.
		/// 
		/// To resolve to an object class, you need to first
		/// resolve the superior ID to another structure rule, then call
		/// the getNameForm.getObjectClass method on that structure rule.
		/// 
		/// </summary>
		/// <returns> A list of all structure rules that are superior to this structure rule.
		/// </returns>
		virtual public System.String[] Superiors
		{
			get
			{
				return superiorIDs;
			}
			
		}
		private int ruleID = 0;
		private System.String nameForm = "";
		private System.String[] superiorIDs = new System.String[]{""};
		
		/// <summary>Constructs a DIT structure rule for adding to or deleting from the
		/// schema.
		/// 
		/// </summary>
		/// <param name="names">      The names of the structure rule.
		/// 
		/// </param>
		/// <param name="ruleID">     The unique identifier of the structure rule. NOTE:
		/// this is an integer, not a dotted numerical
		/// identifier. Structure rules aren't identified
		/// by OID.
		/// 
		/// </param>
		/// <param name="description">An optional description of the structure rule.
		/// 
		/// </param>
		/// <param name="obsolete">   True if the structure rule is obsolete.
		/// 
		/// </param>
		/// <param name="nameForm">   Either the identifier or name of a name form.
		/// This is used to indirectly refer to the object
		/// class that this structure rule applies to.
		/// 
		/// </param>
		/// <param name="superiorIDs">A list of superior structure rules - specified
		/// by their integer ID. The object class
		/// specified by this structure rule (via the
		/// nameForm parameter) may only be subordinate in
		/// the DIT to object classes of those represented
		/// by the structure rules here; it may be null.
		/// 
		/// </param>
		public LdapDITStructureRuleSchema(System.String[] names, int ruleID, System.String description, bool obsolete, System.String nameForm, System.String[] superiorIDs):base(LdapSchema.schemaTypeNames[LdapSchema.DITSTRUCTURE])
		{
			base.names = new System.String[names.Length];
			names.CopyTo(base.names, 0);
			this.ruleID = ruleID;
			base.description = description;
			base.obsolete = obsolete;
			this.nameForm = nameForm;
			this.superiorIDs = superiorIDs;
			base.Value = formatString();
			return ;
		}
		
		/// <summary> Constructs a DIT structure rule from the raw string value returned from
		/// a schema query for dITStructureRules.
		/// 
		/// </summary>
		/// <param name="raw">        The raw string value returned from a schema
		/// query for dITStructureRules.
		/// </param>
		public LdapDITStructureRuleSchema(System.String raw):base(LdapSchema.schemaTypeNames[LdapSchema.DITSTRUCTURE])
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
					ruleID = System.Int32.Parse(parser.ID);
				if ((System.Object) parser.Description != null)
					base.description = parser.Description;
				if (parser.Superiors != null)
				{
					superiorIDs = new System.String[parser.Superiors.Length];
					parser.Superiors.CopyTo(superiorIDs, 0);
				}
				if ((System.Object) parser.NameForm != null)
					nameForm = parser.NameForm;
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
			
			token = RuleID.ToString();
			valueBuffer.Append(token);
			
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
			if ((System.Object) (token = NameForm) != null)
			{
				valueBuffer.Append(" FORM ");
				valueBuffer.Append("'" + token + "'");
			}
			if ((strArray = Superiors) != null)
			{
				valueBuffer.Append(" SUP ");
				if (strArray.Length > 1)
					valueBuffer.Append("( ");
				for (int i = 0; i < strArray.Length; i++)
				{
					if (i > 0)
						valueBuffer.Append(" ");
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
