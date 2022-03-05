
using System;

namespace DoxygenComments.Styles
{
    class SimpleStyle : ACommentStyle
    {
        public SimpleStyle (SettingsPage settings) 
            : base (settings)
        {
        }

        public override string CreateCommentBeginning(
            int     nEditPointIndent,
            bool    bUseBannerStyle)
        {
            return CreateCommentBeginningBody(nEditPointIndent, bUseBannerStyle,  "/**", '*')
                + Environment.NewLine;
        }

        public override string CreateCommentMiddle(
            int     nEditPointIndent,
            int     nTagsIndent, 
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null)
        {
            return new string(' ', nEditPointIndent + nTagsIndent) 
                + CreateCommentMiddleBody(
                    nMaxTagLength, 
                    sTag, 
                    sTagText, 
                    nParamsIndent, 
                    sParamText)
                + Environment.NewLine;
        }

        public override string CreateEmptyString(int nEditPointIndent)
        {
            return Environment.NewLine;
        }

        public override string CreateCommentEnding(
            int     nEditPointIndent,
            bool    bUseBannerStyle)
        {
            return CreateCommentEndingBody(nEditPointIndent, bUseBannerStyle, "**/", '*');
        }
    }
}
