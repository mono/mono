//
// System.Web.UI.TemplateContainerAttribute.cs
//
// Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;

namespace System.Web.UI {

	[AttributeUsage (AttributeTargets.Property)]
	public sealed class TemplateContainerAttribute : Attribute
	{
		Type containerType;
		
		public TemplateContainerAttribute (Type containerType)
		{
			this.containerType = containerType;
		}

		public Type ContainerType {
			get { return containerType; }
		}
	}
}
