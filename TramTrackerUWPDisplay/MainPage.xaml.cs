using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TramTrackerUWPDisplay
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public ObservableCollection<TramInfo> Trams = new ObservableCollection<TramInfo>();
        public const int tramStopNo = 3708;
        public MainPage()
        {
            this.InitializeComponent();
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();

            GetStopInformationAsync();
            GetTramsAsync();
        }

        int updateTimer = 0;

        private void DispatcherTimer_Tick(object sender, object e)
        {
            textBlockTime.Text = DateTime.Now.ToString("hh:mm:ss tt");
            updateTimer++;
            if (updateTimer >= 20)
            {
                updateTimer = 0;
                GetTramsAsync();
            }
        }

        private async void GetStopInformationAsync()
        {
            
            var httpClient = new System.Net.Http.HttpClient();
            var stream = await httpClient.GetStreamAsync(String.Format("http://tramtracker.com/Controllers/GetStopInformation.ashx?s={0}", tramStopNo.ToString()));
            var reader = new StreamReader(stream);
            String jsonString = reader.ReadToEnd();

            var rootObject = JsonObject.Parse(jsonString);
            var responseObject = rootObject.GetNamedObject("ResponseObject");

            String stopName = responseObject.GetNamedString("StopName");
            String cityDirection = responseObject.GetNamedString("CityDirection");
            String flagStopNo = responseObject.GetNamedString("FlagStopNo");

            StopNameTextBlock.Text = flagStopNo + " - " + stopName;
            CityDirectionTextBlock.Text = cityDirection;
        }

        private async void GetTramsAsync()
        {
            Collection<TramInfo> unsortedTrams = new Collection<TramInfo>();
            Trams.Clear();
            var httpClient = new System.Net.Http.HttpClient();
            var stream = await httpClient.GetStreamAsync(String.Format("http://tramtracker.com/Controllers/GetNextPredictionsForStop.ashx?stopNo={0}&routeNo=0&isLowFloor=false", tramStopNo.ToString()));
            var reader = new StreamReader(stream);
            String jsonString = reader.ReadToEnd();

            var rootObject = JsonObject.Parse(jsonString);
            var trams = rootObject.GetNamedArray("responseObject");

            foreach (JsonValue jsonTram in trams)
            {
                JsonObject jsonTramObject = jsonTram.GetObject();
                String routeNumber = jsonTramObject.GetNamedString("RouteNo");
                String destination = jsonTramObject.GetNamedString("Destination");

                String dateTimeString = jsonTramObject.GetNamedString("PredictedArrivalDateTime");

                Regex regex = new Regex(@"^/Date\(([\d]+).*\)/$");
                Match regexMatch = regex.Match(dateTimeString);

                DateTime tramArrivalTime = new DateTime(1970, 1, 1, 0, 0, 0);

                if (regexMatch.Success)
                {
                    if (regexMatch.Groups.Count > 1)
                    {
                        long epochSeconds = Convert.ToInt64(regexMatch.Groups[1].Value);
                        tramArrivalTime = tramArrivalTime.AddMilliseconds(epochSeconds).ToLocalTime();
                    }
                }

                TramInfo tramInfo = new TramInfo(routeNumber, destination, tramArrivalTime);
                
                if (tramInfo.ArrivalTime.Subtract(DateTime.Now).TotalMinutes <= 120)
                {
                    unsortedTrams.Add(tramInfo);
                }
            }

            var tramlist = from tram in unsortedTrams
                    orderby tram.ArrivalTime
                    select tram;

            Trams = new ObservableCollection<TramInfo>(tramlist);

            tramsListView.ItemsSource = Trams;
            //tramsCollectionViewSource.Source = Trams;
        } 
    }
}
