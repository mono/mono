//
// ASN1.cs: Abstract Syntax Notation 1 - micro-parser and generator
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Collections;
using System.Security.Cryptography;

namespace Mono.Security.ASN1 {

// References:
// a.	ITU ASN.1 standards (free download)
//	http://www.itu.int/ITU-T/studygroups/com17/languages/

internal class ASN1 {
	protected byte m_nTag;
	protected byte[] m_aValue;
	protected ArrayList elist;

	public ASN1 () : this (0x00, null) {}

	public ASN1 (byte tag) : this (tag, null) {}

	public ASN1 (byte tag, byte[] data) 
	{
		m_nTag = tag;
		m_aValue = data;
		elist = new ArrayList ();
	}

	public ASN1 (byte[] data) 
	{
		elist = new ArrayList ();
		m_nTag = data [0];

		int nLenLength = 0;
		int nLength = data [1];

		if (nLength > 0x80) {
			// composed length
			nLenLength = nLength - 0x80;
			nLength = 0;
			for (int i = 0; i < nLenLength; i++) {
				nLength *= 256;
				nLength += data [i + 2];
			}
		}

		m_aValue = new byte [nLength];
		Array.Copy (data, (2 + nLenLength), m_aValue, 0, nLength);

		int nStart = 0;
		Decode (data, ref nStart, data.Length);
	}

	public int Count {
		get { return elist.Count; }
	}

	public byte Tag {
		get { return m_nTag; }
	}

	public int Length {
		get { 
			if (m_aValue != null)
				return m_aValue.Length; 
			else
				return 0;
		}
	}

	public byte[] Value {
		get { return (byte[]) m_aValue.Clone (); }
		set { 
			if (value != null)
				m_aValue = (byte[]) value.Clone (); 
		}
	}

	public bool CompareValue (byte[] aValue) 
	{
		bool bResult = (m_aValue.Length == aValue.Length);
		if (bResult) {
			for (int i = 0; i < m_aValue.Length; i++) {
				if (m_aValue[i] != aValue[i])
					return false;
			}
		}
		return bResult;
	}

	public virtual void Add (ASN1 asn1) 
	{
		if (asn1 != null)
			elist.Add (asn1);
	}

	public virtual byte[] GetBytes () 
	{
		byte[] val = null;
		if (m_aValue != null) {
			val = m_aValue;
		}
		else if (elist.Count > 0) {
			int esize = 0;
			ArrayList al = new ArrayList ();
			foreach (ASN1 a in elist) {
				byte[] item = a.GetBytes ();
				al.Add (item);
				esize += item.Length;
			}
			val = new byte [esize];
			int pos = 0;
			for (int i=0; i < elist.Count; i++) {
				byte[] item = (byte[]) al[i];
				Array.Copy (item, 0, val, pos, item.Length);
				pos += item.Length;
			}
		}

		byte[] der;
		int nLengthLen = 0;

		if (val != null) {
			int nLength = val.Length;
			// special for length > 127
			if (nLength > 127) {
				if (nLength < 256) {
					der = new byte [3 + nLength];
					Array.Copy (val, 0, der, 3, nLength);
					nLengthLen += 0x81;
					der[2] = (byte)(nLength);
				}
				else {
					der = new byte [4 + nLength];
					Array.Copy (val, 0, der, 4, nLength);
					nLengthLen += 0x82;
					der[2] = (byte)(nLength / 256);
					der[3] = (byte)(nLength % 256);
				}
			}
			else {
				der = new byte [2 + nLength];
				Array.Copy (val, 0, der, 2, nLength);
				nLengthLen = nLength;
			}
		}
		else
			der = new byte[2];

		der[0] = m_nTag;
		der[1] = (byte)nLengthLen;

		return der;
	}

	// Note: Recursive
	protected void Decode (byte[] asn1, ref int anPos, int anLength) 
	{
		byte nTag;
		int nLength;
		byte[] aValue;

		// minimum is 2 bytes (tag + length of 0)
		while (anPos < anLength - 1) {
			int nPosOri = anPos;
			DecodeTLV (asn1, ref anPos, out nTag, out nLength, out aValue);

			ASN1 elm = new ASN1 (nTag, aValue);
			elist.Add (elm);

			if ((nTag & 0x20) == 0x20) {
				int nConstructedPos = anPos;
				elm.Decode (asn1, ref nConstructedPos, nConstructedPos + nLength);
			}
			anPos += nLength; // value length
		}
	}

	// TLV : Tag - Length - Value
	protected void DecodeTLV (byte[] asn1, ref int anPos, out byte anTag, out int anLength, out byte[] aValue) 
	{
		anTag = asn1 [anPos++];
		anLength = asn1 [anPos++];

		// special case where L contains the Length of the Length + 0x80
		if ((anLength & 0x80) == 0x80) {
			int nLengthLen = anLength & 0x7F;
			anLength = 0;
			for (int i = 0; i < nLengthLen; i++) {
				anLength = anLength * 256 + asn1 [anPos++];
			}
		}

		aValue = new byte [anLength];
		Array.Copy (asn1, anPos, aValue, 0, anLength);
	}

	public ASN1 Element (int index) 
	{
		try {
			return (ASN1) elist [index];
		}
		catch {
			return null;
		}
	}

	public ASN1 Element (int anIndex, byte anTag) 
	{
		try {
			ASN1 elm = (ASN1) elist [anIndex];
			if (elm.Tag == anTag)
				return elm;
			else
				return null;
		}
		catch {
			return null;
		}
	}
}

internal class OID : ASN1 {
	public OID (string oid) : base (CryptoConfig.EncodeOID (oid)) {}
}

}
