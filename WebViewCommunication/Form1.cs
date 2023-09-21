using System;
using System.Windows.Forms;
using Microsoft.Web.WebView2.WinForms;

namespace WebViewCommunication
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
            _webView.Source = new Uri(_navigationBar.Text);
        }
    }
}
