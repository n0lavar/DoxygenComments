* Comments are not generated inside the namespace in the `.inl` file
  ```
  // test.inl
  
  namespace test
  {
  
  // <- comment won't be generated here
  int test_class::get_int()
  {
      return 42;
  }
  
  }
  
  ```
  If there is a namespace in inl, there are no elements in its children   
  
* `@retval` is generated for functions returning `auto` == `void`   
  ```
  /**
    @brief  
    @retval -     <- retval generated
  **/
  auto foo()
  {
      return;
  }
  ```
  no way to distinguish, `vsCMTypeRefOther` is returned for `auto` type
* No comment is generated   
  Check that there is no macro above the element for which you want to create a comment.   
  If it is there, try adding `;` at the end.   
  ```
   SOME_MACRO(arg)
   
   // <- comment won't be generated till you add ; after macro above
   my_class(void) noexcept = default;

  ```
* No `@param` comment for `...` parameter packs   
  ```
  /**
      @brief 
      @param pszFormat - 
      @param           -    <- no ... name
  **/
  void append_sprintf(const_pointer pszFormat, ...) noexcept;
  ```
  In this parameter, its name is empty and cannot be distinguished from a typed parameter without a name

