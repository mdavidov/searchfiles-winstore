﻿using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Settings Flyout item template is documented at http://go.microsoft.com/fwlink/?LinkId=273769

namespace SearchFiles
{
    public sealed partial class AboutFlyout : SettingsFlyout
    {
        ItemsPage rootPage = ItemsPage.Current;

        public AboutFlyout()
        {
            this.InitializeComponent();

            if (rootPage != null)
            {
                m_AppName.Text = rootPage.GetAppTitle();
                m_upgradeBtn.Visibility = (rootPage.IsTrial() || !rootPage.IsPro()) ? Visibility.Visible : Visibility.Collapsed;
                m_upgradeTxt.Visibility = (rootPage.IsTrial() || !rootPage.IsPro()) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private async void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (rootPage != null)
                {
                    //rootPage.EnsureUnsnapped();
                    await rootPage.BuyAppAsync();
                }
            }
            catch (Exception ex) { Debug.WriteLine("### " + ex.ToString()); }
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                HyperlinkButton linkBtn = (HyperlinkButton)sender;
                if (linkBtn != null && linkBtn.Tag != null)
                {
                    Uri uri = new Uri((string)linkBtn.Tag);
                    await Windows.System.Launcher.LaunchUriAsync(uri);
                }
            }
            catch (Exception ex) { Debug.WriteLine("### " + ex.ToString()); }
        }

        private void DevelopedByBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                m_AuthorPanel.Visibility = (m_AuthorPanel.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex) { Debug.WriteLine("### " + ex.ToString()); }
        }
    }
}
