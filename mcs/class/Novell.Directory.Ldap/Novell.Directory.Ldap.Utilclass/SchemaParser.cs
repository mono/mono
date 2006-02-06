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
using LdapObjectClassSchema = Novell.Directory.Ldap.LdapObjectClassSchema;
using LdapAttributeSchema = Novell.Directory.Ldap.LdapAttributeSchema;

namespace Novell.Directory.Ldap.Utilclass
{
	
	public class SchemaParser
	{
		private void  InitBlock()
		{
			usage = LdapAttributeSchema.USER_APPLICATIONS;
			qualifiers = new System.Collections.ArrayList();
		}
		virtual public System.String RawString
		{
			get
			{
				return rawString;
			}
			
			set
			{
				this.rawString = value;
			}
			
		}
		virtual public System.String[] Names
		{
			get
			{
				return names;
			}
			
		}
		virtual public System.Collections.IEnumerator Qualifiers
		{
			get
			{
				return qualifiers.GetEnumerator();
			}
			
		}
		virtual public System.String ID
		{
			get
			{
				return id;
			}
			
		}
		virtual public System.String Description
		{
			get
			{
				return description;
			}
			
		}
		virtual public System.String Syntax
		{
			get
			{
				return syntax;
			}
			
		}
		virtual public System.String Superior
		{
			get
			{
				return superior;
			}
			
		}
		virtual public bool Single
		{
			get
			{
				return single;
			}
			
		}
		virtual public bool Obsolete
		{
			get
			{
				return obsolete;
			}
			
		}
		virtual public System.String Equality
		{
			get
			{
				return equality;
			}
			
		}
		virtual public System.String Ordering
		{
			get
			{
				return ordering;
			}
			
		}
		virtual public System.String Substring
		{
			get
			{
				return substring;
			}
			
		}
		virtual public bool Collective
		{
			get
			{
				return collective;
			}
			
		}
		virtual public bool UserMod
		{
			get
			{
				return userMod;
			}
			
		}
		virtual public int Usage
		{
			get
			{
				return usage;
			}
			
		}
		virtual public int Type
		{
			get
			{
				return type;
			}
			
		}
		virtual public System.String[] Superiors
		{
			get
			{
				return superiors;
			}
			
		}
		virtual public System.String[] Required
		{
			get
			{
				return required;
			}
			
		}
		virtual public System.String[] Optional
		{
			get
			{
				return optional;
			}
			
		}
		virtual public System.String[] Auxiliary
		{
			get
			{
				return auxiliary;
			}
			
		}
		virtual public System.String[] Precluded
		{
			get
			{
				return precluded;
			}
			
		}
		virtual public System.String[] Applies
		{
			get
			{
				return applies;
			}
			
		}
		virtual public System.String NameForm
		{
			get
			{
				return nameForm;
			}
			
		}
		virtual public System.String ObjectClass
		{
			get
			{
				return nameForm;
			}
			
		}
		
		internal System.String rawString;
		internal System.String[] names = null;
		internal System.String id;
		internal System.String description;
		internal System.String syntax;
		internal System.String superior;
		internal System.String nameForm;
		internal System.String objectClass;
		internal System.String[] superiors;
		internal System.String[] required;
		internal System.String[] optional;
		internal System.String[] auxiliary;
		internal System.String[] precluded;
		internal System.String[] applies;
		internal bool single = false;
		internal bool obsolete = false;
		internal System.String equality;
		internal System.String ordering;
		internal System.String substring;
		internal bool collective = false;
		internal bool userMod = true;
		internal int usage;
		internal int type = - 1;
		internal int result;
		internal System.Collections.ArrayList qualifiers;
		
		public SchemaParser(System.String aString)
		{
			InitBlock();
			
			int index;
			
			if ((index = aString.IndexOf((System.Char) '\\')) != - 1)
			{
				/*
				* Unless we escape the slash, StreamTokenizer will interpret the
				* single slash and convert it assuming octal values.
				* Two successive back slashes are intrepreted as one backslash.
				*/
				System.Text.StringBuilder newString = new System.Text.StringBuilder(aString.Substring(0, (index) - (0)));
				for (int i = index; i < aString.Length; i++)
				{
					newString.Append(aString[i]);
					if (aString[i] == '\\')
					{
						newString.Append('\\');
					}
				}
				rawString = newString.ToString();
			}
			else
			{
				rawString = aString;
			}
			
			SchemaTokenCreator st2 = new SchemaTokenCreator(new System.IO.StringReader(rawString));
			st2.OrdinaryCharacter('.');
			st2.OrdinaryCharacters('0', '9');
			st2.OrdinaryCharacter('{');
			st2.OrdinaryCharacter('}');
			st2.OrdinaryCharacter('_');
			st2.OrdinaryCharacter(';');
			st2.WordCharacters('.', '9');
			st2.WordCharacters('{', '}');
			st2.WordCharacters('_', '_');
			st2.WordCharacters(';', ';');
			//First parse out the OID
			try
			{
				System.String currName;
				if ((int)TokenTypes.EOF != st2.nextToken())
				{
					if (st2.lastttype == '(')
					{
						if ((int)TokenTypes.WORD == st2.nextToken())
						{
							id = st2.StringValue;
						}
						while ((int)TokenTypes.EOF != st2.nextToken())
						{
							if (st2.lastttype == (int)TokenTypes.WORD)
							{
								if (st2.StringValue.ToUpper().Equals("NAME".ToUpper()))
								{
									if (st2.nextToken() == '\'')
									{
										names = new System.String[1];
										names[0] = st2.StringValue;
									}
									else
									{
										if (st2.lastttype == '(')
										{
											System.Collections.ArrayList nameList = new System.Collections.ArrayList();
											while (st2.nextToken() == '\'')
											{
												if ((System.Object) st2.StringValue != null)
												{
													nameList.Add(st2.StringValue);
												}
											}
											if (nameList.Count > 0)
											{
												names = new System.String[nameList.Count];
												SupportClass.ArrayListSupport.ToArray(nameList, names);
											}
										}
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("DESC".ToUpper()))
								{
									if (st2.nextToken() == '\'')
									{
										description = st2.StringValue;
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("SYNTAX".ToUpper()))
								{
									result = st2.nextToken();
									if ((result == (int)TokenTypes.WORD) || (result == '\''))
									//Test for non-standard schema
									{
										syntax = st2.StringValue;
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("EQUALITY".ToUpper()))
								{
									if (st2.nextToken() == (int)TokenTypes.WORD)
									{
										equality = st2.StringValue;
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("ORDERING".ToUpper()))
								{
									if (st2.nextToken() == (int)TokenTypes.WORD)
									{
										ordering = st2.StringValue;
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("SUBSTR".ToUpper()))
								{
									if (st2.nextToken() == (int)TokenTypes.WORD)
									{
										substring = st2.StringValue;
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("FORM".ToUpper()))
								{
									if (st2.nextToken() == (int)TokenTypes.WORD)
									{
										nameForm = st2.StringValue;
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("OC".ToUpper()))
								{
									if (st2.nextToken() == (int)TokenTypes.WORD)
									{
										objectClass = st2.StringValue;
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("SUP".ToUpper()))
								{
									System.Collections.ArrayList values = new System.Collections.ArrayList();
									st2.nextToken();
									if (st2.lastttype == '(')
									{
										st2.nextToken();
										while (st2.lastttype != ')')
										{
											if (st2.lastttype != '$')
											{
												values.Add(st2.StringValue);
											}
											st2.nextToken();
										}
									}
									else
									{
										values.Add(st2.StringValue);
										superior = st2.StringValue;
									}
									if (values.Count > 0)
									{
										superiors = new System.String[values.Count];
										SupportClass.ArrayListSupport.ToArray(values, superiors);
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("SINGLE-VALUE".ToUpper()))
								{
									single = true;
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("OBSOLETE".ToUpper()))
								{
									obsolete = true;
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("COLLECTIVE".ToUpper()))
								{
									collective = true;
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("NO-USER-MODIFICATION".ToUpper()))
								{
									userMod = false;
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("MUST".ToUpper()))
								{
									System.Collections.ArrayList values = new System.Collections.ArrayList();
									st2.nextToken();
									if (st2.lastttype == '(')
									{
										st2.nextToken();
										while (st2.lastttype != ')')
										{
											if (st2.lastttype != '$')
											{
												values.Add(st2.StringValue);
											}
											st2.nextToken();
										}
									}
									else
									{
										values.Add(st2.StringValue);
									}
									if (values.Count > 0)
									{
										required = new System.String[values.Count];
										SupportClass.ArrayListSupport.ToArray(values, required);
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("MAY".ToUpper()))
								{
									System.Collections.ArrayList values = new System.Collections.ArrayList();
									st2.nextToken();
									if (st2.lastttype == '(')
									{
										st2.nextToken();
										while (st2.lastttype != ')')
										{
											if (st2.lastttype != '$')
											{
												values.Add(st2.StringValue);
											}
											st2.nextToken();
										}
									}
									else
									{
										values.Add(st2.StringValue);
									}
									if (values.Count > 0)
									{
										optional = new System.String[values.Count];
										SupportClass.ArrayListSupport.ToArray(values, optional);
									}
									continue;
								}

								if (st2.StringValue.ToUpper().Equals("NOT".ToUpper()))
								{
									System.Collections.ArrayList values = new System.Collections.ArrayList();
									st2.nextToken();
									if (st2.lastttype == '(')
									{
										st2.nextToken();
										while (st2.lastttype != ')')
										{
											if (st2.lastttype != '$')
											{
												values.Add(st2.StringValue);
											}
											st2.nextToken();
										}
									}
									else
									{
										values.Add(st2.StringValue);
									}
									if (values.Count > 0)
									{
										precluded = new System.String[values.Count];
										SupportClass.ArrayListSupport.ToArray(values, precluded);
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("AUX".ToUpper()))
								{
									System.Collections.ArrayList values = new System.Collections.ArrayList();
									st2.nextToken();
									if (st2.lastttype == '(')
									{
										st2.nextToken();
										while (st2.lastttype != ')')
										{
											if (st2.lastttype != '$')
											{
												values.Add(st2.StringValue);
											}
											st2.nextToken();
										}
									}
									else
									{
										values.Add(st2.StringValue);
									}
									if (values.Count > 0)
									{
										auxiliary = new System.String[values.Count];
										SupportClass.ArrayListSupport.ToArray(values, auxiliary);
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("ABSTRACT".ToUpper()))
								{
									type = LdapObjectClassSchema.ABSTRACT;
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("STRUCTURAL".ToUpper()))
								{
									type = LdapObjectClassSchema.STRUCTURAL;
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("AUXILIARY".ToUpper()))
								{
									type = LdapObjectClassSchema.AUXILIARY;
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("USAGE".ToUpper()))
								{
									if (st2.nextToken() == (int)TokenTypes.WORD)
									{
										currName = st2.StringValue;
										if (currName.ToUpper().Equals("directoryOperation".ToUpper()))
										{
											usage = LdapAttributeSchema.DIRECTORY_OPERATION;
										}
										else if (currName.ToUpper().Equals("distributedOperation".ToUpper()))
										{
											usage = LdapAttributeSchema.DISTRIBUTED_OPERATION;
										}
										else if (currName.ToUpper().Equals("dSAOperation".ToUpper()))
										{
											usage = LdapAttributeSchema.DSA_OPERATION;
										}
										else if (currName.ToUpper().Equals("userApplications".ToUpper()))
										{
											usage = LdapAttributeSchema.USER_APPLICATIONS;
										}
									}
									continue;
								}
								if (st2.StringValue.ToUpper().Equals("APPLIES".ToUpper()))
								{
									System.Collections.ArrayList values = new System.Collections.ArrayList();
									st2.nextToken();
									if (st2.lastttype == '(')
									{
										st2.nextToken();
										while (st2.lastttype != ')')
										{
											if (st2.lastttype != '$')
											{
												values.Add(st2.StringValue);
											}
											st2.nextToken();
										}
									}
									else
									{
										values.Add(st2.StringValue);
									}
									if (values.Count > 0)
									{
										applies = new System.String[values.Count];
										SupportClass.ArrayListSupport.ToArray(values, applies);
									}
									continue;
								}
								currName = st2.StringValue;
								AttributeQualifier q = parseQualifier(st2, currName);
								if (q != null)
								{
									qualifiers.Add(q);
								}
								continue;
							}
						}
					}
				}
			}
			catch (System.IO.IOException e)
			{
				throw e;
			}
		}
		
		private AttributeQualifier parseQualifier(SchemaTokenCreator st, System.String name)
		{
			System.Collections.ArrayList values = new System.Collections.ArrayList(5);
			try
			{
				if (st.nextToken() == '\'')
				{
					values.Add(st.StringValue);
				}
				else
				{
					if (st.lastttype == '(')
					{
						while (st.nextToken() == '\'')
						{
							values.Add(st.StringValue);
						}
					}
				}
			}
			catch (System.IO.IOException e)
			{
				throw e;
			}
			System.String[] valArray = new System.String[values.Count];
			valArray = (System.String[]) SupportClass.ArrayListSupport.ToArray(values, valArray);
			return new AttributeQualifier(name, valArray);
		}
	}
}
