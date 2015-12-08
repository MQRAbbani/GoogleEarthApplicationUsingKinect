using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    class GEController
    {
        WebBrowser webBrowser = null;
        public GEController(WebBrowser browserInstance)
        {
            this.webBrowser = browserInstance;
        }
        public void zoomIn()
        {
            webBrowser.Document.InvokeScript("zoomIn");
        }
        public void zoomOut()
        {
            webBrowser.Document.InvokeScript("zoomOut");
        }
        public void panLeft()
        {
            webBrowser.Document.InvokeScript("panLeft");
        }
        public void panRight()
        {
            webBrowser.Document.InvokeScript("panRight");
        }


    }
}
