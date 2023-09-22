using System;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using static System.Windows.Forms.LinkLabel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

//https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2.postwebmessageasjson?view=webview2-dotnet-1.0.1938.49
//https://learn.microsoft.com/en-us/microsoft-edge/webview2/reference/javascript/webview

namespace InjectHtml
{
    public partial class Form1 : Form
    {
        private readonly WebView2 _webView = new WebView2();

        private readonly TextBox _addressBar = new TextBox();

        private string _html = GetStartupHtml();

        private StringBuilder _script = new StringBuilder();

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

        private async void OnWebViewCoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            //_webView.NavigateToString(GetStartupHtml());

            _webView.CoreWebView2.WebMessageReceived += UpdateAddressBar;

            await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");

            _webView.NavigateToString(_html);
        }

        //https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2webmessagereceivedeventargs.additionalobjects?view=webview2-dotnet-1.0.1938.49
        void UpdateAddressBar(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var uri = args.TryGetWebMessageAsString();

            if (args.AdditionalObjects != null)
            {

                var objects = args.AdditionalObjects;

                var html = File.ReadAllText((objects[0] as CoreWebView2File).Path);
            }
            
            _addressBar.Text = uri;
            //_webView.CoreWebView2.PostWebMessageAsString(uri);
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
                Text = "Upload HTML",
                Dock = DockStyle.Top
            };
            uploadButton.Click += OnUploadButtonClick;   
            containerLeft.Controls.Add(uploadButton);

            var injectBootstrapLinkButton = new Button()
            {
                Text = "Inject Bootstraplink",
                Dock = DockStyle.Top
            };
            injectBootstrapLinkButton.Click += OnInjectBootstrapLinkButtonClick;
            containerLeft.Controls.Add(injectBootstrapLinkButton);

            var injectFileInputButton = new Button()
            {
                Text = "Inject File input",
                Dock = DockStyle.Top
            };
            injectFileInputButton.Click += OnInjectFileInputButtonClick;
            containerLeft.Controls.Add(injectFileInputButton);

            var injectJsScriptButton = new Button()
            {
                Text = "Inject Js Script",
                Dock = DockStyle.Top
            };
            injectJsScriptButton.Click += OnInjectJsScriptButtonClick;
            containerLeft.Controls.Add(injectJsScriptButton);


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


            _addressBar.Dock = DockStyle.Fill;
            navigation.Controls.Add(_addressBar);

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

  

        private void OnInjectJsScriptButtonClick(object sender, EventArgs e)
        {
            //AppendEventListenerToScript();
            InjectScriptInBody();
        }


        private void OnInjectBootstrapLinkButtonClick(object sender, EventArgs e)
        {
            InjectBootstrapLinkInHead();
        }

        private void OnInjectFileInputButtonClick(object sender, EventArgs e)
        {
            var id = InjectInputFieldInBody("file");
            if (id is null)
            {
                MessageBox.Show("Input could not be injected");
                return;
            }
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
            var uri = args.Uri;
            if (!uri.StartsWith("https://"))
            {
                await _webView.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
                args.Cancel = true;
            }

            _webView.NavigationStarting -= EnsureHttps;
        }

        private async void OnNavigationButtonClick(object sender, EventArgs e)
        {
            //check if the string can be converted to a valid uri, check on https will be done in EnsureHttps()
            if (Uri.TryCreate(_addressBar.Text, UriKind.Absolute, out var uriResult))
            {
                //if the _webView.Source & uriResult are equal, there will be no navigation
                // => unsubscription from the NavigationStarting event is done in the eventhandler EnsureHttps()
                // => risk of the event not being unsubscribed, so on every navigation eg NavigateToString() the EnsureHttps method would run
                if(!_webView.Source.Equals(uriResult))
                {
                    _webView.NavigationStarting += EnsureHttps;
                    _webView.Source = uriResult;
                };
         
            }
            else
            {
                await _webView.CoreWebView2.ExecuteScriptAsync($"alert('{_addressBar.Text} is not a valid url')");
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

        private void InjectBootstrapLinkInHead()
        {
            //check if there is html in the string
            var htmlIndex = _html.IndexOf("<html>", StringComparison.InvariantCulture);
            if (string.IsNullOrEmpty(_html.Trim()) || htmlIndex < 0)
            {
                MessageBox.Show("Error. HTML element could not be found.");
                return;
            }

            var bootstrapLink = "<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@4.3.1/dist/css/bootstrap.min.css\" integrity=\"sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T\" crossorigin=\"anonymous\" >";
            
            //check if there is a head, if not: create head
            if (!_html.Contains("<head>"))
            {
                var head = "<head>" + bootstrapLink + "</head>";

                _html = _html.Insert(htmlIndex + 6, head); // <head> == 6 characters
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

        private Guid? InjectInputFieldInBody(string type)
        {
            var id = Guid.NewGuid();

            var htmlIndex = _html.IndexOf("<html>", StringComparison.InvariantCulture);
            if (string.IsNullOrEmpty(_html.Trim()) || htmlIndex < 0)
            {
                MessageBox.Show("Error. HTML element could not be found.");
                return null;
            }

            var input = $"<input id=\"{id}\" type=\"{type}\" name=\"name-input-1\"/>";   

            //check if there is a head, if not: create head
            if (!_html.Contains("<body>"))
            {
                var head = "<body>" + input + "</body>";

                _html = _html.Insert(htmlIndex + 6, head); // <head> == 6 characters
            }
            else
            {
                var headIndex = _html.IndexOf("<body>", StringComparison.InvariantCulture);

                _html = _html.Insert(headIndex + 6, input);
            }

            _webView.NavigateToString(_html);

            AppendEventListenerToScript(id, "change");

            return id;
        }

        private void InjectScriptInBody()
        {
            var htmlIndex = _html.IndexOf("<html>", StringComparison.InvariantCulture);
            if (string.IsNullOrEmpty(_html.Trim()) || htmlIndex < 0)
            {
                MessageBox.Show("Error. HTML element could not be found.");
                return;
            }
            
            //search for closing tag, scripts need to be placed at the end of the body
            var bodyIndex = _html.IndexOf("</body>", StringComparison.InvariantCulture);
            if (bodyIndex < 0)
            {
                MessageBox.Show("Error. Body element could not be found.");
                return;
            }


            if (!_html.Contains("<script>"))
            {
                var script = "<script>" + _script + "</script>";

                _html = _html.Insert(bodyIndex, script); //  -1 => insert right before closing tag
            }
            else
            {
                var headIndex = _html.IndexOf("<script>", StringComparison.InvariantCulture);

                _html = _html.Insert(headIndex + 8, _script.ToString());
            }

            _webView.NavigateToString(_html);
        }

        private void AppendEventListenerToScript(Guid id, string action)
        {
            //_script.AppendLine(
            //    "document.getElementById(\"id-input-1\").addEventListener(\"change\", event => window.chrome.webview.postMessage(event.target.value));");

            _script.AppendLine($"const input = document.getElementById(\"{id}\");");

            //_script.AppendLine("input.addEventListener(\"change\", event => window.chrome.webview.postMessageWithAdditionalObjects(event.target.id, [{value: input.target.value}]));");
            _script.AppendLine($"input.addEventListener(\"{action}\", event => window.chrome.webview.postMessageWithAdditionalObjects(event.target.id, input.files));");
            //_script.AppendLine("input.addEventListener(\"change\", function(event) {const args = {\"values\": [{\"input\": input.value}]}; console.log(args); window.chrome.webview.postMessageWithAdditionalObjects(event.target.id, args); });");


            // ?? => https://learn.microsoft.com/en-us/microsoft-edge/webview2/reference/javascript/webview#webview2script-webview-postmessagewithadditionalobjects-member(1)
            // Call with the ArrayBuffer from the chrome.webview.sharedbufferreceived event to release the underlying shared memory resource.


        }

    }
}
