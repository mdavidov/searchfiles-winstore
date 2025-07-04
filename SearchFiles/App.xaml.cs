﻿using SearchFiles.Common;

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Windows.Foundation;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Split App template is documented at http://go.microsoft.com/fwlink/?LinkId=234228

namespace SearchFiles
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton Application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            try
            {
                this.InitializeComponent();
                this.Suspending += OnSuspending;
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            try
            {
                //#if DEBUG
                //    if (System.Diagnostics.Debugger.IsAttached)
                //    {
                //        this.DebugSettings.EnableFrameRateCounter = true;
                //    }
                //#endif

                Frame? rootFrame = Window.Current.Content as Frame;

                // Do not repeat app initialization when the Window already has content,
                // just ensure that the window is active

                if (rootFrame == null)
                {
                    // Create a Frame to act as the navigation context and navigate to the first page
                    rootFrame = new Frame();
                    //Associate the frame with a SuspensionManager key                                
                    SuspensionManager.RegisterFrame(rootFrame, "AppFrame");
                    // Set the default language
                    rootFrame.Language = Windows.Globalization.ApplicationLanguages.Languages[0];

                    rootFrame.NavigationFailed += OnNavigationFailed;

                    if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                    {
                        // Restore the saved session state only when appropriate
                        try
                        {
                            await SuspensionManager.RestoreAsync();
                        }
                        catch (SuspensionManagerException)
                        {
                            //Something went wrong restoring state.
                            //Assume there is no state and continue
                        }
                    }

                    // Place the frame in the current Window
                    Window.Current.Content = rootFrame;
                }
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(ItemsPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            //throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
            Debug.WriteLine("### Navigation failed.");
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            try
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await SuspensionManager.SaveAsync();
                deferral.Complete();
            }
            catch (Exception) { }
        }

        async private Task EnsureMainPageActivatedAsync(IActivatedEventArgs args)
        {
            try
            {
                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // Do an asynchronous restore
                    await SuspensionManager.RestoreAsync();
                }

                if (Window.Current.Content == null)
                {
                    var rootFrame = new Frame();
                    rootFrame.Navigate(typeof(ItemsPage));
                    Window.Current.Content = rootFrame;
                }

                Window.Current.Activate();
            }
            catch (Exception) { }
        }

        /// <summary>
        /// This is the handler for Search activation.
        /// </summary>
        /// <param name="args">This is the list of arguments for search activation, including QueryText and Language</param>
        protected async override void OnSearchActivated(SearchActivatedEventArgs args)
        {
            try
            {
                await EnsureMainPageActivatedAsync(args);
                if (args.QueryText == "")
                {
                    // navigate to landing page.
                }
                else
                {
                    if (ItemsPage.Current != null)
                    {
                        ItemsPage.Current.SetSearchWordBox(args.QueryText);
                        await ItemsPage.Current.PerformSearch(args.QueryText);
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        private async void OnQuerySubmitted(object sender, SearchPaneQuerySubmittedEventArgs args)
        {
            try
            {
                if (ItemsPage.Current != null)
                {
                    ItemsPage.Current.SetSearchWordBox(args.QueryText);
                    await ItemsPage.Current.PerformSearch(args.QueryText);
                }
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

        protected override void OnWindowCreated(WindowCreatedEventArgs args)
        {
            try
            {
                // Register QuerySubmitted handler for the window at window creation time and only registered once
                // so that the app can receive user queries at any time.
                if (SearchPane.GetForCurrentView() != null)
                    SearchPane.GetForCurrentView().QuerySubmitted += new TypedEventHandler<SearchPane, SearchPaneQuerySubmittedEventArgs>(OnQuerySubmitted);
                else
                    { Debug.Assert(false); }
            }
            catch (Exception)
            {
                try
                {
                    // on exception simply try again, it will probably work
                    if (SearchPane.GetForCurrentView() != null)
                        SearchPane.GetForCurrentView().QuerySubmitted += new TypedEventHandler<SearchPane, SearchPaneQuerySubmittedEventArgs>(OnQuerySubmitted);
                    else
                        { Debug.Assert(false); }
                }
                catch (Exception ex2) { Debug.WriteLine(ex2.ToString()); }
            }

            try
            {
                // All commands from the Windows Settings charm should come through here: About, App Settings, Privacy Policy, etc.
                if (SettingsPane.GetForCurrentView() != null)
                    SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
                else
                    { Debug.Assert(false); }
            }
            catch (Exception)
            {
                try
                {
                    // on exception simply try again, it will probably work
                    if (SettingsPane.GetForCurrentView() != null)
                        SettingsPane.GetForCurrentView().CommandsRequested += OnCommandsRequested;
                    else
                        { Debug.Assert(false); }
                }
                catch (Exception ex2) { Debug.WriteLine(ex2.ToString()); }
            }
        }

        private void OnCommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            try
            {
                if (ItemsPage.Current != null)
                    ItemsPage.Current.SettingsCharm_CommandsRequested(sender, args);
            }
            catch (Exception ex) { Debug.WriteLine(ex.ToString()); }
        }

    }
}
