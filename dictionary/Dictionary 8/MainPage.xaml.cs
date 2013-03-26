using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Dictionary_8
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class MainPage : Dictionary_8.Common.LayoutAwarePage
    {
        Dictionary<string, string> dc;
        Dictionary<string, string> dc2;
        int _words;

        public MainPage()
        {
            this.InitializeComponent();
            SettingsPane.GetForCurrentView().CommandsRequested += MainPage_CommandsRequested;
            dc = new Dictionary<string, string>();
            dc2 = new Dictionary<string, string>();
            _words = 0;
            button.IsEnabled = false;
            LoadData();
            LoadKeys();
            button.IsEnabled = true;
        }

        void MainPage_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            bool afound = false;
            bool sfound = false;
            bool pfound = false;
            foreach (var command in args.Request.ApplicationCommands.ToList())
            {
                if (command.Label == "About")
                {
                    afound = true;
                }
                if (command.Label == "Settings")
                {
                    sfound = true;
                }
                if (command.Label == "Policy")
                {
                    pfound = true;
                }
            }
            if (!afound)
                args.Request.ApplicationCommands.Add(new SettingsCommand("s", "About", (p) => { cfoAbout.IsOpen = true; }));
            //if (!sfound)
            //    args.Request.ApplicationCommands.Add(new SettingsCommand("s", "Settings", (p) => { cfoSettings.IsOpen = true; }));
            //if (!pfound)
            //    args.Request.ApplicationCommands.Add(new SettingsCommand("s", "Policy", (p) => { cfoPolicy.IsOpen = true; }));
            args.Request.ApplicationCommands.Add(new SettingsCommand("privacypolicy", "Privacy policy", OpenPrivacyPolicy));

        }

        private async void OpenPrivacyPolicy(IUICommand command)
        {
            var uri = new Uri("http://www.thatslink.com/privacy-statment/ ");
            await Launcher.LaunchUriAsync(uri);
        }

        private async void LoadData()
        {
            var line = new StringBuilder();
            var word = new StringBuilder();
            var def = new StringBuilder();

            StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("words.dat");
            
            Stream stream = await file.OpenStreamForReadAsync();
            var sr = new StreamReader(stream, Encoding.UTF8);

            line.Append(await sr.ReadLineAsync());
            while (!line.ToString().Equals("EOF"))
            {
                word.Clear();
                word.Append(line.ToString());
                def.Clear();
                def.Append(System.Environment.NewLine);
                bool eq = false;
                while (!eq)
                {
                    line.Clear();
                    line.Append(await sr.ReadLineAsync());
                    if (line.ToString().Equals("")) { eq = false; def.Append(System.Environment.NewLine + System.Environment.NewLine); }
                    else if (line.ToString().Equals(line.ToString().ToUpper())) eq = true;
                    else def.Append(line.ToString());
                }

                try
                {
                    _words++;
                    dc.Add(word.ToString(), def.ToString());
                }
                catch (ArgumentException)
                {
                    //MessageBox.Show(word+def);
                    dc[word.ToString()] = dc[word.ToString()] + System.Environment.NewLine + System.Environment.NewLine + def.ToString();
                }

            }
            sr.DiscardBufferedData();
            sr.Dispose();
            stream.Dispose();

            //txt_status.Text = "Loading done.";
        }

        private async void LoadKeys()
        {
            StorageFile file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("keys.dat");
            Stream stream = await file.OpenStreamForReadAsync();
            var sr = new StreamReader(stream);

            var line = new StringBuilder();

            line.Append(await sr.ReadLineAsync());
            while (!line.ToString().Equals("EOF"))
            {
                string[] words = line.ToString().Split(';');
                foreach (string word in words)
                {
                    try
                    {
                        dc2.Add(word.Trim(), line.ToString());
                    }
                    catch (Exception ex)
                    {
                    }
                }
                line.Clear();
                line.Append(await sr.ReadLineAsync());
            }
            sr.DiscardBufferedData();
            sr.Dispose();
            stream.Dispose();
        }

        private string Search(string word)
        {
            try
            {
                return System.Environment.NewLine + dc[word.ToUpper()];
            }
            catch (KeyNotFoundException)
            {
                try
                {
                    return System.Environment.NewLine + dc[dc2[word.ToUpper()]];
                }
                catch (KeyNotFoundException)
                {
                    return "Not Found.";
                }
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Restore values stored in session data.
            if (pageState != null && pageState.ContainsKey("word") && pageState.ContainsKey("defn"))
            {
                txt_word.Text = pageState["word"].ToString();
                txt_def.Text = pageState["defn"].ToString();
            }

            // Restore values stored in app data.
            Windows.Storage.ApplicationDataContainer roamingSettings =Windows.Storage.ApplicationData.Current.RoamingSettings;
            if (roamingSettings.Values.ContainsKey("search"))
            {
                tb_search.Text = roamingSettings.Values["search"].ToString();
                Suggest(tb_search.Text);
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            pageState["word"] = txt_word.Text;
            pageState["defn"] = txt_def.Text; 
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            txt_def.Text = Search(tb_search.Text);
            txt_word.Text = tb_search.Text.ToUpper();
        }

        private void Suggest(string word)
        {
            lv_sugg.Items.Clear();

            var sugg = dc.Where(x => x.Key.IndexOf(word.ToUpper(), System.StringComparison.Ordinal) == 0).ToList();

            for (int j = 0; j < 9; j++)
            {
                try
                {
                    if (sugg.ElementAt(j).Key.IndexOf(';') > -1)
                    {
                        string[] words = sugg.ElementAt(j).Key.Split(';');
                        foreach (string w in words)
                        {
                            //txt_sugg.Text += System.Environment.NewLine + w.Trim();
                            lv_sugg.Items.Add(Convert.ToString(System.Environment.NewLine + w.Trim()));
                        }
                    }
                    else
                    {
                        //txt_sugg.Text += System.Environment.NewLine + sugg.ElementAt(j).Key;
                        lv_sugg.Items.Add(Convert.ToString(System.Environment.NewLine + sugg.ElementAt(j).Key));
                    }
                }
                catch (Exception)
                {
                    break;
                }
            }
        }

        private void tb_search_TextChanged(object sender, TextChangedEventArgs e)
        {
            Suggest(tb_search.Text);

            //Saving Application Data
            Windows.Storage.ApplicationDataContainer roamingSettings =Windows.Storage.ApplicationData.Current.RoamingSettings;
            roamingSettings.Values["search"] = tb_search.Text;
        }

        private void lv_sugg_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                var item = (string)lv_sugg.SelectedItem;
                if (item != null)
                {
                    txt_def.Text = Search(item.Trim());
                    txt_word.Text = item.Trim();
                }
            }
            catch (Exception)
            {
            }
            
        }

        private void tb_search_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                txt_def.Text = Search(tb_search.Text);
                txt_word.Text = tb_search.Text.ToUpper();
            }
        }
    }
}
