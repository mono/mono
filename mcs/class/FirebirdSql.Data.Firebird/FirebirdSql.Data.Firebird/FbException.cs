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
using System.ComponentModel;
using System.Security.Permissions;
using FirebirdSql.Data.Common;

namespace FirebirdSql.Data.Firebird
{
	/// <include file='Doc/en_EN/FbException.xml' path='doc/class[@name="FbException"]/overview/*'/>
#if	(!NETCF)
	[Serializable]
#endif
	public sealed class FbException : SystemException
	{
		#region Fields

		private FbErrorCollection errors = new FbErrorCollection();
		private int errorCode;

		#endregion

		#region Properties

		/// <include file='Doc/en_EN/FbException.xml' path='doc/class[@name="FbException"]/property[@name="Errors"]/*'/>
#if	(!NETCF)
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
#endif
		public FbErrorCollection Errors
		{
			get { return this.errors; }
		}

		/// <include file='Doc/en_EN/FbException.xml' path='doc/class[@name="FbException"]/property[@name="ErrorCode"]/*'/>
		public int ErrorCode
		{
			get { return this.errorCode; }
		}

		#endregion

		#region Constructors

		internal FbException() : base()
		{
		}

		internal FbException(string message) : base(message)
		{
		}

		internal FbException(string message, IscException ex) : base(message)
		{
			this.errorCode = ex.ErrorCode;
#if	(!NETCF)
			this.Source = ex.Source;
#endif

			this.GetIscExceptionErrors(ex);
		}

#if	(!NETCF)

		internal FbException(
            System.Runtime.Serialization.SerializationInfo info, 
            System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
			this.errors = (FbErrorCollection)info.GetValue("errors", typeof(FbErrorCollection));
			this.errorCode = info.GetInt32("errorCode");
		}

#endif

		#endregion

		#region Methods

#if	(!NETCF)

		/// <include file='Doc/en_EN/FbException.xml' path='doc/class[@name="FbException"]/method[@name="GetObjectData(SerializationInfo, StreamingContext)"]/*'/>
		[SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
		public override void GetObjectData(
            System.Runtime.Serialization.SerializationInfo info, 
            System.Runtime.Serialization.StreamingContext context)
		{
			info.AddValue("errors", this.errors);
			info.AddValue("errorCode", this.errorCode);

			base.GetObjectData(info, context);
		}

#endif

		#endregion

		#region Internal Methods

		internal void GetIscExceptionErrors(IscException ex)
		{
			foreach (IscError error in ex.Errors)
			{
				this.errors.Add(error.Message, error.ErrorCode);
			}
		}

		#endregion
	}
}
