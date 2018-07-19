using System;
using System.Collections.Specialized;
using System.Net.Mail;
using System.Globalization;
using System.Collections.Generic;

namespace System.Net.Mail {

    // Enumeration of the well-known headers.
    // If you add to this enum you MUST also add the appropriate initializer in m_HeaderInfo below.
    internal enum MailHeaderID {
        Bcc = 0,
        Cc,
        Comments,
        ContentDescription,
        ContentDisposition,
        ContentID,
        ContentLocation,
        ContentTransferEncoding,
        ContentType,
        Date,
        From,
        Importance,
        InReplyTo,
        Keywords,
        Max,
        MessageID,
        MimeVersion,
        Priority,
        References,
        ReplyTo,
        ResentBcc,
        ResentCc,
        ResentDate,
        ResentFrom,
        ResentMessageID,
        ResentSender,
        ResentTo,
        Sender,
        Subject,
        To,
        XPriority,
        XReceiver,
        XSender,
        ZMaxEnumValue = XSender,  // Keep this to equal to the last "known" enum entry if you add to the end
        Unknown = -1
    }

    internal static class MailHeaderInfo {

        // Structure that wraps information about a single mail header
        private struct HeaderInfo {
            public readonly string NormalizedName;
            public readonly bool IsSingleton;
            public readonly MailHeaderID ID;
            public readonly bool IsUserSettable;
            public readonly bool AllowsUnicode;
            public HeaderInfo(MailHeaderID id, string name, bool isSingleton, bool isUserSettable, bool allowsUnicode) {
                ID = id;
                NormalizedName = name;
                IsSingleton = isSingleton;
                IsUserSettable = isUserSettable;
                AllowsUnicode = allowsUnicode;
            }
        }



        // Table of well-known mail headers.
        // Keep the initializers in sync with the enum above.
        private static readonly HeaderInfo[] m_HeaderInfo = {
            //             ID                                     NormalizedString             IsSingleton      IsUserSettable      AllowsUnicode
            new HeaderInfo(MailHeaderID.Bcc,                      "Bcc",                       true,            false,              true),
            new HeaderInfo(MailHeaderID.Cc,                       "Cc",                        true,            false,              true),
            new HeaderInfo(MailHeaderID.Comments,                 "Comments",                  false,           true,               true),
            new HeaderInfo(MailHeaderID.ContentDescription,       "Content-Description",       true,            true,               true),
            new HeaderInfo(MailHeaderID.ContentDisposition,       "Content-Disposition",       true,            true,               true),
            new HeaderInfo(MailHeaderID.ContentID,                "Content-ID",                true,            false,              false),
            new HeaderInfo(MailHeaderID.ContentLocation,          "Content-Location",          true,            false,              true),
            new HeaderInfo(MailHeaderID.ContentTransferEncoding,  "Content-Transfer-Encoding", true,            false,              false),
            new HeaderInfo(MailHeaderID.ContentType,              "Content-Type",              true,            false,              false),
            new HeaderInfo(MailHeaderID.Date,                     "Date",                      true,            false,              false),
            new HeaderInfo(MailHeaderID.From,                     "From",                      true,            false,              true),
            new HeaderInfo(MailHeaderID.Importance,               "Importance",                true,            false,              false),
            new HeaderInfo(MailHeaderID.InReplyTo,                "In-Reply-To",               true,            true,               false),
            new HeaderInfo(MailHeaderID.Keywords,                 "Keywords",                  false,           true,               true),
            new HeaderInfo(MailHeaderID.Max,                      "Max",                       false,           true,               false),
            new HeaderInfo(MailHeaderID.MessageID,                "Message-ID",                true,            true,               false),
            new HeaderInfo(MailHeaderID.MimeVersion,              "MIME-Version",              true,            false,              false),
            new HeaderInfo(MailHeaderID.Priority,                 "Priority",                  true,            false,              false),
            new HeaderInfo(MailHeaderID.References,               "References",                true,            true,               false),
            new HeaderInfo(MailHeaderID.ReplyTo,                  "Reply-To",                  true,            false,              true),
            new HeaderInfo(MailHeaderID.ResentBcc,                "Resent-Bcc",                false,           true,               true),
            new HeaderInfo(MailHeaderID.ResentCc,                 "Resent-Cc",                 false,           true,               true),
            new HeaderInfo(MailHeaderID.ResentDate,               "Resent-Date",               false,           true,               false),
            new HeaderInfo(MailHeaderID.ResentFrom,               "Resent-From",               false,           true,               true),
            new HeaderInfo(MailHeaderID.ResentMessageID,          "Resent-Message-ID",         false,           true,               false),
            new HeaderInfo(MailHeaderID.ResentSender,             "Resent-Sender",             false,           true,               true),
            new HeaderInfo(MailHeaderID.ResentTo,                 "Resent-To",                 false,           true,               true),
            new HeaderInfo(MailHeaderID.Sender,                   "Sender",                    true,            false,              true),
            new HeaderInfo(MailHeaderID.Subject,                  "Subject",                   true,            false,              true),
            new HeaderInfo(MailHeaderID.To,                       "To",                        true,            false,              true),
            new HeaderInfo(MailHeaderID.XPriority,                "X-Priority",                true,            false,              false),
            new HeaderInfo(MailHeaderID.XReceiver,                "X-Receiver",                false,           true,               true),
            new HeaderInfo(MailHeaderID.XSender,                  "X-Sender",                  true,            true,               true)
        };

        private static readonly Dictionary<string, int> m_HeaderDictionary;

        static MailHeaderInfo() {

#if DEBUG
            // Check that enum and header info array are in sync
            for(int i = 0; i < m_HeaderInfo.Length; i++) {
                if((int)m_HeaderInfo[i].ID != i) {
                    throw new Exception("Header info data structures are not in sync");
                }
            }
#endif

            // Create dictionary for string-to-enum lookup.  Ordinal and IgnoreCase are intentional.
            m_HeaderDictionary = new Dictionary<string, int>((int)MailHeaderID.ZMaxEnumValue + 1, StringComparer.OrdinalIgnoreCase);
            for(int i = 0; i < m_HeaderInfo.Length; i++) {
                m_HeaderDictionary.Add(m_HeaderInfo[i].NormalizedName, i);
            }
        }

        internal static string GetString(MailHeaderID id) {
            switch(id) {
                case MailHeaderID.Unknown:
                case MailHeaderID.ZMaxEnumValue+1:
                    return null;
                default:
                    return m_HeaderInfo[(int)id].NormalizedName;
            }
        }

        internal static MailHeaderID GetID(string name) {
            int id;
            if(m_HeaderDictionary.TryGetValue(name, out id)) {
                return (MailHeaderID)id;
            }
            return MailHeaderID.Unknown;
        }

        internal static bool IsWellKnown(string name) {
            int dummy;
            return m_HeaderDictionary.TryGetValue(name, out dummy);
        }

        internal static bool IsUserSettable(string name) {
            int index;
            if (m_HeaderDictionary.TryGetValue(name, out index)) {
                return m_HeaderInfo[index].IsUserSettable;
            }
            //values not in the list of well-known headers are always user-settable
            return true;
        }

        internal static bool IsSingleton(string name) {
            int index;
            if(m_HeaderDictionary.TryGetValue(name, out index)) {
                return m_HeaderInfo[index].IsSingleton;
            }
            return false;
        }

        internal static string NormalizeCase(string name) {
            int index;
            if(m_HeaderDictionary.TryGetValue(name, out index)) {
                return m_HeaderInfo[index].NormalizedName;
            }
            return name;
        }

        internal static bool IsMatch(string name, MailHeaderID header) {
            int index;
            if(m_HeaderDictionary.TryGetValue(name, out index) && (MailHeaderID)index == header) {
                return true;
            }
            return false;
        }

        internal static bool AllowsUnicode(string name) {
            int index;
            if (m_HeaderDictionary.TryGetValue(name, out index)) {
                return m_HeaderInfo[index].AllowsUnicode;
            }
            return true;
        }
    }
}
