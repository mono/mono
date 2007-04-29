//
// System.Collections.Generic.RBTree
//
// Authors:
//   Raja R Harinath <rharinath@novell.com>
//

//
// Copyright (C) 2007, Novell, Inc.
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

#define ONE_MEMBER_CACHE

#if NET_2_0
using System;
using System.Collections;

namespace System.Collections.Generic
{
	internal class RBTree<T> {
		public class Node {
			public Node left, right;
			public T value;
			uint size_black;

			const uint black_mask = 1;
			const int black_shift = 1;
			public bool IsBlack {
				get { return (size_black & black_mask) == black_mask; }
				set { size_black = value ? (size_black | black_mask) : (size_black & ~black_mask); }
			}

			public uint Size {
				get { return size_black >> black_shift; }
				set { size_black = (value << black_shift) | (size_black & black_mask); }
			}

			public uint FixSize ()
			{
				Size = 1;
				if (left != null)
					Size += left.Size;
				if (right != null)
					Size += right.Size;
				return Size;
			}

			public Node (T v)
			{
				value = v;
				size_black = 2; // Size == 1, IsBlack = false
			}

#if TEST
			public int VerifyInvariants ()
			{
				int black_depth_l = 0;
				int black_depth_r = 0;
				uint size = 1;
				bool child_is_red = false;
				if (left != null) {
					black_depth_l = left.VerifyInvariants ();
					size += left.Size;
					child_is_red |= !left.IsBlack;
				}

				if (right != null) {
					black_depth_r = right.VerifyInvariants ();
					size += right.Size;
					child_is_red |= !right.IsBlack;
				}

				if (black_depth_l != black_depth_r)
					throw new SystemException ("Internal error: black depth mismatch");

				if (!IsBlack && child_is_red)
					throw new SystemException ("Internal error: red-red conflict");
				if (Size != size)
					throw new SystemException ("Internal error: metadata error");

				return black_depth_l + (IsBlack ? 1 : 0);
			}

			public void Dump (string indent)
			{
				Console.WriteLine ("{0}{1} {2}({3})", indent, value, IsBlack ? "*" : "", Size);
				if (left != null)
					left.Dump (indent + "  /");
				if (right != null)
					right.Dump (indent + "  \\");
			}
#endif
		}

		Node root;
		IComparer<T> cmp;
		uint version;

#if ONE_MEMBER_CACHE
		[ThreadStatic]
		static List<Node> cached_path;

		static List<Node> alloc_path ()
		{
			if (cached_path == null)
				return new List<Node> ();

			List<Node> retval = cached_path;
			cached_path = null;
			retval.Clear ();
			return retval;
		}

		static void release_path (List<Node> path)
		{
			cached_path = path;
		}
#else
		static List<Node> alloc_path ()
		{
			return new List<Node> ();
		}

		static void release_path (List<Node> path)
		{
		}
#endif

		public RBTree () : this (Comparer<T>.Default)
		{
		}

		public RBTree (IComparer<T> cmp)
		{
			this.cmp = cmp;
		}

		int Find (T value, List<Node> path)
		{
			if (root == null)
				throw new SystemException ("Internal Error: no tree");

			int c = 0;
			Node sibling = null;
			Node current = root;

			if (path != null)
				path.Add (root);

			while (current != null) {
				c = cmp.Compare (value, current.value);
				if (c == 0)
					return c;

				if (c < 0) {
					sibling = current.right;
					current = current.left;
				} else {
					sibling = current.left;
					current = current.right;
				}

				if (path != null) {
					path.Add (sibling);
					path.Add (current);
				}
			}

			return c;
		}

		public void Insert (T value)
		{
			if (root == null) {
				root = new Node (value);
				root.IsBlack = true;
				++version;
				return;
			}

			List<Node> path = alloc_path ();
			int in_tree_cmp = Find (value, path);
			if (in_tree_cmp == 0)
				throw new ArgumentException ("value already in the tree");

			Node parent = path [path.Count - 3];
			Node current = path [path.Count - 1] = new Node (value);

			if (in_tree_cmp < 0)
				parent.left = current;
			else
				parent.right = current;
			for (int i = 0; i < path.Count - 2; i += 2)
				++ path [i].Size;

			if (!parent.IsBlack)
				rebalance_insert (current, path);

			// no need for a try .. finally, this is only used to mitigate allocations/gc
			release_path (path);

			if (!root.IsBlack)
				throw new SystemException ("Internal error: root is not black");
			++version;
		}

		public Node Remove (T value)
		{
			if (root == null)
				return null;

			List<Node> path = alloc_path ();
			int in_tree_cmp = Find (value, path);
			if (in_tree_cmp != 0) {
				release_path (path);
				return null;
			}

			int curpos = path.Count - 1;
			Node current = path [curpos];
			T old_value = current.value;
			bool changed = false;
			for (; current.Size != 1; current = path [curpos]) {
				Node next = current.left == null
					? left_most (current.right, null, path)
					: right_most (current.left, current.right, path);
				current.value = next.value;
				changed = true;
				curpos = path.Count - 1;
				if (next != path [curpos])
					throw new SystemException ("Internal error: path wrong");

			}
			if (changed)
				current.value = old_value;

			// remove it from our data structures
			path [curpos] = null;
			node_reparent (curpos == 0 ? null : path [curpos-2], current, 0, null);

			for (int i = 0; i < path.Count - 2; i += 2)
				-- path [i].Size;

			if (current.IsBlack)
				rebalance_delete (path);

			// no need for a try .. finally, this is only used to mitigate allocations/gc
			release_path (path);

			if (root != null && !root.IsBlack)
				throw new SystemException ("Internal Error: root is not black");

			++version;
			return current;
		}

		public bool Contains (T value)
		{
			return root != null && Find (value, null) == 0;
		}

		public int Count {
			get { return root == null ? 0 : (int) root.Size; }
		}

		public Node this [int index] {
			get {
				if (index < 0 || index >= Count)
					throw new IndexOutOfRangeException ("index");

				Node current = root;
				while (current.Size > 1) {
					int left_size = current.left == null ? 0 : (int) current.left.Size;
					if (index == left_size)
						return current;
					if (index < left_size) {
						current = current.left;
					} else {
						index -= left_size + 1;
						current = current.right;
					}
				}

				if (index != 0)
					throw new SystemException ("Internal Error: index calculation");
				return current;
			}
		}

		public Enumerator GetEnumerator ()
		{
			return new Enumerator (this);
		}

		public NodeEnumerator GetNodeEnumerator ()
		{
			return new NodeEnumerator (this);
		}

#if TEST
		public void VerifyInvariants ()
		{
			if (root != null) {
				if (!root.IsBlack)
					throw new SystemException ("Internal Error: root is not black");
				root.VerifyInvariants ();
			}
		}

		public void Dump ()
		{
			if (root != null)
				root.Dump ("");
		}
#endif

		void rebalance_insert (Node current, List<Node> path)
		{
			for (int parpos = path.Count - 3; !path [parpos].IsBlack; parpos -= 4) {
				// uncle == parpos - 1, grandpa == parpos - 2, great-grandpa = parpos - 4
				if (path [parpos-1] == null || path [parpos-1].IsBlack) {
					rebalance_insert__rotate_final (current, parpos, path);
					return;
				}

				path [parpos].IsBlack = path [parpos-1].IsBlack = true;

				// move to the grandpa
				current = path [parpos-2];
				if (current == root) // => parpos == 2
					return;

				current.IsBlack = false;
			}

		}

		void rebalance_delete (List<Node> path)
		{
			path.Add (null); path.Add (null); // accomodate a rotation

			for (int curpos = path.Count - 3; curpos > 0; curpos -= 2) {
				Node current = path [curpos];
				if (current != null && !current.IsBlack) {
					current.IsBlack = true;
					return;
				}


				Node sibling = path [curpos-1];

				// current is black => sibling != null
				if (!path [curpos-1].IsBlack) {
					// current is black && sibling is red 
					// => both sibling.left and sibling.right are not null and are black
					curpos = ensure_sibling_black (curpos, path);
					sibling = path [curpos-1];
				}

				if ((sibling.left != null && !sibling.left.IsBlack) ||
				    (sibling.right != null && !sibling.right.IsBlack)) {
					rebalance_delete__rotate_final (curpos, path);
					return;
				}

				sibling.IsBlack = false;
			}
		}

		void rebalance_insert__rotate_final (Node current, int parpos, List<Node> path)
		{
			Node parent = path [parpos];
			Node grandpa = path [parpos-2];

			uint grandpa_size = grandpa.Size;

			Node new_root;

			bool l1 = parent == grandpa.left;
			bool l2 = current == parent.left;
			if (l1 && l2) {
				grandpa.left = parent.right; parent.right = grandpa;
				new_root = parent;
			} else if (l1 && !l2) {
				grandpa.left = current.right; current.right = grandpa;
				parent.right = current.left; current.left = parent;
				new_root = current;
			} else if (!l1 && l2) {
				grandpa.right = current.left; current.left = grandpa;
				parent.left = current.right; current.right = parent;
				new_root = current;
			} else { // (!l1 && !l2)
				grandpa.right = parent.left; parent.left = grandpa;
				new_root = parent;
			}

			grandpa.FixSize (); grandpa.IsBlack = false;
			if (new_root != parent)
				parent.FixSize (); /* parent is red already, so no need to set it */

			new_root.IsBlack = true;
			node_reparent (parpos == 2 ? null : path [parpos-4], grandpa, grandpa_size, new_root);
		}

		// Pre-condition: sibling is black, and one of sibling.left and sibling.right is red
		void rebalance_delete__rotate_final (int curpos, List<Node> path)
		{
			//Node current = path [curpos];
			Node sibling = path [curpos-1];
			Node parent = path [curpos-2];

			uint parent_size = parent.Size;
			bool parent_was_black = parent.IsBlack;

			Node new_root;
			if (parent.right == sibling) {
				// if far nephew is black
				if (sibling.right == null || sibling.right.IsBlack) {
					// => near nephew is red, move it up
					Node nephew = sibling.left;
					parent.right = nephew.left; nephew.left = parent;
					sibling.left = nephew.right; nephew.right = sibling;
					new_root = nephew;
				} else {
					parent.right = sibling.left; sibling.left = parent;
					sibling.right.IsBlack = true;
					new_root = sibling;
				}
			} else {
				// if far nephew is black
				if (sibling.left == null || sibling.left.IsBlack) {
					// => near nephew is red, move it up
					Node nephew = sibling.right;
					parent.left = nephew.right; nephew.right = parent;
					sibling.right = nephew.left; nephew.left = sibling;
					new_root = nephew;
				} else {
					parent.left = sibling.right; sibling.right = parent;
					sibling.left.IsBlack = true;
					new_root = sibling;
				}
			}

			parent.FixSize (); parent.IsBlack = true;
			if (new_root != sibling)
				sibling.FixSize (); /* sibling is already black, so no need to set it */

			new_root.IsBlack = parent_was_black;
			node_reparent (curpos == 2 ? null : path [curpos-4], parent, parent_size, new_root);
		}

		// Pre-condition: sibling is red (=> parent, sibling.left and sibling.right are black)
		int ensure_sibling_black (int curpos, List<Node> path)
		{
			Node current = path [curpos];
			Node sibling = path [curpos-1];
			Node parent = path [curpos-2];

			bool current_on_left;
			uint parent_size = parent.Size;

			if (parent.right == sibling) {
				parent.right = sibling.left; sibling.left = parent;
				current_on_left = true;
			} else {
				parent.left = sibling.right; sibling.right = parent;
				current_on_left = false;
			}

			parent.FixSize (); parent.IsBlack = false;

			sibling.IsBlack = true;
			node_reparent (curpos == 2 ? null : path [curpos-4], parent, parent_size, sibling);

			path [curpos-2] = sibling;
			path [curpos-1] = current_on_left ? sibling.right : sibling.left;
			path [curpos] = parent;
			path [curpos+1] = current_on_left ? parent.right : parent.left;
			path [curpos+2] = current;

			return curpos + 2;
		}

		void node_reparent (Node orig_parent, Node orig, uint orig_size, Node updated)
		{
			if (updated != null && updated.FixSize () != orig_size)
				throw new SystemException ("Internal error: rotation");

			if (orig == root)
				root = updated;
			else if (orig == orig_parent.left)
				orig_parent.left = updated;
			else if (orig == orig_parent.right)
				orig_parent.right = updated;
			else
				throw new SystemException ("Internal error: path error");
		}

		// Pre-condition: current != null
		static Node left_most (Node current, Node sibling, List<Node> path)
		{
			for (;;) {
				path.Add (sibling);
				path.Add (current);
				if (current.left == null)
					return current;
				sibling = current.right;
				current = current.left;
			}
		}

		// Pre-condition: current != null
		static Node right_most (Node current, Node sibling, List<Node> path)
		{
			for (;;) {
				path.Add (sibling);
				path.Add (current);
				if (current.right == null)
					return current;
				sibling = current.left;
				current = current.right;
			}
		}

		public struct Enumerator : IDisposable, IEnumerator, IEnumerator<T> {
			NodeEnumerator host;

			internal Enumerator (RBTree<T> tree)
			{
				host = new NodeEnumerator (tree);
			}

			void IEnumerator.Reset ()
			{
				((IEnumerator) host).Reset ();
			}

			public T Current {
				get { return host.Current.value; }
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public bool MoveNext ()
			{
				return host.MoveNext ();
			}

			public void Dispose ()
			{
				host.Dispose ();
			}
		}

		public struct NodeEnumerator : IEnumerator, IEnumerator<Node> {
			RBTree<T> tree;
			uint version;

			List<Node> path;
			Node current;

			internal NodeEnumerator (RBTree<T> tree)
			{
				this.tree = tree;
				version = tree.version;
				current = null;
				path = null;
			}

			void IEnumerator.Reset ()
			{
				check_version ();
				current = null;
				path = null;
			}

			public Node Current {
				get {
					check_version ();
					if (current == null)
						throw new InvalidOperationException ();
					return current;
				}
			}

			object IEnumerator.Current {
				get { return Current; }
			}

			public bool MoveNext ()
			{
				check_version ();

				if (current == null) {
					if (tree.root == null || path != null)
						return false;

					path = new List<Node> ();
					current = left_most (tree.root, null, path);
					return true;
				}

				if (current.right != null) {
					// since we've already traversed current.left, don't bother saving it in path
					current = left_most (current.right, null, path);
					return true;
				}

				int parpos = path.Count - 3;
				Node parent;
				for (;;) {
					parent = parpos < 0 ? null : path [parpos];
					if (parent == null)
						break;
					if (current == parent.left)
						break;
					current = parent;
					parpos -= 2;
				}

				path.RemoveRange (parpos + 1, path.Count - parpos - 1);
				current = parent;

				return current != null;
			}

			public void Dispose ()
			{
				current = null;
				path = null;
				tree = null;
			}

			void check_version ()
			{
				if (tree == null)
					throw new ObjectDisposedException ("tree");
				if (version != tree.version)
					throw new InvalidOperationException ("tree modified");
			}
		}
	}
}

#if TEST
namespace Mono.ValidationTest {
	using System.Collections.Generic;
	
	class Test {
		static void Main (string [] args)
		{
			Random r = new Random ();
			Dictionary<int, int> d = new Dictionary<int, int> ();
			RBTree<int> t = new RBTree<int> ();
			int iters = args.Length == 0 ? 100000 : Int32.Parse (args [0]);

			for (int i = 0; i < iters; ++i) {
				if ((i % 100) == 0)
					t.VerifyInvariants ();

				int n = r.Next ();
				if (d.ContainsKey (n))
					continue;
				d [n] = n;

				if (t.Contains (n))
					throw new Exception ("tree says it has a number it shouldn't");

				try {
					t.Insert (n);
				} catch {
					Console.Error.WriteLine ("Exception while inserting {0} in iteration {1}", n, i);
					throw;
				}
			}
			t.VerifyInvariants ();
			if (d.Count != t.Count)
				throw new Exception ("tree count is wrong?");

			Console.WriteLine ("Tree has {0} elements", t.Count);

			foreach (int n in d.Keys)
				if (!t.Contains (n))
					throw new Exception ("tree says it doesn't have a number it should");

			foreach (int n in t)
				if (!d.ContainsKey (n))
					throw new Exception ("tree has a number it shouldn't");

			for (int i = 0; i < iters; ++i) {
				int n = r.Next ();
				if (!d.ContainsKey (n)) {
					if (t.Contains (n))
						throw new Exception ("tree says it doesn't have a number it should");
				} else if (!t.Contains (n)) {
					throw new Exception ("tree says it has a number it shouldn't");
				}
			}

			int j = 0;
			foreach (int n in d.Keys) {
				if ((j++ % 100) == 0)
					t.VerifyInvariants ();
				try {
					if (t.Remove (n) == null)
						throw new Exception ("tree says it doesn't have a number it should");

				} catch {
					Console.Error.WriteLine ("While trying to remove {0} from tree of size {1}", n, t.Count);
					t.Dump ();
					t.VerifyInvariants ();
					throw;
				}
				if (t.Contains (n))
					throw new Exception ("tree says it has a number it shouldn't");
			}
			t.VerifyInvariants ();

			if (t.Count != 0)
				throw new Exception ("tree claims to have elements");
		}
	}
}
#endif

#endif
