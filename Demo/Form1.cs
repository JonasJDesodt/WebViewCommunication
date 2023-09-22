using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Core.Raw;
using Microsoft.Web.WebView2.WinForms;

namespace Demo
{
    public partial class Form1 : Form
    {
        private readonly WebView2 _webView = new WebView2();

        private readonly TextBox _addressBar = new TextBox();

        private string _html = GetStartupHtml();

        private readonly StringBuilder _script = new StringBuilder();

        private readonly BridgeAnotherClass _bridge = new BridgeAnotherClass() { Prop = "Hello world!" };

        private readonly Panel _addDivContainer = new Panel();


        public Form1()
        {
            InitializeComponent();

            InitializeGui();
        }
        
        //SEQUENCE
        public async void InitializeGui()
        {
            await InitializeControls();
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

            var helloWorldButton = new Button()
            {
                Text = "Hello World",
                Dock = DockStyle.Top
            };
            helloWorldButton.Click += OnHelloWorldButtonClick;
            containerLeft.Controls.Add(helloWorldButton);

            var testHostObjectButton = new Button()
            {
                Text = "Test Host Object",
                Dock = DockStyle.Top
            };
            testHostObjectButton.Click += OnTestHostObjectButtonClick;
            containerLeft.Controls.Add(testHostObjectButton);

            var devToolsButton = new Button()
            {
                Text = "Dev tools",
                Dock = DockStyle.Top
            };
            devToolsButton.Click += OnDevToolsButtonClick;
            containerLeft.Controls.Add(devToolsButton);


            _addDivContainer.Dock = DockStyle.Top;
            var textBoxX = new TextBox()
            {
                Dock = DockStyle.Top,
                Name = "X"
            }; 
            _addDivContainer.Controls.Add(textBoxX);
            var textBoxY = new TextBox()
            {
                Dock = DockStyle.Top,
                Name = "Y"
            }; 
            _addDivContainer.Controls.Add(textBoxY);
            var addDivButton = new Button()
            {
                Text = "Add Div",
                Dock = DockStyle.Top
            };
            addDivButton.Click += OnAddDivButtonClick;
            _addDivContainer.Controls.Add(addDivButton);
            containerLeft.Controls.Add(_addDivContainer);

            //Panel on the right
            var containerRight = new Panel()
            {
                Dock = DockStyle.Fill,
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

        public async Task InitializeWebView()
        {
            //?? CoreWebView2.DOMContentLoaded ??


            //the coreWebView2 needs to be completely loaded before interaction is possible
            _webView.CoreWebView2InitializationCompleted += OnWebViewCoreWebView2InitializationCompleted;

            //force the coreWebView2 initialization
            await _webView.EnsureCoreWebView2Async(null);
        }


        //EVENT HANDLERS SEQUENCE
        private async void OnWebViewCoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            _webView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;

            //await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");

            await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(GetStartupScript());

            _webView.NavigateToString(_html);


            
            _webView.CoreWebView2.AddHostObjectToScript("bridge", _bridge);
        }

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var message = args.TryGetWebMessageAsString();

            switch (message)
            {
                case "files":
                    GetAndLoadHtmlPageFromCoreWebView2FilePath(args);
                    break;
                default:
                    _addressBar.Text = message;
                    break;
            }

        }
     

        //EVENT HANDLERS ACTIONS
        private void OnUploadButtonClick(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "Text files | *.html", // file types, that will be allowed to upload
                Multiselect = false // allow/deny user to upload more than one file at a time
            };
            
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            _html = File.ReadAllText(dialog.FileName);
            _webView.NavigateToString(_html);
        }

        private void OnInjectBootstrapLinkButtonClick(object sender, EventArgs e)
        {
            //check if there is html in the string
            var htmlIndex = _html.IndexOf("<html>", StringComparison.InvariantCulture);
            if (string.IsNullOrEmpty(_html.Trim()) || htmlIndex < 0)
            {
                MessageBox.Show("Error. HTML element could not be found.");
                return;
            }

            const string bootstrapLink = "<link rel=\"stylesheet\" href=\"https://cdn.jsdelivr.net/npm/bootstrap@4.3.1/dist/css/bootstrap.min.css\" integrity=\"sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T\" crossorigin=\"anonymous\" >";

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
        }



        private void OnInjectFileInputButtonClick(object sender, EventArgs e)
        {
            var id = Guid.NewGuid();

            var htmlIndex = _html.IndexOf("<html>", StringComparison.InvariantCulture);
            if (string.IsNullOrEmpty(_html.Trim()) || htmlIndex < 0)
            {
                MessageBox.Show("Error. HTML element could not be found.");
                return;
            }

            var input = $"<input id=\"{id}\" type=\"File\" name=\"name-input-1\"/>";

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
        }

        private void OnInjectJsScriptButtonClick(object sender, EventArgs e)
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

        private async void OnNavigationButtonClick(object sender, EventArgs e)
        {
            //check if the string can be converted to a valid uri, check on https will be done in EnsureHttps()
            if (!Uri.TryCreate(_addressBar.Text, UriKind.Absolute, out var uriResult))
            {
                await _webView.CoreWebView2.ExecuteScriptAsync($"alert('{_addressBar.Text} is not a valid url')");

                return;
            }
            else
            {
                //if the _webView.Source & uriResult are equal, there will be no navigation
                // => un-subscription from the NavigationStarting event is done in the event handler EnsureHttps()
                // => risk of the event not being unsubscribed, so on every navigation eg NavigateToString() the EnsureHttps method would run
                if (!_webView.Source.Equals(uriResult))
                {
                    _webView.NavigationStarting += EnsureHttps;
                    _webView.Source = uriResult;
                };
            }
        }

        private async void OnHelloWorldButtonClick(object sender, EventArgs e)
        {
            await _webView.ExecuteScriptAsync("alert(\'Hello World!\')");
        }

        private async void OnTestHostObjectButtonClick(object sender, EventArgs e)
        {
            MessageBox.Show(_bridge.Prop);
            _bridge.InvokeFireEvent();
        }

        private void OnDevToolsButtonClick(object sender, EventArgs e)
        {
            _webView.CoreWebView2.OpenDevToolsWindow();
        }

        private async void OnAddDivButtonClick(object sender, EventArgs e)
        {
            var x = _addDivContainer.Controls.Find("X", true)[0] as TextBox;
            var y = _addDivContainer.Controls.Find("Y", true)[0] as TextBox;

            if (x != null && y != null)
            {
                await _webView.CoreWebView2.ExecuteScriptAsync($"AddDiv({x.Text}, {y.Text})");
            }
            else
            {
                MessageBox.Show("Give coordinates");
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


        //METHODS
        private void GetAndLoadHtmlPageFromCoreWebView2FilePath(CoreWebView2WebMessageReceivedEventArgs args)
        {
            if (args.AdditionalObjects != null)
            {
                var objects = args.AdditionalObjects;

                if (objects[0] is CoreWebView2File coreWebView2File)
                {
                    _html = File.ReadAllText(coreWebView2File.Path);
                    _webView.NavigateToString(_html);

                    return;
                }

                MessageBox.Show("Error. The object is not a CoreWebView2File.");
            }

        }

        private void AppendEventListenerToScript(Guid id, string action)
        {
            _script.AppendLine($"document.getElementById(\"{id}\").addEventListener(\"{action}\", event => window.chrome.webview.postMessageWithAdditionalObjects(\"files\", event.target.files));");
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

        private string GetStartupScript()
        {
            var builder = new StringBuilder();

            builder.Append("window.chrome.webview.postMessage(window.document.URL);");

            //builder.Append("document.getElementsByTagName('script')[0].text += await chrome.webview.hostObjects.bridge.GetAddDivFunction();");

            return builder.ToString();
        }
    }
}


//const div = document.getElementById("div");
//div.insertAdjacentHTML('beforeend', 'stuff'); ;