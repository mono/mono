//
// System.Collections.Comparer.cs
//
// Author:
//   Sergey Chaban (serge@wildwestsoftware.com)
//

using System.Globalization;
using System.Runtime.Serialization;

namespace System.Collections
{
	[Serializable]
	public sealed class Comparer : IComparer, ISerializable
	{
		public static readonly Comparer Default = new Comparer ();
#if NET_1_1
		public
#else
		internal
#endif
		static readonly Comparer DefaultInvariant = new Comparer (CultureInfo.InvariantCulture);

		private CompareInfo _compareInfo;

		private Comparer ()
		{
			//LAMESPEC: This seems to be encoded at runtime while CaseInsensitiveComparer does at creation
		}

#if NET_1_1
		public
#else
		internal
#endif
		Comparer (CultureInfo culture)
		{
			if (culture == null)
				throw new ArgumentNullException ("culture");

			_compareInfo = culture.CompareInfo;
		}


		// IComparer
		public int Compare (object a, object b)
		{
			if (a == b)
				return 0;
			else if (a == null)
				return -1;
			else if (b == null)
				return 1;

			if (_compareInfo != null) {
				string sa = a as string;
				string sb = b as string;
				if (sa != null && sb != null)
					return _compareInfo.Compare (sa, sb);
			}

			if (a is IComparable)
				return (a as IComparable).CompareTo (b);
			else if (b is IComparable)
				return -(b as IComparable).CompareTo (a);

			throw new ArgumentException (Locale.GetText ("Neither a nor b Comparable."));
		}

		#region Implementation of ISerializable

		private Comparer (SerializationInfo info, StreamingContext context)
		{
			_compareInfo = null;
			foreach (SerializationEntry entry in info)
			{
				if (entry.Name == "CompareInfo")
				{
					_compareInfo = (CompareInfo) info.GetValue("CompareInfo", typeof(CompareInfo));
					break;
				}
			}
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
 			if (info == null)
			{
 				throw new ArgumentNullException ("info");
			}

			if (_compareInfo != null)
			{
				info.AddValue("CompareInfo", _compareInfo);
			}
		}

		#endregion Implementation of ISerializable
	}
}
