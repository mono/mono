using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UnityProfileShaper
{
	class Tools
	{
		internal static IEnumerable<string> ReadLinesFromDataFileWithPlus(string file)
		{
			return ReadLinesFromDataFile(file).Where(l => l.StartsWith("+")).Select(l => l.Substring(1));
		}

		internal static IEnumerable<string> ReadLinesFromDataFileWithoutPlus(string file)
		{
			return ReadLinesFromDataFile(file).Where(l => !l.StartsWith("+"));
		}

		internal static string[] ReadLinesFromDataFile(string filename)
		{
			string tuningFolder = GetTuningFolder();
			var datafile = Path.Combine(tuningFolder, "TuningInput/"+filename);
			return File.ReadAllLines(datafile).Where(l => !l.StartsWith("#")).ToArray();
		}

		internal static string GetTuningFolder()
		{
			var fileinfo = new FileInfo(Assembly.GetExecutingAssembly().Location);
			return fileinfo.Directory.Parent.Parent.Parent.FullName;
		}
	}
}
