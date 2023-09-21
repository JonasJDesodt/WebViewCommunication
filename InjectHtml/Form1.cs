using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using static System.Windows.Forms.LinkLabel;

namespace InjectHtml
{
    public partial class Form1 : Form
    {
        private readonly WebView2 _webView = new WebView2();

        private readonly TextBox _navigationBar = new TextBox();

        private string _html = GetStartupHtml(); 

        public Form1()
        {
            InitializeComponent();

            InitializeGui();
        }

        public async void InitializeGui()
        {
            await InitializeControls();
        }

        public async Task InitializeWebView()
        {
            _webView.CoreWebView2InitializationCompleted += OnWebViewCoreWebView2InitializationCompleted;

            await _webView.EnsureCoreWebView2Async(null);
        }

        private void OnWebViewCoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            //_webView.NavigateToString(GetStartupHtml());
            _webView.NavigateToString(_html);

            InjectHtmlInHead();
        }

        public async Task InitializeControls()
        {
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };
            Controls.Add(splitContainer);

            //Panel on the left
            var containerLeft = new Panel()
            {
                Dock = DockStyle.Fill
            };

            var uploadButton = new Button()
            {
                Text = "Upload HTML"
            };
            uploadButton.Click += OnUploadButtonClick;   
            containerLeft.Controls.Add(uploadButton);

            //Panel on the right
            var containerRight = new Panel()
            {
                Dock = DockStyle.Fill
            };

            _webView.Dock = DockStyle.Fill;
            containerRight.Controls.Add(_webView);

            var navigation = new Panel()
            {
                Dock = DockStyle.Top
            };


            _navigationBar.Dock = DockStyle.Fill;
            navigation.Controls.Add(_navigationBar);

            var navigationButton = new Button()
            {
                Dock = DockStyle.Right,
                Text = "Search"
            };
            navigationButton.Click += OnNavigationButtonClick;
            navigation.Controls.Add(navigationButton);

            containerRight.Controls.Add(navigation);

            splitContainer.Panel1.Controls.Add(containerLeft);
            splitContainer.Panel2.Controls.Add(containerRight);

            await InitializeWebView();
        }

        private void OnUploadButtonClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Text files | *.html"; // file types, that will be allowed to upload
            dialog.Multiselect = false; // allow/deny user to upload more than one file at a time
            if (dialog.ShowDialog() == DialogResult.OK) // if user clicked OK
            {
                //use using w/ streamreader?
                //var html =  File.ReadAllText(dialog.FileName);
                //_webView.NavigateToString(html);

                _html = File.ReadAllText(dialog.FileName);
                _webView.NavigateToString(_html);
            }
        }

        private async void EnsureHttps(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            String uri = args.Uri;
            if (!uri.StartsWith("https://"))
            {
                await _webView.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
                args.Cancel = true;
            }

            _webView.NavigationStarting -= EnsureHttps;
        }

        private async void OnNavigationButtonClick(object sender, EventArgs e)
        {
            _webView.NavigationStarting += EnsureHttps;
            
            if (Uri.TryCreate(_navigationBar.Text, UriKind.Absolute, out var uriResult))
            {
                _webView.Source = uriResult;
            }
            else
            {
                await _webView.CoreWebView2.ExecuteScriptAsync($"alert('{_navigationBar.Text} is not a valid url')");
            }
        }

        private static string GetStartupHtml()
        {
            var html = new StringBuilder();

            html.Append("<!DOCTYPE html>");
            html.Append("<html>");

            html.Append("<head>");
            html.Append("</head>");

            html.Append("<body>");
            html.Append("<h1>Startup page</h1>");
            html.Append("<p>generated in WinForms</p>");
            html.Append("</body>");
            html.Append("</html>");

            return html.ToString();
        }

        private void InjectHtmlInHead()
        {
            var htmlIndex = _html.IndexOf("<html>", StringComparison.InvariantCulture);

            if (string.IsNullOrEmpty(_html.Trim()) || htmlIndex < 0)
            {
                MessageBox.Show("Error. HTML element could not be found.");
                return;
            }

            var bootstrapLink = "<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@4.3.1/dist/css/bootstrap.min.css\" integrity=\"sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T\" crossorigin=\"anonymous\" >";
            
            //check if there is a head, if not create head
            if (!_html.Contains("<head>"))
            {
                var head = "<head>" + bootstrapLink + "</head>";

                _html = _html.Insert(htmlIndex + 6, head);
            }
            else
            {
                var headIndex = _html.IndexOf("<head>", StringComparison.InvariantCulture);
                _html = _html.Insert(headIndex + 6, bootstrapLink);
            }
            _webView.NavigateToString(_html);

            //var output = string.Join(";", Regex.Matches(_html, @"\<body\>(.+?)\<\/body\>")
            //    .Cast<Match>()
            //    .Select(m => m.Groups[1].Value));

            //var output = string.Join(";", Regex.Matches(_html, @"\<body\>(.+?)\<\/body\>"));


            //var output = string.Join(";", Regex.Matches(_html, @"\<form\>(.+?)\<\/form\>")
            //     .Cast<Match>());
            //.Select(m => m.Groups[1].Value));


            //var index = _html.IndexOf("<body>", StringComparison.InvariantCulture);
            //if (index < 0)
            //{
            //    MessageBox.Show("Error. Body element could not be found.");
            //    return;
            //}

        }
    }
}
