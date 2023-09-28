using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace WebViewCommunication
{
    public partial class Form1 : Form
    {
        //CONTROLS
        private readonly WebView2 _webView = new WebView2();

        private readonly Bridge _bridge = new Bridge() { Prop = "Hello world!" };

        private readonly JsFunctions _jsFunctions = new JsFunctions();


        public Form1()
        {
            InitializeComponent();

            InitializeGui();

            _bridge.PropChanged += (sender, args) => MessageBox.Show("Bridge.Prop=" + _bridge.Prop);
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

            var sendAsJsonPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };

            var sendAsJsonTextBox = new TextBox()
            {
                Width = 50,
                Name = "SendAsJson"
            };
            sendAsJsonPanel.Controls.Add(sendAsJsonTextBox);

            var sendAsJsonButton = new Button()
            {
                Text = "Send As Json",
                AutoSize = true,
                Dock = DockStyle.Top
            };
            sendAsJsonButton.Click += OnSendAsJsonButtonClick;
            sendAsJsonPanel.Controls.Add(sendAsJsonButton);

            containerLeft.Controls.Add(sendAsJsonPanel);

            var injectAddReceiveWebMessageScript = new Button()
            {
                Text = "Inject Receive WebMessage Script",
                Dock = DockStyle.Top
            };
            injectAddReceiveWebMessageScript.Click += OnInjectAddReceiveWebMessageScriptButtonClick;
            containerLeft.Controls.Add(injectAddReceiveWebMessageScript);

            var addFormButton = new Button()
            {
                Text = "Add form",
                Dock = DockStyle.Top
            };
            addFormButton.Click += OnAddFormButtonClick;
            containerLeft.Controls.Add(addFormButton);

            var injectAddFormScript = new Button()
            {
                Text = "Inject AddForm Script",
                Dock = DockStyle.Top
            };
            injectAddFormScript.Click += OnInjectAddFormScriptClick;
            containerLeft.Controls.Add(injectAddFormScript);

            var addDivPanel = new FlowLayoutPanel()
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };

            var labelX = new Label()
            {
                AutoSize = true,
                Text = "X"
            };
            addDivPanel.Controls.Add(labelX);
            var textBoxX = new TextBox()
            {
                Width = 50,
                Name = "X"
            };
            addDivPanel.Controls.Add(textBoxX);

            var labelY = new Label()
            {
                AutoSize = true,
                Text = "Y"
            };
            addDivPanel.Controls.Add(labelY);
            var textBoxY = new TextBox()
            {

                Width = 50,
                Name = "Y"
            };
            addDivPanel.Controls.Add(textBoxY);

            var addDivButton = new Button()
            {
                Text = "Add Div",

            };
            addDivButton.Click += OnAddDivButtonClick;
            addDivPanel.Controls.Add(addDivButton);

            containerLeft.Controls.Add(addDivPanel);

            var injectAddDivScript = new Button()
            {
                Text = "Inject AddDiv Script",
                Dock = DockStyle.Top
            };
            injectAddDivScript.Click += OnInjectAddDivScriptClick;
            containerLeft.Controls.Add(injectAddDivScript);

            var injectScriptAppender = new Button()
            {
                Text = "Inject ScriptAppender",
                Dock = DockStyle.Top
            };
            injectScriptAppender.Click += OnInjectScriptAppenderClick;
            containerLeft.Controls.Add(injectScriptAppender);

            var injectBootstrapButton = new Button()
            {
                Text = "Inject bootstrap link",
                Dock = DockStyle.Top
            };
            injectBootstrapButton.Click += OnInjectBootstrapButtonClick;
            containerLeft.Controls.Add(injectBootstrapButton);

            var uploadButton = new Button()
            {
                Text = "Upload HTML",
                Dock = DockStyle.Top
            };
            uploadButton.Click += OnUploadButtonClick;
            containerLeft.Controls.Add(uploadButton);


            //Panel on the right
            var containerRight = new Panel()
            {
                Dock = DockStyle.Fill,
            };

            _webView.Dock = DockStyle.Fill;
            containerRight.Controls.Add(_webView);

            var navigation = new FlowLayoutPanel()
            {
                Dock = DockStyle.Top,
                AutoSize = true
            };

            navigation.Controls.Add(new TextBox() { Name = "AddressBar", Width = 250 });

            var navigationButton = new Button()
            {
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

            await _webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(GetStartupScript());

            _webView.NavigateToString(GetStartupHtml());

            _webView.CoreWebView2.AddHostObjectToScript("bridge", _bridge);
        }

        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            var json = args.WebMessageAsJson;

            if (JsonConvert.DeserializeObject<Address>(json) is Address address && address.Url != null)
            {
                if (this.Controls.Find("AddressBar", true)[0] is TextBox addressBar)
                {
                    addressBar.Text = address.Url;
                }

                return;
            }

            MessageBox.Show(json);
        }



        //EVENT HANDLERS ACTIONS

        private async void OnInjectAddReceiveWebMessageScriptButtonClick(object sender, EventArgs e)
        {
            await _webView.CoreWebView2.ExecuteScriptAsync($"addScript(\"{_jsFunctions.GetReceiveWebMessageFunction()}\")");
        }

        private void OnSendAsJsonButtonClick(object sender, EventArgs e)
        {
            var textBox = this.Controls.Find("SendAsJson", true)[0] as TextBox;

            var address = new Address()
            {
                Url = textBox?.Text,
            };
            
            _webView.CoreWebView2.PostWebMessageAsJson(JsonConvert.SerializeObject(address));
        }


        private async void OnNavigationButtonClick(object sender, EventArgs e)
        {
            var parent = (sender as Button)?.Parent as FlowLayoutPanel;

            var addressBar = parent?.Controls.Find("AddressBar", true)[0] as TextBox;

            //check if the string can be converted to a valid uri, check on https will be done in EnsureHttps()
            if (!Uri.TryCreate(addressBar?.Text, UriKind.Absolute, out var uriResult))
            {
                await _webView.CoreWebView2.ExecuteScriptAsync($"alert('{addressBar?.Text} is not a valid url')");

                return;
            }
            else
            {
                //if the _webView.Source & uriResult are equal, there will be no navigation
                // => un-subscription from the NavigationStarting event is done in the event handler EnsureHttps()
                // => risk of the event not being unsubscribed, so on every navigation (eg NavigateToString()) the EnsureHttps method would run
                if (!_webView.Source.Equals(uriResult))
                {
                    _webView.NavigationStarting += EnsureHttps;
                    _webView.Source = uriResult;
                };
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


        private async void OnAddFormButtonClick(object sender, EventArgs e)
        {
            await _webView.CoreWebView2.ExecuteScriptAsync($"AddForm()");
        }

        private async void OnInjectAddFormScriptClick(object sender, EventArgs e)
        {
            await _webView.CoreWebView2.ExecuteScriptAsync($"addScript(\"{_jsFunctions.GetAddFormFunction()}\")");
        }

        private async void OnAddDivButtonClick(object sender, EventArgs e)
        {
            var x = this.Controls.Find("X", true)[0] as TextBox;
            var y = this.Controls.Find("Y", true)[0] as TextBox;

            if (x?.Text != null && y?.Text != null)
            {
                await _webView.CoreWebView2.ExecuteScriptAsync($"AddDiv({x.Text}, {y.Text})");
            }
            else
            {
                MessageBox.Show("Give coordinates");
            }
        }

        private async void OnInjectAddDivScriptClick(object sender, EventArgs e)
        {
            var script = $"addScript(\"{_jsFunctions.GetAddDivFunction()}\")";


            await _webView.CoreWebView2.ExecuteScriptAsync(script);
        }

        private async void OnInjectScriptAppenderClick(object sender, EventArgs e)
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.Append("const script = document.createElement('script');");
            scriptBuilder.Append("script.type = 'text/javascript';");

            var contentBuilder = new StringBuilder();
            //contentBuilder.Append("const bridge = chrome.webview.hostObjects.bridge;");
            contentBuilder.Append("async function addScript(func){"); // adds the function to add scripts
            contentBuilder.Append("console.log(func);");
            contentBuilder.Append("const script = document.createElement(\"script\");");
            //contentBuilder.Append("script.innerHTML = await bridge[func];");
            contentBuilder.Append("script.innerHTML = func;");

            contentBuilder.Append("document.getElementsByTagName(\"body\")[0].appendChild(script);");
            contentBuilder.Append("};");

            scriptBuilder.Append($"script.innerHTML = '{contentBuilder}';");
            scriptBuilder.Append($"document.getElementsByTagName('head')[0].appendChild(script);");

            await _webView.CoreWebView2.ExecuteScriptAsync(scriptBuilder.ToString());

        }

        private async void OnInjectBootstrapButtonClick(object sender, EventArgs e)
        {
            await _webView.CoreWebView2.ExecuteScriptAsync(
                $"const link = document.createElement('link'); link.rel = 'stylesheet'; link.href='https://cdn.jsdelivr.net/npm/bootstrap@4.3.1/dist/css/bootstrap.min.css'; link.integrity='sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T'; link.crossOrigin='anonymous'; document.getElementsByTagName('head')[0].appendChild(link); ");
        }

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

            _webView.NavigateToString(File.ReadAllText(dialog.FileName));
        }
        
        
        //METHODS
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
            
            //send the url in to change the text in the addressBar
            //as an object, the postmessage turns it into a json string
            builder.Append("window.chrome.webview.postMessage({url: window.document.URL});");
            
            return builder.ToString();
        }
    }
}
