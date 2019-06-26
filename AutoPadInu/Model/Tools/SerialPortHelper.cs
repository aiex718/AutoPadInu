using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace AutoPadInu_UWP.Model.Tools
{

    //based on https://github.com/Jark/FTDISample

    class DeviceNode
    {
        public string Id { get; private set; }
        public string Name { get; private set; }

        public DeviceNode(string id,string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }


    class SerialPort
    {
        private readonly SerialDevice device;

        public static async Task<SerialPort> OpenByDeviceId(string deviceId,uint baud)
        {
            var device = await SerialDevice.FromIdAsync(deviceId);
            return new SerialPort(device, baud);
        }

        public static async Task<IEnumerable<DeviceNode>> GetDeviceList()
        {
            var deviceSelector = SerialDevice.GetDeviceSelector();
            var devices = await DeviceInformation.FindAllAsync(deviceSelector);

            return devices.Select(x => new DeviceNode(x.Id, x.Name));
        }

        public SerialPort(SerialDevice device, uint baudrate)
        {
            this.device = device;

            device.BaudRate = baudrate;
            device.Handshake = SerialHandshake.None;
            device.Parity = SerialParity.None;
            device.StopBits = SerialStopBitCount.One;
            device.DataBits = 8;
        }

        public void SetRxTimeout(int ms)
        {
            device.ReadTimeout = TimeSpan.FromMilliseconds(ms);
        }

        public async Task<uint> WriteAsync(byte[] bytesToWrite)
        {
            return await device.OutputStream.WriteAsync(bytesToWrite.AsBuffer());
        }

        public uint FlushInput()
        {
            uint bytesReadTotal = 0;
            
            using (var dataReader = new DataReader(device.InputStream))
            {
                bool Continue = true;

                while (Continue) 
                {
                    using (CancellationTokenSource source = new CancellationTokenSource())
                    {
                        try
                        {
                            CancellationToken token = source.Token;

                            uint ReadBufferLength = 256;  // only when this buffer would be full next code would be executed

                            Task<UInt32> loadAsyncTask = dataReader.LoadAsync(ReadBufferLength).AsTask(token);   // Create a task object

                            loadAsyncTask.Wait(10);//wait 10ms

                            if (loadAsyncTask.IsCompleted == false)
                                source.Cancel();

                            var bytesRead = loadAsyncTask.Result;    // Launch the task and wait until buffer would be full

                            Debug.Write("Flush bytes:");
                            Debug.WriteLine(bytesRead);
                            if (bytesRead > 0)
                            {
                                string strFromPort = dataReader.ReadString(bytesRead);
                                Debug.Write("Data:");
                                Debug.WriteLine(strFromPort);
                            }

                            bytesReadTotal += bytesRead;
                        }
                        catch (Exception ae)
                        {
                            Debug.WriteLine("Flush timeout,no more bytes");
                            Continue = false;
                        }
                    }
                }

                dataReader.DetachStream();
            }

            return bytesReadTotal;
        }

        public async Task<int> ReadBytesAsync(byte[] bytesBuffer,int WaitTimeMs)
        {
            int result = 0;
            await Task.Factory.StartNew(() =>
            {
                using (CancellationTokenSource source = new CancellationTokenSource())
                {
                    try
                    {
                        CancellationToken token = source.Token;

                        var buffer = bytesBuffer.AsBuffer();
                        uint rxlen = (uint)bytesBuffer.Length;
                        //ReadAsync will try to read as much bytes as rxlen
                        //when first byte arrive,start counting until rx timeout and return
                        var task = device.InputStream.ReadAsync(buffer, rxlen, InputStreamOptions.Partial).AsTask(token);

                        task.Wait(WaitTimeMs);//wait 

                        if (task.IsCompleted == false)
                            source.Cancel();

                        result = (int)task.Result.Length;
                    }
                    catch (Exception)
                    {
                        ;
                    }
                }
            });
            return result;
            
        }
    }

}
