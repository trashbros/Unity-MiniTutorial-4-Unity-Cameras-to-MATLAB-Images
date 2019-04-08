using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace UnityTutorial
{
    public class CameraPipe : MonoBehaviour
    {
        // Use this for initialization
        public int resolutionWidth = 640;
        public int resolutionHeight = 480;
        public String Host = "localhost";
        public Int32 Port = 55000;
        public bool writefile = false;

        const int SEND_RECEIVE_COUNT = 4;
        private Texture2D texture2D;
        private Rect rect;
        TcpClient mySocket = null;
        NetworkStream theStream = null;
        StreamWriter theWriter = null;
        Camera controlCam;

        void Start()
        {
            Application.runInBackground = true;
            controlCam = this.GetComponent<Camera>();

            mySocket = new TcpClient();
            InitializeGameObject();

            if (SetupSocket())
            {
                Debug.Log("socket is set up");
            }

            Camera.onPostRender += SendRenderedCamera;
        }

        // Update is called once per frame
        void Update()
        {
            if (!mySocket.Connected)
            {
                SetupSocket();
            }
        }

        public bool SetupSocket()
        {
            try
            {
                mySocket.Connect(Host, Port);
                theStream = mySocket.GetStream();
                theWriter = new StreamWriter(theStream);
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Socket error: " + e);
                return false;
            }
        }

        private void InitializeGameObject()
        {
            texture2D = new Texture2D(resolutionWidth, resolutionHeight, TextureFormat.RGB24, false);
            rect = new Rect(0, 0, resolutionWidth, resolutionHeight);
            controlCam.targetTexture = new RenderTexture(resolutionWidth, resolutionHeight, 24);
        }

        //Converts the data size to byte array and put result to the fullBytes array
        void byteLengthToFrameByteArray(int byteLength, byte[] fullBytes)
        {
            //Clear old data
            Array.Clear(fullBytes, 0, fullBytes.Length);
            //Convert int to bytes
            byte[] bytesToSendCount = BitConverter.GetBytes(byteLength);
            //Copy result to fullBytes
            bytesToSendCount.CopyTo(fullBytes, 0);
        }

        public void SendRenderedCamera(Camera _camera)
        {
            if (mySocket == null || _camera != controlCam)
            {
                return;
            }

            if (texture2D != null)
            {
                texture2D.ReadPixels(rect, 0, 0);
                Texture2D t2d = texture2D;

                byte[] imgBytes = t2d.GetRawTextureData();

                // Test line to write to file
                if (writefile)
                {
                    string temp = Application.dataPath + @"/../" + controlCam.name + DateTime.Now.Ticks.ToString() + @".png";
                    Debug.Log("Writing camera frame to: " + temp);
                    File.WriteAllBytes(temp, texture2D.EncodeToPNG());
                }

                //Fill header info
                byte[] camHeaderbyte = new byte[SEND_RECEIVE_COUNT];
                byteLengthToFrameByteArray(imgBytes.Length, camHeaderbyte);

                try
                {
                    //Send Header info first
                    if (mySocket.Connected)
                    {
                        theStream.Write(camHeaderbyte, 0, camHeaderbyte.Length);
                        Debug.Log("Sent header byte Length: " + camHeaderbyte.Length);
                    }

                    //Send the image bytes
                    if (mySocket.Connected)
                    {
                        theStream.Write(imgBytes, 0, imgBytes.Length);
                        Debug.Log("Sent image frame");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Socket error: " + e);
                }
            }
        }

        private void OnApplicationQuit()
        {
            Camera.onPostRender -= SendRenderedCamera;
            if (mySocket != null && mySocket.Connected)
                mySocket.Close();
        }
    }
}