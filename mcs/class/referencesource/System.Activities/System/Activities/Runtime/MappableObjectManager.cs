//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Xml.Linq;

    [DataContract]
    class MappableObjectManager
    {
        List<MappableLocation> mappableLocations;

        public MappableObjectManager()
        {
        }

        public int Count
        {
            get
            {
                int result = 0;
                if (this.mappableLocations != null)
                {
                    result += this.mappableLocations.Count;
                }

                return result;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "mappableLocations")]
        internal List<MappableLocation> SerializedMappableLocations
        {
            get { return this.mappableLocations; }
            set { this.mappableLocations = value; }
        }

        public IDictionary<string, LocationInfo> GatherMappableVariables()
        {
            Dictionary<string, LocationInfo> result = null;
            if (this.mappableLocations != null && this.mappableLocations.Count > 0)
            {
                result = new Dictionary<string, LocationInfo>(this.mappableLocations.Count);
                for (int locationIndex = 0; locationIndex < this.mappableLocations.Count; locationIndex++)
                {
                    MappableLocation mappableLocation = this.mappableLocations[locationIndex];
                    result.Add(mappableLocation.MappingKeyName, new LocationInfo(mappableLocation.Name, mappableLocation.OwnerDisplayName, mappableLocation.Location.Value));
                }
            }

            return result;
        }

        public void Register(Location location, Activity activity, LocationReference locationOwner, ActivityInstance activityInstance)
        {
            Fx.Assert(location.CanBeMapped, "should only register mappable locations");

            if (this.mappableLocations == null)
            {
                this.mappableLocations = new List<MappableLocation>();
            }

            this.mappableLocations.Add(new MappableLocation(locationOwner, activity, activityInstance, location));
        }

        public void Unregister(Location location)
        {
            Fx.Assert(location.CanBeMapped, "should only register mappable locations");

            int mappedLocationsCount = this.mappableLocations.Count;
            for (int i = 0; i < mappedLocationsCount; i++)
            {
                if (object.ReferenceEquals(this.mappableLocations[i].Location, location))
                {
                    this.mappableLocations.RemoveAt(i);
                    break;
                }
            }
            Fx.Assert(this.mappableLocations.Count == mappedLocationsCount - 1, "can only unregister locations that have been registered");
        }

        [DataContract]
        internal class MappableLocation
        {
            string mappingKeyName;
            string name;
            string ownerDisplayName;
            Location location;

            public MappableLocation(LocationReference locationOwner, Activity activity, ActivityInstance activityInstance, Location location)
            {
                this.Name = locationOwner.Name;
                this.OwnerDisplayName = activity.DisplayName;
                this.Location = location;
                this.MappingKeyName = string.Format(CultureInfo.InvariantCulture, "activity.{0}-{1}_{2}", activity.Id, locationOwner.Id, activityInstance.Id);
            }
            
            internal string MappingKeyName
            {
                get
                {
                    return this.mappingKeyName;
                }
                private set
                {
                    this.mappingKeyName = value;
                }
            }
            
            public string Name
            {
                get
                {
                    return this.name;
                }
                private set
                {
                    this.name = value;
                }
            }
                        
            public string OwnerDisplayName
            {
                get
                {
                    return this.ownerDisplayName;
                }
                private set
                {
                    this.ownerDisplayName = value;
                }
            }
            
            internal Location Location
            {
                get
                {
                    return this.location;
                }
                private set
                {
                    this.location = value;
                }
            }

            [DataMember(Name = "MappingKeyName")]
            internal string SerializedMappingKeyName
            {
                get { return this.MappingKeyName; }
                set { this.MappingKeyName = value; }
            }

            [DataMember(Name = "Name")]
            internal string SerializedName
            {
                get { return this.Name; }
                set { this.Name = value; }
            }

            [DataMember(EmitDefaultValue = false, Name = "OwnerDisplayName")]
            internal string SerializedOwnerDisplayName
            {
                get { return this.OwnerDisplayName; }
                set { this.OwnerDisplayName = value; }
            }

            [DataMember(Name = "Location")]
            internal Location SerializedLocation
            {
                get { return this.Location; }
                set { this.Location = value; }
            }
        }
    }
}
