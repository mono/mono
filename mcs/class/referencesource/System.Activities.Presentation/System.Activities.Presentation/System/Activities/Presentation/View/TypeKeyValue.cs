namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Automation.Peers;
    using System.Windows.Input;

    //helper class, used to resolve generics. supports cascading generic type arguments (i.e. IList< IList < int > >)
    internal class TypeKeyValue : INotifyPropertyChanged
    {
        string errorText;
        //generic type
        Type genericType;

        bool isValid = true;
        //generic's type generic parameters
        ObservableCollection<TypeKeyValue> subTypes = new ObservableCollection<TypeKeyValue>();
        //target type
        Type targetType;
        //type resolver reference
        Action<TypeKeyValue> typeChangedCallBack;
        //if this type is selected
        bool isSelected;
        Func<Type, bool> filter;
        ObservableCollection<Type> mostRecentlyUsedTypes;
        string hintText = null;
        //if type presenter should skip the drop down list
        bool browseTypeDirectly = true;

        public TypeKeyValue(Type genericType, Action<TypeKeyValue> typeChangedCallBack)
        {
            this.GenericType = genericType;
            this.typeChangedCallBack = typeChangedCallBack;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public string ErrorText
        {
            get { return this.errorText; }
            set
            {
                this.errorText = value;
                OnPropertyChanged("ErrorText");
            }
        }

        public Type GenericType
        {
            get { return this.genericType; }
            set
            {
                this.genericType = value;
                OnPropertyChanged("GenericType");
            }
        }

        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                this.isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public Func<Type, bool> Filter
        {
            get { return this.filter; }
            set
            {
                this.filter = value;
                OnPropertyChanged("Filter");
            }
        }

        public ObservableCollection<Type> MostRecentlyUsedTypes
        {
            get { return this.mostRecentlyUsedTypes; }
            set
            {
                this.mostRecentlyUsedTypes = value;
                OnPropertyChanged("MostRecentlyUsedTypes");
            }
        }

        public string HintText
        {
            get { return this.hintText; }
            set
            {
                this.hintText = value;
                this.OnPropertyChanged("HintText");
            }
        }

        public bool BrowseTypeDirectly
        {
            get { return this.browseTypeDirectly; }
            set
            {
                this.browseTypeDirectly = value;
                OnPropertyChanged("BrowseTypeDirectly");
            }
        }

        public bool IsValid
        {
            get { return this.isValid; }
            set
            {
                this.isValid = value;
                OnPropertyChanged("IsValid");
            }
        }


        public ObservableCollection<TypeKeyValue> SubTypes
        {
            get { return this.subTypes; }
        }

        public Type TargetType
        {
            get { return this.targetType; }
            set
            {
                this.targetType = value;
                //whenever target type changes, check if there are some generic parameters required
                LoadGenericTypes();
                OnPropertyChanged("TargetType");
                if (typeChangedCallBack != null)
                {
                    typeChangedCallBack(this);
                }
            }
        }

        public Type GetConcreteType()
        {
            Type result = null;
            if (null != this.targetType)
            {
                //do we have generic?
                if (this.targetType.IsGenericTypeDefinition)
                {
                    //resolve all generic arguments
                    Type[] arguments = new Type[this.subTypes.Count];
                    bool isValid = true;
                    for (int i = 0; i < this.subTypes.Count && isValid; ++i)
                    {
                        arguments[i] = this.subTypes[i].GetConcreteType();
                        isValid = (null != arguments[i]);
                    }
                    if (isValid)
                    {
                        //and create target type
                        result = this.targetType.MakeGenericType(arguments);
                    }
                }
                else
                {
                    result = targetType;
                }
            }
            return result;
        }

        void LoadGenericTypes()
        {
            this.subTypes.Clear();
            if (null != this.targetType && this.targetType.IsGenericTypeDefinition)
            {
                Type[] generics = this.targetType.GetGenericArguments();
                foreach (Type t in generics)
                {
                    TypeKeyValue entry = new TypeKeyValue(t, typeChangedCallBack);
                    this.subTypes.Add(entry);
                    typeChangedCallBack(entry);
                }
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            if (null != PropertyChanged)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
