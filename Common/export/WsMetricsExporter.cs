using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using WebSocketSharp;
using Newtonsoft.Json;

namespace MetricsCommon.export
{
    /// <summary>
    /// Export metrics data via Websocket
    /// </summary>
    public class WsMetricsExporter
    {
        private WebSocket websocket;
        private static readonly Object websocketLock = new Object();
        private System.Timers.Timer keepAliveTimer;
        private JsonSerializer serializer;
        private string serviceName = "";
        private string linienName = "";
        private string location = "";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public WsMetricsExporter(WebSocket websocket, string serviceName, string LinienName, string Location, int pingInterval = 10)
        {
            this.websocket = websocket;
            this.websocket.EmitOnPing = false;
            this.SetupKeepAliveTimer(intervalSecs: pingInterval);
            this.serializer = new JsonSerializer();
            this.linienName = LinienName;
            this.serviceName = serviceName;
            this.location = Location;
        }

        ~WsMetricsExporter()
        {
            if (this.websocket.IsAlive)
            {
                this.websocket.Close(CloseStatusCode.Normal);
            }
            this.keepAliveTimer.Stop();
        }

        public void Connect()
        {
            this.websocket.Connect();

            this.SetupKeepAliveTimer(intervalSecs: 10);
            this.keepAliveTimer.Start();
            this.keepAliveTimer.Elapsed += new ElapsedEventHandler(this.OnKeepAliveTimer);
            this.websocket.OnError += this.OnError;

            log.Info($"Connected (isAlive {websocket.IsAlive}) to {websocket.Url}");
        }

        public void SendMetric(IEnumerable<IExportableMetric> variables)
        {
            log.Debug($"Waiting for websocketLock Lock");
            lock (websocketLock)
            {
                log.Debug($"Aquired websocketLock Lock");
                this.WaitForConnection();

                var itemsToSend = new List<object>();
                foreach (var item in variables)
                {
                    if (!item.IsCorrupted())
                    {
                        itemsToSend.Add(item.AsSerializeable());
                    }
                }

                if (itemsToSend.Count > 0)
                {
                    log.Debug($"Sending Update");
                    this.SendChunk(itemsToSend);

                }
                else
                {
                    log.Debug($"Nothing to send");
                }
            }
        }

        public void OnMessageReceived(object sender, MessageEventArgs msg)
        {
            if (msg.Data == "ping")
            {
                this.websocket.Send("pong");
            }
        }

        public void OnError(object sender, ErrorEventArgs args)
        {
            log.Debug(args.Message);
            log.Debug("Websocket OnError:", args.Exception);
        }

        protected void SendChunk(IEnumerable<Object> chunk)
        {
            var body = new
            {
                this.serviceName,
                this.linienName,
                this.location,
                metrics = chunk
            };
            var jsonBody = JsonConvert.SerializeObject(body);
            this.websocket.SendAsync(jsonBody, this.OnSent);
        }

        protected void WaitForConnection()
        {
            var numFailiures = 0;
            var exponentialTimeout = (int)(Math.Pow(2, numFailiures) - 1) * 1000;
            var sleepTime = Math.Min(10 + exponentialTimeout, 5 * 60 * 1000);

            while (this.websocket.ReadyState != WebSocketState.Open || !this.websocket.IsAlive)
            {
                numFailiures += 1;

                log.Debug($"Trying to connect - retrying in {sleepTime / 1000} seconds");
                this.keepAliveTimer.Stop(); // prevent queing up of ping requests over time
                this.Connect();
                sleepTime = this.websocket.IsAlive ? 1 : sleepTime;
                numFailiures = 0;
                Thread.Sleep(sleepTime);
                exponentialTimeout = (int)(Math.Pow(2, numFailiures) - 1) * 1000; ;
                sleepTime = Math.Min(1000 + exponentialTimeout, 5 * 60 * 1000);
            }
        }

        protected void OnSent(bool success)
        {
            if (!success)
            {
                log.Debug("Failed to send message");
            }
        }


        protected void OnConnectionLost()
        {
            if (this.websocket.IsAlive)
            {
                log.Error("Connection lost");
                this.websocket.Close(CloseStatusCode.Away);
            }
        }

        protected void OnKeepAliveTimer(object sender, ElapsedEventArgs args)
        {
            lock (websocketLock)
            {
                if (!this.websocket.Ping())
                {
                    this.OnConnectionLost();
                }
            }
        }

        protected void SetupKeepAliveTimer(int intervalSecs = 10)
        {
            if (this.keepAliveTimer != null)
            {
                this.keepAliveTimer.Stop();
            }
            this.keepAliveTimer = new System.Timers.Timer
            {
                Interval = intervalSecs * 1000, // 10 seconds
            };
        }
    }
}
