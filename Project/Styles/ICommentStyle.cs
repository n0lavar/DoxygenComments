using EnvDTE;

namespace DoxygenComments.Styles
{
    interface ICommentStyle
    {
        string CreateCommentBeginning(
            int nEditPointIndent);

        string CreateCommentMiddle(
            int     nEditPointIndent,
            int     nTagsIndent, 
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null);

        string CreateEmptyString(
            int nEditPointIndent);

        string CreateCommentEnding(
            int nEditPointIndent);
    }
}
