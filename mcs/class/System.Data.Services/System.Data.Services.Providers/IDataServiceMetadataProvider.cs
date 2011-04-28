// 
// IDataServiceMetadataProvider.cs
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
using System.Data.Services.Providers;

namespace System.Data.Services.Providers
{
	public interface IDataServiceMetadataProvider
	{
		string ContainerNamespace { get; }
		string ContainerName { get; }
		IEnumerable <ResourceSet> ResourceSets { get; }
		IEnumerable <ResourceType> Types { get; }
		IEnumerable <ServiceOperation> ServiceOperations { get; }
		bool TryResolveResourceSet (string name, out ResourceSet resourceSet);
		ResourceAssociationSet GetResourceAssociationSet (ResourceSet resourceSet, ResourceType resourceType, ResourceProperty resourceProperty);
		bool TryResolveResourceType (string name, out ResourceType resourceType);
		IEnumerable <ResourceType> GetDerivedTypes (ResourceType resourceType);
		bool HasDerivedTypes (ResourceType resourceType);
		bool TryResolveServiceOperation (string name, out ServiceOperation serviceOperation);
	}
}
