/* 
** 1. compile using:
**    gmcs dumper.cs -r:System.dll -r:System.Web.dll -r:System.Web.Services.dll -r:System.Configuration.dll
** 
** 2. run it on windows:
**    > dumper.exe > dumper.out.microsoft
** 
** 3. transfer that file to linux and do:
**    $ dos2unix dumper.out.microsoft && sort dumper.out.microsoft > foo && mv foo dumper.out.microsoft
** 
** 4. run dumper on linux:
**    $ mono dumper.exe > dumper.out.linux && sort dumper.out.linux > foo && mv foo dumper.out.linux
** 
** 5. diff the results:
**    $ diff -u dumper.out.linux dumper.out.microsoft | grep ^[-+]
*/

using System;

using System.Collections;
using System.Configuration;
using System.Net;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Services.Configuration;

public class Dumper {
	object[] things = {
		new AnonymousIdentificationSection (),
		new AssemblyCollection (),
		new AssemblyInfo ("foo"),
		new AuthenticationSection (),
		new AuthorizationRuleCollection (),
		new AuthorizationRule (AuthorizationRuleAction.Allow),
		new AuthorizationSection (),
		new BufferModesCollection (),
		new BufferModeSettings ("name", 10, 10, 10, TimeSpan.FromMinutes (1), TimeSpan.FromMinutes(1), 10),
		new BuildProviderCollection (),
		new BuildProvider (".cs", "CSharpProvider"),
		new CacheSection (),
		new ClientTargetCollection (),
		new ClientTarget ("alias", "userAgent"),
		new ClientTargetSection (),
		new CodeSubDirectoriesCollection (),
		new CodeSubDirectory ("dirname"),
		new CompilationSection (),
		new CompilerCollection (),
		new Compiler ("options", "ext", "lang", "type", 1),
		new CustomErrorCollection (),
		new CustomError (404, "redirect"),
		new CustomErrorsSection (),
		new DeploymentSection (),
		new EventMappingSettingsCollection (),
		new EventMappingSettings ("name", "type"),
		new ExpressionBuilderCollection (),
		new ExpressionBuilder ("prefix", "type"),
		new FormsAuthenticationConfiguration (),
		new FormsAuthenticationCredentials (),
		new FormsAuthenticationUserCollection (),
		new FormsAuthenticationUser ("name", "password"),
		new GlobalizationSection (),
		new HealthMonitoringSection (),
		new HostingEnvironmentSection (),
		new HttpCookiesSection (),
		new HttpHandlerActionCollection (),
		new HttpHandlerAction ("path", "type", "verb"),
		new HttpHandlersSection (),
		new HttpModuleActionCollection (),
		new HttpModuleAction ("name", "type"),
		new HttpModulesSection (),
		new HttpRuntimeSection (),
		new IdentitySection (),
		new MachineKeySection (),
		new MembershipSection (),
		new NamespaceCollection (),
		new NamespaceInfo ("name"),
		new OutputCacheProfileCollection (),
		new OutputCacheProfile ("name"),
		new OutputCacheSection (),
		new OutputCacheSettingsSection (),
		new PagesSection (),
		new PassportAuthentication (),
		new ProcessModelSection (),
		new ProfileGroupSettingsCollection (),
		new ProfileGroupSettings ("name"),
		new ProfilePropertySettingsCollection (),
		new ProfilePropertySettings ("name"),
		new ProfileSettingsCollection (),
		new ProfileSettings ("name"),
		new RoleManagerSection (),
		new RootProfilePropertySettingsCollection (),
		new RuleSettingsCollection (),
		new RuleSettings ("name", "event", "provider", "profile", 1, 1, TimeSpan.FromMinutes (5), "custom"),
		new SecurityPolicySection (),
		new SessionPageStateSection (),
		new SessionStateSection (),
		new SqlCacheDependencyDatabaseCollection (),
		new SqlCacheDependencyDatabase ("name", "connectionStringName"),
		new SqlCacheDependencySection (),
		new TagMapCollection (),
		new TagMapInfo ("tagType", "mappedTag"),
		new TagPrefixCollection (),
		new TagPrefixInfo ("tagPrefix", "nameSpace", "assembly", "tagName", "source"),
		new TraceSection (),
		new TransformerInfoCollection (),
		new TransformerInfo ("name", "type"),
		new TrustLevelCollection (),
		new TrustLevel ("name", "policyFile"),
		new TrustSection (),
		new UrlMappingCollection (),
		new UrlMapping ("~/url", "~/mappedUrl"), /* UrlMapping uses a callback validator to validate the url(s) */
		new UrlMappingsSection (),
		new VirtualDirectoryMappingCollection (),
		//		new VirtualDirectoryMapping ("\\phys\\dir", true),  /* PhysicalDirectory seems to validate its input without using a Validator (decorator, anyway..)*/
		new WebControlsSection (),
		new WebPartsPersonalizationAuthorization (),
		new WebPartsPersonalization (),
		new WebPartsSection (),
		new XhtmlConformanceSection (),

		/* System.Configuration stuff */
		new AppSettingsSection (),
		new IgnoreSection (),
		new ConnectionStringSettings (),
		new ConnectionStringSettingsCollection (),
		new ProtectedProviderSettings (),

		/* System.Web.Services stuff */
		new DiagnosticsElement (),
		new ProtocolElementCollection (),
		new ProtocolElement (),
		new SoapEnvelopeProcessingElement (),
		new SoapExtensionTypeElementCollection (),
		new SoapExtensionTypeElement (),
		new TypeElementCollection (),
		new TypeElement (),
		new WebServicesSection (),
		new WsdlHelpGeneratorElement (),
		new WsiProfilesElementCollection (),
		new WsiProfilesElement (),

		/* System.Net stuff */
		//new AuthenticationModuleElement (),
		//new AuthenticationModuleElementCollection (),
		//new AuthenticationModulesSection (),
		//new BypassElement (),
		//new BypassElementCollection (),
		//new ConnectionManagementElement (),
		//new ConnectionManagementElementCollection (),
		//new ConnectionManagementSection (),
		//new DefaultProxySection (),
		//new HttpCachePolicyElement (),
		//new Ipv6Element (),
		//new ModuleElement (),
		//new ModuleElementCollection (),
	};

	public Dumper ()
	{
	}

	public void DumpProperties ()
	{
		foreach (object o in things) {
			Type type = o.GetType ();
			PropertyInfo pinfo = type.GetProperty ("Properties", BindingFlags.Instance | BindingFlags.NonPublic);

			if (pinfo == null)
				continue;

			ConfigurationPropertyCollection col = (ConfigurationPropertyCollection)pinfo.GetValue (o, null);
			foreach (ConfigurationProperty p in col) {
				Console.WriteLine ("{0}.{1} (Type) = {2}", type, p.Name, p.Type);
				if (p.Validator != null && p.Validator.GetType () != typeof (DefaultValidator))
					Console.WriteLine ("{0}.{1} (Validator) = {2} ", type, p.Name, p.Validator);
			}
		}
	}

	public void DumpConverters ()
	{
		foreach (object o in things) {
			Type type = o.GetType ();
			PropertyInfo pinfo = type.GetProperty ("Properties", BindingFlags.Instance | BindingFlags.NonPublic);

			if (pinfo == null)
				continue;

			ConfigurationPropertyCollection col = (ConfigurationPropertyCollection)pinfo.GetValue (o, null);
			foreach (ConfigurationProperty p in col) {
				if (p.Converter != null)
					Console.WriteLine ("{0}.{1} (Converter) = {2} ", type, p.Name, p.Converter);
			}
		}
	}

	public void DumpElementProperties ()
	{
		foreach (object o in things) {
			Type type = o.GetType ();
			PropertyInfo pinfo = type.GetProperty ("ElementProperty", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			if (pinfo == null)
				continue;

			ConfigurationElementProperty p = (ConfigurationElementProperty)pinfo.GetValue (o, null);
			if (p == null)
				Console.WriteLine ("{0}.ElementProperty = null", type);
			else if (p.Validator == null)
				Console.WriteLine ("{0}.ElementProperty (Validator) = null", type);
			else
				Console.WriteLine ("{0}.ElementProperty (Validator) = {1} ({2})", type, p.Validator, p.Validator.CanValidate (type));
		}
	}

	public void DumpRuntimeObjects ()
	{
		foreach (object o in things) {
			Type type = o.GetType ();
			MethodInfo minfo = type.GetMethod ("GetRuntimeObject", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

			if (minfo == null)
				continue;

			object ro = minfo.Invoke (o, null);

			if (ro == null)
				Console.WriteLine ("{0}.GetRuntimeObject() = null", type);
			else
				Console.WriteLine ("{0}.GetRuntimeObject() = {1} ({2})", type, ro.GetType(), ro == o);
			if (ro.GetType() == typeof (Hashtable)) {
				foreach (object key in ((Hashtable)ro).Keys)
					Console.WriteLine (" hash[{0}] = {1}", key, ((Hashtable)ro)[key]);
			}
		}
	}

	public static void Main (string[] args) {
		Dumper d = new Dumper ();

		try {
			d.DumpProperties ();
			d.DumpConverters ();
			d.DumpElementProperties ();
			d.DumpRuntimeObjects ();
		}
		catch (Exception e) {
			Console.WriteLine (e);
		}
	}

}
