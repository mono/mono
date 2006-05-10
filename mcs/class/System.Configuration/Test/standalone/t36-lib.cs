using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.IO;

namespace FrontDesk.Testing.Configuration {
        public class TestCenterSection : ConfigurationSection {

                [ConfigurationProperty ("sources")]
                public string Sources {
                        get { return (string) base ["sources"]; }
                        set { base ["sources"] = value; }
                }
        }
}

