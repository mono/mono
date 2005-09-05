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
using System.Collections.Specialized;

namespace FirebirdSql.Data.Common
{
	#region Delegates

	internal delegate void EventCountsCallback();

	#endregion

	internal class RemoteEvent
	{
		#region Callbacks

		public EventCountsCallback EventCountsCallback
		{
			get { return this.eventCountsCallback; }
			set { this.eventCountsCallback = value; }
		}

		#endregion

		#region Fields

		private EventCountsCallback eventCountsCallback;
		private StringCollection	events;
		private IDatabase	db;
		private int			localId;
		private int			remoteId;
		private bool		initialCounts;
		private int[]		previousCounts;
		private int[]		actualCounts;

		#endregion

		#region Properties

		public IDatabase Database
		{
			get { return this.db; }
		}

		public int LocalId
		{
			get { return this.localId; }
			set { this.localId = value; }
		}

		public int RemoteId
		{
			get { return this.remoteId; }
			set { this.remoteId = value; }
		}

		public StringCollection Events
		{
			get
			{
				if (this.events == null)
				{
					this.events = new StringCollection();
				}

				return this.events;
			}
		}

		public bool HasChanges
		{
			get
			{
				if (this.actualCounts == null && this.previousCounts == null)
				{
					return false;
				}
				else if (this.actualCounts != null && this.previousCounts == null)
				{
					return true;
				}
				else if (this.actualCounts.Length != this.previousCounts.Length)
				{
					return true;
				}

				for (int i = 0; i < this.actualCounts.Length; i++)
				{
					if (this.actualCounts[i] != this.previousCounts[i])
					{
						return true;
					}
				}

				return false;
			}
		}

		public int[] PreviousCounts
		{
			get { return this.previousCounts; }
		}

		public int[] ActualCounts
		{
			get { return this.actualCounts; }
		}

		#endregion

		#region Constructors

		public RemoteEvent(IDatabase db) : this(db, 0, 0, null)
		{
		}

		public RemoteEvent(IDatabase db, int localId, int remoteId, StringCollection events)
		{
			this.db = db;
			this.localId = localId;
			this.remoteId = remoteId;
			this.events = events;
		}

		#endregion

		#region Methods

		public void QueueEvents()
		{
			lock (this.db)
			{
				this.db.QueueEvents(this);
			}
		}

		public void CancelEvents()
		{
			lock (this.db)
			{
				this.db.CancelEvents(this);
				this.ResetCounts();
			}
		}

		public void ResetCounts()
		{
			this.initialCounts	= false;
			this.actualCounts	= null;
			this.previousCounts = null;
		}

		public void EventCounts(byte[] buffer)
		{
			int pos = 1;
			Charset charset = this.db.Charset;

			if (buffer != null)
			{
				if (this.initialCounts)
				{
					this.previousCounts = this.actualCounts;
				}

				this.actualCounts = new int[this.events.Count];

				while (pos < buffer.Length)
				{
					int length = buffer[pos++];
					string eventName = charset.GetString(buffer, pos, length);

					pos += length;

					int index = this.events.IndexOf(eventName);
					if (index != -1)
					{
						this.actualCounts[index] = BitConverter.ToInt32(buffer, pos) - 1;
					}

					pos += 4;
				}

				if (!this.initialCounts)
				{
					this.QueueEvents();
					this.initialCounts = true;
				}
				else
				{
					if (this.EventCountsCallback != null)
					{
						this.EventCountsCallback();
					}
				}
			}
		}

		public EventParameterBuffer ToEpb()
		{
			EventParameterBuffer epb = this.db.CreateEventParameterBuffer();

			epb.Append(IscCodes.EPB_version1);

			for (int i = 0; i < this.events.Count; i++)
			{
				if (this.actualCounts != null)
				{
					epb.Append(this.events[i], this.actualCounts[i] + 1);
				}
				else
				{
					epb.Append(this.events[i], 0);
				}
			}

			return epb;
		}

		#endregion
	}
}
