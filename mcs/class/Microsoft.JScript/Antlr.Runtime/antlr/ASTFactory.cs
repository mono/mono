using System;
using Assembly			= System.Reflection.Assembly;
using ArrayList			= System.Collections.ArrayList;
using Debug				= System.Diagnostics.Debug;
using AST				= antlr.collections.AST;
using ASTArray			= antlr.collections.impl.ASTArray;
using ANTLRException	= antlr.ANTLRException;

namespace antlr
{
	/*ANTLR Translator Generator
	* Project led by Terence Parr at http://www.jGuru.com
	* Software rights: http://www.antlr.org/RIGHTS.html
	*
	* $Id: ASTFactory.cs,v 1.1 2003/04/22 04:56:12 cesar Exp $
	*/
	
	//
	// ANTLR C# Code Generator by Micheal Jordan
	//                            Kunle Odutola       : kunle UNDERSCORE odutola AT hotmail DOT com
	//                            Anthony Oguntimehin
	//
	// With many thanks to Eric V. Smith from the ANTLR list.
	//


	// HISTORY:
	//
	// 19-Aug-2002 kunle    Augmented the basic flexibility of the default ASTFactory with a map
	//                      of TokenID-to-NodeTypeName. It's now a proper GoF-style Factory ;-)
	//

	/// <summary>
	/// AST Support code shared by TreeParser and Parser.
	/// </summary>
	/// <remarks>
	/// <para>
	/// We use delegation to share code (and have only one 
	/// bit of code to maintain) rather than subclassing
	/// or superclassing (forces AST support code to be
	/// loaded even when you don't want to do AST stuff).
	/// </para>
	/// <para>
	/// Typically, <see cref="setASTNodeType"/>  is used to specify the
	/// homogeneous type of node to create, but you can override
	/// <see cref="create"/>  to make heterogeneous nodes etc...
	/// </para>
	/// </remarks>
	public class ASTFactory
	{
		//---------------------------------------------------------------------
		// CONSTRUCTORS
		//---------------------------------------------------------------------

		/// <summary>
		/// Constructs an <c>ASTFactory</c> with the default AST node type of
		/// <see cref="antlr.CommonAST"/>.
		/// </summary>
		public ASTFactory() : this("antlr.CommonAST")
		{
		}

		/// <summary>
		/// Constructs an <c>ASTFactory</c> and use the specified AST node type
		/// as the default.
		/// </summary>
		/// <param name="nodeTypeName">
		///		Name of default AST node type for this factory.
		/// </param>
		public ASTFactory(string nodeTypeName)
		{
			nodeTypeObjectList_ = new Type[Token.MIN_USER_TYPE+1];
			defaultASTNodeTypeObject_ = loadNodeTypeObject(nodeTypeName);
		}
		
		//---------------------------------------------------------------------
		// DATA MEMBERS
		//---------------------------------------------------------------------

		/// <summary>
		/// Stores the Type of the default AST node class to be used during tree construction.
		/// </summary>
		protected Type			defaultASTNodeTypeObject_;

		/// <summary>
		/// Stores the mapping between custom AST NodeTypes and their NodeTypeName/NodeTypeClass.
		/// </summary>
		protected Type[]		nodeTypeObjectList_;

		//---------------------------------------------------------------------
		// FUNCTION MEMBERS
		//---------------------------------------------------------------------

		/// <summary>
		/// Specify an "override" for the <see cref="AST"/> type created for
		/// the specified Token type.
		/// </summary>
		/// <remarks>
		/// This method is useful for situations that ANTLR cannot oridinarily deal 
		/// with (i.e., when you  create a token based upon a nonliteral token symbol 
		/// like #[LT(1)].  This is a runtime value and ANTLR cannot determine the token 
		/// type (and hence the AST) statically.
		/// </remarks>
		/// <param name="tokenType">Token type to override.</param>
		/// <param name="NodeTypeName">
		///		Fully qualified AST typename (or null to specify 
		///		the factory's default AST type).
		/// </param>
		public void setTokenTypeASTNodeType(int tokenType, string NodeTypeName)
		{
			// check validity of arguments...
			if( tokenType < Token.MIN_USER_TYPE )
				throw new ANTLRException("Internal parser error: Cannot change AST Node Type for Token ID '" + tokenType + "'");

			// resize up to and including 'type' and initialize any gaps to default
			// factory.
			if (tokenType > (nodeTypeObjectList_.Length+1))
				setMaxNodeType(tokenType);
			// And add new thing..
			nodeTypeObjectList_[tokenType] = loadNodeTypeObject(NodeTypeName);
		}

		/// <summary>
		/// Register an AST Node Type for a given Token type ID.
		/// </summary>
		/// <param name="NodeType">The Token type ID.</param>
		/// <param name="NodeTypeName">The AST Node Type to register.</param>
		[Obsolete("Replaced by setTokenTypeASTNodeType(int, string) since version 2.7.2.6", true)]
		public void registerFactory(int NodeType, string NodeTypeName)
		{
			setTokenTypeASTNodeType(NodeType, NodeTypeName);
		}

		/// <summary>
		/// Pre-expands the internal list of TokenTypeID-to-ASTNodeType mappings
		/// to the specified size.
		/// This is primarily a convenience method that can be used to prevent 
		/// unnecessary and costly re-org of the mappings list.
		/// </summary>
		/// <param name="NodeType">Maximum Token Type ID.</param>
		public void setMaxNodeType( int NodeType )
		{
			//Debug.WriteLine(this, "NodeType = " + NodeType + " and NodeList.Length = " + nodeTypeList_.Length);
			if (nodeTypeObjectList_ == null)
			{
				nodeTypeObjectList_ = new Type[NodeType+1];
			}
			else
			{
				int length = nodeTypeObjectList_.Length;

				if ( NodeType > (length + 1) )
				{
					Type[] newList = new Type[NodeType+1];
					Array.Copy(nodeTypeObjectList_, 0, newList, 0, nodeTypeObjectList_.Length);
					nodeTypeObjectList_ = newList;
				}
				else if ( NodeType < (length + 1) )
				{
					Type[] newList = new Type[NodeType+1];
					Array.Copy(nodeTypeObjectList_, 0, newList, 0, (NodeType+1));
					nodeTypeObjectList_ = newList;
				}
			}
			//Debug.WriteLine(this, "NodeType = " + NodeType + " and NodeList.Length = " + nodeTypeList_.Length);
		}

		/// <summary>
		/// Add a child to the current AST
		/// </summary>
		/// <param name="currentAST">The AST to add a child to</param>
		/// <param name="child">The child AST to be added</param>
		public virtual void  addASTChild(ASTPair currentAST, AST child)
		{
			if (child != null)
			{
				if (currentAST.root == null)
				{
					// Make new child the current root
					currentAST.root = child;
				}
				else
				{
					if (currentAST.child == null)
					{
						// Add new child to current root
						currentAST.root.setFirstChild(child);
					}
					else
					{
						currentAST.child.setNextSibling(child);
					}
				}
				// Make new child the current child
				currentAST.child = child;
				currentAST.advanceChildToEnd();
			}
		}
		
		/// <summary>
		/// Creates a new uninitialized AST node. Since a specific AST Node Type
		/// wasn't indicated, the new AST node is created using the current default
		/// AST Node type - <see cref="defaultASTNodeTypeObject_"/>
		/// </summary>
		/// <returns>An uninitialized AST node object.</returns>
		public virtual AST create()
		{
			AST newNode = createFromNodeTypeObject(defaultASTNodeTypeObject_);
			return newNode;
		}
		
		/// <summary>
		/// Creates and initializes a new AST node using the specified Token Type ID.
		/// The <see cref="System.Type"/> used for creating this new AST node is 
		/// determined by the following:
		/// <list type="bullet">
		///		<item>the current TokenTypeID-to-ASTNodeType mapping (if any) or,</item>
		///		<item>the <see cref="defaultASTNodeTypeObject_"/> otherwise</item>
		/// </list>
		/// </summary>
		/// <param name="type">Token type ID to be used to create new AST Node.</param>
		/// <returns>An initialized AST node object.</returns>
		public virtual AST create(int type)
		{
			AST newNode = createFromNodeType(type);
			newNode.initialize(type, "");
			return newNode;
		}
		
		/// <summary>
		/// Creates and initializes a new AST node using the specified Token Type ID.
		/// The <see cref="System.Type"/> used for creating this new AST node is 
		/// determined by the following:
		/// <list type="bullet">
		///		<item>the current TokenTypeID-to-ASTNodeType mapping (if any) or,</item>
		///		<item>the <see cref="defaultASTNodeTypeObject_"/> otherwise</item>
		/// </list>
		/// </summary>
		/// <param name="type">Token type ID to be used to create new AST Node.</param>
		/// <param name="txt">Text for initializing the new AST Node.</param>
		/// <returns>An initialized AST node object.</returns>
		public virtual AST create(int type, string txt)
		{
			AST newNode = createFromNodeType(type);
			newNode.initialize(type, txt);
			return newNode;
		}
		
		/// <summary>
		/// Creates a new AST node using the specified AST Node Type name. Once created,
		/// the new AST node is initialized with the specified Token type ID and string.
		/// The <see cref="System.Type"/> used for creating this new AST node is 
		/// determined solely by <c>ASTNodeTypeName</c>.
		/// The AST Node type must have a default/parameterless constructor.
		/// </summary>
		/// <param name="type">Token type ID to be used to create new AST Node.</param>
		/// <param name="txt">Text for initializing the new AST Node.</param>
		/// <param name="ASTNodeTypeName">Fully qualified name of the Type to be used for creating the new AST Node.</param>
		/// <returns>An initialized AST node object.</returns>
		public virtual AST create(int type, string txt, string ASTNodeTypeName)
		{
			AST newNode = createFromNodeName(ASTNodeTypeName);
			newNode.initialize(type, txt);
			return newNode;
		}
		
		/// <summary>
		/// Creates a new AST node using the specified AST Node Type name.
		/// </summary>
		/// <param name="Token">Token instance to be used to initialize the new AST Node.</param>
		/// <param name="ASTNodeTypeName">
		///		Fully qualified name of the Type to be used for creating the new AST Node.
		///	</param>
		/// <returns>A newly created and initialized AST node object.</returns>
		/// <remarks>
		/// Once created, the new AST node is initialized with the specified Token 
		/// instance. The <see cref="System.Type"/> used for creating this new AST 
		/// node is  determined solely by <c>ASTNodeTypeName</c>.
		/// <para>The AST Node type must have a default/parameterless constructor.</para>
		/// </remarks>
		public virtual AST create(Token tok, string ASTNodeTypeName)
		{
			AST newNode = createFromNodeName(ASTNodeTypeName);
			newNode.initialize(tok);
			return newNode;
		}
		
		/// <summary>
		/// Creates and initializes a new AST node using the specified AST Node instance.
		/// the new AST node is initialized with the specified Token type ID and string.
		/// The <see cref="System.Type"/> used for creating this new AST node is 
		/// determined solely by <c>aNode</c>.
		/// The AST Node type must have a default/parameterless constructor.
		/// </summary>
		/// <param name="aNode">AST Node instance to be used for creating the new AST Node.</param>
		/// <returns>An initialized AST node object.</returns>
		public virtual AST create(AST aNode)
		{
			AST	newNode;

			if (aNode == null)
				newNode = null;
			else
			{			
				newNode = createFromNodeTypeObject(aNode.GetType());
				newNode.initialize(aNode);
			}
			return newNode;
		}
		
		/// <summary>
		/// Creates and initializes a new AST node using the specified Token instance.
		/// The <see cref="System.Type"/> used for creating this new AST node is 
		/// determined by the following:
		/// <list type="bullet">
		///		<item>the current TokenTypeID-to-ASTNodeType mapping (if any) or,</item>
		///		<item>the <see cref="defaultASTNodeTypeObject_"/> otherwise</item>
		/// </list>
		/// </summary>
		/// <param name="tok">Token instance to be used to create new AST Node.</param>
		/// <returns>An initialized AST node object.</returns>
		public virtual AST create(Token tok)
		{
			AST newNode;

			if (tok == null)
				newNode = null;
			else
			{
				newNode = createFromNodeType(tok.Type);
				newNode.initialize(tok);
			}
			return newNode;
		}
		
		/// <summary>
		/// Returns a copy of the specified AST Node instance. The copy is obtained by
		/// using the <see cref="ICloneable"/> method Clone().
		/// </summary>
		/// <param name="t">AST Node to copy.</param>
		/// <returns>An AST Node (or null if <c>t</c> is null).</returns>
		public virtual AST dup(AST t)
		{
			// The Java version is implemented using code like this:
			if (t == null)
				return null;

			AST dup_edNode = createFromNodeTypeObject(t.GetType());
			dup_edNode.initialize(t);
			return dup_edNode;

			//return (AST)((t == null) ? null : t.Clone());
		}
		
		/// <summary>
		/// Duplicate AST Node tree rooted at specified AST node and all of it's siblings.
		/// </summary>
		/// <param name="t">Root of AST Node tree.</param>
		/// <returns>Root node of new AST Node tree (or null if <c>t</c> is null).</returns>
		public virtual AST dupList(AST t)
		{
			AST result = dupTree(t); // if t == null, then result==null
			AST nt = result;
			while (t != null)
			{
				// for each sibling of the root
				t = t.getNextSibling();
				nt.setNextSibling(dupTree(t)); // dup each subtree, building new tree
				nt = nt.getNextSibling();
			}
			return result;
		}
		
		/// <summary>
		/// Duplicate AST Node tree rooted at specified AST node. Ignore it's siblings.
		/// </summary>
		/// <param name="t">Root of AST Node tree.</param>
		/// <returns>Root node of new AST Node tree (or null if <c>t</c> is null).</returns>
		public virtual AST dupTree(AST t)
		{
			AST result = dup(t); // make copy of root
			// copy all children of root.
			if (t != null)
			{
				result.setFirstChild(dupList(t.getFirstChild()));
			}
			return result;
		}
		
		/// <summary>
		/// Make a tree from a list of nodes.  The first element in the
		/// array is the root.  If the root is null, then the tree is
		/// a simple list not a tree.  Handles null children nodes correctly.
		/// For example, build(a, b, null, c) yields tree (a b c).  build(null,a,b)
		/// yields tree (nil a b).
		/// </summary>
		/// <param name="nodes">List of Nodes.</param>
		/// <returns>AST Node tree.</returns>
		public virtual AST make(AST[] nodes)
		{
			if (nodes == null || nodes.Length == 0)
				return null;
			AST root = nodes[0];
			AST tail = null;
			if (root != null)
			{
				root.setFirstChild(null); // don't leave any old pointers set
			}
			// link in children;
			 for (int i = 1; i < nodes.Length; i++)
			{
				if (nodes[i] == null)
					continue;
				// ignore null nodes
				if (root == null)
				{
					// Set the root and set it up for a flat list
					root = (tail = nodes[i]);
				}
				else if (tail == null)
				{
					root.setFirstChild(nodes[i]);
					tail = root.getFirstChild();
				}
				else
				{
					tail.setNextSibling(nodes[i]);
					tail = tail.getNextSibling();
				}
				// Chase tail to last sibling
				while (tail.getNextSibling() != null)
				{
					tail = tail.getNextSibling();
				}
			}
			return root;
		}
		
		/// <summary>
		/// Make a tree from a list of nodes, where the nodes are contained
		/// in an ASTArray object.
		/// </summary>
		/// <param name="nodes">List of Nodes.</param>
		/// <returns>AST Node tree.</returns>
		public virtual AST make(ASTArray nodes)
		{
			return make(nodes.array);
		}
		
		/// <summary>
		/// Make an AST the root of current AST.
		/// </summary>
		/// <param name="currentAST"></param>
		/// <param name="root"></param>
		public virtual void  makeASTRoot(ASTPair currentAST, AST root)
		{
			if (root != null)
			{
				// Add the current root as a child of new root
				root.addChild(currentAST.root);
				// The new current child is the last sibling of the old root
				currentAST.child = currentAST.root;
				currentAST.advanceChildToEnd();
				// Set the new root
				currentAST.root = root;
			}
		}

		/// <summary>
		/// Sets the global default AST Node Type for this ASTFactory instance.
		/// This method also attempts to load the <see cref="System.Type"/> instance
		/// for the specified typename.
		/// </summary>
		/// <param name="t">Fully qualified AST Node Type name.</param>
		public virtual void  setASTNodeType(string t)
		{
			defaultASTNodeTypeObject_ = loadNodeTypeObject(t);
		}
		
		/// <summary>
		/// To change where error messages go, can subclass/override this method
		/// and then setASTFactory in Parser and TreeParser.  This method removes
		/// a prior dependency on class antlr.Tool.
		/// </summary>
		/// <param name="e"></param>
		public virtual void  error(string e)
		{
			Console.Error.WriteLine(e);
		}

		//---------------------------------------------------------------------
		// PRIVATE FUNCTION MEMBERS
		//---------------------------------------------------------------------

		private Type loadNodeTypeObject(string nodeTypeName)
		{
			Type	nodeTypeObject	= null;
			bool	typeCreated		= false;

			if (nodeTypeName != null)
			{
				foreach (Assembly assem in AppDomain.CurrentDomain.GetAssemblies())
				{
					try
					{
						nodeTypeObject = assem.GetType(nodeTypeName);
						if (nodeTypeObject != null)
						{
							typeCreated = true;
							break;
						}
					}
					catch
					{
						typeCreated = false;
					}
				}
			}
			if (!typeCreated)
			{
				throw new TypeLoadException("Unable to load AST Node Type: '" + nodeTypeName + "'");
			}
			return nodeTypeObject;
		}

		private AST createFromNodeName(string nodeTypeName)
		{
			return createFromNodeTypeObject( loadNodeTypeObject(nodeTypeName) );
		}

		private AST createFromNodeType(int nodeTypeIndex)
		{
			Debug.Assert((nodeTypeIndex >= 0) && (nodeTypeIndex <= nodeTypeObjectList_.Length), "Invalid AST node type!");

			Type nodeTypeObject = nodeTypeObjectList_[nodeTypeIndex];
			if (nodeTypeObject == null)
				nodeTypeObject = defaultASTNodeTypeObject_;

			return createFromNodeTypeObject( nodeTypeObject );
		}

		private AST createFromNodeTypeObject(Type nodeTypeObject)
		{
			AST		newNode			= null;

			try
			{
				newNode = (AST) Activator.CreateInstance(nodeTypeObject);
				if (newNode == null)
				{
					throw new ArgumentException("Unable to create AST Node Type: '" + nodeTypeObject.FullName + "'");
				}
			}
			catch(Exception ex)
			{
				throw new ArgumentException("Unable to create AST Node Type: '" + nodeTypeObject.FullName + "'", ex);
			}
			return newNode;
		}
	}
}