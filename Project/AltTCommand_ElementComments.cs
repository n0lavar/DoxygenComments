using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DoxygenComments.Styles;
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
                sCommentText += new string('-', nFillerLeft);
                sCommentText += sText.Length == 0 ? '-' : ' ';
            }

            sCommentText += sText;

            if (nFillerRight >= 0)
            {
                sCommentText += sText.Length == 0 ? '-' : ' ';
                sCommentText += new string('-', nFillerRight);
            }

            editPoint.StartOfLine();
            editPoint.Delete(editPoint.LineLength);
            editPoint.Insert(sCommentText);
        }

        private void CreateHeaderFileHeader(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            TextDocument    textDocument)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                0,
                Settings.HeaderFilesHeaderIndent,
                Settings.HeaderFilesHeaderAddBlankLines,
                Settings.HeaderFilesHeaderUseBannerStyle,
                Settings.HeaderFilesHeaderAddName ? "file" : null,
                textDocument.Parent.Name,
                Settings.HeaderFilesHeaderAddBrief ? "" : null,
                Settings.HeaderFilesHeaderDetails,
                null,
                null,
                null,
                Settings.HeaderFilesHeaderAddAuthor,
                Settings.HeaderFilesHeaderAddDate,
                Settings.HeaderFilesHeaderAddCopyright,
                Settings.HeaderFilesHeaderAdditionalText);
        }

        private void CreateSourceFileHeader(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            TextDocument    textDocument)
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

            string[] additionalText = new string[Settings.SourceFilesHeaderAdditionalText.Length + 1];
            additionalText[0] = sAdditionalText;
            Settings.SourceFilesHeaderAdditionalText.CopyTo(additionalText, 1);

            CreateComment(
                commentStyle,
                editPoint,
                0,
                Settings.SourceFilesHeaderIndent,
                Settings.SourceFilesHeaderAddBlankLines,
                Settings.SourceFilesHeaderUseBannerStyle,
                Settings.SourceFilesHeaderAddName ? "file" : null,
                textDocument.Parent.Name,
                Settings.SourceFilesHeaderAddBrief ? "" : null,
                Settings.SourceFilesHeaderDetails,
                null,
                null,
                null,
                Settings.SourceFilesHeaderAddAuthor,
                Settings.SourceFilesHeaderAddDate,
                Settings.SourceFilesHeaderAddCopyright,
                additionalText);
        }

        private void CreateInlineFileHeader(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            TextDocument    textDocument)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                0,
                Settings.InlineFilesHeaderIndent,
                Settings.InlineFilesHeaderAddBlankLines,
                Settings.InlineFilesHeaderUseBannerStyle,
                Settings.InlineFilesHeaderAddName ? "file" : null,
                textDocument.Parent.Name,
                Settings.InlineFilesHeaderAddBrief ? "" : null,
                Settings.InlineFilesHeaderDetails,
                null,
                null,
                null,
                Settings.InlineFilesHeaderAddAuthor,
                Settings.InlineFilesHeaderAddDate,
                Settings.InlineFilesHeaderAddCopyright,
                Settings.InlineFilesHeaderAdditionalText);
        }

        private void CreateClassComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            int             nElementIndent, 
            VCCodeClass     classElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<string> tparams = new List<string>();
            foreach (CodeElement tparam in classElement.TemplateParameters)
                tparams.Add(tparam.FullName);

            CreateComment(
                commentStyle,
                editPoint,
                nElementIndent,
                Settings.ClassIndent,
                Settings.ClassAddBlankLines,
                Settings.ClassUseBannerStyle,
                Settings.ClassAddName ? "class" : null,
                classElement.Name,
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

        private void CreateStructComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            int             nElementIndent, 
            VCCodeStruct    structElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<string> tparams = new List<string>();
            foreach (CodeElement tparam in structElement.TemplateParameters)
                tparams.Add(tparam.FullName);

            CreateComment(
                commentStyle,
                editPoint,
                nElementIndent,
                Settings.StructIndent,
                Settings.StructAddBlankLines,
                Settings.StructUseBannerStyle,
                Settings.StructAddName ? "struct" : null,
                structElement.Name,
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

        private void CreateFunctionComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            int             nElementIndent, 
            VCCodeFunction  functionElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<string> tparams = new List<string>();
            foreach (CodeElement tparam in functionElement.TemplateParameters)
                tparams.Add(tparam.FullName.Replace("...", ""));

            List<string> Params = new List<string>();
            foreach (CodeElement param in functionElement.Parameters)
                Params.Add(param.FullName.Replace("...", ""));

            string sDefaultBrief  = "";
            string sDefaultRetval = null;
            vsCMFunction functionKind;
            try
            {
                functionKind = functionElement.FunctionKind;
            }
            catch (Exception)
            {
                functionKind = vsCMFunction.vsCMFunctionOther;
            }


            if ((functionKind & vsCMFunction.vsCMFunctionConstructor) > 0)
            {
                sDefaultBrief = functionElement.Name + " object constructor";
            }
            else if ((functionKind & vsCMFunction.vsCMFunctionDestructor) > 0)
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
                    sDefaultRetval =  "";
            }

            CreateComment(
                commentStyle,
                editPoint,
                nElementIndent,
                Settings.FunctionIndent,
                Settings.FunctionAddBlankLines,
                Settings.FunctionUseBannerStyle,
                Settings.FunctionAddName ? "fn" : null,
                functionElement.Name,
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

        private void CreateMacroComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            int             nElementIndent, 
            VCCodeMacro     macroElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<string> Params = new List<string>();
            foreach (CodeElement param in macroElement.Parameters)
                Params.Add(param.FullName.Replace("__VA_ARGS__", "..."));

            CreateComment(
                commentStyle,
                editPoint,
                nElementIndent,
                Settings.MacroIndent,
                Settings.MacroAddBlankLines,
                Settings.MacroUseBannerStyle,
                Settings.MacroAddName ? "def" : null,
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

        private void CreateNamespaceComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            int             nElementIndent, 
            VCCodeNamespace namespaceElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                nElementIndent,
                Settings.NamespaceIndent,
                Settings.NamespaceAddBlankLines,
                Settings.NamespaceUseBannerStyle,
                Settings.NamespaceAddName ? "namespace" : null,
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

        private void CreateUnionComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            int             nElementIndent, 
            VCCodeUnion     unionElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                nElementIndent,
                Settings.UnionIndent,
                Settings.UnionAddBlankLines,
                Settings.UnionUseBannerStyle,
                Settings.UnionAddName ? "union" : null,
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

        private void CreateTypedefComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            int             nElementIndent, 
            VCCodeTypedef   typedefElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                nElementIndent,
                Settings.TypedefIndent,
                Settings.TypedefAddBlankLines,
                Settings.TypedefUseBannerStyle,
                Settings.TypedefAddName ? "typedef" : null,
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

        private void CreateEnumComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint, 
            int             nElementIndent, 
            VCCodeEnum      enumElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                nElementIndent,
                Settings.EnumIndent,
                Settings.EnumAddBlankLines,
                Settings.EnumUseBannerStyle,
                Settings.EnumAddName ? "enum" : null,
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
