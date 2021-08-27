# DoxygenComments
Visual Studio extension for auto-generating Doxygen-style comments in C ++ code

## Extension capabilities

Use a keyboard shortcut `Alt + T` for all actions.   
All lines can be enabled / disabled or configured at your discretion.   
Tag indentation can be configured for each code element.   
Sometimes the same function parameters or method names appear in many places in your code, and you want their documentation to look the same. You can specify the names of such objects and a comment to them in the dictionary, and this comment will be inserted automatically


* Create file comment with copyright info and custom text after comment, such as
  * `#pragma once`
  * `#include <stdafx.h>`
  * `#include <TestClass.h>`
  * `using namespace MyLib;`
  * `namespace MyLib
    {
    }`
  
  etc   
  You can customize comment styles for different kinds of files (header, source or inline)   
![Alt Text](https://github.com/n0lavar/DoxygenComments/blob/main/gifs/file.gif)

* Create class comment with template parameters list   
![Alt Text](https://github.com/n0lavar/DoxygenComments/blob/main/gifs/class.gif)

* Create function/method comment with template and usual parameters along with return value   
![Alt Text](https://github.com/n0lavar/DoxygenComments/blob/main/gifs/fn.gif)

* Create macro comment with all parameters   
![Alt Text](https://github.com/n0lavar/DoxygenComments/blob/main/gifs/macro.gif)

* Create a centered comment to separate logical blocks in the file   
![Alt Text](https://github.com/n0lavar/DoxygenComments/blob/main/gifs/line_comment.gif)

A list of all elements for which a comment can be generated:
* file (header, source, inline)
* class
* function (or method)
* macro
* struct
* union
* typedef
* namespace
* enum
* for all other elements a default comment will be generated: `//!<`

## Examples

[qxLib](https://github.com/n0lavar/qxLib) uses Doxigen based [documentation](https://n0lavar.github.io/qxLib/files.html) and this extension.


## License

DoxygenComments is available under the GPL-3.0 License. See LICENSE.txt.   
You can download VSIX from [marketplace](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments).   


## Authors

DoxygenComments was mainly written and is maintained by Nick Khrapov.
(nick.khrapov@gmail.com). See the git commit log for other authors.

## Can't fix issues

See [can't fix issues file](https://github.com/n0lavar/DoxygenComments/blob/main/ISSUES.md)

## Todo list

See [todo](https://github.com/n0lavar/DoxygenComments/projects/1)
