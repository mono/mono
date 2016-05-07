//------------------------------------------------------------------------------
// <copyright file="ProfileBuildProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/************************************************************************************************************/

/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
namespace System.Web.Compilation {
    using System;
    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Text;
    using System.Globalization;
    using System.Resources;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Web.Caching;
    using System.Web.UI;
    using System.Web.Security;
    using System.Web.Profile;
    using System.Configuration;
    using System.Web.Hosting;

    internal class ProfileBuildProvider: BuildProvider {

        private const string ProfileTypeName = "ProfileCommon";

        private ProfileBuildProvider() { }

        internal static ProfileBuildProvider Create() {
            ProfileBuildProvider buildProvider = new ProfileBuildProvider();

            // Use a fake virtual path of /theapp/profile for the profile build provider
            buildProvider.SetVirtualPath(HttpRuntime.AppDomainAppVirtualPathObject.SimpleCombine("Profile"));

            return buildProvider;
        }

        internal static bool HasCompilableProfile {
            get {
                if (!ProfileManager.Enabled)
                    return false;

                if (ProfileBase.GetPropertiesForCompilation().Count == 0 && !ProfileBase.InheritsFromCustomType && ProfileManager.DynamicProfileProperties.Count == 0) {
                    return false;
                }
                return true;
            }
        }

        internal static Type GetProfileTypeFromAssembly(Assembly assembly, bool isPrecompiledApp) {

            if (!HasCompilableProfile)
                return null;

            Type t = assembly.GetType(
                /*BaseCodeDomTreeGenerator.defaultNamespace + "." + */ ProfileTypeName);

            if (t == null) {
                // If this is a precompiled app and Profile is on, it should always
                // have been precompiled.  If not, then Profile might have been turned
                // on *after* precompiling the app, which is not allowed (VSWhidbey 114884).
                if (isPrecompiledApp) {
                    throw new HttpException(SR.GetString(SR.Profile_not_precomped));
                }

                // We should always find the type
                Debug.Assert(false);
            }

            return t;
        }


        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////

        // BuildProvider implementation

        public override void GenerateCode(AssemblyBuilder assemblyBuilder) {
            Hashtable           properties   = ProfileBase.GetPropertiesForCompilation();
            CodeCompileUnit     compileUnit  = new CodeCompileUnit();
            Hashtable           groups       = new Hashtable();
            Type                baseType     = Type.GetType(ProfileBase.InheritsFromTypeString, false);

            // namespace ASP {
            //
            CodeNamespace ns = new CodeNamespace();
            // ns.Name = BaseCodeDomTreeGenerator.defaultNamespace;

            //GEN: using System;
            ns.Imports.Add(new CodeNamespaceImport("System"));
            //GEN: using System.Web;
            ns.Imports.Add(new CodeNamespaceImport("System.Web"));
            //GEN: using System.Web.Profile;
            ns.Imports.Add(new CodeNamespaceImport("System.Web.Profile"));

            // class Profile :  System.Web.Security.ProfileBase {
            //
            CodeTypeDeclaration type = new CodeTypeDeclaration();
            type.Name = ProfileTypeName;
            if (baseType != null) {
                type.BaseTypes.Add(new CodeTypeReference(baseType));
                assemblyBuilder.AddAssemblyReference(baseType.Assembly, compileUnit);
            } else {
                type.BaseTypes.Add(new CodeTypeReference(ProfileBase.InheritsFromTypeString));
                ProfileSection config = MTConfigUtil.GetProfileAppConfig();
                if (config != null) {
                    PropertyInformation prop = config.ElementInformation.Properties["inherits"];
                    if (prop != null && prop.Source != null && prop.LineNumber > 0)
                        type.LinePragma = new CodeLinePragma(HttpRuntime.GetSafePath(prop.Source), prop.LineNumber);
                }
            }
            // tell the assemblyBuilder to generate a fast factory for this type
            assemblyBuilder.GenerateTypeFactory(/*ns.Name + "." + */ ProfileTypeName);

            foreach(DictionaryEntry de in properties)
            {
                ProfileNameTypeStruct property = (ProfileNameTypeStruct)de.Value;
                if (property.PropertyType != null)
                    assemblyBuilder.AddAssemblyReference(property.PropertyType.Assembly, compileUnit);
                int pos = property.Name.IndexOf('.');
                if (pos < 0) {
                    // public string Color { get { return (string) GetProperty("Color"); } set { SetProperty("Color", value); } }
                    CreateCodeForProperty(assemblyBuilder, type, property);
                } else {
                    string grpName = property.Name.Substring(0, pos);
                    if (!assemblyBuilder.CodeDomProvider.IsValidIdentifier(grpName))
                        throw new ConfigurationErrorsException(SR.GetString(SR.Profile_bad_group, grpName), property.FileName, property.LineNumber);
                    if (groups[grpName] == null) {
                        groups.Add(grpName, property.Name);
                    } else {
                        groups[grpName] = ((string)groups[grpName]) + ";" + property.Name;
                    }
                }
            }

            foreach(DictionaryEntry de in groups) {
                // public ProfileGroupFooClass Foo { get { return ProfileGroupSomething; }}
                //
                // public class ProfileGroupFoo : ProfileGroup {
                //      Properties
                // }
                AddPropertyGroup(assemblyBuilder, (string) de.Key, (string) de.Value, properties, type, ns);
            }


            // public ASP.Profile GetProfileForUser(string username) {
            //      return (ASP.Profile) this.GetUserProfile(username);
            // }
            AddCodeForGetProfileForUser(type);

            // }
            //
            ns.Types.Add(type);
            compileUnit.Namespaces.Add(ns);

            assemblyBuilder.AddCodeCompileUnit(this, compileUnit);
        }

        // End of BuildProvider implementation

        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
        private void CreateCodeForProperty(AssemblyBuilder assemblyBuilder, CodeTypeDeclaration type, ProfileNameTypeStruct property)
        {
            string name = property.Name;
            int    pos = name.IndexOf('.');
            if (pos > 0)
                name = name.Substring(pos+1);
            if (!assemblyBuilder.CodeDomProvider.IsValidIdentifier(name))
                throw new ConfigurationErrorsException(SR.GetString(SR.Profile_bad_name), property.FileName, property.LineNumber);
            // e.g.: public string Color {
            //                       get { return (string) GetProperty("Color"); }
            //                       set { SetProperty("Color", value); } }


            // public  property.Type property.name {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Name       = name;
            prop.Attributes = MemberAttributes.Public;
            prop.HasGet     = true;
            prop.Type       = property.PropertyCodeRefType;

            ////////////////////////////////////////////////////////////
            // Get statements
            // get { return (property.type) GetProperty(property.name); }
            CodeMethodInvokeExpression  cmie;
            CodeMethodReturnStatement   getLine;

            cmie = new CodeMethodInvokeExpression();
            cmie.Method.TargetObject = new CodeThisReferenceExpression();
            cmie.Method.MethodName = "GetPropertyValue";
            cmie.Parameters.Add(new CodePrimitiveExpression(name));
            getLine = new CodeMethodReturnStatement(new CodeCastExpression(prop.Type, cmie));

            prop.GetStatements.Add(getLine);

            if (!property.IsReadOnly)
            {

                ////////////////////////////////////////////////////////////
                // Set statements
                // set { SetProperty(property.name, value); }
                CodeMethodInvokeExpression   setLine;

                setLine = new CodeMethodInvokeExpression();
                setLine.Method.TargetObject = new CodeThisReferenceExpression();
                setLine.Method.MethodName = "SetPropertyValue";
                setLine.Parameters.Add(new CodePrimitiveExpression(name));
                setLine.Parameters.Add(new CodePropertySetValueReferenceExpression());
                prop.HasSet = true;
                prop.SetStatements.Add(setLine);
            }
            //prop.LinePragma = new CodeLinePragma(HttpRuntime.GetSafePath(property.FileName), property.LineNumber);
            type.Members.Add(prop);
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private void AddPropertyGroup(AssemblyBuilder assemblyBuilder, string groupName, string propertyNames, Hashtable  properties,
                                      CodeTypeDeclaration type, CodeNamespace ns) {


            // e.g.: public string Foo {
            //                       get { return (ProfileGroupFooClass) GetProfileGroup("Foo"); } }

            // public  property.Type property.name {
            CodeMemberProperty prop = new CodeMemberProperty();
            prop.Name       = groupName;
            prop.Attributes = MemberAttributes.Public;
            prop.HasGet     = true;
            prop.Type       = new CodeTypeReference("ProfileGroup" + groupName);

            ////////////////////////////////////////////////////////////
            // Get statements
            // get { return (property.type) GetProperty(property.name); }
            CodeMethodInvokeExpression  cmie;
            CodeMethodReturnStatement   getLine;

            cmie = new CodeMethodInvokeExpression();
            cmie.Method.TargetObject = new CodeThisReferenceExpression();
            cmie.Method.MethodName = "GetProfileGroup";
            cmie.Parameters.Add(new CodePrimitiveExpression(prop.Name));
            getLine = new CodeMethodReturnStatement(new CodeCastExpression(prop.Type, cmie));

            prop.GetStatements.Add(getLine);
            type.Members.Add(prop);

            // public class ProfileGroupFooClass : ProfileGroupBase {
            CodeTypeDeclaration grpType = new CodeTypeDeclaration();
            grpType.Name = "ProfileGroup" + groupName;
            grpType.BaseTypes.Add(new CodeTypeReference(typeof(ProfileGroupBase)));

            string [] grpProps = propertyNames.Split(';');
            foreach(string grpProp in grpProps) {
                // public string Color {
                //                       get { return (string) GetProperty("Color"); }
                //                       set { SetProperty("Color", value); } }
                CreateCodeForProperty(assemblyBuilder, grpType, (ProfileNameTypeStruct)properties[grpProp]);
            }
            ns.Types.Add(grpType);
        }


        private void AddCodeForGetProfileForUser(CodeTypeDeclaration type) {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name             = "GetProfile";
            method.Attributes       = MemberAttributes.Public;
            method.ReturnType       = new CodeTypeReference(/*BaseCodeDomTreeGenerator.defaultNamespace + "." + */ ProfileTypeName);
            method.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), "username"));

            CodeMethodInvokeExpression  cmie = new CodeMethodInvokeExpression();
            cmie.Method.TargetObject = new CodeTypeReferenceExpression("ProfileBase");
            cmie.Method.MethodName = "Create";
            cmie.Parameters.Add(new CodeArgumentReferenceExpression("username"));

            CodeMethodReturnStatement returnSatement = new CodeMethodReturnStatement(new CodeCastExpression(method.ReturnType, cmie));
            ProfileSection config = MTConfigUtil.GetProfileAppConfig();
            //if (config != null)
            //{
            //    PropertyInformation prop = config.ElementInformation.Properties["inherits"];
            //    if (prop != null && prop.Source != null && prop.LineNumber > 0)
            //        returnSatement.LinePragma = new CodeLinePragma(HttpRuntime.GetSafePath(prop.Source), prop.LineNumber);
            //}

            method.Statements.Add(returnSatement);
            type.Members.Add(method);
        }
    }
}
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////////////
