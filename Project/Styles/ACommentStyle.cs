using EnvDTE;
using System;

namespace DoxygenComments.Styles
{
    abstract class ACommentStyle : ICommentStyle
    {
        public abstract string CreateCommentBeginning(
            int nEditPointIndent);

        public abstract string CreateCommentMiddle(
            int     nEditPointIndent,
            int     nTagsIndent, 
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null);

        public abstract string CreateEmptyString();

        public abstract string CreateCommentEnding(
            int nEditPointIndent);

        protected ACommentStyle(SettingsPage settings)
        {
            Settings = settings;
        }

        protected string CreateCommentMiddleBody(
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null)
        {
            if (sTag.Length != 0)
                sTag = Settings.TagChar + sTag;

            nMaxTagLength = nMaxTagLength + 1;  // extra TagChar

            string sTextIndent = new string(' ', nMaxTagLength - sTag.Length + 1);
            string sParamIndent;

            if (nParamsIndent != -1)
            {
                sParamIndent = new string(' ', nParamsIndent - sTagText.Length);
                if (Settings.AddРyphen)
                    sParamIndent += "- ";
            }
            else
            {
                sParamIndent = "";
            }

            return sTag 
                + sTextIndent 
                + sTagText
                + sParamIndent 
                + (sParamText != null ? sParamText : "");
        }

        protected SettingsPage Settings { get; set; }
    }
};
