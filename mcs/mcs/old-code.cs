#else

		bla bla bla
		//
		// This code is more conformant to the spec (it follows it step by step),
		// but it has not been tested yet, and there is nothing here that is not
		// caught by the above code.  But it might be a better foundation to improve
		// on in the future
		//
		public ResolveTypeMemberAccess (EmitContext ec, Expression member_lookup,
						Expression left, Location loc)
		{
			if (member_lookup is TypeExpr){
				member_lookup.Resolve (ec);
				return member_lookup;
			}
			
			if (member_lookup is MethodGroupExpr){
				if (!mg.RemoveStaticMethods ()){
					SimpleName.Error120 (loc, mg.Methods [0].Name); 
					return null;
				}
				
				return member_lookup;
			}
			
			if (member_lookup is PropertyExpr){
				PropertyExpr pe = (PropertyExpr) member_lookup;
					
					if (!pe.IsStatic){
						SimpleName.Error120 (loc, pe.PropertyInfo.Name);
						return null;
					}
					return pe;
			}
			
			if (member_lookup is FieldExpr){
				FieldExpr fe = (FieldExpr) member_lookup;
				FieldInfo fi = fe.FieldInfo;
				
				if (fi is FieldBuilder) {
					Const c = TypeManager.LookupConstant ((FieldBuilder) fi);
					
					if (c != null) {
						object o = c.LookupConstantValue (ec);
						return Constantify (o, fi.FieldType);
					}
				}
				
				if (fi.IsLiteral) {
					Type t = fi.FieldType;
					Type decl_type = fi.DeclaringType;
					object o;
					
					if (fi is FieldBuilder)
						o = TypeManager.GetValue ((FieldBuilder) fi);
					else
						o = fi.GetValue (fi);
					
					if (decl_type.IsSubclassOf (TypeManager.enum_type)) {
						Expression enum_member = MemberLookup (
							ec, decl_type, "value__", loc); 
						
						Enum en = TypeManager.LookupEnum (decl_type);
						
						Constant c;
						if (en != null)
							c = Constantify (o, en.UnderlyingType);
						else 
							c = Constantify (o, enum_member.Type);
						
						return new EnumConstant (c, decl_type);
					}
					
					Expression exp = Constantify (o, t);
					
					return exp;
				}

				if (!fe.FieldInfo.IsStatic){
					error176 (loc, fe.FieldInfo.Name);
					return null;
				}
				return member_lookup;
			}

			if (member_lookup is EventExpr){

				EventExpr ee = (EventExpr) member_lookup;
				
				//
				// If the event is local to this class, we transform ourselves into
				// a FieldExpr
				//

				Expression ml = MemberLookup (
					ec, ec.TypeContainer.TypeBuilder, ee.EventInfo.Name,
					MemberTypes.Event, AllBindingFlags, loc);

				if (ml != null) {
					MemberInfo mi = ec.TypeContainer.GetFieldFromEvent ((EventExpr) ml);

					ml = ExprClassFromMemberInfo (ec, mi, loc);
					
					if (ml == null) {
						Report.Error (-200, loc, "Internal error!!");
						return null;
					}

					return ResolveMemberAccess (ec, ml, left, loc);
				}

				if (!ee.IsStatic) {
					SimpleName.Error120 (loc, ee.EventInfo.Name);
					return null;
				}
				
				return ee;
			}

			Console.WriteLine ("Left is: " + left);
			Report.Error (-100, loc, "Support for [" + member_lookup + "] is not present yet");
			Environment.Exit (0);

			return null;
		}
		
		public ResolveInstanceMemberAccess (EmitContext ec, Expression member_lookup,
						    Expression left, Location loc)
		{
			if (member_lookup is MethodGroupExpr){
				//
				// Instance.MethodGroup
				//
				if (!mg.RemoveStaticMethods ()){
					error176 (loc, mg.Methods [0].Name);
					return null;
				}
				
				mg.InstanceExpression = left;
					
				return member_lookup;
			}

			if (member_lookup is PropertyExpr){
				PropertyExpr pe = (PropertyExpr) member_lookup;

				if (pe.IsStatic){
					error176 (loc, pe.PropertyInfo.Name);
					return null;
				}
				Console.WriteLine ("HERE *************");
				pe.InstanceExpression = left;
				
				return pe;
			}

			Type left_type = left.type;

			if (left_type.IsValueType){
			} else {
				
			}
		}
		
		public override Expression DoResolve (EmitContext ec)
		{
			//
			// We are the sole users of ResolveWithSimpleName (ie, the only
			// ones that can cope with it
			//
			expr = expr.ResolveWithSimpleName (ec);

			if (expr == null)
				return null;

			if (expr is SimpleName){
				SimpleName child_expr = (SimpleName) expr;
				
				expr = new SimpleName (child_expr.Name + "." + Identifier, loc);

				return expr.ResolveWithSimpleName (ec);
			}

			//
			// Handle enums here when they are in transit.
			// Note that we cannot afford to hit MemberLookup in this case because
			// it will fail to find any members at all (Why?)
			//

			Type expr_type = expr.Type;
			if (expr_type.IsSubclassOf (TypeManager.enum_type)) {
				
				Enum en = TypeManager.LookupEnum (expr_type);
				
				if (en != null) {
					object value = en.LookupEnumValue (ec, Identifier, loc);

					if (value == null)
						return null;
					
					Constant c = Constantify (value, en.UnderlyingType);
					return new EnumConstant (c, expr_type);
				}
			}

			member_lookup = MemberLookup (ec, expr.Type, Identifier, loc);

			if (member_lookup == null)
				return null;

			if (expr is TypeExpr)
				return ResolveTypeMemberAccess (ec, member_lookup, expr, loc);
			else
				return ResolveInstanceMemberAccess (ec, member_lookup, expr, loc);
		}
#endif
