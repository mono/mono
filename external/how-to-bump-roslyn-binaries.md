How to bump roslyn

1. Pick a revision of roslyn to use. Ideally, use a revision that Visual Studio is using, but any revision should do.
2. Make a new folder in roslyn-binaries for this revision. Copy over the contents of the *previous folder* into this folder.
3. Make a temporary commit.
4. Find that revision in the 'roslyn compilers' nuget feed: https://dotnet.myget.org/feed/roslyn/package/vsix/7922692f-f018-45e7-8f3f-d3b7c0262841
5. Download the vsix file and unpack it (it's a ZIP file).
6. Inside the vsix you will find lots of metadata, along with executables for this build of roslyn. We want to copy most of these into a new folder in roslyn-binaries, so...
7. Copy all the dll, exe and rsp files from the vsix into our new folder, overwriting the ones from the previous version. It's time to look at the diff.
8. The vsix contains DLLs we don't need to ship. Ideally we will not need to add any new files, but roslyn sometimes picks up new dependencies. Figuring out what to keep will require looking at what facades we ship (note that this is profile-specific - check Facades/subdirs.make) and which of these DLLs are actually used by roslyn instead of Visual Studio.
9. Remove any DLLs from the new folder that we don't need. In practice your commit should be adding 1-2 DLLs at most or something very bad happened.
10. Examine the diffs for the .rsp files. We ship our own versions of the .rsps, so we don't want to carry over the vsix versions, but we *do* want to make note of anything new they have added to the rsp files and add it to ours if necessary.
11. Add and commit your new folder. Push it to a branch and make a PR against roslyn-binaries, then get it merged.

----

11. Okay, we have a new roslyn-binaries. The next step is to prepare a mono bump. Do this by updating the roslyn-binaries submodule to point to the new revision of roslyn-binaries. You can temporarily point it to your own branch for testing and run CI builds against that, just don't merge that into master.
12. Once you commit the submodule bump to your local repository, you can do test builds. **If you do test builds without committing the bump, the build process will erase the bump.**
13. Now we need to change the roslyn paths used by the build process - we're still using the older roslyn folder. We do this by changing the CSC_LOCATION and VBCS_LOCATION variables in configure.ac. They should be something like ```Microsoft.Net.Compilers/3.1.0/csc.exe```, change the version number in those paths, save, and make a local commit. Then do the full autogen, make, make install routine and do some basic tests. This should work.
14. Next, make sure the bump didn't break csi. If you managed the .rsp files correctly in the last step, it should work. This has test coverage, so just pay attention to the automated tests for it.
15. Ensure the 'Linux x64 - prefix sanity checks' lane passed on CI, even if it's currently disabled or not required. This is required for a bump. If it isn't visible on the PR statuses, manually trigger the lane using Jenkins.
16. Ensure that 'make install' output has working csc. Install to a prefix is fine.
17. Verify that the compiler server works, even if it is currently off by default. You can do this by editing your ```mcs/build/config.make``` and changing ENABLE_COMPILER_SERVER to ?=1, doing make clean, and then make -j 1. Use 'top' with the O->COMMAND=mono filter to identify whether the compiler server is being used and working. Also, for safety's sake make sure that all three of the roslyn-binaries paths in there match the expected version.
18. Double-check that you have a clean (points to roslyn-binaries master) PR ready to commit. Now it's msbuild time.

----

19. Prepare a PR against mono/msbuild's xplat-master branch:
20. Modify the CompilerToolsVersion and MicrosoftNetCompilersVersion properties in ```eng/Packages.props``` to point to the same new roslyn packages. The version strings have weird textual info in them so you'll have to figure out the new version string.
21. Verify that a build of this new msbuild succeeds.
22. Verify that you can manually run the MSBuild.dll from this new build using a new source build of mono (i.e. with new roslyn installed to a prefix)
23. Get your PR merged.

----

24. Now that msbuild is updated, we make a final change to the mono PR:
25. Modify ```packaging/MacSDK/msbuild.py```, updating the ```revision =``` line's commit hash to be the commit hash of your post-merge xplat-master commit.
26. Get your PR merged.

----

27. Now we need to update the linux packaging and deploy new packages (this involves changing the https://github.com/mono/linux-packaging-msbuild repository)
28. import upstream xplat-master changes by giving the CI package job a URL to a tarball containing the contents of the latest xplat-master revision.
29. trigger CI lane builds to generate a new package
30. identify necessary changes to the debian/rules file for the build to succeed, if step 29 failed
31. identify any new patches required to make latest xplat-master compatible with debian, including build script changes (see above)
32. eventually the lane succeeds. you should have updated linux packages for debian and ubuntu now.

----

33. Follow up with the VSfM team to make sure they know that roslyn is being bumped, it may break them on the next integration. Ensure the rest of the team knows roslyn has been bumped. Keep an eye out for any new bug reports about issues with csc or msbuild.
