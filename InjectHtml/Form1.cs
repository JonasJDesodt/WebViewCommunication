﻿using System;

using System.IO;

using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace InjectHtml
{
    public partial class Form1 : Form
    {
        private readonly WebView2 _webView = new WebView2();

        private readonly TextBox _navigationBar = new TextBox();

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
            _webView.NavigateToString(GetStartupHtml());
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
                var path = dialog.FileName; // get name of file
          
                //use using w/ streamreader?
               var html =  File.ReadAllText(dialog.FileName);
               _webView.NavigateToString(html);

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

        private string GetStartupHtml()
        {
            var html = new StringBuilder();

            html.Append("<!DOCTYPE html>");
            html.Append("<html>");
            html.Append("<body>");
            html.Append("<h1>My First Heading</h1>");
            html.Append("<p>My first paragraph</p>");
            html.Append("</body>");
            html.Append("</html>");

            return html.ToString();
        }
    }
}
