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
using System.Collections;
using System.Globalization;
using System.Text;
using System.Reflection;
using System.Resources;

namespace FirebirdSql.Data.Common
{
	internal sealed class IscException : Exception
	{
		#region Fields

		private IscErrorCollection errors;
		private int errorCode;
		private string message;

		#endregion

		#region Properties

		public IscErrorCollection Errors
		{
			get
			{
				if (this.errors == null)
				{
					this.errors = new IscErrorCollection();
				}

				return this.errors;
			}
		}

		public new string Message
		{
			get { return this.message; }
		}

		public int ErrorCode
		{
			get { return this.errorCode; }
		}

		public bool IsWarning
		{
			get
			{
				if (this.errors.Count > 0)
				{
					return this.errors[0].IsWarning;
				}
				else
				{
					return false;
				}
			}
		}

		#endregion

		#region Constructors

		public IscException() : base()
		{
		}

		public IscException(int errorCode) : base()
		{
			this.Errors.Add(IscCodes.isc_arg_gds, errorCode);
			this.BuildExceptionMessage();
		}

		public IscException(string strParam) : base()
		{
			this.Errors.Add(IscCodes.isc_arg_string, strParam);
			this.BuildExceptionMessage();
		}

		public IscException(int errorCode, int intparam) : base()
		{
			this.Errors.Add(IscCodes.isc_arg_gds, errorCode);
			this.Errors.Add(IscCodes.isc_arg_number, intparam);
			this.BuildExceptionMessage();
		}

		public IscException(int type, int errorCode, string strParam) : base()
		{
			this.Errors.Add(type, errorCode);
			this.Errors.Add(IscCodes.isc_arg_string, strParam);
			this.BuildExceptionMessage();
		}

		public IscException(int type, int errorCode, int intParam, string strParam) 
			: base()
		{
			this.Errors.Add(type, errorCode);
			this.Errors.Add(IscCodes.isc_arg_string, strParam);
			this.Errors.Add(IscCodes.isc_arg_number, intParam);
			this.BuildExceptionMessage();
		}

		#endregion

		#region Methods

		public void BuildExceptionMessage()
		{
			string resources = "FirebirdSql.Data.Common.Resources.isc_error_msg";			

			StringBuilder builder = new StringBuilder();
			ResourceManager rm = new ResourceManager(resources, Assembly.GetExecutingAssembly());

			this.errorCode = (this.Errors.Count != 0) ? this.Errors[0].ErrorCode : 0;

			for (int i = 0; i < this.Errors.Count; i++)
			{
				if (this.Errors[i].Type == IscCodes.isc_arg_gds ||
					this.Errors[i].Type == IscCodes.isc_arg_warning)
				{
					int code = this.Errors[i].ErrorCode;
					string message = null;

					try
					{
						message = rm.GetString(code.ToString());
					}
					catch
					{
						message = null;
					}
					finally
					{
						if (message == null)
						{
							message = String.Format(CultureInfo.CurrentCulture, "No message for error code {0} found.", code);
						}
					}

					ArrayList param = new ArrayList();

					int index = i + 1;

					while (index < this.Errors.Count && this.Errors[index].IsArgument)
					{
						param.Add(this.Errors[index++].StrParam);
						i++;
					}

					object[] args = (object[])param.ToArray(typeof(object));

					try
					{
						if (code == IscCodes.isc_except)
						{
							// Custom exception	add	the	first argument as error	code
							this.errorCode = Convert.ToInt32(args[0], CultureInfo.InvariantCulture);
						}
						else
						{
							if (builder.Length > 0)
							{
								builder.Append("\n");
							}

							builder.AppendFormat(CultureInfo.CurrentCulture, message, args);
						}
					}
					catch
					{
						message = String.Format(CultureInfo.CurrentCulture, "No message for error code {0} found.", code);

						builder.AppendFormat(CultureInfo.CurrentCulture, message, args);
					}
				}
			}

			// Update error	collection only	with the main error
			IscError mainError = new IscError(this.errorCode);
			mainError.Message = builder.ToString();

			this.errors = new IscErrorCollection();
			this.Errors.Add(mainError);

			// Update exception	message
			this.message = builder.ToString();
		}

		#endregion
	}
}
