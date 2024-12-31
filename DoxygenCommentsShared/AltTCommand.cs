﻿using System;
using System.Linq;
using System.ComponentModel.Design;
using DoxygenComments.Styles;
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

        private readonly DTE m_DTE;
        private readonly IVsTextManager m_TextManager;

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
            ".ii",
            ".ixx",
            ".ipp",
            ".i++",
            ".II",
            ".IXX",
            ".IPP",
            ".I++",
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

        private static readonly vsCMElement[] m_IgnoredElements = {
            vsCMElement.vsCMElementVCBase,
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="AltTCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private AltTCommand(
            AsyncPackage package,
            OleMenuCommandService commandService,
            DTE dte,
            IVsTextManager textManager)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.package = package
                ?? throw new ArgumentNullException(nameof(package));

            m_DTE = dte
                ?? throw new ArgumentNullException(nameof(dte));

            m_TextManager = textManager
                ?? throw new ArgumentNullException(nameof(textManager));

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
                const string sPathEndMarker = "DoxygenComments\\DoxygenCommentsShared\\";
                string[] sPathStartMarkers = { "C:\\", "D:\\" };
                string sStackTrace = ex.StackTrace;
                int nEndIndex = sStackTrace.IndexOf(sPathEndMarker) + sPathEndMarker.Length;
                foreach (string sPathStartMarker in sPathStartMarkers)
                {
                    int nStartIndex = sStackTrace.IndexOf(sPathStartMarker);
                    if (nStartIndex != -1)
                    {
                        string sPath = sStackTrace.Substring(nStartIndex, nEndIndex - nStartIndex);
                        sStackTrace = sStackTrace.Replace(sPath, "");
                    }
                }

                string sMessage = "An exception was thrown :\n\n" + ex.Message + "\n\n" + sStackTrace;

                VsShellUtilities.ShowMessageBox(
                    package,
                    sMessage,
                    "DoxygenComments",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
            }
        }

        private void CreateComment()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (m_TextManager.GetActiveView(1, null, out var textView) != VSConstants.S_OK)
                throw new Exception(nameof(m_TextManager.GetActiveView));

            if (textView.GetCaretPos(out var nLine, out var nColumn) != VSConstants.S_OK)
                throw new Exception(nameof(textView.GetCaretPos));

            if (textView.GetBuffer(out var buffer) != VSConstants.S_OK)
                throw new Exception(nameof(textView.GetBuffer));

            if (buffer.GetLengthOfLine(nLine, out var nLineLen) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.GetLengthOfLine));

            if (buffer.GetLineText(nLine, 0, nLine, nLineLen, out var sLine) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.GetLineText));

            if (buffer.CreateEditPoint(nLine, 0, out var oEditPoint) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.CreateEditPoint));

            EditPoint editPoint = (EditPoint)oEditPoint;

            TextDocument textDocument = m_DTE.ActiveDocument.Object() as TextDocument
                ?? throw new ArgumentNullException(nameof(m_DTE.ActiveDocument));

            ICommentStyle style;
            switch (Settings.Style)
            {
                case SettingsPage.ECommentStyle.Simple:
                    style = new SimpleStyle(Settings);
                    break;

                case SettingsPage.ECommentStyle.SlashBlock:
                    style = new SlashBlockStyle(Settings);
                    break;

                case SettingsPage.ECommentStyle.Qt:
                    style = new QtStyle(Settings);
                    break;

                case SettingsPage.ECommentStyle.Javadoc:
                    style = new JavadocStyle(Settings);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (editPoint.AtStartOfDocument)
            {
                // create file header
                string sFileName = textDocument.Parent.Name;
                int nDotPosition = sFileName.LastIndexOf(".", StringComparison.Ordinal);
                string sFileNameExtension = sFileName.Substring(nDotPosition < 0 ? 0 : nDotPosition);

                if (Array.IndexOf(m_HeaderFileExtensions, sFileNameExtension) != -1)
                {
                    CreateHeaderFileHeader(style, editPoint, textView, textDocument);
                }
                else if (Array.IndexOf(m_SourceFileExtensions, sFileNameExtension) != -1)
                {
                    CreateSourceFileHeader(style, editPoint, textView, textDocument);
                }
                else if (Array.IndexOf(m_InlineFileExtensions, sFileNameExtension) != -1)
                {
                    CreateInlineFileHeader(style, editPoint, textView, textDocument);
                }

                return;
            }

            int nCommentIndex = sLine.IndexOf("//", StringComparison.Ordinal);
            if (nCommentIndex != -1)
            {
                bool bAllWhitespaces = true;
                for (int i = 0; bAllWhitespaces && i < nCommentIndex; ++i)
                    bAllWhitespaces &= Char.IsWhiteSpace(sLine, i);

                if (bAllWhitespaces)
                {
                    // create line comment
                    CreateLineComment(editPoint, nCommentIndex, sLine.Substring(nCommentIndex + 2));
                    return;
                }
            }

            if (!sLine.All(Char.IsWhiteSpace))
            {
                editPoint.EndOfLine();
                CreateSimpleComment(editPoint);
                return;
            }

            if (nLine + 1 >= textDocument.EndPoint.Line)
                return;

            if (buffer.GetLengthOfLine(nLine + 1, out var nNextLineLen) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.GetLengthOfLine));

            if (buffer.GetLineText(nLine + 1, 0, nLine + 1, nNextLineLen, out var sNextLine) != VSConstants.S_OK)
                throw new Exception(nameof(buffer.GetLineText));

            int nWhiteSpaces = sNextLine.TakeWhile(Char.IsWhiteSpace).Count();

            ProjectItem projectItem = m_DTE.ActiveDocument.ProjectItem
                ?? throw new ArgumentNullException(nameof(m_DTE.ActiveDocument.ProjectItem));

            FileCodeModel fileCodeModel = projectItem.FileCodeModel
                ?? throw new ArgumentNullException(
                    nameof(projectItem.FileCodeModel),
                    "The model is null. \n" +
                    "This may be because you are trying to generate a comment in an external file or in a file in the CMake project (Open Folder). \n" +
                    "To generate something more complicated than a file comment, an extension needs the parsed code model that Visual Studio generates, " +
                    "which is only available when using .sln\n");

            CodeElement codeElement = FindNextLineCodeElement(fileCodeModel.CodeElements, editPoint, nWhiteSpaces);
            if (codeElement != null)
            {
                // create code element comment
                switch (codeElement.Kind)
                {
                    case vsCMElement.vsCMElementFunction:
                        CreateFunctionComment(style, editPoint, textView, nWhiteSpaces, codeElement as VCCodeFunction);
                        break;

                    case vsCMElement.vsCMElementClass:
                        CreateClassComment(style, editPoint, textView, nWhiteSpaces, codeElement as VCCodeClass);
                        break;

                    case vsCMElement.vsCMElementStruct:
                        CreateStructComment(style, editPoint, textView, nWhiteSpaces, codeElement as VCCodeStruct);
                        break;

                    case vsCMElement.vsCMElementMacro:
                        CreateMacroComment(style, editPoint, textView, nWhiteSpaces, codeElement as VCCodeMacro);
                        break;

                    case vsCMElement.vsCMElementNamespace:
                        CreateNamespaceComment(style, editPoint, textView, nWhiteSpaces, codeElement as VCCodeNamespace);
                        break;

                    case vsCMElement.vsCMElementUnion:
                        CreateUnionComment(style, editPoint, textView, nWhiteSpaces, codeElement as VCCodeUnion);
                        break;

                    case vsCMElement.vsCMElementTypeDef:
                        CreateTypedefComment(style, editPoint, textView, nWhiteSpaces, codeElement as VCCodeTypedef);
                        break;

                    case vsCMElement.vsCMElementEnum:
                        CreateEnumComment(style, editPoint, textView, nWhiteSpaces, codeElement as VCCodeEnum);
                        break;

                    default:
                        CreateSimpleComment(editPoint, nWhiteSpaces);
                        break;
                }
            }
        }
    }
}
