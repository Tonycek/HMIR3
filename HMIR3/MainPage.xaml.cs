using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Media.Capture;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Graphics.Display;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.MediaProperties;
using Windows.Phone.UI.Input;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Core;

// Required to support the core dispatcher and the accelerometer

using Windows.Devices.Sensors;
using Windows.UI;
using System.Collections.ObjectModel;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

//<Line x:Name="xLine" X1="240" Y1="350" X2="340" Y2="350" Stroke="Red" StrokeThickness="4"></Line>
//    <Line x:Name="yLine" X1="240" Y1="350" X2="240" Y2="270" Stroke="Green" StrokeThickness="4"></Line>
//    <Line x:Name="zLine" X1="240" Y1="350" X2="190" Y2="400" Stroke="Blue" StrokeThickness="4"></Line>

namespace HMIR3
{
    
    public class Uhol:WinRTXamlToolkit.Controls.DataVisualization.Charting.Series
    {
        public ObservableCollection<object> LegendItems
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public ISeriesHost SeriesHost
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public string title { get; set; }
        public double value { get; set; }
    } 

    public sealed partial class MainPage : Page
    {
        private Accelerometer _accelerometer;
        private LightSensor _lightsensor;
        private Compass _compass;
        private OrientationSensor _sensor;
        private Inclinometer _inclinometer;
        private Magnetometer _magnetometer;
        private List<Uhol> listUhlov = new List<Uhol>();

        private MediaCapture _mediaCapture;
        private bool _isPreviewing;
        private DisplayRequest _displayRequest;


        private async void ReadingChanged(object sender, MagnetometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                //MagnetometerReading reading = e.Reading;
                //magnetometerX.Text = String.Format("{0,5:0.00}", reading.MagneticFieldX);
                //magnetometerY.Text = String.Format("{0,5:0.00}", reading.MagneticFieldY);
                //magnetometerZ.Text = String.Format("{0,5:0.00}", reading.MagneticFieldZ);

                //xLine.X2 = xLine.X1 + reading.AccelerationX * 100;
                //yLine.Y2 = yLine.Y1 - reading.AccelerationY * 100;
                //zLine.X2 = zLine.X1 - reading.AccelerationZ * 65;
                //zLine.Y2 = zLine.Y1 + reading.AccelerationZ * 65;
            });
        }

        // This event handler writes the current accelerometer reading to
        // the three acceleration text blocks on the app' s main page.
        private async void ReadingChanged(object sender, AccelerometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                AccelerometerReading reading = e.Reading;
                txtXAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationX);
                txtYAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationY);
                txtZAxis.Text = String.Format("{0,5:0.00}", reading.AccelerationZ);

                xLine.X2 = xLine.X1 + reading.AccelerationX * 100;
                yLine.Y2 = yLine.Y1 - reading.AccelerationY * 100;
                zLine.X2 = zLine.X1 - reading.AccelerationZ * 65;
                zLine.Y2 = zLine.Y1 + reading.AccelerationZ * 65;
            });
        }

        private Color rectangle_Tap()
        {
            // Change this colour to whatever colour you want.
            return Color.FromArgb(255, 255, 255, 0);
        }

        private async void ReadingChanged(object sender, LightSensorReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                LightSensorReading reading = e.Reading;
                txtLuxValue.Text = String.Format("{0,5:0.00}", reading.IlluminanceInLux);
                double farba = reading.IlluminanceInLux/3000;
                farba = farba * 255; 

                if (farba > 255) {
                    mon9a.Background = new SolidColorBrush(Colors.Red);
                    mon9a.Content = "Too High";
                }

                else { 
                    mon9a.Background = new SolidColorBrush(Color.FromArgb((byte)farba, 0, (byte)farba, 0));
                    mon9a.Content = "";
                }
            });
        }

        private async void ReadingChanged(object sender, CompassReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CompassReading reading = e.Reading;
                txtMagnetic.Text = String.Format("{0,5:0.00}", reading.HeadingMagneticNorth);
                if (reading.HeadingTrueNorth.HasValue)
                    txtNorth.Text = String.Format("{0,5:0.00}", reading.HeadingTrueNorth);
                else
                    txtNorth.Text = "No reading.";

                if (reading.HeadingMagneticNorth > 337.5 || reading.HeadingMagneticNorth < 22.5)
                {
                    Line1C.Stroke = new SolidColorBrush(Colors.Red);
                    Line15C.Stroke = new SolidColorBrush(Colors.Red);
                    Line2C.Stroke = new SolidColorBrush(Colors.Red);
                    Line25C.Stroke = new SolidColorBrush(Colors.Red);
                    Line3C.Stroke = new SolidColorBrush(Colors.Red);
                    Line35C.Stroke = new SolidColorBrush(Colors.Red);
                    Line4C.Stroke = new SolidColorBrush(Colors.Green);
                    Line45C.Stroke = new SolidColorBrush(Colors.Red);
                }

                else if (reading.HeadingMagneticNorth > 22.5 && reading.HeadingMagneticNorth < 67.5)
                {
                    Line1C.Stroke = new SolidColorBrush(Colors.Red);
                    Line15C.Stroke = new SolidColorBrush(Colors.Red);
                    Line2C.Stroke = new SolidColorBrush(Colors.Red);
                    Line25C.Stroke = new SolidColorBrush(Colors.Red);
                    Line3C.Stroke = new SolidColorBrush(Colors.Red);
                    Line35C.Stroke = new SolidColorBrush(Colors.Green);
                    Line4C.Stroke = new SolidColorBrush(Colors.Red);
                    Line45C.Stroke = new SolidColorBrush(Colors.Red);
                }

                else if (reading.HeadingMagneticNorth > 67.5 && reading.HeadingMagneticNorth < 112.5)
                {
                    Line1C.Stroke = new SolidColorBrush(Colors.Red);
                    Line15C.Stroke = new SolidColorBrush(Colors.Red);
                    Line2C.Stroke = new SolidColorBrush(Colors.Red);
                    Line25C.Stroke = new SolidColorBrush(Colors.Red);
                    Line3C.Stroke = new SolidColorBrush(Colors.Green);
                    Line35C.Stroke = new SolidColorBrush(Colors.Red);
                    Line4C.Stroke = new SolidColorBrush(Colors.Red);
                    Line45C.Stroke = new SolidColorBrush(Colors.Red);
                }

                else if (reading.HeadingMagneticNorth > 112.5 && reading.HeadingMagneticNorth < 157.5)
                {
                    Line1C.Stroke = new SolidColorBrush(Colors.Red);
                    Line15C.Stroke = new SolidColorBrush(Colors.Red);
                    Line2C.Stroke = new SolidColorBrush(Colors.Red);
                    Line25C.Stroke = new SolidColorBrush(Colors.Green);
                    Line3C.Stroke = new SolidColorBrush(Colors.Red);
                    Line35C.Stroke = new SolidColorBrush(Colors.Red);
                    Line4C.Stroke = new SolidColorBrush(Colors.Red);
                    Line45C.Stroke = new SolidColorBrush(Colors.Red);
                }

                else if (reading.HeadingMagneticNorth > 157.5 && reading.HeadingMagneticNorth < 202.5)
                {
                    Line1C.Stroke = new SolidColorBrush(Colors.Red);
                    Line15C.Stroke = new SolidColorBrush(Colors.Red);
                    Line2C.Stroke = new SolidColorBrush(Colors.Green);
                    Line25C.Stroke = new SolidColorBrush(Colors.Red);
                    Line3C.Stroke = new SolidColorBrush(Colors.Red);
                    Line35C.Stroke = new SolidColorBrush(Colors.Red);
                    Line4C.Stroke = new SolidColorBrush(Colors.Red);
                    Line45C.Stroke = new SolidColorBrush(Colors.Red);
                }

                else if (reading.HeadingMagneticNorth > 202.5 && reading.HeadingMagneticNorth < 247.5)
                {
                    Line1C.Stroke = new SolidColorBrush(Colors.Red);
                    Line15C.Stroke = new SolidColorBrush(Colors.Green);
                    Line2C.Stroke = new SolidColorBrush(Colors.Red);
                    Line25C.Stroke = new SolidColorBrush(Colors.Red);
                    Line3C.Stroke = new SolidColorBrush(Colors.Red);
                    Line35C.Stroke = new SolidColorBrush(Colors.Red);
                    Line4C.Stroke = new SolidColorBrush(Colors.Red);
                    Line45C.Stroke = new SolidColorBrush(Colors.Red);
                }

                else if (reading.HeadingMagneticNorth > 247.5 && reading.HeadingMagneticNorth < 292.5)
                {
                    Line1C.Stroke = new SolidColorBrush(Colors.Green);
                    Line15C.Stroke = new SolidColorBrush(Colors.Red);
                    Line2C.Stroke = new SolidColorBrush(Colors.Red);
                    Line25C.Stroke = new SolidColorBrush(Colors.Red);
                    Line3C.Stroke = new SolidColorBrush(Colors.Red);
                    Line35C.Stroke = new SolidColorBrush(Colors.Red);
                    Line4C.Stroke = new SolidColorBrush(Colors.Red);
                    Line45C.Stroke = new SolidColorBrush(Colors.Red);
                }

                else if (reading.HeadingMagneticNorth > 292.5 && reading.HeadingMagneticNorth < 337.5)
                {
                    Line1C.Stroke = new SolidColorBrush(Colors.Red);
                    Line15C.Stroke = new SolidColorBrush(Colors.Red);
                    Line2C.Stroke = new SolidColorBrush(Colors.Red);
                    Line25C.Stroke = new SolidColorBrush(Colors.Red);
                    Line3C.Stroke = new SolidColorBrush(Colors.Red);
                    Line35C.Stroke = new SolidColorBrush(Colors.Red);
                    Line4C.Stroke = new SolidColorBrush(Colors.Red);
                    Line45C.Stroke = new SolidColorBrush(Colors.Green);
                }
            });
        }

        private async void ReadingChanged(object sender, OrientationSensorReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                OrientationSensorReading reading = e.Reading;

                // Quaternion values
                //txtQuaternionX.Text = String.Format("{0,8:0.00000}", reading.Quaternion.X);
                //txtQuaternionY.Text = String.Format("{0,8:0.00000}", reading.Quaternion.Y);
                //txtQuaternionZ.Text = String.Format("{0,8:0.00000}", reading.Quaternion.Z);
                //txtQuaternionW.Text = String.Format("{0,8:0.00000}", reading.Quaternion.W);

                //// Rotation Matrix values
                //txtM11.Text = String.Format("{0,8:0.00000}", reading.RotationMatrix.M11);
                //txtM12.Text = String.Format("{0,8:0.00000}", reading.RotationMatrix.M12);
                //txtM13.Text = String.Format("{0,8:0.00000}", reading.RotationMatrix.M13);
                //txtM21.Text = String.Format("{0,8:0.00000}", reading.RotationMatrix.M21);
                //txtM22.Text = String.Format("{0,8:0.00000}", reading.RotationMatrix.M22);
                //txtM23.Text = String.Format("{0,8:0.00000}", reading.RotationMatrix.M23);
                //txtM31.Text = String.Format("{0,8:0.00000}", reading.RotationMatrix.M31);
                //txtM32.Text = String.Format("{0,8:0.00000}", reading.RotationMatrix.M32);
                //txtM33.Text = String.Format("{0,8:0.00000}", reading.RotationMatrix.M33);
            });
        }

        private async void ReadingChanged(object sender, InclinometerReadingChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                

                InclinometerReading reading = e.Reading;
                txtPitch.Text = String.Format("{0,5:0.00}", reading.PitchDegrees);
                txtRoll.Text = String.Format("{0,5:0.00}", reading.RollDegrees);
                txtYaw.Text = String.Format("{0,5:0.00}", reading.YawDegrees);

             //   listUhlov.Where(w => w.title == "Pitch").ToList().TrueForAll(w.value == 30);
                //listUhlov.Where(d => d.title == "Pitch").First().value = reading.PitchDegrees;
                //listUhlov.Where(d => d.title == "Roll").First().value = reading.RollDegrees;
                //listUhlov.Where(d => d.title == "Yaw").First().value = reading.YawDegrees;

                //listUhlov.Add(new Uhol { title = "Pitch", value = reading.PitchDegrees });
                //listUhlov.Add(new Uhol { title = "Roll", value = reading.RollDegrees });
                //listUhlov.Add(new Uhol { title = "Yaw", value = reading.YawDegrees });
                //        gyroskopeItemsSource.Refresh();
            });
        }

        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        private async Task SetPreviewRotationAsync()
        {
            // Only need to update the orientation if the camera is mounted on the device
            // Add rotation metadata to the preview stream to make sure the aspect ratio / dimensions match when rendering and getting preview frames
            var props = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
            props.Properties.Add(RotationKey, 0);
            await _mediaCapture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, props, null);
        }

        private async Task StartPreviewAsync()
        {
            try
            {

                _mediaCapture = new MediaCapture();
                await _mediaCapture.InitializeAsync();

                PreviewControl.Source = _mediaCapture;
                await _mediaCapture.StartPreviewAsync();
                _isPreviewing = true;

                _displayRequest.RequestActive();
                //       _mediaCapture.SetPreviewRotation(VideoRotation.Clockwise180Degrees);


                DisplayInformation.AutoRotationPreferences = DisplayOrientations.LandscapeFlipped;

                if (_isPreviewing)
                {
                    await SetPreviewRotationAsync();
                }
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                System.Diagnostics.Debug.WriteLine("The app was denied access to the camera");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MediaCapture initialization failed. {0}", ex.Message);
            }
        }

        private async Task CleanupCameraAsync()
        {
            if (_mediaCapture != null)
            {
                if (_isPreviewing)
                {
                    await _mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PreviewControl.Source = null;
                    if (_displayRequest != null)
                    {
                        _displayRequest.RequestRelease();
                    }

                    _mediaCapture.Dispose();
                    _mediaCapture = null;
                });
            }

        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await CleanupCameraAsync();
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();

            _accelerometer = Accelerometer.GetDefault();
            _lightsensor = LightSensor.GetDefault();
            _compass = Compass.GetDefault();
            _sensor = OrientationSensor.GetDefault();
            _inclinometer = Inclinometer.GetDefault();
            _magnetometer = Magnetometer.GetDefault();

            Application.Current.Suspending += Application_Suspending;

            if (_magnetometer != null)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _magnetometer.ReportInterval = reportInterval;

                // Assign an event handler for the reading-changed event
                _magnetometer.ReadingChanged += new TypedEventHandler<Magnetometer, MagnetometerReadingChangedEventArgs>(ReadingChanged);
            }

            if (_accelerometer != null)
            {
                // Establish the report interval
                uint minReportInterval = _accelerometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _accelerometer.ReportInterval = reportInterval;

                // Assign an event handler for the reading-changed event
                _accelerometer.ReadingChanged += new TypedEventHandler<Accelerometer, AccelerometerReadingChangedEventArgs>(ReadingChanged);
            }

            if (_lightsensor != null)
            {
                // Establish the report interval for all scenarios
                uint minReportInterval = _lightsensor.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _lightsensor.ReportInterval = reportInterval;

                // Establish the even thandler
                _lightsensor.ReadingChanged += new TypedEventHandler<LightSensor, LightSensorReadingChangedEventArgs>(ReadingChanged);
            }

            if (_compass != null)
            {
                // Establish the report interval for all scenarios
                uint minReportInterval = _compass.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _compass.ReportInterval = reportInterval;
                _compass.ReadingChanged += new TypedEventHandler<Compass, CompassReadingChangedEventArgs>(ReadingChanged);
            }


            if(_sensor != null)
            {
                uint minReportInterval = _sensor.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _sensor.ReportInterval = reportInterval;

                // Establish event handler
                _sensor.ReadingChanged += new TypedEventHandler<OrientationSensor, OrientationSensorReadingChangedEventArgs>(ReadingChanged);
            }

            if (_inclinometer != null)
            {
                // Establish the report interval for all scenarios
                uint minReportInterval = _inclinometer.MinimumReportInterval;
                uint reportInterval = minReportInterval > 16 ? minReportInterval : 16;
                _inclinometer.ReportInterval = reportInterval;

                // Establish the event handler
                _inclinometer.ReadingChanged += new TypedEventHandler<Inclinometer, InclinometerReadingChangedEventArgs>(ReadingChanged);
            }

            listUhlov.Add(new Uhol { title = "Pitch", value = 0 });
            listUhlov.Add(new Uhol { title = "Roll", value = 0 });
            listUhlov.Add(new Uhol { title = "Yaw", value = 0 });

            //listUhlov.Add(new Uhol { title = "Tono", value = 20 });
            //listUhlov.Add(new Uhol { title = "Tomas", value = 30 });
            //listUhlov.Add(new Uhol { title = "Ivan", value = 15 });

            //gyroskopeItemsSource.ItemsSource = listUhlov;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StartPreviewAsync();
        }
    }
}
