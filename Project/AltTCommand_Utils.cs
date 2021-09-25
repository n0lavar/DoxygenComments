using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using EnvDTE;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DoxygenComments.Styles;

namespace DoxygenComments
{
    internal sealed partial class AltTCommand
    {
        private struct Param
        {
            public string Name;
            public string Value;
        }

        private CodeElement FindNextLineCodeElement(CodeElements elements, TextPoint textPoint, int nWhiteSpaces)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // try to get symbol right under the edit point ( O(1) )
            EditPoint searchPoint = textPoint.CreateEditPoint();
            searchPoint.LineDown();
            searchPoint.CharRight(nWhiteSpaces);

            CodeElement elem = searchPoint.CodeElement[vsCMElement.vsCMElementClass];
            if (elem != null 
                && Array.IndexOf(m_FastSearchElements, elem.Kind) != -1
                && Array.IndexOf(m_IgnoredElements, elem.Kind) == -1)
            {
                try
                {
                    TextPoint start  = elem.GetStartPoint(vsCMPart.vsCMPartHeader);
                    TextPoint finish = elem.GetEndPoint(vsCMPart.vsCMPartHeader);

                    try
                    {
                        if (textPoint.LessThan(start) || textPoint.GreaterThan(finish))
                            return elem; // not inside the body
                    }
                    catch (Exception)
                    {
                        return elem; // other document
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            // some elements can't be found by searchPoint (for instance, templated method inside the class)
            // iterate over all the document elements ( O(N) )
            return FindNextLineCodeElementRecursive(elements, textPoint);
        }

        private CodeElement FindNextLineCodeElementRecursive(CodeElements elements, TextPoint textPoint)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string sTextPointDocName = textPoint.Parent.Parent.FullName;

            CodeElement CheckElement(TextPoint startPoint, CodeElement codeElement)
            {
                CodeElement ret = null;
                if (sTextPointDocName == startPoint.Parent.Parent.FullName)
                    if (startPoint.Line == textPoint.Line + 1)
                        ret = codeElement;

                return ret;
            }

            foreach (CodeElement codeElement in elements)
            {
                if (Array.IndexOf(m_IgnoredElements, codeElement.Kind) != -1)
                    continue;

                TextPoint startPoint;
                try
                {
                    startPoint = codeElement.StartPoint;
                }
                catch (Exception)
                {
                    startPoint = null;
                }

                if (startPoint != null)
                {
                    var ret = CheckElement(startPoint, codeElement);
                    if (ret != null)
                        return ret;

                    if (codeElement.Kind == vsCMElement.vsCMElementFunction)
                    {
                        VCCodeFunction codeFunction = codeElement as VCCodeFunction;

                        ret = CheckElement(
                            codeFunction.StartPointOf[vsCMPart.vsCMPartHeader, vsCMWhere.vsCMWhereDeclaration],
                            codeElement);

                        if (ret != null)
                            return ret;
                    }
                }

                // don't care about function params
                if (codeElement.Kind != vsCMElement.vsCMElementFunction)
                {
                    CodeElements children = codeElement.Children;
                    if (children != null && children.Count != 0)
                    {
                        var child = FindNextLineCodeElementRecursive(children, textPoint);
                        if (child != null)
                            return child;
                    }
                }
            }

            return null;
        }

        private static bool TryRemoveWithRoot(ref string path, string rootFolder)
        {
            int nPos = path.LastIndexOf(rootFolder, StringComparison.Ordinal);

            if (nPos != -1)
            {
                path = path.Remove(0, nPos + rootFolder.Length + 1);
                return true;
            }

            return false;
        }

        private static List<string> SplitElementName(string sName)
        {
            List<string> ret = new List<string>();

            string[] underscoreSplit = sName.Split('_');
            foreach (string s in underscoreSplit)
            {
                string[] camelCaseSplit = Regex.Replace(
                    s, 
                    "(?<=[a-z])([A-Z])", 
                    " $1", 
                    RegexOptions.Compiled).Trim().Split(' ');

                foreach (string s1 in camelCaseSplit)
                    if (!string.IsNullOrEmpty(s1))
                        ret.Add(s1);
            }

            return ret;
        }

        private static bool IsWithCapital(string sName)
        {
            return !string.IsNullOrEmpty(sName)
                && Char.IsUpper(sName[0])
                && sName.Substring(1, sName.Length - 1).All(Char.IsLower);
        }

        private List<Param> FillParams(CodeElements parameters, string[] dictionary)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            List<Param> tparams = new List<Param>();
            foreach (CodeElement tparam in parameters)
            {
                string sName = tparam.FullName.Replace("...", "").Replace("__VA_ARGS__", "...");
                string sValue = FindStringDictionaryValue(dictionary, sName);
                tparams.Add(new Param() { Name = sName, Value = sValue });
            }

            return tparams;
        }

        private static string FindStringDictionaryValue(string[] dictionary, string sKeyToFind)
        {
            if (string.IsNullOrEmpty(sKeyToFind))
                return "";

            foreach (string sKeyValue in dictionary)
            {
                int nKeyEnd = sKeyValue.IndexOf(" ", StringComparison.Ordinal);
                string sKey = sKeyValue.Substring(0, nKeyEnd);

                if (sKeyToFind == sKey)
                {
                    int nValueStart = nKeyEnd;
                    while (Char.IsWhiteSpace(sKeyValue[nValueStart]))
                        ++nValueStart;

                    return sKeyValue.Substring(nValueStart, sKeyValue.Length - nValueStart);
                }
            }

            return "";
        }

        private void CreateComment(
            ICommentStyle   commentStyle,
            EditPoint       editPoint,
            int             nElementIndent, 
            int             nIndent, 
            bool            bAddBlankLines, 
            bool            bUseBannerStyle,
            string          sCommentType,
            string          sCommentTypeValue,
            string          sDefaultBrief,
            string          sDetails,
            List<Param>     templateParameters,
            List<Param>     parameters,
            string          sRetvalValue,
            bool            bAddAuthor,
            bool            bAddDate,
            bool            bAddCopyright,
            string[]        additionalTextAfterComment)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            const string sBriefTag      = "brief";
            const string sDetailsTag    = "details";
            const string sTParamTag     = "tparam";
            const string sParamTag      = "param";
            const string sRetvalTag     = "retval";
            const string sAuthorTag     = "author";
            const string sDateTag       = "date";
            const string sCopyrightTag  = "copyright";

            int nMaxTagLength = sBriefTag.Length;

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                sCommentType == null || sCommentTypeValue == null ? 0 : sCommentType.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                string.IsNullOrEmpty(sDetails) ? 0 : sDetailsTag.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                templateParameters == null || templateParameters.Count == 0 ? 0 : sTParamTag.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                parameters == null || parameters.Count == 0 ? 0 : sParamTag.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                sRetvalValue == null ? 0 : sRetvalTag.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                !bAddAuthor ? 0 : sAuthorTag.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                !bAddDate ? 0 : sDateTag.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                !bAddCopyright ? 0 : sCopyrightTag.Length);


            int nMaxParamLength = 0;
            if (templateParameters != null && templateParameters.Count != 0)
                foreach (Param sTParam in templateParameters)
                    nMaxParamLength = Math.Max(nMaxParamLength, sTParam.Name.Length + 1); // extra space

            if (parameters != null && parameters.Count != 0)
                foreach (Param sParam in parameters)
                    nMaxParamLength = Math.Max(nMaxParamLength, sParam.Name.Length + 1); // extra space

            StringBuilder sComment = new StringBuilder(256);

            if (sCommentType != null && sCommentTypeValue != null)
            {
                sComment.Append(commentStyle.CreateCommentMiddle(
                    nElementIndent,
                    nIndent, 
                    nMaxTagLength, 
                    sCommentType, 
                    sCommentTypeValue));
            }

            if (sDefaultBrief != null)
            {
                sComment.Append(commentStyle.CreateCommentMiddle(
                    nElementIndent,
                    nIndent, 
                    nMaxTagLength, 
                    sBriefTag, 
                    sDefaultBrief.Length != 0 
                        ? sDefaultBrief 
                        : FindStringDictionaryValue(Settings.BriefDictionary, sCommentTypeValue)));
            }

            if (!string.IsNullOrEmpty(sDetails))
            {
                sComment.Append(commentStyle.CreateCommentMiddle(
                    nElementIndent,
                    nIndent, 
                    nMaxTagLength, 
                    sDetailsTag, 
                    sDetails));
            }

            if (templateParameters != null && templateParameters.Count != 0)
            {
                foreach (Param sTParam in templateParameters)
                {
                    sComment.Append(commentStyle.CreateCommentMiddle(
                        nElementIndent,
                        nIndent, 
                        nMaxTagLength, 
                        sTParamTag, 
                        sTParam.Name, 
                        nMaxParamLength,
                        sTParam.Value));
                }
            }
            
            if (parameters != null && parameters.Count != 0)
            {
                foreach (Param sParam in parameters)
                {
                    sComment.Append(commentStyle.CreateCommentMiddle(
                        nElementIndent,
                        nIndent, 
                        nMaxTagLength, 
                        sParamTag, 
                        sParam.Name,
                        nMaxParamLength,
                        sParam.Value));
                }
            }

            if (sRetvalValue != null)
            {
                sComment.Append(commentStyle.CreateCommentMiddle(
                    nElementIndent,
                    nIndent, 
                    nMaxTagLength, 
                    sRetvalTag, 
                    "",
                    nMaxParamLength,
                    sRetvalValue.Length != 0
                        ? sRetvalValue
                        : FindStringDictionaryValue(Settings.RetvalDictionary, sCommentTypeValue)));
            }

            if (bAddAuthor)
            {
                sComment.Append(commentStyle.CreateCommentMiddle(
                    nElementIndent,
                    nIndent, 
                    nMaxTagLength, 
                    sAuthorTag, 
                    Settings.Author));
            }

            if (bAddDate)
            {
                int nYear    = DateTime.Now.Year;
                int nMonth   = DateTime.Now.Month;
                int nDay     = DateTime.Now.Day;
                string sDate = nDay + "." + (nMonth < 10 ? "0" : "") + nMonth + "." + nYear;

                sComment.Append(commentStyle.CreateCommentMiddle(
                    nElementIndent,
                    nIndent, 
                    nMaxTagLength, 
                    sDateTag, 
                    sDate));
            }

            if (bAddCopyright && Settings.Copyright != null && Settings.Copyright.Length != 0)
            {
                sComment.Append(commentStyle.CreateCommentMiddle(
                    nElementIndent,
                    nIndent, 
                    nMaxTagLength, 
                    sCopyrightTag, 
                    Settings.Copyright[0].Replace("{year}", DateTime.Now.Year.ToString())));

                for (int i = 1; i < Settings.Copyright.Length; ++i)
                {
                    sComment.Append(commentStyle.CreateCommentMiddle(
                        nElementIndent,
                        nIndent, 
                        nMaxTagLength, 
                        "", 
                        Settings.Copyright[i].Replace("{year}", DateTime.Now.Year.ToString())));
                }
            }

            if (sComment.Length != 0 && !sComment.ToString().All(Char.IsWhiteSpace))
            {
                string sBegin = commentStyle.CreateCommentBeginning(nElementIndent, bUseBannerStyle);

                if (bAddBlankLines)
                    sBegin += commentStyle.CreateEmptyString(nElementIndent);

                sComment = sComment.Insert(0, sBegin);

                if (bAddBlankLines)
                    sComment.Append(commentStyle.CreateEmptyString(nElementIndent));

                string sEnding = commentStyle.CreateCommentEnding(nElementIndent, bUseBannerStyle);
                if (!string.IsNullOrEmpty(sEnding))
                {
                    sComment.Append(sEnding);
                }
                else if (sComment.Length > Environment.NewLine.Length)
                {
                    sComment.Remove(
                        sComment.Length - Environment.NewLine.Length, 
                        Environment.NewLine.Length);
                }
            }

            if (additionalTextAfterComment != null && additionalTextAfterComment.Length > 0)
            {
                sComment.Append(Environment.NewLine);
                foreach (string str in additionalTextAfterComment)
                    sComment.Append(str + Environment.NewLine);
            }

            if (sComment.Length != 0)
            {
                editPoint.StartOfLine();
                editPoint.Delete(editPoint.LineLength);
                editPoint.Insert(sComment.ToString());
            }
        }

        void CreateSimpleComment(EditPoint editPoint, int nElementIndent = -1)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string sComment;
            if (nElementIndent != -1)
            {
                editPoint.StartOfLine();
                editPoint.Delete(editPoint.LineLength);
                sComment = new string(' ', nElementIndent);
            }
            else
            {
                sComment = " ";
            }

            sComment += "//!< ";
            editPoint.Insert(sComment);
        }
    }
}
