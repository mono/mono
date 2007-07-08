using System;
using System.Web.Configuration;

public class ConfigSectionSample
{
    public static void GetAuthServiceSection()
    {
        // Get the Web application configuration.
        System.Configuration.Configuration configuration =
            WebConfigurationManager.OpenWebConfiguration("/aspnetTest");

        // Get the external Web services section.
        ScriptingWebServicesSectionGroup webServicesSection =
            (ScriptingWebServicesSectionGroup)configuration.GetSectionGroup(
            "system.web.extensions/scripting/webServices");

        // Get the authentication service section.
        ScriptingAuthenticationServiceSection authenticationSection =
            webServicesSection.AuthenticationService;
    }

    public static void GetProfileServiceSection()
    {
        // Get the Web application configuration.
        System.Configuration.Configuration configuration =
            WebConfigurationManager.OpenWebConfiguration("/aspnetTest");

        // Get the external Web services section.
        ScriptingWebServicesSectionGroup webServicesSection =
            (ScriptingWebServicesSectionGroup)configuration.GetSectionGroup(
            "system.web.extensions/scripting/webServices");

        // Get the profile service section.
        ScriptingProfileServiceSection profileSection =
            webServicesSection.ProfileService;
    }

    public static void GetConverterElement()
    {
        // Get the Web application configuration.
        System.Configuration.Configuration configuration =
            WebConfigurationManager.OpenWebConfiguration("/aspnetTest");

        // Get the external JSON section.
        ScriptingJsonSerializationSection jsonSection =
            (ScriptingJsonSerializationSection)configuration.GetSection(
            "system.web.extensions/scripting/webServices/jsonSerialization");

        //Get the converters collection.
        ConvertersCollection converters =
            jsonSection.Converters;

        if ((converters != null) && converters.Count > 0)
        {
            // Get the first registered converter.
            Converter converterElement = converters[0];
        }
    }
}
