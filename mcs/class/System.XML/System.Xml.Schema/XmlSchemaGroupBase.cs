//
// XmlSchemaGroupBase.cs
//
// Authors:
//	Dwivedi, Ajay kumar Adwiv@Yahoo.com
//	Atsushi Enomoto atsushi@ximian.com
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Collections;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	public abstract class XmlSchemaGroupBase : XmlSchemaParticle
	{
		private XmlSchemaObjectCollection compiledItems;

		protected XmlSchemaGroupBase ()
		{
			compiledItems = new XmlSchemaObjectCollection ();
		}

		[XmlIgnore]
		public abstract XmlSchemaObjectCollection Items { get; }

		internal XmlSchemaObjectCollection CompiledItems 
		{
			get{ return compiledItems; }
		}

		internal void CopyOptimizedItems (XmlSchemaGroupBase gb)
		{
			for (int i = 0; i < Items.Count; i++) {
				XmlSchemaParticle p = Items [i] as XmlSchemaParticle;
				p = p.GetOptimizedParticle (false);
				if (p == XmlSchemaParticle.Empty)
					continue;
				gb.Items.Add (p);
				gb.CompiledItems.Add (p);
			}
		}

		internal override bool ParticleEquals (XmlSchemaParticle other)
		{
			XmlSchemaGroupBase gb = other as XmlSchemaGroupBase;
			if (gb == null)
				return false;
			if (this.GetType () != gb.GetType ())
				return false;

			if (this.ValidatedMaxOccurs != gb.ValidatedMaxOccurs ||
				this.ValidatedMinOccurs != gb.ValidatedMinOccurs)
				return false;
			if (this.CompiledItems.Count != gb.CompiledItems.Count)
				return false;
			for (int i = 0; i < CompiledItems.Count; i++) {
				XmlSchemaParticle p1 = this.CompiledItems [i] as XmlSchemaParticle;
				XmlSchemaParticle p2 = gb.CompiledItems [i] as XmlSchemaParticle;
				if (!p1.ParticleEquals (p2))
					return false;
			}
			return true;
		}

		internal override void CheckRecursion (Stack stack, ValidationEventHandler h, XmlSchema schema)
		{
			foreach (XmlSchemaParticle p in this.Items)
				p.CheckRecursion (stack, h, schema);
		}

		internal bool ValidateNSRecurseCheckCardinality (XmlSchemaAny any,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			foreach (XmlSchemaParticle p in Items)
				if (!p.ValidateDerivationByRestriction (any, h, schema, raiseError))
					return false;
			return ValidateOccurenceRangeOK (any, h, schema, raiseError);
		}

		internal bool ValidateRecurse (XmlSchemaGroupBase baseGroup,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			return ValidateSeqRecurseMapSumCommon (baseGroup, h, schema, false, false, raiseError);
		}

		internal bool ValidateSeqRecurseMapSumCommon (XmlSchemaGroupBase baseGroup,
			ValidationEventHandler h, XmlSchema schema, bool isLax, bool isMapAndSum, bool raiseError)
		{
			int index = 0;
			int baseIndex = 0;
			decimal baseOccured = 0;
			if (baseGroup.CompiledItems.Count == 0 && this.CompiledItems.Count > 0) {
				if (raiseError)
					error (h, "Invalid particle derivation by restriction was found. base particle does not contain particles.");
				return false;
			}

			for (int i = 0; i < CompiledItems.Count; i++) {
				// get non-empty derived particle
				XmlSchemaParticle pd = null;
				while (this.CompiledItems.Count > index) {
					pd = ((XmlSchemaParticle) this.CompiledItems [index]);//.GetOptimizedParticle (false);
					if (pd != XmlSchemaParticle.Empty)// && pd.ValidatedMaxOccurs > 0)
						break;
					else
						index++;
				}
				if (index >= CompiledItems.Count) {
					if (raiseError)
						error (h, "Invalid particle derivation by restriction was found. Cannot be mapped to base particle.");
					return false;
				}

				// get non-empty base particle
				XmlSchemaParticle pb = null;
				while (baseGroup.CompiledItems.Count > baseIndex) {
					pb = ((XmlSchemaParticle) baseGroup.CompiledItems [baseIndex]);//.GetOptimizedParticle (false);
					if (pb == XmlSchemaParticle.Empty && pb.ValidatedMaxOccurs > 0)
						continue;
					if (!pd.ValidateDerivationByRestriction (pb, h, schema, false)) {
						if (!isLax && !isMapAndSum && pb.MinOccurs > baseOccured && !pb.ValidateIsEmptiable ()) {
							if (raiseError)
								error (h, "Invalid particle derivation by restriction was found. Invalid sub-particle derivation was found.");
							return false;
						}
						else {
							baseOccured = 0;
							baseIndex++;
						}
					} else {
						baseOccured += pb.ValidatedMinOccurs;
						if (baseOccured >= baseGroup.ValidatedMaxOccurs) {
							baseOccured = 0;
							baseIndex++;
						}
						index++;
						break;
					}
				}
			}
			if (this.CompiledItems.Count > 0 && index != this.CompiledItems.Count) {
				if (raiseError)
					error (h, "Invalid particle derivation by restriction was found. Extraneous derived particle was found.");
				return false;
			}
			if (!isLax && !isMapAndSum) {
				if (baseOccured > 0)
					baseIndex++;
				for (int i = baseIndex; i < baseGroup.CompiledItems.Count; i++) {
					XmlSchemaParticle p = baseGroup.CompiledItems [i] as XmlSchemaParticle;
					if (!p.ValidateIsEmptiable ()) {
						if (raiseError)
							error (h, "Invalid particle derivation by restriction was found. There is a base particle which does not have mapped derived particle and is not emptiable.");
						return false;
					}
				}
			}
			return true;
		}
	}
}
