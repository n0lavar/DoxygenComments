using System;
using System.Linq;
using System.ComponentModel.Design;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.VCCodeModel;
using EnvDTE;

using Task = System.Threading.Tasks.Task;

namespace DoxygenComments
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed partial class AltTCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f78c7266-2a13-40c4-b379-b805a6f4c3af");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private DTE m_DTE;
        private IVsTextManager m_TextManager;

        private static readonly string[] m_HeaderFileExtensions = { 
            ".h", 
            ".hpp",
            ".hh", 
            ".hp", 
            ".hxx",
            ".H",
            ".HPP",
            ".HXX",
            ".h++",
            ".H++",
        };

        private static readonly string[] m_SourceFileExtensions = { 
            ".c", 
            ".cpp",
            ".cc",
            ".cp", 
            ".cxx", 
            ".C", 
            ".CPP", 
            ".CXX", 
            ".c++", 
            ".C++", 
        };

        private static readonly string[] m_InlineFileExtensions = { 
            ".inl",
            ".inc", 
            ".tli", 
            ".tlh", 
        };

        private static readonly vsCMElement[] m_FastSearchElements = {
            //vsCMElement.vsCMElementOther,
            vsCMElement.vsCMElementClass,
            vsCMElement.vsCMElementFunction,
            vsCMElement.vsCMElementVariable,
            vsCMElement.vsCMElementNamespace,
            //vsCMElement.vsCMElementParameter,
            vsCMElement.vsCMElementEnum,
            vsCMElement.vsCMElementStruct,
            vsCMElement.vsCMElementUnion,
            //vsCMElement.vsCMElementLocalDeclStmt,
            //vsCMElement.vsCMElementFunctionInvokeStmt,
            //vsCMElement.vsCMElementAssignmentStmt,
            vsCMElement.vsCMElementDefineStmt,
            vsCMElement.vsCMElementTypeDef,
            //vsCMElement.vsCMElementIncludeStmt,
            vsCMElement.vsCMElementMacro,
        };


        /// <summary>
        /// Initializes a new instance of the <see cref="AltTCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AltTCommand(AsyncPackage package, OleMenuCommandService commandService, DTE dte, IVsTextManager textManager)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.package = package 
                ?? throw new ArgumentNullException(nameof(package));

            m_DTE = dte 
                ?? throw new ArgumentNullException(nameof(dte));

            m_TextManager = textManager 
                ?? throw new ArgumentNullException(nameof(m_TextManager));

            commandService = commandService 
                ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static AltTCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return package;
            }
        }

        /// <summary>
        /// Gets settings instance
        /// </summary>
        private SettingsPage Settings
        {
            get { return (SettingsPage)package.GetDialogPage(typeof(SettingsPage)); }
        }


        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, DTE dte, IVsTextManager textManager)
        {
            // Switch to the main thread - the call to AddCommand in AltTCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AltTCommand(package, commandService, dte, textManager);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            try
            {
                CreateComment();
            }
            catch (Exception ex)
            {
                string sTitle = "DoxygenComments";
                string sMessage = "An exception was thrown :\n\n" + ex.Message + "\n\n" + ex.StackTrace;

                VsShellUtilities.ShowMessageBox(
                    package,
                    sMessage,
                    sTitle,
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private void CreateComment()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsTextView textView;
            if (m_TextManager.GetActiveView(1, null, out textView) != VSConstants.S_OK)
                throw new Exception(nameof(m_TextManager.GetActiveView));

            int nLine;
            int nColumn;
            if (textView.GetCaretPos(out nLine, out nColumn) != VSConstants.S_OK)
                throw new Exception(nameof(textView.GetCaretPos));

            IVsTextLines buffer;
            if (textView.GetBuffer(out buffer) != VSConstants.S_OK)
                throw new Exception(nameof(textView.GetBuffer));

            int nLineLen;
            if (buffer.GetLengthOfLine(nLine, out nLineLen) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.GetLengthOfLine));

            string sLine;
            if (buffer.GetLineText(nLine, 0, nLine, nLineLen, out sLine) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.GetLineText));

            object oEditPoint;
            if (buffer.CreateEditPoint(nLine, 0, out oEditPoint) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.CreateEditPoint));

            EditPoint editPoint = (EditPoint)oEditPoint;

            TextDocument textDocument = m_DTE.ActiveDocument.Object() as TextDocument
                ?? throw new ArgumentNullException(nameof(m_DTE.ActiveDocument));

            if (editPoint.AtStartOfDocument)
            {
                // create file header
                string sFileName = textDocument.Parent.Name;
                int nDotPosition = sFileName.LastIndexOf(".");
                string sFileNameExtension = sFileName.Substring(nDotPosition < 0 ? 0 : nDotPosition);

                if (Array.IndexOf(m_HeaderFileExtensions, sFileNameExtension) != -1)
                {
                    CreateHeaderFileHeader(editPoint, textDocument);
                }
                else if (Array.IndexOf(m_SourceFileExtensions, sFileNameExtension) != -1)
                {
                    CreateSourceFileHeader(editPoint, textDocument);
                }
                else if (Array.IndexOf(m_InlineFileExtensions, sFileNameExtension) != -1)
                {
                    CreateInlineFileHeader(editPoint, textDocument);
                }

                return;
            }

            int nCommentIndex = sLine.IndexOf("//");
            if (nCommentIndex != -1)
            {
                bool bAllWhilespaces = true;
                for (int i = 0; bAllWhilespaces && i < nCommentIndex; ++i)
                    bAllWhilespaces &= Char.IsWhiteSpace(sLine, i);

                if (bAllWhilespaces)
                {
                    // create line comment
                    CreateLineComment(editPoint, sLine.Substring(nCommentIndex + 2));
                    return;
                }
            }

            if(!sLine.All(Char.IsWhiteSpace))
            {
                editPoint.EndOfLine();
                CreateSimpleComment(editPoint);
                return;
            }

            int nNextLineLen;
            if (buffer.GetLengthOfLine(nLine + 1, out nNextLineLen) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.GetLengthOfLine));

            string sNextLine;
            if (buffer.GetLineText(nLine + 1, 0, nLine + 1, nNextLineLen, out sNextLine) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.GetLineText));

            int nWhiteSpaces = sNextLine.TakeWhile(Char.IsWhiteSpace).Count();

            ProjectItem projectItem = m_DTE.ActiveDocument.ProjectItem
                ?? throw new ArgumentNullException(nameof(m_DTE.ActiveDocument.ProjectItem));

            VCFileCodeModel fileCodeModel = projectItem.FileCodeModel as VCFileCodeModel
                ?? throw new ArgumentNullException(nameof(projectItem.FileCodeModel));

            CodeElement codeElement = FindNextLineCodeElement(fileCodeModel.CodeElements, editPoint, nWhiteSpaces);
            if (codeElement != null)
            {
                // create code element comment
                switch (codeElement.Kind)
                {
                case vsCMElement.vsCMElementFunction:
                    CreateFunctionComment(editPoint, nWhiteSpaces, (VCCodeFunction)codeElement);
                    break;

                case vsCMElement.vsCMElementClass:
                    CreateClassComment(editPoint, nWhiteSpaces, (VCCodeClass)codeElement);
                    break;

                case vsCMElement.vsCMElementStruct:
                    CreateStructComment(editPoint, nWhiteSpaces, (VCCodeStruct)codeElement);
                    break;

                case vsCMElement.vsCMElementMacro:
                    CreateMacroComment(editPoint, nWhiteSpaces, (VCCodeMacro)codeElement);
                    break;

                case vsCMElement.vsCMElementNamespace:
                    CreateNamespaceComment(editPoint, nWhiteSpaces, (VCCodeNamespace)codeElement);
                    break;

                case vsCMElement.vsCMElementUnion:
                    CreateUnionComment(editPoint, nWhiteSpaces, (VCCodeUnion)codeElement);
                    break;

                case vsCMElement.vsCMElementTypeDef:
                    CreateTypedefComment(editPoint, nWhiteSpaces, (VCCodeTypedef)codeElement);
                    break;

                case vsCMElement.vsCMElementEnum:
                    CreateEnumComment(editPoint, nWhiteSpaces, (VCCodeEnum)codeElement);
                    break;

                default:
                    CreateSimpleComment(editPoint, nWhiteSpaces);
                    break;
                }
            }
        }
    }
}
