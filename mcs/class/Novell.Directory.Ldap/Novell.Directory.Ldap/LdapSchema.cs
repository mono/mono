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
// Novell.Directory.Ldap.LdapSchema.cs
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
	
	/// <summary> Represents a schema entry that controls one or more entries held by a
	/// Directory Server.
	/// 
	/// <code>LdapSchema</code> Contains methods to parse schema attributes into
	/// individual schema definitions, represented by subclasses of
	/// {@link LdapSchemaElement}.  Schema may be retrieved from a Directory server
	/// with the fetchSchema method of LdapConnection or by creating an LdapEntry
	/// containing schema attributes.  The following sample code demonstrates how to
	/// retrieve schema elements from LdapSchema
	/// 
	/// <pre><code>
	/// .
	/// .
	/// .
	/// LdapSchema schema;
	/// LdapSchemaElement element;
	/// 
	/// // connect to the server
	/// lc.connect( ldapHost, ldapPort );
	/// lc.bind( ldapVersion, loginDN, password );
	/// 
	/// // read the schema from the directory
	/// schema = lc.fetchSchema( lc.getSchemaDN() );
	/// 
	/// // retrieve the definition of common name
	/// element = schema.getAttributeSchema( "cn" );
	/// System.out.println("The attribute cn has an oid of " + element.getID());
	/// .
	/// .
	/// .
	/// </code></pre>
	/// 
	/// 
	/// </summary>
	/// <seealso cref="LdapSchemaElement">
	/// </seealso>
	/// <seealso cref="LdapConnection.FetchSchema">
	/// </seealso>
	/// <seealso cref="LdapConnection.GetSchemaDN">
	/// </seealso>
	public class LdapSchema:LdapEntry
	{
		private void  InitBlock()
		{
			nameTable = new System.Collections.Hashtable[8];
			idTable = new System.Collections.Hashtable[8];
		}
		/// <summary> Returns an enumeration of attribute definitions.
		/// 
		/// </summary>
		/// <returns> An enumeration of attribute definitions.
		/// </returns>
		virtual public System.Collections.IEnumerator AttributeSchemas
		{
			get
			{
				return new EnumeratedIterator(idTable[ATTRIBUTE].Values.GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of DIT content rule definitions.
		/// 
		/// </summary>
		/// <returns> An enumeration of DIT content rule definitions.
		/// </returns>
		virtual public System.Collections.IEnumerator DITContentRuleSchemas
		{
			get
			{
				return new EnumeratedIterator(idTable[DITCONTENT].Values.GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of DIT structure rule definitions.
		/// 
		/// </summary>
		/// <returns> An enumeration of DIT structure rule definitions.
		/// </returns>
		virtual public System.Collections.IEnumerator DITStructureRuleSchemas
		{
			get
			{
				return new EnumeratedIterator(idTable[DITSTRUCTURE].Values.GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of matching rule definitions.
		/// 
		/// </summary>
		/// <returns> An enumeration of matching rule definitions.
		/// </returns>
		virtual public System.Collections.IEnumerator MatchingRuleSchemas
		{
			get
			{
				return new EnumeratedIterator(idTable[MATCHING].Values.GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of matching rule use definitions.
		/// 
		/// </summary>
		/// <returns> An enumeration of matching rule use definitions.
		/// </returns>
		virtual public System.Collections.IEnumerator MatchingRuleUseSchemas
		{
			get
			{
				return new EnumeratedIterator(idTable[MATCHING_USE].Values.GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of name form definitions.
		/// 
		/// </summary>
		/// <returns> An enumeration of name form definitions.
		/// </returns>
		virtual public System.Collections.IEnumerator NameFormSchemas
		{
			get
			{
				return new EnumeratedIterator(idTable[NAME_FORM].Values.GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of object class definitions.
		/// 
		/// </summary>
		/// <returns> An enumeration of object class definitions.
		/// </returns>
		virtual public System.Collections.IEnumerator ObjectClassSchemas
		{
			get
			{
				return new EnumeratedIterator(idTable[OBJECT_CLASS].Values.GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of syntax definitions.
		/// 
		/// </summary>
		/// <returns> An enumeration of syntax definitions.
		/// </returns>
		virtual public System.Collections.IEnumerator SyntaxSchemas
		{
			get
			{
				return new EnumeratedIterator(idTable[SYNTAX].Values.GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of attribute names.
		/// 
		/// </summary>
		/// <returns> An enumeration of attribute names.
		/// </returns>
		virtual public System.Collections.IEnumerator AttributeNames
		{
			get
			{
				return new EnumeratedIterator(new SupportClass.SetSupport(nameTable[ATTRIBUTE].Keys).GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of DIT content rule names.
		/// 
		/// </summary>
		/// <returns> An enumeration of DIT content rule names.
		/// </returns>
		virtual public System.Collections.IEnumerator DITContentRuleNames
		{
			get
			{
				return new EnumeratedIterator(new SupportClass.SetSupport(nameTable[DITCONTENT].Keys).GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of DIT structure rule names.
		/// 
		/// </summary>
		/// <returns> An enumeration of DIT structure rule names.
		/// </returns>
		virtual public System.Collections.IEnumerator DITStructureRuleNames
		{
			get
			{
				return new EnumeratedIterator(new SupportClass.SetSupport(nameTable[DITSTRUCTURE].Keys).GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of matching rule names.
		/// 
		/// </summary>
		/// <returns> An enumeration of matching rule names.
		/// </returns>
		virtual public System.Collections.IEnumerator MatchingRuleNames
		{
			get
			{
				return new EnumeratedIterator(new SupportClass.SetSupport(nameTable[MATCHING].Keys).GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of matching rule use names.
		/// 
		/// </summary>
		/// <returns> An enumeration of matching rule use names.
		/// </returns>
		virtual public System.Collections.IEnumerator MatchingRuleUseNames
		{
			get
			{
				return new EnumeratedIterator(new SupportClass.SetSupport(nameTable[MATCHING_USE].Keys).GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of name form names.
		/// 
		/// </summary>
		/// <returns> An enumeration of name form names.
		/// </returns>
		virtual public System.Collections.IEnumerator NameFormNames
		{
			get
			{
				return new EnumeratedIterator(new SupportClass.SetSupport(nameTable[NAME_FORM].Keys).GetEnumerator());
			}
			
		}
		/// <summary> Returns an enumeration of object class names.
		/// 
		/// </summary>
		/// <returns> An enumeration of object class names.
		/// </returns>
		virtual public System.Collections.IEnumerator ObjectClassNames
		{
			get
			{
				return new EnumeratedIterator(new SupportClass.SetSupport(nameTable[OBJECT_CLASS].Keys).GetEnumerator());
			}
			
		}
		
		/// <summary>The idTable hash on the oid (or integer ID for DITStructureRule) and
		/// is used for retrieving enumerations
		/// </summary>
		private System.Collections.Hashtable[] idTable;
		
		/// <summary>The nameTable will hash on the names (if available). To insure
		/// case-insensibility, the Keys for this table will be a String cast to
		/// Uppercase.
		/// </summary>
		private System.Collections.Hashtable[] nameTable;
		
		/*package*/ /// <summary> The following lists the Ldap names of subschema attributes for
		/// schema elements (definitions):
		/// </summary>
		internal static readonly System.String[] schemaTypeNames = new System.String[]{"attributeTypes", "objectClasses", "ldapSyntaxes", "nameForms", "dITContentRules", "dITStructureRules", "matchingRules", "matchingRuleUse"};
		
		/// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
		/*package*/
		internal const int ATTRIBUTE = 0;
		/// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
		/*package*/
		internal const int OBJECT_CLASS = 1;
		/// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
		/*package*/
		internal const int SYNTAX = 2;
		/// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
		/*package*/
		internal const int NAME_FORM = 3;
		/// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
		/*package*/
		internal const int DITCONTENT = 4;
		/// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
		/*package*/
		internal const int DITSTRUCTURE = 5;
		/// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
		/*package*/
		internal const int MATCHING = 6;
		/// <summary>An index into the the arrays schemaTypeNames, idTable, and nameTable </summary>
		/*package*/
		internal const int MATCHING_USE = 7;
		
		
		/// <summary> Constructs an LdapSchema object from attributes of an LdapEntry.
		/// The object is empty if the entry parameter contains no schema
		/// attributes.  The recognized schema attributes are the following: 
		/// <pre><code>
		/// "attributeTypes", "objectClasses", "ldapSyntaxes",
		/// "nameForms", "dITContentRules", "dITStructureRules",
		/// "matchingRules","matchingRuleUse"
		/// </code></pre>
		/// </summary>
		/// <param name="ent">         An LdapEntry containing schema information.
		/// </param>
		public LdapSchema(LdapEntry ent):base(ent.DN, ent.getAttributeSet())
		{
			InitBlock();
			//reset all definitions
			for (int i = 0; i < schemaTypeNames.Length; i++)
			{
				idTable[i] = new System.Collections.Hashtable();
				nameTable[i] = new System.Collections.Hashtable();
			}
			System.Collections.IEnumerator itr = base.getAttributeSet().GetEnumerator();
			while (itr.MoveNext())
			{
				
				LdapAttribute attr = (LdapAttribute) itr.Current;
				System.String value_Renamed, attrName = attr.Name;
				System.Collections.IEnumerator enumString = attr.StringValues;
				
				if (attrName.ToUpper().Equals(schemaTypeNames[OBJECT_CLASS].ToUpper()))
				{
					LdapObjectClassSchema classSchema;
					while (enumString.MoveNext())
					{
						value_Renamed = ((System.String) enumString.Current);
						try
						{
							classSchema = new LdapObjectClassSchema(value_Renamed);
						}
						catch (System.Exception e)
						{
							continue; //Error parsing: do not add this definition
						}
						addElement(OBJECT_CLASS, classSchema);
					}
				}
				else if (attrName.ToUpper().Equals(schemaTypeNames[ATTRIBUTE].ToUpper()))
				{
					LdapAttributeSchema attrSchema;
					while (enumString.MoveNext())
					{
						value_Renamed = ((System.String) enumString.Current);
						try
						{
							attrSchema = new LdapAttributeSchema(value_Renamed);
						}
						catch (System.Exception e)
						{
							continue; //Error parsing: do not add this definition
						}
						addElement(ATTRIBUTE, attrSchema);
					}
				}
				else if (attrName.ToUpper().Equals(schemaTypeNames[SYNTAX].ToUpper()))
				{
					LdapSyntaxSchema syntaxSchema;
					while (enumString.MoveNext())
					{
						value_Renamed = ((System.String) enumString.Current);
						syntaxSchema = new LdapSyntaxSchema(value_Renamed);
						addElement(SYNTAX, syntaxSchema);
					}
				}
				else if (attrName.ToUpper().Equals(schemaTypeNames[MATCHING].ToUpper()))
				{
					LdapMatchingRuleSchema matchingRuleSchema;
					while (enumString.MoveNext())
					{
						value_Renamed = ((System.String) enumString.Current);
						matchingRuleSchema = new LdapMatchingRuleSchema(value_Renamed, null);
						addElement(MATCHING, matchingRuleSchema);
					}
				}
				else if (attrName.ToUpper().Equals(schemaTypeNames[MATCHING_USE].ToUpper()))
				{
					LdapMatchingRuleUseSchema matchingRuleUseSchema;
					while (enumString.MoveNext())
					{
						value_Renamed = ((System.String) enumString.Current);
						matchingRuleUseSchema = new LdapMatchingRuleUseSchema(value_Renamed);
						addElement(MATCHING_USE, matchingRuleUseSchema);
					}
				}
				else if (attrName.ToUpper().Equals(schemaTypeNames[DITCONTENT].ToUpper()))
				{
					LdapDITContentRuleSchema dITContentRuleSchema;
					while (enumString.MoveNext())
					{
						value_Renamed = ((System.String) enumString.Current);
						dITContentRuleSchema = new LdapDITContentRuleSchema(value_Renamed);
						addElement(DITCONTENT, dITContentRuleSchema);
					}
				}
				else if (attrName.ToUpper().Equals(schemaTypeNames[DITSTRUCTURE].ToUpper()))
				{
					LdapDITStructureRuleSchema dITStructureRuleSchema;
					while (enumString.MoveNext())
					{
						value_Renamed = ((System.String) enumString.Current);
						dITStructureRuleSchema = new LdapDITStructureRuleSchema(value_Renamed);
						addElement(DITSTRUCTURE, dITStructureRuleSchema);
					}
				}
				else if (attrName.ToUpper().Equals(schemaTypeNames[NAME_FORM].ToUpper()))
				{
					LdapNameFormSchema nameFormSchema;
					while (enumString.MoveNext())
					{
						value_Renamed = ((System.String) enumString.Current);
						nameFormSchema = new LdapNameFormSchema(value_Renamed);
						addElement(NAME_FORM, nameFormSchema);
					}
				}
				//All non schema attributes are ignored.
				continue;
			}
		}
		
		/// <summary> Adds the schema definition to the idList and nameList HashMaps.
		/// This method is used by the methods fetchSchema and add.
		/// 
		/// Note that the nameTable has all keys cast to Upper-case.  This is so we
		/// can have a case-insensitive HashMap.  The getXXX (String key) methods
		/// will also cast to uppercase.
		/// 
		/// </summary>
		/// <param name="schemaType">   Type of schema definition, use one of the final
		/// integers defined at the top of this class:
		/// ATTRIBUTE, OBJECT_CLASS, SYNTAX, NAME_FORM,
		/// DITCONTENT, DITSTRUCTURE, MATCHING, MATCHING_USE
		/// 
		/// </param>
		/// <param name="element">      Schema element definition.
		/// </param>
		private void  addElement(int schemaType, LdapSchemaElement element)
		{
			SupportClass.PutElement(idTable[schemaType], element.ID, element);
			System.String[] names = element.Names;
			for (int i = 0; i < names.Length; i++)
			{
				SupportClass.PutElement(nameTable[schemaType], names[i].ToUpper(), element);
			}
			return ;
		}
		
		// #######################################################################
		//   The following methods retrieve a SchemaElement given a Key name:
		// #######################################################################
		
		/// <summary> This function abstracts retrieving LdapSchemaElements from the local
		/// copy of schema in this LdapSchema class.  This is used by
		/// <code>getXXX(String name)</code> functions.
		/// 
		/// Note that the nameTable has all keys cast to Upper-case.  This is so
		/// we can have a case-insensitive HashMap.  The getXXX (String key)
		/// methods will also cast to uppercase.
		/// 
		/// The first character of a NAME string can only be an alpha character
		/// (see section 4.1 of rfc2252) Thus if the first character is a digit we
		/// can conclude it is an OID.  Note that this digit is ASCII only.
		/// 
		/// </summary>
		/// <param name="schemaType">Specifies which list is to be used in schema
		/// lookup.
		/// </param>
		/// <param name="key">       The key can be either an OID or a name string.
		/// </param>
		private LdapSchemaElement getSchemaElement(int schemaType, System.String key)
		{
			if ((System.Object) key == null || key.ToUpper().Equals("".ToUpper()))
				return null;
			char c = key[0];
			if (c >= '0' && c <= '9')
			{
				//oid lookup
				return (LdapSchemaElement) idTable[schemaType][key];
			}
			else
			{
				//name lookup
				return (LdapSchemaElement) nameTable[schemaType][key.ToUpper()];
			}
		}
		
		/// <summary> Returns a particular attribute definition, or null if not found.
		/// 
		/// </summary>
		/// <param name="name">    Name or OID of the attribute for which a definition is
		/// to be returned.
		/// 
		/// </param>
		/// <returns> The attribute definition, or null if not found.
		/// </returns>
		public virtual LdapAttributeSchema getAttributeSchema(System.String name)
		{
			return (LdapAttributeSchema) getSchemaElement(ATTRIBUTE, name);
		}
		
		/// <summary> Returns a particular DIT content rule definition, or null if not found.
		/// 
		/// </summary>
		/// <param name="name">    The name of the DIT content rule use for which a
		/// definition is to be returned.
		/// 
		/// </param>
		/// <returns> The DIT content rule definition, or null if not found.
		/// </returns>
		public virtual LdapDITContentRuleSchema getDITContentRuleSchema(System.String name)
		{
			return (LdapDITContentRuleSchema) getSchemaElement(DITCONTENT, name);
		}
		
		/// <summary> Returns a particular DIT structure rule definition, or null if not found.
		/// 
		/// </summary>
		/// <param name="name">    The name of the DIT structure rule use for which a
		/// definition is to be returned.
		/// 
		/// </param>
		/// <returns> The DIT structure rule definition, or null if not found.
		/// </returns>
		public virtual LdapDITStructureRuleSchema getDITStructureRuleSchema(System.String name)
		{
			return (LdapDITStructureRuleSchema) getSchemaElement(DITSTRUCTURE, name);
		}
		
		/// <summary> Returns a particular DIT structure rule definition, or null if not found.
		/// 
		/// </summary>
		/// <param name="ID">    The ID of the DIT structure rule use for which a
		/// definition is to be returned.
		/// 
		/// </param>
		/// <returns> The DIT structure rule definition, or null if not found.
		/// </returns>
		public virtual LdapDITStructureRuleSchema getDITStructureRuleSchema(int ID)
		{
			System.Int32 IDKey = ID;
			return (LdapDITStructureRuleSchema) idTable[DITSTRUCTURE][IDKey];
		}
		
		/// <summary> Returns a particular matching rule definition, or null if not found.
		/// 
		/// </summary>
		/// <param name="name">    The name of the matching rule for which a definition
		/// is to be returned.
		/// 
		/// </param>
		/// <returns> The matching rule definition, or null if not found.
		/// </returns>
		public virtual LdapMatchingRuleSchema getMatchingRuleSchema(System.String name)
		{
			return (LdapMatchingRuleSchema) getSchemaElement(MATCHING, name);
		}
		
		/// <summary> Returns a particular matching rule use definition, or null if not found.
		/// 
		/// </summary>
		/// <param name="name">    The name of the matching rule use for which a definition
		/// is to be returned.
		/// 
		/// </param>
		/// <returns> The matching rule use definition, or null if not found.
		/// </returns>
		public virtual LdapMatchingRuleUseSchema getMatchingRuleUseSchema(System.String name)
		{
			return (LdapMatchingRuleUseSchema) getSchemaElement(MATCHING_USE, name);
		}
		
		/// <summary> Returns a particular name form definition, or null if not found.
		/// 
		/// </summary>
		/// <param name="name">    The name of the name form for which a definition
		/// is to be returned.
		/// 
		/// </param>
		/// <returns> The name form definition, or null if not found.
		/// </returns>
		public virtual LdapNameFormSchema getNameFormSchema(System.String name)
		{
			return (LdapNameFormSchema) getSchemaElement(NAME_FORM, name);
		}
		
		/// <summary> Returns a particular object class definition, or null if not found.
		/// 
		/// </summary>
		/// <param name="name">   The name or OID of the object class for which a
		/// definition is to be returned.
		/// 
		/// </param>
		/// <returns> The object class definition, or null if not found.
		/// </returns>
		public virtual LdapObjectClassSchema getObjectClassSchema(System.String name)
		{
			return (LdapObjectClassSchema) getSchemaElement(OBJECT_CLASS, name);
		}
		
		/// <summary> Returns a particular syntax definition, or null if not found.
		/// 
		/// </summary>
		/// <param name="oid">    The oid of the syntax for which a definition
		/// is to be returned.
		/// 
		/// </param>
		/// <returns> The syntax definition, or null if not found.
		/// </returns>
		public virtual LdapSyntaxSchema getSyntaxSchema(System.String oid)
		{
			return (LdapSyntaxSchema) getSchemaElement(SYNTAX, oid);
		}
		
		// ########################################################################
		// The following methods return an Enumeration of SchemaElements by schema type
		// ########################################################################
		
		// #######################################################################
		//  The following methods retrieve an Enumeration of Names of a schema type
		// #######################################################################
		
		
		/// <summary> This helper function returns a number that represents the type of schema
		/// definition the element represents.  The top of this file enumerates
		/// these types.
		/// 
		/// </summary>
		/// <param name="element">  A class extending LdapSchemaElement.
		/// 
		/// </param>
		/// <returns>      a Number that identifies the type of schema element and
		/// will be one of the following:
		/// ATTRIBUTE, OBJECT_CLASS, SYNTAX, NAME_FORM,
		/// DITCONTENT, DITSTRUCTURE, MATCHING, MATCHING_USE
		/// </returns>
		private int getType(LdapSchemaElement element)
		{
			if (element is LdapAttributeSchema)
				return LdapSchema.ATTRIBUTE;
			else if (element is LdapObjectClassSchema)
				return LdapSchema.OBJECT_CLASS;
			else if (element is LdapSyntaxSchema)
				return LdapSchema.SYNTAX;
			else if (element is LdapNameFormSchema)
				return LdapSchema.NAME_FORM;
			else if (element is LdapMatchingRuleSchema)
				return LdapSchema.MATCHING;
			else if (element is LdapMatchingRuleUseSchema)
				return LdapSchema.MATCHING_USE;
			else if (element is LdapDITContentRuleSchema)
				return LdapSchema.DITCONTENT;
			else if (element is LdapDITStructureRuleSchema)
				return LdapSchema.DITSTRUCTURE;
			else
				throw new System.ArgumentException("The specified schema element type is not recognized");
		}
	}
}
