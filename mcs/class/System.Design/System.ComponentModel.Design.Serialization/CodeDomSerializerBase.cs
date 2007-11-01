//
// System.ComponentModel.Design.Serialization.CodeDomSerializerBase
//
// Authors:	 
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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

#if NET_2_0

using System;
using System.Collections;
using System.Reflection;
using System.ComponentModel;
using System.ComponentModel.Design;

using System.CodeDom;

namespace System.ComponentModel.Design.Serialization
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	public abstract class CodeDomSerializerBase
	{

		private string _deserializationErrorTrace = null;
		private string _deserializationError = null;

		internal CodeDomSerializerBase ()
		{
		}

		private class ExpressionTable : Hashtable // just so that we have a specific type to append to the context stack
		{
		}

		protected CodeExpression SerializeToExpression (IDesignerSerializationManager manager, object instance)
		{
			if (manager == null)
				throw new ArgumentNullException ("manager");

			CodeExpression expression = null;
			if (instance != null)
				expression = this.GetExpression (manager, instance); // 1 - IDesignerSerializationManager.GetExpression
			if (expression == null) {
				CodeDomSerializer serializer = this.GetSerializer (manager, instance); // 2 - manager.GetSerializer().Serialize()
				if (serializer != null) {
					object serialized = serializer.Serialize (manager, instance);
					expression = serialized as CodeExpression; // 3 - CodeStatement or CodeStatementCollection
					if (expression == null) {
						CodeStatement statement = serialized as CodeStatement;
						CodeStatementCollection statements = serialized as CodeStatementCollection;

						if (statement != null || statements != null) {
							CodeStatementCollection contextStatements = null;

							StatementContext context = manager.Context[typeof (StatementContext)] as StatementContext;
							if (context != null && instance != null)
								contextStatements = context.StatementCollection[instance];

							if (contextStatements == null)
								contextStatements = manager.Context[typeof (CodeStatementCollection)] as CodeStatementCollection;

							if (contextStatements != null) {
								if (statements != null)
									contextStatements.AddRange (statements);
								else
									contextStatements.Add (statement);
							}
						}
					}
					if (expression == null && instance != null)
						expression = this.GetExpression (manager, instance); // 4
				}
			}
			return expression;
		}

		protected CodeDomSerializer GetSerializer (IDesignerSerializationManager manager, object instance)
		{
			DesignerSerializerAttribute attrInstance, attrType;
			attrType = attrInstance = null;

			CodeDomSerializer serializer = null;
			if (instance == null)
				serializer = this.GetSerializer (manager, null);
			else {		
				AttributeCollection attributes = TypeDescriptor.GetAttributes (instance);
				foreach (Attribute a in attributes) {
					DesignerSerializerAttribute designerAttr = a as DesignerSerializerAttribute;
					if (designerAttr != null && manager.GetType (designerAttr.SerializerBaseTypeName) == typeof (CodeDomSerializer)) {
						attrInstance = designerAttr;
						break;
					}
				}
	
				attributes = TypeDescriptor.GetAttributes (instance.GetType ());
				foreach (Attribute a in attributes) {
					DesignerSerializerAttribute designerAttr = a as DesignerSerializerAttribute;
					if (designerAttr != null && manager.GetType (designerAttr.SerializerBaseTypeName) == typeof (CodeDomSerializer)) {
						attrType = designerAttr;
						break;
					}
				}
	
				// if there is metadata modification in the instance then create the specified serializer instead of the one
				// in the Type.
				if (attrType != null && attrInstance != null && attrType.SerializerTypeName != attrInstance.SerializerTypeName)
					serializer = Activator.CreateInstance (manager.GetType (attrInstance.SerializerTypeName)) as CodeDomSerializer;
				else
					serializer = this.GetSerializer (manager, instance.GetType ());
			}

			return serializer;
		}

		protected CodeDomSerializer GetSerializer (IDesignerSerializationManager manager, Type instanceType)
		{
			return manager.GetSerializer (instanceType, typeof (CodeDomSerializer)) as CodeDomSerializer;
		}

		protected CodeExpression GetExpression (IDesignerSerializationManager manager, object instance)
		{
			if (manager == null)
				throw new ArgumentNullException ("manager");
			if (instance == null)
				throw new ArgumentNullException ("instance");

			CodeExpression expression = null;

			ExpressionTable expressions = manager.Context[typeof (ExpressionTable)] as ExpressionTable;
			if (expressions != null) // 1st try: ExpressionTable
				expression = expressions [instance] as CodeExpression;

			if (expression == null) { // 2nd try: RootContext
				RootContext context = manager.Context[typeof (RootContext)] as RootContext;
				if (context != null && context.Value == instance)
					expression = context.Expression;
			}

			if (expression == null) { // 3rd try: IReferenceService (instnace.property.property.property
				string name = manager.GetName (instance);
				if (name == null || name.IndexOf (".") == -1) {
					IReferenceService service = manager.GetService (typeof (IReferenceService)) as IReferenceService;
					if (service != null) {
						name = service.GetName (instance);
						if (name != null && name.IndexOf (".") != -1) {
							string[] parts = name.Split (new char[] { ',' });
							instance = manager.GetInstance (parts[0]);
							if (instance != null) {
								expression = SerializeToExpression (manager, instance);
								if (expression != null) {
									for (int i=1; i < parts.Length; i++)
										expression = new CodePropertyReferenceExpression (expression, parts[i]);
								}
							}
						}
					}
				}
			}
			return expression;
		}

		protected void SetExpression (IDesignerSerializationManager manager, object instance, CodeExpression expression)
		{
			SetExpression (manager, instance, expression, false);
		}

		// XXX: isPreset - what does this do when set?
		//
		protected void SetExpression (IDesignerSerializationManager manager, object instance, CodeExpression expression, bool isPreset)
		{
			if (manager == null)
				throw new ArgumentNullException ("manager");
			if (instance == null)
				throw new ArgumentNullException ("instance");
			if (expression == null)
				throw new ArgumentNullException ("expression");

			ExpressionTable expressions = manager.Context[typeof (ExpressionTable)] as ExpressionTable;
			if (expressions == null) {
				expressions = new ExpressionTable ();
				manager.Context.Append (expressions);
			}

			expressions[instance] = expression;
		}

		protected bool IsSerialized (IDesignerSerializationManager manager, object value) 
		{
			return this.IsSerialized (manager, value, false);
		}

		// XXX: What should honorPreset do?
		protected bool IsSerialized (IDesignerSerializationManager manager, object instance, bool honorPreset) 
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			if (this.GetExpression (manager, instance) != null)
				return true;
			else
				return false;
		}

		protected CodeExpression SerializeCreationExpression (IDesignerSerializationManager manager, object value, out bool isComplete) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			CodeExpression expression = null;

			TypeConverter converter = TypeDescriptor.GetConverter (value);
			if (converter != null && converter.CanConvertTo (typeof (InstanceDescriptor))) {
				InstanceDescriptor descriptor = converter.ConvertTo (value, typeof (InstanceDescriptor)) as InstanceDescriptor;
				isComplete = descriptor.IsComplete;
				if (descriptor != null || descriptor.MemberInfo != null)
					expression = this.SerializeInstanceDescriptor (manager, descriptor);
				else
					ReportError (manager, "Unable to serialize to InstanceDescriptor", 
								 "Value Type: " + value.GetType ().Name + System.Environment.NewLine + 
								 "Value (ToString): " + value.ToString ());
			} else {
				expression = new CodeObjectCreateExpression (value.GetType ().FullName, new CodeExpression[0]);
				isComplete = false;
			}
			return expression;
		}

		private CodeExpression SerializeInstanceDescriptor (IDesignerSerializationManager manager, InstanceDescriptor descriptor)
		{
			CodeExpression expression = null;
			MemberInfo member = descriptor.MemberInfo;
			CodeExpression target = new CodeTypeReferenceExpression (member.DeclaringType);

			if (member is PropertyInfo) {
				expression = new CodePropertyReferenceExpression (target, member.Name);
			} else if (member is FieldInfo) {
				expression = new CodeFieldReferenceExpression (target, member.Name);
			} else if (member is MethodInfo) {
				CodeMethodInvokeExpression methodInvoke = new CodeMethodInvokeExpression (target, member.Name);
				manager.Context.Push (new ExpressionContext (methodInvoke, methodInvoke.GetType (), null, null));
				if (descriptor.Arguments != null && descriptor.Arguments.Count > 0)
					methodInvoke.Parameters.AddRange (SerializeParameters (manager, descriptor.Arguments));
				manager.Context.Pop ();
				expression = methodInvoke;
			} else if (member is ConstructorInfo) {
				CodeObjectCreateExpression createExpr = new CodeObjectCreateExpression (member.DeclaringType);
				manager.Context.Push (new ExpressionContext (createExpr, createExpr.GetType (), null, null));
				if (descriptor.Arguments != null && descriptor.Arguments.Count > 0)
					createExpr.Parameters.AddRange (SerializeParameters (manager, descriptor.Arguments));
				manager.Context.Pop ();
				expression = createExpr;
			}

			return expression;
		}

		private CodeExpression[] SerializeParameters (IDesignerSerializationManager manager, ICollection parameters)
		{
			CodeExpression[] expressions = null;

			if (parameters != null && parameters.Count > 0) {
				expressions = new CodeExpression[parameters.Count];
				int i = 0;
				foreach (object parameter in parameters) {
					expressions[i] = this.SerializeToExpression (manager, parameter);
					i++;
				}
			}

			return expressions;
		}

		protected void SerializeEvent (IDesignerSerializationManager manager, CodeStatementCollection statements, 
									   object value, EventDescriptor descriptor) 
		{
			if (descriptor == null)
				throw new ArgumentNullException ("descriptor");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (statements == null)
				throw new ArgumentNullException ("statements");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			MemberCodeDomSerializer serializer = manager.GetSerializer (descriptor.GetType (), typeof (MemberCodeDomSerializer)) as MemberCodeDomSerializer;
			if (serializer != null && serializer.ShouldSerialize (manager, value, descriptor))
				serializer.Serialize (manager, value, descriptor, statements);
		}

		protected void SerializeEvents (IDesignerSerializationManager manager, CodeStatementCollection statements, 
										object value, params Attribute[] filter)
		{
			if (filter == null)
				throw new ArgumentNullException ("filter");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (statements == null)
				throw new ArgumentNullException ("statements");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			EventDescriptorCollection events = TypeDescriptor.GetEvents (value, filter);
			foreach (EventDescriptor e in events) 
				this.SerializeEvent (manager, statements, value, e);
		}

		protected void SerializeProperty (IDesignerSerializationManager manager, CodeStatementCollection statements, object value, PropertyDescriptor propertyToSerialize)
		{
			if (propertyToSerialize == null)
				throw new ArgumentNullException ("propertyToSerialize");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (statements == null)
				throw new ArgumentNullException ("statements");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			MemberCodeDomSerializer serializer = manager.GetSerializer (propertyToSerialize.GetType (), typeof (MemberCodeDomSerializer)) as MemberCodeDomSerializer;
			if (serializer != null && serializer.ShouldSerialize (manager, value, propertyToSerialize))
				serializer.Serialize (manager, value, propertyToSerialize, statements);
		}
		
		protected void SerializeProperties (IDesignerSerializationManager manager, CodeStatementCollection statements, 
											object value, Attribute[] filter) 
		{
			if (filter == null)
				throw new ArgumentNullException ("filter");
			if (value == null)
				throw new ArgumentNullException ("value");
			if (statements == null)
				throw new ArgumentNullException ("statements");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties (value, filter);
			foreach (PropertyDescriptor property in properties)
				this.SerializeProperty (manager, statements, value, property);
		}

		protected virtual object DeserializeInstance (IDesignerSerializationManager manager, Type type, 
													  object[] parameters, string name, bool addToContainer)
		{
			if (type == null)
				throw new ArgumentNullException ("type");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			return manager.CreateInstance (type, parameters, name, addToContainer);
		}

		protected string GetUniqueName (IDesignerSerializationManager manager, object instance)
		{
			if (instance == null)
				throw new ArgumentNullException ("instance");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			string name = manager.GetName (instance);
			if (name == null) {
				INameCreationService service = manager.GetService (typeof (INameCreationService)) as INameCreationService;
				name = service.CreateName (null, instance.GetType ());
				if (name == null)
					name = instance.GetType ().Name.ToLower ();
				manager.SetName (instance, name);
			}
			return name;
		}

		protected object DeserializeExpression (IDesignerSerializationManager manager, string name, CodeExpression expression) 
		{
			if (expression == null)
				throw new ArgumentNullException ("expression");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			object deserialized = null;

			// CodeThisReferenceExpression
			//
			CodeThisReferenceExpression thisExpr = expression as CodeThisReferenceExpression;
			if (thisExpr != null) {
				RootContext context = manager.Context[typeof (RootContext)] as RootContext;
				if (context != null) {
					deserialized = context.Value;
				} else {
					IDesignerHost host = manager.GetService (typeof (IDesignerHost)) as IDesignerHost;
					if (host != null)
						deserialized = host.RootComponent;
				}
			}
			
			// CodeVariableReferenceExpression
			//
			CodeVariableReferenceExpression varRef = expression as CodeVariableReferenceExpression;
			if (deserialized == null && varRef != null) {
				deserialized = manager.GetInstance (varRef.VariableName);
				if (deserialized == null)
					_deserializationError = "Cannot find the value of the variable " + varRef.VariableName + "";
			}

			// CodeFieldReferenceExpression
			//
			CodeFieldReferenceExpression fieldRef = expression as CodeFieldReferenceExpression;
			if (deserialized == null && fieldRef != null) {
				deserialized = manager.GetInstance (fieldRef.FieldName);
				if (deserialized == null)
					_deserializationError = "Cannot find the value of the field " + fieldRef.FieldName + "";
			}
				

			// CodePrimitiveExpression
			//
			CodePrimitiveExpression primitiveExp = expression as CodePrimitiveExpression;
			if (deserialized == null && primitiveExp != null)
				deserialized = primitiveExp.Value;

			// CodePropertyReferenceExpression
			//
			CodePropertyReferenceExpression propRef = expression as CodePropertyReferenceExpression;
			if (deserialized == null && propRef != null) {
				object target = DeserializeExpression (manager, null, propRef.TargetObject);
				if (target != null) {
					bool found = false;
					if (target is Type) {
						PropertyInfo property = ((Type)target).GetProperty (propRef.PropertyName,
																			BindingFlags.GetProperty | 
																			BindingFlags.Public | BindingFlags.Static);
						if (property != null) {
							deserialized = property.GetValue (null, null);
							found = true;
						}

						// NRefactory seems to produce PropertyReferences to reference some fields and enums
						//
						FieldInfo field = ((Type)target).GetField (propRef.PropertyName,
																   BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static);
						if (field != null) {
							deserialized = field.GetValue (null);
							found = true;
						}
					} else {
						PropertyDescriptor property = TypeDescriptor.GetProperties (target)[propRef.PropertyName];
						if (property != null) {
							deserialized = property.GetValue (target);
<<<<<<< .working
						else {
							_deserializationError = "No property [" + propRef.PropertyName + "] found in type " + target.GetType ().Name;
							_deserializationErrorTrace = "Property Name: " + propRef.PropertyName + System.Environment.NewLine +
								"Property Value: " + value.ToString() + System.Environment.NewLine +
								"Property Holder Type: " + target.GetType ().Name + System.Environment.NewLine +
								"Property Holder Expression Type: " + propRef.TargetObject.GetType ().Name + System.Environment.NewLine;
						}
=======
							found = true;
						}

						FieldInfo field = target.GetType().GetField (propRef.PropertyName,
																   BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);
						if (field != null) {
							deserialized = field.GetValue (null);
							found = true;
						}
>>>>>>> .merge-right.r140
					}
					
					if (!found) {
						_deserializationError = "No property " + propRef.PropertyName + " found in type " + 
							(target is Type ? ((Type)target).Name : target.GetType ().Name);
						_deserializationErrorTrace = "Property Name: " + propRef.PropertyName + System.Environment.NewLine +
							"Property is: " + (target is Type ? "static" : "instance") + System.Environment.NewLine +
							"Property Holder Type: " + (target is Type ? ((Type)target).Name : target.GetType ().Name) + System.Environment.NewLine +
							"Property Holder Expression Type: " + propRef.TargetObject.GetType ().Name + System.Environment.NewLine;
					}
				}
			}

			// CodeObjectCreateExpression
			//
			CodeObjectCreateExpression createExpr = expression as CodeObjectCreateExpression;
			if (deserialized == null && createExpr != null) {
				Type type = manager.GetType (createExpr.CreateType.BaseType);
				if (type == null) {
					_deserializationError = "Unable to find the type " + createExpr.CreateType.BaseType;
				} else {
					object[] arguments = new object[createExpr.Parameters.Count];
					for (int i=0; i < createExpr.Parameters.Count; i++) {
						arguments[i] = this.DeserializeExpression (manager, null, createExpr.Parameters[i]);
						if (_deserializationError != null) {
							_deserializationErrorTrace = "Type to create: " + createExpr.CreateType.BaseType + System.Environment.NewLine +
								"Parameter Number: " + i.ToString () + System.Environment.NewLine +
								"Parameter Expression Type: " + createExpr.Parameters[i].GetType ().Name + System.Environment.NewLine +
								"Parameters Count: " + createExpr.Parameters.Count + System.Environment.NewLine + 
								_deserializationErrorTrace;
							break;
						}
					}
					if (_deserializationError == null) {
						bool addToContainer = false;
						if (typeof(IComponent).IsAssignableFrom (type))
							addToContainer = true;
						deserialized = this.DeserializeInstance (manager, type, arguments, name, addToContainer);
						if (deserialized == null) {
							_deserializationError = "Unable to create an instance of type " + createExpr.CreateType.BaseType;
							_deserializationErrorTrace = "Type to create: " + createExpr.CreateType.BaseType + System.Environment.NewLine +
								"Name: " + name + System.Environment.NewLine +
								"addToContainer: " + addToContainer.ToString () + System.Environment.NewLine +
								"Parameters Count: " + createExpr.Parameters.Count + System.Environment.NewLine;
	
							for (int i=0; i < arguments.Length; i++) {
								_deserializationErrorTrace += "Parameter Number: " + i.ToString () + System.Environment.NewLine +
									"Parameter Type: " + (arguments[i] == null ? "null" : arguments[i].GetType ().Name) +
									System.Environment.NewLine +
									"Parameter " + i.ToString () + " Value: " + arguments[i].ToString () + System.Environment.NewLine;
							}
						}
					}
				}
			}

			// CodeArrayCreateExpression
			//
			CodeArrayCreateExpression arrayCreateExpr = expression as CodeArrayCreateExpression;
			if (deserialized == null && arrayCreateExpr != null) {
				Type arrayType = manager.GetType (arrayCreateExpr.CreateType.BaseType);
				if (arrayType == null) {
					_deserializationError = "Unable to find the type " + arrayCreateExpr.CreateType.BaseType;
				} else {
					ArrayList initializers = new ArrayList ();
					Type elementType = arrayType.GetElementType ();
					deserialized = Array.CreateInstance (arrayType, arrayCreateExpr.Initializers.Count);
					for (int i = 0; i < arrayCreateExpr.Initializers.Count; i++) {
						object element = this.DeserializeExpression (manager, null, arrayCreateExpr.Initializers[i]);
						if (_deserializationError != null) {
							_deserializationErrorTrace = "Array Type: " + arrayType.Name + System.Environment.NewLine +
								"Array Element Type: " + elementType + System.Environment.NewLine +
								"Initializer Number: " + i.ToString () + System.Environment.NewLine +
								"Initializer Type: " + (element == null ? "null" : element.GetType ().Name) + System.Environment.NewLine +
								"Initialzer Value" + (element == null ? "null" : element.ToString ()) + System.Environment.NewLine +
								_deserializationErrorTrace;
						} else {
							if (arrayType.IsInstanceOfType (element)) {
								initializers.Add (element);
							} else {
								_deserializationError = "Array initializer of incompatible type detected.";
								_deserializationErrorTrace = "Array Type: " + arrayType.Name + System.Environment.NewLine +
									"Array Element Type: " + elementType + System.Environment.NewLine +
									"Initializer Type: " + (element == null ? "null" : element.GetType ().Name) + System.Environment.NewLine;
							}
						}
					}
					if (_deserializationError == null)
						initializers.CopyTo ((Array)deserialized, 0);
					else
						deserialized = null;
				}
			}

			// CodeMethodInvokeExpression
			//
			CodeMethodInvokeExpression methodExpr = expression as CodeMethodInvokeExpression;
			if (deserialized == null && methodExpr != null) {
				object target = this.DeserializeExpression (manager, null, methodExpr.Method.TargetObject);
				object[] parameters = new object[methodExpr.Parameters.Count];
				for (int i=0; i < methodExpr.Parameters.Count; i++) {
					parameters[i] = this.DeserializeExpression (manager, null, methodExpr.Parameters[i]);
					if (_deserializationError != null) {
						_deserializationErrorTrace =
							"Parameter Number: " + i.ToString () + System.Environment.NewLine +
							"Parameter Expression Type: " + methodExpr.Parameters[i].GetType ().Name + System.Environment.NewLine +
							_deserializationErrorTrace;
						break;
					}
				}

				if (_deserializationError == null) {
					MethodInfo method = null;
					if (target is Type) {
						method = GetExactMethod ((Type)target, methodExpr.Method.MethodName, 
												 BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Static,
												 parameters);
					} else {
						method = GetExactMethod (target.GetType(), methodExpr.Method.MethodName, 
												 BindingFlags.InvokeMethod | BindingFlags.Public | BindingFlags.Instance,
												 parameters);
					}
	
					if (method != null) {
						deserialized = method.Invoke (target, parameters);
					} else {
						_deserializationError = "Unable to find a matching method " + methodExpr.Method.MethodName + " in type " +
							(target is Type ? ((Type)target).Name : target.GetType ().Name);
						for (int i = 0; i < parameters.Length; i++) {
							_deserializationErrorTrace += "Parameter Number: " + i.ToString () + System.Environment.NewLine +
								"Parameter Type: " + (parameters[i] == null ? "null" : parameters[i].GetType ().Name) +
								System.Environment.NewLine +
								"Parameter " + i.ToString () + " Value: " + parameters[i].ToString () + System.Environment.NewLine;
						}
					}
	
					if (_deserializationError != null) {
						_deserializationErrorTrace = 
							"Method Name: " + methodExpr.Method.MethodName + System.Environment.NewLine +
							"Method is: " + (target is Type ? "static" : "instance") + System.Environment.NewLine +
							"Method Holder Type: " + (target is Type ? ((Type)target).Name : target.GetType ().Name) + System.Environment.NewLine +
							"Parameters Count: " + methodExpr.Parameters.Count + System.Environment.NewLine +
							_deserializationErrorTrace;
					}
				}
			}

			// CodeTypeReferenceExpression
			//
			CodeTypeReferenceExpression typeRef = expression as CodeTypeReferenceExpression;
			if (deserialized == null && typeRef != null) {
				deserialized = manager.GetType (typeRef.Type.BaseType);
				if (deserialized == null)
					_deserializationError = "Unable to find the type " + typeRef.Type.BaseType;
			}

			// CodeCastExpression
			// 
			CodeCastExpression castExpr = expression as CodeCastExpression;
			if (deserialized == null && castExpr != null) {
				Type targetType = manager.GetType (castExpr.TargetType.BaseType);
				if (targetType == null)
					_deserializationError = "Unable to find the type " + castExpr.TargetType.BaseType;

				object instance = DeserializeExpression (manager, null, castExpr.Expression);
				if (instance != null && targetType != null) {
					IConvertible convertible = instance as IConvertible;
					if (convertible != null) {
						try {
							instance = convertible.ToType (targetType, null);
						} catch {
							_deserializationError = "Unable to convert type " + instance.GetType ().Name + 
								" to type " + castExpr.TargetType.BaseType;
						}
					} else {
						_deserializationError = "Unable to convert type " + instance.GetType ().Name + 
							" to type " + castExpr.TargetType.BaseType;
					}

					deserialized = instance;
				}
				if (_deserializationError != null) {
					_deserializationErrorTrace = "Target Type: " + castExpr.TargetType.BaseType + System.Environment.NewLine +
						"Instance Type: " + (instance == null ? "null" : instance.GetType ().Name) + System.Environment.NewLine +
						"Instance Value: " + (instance == null ? "null" : instance.ToString()) + System.Environment.NewLine +
						"Instance is IConvertible: " + (instance is IConvertible).ToString() + System.Environment.NewLine +
						_deserializationErrorTrace;
				}
			}


			// CodeBinaryOperatorExpression
			//
			CodeBinaryOperatorExpression binOperator = expression as CodeBinaryOperatorExpression;
			if (deserialized == null && binOperator != null) {
				IConvertible left = null;
				IConvertible right = null;
				switch (binOperator.Operator) {
					case CodeBinaryOperatorType.BitwiseOr:
						left = DeserializeExpression (manager, null, binOperator.Left) as IConvertible;
						right = DeserializeExpression (manager, null, binOperator.Right) as IConvertible;
						if (left is Enum && right is Enum) 
							deserialized = Enum.ToObject (left.GetType (), Convert.ToInt64 (left) | Convert.ToInt64 (right));
						else
							_deserializationError = "CodeBinaryOperatorType.BitwiseOr only supported for Enum types";
						break;
					default:
						_deserializationError = "Unsupported CodeBinaryOperatorType: " + binOperator.Operator.ToString ();
						break;
				}

				if (_deserializationError != null) {
					_deserializationErrorTrace = "BinaryOperator Type: " + binOperator.Operator.ToString() + System.Environment.NewLine +
						"Left Type: " + (left == null ? "null" : left.GetType().Name) + System.Environment.NewLine +
						"Left Value: " + (left == null ? "null" : left.ToString ()) + System.Environment.NewLine +
						"Left Expression Type: " + binOperator.Left.GetType ().Name + System.Environment.NewLine +
						"Right Type: " + (right == null ? "null" : right.GetType().Name) + System.Environment.NewLine +
						"Right Value: " + (right == null ? "null" : right.ToString ()) + System.Environment.NewLine +
						"Right Expression Type: " + binOperator.Right.GetType ().Name + System.Environment.NewLine +
						_deserializationErrorTrace;
				}
			}


			if (_deserializationError != null) {
				_deserializationErrorTrace = "* DeserializeExpression (" + expression.GetType().Name + ")" 
					+ System.Environment.NewLine + _deserializationErrorTrace;
			} else {
				if (deserialized == null && !(expression is CodePrimitiveExpression) && !(expression is CodeMethodInvokeExpression))
					_deserializationError = "Unsupported Expression Type: " + expression.GetType ().Name;
			}

			return deserialized;
		}

		// Searches for a method on type that matches argument types
		//
		private MethodInfo GetExactMethod (Type type, string methodName, BindingFlags flags, ICollection argsCollection)
		{
			object[] arguments = null;
			Type[] types = new Type[0];

			if (argsCollection != null) {
				arguments = new object[argsCollection.Count];
				types = new Type[argsCollection.Count];
				argsCollection.CopyTo (arguments, 0);

				for (int i=0; i < arguments.Length; i++) {
					if (arguments[i] == null)
						types[i] = null;
					else
						types[i] = arguments[i].GetType ();
				}
			}

			return type.GetMethod (methodName, flags, null, types, null);
		}

		protected void DeserializeStatement (IDesignerSerializationManager manager, CodeStatement statement)
		{
			if (statement == null)
				throw new ArgumentNullException ("statement");
			if (manager == null)
				throw new ArgumentNullException ("manager");

			_deserializationErrorTrace = null;
			_deserializationError = null;

			// CodeAssignStatement
			//
			CodeAssignStatement assignment = statement as CodeAssignStatement;
			if (assignment != null)
				DeserializeAssignmentStatement (manager, assignment);

			// CodeExpressionStatement
			//
			CodeExpressionStatement expression = statement as CodeExpressionStatement;
			if (expression != null)
				this.DeserializeExpression (manager, null, expression.Expression);

			// CodeAttachEventStatement
			//
			CodeAttachEventStatement attachStatement = statement as CodeAttachEventStatement;
			if (attachStatement != null) {
				string methodName = null;

				CodeObjectCreateExpression createExpr = attachStatement.Listener as CodeObjectCreateExpression;
				if (createExpr != null && createExpr.Parameters.Count == 1 ) { // += new EventType (method)
					CodeMethodReferenceExpression handlerRef = createExpr.Parameters[0] as CodeMethodReferenceExpression;
					if (handlerRef != null)
						methodName = handlerRef.MethodName;
				}

				CodeDelegateCreateExpression delegateCreateExpr = attachStatement.Listener as CodeDelegateCreateExpression;
				if (delegateCreateExpr != null)// += new EventType (method)
					methodName = delegateCreateExpr.MethodName;

				CodeMethodReferenceExpression methodRef = attachStatement.Listener as CodeMethodReferenceExpression;
				if (methodRef != null) // += method
					methodName = methodRef.MethodName;

				object component = DeserializeExpression (manager, null, attachStatement.Event.TargetObject);
				if (component != null && methodName != null) {
					EventDescriptor eventDescriptor = TypeDescriptor.GetEvents (component)[attachStatement.Event.EventName];
					if (eventDescriptor != null) {
						IEventBindingService service = manager.GetService (typeof (IEventBindingService)) as IEventBindingService;
						if (service != null)
							service.GetEventProperty (eventDescriptor).SetValue (component, methodName);
						else
							_deserializationError = "IEventBindingService missing.";
					} else {
						_deserializationError = "No event " + attachStatement.Event.EventName + 
							" found in type " + component.GetType ().Name;
					}
				}

				if (_deserializationError != null) {
					_deserializationErrorTrace = "Method Name: " + methodName + System.Environment.NewLine +
						"Event Name: " + attachStatement.Event.EventName + System.Environment.NewLine +
						"Listener Expression Type: " + methodRef.GetType ().Name + System.Environment.NewLine +
						"Event Holder Type: " + component.GetType ().Name + System.Environment.NewLine +
						"Event Holder Expression Type: " + attachStatement.Event.TargetObject.GetType ().Name + System.Environment.NewLine;
				}
			}

			if (_deserializationErrorTrace != null) {
				_deserializationErrorTrace = "* DeserializeStatement (" + statement.GetType().Name + ")" 
					+ System.Environment.NewLine + _deserializationErrorTrace;
				ReportError (manager, _deserializationError, _deserializationErrorTrace);
				_deserializationErrorTrace = null;
				_deserializationError = null;
			}
		}

		private void DeserializeAssignmentStatement (IDesignerSerializationManager manager, CodeAssignStatement statement)
		{
			CodeExpression leftExpr = statement.Left;
			
			// Assign to a Property
			//
			CodePropertyReferenceExpression propRef = leftExpr as CodePropertyReferenceExpression;
			if (propRef != null) {
				object propertyHolder = DeserializeExpression (manager, null, propRef.TargetObject);
				object value = null;
				if (_deserializationError == null)
					value = DeserializeExpression (manager, null, statement.Right);

				if (_deserializationError == null && propertyHolder != null) {
					PropertyDescriptor property = TypeDescriptor.GetProperties (propertyHolder)[propRef.PropertyName];
					if (property != null) {
						property.SetValue (target, value);
					} else {
						_deserializationError = "No property " + propRef.PropertyName + " found in type " + propertyHolder.GetType ().Name;
						_deserializationErrorTrace = "Property Name: " + propRef.PropertyName + System.Environment.NewLine +
							"Property Value: " + (value == null ? "null" : value.ToString()) + System.Environment.NewLine +
							"Property Type: " + propertyHolder.GetType ().Name + System.Environment.NewLine +
							"Property Holder Expression Type: " + propRef.TargetObject.GetType ().Name + System.Environment.NewLine;
					}
				}
			}
			
			// Assign to a Field
			// 
			CodeFieldReferenceExpression fieldRef = leftExpr as CodeFieldReferenceExpression;
			if (fieldRef != null && fieldRef.FieldName != null) {
				// Note that if the Right expression is a CodeCreationExpression the component will be created in this call
				//
				object fieldHolder = DeserializeExpression (manager, null, fieldRef.TargetObject);
				object value = null;
				if (_deserializationError == null)
					value = DeserializeExpression (manager, fieldRef.FieldName, statement.Right);
				FieldInfo field = null;

				RootContext context = manager.Context[typeof (RootContext)] as RootContext;
				if (_deserializationError == null && fieldHolder != null) {
					if (fieldRef.TargetObject is CodeThisReferenceExpression && context != null && context.Value == fieldHolder) {
						// Do not deserialize the fields of the root component, because the root component type 
						// is actually an instance of the its parent type, e.g: CustomControl : _UserControl_
						// and thus it doesn't contain the fields. The trick is that once DeserializeExpression 
						// is called on a CodeObjectCreateExpression the component is created and is added to the name-instance
						// table.
						// 
					} else {
						if (fieldHolder is Type) // static field
							field = ((Type)fieldHolder).GetField (fieldRef.FieldName, 
																	BindingFlags.GetField | BindingFlags.Public | BindingFlags.Static);
						else // instance field
							field = fieldHolder.GetType().GetField (fieldRef.FieldName, 
																	BindingFlags.GetField | BindingFlags.Public | BindingFlags.Instance);
						if (field != null)
							field.SetValue (fieldHolder, value);
						else {
							_deserializationError = "No field " + fieldRef.FieldName + " found in type " + fieldHolder.GetType ().Name;
							_deserializationErrorTrace = "Field Name: " + fieldRef.FieldName + System.Environment.NewLine +
								"Field is: " + (fieldHolder is Type ? "static" : "instance") + System.Environment.NewLine +
								"Field Value: " + (value == null ? "null" : value.ToString()) + System.Environment.NewLine +
								"Field Holder Type: " + fieldHolder.GetType ().Name + System.Environment.NewLine +
								"Field Holder Expression Type: " + fieldRef.TargetObject.GetType ().Name + System.Environment.NewLine;
						}
					}
				}
			}

			if (_deserializationError != null) {
				_deserializationErrorTrace = "* DeserializeAssignStatement" + System.Environment.NewLine + 
					"Left Expression Type: " + statement.Left.GetType().Name + System.Environment.NewLine +
					"Right Expression Type: " + statement.Right.GetType().Name + System.Environment.NewLine +
					_deserializationErrorTrace;
			}
		}

		internal void ReportError (IDesignerSerializationManager manager, string message)
		{
			this.ReportError (manager, message, "");
		}

		internal void ReportError (IDesignerSerializationManager manager, string message, string details)
		{
			try {
				throw new Exception (message);
			} catch (Exception e) {
				e.Data["Details"] = message + Environment.NewLine + Environment.NewLine + details;
				manager.ReportError (e);
			}
		}

#region Resource Serialization - TODO
		protected CodeExpression SerializeToResourceExpression (IDesignerSerializationManager manager, object value) 
		{
			throw new NotImplementedException ();
		}
		
		protected CodeExpression SerializeToResourceExpression (IDesignerSerializationManager manager, object value, bool ensureInvariant) 
		{
			throw new NotImplementedException ();
		}
	 	
		protected void SerializePropertiesToResources (IDesignerSerializationManager manager, CodeStatementCollection statements, 
													   object value, Attribute[] filter) 
		{
			throw new NotImplementedException ();
		}

		protected void SerializeResource (IDesignerSerializationManager manager, string resourceName, object value)
		{
			throw new NotImplementedException ();
		}

		protected void SerializeResourceInvariant (IDesignerSerializationManager manager, string resourceName, object value) 
		{
			throw new NotImplementedException ();
		}

		protected void DeserializePropertiesFromResources (IDesignerSerializationManager manager, object value, Attribute[] filter)
		{
			throw new NotImplementedException ();
		}
#endregion
	}
}
#endif
