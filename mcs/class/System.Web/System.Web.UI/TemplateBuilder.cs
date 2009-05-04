//
// System.Web.UI.TemplateBuilder
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Reflection;
using System.Security.Permissions;
#if NET_2_0
using System.ComponentModel;
using System.Collections.Generic;
#endif

namespace System.Web.UI {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class TemplateBuilder : ControlBuilder, ITemplate {

		string text;
		TemplateContainerAttribute containerAttribute;
#if NET_2_0
		TemplateInstanceAttribute instanceAttribute;
		List <TemplateBinding> bindings;
#endif

		public TemplateBuilder ()
		{
		}

		internal TemplateBuilder (ICustomAttributeProvider prov)
		{
			object[] ats = prov.GetCustomAttributes (typeof (TemplateContainerAttribute), true);
			if (ats.Length > 0)
				containerAttribute = (TemplateContainerAttribute) ats [0];

#if NET_2_0
			ats = prov.GetCustomAttributes (typeof (TemplateInstanceAttribute), true);
			if (ats.Length > 0)
				instanceAttribute = (TemplateInstanceAttribute) ats [0];
#endif
		}

		public virtual string Text {
			get { return text; }
			set { text = value; }
		}
		
		internal Type ContainerType {
			get { return containerAttribute != null ? containerAttribute.ContainerType : null; }
		}
		
#if NET_2_0
		internal TemplateInstance? TemplateInstance {
			get { return instanceAttribute != null ? instanceAttribute.Instances : (TemplateInstance?)null; }
		}
					
		internal BindingDirection BindingDirection {
			get { return containerAttribute != null ? containerAttribute.BindingDirection : BindingDirection.TwoWay; }
		}
		
		internal void RegisterBoundProperty (Type controlType, string controlProperty, string controlId, string fieldName)
		{
			if (bindings == null)
				bindings = new List <TemplateBinding> ();
			bindings.Add (new TemplateBinding (controlType, controlProperty, controlId, fieldName));
		}
		
		internal ICollection Bindings {
			get { return bindings; }
		}

		public override object BuildObject ()
		{
			return base.BuildObject ();
		}
#endif

		public override void Init (TemplateParser parser,
					  ControlBuilder parentBuilder,
					  Type type,
					  string tagName,
					  string ID,
					  IDictionary attribs)
		{
			// enough?
			if (parser != null)
				FileName = parser.InputFile;
			base.Init (parser, parentBuilder, type, tagName, ID, attribs);
		}
		
		public virtual void InstantiateIn (Control container)
		{
			CreateChildren (container);
		}

		public override bool NeedsTagInnerText ()
		{
			return false;
		}

		public override void SetTagInnerText (string text)
		{
			this.text = text;
		}
	}
	
#if NET_2_0
	internal class TemplateBinding
	{
		public Type ControlType;
		public string ControlProperty;
		public string ControlId;
		public string FieldName;
		
		public TemplateBinding (Type controlType, string controlProperty, string controlId, string fieldName)
		{
			ControlType = controlType;
			ControlProperty = controlProperty;
			ControlId = controlId;
			FieldName = fieldName;
		}
	}
#endif
}

