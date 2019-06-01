#include <mono/jit/jit.h>
#include <mono/metadata/assembly.h>
#include <mono/metadata/appdomain.h>
#include <mono/metadata/image.h>
#include <mono/metadata/object.h>
#include <mono/metadata/threads.h>

void internal_test()
{

}

void internal_test1(MonoObject* var)
{

}

void internal_test_float(float v)
{

}

int
main(int argc, char* argv[]) {
	MonoDomain* domain = mono_jit_init_version("UnrealMono", "v4.0.30319");
	mono_thread_set_main(mono_thread_current());
	mono_add_internal_call("UnloadAssemblySecond.TestSecond::InternalMethod", internal_test);
	mono_add_internal_call("UnloadAssemblySecond.TestSecond::InternalMethod1", internal_test1);
	mono_add_internal_call("UnloadAssemblySecond.TestSecond::InternalFloatMethod", internal_test_float);
	MonoDomain* newDomain = mono_domain_create_appdomain("UnrealDomain", NULL);
	bool value = mono_domain_set(newDomain, false);
	MonoImageOpenStatus status;
	MonoAssembly* firtAssembly = mono_assembly_open("F:\\trunk_branch\\custom_software\\MonoEmbedded\\TestCSharp\\Plugins\\UnrealMono\\tools\\Test\\UnloadAssemblySecond\\bin\\Debug\\UnloadAssemblyFirst.dll", &status);
	MonoAssembly* secondAssembly = mono_assembly_open("F:\\trunk_branch\\custom_software\\MonoEmbedded\\TestCSharp\\Plugins\\UnrealMono\\tools\\Test\\UnloadAssemblySecond\\bin\\Debug\\UnloadAssemblySecond.dll", &status);
	MonoImage* ScriptImage = mono_assembly_get_image(secondAssembly);
	MonoClass* monoFooClass = mono_class_from_name(ScriptImage, "UnloadAssemblySecond", "TestSecond");
	MonoMethod* staticMethod = mono_class_get_method_from_name(monoFooClass, "Entry", 0);
	mono_runtime_invoke(staticMethod, NULL, NULL, NULL);
	MonoDomain* domainToUnload = mono_domain_get();
	mono_domain_remove_unused_assembly(secondAssembly);
	MonoImage* firstImage = mono_assembly_get_image(firtAssembly);
	MonoClass* firstClass = mono_class_from_name(firstImage, "UnloadAssemblyFirst", "TestFirst");
	MonoMethod* firstStaticMethod = mono_class_get_method_from_name(firstClass, "FirstStaticMethod", 0);
	mono_runtime_invoke(firstStaticMethod, NULL, NULL, NULL);

	// 在加载一次
	secondAssembly = mono_assembly_open("F:\\trunk_branch\\custom_software\\MonoEmbedded\\TestCSharp\\Plugins\\UnrealMono\\tools\\Test\\UnloadAssemblySecond\\bin\\Debug\\UnloadAssemblySecond.dll", &status);
	ScriptImage = mono_assembly_get_image(secondAssembly);
	monoFooClass = mono_class_from_name(ScriptImage, "UnloadAssemblySecond", "TestSecond");
	staticMethod = mono_class_get_method_from_name(monoFooClass, "Entry", 0);
	mono_runtime_invoke(staticMethod, NULL, NULL, NULL);
	mono_domain_remove_unused_assembly(secondAssembly);
	if (domainToUnload && domainToUnload != mono_get_root_domain())
	{
		mono_domain_set(mono_get_root_domain(), false);
		//mono_thread_pop_appdomain_ref();
		mono_domain_unload(domainToUnload);
	}
	mono_jit_cleanup(domain);
	return 0;
}