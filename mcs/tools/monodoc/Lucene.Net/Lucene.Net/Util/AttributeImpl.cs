/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Mono.Lucene.Net.Util
{
	
	/// <summary> Base class for Attributes that can be added to a 
	/// {@link Mono.Lucene.Net.Util.AttributeSource}.
	/// <p/>
	/// Attributes are used to add data in a dynamic, yet type-safe way to a source
	/// of usually streamed objects, e. g. a {@link Mono.Lucene.Net.Analysis.TokenStream}.
	/// </summary>
	[Serializable]
	public abstract class AttributeImpl : System.ICloneable, Attribute
	{
		/// <summary> Clears the values in this AttributeImpl and resets it to its 
		/// default value. If this implementation implements more than one Attribute interface
		/// it clears all.
		/// </summary>
		public abstract void  Clear();
		
		/// <summary> The default implementation of this method accesses all declared
		/// fields of this object and prints the values in the following syntax:
		/// 
		/// <pre>
		/// public String toString() {
		/// return "start=" + startOffset + ",end=" + endOffset;
		/// }
		/// </pre>
		/// 
		/// This method may be overridden by subclasses.
		/// </summary>
		public override System.String ToString()
		{
			System.Text.StringBuilder buffer = new System.Text.StringBuilder();
			System.Type clazz = this.GetType();
			System.Reflection.FieldInfo[] fields = clazz.GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Static);
			try
			{
				for (int i = 0; i < fields.Length; i++)
				{
					System.Reflection.FieldInfo f = fields[i];
					if (f.IsStatic)
						continue;
                    //f.setAccessible(true);   // {{Aroush-2.9}} java.lang.reflect.AccessibleObject.setAccessible
					System.Object value_Renamed = f.GetValue(this);
					if (buffer.Length > 0)
					{
						buffer.Append(',');
					}
					if (value_Renamed == null)
					{
						buffer.Append(f.Name + "=null");
					}
					else
					{
						buffer.Append(f.Name + "=" + value_Renamed);
					}
				}
			}
			catch (System.UnauthorizedAccessException e)
			{
				// this should never happen, because we're just accessing fields
				// from 'this'
				throw new System.SystemException(e.Message, e);
			}
			
			return buffer.ToString();
		}
		
		/// <summary> Subclasses must implement this method and should compute
		/// a hashCode similar to this:
		/// <pre>
		/// public int hashCode() {
		/// int code = startOffset;
		/// code = code * 31 + endOffset;
		/// return code;
		/// }
		/// </pre> 
		/// 
		/// see also {@link #equals(Object)}
		/// </summary>
		abstract public override int GetHashCode();
		
		/// <summary> All values used for computation of {@link #hashCode()} 
		/// should be checked here for equality.
		/// 
		/// see also {@link Object#equals(Object)}
		/// </summary>
		abstract public  override bool Equals(System.Object other);
		
		/// <summary> Copies the values from this Attribute into the passed-in
		/// target attribute. The target implementation must support all the
		/// Attributes this implementation supports.
		/// </summary>
		public abstract void  CopyTo(AttributeImpl target);
		
		/// <summary> Shallow clone. Subclasses must override this if they 
		/// need to clone any members deeply,
		/// </summary>
		public virtual System.Object Clone()
		{
			System.Object clone = null;
			try
			{
				clone = base.MemberwiseClone();
			}
			catch (System.Exception e)
			{
				throw new System.SystemException(e.Message, e); // shouldn't happen
			}
			return clone;
		}
	}
}
