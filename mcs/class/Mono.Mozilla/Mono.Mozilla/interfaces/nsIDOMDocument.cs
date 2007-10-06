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
// Copyright (c) 2007 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Mono.Mozilla
{

	[Guid ("a6cf9075-15b3-11d2-932e-00805f8add32")]
	[InterfaceType (ComInterfaceType.InterfaceIsIUnknown)]
	[ComImport ()]
	internal interface nsIDOMDocument : nsIDOMNode
	{
		#region nsIDOMNode
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNodeName (HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNodeValue (HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setNodeValue (HandleRef value);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNodeType (out ushort ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getParentNode ([MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getChildNodes ([MarshalAs (UnmanagedType.Interface)] out nsIDOMNodeList ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getFirstChild ([MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getLastChild ([MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getPreviousSibling ([MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNextSibling ([MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getAttributes ([MarshalAs (UnmanagedType.Interface)] out nsIDOMNamedNodeMap ret);

		// Modified in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getOwnerDocument ([MarshalAs (UnmanagedType.Interface)] out nsIDOMDocument ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int insertBefore ([MarshalAs (UnmanagedType.Interface)] nsIDOMNode newChild,
						  [MarshalAs (UnmanagedType.Interface)] nsIDOMNode refChild,
						  [MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int replaceChild ([MarshalAs (UnmanagedType.Interface)] nsIDOMNode newChild,
						  [MarshalAs (UnmanagedType.Interface)] nsIDOMNode oldChild,
						  [MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int removeChild ([MarshalAs (UnmanagedType.Interface)] nsIDOMNode oldChild,
						 [MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int appendChild ([MarshalAs (UnmanagedType.Interface)] nsIDOMNode newChild,
						 [MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int hasChildNodes (out bool ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int cloneNode (bool deep, [MarshalAs (UnmanagedType.Interface)] out nsIDOMNode ret);

		// Modified in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int normalize ();

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int isSupported (HandleRef feature,
						 HandleRef version,
						 out bool ret);

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getNamespaceURI (HandleRef ret);

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getPrefix (HandleRef ret);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int setPrefix (HandleRef prefix);
		// raises(DOMException) on setting

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getLocalName (HandleRef ret);

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int hasAttributes (out bool ret);
		#endregion

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getDoctype ([MarshalAs (UnmanagedType.Interface)] out nsIDOMDocumentType docType);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getImplementation ([MarshalAs (UnmanagedType.Interface)] out nsIDOMDOMImplementation implementation);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getDocumentElement ([MarshalAs (UnmanagedType.Interface)] out nsIDOMElement element);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createElement (HandleRef tagName,
							[MarshalAs (UnmanagedType.Interface)] out nsIDOMElement element);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createDocumentFragment ([MarshalAs (UnmanagedType.Interface)] out nsIDOMDocumentFragment fragment);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createTextNode (HandleRef data,
							[MarshalAs (UnmanagedType.Interface)] out nsIDOMText text);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createComment (HandleRef data,
							[MarshalAs (UnmanagedType.Interface)] out nsIDOMComment comment);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createCDATASection (HandleRef data,
								[MarshalAs (UnmanagedType.Interface)] out nsIDOMCDATASection cdata);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createProcessingInstruction (HandleRef target,
										HandleRef data,
									   [MarshalAs (UnmanagedType.Interface)] out nsIDOMProcessingInstruction procInstruction);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createAttribute (HandleRef name,
							 [MarshalAs (UnmanagedType.Interface)] out nsIDOMAttr attr);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createEntityReference (HandleRef name,
									[MarshalAs (UnmanagedType.Interface)] out nsIDOMEntityReference entity);

		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getElementsByTagName (HandleRef tagname,
								  [MarshalAs (UnmanagedType.Interface)] out nsIDOMNodeList nodes);

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int importNode ([MarshalAs (UnmanagedType.Interface)] nsIDOMNode importedNode,
						 bool deep, 
						[MarshalAs (UnmanagedType.Interface)] out nsIDOMNode node);

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createElementNS (HandleRef namespaceURI,
							 HandleRef qualifiedName,
							[MarshalAs (UnmanagedType.Interface)] out nsIDOMElement element);

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int createAttributeNS (HandleRef namespaceURI,
								HandleRef qualifiedName,
							   [MarshalAs (UnmanagedType.Interface)] out nsIDOMAttr attr);

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getElementsByTagNameNS (HandleRef namespaceURI,
									HandleRef localName,
								   [MarshalAs (UnmanagedType.Interface)] out nsIDOMNodeList nodes);

		// Introduced in DOM Level 2:
		[PreserveSigAttribute]
		[MethodImpl (MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int getElementById (HandleRef elementId,
							[MarshalAs (UnmanagedType.Interface)] out nsIDOMElement element);
	}
}

