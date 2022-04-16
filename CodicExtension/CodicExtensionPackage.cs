using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using CodicExtension.Presentation;
using CodicExtension.Settings;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using CodicExtension.Model;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.VisualStudio;

namespace CodicExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideOptionPage(typeof(OptionsPage), "CodicExtension", "Options", 110, 111, true)]
    [ProvideProfile(typeof(OptionsPage), "CodicExtension", "Options", 110, 111, true, DescriptionResourceID = 110)]

    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.CodicExtensionPackageGuid)]
    [ProvideService(typeof(CodicExtensionPackage), IsAsyncQueryable = true)]
    //https://github.com/microsoft/VSSDK-Extensibility-Samples/tree/master/AsyncPackageMigration
    //https://docs.microsoft.com/en-us/visualstudio/extensibility/how-to-use-asyncpackage-to-load-vspackages-in-the-background?view=vs-2019
    [ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExistsAndFullyLoaded_string, PackageAutoLoadFlags.BackgroundLoad)]

    //[ProvideAutoLoad(UIContextGuids80.CodeWindow)]
    //[ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class CodicExtensionPackage : AsyncPackage
    {
        /// <summary>
        /// This read-only property returns the package instance
        /// </summary>
        internal static CodicExtensionPackage Instance { get; private set; }

        private static Dispatcher _dispatcher;

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            Instance = this;
            _dispatcher = Dispatcher.CurrentDispatcher;

            //await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await ThreadHelper.JoinableTaskFactory.RunAsync(async delegate
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
                if (null != mcs)
                {
                    // Create the command for the menu item.
                    var generateCommandId = new CommandID(Guids.guidTranslatorCmdSet, (int)Guids.GenerationCmdId);
                    var menuItemTranslate = new OleMenuCommand(GenerateMenu_Clicked, generateCommandId);
                    menuItemTranslate.BeforeQueryStatus += MenuItemTranslateOnBeforeQueryStatus;
                    mcs.AddCommand(menuItemTranslate);
                }
            });
        }

        private void MenuItemTranslateOnBeforeQueryStatus(object sender, EventArgs eventArgs)
        {
            var oleMenuCommand = sender as OleMenuCommand;
            if (oleMenuCommand != null)
            {
                oleMenuCommand.Enabled = true;
                //oleMenuCommand.Text = "Generate Naming";
                oleMenuCommand.Text = "codic: ネーミングを生成";
                var view = GetActiveTextView();
                if (view != null)
                {
                    if (view.Selection != null && !view.Selection.IsEmpty)
                    {
                        var span = view.Selection.SelectedSpans[0];
                        var selectedText = span.GetText();
                        //oleMenuCommand.Text = string.Format("Generate Naming for '{0}'", selectedText.Truncate(26));
                        oleMenuCommand.Text = string.Format("codic: '{0}'のネーミングを生成", selectedText.Truncate(26));
                    }
                } else
                {
                    oleMenuCommand.Enabled = false;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            Instance = null;
            base.Dispose(disposing);
        }

        public static ITextDocument GetTextDocument(ITextBuffer TextBuffer)
        {
            ITextDocument textDoc;
            var rc = TextBuffer.Properties.TryGetProperty<ITextDocument>(
              typeof(ITextDocument), out textDoc);
            if (rc == true)
                return textDoc;
            else
                return null;
        }

        private IWpfTextView GetActiveTextView()
        {
            IVsTextView vTextView = null;
            
            IVsTextManager txtMgr = (IVsTextManager)GetService(typeof(SVsTextManager));
            txtMgr.GetActiveView(1, null, out vTextView);

            var userData = vTextView as IVsUserData;
            if (null == userData) return null;

            object holder;
            var guidViewHost = DefGuidList.guidIWpfTextViewHost;
            userData.GetData(ref guidViewHost, out holder);
            var viewHost = (IWpfTextViewHost)holder;
            
            return viewHost.TextView;
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void GenerateMenu_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("#GenerateMenu_Clicked");
            var props = Properties.Settings.Default;
            IWpfTextView view = GetActiveTextView();
            
            ITextSelection selection = view.Selection;
            if (selection != null)
            {
                // Get extension of active document.
                String fileType = "_default_";
                var textDoc = GetTextDocument(view.TextBuffer);
                if (textDoc != null)
                {
                    fileType = Path.GetExtension(textDoc.FilePath).ToLower();
                }

                LetterCase letterCase = LetterCase.ValueOf(props.LetterCaseConvention[fileType],
                    LetterCase.Enumulate()[0]);

                // Get selection in view.
                SnapshotSpan span = view.Selection.SelectedSpans[0];
                var selectedText = span.GetText();
                //if (string.IsNullOrEmpty(selectedText))
                //{
                //ITextSnapshotLine line = span.Start.GetContainingLine();
                //selectedText = span.Start.GetContainingLine().GetText();
                //view.Selection.Select(new SnapshotSpan(line.Start, line.End), false);
                //}

                ITrackingSpan trackingSpan = span.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
                QuickLookDialog dialog = new QuickLookDialog();
                Console.WriteLine("#GenerateMenu_Clicked > SetDialogPosition");
                SetDialogPosition(view, dialog);
                dialog.SetText(selectedText);
                dialog.Selected += (sender2, args) => {
                    ThreadHelper.JoinableTaskFactory.Run(async delegate {
                        await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        var buffer = trackingSpan.TextBuffer;
                        Span sp = trackingSpan.GetSpan(buffer.CurrentSnapshot);
                        buffer.Replace(sp, (string)args.Selection);
                    });
                    //_dispatcher.Invoke(new Action(() => {
                    //    var buffer = trackingSpan.TextBuffer;
                    //Span sp = trackingSpan.GetSpan(buffer.CurrentSnapshot);
                    //buffer.Replace(sp, (string)args.Selection);
                    //}));

                };
                dialog.LetterCaseChanged += (sender2, args) => {
                    if (props.LetterCaseConvention.ContainsKey(fileType))
                        props.LetterCaseConvention.Remove(fileType);
                    if (args.Selection != null)
                    {
                        props.LetterCaseConvention.Add(fileType, args.Selection.Id);
                        props.Save();
                    }
                };
                Console.WriteLine("#GenerateMenu_Clicked > dialog.ShowModeless");
                dialog.ShowModeless(letterCase);
            }
        }

        private void SetDialogPosition(IWpfTextView _view, QuickLookDialog dialog)
        {
            SnapshotSpan span = _view.Selection.SelectedSpans[0];

            var textViewOrigin = (_view as System.Windows.UIElement).PointToScreen(new Point(0, 0));

            var caretPos = _view.Selection.Start.Position;
            var charBounds = _view
                .GetTextViewLineContainingBufferPosition(caretPos)
                .GetCharacterBounds(caretPos);
            double textBottom = charBounds.Bottom;
            double textX = charBounds.Left;

            double newLeft = textViewOrigin.X + textX - _view.ViewportLeft;
            double newTop = textViewOrigin.Y + textBottom - _view.ViewportTop;

            dialog.Left = newLeft;
            dialog.Top = newTop;
        }
    }
}