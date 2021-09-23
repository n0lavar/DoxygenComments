using EnvDTE;

namespace DoxygenComments.Styles
{
    interface ICommentStyle
    {
        string CreateCommentBeginning(
            int nEditPointIndent);

        string CreateCommentMiddle(
            int     nTagsIndent, 
            int     nMaxTagLength, 
            string  sTag, 
            string  sTagText,
            int     nParamsIndent = -1,
            string  sParamText = null);

        string CreateCommentEnding(
            int nEditPointIndent);

        string CreateEmptyString();
    }
}
