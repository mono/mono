/*
  Copyright (C) 2004 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
#include <stdarg.h>
#include "jni.h"

#ifdef _WIN32
#include <malloc.h>
#define ALLOCA _alloca
#else
#if defined(__FreeBSD__) || defined(__NetBSD__) || defined(__OpenBSD__)
#include <stdlib.h>
#else
#include <alloca.h>
#endif
#define ALLOCA alloca
#endif

static jobject JNICALL NewObject(JNIEnv* pEnv, jclass clazz, jmethodID methodID, ...)
{
	jobject o;
	va_list args;
	va_start(args, methodID);
	o = (*pEnv)->NewObjectV(pEnv, clazz, methodID, args);
	va_end(args);
	return o;
}

#define MAKE_ARG_ARRAY(pEnv, args, argarray) \
do { \
	jbyte sig[257];\
	int argc = (*pEnv)->GetMethodArgs(pEnv, methodID, sig);\
	int i;\
	argarray = (jvalue*)ALLOCA(argc * sizeof(jvalue));\
	for(i = 0; i < argc; i++)\
	{\
		switch(sig[i])\
		{\
		case 'Z':\
		case 'B':\
		case 'S':\
		case 'C':\
		case 'I':\
			argarray[i].i = va_arg(args, jint);\
			break;\
		case 'J':\
			argarray[i].j = va_arg(args, jlong);\
			break;\
		case 'L':\
			argarray[i].l = va_arg(args, jobject);\
			break;\
		case 'D':\
			argarray[i].d = va_arg(args, double);\
			break;\
		case 'F':\
			argarray[i].f = (float)va_arg(args, double);\
			break;\
		}\
	}\
} while(0);

static jobject JNICALL NewObjectV(JNIEnv* pEnv, jclass clazz, jmethodID methodID, va_list args)
{
	jvalue* argarray;
	MAKE_ARG_ARRAY(pEnv, args, argarray);
	return (*pEnv)->NewObjectA(pEnv, clazz, methodID, argarray);
}

#define MAKE_METHOD(Type, type) \
static type JNICALL Call##Type##Method(JNIEnv* pEnv, jobject obj, jmethodID methodID, ...)\
{\
	type ret;\
	va_list args;\
	va_start(args, methodID);\
	ret = (*pEnv)->Call##Type##MethodV(pEnv, obj, methodID, args);\
	va_end(args);\
	return ret;\
}\
static type JNICALL Call##Type##MethodV(JNIEnv* pEnv, jobject obj, jmethodID methodID, va_list args)\
{\
	jvalue* argarray;\
	MAKE_ARG_ARRAY(pEnv, args, argarray);\
	return (*pEnv)->Call##Type##MethodA(pEnv, obj, methodID, argarray);\
}\
static type JNICALL CallNonvirtual##Type##Method(JNIEnv* pEnv, jobject obj, jclass clazz, jmethodID methodID, ...)\
{\
	type ret;\
	va_list args;\
	va_start(args, methodID);\
	ret = (*pEnv)->CallNonvirtual##Type##MethodV(pEnv, obj, clazz, methodID, args);\
	va_end(args);\
	return ret;\
}\
static type JNICALL CallNonvirtual##Type##MethodV(JNIEnv* pEnv, jobject obj, jclass clazz, jmethodID methodID, va_list args)\
{\
	jvalue* argarray;\
	MAKE_ARG_ARRAY(pEnv, args, argarray);\
	return (*pEnv)->CallNonvirtual##Type##MethodA(pEnv, obj, clazz, methodID, argarray);\
}\
static type JNICALL CallStatic##Type##Method(JNIEnv* pEnv, jclass clazz, jmethodID methodID, ...)\
{\
	type ret;\
	va_list args;\
	va_start(args, methodID);\
	ret = (*pEnv)->CallStatic##Type##MethodV(pEnv, clazz, methodID, args);\
	va_end(args);\
	return ret;\
}\
static type JNICALL CallStatic##Type##MethodV(JNIEnv* pEnv, jclass clazz, jmethodID methodID, va_list args)\
{\
	jvalue* argarray;\
	MAKE_ARG_ARRAY(pEnv, args, argarray);\
	return (*pEnv)->CallStatic##Type##MethodA(pEnv, clazz, methodID, argarray);\
}

MAKE_METHOD(Object, jobject)
MAKE_METHOD(Boolean, jboolean)
MAKE_METHOD(Byte, jbyte)
MAKE_METHOD(Char, jchar)
MAKE_METHOD(Short, jshort)
MAKE_METHOD(Int, jint)
MAKE_METHOD(Long, jlong)
MAKE_METHOD(Float, jfloat)
MAKE_METHOD(Double, jdouble)

static void JNICALL CallVoidMethod(JNIEnv* pEnv, jobject obj, jmethodID methodID, ...)
{
	va_list args;
	va_start(args, methodID);
	(*pEnv)->CallVoidMethodV(pEnv, obj, methodID, args);
	va_end(args);
}
static void JNICALL CallVoidMethodV(JNIEnv* pEnv, jobject obj, jmethodID methodID, va_list args)
{
	jvalue* argarray;
	MAKE_ARG_ARRAY(pEnv, args, argarray);
	(*pEnv)->CallVoidMethodA(pEnv, obj, methodID, argarray);
}
static void JNICALL CallNonvirtualVoidMethod(JNIEnv* pEnv, jobject obj, jclass clazz, jmethodID methodID, ...)
{
	va_list args;
	va_start(args, methodID);
	(*pEnv)->CallNonvirtualVoidMethodV(pEnv, obj, clazz, methodID, args);
	va_end(args);
}
static void JNICALL CallNonvirtualVoidMethodV(JNIEnv* pEnv, jobject obj, jclass clazz, jmethodID methodID, va_list args)
{
	jvalue* argarray;
	MAKE_ARG_ARRAY(pEnv, args, argarray);
	(*pEnv)->CallNonvirtualVoidMethodA(pEnv, obj, clazz, methodID, argarray);
}
static void JNICALL CallStaticVoidMethod(JNIEnv* pEnv, jclass clazz, jmethodID methodID, ...)
{
	va_list args;
	va_start(args, methodID);
	(*pEnv)->CallStaticVoidMethodV(pEnv, clazz, methodID, args);
	va_end(args);
}
static void JNICALL CallStaticVoidMethodV(JNIEnv* pEnv, jclass clazz, jmethodID methodID, va_list args)
{
	jvalue* argarray;
	MAKE_ARG_ARRAY(pEnv, args, argarray);
	(*pEnv)->CallStaticVoidMethodA(pEnv, clazz, methodID, argarray);
}

static void* JNIEnv_vtable[] = 
{
	0, // void JNICALL reserved0();
	0, // void JNICALL reserved1();
	0, // void JNICALL reserved2();
	0, // void JNICALL reserved3();

	0, // jint JNICALL GetVersion();

	0, // jclass JNICALL DefineClass(const char *name, jobject loader, const jbyte *buf, jsize len);
	0, // jclass JNICALL FindClass(const char *name);

	0, // jmethodID JNICALL FromReflectedMethod(jobject method);
	0, // jfieldID JNICALL FromReflectedField(jobject field);
	0, // jobject JNICALL ToReflectedMethod(jclass clazz, jmethodID methodID);

	0, // jclass JNICALL GetSuperclass(jclass sub);
	0, // jboolean JNICALL IsAssignableFrom(jclass sub, jclass sup);

	0, // jobject JNICALL ToReflectedField(jclass clazz, jfieldID fieldID);

	0, // jint JNICALL Throw(jthrowable obj);
	0, // jint JNICALL ThrowNew(jclass clazz, const char *msg);
	0, // jthrowable JNICALL ExceptionOccurred();
	0, // void JNICALL ExceptionDescribe();
	0, // void JNICALL ExceptionClear();
	0, // void JNICALL FatalError(const char *msg);

	0, // jint JNICALL PushLocalFrame(jint capacity); 
	0, // jobject JNICALL PopLocalFrame(jobject result);

	0, // jobject JNICALL NewGlobalRef(jobject lobj);
	0, // void JNICALL DeleteGlobalRef(jobject gref);
	0, // void JNICALL DeleteLocalRef(jobject obj);
	0, // jboolean JNICALL IsSameObject(jobject obj1, jobject obj2);

	0, // jobject JNICALL NewLocalRef(jobject ref);
	0, // jint JNICALL EnsureLocalCapacity(jint capacity);

	0, // jobject JNICALL AllocObject(jclass clazz);
	NewObject, // jobject JNICALL NewObject(jclass clazz, jmethodID methodID, ...);
	NewObjectV, // jobject JNICALL NewObjectV(jclass clazz, jmethodID methodID, va_list args);
	0, // jobject JNICALL NewObjectA(jclass clazz, jmethodID methodID, jvalue *args);

	0, // jclass JNICALL GetObjectClass(jobject obj);
	0, // jboolean JNICALL IsInstanceOf(jobject obj, jclass clazz);

	0, // jmethodID JNICALL GetMethodID(jclass clazz, const char *name, const char *sig);

	CallObjectMethod, // jobject JNICALL CallObjectMethod(jobject obj, jmethodID methodID, ...);
	CallObjectMethodV, // jobject JNICALL CallObjectMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // jobject JNICALL CallObjectMethodA(jobject obj, jmethodID methodID, jvalue * args);

	CallBooleanMethod, // jboolean JNICALL CallBooleanMethod(jobject obj, jmethodID methodID, ...);
	CallBooleanMethodV, // jboolean JNICALL CallBooleanMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // jboolean JNICALL CallBooleanMethodA(jobject obj, jmethodID methodID, jvalue * args);

	CallByteMethod, // jbyte JNICALL CallByteMethod(jobject obj, jmethodID methodID, ...);
	CallByteMethodV, // jbyte JNICALL CallByteMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // jbyte JNICALL CallByteMethodA(jobject obj, jmethodID methodID, jvalue *args);

	CallCharMethod, // jchar JNICALL CallCharMethod(jobject obj, jmethodID methodID, ...);
	CallCharMethodV, // jchar JNICALL CallCharMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // jchar JNICALL CallCharMethodA(jobject obj, jmethodID methodID, jvalue *args);

	CallShortMethod, // jshort JNICALL CallShortMethod(jobject obj, jmethodID methodID, ...);
	CallShortMethodV, // jshort JNICALL CallShortMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // jshort JNICALL CallShortMethodA(jobject obj, jmethodID methodID, jvalue *args);

	CallIntMethod, // jint JNICALL CallIntMethod(jobject obj, jmethodID methodID, ...);
	CallIntMethodV, // jint JNICALL CallIntMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // jint JNICALL CallIntMethodA(jobject obj, jmethodID methodID, jvalue *args);

	CallLongMethod, // jlong JNICALL CallLongMethod(jobject obj, jmethodID methodID, ...);
	CallLongMethodV, // jlong JNICALL CallLongMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // jlong JNICALL CallLongMethodA(jobject obj, jmethodID methodID, jvalue *args);

	CallFloatMethod, // jfloat JNICALL CallFloatMethod(jobject obj, jmethodID methodID, ...);
	CallFloatMethodV, // jfloat JNICALL CallFloatMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // jfloat JNICALL CallFloatMethodA(jobject obj, jmethodID methodID, jvalue *args);

	CallDoubleMethod, // jdouble JNICALL CallDoubleMethod(jobject obj, jmethodID methodID, ...);
	CallDoubleMethodV, // jdouble JNICALL CallDoubleMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // jdouble JNICALL CallDoubleMethodA(jobject obj, jmethodID methodID, jvalue *args);

	CallVoidMethod, // void JNICALL CallVoidMethod(jobject obj, jmethodID methodID, ...);
	CallVoidMethodV, // void JNICALL CallVoidMethodV(jobject obj, jmethodID methodID, va_list args);
	0, // void JNICALL CallVoidMethodA(jobject obj, jmethodID methodID, jvalue * args);

	CallNonvirtualObjectMethod, // jobject JNICALL CallNonvirtualObjectMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualObjectMethodV, // jobject JNICALL CallNonvirtualObjectMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // jobject JNICALL CallNonvirtualObjectMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue * args);

	CallNonvirtualBooleanMethod, // jboolean JNICALL CallNonvirtualBooleanMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualBooleanMethodV, // jboolean JNICALL CallNonvirtualBooleanMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // jboolean JNICALL CallNonvirtualBooleanMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue * args);

	CallNonvirtualByteMethod, // jbyte JNICALL CallNonvirtualByteMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualByteMethodV, // jbyte JNICALL CallNonvirtualByteMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // jbyte JNICALL CallNonvirtualByteMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue *args);

	CallNonvirtualCharMethod, // jchar JNICALL CallNonvirtualCharMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualCharMethodV, // jchar JNICALL CallNonvirtualCharMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // jchar JNICALL CallNonvirtualCharMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue *args);

	CallNonvirtualShortMethod, // jshort JNICALL CallNonvirtualShortMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualShortMethodV, // jshort JNICALL CallNonvirtualShortMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // jshort JNICALL CallNonvirtualShortMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue *args);

	CallNonvirtualIntMethod, // jint JNICALL CallNonvirtualIntMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualIntMethodV, // jint JNICALL CallNonvirtualIntMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // jint JNICALL CallNonvirtualIntMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue *args);

	CallNonvirtualLongMethod, // jlong JNICALL CallNonvirtualLongMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualLongMethodV, // jlong JNICALL CallNonvirtualLongMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // jlong JNICALL CallNonvirtualLongMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue *args);

	CallNonvirtualFloatMethod, // jfloat JNICALL CallNonvirtualFloatMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualFloatMethodV, // jfloat JNICALL CallNonvirtualFloatMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // jfloat JNICALL CallNonvirtualFloatMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue *args);

	CallNonvirtualDoubleMethod, // jdouble JNICALL CallNonvirtualDoubleMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualDoubleMethodV, // jdouble JNICALL CallNonvirtualDoubleMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // jdouble JNICALL CallNonvirtualDoubleMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue *args);

	CallNonvirtualVoidMethod, // void JNICALL CallNonvirtualVoidMethod(jobject obj, jclass clazz, jmethodID methodID, ...);
	CallNonvirtualVoidMethodV, // void JNICALL CallNonvirtualVoidMethodV(jobject obj, jclass clazz, jmethodID methodID, va_list args);
	0, // void JNICALL CallNonvirtualVoidMethodA(jobject obj, jclass clazz, jmethodID methodID, jvalue * args);

	0, // jfieldID JNICALL GetFieldID(jclass clazz, const char *name, const char *sig);

	0, // jobject JNICALL GetObjectField(jobject obj, jfieldID fieldID);
	0, // jboolean JNICALL GetBooleanField(jobject obj, jfieldID fieldID);
	0, // jbyte JNICALL GetByteField(jobject obj, jfieldID fieldID);
	0, // jchar JNICALL GetCharField(jobject obj, jfieldID fieldID);
	0, // jshort JNICALL GetShortField(jobject obj, jfieldID fieldID);
	0, // jint JNICALL GetIntField(jobject obj, jfieldID fieldID);
	0, // jlong JNICALL GetLongField(jobject obj, jfieldID fieldID);
	0, // jfloat JNICALL GetFloatField(jobject obj, jfieldID fieldID);
	0, // jdouble JNICALL GetDoubleField(jobject obj, jfieldID fieldID);

	0, // void JNICALL SetObjectField(jobject obj, jfieldID fieldID, jobject val);
	0, // void JNICALL SetBooleanField(jobject obj, jfieldID fieldID, jboolean val);
	0, // void JNICALL SetByteField(jobject obj, jfieldID fieldID, jbyte val);
	0, // void JNICALL SetCharField(jobject obj, jfieldID fieldID, jchar val);
	0, // void JNICALL SetShortField(jobject obj, jfieldID fieldID, jshort val);
	0, // void JNICALL SetIntField(jobject obj, jfieldID fieldID, jint val);
	0, // void JNICALL SetLongField(jobject obj, jfieldID fieldID, jlong val);
	0, // void JNICALL SetFloatField(jobject obj, jfieldID fieldID, jfloat val);
	0, // void JNICALL SetDoubleField(jobject obj, jfieldID fieldID, jdouble val);

	0, // jmethodID JNICALL GetStaticMethodID(jclass clazz, const char *name, const char *sig);

	CallStaticObjectMethod, // jobject JNICALL CallStaticObjectMethod(jclass clazz, jmethodID methodID, ...);
	CallStaticObjectMethodV, // jobject JNICALL CallStaticObjectMethodV(jclass clazz, jmethodID methodID, va_list args);
	0, // jobject JNICALL CallStaticObjectMethodA(jclass clazz, jmethodID methodID, jvalue *args);

	CallStaticBooleanMethod, // jboolean JNICALL CallStaticBooleanMethod(jclass clazz, jmethodID methodID, ...);
	CallStaticBooleanMethodV, // jboolean JNICALL CallStaticBooleanMethodV(jclass clazz, jmethodID methodID, va_list args);
	0, // jboolean JNICALL CallStaticBooleanMethodA(jclass clazz, jmethodID methodID, jvalue *args);

	CallStaticByteMethod, // jbyte JNICALL CallStaticByteMethod(jclass clazz, jmethodID methodID, ...);
	CallStaticByteMethodV, // jbyte JNICALL CallStaticByteMethodV(jclass clazz, jmethodID methodID, va_list args);
	0, // jbyte JNICALL CallStaticByteMethodA(jclass clazz, jmethodID methodID, jvalue *args);

	CallStaticCharMethod, // jchar JNICALL CallStaticCharMethod(jclass clazz, jmethodID methodID, ...);
	CallStaticCharMethodV, // jchar JNICALL CallStaticCharMethodV(jclass clazz, jmethodID methodID, va_list args);
	0, // jchar JNICALL CallStaticCharMethodA(jclass clazz, jmethodID methodID, jvalue *args);

	CallStaticShortMethod, // jshort JNICALL CallStaticShortMethod(jclass clazz, jmethodID methodID, ...);
	CallStaticShortMethodV, // jshort JNICALL CallStaticShortMethodV(jclass clazz, jmethodID methodID, va_list args);
	0, // jshort JNICALL CallStaticShortMethodA(jclass clazz, jmethodID methodID, jvalue *args);

	CallStaticIntMethod, // jint JNICALL CallStaticIntMethod(jclass clazz, jmethodID methodID, ...);
	CallStaticIntMethodV, // jint JNICALL CallStaticIntMethodV(jclass clazz, jmethodID methodID, va_list args);
	0, // jint JNICALL CallStaticIntMethodA(jclass clazz, jmethodID methodID, jvalue *args);

	CallStaticLongMethod, // jlong JNICALL CallStaticLongMethod(jclass clazz, jmethodID methodID, ...);
	CallStaticLongMethodV, // jlong JNICALL CallStaticLongMethodV(jclass clazz, jmethodID methodID, va_list args);
	0, // jlong JNICALL CallStaticLongMethodA(jclass clazz, jmethodID methodID, jvalue *args);

	CallStaticFloatMethod, // jfloat JNICALL CallStaticFloatMethod(jclass clazz, jmethodID methodID, ...);
	CallStaticFloatMethodV, // jfloat JNICALL CallStaticFloatMethodV(jclass clazz, jmethodID methodID, va_list args);
	0, // jfloat JNICALL CallStaticFloatMethodA(jclass clazz, jmethodID methodID, jvalue *args);

	CallStaticDoubleMethod, // jdouble JNICALL CallStaticDoubleMethod(jclass clazz, jmethodID methodID, ...);
	CallStaticDoubleMethodV, // jdouble JNICALL CallStaticDoubleMethodV(jclass clazz, jmethodID methodID, va_list args);
	0, // jdouble JNICALL CallStaticDoubleMethodA(jclass clazz, jmethodID methodID, jvalue *args);

	CallStaticVoidMethod, // void JNICALL CallStaticVoidMethod(jclass cls, jmethodID methodID, ...);
	CallStaticVoidMethodV, // void JNICALL CallStaticVoidMethodV(jclass cls, jmethodID methodID, va_list args);
	0, // void JNICALL CallStaticVoidMethodA(jclass cls, jmethodID methodID, jvalue * args);

	0, // jfieldID JNICALL GetStaticFieldID(jclass clazz, const char *name, const char *sig);

	0, // jobject JNICALL GetObjectField(jobject obj, jfieldID fieldID);
	0, // jboolean JNICALL GetBooleanField(jobject obj, jfieldID fieldID);
	0, // jbyte JNICALL GetByteField(jobject obj, jfieldID fieldID);
	0, // jchar JNICALL GetCharField(jobject obj, jfieldID fieldID);
	0, // jshort JNICALL GetShortField(jobject obj, jfieldID fieldID);
	0, // jint JNICALL GetIntField(jobject obj, jfieldID fieldID);
	0, // jlong JNICALL GetLongField(jobject obj, jfieldID fieldID);
	0, // jfloat JNICALL GetFloatField(jobject obj, jfieldID fieldID);
	0, // jdouble JNICALL GetDoubleField(jobject obj, jfieldID fieldID);

	0, // void JNICALL SetObjectField(jobject obj, jfieldID fieldID, jobject val);
	0, // void JNICALL SetBooleanField(jobject obj, jfieldID fieldID, jboolean val);
	0, // void JNICALL SetByteField(jobject obj, jfieldID fieldID, jbyte val);
	0, // void JNICALL SetCharField(jobject obj, jfieldID fieldID, jchar val);
	0, // void JNICALL SetShortField(jobject obj, jfieldID fieldID, jshort val);
	0, // void JNICALL SetIntField(jobject obj, jfieldID fieldID, jint val);
	0, // void JNICALL SetLongField(jobject obj, jfieldID fieldID, jlong val);
	0, // void JNICALL SetFloatField(jobject obj, jfieldID fieldID, jfloat val);
	0, // void JNICALL SetDoubleField(jobject obj, jfieldID fieldID, jdouble val);

	0, // jstring JNICALL NewString(const jchar *unicode, jsize len);
	0, // jsize JNICALL GetStringLength(jstring str);
	0, // const jchar *JNICALL GetStringChars(jstring str, jboolean *isCopy);
	0, // void JNICALL ReleaseStringChars(jstring str, const jchar *chars);

	0, // jstring JNICALL NewStringUTF(const char *utf);
	0, // jsize JNICALL GetStringUTFLength(jstring str);
	0, // const char* JNICALL GetStringUTFChars(jstring str, jboolean *isCopy);
	0, // void JNICALL ReleaseStringUTFChars(jstring str, const char* chars);

	0, // jsize JNICALL GetArrayLength(jarray array);

	0, // jobjectArray JNICALL NewObjectArray(jsize len, jclass clazz, jobject init);
	0, // jobject JNICALL GetObjectArrayElement(jobjectArray array, jsize index);
	0, // void JNICALL SetObjectArrayElement(jobjectArray array, jsize index, jobject val);

	0, // jbooleanArray JNICALL NewBooleanArray(jsize len);
	0, // jbyteArray JNICALL NewByteArray(jsize len);
	0, // jcharArray JNICALL NewCharArray(jsize len);
	0, // jshortArray JNICALL NewShortArray(jsize len);
	0, // jintArray JNICALL NewIntArray(jsize len);
	0, // jlongArray JNICALL NewLongArray(jsize len);
	0, // jfloatArray JNICALL NewFloatArray(jsize len);
	0, // jdoubleArray JNICALL NewDoubleArray(jsize len);

	0, // jboolean * JNICALL GetBooleanArrayElements(jbooleanArray array, jboolean *isCopy);
	0, // jbyte * JNICALL GetByteArrayElements(jbyteArray array, jboolean *isCopy);
	0, // jchar * JNICALL GetCharArrayElements(jcharArray array, jboolean *isCopy);
	0, // jshort * JNICALL GetShortArrayElements(jshortArray array, jboolean *isCopy);
	0, // jint * JNICALL GetIntArrayElements(jintArray array, jboolean *isCopy);
	0, // jlong * JNICALL GetLongArrayElements(jlongArray array, jboolean *isCopy);
	0, // jfloat * JNICALL GetFloatArrayElements(jfloatArray array, jboolean *isCopy);
	0, // jdouble * JNICALL GetDoubleArrayElements(jdoubleArray array, jboolean *isCopy);

	0, // void JNICALL ReleaseBooleanArrayElements(jbooleanArray array, jboolean *elems, jint mode);
	0, // void JNICALL ReleaseByteArrayElements(jbyteArray array, jbyte *elems, jint mode);
	0, // void JNICALL ReleaseCharArrayElements(jcharArray array, jchar *elems, jint mode);
	0, // void JNICALL ReleaseShortArrayElements(jshortArray array, jshort *elems, jint mode);
	0, // void JNICALL ReleaseIntArrayElements(jintArray array, jint *elems, jint mode);
	0, // void JNICALL ReleaseLongArrayElements(jlongArray array, jlong *elems, jint mode);
	0, // void JNICALL ReleaseFloatArrayElements(jfloatArray array, jfloat *elems, jint mode);
	0, // void JNICALL ReleaseDoubleArrayElements(jdoubleArray array, jdouble *elems, jint mode);

	0, // void JNICALL GetBooleanArrayRegion(jbooleanArray array, jsize start, jsize l, jboolean *buf);
	0, // void JNICALL GetByteArrayRegion(jbyteArray array, jsize start, jsize len, jbyte *buf);
	0, // void JNICALL GetCharArrayRegion(jcharArray array, jsize start, jsize len, jchar *buf);
	0, // void JNICALL GetShortArrayRegion(jshortArray array, jsize start, jsize len, jshort *buf);
	0, // void JNICALL GetIntArrayRegion(jintArray array, jsize start, jsize len, jint *buf);
	0, // void JNICALL GetLongArrayRegion(jlongArray array, jsize start, jsize len, jlong *buf);
	0, // void JNICALL GetFloatArrayRegion(jfloatArray array, jsize start, jsize len, jfloat *buf);
	0, // void JNICALL GetDoubleArrayRegion(jdoubleArray array, jsize start, jsize len, jdouble *buf);

	0, // void JNICALL SetBooleanArrayRegion(jbooleanArray array, jsize start, jsize l, jboolean *buf);
	0, // void JNICALL SetByteArrayRegion(jbyteArray array, jsize start, jsize len, jbyte *buf);
	0, // void JNICALL SetCharArrayRegion(jcharArray array, jsize start, jsize len, jchar *buf);
	0, // void JNICALL SetShortArrayRegion(jshortArray array, jsize start, jsize len, jshort *buf);
	0, // void JNICALL SetIntArrayRegion(jintArray array, jsize start, jsize len, jint *buf);
	0, // void JNICALL SetLongArrayRegion(jlongArray array, jsize start, jsize len, jlong *buf);
	0, // void JNICALL SetFloatArrayRegion(jfloatArray array, jsize start, jsize len, jfloat *buf);
	0, // void JNICALL SetDoubleArrayRegion(jdoubleArray array, jsize start, jsize len, jdouble *buf);

	0, // jint JNICALL RegisterNatives(jclass clazz, const JNINativeMethod *methods, jint nMethods);
	0, // jint JNICALL UnregisterNatives(jclass clazz);

	0, // jint JNICALL MonitorEnter(jobject obj);
	0, // jint JNICALL MonitorExit(jobject obj);

	0, // jint JNICALL GetJavaVM(JavaVM **vm);

	0, // void JNICALL GetStringRegion(jstring str, jsize start, jsize len, jchar *buf);
	0, // void JNICALL GetStringUTFRegion(jstring str, jsize start, jsize len, char *buf);

	0, // void* JNICALL GetPrimitiveArrayCritical(jarray array, jboolean *isCopy);
	0, // void JNICALL ReleasePrimitiveArrayCritical(jarray array, void *carray, jint mode);

	0, // const jchar* JNICALL GetStringCritical(jstring string, jboolean *isCopy);
	0, // void JNICALL ReleaseStringCritical(jstring string, const jchar *cstring);

	0, // jweak JNICALL NewWeakGlobalRef(jobject obj);
	0, // void JNICALL DeleteWeakGlobalRef(jweak ref);

	0, // jboolean JNICALL ExceptionCheck();

	0, // jobject JNICALL NewDirectByteBuffer(void* address, jlong capacity);
	0, // void* JNICALL GetDirectBufferAddress(jobject buf);
	0  // jlong JNICALL GetDirectBufferCapacity(jobject buf);
};

JNIEXPORT void** JNICALL ikvm_GetJNIEnvVTable()
{
	return JNIEnv_vtable;
}

JNIEXPORT void* JNICALL ikvm_MarshalDelegate(void* p)
{
	return p;
}

typedef jint (JNICALL *PJNI_ONLOAD)(JavaVM* vm, void* reserved);

JNIEXPORT jint JNICALL ikvm_CallOnLoad(PJNI_ONLOAD method, JavaVM* vm, void* reserved)
{
	return method(vm, reserved);
}
