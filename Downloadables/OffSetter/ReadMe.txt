This is an ASAP-coded .net5 application to automate offset changes in .uasset files for modding purposes.

In order to make extensive edits of .uasset and .uexp files and keep them pars-able for the UE4 engine,
you should update offsets updated according to size changes in different blocks of the files. This tool
can handle all the offsets "offsetting" in some of the scenarios as long as you can tell it where
and how much the size was changed.

v 1.0

Supported offseting cases:

- Changed size of names block of .uasset file.
- Changed size of imports block of .uasset file.
- Changed structure size in .uexp file (sub-structure size marks in the .uexp should be changed manually).

Instructions:
How to use:

0. Replace filenames in all the 3 .bats to absolute filenames of OffSetter.exe according to your installation folder.
1. Drag'n'drop file to update offsets into the bat you need (NamesTable.bat for changes in names table, ImportsTable.bat
for changes in imports table and Exports.bat for changes in data exported to .uexp).
OR
Run via terminal, first argument is file to tweak, second is mode key. -n, -i and -e for the modes according to
changes scenario.
2. Input net sizechange and countchange or sizechange offset (you will be told what to input, inputs depend on mode).
3. Validate the .offset-postfixed file copy (for example, rename to what original file is and try using DRG Parser).

Warning:

Only delete pre-"offsetting" file copy after sucessful validation, in case you delete the original file and do
something with "offsetting", it will be faster to start again from fresh game files.

Always use the tool between altering sizes of 2 different blocks. "Safe and easy" usage does not include changing
sizes of 2 blocks in one pass. All-modes run ability is POSTPONED!