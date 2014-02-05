// ZipEntryFactory.cs
//
// Copyright 2006 John Reilly
//
// Copyright (C) 2001 Free Software Foundation, Inc.
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
//
// Linking this library statically or dynamically with other modules is
// making a combined work based on this library.  Thus, the terms and
// conditions of the GNU General Public License cover the whole
// combination.
//
// As a special exception, the copyright holders of this library give you
// permission to link this library with independent modules to produce an
// executable, regardless of the license terms of these independent
// modules, and to copy and distribute the resulting executable under
// terms of your choice, provided that you also meet, for each linked
// independent module, the terms and conditions of the license of that
// module.  An independent module is a module which is not derived from
// or based on this library.  If you modify this library, you may extend
// this exception to your version of the library, but you are not
// obligated to do so.  If you do not wish to do so, delete this
// exception statement from your version.

// HISTORY
//	2012-11-29	Z-1684	Added MakeFileEntry(string fileName, string entryName, bool useFileSystem)

using System;
using System.IO;

using ICSharpCode.SharpZipLib.Core;

namespace ICSharpCode.SharpZipLib.Zip
{
	/// <summary>
	/// Basic implementation of <see cref="IEntryFactory"></see>
	/// </summary>
	public class ZipEntryFactory : IEntryFactory
	{
		#region Enumerations
		/// <summary>
		/// Defines the possible values to be used for the <see cref="ZipEntry.DateTime"/>.
		/// </summary>
		public enum TimeSetting
		{
			/// <summary>
			/// Use the recorded LastWriteTime value for the file.
			/// </summary>
			LastWriteTime,
			/// <summary>
			/// Use the recorded LastWriteTimeUtc value for the file
			/// </summary>
			LastWriteTimeUtc,
			/// <summary>
			/// Use the recorded CreateTime value for the file.
			/// </summary>
			CreateTime,
			/// <summary>
			/// Use the recorded CreateTimeUtc value for the file.
			/// </summary>
			CreateTimeUtc,
			/// <summary>
			/// Use the recorded LastAccessTime value for the file.
			/// </summary>
			LastAccessTime,
			/// <summary>
			/// Use the recorded LastAccessTimeUtc value for the file.
			/// </summary>
			LastAccessTimeUtc,
			/// <summary>
			/// Use a fixed value.
			/// </summary>
			/// <remarks>The actual <see cref="DateTime"/> value used can be
			/// specified via the <see cref="ZipEntryFactory(DateTime)"/> constructor or
			/// using the <see cref="ZipEntryFactory(TimeSetting)"/> with the setting set
			/// to <see cref="TimeSetting.Fixed"/> which will use the <see cref="DateTime"/> when this class was constructed.
			/// The <see cref="FixedDateTime"/> property can also be used to set this value.</remarks>
			Fixed,
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Initialise a new instance of the <see cref="ZipEntryFactory"/> class.
		/// </summary>
		/// <remarks>A default <see cref="INameTransform"/>, and the LastWriteTime for files is used.</remarks>
		public ZipEntryFactory()
		{
			nameTransform_ = new ZipNameTransform();
		}

		/// <summary>
		/// Initialise a new instance of <see cref="ZipEntryFactory"/> using the specified <see cref="TimeSetting"/>
		/// </summary>
		/// <param name="timeSetting">The <see cref="TimeSetting">time setting</see> to use when creating <see cref="ZipEntry">Zip entries</see>.</param>
		public ZipEntryFactory(TimeSetting timeSetting)
		{
			timeSetting_ = timeSetting;
			nameTransform_ = new ZipNameTransform();
		}

		/// <summary>
		/// Initialise a new instance of <see cref="ZipEntryFactory"/> using the specified <see cref="DateTime"/>
		/// </summary>
		/// <param name="time">The time to set all <see cref="ZipEntry.DateTime"/> values to.</param>
		public ZipEntryFactory(DateTime time)
		{
			timeSetting_ = TimeSetting.Fixed;
			FixedDateTime = time;
			nameTransform_ = new ZipNameTransform();
		}

		#endregion

		#region Properties
		/// <summary>
		/// Get / set the <see cref="INameTransform"/> to be used when creating new <see cref="ZipEntry"/> values.
		/// </summary>
		/// <remarks>
		/// Setting this property to null will cause a default <see cref="ZipNameTransform">name transform</see> to be used.
		/// </remarks>
		public INameTransform NameTransform
		{
			get { return nameTransform_; }
			set
			{
				if (value == null) {
					nameTransform_ = new ZipNameTransform();
				}
				else {
					nameTransform_ = value;
				}
			}
		}

		/// <summary>
		/// Get / set the <see cref="TimeSetting"/> in use.
		/// </summary>
		public TimeSetting Setting
		{
			get { return timeSetting_; }
			set { timeSetting_ = value; }
		}

		/// <summary>
		/// Get / set the <see cref="DateTime"/> value to use when <see cref="Setting"/> is set to <see cref="TimeSetting.Fixed"/>
		/// </summary>
		public DateTime FixedDateTime
		{
			get { return fixedDateTime_; }
			set
			{
				if (value.Year < 1970) {
					throw new ArgumentException("Value is too old to be valid", "value");
				}
				fixedDateTime_ = value;
			}
		}

		/// <summary>
		/// A bitmask defining the attributes to be retrieved from the actual file.
		/// </summary>
		/// <remarks>The default is to get all possible attributes from the actual file.</remarks>
		public int GetAttributes
		{
			get { return getAttributes_; }
			set { getAttributes_ = value; }
		}

		/// <summary>
		/// A bitmask defining which attributes are to be set on.
		/// </summary>
		/// <remarks>By default no attributes are set on.</remarks>
		public int SetAttributes
		{
			get { return setAttributes_; }
			set { setAttributes_ = value; }
		}

		/// <summary>
		/// Get set a value indicating wether unidoce text should be set on.
		/// </summary>
		public bool IsUnicodeText
		{
			get { return isUnicodeText_; }
			set { isUnicodeText_ = value; }
		}

		#endregion

		#region IEntryFactory Members

		/// <summary>
		/// Make a new <see cref="ZipEntry"/> for a file.
		/// </summary>
		/// <param name="fileName">The name of the file to create a new entry for.</param>
		/// <returns>Returns a new <see cref="ZipEntry"/> based on the <paramref name="fileName"/>.</returns>
		public ZipEntry MakeFileEntry(string fileName)
		{
			return MakeFileEntry(fileName, null, true);
		}

		/// <summary>
		/// Make a new <see cref="ZipEntry"/> for a file.
		/// </summary>
		/// <param name="fileName">The name of the file to create a new entry for.</param>
		/// <param name="useFileSystem">If true entry detail is retrieved from the file system if the file exists.</param>
		/// <returns>Returns a new <see cref="ZipEntry"/> based on the <paramref name="fileName"/>.</returns>
		public ZipEntry MakeFileEntry(string fileName, bool useFileSystem) {
			return MakeFileEntry(fileName, null, useFileSystem);
		}

		/// <summary>
		/// Make a new <see cref="ZipEntry"/> from a name.
		/// </summary>
		/// <param name="fileName">The name of the file to create a new entry for.</param>
		/// <param name="entryName">An alternative name to be used for the new entry. Null if not applicable.</param>
		/// <param name="useFileSystem">If true entry detail is retrieved from the file system if the file exists.</param>
		/// <returns>Returns a new <see cref="ZipEntry"/> based on the <paramref name="fileName"/>.</returns>
		public ZipEntry MakeFileEntry(string fileName, string entryName, bool useFileSystem)
		{
			ZipEntry result = new ZipEntry(nameTransform_.TransformFile(entryName != null && entryName.Length > 0 ? entryName : fileName));
			result.IsUnicodeText = isUnicodeText_;

			int externalAttributes = 0;
			bool useAttributes = (setAttributes_ != 0);

			FileInfo fi = null;
			if (useFileSystem)
			{
				fi = new FileInfo(fileName);
			}

			if ((fi != null) && fi.Exists)
			{
				switch (timeSetting_)
				{
					case TimeSetting.CreateTime:
						result.DateTime = fi.CreationTime;
						break;

					case TimeSetting.CreateTimeUtc:
#if NETCF_1_0 || NETCF_2_0
						result.DateTime = fi.CreationTime.ToUniversalTime();
#else
						result.DateTime = fi.CreationTimeUtc;
#endif
						break;

					case TimeSetting.LastAccessTime:
						result.DateTime = fi.LastAccessTime;
						break;

					case TimeSetting.LastAccessTimeUtc:
#if NETCF_1_0 || NETCF_2_0
						result.DateTime = fi.LastAccessTime.ToUniversalTime();
#else
						result.DateTime = fi.LastAccessTimeUtc;
#endif
						break;

					case TimeSetting.LastWriteTime:
						result.DateTime = fi.LastWriteTime;
						break;

					case TimeSetting.LastWriteTimeUtc:
#if NETCF_1_0 || NETCF_2_0
						result.DateTime = fi.LastWriteTime.ToUniversalTime();
#else
						result.DateTime = fi.LastWriteTimeUtc;
#endif
						break;

					case TimeSetting.Fixed:
						result.DateTime = fixedDateTime_;
						break;

					default:
						throw new ZipException("Unhandled time setting in MakeFileEntry");
				}

				result.Size = fi.Length;

				useAttributes = true;
				externalAttributes = ((int)fi.Attributes & getAttributes_);
			}
			else
			{
				if (timeSetting_ == TimeSetting.Fixed)
				{
					result.DateTime = fixedDateTime_;
				}
			}

			if (useAttributes)
			{
				externalAttributes |= setAttributes_;
				result.ExternalFileAttributes = externalAttributes;
			}

			return result;
		}

		/// <summary>
		/// Make a new <see cref="ZipEntry"></see> for a directory.
		/// </summary>
		/// <param name="directoryName">The raw untransformed name for the new directory</param>
		/// <returns>Returns a new <see cref="ZipEntry"></see> representing a directory.</returns>
		public ZipEntry MakeDirectoryEntry(string directoryName)
		{
			return MakeDirectoryEntry(directoryName, true);
		}

		/// <summary>
		/// Make a new <see cref="ZipEntry"></see> for a directory.
		/// </summary>
		/// <param name="directoryName">The raw untransformed name for the new directory</param>
		/// <param name="useFileSystem">If true entry detail is retrieved from the file system if the file exists.</param>
		/// <returns>Returns a new <see cref="ZipEntry"></see> representing a directory.</returns>
		public ZipEntry MakeDirectoryEntry(string directoryName, bool useFileSystem)
		{

			ZipEntry result = new ZipEntry(nameTransform_.TransformDirectory(directoryName));
            result.IsUnicodeText = isUnicodeText_;
            result.Size = 0;

			int externalAttributes = 0;

			DirectoryInfo di = null;

			if (useFileSystem)
			{
				di = new DirectoryInfo(directoryName);
			}


			if ((di != null) && di.Exists)
			{
				switch (timeSetting_)
				{
					case TimeSetting.CreateTime:
						result.DateTime = di.CreationTime;
						break;

					case TimeSetting.CreateTimeUtc:
#if NETCF_1_0 || NETCF_2_0
						result.DateTime = di.CreationTime.ToUniversalTime();
#else
						result.DateTime = di.CreationTimeUtc;
#endif
						break;

					case TimeSetting.LastAccessTime:
						result.DateTime = di.LastAccessTime;
						break;

					case TimeSetting.LastAccessTimeUtc:
#if NETCF_1_0 || NETCF_2_0
						result.DateTime = di.LastAccessTime.ToUniversalTime();
#else
						result.DateTime = di.LastAccessTimeUtc;
#endif
						break;

					case TimeSetting.LastWriteTime:
						result.DateTime = di.LastWriteTime;
						break;

					case TimeSetting.LastWriteTimeUtc:
#if NETCF_1_0 || NETCF_2_0
						result.DateTime = di.LastWriteTime.ToUniversalTime();
#else
						result.DateTime = di.LastWriteTimeUtc;
#endif
						break;

					case TimeSetting.Fixed:
						result.DateTime = fixedDateTime_;
						break;

					default:
						throw new ZipException("Unhandled time setting in MakeDirectoryEntry");
				}

				externalAttributes = ((int)di.Attributes & getAttributes_);
			}
			else
			{
				if (timeSetting_ == TimeSetting.Fixed)
				{
					result.DateTime = fixedDateTime_;
				}
			}

			// Always set directory attribute on.
			externalAttributes |= (setAttributes_ | 16);
			result.ExternalFileAttributes = externalAttributes;

			return result;
		}

		#endregion

		#region Instance Fields
		INameTransform nameTransform_;
		DateTime fixedDateTime_ = DateTime.Now;
		TimeSetting timeSetting_;
		bool isUnicodeText_;

		int getAttributes_ = -1;
		int setAttributes_;
		#endregion
	}
}
