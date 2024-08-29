using System.Globalization;
using System.Resources;
using System.Threading;

namespace System.Web.UI
{
  internal sealed class WebControlsRes
  {
    internal const string DefaultConstructorNotFound = "DefaultConstructorNotFound";
    internal const string PropertyNotFound = "PropertyNotFound";
    internal const string ValueNotResettable = "ValueNotResettable";
    internal const string DisplayNameCollision = "DisplayNameCollision";
    internal const string SetValueNotSupported = "SetValueNotSupported";
    internal const string ComponentNotFromProperCollection = "ComponentNotFromProperCollection";
    internal const string EntityDataSource_Description = "EntityDataSource_Description";
    internal const string EntityDataSource_DisplayName = "EntityDataSource_DisplayName";
    internal const string PropertyDescription_AutoGenerateOrderByClause = "PropertyDescription_AutoGenerateOrderByClause";
    internal const string PropertyDescription_AutoGenerateWhereClause = "PropertyDescription_AutoGenerateWhereClause";
    internal const string PropertyDescription_AutoPage = "PropertyDescription_AutoPage";
    internal const string PropertyDescription_AutoSort = "PropertyDescription_AutoSort";
    internal const string PropertyDescription_EnableFlattening = "PropertyDescription_EnableFlattening";
    internal const string PropertyDescription_EnableDelete = "PropertyDescription_EnableDelete";
    internal const string PropertyDescription_EnableInsert = "PropertyDescription_EnableInsert";
    internal const string PropertyDescription_EnableUpdate = "PropertyDescription_EnableUpdate";
    internal const string PropertyDescription_GroupBy = "PropertyDescription_GroupBy";
    internal const string PropertyDescription_Include = "PropertyDescription_Include";
    internal const string PropertyDescription_ContextTypeName = "PropertyDescription_ContextTypeName";
    internal const string PropertyDescription_StoreOriginalValuesInViewState = "PropertyDescription_StoreOriginalValuesInViewState";
    internal const string PropertyDescription_ContextCreating = "PropertyDescription_ContextCreating";
    internal const string PropertyDescription_ContextCreated = "PropertyDescription_ContextCreated";
    internal const string PropertyDescription_ContextDisposing = "PropertyDescription_ContextDisposing";
    internal const string PropertyDescription_Selecting = "PropertyDescription_Selecting";
    internal const string PropertyDescription_Selected = "PropertyDescription_Selected";
    internal const string PropertyDescription_Deleting = "PropertyDescription_Deleting";
    internal const string PropertyDescription_Deleted = "PropertyDescription_Deleted";
    internal const string PropertyDescription_Inserting = "PropertyDescription_Inserting";
    internal const string PropertyDescription_Inserted = "PropertyDescription_Inserted";
    internal const string PropertyDescription_Updating = "PropertyDescription_Updating";
    internal const string PropertyDescription_Updated = "PropertyDescription_Updated";
    internal const string PropertyDescription_QueryCreated = "PropertyDescription_QueryCreated";
    internal const string EntityDataSource_CommandTextOrEntitySetName = "EntityDataSource_CommandTextOrEntitySetName";
    internal const string EntityDataSource_CommandTextOrEntitySetNameRequired = "EntityDataSource_CommandTextOrEntitySetNameRequired";
    internal const string EntityDataSource_SelectNotEditable = "EntityDataSource_SelectNotEditable";
    internal const string EntityDataSource_CommandTextNotEditable = "EntityDataSource_CommandTextNotEditable";
    internal const string EntityDataSource_GroupByNotEditable = "EntityDataSource_GroupByNotEditable";
    internal const string EntityDataSource_AutoGenerateWhereNotAllowedIfWhereDefined = "EntityDataSource_AutoGenerateWhereNotAllowedIfWhereDefined";
    internal const string EntityDataSource_AutoGenerateOrderByNotAllowedIfOrderByIsDefined = "EntityDataSource_AutoGenerateOrderByNotAllowedIfOrderByIsDefined";
    internal const string EntityDataSource_WhereParametersNeedsWhereOrAutoGenerateWhere = "EntityDataSource_WhereParametersNeedsWhereOrAutoGenerateWhere";
    internal const string EntityDataSource_OrderByParametersNeedsOrderByOrAutoGenerateOrderBy = "EntityDataSource_OrderByParametersNeedsOrderByOrAutoGenerateOrderBy";
    internal const string EntityDataSource_CommandParametersNeedCommandText = "EntityDataSource_CommandParametersNeedCommandText";
    internal const string EntityDataSource_SelectParametersNeedSelect = "EntityDataSource_SelectParametersNeedSelect";
    internal const string EntityDataSource_GroupByNeedsSelect = "EntityDataSource_GroupByNeedsSelect";
    internal const string EntityDataSource_CommandTextCantHaveEntityTypeFilter = "EntityDataSource_CommandTextCantHaveEntityTypeFilter";
    internal const string EntityDataSourceQueryBuilder_PagingRequiresOrderBy = "EntityDataSourceQueryBuilder_PagingRequiresOrderBy";
    internal const string EntityDataSourceView_UpdateDisabledForThisControl = "EntityDataSourceView_UpdateDisabledForThisControl";
    internal const string EntityDataSourceView_DeleteDisabledForThiscontrol = "EntityDataSourceView_DeleteDisabledForThiscontrol";
    internal const string EntityDataSourceView_InsertDisabledForThisControl = "EntityDataSourceView_InsertDisabledForThisControl";
    internal const string EntityDataSourceView_NoParameterlessConstructorForTheContext = "EntityDataSourceView_NoParameterlessConstructorForTheContext";
    internal const string EntityDataSourceView_ObjectContextMustBeSpecified = "EntityDataSourceView_ObjectContextMustBeSpecified";
    internal const string EntityDataSourceView_ContainerNameMustBeSpecified = "EntityDataSourceView_ContainerNameMustBeSpecified";
    internal const string EntityDataSourceView_PropertyDoesNotExistOnEntity = "EntityDataSourceView_PropertyDoesNotExistOnEntity";
    internal const string EntityDataSourceView_EntitySetDoesNotExistOnTheContainer = "EntityDataSourceView_EntitySetDoesNotExistOnTheContainer";
    internal const string EntityDataSourceView_EntitySetMismatchWithQueryResults = "EntityDataSourceView_EntitySetMismatchWithQueryResults";
    internal const string EntityDataSourceView_ContainerNameDoesNotExistOnTheContext = "EntityDataSourceView_ContainerNameDoesNotExistOnTheContext";
    internal const string EntityDataSourceView_ColumnHeader = "EntityDataSourceView_ColumnHeader";
    internal const string EntityDataSourceView_DataConversionError = "EntityDataSourceView_DataConversionError";
    internal const string EntityDataSourceView_FilteredEntityTypeMustBeDerivableFromEntitySet = "EntityDataSourceView_FilteredEntityTypeMustBeDerivableFromEntitySet";
    internal const string EntityDataSourceView_QueryCreatedNotAnObjectQuery = "EntityDataSourceView_QueryCreatedNotAnObjectQuery";
    internal const string EntityDataSourceView_QueryCreatedWrongType = "EntityDataSourceView_QueryCreatedWrongType";
    internal const string EntityDataSourceView_UnknownProperty = "EntityDataSourceView_UnknownProperty";
    internal const string EntityDataSourceView_NoKeyProperty = "EntityDataSourceView_NoKeyProperty";
    internal const string EntityDataSourceView_AutoGenerateOrderByParameters = "EntityDataSourceView_AutoGenerateOrderByParameters";
    internal const string EntityDataSourceView_EmptyPropertyName = "EntityDataSourceView_EmptyPropertyName";
    internal const string EntityDataSourceUtil_UnableToConvertTypeCodeToType = "EntityDataSourceUtil_UnableToConvertTypeCodeToType";
    internal const string EntityDataSourceUtil_EntityQueryCannotReturnPolymorphicTypes = "EntityDataSourceUtil_EntityQueryCannotReturnPolymorphicTypes";
    internal const string EntityDataSourceUtil_InsertUpdateParametersDontMatchPropertyNameOnEntity = "EntityDataSourceUtil_InsertUpdateParametersDontMatchPropertyNameOnEntity";
    internal const string EntityDataSourceUtil_UnableToConvertStringToType = "EntityDataSourceUtil_UnableToConvertStringToType";
    internal const string WebControlParameterProxy_TypeDbTypeMutuallyExclusive = "WebControlParameterProxy_TypeDbTypeMutuallyExclusive";
    private static WebControlsRes loader;
    private ResourceManager resources;

    internal WebControlsRes()
    {
      this.resources = new ResourceManager("System.Web.UI.WebControls", this.GetType().Assembly);
    }

    private static WebControlsRes GetLoader()
    {
      if (WebControlsRes.loader == null)
      {
        WebControlsRes webControlsRes = new WebControlsRes();
        Interlocked.CompareExchange<WebControlsRes>(ref WebControlsRes.loader, webControlsRes, (WebControlsRes) null);
      }
      return WebControlsRes.loader;
    }

    private static CultureInfo Culture
    {
      get
      {
        return (CultureInfo) null;
      }
    }

    public static ResourceManager Resources
    {
      get
      {
        return WebControlsRes.GetLoader().resources;
      }
    }

    public static string GetString(string name, params object[] args)
    {
      WebControlsRes loader = WebControlsRes.GetLoader();
      if (loader == null)
        return (string) null;
      string format = loader.resources.GetString(name, WebControlsRes.Culture);
      if (args == null || args.Length == 0)
        return format;
      for (int index = 0; index < args.Length; ++index)
      {
        string str = args[index] as string;
        if (str != null && str.Length > 1024)
          args[index] = (object) (str.Substring(0, 1021) + "...");
      }
      return string.Format((IFormatProvider) CultureInfo.CurrentCulture, format, args);
    }

    public static string GetString(string name)
    {
      WebControlsRes loader = WebControlsRes.GetLoader();
      if (loader == null)
        return (string) null;
      return loader.resources.GetString(name, WebControlsRes.Culture);
    }

    public static string GetString(string name, out bool usedFallback)
    {
      usedFallback = false;
      return WebControlsRes.GetString(name);
    }

    public static object GetObject(string name)
    {
      WebControlsRes loader = WebControlsRes.GetLoader();
      if (loader == null)
        return (object) null;
      return loader.resources.GetObject(name, WebControlsRes.Culture);
    }
  }
}