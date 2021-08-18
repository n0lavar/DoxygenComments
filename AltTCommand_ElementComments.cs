using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using EnvDTE;

namespace DoxygenComments
{
    internal sealed partial class AltTCommand
    {
        private void CreateLineComment(EditPoint editPoint, string sText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            sText = sText.Trim();

            int nFillerTotal = Settings.ColumnLimit - 5 - sText.Length;
            int nFillerRight = nFillerTotal / 2;
            int nFillerLeft  = nFillerTotal - nFillerRight;

            string sCommentText = "// ";

            if (nFillerLeft >= 0)
            {
                sCommentText += new string('=', nFillerLeft);
                sCommentText += sText.Length == 0 ? '=' : ' ';
            }

            sCommentText += sText;

            if (nFillerRight >= 0)
            {
                sCommentText += sText.Length == 0 ? '=' : ' ';
                sCommentText += new string('=', nFillerRight);
            }

            editPoint.StartOfLine();
            editPoint.Delete(editPoint.LineLength);
            editPoint.Insert(sCommentText);
        }

        private void CreateHeaderFileHeader(EditPoint editPoint, TextDocument textDocument)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string sPragmaOnce = Settings.HeaderFilesHeaderAddPragmaOnce 
                ? "#pragma once" 
                : "";

            CreateComment(
                editPoint,
                0,
                Settings.HeaderFilesHeaderIndent,
                Settings.HeaderFilesHeaderEmptyStringTags,
                Settings.HeaderFilesHeaderAddName ? "@file" : null,
                textDocument.Parent.Name,
                Settings.HeaderFilesHeaderAddBrief ? "" : null,
                Settings.HeaderFilesHeaderDetails,
                null,
                null,
                null,
                Settings.HeaderFilesHeaderAddAuthor,
                Settings.HeaderFilesHeaderAddDate,
                Settings.HeaderFilesHeaderAddCopyright,
                sPragmaOnce);
        }

        private void CreateSourceFileHeader(EditPoint editPoint, TextDocument textDocument)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string sAdditionalText = "";
            
            if (Settings.SourceFilesHeaderPCHPath.Length != 0)
                sAdditionalText += "#include " + Settings.SourceFilesHeaderPCHPath + Environment.NewLine;

            if (Settings.SourceFilesHeaderIncludeExtension.Length != 0)
            {
                string sHeaderInclude = textDocument.Parent.Name;

                string sHeadersRegex = "(";
                foreach (var extension in m_SourceFileExtensions)
                    sHeadersRegex += extension + "|";

                sHeadersRegex = sHeadersRegex.Remove(sHeadersRegex.Length - 1);
                sHeadersRegex += ")$";
                sHeadersRegex = sHeadersRegex.Replace(".", "\\.");
                sHeadersRegex = sHeadersRegex.Replace("+", "\\+");

                sHeaderInclude = Regex.Replace(
                    sHeaderInclude, 
                    sHeadersRegex, 
                    Settings.SourceFilesHeaderIncludeExtension);

                if (Settings.SourceFilesHeaderIncludeRoot.Length != 0)
                {
                    var roots = Settings.SourceFilesHeaderIncludeRoot.Split(' ');

                    string path = textDocument.Parent.Path;
                    bool bRemoved = false;
                    foreach (var root in roots)
                    {
                        bRemoved = TryRemoveWithRoot(ref path, root);
                        if (bRemoved)
                            break;
                    }

                    if (bRemoved)
                    {
                        path = path.Replace('\\', '/');
                        sAdditionalText += "#include <" + path + sHeaderInclude + ">";
                    }
                }
                else
                {
                    sAdditionalText += "#include \"" + sHeaderInclude + "\"" + Environment.NewLine;
                }

            }

            CreateComment(
                editPoint,
                0,
                Settings.SourceFilesHeaderIndent,
                Settings.SourceFilesHeaderEmptyStringTags,
                Settings.SourceFilesHeaderAddName ? "@file" : null,
                textDocument.Parent.Name,
                Settings.SourceFilesHeaderAddBrief ? "" : null,
                Settings.SourceFilesHeaderDetails,
                null,
                null,
                null,
                Settings.SourceFilesHeaderAddAuthor,
                Settings.SourceFilesHeaderAddDate,
                Settings.SourceFilesHeaderAddCopyright,
                sAdditionalText);
        }

        private void CreateInlineFileHeader(EditPoint editPoint, TextDocument textDocument)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                editPoint,
                0,
                Settings.InlineFilesHeaderIndent,
                Settings.InlineFilesHeaderEmptyStringTags,
                Settings.InlineFilesHeaderAddName ? "@file" : null,
                textDocument.Parent.Name,
                Settings.InlineFilesHeaderAddBrief ? "" : null,
                Settings.InlineFilesHeaderDetails,
                null,
                null,
                null,
                Settings.InlineFilesHeaderAddAuthor,
                Settings.InlineFilesHeaderAddDate,
                Settings.InlineFilesHeaderAddCopyright,
                null);
        }

        private void CreateClassComment(EditPoint editPoint, int nElementIndent, VCCodeClass classElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<Parameter> tparams = new List<Parameter>();
            foreach (CodeElement tparam in classElement.TemplateParameters)
                tparams.Add(new Parameter(tparam.FullName + " "));

            CreateComment(
                editPoint,
                nElementIndent,
                Settings.ClassIndent,
                Settings.ClassEmptyStringTags,
                Settings.ClassAddName ? "@class" : null,
                classElement.FullName,
                Settings.ClassAddBrief ? "" : null,
                Settings.ClassDetails,
                tparams.ToArray(),
                null,
                null,
                Settings.ClassAddAuthor,
                Settings.ClassAddDate,
                false,
                null);
        }

        private void CreateStructComment(EditPoint editPoint, int nElementIndent, VCCodeStruct structElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<Parameter> tparams = new List<Parameter>();
            foreach (CodeElement tparam in structElement.TemplateParameters)
                tparams.Add(new Parameter(tparam.FullName + " "));

            CreateComment(
                editPoint,
                nElementIndent,
                Settings.StructIndent,
                Settings.StructEmptyStringTags,
                Settings.StructAddName ? "@struct" : null,
                structElement.FullName,
                Settings.StructAddBrief ? "" : null,
                Settings.StructDetails,
                tparams.ToArray(),
                null,
                null,
                Settings.StructAddAuthor,
                Settings.StructAddDate,
                false,
                null);
        }

        private void CreateFunctionComment(EditPoint editPoint, int nElementIndent, VCCodeFunction functionElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<Parameter> tparams = new List<Parameter>();
            foreach (CodeElement tparam in functionElement.TemplateParameters)
            {
                string sName = tparam.FullName.Replace("...", "") + " ";
                tparams.Add(new Parameter(
                    sName, 
                    sName == Settings.TemplateParameterPackTypeName 
                        ? "template parameter pack type" 
                        : null));
            }

            List<Parameter> Params = new List<Parameter>();
            foreach (CodeElement param in functionElement.Parameters)
            {
                Params.Add(new Parameter(
                    param.FullName.Replace("...", "") + " "));
            }

            string sDefaultBrief  = "";
            string sDefaultRetval =  null;
            if ((functionElement.FunctionKind & vsCMFunction.vsCMFunctionConstructor) > 0)
            {
                sDefaultBrief = functionElement.Name + " object constructor";
            }
            else if ((functionElement.FunctionKind & vsCMFunction.vsCMFunctionDestructor) > 0)
            {
                sDefaultBrief = functionElement.Name.Substring(1) + " object destructor";
            }
            else
            {
                // sometimes functionElement.Type.TypeKind = vsCMTypeRef.vsCMTypeRefOther 
                // even if retval is "void"
                bool bVoidRetval = functionElement.Type.TypeKind == vsCMTypeRef.vsCMTypeRefVoid 
                    || Regex.Replace(functionElement.TypeString.Replace("noexcept", ""), @"\s+", "") == "void";

                if (!bVoidRetval)
                {
                    sDefaultRetval =  "";

                    switch (functionElement.Name)
                    {
                    case "begin":
                        sDefaultBrief  = "Return iterator to beginning";
                        sDefaultRetval = "iterator to beginning";
                        break;

                    case "end":
                        sDefaultBrief  = "Return iterator to end";
                        sDefaultRetval = "iterator to end";
                        break;

                    case "rbegin":
                        sDefaultBrief  = "Return reverse iterator to reverse beginning";
                        sDefaultRetval = "reverse iterator to reverse beginning";
                        break;

                    case "rend":
                        sDefaultBrief  = "Return reverse iterator to reverse end";
                        sDefaultRetval = "reverse iterator to reverse end";
                        break;

                    case "cbegin":
                        sDefaultBrief  = "Return const iterator to beginning";
                        sDefaultRetval = "const iterator to beginning";
                        break;

                    case "cend":
                        sDefaultBrief  = "Return const iterator to end";
                        sDefaultRetval = "const iterator to end";
                        break;

                    case "crbegin":
                        sDefaultBrief  = "Return const reverse iterator to reverse beginning";
                        sDefaultRetval = "const reverse iterator to reverse beginning";
                        break;

                    case "crend":
                        sDefaultBrief  = "Return const reverse iterator to reverse end";
                        sDefaultRetval = "const reverse iterator to reverse end";
                        break;

                    case "operator+=":
                    case "operator-=":
                    case "operator*=":
                    case "operator/=":
                    case "operator%=":
                    case "operator^=":
                    case "operator&=":
                    case "operator|=":
                    case "operator>>=":
                    case "operator<<=":
                    case "operator=":
                        sDefaultRetval = "this object reference";
                        break;

                    case "operator<":
                        sDefaultRetval = "true, if left object is less than right";
                        break;

                    case "operator>":
                        sDefaultRetval = "true, if left object is greater than right";
                        break;

                    case "operator==":
                        sDefaultRetval = "true, if objects are equal";
                        break;

                    case "operator!=":
                        sDefaultRetval = "true, if objects are not equal";
                        break;

                    case "operator<=":
                        sDefaultRetval = "true, if left object is less or equal than right";
                        break;

                    case "operator>=":
                        sDefaultRetval = "true, if left object is greater or equal than right";
                        break;

                    case "operator->":
                        sDefaultRetval = "this object pointer";
                        break;

                    case "operator*":
                        if (functionElement.Parameters.Count == 0)
                            sDefaultRetval = "this object reference";

                        break;
                    }

                    if ((functionElement.FunctionKind & vsCMFunction.vsCMFunctionOperator) != 0)
                        sDefaultBrief = functionElement.Name;
                }
            }

            CreateComment(
                editPoint,
                nElementIndent,
                Settings.FunctionIndent,
                Settings.FunctionEmptyStringTags,
                Settings.FunctionAddName ? "@fn" : null,
                functionElement.FullName,
                Settings.FunctionAddBrief ? sDefaultBrief : null,
                Settings.FunctionDetails,
                tparams.ToArray(),
                Params.ToArray(),
                sDefaultRetval,
                Settings.FunctionAddAuthor,
                Settings.FunctionAddDate,
                false,
                null);
        }

        private void CreateMacroComment(EditPoint editPoint, int nElementIndent, VCCodeMacro macroElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<Parameter> Params = new List<Parameter>();
            foreach (CodeElement param in macroElement.Parameters)
                Params.Add(new Parameter(param.FullName));

            CreateComment(
                editPoint,
                nElementIndent,
                Settings.MacroIndent,
                Settings.MacroEmptyStringTags,
                Settings.MacroAddName ? "@macro" : null,
                macroElement.FullName,
                Settings.MacroAddBrief ? "" : null,
                Settings.MacroDetails,
                null,
                Params.ToArray(),
                null,
                Settings.MacroAddAuthor,
                Settings.MacroAddDate,
                false,
                null);
        }

        private void CreateNamespaceComment(EditPoint editPoint, int nElementIndent, VCCodeNamespace namespaceElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                editPoint,
                nElementIndent,
                Settings.NamespaceIndent,
                Settings.NamespaceEmptyStringTags,
                Settings.NamespaceAddName ? "@namespace" : null,
                namespaceElement.FullName,
                Settings.NamespaceAddBrief ? "" : null,
                Settings.NamespaceDetails,
                null,
                null,
                null,
                Settings.NamespaceAddAuthor,
                Settings.NamespaceAddDate,
                false,
                null);
        }

        private void CreateUnionComment(EditPoint editPoint, int nElementIndent, VCCodeUnion unionElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                editPoint,
                nElementIndent,
                Settings.UnionIndent,
                Settings.UnionEmptyStringTags,
                Settings.UnionAddName ? "@union" : null,
                unionElement.FullName,
                Settings.UnionAddBrief ? "" : null,
                Settings.UnionDetails,
                null,
                null,
                null,
                Settings.UnionAddAuthor,
                Settings.UnionAddDate,
                false,
                null);
        }

        private void CreateTypedefComment(EditPoint editPoint, int nElementIndent, VCCodeTypedef typedefElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                editPoint,
                nElementIndent,
                Settings.TypedefIndent,
                Settings.TypedefEmptyStringTags,
                Settings.TypedefAddName ? "@typedef" : null,
                typedefElement.FullName,
                Settings.TypedefAddBrief ? "" : null,
                Settings.TypedefDetails,
                null,
                null,
                null,
                Settings.TypedefAddAuthor,
                Settings.TypedefAddDate,
                false,
                null);
        }

        private void CreateEnumComment(EditPoint editPoint, int nElementIndent, VCCodeEnum enumElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                editPoint,
                nElementIndent,
                Settings.EnumIndent,
                Settings.EnumEmptyStringTags,
                Settings.EnumAddName ? "@enum" : null,
                enumElement.FullName,
                Settings.EnumAddBrief ? "" : null,
                Settings.EnumDetails,
                null,
                null,
                null,
                Settings.EnumAddAuthor,
                Settings.EnumAddDate,
                false,
                null);
        }
    }
}
