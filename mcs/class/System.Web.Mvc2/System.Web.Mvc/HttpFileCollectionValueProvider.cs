/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found in the license.htm file included 
 * in this distribution.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

namespace System.Web.Mvc {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public sealed class HttpFileCollectionValueProvider : DictionaryValueProvider<HttpPostedFileBase[]> {

        private static readonly Dictionary<string, HttpPostedFileBase[]> _emptyDictionary = new Dictionary<string, HttpPostedFileBase[]>();

        public HttpFileCollectionValueProvider(ControllerContext controllerContext)
            : base(GetHttpPostedFileDictionary(controllerContext), CultureInfo.InvariantCulture) {
        }

        private static Dictionary<string, HttpPostedFileBase[]> GetHttpPostedFileDictionary(ControllerContext controllerContext) {
            HttpFileCollectionBase files = controllerContext.HttpContext.Request.Files;

            // fast-track common case of no files
            if (files.Count == 0) {
                return _emptyDictionary;
            }

            // build up the 1:many file mapping
            List<KeyValuePair<string, HttpPostedFileBase>> mapping = new List<KeyValuePair<string, HttpPostedFileBase>>();
            string[] allKeys = files.AllKeys;
            for (int i = 0; i < files.Count; i++) {
                string key = allKeys[i];
                if (key != null) {
                    HttpPostedFileBase file = HttpPostedFileBaseModelBinder.ChooseFileOrNull(files[i]);
                    mapping.Add(new KeyValuePair<string, HttpPostedFileBase>(key, file));
                }
            }

            // turn the mapping into a 1:many dictionary
            var grouped = mapping.GroupBy(el => el.Key, el => el.Value, StringComparer.OrdinalIgnoreCase);
            return grouped.ToDictionary(g => g.Key, g => g.ToArray(), StringComparer.OrdinalIgnoreCase);
        }

    }
}
