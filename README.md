# DoxygenComments
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](http://paypal.me/nickkhrapov)
[![Version](https://vsmarketplacebadge.apphb.com/version-short/NickKhrapov.DoxygenComments.svg)](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments)
[![Installs](https://vsmarketplacebadge.apphb.com/installs-short/NickKhrapov.DoxygenComments.svg)](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments)
[![Ratings](https://vsmarketplacebadge.apphb.com/rating-star/NickKhrapov.DoxygenComments.svg)](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments)   
Visual Studio extension for auto-generating Doxygen-style comments in C ++ code.   

## Extension capabilities

* Use a keyboard shortcut `Alt + T` to create a comment.   
* All lines can be enabled / disabled or configured at your discretion.   
* Tag indentation can be configured for each code element.   
* Sometimes the same function parameters or method names appear in many places in your code, and you want their documentation to look the same. You can specify the names of such objects and a comment to them in the dictionary, and this comment will be inserted automatically

## Examples

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
![file.gif](https://s9.gifyu.com/images/file.gif)

* Create class comment with template parameters list   
![class.gif](https://s9.gifyu.com/images/class.gif)

* Create function/method comment with template and usual parameters along with return value   
![fn.gif](https://s9.gifyu.com/images/fn.gif)

* Create macro comment with all parameters   
![macro.gif](https://s9.gifyu.com/images/macro.gif)

* Create a centered comment to separate logical blocks in the file   
![line_comment.gif](https://s9.gifyu.com/images/line_comment.gif)

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

## Project examples

[qxLib](https://github.com/n0lavar/qxLib) uses Doxigen based [documentation](https://n0lavar.github.io/qxLib/files.html) and this extension.


## License

DoxygenComments is available under the GPL-3.0 License. See LICENSE.txt.   


## Authors

DoxygenComments was mainly written and is maintained by Nick Khrapov.
(nick.khrapov@gmail.com). See the git commit log for other authors.

## Can't fix issues

See [can't fix issues file](https://github.com/n0lavar/DoxygenComments/blob/main/ISSUES.md)

## Todo list

See [todo](https://github.com/n0lavar/DoxygenComments/projects/1)
