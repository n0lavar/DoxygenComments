using EnvDTE;
using System;

namespace DoxygenComments.Styles
{
    abstract class ACommentStyle : ICommentStyle
    {
        public abstract string CreateCommentBeginning(
            int     nEditPointIndent,
            bool    bUseBannerStyle);

        public abstract string CreateCommentMiddle(
            int     nEditPointIndent,
            int     nTagsIndent, 
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null);

        public abstract string CreateEmptyString(
            int     nEditPointIndent);

        public abstract string CreateCommentEnding(
            int     nEditPointIndent,
            bool    bUseBannerStyle);

        protected ACommentStyle(SettingsPage settings)
        {
            Settings = settings;
        }

        protected string CreateCommentBeginningBody(
            int     nEditPointIndent,
            bool    bUseBannerStyle,
            string  sBeginning,
            char    chFiling)
        {
            string sIndent = new string(' ', nEditPointIndent);
            string sFill = sBeginning;
            if (bUseBannerStyle)
                sFill += new string(chFiling, Settings.ColumnLimit - nEditPointIndent - sFill.Length);

            return sIndent
                + sFill;
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

        protected string CreateCommentEndingBody(
            int     nEditPointIndent,
            bool    bUseBannerStyle,
            string  sEnding,
            char    chFiling)
        {
            string sIndent = new string(' ', nEditPointIndent);
            string sFill = "";
            if (bUseBannerStyle)
                sFill += new string(chFiling, Settings.ColumnLimit - nEditPointIndent - sEnding.Length);

            sFill += sEnding;

            return sIndent 
                + sFill;
        }


        protected SettingsPage Settings { get; set; }
    }
};
