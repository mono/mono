﻿<?xml version="1.0"?>
<!-- 
    Note: As an alternative to hand editing this file you can use the 
    web admin tool to configure settings for your application. Use
    the Website->Asp.Net Configuration option in Visual Studio.
    A full list of settings and comments can be found in 
    machine.config.comments usually located in 
    \Windows\Microsoft.Net\Framework\v2.x\Config 
-->
<configuration>
	<configSections>
		<sectionGroup name="system.web.extensions" type="System.Web.Configuration.SystemWebExtensionsSectionGroup, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35">
			<sectionGroup name="scripting" type="System.Web.Configuration.ScriptingSectionGroup, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35">
				<section name="scriptResourceHandler" type="System.Web.Configuration.ScriptingScriptResourceHandlerSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" allowDefinition="MachineToApplication"/>
				<sectionGroup name="webServices" type="System.Web.Configuration.ScriptingWebServicesSectionGroup, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35">
					<section name="jsonSerialization" type="System.Web.Configuration.ScriptingJsonSerializationSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" allowDefinition="Everywhere"/>
					<section name="profileService" type="System.Web.Configuration.ScriptingProfileServiceSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" allowDefinition="MachineToApplication"/>
					<section name="authenticationService" type="System.Web.Configuration.ScriptingAuthenticationServiceSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" allowDefinition="MachineToApplication"/>
					<section name="roleService" type="System.Web.Configuration.ScriptingRoleServiceSection, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" requirePermission="false" allowDefinition="MachineToApplication"/>
				</sectionGroup>
			</sectionGroup>
		</sectionGroup>
	</configSections>
	<appSettings>
		<add key="OutsideTestSuite" value="false"/>
	</appSettings>
	<connectionStrings/>
	<system.web>
		<!-- 
            Set compilation debug="true" to insert debugging 
            symbols into the compiled page. Because this 
            affects performance, set this value to true only 
            during development.
    -->
		<compilation debug="true">
			<assemblies>
				<add assembly="System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089"/>
				<add assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
				<add assembly="System.Web.Abstractions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
				<add assembly="System.Web.Routing, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
				<add assembly="System.ComponentModel.DataAnnotations, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
				<add assembly="System.Web.DynamicData, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
				<add assembly="System.Data.DataSetExtensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089"/>
				<add assembly="System.Xml.Linq, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089"/>
				<add assembly="System.Data.Linq, Version=3.5.0.0, Culture=neutral, PublicKeyToken=B77A5C561934E089"/>
			</assemblies>
		</compilation>
		<!--
            The <authentication> section enables configuration 
            of the security authentication mode used by 
            ASP.NET to identify an incoming user. 
    -->
		<authentication mode="Windows"/>
		<!--
            The <customErrors> section enables configuration 
            of what to do if/when an unhandled error occurs 
            during the execution of a request. Specifically, 
            it enables developers to configure html error pages 
            to be displayed in place of a error stack trace.

        <customErrors mode="RemoteOnly" defaultRedirect="GenericErrorPage.htm">
            <error statusCode="403" redirect="NoAccess.htm" />
            <error statusCode="404" redirect="FileNotFound.htm" />
        </customErrors>
    -->
		<pages>
			<controls>
				<add tagPrefix="asp" namespace="System.Web.UI" assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
				<add tagPrefix="asp" namespace="System.Web.UI.WebControls" assembly="System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
				<add tagPrefix="asp" namespace="System.Web.DynamicData" assembly="System.Web.DynamicData, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
				<add tagPrefix="test" namespace="MonoTests.DataSource" assembly="System.Web.DynamicData_test_net_4_x"/>
				<add tagPrefix="test" namespace="MonoTests.Common" assembly="System.Web.DynamicData_test_net_4_x"/>
			</controls>
		</pages>
		<httpHandlers>
			<remove verb="*" path="*.asmx"/>
			<add verb="*" path="*.asmx" validate="false" type="System.Web.Script.Services.ScriptHandlerFactory, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
			<add verb="*" path="*_AppService.axd" validate="false" type="System.Web.Script.Services.ScriptHandlerFactory, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
			<add verb="GET,HEAD" path="ScriptResource.axd" type="System.Web.Handlers.ScriptResourceHandler, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35" validate="false"/>
		</httpHandlers>
		<httpModules>
			<add name="ScriptModule" type="System.Web.Handlers.ScriptModule, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
			<add name="UrlRoutingModule" type="System.Web.Routing.UrlRoutingModule, System.Web.Routing, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
		</httpModules>
	</system.web>
	<system.codedom>
		<compilers>
			<compiler language="c#;cs;csharp" extension=".cs" warningLevel="4" type="Microsoft.CSharp.CSharpCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
				<providerOption name="CompilerVersion" value="v3.5"/>
				<providerOption name="WarnAsError" value="false"/>
			</compiler>
			<compiler language="vb;vbs;visualbasic;vbscript" extension=".vb" warningLevel="4" type="Microsoft.VisualBasic.VBCodeProvider, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089">
				<providerOption name="CompilerVersion" value="v3.5"/>
				<providerOption name="OptionInfer" value="true"/>
				<providerOption name="WarnAsError" value="false"/>
			</compiler>
		</compilers>
	</system.codedom>
	<system.web.extensions/>
	<!-- 
        The system.webServer section is required for running ASP.NET AJAX under Internet
        Information Services 7.0.  It is not necessary for previous version of IIS.
  -->
	<system.webServer>
		<validation validateIntegratedModeConfiguration="false"/>
		<modules runAllManagedModulesForAllRequests="true">
			<remove name="ScriptModule"/>
			<remove name="UrlRoutingModule"/>
			<add name="ScriptModule" preCondition="managedHandler" type="System.Web.Handlers.ScriptModule, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
			<add name="UrlRoutingModule" type="System.Web.Routing.UrlRoutingModule, System.Web.Routing, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
		</modules>
		<handlers>
			<remove name="WebServiceHandlerFactory-Integrated"/>
			<remove name="ScriptHandlerFactory"/>
			<remove name="ScriptHandlerFactoryAppServices"/>
			<remove name="ScriptResource"/>
			<add name="ScriptHandlerFactory" verb="*" path="*.asmx" preCondition="integratedMode" type="System.Web.Script.Services.ScriptHandlerFactory, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
			<add name="ScriptHandlerFactoryAppServices" verb="*" path="*_AppService.axd" preCondition="integratedMode" type="System.Web.Script.Services.ScriptHandlerFactory, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
			<add name="ScriptResource" preCondition="integratedMode" verb="GET,HEAD" path="ScriptResource.axd" type="System.Web.Handlers.ScriptResourceHandler, System.Web.Extensions, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31BF3856AD364E35"/>
			<add name="UrlRoutingHandler" preCondition="integratedMode" verb="*" path="UrlRouting.axd" type="System.Web.HttpForbiddenHandler, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"/>
		</handlers>
	</system.webServer>
	<runtime>
		<assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
			<dependentAssembly>
				<assemblyIdentity name="System.Web.Extensions" publicKeyToken="31bf3856ad364e35"/>
				<bindingRedirect oldVersion="1.0.0.0-1.1.0.0" newVersion="3.5.0.0"/>
			</dependentAssembly>
			<dependentAssembly>
				<assemblyIdentity name="System.Web.Extensions.Design" publicKeyToken="31bf3856ad364e35"/>
				<bindingRedirect oldVersion="1.0.0.0-1.1.0.0" newVersion="3.5.0.0"/>
			</dependentAssembly>
		</assemblyBinding>
	</runtime>
</configuration>
