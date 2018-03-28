//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Text;

    class QualifiedId : IEquatable<QualifiedId>
    {
        byte[] compressedId;

        public QualifiedId(Activity element)
        {
            int bufferSize = 0;

            Stack<int> ids = new Stack<int>();
            
            int id = element.InternalId;
            bufferSize += GetEncodedSize(id);
            ids.Push(id);

            IdSpace space = element.MemberOf;

            while (space != null && space.ParentId != 0)
            {
                bufferSize += GetEncodedSize(space.ParentId);
                ids.Push(space.ParentId);

                space = space.Parent;
            }

            this.compressedId = new byte[bufferSize];

            int offset = 0;
            while (ids.Count > 0)
            {
                offset += Encode(ids.Pop(), this.compressedId, offset);
            }
        }

        public QualifiedId(byte[] bytes)
        {
            this.compressedId = bytes;
        }

        public QualifiedId(int[] idArray)
        {
            int bufferSize = 0;

            for (int i = 0; i < idArray.Length; i++)
            {
                bufferSize += GetEncodedSize(idArray[i]);
            }

            this.compressedId = new byte[bufferSize];

            int offset = 0;
            for (int i = 0; i < idArray.Length; i++)
            {
                offset += Encode(idArray[i], this.compressedId, offset);
            }
        }

        public static bool TryGetElementFromRoot(Activity root, QualifiedId id, out Activity targetElement)
        {
            return TryGetElementFromRoot(root, id.compressedId, out targetElement);
        }

        public static bool TryGetElementFromRoot(Activity root, byte[] idBytes, out Activity targetElement)
        {
            Fx.Assert(root.MemberOf != null, "We need to have our IdSpaces set up for this to work.");

            Activity currentActivity = root;
            IdSpace currentIdSpace = root.MemberOf;

            int offset = 0;
            while (offset < idBytes.Length)
            {
                int value;
                offset += Decode(idBytes, offset, out value);

                if (currentIdSpace == null)
                {
                    targetElement = null;
                    return false;
                }

                currentActivity = currentIdSpace[value];

                if (currentActivity == null)
                {
                    targetElement = null;
                    return false;
                }

                currentIdSpace = currentActivity.ParentOf;
            }

            targetElement = currentActivity;
            return true;
        }

        public static QualifiedId Parse(string value)
        {
            QualifiedId result;
            if (!TryParse(value, out result))
            {
                throw FxTrace.Exception.AsError(new FormatException(SR.InvalidActivityIdFormat));
            }

            return result;
        }

        public static bool TryParse(string value, out QualifiedId result)
        {
            Fx.Assert(!string.IsNullOrEmpty(value), "We should have already made sure it isn't null or empty.");

            string[] idStrings = value.Split('.');
            int[] ids = new int[idStrings.Length];
            int bufferSize = 0;

            for (int i = 0; i < idStrings.Length; i++)
            {
                // only support non-negative integers as id segments
                int parsedInt;
                if (!int.TryParse(idStrings[i], out parsedInt) || parsedInt < 0)
                {
                    result = null;
                    return false;
                }

                ids[i] = parsedInt;
                bufferSize += GetEncodedSize(ids[i]);
            }

            byte[] bytes = new byte[bufferSize];
            int offset = 0;

            for (int i = 0; i < ids.Length; i++)
            {
                offset += Encode(ids[i], bytes, offset);
            }

            result = new QualifiedId(bytes);
            return true;
        }

        public static bool Equals(byte[] lhs, byte[] rhs)
        {
            if (lhs.Length == rhs.Length)
            {
                for (int i = 0; i < lhs.Length; i++)
                {
                    if (lhs[i] != rhs[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public byte[] AsByteArray()
        {
            // Note that we don't do a copy because we assume all users will
            // treat it as immutable.
            return this.compressedId;
        }

        public int[] AsIDArray()
        {
            List<int> tmpList = new List<int>();
            int offset = 0;
            while (offset < this.compressedId.Length)
            {
                int value;
                offset += Decode(this.compressedId, offset, out value);

                tmpList.Add(value);
            }
            return tmpList.ToArray();
        }

        public bool Equals(QualifiedId rhs)
        {
            return Equals(this.compressedId, rhs.compressedId);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            bool needDot = false;
            int offset = 0;
            while (offset < this.compressedId.Length)
            {
                if (needDot)
                {
                    builder.Append('.');   
                }

                int value;
                offset += Decode(this.compressedId, offset, out value);

                builder.Append(value);

                needDot = true;
            }

            return builder.ToString();
        }

        // This is the same Encode/Decode logic as the WCF FramingEncoder
        static int Encode(int value, byte[] bytes, int offset)
        {
            Fx.Assert(value >= 0, "Must be non-negative");

            int count = 1;
            while ((value & 0xFFFFFF80) != 0)
            {
                bytes[offset++] = (byte)((value & 0x7F) | 0x80);
                count++;
                value >>= 7;
            }
            bytes[offset] = (byte)value;
            return count;
        }

        // This is the same Encode/Decode logic as the WCF FramingEncoder
        static int Decode(byte[] buffer, int offset, out int value)
        {
            int bytesConsumed = 0;
            value = 0;
            
            while (offset < buffer.Length)
            {
                int next = buffer[offset];
                value |= (next & 0x7F) << (bytesConsumed * 7);
                bytesConsumed++;
                if ((next & 0x80) == 0)
                {
                    break;
                }
                offset++;
            }

            return bytesConsumed;
        }

        static int GetEncodedSize(int value)
        {
            Fx.Assert(value >= 0, "Must be non-negative");

            int count = 1;
            while ((value & 0xFFFFFF80) != 0)
            {
                count++;
                value >>= 7;
            }
            return count;
        }
    }
}


