using System;
using System.IO;
using System.Collections.Generic;

namespace Mono
{
	static class BinaryReaderExtensions
	{
		public static int ReadVariableInt (this BinaryReader reader)
		{
			int val = 0;
			for (var i = 0; i < 4; i++) {
				var b = reader.ReadByte ();
				val |= (b & 0x7f) << (7 * i);
				if ((b & 0x80) == 0)
					return val;
			}

			throw new Exception ("Invalid variable int");
		}

		public static int ReadVariableZigZagInt (this BinaryReader reader)
		{
			int enc = ReadVariableInt (reader);
			int val = enc >> 1;
			return ((enc & 1) == 0)? val : -val;
		}
	}

	class SeqPointInfo
	{
		class MethodData
		{
			List<SeqPoint> seqPoints;

			public static MethodData Read (BinaryReader reader)
			{
				var hasDebugData = reader.ReadVariableInt () != 0;
				var dataSize = reader.ReadVariableInt ();
				var dataEnd = reader.BaseStream.Position + dataSize;

				var seqPoints = new List<SeqPoint> ();
				SeqPoint prev = null;
				while (reader.BaseStream.Position < dataEnd) {
					var seqPoint = SeqPoint.Read (reader, prev, hasDebugData);
					seqPoints.Add (seqPoint);
					prev = seqPoint;
				}

				if (reader.BaseStream.Position != dataEnd)
					throw new Exception ("Read more seq point than expected.");

				return new MethodData () { seqPoints = seqPoints };
			}

			public bool TryGetILOffset (int nativeOffset, out int ilOffset)
			{
				ilOffset = 0;
				SeqPoint prev = null;
				foreach (var seqPoint in seqPoints) {
					if (seqPoint.NativeOffset > nativeOffset)
						break;
					prev = seqPoint;
				}

				if (prev == null)
					return false;

				ilOffset = prev.ILOffset;
				return true;
			}
		}

		class SeqPoint
		{
			public readonly int ILOffset;
			public readonly int NativeOffset;

			public SeqPoint (int ilOffset, int nativeOffset)
			{
				ILOffset = ilOffset;
				NativeOffset = nativeOffset;
			}

			public static SeqPoint Read (BinaryReader reader, SeqPoint prev, bool hasDebug)
			{
				var ilOffset = reader.ReadVariableZigZagInt ();
				var nativeOffset = reader.ReadVariableZigZagInt ();

				// Respect delta encoding
				if (prev != null) {
					ilOffset += prev.ILOffset;
					nativeOffset += prev.NativeOffset;
				}

				//Read everything to ensure the buffer position is at the end of the seq point data.
				if (hasDebug) {
					reader.ReadVariableInt (); // flags

					var next_length = reader.ReadVariableInt ();
					for (var i = 0; i < next_length; ++i)
						reader.ReadVariableInt ();
				}

				return new SeqPoint (ilOffset, nativeOffset);
			}
		};

		Dictionary<Tuple<int,int>, MethodData> dataByIds;
		Dictionary<int, MethodData> dataByTokens;

		public static SeqPointInfo Read (string path)
		{
			using (var reader = new BinaryReader (File.Open (path, FileMode.Open)))
			{
				var dataByIds = new Dictionary<Tuple<int,int>, MethodData> ();
				var dataByTokens = new Dictionary<int, MethodData> ();

				var methodCount = reader.ReadVariableInt ();

				for (var i = 0; i < methodCount; ++i) {
					var methodToken = reader.ReadVariableInt ();
					var methodIndex = reader.ReadVariableInt ();
					var methodId = new Tuple<int, int> (methodToken, methodIndex);

					var methodData = MethodData.Read (reader);

					dataByIds.Add (methodId, methodData);
					if (!dataByTokens.ContainsKey (methodToken))
						dataByTokens.Add (methodToken, methodData);
				}

				return new SeqPointInfo { dataByIds  = dataByIds, dataByTokens = dataByTokens };
			}
		}

		public int GetILOffset (int methodToken, uint methodIndex, int nativeOffset)
		{
			MethodData methodData;
			if (methodIndex == 0xffffff) {
			   if (!dataByTokens.TryGetValue (methodToken, out methodData))
					throw new Exception (string.Format ("Could not find data for method token {0:X}", methodToken));
			} else {
				var methodId = new Tuple<int, int> (methodToken, (int)methodIndex);
				if (!dataByIds.TryGetValue (methodId, out methodData))
					throw new Exception (string.Format ("Could not find data for method token {0:X} with index {1:X}", methodToken, methodIndex));
			}

			int ilOffset;
			if (!methodData.TryGetILOffset (nativeOffset, out ilOffset))
				throw new Exception ("Could not retrieve IL offset");

			return ilOffset;
		}
	}
}
