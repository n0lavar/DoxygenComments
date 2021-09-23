using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace DoxygenComments
{
    class SettingsPage: DialogPage
    {
        private const string sIndent              = "Indent";
        private const string sAddBrief            = "Add @brief";
        private const string sDetils              = "@details default string";
        private const string sDetailsDesc         = "If empty - don't add @details";
        private const string sAddAuthor           = "Add @author";
        private const string sAddDate             = "Add @date";
        private const string sAddCopyright        = "Add @copyright";
        private const string sAddTParam           = "Add @tparam";
        private const string sAddParam            = "Add @param";
        private const string sAddFile             = "Add @file";
        private const string sAddBlankLines       = "Add blank lines before the first tag and after the last";

        // Common settings
        private const string sCommonSettings = "Common settings";

        [Category(sCommonSettings)]
        [DisplayName("Author name")]
        [Description("Author name for @author")]
        public string Author { get; set; } = "Cool Guy";

        [Category(sCommonSettings)]
        [DisplayName("Сopyright string")]
        [Description("Сopyright string for @copyright. {year} will be replaced with the current year. This option supports inserting multiple lines, but remember that for correct documentation they must form a Doxygen paragraph (there must be no blank lines).")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] Copyright { get; set; } = { "© Cool Guy, {year}. All right reserved." };

        [Category(sCommonSettings)]
        [DisplayName("Column limit")]
        [Description("Column limit for full line comments")]
        public int ColumnLimit { get; set; } = 80;

        [Category(sCommonSettings)]
        [DisplayName("Tab width")]
        [Description("Tab width for correct alighment if [Indent char] is tab")]
        public int TabWidth { get; set; } = 4;

        public enum EIndentChar 
        { 
            Space, 
            Tab 
        };

        [Category(sCommonSettings)]
        [DisplayName("Indent char")]
        public EIndentChar IndentChar { get; set; } = EIndentChar.Space;

        public char GetIndentChar()
        {
            return IndentChar == EIndentChar.Space ? ' ' : '\t';
        }

        [Category(sCommonSettings)]
        [DisplayName("Tag char")]
        [Description("The character to be inserted before the tag name. Usually \\ (\\brief) or @ (@brief) is used.")]
        public char TagChar { get; set; } = '@';

        [Category(sCommonSettings)]
        [DisplayName("Add \"- \" to param, tparam and retval")]
        public bool AddРyphen { get; set; } = false;

        [Category(sCommonSettings)]
        [DisplayName("@brief dictionary")]
        [Description("A list of lines, each of which consists of the first key word (object name) and a value that will be automatically written in the @brief field")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] BriefDictionary { get; set; } = {
            "begin Return iterator to beginning",
            "end Return iterator to end",
            "rbegin Return reverse iterator to reverse beginning",
            "rend Return reverse iterator to reverse end",
            "cbegin Return const iterator to beginning",
            "cend Return const iterator to end",
            "crbegin Return const reverse iterator to reverse beginning",
            "crend Return const reverse iterator to reverse end",
            "operator+ operator+",
            "operator- operator-",
            "operator* operator*",
            "operator/ operator/",
            "operator% operator%",
            "operator^ operator^",
            "operator& operator&",
            "operator| operator|",
            "operator~ operator~",
            "operator! operator!",
            "operator= operator=",
            "operator< operator<",
            "operator> operator>",
            "operator+= operator+=",
            "operator-= operator-=",
            "operator*= operator*=",
            "operator/= operator/=",
            "operator%= operator%=",
            "operator^= operator^=",
            "operator&= operator&=",
            "operator|= operator|=",
            "operator<< operator<<",
            "operator>> operator>>",
            "operator<<= operator<<=",
            "operator>>= operator>>=",
            "operator== operator==",
            "operator!= operator!=",
            "operator<= operator<=",
            "operator>= operator>=",
            "operator<=> operator<=>",
            "operator&& operator&&",
            "operator|| operator||",
            "operator++ operator++",
            "operator-- operator--",
            "operator, operator,",
            "operator->* operator->*",
            "operator-> operator->",
        };

        [Category(sCommonSettings)]
        [DisplayName("@retval dictionary")]
        [Description("A list of lines, each of which consists of the first key word (object name) and a value that will be automatically written in the @retval field")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] RetvalDictionary { get; set; } = {
            "begin iterator to beginning",
            "end iterator to end",
            "rbegin reverse iterator to reverse beginning",
            "rend reverse iterator to reverse end",
            "cbegin const iterator to beginning",
            "cend const iterator to end",
            "crbegin const reverse iterator to reverse beginning",
            "crend const reverse iterator to reverse end",
            "operator+= this object reference",
            "operator-= this object reference",
            "operator*= this object reference",
            "operator/= this object reference",
            "operator%= this object reference",
            "operator^= this object reference",
            "operator&= this object reference",
            "operator|= this object reference",
            "operator>>= this object reference",
            "operator<<= this object reference",
            "operator= this object reference",
            "operator< true, if left object is less than right",
            "operator> true, if left object is greater than right",
            "operator== true, if objects are equal",
            "operator!= true, if objects are not equal",
            "operator<= true, if left object is less or equal than right",
            "operator>= true, if left object is greater or equal than right",
            "operator-> this object pointer",
            "operator* this object reference",
        };

        [Category(sCommonSettings)]
        [DisplayName("@param dictionary")]
        [Description("A list of lines, each of which consists of the first key word (param name) and a value that will be automatically written in the @param field")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] ParamDictionary { get; set; } = {
            "args template parameter pack",
        };

        [Category(sCommonSettings)]
        [DisplayName("@tparam dictionary")]
        [Description("A list of lines, each of which consists of the first key word (tparam name) and a value that will be automatically written in the @tparam field")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] TParamDictionary { get; set; } = {
            "Args template parameter pack type",
        };

        // Header files header
        private const string sHeaderFilesHeader = "Header files header";

        [Category(sHeaderFilesHeader)]
        [DisplayName(sIndent)]
        public int HeaderFilesHeaderIndent { get; set; } = 4;

        [Category(sHeaderFilesHeader)]
        [DisplayName(sAddFile)]
        public bool HeaderFilesHeaderAddName { get; set; } = true;

        [Category(sHeaderFilesHeader)]
        [DisplayName(sAddBrief)]
        public bool HeaderFilesHeaderAddBrief { get; set; } = true;

        [Category(sHeaderFilesHeader)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string HeaderFilesHeaderDetails{ get; set; } = "~";

        [Category(sHeaderFilesHeader)]
        [DisplayName(sAddAuthor)]
        public bool HeaderFilesHeaderAddAuthor { get; set; } = true;

        [Category(sHeaderFilesHeader)]
        [DisplayName(sAddDate)]
        public bool HeaderFilesHeaderAddDate { get; set; } = true;

        [Category(sHeaderFilesHeader)]
        [DisplayName(sAddCopyright)]
        public bool HeaderFilesHeaderAddCopyright { get; set; } = true;

        [Category(sHeaderFilesHeader)]
        [DisplayName("Additional text for header files")]
        [Description("Additional text that will be placed after the main comment text. Can be used for [#pragma once], [namespace MyLib { }] or whatever you wish")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] HeaderFilesHeaderAdditionalText { get; set; } = { "#pragma once" };

        [Category(sHeaderFilesHeader)]
        [DisplayName(sAddBlankLines)]
        public bool HeaderFilesHeaderAddBlankLines { get; set; } = true;

        // Source files header
        private const string sSourceFilesHeader = "Source files header";

        [Category(sSourceFilesHeader)]
        [DisplayName(sIndent)]
        public int SourceFilesHeaderIndent { get; set; } = 4;

        [Category(sSourceFilesHeader)]
        [DisplayName(sAddFile)]
        public bool SourceFilesHeaderAddName { get; set; } = true;

        [Category(sSourceFilesHeader)]
        [DisplayName(sAddBrief)]
        public bool SourceFilesHeaderAddBrief { get; set; } = true;

        [Category(sSourceFilesHeader)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string SourceFilesHeaderDetails{ get; set; } = "~";

        [Category(sSourceFilesHeader)]
        [DisplayName(sAddAuthor)]
        public bool SourceFilesHeaderAddAuthor { get; set; } = true;

        [Category(sSourceFilesHeader)]
        [DisplayName(sAddDate)]
        public bool SourceFilesHeaderAddDate { get; set; } = true;

        [Category(sSourceFilesHeader)]
        [DisplayName(sAddCopyright)]
        public bool SourceFilesHeaderAddCopyright { get; set; } = true;

        [Category(sSourceFilesHeader)]
        [DisplayName("PCH path")]
        [Description("If specified, PCH include will be added. For example, for value [\"stdafx.h\"] [#include \"stdafx.h\"] will be added")]
        public string SourceFilesHeaderPCHPath { get; set; } = "\"stdafx.h\"";

        [Category(sSourceFilesHeader)]
        [DisplayName("Header include extension")]
        [Description("If specified, header include will be added. For example, for value [.h] and [test.cpp] file [#include \"test.h\"] will be added")]
        public string SourceFilesHeaderIncludeExtension { get; set; } = ".h";

        [Category(sSourceFilesHeader)]
        [DisplayName("Root")]
        [Description("If specified, header include will be added based on [Source files header] option. For example, for value [.h] of [Source files header], value [inc] of this option and and [C:\\TestProject\\inc\\folder1\\folder2\\test.cpp] file [#include <folder1\\folder2\\test.h>] will be added. You can specify multiple roots by separating them with spaces.")]
        public string SourceFilesHeaderIncludeRoot { get; set; } = "inc";

        [Category(sSourceFilesHeader)]
        [DisplayName("Additional text for source files")]
        [Description("Additional text that will be placed after the main comment text. Can be used for [using namespace MyLib;], [namespace MyLib { }] or whatever you wish")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] SourceFilesHeaderAdditionalText { get; set; } = { "" };

        [Category(sSourceFilesHeader)]
        [DisplayName(sAddBlankLines)]
        public bool SourceFilesHeaderAddBlankLines { get; set; } = true;

        // Inline files header
        private const string sInlineFilesHeader = "Inline files header";

        [Category(sInlineFilesHeader)]
        [DisplayName(sIndent)]
        public int InlineFilesHeaderIndent { get; set; } = 4;

        [Category(sInlineFilesHeader)]
        [DisplayName(sAddFile)]
        public bool InlineFilesHeaderAddName { get; set; } = true;

        [Category(sInlineFilesHeader)]
        [DisplayName(sAddBrief)]
        public bool InlineFilesHeaderAddBrief { get; set; } = true;

        [Category(sInlineFilesHeader)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string InlineFilesHeaderDetails{ get; set; } = "~";

        [Category(sInlineFilesHeader)]
        [DisplayName(sAddAuthor)]
        public bool InlineFilesHeaderAddAuthor { get; set; } = true;

        [Category(sInlineFilesHeader)]
        [DisplayName(sAddDate)]
        public bool InlineFilesHeaderAddDate { get; set; } = true;

        [Category(sInlineFilesHeader)]
        [DisplayName(sAddCopyright)]
        public bool InlineFilesHeaderAddCopyright { get; set; } = true;

        [Category(sInlineFilesHeader)]
        [DisplayName("Additional text for inline files")]
        [Description("Additional text that will be placed after the main comment text. Can be used for [namespace MyLib { }] or whatever you wish")]
        [TypeConverter(typeof(StringArrayConverter))]
        public string[] InlineFilesHeaderAdditionalText { get; set; } = { "" };

        [Category(sInlineFilesHeader)]
        [DisplayName(sAddBlankLines)]
        public bool InlineFilesHeaderAddBlankLines { get; set; } = true;

        // Class comment
        private const string sClassComment = "Class comment";

        [Category(sClassComment)]
        [DisplayName(sIndent)]
        public int ClassIndent { get; set; } = 4;

        [Category(sClassComment)]
        [DisplayName("Add @class")]
        public bool ClassAddName { get; set; } = true;

        [Category(sClassComment)]
        [DisplayName(sAddBrief)]
        public bool ClassAddBrief { get; set; } = true;

        [Category(sClassComment)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string ClassDetails{ get; set; } = "~";

        [Category(sClassComment)]
        [DisplayName(sAddTParam)]
        public bool ClassAddTparam { get; set; } = true;

        [Category(sClassComment)]
        [DisplayName(sAddAuthor)]
        public bool ClassAddAuthor { get; set; } = false;

        [Category(sClassComment)]
        [DisplayName(sAddDate)]
        public bool ClassAddDate { get; set; } = false;

        [Category(sClassComment)]
        [DisplayName(sAddBlankLines)]
        public bool ClassAddBlankLines { get; set; } = true;

        // Struct comment
        private const string sStructComment = "Struct comment";

        [Category(sStructComment)]
        [DisplayName(sIndent)]
        public int StructIndent { get; set; } = 4;

        [Category(sStructComment)]
        [DisplayName("Add @struct")]
        public bool StructAddName { get; set; } = true;

        [Category(sStructComment)]
        [DisplayName(sAddBrief)]
        public bool StructAddBrief { get; set; } = true;

        [Category(sStructComment)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string StructDetails{ get; set; } = "";

        [Category(sStructComment)]
        [DisplayName(sAddTParam)]
        public bool StructAddTparam { get; set; } = true;

        [Category(sStructComment)]
        [DisplayName(sAddAuthor)]
        public bool StructAddAuthor { get; set; } = false;

        [Category(sStructComment)]
        [DisplayName(sAddDate)]
        public bool StructAddDate { get; set; } = false;

        [Category(sStructComment)]
        [DisplayName(sAddBlankLines)]
        public bool StructAddBlankLines { get; set; } = false;

        // Function / method comment
        private const string sFunctionComment = "Function / method comment";

        [Category(sFunctionComment)]
        [DisplayName(sIndent)]
        public int FunctionIndent { get; set; } = 4;

        [Category(sFunctionComment)]
        [DisplayName("Add @fn")]
        public bool FunctionAddName { get; set; } = false;

        [Category(sFunctionComment)]
        [DisplayName(sAddBrief)]
        public bool FunctionAddBrief { get; set; } = true;

        [Category(sFunctionComment)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string FunctionDetails{ get; set; } = "";

        [Category(sFunctionComment)]
        [DisplayName(sAddTParam)]
        public bool FunctionAddTparam { get; set; } = true;

        [Category(sFunctionComment)]
        [DisplayName(sAddParam)]
        public bool FunctionAddParam { get; set; } = true;

        [Category(sFunctionComment)]
        [DisplayName("Add @retval")]
        public bool FunctionAddRetval { get; set; } = true;

        [Category(sFunctionComment)]
        [DisplayName(sAddAuthor)]
        public bool FunctionAddAuthor { get; set; } = false;

        [Category(sFunctionComment)]
        [DisplayName(sAddDate)]
        public bool FunctionAddDate { get; set; } = false;

        [Category(sFunctionComment)]
        [DisplayName(sAddBlankLines)]
        public bool FunctionAddBlankLines { get; set; } = false;

        // Macro comment
        private const string sMacroComment = "Macro comment";

        [Category(sMacroComment)]
        [DisplayName(sIndent)]
        public int MacroIndent { get; set; } = 4;

        [Category(sMacroComment)]
        [DisplayName("Add @def")]
        public bool MacroAddName { get; set; } = true;

        [Category(sMacroComment)]
        [DisplayName(sAddBrief)]
        public bool MacroAddBrief { get; set; } = true;

        [Category(sMacroComment)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string MacroDetails{ get; set; } = "";

        [Category(sMacroComment)]
        [DisplayName(sAddParam)]
        public bool MacroAddParam { get; set; } = true;

        [Category(sMacroComment)]
        [DisplayName(sAddAuthor)]
        public bool MacroAddAuthor { get; set; } = false;

        [Category(sMacroComment)]
        [DisplayName(sAddDate)]
        public bool MacroAddDate { get; set; } = false;

        [Category(sMacroComment)]
        [DisplayName(sAddBlankLines)]
        public bool MacroAddBlankLines { get; set; } = false;

        // Namespace comment
        private const string sNamespaceComment = "Namespace comment";

        [Category(sNamespaceComment)]
        [DisplayName(sIndent)]
        public int NamespaceIndent { get; set; } = 4;

        [Category(sNamespaceComment)]
        [DisplayName("Add @namespace")]
        public bool NamespaceAddName { get; set; } = true;

        [Category(sNamespaceComment)]
        [DisplayName(sAddBrief)]
        public bool NamespaceAddBrief { get; set; } = true;

        [Category(sNamespaceComment)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string NamespaceDetails{ get; set; } = "";

        [Category(sNamespaceComment)]
        [DisplayName(sAddAuthor)]
        public bool NamespaceAddAuthor { get; set; } = false;

        [Category(sNamespaceComment)]
        [DisplayName(sAddDate)]
        public bool NamespaceAddDate { get; set; } = false;

        [Category(sNamespaceComment)]
        [DisplayName(sAddBlankLines)]
        public bool NamespaceAddBlankLines { get; set; } = false;

        // Union comment
        private const string sUnionComment = "Union comment";

        [Category(sUnionComment)]
        [DisplayName(sIndent)]
        public int UnionIndent { get; set; } = 4;

        [Category(sUnionComment)]
        [DisplayName("Add @union")]
        public bool UnionAddName { get; set; } = true;

        [Category(sUnionComment)]
        [DisplayName(sAddBrief)]
        public bool UnionAddBrief { get; set; } = true;

        [Category(sUnionComment)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string UnionDetails{ get; set; } = "";

        [Category(sUnionComment)]
        [DisplayName(sAddAuthor)]
        public bool UnionAddAuthor { get; set; } = false;

        [Category(sUnionComment)]
        [DisplayName(sAddDate)]
        public bool UnionAddDate { get; set; } = false;

        [Category(sUnionComment)]
        [DisplayName(sAddBlankLines)]
        public bool UnionAddBlankLines { get; set; } = false;

        // Typedef comment
        private const string sTypedefComment = "Typedef comment";

        [Category(sTypedefComment)]
        [DisplayName(sIndent)]
        public int TypedefIndent { get; set; } = 4;

        [Category(sTypedefComment)]
        [DisplayName("Add @typedef")]
        public bool TypedefAddName { get; set; } = true;

        [Category(sTypedefComment)]
        [DisplayName(sAddBrief)]
        public bool TypedefAddBrief { get; set; } = true;

        [Category(sTypedefComment)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string TypedefDetails{ get; set; } = "";

        [Category(sTypedefComment)]
        [DisplayName(sAddAuthor)]
        public bool TypedefAddAuthor { get; set; } = false;

        [Category(sTypedefComment)]
        [DisplayName(sAddDate)]
        public bool TypedefAddDate { get; set; } = false;

        [Category(sTypedefComment)]
        [DisplayName(sAddBlankLines)]
        public bool TypedefAddBlankLines { get; set; } = false;

        // Enum comment
        private const string sEnumComment = "Enum comment";

        [Category(sEnumComment)]
        [DisplayName(sIndent)]
        public int EnumIndent { get; set; } = 4;

        [Category(sEnumComment)]
        [DisplayName("Add @enum")]
        public bool EnumAddName { get; set; } = true;

        [Category(sEnumComment)]
        [DisplayName(sAddBrief)]
        public bool EnumAddBrief { get; set; } = true;

        [Category(sEnumComment)]
        [DisplayName(sDetils)]
        [Description(sDetailsDesc)]
        public string EnumDetails{ get; set; } = "";

        [Category(sEnumComment)]
        [DisplayName(sAddAuthor)]
        public bool EnumAddAuthor { get; set; } = false;

        [Category(sEnumComment)]
        [DisplayName(sAddDate)]
        public bool EnumAddDate { get; set; } = false;

        [Category(sEnumComment)]
        [DisplayName(sAddBlankLines)]
        public bool EnumAddBlankLines { get; set; } = false;
    }
}
