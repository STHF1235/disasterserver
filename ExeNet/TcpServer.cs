﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ExeNet
{
    public class TcpServer : IDisposable
    {
        public IPAddress IPAddress { get; private set; }
        public int Port { get; private set; }
        public bool IsRunning { get; private set; }
        public List<TcpSession> Sessions = new();

        protected TcpListener _listener;

        private Thread? _acceptThread;
        private ushort _idGen = 1;

        public TcpServer(IPAddress ip, int port)
        {
            IPAddress = ip;
            Port = port;

            _listener = new(ip, port);
        }

        public bool Start()
        {
            if (IsRunning)
                return false;

            try
            {
                _listener.Start();
            }
            catch(SocketException e)
            {
                OnSocketError(e.SocketErrorCode);
                return false;
            }

            _idGen = 1;
            IsRunning = true;

            _acceptThread = new(Run);
            _acceptThread.Name = "TcpServer Worker";
            _acceptThread.Start();

            OnReady();

            return true;
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            _acceptThread!.Join();

            try
            {
                _listener.Stop();
            }
            catch(SocketException ex)
            {
                OnSocketError(ex.SocketErrorCode);
            }
        }

        public TcpSession? GetSession(ushort id) => Sessions.Where(e => e.ID == id).FirstOrDefault();

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        public ushort RequestID() => _idGen++;
        
        private void Run()
        {
            while(IsRunning)
            {
                try
                {
                    TcpClient client = _listener.AcceptTcpClient();

                    lock (Sessions)
                    {
                        TcpSession session = CreateSession(client);
                        session.Start();

                        Sessions.Add(session);
                    }
                }
                catch(SocketException ex)
                {
                    OnSocketError(ex.SocketErrorCode);
                }
                catch(InvalidOperationException ex)
                {
                    OnError(ex.Message);
                }
            }
        }

        protected virtual TcpSession CreateSession(TcpClient client) => new(this, client);
        protected virtual void OnReady() { }
        protected virtual void OnSocketError(SocketError error) { }
        protected virtual void OnError(string message) { }
    }
}
