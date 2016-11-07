
using CodicExtension.Model;
using CodicExtension.Settings;
using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace CodicExtension.Presentation
{

    /// <summary>
    /// Interaction logic for EditTranslationDialog.xaml
    /// </summary>
    public partial class QuickLookDialog : Window
    {
        public event EventHandler<SelectedEventArgs<String>> Selected;
        public event EventHandler<SelectedEventArgs<LetterCase>> LetterCaseChanged;

        private HwndSource _hwndSource;
        private Point offset;
        private bool resizing = false;
        
        private HookProc mouseHookProcedure;
        private int hHook = 0;


        public QuickLookDialog()
        {
            InitializeComponent();

            Activated += (sender, args) =>
            {
                SearchField.Focus();
            };

            Closed += (sender, obj) =>
            {
                Release();
            };

            // Initialize cancel (Emulate the MODELESS dialog)
            InitializeCancel();

            // Initialize resize grip events.
            InitializeResizeGrip();

            // Initialize search box.
            InitializeSearchBox();

            LetterCaseCombo.DisplayMemberPath = "ShortName";
            LetterCaseCombo.SelectedValuePath = "Id";
            LetterCaseCombo.ItemsSource = LetterCase.Enumulate();
            LetterCaseCombo.SelectedIndex = 0;
            LetterCaseCombo.SelectionChanged += (sender, xxx) =>
            {
                OnLetterCaseChanged(new SelectedEventArgs<LetterCase> { Selection = LetterCaseCombo.SelectedItem as LetterCase });
                UpdateListAsync(SearchField.Text, LetterCaseCombo.SelectedItem as LetterCase);
            };

            CandidateList.MouseDoubleClick += (sender, ev) => {
                Select();
            };

            PreviewMouseMove += (sender, ev) => {
                Cursor = Cursors.Arrow;
            };

        }

        public int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            MouseHookStruct myMouseHookStruct = (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));

            var handled = false;
            if (nCode >= 0)
            {
                if (!resizing && (MouseMessages.WM_NCLBUTTONUP == (MouseMessages)wParam ||
                   MouseMessages.WM_NCLBUTTONDOWN == (MouseMessages)wParam))
                {
                    this.Close();
                    handled = true;
                }
                else if (resizing && (MouseMessages.WM_NCLBUTTONUP == (MouseMessages)wParam ||
                    MouseMessages.WM_LBUTTONUP == (MouseMessages)wParam))
                {
                    
                    if (resizing)
                    {
                        Properties.Settings.Default.DialogWidth = this.Width;
                        Properties.Settings.Default.DialogHeight = this.Height;
                    }
                    resizing = false;
                    
                }
                else if ((MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam ||
                    MouseMessages.WM_NCMOUSEMOVE == (MouseMessages)wParam))
                {
                    if (resizing)
                    {
                        POINT p = myMouseHookStruct.pt;
                        Debug.WriteLine(p.x + "," + p.y);
                        this.Width = Math.Max(p.x + offset.X - this.Left, 100);
                        this.Height = Math.Max(p.y + offset.Y - this.Top, 100);
                    }
                }
            }

            if (handled)
            {
                return 1;
            }
            else
            {
                return NativeMethods.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
        }

        private void InitializeSearchBox()
        {
            Observable.FromEventPattern<TextChangedEventHandler, TextChangedEventArgs>(
                s => SearchField.TextChanged += s,
                s => SearchField.TextChanged -= s)
                .Throttle(TimeSpan.FromMilliseconds(200))
                .ObserveOnDispatcher()
                .Subscribe(e =>
                {
                    UpdateListAsync(SearchField.Text, LetterCaseCombo.SelectedItem as LetterCase);
                });


            SearchField.PreviewKeyDown += (sender, e) =>
            {
                if (e.Key == Key.Down)
                {
                    e.Handled = true;
                }
                else if (e.Key == Key.Up)
                {
                    e.Handled = true;
                }
                else if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                    Select();   // Use key-down event (Filters IME enter).
                }
            };


            SearchField.PreviewKeyUp += (sender, e) =>
            {
                if (e.Key == Key.Down)
                {
                    e.Handled = true;
                    ScrollDown();
                }
                else if (e.Key == Key.Up)
                {
                    e.Handled = true;
                    ScrollUp();
                }
                else if (e.Key == Key.Enter)
                {
                    e.Handled = true;
                }
            };
        }

        public void ShowModeless()
        {
            ShowModeless(null);
        }

        public void ShowModeless(LetterCase letterCae)
        {
            this.Width = Properties.Settings.Default.DialogWidth;
            this.Height = Properties.Settings.Default.DialogHeight;

            this.LetterCaseCombo.SelectedItem = letterCae;

            base.ShowDialog();
        }

        protected override void OnInitialized(EventArgs e)
        {
            SourceInitialized += OnSourceInitialized;
            base.OnInitialized(e);
        }

        /// <summary>
        /// Initialzie canceling.
        /// See : http://stackoverflow.com/questions/10299856/wpf-intercept-clicks-outside-a-modal-window
        /// </summary>
        private void InitializeCancel()
        {
            // Create an instance of HookProc.
            mouseHookProcedure = new HookProc(MouseHookProc);
            hHook = NativeMethods.SetWindowsHookEx(WindowHandleConstans.WH_MOUSE,
                        mouseHookProcedure,
                        (IntPtr)0,
                        AppDomain.GetCurrentThreadId());
            if (hHook == 0)
            {
                return;
            }

            Deactivated += (sender, args) =>
            {
                // Cannot call 'Close()' directly from window event handler.
                Dispatcher.BeginInvoke((Action)(() => Close()));
            };

            // Handing of ESC key.
            KeyDown += (sender, args) =>
            {
                if (args.Key == Key.Escape)
                {
                    Close();
                }
            };
        }

        /// <summary>
        /// Initialize resize grip.
        /// </summary>
        private void InitializeResizeGrip()
        {
            foreach (UIElement element in ResizeGrid.Children)
            {
                element.MouseDown += (sender, e) =>
                {
                    Point p1 = e.GetPosition(this);
                    offset = new Point(Width - p1.X, Height - p1.Y);
                    resizing = true;
                    Cursor = Cursors.SizeNWSE;
                };
                element.MouseMove += (sender, e) =>
                {
                    Cursor = Cursors.SizeNWSE;
                };
            }
        }

        private void ScrollUp()
        {
            if (CandidateList.Items.IsEmpty)
            {
                return;
            }
            if (CandidateList.SelectedIndex <= 0)
            {
                CandidateList.SelectedIndex = CandidateList.Items.Count - 1;
            }
            else
            {
                CandidateList.SelectedIndex--;
            }
        }

        private void ScrollDown()
        {
            if (CandidateList.Items.IsEmpty)
            {
                return;
            }
            if (CandidateList.SelectedIndex < 0 ||
                CandidateList.SelectedIndex >= CandidateList.Items.Count - 1)
            {
                CandidateList.SelectedIndex = 0;
            }
            else
            {
                CandidateList.SelectedIndex++;
            }
        }


        private void Select()
        {
            if (CandidateList.SelectedIndex >= 0)
            {
                OnSelected(new SelectedEventArgs<string> {  Selection = (CandidateList.SelectedItem as string) } );
                Close();
            }
        }

        private void WindowResize(object sender, MouseButtonEventArgs e) //PreviewMousLeftButtonDown
        {
            NativeMethods.SendMessage(_hwndSource.Handle, 0x112, (IntPtr)61448, IntPtr.Zero);
        }

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            _hwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }

        public void SetText(String text)
        {
            SearchField.Text = text;
            SearchField.SelectAll();
        }

        private void SetStatusMessage(string message)
        {
            if (message == null)
            {
                CandidateList.Visibility = Visibility.Visible;
                StatusLabel.Visibility = Visibility.Hidden;
            } else
            {
                CandidateList.Visibility = Visibility.Hidden;
                StatusLabel.Visibility = Visibility.Visible;
                StatusLabel.Text = message;
            }

        }

        private async void UpdateListAsync(string text, LetterCase letterCase)
        {
            SetStatusMessage(null);

            var options = Options.Get();
            if (options.AccessToken == null ||
                options.AccessToken.Equals(""))
            {
                SetStatusMessage("オプションページよりアクセストークンを設定してください。");
                return;
            }

            if (text == null || text.Equals(""))
            {
                CandidateList.Items.Clear();
                return;
            }

           
            var api = new CodicApi();
            TranslationResult result = null;
            try
            {
                result = await api.TranslateAsync(options.AccessToken, options.ProjectId, text, letterCase.Id);
            }
            catch (ApiException e)
            {
                SetStatusMessage(e.Message + "");
                return;
            }

            CandidateList.Items.Clear();
            if (result.Translations[0].Words.Count == 1 && result.Translations[0].Words[0].Successful)
            {
                foreach (string text_ in result.Translations[0].Words[0].Candidates)
                {
                    CandidateList.Items.Add(text_);
                }
            }
            else {
                CandidateList.Items.Add(result.Translations[0].TranslatedText);
            }
        }

        private void Release()
        {
            if (hHook != 0)
                NativeMethods.UnhookWindowsHookEx(hHook);
            hHook = 0;
        }

        protected virtual void OnSelected(SelectedEventArgs<string> e)
        {
            if (Selected != null)
                Selected(this, e);
        }

        protected virtual void OnLetterCaseChanged(SelectedEventArgs<LetterCase> e)
        {
            if (LetterCaseChanged != null)
                LetterCaseChanged(this, e);
        }
    }

    public class SelectedEventArgs<T> : EventArgs
    {
        public T Selection { get; set; }
    }

}
