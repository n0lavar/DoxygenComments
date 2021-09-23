
using System;

namespace DoxygenComments.Styles
{
    class SimpleStyle : ACommentStyle
    {
        public SimpleStyle (SettingsPage settings) 
            : base (settings)
        {
        }

        public override string CreateCommentBeginning(int nEditPointIndent)
        {
            return new string(Settings.GetIndentChar(), nEditPointIndent) + "/**" + Environment.NewLine;
        }

        public override string CreateCommentMiddle(
            int     nTagsIndent, 
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null)
        {
            string sTagsIndent = new string(Settings.GetIndentChar(), nTagsIndent);
            return sTagsIndent + CreateCommentMiddleBody(
                nMaxTagLength, 
                sTag, 
                sTagText, 
                nParamsIndent, 
                sParamText);
        }

        public override string CreateCommentEnding(int nEditPointIndent)
        {
            return new string(Settings.GetIndentChar(), nEditPointIndent) + "**/";
        }

        public override string CreateEmptyString()
        {
            return Environment.NewLine;
        }
    }
}
