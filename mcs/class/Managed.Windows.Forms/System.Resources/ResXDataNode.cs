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
// Copyright (c) 2007 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Andreia Gaita	(avidigal@novell.com)
//  	Olivier Dufour  olivier.duff@gmail.com
//	Gary Barnett	gary.barnett.mono@gmail.com

using System;
using System.Runtime.Serialization;
using System.Drawing;
using System.ComponentModel;
using System.Reflection;
using System.ComponentModel.Design;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Text;

namespace System.Resources
{
	[SerializableAttribute]
#if INSIDE_SYSTEM_WEB
	internal
#else
	public
#endif
	sealed class ResXDataNode : ISerializable
	{
		private string name;
		private ResXFileRef fileRef;
		private string comment;
		private Point pos;

		internal ResXDataNodeHandler handler;


		public string Comment {
			get { return comment ?? String.Empty; }
			set { this.comment = value; }
		}
		
		public ResXFileRef FileRef {
			get { return this.fileRef; }
		}

		public string Name {
			get { return name ?? String.Empty; }
			set { 
				if (value == null)
					throw new ArgumentNullException ("name");
				if (value == String.Empty)
					throw new ArgumentException ("name");

				this.name = value; 
			}
		}

		internal bool IsWritable {
			get {
				return (handler is IWritableHandler);
			}
		}

		internal string MimeType { get; set; }
		internal string Type { get;set;}

		internal string DataString {
			get {
				if (IsWritable)
					return ((IWritableHandler) handler).DataString;
				else
					throw new NotSupportedException ("Node Not Writable");
			}
		}

		public ResXDataNode (string name, object value) : this (name, value, Point.Empty)
		{
		}

		public ResXDataNode (string name, ResXFileRef fileRef)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (fileRef == null)
				throw new ArgumentNullException ("fileRef");

			if (name.Length == 0)
				throw new ArgumentException ("name");

			this.name = name;
			this.fileRef = fileRef;
			pos = Point.Empty;
			handler = new FileRefHandler (fileRef);
		}

		internal ResXDataNode (string name, object value, Point position)
		{
			if (name == null)
				throw new ArgumentNullException ("name");

			if (name.Length == 0)
				throw new ArgumentException ("name");

			Type type = (value == null) ? typeof (object) : value.GetType ();
			if ((value != null) && !type.IsSerializable) {
				throw new InvalidOperationException (String.Format ("'{0}' of type '{1}' cannot be added" 
				                                                    + " because it is not serializable", 
				                                                    name, type));
			}

			this.name = name;
			this.pos = position;
			handler = new InMemoryHandler (value);
		}

		internal ResXDataNode (string nameAtt, string mimeTypeAtt, string typeAtt, 
		                       string dataString, string commentString, Point position, 
		                       string basePath)
		{

			name = nameAtt;
			comment = commentString;
			pos = position;
			MimeType = mimeTypeAtt;
			Type = typeAtt;

			if (!String.IsNullOrEmpty (mimeTypeAtt)) {
				if (!String.IsNullOrEmpty(typeAtt)) {
					handler = new TypeConverterFromResXHandler (dataString, mimeTypeAtt, typeAtt);
				} else {
					handler = new SerializedFromResXHandler (dataString, mimeTypeAtt);
				}
			} else if (!String.IsNullOrEmpty (typeAtt)) { //using hard coded types to avoid version mismatches
				if (typeAtt.StartsWith ("System.Resources.ResXNullRef, System.Windows.Forms")) {
					handler = new NullRefHandler (typeAtt);
				} else if (typeAtt.StartsWith ("System.Byte[], mscorlib")) { 
					handler = new ByteArrayFromResXHandler (dataString);
				} else if (typeAtt.StartsWith ("System.Resources.ResXFileRef, System.Windows.Forms")) {
					ResXFileRef newFileRef = BuildFileRef (dataString, basePath);
					handler = new FileRefHandler (newFileRef);
					this.fileRef = newFileRef;
				} else {
					handler = new TypeConverterFromResXHandler (dataString, mimeTypeAtt, typeAtt);
				}
			} else {
				handler = new InMemoryHandler (dataString);
			}

			if (handler == null)
				throw new Exception ("handler is null");
		}

		public Point GetNodePosition ()
		{
			return pos;
		}

		public string GetValueTypeName (AssemblyName[] names)
		{
			return handler.GetValueTypeName (names);
		}

		public string GetValueTypeName (ITypeResolutionService typeResolver)
		{
			return handler.GetValueTypeName (typeResolver);
		}

		public Object GetValue (AssemblyName[] names)
		{
			return handler.GetValue (names);
		}

		public Object GetValue (ITypeResolutionService typeResolver)
		{
			return handler.GetValue (typeResolver);
		}
		//FIXME: .net doesnt instantiate encoding at this stage
		ResXFileRef BuildFileRef (string dataString, string basePath)
		{
			ResXFileRef fr;

			string[] parts = ResXFileRef.Parse (dataString);

			if (parts.Length < 2)
				throw new ArgumentException ("ResXFileRef cannot be generated");

			string fileName = parts[0];
			if (basePath != null) 
				fileName = Path.Combine (basePath, parts[0]);

			string typeName = parts[1];

			if (parts.Length == 3) {
				Encoding encoding = Encoding.GetEncoding(parts[2]);
				fr = new ResXFileRef (fileName, typeName, encoding);
			} else
				fr = new ResXFileRef (fileName, typeName);
			return fr;
		}

		#region ISerializable Members
		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext context)
		{
			si.AddValue ("Name", this.Name);
			si.AddValue ("Comment", this.Comment);
		}

		#endregion
	}
}
