# DoxygenComments
Visual Studio extension for auto-generating Doxygen-style comments in C ++ code

## Extension capabilities

Use a keyboard shortcut `Alt + T` for all actions   
All lines can be enabled / disabled or configured at your discretion   

* Create file comment with copyright info and custom text after comment (`#pragma once`, `#include <stdafx.h>`, `#include <classHeader.h>`, `using namespace MyLib;` etc). You can customize comment styles for different kinds of files (header, source or inline)   
![Alt Text](https://github.com/n0lavar/DoxygenComments/blob/main/gifs/header.gif)

* Create class comment with template parameters list 
![Alt Text](https://github.com/n0lavar/DoxygenComments/blob/main/gifs/class.gif)

* Create function/method comment with all template and common parameters
![Alt Text](https://github.com/n0lavar/DoxygenComments/blob/main/gifs/fn.gif)

* Create a centered comment to separate logical blocks in the file 
![Alt Text](https://github.com/n0lavar/DoxygenComments/blob/main/gifs/line_comment.gif)

A list of all elements for which a comment can be generated
* file (header, source, inline)
* class
* fn (or method)
* macro
* struct
* union
* typedef
* namespace
* enum
* for all other elements a default comment will be generated: `//!<`

## Examples

[qxLib](https://github.com/n0lavar/qxLib) uses Doxigen based documentation and this extension.


## License

DoxygenComments is available under the GPL-3.0 License. See LICENSE.txt.   
You can download the code and build the VSIX yourself or support me and buy it on the market and get updates.   


## Authors

DoxygenComments was mainly written and is maintained by Nick Khrapov.
(nick.khrapov@gmail.com). See the git commit log for other authors.
