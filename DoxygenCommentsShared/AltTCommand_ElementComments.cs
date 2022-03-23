using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DoxygenComments.Styles;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using EnvDTE;
using Microsoft.VisualStudio.TextManager.Interop;

namespace DoxygenComments
{
    internal sealed partial class AltTCommand
    {
        private void CreateLineComment(
            EditPoint editPoint, 
            int       nStart,
            string    sText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            sText = sText.Trim();

            int nFillerTotal = Settings.ColumnLimit - 5 - sText.Length - nStart;
            int nFillerRight = nFillerTotal / 2;
            int nFillerLeft  = nFillerTotal - nFillerRight;

            string sCommentText = new string(' ', nStart) + "// ";

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
            IVsTextView     textView,
            TextDocument    textDocument)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                textView,
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
            IVsTextView     textView,
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
                textView,
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
            IVsTextView     textView,
            TextDocument    textDocument)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                textView,
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
            IVsTextView     textView,
            int             nElementIndent, 
            VCCodeClass     classElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<Param> tparams = new List<Param>();
            if (Settings.ClassAddTparam)
                tparams = FillParams(classElement.TemplateParameters, Settings.TParamDictionary);

            CreateComment(
                commentStyle,
                editPoint,
                textView,
                nElementIndent,
                Settings.ClassIndent,
                Settings.ClassAddBlankLines,
                Settings.ClassUseBannerStyle,
                Settings.ClassAddName ? "class" : null,
                classElement.Name,
                Settings.ClassAddBrief ? "" : null,
                Settings.ClassDetails,
                tparams,
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
            IVsTextView     textView,
            int             nElementIndent, 
            VCCodeStruct    structElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<Param> tparams = new List<Param>();
            if (Settings.StructAddTparam)
                tparams = FillParams(structElement.TemplateParameters, Settings.TParamDictionary);

            CreateComment(
                commentStyle,
                editPoint,
                textView,
                nElementIndent,
                Settings.StructIndent,
                Settings.StructAddBlankLines,
                Settings.StructUseBannerStyle,
                Settings.StructAddName ? "struct" : null,
                structElement.Name,
                Settings.StructAddBrief ? "" : null,
                Settings.StructDetails,
                tparams,
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
            IVsTextView     textView,
            int             nElementIndent, 
            VCCodeFunction  functionElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<Param> tparams = new List<Param>();
            if (Settings.FunctionAddTparam)
                tparams = FillParams(functionElement.TemplateParameters, Settings.TParamDictionary);

            List<Param> Params = new List<Param>();
            if (Settings.FunctionAddParam)
                Params = FillParams(functionElement.Parameters, Settings.ParamDictionary);

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

                List<string> tokens = SplitElementName(functionElement.Name);

                if (!bVoidRetval)
                {
                    sDefaultRetval = "";

                    // check if this function is getter
                    if (Settings.FunctionGenerateGetter
                        && tokens.Count > 1 
                        && tokens[0].Equals("get", StringComparison.InvariantCultureIgnoreCase))
                    {
                        for (int i = 1; i < tokens.Count; ++i)
                        {
                            sDefaultRetval += IsWithCapital(tokens[i])
                                ? tokens[i].ToLower()
                                : tokens[i];

                            if (i != tokens.Count - 1)
                                sDefaultRetval += ' ';
                        }

                        sDefaultBrief = "Get " + sDefaultRetval;
                    }
                }
                else
                {
                    // check if this function is setter
                    if (Settings.FunctionGenerateSetter
                        && tokens.Count > 1 
                        && tokens[0].Equals("set", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string sParamText = "";
                        for (int i = 1; i < tokens.Count; i++)
                        {
                            sParamText += IsWithCapital(tokens[i])
                                ? tokens[i].ToLower()
                                : tokens[i];

                            if (i != tokens.Count - 1)
                                sParamText += ' ';
                        }

                        sDefaultBrief = "Set " + sParamText;

                        if (Params.Count == 1 
                            && Params[0].Value != null 
                            && Params[0].Value.Length == 0)
                        {
                            string sName = Params[0].Name;
                            Params[0] = new Param() { Name = sName, Value = sParamText };
                        }
                    }
                }
            }

            CreateComment(
                commentStyle,
                editPoint,
                textView,
                nElementIndent,
                Settings.FunctionIndent,
                Settings.FunctionAddBlankLines,
                Settings.FunctionUseBannerStyle,
                Settings.FunctionAddName ? "fn" : null,
                functionElement.Name,
                Settings.FunctionAddBrief ? sDefaultBrief : null,
                Settings.FunctionDetails,
                tparams,
                Params,
                Settings.FunctionAddRetval ? sDefaultRetval : null,
                Settings.FunctionAddAuthor,
                Settings.FunctionAddDate,
                false,
                null);
        }

        private void CreateMacroComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint,
            IVsTextView     textView,
            int             nElementIndent, 
            VCCodeMacro     macroElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<Param> Params = new List<Param>();
            if (Settings.MacroAddParam)
                Params = FillParams(macroElement.Parameters, Settings.ParamDictionary);

            CreateComment(
                commentStyle,
                editPoint,
                textView,
                nElementIndent,
                Settings.MacroIndent,
                Settings.MacroAddBlankLines,
                Settings.MacroUseBannerStyle,
                Settings.MacroAddName ? "def" : null,
                macroElement.FullName,
                Settings.MacroAddBrief ? "" : null,
                Settings.MacroDetails,
                null,
                Params,
                null,
                Settings.MacroAddAuthor,
                Settings.MacroAddDate,
                false,
                null);
        }

        private void CreateNamespaceComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint,
            IVsTextView     textView,
            int             nElementIndent, 
            VCCodeNamespace namespaceElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                textView,
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
            IVsTextView     textView,
            int             nElementIndent, 
            VCCodeUnion     unionElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                textView,
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
            IVsTextView     textView,
            int             nElementIndent, 
            VCCodeTypedef   typedefElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                textView,
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
            IVsTextView     textView,
            int             nElementIndent, 
            VCCodeEnum      enumElement)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            CreateComment(
                commentStyle,
                editPoint,
                textView,
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
