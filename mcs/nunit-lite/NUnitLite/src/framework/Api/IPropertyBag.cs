using System;

namespace NUnit.Framework.Api
{
    /// <summary>
    /// A PropertyBag represents a collection of name/value pairs
    /// that allows duplicate entries with the same key. Methods
    /// are provided for adding a new pair as well as for setting
    /// a key to a single value. All keys are strings but values
    /// may be of any type. Null values are not permitted, since
    /// a null entry represents the absence of the key.
    /// 
    /// The entries in a PropertyBag are of two kinds: those that
    /// take a single value and those that take multiple values.
    /// However, the PropertyBag has no knowledge of which entries
    /// fall into each category and the distinction is entirely
    /// up to the code using the PropertyBag.
    /// 
    /// When working with multi-valued properties, client code
    /// should use the Add method to add name/value pairs and 
    /// indexing to retrieve a list of all values for a given
    /// key. For example:
    /// 
    ///     bag.Add("Tag", "one");
    ///     bag.Add("Tag", "two");
    ///     Assert.That(bag["Tag"],
    ///       Is.EqualTo(new string[] { "one", "two" })); 
    /// 
    /// When working with single-valued propeties, client code
    /// should use the Set method to set the value and Get to
    /// retrieve the value. The GetSetting methods may also be
    /// used to retrieve the value in a type-safe manner while
    /// also providing  default. For example:
    /// 
    ///     bag.Set("Priority", "low");
    ///     bag.Set("Priority", "high"); // replaces value
    ///     Assert.That(bag.Get("Priority"),
    ///       Is.EqualTo("high"));
    ///     Assert.That(bag.GetSetting("Priority", "low"),
    ///       Is.EqualTo("high"));
    /// </summary>
    public interface IPropertyBag : IXmlNodeBuilder, System.Collections.IEnumerable
    {
        /// <summary>
        /// Get the number of key/value pairs in the property bag
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds a key/value pair to the property bag
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        void Add(string key, object value);

        
        /// <summary>
        /// Sets the value for a key, removing any other
        /// values that are already in the property set.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Set(string key, object value);

        /// <summary>
        /// Gets a single value for a key, using the first
        /// one if multiple values are present and returning
        /// null if the value is not found.
        /// </summary>
        object Get(string key);

        /// <summary>
        /// Gets a single string value for a key, using the first
        /// one if multiple values are present and returning the
        /// default value if no entry is found.
        /// </summary>
        string GetSetting(string key, string defaultValue);

        /// <summary>
        /// Gets a single int value for a key, using the first
        /// one if multiple values are present and returning the
        /// default value if no entry is found.
        /// </summary>
        int GetSetting(string key, int defaultValue);

        /// <summary>
        /// Gets a single boolean value for a key, using the first
        /// one if multiple values are present and returning the
        /// default value if no entry is found.
        /// </summary>
        bool GetSetting(string key, bool defaultValue);

        /// <summary>
        /// Gets a single enum value for a key, using the first
        /// one if multiple values are present and returning the
        /// default value if no entry is found.
        /// </summary>
        System.Enum GetSetting(string key, System.Enum defaultValue);

        /// <summary>
        ///  Removes all entries for a key from the property set.
        ///  If the key is not found, no error occurs.
        /// </summary>
        /// <param name="key">The key for which the entries are to be removed</param>
        void Remove(string key);

        /// <summary>
        /// Removes a single entry if present. If not found,
        /// no error occurs.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Remove(string key, object value);

        /// <summary>
        /// Removes a specific PropertyEntry. If the entry is not
        /// found, no errr occurs.
        /// </summary>
        /// <param name="entry">The property entry to remove</param>
        void Remove(PropertyEntry entry);

        /// <summary>
        /// Gets a flag indicating whether the specified key has
        /// any entries in the property set.
        /// </summary>
        /// <param name="key">The key to be checked</param>
        /// <returns>True if their are values present, otherwise false</returns>
        bool ContainsKey(string key);

        /// <summary>
        /// Gets a flag indicating whether the specified key and
        /// value are present in the property set.
        /// </summary>
        /// <param name="key">The key to be checked</param>
        /// <param name="value">The value to be checked</param>
        /// <returns>True if the key and value are present, otherwise false</returns>
        bool Contains(string key, object value);

        /// <summary>
        /// Gets a flag indicating whether the specified key and
        /// value are present in the property set.
        /// </summary>
        /// <param name="entry">The property entry to be checked</param>
        /// <returns>True if the entry is present, otherwise false</returns>
        bool Contains(PropertyEntry entry);

        /// <summary>
        /// Gets or sets the list of values for a particular key
        /// </summary>
        /// <param name="key">The key for which the values are to be retrieved or set</param>
        System.Collections.IList this[string key] { get; set; }

        /// <summary>
        /// Gets a collection containing all the keys in the property set
        /// </summary>
#if CLR_2_0 || CLR_4_0
        System.Collections.Generic.ICollection<string> Keys { get; }
#else
        System.Collections.ICollection Keys { get; }
#endif
    }
}
