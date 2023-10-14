﻿// Copyright © 2017 Chromely Projects. All rights reserved.
// Use of this source code is governed by MIT license that can be found in the LICENSE file.

namespace Chromely.Browser;

/// <summary>
/// Default implementation of <see cref="DefaultDragHandler"/>.
/// </summary>
public class DefaultDragHandler : CefDragHandler
{
    protected static readonly object objLock = new object();
    protected readonly IChromelyConfiguration _config;

    /// <summary>
    /// Initializes a new instance of <see cref="DefaultDragHandler"/>.
    /// </summary>
    /// <param name="config">Instance of <see cref="IChromelyConfiguration"/>.</param>
    public DefaultDragHandler(IChromelyConfiguration config)
    {
        _config = config;
    }

    /// <inheritdoc/>
    protected override bool OnDragEnter(CefBrowser browser, CefDragData dragData, CefDragOperationsMask mask)
    {
        return false;
    }

    /*
        <html>
        <head>
            <title>Draggable Regions Test</title>
            <style>
                .titlebar {
                    -webkit-app-region: drag;
                    -webkit-user-select: none;
                    position: absolute;
                    top: 0px;
                    left: 50px;
                    width: 100%;
                    height: 32px;
                }

                .titlebar-button {
                    -webkit-app-region: no-drag;
                    position: absolute;
                    top: 0px;
                    width: 140px;
                    height: 32px;
                }
            </style>
        </head>
        <body bgcolor="white">
            Draggable regions can be defined using the -webkit-app-region CSS property.
            <br />In the below example the red region is draggable and the blue sub-region is non-draggable.
            <br />Windows can be resized by default and closed using JavaScript <a href="#" onClick="window.close(); return false;">window.close()</a>.
            <div class="titlebar">
                <div class="titlebar-button"></div>
            </div>
        </body>
        </html>
     */
    /// <inheritdoc/>
    protected override void OnDraggableRegionsChanged(CefBrowser browser, CefFrame frame, CefDraggableRegion[] regions)
    {
        var framelessOption = _config?.WindowOptions?.FramelessOption;
        if (framelessOption is null || !framelessOption.UseWebkitAppRegions)
        {
            return;
        }

        if (!browser.IsPopup)
        {
            lock (objLock)
            {
                framelessOption.IsDraggable = (nativeHost, point) =>
                {
                    var scale = nativeHost.GetWindowDpiScale();
                    var hitNoDrag = regions.Any(r => !r.Draggable && ContainsPoint(r, point, scale));
                    if (hitNoDrag)
                    {
                        return false;
                    }

                    return regions.Any(r => r.Draggable && ContainsPoint(r, point, scale));
                };
            }
        }
    }

    private static bool ContainsPoint(CefDraggableRegion region, Point point, float scale)
    {
        point.X = (int)Math.Round(point.X / scale);
        point.Y = (int)Math.Round(point.Y / scale);

        return point.X >= region.Bounds.X && point.X <= (region.Bounds.X + region.Bounds.Width)
            && point.Y >= region.Bounds.Y && point.Y <= (region.Bounds.Y + region.Bounds.Height);
    }
}