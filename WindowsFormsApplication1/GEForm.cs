using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Kinect;
using System.IO;
using Microsoft.Kinect.Toolkit;
using System.Runtime.InteropServices;

namespace WindowsFormsApplication1
{
    public partial class GEForm : Form
    {
        private GEController GECtrl;
        private KinectSensor KCTSensor;
        private GEDetector GestureDetection;
        private KinectSensorChooser SensorChooser;
        private Bitmap SensorVideo;
        public GEForm()
        {
            InitializeComponent();
            GECtrl = new GEController(webBrowser);

            webBrowser.Navigate(new Uri("file:///C:/GoogleEarth/Map.html"));
          //  webBrowser.Navigate("http://earth-api-samples.googlecode.com/svn/trunk/demos/desktop-embedded/pluginhost.html");
            
            DiscoverKinectSensor();
            this.GestureDetection = new GEDetector();
            this.GestureDetection.GestureDetected += GesturHandler;
        }

        private void DiscoverKinectSensor()
        {
            KinectSensor.KinectSensors.StatusChanged += KinectSensor_StatusChanged;
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.Sensor = potentialSensor;
                    break;
                }
            }
            initKinectSensor();
        }
        public KinectSensor Sensor
        {
            get { return this.KCTSensor; }
            set
            {
                if (this.KCTSensor != value && this.KCTSensor != null)
                {
                    unIntiSensor();
                    this.KCTSensor = null;
                }
                this.KCTSensor = value;
                initKinectSensor();
            }
        }

        private void unIntiSensor()
        {
            this.Sensor.Stop();
            this.Sensor.SkeletonFrameReady -= KinectSensor_SkeletonFrameReady;
            this.Sensor.SkeletonStream.Disable();
        }

        private void initKinectSensor()
        {
            if (this.Sensor != null)
            {
               this.Sensor.SkeletonStream.Enable();
               
               
                this.Sensor.SkeletonFrameReady += KinectSensor_SkeletonFrameReady;
                
                try
                {
                    this.Sensor.Start();
                }
                catch (IOException)
                {
                    this.Sensor = null;
                    return;
                }
                //RecognizerInfo ri = GetKinectRecognizer();
            }
        }

        private void KinectSensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            Skeleton[] skeletons = new Skeleton[0];
            long timestamp = new long();

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletons = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletons);
                    timestamp = skeletonFrame.Timestamp;
                }
            }
            this.GestureDetection.Update(skeletons, timestamp);
        }


        private void KinectSensor_StatusChanged(object sender, StatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case KinectStatus.Connected:
                    if (this.Sensor == null)
                    {
                        this.Sensor = e.Sensor;
                    }
                    break;

                case KinectStatus.Disconnected:
                    if (this.Sensor == e.Sensor)
                    {
                        this.Sensor = null;
                        this.Sensor = KinectSensor.KinectSensors.FirstOrDefault(x => x.Status == KinectStatus.Connected);

                       }
                    break;
            }
        }

       
        private void GesturHandler(object sender, EventArgs e)
        {
            switch (this.GestureDetection.Gesture)
            {
                case 0:
                    break;
                case 1:
                    //Left Hand Waved!"
                  GECtrl.panLeft();
                    break;
                case 2:
                    //"Right Hand Waved!"
                    GECtrl.panLeft();
                    break;
                case 3:
                    //"Left Hand Swiped!"
                    GECtrl.panRight();
                    break;
                case 4:
                    //"Right Hand Swiped!"
                    GECtrl.panRight();
                    break;
                case 5:
                    //hand down
                    GECtrl.zoomIn();
                    break;
                case 6:
                    //"Turn up Volume!"
                    GECtrl.zoomOut();
                    break;
                
                case 11:
                    //"Stop!"
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            GECtrl.zoomIn();
        }

        private void webBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
        
    }
}
