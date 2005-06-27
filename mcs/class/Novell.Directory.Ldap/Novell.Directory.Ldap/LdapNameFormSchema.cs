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
// Novell.Directory.Ldap.LdapAttributeSchema.cs
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
	
	/// <summary> A specific a name form in the directory schema.
	/// 
	/// The LdapNameFormSchema class represents the definition of a Name Form.  It
	/// is used to discover or modify the allowed naming attributes for a particular
	/// object class.
	/// 
	/// </summary>
	/// <seealso cref="LdapSchemaElement">
	/// </seealso>
	/// <seealso cref="LdapSchema">
	/// </seealso>
	
	public class LdapNameFormSchema:LdapSchemaElement
	{
		/// <summary> Returns the name of the object class which this name form applies to.
		/// 
		/// </summary>
		/// <returns> The name of the object class.
		/// </returns>
		virtual public System.String ObjectClass
		{
			get
			{
				return objectClass;
			}
			
		}
		/// <summary> Returns the list of required naming attributes for an entry
		/// controlled by this name form.
		/// 
		/// </summary>
		/// <returns> The list of required naming attributes.
		/// </returns>
		virtual public System.String[] RequiredNamingAttributes
		{
			get
			{
				return required;
			}
			
		}
		/// <summary> Returns the list of optional naming attributes for an entry
		/// controlled by this content rule.
		/// 
		/// </summary>
		/// <returns> The list of the optional naming attributes.
		/// </returns>
		virtual public System.String[] OptionalNamingAttributes
		{
			get
			{
				return optional;
			}
			
		}
		private System.String objectClass;
		private System.String[] required;
		private System.String[] optional;
		
		/// <summary> Constructs a name form for adding to or deleting from the schema.
		/// 
		/// </summary>
		/// <param name="names">      The name(s) of the name form.
		/// 
		/// </param>
		/// <param name="oid">        The unique object identifier of the name form - in
		/// dotted numerical format.
		/// 
		/// </param>
		/// <param name="description">An optional description of the name form.
		/// 
		/// </param>
		/// <param name="obsolete">   True if the name form is obsolete.
		/// 
		/// </param>
		/// <param name="objectClass">The object to which this name form applies.
		/// This may be specified by either name or
		/// numeric oid.
		/// 
		/// </param>
		/// <param name="required">   A list of the attributes that must be present
		/// in the RDN of an entry that this name form
		/// controls. These attributes may be specified by
		/// either name or numeric oid.
		/// 
		/// </param>
		/// <param name="optional">   A list of the attributes that may be present
		/// in the RDN of an entry that this name form
		/// controls. These attributes may be specified by
		/// either name or numeric oid.
		/// </param>
		public LdapNameFormSchema(System.String[] names, System.String oid, System.String description, bool obsolete, System.String objectClass, System.String[] required, System.String[] optional):base(LdapSchema.schemaTypeNames[LdapSchema.NAME_FORM])
		{
			base.names = new System.String[names.Length];
			names.CopyTo(base.names, 0);
			base.oid = oid;
			base.description = description;
			base.obsolete = obsolete;
			this.objectClass = objectClass;
			this.required = new System.String[required.Length];
			required.CopyTo(this.required, 0);
			this.optional = new System.String[optional.Length];
			optional.CopyTo(this.optional, 0);
			base.Value = formatString();
			return ;
		}
		
		/*
		}
		
		/**
		* Constructs a Name Form from the raw string value returned on a
		* schema query for nameForms.
		*
		* @param raw        The raw string value returned on a schema
		*                   query for nameForms.
		*/
		public LdapNameFormSchema(System.String raw):base(LdapSchema.schemaTypeNames[LdapSchema.NAME_FORM])
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
					base.oid = new System.Text.StringBuilder(parser.ID).ToString();
				if ((System.Object) parser.Description != null)
					base.description = new System.Text.StringBuilder(parser.Description).ToString();
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
				if ((System.Object) parser.ObjectClass != null)
					objectClass = parser.ObjectClass;
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
			if ((System.Object) (token = ObjectClass) != null)
			{
				valueBuffer.Append(" OC ");
				valueBuffer.Append("'" + token + "'");
			}
			if ((strArray = RequiredNamingAttributes) != null)
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
			if ((strArray = OptionalNamingAttributes) != null)
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
