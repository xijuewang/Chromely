﻿// Copyright © 2017 Chromely Projects. All rights reserved.
// Use of this source code is governed by MIT license that can be found in the LICENSE file.

#nullable disable
namespace Chromely.Browser;

/// <summary>
/// The CefGlue browser.
/// </summary>
public abstract partial class ChromiumBrowser
{
    protected readonly IChromelyConfiguration _config;
    protected readonly ChromelyHandlersResolver _handlersResolver;
    protected CefMessageRouterBrowserSide _browserMessageRouter;
    protected IntPtr _browserWindowHandle;

    private CefBrowserSettings _settings;
    private CefClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChromiumBrowser"/> class.
    /// </summary>
    /// <param name="nativeHost"></param>
    /// <param name="config"></param>
    /// <param name="handlersResolver"></param>
    public ChromiumBrowser(IChromelyNativeHost nativeHost, IChromelyConfiguration config, ChromelyHandlersResolver handlersResolver)
    {
        Browser = null;
        NativeHost = nativeHost;
        _config = config;
        _handlersResolver = handlersResolver;
    }

    /// <summary>
    /// Gets the native host - instance of <see cref="IChromelyNativeHost"/>.
    /// </summary>
    public IChromelyNativeHost NativeHost { get; private set; }

    #region Events Handling Properties

    /// <summary>
    /// The created.
    /// </summary>
    public event EventHandler Created;

    /// <summary>
    /// The title changed.
    /// </summary>
    public event EventHandler<TitleChangedEventArgs> TitleChanged;

    /// <summary>
    /// The address changed.
    /// </summary>
    public event EventHandler<AddressChangedEventArgs> AddressChanged;

    /// <summary>
    /// The status message.
    /// </summary>
    public event EventHandler<StatusMessageEventArgs> StatusMessage;

    /// <summary>
    /// The console message.
    /// </summary>
    public event EventHandler<ConsoleMessageEventArgs> ConsoleMessage;

    /// <summary>
    /// The loading state change.
    /// </summary>
    public event EventHandler<LoadingStateChangedEventArgs> LoadingStateChanged;

    /// <summary>
    /// The tooltip changed.
    /// </summary>
    public event EventHandler<TooltipEventArgs> TooltipChanged;

    /// <summary>
    /// The before close.
    /// </summary>
    public event EventHandler<BeforeCloseEventArgs> BeforeClose;

    /// <summary>
    /// The before popup.
    /// </summary>
    public event EventHandler<BeforePopupEventArgs> BeforePopup;

    /// <summary>
    /// The frame load started.
    /// </summary>
    public event EventHandler<FrameLoadStartEventArgs> FrameLoadStart;

    /// <summary>
    /// The frame load end.
    /// </summary>
    public event EventHandler<FrameLoadEndEventArgs> FrameLoadEnd;

    /// <summary>
    /// The load error.
    /// </summary>
    public event EventHandler<LoadErrorEventArgs> LoadError;

    /// <summary>
    /// The plugin crashed.
    /// </summary>
    public event EventHandler<PluginCrashedEventArgs> PluginCrashed;

    /// <summary>
    /// The render process terminated.
    /// </summary>
    public event EventHandler<RenderProcessTerminatedEventArgs> RenderProcessTerminated;

    #endregion Events Handling Properties

    /// <summary>
    /// Gets the owner.
    /// </summary>
    public object Owner { get; }

    /// <summary>
    /// Gets or sets the HostHandle.
    /// </summary>
    public IntPtr HostHandle { get; set; }

    /// <summary>
    /// Gets or sets the start url.
    /// </summary>
    public string StartUrl { get; set; }

    /// <summary>
    /// Gets the title.
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// Gets the address.
    /// </summary>
    public string Address { get; private set; }

    /// <summary>
    /// Gets the cef browser.
    /// </summary>
    public CefBrowser Browser { get; private set; }

    /// <summary>
    /// Gets the browser host - instance of <see cref="CefBrowserHost"/>.
    /// </summary>
    public CefBrowserHost BrowserHost
    {
        get
        {
            if (_browserWindowHandle != IntPtr.Zero)
            {
                return Browser?.GetHost();
            }

            return default;
        }
    }

    /// <summary>
    /// Create the CEF client - instance of <see cref="CefClient"/>.
    /// </summary>
    /// <returns>instance of <see cref="CefClient"/>.</returns>
    protected virtual CefClient CreateClient()
    {
        return new CefBrowserClient(_browserMessageRouter, _handlersResolver);
    }

    internal void NotifyMoveOrResize()
    {
        NotifyMoveOrResizeStarted();
    }

    internal void CreateBrowser(IntPtr hostHandle, IntPtr winXID)
    {
        if (_client is null)
        {
            _client = CreateClient();
        }
        if (_settings is null)
        {
            _settings = new CefBrowserSettings();
        }

        _settings.DefaultEncoding = "UTF-8";

        HostHandle = hostHandle;
        var windowInfo = CefWindowInfo.Create();
        
        var scale = NativeHost.GetWindowDpiScale();
        var width = (int)Math.Round(_config.WindowOptions.Size.Width * scale, 0);
        var height = (int)Math.Round(_config.WindowOptions.Size.Height * scale, 0);
        windowInfo.SetAsChild(winXID, new CefRectangle(0, 0, width, height));

        Address = _config.StartUrl;
        StartUrl = _config.StartUrl;
        CefBrowserHost.CreateBrowser(windowInfo, _client, _settings, StartUrl);
    }

    #region Events Handling

    /// <summary>
    /// The on created.
    /// </summary>
    /// <param name="browser">
    /// The browser.
    /// </param>
    public void OnBrowserAfterCreated(CefBrowser browser)
    {
        Browser = browser;

        // Register JavaScriptExecutor
        _config.JavaScriptExecutor = new DefaultJavaScriptExecutor(browser);

        Created?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// The on title changed.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnTitleChanged(TitleChangedEventArgs eventArgs)
    {
        Title = eventArgs.Title;
        TitleChanged?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on address changed.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnAddressChanged(AddressChangedEventArgs eventArgs)
    {
        Address = eventArgs.Address;
        AddressChanged?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on status message.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnStatusMessage(StatusMessageEventArgs eventArgs)
    {
        StatusMessage?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on console message.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnConsoleMessage(ConsoleMessageEventArgs eventArgs)
    {
        ConsoleMessage?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on loading state change.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnLoadingStateChange(LoadingStateChangedEventArgs eventArgs)
    {
        LoadingStateChanged?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on tooltip.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnTooltip(TooltipEventArgs eventArgs)
    {
        TooltipChanged?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on before close.
    /// </summary>
    public void OnBeforeClose()
    {
        BeforeClose?.Invoke(this, null);

        // Flush cache
        // This is needed since there is some delay from when a cookie
        // has been set/deleted before it is actually persisted to disk.
        // This ensures all cookies is written to disk before exit.
        var cookieManager = CefCookieManager.GetGlobal(null);
        cookieManager?.FlushStore(null);
    }

    /// <summary>
    /// The on before popup.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnBeforePopup(BeforePopupEventArgs eventArgs)
    {
        BeforePopup?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on frame load start.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnFrameLoadStart(FrameLoadStartEventArgs eventArgs)
    {
        FrameLoadStart?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on load end.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnFrameLoadEnd(FrameLoadEndEventArgs eventArgs)
    {
        FrameLoadEnd?.Invoke(this, eventArgs);

        // Setup window subclass to intercept message for frameless window dragging
        NativeHost.SetupMessageInterceptor(_browserWindowHandle);
    }

    /// <summary>
    /// The on load error.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnLoadError(LoadErrorEventArgs eventArgs)
    {
        LoadError?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on load start.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnLoadStart(FrameLoadStartEventArgs eventArgs)
    {
        FrameLoadStart?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on plugin crashed.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnPluginCrashed(PluginCrashedEventArgs eventArgs)
    {
        PluginCrashed?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// The on render process terminated.
    /// </summary>
    /// <param name="eventArgs">
    /// The event args.
    /// </param>
    public void OnRenderProcessTerminated(RenderProcessTerminatedEventArgs eventArgs)
    {
        RenderProcessTerminated?.Invoke(this, eventArgs);
    }

    #endregion Events Handling
}