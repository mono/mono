
// -------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All Rights Reserved.
// -------------------------------------------------------------------
//From \\authoring\Sparkle\Source\1.0.1083.0\Common\Source\Framework\Properties
namespace System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector
{
    using System.Runtime;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System;
    using System.Activities.Presentation.PropertyEditing;
    using System.Collections;
    using System.Windows.Media;
    using System.Windows;
    using System.Globalization;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.Data;
    using System.Diagnostics.CodeAnalysis;

    [SuppressMessage(FxCop.Category.Naming, "CA1724:TypeNamesShouldNotMatchNamespaces", 
        Justification = "Code imported from Cider; keeping changes to a minimum as it impacts xaml files as well")]
    internal abstract class CategoryBase : CategoryEntry, IEnumerable<PropertyEntry>
    {

        private static AlphabeticalCategoryEditorComparer alphabeticalCategoryEditorComparer = new AlphabeticalCategoryEditorComparer();

        private bool basicPropertyMatchesFilter = true;
        private bool advancedPropertyMatchesFilter = true;
        private ObservableCollectionWorkaround<CategoryEditor> categoryEditors = new ObservableCollectionWorkaround<CategoryEditor>();

        // Track the category editor that is currently providing the icon:
        private CategoryEditor iconProvider = null;
        private ImageSource categoryIcon = null;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        protected CategoryBase(string name)
            : base(name)
        {
            this.categoryEditors.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(categoryEditors_CollectionChanged);
            this.InitializeIcons();
        }

        public override IEnumerable<PropertyEntry> Properties
        {
            get
            {
                return this;
            }
        }

        public abstract ObservableCollection<PropertyEntry> BasicProperties
        { get; }
        public abstract ObservableCollection<PropertyEntry> AdvancedProperties
        { get; }

        public bool BasicPropertyMatchesFilter
        {
            get { return this.basicPropertyMatchesFilter; }
            set
            {
                if (this.basicPropertyMatchesFilter != value)
                {
                    this.basicPropertyMatchesFilter = value;
                    this.OnPropertyChanged("BasicPropertyMatchesFilter");
                }
            }
        }

        public bool AdvancedPropertyMatchesFilter
        {
            get { return this.advancedPropertyMatchesFilter; }
            set
            {
                if (this.advancedPropertyMatchesFilter != value)
                {
                    this.advancedPropertyMatchesFilter = value;
                    this.OnPropertyChanged("AdvancedPropertyMatchesFilter");
                }
            }
        }

        public virtual IComparable SortOrdering
        {
            get { return this.CategoryName; }
        }

        public ObservableCollection<CategoryEditor> CategoryEditors
        {
            get { return categoryEditors; }
        }

        public ImageSource CategoryIcon
        {
            get { return this.categoryIcon; }
            protected set { this.categoryIcon = value; this.OnPropertyChanged("CategoryIcon"); }
        }

        // IPropertyFilterTarget Members

        public override PropertyEntry this[string propertyName]
        {
            get
            {
                foreach (PropertyEntry property in this.Properties)
                {
                    if (property.PropertyName == propertyName)
                    {
                        return property;
                    }
                }
                return null;
            }
        }

        private void categoryEditors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.InitializeIcons();
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Propagating the error might cause VS to crash")]
        [SuppressMessage("Reliability", "Reliability108", Justification = "Propagating the error might cause VS to crash")]
        private void InitializeIcons()
        {
            if (this.iconProvider == null || !this.categoryEditors.Contains(this.iconProvider))
            {
                foreach (CategoryEditor editor in this.categoryEditors)
                {
                    ImageSource icon = null;

                    // CategoryEditor.GetImage is user code and could throw:
                    try
                    {
                        // 24,24 is the desired default size for category icons; it may or may not be respected
                        // by the implementation of GetImage
                        icon = editor.GetImage(new Size(24, 24)) as ImageSource;
                    }
                    catch (Exception exception)
                    {
                        this.ReportCategoryException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.CategoryIconLoadFailed, this.CategoryName, exception.Message));
                        continue;
                    }

                    if (icon != null)
                    {
                        if (icon is ISupportInitialize)
                        {
                            // Attempt to access some property on the image to ensure it has been initialized properly.  Display the error now, instead of later in the artboard.
                            try
                            {
                                double dummyHeight = icon.Height;
                            }
                            catch (InvalidOperationException exception)
                            {
                                this.ReportCategoryException(string.Format(CultureInfo.CurrentCulture, ExceptionStringTable.CategoryIconLoadFailed, this.CategoryName, exception.Message));
                                continue;
                            }
                        }
                        this.CategoryIcon = icon;
                        this.iconProvider = editor;
                        return;
                    }
                }
                this.CategoryIcon = null;
                this.iconProvider = null;
            }
        }

        public virtual void ReportCategoryException(string message)
        {
        }

        IEnumerator<PropertyEntry> IEnumerable<PropertyEntry>.GetEnumerator()
        {
            return new PropertyEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new PropertyEnumerator(this);
        }

        public void AddCategoryEditor(CategoryEditor categoryEditor)
        {
            int insertionIndex = this.categoryEditors.BinarySearch(categoryEditor, CategoryBase.alphabeticalCategoryEditorComparer);
            if (insertionIndex < 0)
            {
                insertionIndex = ~insertionIndex;
            }
            this.categoryEditors.Insert(insertionIndex, categoryEditor);
        }

        public void RemoveCategoryEditor(Type categoryEditor)
        {
            for (int i = 0; i < this.CategoryEditors.Count; i++)
            {
                if (this.CategoryEditors[i].GetType() == categoryEditor)
                {
                    this.CategoryEditors.RemoveAt(i);
                    return;
                }
            }
        }

        public override void ApplyFilter(PropertyFilter filter)
        {
            this.MatchesFilter = filter.Match(this);

            // Now Match all the properties in this category
            bool newBasicPropertyMatchesFilter = false;
            bool newAdvancedPropertyMatchesFilter = false;

            foreach (PropertyEntry property in this.BasicProperties)
            {
                if (this.DoesPropertyMatchFilter(filter, property))
                {
                    newBasicPropertyMatchesFilter = true;
                }
            }

            foreach (PropertyEntry property in this.AdvancedProperties)
            {
                if (this.DoesPropertyMatchFilter(filter, property))
                {
                    newAdvancedPropertyMatchesFilter = true;
                }
            }

            this.BasicPropertyMatchesFilter = newBasicPropertyMatchesFilter;
            this.AdvancedPropertyMatchesFilter = newAdvancedPropertyMatchesFilter;

            this.OnFilterApplied(filter);
        }

        public override bool MatchesPredicate(PropertyFilterPredicate predicate)
        {
            return predicate.Match(this.CategoryName);
        }


        protected virtual bool DoesPropertyMatchFilter(PropertyFilter filter, PropertyEntry property)
        {
            property.ApplyFilter(filter);
            return property.MatchesFilter;
        }

        private struct PropertyEnumerator : IEnumerator<PropertyEntry>
        {
            private CategoryBase category;
            private IEnumerator<PropertyEntry> current;
            private bool enumeratingBasic;

            public PropertyEnumerator(CategoryBase category)
            {
                this.category = category;
                this.current = null;
                this.enumeratingBasic = false;
                this.Reset();
            }

            public PropertyEntry Current
            {
                get
                {
                    return this.current.Current;
                }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                if (this.current.MoveNext())
                {
                    return true;
                }
                else
                {
                    if (this.enumeratingBasic)
                    {
                        this.enumeratingBasic = false;
                        this.current = this.category.AdvancedProperties.GetEnumerator();
                        return this.MoveNext();
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            public void Reset()
            {
                this.current = category.BasicProperties.GetEnumerator();
                this.enumeratingBasic = true;
            }

            void IDisposable.Dispose()
            {
            }
        }
        private class AlphabeticalCategoryEditorComparer : Comparer<CategoryEditor>
        {
            public override int Compare(CategoryEditor x, CategoryEditor y)
            {
                //return x.GetType().ToString().CompareTo(y.GetType().ToString());
                //CompareTo uses currentCulture for string comparison
                return string.Compare(x.GetType().ToString(), y.GetType().ToString(), StringComparison.CurrentCulture);
            }
        }
    }
}

