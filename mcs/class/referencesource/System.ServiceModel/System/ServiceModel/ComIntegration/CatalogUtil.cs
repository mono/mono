//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime;

    class ComCatalogObject
    {
        ICatalogObject catalogObject;
        ICatalogCollection catalogCollection;

        public ComCatalogObject(ICatalogObject catalogObject,
                                ICatalogCollection catalogCollection)
        {
            this.catalogObject = catalogObject;
            this.catalogCollection = catalogCollection;
        }

        public object GetValue(string key)
        {
            return this.catalogObject.GetValue(key);
        }

        public string Name
        {
            get
            {
                return (string)(this.catalogObject.Name());
            }
        }

        public ComCatalogCollection GetCollection(string collectionName)
        {
            ICatalogCollection collection;
            collection = (ICatalogCollection)this.catalogCollection.GetCollection(
                collectionName,
                this.catalogObject.Key());
            collection.Populate();

            return new ComCatalogCollection(collection);
        }
    }

    class ComCatalogCollection
    {
        ICatalogCollection catalogCollection;

        public ComCatalogCollection(ICatalogCollection catalogCollection)
        {
            this.catalogCollection = catalogCollection;
        }

        public int Count
        {
            get
            {
                return this.catalogCollection.Count();
            }
        }

        // (Not a property because I make a new object every time.)
        public ComCatalogObject Item(int index)
        {
            ICatalogObject catalogObject;
            catalogObject = (ICatalogObject)this.catalogCollection.Item(index);

            return new ComCatalogObject(catalogObject, this.catalogCollection);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        // This is kind of a half-baked IEnumerator implementation. It
        // lets you use foreach(), but don't expect fancy things like
        // InvalidOperationExceptions and such.
        //
        public struct Enumerator
        {
            ComCatalogCollection collection;
            ComCatalogObject current;
            int count;

            public Enumerator(ComCatalogCollection collection)
            {
                this.collection = collection;
                this.current = null;
                this.count = -1;
            }

            public ComCatalogObject Current
            {
                get { return this.current; }
            }

            public bool MoveNext()
            {
                this.count++;
                if (this.count >= collection.Count)
                    return false;

                this.current = this.collection.Item(this.count);
                return true;
            }

            public void Reset()
            {
                this.count = -1;
            }
        }
    }

    internal static class CatalogUtil
    {
        internal static string[] GetRoleMembers(
            ComCatalogObject application,
            ComCatalogCollection rolesCollection)
        {
            ComCatalogCollection applicationRoles;
            applicationRoles = application.GetCollection("Roles");

            // This is inefficient. If it turns into a
            // performance problem, then we'll need to put a cache in
            // somewhere.
            //
            List<string> roleMembers = new List<string>();
            foreach (ComCatalogObject role in rolesCollection)
            {
                string roleName = (string)role.GetValue("Name");

                // Find the role in the app roles list.
                //
                foreach (ComCatalogObject appRole in applicationRoles)
                {
                    string appRoleName = (string)appRole.GetValue("Name");
                    if (roleName == appRoleName)
                    {
                        // Found it, put all of the user names into
                        // the role members list.
                        //
                        ComCatalogCollection users;
                        users = appRole.GetCollection("UsersInRole");
                        foreach (ComCatalogObject userObject in users)
                        {
                            string user = (string)userObject.GetValue("User");
                            roleMembers.Add(user);
                        }

                        break;
                    }
                }
            }

            return roleMembers.ToArray();
        }

        internal static ComCatalogObject FindApplication(Guid applicationId)
        {
            ICatalog2 catalog = (ICatalog2)(new xCatalog());

            ICatalogObject appObject = null;
            ICatalogCollection partitionCollection = null;

            try
            {
                partitionCollection = (ICatalogCollection)catalog.GetCollection(
                    "Partitions");
                partitionCollection.Populate();
            }
            catch (COMException comException)
            {
                if (comException.ErrorCode != HR.COMADMIN_E_PARTITIONS_DISABLED)
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(comException);
            }

            if (partitionCollection != null)
            {
                for (int i = 0; i < partitionCollection.Count(); i++)
                {
                    ICatalogObject partition;
                    partition = (ICatalogObject)partitionCollection.Item(i);

                    ICatalogCollection appCollection;
                    appCollection = (ICatalogCollection)partitionCollection.GetCollection(
                        "Applications",
                        partition.Key());
                    appCollection.Populate();

                    appObject = FindApplication(appCollection, applicationId);
                    if (appObject != null)
                        return new ComCatalogObject(appObject, appCollection);
                }
            }
            else
            {
                ICatalogCollection appCollection;
                appCollection = (ICatalogCollection)catalog.GetCollection(
                    "Applications");
                appCollection.Populate();

                appObject = FindApplication(appCollection, applicationId);
                if (appObject != null)
                    return new ComCatalogObject(appObject, appCollection);
            }

            return null;
        }

        static ICatalogObject FindApplication(ICatalogCollection appCollection,
                                              Guid applicationId)
        {
            ICatalogObject appObject = null;

            for (int i = 0; i < appCollection.Count(); i++)
            {
                appObject = (ICatalogObject)appCollection.Item(i);
                Guid id = Fx.CreateGuid((string)appObject.GetValue("ID"));

                if (id == applicationId)
                    return appObject;
            }

            return null;
        }
    }
}
