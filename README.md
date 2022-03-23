# DoxygenComments
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](http://paypal.me/nickkhrapov)

## VS2019
[![Version](https://vsmarketplacebadge.apphb.com/version-short/NickKhrapov.DoxygenComments.svg)](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments)
[![Installs](https://vsmarketplacebadge.apphb.com/installs-short/NickKhrapov.DoxygenComments.svg)](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments)
[![Ratings](https://vsmarketplacebadge.apphb.com/rating-star/NickKhrapov.DoxygenComments.svg)](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments)   

## VS2022
[![Version](https://vsmarketplacebadge.apphb.com/version-short/NickKhrapov.DoxygenComments2022.svg)](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments2022)
[![Installs](https://vsmarketplacebadge.apphb.com/installs-short/NickKhrapov.DoxygenComments2022.svg)](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments2022)
[![Ratings](https://vsmarketplacebadge.apphb.com/rating-star/NickKhrapov.DoxygenComments2022.svg)](https://marketplace.visualstudio.com/items?itemName=NickKhrapov.DoxygenComments2022)   
Visual Studio extension for auto-generating Doxygen-style comments in C ++ code.   

## Extension capabilities

* Use a keyboard shortcut `Alt + T` to create a comment.   
* All lines can be enabled / disabled or configured at your discretion.   
* Tag indentation can be configured for each code element.   
* Sometimes the same function parameters or method names appear in many places in your code, and you want their documentation to look the same. You can specify the names of such objects and a comment to them in the dictionary, and this comment will be inserted automatically
* The following styles are supported:
  * Simple   
    ```
    /**
        @brief Foo
    **/
    ```
  * SlashBlock   
    ```
    ///
    /// @brief Foo
    ///
    ```
  * Javadoc   
    ```
    /**
     *  @brief Foo
     */
    ```
  * Qt   
    ```
    /*!
     *  @brief Foo
     */
    ```
 * You can make a comment block with a specific type more visible by filling the first and last lines of the comment:
   ```
    /*******************************************************************************
     *
     *  @file      test.h
     *  @author    Khrapov
     *  @date      24.09.2021
     *  @copyright © Nick Khrapov, 2021. All right reserved.
     *
     ******************************************************************************/
    #pragma once
   ```
* Auto-generation of comments for trivial functions:
  * "ClassName object constructor" for constructors
  * "ClassName object destructor" for destructors
  * @brief and @retval for getters based on function name:
    ```
    /**
        @brief  Get answer to the ultimate question of life the universe and everything
        @retval - answer to the ultimate question of life the universe and everything
    **/
    int GetAnswerToTheUltimateQuestionOfLifeTheUniverseAndEverything()
    {
        return 42;
    }
    ```
  * @brief and @param for setters based on function name:
    ```
    /**
        @brief Set programmer salary
        @param nNewSalary - programmer salary
    **/
    void SetProgrammerSalary(size_t nNewSalary = -1)
    {
    }
    ```
* A list of all elements for which a comment can be generated:   
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
