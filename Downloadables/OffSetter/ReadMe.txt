This is an ASAP-coded .net5 application to automate offset changes in .uasset files for modding purposes.

In order to make extensive edits of .uasset and .uexp files and keep them pars-able for the UE4 engine,
you should update offsets updated according to size changes in different blocks of the files. This tool
can handle all the offsets "offsetting" in some of the scenarios as long as you can tell it where
and how much the size was changed.

v 0.0

Supported offseting cases:

- Changed size of names block of .uasset file.
- Changed structure size in .uexp file (sub-structure size marks in the .uexp should be changed manually).

Instructions:

- Drag'n'drop a .uasset that needs offsets updating into the .exe.
- Input names number change.
- Input net size change of names block.
- Input change of a struct size in the .uexp.
- Input the struct's serial offset, increased by net size change of names block.
- You should get a copy of the original file with changed offsets with .offset "suffix".

Warning:

It is strongly recommended to perform "offsetting" in small steps along editing the files. It can be easy to
get confused with numbers you need to input after performing complex operations with the files, including size changes
in different places.

"Easy and safe" step can include:
1) Multiple changes in names block (you only need to know net changes of names count
and the block's size).
2) Size change of a single exported structure referred in exports block. You need to know its initial SerialOffset and
its net size change. YOU WILL ALSO HAVE TO ADD NAMES BLOCK SIZE CHANGE TO SIZECHANGE SERIAL OFFSET!

Try to keep all changes between "offsettings" fitting these 2 limitations of single "offsetting pass".

DO NOT apply exported structure size change corrections prior to applying all changes connected with sizes of Names,
Imports and Exprots blocks! If changes only involve exported struct and Names block size changes, then you can apply both
corretions with single "offsetting pass". 

Avoid changing sizes of 2 exported structures without automatic "offsetting". If you have changed net size of multiple
exported structures and want to use this tool for automated offsets updating, you will need to pass the file to the tool
multiple times (original file, then its .offset copy and so on). You will have to take into account offsets you made 
on previous steps.

It is still possible to perform all the offsettings with multiple "passes" after all changes are done. In that case
you need understanding of .uasset strucure and some calculations to be done or have a reliable access to changed offsets
after each "pass".