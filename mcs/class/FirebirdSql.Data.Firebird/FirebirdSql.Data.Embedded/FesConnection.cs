/*
 *	Firebird ADO.NET Data provider for .NET and Mono 
 * 
 *	   The contents of this file are subject to the Initial 
 *	   Developer's Public License Version 1.0 (the "License"); 
 *	   you may not use this file except in compliance with the 
 *	   License. You may obtain a copy of the License at 
 *	   http://www.firebirdsql.org/index.php?op=doc&id=idpl
 *
 *	   Software distributed under the License is distributed on 
 *	   an "AS IS" basis, WITHOUT WARRANTY OF ANY KIND, either 
 *	   express or implied. See the License for the specific 
 *	   language governing rights and limitations under the License.
 * 
 *	Copyright (c) 2002, 2005 Carlos Guzman Alvarez
 *	All Rights Reserved.
 */

using System;
using System.Runtime.InteropServices;

using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Embedded
{
	internal sealed class FesConnection
	{
		#region Constructors

		private FesConnection()
		{
		}

		#endregion

		#region Static Methods

		public static int[] GetNewStatusVector()
		{
			return new int[IscCodes.ISC_STATUS_LENGTH];
		}

		public static IscException ParseStatusVector(int[] statusVector)
		{
			IscException exception = null;
			bool eof = false;

			for (int i = 0; i < statusVector.Length; )
			{
				int arg = statusVector[i++];

				switch (arg)
				{
					case IscCodes.isc_arg_gds:
						int er = statusVector[i++];
						if (er != 0)
						{
							if (exception == null)
							{
								exception = new IscException();
							}
							exception.Errors.Add(arg, er);
						}
						break;

					case IscCodes.isc_arg_end:
						if (exception != null && exception.Errors.Count != 0)
						{
							exception.BuildExceptionMessage();
						}
						eof = true;
						break;

					case IscCodes.isc_arg_interpreted:
					case IscCodes.isc_arg_string:
						{
							IntPtr ptr = new IntPtr(statusVector[i++]);
							string arg_value = Marshal.PtrToStringAnsi(ptr);
							exception.Errors.Add(arg, arg_value);
						}
						break;

					case IscCodes.isc_arg_cstring:
						{
							i++;

							IntPtr ptr = new IntPtr(statusVector[i++]);
							string arg_value = Marshal.PtrToStringAnsi(ptr);
							exception.Errors.Add(arg, arg_value);
						}
						break;

					case IscCodes.isc_arg_win32:
					case IscCodes.isc_arg_number:
						exception.Errors.Add(arg, statusVector[i++]);
						break;

					default:
						{
							int e = statusVector[i++];
							if (e != 0)
							{
								if (exception == null)
								{
									exception = new IscException();
								}
								exception.Errors.Add(arg, e);
							}
						}
						break;
				}

				if (eof)
				{
					break;
				}
			}

			return exception;
		}

		#endregion
	}
}
