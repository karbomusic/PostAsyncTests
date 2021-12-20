using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Web.Http;
using HttpClient = Windows.Web.Http.HttpClient;
using HttpResponseMessage = Windows.Web.Http.HttpResponseMessage;

/// <summary>
/// Title: Task Machine
/// Description: This app tests various tasks with cancellation tokens.
/// Usage: Click "Start" to run two separate mathods as async tasks.
///        Click "Post Async" to test HttpClient.PostAsyn()AsTask()
///        Click "Cancel" to cancel the token for either of the above.
/// </summary>

namespace UWPApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Cancellation token source used by all tests
        System.Threading.CancellationTokenSource cTokenSrc;

        // Timer for UI continuous but doesn't seem to work with the PostAsync test
        DispatcherTimer threadCountTimer = new DispatcherTimer();
       
        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(790, 500);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ThreadCountTextBlock.Text = GetThreadCountString();

            // timer that grabs and displays thread count;
            threadCountTimer.Interval = new System.TimeSpan(0, 0, 1);
            threadCountTimer.Tick += ThreadCountTimer_Tick;
            threadCountTimer.Start();
        }

        /// <summary>
        /// Writes messages to the Output Textbox
        /// </summary>
        /// <param name="info"></param>
        private async void LogInfoMessage(string info)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => {
                OutputTextBox.Text += DateTime.Now.ToLongTimeString() + ": " + info + System.Environment.NewLine;
                ScrollToBottom(OutputTextBox);
            });
        }

        /// <summary>
        /// Autoscroll for output text
        /// </summary>
        /// <param name="textBox"></param>
        private void ScrollToBottom(TextBox textBox)
        {
            var grid = (Grid)VisualTreeHelper.GetChild(textBox, 0);
            for (var i = 0; i <= VisualTreeHelper.GetChildrenCount(grid) - 1; i++)
            {
                object obj = VisualTreeHelper.GetChild(grid, i);
                if (!(obj is ScrollViewer)) continue;
                ((ScrollViewer)obj).ChangeView(0.0f, ((ScrollViewer)obj).ExtentHeight, 1.0f, true);
                break;
            }
        }

        /// <summary>
        /// Updates thread count in UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ThreadCountTimer_Tick(object sender, object e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () => {
                ThreadCountTextBlock.Text = GetThreadCountString();
            });
        }

        /// <summary>
        /// Clear output log
        /// </summary>
        private void ClearOutput()
        {
            OutputTextBox.Text = String.Empty;
        }

        /// <summary>
        /// No idea what I'm doing here.
        /// </summary>
        /// <returns></returns>
        private string GetThreadCountString()
        {
            ThreadPool.GetMaxThreads(out int workerThreadsMax, out int cPortsMax);
            ThreadPool.GetAvailableThreads(out int availableThreads, out int cPorts);
            return (workerThreadsMax - availableThreads + " of " + workerThreadsMax + " threads used.");
        }

        /// <summary>
        /// Starts two tasks on two different threads.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButtons(false);
            ClearOutput();
            cTokenSrc = new System.Threading.CancellationTokenSource();
            LogInfoMessage("Starting jobs... " + GetThreadCountString());
            Task.Run(() => TaskOne(cTokenSrc.Token));
            Task.Run(() => TaskTwo(cTokenSrc.Token));
            LogInfoMessage(GetThreadCountString());
        }

        /// <summary>
        /// Cancels the currently running task(s) in the background
        /// by calling Cancel() on the TokenSource.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (cTokenSrc != null)
            {
                LogInfoMessage("Cancelling jobs... " + GetThreadCountString());
                cTokenSrc.Cancel();
            }
            ToggleButtons(true);
        }

        private void ToggleButtons(bool enabled)
        {
            PostButton.IsEnabled = enabled;
            StartButton.IsEnabled = enabled;
        }

        /// <summary>
        /// Calls TryPostJsonAsync > HttpClient.PostAsync to the .NET core web server.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PostButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleButtons(false);
            ClearOutput();
            LogInfoMessage("Exectuting PostAsync().AsTask()... " + GetThreadCountString());
            Task.Run(() => TryPostJsonAsync());
            LogInfoMessage(GetThreadCountString());
        }

        #region TASKS 
        // ===================================================================================

        /// <summary>
        /// Represents task job 1
        /// </summary>
        /// <param name="token"></param>
        private void TaskOne(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                LogInfoMessage("TaskOne is running..."); 
                System.Threading.Thread.Sleep(1000);
            }

            if (token.IsCancellationRequested)
                LogInfoMessage("TaskOne was cancelled.");
            return;
        }

        /// <summary>
        /// Represents task job 2
        /// </summary>
        /// <param name="token"></param>
        private void TaskTwo(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                LogInfoMessage("TaskTwo is running..."); 
                System.Threading.Thread.Sleep(995);
            }

            if (token.IsCancellationRequested)
                LogInfoMessage("TaskTwo was cancelled."); 
            return;
        }

        /// <summary>
        /// Makes the HTTP PostAsyn() request.
        /// </summary>
        /// <returns></returns>
        private async void TryPostJsonAsync()
        {
            cTokenSrc = new System.Threading.CancellationTokenSource();
      
            try
            {
                // Create the HttpClient and test Uri - in this case the Uri is the .net core web server
                Windows.Web.Http.HttpClient httpClient = new HttpClient();
                Uri uri = new Uri("http://localhost:5000/main");

                // Some data to post
                HttpStringContent content = new HttpStringContent(
                    "Some post data",
                    UnicodeEncoding.Utf8,
                    "application/text");

                // Do the post
                try
                {
                    HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(uri, content).AsTask(cTokenSrc.Token);
                    // Make sure the post succeeded, and write out the response.
                    httpResponseMessage.EnsureSuccessStatusCode();
                    var httpResponseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                    LogInfoMessage("Request completed: " + httpResponseBody + System.Environment.NewLine);
                }
                catch (TaskCanceledException tcEx)
                {
                    LogInfoMessage(tcEx.Message);
                    LogInfoMessage("PostAsync background thread cancelled successfully.");
                }
            }
            catch (Exception ex)
            {
                // Write out any exceptions.
                Debug.WriteLine(ex);
            }
        }
        // ===================================================================================
        #endregion

    }
}
