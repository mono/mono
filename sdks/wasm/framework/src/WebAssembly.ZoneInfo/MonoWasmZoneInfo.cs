using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;

namespace WebAssembly.ZoneInfo
{
	public static class MonoWasmZoneInfo
	{
		const string zoneInfoPrefix = "WebAssembly.ZoneInfo.zoneinfo.";
		public static byte[] mono_timezone_get_data(string name, ref int size)
		{
			var data = GetEmbeddedResource(name);
			size = data.Length;
			return data;
		}
		public static string[] mono_timezone_get_names(ref int count)
		{
			var resourceNames = typeof(MonoWasmZoneInfo).Assembly.GetManifestResourceNames();
			var zoneInfoPrefixLengh = zoneInfoPrefix.Length;
			var zoneInfoIDS = new List<string>();
			for (int x = 0; x < resourceNames.Length; x++)
			{
				if (resourceNames[x].StartsWith(zoneInfoPrefix))
					zoneInfoIDS.Add(resourceNames[x].Remove(0, zoneInfoPrefixLengh).Replace('.', '/'));
			}

			count = zoneInfoIDS.Count;
			return zoneInfoIDS.ToArray();
		}
		public static byte[] GetEmbeddedResource(String filename)
		{
			var assembly = typeof(MonoWasmZoneInfo).Assembly;
			var dataResourceName = $"{zoneInfoPrefix}{filename.Replace('/', '.')}";

			using (Stream resFilestream = assembly.GetManifestResourceStream(dataResourceName))
			{
				if (resFilestream == null) return null;
				byte[] ba = new byte[resFilestream.Length];
				resFilestream.Read(ba, 0, ba.Length);
				return ba;
			}
		}
	}
}
