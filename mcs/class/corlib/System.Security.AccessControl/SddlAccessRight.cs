//
// System.Security.AccessControl.SddlAccessRight.cs
//
// Author:
//	Kenneth Bell
//

using System.Collections.Generic;

namespace System.Security.AccessControl
{
	internal class SddlAccessRight
	{
		public string Name { get; set; }
		public int Value { get; set; }
		public int ObjectType { get; set; }

		public static SddlAccessRight LookupByName(string s)
		{
			foreach (var right in rights) {
				if (right.Name == s)
					return right;
			}
			
			return null;
		}
		
		public static SddlAccessRight[] Decompose(int mask)
		{
			foreach (var right in rights) {
				if (mask == right.Value)
					return new SddlAccessRight[] {right};
			}
			
			int foundType = 0;
			List<SddlAccessRight> found = new List<SddlAccessRight>();
			int accountedBits = 0;
			foreach (var right in rights) {
				if ((mask & right.Value) == right.Value
				    && (accountedBits | right.Value) != accountedBits) {
					
					if (foundType == 0)
						foundType = right.ObjectType;
					
					if(right.ObjectType != 0
					   && foundType != right.ObjectType)
						return null;

					found.Add(right);
					accountedBits |= right.Value;
				}
				
				if (accountedBits == mask)
				{
					return found.ToArray();
				}
			}
			
			return null;
		}

		private static readonly SddlAccessRight[] rights = new SddlAccessRight[] {
			new SddlAccessRight { Name = "CC", Value = 0x00000001, ObjectType = 1},
			new SddlAccessRight { Name = "DC", Value = 0x00000002, ObjectType = 1},
			new SddlAccessRight { Name = "LC", Value = 0x00000004, ObjectType = 1},
			new SddlAccessRight { Name = "SW", Value = 0x00000008, ObjectType = 1},
			new SddlAccessRight { Name = "RP", Value = 0x00000010, ObjectType = 1},
			new SddlAccessRight { Name = "WP", Value = 0x00000020, ObjectType = 1},
			new SddlAccessRight { Name = "DT", Value = 0x00000040, ObjectType = 1},
			new SddlAccessRight { Name = "LO", Value = 0x00000080, ObjectType = 1},
			new SddlAccessRight { Name = "CR", Value = 0x00000100, ObjectType = 1},
			
			new SddlAccessRight { Name = "SD", Value = 0x00010000},
			new SddlAccessRight { Name = "RC", Value = 0x00020000},
			new SddlAccessRight { Name = "WD", Value = 0x00040000},
			new SddlAccessRight { Name = "WO", Value = 0x00080000},
			
			new SddlAccessRight { Name = "GA", Value = 0x10000000},
			new SddlAccessRight { Name = "GX", Value = 0x20000000},
			new SddlAccessRight { Name = "GW", Value = 0x40000000},
			new SddlAccessRight { Name = "GR", Value = unchecked((int)0x80000000)},

			new SddlAccessRight { Name = "FA", Value = 0x001F01FF, ObjectType = 2},
			new SddlAccessRight { Name = "FR", Value = 0x00120089, ObjectType = 2},
			new SddlAccessRight { Name = "FW", Value = 0x00120116, ObjectType = 2},
			new SddlAccessRight { Name = "FX", Value = 0x001200A0, ObjectType = 2},
			
			new SddlAccessRight { Name = "KA", Value = 0x000F003F, ObjectType = 3},
			new SddlAccessRight { Name = "KR", Value = 0x00020019, ObjectType = 3},
			new SddlAccessRight { Name = "KW", Value = 0x00020006, ObjectType = 3},
			new SddlAccessRight { Name = "KX", Value = 0x00020019, ObjectType = 3},

			new SddlAccessRight { Name = "NW", Value = 0x00000001},
			new SddlAccessRight { Name = "NR", Value = 0x00000002},
			new SddlAccessRight { Name = "NX", Value = 0x00000004},
		};
	}
}
