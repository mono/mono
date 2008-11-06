// NpgsqlTypes.NpgsqlDbType.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

namespace NpgsqlTypes
{
	public enum NpgsqlDbType
	{
		// This list used to be ordered. But this ordering would break compiled applications
		// as enum values would change after each insertion. Now, just append new types.

		// Binary or with other values. E.g. Array of Box is NpgsqlDbType.Array | NpgsqlDbType.Box

		Array = int.MinValue,

		Bigint = 1,

		Boolean,
		Box,
		Bytea,
		Circle,
		Char,
		Date,
		Double,
		Integer,
		Line,
		LSeg,
		Money,
		Numeric,
		Path,
		Point,
		Polygon,
		Real,
		Smallint,
		Text,
		Time,
		Timestamp,
		Varchar,
		Refcursor,
		Inet,
		Bit,
		TimestampTZ,
		Uuid,
		Xml,
		Oidvector,
		Interval,
		TimeTZ
	}
}