using System;
using System.Linq.Expressions;
using System.Collections.Generic;

public class Node
{
	Node parent;

	public Node ()
	{
		Values = new List<int> ();
	}

	public string Name { get; set; }

	public Node Parent {
		get {
			return parent ?? new Node ();
		}
		set {
			parent = value;
		}
	}

	public List<int> Values { get; set; }

	public static int Main ()
	{
		Expression<Func<Node>> e = () => new Node () { Parent = { Name = "Parent" } };
		var mie = (MemberInitExpression) e.Body;
		if (mie.Bindings[0].BindingType != MemberBindingType.MemberBinding)
			return 1;

		e.Compile () ();

		e = () => new Node () { Values = { 1, 2, 3 } };
		mie = (MemberInitExpression) e.Body;
		if (mie.Bindings[0].BindingType != MemberBindingType.ListBinding)
			return 2;

		e.Compile () ();

		e = () => new Node () { Parent = null };
		mie = (MemberInitExpression) e.Body;
		if (mie.Bindings[0].BindingType != MemberBindingType.Assignment)
			return 3;

		e.Compile () ();

		e = () => new Node () { Values = { } };
		mie = (MemberInitExpression) e.Body;
		if (mie.Bindings[0].BindingType != MemberBindingType.MemberBinding)
			return 4;

		e.Compile () ();

		e = () => new Node() { Parent = { Name = "Parent" }, Values = { 4, 5, 7, 8 } };
		mie = (MemberInitExpression) e.Body;
		if (mie.Bindings[0].BindingType != MemberBindingType.MemberBinding)
			return 5;
		
		if (mie.Bindings[1].BindingType != MemberBindingType.ListBinding)
			return 6;

		e.Compile () ();
		Console.WriteLine ("ok");
		return 0;
	}
}
