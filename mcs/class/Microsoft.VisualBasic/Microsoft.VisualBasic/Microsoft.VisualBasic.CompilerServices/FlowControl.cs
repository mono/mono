 //
// FlowControl.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Chris J Breisch
//
/*
  * Copyright (c) 2002-2003 Mainsoft Corporation.
  * Copyright (C) 2004 Novell, Inc (http://www.novell.com)
  *
  * Permission is hereby granted, free of charge, to any person obtaining a
  * copy of this software and associated documentation files (the "Software"),
  * to deal in the Software without restriction, including without limitation
  * the rights to use, copy, modify, merge, publish, distribute, sublicense,
  * and/or sell copies of the Software, and to permit persons to whom the
  * Software is furnished to do so, subject to the following conditions:
  * 
  * The above copyright notice and this permission notice shall be included in
  * all copies or substantial portions of the Software.
  * 
  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  * DEALINGS IN THE SOFTWARE.
  */
/**
 * This class allows to execute loop statement of VisualBasic .NET
 */

using System;
using System.Collections;
using System.ComponentModel;

namespace Microsoft.VisualBasic.CompilerServices 
{
	[StandardModule, EditorBrowsable(EditorBrowsableState.Never)] 
	sealed public class FlowControl {
		private FlowControl () {}

		private sealed /*static (final)in mainsoft java code*/ class ObjectFor {
			public object Counter;

			public object Limit;

			public object StepValue;

			public bool PositiveStep;

			public Type EnumType;
		}
    
		/**
		 * This method check if the loop can continued.
		 * if the StepValue is positive it check that count is smaller than the limit.
		 * if the StepValue is negative it check that count is bigger than the limit. 
		 * @param count
		 * @param limit
		 * @param StepValue
		 * @return boolean True of the for next loop can continue and false otherwise.
		 */
		public static bool ForNextCheckR4(float count, float limit, float StepValue) {
			bool positiveStep = StepValue > 0.0F;
			bool isCountSmallThenLimit = count <= limit;
			return positiveStep? isCountSmallThenLimit : !isCountSmallThenLimit;
		}
    
		/**
		 * This method check if the loop can continued.
		 * if the StepValue is positive it check that count is smaller than the limit.
		 * if the StepValue is negative it check that count is bigger than the limit. 
		 * @param count
		 * @param limit
		 * @param StepValue
		 * @return boolean True of the for next loop can continue and false otherwise.
		 */
		public static bool ForNextCheckR8(double count, double limit, double StepValue) {
			bool positiveStep = StepValue > 0.0;
			bool isCountSmallThenLimit = count <= limit;
			return positiveStep? isCountSmallThenLimit : !isCountSmallThenLimit;
		}

		/**
		 * This method check if the loop can continued.
		 * if the StepValue is positive it check that count is smaller than the limit.
		 * if the StepValue is negative it check that count is bigger than the limit. 
		 * @param count
		 * @param limit
		 * @param StepValue
		 * @return boolean True of the for next loop can continue and false otherwise.
		 */
		public static bool ForNextCheckDec(Decimal count, Decimal limit, Decimal StepValue) {
			bool positiveStep = StepValue.CompareTo(Decimal.Zero) < 0;
			bool isCountSmallThenLimit = count.CompareTo(limit) >= 0;
			return positiveStep? isCountSmallThenLimit : !isCountSmallThenLimit; 
		}
    
		/**
		 * This method method updates the LoopFor reference and the Counter reference
		 * object according to the given params and returns if this loop can continue. 
		 * @param Counter this loop counter value
		 * @param Start this loop start value 
		 * @param Limit this loop limitation value
		 * @param StepValue this loop step value
		 * @param lfr the LoopFor reference object
		 * @param cr the Counter object reference 
		 * @return boolean is the returned LoopFor object can continue.
		 */
		public static bool ForLoopInitObj(
			object Counter,
			object Start,
			object Limit,
			object StepValue,
			ref System.Object lfr,
			ref System.Object cr) {

			object CounterResult = cr;

			if (Start == null) {
				throw new ArgumentException("Argument_InvalidNullValue1 " + " Start");
			}
			if (Limit == null) {
				throw new ArgumentException("Argument_InvalidNullValue1 " + " Limit");
			}
			if (StepValue == null) {
				throw new ArgumentException("Argument_InvalidNullValue1 " + " Step");
			}
			//gets the type of all the given parameters
			Type startType = Start.GetType();
			Type limitType = Limit.GetType();
			Type stepType = StepValue.GetType();

			//gets the widest common type code
		
			TypeCode commonTypeCode = ObjectType.GetWidestType(Start, Limit, false);
			commonTypeCode = ObjectType.GetWidestType(StepValue, commonTypeCode);
			if (commonTypeCode == TypeCode.String) {
				commonTypeCode = TypeCode.Double;
			}
			if (commonTypeCode == TypeCode.Object) {
				//TODO:
				//throw new ArgumentException(
				//	Utils.GetResourceString(
				//	"ForLoop_CommonType3",
				//	Utils.VBFriendlyName(startType),
				//	Utils.VBFriendlyName(limitType),
				//	Utils.VBFriendlyName(StepValue)));
				throw new ArgumentException("ForLoop_CommonType3 startType limitType StepValue");
			}

			ObjectFor objectFor = new ObjectFor();
			TypeCode startTypeCode = Type.GetTypeCode(startType);
			TypeCode limitTypeCode = Type.GetTypeCode(limitType);
			TypeCode stepTypeCode = Type.GetTypeCode(stepType);
			Type enumType = null;

			bool isStartTypeValidEnum =  (startTypeCode == commonTypeCode) && (startType.IsEnum);
			bool isLimitTypeValidEnum =  (limitTypeCode == commonTypeCode) && (limitType.IsEnum);
			bool isStepTypeValidEnum =  (stepTypeCode == commonTypeCode) && (stepType.IsEnum);
        
			bool isStartAndStepTypeEqual = (startType == stepType);
			bool isStartAndLimitTypeEqual = (startType == limitType);
			bool isStepAndLimitTypeEqual = (stepType == limitType);

			//the For loop has enum type in the following case
			//1. step is enum and it's type code equal to commonTypeCode and start and 
			//  limit don't meet this condition.
			//2. step and start are enum and their type code equal to commonTypeCode and
			//  their types are equal. limit doesn't meet this condition about been enum
			//  or about been equal to commonTypeCode.
			//3. step and limit are enum and their type code equal to commonTypeCode and
			//  their types are equal. start doesn't meet this condition about been enum
			//  or about been equal to commonTypeCode.
			//4. step and limit and start are enum and their type code equal to commonTypeCode and
			//  their types are equal.
			//5. start is enum and it's type code equal to commonTypeCode .step and 
			//  limit don't meet this condition.
			//6. limit is enum and it's type code equal to commonTypeCode .step and 
			//  start don't meet this condition.
			//7.start and limit are enum and their type code equal to commonTypeCode and
			//  their types are equal. step doesn't meet this condition about been enum
			//  or about been equal to commonTypeCode.
			//
        
			if (isStartTypeValidEnum && isLimitTypeValidEnum && isStepTypeValidEnum
				&&  isStartAndStepTypeEqual && isStartAndLimitTypeEqual)
				enumType = startType;
			else if (isStartTypeValidEnum && isStepTypeValidEnum && isStartAndStepTypeEqual)
				enumType = startType;
			else if (isStartTypeValidEnum && isStepTypeValidEnum && isStartAndStepTypeEqual)
				enumType = startType;
			else if (isStartTypeValidEnum && isLimitTypeValidEnum && isStartAndLimitTypeEqual)
				enumType = startType;
			else if (isStartTypeValidEnum && !isLimitTypeValidEnum && !isStepTypeValidEnum)
				enumType = startType;
			else if (!isStartTypeValidEnum && isLimitTypeValidEnum && !isStepTypeValidEnum)
				enumType = limitType; 
			else if (!isStartTypeValidEnum && !isLimitTypeValidEnum && isStepTypeValidEnum)
				enumType = stepType;            
        
			objectFor.EnumType = enumType;
        
			//set the counter field of objectFor with Start value transleted to 
			// the widest common type code
			objectFor.Counter = convertType(Start, commonTypeCode,"Start");
			//set the Limit field of objectFor with Limit value transleted to 
			// the widest common type code
			objectFor.Limit = convertType(Limit, commonTypeCode,"Limit");
			//set the StepValue field of objectFor with StepValue value transleted to 
			// the widest common type code
			objectFor.StepValue = convertType(StepValue, commonTypeCode,"Step");
			//local is the value of zero in the widest common type code

			object local = ObjectType.CTypeHelper(0, commonTypeCode);
        
			IComparable iComparable = (IComparable)objectFor.StepValue;
			objectFor.PositiveStep = iComparable.CompareTo(local) >= 0;

			// sets the loop for reference 
			lfr = objectFor;
        
			//sets the counter reference
			if (objectFor.EnumType != null) {
				cr = Enum.ToObject(objectFor.EnumType, objectFor.Counter);
			}
			else {
				cr = objectFor.Counter;
			}        
			return CheckContinueLoop(objectFor);
		}
    
		private static object convertType(object original, TypeCode typeCode, string fieldName) {
			try {
				return ObjectType.CTypeHelper(original, typeCode);
			}
			catch /*(Exception e)*/ {
				throw new ArgumentException("ForLoop_ConvertToType3 " + fieldName);
			}
		}

		public static bool ForNextCheckObj(object Counter, object LoopObj,
			ref System.Object CounterResult) {// throws java.lang.Exception
			TypeCode generalTypeCode = 0;

			if (LoopObj == null) {
				//TODO: use resource for the correct execption.
				throw new Exception("VB error message #92 ForNextCheckObj LoopObj cannot be null");
				//throw ExceptionUtils.VbMakeException(92);//correct java version
			}
			if (Counter == null) {
				throw new NullReferenceException("Argument_InvalidNullValue1 " + " Counter");
				//TODO:
				//throw new NullReferenceException(
				//    Utils.GetResourceString(
				//        "Argument_InvalidNullValue1",
				//        "Counter"));
			}
			 ObjectFor objectFor = (ObjectFor) LoopObj;

			IConvertible iConvertible_counter = (IConvertible)Counter;
			IConvertible iConvertible_step = (IConvertible) objectFor.StepValue;

			TypeCode counterTypeCode = iConvertible_counter.GetTypeCode();
			TypeCode stepTypeCode = iConvertible_step.GetTypeCode();
			
			if (counterTypeCode == stepTypeCode && counterTypeCode != TypeCode.String) {
				generalTypeCode = counterTypeCode;
			}
			else {
				generalTypeCode = ObjectType.GetWidestType(counterTypeCode, stepTypeCode);
				if (generalTypeCode == TypeCode.String) {
					generalTypeCode = TypeCode.Double;
				}
				Counter = convertType(Counter, generalTypeCode,"Start");
				objectFor.Limit = convertType(objectFor.Limit, generalTypeCode,"Limit");
				objectFor.StepValue = convertType(objectFor.StepValue, generalTypeCode,"Step");
			}
			//changes the counter field to be the sum of step and counter 
			objectFor.Counter = ObjectType.AddObj(Counter, objectFor.StepValue);
			IConvertible iConvertible_objectCounter = (IConvertible)objectFor.Counter;
			TypeCode objectCounterTypeCode = iConvertible_objectCounter.GetTypeCode();

			//setting the counter in counter reference.
			//if the for is enum type change counter to enum. 
			if (objectFor.EnumType != null) {
				CounterResult = Enum.ToObject(objectFor.EnumType, objectFor.Counter);
			}
			else {
				CounterResult = objectFor.Counter;
			}
        
			//if the counter after the change didn't change it's type return true if 
			// the for  object can continue loop and false otherwise.
			//if the counter changed it's type change all for object fields to counter
			//current type and return false. 
			if (objectCounterTypeCode == generalTypeCode) {
				return CheckContinueLoop(objectFor);
			}
			else {
				objectFor.Limit = ObjectType.CTypeHelper(objectFor.Limit, objectCounterTypeCode);
        
				objectFor.StepValue =
					ObjectType.CTypeHelper(objectFor.StepValue, objectCounterTypeCode);
				return false;
			}
		}
    
		/**
		 * This method returns IEnumertator for a given array
		 * @param ary the given array
		 * @return IEnumerator the array's Enumerator
		 */
		public static IEnumerator ForEachInArr(Array ary) {// throws java.lang.Exception
			IEnumerator iEnumerator = (IEnumerator)ary;//is ArrayStaticWrapper.GetEnumerator(ary); in java code.
			if (iEnumerator != null)
				return iEnumerator;
			throw ExceptionUtils.VbMakeException(92);
		}
    
		/**
		 * This method gets IEnumerator for a given object that implements IEnumerable
		 * @param obj the object that implements IEnumerable
		 * @return IEnumerator the object's IEnumerator.
		 */
		public static IEnumerator ForEachInObj(object obj) {// throws java.lang.Exception
			if (obj == null)
				throw ExceptionUtils.VbMakeException(91);

			IEnumerable iEnumerable = (IEnumerable)obj;
			if (iEnumerable != null) {
				IEnumerator iEnumerator = iEnumerable.GetEnumerator();
				if (iEnumerator != null)
					return iEnumerator;
			}
			string s = obj.GetType().ToString();
			ExceptionUtils.ThrowException1(100, s);
			return null;
		}
    
		/**
		 * This method set the next value of teh Enumerator in the reference.
		 * if there isn't next value , null is been set in the referece. 
		 * @param obj
		 * @param enumerator
		 * @return boolean returns the value of enumerator.MoveNext().
		 */
		public static bool ForEachNextObj(ref System.Object obj, IEnumerator enumerator) {
			if (enumerator.MoveNext()) {
				obj = enumerator.Current;
				return true;
			}
			obj = null;
			return false;
		}
    
		/**
		 * This method check if the loop can continued.
		 * if the step is positive it check that the counter is smaller than the limit.
		 * if the step is negative it check that the counter is bigger than the limit. 
		 * @param LoopFor
		 * @return boolean
		 */
		private static bool CheckContinueLoop(ObjectFor LoopFor) {
			//TODO:
			//throw new NotImplementedException("MSVB.Compilerservices.flowcontrol needs help");
			IComparable iComparable = (IComparable)LoopFor.Counter;
			
			if (iComparable != null) {
				int i = iComparable.CompareTo(LoopFor.Limit);
				bool isCountSmallThenLimit = i<=0;
				return LoopFor.PositiveStep ? isCountSmallThenLimit : !isCountSmallThenLimit;
			}
			throw new ArgumentException("Argument_IComparable2 loop control variable"); // + Utils.VBFriendlyName(LoopFor.Counter)));
				//TODO: verify this and the above are the same and remove.
				//throw new ArgumentException(Utils.GetResourceString(
				//	"Argument_IComparable2", "loop control variable",
				//	Utils.VBFriendlyName(LoopFor.Counter)));
			}
    
		/**
		 * This method throws exception if the input is Valuetype  
		 * @param obj the object that need to be checked
		 */
		public static void CheckForSyncLockOnValueType(object obj) {
			//TODO:
			//throw new NotImplementedException("MSVB.Compilerservices.flowcontrol needs help");
			if (obj != null && obj.GetType().IsValueType)
				throw new ArgumentException(Utils.GetResourceString("SyncLockRequiresReferenceType1 "));
			//TODO: verify this and the above are the same and remove.
			//if (obj != null && ObjectStaticWrapper.GetType(obj).get_IsValueType())
			//	throw new ArgumentException(Utils.GetResourceString(
			//		"SyncLockRequiresReferenceType1",Utils.VBFriendlyName(obj)));
		}
	}
}
