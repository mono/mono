// ValueConstrainedArrayList.cs
// John Barnette (jbarn@httcb.net)
// 
// Copyright (c) 2002 John Barnette
//
// This file is part of Monodoc, a multilingual API documentation tool.
//
// Monodoc is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// Monodoc is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Monodoc; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA

using System;
using System.Collections;

namespace Mono.Doc.Core
{
	public class ValueConstrainedArrayList : ArrayList
	{
		#region Protected Instance Fields

		protected Type allowed;

		#endregion // Protected Instance Fields

		#region Constructors and Destructors

		public ValueConstrainedArrayList(Type allowed) : base()
		{
			if (allowed == null)
			{
				throw new ArgumentNullException("allowed", "Specified Type constraint cannot be null.");
			}

			this.allowed = allowed;
		}

		public ValueConstrainedArrayList(Type allowed, ICollection c)
		{
			if (allowed == null)
			{
				throw new ArgumentNullException("allowed", "Specified Type constraint cannot be null.");
			}

			this.allowed = allowed;
			
			this.AddRange(c);
		}

		public ValueConstrainedArrayList(Type allowed, int capacity) : base(capacity)
		{
			if (allowed == null)
			{
				throw new ArgumentNullException("allowed", "Specified Type constraint cannot be null.");
			}

			this.allowed = allowed;
		}

		#endregion // Constructors and Destructors

		#region Public Instance Methods

		public override int Add(object value)
		{
			if (allowed != value.GetType())
			{
				throw new ArgumentException("Values in constrained collection must be of type " + allowed.ToString() +
					".", "value"
					);
			}

			return base.Add(value);
		}

		public override void AddRange(ICollection c)
		{
			foreach (object o in c)
			{
				if (allowed != o.GetType())
				{
					throw new ArgumentException("Values in constrained collection must be of type " +
						allowed.ToString() + ".", "c"
						);
				}
			}

			base.AddRange(c);
		}

		public override void Insert(int index, object value)
		{
			if (allowed != value.GetType())
			{
				throw new ArgumentException("Values in constrained collection must be of type " + allowed.ToString() +
					".", "value"
					);
			}

			base.Insert(index, value);
		}

		public override void InsertRange(int index, ICollection c)
		{
			foreach (object o in c)
			{
				if (allowed != o.GetType())
				{
					throw new ArgumentException("Values in constrained collection must be of type " +
						allowed.ToString() + ".", "c"
						);
				}
			}
			
			base.InsertRange(index, c);
		}

		public override void SetRange(int index, ICollection c)
		{
			foreach (object o in c)
			{
				if (allowed != o.GetType())
				{
					throw new ArgumentException("Values in constrained collection must be of type " +
						allowed.ToString() + ".", "c"
						);
				}
			}
			
			base.SetRange(index, c);
		}

		#endregion // Public Instance Methods

		#region Public Instance Properties

		public override object this[int index]
		{
			get { return base[index]; }
			set
			{
				if (allowed != value.GetType())
				{
					throw new ArgumentException("Values in constrained collection must be of type " +
						allowed.ToString() + "." + "value"
						);
				}

				base[index] = value;
			}
		}

		#endregion // Public Instance Properties
	}
}
