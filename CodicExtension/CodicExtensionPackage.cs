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
using System.Diagnostics;
using System.IO;
using CodicExtension.Model;

namespace CodicExtension
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideOptionPage(typeof(OptionsPage), "CodicExtension", "Options", 110, 111, true)]
    [ProvideProfile(typeof(OptionsPage), "CodicExtension", "Options", 110, 111, true, DescriptionResourceID = 110)]

    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(Guids.CodicExtensionPackageGuid)]
    [ProvideAutoLoad(UIContextGuids80.CodeWindow)]
    [ProvideAutoLoad("f1536ef8-92ec-443c-9ed7-fdadf150da82")]
    public sealed class CodicExtensionPackage : Microsoft.VisualStudio.Shell.Package
    {
        private IVsStatusbar _vsStatusbar;

        /// <summary>
        /// This read-only property returns the package instance
        /// </summary>
        internal static CodicExtensionPackage Instance { get; private set; }

        private IVsStatusbar StatusBar
        {
            get { return _vsStatusbar ?? (_vsStatusbar = GetService(typeof(SVsStatusbar)) as IVsStatusbar); }
        }

        protected override void Initialize()
        {
            base.Initialize();

            Instance = this;

            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Create the command for the menu item.
                var generateCommandId = new CommandID(Guids.guidTranslatorCmdSet, (int)Guids.GenerationCmdId);
                var menuItemTranslate = new OleMenuCommand(GenerateMenu_Clicked, generateCommandId);
                menuItemTranslate.BeforeQueryStatus += MenuItemTranslateOnBeforeQueryStatus;
                mcs.AddCommand(menuItemTranslate);
            }
        }

        private void MenuItemTranslateOnBeforeQueryStatus(object sender, EventArgs eventArgs)
        {
            var oleMenuCommand = sender as OleMenuCommand;
            if (oleMenuCommand != null)
            {
                var view = GetActiveTextView();
                oleMenuCommand.Enabled = view != null && view.Selection != null && !view.Selection.IsEmpty;
                if (oleMenuCommand.Enabled)
                {
                    var span = view.Selection.SelectedSpans[0];
                    var selectedText = span.GetText();
                    oleMenuCommand.Text = string.Format("Generate Naming for '{0}'", selectedText.Truncate(26));
                }
                else
                {
                    oleMenuCommand.Text = "Generate Naming";
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
            ClearStatusBar();
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

                LetterCase letterCase = LetterCase.ValueOf(props.LetterCaseConvention[fileType]);

                // Get selection in view.
                SnapshotSpan span = view.Selection.SelectedSpans[0];
                var selectedText = span.GetText();
                if (string.IsNullOrEmpty(selectedText))
                {
                    ITextSnapshotLine line = span.Start.GetContainingLine();
                    selectedText = span.Start.GetContainingLine().GetText();
                    view.Selection.Select(new SnapshotSpan(line.Start, line.End), false);
                }

                ITrackingSpan trackingSpan = span.Snapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeExclusive);
                QuickLookDialog dialog = new QuickLookDialog();
                SetDialogPosition(view, dialog);
                dialog.SetText(selectedText);
                dialog.Selected += (sender2, args) => {
                    var buffer = trackingSpan.TextBuffer;
                    Span sp = trackingSpan.GetSpan(buffer.CurrentSnapshot);
                    buffer.Replace(sp, (string)args.Selection);
                };
                dialog.LetterCaseChanged += (sender2, args) => {
                    if (props.LetterCaseConvention.ContainsKey(fileType))
                        props.LetterCaseConvention.Remove(fileType);
                    props.LetterCaseConvention.Add(fileType, args.Selection.Id);
                    props.Save();
                };
                dialog.ShowModeless(letterCase);
            }
            else
            {
                NotifyNothingToTranslate();
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

        private void ClearStatusBar()
        {
            StatusBar.FreezeOutput(0);
            StatusBar.Clear();
        }

        private void NotifyNothingToTranslate()
        {
            int frozen;
            StatusBar.IsFrozen(out frozen);
            if (frozen == 0)
            {
                StatusBar.SetText("Nothing to translate.");
            }
        }
    }
}