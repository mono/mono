// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Mono.Profiler.Log {

	public sealed class LogStreamHeader {
		
		const int MinVersion = 13;
		const int MaxVersion = 15;

		const int Id = 0x4d505a01;

		public Version Version { get; }

		public int FormatVersion { get; }

		public byte PointerSize { get; }

		public ulong StartupTime { get; }

		public int TimerOverhead { get; }

		public int Flags { get; }

		public int ProcessId { get; }

		public int Port { get; }

		public string Arguments { get; }

		public string Architecture { get; }

		public string OperatingSystem { get; }

		internal LogStreamHeader (LogReader reader)
		{
			var id = reader.ReadInt32 ();

			if (id != Id)
				throw new LogException ($"Invalid stream header ID (0x{id:X}).");

			Version = new Version (reader.ReadByte (), reader.ReadByte ());
			FormatVersion = reader.ReadByte ();

			if (FormatVersion < MinVersion || FormatVersion > MaxVersion)
				throw new LogException ($"Unsupported MLPD version {FormatVersion}. Should be >= {MinVersion} and <= {MaxVersion}.");
			
			PointerSize = reader.ReadByte ();
			StartupTime = reader.ReadUInt64 ();
			TimerOverhead = reader.ReadInt32 ();
			Flags = reader.ReadInt32 ();
			ProcessId = reader.ReadInt32 ();
			Port = reader.ReadUInt16 ();
			Arguments = reader.ReadHeaderString ();
			Architecture = reader.ReadHeaderString ();
			OperatingSystem = reader.ReadHeaderString ();
		}
	}
}
