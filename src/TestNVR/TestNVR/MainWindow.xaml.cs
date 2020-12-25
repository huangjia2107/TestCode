using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace TestNVR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var d = fff("172.16.154.200", 8000, "admin", "jidian123", 33, new DateTime(2020, 12, 25, 16, 00, 00), new DateTime(2020, 12, 25, 16, 01, 00));
        }

        private CHCNetSDK.NET_DVR_TIME DateTimeToNETDVRTIME(DateTime time)
        {
            return new CHCNetSDK.NET_DVR_TIME
            {
                dwYear = (uint)time.Year,
                dwMonth= (uint)time.Month,
                dwDay= (uint)time.Day,
                dwHour= (uint)time.Hour,
                dwMinute= (uint)time.Minute,
                dwSecond= (uint)time.Second
            };
        }

        private bool fff(string ip, int port, string username, string password, int channel, DateTime startTime, DateTime endTime)
        {
            var initFlag = CHCNetSDK.NET_DVR_Init();
            //返回值为布尔值 fasle初始化失败
            if (!initFlag)
            {
                //log.warn("hksdk(视频)-海康sdk初始化失败!");
                return false;
            }

            var deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            var userId = CHCNetSDK.NET_DVR_Login_V30(ip, (short)port, username, password, ref  deviceInfo);

            //log.info("hksdk(视频)-登录海康录像机信息,状态值:" + hCNetSDK.NET_DVR_GetLastError());
            if (userId == -1)
            {
                //log.warn("hksdk(视频)-海康sdk登录失败!");
                return false;
            }

            var st = DateTimeToNETDVRTIME(startTime);
            var et = DateTimeToNETDVRTIME(endTime);

            var playHandle = CHCNetSDK.NET_DVR_PlayBackByTime(userId, channel, ref st, ref et, IntPtr.Zero);//new WindowInteropHelper(this).Handle
            if (playHandle < 0)
            {
                var d = CHCNetSDK.NET_DVR_GetLastError();

                CHCNetSDK.NET_DVR_Logout(userId);
                CHCNetSDK.NET_DVR_Cleanup();
                return false;
            }

            uint pos = 0;
            if (!CHCNetSDK.NET_DVR_PlayBackControl(playHandle, CHCNetSDK.NET_DVR_PLAYSTART, 0, ref pos))
            {
                //printf("play back control failed [%d]\n", NET_DVR_GetLastError());
                CHCNetSDK.NET_DVR_Logout(userId);
                CHCNetSDK.NET_DVR_Cleanup();
                return false;
            }

            if(!CHCNetSDK.NET_DVR_SetPlayDataCallBack(playHandle, PlayDataCallback, 0))
            {
                CHCNetSDK.NET_DVR_Logout(userId);
                CHCNetSDK.NET_DVR_Cleanup();
                return false;
            }

            return true;
        }

        void  PlayDataCallback(int lPlayHandle, uint dwDataType, IntPtr pBuffer, uint dwBufSize, uint dwUser)
        {
            var bufHandle = new byte[dwBufSize]; 
            Marshal.Copy(pBuffer, bufHandle, 0, (int)dwBufSize);


            switch (dwDataType)
            {

                case CHCNetSDK.FILE_HEAD:     // 头
                    break;

                case CHCNetSDK.VIDEO_I_FRAME: // I 帧 
                    Win32.GenerateMP4File(AppDomain.CurrentDomain.BaseDirectory+"222.mp4", bufHandle, dwBufSize, dwDataType);
                    break;

                case CHCNetSDK.VIDEO_B_FRAME: // B 帧
                    break;

                case CHCNetSDK.VIDEO_P_FRAME: // P 帧
                    Win32.GenerateMP4File(AppDomain.CurrentDomain.BaseDirectory + "222.mp4", bufHandle, dwBufSize, dwDataType);
                    break;

                default:       // 其他数据
                    break;
            }

            
        }
    }
}
