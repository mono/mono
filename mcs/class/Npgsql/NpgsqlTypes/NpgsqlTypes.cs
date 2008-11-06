// NpgsqlTypes.NpgsqlTypesHelper.cs
//
// Author:
//    Glen Parker <glenebob@nwlink.com>
//
//    Copyright (C) 2004 The Npgsql Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
// 
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

// This file provides implementations of PostgreSQL specific data types that cannot
// be mapped to standard .NET classes.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Npgsql;

namespace NpgsqlTypes
{
    /// <summary>
    /// Represents a PostgreSQL Point type
    /// </summary>
    public struct NpgsqlPoint : IEquatable<NpgsqlPoint>
    {
        private Single _x;
        private Single _y;

        public NpgsqlPoint(Single x, Single y)
        {
            _x = x;
            _y = y;
        }
        
        public Single X
        {
            get
            {
                return _x;
            }
            
            set
            {
                _x = value;
            }
        }
        
        public Single Y
        {
            get
            {
                return _y;
            }
            
            set
            {
                _y = value;
            }
        }


        public bool Equals(NpgsqlPoint other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is NpgsqlPoint && Equals((NpgsqlPoint) obj);
        }

        public static bool operator ==(NpgsqlPoint x, NpgsqlPoint y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NpgsqlPoint x, NpgsqlPoint y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ PGUtil.RotateShift(Y.GetHashCode(), sizeof (int)/2);
        }
    }

    public struct NpgsqlBox : IEquatable<NpgsqlBox>
    {
        
        private NpgsqlPoint _upperRight;
        private NpgsqlPoint _lowerLeft;
        
        public NpgsqlBox(NpgsqlPoint upperRight, NpgsqlPoint lowerLeft)
        {
            _upperRight = upperRight;
            _lowerLeft = lowerLeft;
        }

        public NpgsqlBox(float Top, float Right, float Bottom, float Left)
            : this(new NpgsqlPoint(Right, Top), new NpgsqlPoint(Left, Bottom))
        {
        }

        public NpgsqlPoint UpperRight
        {
            get
            {
                return _upperRight;
            }
            
            set
            {
                _upperRight = value;
            }
            
        }
        
        public NpgsqlPoint LowerLeft
        {
            get
            {
                return _lowerLeft;
            }
            
            set
            {
                _lowerLeft = value;
            }
            
        }
        
        public float Left
        {
            get { return LowerLeft.X; }
        }

        public float Right
        {
            get { return UpperRight.X; }
        }

        public float Bottom
        {
            get { return LowerLeft.Y; }
        }

        public float Top
        {
            get { return UpperRight.Y; }
        }

        public float Width
        {
            get { return Right - Left; }
        }

        public float Height
        {
            get { return Top - Bottom; }
        }

        public bool IsEmpty
        {
            get { return Width == 0 || Height == 0; }
        }

        public bool Equals(NpgsqlBox other)
        {
            return UpperRight == other.UpperRight && LowerLeft == other.LowerLeft;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is NpgsqlBox && Equals((NpgsqlBox) obj);
        }

        public static bool operator ==(NpgsqlBox x, NpgsqlBox y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NpgsqlBox x, NpgsqlBox y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return
                Top.GetHashCode() ^ PGUtil.RotateShift(Right.GetHashCode(), sizeof (int)/4) ^
                PGUtil.RotateShift(Bottom.GetHashCode(), sizeof (int)/2) ^
                PGUtil.RotateShift(LowerLeft.GetHashCode(), sizeof (int)*3/4);
        }
    }


    /// <summary>
    /// Represents a PostgreSQL Line Segment type.
    /// </summary>
    public struct NpgsqlLSeg : IEquatable<NpgsqlLSeg>
    {
        public NpgsqlPoint Start;
        public NpgsqlPoint End;

        public NpgsqlLSeg(NpgsqlPoint start, NpgsqlPoint end)
        {
            Start = start;
            End = end;
        }

        public override String ToString()
        {
            return String.Format("({0}, {1})", Start, End);
        }

        public override int GetHashCode()
        {
            return
                Start.X.GetHashCode() ^ PGUtil.RotateShift(Start.Y.GetHashCode(), sizeof (int)/4) ^
                PGUtil.RotateShift(End.X.GetHashCode(), sizeof (int)/2) ^ PGUtil.RotateShift(End.Y.GetHashCode(), sizeof (int)*3/4);
        }

        public bool Equals(NpgsqlLSeg other)
        {
            return Start == other.Start && End == other.End;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is NpgsqlLSeg && Equals((NpgsqlLSeg) obj);
        }

        public static bool operator ==(NpgsqlLSeg x, NpgsqlLSeg y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NpgsqlLSeg x, NpgsqlLSeg y)
        {
            return !(x == y);
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Path type.
    /// </summary>
    public struct NpgsqlPath : IList<NpgsqlPoint>, IEquatable<NpgsqlPath>
    {
        private bool _open;
        private readonly List<NpgsqlPoint> _points;

        public NpgsqlPath(IEnumerable<NpgsqlPoint> points, bool open)
        {
            _points = new List<NpgsqlPoint>(points);
            _open = open;
        }

        public NpgsqlPath(IEnumerable<NpgsqlPoint> points)
            : this(points, false)
        {
        }
        
        public NpgsqlPath(NpgsqlPoint[] points) : this((IEnumerable<NpgsqlPoint>)points, false)
        {
        }

        public NpgsqlPath(bool open)
        {
            _points = new List<NpgsqlPoint>();
            _open = open;
        }

        public NpgsqlPath(int capacity, bool open)
        {
            _points = new List<NpgsqlPoint>(capacity);
            _open = open;
        }

        public NpgsqlPath(int capacity)
            : this(capacity, false)
        {
        }
        
        public bool Open
        {
            get
            {
                return _open;
            }
            
            set
            {
                _open = value;
            }
        }

        public NpgsqlPoint this[int index]
        {
            get { return _points[index]; }
            set { _points[index] = value; }
        }

        public int Count
        {
            get { return _points.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(NpgsqlPoint item)
        {
            return _points.IndexOf(item);
        }

        public void Insert(int index, NpgsqlPoint item)
        {
            _points.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _points.RemoveAt(index);
        }

        public void Add(NpgsqlPoint item)
        {
            _points.Add(item);
        }

        public void Clear()
        {
            _points.Clear();
        }

        public bool Contains(NpgsqlPoint item)
        {
            return _points.Contains(item);
        }

        public void CopyTo(NpgsqlPoint[] array, int arrayIndex)
        {
            _points.CopyTo(array, arrayIndex);
        }

        public bool Remove(NpgsqlPoint item)
        {
            return _points.Remove(item);
        }

        public IEnumerator<NpgsqlPoint> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(NpgsqlPath other)
        {
            if (Open != other.Open || Count != other.Count)
            {
                return false;
            }
            for (int i = 0; i != Count; ++i)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is NpgsqlPath && Equals((NpgsqlPath) obj);
        }

        public static bool operator ==(NpgsqlPath x, NpgsqlPath y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NpgsqlPath x, NpgsqlPath y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            int ret = 0;
            foreach (NpgsqlPoint point in this)
            {
                //The ideal amount to shift each value is one that would evenly spread it throughout
                //the resultant bytes. Using the current result % 32 is essentially using a random value
                //but one that will be the same on subsequent calls.
                ret ^= PGUtil.RotateShift(point.GetHashCode(), ret%sizeof (int));
            }
            return Open ? ret : -ret;
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Polygon type.
    /// </summary>
    public struct NpgsqlPolygon : IList<NpgsqlPoint>, IEquatable<NpgsqlPolygon>
    {
        private readonly List<NpgsqlPoint> _points;

        public NpgsqlPolygon(IEnumerable<NpgsqlPoint> points)
        {
            _points = new List<NpgsqlPoint>(points);
        }
        
        public NpgsqlPolygon(NpgsqlPoint[] points) : this ((IEnumerable<NpgsqlPoint>) points)
        {
        }

        public NpgsqlPoint this[int index]
        {
            get { return _points[index]; }
            set { _points[index] = value; }
        }

        public int Count
        {
            get { return _points.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int IndexOf(NpgsqlPoint item)
        {
            return _points.IndexOf(item);
        }

        public void Insert(int index, NpgsqlPoint item)
        {
            _points.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _points.RemoveAt(index);
        }

        public void Add(NpgsqlPoint item)
        {
            _points.Add(item);
        }

        public void Clear()
        {
            _points.Clear();
        }

        public bool Contains(NpgsqlPoint item)
        {
            return _points.Contains(item);
        }

        public void CopyTo(NpgsqlPoint[] array, int arrayIndex)
        {
            _points.CopyTo(array, arrayIndex);
        }

        public bool Remove(NpgsqlPoint item)
        {
            return _points.Remove(item);
        }

        public IEnumerator<NpgsqlPoint> GetEnumerator()
        {
            return _points.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Equals(NpgsqlPolygon other)
        {
            if (Count != other.Count)
            {
                return false;
            }
            for (int i = 0; i != Count; ++i)
            {
                if (this[i] != other[i])
                {
                    return false;
                }
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is NpgsqlPolygon && Equals((NpgsqlPolygon) obj);
        }

        public static bool operator ==(NpgsqlPolygon x, NpgsqlPolygon y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NpgsqlPolygon x, NpgsqlPolygon y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            int ret = 0;
            foreach (NpgsqlPoint point in this)
            {
                //The ideal amount to shift each value is one that would evenly spread it throughout
                //the resultant bytes. Using the current result % 32 is essentially using a random value
                //but one that will be the same on subsequent calls.
                ret ^= PGUtil.RotateShift(point.GetHashCode(), ret%sizeof (int));
            }
            return ret;
        }
    }

    /// <summary>
    /// Represents a PostgreSQL Circle type.
    /// </summary>
    public struct NpgsqlCircle : IEquatable<NpgsqlCircle>
    {
        public NpgsqlPoint Center;
        public Double Radius;

        public NpgsqlCircle(NpgsqlPoint center, Double radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Equals(NpgsqlCircle other)
        {
            return Center == other.Center && Radius == other.Radius;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is NpgsqlCircle && Equals((NpgsqlCircle) obj);
        }

        public override String ToString()
        {
            return string.Format("({0}), {1}", Center, Radius);
        }

        public static bool operator ==(NpgsqlCircle x, NpgsqlCircle y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NpgsqlCircle x, NpgsqlCircle y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return
                Center.X.GetHashCode() ^ PGUtil.RotateShift(Center.Y.GetHashCode(), sizeof (int)/4) ^
                PGUtil.RotateShift(Radius.GetHashCode(), sizeof (int)/2);
        }
    }


    /// <summary>
    /// Represents a PostgreSQL inet type.
    /// </summary>
    public struct NpgsqlInet : IEquatable<NpgsqlInet>
    {
        public IPAddress addr;
        public int mask;

        public NpgsqlInet(IPAddress addr, int mask)
        {
            this.addr = addr;
            this.mask = mask;
        }

        public NpgsqlInet(IPAddress addr)
        {
            this.addr = addr;
            this.mask = 32;
        }

        public NpgsqlInet(string addr)
        {
            if (addr.IndexOf('/') > 0)
            {
                string[] addrbits = addr.Split('/');
                if (addrbits.GetUpperBound(0) != 1)
                {
                    throw new FormatException("Invalid number of parts in CIDR specification");
                }
                this.addr = IPAddress.Parse(addrbits[0]);
                this.mask = int.Parse(addrbits[1]);
            }
            else
            {
                this.addr = IPAddress.Parse(addr);
                this.mask = 32;
            }
        }

        public override String ToString()
        {
            if (mask != 32)
            {
                return string.Format("{0}/{1}", addr, mask);
            }
                return addr.ToString();
            
        }

        public static implicit operator IPAddress(NpgsqlInet x)
        {
            if (x.mask != 32)
            {
                throw new InvalidCastException("Cannot cast CIDR network to address");
            }
                return x.addr;
            
        }

        public bool Equals(NpgsqlInet other)
        {
            return addr.Equals(other.addr) && mask == other.mask;
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is NpgsqlInet && Equals((NpgsqlInet) obj);
        }

        public override int GetHashCode()
        {
            return PGUtil.RotateShift(addr.GetHashCode(), mask%32);
        }

        public static bool operator ==(NpgsqlInet x, NpgsqlInet y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NpgsqlInet x, NpgsqlInet y)
        {
            return !(x == y);
        }
    }
}