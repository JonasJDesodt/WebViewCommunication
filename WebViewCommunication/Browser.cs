using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WebViewCommunication
{
    public partial class Browser : Form
    {
        private readonly WebView2 _webView = new WebView2();

        private readonly TextBox _navigationBar = new TextBox();

        public Browser()
        {
            InitializeComponent();
            InitializeGui();
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

            _webView.Source = new Uri("https://learn.microsoft.com/en-us/microsoft-edge/webview2/get-started/winforms");
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
        }

        private void OnNavigationButtonClick(object sender, EventArgs e)
        {
            var result = Uri.TryCreate(_navigationBar.Text, UriKind.Absolute, out var uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (result)
            {
                _webView.Source = uriResult;
            }
            else
            {
                MessageBox.Show("Error. Enter a valid url");
            }
        }
    }
}
