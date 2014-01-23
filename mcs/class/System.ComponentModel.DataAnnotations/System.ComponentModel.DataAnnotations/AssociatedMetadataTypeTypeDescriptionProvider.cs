//
// AssociatedMetadataTypeTypeDescriptionProvider.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2008 Novell Inc. http://novell.com
//

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
#if !MOBILE
using System;
using System.ComponentModel;

namespace System.ComponentModel.DataAnnotations
{
	public class AssociatedMetadataTypeTypeDescriptionProvider : TypeDescriptionProvider
	{
		Type type;
		Type associatedMetadataType;
		
		public AssociatedMetadataTypeTypeDescriptionProvider (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			
			this.type = type;
		}

		public AssociatedMetadataTypeTypeDescriptionProvider (Type type, Type associatedMetadataType)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (associatedMetadataType == null)
				throw new ArgumentNullException ("associatedMetadataType");
			
			this.type = type;
			this.associatedMetadataType = associatedMetadataType;
		}

		public override ICustomTypeDescriptor GetTypeDescriptor (Type objectType, object instance)
		{
			return new AssociatedMetadataTypeTypeDescriptor (base.GetTypeDescriptor (objectType, instance), type, associatedMetadataType);
		}
	}
}
#endif
