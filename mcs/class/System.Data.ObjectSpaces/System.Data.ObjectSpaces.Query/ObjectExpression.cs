//
// System.Data.ObjectSpaces.Query.ObjectExpression
//
//
// Author:
//     Richard Thombs (stony@stony.org)
//     Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Data.ObjectSpaces;

namespace System.Data.ObjectSpaces.Query {
	public class ObjectExpression
	{
		#region Fields

		Expression expression;
		ObjectSchema objectSchema;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public ObjectExpression (Expression expression, ObjectSchema objectSchama)
		{
			this.expression = expression;
			this.objectSchema = objectSchema;
		}

		#endregion // Constructors

		#region Properties

		public Expression Expression {
			get { return expression; }
		}

		public ObjectSchema ObjectSchema {
			get { return objectSchema; }
		}

		[MonoTODO]
		public Type ObjectType {
			get { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public CompiledQuery Compile (MappingSchema map)
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods
	}
}

#endif
