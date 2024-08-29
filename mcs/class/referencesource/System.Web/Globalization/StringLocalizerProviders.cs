using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

 namespace System.Web.Globalization {
    public static class StringLocalizerProviders {
        private static IStringLocalizerProvider _dataAnnotationStringLocalizerProvider;
        private static bool _setStringLocalizerProvider = false;

         /// <summary>
        /// The current StringLocalizerProvider used by the DataAnnotation attributes
        /// </summary>
        public static IStringLocalizerProvider DataAnnotationStringLocalizerProvider {
            get {
                if (_dataAnnotationStringLocalizerProvider == null && !_setStringLocalizerProvider) {
                    _dataAnnotationStringLocalizerProvider = new ResourceFileStringLocalizerProvider();
                }
                return _dataAnnotationStringLocalizerProvider;
            }
            set {
                // Allow the developers to set null, which means to opt out the new localization mechanism
                _dataAnnotationStringLocalizerProvider = value;
                _setStringLocalizerProvider = true;
            }
        }
    }
}