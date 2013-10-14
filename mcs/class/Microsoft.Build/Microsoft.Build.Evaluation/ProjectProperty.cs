// ProjectProperty.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//
// Copyright (C) 2011 Xamarin Inc.
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
using Microsoft.Build.Construction;

namespace Microsoft.Build.Evaluation
{
	// In .NET 4.0 MSDN says it is non-abstract, but some of those
	// members are abstract and had been there since 4.0.
	// I take this as doc bug, as non-abstract to abstract is a
	// breaking change and I'd rather believe API designer's sanity.
	public abstract class ProjectProperty
	{
		internal ProjectProperty (Project project) // hide default ctor
		{
			Project = project;
		}

		public string EvaluatedValue {
			get {
				throw new NotImplementedException ();
			}
		}

		public abstract bool IsEnvironmentProperty { get; }

		public abstract bool IsGlobalProperty { get; }

		[MonoTODO]
		public abstract bool IsImported { get; }

		public abstract bool IsReservedProperty { get; }

		public abstract string Name { get; }

		[MonoTODO]
		public abstract ProjectProperty Predecessor { get; }

		public Project Project { get; private set; }

		public abstract string UnevaluatedValue { get; set; }

		public abstract ProjectPropertyElement Xml { get; }
	}

	// copy from MS.Build.Engine/BuildProperty.cs
	internal enum PropertyType {
		Reserved,
		Global,
		Normal,
		Environment
	}
	
	internal class XmlProjectProperty : ProjectProperty
	{
		public XmlProjectProperty (Project project, ProjectPropertyElement xml, PropertyType propertyType)
			: base (project)
		{
			this.xml = xml;
			property_type = propertyType;
		}
		
		ProjectPropertyElement xml;
		PropertyType property_type;
		
		public override bool IsEnvironmentProperty {
			get { return property_type == PropertyType.Environment; }
		}
		public override bool IsGlobalProperty {
			get { return property_type == PropertyType.Global; }
		}
		public override bool IsImported {
			get {
				throw new NotImplementedException ();
			}
		}
		public override bool IsReservedProperty {
			get { return property_type == PropertyType.Reserved; }
		}
		public override string Name {
			get { return xml.Name; }
		}
		public override ProjectProperty Predecessor {
			get {
				throw new NotImplementedException ();
			}
		}
		public override string UnevaluatedValue {
			get { return xml.Value; }
			set { xml.Value = value; }
		}
		public override ProjectPropertyElement Xml {
			get { return xml; }
		}
	}
}

