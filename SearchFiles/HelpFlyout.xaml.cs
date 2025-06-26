using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
