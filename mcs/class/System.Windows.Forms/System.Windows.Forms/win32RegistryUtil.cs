/*
 * Copyright (C) 5/11/2002 Carlos Harvey Perez 
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject
 * to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT.
 * IN NO EVENT SHALL CARLOS HARVEY PEREZ BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR
 * THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 * Except as contained in this notice, the name of Carlos Harvey Perez
 * shall not be used in advertising or otherwise to promote the sale,
 * use or other dealings in this Software without prior written
 * authorization from Carlos Harvey Perez.
 */

using System;
using Microsoft.Win32;

//namespace UtilityLibrary.Win32
namespace System.Windows.Forms
{
	/// <summary>
	/// Summary description for Registry.
	/// </summary>
	public class RegistryUtil
	{
		#region Constructors
		// No need to constructo this object
		private RegistryUtil()
		{
		}
		#endregion

		#region Implementation
		static public void WriteToRegistry(RegistryKey RegHive, string RegPath, string KeyName, string KeyValue)
		{
			// Split the registry path 
			string[] regStrings;
   			regStrings = RegPath.Split('\\'); 
			// First item of array will be the base key, so be carefull iterating below
			RegistryKey[] RegKey = new RegistryKey[regStrings.Length + 1]; 
			RegKey[0] = RegHive; 
  
			for( int i = 0; i < regStrings.Length; i++ )
			{ 
				RegKey[i + 1] = RegKey[i].OpenSubKey(regStrings[i], true);
				// If key does not exist, create it
				if (RegKey[i + 1] == null)  
				{
					RegKey[i + 1] = RegKey[i].CreateSubKey(regStrings[i]);
				}
			} 
			
			// Write the value to the registry
			try
			{
				RegKey[regStrings.Length].SetValue(KeyName, KeyValue);     
			}
			catch (System.NullReferenceException)
			{
				throw(new Exception("Null Reference"));
			}
			catch (System.UnauthorizedAccessException)
			{
    			throw(new Exception("Unauthorized Access"));
			}
		}

		static public string ReadFromRegistry(RegistryKey RegHive, string RegPath, string KeyName, string DefaultValue)
		{
			string[] regStrings;
			string result = ""; 
			
			regStrings = RegPath.Split('\\');
			//First item of array will be the base key, so be carefull iterating below
			RegistryKey[] RegKey = new RegistryKey[regStrings.Length + 1]; 
			RegKey[0] = RegHive; 
			
			for( int i = 0; i < regStrings.Length; i++ )
			{
				RegKey[i + 1] = RegKey[i].OpenSubKey(regStrings[i]);
				if (i  == regStrings.Length - 1 )
				{
					result = (string)RegKey[i + 1].GetValue(KeyName, DefaultValue); 
				}
			} 
			return result; 
		}  

		#endregion
	}
}




