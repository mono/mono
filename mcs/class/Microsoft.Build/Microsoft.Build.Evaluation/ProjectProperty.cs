// ProjectProperty.cs
//
// Author:
//   Rolf Bjarne Kvinge (rolf@xamarin.com)
//   Atsushi Enomoto (atsushi@xamarin.com)
//
// Copyright (C) 2011,2013 Xamarin Inc.
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
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Internal;

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

		string evaluated_value; // see UpdateEvaluatedValue().
		public string EvaluatedValue {
			get { return evaluated_value; }
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
		
		internal void UpdateEvaluatedValue ()
		{
			evaluated_value = Project.ExpandString (UnevaluatedValue);
		}
	}

	// copy from MS.Build.Engine/BuildProperty.cs
	internal enum PropertyType {
		Reserved,
		Global,
		Normal,
		Environment
	}
	
	internal abstract class BaseProjectProperty : ProjectProperty
	{
		public BaseProjectProperty (Project project, PropertyType propertyType, string name)
			: base (project)
		{
			property_type = propertyType;
			this.name = name;
			predecessor = project.Properties.FirstOrDefault (p => p.Name == name);
			if (predecessor != null)
				project.RemoveProperty (predecessor);
		}
		
		PropertyType property_type;
		
		readonly string name;
		public override string Name {
			get { return name; }
		}
		
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
		readonly ProjectProperty predecessor; 
		public override ProjectProperty Predecessor {
			get { return predecessor; }
		}
	}
	
	internal class XmlProjectProperty : BaseProjectProperty
	{
		public XmlProjectProperty (Project project, ProjectPropertyElement xml, PropertyType propertyType)
			: base (project, propertyType, xml.Name)
		{
			this.xml = xml;
			UpdateEvaluatedValue ();
		}
		
		ProjectPropertyElement xml;
		
		public override string UnevaluatedValue {
			get { return xml.Value; }
			set { xml.Value = value; }
		}
		public override ProjectPropertyElement Xml {
			get { return xml; }
		}
	}
	
	internal class EnvironmentProjectProperty : BaseProjectProperty
	{
		public EnvironmentProjectProperty (Project project, string name, string value)
			: base (project, PropertyType.Environment, name)
		{
			this.value = value;
			UpdateEvaluatedValue ();
		}
		
		readonly string value;
		
		public override string UnevaluatedValue {
			get { return value; }
			set { throw new InvalidOperationException (string.Format ("You cannot change value of environment property '{0}'.", Name)); }
		}
		public override ProjectPropertyElement Xml {
			get { return null; }
		}
	}
	
	internal class GlobalProjectProperty : BaseProjectProperty
	{
		public GlobalProjectProperty (Project project, string name, string value)
			: base (project, PropertyType.Global, name)
		{
			this.value = value;
			UpdateEvaluatedValue ();
		}
		
		readonly string value;
		
		public override string UnevaluatedValue {
			get { return value; }
			set { throw new InvalidOperationException (string.Format ("You cannot change value of global property '{0}'.", Name)); }
		}
		public override ProjectPropertyElement Xml {
			get { return null; }
		}
	}
	
	internal class ManuallyAddedProjectProperty : BaseProjectProperty
	{
		public ManuallyAddedProjectProperty (Project project, string name, string value)
			: base (project, PropertyType.Normal, name)
		{
			this.UnevaluatedValue = value;
		}
		
		public override string UnevaluatedValue { get; set; }
		
		public override ProjectPropertyElement Xml {
			get { return null; }
		}
	}
}

