//
// System.Diagnostics.EventLogEntry.cs
//
// Authors:
//   Jonathan Pryor (jonpryor@vt.edu)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace System.Diagnostics {

	[Serializable]
	[MonoTODO("Just stubbed out")]
	[ToolboxItem (""), DesignTimeVisible (false), DesignerCategory ("Category")]
	public sealed class EventLogEntry : Component, ISerializable {

		[MonoTODO]
		internal EventLogEntry ()
		{
		}

		[MonoTODO]
		[MonitoringDescription ("The category of this event entry.")]
		public string Category {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("An ID for the category of this event entry.")]
		public short CategoryNumber {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("Binary data associated with this event entry.")]
		public byte[] Data {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("The type of this event entry.")]
		public EventLogEntryType EntryType {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("An ID number for this event entry.")]
		public int EventID {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("Sequence numer of this event entry.")]
		public int Index {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("The Computer on which this event entry occured.")]
		public string MachineName {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		#if (NET_1_0)
			[Editor ("System.ComponentModel.Design.BinaryEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		#if (NET_1_1)
    			[Editor ("System.ComponentModel.Design.BinaryEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		[MonitoringDescription ("The message of this event entry.")]
		public string Message {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("Application strings for this event entry.")]
		public string[] ReplacementStrings {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("The source application of this event entry.")]
		public string Source {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("Generation time of this event entry.")]
		public DateTime TimeGenerated {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("The time at which this event entry was written to the logfile.")]
		public DateTime TimeWritten {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		[MonitoringDescription ("The name of a user associated with this event entry.")]
		public string UserName {
			get {throw new NotImplementedException ();}
		}

		[MonoTODO]
		public bool Equals(EventLogEntry otherEntry)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		void ISerializable.GetObjectData(SerializationInfo info, 
			StreamingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}

