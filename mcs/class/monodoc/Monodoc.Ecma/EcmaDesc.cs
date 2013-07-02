using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Monodoc.Ecma
{
	/* Some properties might not be filled/meaningful depending on kind
	 * like a namespace EcmaUrl won't have a valid TypeName
	 */
	public class EcmaDesc : IEquatable<EcmaDesc>
	{
		public enum Kind
		{
			Type,
			Constructor,
			Method,
			Namespace,
			Field,
			Property,
			Event,
			Operator
		}

		public enum Mod
		{
			Normal,
			Pointer,
			Ref,
			Out
		}

		public enum Format
		{
			WithArgs,
			WithoutArgs
		}

		public Kind DescKind {
			get;
			set;
		}

		public Mod DescModifier {
			get;
			set;
		}

		public string Namespace {
			get;
			set;
		}

		public string TypeName {
			get;
			set;
		}

		public string MemberName {
			get;
			set;
		}

		public EcmaDesc NestedType {
			get;
			set;
		}

		/* A list of the array dimensions attached to this type.
		 * The list count corresponds to the number of recursive
		 * array definition (jagged arrays) the value of the
		 * corresponding list item is the number of dimension
		 * attached to that array definition instance
		 */
		public IList<int> ArrayDimensions {
			get;
			set;
		}

		/* Depending on the form of the url, we might not have the type
		 * of the argument but only how many the type/member has i.e.
		 * when such number is specified with a backtick
		 */
		public IList<EcmaDesc> GenericTypeArguments {
			get;
			set;
		}

		/* This property tells if the above collections only correct value
		 * is the number of item in it to represent generic arguments
		 */
		public bool GenericTypeArgumentsIsNumeric {
			get {
				return GenericTypeArguments != null && GenericTypeArguments.FirstOrDefault () == null;
			}
		}

		public IList<EcmaDesc> GenericMemberArguments {
			get;
			set;
		}

		public bool GenericMemberArgumentsIsNumeric {
			get {
				return GenericMemberArguments != null && GenericMemberArguments.FirstOrDefault () == null;
			}
		}

		public IList<EcmaDesc> MemberArguments {
			get;
			set;
		}

		/* This indicates that we actually want an inner part of the ecmadesc
		 * i.e. in case of T: we could want the members (*), ctor (C), methods (M), ...
		 */
		public char Etc {
			get;
			set;
		}

		public bool IsEtc {
			get {
				return Etc != (char)0;
			}
		}

		/* EtcFilter is only valid in some case of IsEtc when the inner part needs
		 * to be further filtered e.g. in case we want a listing of the type overloads
		 * Equals
		 */
		public string EtcFilter {
			get;
			set;
		}

		/* When a member is an explicit implementation of an interface member, we register
		 * the member EcmaDesc with its interface parent here
		 */
		public EcmaDesc ExplicitImplMember {
			get;
			set;
		}

		// Returns the TypeName and the generic/inner type information if existing
		public string ToCompleteTypeName (char innerTypeSeparator = '.')
		{
			var result = TypeName;
			if (GenericTypeArguments != null)
				result += FormatGenericArgs (GenericTypeArguments);
			if (NestedType != null)
				result += innerTypeSeparator + NestedType.ToCompleteTypeName ();
			if (ArrayDimensions != null && ArrayDimensions.Count > 0)
				result += ArrayDimensions.Select (dim => "[" + new string (',', dim - 1) + "]").Aggregate (string.Concat);

			return result;
		}

		// Returns the member name with its generic types if existing
		public string ToCompleteMemberName (Format format)
		{
			/* We special process two cases:
			 *   - Explicit member implementation which append a full type specification
			 *   - Conversion operator which are exposed as normal method but have specific captioning in the end
			 */
			if (ExplicitImplMember != null) {
				var impl = ExplicitImplMember;
				return impl.FormattedNamespace + impl.ToCompleteTypeName () + "." + impl.ToCompleteMemberName (format);
			} else if (format == Format.WithArgs && DescKind == Kind.Operator && MemberName.EndsWith ("Conversion")) {
				var type1 = MemberArguments[0].FormattedNamespace + MemberArguments[0].ToCompleteTypeName () + ModToString (MemberArguments[0]);
				var type2 = MemberArguments[1].FormattedNamespace + MemberArguments[1].ToCompleteTypeName () + ModToString (MemberArguments[1]);
				return type1 + " to " + type2;
			}

			var result = IsEtc && !string.IsNullOrEmpty (EtcFilter) ? EtcFilter : MemberName;

			// Temporary hack for monodoc produced inner type ctor
			//if (DescKind == Kind.Constructor && NestedType != null)
				//result = ToCompleteTypeName ();

			if (GenericMemberArguments != null)
				result += FormatGenericArgs (GenericMemberArguments);

			if (format == Format.WithArgs) {
				result += '(';
				if (MemberArguments != null && MemberArguments.Count > 0) {
					var args = MemberArguments.Select (a => FormatNamespace (a) + a.ToCompleteTypeName ('+') + ModToString (a));
					result += string.Join (",", args);
				}
				result += ')';
			}

			return result;
		}

		public string ToEcmaCref ()
		{
			var sb = new StringBuilder ();
			// Cref type
			sb.Append (DescKind.ToString ()[0]);
			// Create the rest
			ConstructCRef (sb);

			return sb.ToString ();
		}

		void ConstructCRef (StringBuilder sb)
		{
			sb.Append (Namespace);
			if (DescKind == Kind.Namespace)
				return;

			sb.Append ('.');
			sb.Append (TypeName);
			if (GenericTypeArguments != null) {
				sb.Append ('<');
				foreach (var t in GenericTypeArguments)
					t.ConstructCRef (sb);
				sb.Append ('>');
			}
			if (NestedType != null) {
				sb.Append ('+');
				NestedType.ConstructCRef (sb);
			}
			if (ArrayDimensions != null && ArrayDimensions.Count > 0) {
				for (int i = 0; i < ArrayDimensions.Count; i++) {
					sb.Append ('[');
					sb.Append (new string (',', ArrayDimensions[i] - 1));
					sb.Append (']');
				}
			}
			if (DescKind == Kind.Type)
				return;

			if (MemberArguments != null) {
				
			}
		}

		public override string ToString ()
		{
			return string.Format ("({8}) {0}::{1}{2}{3}{7} {4}{5}{6} {9} {10}",
			                      Namespace,
			                      TypeName,
			                      FormatGenericArgsFull (GenericTypeArguments),
			                      NestedType != null ? "+" + NestedType.ToString () : string.Empty,
			                      MemberName ?? string.Empty,
			                      FormatGenericArgsFull (GenericMemberArguments),
			                      MemberArguments != null ? "(" + string.Join (",", MemberArguments.Select (m => m.ToString ())) + ")" : string.Empty,
			                      ArrayDimensions != null && ArrayDimensions.Count > 0 ? ArrayDimensions.Select (dim => "[" + new string (',', dim - 1) + "]").Aggregate (string.Concat) : string.Empty,
			                      DescKind.ToString ()[0],
			                      Etc != 0 ? '(' + Etc.ToString () + ')' : string.Empty,
			                      ExplicitImplMember != null ? "$" + ExplicitImplMember.ToString () : string.Empty);
			                      
		}

		public override bool Equals (object other)
		{
			var otherDesc = other as EcmaDesc;
			return otherDesc != null && Equals (otherDesc);
		}

		public bool Equals (EcmaDesc other)
		{
			if (other == null)
				return false;

			if (NestedType == null ^ other.NestedType == null
			    || ArrayDimensions == null ^ other.ArrayDimensions == null
			    || GenericTypeArguments == null ^ other.GenericTypeArguments == null
			    || GenericMemberArguments == null ^ other.GenericMemberArguments == null
			    || MemberArguments == null ^ other.MemberArguments == null
			    || ExplicitImplMember == null ^ other.ExplicitImplMember == null)
				return false;

			return other != null
				&& DescKind == other.DescKind
				&& TypeName == other.TypeName
				&& Namespace == other.Namespace
				&& MemberName == other.MemberName
				&& (NestedType == null || NestedType.Equals (other.NestedType))
				&& (ArrayDimensions == null || ArrayDimensions.SequenceEqual (other.ArrayDimensions))
				&& (GenericTypeArguments == null || GenericTypeArguments.SequenceEqual (other.GenericTypeArguments))
				&& (GenericMemberArguments == null || GenericMemberArguments.SequenceEqual (other.GenericMemberArguments))
				&& (MemberArguments == null || MemberArguments.SequenceEqual (other.MemberArguments))
				&& Etc == other.Etc
				&& EtcFilter == other.EtcFilter
				&& (ExplicitImplMember == null || ExplicitImplMember.Equals (other.ExplicitImplMember));
		}

		public override int GetHashCode ()
		{
			return DescKind.GetHashCode ()
				^ TypeName.GetHashCode ()
				^ Namespace.GetHashCode ()
				^ MemberName.GetHashCode ();
		}

		bool What (bool input)
		{
			if (!input)
				throw new Exception ("Not equal");
			return input;
		}

		bool WhatT (bool input)
		{
			if (input)
				throw new Exception ("Not equal");
			return input;
		}

		string FormatNamespace (EcmaDesc desc)
		{
			return string.IsNullOrEmpty (desc.Namespace) ? string.Empty : desc.Namespace + ".";
		}

		string FormatGenericArgs (IEnumerable<EcmaDesc> args)
		{
			if (args == null || !args.Any ())
				return string.Empty;
			// If we only have the number of generic arguments, use ` notation
			if (args.First () == null)
				return "`" + args.Count ();

			IEnumerable<string> argsList = args.Select (t => FormatNamespace (t) + t.ToCompleteTypeName ());

			return "<" + string.Join (",", argsList) + ">";
		}

		string FormatGenericArgsFull (IEnumerable<EcmaDesc> genericArgs)
		{
			return genericArgs != null ? "<" + string.Join (",", genericArgs.Select (t => t.ToString ())) + ">" : string.Empty;
		}

		string ModToString (EcmaDesc desc)
		{
			switch (desc.DescModifier) {
			case Mod.Pointer:
				return "*";
			case Mod.Ref:
				return "&";
			case Mod.Out:
				return "@";
			default:
				return string.Empty;
			}
		}

		string FormattedNamespace {
			get {
				return !string.IsNullOrEmpty (Namespace) ? Namespace + "." : string.Empty;
			}
		}
	}
}
