//
// System.ServiceModel.MessageVersion.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ServiceModel;

namespace System.ServiceModel.Channels {

	public sealed class MessageVersion
	{
		EnvelopeVersion envelope;
		AddressingVersion addressing;

		static MessageVersion ()
		{
			None = new MessageVersion (EnvelopeVersion.None, AddressingVersion.None);
			Soap11 = new MessageVersion (EnvelopeVersion.Soap11, AddressingVersion.None);
			Soap12WSAddressing10 = new MessageVersion  (EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10);

#if !MOONLIGHT
			Soap12 = new MessageVersion (EnvelopeVersion.Soap12, AddressingVersion.None);
			Soap11WSAddressing10 = new MessageVersion (EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10);
			Soap11WSAddressingAugust2004 = new MessageVersion (EnvelopeVersion.Soap11, AddressingVersion.WSAddressingAugust2004);
			Soap12WSAddressingAugust2004 = new MessageVersion (EnvelopeVersion.Soap12, AddressingVersion.WSAddressingAugust2004);
#endif
		}

		MessageVersion (EnvelopeVersion envelope, AddressingVersion addressing)
		{
			this.envelope = envelope;
			this.addressing = addressing;
		}
		
		public static MessageVersion CreateVersion (EnvelopeVersion envelope_version)
		{
			return CreateVersion (envelope_version,
				AddressingVersion.WSAddressing10);
		}

		public static MessageVersion CreateVersion (EnvelopeVersion envelope_version,
							    AddressingVersion addressing_version)
		{
			if (envelope_version == EnvelopeVersion.None && addressing_version == AddressingVersion.None)
				return None;
			if (envelope_version == EnvelopeVersion.Soap11 && addressing_version == AddressingVersion.None)
				return Soap11;
			if (envelope_version == EnvelopeVersion.Soap12 && addressing_version == AddressingVersion.WSAddressing10)
				return Soap12WSAddressing10;

#if !MOONLIGHT
			if (envelope_version == EnvelopeVersion.Soap12 && addressing_version == AddressingVersion.None)
				return Soap12;
			if (envelope_version == EnvelopeVersion.Soap11 && addressing_version == AddressingVersion.WSAddressing10)
				return Soap11WSAddressing10;
			if (envelope_version == EnvelopeVersion.Soap11 && addressing_version == AddressingVersion.WSAddressingAugust2004)
				return Soap11WSAddressingAugust2004;
			if (envelope_version == EnvelopeVersion.Soap12 && addressing_version == AddressingVersion.WSAddressingAugust2004)
				return Soap12WSAddressingAugust2004;
#endif
			throw new ArgumentException (string.Format ("EnvelopeVersion {0} cannot be used with AddressingVersion {1}", envelope_version, addressing_version));
		}

		public override bool Equals (object value)
		{
			MessageVersion other = value as MessageVersion;

			if (other == null)
				return false;

			return (other.Addressing == this.Addressing) && (other.Envelope == this.Envelope);
		}

		public override int GetHashCode ()
		{
			return addressing.GetHashCode () + envelope.GetHashCode ();
		}

		public override string ToString ()
		{
			return envelope.ToString () + " " +  addressing.ToString ();
		}

		public AddressingVersion Addressing { 
			get { return addressing; }
		}

		public static MessageVersion Default { 
			get { return CreateVersion (EnvelopeVersion.Soap12); }
		}

		public EnvelopeVersion Envelope {
			get { return envelope; }
		}

		public static MessageVersion None { 
			get; private set;
		}

		public static MessageVersion Soap11 {
			get; private set;
		}

		public static MessageVersion Soap12WSAddressing10 {
			get; private set;
		}

#if !MOONLIGHT
		public static MessageVersion Soap12 {
			get; private set;
		}

		public static MessageVersion Soap11WSAddressing10 {
			get; private set;
		}

		public static MessageVersion Soap11WSAddressingAugust2004 {
			get; private set;
		}

		public static MessageVersion Soap12WSAddressingAugust2004 {
			get; private set;
		}
#endif
	}
}
