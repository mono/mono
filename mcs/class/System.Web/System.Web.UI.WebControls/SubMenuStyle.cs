//
// System.Web.UI.WebControls.SubMenuStyle.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//

#if NET_2_0

using System;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public class SubMenuStyle: Style, ICustomTypeDescriptor
	{
		const string HORZ_PADD = "HorizontalPadding";
		const string VERT_PADD = "VerticalPadding";
		
		public SubMenuStyle ()
		{
		}
		
		public SubMenuStyle (StateBag bag): base (bag)
		{
		}
		
		bool IsSet (string v)
		{
			return ViewState [v] != null;
		}
		
		[DefaultValue (typeof(Unit), "")]
		[NotifyParentProperty (true)]
		public Unit HorizontalPadding {
			get {
				if(IsSet(HORZ_PADD))
					return (Unit)(ViewState[HORZ_PADD]);
				return Unit.Empty;
			}
			set {
				ViewState[HORZ_PADD] = value;
			}
		}

		[DefaultValue (typeof(Unit), "")]
		[NotifyParentProperty (true)]
		public Unit VerticalPadding {
			get {
				if(IsSet(VERT_PADD))
					return (Unit)(ViewState[VERT_PADD]);
				return Unit.Empty;
			}
			set {
				ViewState[VERT_PADD] = value;
			}
		}

		public override void CopyFrom (Style s)
		{
			if (s == null || s.IsEmpty)
				return;

			base.CopyFrom (s);
			SubMenuStyle from = s as SubMenuStyle;
			if (from == null)
				return;

			if (from.IsSet (HORZ_PADD))
				HorizontalPadding = from.HorizontalPadding;

			if (from.IsSet (VERT_PADD))
				VerticalPadding = from.VerticalPadding;
		}
		
		public override void MergeWith(Style s)
		{
			if(s != null && !s.IsEmpty)
			{
				if (IsEmpty) {
					CopyFrom (s);
					return;
				}
				base.MergeWith(s);

				SubMenuStyle with = s as SubMenuStyle;
				if (with == null) return;
				
				if (with.IsSet(HORZ_PADD) && !IsSet(HORZ_PADD)) {
					HorizontalPadding = with.HorizontalPadding;
				}
				if (with.IsSet(VERT_PADD) && !IsSet(VERT_PADD)) {
					VerticalPadding = with.VerticalPadding;
				}
			}
		}

		public override void Reset()
		{
			if(IsSet(HORZ_PADD))
				ViewState.Remove("HorizontalPadding");
			if(IsSet(VERT_PADD))
				ViewState.Remove("VerticalPadding");
			base.Reset();
		}
		
		protected override void FillStyleAttributes (CssStyleCollection attributes, IUrlResolutionService urlResolver)
		{
			base.FillStyleAttributes (attributes, urlResolver);
			if (IsSet (HORZ_PADD)) {
				attributes.Add (HtmlTextWriterStyle.PaddingLeft, HorizontalPadding.ToString ());
				attributes.Add (HtmlTextWriterStyle.PaddingRight, HorizontalPadding.ToString ());
			}
			if (IsSet (VERT_PADD)) {
				attributes.Add (HtmlTextWriterStyle.PaddingTop, VerticalPadding.ToString ());
				attributes.Add (HtmlTextWriterStyle.PaddingBottom, VerticalPadding.ToString ());
			}
		}

		[MonoTODO ("Not implemented")]
		System.ComponentModel.AttributeCollection ICustomTypeDescriptor.GetAttributes ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		string ICustomTypeDescriptor.GetClassName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		string ICustomTypeDescriptor.GetComponentName ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		TypeConverter ICustomTypeDescriptor.GetConverter ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		object ICustomTypeDescriptor.GetEditor (Type editorBaseType)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		EventDescriptorCollection ICustomTypeDescriptor.GetEvents (Attribute [] arr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties (Attribute [] arr)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		object ICustomTypeDescriptor.GetPropertyOwner (PropertyDescriptor pd)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
