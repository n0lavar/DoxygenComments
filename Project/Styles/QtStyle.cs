
using System;

namespace DoxygenComments.Styles
{
    class QtStyle : JavadocStyle
    {
        public QtStyle (SettingsPage settings) 
            : base (settings)
        {
        }

        public override string CreateCommentBeginning(
            int     nEditPointIndent,
            bool    bUseBannerStyle)
        {
            if (bUseBannerStyle)
            {
                return base.CreateCommentBeginning(nEditPointIndent, bUseBannerStyle);
            }
            else
            {
                return new string(' ', nEditPointIndent) 
                    + "/*!"
                    + Environment.NewLine;
            }
        }
    }
}
