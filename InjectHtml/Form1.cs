using System;
using System.Security.Policy;
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

        public void InitializeWebView()
        {
            _webView.NavigationStarting += EnsureHttps;
        }
        
        public void InitializeGui()
        {
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical
            };
            Controls.Add(splitContainer);


            //Panel on the right
            var container = new Panel()
            {
                Dock = DockStyle.Fill
            };

            _webView.Source = new Uri("https://localhost:7205");
            _webView.Dock = DockStyle.Fill;
            container.Controls.Add(_webView);

            var navigation = new Panel()
            {
                Dock = DockStyle.Top
            };


            _navigationBar.Dock = DockStyle.Fill;
            navigation.Controls.Add(_navigationBar);

            var navigationButton = new Button()
            {
                Dock = DockStyle.Right,
                AutoSize = true,
                Text = "Search"
            };
            navigationButton.Click += OnNavigationButtonClick;
            navigation.Controls.Add(navigationButton);

            container.Controls.Add(navigation);


            splitContainer.Panel2.Controls.Add(container);

            InitializeWebView();
        }

        void EnsureHttps(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            String uri = args.Uri;
            if (!uri.StartsWith("https://"))
            {
                _webView.CoreWebView2.ExecuteScriptAsync($"alert('{uri} is not safe, try an https link')");
                args.Cancel = true;
            }
        }

        private void OnNavigationButtonClick(object sender, EventArgs e)
        {
            if (Uri.TryCreate(_navigationBar.Text, UriKind.Absolute, out var uriResult))
            {
                _webView.Source = uriResult;
            }
            else
            {
                _webView.CoreWebView2.ExecuteScriptAsync($"alert('{_navigationBar.Text} is not a valid url')");

            }
        }
    }
}
