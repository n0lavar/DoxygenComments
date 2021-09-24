
using System;

namespace DoxygenComments.Styles
{
    class JavadocStyle : ACommentStyle
    {
        string sBlock = " *";

        public JavadocStyle (SettingsPage settings) 
            : base (settings)
        {
        }

        public override string CreateCommentBeginning(int nEditPointIndent)
        {
            return new string(' ', nEditPointIndent) 
                   + "/**"
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
                Math.Max(nTagsIndent - sBlock.Length, 0));

            return new string(' ', nEditPointIndent)
                + sBlock
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
                + sBlock 
                + Environment.NewLine;
        }

        public override string CreateCommentEnding(int nEditPointIndent)
        {
            return new string(' ', nEditPointIndent) 
                   + " */";
        }
    }
}
