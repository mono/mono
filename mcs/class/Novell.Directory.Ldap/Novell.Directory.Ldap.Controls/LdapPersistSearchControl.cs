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
// Novell.Directory.Ldap.Controls.LdapPersistSearchControl.cs
//
// Author:
//   Sunil Kumar (Sunilk@novell.com)
//
// (C) 2003 Novell, Inc (http://www.novell.com)
//

using System;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Asn1;

namespace Novell.Directory.Ldap.Controls
{
	
	
	/// <summary>  LdapPersistSearchControl is a Server Control that allows a client
	/// to receive notifications from the server of changes to entries within
	/// the searches result set. The client can be notified when an entry is
	/// added to the result set, when an entry is deleted from the result set,
	/// when a DN has been changed or when and attribute value has been changed.
	/// </summary>
	public class LdapPersistSearchControl:LdapControl
	{
		/// <summary>  Returns the change types to be monitored as a logical OR of any or
		/// all of these values: ADD, DELETE, MODIFY, and/or MODDN.
		/// 
		/// </summary>
		/// <returns>  the change types to be monitored. The logical or of any of
		/// the following values: ADD, DELETE, MODIFY, and/or MODDN.
		/// </returns>
		/// <summary>  Sets the change types to be monitored.
		/// 
		/// types  The change types to be monitored as a logical OR of any or all
		/// of these types: ADD, DELETE, MODIFY, and/or MODDN. Can also be set
		/// to the value ANY which is defined as the logical OR of all of the
		/// preceding values.
		/// </summary>
		virtual public int ChangeTypes
		{
			get
			{
				return m_changeTypes;
			}
			
			set
			{
				m_changeTypes = value;
				m_sequence.set_Renamed(CHANGETYPES_INDEX, new Asn1Integer(m_changeTypes));
				setValue();
				return ;
			}
			
		}
		/// <summary>  Returns true if entry change controls are to be returned with the
		/// search results.
		/// 
		/// </summary>
		/// <returns>  true if entry change controls are to be returned with the
		/// search results. Otherwise, false is returned
		/// </returns>
		/// <summary>  When set to true, requests that entry change controls be returned with
		/// the search results.
		/// 
		/// </summary>
		/// <param name="returnControls">  true to return entry change controls.
		/// </param>
		virtual public bool ReturnControls
		{
			get
			{
				return m_returnControls;
			}
			
			set
			{
				m_returnControls = value;
				m_sequence.set_Renamed(RETURNCONTROLS_INDEX, new Asn1Boolean(m_returnControls));
				setValue();
				return ;
			}
			
		}
		/// <summary>  getChangesOnly returns true if only changes are to be returned.
		/// Results from the initial search are not returned.
		/// 
		/// </summary>
		/// <returns>  true of only changes are to be returned
		/// </returns>
		/// <summary>  When set to true, requests that only changes be returned, results from
		/// the initial search are not returned.
		/// </summary>
		/// <param name="changesOnly"> true to skip results for the initial search
		/// </param>
		virtual public bool ChangesOnly
		{
			get
			{
				return m_changesOnly;
			}
			
			set
			{
				m_changesOnly = value;
				m_sequence.set_Renamed(CHANGESONLY_INDEX, new Asn1Boolean(m_changesOnly));
				setValue();
				return ;
			}
			
		}
		/* private data members */
		private static int SEQUENCE_SIZE = 3;
		
		private static int CHANGETYPES_INDEX = 0;
		private static int CHANGESONLY_INDEX = 1;
		private static int RETURNCONTROLS_INDEX = 2;
		
		private static LBEREncoder s_encoder;
		
		private int m_changeTypes;
		private bool m_changesOnly;
		private bool m_returnControls;
		private Asn1Sequence m_sequence;
		
		/// <summary> The requestOID of the persistent search control</summary>
		private static System.String requestOID = "2.16.840.1.113730.3.4.3";
		
		/// <summary> The responseOID of the psersistent search - entry change control</summary>
		private static System.String responseOID = "2.16.840.1.113730.3.4.7";
		
		/// <summary>  Change type specifying that you want to track additions of new entries
		/// to the directory.
		/// </summary>
		public const int ADD = 1;
		
		/// <summary>  Change type specifying that you want to track removals of entries from
		/// the directory.
		/// </summary>
		public const int DELETE = 2;
		
		/// <summary>  Change type specifying that you want to track modifications of entries
		/// in the directory.
		/// </summary>
		public const int MODIFY = 4;
		
		/// <summary>  Change type specifying that you want to track modifications of the DNs
		/// of entries in the directory.
		/// </summary>
		public const int MODDN = 8;
		
		/// <summary>  Change type specifying that you want to track any of the above
		/// modifications.
		/// </summary>
		public static readonly int ANY = ADD | DELETE | MODIFY | MODDN;
		
		/* public constructors */
		
		/// <summary>  The default constructor. A control with changes equal to ANY,
		/// isCritical equal to true, changesOnly equal to true, and
		/// returnControls equal to true
		/// </summary>
		public LdapPersistSearchControl():this(ANY, true, true, true)
		{
			return ;
		}
		
		/// <summary>  Constructs an LdapPersistSearchControl object according to the
		/// supplied parameters. The resulting control is used to specify a
		/// persistent search.
		/// 
		/// </summary>
		/// <param name="changeTypes"> the change types to monitor. The bitwise OR of any
		/// of the following values:
		/// <li>                           LdapPersistSearchControl.ADD</li>
		/// <li>                           LdapPersistSearchControl.DELETE</li>
		/// <li>                           LdapPersistSearchControl.MODIFY</li>
		/// <li>                           LdapPersistSearchControl.MODDN</li>
		/// To track all changes the value can be set to:
		/// <li>                           LdapPersistSearchControl.ANY</li>
		/// 
		/// </param>
		/// <param name="changesOnly"> true if you do not want the server to return
		/// all existing entries in the directory that match the search
		/// criteria. (Use this if you just want the changed entries to be
		/// returned.)
		/// 
		/// </param>
		/// <param name="returnControls"> true if you want the server to return entry
		/// change controls with each entry in the search results. You need to
		/// return entry change controls to discover what type of change
		/// and other additional information about the change.
		/// 
		/// </param>
		/// <param name="isCritical"> true if this control is critical to the search
		/// operation. If true and the server does not support this control,
		/// the server will not perform the search at all.
		/// </param>
		public LdapPersistSearchControl(int changeTypes, bool changesOnly, bool returnControls, bool isCritical):base(requestOID, isCritical, null)
		{
			
			m_changeTypes = changeTypes;
			m_changesOnly = changesOnly;
			m_returnControls = returnControls;
			
			m_sequence = new Asn1Sequence(SEQUENCE_SIZE);
			
			m_sequence.add(new Asn1Integer(m_changeTypes));
			m_sequence.add(new Asn1Boolean(m_changesOnly));
			m_sequence.add(new Asn1Boolean(m_returnControls));
			
			setValue();
			return ;
		}
		
		public override System.String ToString()
		{
			sbyte[] data = m_sequence.getEncoding(s_encoder);
			
			System.Text.StringBuilder buf = new System.Text.StringBuilder(data.Length);
			
			for (int i = 0; i < data.Length; i++)
			{
				buf.Append(data[i].ToString());
				if (i < data.Length - 1)
					buf.Append(",");
			}
			
			return buf.ToString();
		}
		
		/// <summary>  Sets the encoded value of the LdapControlClass</summary>
		private void  setValue()
		{
			base.setValue(m_sequence.getEncoding(s_encoder));
			return ;
		}
		static LdapPersistSearchControl()
		{
			s_encoder = new LBEREncoder();
			/*
			* This is where we register the control response
			*/
			{
				/* Register the Entry Change control class which is returned by the
				* server in response to a persistent search request
				*/
				try
				{
					// Register LdapEntryChangeControl
					LdapControl.register(responseOID, System.Type.GetType("Novell.Directory.Ldap.Controls.LdapEntryChangeControl"));
				}
				catch (System.Exception e)
				{
				}
			}
		}
	} // end class LdapPersistentSearchControl
}
