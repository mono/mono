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
using System.Globalization;

namespace FirebirdSql.Data.Common
{
	internal sealed class IscError
	{
		#region Fields

		private string	message;
		private int		type;
		private int		errorCode;
		private string	strParam;

		#endregion

		#region Properties

		public string Message
		{
			get { return this.message; }
			set { this.message = value; }
		}

		public int ErrorCode
		{
			get { return this.errorCode; }
		}

		public string StrParam
		{
			get
			{
				switch (this.type)
				{
					case IscCodes.isc_arg_interpreted:
					case IscCodes.isc_arg_string:
					case IscCodes.isc_arg_cstring:
						return this.strParam;

					case IscCodes.isc_arg_number:
						return this.errorCode.ToString(CultureInfo.InvariantCulture);

					default:
						return String.Empty;
				}
			}
		}

		public int Type
		{
			get { return this.type; }
		}

		public bool IsArgument
		{
			get
			{
				switch (this.type)
				{
					case IscCodes.isc_arg_interpreted:
					case IscCodes.isc_arg_string:
					case IscCodes.isc_arg_cstring:
					case IscCodes.isc_arg_number:
						return true;

					default:
						return false;
				}
			}
		}

		public bool IsWarning
		{
			get { return this.type == IscCodes.isc_arg_warning; }
		}

		#endregion

		#region Constructors

		internal IscError(int errorCode)
		{
			this.errorCode = errorCode;
		}

		internal IscError(int type, string strParam)
		{
			this.type		= type;
			this.strParam	= strParam;
		}

		internal IscError(int type, int errorCode)
		{
			this.type		= type;
			this.errorCode	= errorCode;
		}

		#endregion
	}
}
