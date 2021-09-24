
using System;

namespace DoxygenComments.Styles
{
    class QtStyle : JavadocStyle
    {
        public QtStyle (SettingsPage settings) 
            : base (settings)
        {
        }

        public override string CreateCommentBeginning(int nEditPointIndent)
        {
            return new string(' ', nEditPointIndent) 
                   + "/*!"
                   + Environment.NewLine;
        }
    }
}
