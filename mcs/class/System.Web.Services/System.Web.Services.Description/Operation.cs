// 
// System.Web.Services.Description.Operation.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.Collections;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services.Configuration;

namespace System.Web.Services.Description 
{
#if NET_2_0
	[XmlFormatExtensionPoint ("Extensions")]
#endif
	public sealed class Operation :
#if NET_2_0
		NamedItem
#else
		DocumentableItem 
#endif
	{
		#region Fields

		OperationFaultCollection faults;
		OperationMessageCollection messages;
#if !NET_2_0
		string name;
#endif
		string[] parameterOrder;
		PortType portType;
#if NET_2_0
		ServiceDescriptionFormatExtensionCollection extensions;
#endif

		#endregion // Fields

		#region Constructors
		
		public Operation ()
		{
			faults = new OperationFaultCollection (this);
			messages = new OperationMessageCollection (this);
#if !NET_2_0
			name = String.Empty;
#endif
			parameterOrder = null;
			portType = null;
#if NET_2_0
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
#endif
		}
		
		#endregion // Constructors

		#region Properties

		[XmlElement ("fault")]
		public OperationFaultCollection Faults {
			get { return faults; }
		}

		[XmlElement ("output", typeof (OperationOutput))]
		[XmlElement ("input", typeof (OperationInput))]
		public OperationMessageCollection Messages {
			get { return messages; }
		}

#if !NET_2_0
		[XmlAttribute ("name", DataType = "NCName")]
		public string Name {
			get { return name; }
			set { name = value; }
		}
#endif

		[XmlIgnore]
		public string[] ParameterOrder {
			get { return parameterOrder; }
			set { parameterOrder = value; }
		}

		static readonly char [] wsChars = new char [] {' ', '\r', '\n', '\t'};

		[DefaultValue ("")]
		// LAMESPEC: it could simply use xs:NMTOKENS
		[XmlAttribute ("parameterOrder")]
		public string ParameterOrderString {
			get { 
				if (parameterOrder == null)
					return String.Empty;
				return String.Join (" ", parameterOrder); 
			}
			set {
				ArrayList al = new ArrayList ();
				foreach (string s in value.Split (' ')) {
					value = s.Trim (wsChars);
					if (value.Length > 0)
						al.Add (value);
				}
				ParameterOrder = (string []) al.ToArray (typeof (string));
			}
		}

//		[XmlIgnore]
		public PortType PortType {
			get { return portType; }
		}

#if NET_2_0
		[XmlIgnore]
		public override ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}
#endif

		#endregion // Properties

		#region Methods

		public bool IsBoundBy (OperationBinding operationBinding)
		{
			return (operationBinding.Name == Name);
		}

		internal void SetParent (PortType portType)
		{
			this.portType = portType;
		}

		#endregion
	}
}
