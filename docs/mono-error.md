# MonoError

MonoError is the latest attempt at cleaning up and sanitizing error handling in the runtime.
This document highlights some of the design goals and decisions, the implementation and the migration strategy.

## Design goals

- Replace the majority of the adhoc error handling subsystems present today in the runtime. Each one is broken
is a subtle way, has slightly different semantics and error conversion between them is spot, at best.

- Map well to the final destination of all runtime errors: managed exceptions. This includes been compatible
with .net when it comes to the kind of exception produced by a given error condition.

- Be explicit, lack any magic. The loader-error setup does control flow happens in the background through a TLS variable,
which made it very brittle and error prone.

- Explicit and multiple error scopes. Make it possible to have multiple error scopes and make them explicit. We need to
support nested scopes during type loading, even if reporting is flat.

- Be as simple as possible. Error handling is the hardest part of the runtime to test so it must be simple. Which means
complex error reporting, such as chaining, is out of question.

## Current implementation

The current implementation exists in mono-error.h and mono-error-internals.h. The split is so API users can consume errors, but they
are not supported to be able to produce them - such use case has yet to arise.

### Writing a function that produces errors

```
/**
 *
 * @returns NULL on error
 */
void*
my_function (int a, MonoError *error)
{
	mono_error_init (error);
	if (a <= 0) {//
		mono_error_set_argument (error, "a", "argument a must be bigger than zero, it was %d", a);
		return NULL;
	}
	return malloc (a);
}
```

Important points from the above:

- Add a "MonoError *error" argument as the last to your function
- Call "mono_error_init (error)" unconditionally at the beginning
- Call one of the mono_error_set functions based on what managed exception this should produce and the available information
- Document that a NULL returns means an error


### Writing a function that consumes errors

```
void
other_function (void)
{
	MonoError error;
	void *res;
	
	res = my_function (10, &error);
	//handling the error:
	//1st option: raise an exception
	mono_error_raise (&error); //won't raise in case of no error

	//2nd option: legacy code that can't handle failures:
	g_assert (mono_error_ok (&error));

	//3rd option: convert to an exception and handle it upwards
	if (!res)
		return mono_error_convert_to_exception (&error); //implicit cleanup

	//4th option: ignore
	mono_error_cleanup (&error);
}
```

Important points from the above:

- Add a "MonoError error" variable to the required argument
- Pass it to the required function and always do something with the result
- Given we're still transitioning, not all code can handle in the same ways
- Since all calls that take a MonoError init it, only check it after the first call;


## Handling the transition

The transition work is not complete and we're doing it piece-by-piece to ensure we
don't introduce massive regressions in the runtime. The idea is to move the least
amount of code a time to use the new error machinery.

Due to the loader error been transparently passed around, we need a setup where
we can slowly contain its presence in the runtime. With that in mind, here are the
rules for code conversion:

- Code that takes a MonoError* arguments MUST NOT accept or produce loader errors.
Add asserts both on the entry and exit paths to ensure that if you're not sure.

- Sometimes we need to keep the old semantics while porting a piece of code. Use
mono_error_set_from_loader_error and mono_loader_set_error_from_mono_error to move
between the two error models.

- Mono API functions that need to call functions which take a MonoError should
assert on failure as there's no adequate alternative at this point.

- When possible, change the function signature. If not, add a _checked variant.

## Design issues

- Memory management of the error setting functions is not consistent or clear
- Use a static initializer in the declaration site instead of mono_error_init?
- Force an error to always be set or only when there's an exception situation? I.E. mono_class_from_name failing to find the class X finding the class but it failed to load.
- g_assert (mono_errork_ok (&error)) could be replaced by a macro that uses g_error so we can see the error contents on crashes.


