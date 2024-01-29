using System;
using System.Collections.Generic;
using System.Text;

namespace Chromely.Browser.Handlers
{
    public class DefaultFocusHandler : CefFocusHandler
    {
        protected override void OnTakeFocus(CefBrowser browser, bool next)
        {
            base.OnTakeFocus(browser, next);
        }

        protected override bool OnSetFocus(CefBrowser browser, CefFocusSource source)
        {
            return base.OnSetFocus(browser, source);
        }

        protected override void OnGotFocus(CefBrowser browser)
        {
            base.OnGotFocus(browser);
        }
    }
}
