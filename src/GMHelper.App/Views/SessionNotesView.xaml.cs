using System.Windows.Controls;
using System.Windows.Threading;

namespace GMHelper.App.Views;

public partial class SessionNotesView : UserControl
{
    private readonly DispatcherTimer _previewTimer;
    private bool _webViewReady;

    public SessionNotesView()
    {
        InitializeComponent();

        _previewTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
        _previewTimer.Tick += PreviewTimer_Tick;

        MarkdownTextBox.TextChanged += (_, _) => RestartPreviewTimer();
        Loaded += async (_, _) => await InitializeWebViewAsync();
    }

    private async Task InitializeWebViewAsync()
    {
        try
        {
            await PreviewWebView.EnsureCoreWebView2Async();
            _webViewReady = true;
            UpdatePreview();
        }
        catch (Exception)
        {
            // WebView2 Runtime not installed on this machine — markdown editing still works,
            // the live preview just stays blank instead of crashing the tab.
        }
    }

    private void RestartPreviewTimer()
    {
        _previewTimer.Stop();
        _previewTimer.Start();
    }

    private void PreviewTimer_Tick(object? sender, EventArgs e)
    {
        _previewTimer.Stop();
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (!_webViewReady)
        {
            return;
        }

        var html = Markdig.Markdown.ToHtml(MarkdownTextBox.Text ?? string.Empty);
        var document = $"""
            <html>
            <head>
            <meta charset="utf-8">
            <meta name="color-scheme" content="light">
            </head>
            <body style="font-family: Segoe UI, sans-serif; padding: 8px; background: #ffffff; color: #000000;">
            {html}
            </body>
            </html>
            """;

        PreviewWebView.CoreWebView2.NavigateToString(document);
    }
}
