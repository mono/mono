//
// Mainsoft.Web.Security.GenericRoleQueries
//
// Authors:
//      Marek Habersack (grendello@gmail.com)
//
// (C) 2007 Marek Habersack
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

#if NET_2_0
using System;
using System.Web.Security;
using System.Data;
using System.Data.Common;
using System.Collections.Generic;
using System.Text;

using Mainsoft.Web.Configuration;

namespace Mainsoft.Web.Security
{
        public partial class GenericRoleProvider
        {
		// Query builders
                string GetParamName (DbParameter[] parms, int i)
                {
                        return dbHelper.GetParamName (parms, i);
                }

		string RolesCreateRoleGetRoleNameQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT RoleName FROM aspnet_Roles WHERE ");
                        sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND LoweredRoleName = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
                        return sb.ToString ();
		}

		string RolesCreateRoleInsertRoleQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
			StringBuilder sb = new StringBuilder ("INSERT INTO aspnet_Roles (ApplicationId, RoleId, RoleName, LoweredRoleName) VALUES (");
			
			int count = parms.Length;
			for (int i = 0; i < count; i++) {
				sb.Append (dbHelper.PrepareQueryParameter (GetParamName (parms, i), bpi));
				if (i + 1 < count)
					sb.Append (",");
			}
                        sb.Append (")");

                        return sb.ToString ();
		}

		string RolesAnyUsersInRoleQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT RoleId FROM aspnet_UsersInRoles WHERE ");
                        sb.AppendFormat ("RoleId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));

                        return sb.ToString ();
		}

		string RolesDeleteRoleUsersQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("DELETE FROM aspnet_UsersInRoles WHERE ");
                        sb.AppendFormat ("RoleId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));

                        return sb.ToString ();
		}

		string RolesDeleteRoleQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("DELETE FROM aspnet_Roles WHERE ");
                        sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND RoleId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
                        return sb.ToString ();
		}

		string RolesGetAllRolesQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT RoleName FROM aspnet_Roles WHERE ");
                        sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.Append ("ORDER BY RoleName");
			
                        return sb.ToString ();
		}

		string RolesRoleExistsQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT RoleName FROM aspnet_Roles WHERE ");
                        sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
                        sb.AppendFormat ("AND LoweredRoleName = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
                        return sb.ToString ();
		}

		string GenerateInStatement (DbParameter[] parms, BackendProviderInfo bpi, int startIndex)
		{
			return GenerateInStatement (parms, bpi, startIndex, -1);
		}
		
		string GenerateInStatement (DbParameter[] parms, BackendProviderInfo bpi, int startIndex, int endIndex)
		{
			if (parms.Length <= startIndex)
				return "()";
			StringBuilder sb = new StringBuilder ("(");
			int count = endIndex != -1 ? endIndex : parms.Length;
			for (int i = startIndex; i < count; i++)
				sb.AppendFormat ("{0}{1}",
						 dbHelper.PrepareQueryParameter (GetParamName (parms, i), bpi),
						 i + 1 < count ? "," : ")");
			return sb.ToString ();
		}
		
		string RolesAddUsersToRolesGetUsersQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT UserId, LoweredUserName FROM aspnet_Users WHERE ");
                        sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND LoweredUserName in {0}", GenerateInStatement (parms, bpi, 1));
			
                        return sb.ToString ();
		}

		string RolesAddUsersToRolesCreateUserQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder (
				"INSERT INTO aspnet_Users (ApplicationId, UserId, UserName, LoweredUserName, IsAnonymous, LastActivityDate) VALUES (");
			int count = parms.Length;
			for (int i = 0; i < count; i++)
				sb.AppendFormat ("{0}{1}",
						 dbHelper.PrepareQueryParameter (GetParamName (parms, i), bpi),
						 i + 1 < count ? "," : ")");
			
                        return sb.ToString ();
		}

		string RolesAddUsersToRolesGetRolesQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT RoleId FROM aspnet_Roles WHERE ");
                        sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND LoweredRoleName in {0}", GenerateInStatement (parms, bpi, 1));
			
                        return sb.ToString ();
		}

		string RolesCountUsersQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT COUNT(*) FROM aspnet_UsersInRoles WHERE ");

			if (data == null)
				throw new ApplicationException ("Passed data is invalid");
			int startRoleIds = (int)data;
			
			sb.AppendFormat ("UserId in {0} ", GenerateInStatement (parms, bpi, 0, startRoleIds));
			sb.AppendFormat ("AND RoleId in {0}", GenerateInStatement (parms, bpi, startRoleIds));
			
                        return sb.ToString ();
		}

		string RolesAddUsersInsertUsersToRolesQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("INSERT INTO aspnet_UsersInRoles (UserId, RoleId) VALUES (");

			for (int i = 0; i < parms.Length; i += 2)
				sb.AppendFormat ("({0}{2},{1}{2}),",
						 dbHelper.PrepareQueryParameter (GetParamName (parms, i), bpi),
						 dbHelper.PrepareQueryParameter (GetParamName (parms, i + 1), bpi),
						 i);
			
                        string ret = sb.ToString ();
			return ret.Trim (',');
		}

		string RolesFindUsersInRoleQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder (
				"SELECT usr.UserName FROM aspnet_Users usr, aspnet_UsersInRoles uir " +
				"WHERE usr.UserId = uir.UserId ");
                        sb.AppendFormat ("usr.ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND uir.RoleId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.AppendFormat ("AND usr.LoweredUserName LIKE {0} ",
					 dbHelper.PrepareQueryParameter (GetParamName (parms, 2), bpi));
			sb.Append ("ORDER BY usr.UserName");
			
                        return sb.ToString ();
		}

		string RolesGetRolesForUserQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder (
				"SELECT rol.RoleName FROM aspnet_Roles rol, aspnet_UsersInRoles uir WHERE rol.RoleId = uir.RoleId ");
                        sb.AppendFormat ("rol.ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND uir.UserId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.Append ("ORDER BY rol.RoleName");
			
                        return sb.ToString ();
		}

		string RolesGetUsersInRolesQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder (
				"SELECT usr.UserName FROM aspnet_Users usr, aspnet_UsersInRoles uir WHERE usr.UserId = uir.UserId ");
                        sb.AppendFormat ("usr.ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND uir.RoleId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			sb.Append ("ORDER BY usr.UserName");
			
                        return sb.ToString ();
		}

		string RolesIsUserInRoleQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT UserId FROM aspnet_UsersInRoles WHERE ");
                        sb.AppendFormat ("UserId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND RoleId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
                        return sb.ToString ();
		}

		string RolesRemoveUsersFromRolesSelUsersQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT UserId FROM aspnet_Users WHERE ");
                        sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND LoweredUserName in {0} ", GenerateInStatement (parms, bpi, 1));
			
                        return sb.ToString ();
		}

		string RolesRemoveUsersFromRolesSelRolesQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT RoleId FROM aspnet_Roles WHERE ");
                        sb.AppendFormat ("ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND LoweredRoleName in {0} ", GenerateInStatement (parms, bpi, 1));
			
                        return sb.ToString ();
		}

		string RolesRemoveUsersFromRolesDeleteQueryBuilder (DbParameter[] parms, object data)
		{
			if (data == null)
				throw new ApplicationException ("Passed data is invalid");
			
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("DELETE FROM aspnet_UsersInRoles WHERE ");
			int startRoleIds = (int)data;
			
			sb.AppendFormat ("UserId in {0} ", GenerateInStatement (parms, bpi, 0, startRoleIds));
			sb.AppendFormat ("AND RoleId in {0}", GenerateInStatement (parms, bpi, startRoleIds));
                        return sb.ToString ();
		}

		string RolesGetRoleIdQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT RoleId FROM aspnet_Roles WHERE ");
			
                        sb.AppendFormat ("LoweredRoleName = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
                        return sb.ToString ();
		}

		string RolesGetUserIdQueryBuilder (DbParameter[] parms, object data)
		{
			BackendProviderInfo bpi = dbHelper.GetBackendProviderInfo ();
                        StringBuilder sb = new StringBuilder ("SELECT UserId FROM aspnet_Users WHERE ");
			
                        sb.AppendFormat ("LoweredUserName = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 0), bpi));
			sb.AppendFormat ("AND ApplicationId = {0} ", dbHelper.PrepareQueryParameter (GetParamName (parms, 1), bpi));
			
                        return sb.ToString ();
		}
	}
}
#endif
