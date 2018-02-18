﻿using BencodeNET.Torrents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using torrent_library.Model;
using torrent_library.Util;

namespace torrent_library.Tracker
{
    public class UDPTracker
    {


        private int _nTimeout = 0;
        private int _ReceiveTimeout = 0;
        private long ConnectionID = -1;
        private string InfoHash { get; set; }
        private byte[] PeerID { get; set; }
        private TrackerAdress _TrackerAddress { get; set; }

        public bool IsConnected
        {
            get; set;
        }
        public int NTimeout
        {
            get { return _nTimeout; }
            set
            {
                _nTimeout = value;
                CalculateReceiveTimeout();
            }
        }
        public AnnounceResponse _AnnounceResponse = null;
        public AnnounceRequest _AnnounceRequest = null;
        public ScrapeResponse _ScrapeResponse = null;
        public DateTime LastAnnounced = DateTime.MaxValue;
        public Torrent _Torrent { get; set; }


        public UDPTracker(TrackerAdress address, string infoHash, Torrent torrent)
        {
            _TrackerAddress = address;
            CalculateReceiveTimeout();
            InfoHash = infoHash;
            PeerID = PeerIDUtil.GenerateRandom();
            _Torrent = torrent;

            IsConnected = false;
        }

        private void CalculateReceiveTimeout()
        {
            _ReceiveTimeout = (int)(15000 * Math.Pow((double)2, (double)_nTimeout));
        }


        public void ConnectToTracker()
        {
            try
            {
                Connect();
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.TimedOut && _nTimeout <= 8)
                {
                    NTimeout++;
                    ConsoleUtil.WriteError("Connection timed out");
                    ConnectToTracker();
                }
                else if (_nTimeout > 8)
                {
                    ConsoleUtil.WriteError("Couldn't connect to tracker.");
                }
                else
                    ConsoleUtil.WriteError(e.Message);
            }
        }

        public void Connect()
        {
            ConsoleUtil.Write("Connecting to tracker... => {0}:{1}", _TrackerAddress.Host, _TrackerAddress.Port);
            using (UdpClient client = new UdpClient(_TrackerAddress.Host, _TrackerAddress.Port))
            {

                client.Client.ReceiveTimeout = _ReceiveTimeout;

                var connectRequest = new TrackerConnectRequest();
                var request = connectRequest.GetRequestArray();

                var resp = client.Send(request, request.Length);
                var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var result = client.Receive(ref remoteEndPoint);

                NTimeout = 0;

                if (result.Length < 16)
                    throw new Exception("Couldn't received response correctly.");

                var trackerConnectResponse = new TrackerConnectResponse(connectRequest, result);
                ConnectionID = trackerConnectResponse.ConnectionID;

                IsConnected = true;
                ConsoleUtil.WriteSuccess("Connected to tracker => {0}:{1}", _TrackerAddress.Host, _TrackerAddress.Port);
            }
        }

        public void Scrape()
        {
            try
            {
                ConsoleUtil.Write("Sending scrape request...");
                //0               64 - bit integer connection_id
                //8               32 - bit integer action          2 // scrape
                //12              32 - bit integer transaction_id
                //16 + 20 * n     20 - byte string info_hash
                //16 + 20 * N


                //                scrape response:
                //Offset Size            Name Value
                //0           32 - bit integer action          2 // scrape
                //4           32 - bit integer transaction_id
                //8 + 12 * n  32 - bit integer seeders
                //12 + 12 * n 32 - bit integer completed
                //16 + 12 * n 32 - bit integer leechers
                //8 + 12 * N

                var client = new UdpClient(_TrackerAddress.Host, _TrackerAddress.Port);
                client.Client.ReceiveTimeout = _ReceiveTimeout;

                var scrapeRequest = new ScrapeRequest(InfoHash, ConnectionID);
                var requestByteArray = scrapeRequest.CreateScrapeRequestArray();


                var resp = client.Send(requestByteArray, requestByteArray.Length);
                var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var result = client.Receive(ref remoteEndPoint);
                NTimeout = 0;

                var scrapeResponse = new ScrapeResponse(result);
                _ScrapeResponse = scrapeResponse;
                ConsoleUtil.WriteSuccess("Scraped successfully! => {0}:{1}", _TrackerAddress.Host, _TrackerAddress.Port);
                ConsoleUtil.Write("Seeders = {0}, Leechers {1}, Completed = {2}, ResultLength = {3}", scrapeResponse.Seeders, scrapeResponse.Leechers, scrapeResponse.Completed, scrapeResponse.ResponseLength);

            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.TimedOut && _nTimeout <= 8)
                {
                    NTimeout++;
                    ConsoleUtil.WriteError("Scrape request timed out");
                    Scrape();
                }
            }

        }

        public void Announce()
        {
            try
            {
                ConsoleUtil.Write("Sending Announce request...");
                var announceRequest = new AnnounceRequest(ConnectionID, InfoHash, AnnounceAction.Started, PeerID, _Torrent);
                this._AnnounceRequest = announceRequest;
                var announceRequestArray = announceRequest.GetRequestArray();

                var client = new UdpClient(_TrackerAddress.Host, _TrackerAddress.Port);
                client.Client.ReceiveTimeout = _ReceiveTimeout;

                var resp = client.Send(announceRequestArray, announceRequestArray.Length);
                var remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                var result = client.Receive(ref remoteEndPoint);

                _AnnounceResponse = new AnnounceResponse(result);
                LastAnnounced = DateTime.Now;
                NTimeout = 0;
                ConsoleUtil.WriteSuccess("Announced successfully.");
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.TimedOut && _nTimeout <= 8)
                {
                    ConsoleUtil.WriteError("Announce request timed out");
                    NTimeout++;
                    Announce();
                }

            }
        }
    }
}
