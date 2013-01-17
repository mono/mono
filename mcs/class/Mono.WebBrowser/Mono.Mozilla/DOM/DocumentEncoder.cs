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
// Copyright (c) 2008 Novell, Inc.
//
// Authors:
//	Andreia Gaita (avidigal@novell.com)
//

using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using io=System.IO;
using Mono.Mozilla.DOM;

namespace Mono.Mozilla {

	internal class DocumentEncoder : DOMObject {

		nsIDocumentEncoder docEncoder = null;
		
		public DocumentEncoder (WebBrowser control) : base (control) {		
			IntPtr docEncoderServicePtr = IntPtr.Zero;

			this.control.ServiceManager.getServiceByContractID (
							"@mozilla.org/layout/documentEncoder;1?type=text/html",
							typeof (nsIDocumentEncoder).GUID,
							out docEncoderServicePtr);
			if (docEncoderServicePtr == IntPtr.Zero)
				throw new Mono.WebBrowser.Exception (Mono.WebBrowser.Exception.ErrorCodes.DocumentEncoderService);

			try {
				docEncoder = (nsIDocumentEncoder)Marshal.GetObjectForIUnknown (docEncoderServicePtr);
			} catch (System.Exception) {
				throw new Mono.WebBrowser.Exception (Mono.WebBrowser.Exception.ErrorCodes.DocumentEncoderService);
			}

			if (control.platform != control.enginePlatform)
				this.docEncoder = nsDocumentEncoder.GetProxy (control, docEncoder);
		}

#region IDisposable Members

		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					this.docEncoder = null;
				}
				disposed = true;
			}
		}
#endregion
		
		string mimeType;
		public string MimeType {
			get {
				if (mimeType == null)
					mimeType = "text/html";
				return mimeType;
			}
			set {
				mimeType = value;
			}
		}
		
		DocumentEncoderFlags flags;
		public DocumentEncoderFlags Flags {
			get { return flags; }
			set { flags = value; }
		}
		
		void Init (Document document, string mimeType, DocumentEncoderFlags flags)
		{
			UniString type = new UniString (mimeType);
			
			try {
				docEncoder.init ((nsIDOMDocument)document.nodeNoProxy, 
			    	             type.Handle, 
			        	         (uint)flags);
			} catch (System.Exception ex) {
				throw new Mono.WebBrowser.Exception (Mono.WebBrowser.Exception.ErrorCodes.DocumentEncoderService, ex);
			}
		}
		
		public string EncodeToString (Document document) {
			Init (document, MimeType, Flags);
			docEncoder.encodeToString(storage);
			return Base.StringGet (storage);
		}
		
		public string EncodeToString (HTMLElement element) {
			Init ((Document)element.Owner, MimeType, Flags);
			docEncoder.setNode (element.nodeNoProxy);
			docEncoder.encodeToString(storage);
			string content = Base.StringGet (storage);
			
			string tag = element.TagName;
			string str = "<" + tag;
			foreach (Mono.WebBrowser.DOM.IAttribute att in element.Attributes) {
				str += " " + att.Name + "=\"" + att.Value + "\"";
			}
			
			str += ">" + content + "</" + tag + ">";
			return str;
		}

		public io.Stream EncodeToStream (Document document) {
			Init (document, MimeType, Flags);
			Stream m = new Stream (new io.MemoryStream ());
			docEncoder.encodeToStream (m);
			return m.BaseStream;
		}

		public io.Stream EncodeToStream (HTMLElement element) {
			Init ((Document)element.Owner, MimeType, Flags);
			docEncoder.setNode (element.nodeNoProxy);
			Stream m = new Stream (new io.MemoryStream ());
			docEncoder.encodeToStream (m);
			return m.BaseStream;
		}
		
/*
		void nsIDocumentEncoder.setSelection ([MarshalAs (UnmanagedType.Interface)]  nsISelection aSelection)
		{
			return ;
		}



		void nsIDocumentEncoder.setRange ([MarshalAs (UnmanagedType.Interface)]  nsIDOMRange aRange)
		{
			return ;
		}



		void nsIDocumentEncoder.setNode ([MarshalAs (UnmanagedType.Interface)]  nsIDOMNode aNode)
		{
			return ;
		}



		void nsIDocumentEncoder.setContainerNode ([MarshalAs (UnmanagedType.Interface)]  nsIDOMNode aContainer)
		{
			return ;
		}



		void nsIDocumentEncoder.setCharset ( string aCharset)
		{
			return ;
		}



		void nsIDocumentEncoder.setWrapColumn ( uint aWrapColumn)
		{
			return ;
		}



		string nsIDocumentEncoder.getMimeType ()
		{
			return null;
		}

		void nsIDocumentEncoder.encodeToStream ([MarshalAs (UnmanagedType.Interface)]  nsIOutputStream aStream)
		{
			return ;
		}



		string nsIDocumentEncoder.encodeToString ()
		{
			return ;
		}



		string nsIDocumentEncoder.encodeToStringWithContext (out string aContextString,
				out string aInfoString)
		{
			return ;
		}



		void nsIDocumentEncoder.setNodeFixup ([MarshalAs (UnmanagedType.Interface)]  nsIDocumentEncoderNodeFixup aFixup)
		{
			return ;
		}
*/

	}
}
