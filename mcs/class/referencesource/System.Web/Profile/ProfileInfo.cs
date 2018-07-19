//------------------------------------------------------------------------------
// <copyright file="ProfileInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
/*
 * ProfileInfo
 *
 * Copyright (c) 2002 Microsoft Corporation
 */
namespace System.Web.Profile
{
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Web.Configuration;
    using System.Web.Util;
    using System.Web.Security;
    using System.Web.Compilation;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Reflection;
    using System.CodeDom;

    [Serializable]
    public class ProfileInfo
    {
        public ProfileInfo(string username, bool isAnonymous, DateTime lastActivityDate, DateTime lastUpdatedDate, int size)
        {
            if( username != null )
            {
                username = username.Trim();
            }

            _UserName = username;
            if (lastActivityDate.Kind == DateTimeKind.Local) {
                lastActivityDate = lastActivityDate.ToUniversalTime();
            }
            _LastActivityDate = lastActivityDate;
            if (lastUpdatedDate.Kind == DateTimeKind.Local) {
                lastUpdatedDate = lastUpdatedDate.ToUniversalTime();
            }
            _LastUpdatedDate = lastUpdatedDate;
            _IsAnonymous = isAnonymous;
            _Size = size;
        }

        protected ProfileInfo() { }

        public virtual string    UserName         { get { return _UserName;} }
        public virtual DateTime  LastActivityDate { get { return _LastActivityDate.ToLocalTime();} }
        public virtual DateTime  LastUpdatedDate  { get { return _LastUpdatedDate.ToLocalTime(); } }
        public virtual bool      IsAnonymous      { get { return _IsAnonymous;} }
        public virtual int       Size             { get { return _Size; } }


        private string   _UserName;
        private DateTime _LastActivityDate;
        private DateTime _LastUpdatedDate;
        private bool     _IsAnonymous;
        private int      _Size;
    }

    [Serializable]
    public sealed class ProfileInfoCollection : IEnumerable, ICollection
    {
        private Hashtable _Hashtable  = null;
        private ArrayList _ArrayList  = null;
        private bool _ReadOnly        = false;
        private int _CurPos = 0;
        private int _NumBlanks = 0;


        public ProfileInfoCollection() {
            _Hashtable = new Hashtable(10, StringComparer.CurrentCultureIgnoreCase);
            _ArrayList = new ArrayList();
        }
        public void Add(ProfileInfo profileInfo)
        {
            if (_ReadOnly)
                throw new NotSupportedException();

            if (profileInfo == null || profileInfo.UserName == null)
                throw new ArgumentNullException( "profileInfo" );
            _Hashtable.Add(profileInfo.UserName, _CurPos);
            _ArrayList.Add(profileInfo);
            _CurPos++;
        }
        public void Remove(string name)
        {
            if (_ReadOnly)
                throw new NotSupportedException();
            object pos = _Hashtable[name];
            if (pos == null)
                return;
            _Hashtable.Remove(name);
            _ArrayList[(int)pos] = null;
            _NumBlanks++;
        }
        public ProfileInfo this[string name]
        {
            get {
                object pos = _Hashtable[name];
                if (pos == null)
                    return null;
                return _ArrayList[(int)pos] as ProfileInfo;
            }
        }

        public IEnumerator GetEnumerator() {
            DoCompact();
            return _ArrayList.GetEnumerator();
        }

        public void SetReadOnly() {
            if (_ReadOnly)
                return;
            _ReadOnly = true;
        }
        public void Clear() {
            if (_ReadOnly)
                throw new NotSupportedException();
            _Hashtable.Clear();
            _ArrayList.Clear();
            _CurPos = 0;
            _NumBlanks = 0;
        }
        public int Count { get { return _Hashtable.Count; } }
        public bool IsSynchronized {get { return false;}}
        public object SyncRoot {get { return this;}}
        public void CopyTo(Array array, int index)
        {
            DoCompact();
            _ArrayList.CopyTo(array, index);
        }
        public void CopyTo(ProfileInfo [] array, int index)
        {
            DoCompact();
            _ArrayList.CopyTo(array, index);
        }

        private void DoCompact() {
            if (_NumBlanks < 1)
                return;
            ArrayList al = new ArrayList(_CurPos - _NumBlanks);
            int firstBlankPos = -1;
            for (int iter = 0; iter < _CurPos; iter++) {
                if (_ArrayList[iter] != null)
                    al.Add(_ArrayList[iter]);
                else if (firstBlankPos == -1)
                    firstBlankPos = iter;
            }
            _NumBlanks = 0;
            _ArrayList = al;
            _CurPos = _ArrayList.Count;
            for (int iter = firstBlankPos; iter < _CurPos; iter++) {
                ProfileInfo profileInfo = _ArrayList[iter] as ProfileInfo;
                _Hashtable[profileInfo.UserName] = iter;
            }
        }
    }
}
