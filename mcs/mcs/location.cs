//
// location.cs: Keeps track of the location of source code entity
//
// Author:
//   Miguel de Icaza
//   Atsushi Enomoto  <atsushi@ximian.com>
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2001 Ximian, Inc.
// Copyright 2005 Novell, Inc.
//

using System;
using System.Collections.Generic;
using Mono.CompilerServices.SymbolWriter;
using System.Diagnostics;
using System.Linq;

namespace Mono.CSharp
{
	//
	//  This is one single source file.
	//
	public class SourceFile : IEquatable<SourceFile>
	{
		//
		// Used by #line directive to track hidden sequence point
		// regions
		//
		struct LocationRegion : IComparable<LocationRegion>
		{
			public readonly Location Start;
			public readonly Location End;

			public LocationRegion (Location start, Location end)
			{
				this.Start = start;
				this.End = end;
			}

			public int CompareTo (LocationRegion other)
			{
				if (Start.Row == other.Start.Row)
					return Start.Column.CompareTo (other.Start.Column);

				return Start.Row.CompareTo (other.Start.Row);
			}

			public override string ToString ()
			{
				return Start.ToString () + " - " + End.ToString ();
			}
		}

		static readonly byte[] MD5Algorith = { 96, 166, 110, 64, 207, 100, 130, 76, 182, 240, 66, 212, 129, 114, 167, 153 };

		public readonly string Name;
		public readonly string FullPathName;
		public readonly int Index;
		public bool AutoGenerated;

		SourceFileEntry file;
		byte[] algGuid, checksum;
		List<LocationRegion> hidden_lines;

		public SourceFile (string name, string path, int index)
		{
			this.Index = index;
			this.Name = name;
			this.FullPathName = path;
		}

		public byte[] Checksum {
			get {
				return checksum;
			}
		}

		public bool HasChecksum {
			get {
				return checksum != null;
			}
		}

		public SourceFileEntry SourceFileEntry {
			get {
				return file;
			}
		}

		public void SetChecksum (byte[] checksum)
		{
			SetChecksum (MD5Algorith, checksum);
		}

		public void SetChecksum (byte[] algorithmGuid, byte[] checksum)
		{
			this.algGuid = algorithmGuid;
			this.checksum = checksum;
		}

		public SourceFileEntry CreateSymbolInfo (MonoSymbolFile symwriter)
		{
			if (hidden_lines != null)
				hidden_lines.Sort ();

			file = new SourceFileEntry (symwriter, FullPathName, algGuid, checksum);
			if (AutoGenerated)
				file.SetAutoGenerated ();

			return file;
		}

		public bool Equals (SourceFile other)
		{
			return FullPathName == other.FullPathName;
		}

		public bool IsHiddenLocation (Location loc)
		{
			if (hidden_lines == null)
				return false;

			int index = hidden_lines.BinarySearch (new LocationRegion (loc, loc));
			index = ~index;
			if (index > 0) {
				var found = hidden_lines[index - 1];
				if (loc.Row < found.End.Row)
					return true;
			}

			return false;
		}

		public void RegisterHiddenScope (Location start, Location end)
		{
			if (hidden_lines == null)
				hidden_lines = new List<LocationRegion> ();

			hidden_lines.Add (new LocationRegion (start, end));
		}

		public override string ToString ()
		{
			return String.Format ("SourceFile ({0}:{1}:{2})", Name, FullPathName, Index);
		}
	}

	/// <summary>
	///   Keeps track of the location in the program
	/// </summary>
	///
	/// <remarks>
	///   This uses a compact representation and a couple of auxiliary
	///   structures to keep track of tokens to (file,line and column) 
	///   mappings. The usage of the bits is:
	///   
	///     - 16 bits for "checkpoint" which is a mixed concept of
	///       file and "line segment"
	///     - 8 bits for line delta (offset) from the line segment
	///     - 8 bits for column number.
	///
	///   http://lists.ximian.com/pipermail/mono-devel-list/2004-December/009508.html
	/// </remarks>
	public struct Location : IEquatable<Location>
	{
		struct Checkpoint {
			public readonly int LineOffset;
			public readonly int File;

			public Checkpoint (int file, int line)
			{
				File = file;
				LineOffset = line - (int) (line % (1 << line_delta_bits));
			}
		}

#if FULL_AST
		readonly long token;

		const int column_bits = 24;
		const int line_delta_bits = 24;
#else
		readonly int token;

		const int column_bits = 8;
		const int line_delta_bits = 8;
#endif
		const int checkpoint_bits = 16;

		const int column_mask = (1 << column_bits) - 1;
		const int max_column = column_mask;

		static List<SourceFile> source_list;
		static Checkpoint [] checkpoints;
		static int checkpoint_index;
		
		public readonly static Location Null = new Location ();
		public static bool InEmacs;
		
		static Location ()
		{
			Reset ();
		}

		public static void Reset ()
		{
			source_list = new List<SourceFile> ();
			checkpoint_index = 0;
		}

		public static void AddFile (SourceFile file)
		{
			source_list.Add (file);
		}

		// <summary>
		//   After adding all source files we want to compile with AddFile(), this method
		//   must be called to `reserve' an appropriate number of bits in the token for the
		//   source file.  We reserve some extra space for files we encounter via #line
		//   directives while parsing.
		// </summary>
		static public void Initialize (List<SourceFile> files)
		{
#if NET_4_0 || MONODROID
			source_list.AddRange (files);
#else
			source_list.AddRange (files.ToArray ());
#endif

			checkpoints = new Checkpoint [System.Math.Max (1, source_list.Count * 2)];
			if (checkpoints.Length > 0)
				checkpoints [0] = new Checkpoint (0, 0);
		}

		public Location (SourceFile file, int row, int column)
		{
			if (row <= 0)
				token = 0;
			else {
				if (column > max_column)
					column = max_column;

				long target = -1;
				long delta = 0;

				// TODO: For eval only, need better handling of empty
				int file_index = file == null ? 0 : file.Index;

				// FIXME: This value is certainly wrong but what was the intension
				int max = checkpoint_index < 10 ?
					checkpoint_index : 10;
				for (int i = 0; i < max; i++) {
					int offset = checkpoints [checkpoint_index - i].LineOffset;
					delta = row - offset;
					if (delta >= 0 &&
						delta < (1 << line_delta_bits) &&
						checkpoints[checkpoint_index - i].File == file_index) {
						target = checkpoint_index - i;
						break;
					}
				}
				if (target == -1) {
					AddCheckpoint (file_index, row);
					target = checkpoint_index;
					delta = row % (1 << line_delta_bits);
				}

				long l = column +
					(delta << column_bits) +
					(target << (line_delta_bits + column_bits));
#if FULL_AST
				token = l;
#else
				token = l > 0xFFFFFFFF ? 0 : (int) l;
#endif
			}
		}

		public static Location operator - (Location loc, int columns)
		{
			return new Location (loc.SourceFile, loc.Row, loc.Column - columns);
		}

		static void AddCheckpoint (int file, int row)
		{
			if (checkpoints.Length == ++checkpoint_index) {
				Array.Resize (ref checkpoints, checkpoint_index * 2);
			}
			checkpoints [checkpoint_index] = new Checkpoint (file, row);
		}

		string FormatLocation (string fileName)
		{
			if (column_bits == 0 || InEmacs)
				return fileName + "(" + Row.ToString () + "):";

			return fileName + "(" + Row.ToString () + "," + Column.ToString () +
				(Column == max_column ? "+):" : "):");
		}
		
		public override string ToString ()
		{
			return FormatLocation (Name);
		}

		public string ToStringFullName ()
		{
			return FormatLocation (NameFullPath);
		}
		
		/// <summary>
		///   Whether the Location is Null
		/// </summary>
		public bool IsNull {
			get { return token == 0; }
		}

		public string Name {
			get {
				int index = File;
				if (token == 0 || index <= 0)
					return null;

				SourceFile file = source_list [index - 1];
				return file.Name;
			}
		}

		public string NameFullPath {
			get {
				int index = File;
				if (token == 0 || index <= 0)
					return null;

				return source_list[index - 1].FullPathName;
			}
		}

		int CheckpointIndex {
			get {
				const int checkpoint_mask = (1 << checkpoint_bits) - 1;
				return ((int) (token >> (line_delta_bits + column_bits))) & checkpoint_mask;
			}
		}

		public int Row {
			get {
				if (token == 0)
					return 1;

				int offset = checkpoints[CheckpointIndex].LineOffset;

				const int line_delta_mask = (1 << column_bits) - 1;
				return offset + (((int)(token >> column_bits)) & line_delta_mask);
			}
		}

		public int Column {
			get {
				if (token == 0)
					return 1;
				return (int) (token & column_mask);
			}
		}

		public int File {
			get {
				if (token == 0)
					return 0;
if (checkpoints.Length <= CheckpointIndex) throw new Exception (String.Format ("Should not happen. Token is {0:X04}, checkpoints are {1}, index is {2}", token, checkpoints.Length, CheckpointIndex));
				return checkpoints [CheckpointIndex].File;
			}
		}

		// The ISymbolDocumentWriter interface is used by the symbol writer to
		// describe a single source file - for each source file there's exactly
		// one corresponding ISymbolDocumentWriter instance.
		//
		// This class has an internal hash table mapping source document names
		// to such ISymbolDocumentWriter instances - so there's exactly one
		// instance per document.
		//
		// This property returns the ISymbolDocumentWriter instance which belongs
		// to the location's source file.
		//
		// If we don't have a symbol writer, this property is always null.
		public SourceFile SourceFile {
			get {
				int index = File;
				if (index == 0)
					return null;
				return source_list [index - 1];
			}
		}

		#region IEquatable<Location> Members

		public bool Equals (Location other)
		{
			return this.token == other.token;
		}

		#endregion
	}

	//
	// A bag of additional locations to support full ast tree
	//
	public class LocationsBag
	{
		public class MemberLocations
		{
			public readonly IList<Tuple<Modifiers, Location>> Modifiers;
			List<Location> locations;

			public MemberLocations (IList<Tuple<Modifiers, Location>> mods)
			{
				Modifiers = mods;
			}

			public MemberLocations (IList<Tuple<Modifiers, Location>> mods, Location loc)
				: this (mods)
			{
				AddLocations (loc);
			}

			public MemberLocations (IList<Tuple<Modifiers, Location>> mods, Location[] locs)
				: this (mods)
			{
				AddLocations (locs);
			}

			public MemberLocations (IList<Tuple<Modifiers, Location>> mods, List<Location> locs)
				: this (mods)
			{
				locations = locs;
			}

			#region Properties

			public Location this [int index] {
				get {
					return locations [index];
				}
			}
			
			public int Count {
				get {
					return locations.Count;
				}
			}

			#endregion

			public void AddLocations (Location loc)
			{
				if (locations == null) {
					locations = new List<Location> ();
				}

				locations.Add (loc);
			}

			public void AddLocations (params Location[] additional)
			{
				if (locations == null) {
					locations = new List<Location> (additional);
				} else {
					locations.AddRange (additional);
				}
			}
		}

		Dictionary<object, List<Location>> simple_locs = new Dictionary<object, List<Location>> (ReferenceEquality<object>.Default);
		Dictionary<MemberCore, MemberLocations> member_locs = new Dictionary<MemberCore, MemberLocations> (ReferenceEquality<MemberCore>.Default);

		[Conditional ("FULL_AST")]
		public void AddLocation (object element, params Location[] locations)
		{
			simple_locs.Add (element, new List<Location> (locations));
		}

		[Conditional ("FULL_AST")]
		public void InsertLocation (object element, int index, Location location)
		{
			List<Location> found;
			if (!simple_locs.TryGetValue (element, out found)) {
				found = new List<Location> ();
				simple_locs.Add (element, found);
			}

			found.Insert (index, location);
		}

		[Conditional ("FULL_AST")]
		public void AddStatement (object element, params Location[] locations)
		{
			if (locations.Length == 0)
				throw new ArgumentException ("Statement is missing semicolon location");

			AddLocation (element, locations);
		}

		[Conditional ("FULL_AST")]
		public void AddMember (MemberCore member, IList<Tuple<Modifiers, Location>> modLocations)
		{
			member_locs.Add (member, new MemberLocations (modLocations));
		}

		[Conditional ("FULL_AST")]
		public void AddMember (MemberCore member, IList<Tuple<Modifiers, Location>> modLocations, Location location)
		{
			member_locs.Add (member, new MemberLocations (modLocations, location));
		}

		[Conditional ("FULL_AST")]
		public void AddMember (MemberCore member, IList<Tuple<Modifiers, Location>> modLocations, params Location[] locations)
		{
			member_locs.Add (member, new MemberLocations (modLocations, locations));
		}

		[Conditional ("FULL_AST")]
		public void AddMember (MemberCore member, IList<Tuple<Modifiers, Location>> modLocations, List<Location> locations)
		{
			member_locs.Add (member, new MemberLocations (modLocations, locations));
		}

		[Conditional ("FULL_AST")]
		public void AppendTo (object element, Location location)
		{
			List<Location> found;
			if (!simple_locs.TryGetValue (element, out found)) {
				found = new List<Location> ();
				simple_locs.Add (element, found);
			}

			found.Add (location);
		}

		[Conditional ("FULL_AST")]
		public void AppendToMember (MemberCore existing, params Location[] locations)
		{
			MemberLocations member;
			if (member_locs.TryGetValue (existing, out member)) {
				member.AddLocations (locations);
				return;
			}
		}

		public List<Location> GetLocations (object element)
		{
			List<Location> found;
			simple_locs.TryGetValue (element, out found);
			return found;
		}

		public MemberLocations GetMemberLocation (MemberCore element)
		{
			MemberLocations found;
			member_locs.TryGetValue (element, out found);
			return found;
		}
	}
}
