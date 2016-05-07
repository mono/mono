using System.Data.Linq;
using System.Data.Objects;
using System.Globalization;
using System.Web.Resources;

namespace System.Web.DynamicData.ModelProviders {
    internal class SchemaCreator {
        private static SchemaCreator s_instance = new SchemaCreator();

        public static SchemaCreator Instance {
            get {
                return s_instance;
            }
        }

        public virtual DataModelProvider CreateDataModel(object contextInstance, Func<object> contextFactory) {
            if (IsDataContext(contextInstance.GetType())) {
                return new DLinqDataModelProvider(contextInstance, contextFactory);
            }
            if (IsObjectContext(contextInstance.GetType())) {
                return new EFDataModelProvider(contextInstance, contextFactory);
            }

            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.SchemaCreator_UnknownModel, contextInstance.GetType().FullName));
        }

        public virtual bool ValidDataContextType(Type contextType) {
            // 
            return IsDataContext(contextType) || IsObjectContext(contextType);
        }

        internal static bool IsDataContext(Type contextType) {
            return IsValidType<DataContext>(contextType);
        }

        internal static bool IsObjectContext(Type contextType) {
            return IsValidType<ObjectContext>(contextType);
        }

        private static bool IsValidType<T>(Type contextType) where T : class {
            return contextType != null && typeof(T).IsAssignableFrom(contextType);
        }
    }
}
