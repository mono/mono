/********************************************************************************************************************
'
' Copyright (c) 2002, James Newkirk, Michael C. Two, Alexei Vorontsov, Philip Craig
'
' Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
' documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
' the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
' to permit persons to whom the Software is furnished to do so, subject to the following conditions:
'
' The above copyright notice and this permission notice shall be included in all copies or substantial portions 
' of the Software.
'
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
' THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
' AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
'
'*******************************************************************************************************************/
namespace NUnit.Util
{
	using System;
	using System.Collections;
	using Microsoft.Win32;

	/// <summary>
	/// Summary description for RecentAssembly.
	/// </summary>
	public class RecentAssemblyUtil
	{
		private RegistryKey key;
		private static string[] valueNames = { "RecentAssembly1", 
											   "RecentAssembly2", 
											   "RecentAssembly3", 
											   "RecentAssembly4", 
											   "RecentAssembly5" };
		private string subKey;

		private IList assemblyEntries;

		public RecentAssemblyUtil(string subKey)
		{
			this.subKey = subKey;
			key = NUnitRegistry.CurrentUser.CreateSubKey(subKey);
			assemblyEntries = new ArrayList();
			for(int index = 0; index < valueNames.Length; index++)
			{
				string valueName = (string)key.GetValue(valueNames[index]);
				if(valueName != null)
					assemblyEntries.Add(valueName);
			}
		}

		public void Clear()
		{
			NUnitRegistry.CurrentUser.DeleteSubKeyTree(subKey);
			assemblyEntries = new ArrayList();
		}

		public string RecentAssembly
		{
			get 
			{ 
				if(assemblyEntries.Count > 0)
					return (string)assemblyEntries[0];

				return null;
			}
			set
			{
				int index = assemblyEntries.IndexOf(value);

				if(index == 0) return;

				if(index != -1)
				{
					assemblyEntries.RemoveAt(index);
				}

				assemblyEntries.Insert(0, value);
				if(assemblyEntries.Count > valueNames.Length)
					assemblyEntries.RemoveAt(valueNames.Length);

				SaveToRegistry();			
			}
		}

		public IList GetAssemblies()
		{
			return assemblyEntries;
		}

		public void Remove(string assemblyName)
		{
			assemblyEntries.Remove(assemblyName);
			SaveToRegistry();
		}

		private void SaveToRegistry()
		{
			for(int index = 0; 
				index < valueNames.Length;
				index++)
			{
				if ( index < assemblyEntries.Count )
					key.SetValue(valueNames[index], assemblyEntries[index]);
				else
					key.DeleteValue(valueNames[index], false);
			}
		}
	}
}
