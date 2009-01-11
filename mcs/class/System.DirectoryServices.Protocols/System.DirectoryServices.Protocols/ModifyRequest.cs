//
// ModifyRequest.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.
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

using System;
using System.Xml;

namespace System.DirectoryServices.Protocols
{
	[MonoTODO]
	public class ModifyRequest : DirectoryRequest
	{
		static DirectoryAttributeModification ToAttr (DirectoryAttributeOperation operation, string attributeName, params object [] values)
		{
			var a = new DirectoryAttributeModification ();
			a.Name = attributeName;
			a.Operation = operation;
			a.AddRange (values);
			return a;
		}

		public ModifyRequest ()
		{
		}

		public ModifyRequest (string distinguishedName, DirectoryAttributeOperation operation, string attributeName, params object [] values)
			: this (distinguishedName, new DirectoryAttributeModification [] {ToAttr (operation, attributeName, values)})
		{
		}

		public ModifyRequest (string distinguishedName, params DirectoryAttributeModification [] modifications)
			: this ()
		{
			DistinguishedName = distinguishedName;
			Modifications = new DirectoryAttributeModificationCollection ();
			Modifications.AddRange (modifications);
		}

		public string DistinguishedName { get; set; }

		public DirectoryAttributeModificationCollection Modifications { get; private set; }

		protected override XmlElement ToXmlNode (XmlDocument doc)
		{
			throw new NotImplementedException ();
		}
	}
}
