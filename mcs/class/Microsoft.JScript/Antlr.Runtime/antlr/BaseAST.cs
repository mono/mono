using System;
using StringBuilder		= System.Text.StringBuilder;
using ISerializable		= System.Runtime.Serialization.ISerializable;
using TextWriter		= System.IO.TextWriter;
using ArrayList			= System.Collections.ArrayList;
using IEnumerator		= System.Collections.IEnumerator;

using AST				= antlr.collections.AST;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: BaseAST.cs,v 1.1 2003/04/22 04:56:12 cesar Exp $
	*/
	
	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//

	/*
	* A Child-Sibling Tree.
	*
	* A tree with PLUS at the root and with two children 3 and 4 is
	* structured as:
	*
	*		PLUS
	*		  |
	*		  3 -- 4
	*
	* and can be specified easily in LISP notation as
	*
	* (PLUS 3 4)
	*
	* where every '(' starts a new subtree.
	*
	* These trees are particular useful for translators because of
	* the flexibility of the children lists.  They are also very easy
	* to walk automatically, whereas trees with specific children
	* reference fields can't easily be walked automatically.
	*
	* This class contains the basic support for an AST.
	* Most people will create ASTs that are subclasses of
	* BaseAST or of CommonAST.
	*/
	[Serializable()] 
	public abstract class BaseAST : AST
	{
		protected internal BaseAST down;
		protected internal BaseAST right;
		
		private static bool verboseStringConversion = false;
		private static string[] tokenNames = null;

		/*Add a node to the end of the child list for this node */
		public virtual void  addChild(AST node)
		{
			if (node == null)
				return ;
			BaseAST t = this.down;
			if (t != null)
			{
				while (t.right != null)
				{
					t = t.right;
				}
				t.right = (BaseAST) node;
			}
			else
			{
				this.down = (BaseAST) node;
			}
		}
		
		private void  doWorkForFindAll(ArrayList v, AST target, bool partialMatch)
		{
			AST sibling;
			
			// Start walking sibling lists, looking for matches.
//siblingWalk: 
			 for (sibling = this; sibling != null; sibling = sibling.getNextSibling())
			{
				if ((partialMatch && sibling.EqualsTreePartial(target)) || (!partialMatch && sibling.EqualsTree(target)))
				{
					v.Add(sibling);
				}
				// regardless of match or not, check any children for matches
				if (sibling.getFirstChild() != null)
				{
					((BaseAST) sibling.getFirstChild()).doWorkForFindAll(v, target, partialMatch);
				}
			}
		}
		
		public override bool Equals(Object obj) 
		{      
			if (obj == null) 
				return false;       			
			if (this.GetType() != obj.GetType()) 
				return false;       			
			return Equals((AST)obj);       
		}    
		
		/*Is node t equal to this in terms of token type and text? */
		public virtual bool Equals(AST t)
		{
			if (t == null)
				return false;

			return	(Object.Equals(this.getText(), t.getText())) && 
					(this.Type == t.Type);
		}
		
		/*Is t an exact structural and equals() match of this tree.  The
		*  'this' reference is considered the start of a sibling list.
		*/
		public virtual bool EqualsList(AST t)
		{
			AST sibling;
			
			// the empty tree is not a match of any non-null tree.
			if (t == null)
			{
				return false;
			}
			
			// Otherwise, start walking sibling lists.  First mismatch, return false.
			 for (sibling = this; sibling != null && t != null; sibling = sibling.getNextSibling(), t = t.getNextSibling())
			{
				// as a quick optimization, check roots first.
				if (!sibling.Equals(t))
				{
					return false;
				}
				// if roots match, do full list match test on children.
				if (sibling.getFirstChild() != null)
				{
					if (!sibling.getFirstChild().EqualsList(t.getFirstChild()))
					{
						return false;
					}
				}
				else if (t.getFirstChild() != null)
				{
					return false;
				}
			}
			if (sibling == null && t == null)
			{
				return true;
			}
			// one sibling list has more than the other
			return false;
		}
		
		/*Is 'sub' a subtree of this list?
		*  The siblings of the root are NOT ignored.
		*/
		public virtual bool EqualsListPartial(AST sub)
		{
			AST sibling;
			
			// the empty tree is always a subset of any tree.
			if (sub == null)
			{
				return true;
			}
			
			// Otherwise, start walking sibling lists.  First mismatch, return false.
			 for (sibling = this; sibling != null && sub != null; sibling = sibling.getNextSibling(), sub = sub.getNextSibling())
			{
				// as a quick optimization, check roots first.
				if (!sibling.Equals(sub))
					return false;
				// if roots match, do partial list match test on children.
				if (sibling.getFirstChild() != null)
				{
					if (!sibling.getFirstChild().EqualsListPartial(sub.getFirstChild()))
						return false;
				}
			}
			if (sibling == null && sub != null)
			{
				// nothing left to match in this tree, but subtree has more
				return false;
			}
			// either both are null or sibling has more, but subtree doesn't
			return true;
		}
		
		/*Is tree rooted at 'this' equal to 't'?  The siblings
		*  of 'this' are ignored.
		*/
		public virtual bool EqualsTree(AST t)
		{
			// check roots first.
			if (!this.Equals(t))
				return false;
			// if roots match, do full list match test on children.
			if (this.getFirstChild() != null)
			{
				if (!this.getFirstChild().EqualsList(t.getFirstChild()))
					return false;
			}
			else if (t.getFirstChild() != null)
			{
				return false;
			}
			return true;
		}
		
		/*Is 't' a subtree of the tree rooted at 'this'?  The siblings
		*  of 'this' are ignored.
		*/
		public virtual bool EqualsTreePartial(AST sub)
		{
			// the empty tree is always a subset of any tree.
			if (sub == null)
			{
				return true;
			}
			
			// check roots first.
			if (!this.Equals(sub))
				return false;
			// if roots match, do full list partial match test on children.
			if (this.getFirstChild() != null)
			{
				if (!this.getFirstChild().EqualsListPartial(sub.getFirstChild()))
					return false;
			}
			return true;
		}
		
		/*Walk the tree looking for all exact subtree matches.  Return
		*  an IEnumerator that lets the caller walk the list
		*  of subtree roots found herein.
		*/
		public virtual IEnumerator findAll(AST target)
		{
			ArrayList roots = new ArrayList(10);
			//AST sibling;
			
			// the empty tree cannot result in an enumeration
			if (target == null)
			{
				return null;
			}
			
			doWorkForFindAll(roots, target, false); // find all matches recursively
			
			return roots.GetEnumerator();
		}
		
		/*Walk the tree looking for all subtrees.  Return
		*  an IEnumerator that lets the caller walk the list
		*  of subtree roots found herein.
		*/
		public virtual IEnumerator findAllPartial(AST sub)
		{
			ArrayList roots = new ArrayList(10);
			//AST sibling;
			
			// the empty tree cannot result in an enumeration
			if (sub == null)
			{
				return null;
			}
			
			doWorkForFindAll(roots, sub, true); // find all matches recursively
			
			return roots.GetEnumerator();
		}
		
		/*Get the first child of this node; null if not children */
		public virtual AST getFirstChild()
		{
			return down;
		}
		
		/*Get the next sibling in line after this one */
		public virtual AST getNextSibling()
		{
			return right;
		}
		
		/*Get the token text for this node */
		public virtual string getText()
		{
			return "";
		}
		
		/*Get the token type for this node */
		public virtual int Type
		{
			get { return 0; }
			set { ; }
		}
		
		/// <summary>
		/// Get number of children of this node; if leaf, returns 0
		/// </summary>
		/// <returns>Number of children</returns>
		public int getNumberOfChildren() 
		{
			BaseAST t = this.down;
			int n = 0;
			if (t != null) 
			{
				n = 1;
				while (t.right != null) 
				{
					t = t.right;
					n++;
				}
			}
			return n;
		}

		public abstract void  initialize(int t, string txt);
		
		public abstract void  initialize(AST t);
		
		public abstract void  initialize(Token t);
		
		/*Remove all children */
		public virtual void  removeChildren()
		{
			down = null;
		}
		
		public virtual void  setFirstChild(AST c)
		{
			down = (BaseAST) c;
		}
		
		public virtual void  setNextSibling(AST n)
		{
			right = (BaseAST) n;
		}
		
		/*Set the token text for this node */
		public virtual void  setText(string text)
		{
			;
		}
		
		/*Set the token type for this node */
		public virtual void  setType(int ttype)
		{
			this.Type = ttype;
		}
		
		public static void  setVerboseStringConversion(bool verbose, string[] names)
		{
			verboseStringConversion = verbose;
			tokenNames = names;
		}
		
		override public string ToString()
		{
			StringBuilder b = new StringBuilder();
			// if verbose and type name not same as text (keyword probably)
			if (verboseStringConversion && 
					(0 != String.Compare(getText(), (tokenNames[Type]), true)) &&
					(0 != String.Compare(getText(), StringUtils.stripFrontBack(tokenNames[Type], @"""", @""""), true)))
			{
				b.Append('[');
				b.Append(getText());
				b.Append(",<");
				b.Append(tokenNames[Type]);
				b.Append(">]");
				return b.ToString();
			}
			return getText();
		}
		
		/*Print out a child-sibling tree in LISP notation */
		public virtual string ToStringList()
		{
			AST t = this;
			string ts = "";
			if (t.getFirstChild() != null)
				ts += " (";
			ts += " " + this.ToString();
			if (t.getFirstChild() != null)
			{
				ts += ((BaseAST) t.getFirstChild()).ToStringList();
			}
			if (t.getFirstChild() != null)
				ts += " )";
			if (t.getNextSibling() != null)
			{
				ts += ((BaseAST) t.getNextSibling()).ToStringList();
			}
			return ts;
		}
		
		public virtual string ToStringTree()
		{
			return ToStringTree(string.Empty);
		}
		
		public string ToStringTree(string prefix) 
		{
			StringBuilder sb = new StringBuilder(prefix);
		
			// Replace vertical bar if there is no next sibling.
			if ( (getNextSibling() == null) )
				sb.Append("+--");
			else
				sb.Append("|--");
		
			sb.Append( ToString() );
			sb.Append( Environment.NewLine );

			if ( getFirstChild() != null ) 
			{
				// Replace vertical bar if there is no next sibling.
				if ( getNextSibling() == null )
					sb.Append( ((BaseAST) getFirstChild()).ToStringTree(prefix + "   ") );
				else
					sb.Append( ((BaseAST) getFirstChild()).ToStringTree(prefix + "|  ") );
			}

			if ( getNextSibling() != null )
				sb.Append( ((BaseAST) getNextSibling()).ToStringTree(prefix) );

			return sb.ToString();
		}

		public static string decode(string text)
		{
			char c, c1, c2, c3, c4, c5;
			StringBuilder n = new StringBuilder();
			 for (int i = 0; i < text.Length; i++)
			{
				c = text[i];
				if (c == '&')
				{
					c1 = text[i + 1];
					c2 = text[i + 2];
					c3 = text[i + 3];
					c4 = text[i + 4];
					c5 = text[i + 5];
					
					if (c1 == 'a' && c2 == 'm' && c3 == 'p' && c4 == ';')
					{
						n.Append("&");
						i += 5;
					}
					else if (c1 == 'l' && c2 == 't' && c3 == ';')
					{
						n.Append("<");
						i += 4;
					}
					else if (c1 == 'g' && c2 == 't' && c3 == ';')
					{
						n.Append(">");
						i += 4;
					}
					else if (c1 == 'q' && c2 == 'u' && c3 == 'o' && c4 == 't' && c5 == ';')
					{
						n.Append("\"");
						i += 6;
					}
					else if (c1 == 'a' && c2 == 'p' && c3 == 'o' && c4 == 's' && c5 == ';')
					{
						n.Append("'");
						i += 6;
					}
					else
						n.Append("&");
				}
				else
					n.Append(c);
			}
			return n.ToString();
		}
		
		public static string encode(string text)
		{
			char c;
			StringBuilder n = new StringBuilder();
			 for (int i = 0; i < text.Length; i++)
			{
				c = text[i];
				switch (c)
				{
					case '&': 
					{
						n.Append("&amp;");
						break;
					}
					
					case '<': 
					{
						n.Append("&lt;");
						break;
					}
					
					case '>': 
					{
						n.Append("&gt;");
						break;
					}
					
					case '"': 
					{
						n.Append("&quot;");
						break;
					}
					
					case '\'': 
					{
						n.Append("&apos;");
						break;
					}
					
					default: 
					{
						n.Append(c);
						break;
					}
					
				}
			}
			return n.ToString();
		}
		
		public virtual void  xmlSerializeNode(TextWriter outWriter)
		{
			StringBuilder buf = new StringBuilder(100);
			buf.Append("<");
			buf.Append(GetType().FullName + " ");
			buf.Append("text=\"" + encode(getText()) + "\" type=\"" + Type + "\"/>");
			outWriter.Write(buf.ToString());
		}
		
		public virtual void  xmlSerializeRootOpen(TextWriter outWriter)
		{
			StringBuilder buf = new StringBuilder(100);
			buf.Append("<");
			buf.Append(GetType().FullName + " ");
			buf.Append("text=\"" + encode(getText()) + "\" type=\"" + Type + "\">\n");
			outWriter.Write(buf.ToString());
		}
		
		public virtual void  xmlSerializeRootClose(TextWriter outWriter)
		{
			outWriter.Write("</" + GetType().FullName + ">\n");
		}
		
		public virtual void  xmlSerialize(TextWriter outWriter)
		{
			// print out this node and all siblings
			 for (AST node = this; node != null; node = node.getNextSibling())
			{
				if (node.getFirstChild() == null)
				{
					// print guts (class name, attributes)
					((BaseAST) node).xmlSerializeNode(outWriter);
				}
				else
				{
					((BaseAST) node).xmlSerializeRootOpen(outWriter);
					
					// print children
					((BaseAST) node.getFirstChild()).xmlSerialize(outWriter);
					
					// print end tag
					((BaseAST) node).xmlSerializeRootClose(outWriter);
				}
			}
		}

		#region Implementation of ICloneable
		public virtual object Clone()
		{
			return MemberwiseClone();
		}
		#endregion

		public override Int32 GetHashCode() 
		{      
			return down.GetHashCode() ^ right.GetHashCode();
		}	
	}
}