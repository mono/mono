//
// Authors:
//   Rafael Mizrahi   <rafim@mainsoft.com>
//   Erez Lotan       <erezl@mainsoft.com>
//   Vladimir Krasnov <vladimirk@mainsoft.com>
//   
// 
// Copyright (c) 2002-2005 Mainsoft Corporation.
// 
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Web.UI;
using System.Collections;
using System.Web.UI.WebControls;

namespace GHTTests
{
	/// <summary>
	/// Summary description for GHTWebControlBase.
	/// </summary>
	public class GHTWebControlBase : GHTControlBase
	{
		#region "Properties"
		protected new WebControl TestedControl
		{
			get
			{
				return (WebControl) m_cToTest;
			}
		}
		public new Type[] TypesToTest
		{
			get
			{
				return (System.Type[])(m_derivedTypes.ToArray(typeof(System.Type)));
			}
		}
		#endregion

		#region "Methods"
		/// <summary>
		/// Initializes all the derived types that need to be tested.
		/// </summary>
		protected override  void InitTypes()
		{
			m_derivedTypes = new ArrayList();


			//System.Web.UI.WebControls basic:
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Button));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.CheckBox));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.HyperLink));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Image));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.ImageButton));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Label));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.LinkButton));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Panel));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RadioButton));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.TextBox));

			//System.Web.UI.WebControls basic list controls:
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DropDownList));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.ListBox));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RadioButtonList));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.CheckBoxList));

			//System.Web.UI.WebControls validation controls:
#if KNOWN_BUG //BUG_NUM:935
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.CompareValidator));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.CustomValidator));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RangeValidator));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RegularExpressionValidator));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.RequiredFieldValidator));
#endif
#if KNOWN_BUG //BUG_NUM:1195
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.ValidationSummary));
#endif
			//System.Web.UI.WebControls rich controls (currently not supported):
			//			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.AdRotator));
			//			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Calendar));

			//System.Web.UI.WebControls advanced list controls:
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DataGrid));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DataGridItem));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DataList));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.DataListItem));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.Table));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.TableCell));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.TableHeaderCell));
			m_derivedTypes.Add(typeof(System.Web.UI.WebControls.TableRow));
			//m_derivedTypes.Add(typeof( System.Web.UI.WebControls.Xml));
		}

		#endregion
	}
}
