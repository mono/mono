Known conditionals:

* MONO_FEATURE_WEB_STACK: when we have the full web stack.

* MONO_FEATURE_NEW_TLS: we are using the new TLS implementation.

* MONO_FEATURE_LOGGING: whether we want the internal logging API.

* MONO_NOT_SUPPORTED: not supported in Mono.

* MONO_NOT_IMPLEMENTED: not yet implemented in Mono.

Other defines which are used when building the BCL and tests:

* MONO_INSIDE_SYSTEM: we're building System.dll.

* MONO_INSIDE_MONO_SECURITY: we're building Mono.Security.dll.

Extern aliases:

* MONO_SECURITY_ALIAS: we're using Mono.Security from the "MonoSecurity" extern alias.

## How to import new version update

```
wget https://patch-diff.githubusercontent.com/raw/Microsoft/referencesource/pull/{pull-request-number}.patch
patch -p1 <{pull-request-number}.patch
```

After that manually review .rej files generated during patching (they are hidden by .gitignore)
