namespace System.Web.Mvc.Html {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web.Mvc.Resources;
    using System.Web.Routing;
    using System.Web.UI.WebControls;

    internal static class TemplateHelpers {
        static readonly Dictionary<DataBoundControlMode, string> modeViewPaths =
            new Dictionary<DataBoundControlMode, string> {
                { DataBoundControlMode.ReadOnly, "DisplayTemplates" },
                { DataBoundControlMode.Edit,     "EditorTemplates" }
            };

        static readonly Dictionary<string, Func<HtmlHelper, string>> defaultDisplayActions =
            new Dictionary<string, Func<HtmlHelper, string>>(StringComparer.OrdinalIgnoreCase) {
                { "EmailAddress",       DefaultDisplayTemplates.EmailAddressTemplate },
                { "HiddenInput",        DefaultDisplayTemplates.HiddenInputTemplate },
                { "Html",               DefaultDisplayTemplates.HtmlTemplate },
                { "Text",               DefaultDisplayTemplates.StringTemplate },
                { "Url",                DefaultDisplayTemplates.UrlTemplate },
                { "Collection",         DefaultDisplayTemplates.CollectionTemplate },
                { typeof(bool).Name,    DefaultDisplayTemplates.BooleanTemplate },
                { typeof(decimal).Name, DefaultDisplayTemplates.DecimalTemplate },
                { typeof(string).Name,  DefaultDisplayTemplates.StringTemplate },
                { typeof(object).Name,  DefaultDisplayTemplates.ObjectTemplate },
            };

        static readonly Dictionary<string, Func<HtmlHelper, string>> defaultEditorActions =
            new Dictionary<string, Func<HtmlHelper, string>>(StringComparer.OrdinalIgnoreCase) {
                { "HiddenInput",        DefaultEditorTemplates.HiddenInputTemplate },
                { "MultilineText",      DefaultEditorTemplates.MultilineTextTemplate },
                { "Password",           DefaultEditorTemplates.PasswordTemplate },
                { "Text",               DefaultEditorTemplates.StringTemplate },
                { "Collection",         DefaultEditorTemplates.CollectionTemplate },
                { typeof(bool).Name,    DefaultEditorTemplates.BooleanTemplate },
                { typeof(decimal).Name, DefaultEditorTemplates.DecimalTemplate },
                { typeof(string).Name,  DefaultEditorTemplates.StringTemplate },
                { typeof(object).Name,  DefaultEditorTemplates.ObjectTemplate },
            };

        internal static string cacheItemId = Guid.NewGuid().ToString();

        internal delegate string ExecuteTemplateDelegate(HtmlHelper html, ViewDataDictionary viewData, string templateName, DataBoundControlMode mode, GetViewNamesDelegate getViewNames, GetDefaultActionsDelegate getDefaultActions);

        internal static string ExecuteTemplate(HtmlHelper html, ViewDataDictionary viewData, string templateName, DataBoundControlMode mode, GetViewNamesDelegate getViewNames, GetDefaultActionsDelegate getDefaultActions) {
            Dictionary<string, ActionCacheItem> actionCache = GetActionCache(html);
            Dictionary<string, Func<HtmlHelper, string>> defaultActions = getDefaultActions(mode);
            string modeViewPath = modeViewPaths[mode];

            foreach (string viewName in getViewNames(viewData.ModelMetadata, templateName, viewData.ModelMetadata.TemplateHint, viewData.ModelMetadata.DataTypeName)) {
                string fullViewName = modeViewPath + "/" + viewName;
                ActionCacheItem cacheItem;

                if (actionCache.TryGetValue(fullViewName, out cacheItem)) {
                    if (cacheItem != null) {
                        return cacheItem.Execute(html, viewData);
                    }
                }
                else {
                    ViewEngineResult viewEngineResult = ViewEngines.Engines.FindPartialView(html.ViewContext, fullViewName);
                    if (viewEngineResult.View != null) {
                        actionCache[fullViewName] = new ActionCacheViewItem { ViewName = fullViewName };

                        using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture)) {
                            viewEngineResult.View.Render(new ViewContext(html.ViewContext, viewEngineResult.View, viewData, html.ViewContext.TempData, writer), writer);
                            return writer.ToString();
                        }
                    }

                    Func<HtmlHelper, string> defaultAction;
                    if (defaultActions.TryGetValue(viewName, out defaultAction)) {
                        actionCache[fullViewName] = new ActionCacheCodeItem { Action = defaultAction };
                        return defaultAction(MakeHtmlHelper(html, viewData));
                    }

                    actionCache[fullViewName] = null;
                }
            }

            throw new InvalidOperationException(
                String.Format(
                    CultureInfo.CurrentCulture,
                    MvcResources.TemplateHelpers_NoTemplate,
                    viewData.ModelMetadata.RealModelType.FullName
                )
            );
        }

        internal static Dictionary<string, ActionCacheItem> GetActionCache(HtmlHelper html) {
            HttpContextBase context = html.ViewContext.HttpContext;
            Dictionary<string, ActionCacheItem> result;

            if (!context.Items.Contains(cacheItemId)) {
                result = new Dictionary<string, ActionCacheItem>();
                context.Items[cacheItemId] = result;
            }
            else {
                result = (Dictionary<string, ActionCacheItem>)context.Items[cacheItemId];
            }

            return result;
        }

        internal delegate Dictionary<string, Func<HtmlHelper, string>> GetDefaultActionsDelegate(DataBoundControlMode mode);

        internal static Dictionary<string, Func<HtmlHelper, string>> GetDefaultActions(DataBoundControlMode mode) {
            return mode == DataBoundControlMode.ReadOnly ? defaultDisplayActions : defaultEditorActions;
        }

        internal delegate IEnumerable<string> GetViewNamesDelegate(ModelMetadata metadata, params string[] templateHints);

        internal static IEnumerable<string> GetViewNames(ModelMetadata metadata, params string[] templateHints) {
            foreach (string templateHint in templateHints.Where(s => !String.IsNullOrEmpty(s))) {
                yield return templateHint;
            }

            // We don't want to search for Nullable<T>, we want to search for T (which should handle both T and Nullable<T>)
            Type fieldType = Nullable.GetUnderlyingType(metadata.RealModelType) ?? metadata.RealModelType;

            // TODO: Make better string names for generic types
            yield return fieldType.Name;

            if (!metadata.IsComplexType) {
                yield return "String";
            }
            else if (fieldType.IsInterface) {
                if (typeof(IEnumerable).IsAssignableFrom(fieldType)) {
                    yield return "Collection";
                }

                yield return "Object";
            }
            else {
                bool isEnumerable = typeof(IEnumerable).IsAssignableFrom(fieldType);

                while (true) {
                    fieldType = fieldType.BaseType;
                    if (fieldType == null)
                        break;

                    if (isEnumerable && fieldType == typeof(Object)) {
                        yield return "Collection";
                    }

                    yield return fieldType.Name;
                }
            }
        }

        internal static MvcHtmlString Template(HtmlHelper html, string expression, string templateName, string htmlFieldName, DataBoundControlMode mode, object additionalViewData) {
            return MvcHtmlString.Create(Template(html, expression, templateName, htmlFieldName, mode, additionalViewData, TemplateHelper));
        }

        // Unit testing version
        internal static string Template(HtmlHelper html, string expression, string templateName, string htmlFieldName,
                                        DataBoundControlMode mode, object additionalViewData, TemplateHelperDelegate templateHelper) {
            return templateHelper(html,
                                  ModelMetadata.FromStringExpression(expression, html.ViewData),
                                  htmlFieldName ?? ExpressionHelper.GetExpressionText(expression),
                                  templateName,
                                  mode,
                                  additionalViewData);
        }

        internal static MvcHtmlString TemplateFor<TContainer, TValue>(this HtmlHelper<TContainer> html, Expression<Func<TContainer, TValue>> expression,
                                                                      string templateName, string htmlFieldName, DataBoundControlMode mode,
                                                                      object additionalViewData) {
            return MvcHtmlString.Create(TemplateFor(html, expression, templateName, htmlFieldName, mode, additionalViewData, TemplateHelper));
        }

        // Unit testing version
        internal static string TemplateFor<TContainer, TValue>(this HtmlHelper<TContainer> html, Expression<Func<TContainer, TValue>> expression,
                                                               string templateName, string htmlFieldName, DataBoundControlMode mode,
                                                               object additionalViewData, TemplateHelperDelegate templateHelper) {
            return templateHelper(html,
                                  ModelMetadata.FromLambdaExpression(expression, html.ViewData),
                                  htmlFieldName ?? ExpressionHelper.GetExpressionText(expression),
                                  templateName,
                                  mode,
                                  additionalViewData);
        }

        internal delegate string TemplateHelperDelegate(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string templateName, DataBoundControlMode mode, object additionalViewData);

        internal static string TemplateHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string templateName, DataBoundControlMode mode, object additionalViewData) {
            return TemplateHelper(html, metadata, htmlFieldName, templateName, mode, additionalViewData, ExecuteTemplate);
        }

        internal static string TemplateHelper(HtmlHelper html, ModelMetadata metadata, string htmlFieldName, string templateName, DataBoundControlMode mode, object additionalViewData, ExecuteTemplateDelegate executeTemplate) {
            // TODO: Convert Editor into Display if model.IsReadOnly is true? Need to be careful about this because
            // the Model property on the ViewPage/ViewUserControl is get-only, so the type descriptor automatically
            // decorates it with a [ReadOnly] attribute...

            if (metadata.ConvertEmptyStringToNull && String.Empty.Equals(metadata.Model)) {
                metadata.Model = null;
            }

            object formattedModelValue = metadata.Model;
            if (metadata.Model == null && mode == DataBoundControlMode.ReadOnly) {
                formattedModelValue = metadata.NullDisplayText;
            }

            string formatString = mode == DataBoundControlMode.ReadOnly ? metadata.DisplayFormatString : metadata.EditFormatString;
            if (metadata.Model != null && !String.IsNullOrEmpty(formatString)) {
                formattedModelValue = String.Format(CultureInfo.CurrentCulture, formatString, metadata.Model);
            }

            // Normally this shouldn't happen, unless someone writes their own custom Object templates which
            // don't check to make sure that the object hasn't already been displayed
            object visitedObjectsKey = metadata.Model ?? metadata.RealModelType;
            if (html.ViewDataContainer.ViewData.TemplateInfo.VisitedObjects.Contains(visitedObjectsKey)) {    // DDB #224750
                return String.Empty;
            }

            ViewDataDictionary viewData = new ViewDataDictionary(html.ViewDataContainer.ViewData) {
                Model = metadata.Model,
                ModelMetadata = metadata,
                TemplateInfo = new TemplateInfo {
                    FormattedModelValue = formattedModelValue,
                    HtmlFieldPrefix = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName),
                    VisitedObjects = new HashSet<object>(html.ViewContext.ViewData.TemplateInfo.VisitedObjects),    // DDB #224750
                }
            };

            if (additionalViewData != null) {
                foreach (KeyValuePair<string, object> kvp in new RouteValueDictionary(additionalViewData)) {
                    viewData[kvp.Key] = kvp.Value;
                }
            }

            viewData.TemplateInfo.VisitedObjects.Add(visitedObjectsKey);    // DDB #224750

            return executeTemplate(html, viewData, templateName, mode, GetViewNames, GetDefaultActions);
        }

        // Helpers

        private static HtmlHelper MakeHtmlHelper(HtmlHelper html, ViewDataDictionary viewData) {
            return new HtmlHelper(
                new ViewContext(html.ViewContext, html.ViewContext.View, viewData, html.ViewContext.TempData, html.ViewContext.Writer),
                new ViewDataContainer(viewData)
            );
        }

        internal abstract class ActionCacheItem {
            public abstract string Execute(HtmlHelper html, ViewDataDictionary viewData);
        }

        internal class ActionCacheCodeItem : ActionCacheItem {
            public Func<HtmlHelper, string> Action { get; set; }

            public override string Execute(HtmlHelper html, ViewDataDictionary viewData) {
                return Action(MakeHtmlHelper(html, viewData));
            }
        }

        internal class ActionCacheViewItem : ActionCacheItem {
            public string ViewName { get; set; }

            public override string Execute(HtmlHelper html, ViewDataDictionary viewData) {
                ViewEngineResult viewEngineResult = ViewEngines.Engines.FindPartialView(html.ViewContext, ViewName);
                using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture)) {
                    viewEngineResult.View.Render(new ViewContext(html.ViewContext, viewEngineResult.View, viewData, html.ViewContext.TempData, writer), writer);
                    return writer.ToString();
                }
            }
        }

        private class ViewDataContainer : IViewDataContainer {
            public ViewDataContainer(ViewDataDictionary viewData) {
                ViewData = viewData;
            }

            public ViewDataDictionary ViewData { get; set; }
        }
    }
}
