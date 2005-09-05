/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/overview/*'/>
	public sealed class FbRemoteEvent
	{
		#region Events

		/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/event[@name="RemoteEventCounts"]/*'/>
		public event FbRemoteEventEventHandler RemoteEventCounts;

		#endregion

		#region Fields

		private FbConnection	connection;
		private RemoteEvent		revent;

		#endregion

		#region Indexers

		/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/indexer[@name="Item(System.Int32)"]/*'/>
		public string this[int index]
		{
			get { return this.revent.Events[index]; }
		}

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/property[@name="Connection"]/*'/>
		public FbConnection Connection
		{
			get { return this.connection; }
			set { this.connection = value; }
		}

		/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/property[@name="HasChanges"]/*'/>
		public bool HasChanges
		{
			get { return this.revent.HasChanges; }
		}

		#endregion

		#region Constructors

		/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/constructor[@name="ctor(FbConnection)"]/*'/>
		public FbRemoteEvent(FbConnection connection) : this(connection, null)
		{
		}

		/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/constructor[@name="ctor(FbConnection, System.Array)"]/*'/>
		public FbRemoteEvent(FbConnection connection, string[] events)
		{
			if (connection == null || connection.State != System.Data.ConnectionState.Open)
			{
				throw new InvalidOperationException("Connection must valid and open");
			}
			this.connection = connection;
			this.revent		= connection.InnerConnection.Database.CreateEvent();
			this.revent.EventCountsCallback = new EventCountsCallback(this.OnRemoteEventCounts);

			if (events != null)
			{
				this.AddEvents(events);
			}
		}

		#endregion

		#region Methods

		/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/method[@name="AddEvents(System.Array)"]/*'/>
		public void AddEvents(string[] events)
		{
			if (events == null)
			{
				throw new ArgumentNullException("events cannot be null.");
			}
			if (events.Length > 15)
			{
				throw new ArgumentException("Max number of events for request interest is 15");
			}

			if (events.Length != this.revent.Events.Count)
			{
				this.revent.ResetCounts();
			}
			else
			{
				string[] actualEvents = new string[this.revent.Events.Count];
				this.revent.Events.CopyTo(actualEvents, 0);

				for (int i = 0; i < actualEvents.Length; i++)
				{
					if (events[i] != actualEvents[i])
					{
						this.revent.ResetCounts();
						break;
					}
				}
			}

			this.revent.Events.Clear();

			for (int i = 0; i < events.Length; i++)
			{
				this.revent.Events.Add(events[i]);
			}
		}

		/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/method[@name="QueueEvents"]/*'/>
		public void QueueEvents()
		{
			this.revent.QueueEvents();
		}

		/// <include file='Doc/en_EN/FbRemoteEvent.xml'	path='doc/class[@name="FbRemoteEvent"]/method[@name="CancelEvents"]/*'/>
		public void CancelEvents()
		{
			this.revent.CancelEvents();
		}

		#endregion

		#region Callbacks methods

		private void OnRemoteEventCounts()
		{
			bool canceled = false;

			if (this.RemoteEventCounts != null)
			{
				int[] actualCounts = (int[])this.revent.ActualCounts.Clone();
				if (this.revent.PreviousCounts != null)
				{
					for (int i = 0; i < this.revent.ActualCounts.Length; i++)
					{
						actualCounts[i] -= this.revent.PreviousCounts[i];
					}
				}

				// Send	individual event notifications
				for (int i = 0; i < actualCounts.Length; i++)
				{
					FbRemoteEventEventArgs args = new FbRemoteEventEventArgs(this.revent.Events[i], actualCounts[i]);
					this.RemoteEventCounts(this, args);

					if (args.Cancel)
					{
						canceled = true;
						break;
					}
				}

				if (canceled)
				{
					// Requeque
					this.CancelEvents();
				}
				else
				{
					// Requeque
					this.QueueEvents();
				}
			}
		}

		#endregion
	}
}
