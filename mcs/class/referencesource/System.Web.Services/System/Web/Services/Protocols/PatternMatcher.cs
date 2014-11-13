namespace System.Web.Services.Protocols {
    using System;
    using System.Reflection;
    using System.Collections;
    using System.Text.RegularExpressions;
    using System.Security.Permissions;

    /// <include file='doc\PatternMatcher.uex' path='docs/doc[@for="PatternMatcher"]/*' />
    /// <devdoc>
    ///    <para>[To be supplied.]</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.LinkDemand, Name = "FullTrust")]
    public sealed class PatternMatcher {
        MatchType matchType;

        /// <include file='doc\PatternMatcher.uex' path='docs/doc[@for="PatternMatcher.PatternMatcher"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public PatternMatcher(Type type) {
            matchType = MatchType.Reflect(type);
        }

        /// <include file='doc\PatternMatcher.uex' path='docs/doc[@for="PatternMatcher.Match"]/*' />
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public object Match(string text) {
            return matchType.Match(text);
        }
    }

    internal class MatchType {
        Type type;
        MatchMember[] fields;

        internal Type Type {
            get { return type; }
        }

        internal static MatchType Reflect(Type type) {
            MatchType matchType = new MatchType();
            matchType.type = type;

            MemberInfo[] memberInfos = type.GetMembers(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
            ArrayList list = new ArrayList();
            for (int i = 0; i < memberInfos.Length; i++) {
                MatchMember member = MatchMember.Reflect(memberInfos[i]);
                if (member != null) list.Add(member);
            }
            matchType.fields = (MatchMember[])list.ToArray(typeof(MatchMember));
            return matchType;
        }

        internal object Match(string text) {
            object target = Activator.CreateInstance(type);
            for (int i = 0; i < fields.Length; i++)
                fields[i].Match(target, text);
            return target;
        }
    }

    internal class MatchMember {
        MemberInfo memberInfo;
        Regex regex;
        int group;
        int capture;
        int maxRepeats;
        MatchType matchType;

        internal void Match(object target, string text) {
            if (memberInfo is FieldInfo)
                ((FieldInfo)memberInfo).SetValue(target, matchType == null ? MatchString(text) : MatchClass(text));
            else if (memberInfo is PropertyInfo) {
                ((PropertyInfo)memberInfo).SetValue(target, matchType == null ? MatchString(text) : MatchClass(text), new object[0]);
            }
        }

        object MatchString(string text) {
            Match m = regex.Match(text);
            Type fieldType = memberInfo is FieldInfo ? ((FieldInfo)memberInfo).FieldType : ((PropertyInfo)memberInfo).PropertyType;
            if (fieldType.IsArray) {
                ArrayList matches = new ArrayList();
                int matchCount = 0;
                while (m.Success && matchCount < maxRepeats) {
                    if (m.Groups.Count <= group) 
                        throw BadGroupIndexException(group, memberInfo.Name, m.Groups.Count - 1);
                    Group g = m.Groups[group];
                    foreach (Capture c in g.Captures) {
                        matches.Add(text.Substring(c.Index, c.Length));
                    }
                    m = m.NextMatch();
                    matchCount++;
                }
                return matches.ToArray(typeof(string));
            }
            else {
                if (m.Success) {
                    if (m.Groups.Count <= group) 
                        throw BadGroupIndexException(group, memberInfo.Name, m.Groups.Count - 1);
                    Group g = m.Groups[group];
                    if (g.Captures.Count > 0) {
                        if (g.Captures.Count <= capture) 
                            throw BadCaptureIndexException(capture, memberInfo.Name, g.Captures.Count - 1);
                        Capture c = g.Captures[capture];
                        return text.Substring(c.Index, c.Length);
                    }
                }
                return null;
            }
        }

        object MatchClass(string text) {
            Match m = regex.Match(text);
            Type fieldType = memberInfo is FieldInfo ? ((FieldInfo)memberInfo).FieldType : ((PropertyInfo)memberInfo).PropertyType;
            if (fieldType.IsArray) {
                ArrayList matches = new ArrayList();
                int matchCount = 0;
                while (m.Success && matchCount < maxRepeats) {
                    if (m.Groups.Count <= group) 
                        throw BadGroupIndexException(group, memberInfo.Name, m.Groups.Count - 1);
                    Group g = m.Groups[group];
                    foreach (Capture c in g.Captures) {
                        matches.Add(matchType.Match(text.Substring(c.Index, c.Length)));
                    }
                    m = m.NextMatch();
                    matchCount++;
                }
                return matches.ToArray(matchType.Type);
            }
            else {
                if (m.Success) {
                    if (m.Groups.Count <= group) 
                        throw BadGroupIndexException(group, memberInfo.Name, m.Groups.Count - 1);
                    Group g = m.Groups[group];
                    if (g.Captures.Count > 0) {
                        if (g.Captures.Count <= capture) 
                            throw BadCaptureIndexException(capture, memberInfo.Name, g.Captures.Count - 1);
                        Capture c = g.Captures[capture];
                        return matchType.Match(text.Substring(c.Index, c.Length));
                    }
                }
                return null;
            }
        }

        static Exception BadCaptureIndexException(int index, string matchName, int highestIndex) {
            return new Exception(Res.GetString(Res.WebTextMatchBadCaptureIndex, index, matchName, highestIndex));
        }

        static Exception BadGroupIndexException(int index, string matchName, int highestIndex) {
            return new Exception(Res.GetString(Res.WebTextMatchBadGroupIndex, index, matchName, highestIndex));
        }

        internal static MatchMember Reflect(MemberInfo memberInfo) {
            Type memberType = null;
            if (memberInfo is PropertyInfo) {
                PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
                if (!propertyInfo.CanRead)
                    return null;
                // 
                if (!propertyInfo.CanWrite)
                    return null;
                
                MethodInfo getMethod = propertyInfo.GetGetMethod();
                if (getMethod.IsStatic) 
                    return null;
                ParameterInfo[] parameters = getMethod.GetParameters();
                if (parameters.Length > 0) 
                    return null;
                memberType = propertyInfo.PropertyType;
            }
            if (memberInfo is FieldInfo) {
                FieldInfo fieldInfo = (FieldInfo)memberInfo;
                if (!fieldInfo.IsPublic) 
                    return null;
                if (fieldInfo.IsStatic)
                    return null;
                if (fieldInfo.IsSpecialName) 
                    return null;
                memberType = fieldInfo.FieldType;
            }
            object[] attrs = memberInfo.GetCustomAttributes(typeof(MatchAttribute), false);
            if (attrs.Length == 0) return null;
            MatchAttribute attr = (MatchAttribute)attrs[0];
            MatchMember member = new MatchMember();
            member.regex = new Regex(attr.Pattern, RegexOptions.Singleline | (attr.IgnoreCase ? RegexOptions.IgnoreCase | RegexOptions.CultureInvariant : 0));
            member.group = attr.Group;
            member.capture = attr.Capture;
            member.maxRepeats = attr.MaxRepeats;
            member.memberInfo = memberInfo;
            
            if (member.maxRepeats < 0) // unspecified
                member.maxRepeats = memberType.IsArray ? int.MaxValue : 1;
            if (memberType.IsArray) {
                memberType = memberType.GetElementType();
            }
            if (memberType != typeof(string)) {
                member.matchType = MatchType.Reflect(memberType);
            }
            return member;
        }
    }
}
