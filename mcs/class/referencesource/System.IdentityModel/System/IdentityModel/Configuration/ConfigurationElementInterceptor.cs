using System.Configuration;
using System.Text;
using System.Xml;

namespace System.IdentityModel.Configuration
{
#pragma warning disable 1591
    public partial class ConfigurationElementInterceptor : ConfigurationElement
    {
        private XmlDocument elementXml;

        protected override void DeserializeElement( XmlReader reader, bool serializeCollectionKey )
        {
            elementXml = new XmlDocument();
            elementXml.LoadXml( reader.ReadOuterXml() );

            // Create a new XmlTextReader so this element can be loaded
            // by the framework.
            using ( XmlReader newReader = XmlDictionaryReader.CreateTextReader( Encoding.UTF8.GetBytes( elementXml.DocumentElement.OuterXml ), XmlDictionaryReaderQuotas.Max ) )
            {
                newReader.Read();
                base.DeserializeElement( newReader, serializeCollectionKey );
            }
        }

        // There are parts in the configuration where users can specify arbitrary elements and attributes.
        // For example, when loading a custom token handler. The interceptor is implemented to 
        // specifically handle these cases. So return true when the Framework detects a unrecognized element
        // or attribute to keep the parser running.
        protected override bool OnDeserializeUnrecognizedAttribute( string name, string value )
        {
            return true;
        }

        protected override bool OnDeserializeUnrecognizedElement( string elementName, XmlReader reader )
        {
            return true;
        }

        //
        // The Reset method is called in the nested vdir scenario,
        // where the child inherits the parent's config section.
        // The sequence of calls is as follows:
        // 1. Application accesses the section in the child app
        // 2. The config system walks up the inheritance chain and finds that it can instantiate the section at the parent level.
        // 3. The config system populates the section with the values from the parent, including setting the custom XML property.
        // 4. Now, the config system tries to instantiage the section at the child level. It creates a brand new instance of the section.
        // 5. The config system takes the parent section as a template and uses it to initialize the child (by calling this Reset method).
        // 6. Then the config system populates the child with values that were overwritten at the child level.
        //
        protected override void Reset( ConfigurationElement parentElement )
        {
            base.Reset( parentElement );
            Reset( (ConfigurationElementInterceptor)parentElement );
        }

        public XmlElement ElementAsXml
        {
            get
            {
                if ( elementXml != null )
                {
                    return elementXml.DocumentElement;
                }

                return null;
            }
        }

        public XmlNodeList ChildNodes
        {
            get
            {
                if ( ( elementXml != null ) && ( ElementAsXml.ChildNodes.Count != 0 ) )
                {
                    return ElementAsXml.ChildNodes;
                }

                return null;
            }
        }

        //
        // Copy custom properties from parent level.
        //
        private void Reset( ConfigurationElementInterceptor parentElement )
        {
            this.elementXml = parentElement.elementXml;
        }
    }
#pragma warning restore 1591
}
