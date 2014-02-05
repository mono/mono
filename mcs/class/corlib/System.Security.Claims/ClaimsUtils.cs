//
// ClaimUtils.cs
//
// Authors:
//	Matthias Dittrich <matthi.d@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
#if NET_4_5
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
namespace System.SimpleLinq {
	[MonoTODO ("We should move this to it's own file. Why should we not be able to use simple linq queries in corlib?")]
	internal static class EnumerableExtensions {
		internal static IEnumerable<T> Filter<T> (this IEnumerable<T> data, Predicate<T> match)
		{
			foreach (T d in data) {
				if (match (d))
					yield return d;
			}
		}
		internal static bool Any<T> (this IEnumerable<T> data, Predicate<T> match = null)
		{
			if (match == null) match = i => true;
			foreach (T d in data) {
				if (match (d))
					return true;
			}
			return false;
		}
		internal static IEnumerable<U> Select<T, U> (this IEnumerable<T> data, Func<T, U> selector)
		{
			foreach (T d in data) {
				yield return selector (d);
			}
		}
		internal static T FirstOrDefault<T> (this IEnumerable<T> data)
		{
			foreach (T d in data) {
				return d;
			}
			return default (T);
		}
		internal static IEnumerable<U> SelectMany<T, U> (this IEnumerable<T> data, Func<T, IEnumerable<U>> selector)
		{
			foreach (T d in data) {
				foreach (U u in selector (d))
					yield return u;
			}
		}
		internal static List<T> ToList<T> (this IEnumerable<T> data)
		{
			return new List<T> (data);
		}
	}
}
namespace System.Security.Claims {
	internal static class ClaimsUtils {
		[SecurityCritical]
		internal static object DeserializeString (string serializedData, BinaryFormatter formatter = null)
		{
			if (formatter == null) formatter = new BinaryFormatter ();
			using (MemoryStream memoryStream = new MemoryStream (Convert.FromBase64String (serializedData))) {
				return formatter.Deserialize (memoryStream, null);
			}
		}
		[SecurityCritical]
		internal static string SerializeToString (object data, BinaryFormatter formatter = null)
		{
			if (formatter == null) formatter = new BinaryFormatter ();
			using (MemoryStream memoryStream = new MemoryStream ()) {
				formatter.Serialize (memoryStream, data, null);
				return Convert.ToBase64String (memoryStream.GetBuffer (), 0, (int) memoryStream.Length);
			}
		}
	}
}
#endif