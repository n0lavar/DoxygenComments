using System;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCCodeModel;
using EnvDTE;
using System.Linq;
using System.Text;

namespace DoxygenComments
{
    internal sealed partial class AltTCommand
    {
        private CodeElement FindNextLineCodeElement(CodeElements elements, TextPoint textPoint, int nWhiteSpaces)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // try to get symbol right under the edit point ( O(1) )
            EditPoint searchPoint = textPoint.CreateEditPoint();
            searchPoint.LineDown();
            searchPoint.CharRight(nWhiteSpaces);

            CodeElement elem = searchPoint.get_CodeElement(vsCMElement.vsCMElementClass);
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

        private bool TryRemoveWithRoot(ref string path, string rootFolder)
        {
            int nPos = path.LastIndexOf(rootFolder);

            if (nPos != -1)
            {
                path = path.Remove(0, nPos + rootFolder.Length + 1);
                return true;
            }

            return false;
        }

        private string FindStringDictionaryValue(string[] dictionary, string sKeyToFind)
        {
            if (string.IsNullOrEmpty(sKeyToFind))
                return "";

            foreach (string sKeyValue in dictionary)
            {
                int nKeyEnd = sKeyValue.IndexOf(" ");
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
            EditPoint   editPoint,
            int         nElementIndent, 
            int         nIndent, 
            bool        bAddBlankLines, 
            string      sCommentType,
            string      sCommentTypeValue,
            string      sDefaultBrief,
            string      sDetails,
            string[]    templateParameters,
            string[]    parameters,
            string      sRetvalValue,
            bool        bAddAuthor,
            bool        bAddDate,
            bool        bAddCopyright,
            string[]    additionalTextAfterComment)
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

            int nTagsIndent = nElementIndent + nIndent;

            int nMaxTagLength = sBriefTag.Length;

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                sCommentType == null || sCommentTypeValue == null ? 0 : sCommentType.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                sDetails == null || sDetails.Length == 0 ? 0 : sDetailsTag.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                templateParameters == null || templateParameters.Length == 0 ? 0 : sTParamTag.Length);

            nMaxTagLength = Math.Max(
                nMaxTagLength, 
                parameters == null || parameters.Length == 0 ? 0 : sParamTag.Length);

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
            if (templateParameters != null && templateParameters.Length != 0)
                foreach (string sTParam in templateParameters)
                    nMaxParamLength = Math.Max(nMaxParamLength, sTParam.Length + 1); // extra space

            if (parameters != null && parameters.Length != 0)
                foreach (string sParam in parameters)
                    nMaxParamLength = Math.Max(nMaxParamLength, sParam.Length + 1); // extra space

            StringBuilder sComment = new StringBuilder(256);

            if (sCommentType != null && sCommentTypeValue != null)
            {
                sComment.Append(CreateCommentMiddle(
                    nTagsIndent, 
                    nMaxTagLength, 
                    sCommentType, 
                    sCommentTypeValue));
            }

            if (sDefaultBrief != null)
            {
                sComment.Append(CreateCommentMiddle(
                    nTagsIndent, 
                    nMaxTagLength, 
                    sBriefTag, 
                    sDefaultBrief.Length != 0 
                        ? sDefaultBrief 
                        : FindStringDictionaryValue(Settings.BriefDictionary, sCommentTypeValue)));
            }

            if (sDetails != null && sDetails.Length != 0)
            {
                sComment.Append(CreateCommentMiddle(
                    nTagsIndent, 
                    nMaxTagLength, 
                    sDetailsTag, 
                    sDetails));
            }

            if (templateParameters != null && templateParameters.Length != 0)
            {
                foreach (string sTParam in templateParameters)
                {
                    sComment.Append(CreateCommentMiddle(
                        nTagsIndent, 
                        nMaxTagLength, 
                        sTParamTag, 
                        sTParam, 
                        nMaxParamLength,
                        FindStringDictionaryValue(Settings.TParamDictionary, sTParam)));
                }
            }
            
            if (parameters != null && parameters.Length != 0)
            {
                foreach (string sParam in parameters)
                {
                    sComment.Append(CreateCommentMiddle(
                        nTagsIndent, 
                        nMaxTagLength, 
                        sParamTag, 
                        sParam, 
                        nMaxParamLength,
                        FindStringDictionaryValue(Settings.ParamDictionary, sParam)));
                }
            }

            if (sRetvalValue != null)
            {
                sComment.Append(CreateCommentMiddle(
                    nTagsIndent, 
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
                sComment.Append(CreateCommentMiddle(
                    nTagsIndent, 
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

                sComment.Append(CreateCommentMiddle(
                    nTagsIndent, 
                    nMaxTagLength, 
                    sDateTag, 
                    sDate));
            }

            if (bAddCopyright && Settings.Copyright != null && Settings.Copyright.Length != 0)
            {
                sComment.Append(CreateCommentMiddle(
                    nTagsIndent, 
                    nMaxTagLength, 
                    sCopyrightTag, 
                    Settings.Copyright[0].Replace("{year}", DateTime.Now.Year.ToString())));

                for (int i = 1; i < Settings.Copyright.Length; ++i)
                {
                    sComment.Append(CreateCommentMiddle(
                        nTagsIndent, 
                        nMaxTagLength, 
                        "", 
                        Settings.Copyright[i].Replace("{year}", DateTime.Now.Year.ToString())));
                }
            }

            if (sComment.Length != 0 && !sComment.ToString().All(Char.IsWhiteSpace))
            {
                string sBegin = CreateCommentBeginning(nElementIndent);

                if (bAddBlankLines)
                    sBegin += CreateEmptyString();

                sComment = sComment.Insert(0, sBegin);

                if (bAddBlankLines)
                    sComment.Append(CreateEmptyString());

                sComment.Append(CreateCommentEnding(nElementIndent));
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

        private string CreateCommentBeginning(int nEditPointIndent)
        {
            return new string(Settings.GetIndentChar(), nEditPointIndent) + "/**" + Environment.NewLine;
        }

        private string CreateCommentMiddle(
            int     nTagsIndent, 
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null)
        {
            char chIndentChar = Settings.GetIndentChar();
            sTag = Settings.TagChar + sTag;
            nMaxTagLength = nMaxTagLength + 1;  // extra TagChar

            string sTagsIndent = new string(chIndentChar, nTagsIndent);
            string sTextIndent;
            string sParamIndent;
            if (Settings.IndentChar == SettingsPage.EIndentChar.Space)
            {
                sTextIndent = new string(chIndentChar, nMaxTagLength - sTag.Length + 1);

                if (nParamsIndent != -1)
                {
                    sParamIndent = new string(chIndentChar, nParamsIndent - sTagText.Length);
                    if (Settings.AddРyphen)
                        sParamIndent += "- ";
                }
                else
                {
                    sParamIndent = "";
                }
            }
            else
            {
                int nTabsLongestTag = nMaxTagLength / Settings.TabWidth + 1;
                int nTabsThisTag = sTag.Length / Settings.TabWidth + 1;
                sTextIndent = new string(chIndentChar, nTabsLongestTag - nTabsThisTag + 1);

                if (nParamsIndent != -1)
                {
                    sParamIndent = new string(chIndentChar, (nParamsIndent - sTagText.Length + 1) / Settings.TabWidth + 1);
                    if (Settings.AddРyphen)
                        sParamIndent += "- ";
                }
                else
                {
                    sParamIndent = "";
                }
            }

            return sTagsIndent 
                + sTag 
                + sTextIndent 
                + sTagText
                + sParamIndent 
                + (sParamText != null ? sParamText : "")
                + Environment.NewLine;
        }

        private string CreateCommentEnding(int nEditPointIndent)
        {
            return new string(Settings.GetIndentChar(), nEditPointIndent) + "**/";
        }

        private string CreateEmptyString()
        {
            return Environment.NewLine;
        }

        private void CreateSimpleComment(EditPoint editPoint, int nElementIndent = -1)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string sComment;
            if (nElementIndent != -1)
            {
                editPoint.StartOfLine();
                editPoint.Delete(editPoint.LineLength);
                sComment = new string(Settings.GetIndentChar(), nElementIndent);
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
