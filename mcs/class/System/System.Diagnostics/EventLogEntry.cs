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

namespace System.Diagnostics
{

	[Serializable]
	[ToolboxItem (false), DesignTimeVisible (false)]
	public sealed class EventLogEntry : Component, ISerializable
	{

		private string category;
		private short categoryNumber;
		private byte[] data;
		private EventLogEntryType entryType;
		private int eventID;
		private int index;
		private string machineName;
		private string message;
		private string[] replacementStrings;
		private string source;
		private DateTime timeGenerated;
		private DateTime timeWritten;
		private string userName;

		internal EventLogEntry (string category, short categoryNumber, int index, 
					int eventID, string message, string source,
					string userName, string machineName, EventLogEntryType entryType,
					DateTime timeGenerated, DateTime timeWritten, byte[] data,
					string[] replacementStrings)
		{
			this.category = category;
			this.categoryNumber = categoryNumber;
			this.data = data;
			this.entryType = entryType;
			this.eventID = eventID;
			this.index = index;
			this.machineName = machineName;
			this.message = message;
			this.replacementStrings = replacementStrings;
			this.source = source;
			this.timeGenerated = timeGenerated;
			this.timeWritten = timeWritten;
			this.userName = userName;
		}

		[MonoTODO]
		private EventLogEntry (SerializationInfo info, StreamingContext context)
		{
		}

		[MonitoringDescription ("The category of this event entry.")]
		public string Category {
			get { return category; }
		}

		[MonitoringDescription ("An ID for the category of this event entry.")]
		public short CategoryNumber {
			get { return categoryNumber; }
		}

		[MonitoringDescription ("Binary data associated with this event entry.")]
		public byte[] Data {
			get { return data; }
		}

		[MonitoringDescription ("The type of this event entry.")]
		public EventLogEntryType EntryType {
			get { return entryType; }
		}

		[MonitoringDescription ("An ID number for this event entry.")]
		public int EventID {
			get { return eventID; }
		}

		[MonitoringDescription ("Sequence numer of this event entry.")]
		public int Index {
			get { return index; }
		}

		[MonitoringDescription ("The Computer on which this event entry occured.")]
		public string MachineName {
			get { return machineName; }
		}

		[Editor ("System.ComponentModel.Design.BinaryEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MonitoringDescription ("The message of this event entry.")]
		public string Message {
			get { return message; }
		}

		[MonitoringDescription ("Application strings for this event entry.")]
		public string[] ReplacementStrings {
			get { return replacementStrings; }
		}

		[MonitoringDescription ("The source application of this event entry.")]
		public string Source {
			get { return source; }
		}

		[MonitoringDescription ("Generation time of this event entry.")]
		public DateTime TimeGenerated {
			get { return timeGenerated; }
		}

		[MonitoringDescription ("The time at which this event entry was written to the logfile.")]
		public DateTime TimeWritten {
			get { return timeWritten; }
		}

		[MonitoringDescription ("The name of a user associated with this event entry.")]
		public string UserName {
			get { return userName; }
		}

		public bool Equals (EventLogEntry otherEntry)
		{
			if (otherEntry == this)
				return true;

			return (
				(otherEntry.Category == category) &&
				(otherEntry.CategoryNumber == categoryNumber) &&
				(otherEntry.Data.Equals (data)) &&
				(otherEntry.EntryType == entryType) &&
				(otherEntry.EventID == eventID) &&
				(otherEntry.Index == index) &&
				(otherEntry.MachineName == machineName) &&
				(otherEntry.Message == message) &&
				(otherEntry.ReplacementStrings.Equals (replacementStrings)) &&
				(otherEntry.Source == source) &&
				(otherEntry.TimeGenerated.Equals (timeGenerated)) &&
				(otherEntry.TimeWritten.Equals (timeWritten)) &&
				(otherEntry.UserName == userName)
				);
		}

		[MonoTODO ("Needs serialization support")]
		void ISerializable.GetObjectData (SerializationInfo info, 
			StreamingContext context)
		{
			throw new NotImplementedException ();
		}
	}
}

