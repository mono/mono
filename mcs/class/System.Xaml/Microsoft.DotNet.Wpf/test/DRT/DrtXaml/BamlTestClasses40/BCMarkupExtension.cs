using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace BamlTestClasses40
{
    public class BCMarkupExtension : MarkupExtension
    {
        private string _value;

        public BCMarkupExtension() { }
        public BCMarkupExtension(string value)
        {
            _value = value;
        }

        public BCMarkupExtension(string value, string value2)
        {
            _value = value;
            Mode = value2;
        }

        public override object ProvideValue(
                        IServiceProvider serviceProvider)
        {
            return string.Format("Path: {0} , Mode: {1}", Path, Mode);
        }
        [CLSCompliant(false)]
        [MarkupExtensionBracketCharacters('(',')')]
        [MarkupExtensionBracketCharacters('[',']')]
        [ConstructorArgument("value")]
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
        public string Path
#pragma warning restore CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
        {
            get { return _value; }
            set { _value = value; }
        }

        [CLSCompliant(false)]
        [ConstructorArgument("value2")]
        [MarkupExtensionBracketCharacters('$','^')]
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
        public string Mode
#pragma warning restore CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
        {
            get; set;
        }
    }

    public class BCNestedMarkupExtension : MarkupExtension
    {
        private string _value;

        public BCNestedMarkupExtension() { }
        public BCNestedMarkupExtension(string path)
        {
            _value = path;
        }

        public override object ProvideValue(
                        IServiceProvider serviceProvider)
        {
            return Path;
        }

        [CLSCompliant(false)]
        [MarkupExtensionBracketCharacters('[',']')]
        [MarkupExtensionBracketCharacters('(', ')')]
#pragma warning disable CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
        public string Path
#pragma warning restore CS3021 // Type or member does not need a CLSCompliant attribute because the assembly does not have a CLSCompliant attribute
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}
