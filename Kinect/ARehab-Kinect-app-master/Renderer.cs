using Microsoft.Azure.Kinect.BodyTracking;
using OpenGL;
using OpenGL.CoreUI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MathNet;
using MathNet.Numerics.LinearAlgebra;
using System.IO;

namespace Csharp_3d_viewer
{
    public class Renderer
    {
        private SphereRenderer SphereRenderer;
        private CylinderRenderer CylinderRenderer;
        private PointCloudRenderer PointCloudRenderer;

        private readonly VisualizerData visualizerData;
        private List<Vertex> pointCloud = null;

        private UdpClient udpClient; // = new UdpClient(11000);

        private string hololens_ip = "10.189.82.212";
        private string paul_pc_ip = "10.189.92.159";
        private string paul_pc_ip_local = "192.168.137.22";
        //private string paul_pc_ip_local = "192.168.137.11";

        private string IP_CLIENT = "";

        private const int STATUS_INIT = 0;
        private const int STATUS_CALIBRATION_ACQUIRING = 2;
        private const int STATUS_CALIBRATION_PROCESSING = 3;

        private const int STATUS_RUNNING = 5;

        private int Status = STATUS_INIT;

        private const float KINECT_HEIGHT = 914.4f;

        //*********************************************
        // Matrix Processing
        //MatrixProcessing
        MatrixTransformation MT;

        //**********************************************
        // Calibration structures

        const int MAX_NUM_OF_POINTS = 10000;

        PointArray LeftHand_Kinect_PA;
        PointArray RightHand_Kinect_PA;

        PointArray LeftHand_Hololens_PA;
        PointArray RightHand_Hololens_PA;

        List<Point3D> LeftHand_Hololens_list, RightHand_Hololens_list, LeftHand_Kinect_list, RightHand_Kinect_list;
        int calibration_points_acquired = 0;

        float[] data_to_send = new float[6];

        //***********************************************
        // Hololens HAnds Position
        Point3D LH_Hololens_pos, RH_Hololens_pos;

        private bool receive_data_from_hololens = true;

        private bool server_started = false;


        //***********************************************
        // DEBUG

        bool debug_print_all = false;
        bool debug_print_incoming_kinect = false;
        bool debug_print_sending_data_UDP = false;
        bool debug_print_receiving_data_UDP = false;

        //**********************************************
        // FLAGS
        public bool IsActive { get; private set; }
        private bool TrackingAnybody = false;
        private bool normalize_height = true;
        private bool send_data_to_hololens = false; 
        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="visualizerData"></param>
        public Renderer(VisualizerData visualizerData)
        {
            this.visualizerData = visualizerData;

            LeftHand_Kinect_PA = new PointArray(MAX_NUM_OF_POINTS);
            RightHand_Kinect_PA = new PointArray(MAX_NUM_OF_POINTS);

            LeftHand_Hololens_PA = new PointArray(MAX_NUM_OF_POINTS);
            RightHand_Hololens_PA = new PointArray(MAX_NUM_OF_POINTS);

            LeftHand_Hololens_list = new List<Point3D>();
            RightHand_Hololens_list = new List<Point3D>();
            LeftHand_Kinect_list = new List<Point3D>();
            RightHand_Kinect_list = new List<Point3D>();

            LH_Hololens_pos = new Point3D(0,0,0);
            RH_Hololens_pos = new Point3D(0, 0, 0);

            MT = new MatrixTransformation();
            ReadHololensIP();

            udpClient = new UdpClient(20201);
        }

        private void ReadHololensIP()
        {

            string path = @"hololens_ip.txt";

            // This text is added only once to the file.
            if (File.Exists(path))
            {
                // Read the file and display it line by line.  
                System.IO.StreamReader file =
                    new System.IO.StreamReader(path);
                string s = file.ReadLine();

                file.Close();

                if (s != "")
                {
                    IP_CLIENT = s;
                    Console.WriteLine("HOLOLENS IP Address = " + IP_CLIENT);
                }
            }

        }

        private float GetKinectHeight(float y)
        {
            return KINECT_HEIGHT - y; 
        }


        private float min(float a, float b)
        {
            if (a < b)
                return a;
            
            return b;
        }


        /// <summary>
        /// StartUDPServerRoutine to receive info from Hololoens 2
        /// </summary>
        public void StartServer()
        {
            if (!server_started)
            {
                server_started = true; 
                UdpClient receivingUdpClient = new UdpClient(20202);

                //Creates an IPEndPoint to record the IP Address and port number of the sender. 
                // The IPEndPoint will allow you to read datagrams sent from any source.
                IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                try
                {

                    while (receive_data_from_hololens)
                    {
                        // Blocks until a message returns on this socket from a remote host.
                        Byte[] data = receivingUdpClient.Receive(ref RemoteIpEndPoint);

                        //string returnData = Encoding.ASCII.GetString(receiveBytes);

                        string NEW_IP_CLIENT = RemoteIpEndPoint.Address.ToString();

                        if (IP_CLIENT == "" && NEW_IP_CLIENT != "")
                        {
                            Console.WriteLine("Coonecting to " + NEW_IP_CLIENT);
                            udpClient.Connect(NEW_IP_CLIENT, 20201);
                        }

                        int index = 0;
                        LH_Hololens_pos.X = BitConverter.ToSingle(data, index); index += 4;
                        LH_Hololens_pos.Y = BitConverter.ToSingle(data, index); index += 4;
                        LH_Hololens_pos.Z = BitConverter.ToSingle(data, index); index += 4;

                        RH_Hololens_pos.X = BitConverter.ToSingle(data, index); index += 4;
                        RH_Hololens_pos.Y = BitConverter.ToSingle(data, index); index += 4;
                        RH_Hololens_pos.Z = BitConverter.ToSingle(data, index);

                        if (Status == STATUS_CALIBRATION_ACQUIRING)
                        {
                            LeftHand_Hololens_PA.AddPoint(LH_Hololens_pos.X, LH_Hololens_pos.Y, LH_Hololens_pos.Z);
                            RightHand_Hololens_PA.AddPoint(RH_Hololens_pos.X, RH_Hololens_pos.Y, RH_Hololens_pos.Z);
                        }

                        if (debug_print_receiving_data_UDP)
                        {
                            Console.WriteLine("RECEIVING-> LH: " + LH_Hololens_pos.X.ToString("0.00") + " " + LH_Hololens_pos.Y.ToString("0.00") + " " + LH_Hololens_pos.Z.ToString("0.00") +

                            " --- RH: " + RH_Hololens_pos.X.ToString("0.00") + " " + RH_Hololens_pos.Y.ToString("0.00") + " " + RH_Hololens_pos.Z.ToString("0.00"));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }



        /// <summary>
        /// Old StartServer Method (to be deleted)
        /// </summary>
        public void StartServer_old()
        {
            int recv;
            byte[] data = new byte[1024];
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 20202);

            Socket newsock = new Socket(AddressFamily.InterNetwork,
                            SocketType.Dgram, ProtocolType.Udp);

            newsock.Bind(ipep);
            Console.WriteLine("Waiting for a client...");

            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(sender);

            Console.WriteLine("Address IP = " + sender.Address.ToString());

            recv = newsock.ReceiveFrom(data, ref Remote);

            //Console.WriteLine("Message received from {0}:", Remote.ToString());
            //Console.WriteLine(Encoding.ASCII.GetString(data, 0, recv));

            //string welcome = "Welcome to my test server";
            //data = Encoding.ASCII.GetBytes(welcome);
            //newsock.SendTo(data, data.Length, SocketFlags.None, Remote);

            while (receive_data_from_hololens)
            {
                data = new byte[128];
                recv = newsock.ReceiveFrom(data, ref Remote);

                //Console.WriteLine("Address IP = " + Remote.AddressFamily.ToString());
                //data = newsock.Receive(ref Remote);

                int index = 0;
                LH_Hololens_pos.X = BitConverter.ToSingle(data, index); index += 4;
                LH_Hololens_pos.Y = BitConverter.ToSingle(data, index); index += 4;
                LH_Hololens_pos.Z = BitConverter.ToSingle(data, index); index += 4;

                RH_Hololens_pos.X = BitConverter.ToSingle(data, index); index += 4;
                RH_Hololens_pos.Y = BitConverter.ToSingle(data, index); index += 4;
                RH_Hololens_pos.Z = BitConverter.ToSingle(data, index);

                if (Status == STATUS_CALIBRATION_ACQUIRING)
                {
                    LeftHand_Hololens_PA.AddPoint(LH_Hololens_pos.X, LH_Hololens_pos.Y, LH_Hololens_pos.Z);
                    RightHand_Hololens_PA.AddPoint(RH_Hololens_pos.X, RH_Hololens_pos.Y, RH_Hololens_pos.Z);
                }

                if (debug_print_receiving_data_UDP)
                {
                    Console.WriteLine("RECEIVING-> LH: " + LH_Hololens_pos.X.ToString("0.00") + " " + LH_Hololens_pos.Y.ToString("0.00") + " " + LH_Hololens_pos.Z.ToString("0.00") +
                        " --- RH: " + RH_Hololens_pos.X.ToString("0.00") + " " + RH_Hololens_pos.Y.ToString("0.00") + " " + RH_Hololens_pos.Z.ToString("0.00"));
                }
                //newsock.SendTo(data, recv, SocketFlags.None, Remote);
            }
        }

        /// <summary>
        /// Method to start the visualization thread
        /// </summary>
        public void StartVisualizationThread()
        {
            Task.Run(() =>
            {
                using (NativeWindow nativeWindow = NativeWindow.Create())
                {
                    IsActive = true;
                    nativeWindow.ContextCreated += NativeWindow_ContextCreated;
                    nativeWindow.Render += NativeWindow_Render;
                    nativeWindow.KeyDown += (object obj, NativeWindowKeyEventArgs e) =>
                    {
                        switch (e.Key)
                        {
                            case KeyCode.Escape:
                                nativeWindow.Stop();
                                IsActive = false;
                                receive_data_from_hololens = false; 

                                break;

                            case KeyCode.F:
                                nativeWindow.Fullscreen = !nativeWindow.Fullscreen;
                                break;

                            case KeyCode.C:
                                Console.WriteLine("***************************** - CALIBRATION STARTED - ***************************");
                                StartCalibrationProcedure();

                                break;


                            case KeyCode.Space:
                                Console.WriteLine("***************************** - CALIBRATION NEW POINT- ***************************");
                                Status = STATUS_CALIBRATION_ACQUIRING;
                                CalibratenewCoupleofPoints();

                                break;
                            case KeyCode.P:
                                Console.WriteLine("***************************** - PROCESS CALIBRATION DATA - ***************************");
                                ProcessCalibrationData();

                                break;


                            case KeyCode.R:
                                Console.WriteLine("***************************** - RUNNING - ***************************");
                                Status = STATUS_RUNNING;

                                break;

                            case KeyCode.S:
                                Console.WriteLine("***************************** - START RECEIVIG SERVER - ***************************");
                                //Status = STATUS_RUNNING;
                                StartReceiver();

                                break;

                            case KeyCode.X:
                                Console.WriteLine("***************************** - SAVING RAW DATA - ***************************");
                                //Status = STATUS_RUNNING;
                                StartReceiver();

                                break;

                            case KeyCode.N0:
                                Console.WriteLine("***************************** - TOOGLE DEBUG PRINT ALL - ***************************");
                                if( debug_print_all)
                                {
                                    debug_print_sending_data_UDP = false;
                                    debug_print_receiving_data_UDP = false;
                                    debug_print_incoming_kinect = false; 

                                    debug_print_all = false; 
                                }
                                else
                                {
                                    debug_print_sending_data_UDP = true;
                                    debug_print_receiving_data_UDP = true;
                                    debug_print_incoming_kinect = true;

                                    debug_print_all = true;
                                }

                                break;
                            case KeyCode.U:
                                if (!send_data_to_hololens)
                                {
                                    Console.WriteLine("Coonecting to " + IP_CLIENT);
                                    udpClient.Connect(IP_CLIENT, 20201);
                                    send_data_to_hololens = true; 
                                }
                                else
                                {
                                    send_data_to_hololens = false;
                                }

                                    break;



                            case KeyCode.N1:
                                Console.WriteLine("***************************** - TOOGLE DEBUG PRINT SENDING DATA - ***************************");
                                if (debug_print_sending_data_UDP)
                                {
                                    debug_print_sending_data_UDP = false;
                                }
                                else
                                {
                                    debug_print_sending_data_UDP = true;
                                }

                                break;

                            case KeyCode.N2:
                                Console.WriteLine("***************************** - TOOGLE DEBUG PRINT RECEIVING DATA - ***************************");
                                if (debug_print_sending_data_UDP)
                                {
                                    debug_print_receiving_data_UDP = false;
                                }
                                else
                                {
                                    debug_print_receiving_data_UDP = true;
                                }

                                break;

                            case KeyCode.N3:
                                Console.WriteLine("***************************** - TOOGLE DEBUG PRINT RECEIVING DATA FROM KINECT- ***************************");
                                if (debug_print_sending_data_UDP)
                               {
                                    debug_print_incoming_kinect = false;
                                }
                                else
                                {
                                    debug_print_incoming_kinect = true;
                                }

                                break;


                        }
                    };
                    nativeWindow.Animation = true;

                    nativeWindow.Create(0, 0, 640, 480, NativeWindowStyle.Overlapped);

                    nativeWindow.Show();
                    nativeWindow.Run();
                }
            });
        }

        /// <summary>
        /// Method to start the UDP Receiving thread, running the StartServer method
        /// </summary>
        public void StartReceiver()
        {
            Thread UDP_ReceivingThread = new Thread(StartServer);
            UDP_ReceivingThread.Start();
        }

        /// <summary>
        /// Start the Calibration Procedure, by zeroing the data structures
        /// </summary>
        private void StartCalibrationProcedure()
        {
            LeftHand_Kinect_PA.Reset();
            RightHand_Kinect_PA.Reset();
            LeftHand_Hololens_PA.Reset();
            RightHand_Hololens_PA.Reset();

            LeftHand_Hololens_list.Clear();
            RightHand_Hololens_list.Clear();
            LeftHand_Kinect_list.Clear();
            RightHand_Kinect_list.Clear();

            calibration_points_acquired = 0;
        }

        /// <summary>
        /// First step of the calibration procedure: it gathers couples of points
        /// </summary>
        private void CalibratenewCoupleofPoints()
        {
            if (calibration_points_acquired++ > 0)
            {
                LeftHand_Kinect_PA.UpdateAverages(25, 80);
                RightHand_Kinect_PA.UpdateAverages(25, 80);
                LeftHand_Hololens_PA.UpdateAverages(25, 80);
                RightHand_Hololens_PA.UpdateAverages(25, 80);

                LeftHand_Hololens_list.Add(new Point3D(LeftHand_Hololens_PA.avg_x, LeftHand_Hololens_PA.avg_y, LeftHand_Hololens_PA.avg_z));
                RightHand_Hololens_list.Add(new Point3D(RightHand_Hololens_PA.avg_x, RightHand_Hololens_PA.avg_y, RightHand_Hololens_PA.avg_z));

                LeftHand_Kinect_list.Add(new Point3D(LeftHand_Kinect_PA.avg_x, LeftHand_Kinect_PA.avg_y, LeftHand_Kinect_PA.avg_z));
                RightHand_Kinect_list.Add(new Point3D(RightHand_Kinect_PA.avg_x, RightHand_Kinect_PA.avg_y, RightHand_Kinect_PA.avg_z));

                LeftHand_Kinect_PA.Reset();
                RightHand_Kinect_PA.Reset();
                LeftHand_Hololens_PA.Reset();
                RightHand_Hololens_PA.Reset();

            }
        }

        private void ProcessCalibrationData()
        {
            Status = STATUS_CALIBRATION_PROCESSING;
            int num_of_points = LeftHand_Hololens_list.Count;
            int size = 3;
            try
            {
                MT.UpdateTransformation(LeftHand_Kinect_list, RightHand_Kinect_list, LeftHand_Hololens_list, RightHand_Hololens_list);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private void NativeWindow_ContextCreated(object sender, NativeWindowEventArgs e)
        {
            Gl.ReadBuffer(ReadBufferMode.Back);

            Gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            Gl.LineWidth(2.5f);

            CreateResources();
        }

        private static float ToRadians(float degrees)
        {
            return degrees / 180.0f * (float)Math.PI;
        }

        private void NativeWindow_Render(object sender, NativeWindowEventArgs e)
        {
            float head_height;
            float feet_y_min;
            float delta; 

            using (var lastFrame = visualizerData.TakeFrameWithOwnership())
            {
                if (lastFrame == null)
                {
                    return;
                }

                NativeWindow nativeWindow = (NativeWindow)sender;

                Gl.Viewport(0, 0, (int)nativeWindow.Width, (int)nativeWindow.Height);
                Gl.Clear(ClearBufferMask.ColorBufferBit);

                // Update model/view/projective matrices in shader
                var proj = Matrix4x4.CreatePerspectiveFieldOfView(ToRadians(65.0f), (float)nativeWindow.Width / nativeWindow.Height, 0.1f, 150.0f);
                var view = Matrix4x4.CreateLookAt(Vector3.Zero, Vector3.UnitZ, -Vector3.UnitY);

                SphereRenderer.View = view;
                SphereRenderer.Projection = proj;

                CylinderRenderer.View = view;
                CylinderRenderer.Projection = proj;

                //PointCloudRenderer.View = view;
                //PointCloudRenderer.Projection = proj;

                //PointCloud.ComputePointCloud(lastFrame.Capture.Depth, ref pointCloud);
                //PointCloudRenderer.Render(pointCloud, new Vector4(1, 1, 1, 1));

                if (lastFrame.NumberOfBodies > 0 && !TrackingAnybody)
                {
                    Console.WriteLine("+++++++++++++++++ TRACKING SOMEBODY ++++++++++++++++");
                    TrackingAnybody = true; 
                }
                if (lastFrame.NumberOfBodies == 0 && TrackingAnybody)
                {
                    Console.WriteLine("-----------------  NOBODY TRACKED  -----------------");
                    TrackingAnybody = false;
                }

                for (uint i = 0; i < lastFrame.NumberOfBodies; ++i)
                {
                    var skeleton = lastFrame.GetBodySkeleton(i);
                    var bodyId = lastFrame.GetBodyId(i);
                    var bodyColor = BodyColors.GetColorAsVector(bodyId);

                    switch(Status)
                    {
                        case STATUS_INIT:
                        case STATUS_RUNNING:
                        case STATUS_CALIBRATION_PROCESSING:    
                            
                            break;

                        case STATUS_CALIBRATION_ACQUIRING:
                            LeftHand_Kinect_PA.AddPoint(
                                skeleton.GetJoint(JointId.HandLeft).Position.X,
                                skeleton.GetJoint(JointId.HandLeft).Position.Y,
                                skeleton.GetJoint(JointId.HandLeft).Position.Z);

                            RightHand_Kinect_PA.AddPoint(
                                skeleton.GetJoint(JointId.HandRight).Position.X,
                                skeleton.GetJoint(JointId.HandRight).Position.Y,
                                skeleton.GetJoint(JointId.HandRight).Position.Z);

                            //LeftHand_Hololens_PA.AddPoint(LH_Hololens_pos.X, LH_Hololens_pos.Y, LH_Hololens_pos.Z);
                            //RightHand_Hololens_PA.AddPoint(RH_Hololens_pos.X, RH_Hololens_pos.Y, RH_Hololens_pos.Z);

                            break;
                    }


                    if (true)
                    {
                        if (true)
                        {
                            data_to_send[0] = skeleton.GetJoint(JointId.FootLeft).Position.X;
                            data_to_send[1] = GetKinectHeight(skeleton.GetJoint(JointId.FootLeft).Position.Y);
                            data_to_send[2] = skeleton.GetJoint(JointId.FootLeft).Position.Z;

                            data_to_send[3] = skeleton.GetJoint(JointId.FootRight).Position.X;
                            data_to_send[4] = GetKinectHeight(skeleton.GetJoint(JointId.FootRight).Position.Y);
                            data_to_send[5] = skeleton.GetJoint(JointId.FootRight).Position.Z;
                        }
                        else
                        {
                            data_to_send[0] = skeleton.GetJoint(JointId.HandLeft).Position.X;
                            data_to_send[1] = skeleton.GetJoint(JointId.HandLeft).Position.Y;
                            data_to_send[2] = skeleton.GetJoint(JointId.HandLeft).Position.Z;

                            data_to_send[3] = skeleton.GetJoint(JointId.HandRight).Position.X;
                            data_to_send[4] = skeleton.GetJoint(JointId.HandRight).Position.Y;
                            data_to_send[5] = skeleton.GetJoint(JointId.HandRight).Position.Z;
                        }

                    }
                    else
                    {
                        //DEBUG

                        data_to_send[0] = 1.0f;
                        data_to_send[1] = 2.0f;
                        data_to_send[2] = 3.0f;

                        data_to_send[3] = 4.0f;
                        data_to_send[4] = 5.0f;
                        data_to_send[5] = 6.0f;
                    }

                    head_height = GetKinectHeight(skeleton.GetJoint(JointId.Head).Position.Y);

                    feet_y_min = min(data_to_send[1], data_to_send[4]);
                    delta = head_height - feet_y_min;

                    data_to_send[1] = (data_to_send[1] - feet_y_min) / delta;
                    data_to_send[4] = (data_to_send[4] - feet_y_min) / delta;
                    
                    //data_to_send[1] /= head_height;
                    if (data_to_send[1] > 1.0f)
                        data_to_send[1] = 1.0f;
                    else
                    {
                        if (data_to_send[1] < 0)
                            data_to_send[1] = 0;
                    }

                    //data_to_send[4] /= head_height;
                    if (data_to_send[4] > 1.0f)
                        data_to_send[4] = 1.0f;
                    else
                    { 
                    if (data_to_send[4] < 0)
                        data_to_send[4] = 0;
                    }

                //unbias data 
                if (false)
                    {
                        if (true)
                        {
                            //unbias data subtracting the measures from the head's ones
                            feet_y_min = min(data_to_send[1], data_to_send[4]);
                            delta = head_height - feet_y_min;

                            data_to_send[0] = skeleton.GetJoint(JointId.Head).Position.X - data_to_send[0];
                            data_to_send[1] = data_to_send[1] - feet_y_min;
                            data_to_send[2] = skeleton.GetJoint(JointId.Head).Position.Z - data_to_send[2];

                            data_to_send[3] = skeleton.GetJoint(JointId.Head).Position.X - data_to_send[3];
                            data_to_send[4] = head_height - data_to_send[4];
                            data_to_send[5] = skeleton.GetJoint(JointId.Head).Position.Z - data_to_send[5];

                            if (data_to_send[1] < 0)
                                data_to_send[1] = -data_to_send[1];

                            if (data_to_send[4] < 0)
                                data_to_send[4] = -data_to_send[4];

                            if (normalize_height && head_height > 0)
                            {
                                data_to_send[1] /= head_height;
                                if (data_to_send[1] > 1.0f)
                                    data_to_send[1] = 1.0f;
                                else
                                if (data_to_send[1] <0)
                                    data_to_send[1] = 0;

                                data_to_send[4] /= head_height;
                                if (data_to_send[4] > 1.0f)
                                    data_to_send[4] = 1.0f;
                                else
                                if (data_to_send[4] < 0)
                                    data_to_send[4] = 0;
                            }
                        }
                        else
                        {
                            //unbias data subtracting the the head's measures from the actual ones
                            data_to_send[0] -= skeleton.GetJoint(JointId.Head).Position.X;
                            data_to_send[1] -= skeleton.GetJoint(JointId.Head).Position.Y;
                            data_to_send[2] -= skeleton.GetJoint(JointId.Head).Position.Z;

                            data_to_send[3] -= skeleton.GetJoint(JointId.Head).Position.X;
                            data_to_send[4] -= skeleton.GetJoint(JointId.Head).Position.Y;
                            data_to_send[5] -= skeleton.GetJoint(JointId.Head).Position.Z;
                        }
                    }

                    if (debug_print_sending_data_UDP)
                    {
                        Console.WriteLine("SENDING-> LH: " + data_to_send[0].ToString("0.00") + " " + data_to_send[1].ToString("0.00") + " " + data_to_send[2].ToString("0.00") +
                        " --- RH: " + data_to_send[3].ToString("0.00") + " " + data_to_send[4].ToString("0.00") + " " + data_to_send[5].ToString("0.00") + " Head height = " + head_height.ToString("0.00"));
                    }

                    if(true)
                        SendPositions(data_to_send);

                    for (int jointId = 0; jointId < (int)JointId.Count; ++jointId)
                    {
                        if(jointId == (int)JointId.SpineNavel)
                        {
                            float f = skeleton.GetJoint(jointId).Position.X;
                            //SendFloat(f);
                        }

                        if (jointId == (int)JointId.HandRight || jointId == (int)JointId.HandLeft ||
                            jointId == (int)JointId.FootLeft || jointId == (int)JointId.FootRight ||
                            jointId == (int)JointId.SpineNavel)
                        {

                            var joint = skeleton.GetJoint(jointId);

                            // Render the joint as a sphere.
                            const float radius = 0.012f;
                            SphereRenderer.Render(joint.Position / 1000, radius, bodyColor);

                            if (false)
                            {
                                if (JointConnections.JointParent.TryGetValue((JointId)jointId, out JointId parentId))
                                {
                                    // Render a bone connecting this joint and its parent as a cylinder.
                                    CylinderRenderer.Render(joint.Position / 1000, skeleton.GetJoint((int)parentId).Position / 1000, bodyColor);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Debug Method to sent just one float over UDP socket
        /// </summary>
        /// <param name="f"></param>
        private void SendFloat(float f)
        {
            //Console.WriteLine(f.ToString("0.000"));

            Byte[] sendBytes = BitConverter.GetBytes(f);


            if (IP_CLIENT != "")
                udpClient.Send(sendBytes, sendBytes.Length);

        }


        /// <summary>
        /// Method to send position information about the feet
        /// </summary>
        /// <param name="data"></param>
        private void SendPositions(float[] data)
        {
            Byte[] sendBytes = new Byte[data.Length * 4];

            for (int i = 0; i < data.Length; i++)
            {
                Byte[] temobytes = BitConverter.GetBytes((float)data[i]);

                sendBytes[i * 4] = temobytes[0];
                sendBytes[i * 4 + 1] = temobytes[1];
                sendBytes[i * 4 + 2] = temobytes[2];
                sendBytes[i * 4 + 3] = temobytes[3];
            }

            if (send_data_to_hololens)
                try
                {
                    udpClient.Send(sendBytes, sendBytes.Length);
                }
                catch(Exception ex)
                {

                }
            }

        /// <summary>
        /// Utility to create the needed resources
        /// </summary>
        private void CreateResources()
        {
            SphereRenderer = new SphereRenderer();
            CylinderRenderer = new CylinderRenderer();
            PointCloudRenderer = new PointCloudRenderer();
        }
    }
}
