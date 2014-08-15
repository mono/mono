// 
// ResourceType.cs
//  
// Author:
//       Marek Habersack <grendel@twistedcode.net>
// 
// Copyright (c) 2011 Novell, Inc
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Common;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;

namespace System.Data.Services.Providers
{
	[DebuggerDisplay ("{Name}: {InstanceType}, {ResourceTypeKind}")]
	public class ResourceType
	{
		string nameSpace;
		List<ResourceProperty> resourceProperties;

		public bool IsMediaLinkEntry {
			get; set;
		}

		public Type InstanceType {
			get; private set;
		}

		public ResourceType BaseType {
			get; private set;
		}

		public ResourceTypeKind ResourceTypeKind {
			get; private set;
		}

		public ReadOnlyCollection <ResourceProperty> Properties {
			get { return new ReadOnlyCollection<ResourceProperty> (resourceProperties); }
		}

		public ReadOnlyCollection <ResourceProperty> PropertiesDeclaredOnThisType {
			get { throw new NotImplementedException (); }
		}

		public ReadOnlyCollection <ResourceProperty> KeyProperties {
			get { throw new NotImplementedException (); }
		}

		public ReadOnlyCollection <ResourceProperty> ETagProperties {
			get { throw new NotImplementedException (); }
		}

		public string Name {
			get; private set;
		}

		public string FullName {
			get; private set;
		}

		public string Namespace {
			get {
				if (nameSpace == null)
					return String.Empty;
				return nameSpace;
			}
			private set {
				nameSpace = value;
			}
		}

		public bool IsAbstract {
			get; private set;
		}

		public bool IsOpenType {
			get; set;
		}

		public bool CanReflectOnInstanceType {
			get; set;
		}

		public object CustomState {
			get; set;
		}

		public bool IsReadOnly {
			get; private set;
		}

		public ResourceType (Type instanceType, ResourceTypeKind resourceTypeKind, ResourceType baseType, string namespaceName, string name, bool isAbstract)
		{
			if (instanceType == null)
				throw new ArgumentNullException ("instanceType");
			if (String.IsNullOrEmpty (name))
				throw new ArgumentNullException ("name");
			if (resourceTypeKind == ResourceTypeKind.Primitive)
				throw new ArgumentException ("'Primitive' is not a valid value for resourceTypeKind", "resourceTypeKind");
			if (instanceType.IsValueType)
				throw new ArgumentException ("Clr type for the resource type cannot be a value type.");
			
			this.IsOpenType = false;
			this.IsMediaLinkEntry = false;
			this.IsReadOnly = false;
			this.InstanceType = instanceType;
			this.ResourceTypeKind = resourceTypeKind;
			this.BaseType = baseType;
			if (String.IsNullOrEmpty (namespaceName))
				this.FullName = name;
			else
				this.FullName = namespaceName + "." + name;
			this.Name = name;
			this.Namespace = namespaceName;
			this.IsAbstract = isAbstract;

			// Appears to always be true
			this.CanReflectOnInstanceType = true;

			this.resourceProperties = new List<ResourceProperty> ();
		}

		private ResourceType ()
		{
		}

		public static ResourceType GetPrimitiveResourceType (Type type)
		{
			if (type.IsValueType == false || type.IsPrimitive == false)
				return null;

			ResourceType resourceType = new ResourceType ();
			resourceType.IsOpenType = false;
			resourceType.IsMediaLinkEntry = false;
			resourceType.IsReadOnly = true;
			resourceType.InstanceType = type;
			resourceType.ResourceTypeKind = ResourceTypeKind.Primitive;
			resourceType.BaseType = null;
			resourceType.Name = type.Name;
			resourceType.Namespace = type.Namespace;
			resourceType.FullName = type.FullName;
			resourceType.IsAbstract = false;
			resourceType.CanReflectOnInstanceType = true;
			resourceType.resourceProperties = new List<ResourceProperty> ();

			return resourceType;
		}

		public void AddProperty (ResourceProperty property)
		{
			if (property == null)
				throw new ArgumentNullException ("Value cannot be null.");

			foreach (var pr in resourceProperties) {
				if (pr.Name == property.Name) {
					StringBuilder sb = new StringBuilder ("Property with name '");
					sb.Append (property.Name);
					sb.Append ("' already exists in type '");
					sb.Append (this.FullName);
					sb.Append ("'. Please make sure that there is no property with the same name defined in one of the ancestor types.");

					throw new InvalidOperationException (sb.ToString ());
				}
			}

			this.resourceProperties.Add (property);
		}

		public void AddEntityPropertyMappingAttribute (EntityPropertyMappingAttribute attribute)
		{
			throw new NotImplementedException ();
		}

		public void SetReadOnly ()
		{
			// TODO: anything else?
			IsReadOnly = true;
		}

		protected virtual IEnumerable <ResourceProperty> LoadPropertiesDeclaredOnThisType ()
		{
			throw new NotImplementedException ();
		}
	}
}
