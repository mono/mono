//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Presentation.Toolbox
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Globalization;

    // This is helper class which tries to load and create tool items from flat text file.
    // The content of a file must contain following, colon separated values
    // 1.) fully qualified class name, which is flagged with ToolboxItemAttribute 
    // 2.) name of the assembly (with extension) where the tool class is located
    // 3.) an optional bitmap file (in case when activity is not marked with toolboxitem attribute)
    // 4.) an optional display name 
    //
    // Positions 3 & 4 can be in mixed order. the loader checks for known graphic extension 
    //
    // The category information is defined by string content placed within square brackets [ ].
    // All items under category definition fall into that category unless new category is defined.
    // If no category is defined, default one is created
    //
    // All lines starting with ';' or '#' char are treaded as comments and skipped

    sealed class ToolboxItemLoader
    {
        static readonly string FormatExceptionText =
            "Tool has to be defined either as 'name, assembly', or 'name, assembly, bitmap'";
        static readonly string DefaultCategory = "default";

        private ToolboxItemLoader()
        {
        }

        public static ToolboxItemLoader GetInstance()
        {
            return new ToolboxItemLoader();
        }

        public void LoadToolboxItems(string fileName, ToolboxCategoryItems container, bool resetContainer)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("fileName"));
            }

            if (null == container)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("container"));
            }

            if (resetContainer)
            {
                container.Clear();
            }

            using (StreamReader reader = File.OpenText(fileName))
            {
                string entry = null;
                ToolboxCategory category = null;
                while (null != (entry = reader.ReadLine()))
                {
                    entry = entry.Trim();
                    if (entry.Length > 1 && (entry[0] != ';' && entry[0] != '#'))
                    {
                        if (entry.StartsWith("[", StringComparison.CurrentCulture) && entry.EndsWith("]", StringComparison.CurrentCulture))
                        {
                            string categoryName = entry.Substring(1, entry.Length - 2);
                            category = GetCategoryItem(container, categoryName);
                        }
                        else
                        {
                            if (null == category)
                            {
                                category = GetCategoryItem(container, DefaultCategory);
                            }
                            string[] toolDefinition = entry.Split(';');

                            string toolName = null;
                            string assembly = null;
                            string displayName = null;
                            string bitmap = null;

                            if (GetToolAssemblyAndName(toolDefinition, ref toolName, ref assembly))
                            {
                                GetBitmap(toolDefinition, ref bitmap);
                                GetDisplayName(toolDefinition, ref displayName);
                                category.Add(new ToolboxItemWrapper(toolName, assembly, bitmap, displayName));
                            }
                            else
                            {
                                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException(FormatExceptionText));
                            }
                        }
                    }
                }
            }
        }

        bool GetToolAssemblyAndName(string[] toolDefinition, ref string toolName, ref string assembly)
        {
            if (toolDefinition.Length >= 2)
            {
                toolName = toolDefinition[0].Trim();
                assembly = toolDefinition[1].Trim();
                return true;
            }
            return false;
        }

        bool GetBitmap(string[] toolDefinition, ref string bitmap)
        {
            for (int i = 2; i < toolDefinition.Length; ++i)
            {
                string current = toolDefinition[i].Trim();
                if (current.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) ||
                    current.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    current.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                    current.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    current.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) ||
                    current.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    current.EndsWith(".exig", StringComparison.OrdinalIgnoreCase))
                {
                    bitmap = current;
                    return true;
                }
            }
            return false;
        }

        bool GetDisplayName(string[] toolDefinition, ref string displayName)
        {
            for (int i = 2; i < toolDefinition.Length; ++i)
            {
                string current = toolDefinition[i].Trim();
                if (!current.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase) &&
                    !current.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
                    !current.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) &&
                    !current.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
                    !current.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase) &&
                    !current.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) &&
                    !current.EndsWith(".exig", StringComparison.OrdinalIgnoreCase))
                {
                    displayName = current;
                    return true;
                }
            }
            return false;
        }

        ToolboxCategory GetCategoryItem(ToolboxCategoryItems container, string categoryName)
        {
            foreach (ToolboxCategory category in container)
            {
                if (0 == string.Compare(category.CategoryName, categoryName, true, CultureInfo.CurrentUICulture))
                {
                    return category;
                }
            }
            ToolboxCategory newCategory = new ToolboxCategory(categoryName);
            container.Add(newCategory);
            return newCategory;
        }
    }
}
