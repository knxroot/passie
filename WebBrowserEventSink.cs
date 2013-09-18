//---------------------------------------------------------------------
// <copyright file="WebBrowserEventSink.cs">
//      Copyright 2013 (c) Valeriy Provalov.
// </copyright>
//--------------------------------------------------------------------

using System;
using WebBrowser = SHDocVw.WebBrowser;

namespace PassIE
{
    public class WebBrowserEventSink
    {
        private int pageCounter;
        private int objCounter;
        private bool isRefresh;
        private WebBrowser webBrowser;
        private Action onRefreshCallback;
        private Action onDocumentComplete;

        public WebBrowserEventSink(Action refreshCallback, Action documentCompleteCallback)
        {
            this.onRefreshCallback = refreshCallback;
            this.onDocumentComplete = documentCompleteCallback;
        }

        public void Connect(WebBrowser webBrowser)
        {
            this.webBrowser = webBrowser;
            this.webBrowser.BeforeNavigate2 += this.OnBeforeNavigate2;
            this.webBrowser.DocumentComplete += this.OnDocumentComplete;
            this.webBrowser.DownloadBegin += this.OnDownloadBegin;
            this.webBrowser.DownloadComplete += this.OnDownloadEnd;
        }

        public void Disconnect(WebBrowser webBrowser)
        {
            this.webBrowser.BeforeNavigate2 -= this.OnBeforeNavigate2;
            this.webBrowser.DocumentComplete -= this.OnDocumentComplete;
            this.webBrowser.DownloadBegin -= this.OnDownloadBegin;
            this.webBrowser.DownloadComplete -= this.OnDownloadEnd;
            this.webBrowser = null;
        }

        private void OnBeforeNavigate2(
            object pDisp,
            ref object URL,
            ref object Flags,
            ref object TargetFrameName,
            ref object PostData,
            ref object Headers,
            ref bool Cancel)
        {
            ++this.pageCounter;
        }

        private void OnDownloadBegin()
        {
            if (this.pageCounter == 0)
            {
                this.isRefresh = true;
            }

            ++this.objCounter;
        }

        private void OnDownloadEnd()
        {
            --this.objCounter;

            if (this.objCounter == 0 && this.isRefresh)
            {
                this.onRefreshCallback();

                this.isRefresh = false;
            }
        }

        private void OnDocumentComplete(
            object pDisp,
            ref object url)
        {
            --this.pageCounter;

            this.onDocumentComplete();
        }
    }
}
