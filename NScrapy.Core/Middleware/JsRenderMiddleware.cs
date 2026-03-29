using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NScrapy.Infra;

using InfraIRequest = NScrapy.Infra.IRequest;
using InfraIResponse = NScrapy.Infra.IResponse;

namespace NScrapy.Middleware
{
    public class JsRenderOptions
    {
        public bool Enabled { get; set; } = false;
        public string Browser { get; set; } = "Chromium"; // Chromium, Firefox
        public string WaitUntil { get; set; } = "networkidle"; // domcontentloaded, networkidle, load
        public int TimeoutMs { get; set; } = 30000;
    }

    public class JsRenderMiddleware : EmptyDownloaderMiddleware
    {
        public JsRenderOptions Options { get; set; } = new JsRenderOptions();
        private IBrowser _browser;
        private IBrowserContext _context;
        private IPlaywright _playwright;
        private bool _browserInitialized;

        public JsRenderMiddleware()
        {
        }

        public JsRenderMiddleware(JsRenderOptions options)
        {
            Options = options ?? new JsRenderOptions();
        }

        private async Task EnsureBrowserInitializedAsync()
        {
            if (_browser != null && _browser.IsConnected)
            {
                return;
            }

            if (_browserInitialized)
            {
                // Already initializing or failed, wait a bit and check again
                await Task.Delay(100);
                if (_browser != null && _browser.IsConnected)
                {
                    return;
                }
                throw new InvalidOperationException("Browser initialization in progress or failed");
            }

            _browserInitialized = true;

            try
            {
                _playwright = await Microsoft.Playwright.Playwright.CreateAsync();

                IBrowserType browserType = Options.Browser?.ToLowerInvariant() switch
                {
                    "firefox" => _playwright.Firefox,
                    _ => _playwright.Chromium
                };

                var launchOptions = new BrowserTypeLaunchOptions
                {
                    Headless = true
                };

                _browser = await browserType.LaunchAsync(launchOptions);
                _context = await _browser.NewContextAsync();
            }
            catch
            {
                _browserInitialized = false;
                throw;
            }
        }

        public override async Task<InfraIResponse> ProcessAsync(InfraIRequest request)
        {
            if (Options == null || !Options.Enabled)
            {
                return null;
            }

            try
            {
                await EnsureBrowserInitializedAsync();
                var page = await _context.NewPageAsync();

                var waitUntil = Options.WaitUntil?.ToLowerInvariant() switch
                {
                    "domcontentloaded" => WaitUntilState.DOMContentLoaded,
                    "load" => WaitUntilState.Load,
                    _ => WaitUntilState.NetworkIdle
                };

                var navigationOptions = new PageGotoOptions
                {
                    WaitUntil = waitUntil,
                    Timeout = Options.TimeoutMs > 0 ? Options.TimeoutMs : 30000
                };

                await page.GotoAsync(request.URL, navigationOptions);

                // Wait a bit more for any dynamic content
                await page.WaitForTimeoutAsync(1000);

                var renderedHtml = await page.ContentAsync();

                await page.CloseAsync();

                var response = new HttpResponse
                {
                    URL = request.URL,
                    Request = request,
                    ResponsePlanText = renderedHtml
                };

                return response;
            }
            catch (PlaywrightException ex)
            {
                Console.WriteLine($"JsRenderMiddleware Playwright error: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"JsRenderMiddleware error: {ex.Message}");
                return null;
            }
        }

        public override void PreDownload(InfraIRequest request)
        {
            // Intentionally empty - we use ProcessAsync for JS rendering
            // The base middleware chain will be skipped when ProcessAsync returns a response
        }

        public override void PostDownload(InfraIResponse response)
        {
            // Intentionally empty - response is already handled in ProcessAsync
        }

        public void Dispose()
        {
            _context?.CloseAsync().GetAwaiter().GetResult();
            _browser?.CloseAsync().GetAwaiter().GetResult();
            _playwright?.Dispose();
        }
    }
}
