//
// System.Diagnostics.EventLogEntry.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//
// (C) 2002
//

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace System.Diagnostics {

	[Serializable]
	[MonoTODO("Just stubbed out")]
	public sealed class EventLogEntry : Component, ISerializable {

		[MonoTODO]
		internal EventLogEntry ()
		{
		}

//		[MonoTODO]
//		public string Categery {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public short CategoryNumber {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public byte[] Data {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public EventLogEntryType EntryType {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public int EventID {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public int Index {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public string Machineame {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public string Message {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public string[] ReplacementStrings {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public string Source {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public DateTime TimeGenerated {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public DateTime TimeWritten {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public string UserName {
//			get {throw new NotImplementedException ();}
//		}
//
//		[MonoTODO]
//		public bool Equals(EventLogEntry otherEntry)
//		{
//			throw new NotImplementedException ();
//		}
//
		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info, 
			StreamingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}

