// 
// System.IO.FileUtil.cs 
//
// 
// Authors:
//   Matthew Leibowitz (matthew.leibowitz@xamarin.com)
//
// Copyright 2002 Ximian, Inc. http://www.ximian.com
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// Copyright (C) 2004, 2006, 2010 Novell, Inc (http://www.novell.com)
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

using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace System.IO {
	
	/// <summary>
	/// This is a basic wrapper for the core File type for WinRT.
	/// All paths must be absolute.
	/// </summary>
	static class File {
		
		internal static void Delete (string fileName)
		{
			try {
				DeleteAsync (fileName).Wait ();
			} catch (AggregateException ex) {
				throw ex.InnerException;
			}			
		}
		
		private static async Task DeleteAsync (string fileName)
		{
			await Task.Run (async () => {
				try {
					StorageFile file = await StorageFile.GetFileFromPathAsync (fileName);
					await file.DeleteAsync ();
				} catch (FileNotFoundException ex) {
					// don't throw if the file does not exist
				}
			});
		}
		
		internal static Stream OpenRead (string fileName)
		{
			try {
				return OpenReadAsync (fileName).Result;
			} catch (AggregateException ex) {
				throw ex.InnerException;
			}
		}
		
		internal static async Task<Stream> OpenReadAsync (string fileName)
		{
			StorageFile file = await StorageFile.GetFileFromPathAsync (fileName);
			return await file.OpenStreamForReadAsync ();
		}
		
		internal static bool Exists (string fileName)
		{
			try {
				return ExistsAsync (fileName).Result;
			} catch (AggregateException ex) {
				throw ex.InnerException;
			}
		}
		
		internal static async Task<bool> ExistsAsync (string fileName)
		{
			try {
				await StorageFile.GetFileFromPathAsync (fileName);
			} catch (FileNotFoundException) {
				return false;
			}
			return true;
		}
		
		internal static Stream Create (string fileName)
		{
			try {
				return CreateAsync (fileName).Result;
			} catch (AggregateException ex) {
				throw ex.InnerException;
			}
		}
		
		static async Task<Stream> CreateAsync (string fileName)
		{
			StorageFile file = await CreateFileAsync (fileName);
			return await file.OpenStreamForWriteAsync ();
		}

		static async Task<StorageFile> CreateFileAsync (string fileName)
		{
			StorageFolder folder = await CreateFolderAsync (Path.GetDirectoryName (fileName));
			if (folder != null)
				return await folder.CreateFileAsync (Path.GetFileName (fileName), CreationCollisionOption.ReplaceExisting);
			else
				throw new UnauthorizedAccessException ();
		}

		static async Task<StorageFolder> CreateFolderAsync (string folderPath)
		{
			try {
				return await StorageFolder.GetFolderFromPathAsync(folderPath);
			} catch (FileNotFoundException) {
			}

			StorageFolder folder = null;
			string parent = Path.GetDirectoryName (folderPath);
			if (parent != folderPath)
				folder = await CreateFolderAsync (parent);
			if (folder != null)
				return await folder.CreateFolderAsync (Path.GetFileName (folderPath));

			throw new UnauthorizedAccessException ();
		}		
	}
}
