using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Nulah.Everythinger.Plugins.Tasks.Models;

namespace Nulah.Everythinger.Plugins.Tasks.Controls
{
    /// <summary>
    /// Interaction logic for TaskViewControl.xaml
    /// </summary>
    public partial class TaskViewControl : UserControl
    {
        public TaskViewControl()
        {
            InitializeComponent();
        }
    }

    public class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
                "Html",
                typeof(string),
                typeof(BrowserBehavior),
                new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser webBrowser = dependencyObject as WebBrowser;
            if (webBrowser != null)
            {
                var content = $"<html><style>html,body{{padding:0px;margin:0px;border:1px solid #f0f;}}</style><body>{e.NewValue as string ?? string.Empty}</body></html>";
                webBrowser.NavigateToString(content);
            }
        }
    }
}
