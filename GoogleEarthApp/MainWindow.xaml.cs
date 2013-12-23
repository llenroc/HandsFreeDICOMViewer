using System.Windows;
using System.Windows.Input;
using Microsoft.Kinect;
using System.Runtime.InteropServices;

namespace GoogleEarthApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private float oldRightHandX = 0;
        private float oldRightHandY = 0;
        private float oldLeftHandX = 0;
        private float oldLeftHandY = 0;

        private bool clicked = false;
        private bool process = false;
        private bool wait = false;

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, int cButtons, uint dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const int MOUSEEVENTF_LEFTUP = 0x0004;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const int MOUSEEVENTF_RIGHTUP = 0x0010;
        private const int MOUSEEVENTF_WHEEL = 0x0800;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.None;
            kinectSensorChooser.KinectSensorChanged += kinectSensorChooser_KinectSensorChanged;

            kinectSkeletonViewer.FrameReloaded +=new Microsoft.Samples.Kinect.WpfViewers.KinectCustomEventHandler(kinectSkeletonViewer_FrameReloaded);
        }

        void kinectSkeletonViewer_FrameReloaded()
        {
            if (wait && kinectSkeletonViewer.RightHandX < kinectSkeletonViewer.LeftHandX && kinectSkeletonViewer.RightHandY > kinectSkeletonViewer.HeadY)
            {
                process = !process;             
                wait = false;
            }

            if (kinectSkeletonViewer.RightHandX > kinectSkeletonViewer.LeftHandX)
            {
                wait = true;
            }
            //process = true;
            lblMessageZ.Content = process;

            if(process)
            {
                var posX = System.Windows.Forms.Cursor.Position.X;
                var posY = System.Windows.Forms.Cursor.Position.Y;
                
                var rightSubX = (int)((kinectSkeletonViewer.RightHandX - oldRightHandX) * 1000);
                var rightSubY = (int)((kinectSkeletonViewer.RightHandY - oldRightHandY) * 1000);
                var leftSubX = (int)((kinectSkeletonViewer.LeftHandX - oldLeftHandX) * 1000);
                var leftSubY = (int)((kinectSkeletonViewer.LeftHandY - oldLeftHandY) * 1000);

                lblMessageY.Content = rightSubY;

                if(rightSubX < 50 || rightSubY < 50)
                {
                    posX += rightSubX;
                    posY += -rightSubY;

                    System.Windows.Forms.Cursor.Position = new System.Drawing.Point(posX, posY);

                    if (kinectSkeletonViewer.LeftHandX < -0.5)
                    {
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, 120, 0);
                        lblMessageX.Content = "Zoom In";
                    }
                    else if (kinectSkeletonViewer.LeftHandX > -0.2 && kinectSkeletonViewer.LeftHandX < 0)
                    {
                        mouse_event(MOUSEEVENTF_WHEEL, 0, 0, -120, 0);
                        lblMessageX.Content = "Zoom Out";
                    }
                    else
                    {
                        lblMessageX.Content = "-";
                    }

                    if (kinectSkeletonViewer.LeftHandY > 0.2)
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                        lblMessageX.Content = "Click";
                        clicked = true;
                    }
                    else
                    {
                        if (clicked)
                        {
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            lblMessageX.Content = "Release";
                            clicked = false;
                        }
                    }
                }

                oldRightHandX = kinectSkeletonViewer.RightHandX;
                oldRightHandY = kinectSkeletonViewer.RightHandY;
                oldLeftHandX = kinectSkeletonViewer.LeftHandX;
                oldLeftHandY = kinectSkeletonViewer.LeftHandY;
            }
        }

        private void kinectSensorChooser_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            KinectSensor oldSensor = (KinectSensor)e.OldValue;
            StopKinect(oldSensor);

            KinectSensor newSensor = (KinectSensor)e.NewValue;

            newSensor.DepthStream.Enable();
            newSensor.SkeletonStream.Enable();
            newSensor.ColorStream.Enable();

            try
            {
                newSensor.Start();
            }
            catch (System.IO.IOException)
            {
                kinectSensorChooser.AppConflictOccurred();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(kinectSensorChooser.Kinect);
        }

        private static void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }
    }
}
