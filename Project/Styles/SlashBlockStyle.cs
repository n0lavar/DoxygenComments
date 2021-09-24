
using System;

namespace DoxygenComments.Styles
{
    class SlashBlockStyle : ACommentStyle
    {
        private const string m_sBlock = "///";

        public SlashBlockStyle (SettingsPage settings) 
            : base (settings)
        {
        }

        public override string CreateCommentBeginning(
            int     nEditPointIndent,
            bool    bUseBannerStyle)
        {
            if (bUseBannerStyle)
                return CreateCommentBeginningBody(nEditPointIndent, bUseBannerStyle, m_sBlock, '/') + Environment.NewLine;
            else
                return "";
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
            string sTagsIndent = new string(
                ' ', 
                Math.Max(nTagsIndent - m_sBlock.Length, 0));

            return new string(' ', nEditPointIndent)
                + m_sBlock 
                + sTagsIndent 
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
            return new string(' ', nEditPointIndent)
                + m_sBlock 
                + Environment.NewLine;
        }

        public override string CreateCommentEnding(
            int     nEditPointIndent,
            bool    bUseBannerStyle)
        {
            if (bUseBannerStyle)
                return CreateCommentEndingBody(nEditPointIndent, bUseBannerStyle, m_sBlock, '/');
            else
                return "";
        }
    }
}
