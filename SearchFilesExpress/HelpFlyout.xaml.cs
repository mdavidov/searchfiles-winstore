using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace SearchFiles
{
    public sealed partial class HelpFlyout : SettingsFlyout
    {
        public HelpFlyout()
        {
            this.InitializeComponent();
        }

        private void DevelopedByBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (m_AuthorPanel != null)
                    m_AuthorPanel.Visibility = (m_AuthorPanel.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex) { Debug.WriteLine("### " + ex.ToString()); }
        }
    }
}
