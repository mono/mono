//
// Copyright (C) 2010 Novell Inc. http://novell.com
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Xaml;
using System.Windows.Markup;

namespace System.Xaml.Schema
{
	public static class XamlLanguage
	{
		public const string Xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";
		public const string Xml1998Namespace = "http://www.w3.org/XML/1998/namespace";

		public static ReadOnlyCollection<XamlDirective> AllDirectives {
			get { throw new NotImplementedException (); }
		}
		public static ReadOnlyCollection<XamlType> AllTypes {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Arguments {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Array {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective AsyncRecords {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Base {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Boolean {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Byte {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Char {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Class {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective ClassAttributes {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective ClassModifier {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Code {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective ConnectionId {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Decimal {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Double {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective FactoryMethod {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective FieldModifier {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Initialization {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Int16 {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Int32 {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Int64 {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Items {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Key {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Lang {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Member {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Members {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Name {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Null {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Object {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective PositionalParameters {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Property {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Reference {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Shared {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Single {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Space {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Static {
			get { throw new NotImplementedException (); }
		}
		public static XamlType String {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Subclass {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective SynchronousMode {
			get { throw new NotImplementedException (); }
		}
		public static XamlType TimeSpan {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Type {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective TypeArguments {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective Uid {
			get { throw new NotImplementedException (); }
		}
		public static XamlDirective UnknownContent {
			get { throw new NotImplementedException (); }
		}
		public static XamlType Uri {
			get { throw new NotImplementedException (); }
		}
		public static IList<string> XamlNamespaces {
			get { throw new NotImplementedException (); }
		}
		public static XamlType XData {
			get { throw new NotImplementedException (); }
		}
		public static IList<string> XmlNamespaces {
			get { throw new NotImplementedException (); }
		}
	}
}
