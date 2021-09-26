
using System;
using System.Text;

namespace DoxygenComments.Styles
{
    class JavadocStyle : ACommentStyle
    {
        private const string m_sBlock    = " *";
        private const char   m_chFilling = '*';

        public JavadocStyle (SettingsPage settings) 
            : base (settings)
        {
        }

        public override string CreateCommentBeginning(
            int     nEditPointIndent,
            bool    bUseBannerStyle)
        {
            return CreateCommentBeginningBody(nEditPointIndent, bUseBannerStyle,  "/**", m_chFilling)
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
            string sRet = CreateCommentEndingBody(nEditPointIndent, bUseBannerStyle, "**/", m_chFilling);
            int nFirstFillingChar = sRet.IndexOf(m_chFilling);
            StringBuilder stringBuilder = new StringBuilder(sRet)
            {
                [nFirstFillingChar] = ' '
            };
            return stringBuilder.ToString();
        }
    }
}
