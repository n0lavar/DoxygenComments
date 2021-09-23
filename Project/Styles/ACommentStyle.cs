using EnvDTE;
using System;

namespace DoxygenComments.Styles
{
    abstract class ACommentStyle : ICommentStyle
    {
        protected ACommentStyle(SettingsPage settings)
        {
            Settings = settings;
        }

        public abstract string CreateCommentBeginning(
            int nEditPointIndent);

        public abstract string CreateCommentMiddle(
            int     nTagsIndent, 
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null);

        public abstract string CreateCommentEnding(
            int nEditPointIndent);

        public abstract string CreateEmptyString();

        protected string CreateCommentMiddleBody(
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null)
        {
            char chIndentChar = Settings.GetIndentChar();
            sTag = Settings.TagChar + sTag;
            nMaxTagLength = nMaxTagLength + 1;  // extra TagChar

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

            return sTag 
                + sTextIndent 
                + sTagText
                + sParamIndent 
                + (sParamText != null ? sParamText : "")
                + Environment.NewLine;
        }

        protected SettingsPage Settings { get; set; }
    }
};
