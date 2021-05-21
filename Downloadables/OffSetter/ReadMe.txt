This is an ASAP-coded .net5 application to automate offset changes in .uasset files for modding purposes.
You have .net5 runtime intalled to run the tool! You can get it here: https://dotnet.microsoft.com/download/dotnet/5.0

In order to make extensive edits of .uasset and .uexp files and keep them pars-able for the UE4 engine,
you should update offsets updated according to size changes in different blocks of the files. This tool
can handle all the offsets "offsetting" in some of the scenarios as long as you can tell it where
and how much the size was changed.

v 1.3

Supported offseting cases:

- Changed size of Name Map of .uasset file.
- Changed size of Import Map of .uasset file.
- Changed size of Export Map of .uasset file.
- Changed structure size in .uexp file (sub-structure size marks in the .uexp should be changed manually).

Instructions:
How to use:

0. Replace filenames in all the 3 .bats to absolute filenames of OffSetter.exe according to your installation folder.
1. Drag'n'drop file to update offsets into the bat you need (NamesTable.bat for changes in names table, ImportsTable.bat
for changes in imports table and Exports.bat for changes in data exported to .uexp) and input values you are asked to.
OR
Run via terminal, first argument is file to tweak, then keys and values.
2. Input net sizechange and countchange or sizechange offset (you will be told what to input, inputs depend on mode).
3. Validate the .offset-postfixed file copy (for example, rename to what original file is and try using DRG Parser).

Supposed command structure is: [mode key] [value(s)] [additional key(s)]

Relative values order is SizeChange -> CountChange -> SizeChangeOffset (skipping those not needed for current mode).
If you pass insufficient number of values, you will be prompted to input them, being told which you need to input.

Mode keys:
-n: NameMap, needs size change and count change
-i: ImportMap, needs size change and count change
-edef: ExportMap, needs size change and count change
-e: Exports (size changes in .uexp), needs size change and SERIAL offset of structure changed size

Additional keys:
-m: Muted. No "Done!" shoutout and awating for input to terminate the process. Also no prompts to input, thus insufficient 
arguments will cause an exception.
-r: Reverted saving. Sets backup file to be .offset-postfixed, new file takes the name of the original file.

Example:
-n 30 1 is used if you add 1 name in Name Map and the whole definition takes 30 bytes.

Warning:

Only delete pre-"offsetting" file copy after sucessful validation, in case you delete the original file and do
something with "offsetting", it will be faster to start again from fresh game files.

Always use the tool between altering sizes of 2 different blocks. "Safe and easy" usage does not include changing
sizes of 2 blocks in one pass. All-modes run ability is POSTPONED (probably will never be enabled, since DAUM exists)!