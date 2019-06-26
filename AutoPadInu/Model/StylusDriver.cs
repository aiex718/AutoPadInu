using AutoPadInu_UWP.Model.Datatype;
using AutoPadInu_UWP.Model.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace AutoPadInu_UWP.Model
{
    class StylusDriver
    {
        //       ↑ +x
        //       ↓ -x
        //← -y        +y →

        SerialPort sp = null;
        PhoneConfig Config = null;
        Point3D NowPosition;

        static readonly Point OriginBead = new Point(5, 4);

        public StylusDriver(PhoneConfig phoneCfg)
        {
            NowPosition = new Point3D(0, 0, 0);
            Config = phoneCfg;
        }
        public void SetSerialPort(SerialPort serialPort)
        {
            sp = serialPort;
        }

        public async Task SendRoute(Route route,bool FastMode,int Speed)
        {
            if (route != null && sp != null)
            {
                sp.FlushInput();

                double PerBeadSize = Math.Min(Config.PanelSizeMM.Width / PadPanel.Width, Config.PanelSizeMM.Height / PadPanel.Height);

                await Task.Factory.StartNew(() => 
                {
                    SetSpeedFactor(Speed);
                    InitPosition();

                    double offsetX = (OriginBead.Y - route.StartPoint.Y) * PerBeadSize;
                    double offsetY = (OriginBead.X - route.StartPoint.X) * PerBeadSize;

                    var StartPoint = new Point3D(Config.OriginPoint.X+offsetX, Config.OriginPoint.Y+offsetY, -1);
                    
                    MoveTo3DPointSimultaneous(StartPoint);
                    MoveToZ(Config.OriginPoint.Z);

                    SetSpeedFactor(Speed);

                    foreach (var dir in route.Directions)
                    {
                        switch (dir)
                        {
                            case Direction.Up:
                                MoveRelativeX(PerBeadSize);
                                break;
                            case Direction.Right:
                                MoveRelativeY(PerBeadSize * -1);
                                break;
                            case Direction.Down:
                                MoveRelativeX(PerBeadSize*-1);
                                break;
                            case Direction.Left:
                                MoveRelativeY(PerBeadSize);
                                break;
                            default:
                                break;
                        }
                    }

                    if (FastMode == false)
                        AutoHome();

                    InitPosition();
                    WaitUntilFinish();
                });
            }
        }

        void WaitUntilFinish()
        {
            SendCommand("M400",30000);
        }

        public void AutoHome()
        {
            MoveToZ(Config.InitLiftZmm);
            MoveTo3DPointSimultaneous(new Point3D(2, 2, -1));
            SendCommand("G28",30000);//wait long
            NowPosition = new Point3D(0, 0, 0);
        }

        void SetSpeedFactor(int factor)
        {
            if (factor>=10 && factor<=1000)
                SendCommand("M220 S" + factor.ToString());
        }

        void InitPosition()
        {
            if (NowPosition.Equals(new Point3D(0,0, Config.InitLiftZmm))==false)
            {
                SetSpeedFactor(200);
                MoveToZ(Config.InitLiftZmm);
                MoveTo3DPointSimultaneous(new Point3D(0, 0, -1));
            }
        }

        void MoveRelativeX(double Xmm)
        {
            MoveToX(NowPosition.X + Xmm);
        }

        void MoveRelativeY(double Ymm)
        {
            MoveToY(NowPosition.Y + Ymm);
        }

        void MoveRelativeZ(double Zmm)
        {
            MoveToZ(NowPosition.Z + Zmm);
        }

        void MoveTo3DPoint(Point3D p)
        {
            MoveToX(p.X);
            MoveToY(p.Y);
            MoveToZ(p.Z);
        }

        void MoveTo3DPointSimultaneous(Point3D p)
        {
            StringBuilder AsciiCmdSb = new StringBuilder();

            AsciiCmdSb.Append("G0");
            if (p.X>=0)
            {
                NowPosition.X = p.X;
                AsciiCmdSb.Append(" X");
                AsciiCmdSb.Append(p.X.ToString("000.0"));
            }

            if (p.Y >= 0)
            {
                NowPosition.Y = p.Y;
                AsciiCmdSb.Append(" Y");
                AsciiCmdSb.Append(p.Y.ToString("000.0"));
            }

            if (p.Z >= 0)
            {
                NowPosition.Z = p.Z;
                AsciiCmdSb.Append(" Z");
                AsciiCmdSb.Append(p.Z.ToString("000.0"));
            }
            SendCommand(AsciiCmdSb.ToString());
        }

        void MoveToX(double Xmm)
        {
            NowPosition.X = Xmm;
            SendCommand("G0 X"+Xmm.ToString("000.0"));
        }

        void MoveToY(double Ymm)
        {
            NowPosition.Y = Ymm;
            SendCommand("G0 Y" + Ymm.ToString("000.0"));
        }

        void MoveToZ(double Zmm)
        {
            NowPosition.Z = Zmm;
            SendCommand("G0 Z" + Zmm.ToString("000.0"));
        }


        //dont use async here
        string SendCommand(string ascii_cmd,int? WaitTimeMs=null)
        {
            if (sp==null)
            {
                return "Serial not open";
            }

            const int DefaultWaitTimeMs = 5000;
            string echo = String.Empty;

            Task.Factory.StartNew( () =>
            {
                Debug.Write("Tx cmd:");
                Debug.WriteLine(ascii_cmd);

                List<byte> command = new List<byte>();
                command.AddRange(Encoding.ASCII.GetBytes(ascii_cmd));
                command.Add(0x0d);
                command.Add(0x0a);

                StringBuilder ReceiveMessageSB = new StringBuilder();
                byte[] Buffer = new byte[128];

                sp.SetRxTimeout(10);

                sp.WriteAsync(command.ToArray()).Wait();

                while (true)
                {
                    var t = sp.ReadBytesAsync(Buffer, WaitTimeMs ?? DefaultWaitTimeMs);
                    t.Wait();

                    var RxCnt = t.Result;

                    if (RxCnt > 0)
                    {
                        string msg = Encoding.ASCII.GetString(Buffer, 0, (int)RxCnt);
                        ReceiveMessageSB.Append(msg);

                        if (msg.Contains('\n'))
                            break;
                    }
                    else
                    {
                        Debug.Write("ReadBytesAsync timeout,no terminate char found,RxLen:");
                        Debug.WriteLine(ReceiveMessageSB.Length);
                        break;
                    }

                }

                echo = ReceiveMessageSB.ToString();
                Debug.Write("Rx echo:");
                Debug.WriteLine(echo);
                
            }).Wait();
            return echo;
        }

        
    }
}
