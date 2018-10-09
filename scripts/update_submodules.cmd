@if not exist .git goto :eof

git submodule update --init --recursive && @goto :ok
git submodule init && git submodule update --recursive && @goto :ok

:: This looks wrong but is what update_submodules.sh does.
git submodule init && git submodule update && @goto :error

@echo Git submodules could not be updated. Compilation will fail.
@exit /b 1

:error
@echo Could not recursively update all git submodules. You may experience compilation problems if some submodules are out of date.
@exit /b 1

:ok
@echo Git submodules updated successfully.
