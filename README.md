Silk
=====

Silk is a library and application (Weave) to expose features of CIL that
aren't available in all CIL high level languages. 

Usage
=====

Add a reference to Silk.dll to your project. Call appropriate Silk.Cil methods and then rewrite the assembly after compilation with Weave.
This can be automated by adding the following to your msbuild project file. Add the appropriate path to Weave for your project.

<Target Name="AfterBuild">
<Exec Command="Weave.exe --input $(TargetPath) --output $(TargetPath)" Condition="$(OS) == 'Windows_NT'" />
<Exec Command="mono Weave.exe --input $(TargetPath) --output $(TargetPath)" Condition="$(OS) != 'Windows_NT'" />
</Target>