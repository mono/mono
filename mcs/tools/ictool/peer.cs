//
// file:	peer.cs
// author:	Dan Lewis (dihlewis@yahoo.co.uk)
// 		(C) 2002
//

using System;
using System.Reflection;
using System.Collections;

class Peer {
	public Peer (Type clr_type, string name, bool is_opaque) {
		this.clr_type = clr_type;
		this.name = name;
		this.is_opaque = is_opaque;

		this.nearest_base = null;	// resolve later
		this.underlying = null;
		this.enum_constants = null;
		this.fields = new PeerFieldCollection ();

		this.is_enum = CLRIsEnum (clr_type);
		this.is_value_type = CLRIsValueType (clr_type);
	}

	public string Name {
		get { return name; }
	}

	public Type CLRType {
		get { return clr_type; }
	}

	public bool IsOpaque {
		get { return is_opaque; }
	}

	public bool IsValueType {
		get { return is_value_type; }
	}

	public bool IsEnum {
		get { return is_enum; }
	}

	public Peer NearestBase {
		get { return nearest_base; }
		set { nearest_base = value; }
	}

	public Peer UnderlyingPeer {
		get { return underlying; }
		set { underlying = value; }
	}

	public IDictionary EnumConstants {
		get { return enum_constants; }
		set { enum_constants = value; }
	}

	public PeerFieldCollection Fields {
		get { return fields; }
	}

	public string GetTypedef (int refs) {
		if (refs == 0)
			return String.Format ("{0} ", name);

		return String.Format ("{0} {1}", name, new string ('*', refs));
	}

	// internal

	internal static bool CLRIsValueType (Type clr_type) {
		return clr_type.IsValueType;
		/*
		if (clr_type.BaseType == null)
			return false;
	
		return
			clr_type.BaseType.FullName == "System.ValueType" ||
			clr_type.BaseType.FullName == "System.Enum";
		*/
	}

	internal static bool CLRIsEnum (Type clr_type) {
		return clr_type.IsEnum;
		/*
		if (clr_type.BaseType == null)
			return false;

		return clr_type.BaseType.FullName == "System.Enum";
		*/
	}

	internal static Type CLRUnderlyingType (Type clr_type) {
		return Enum.GetUnderlyingType (clr_type);
		/*
		Type ebase = type.BaseType;

		return (Type)ebase.InvokeMember ("GetUnderlyingType",
			BindingFlags.Public | BindingFlags.InvokeMethod | BindingFlags.Static,
			null, null,
			new object[] { type }
		);
		*/
	}

	// private

	private Type clr_type;
	private bool is_opaque;
	private bool is_value_type;
	private bool is_enum;

	private string name;
	private Peer nearest_base;
	private Peer underlying;
	private IDictionary enum_constants;
	private PeerFieldCollection fields;
}

class PeerField {
	public PeerField (Peer peer, string name) {
		this.peer = peer;
		this.name = name;
	}

	public Peer Peer {
		get { return peer; }
	}

	public string Name {
		get { return name; }
	}

	private Peer peer;
	private string name;
}

class PeerFieldCollection : CollectionBase {
	public void Add (PeerField f) {
		List.Add (f);
	}

	public PeerField this[int i] {
		get { return (PeerField)List[i]; }
	}
}

class PeerMap {
	public PeerMap () {
		peers = new Hashtable ();
	}

	public void Add (Peer peer) {
		Add (peer.CLRType, peer);
	}

	public void Add (Type clr_type, Peer peer) {
		peers.Add (clr_type, peer);
	}

	public ICollection Peers {
		get { return peers.Values; }
	}

	public Peer this[Type clr_type] {
		get {
			if (peers.Contains (clr_type))
				return (Peer)peers[clr_type];

			return null;
		}
	}

	public Peer GetPeer (Type clr_type) {
		Peer peer;

		if (Peer.CLRIsValueType (clr_type)) {
			peer = this[clr_type];
			if (peer != null)
				return peer;

			if (Peer.CLRIsEnum (clr_type)) {
				peer = this[Peer.CLRUnderlyingType (clr_type)];
				if (peer != null)
				return peer;

				throw new ArgumentException ("Could not find peer or underlying peer for enum " + clr_type);
			}
			else
				throw new ArgumentException ("Could not find peer for value type " + clr_type);
		}
		else {
			Type type = clr_type;
			while (type != null) {
				peer = this[type];
				if (peer != null)
					return peer;

				type = type.BaseType;
			}

			throw new ArgumentException ("Could not find peer for class " + clr_type);
		}
	}

	public void ResolvePeers () {
		BindingFlags binding =
			BindingFlags.DeclaredOnly |
			BindingFlags.Instance |
			BindingFlags.NonPublic |
			BindingFlags.Public;

		// base type

		foreach (Peer peer in Peers) {
			if (peer.IsOpaque || peer.IsValueType || peer.CLRType.BaseType == null)
				continue;

			peer.NearestBase = GetPeer (peer.CLRType.BaseType);
			if (peer.NearestBase == null) {
				Console.Error.WriteLine ("Error: cannot find an internal base type for {0}.", peer.Name);
				Environment.Exit (-1);
			}
		}

		// fields

		foreach (Peer peer in Peers) {
			if (peer.IsOpaque || peer.IsEnum)
				continue;

			Type clr_base = null;
			if (peer.NearestBase != null)
				clr_base = peer.NearestBase.CLRType;

			Stack declared = new Stack ();
			Type type = peer.CLRType;

			while (type != clr_base) {
				declared.Push (type);
				type = type.BaseType;
			}

			// build declared field list

			while (declared.Count > 0) {
				type = (Type)declared.Pop ();
				foreach (FieldInfo info in type.GetFields (binding)) {
					PeerField field = new PeerField (
						GetPeer (info.FieldType),
						info.Name
					);

					peer.Fields.Add (field);
				}
			}
		}

		// enums

		foreach (Peer peer in Peers) {
			if (peer.IsOpaque || !peer.IsEnum)
				continue;

			Type clr_type = peer.CLRType;

			// constants

			Hashtable constants = new Hashtable ();
			foreach (string name in Enum.GetNames (clr_type))
				constants.Add (name, (int)Enum.Parse (clr_type, name));

			peer.UnderlyingPeer = GetPeer (Enum.GetUnderlyingType (clr_type));
			peer.EnumConstants = constants;
		}
	}

	// private

	private Hashtable peers;
}
