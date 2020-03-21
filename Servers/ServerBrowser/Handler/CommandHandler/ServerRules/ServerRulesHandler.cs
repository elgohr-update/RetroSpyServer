﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using GameSpyLib.Encryption;
using QueryReport.Entity.Structure;
using ServerBrowser.Entity.Enumerator;
using ServerBrowser.Entity.Structure;
using ServerBrowser.Entity.Structure.Packet.Request;
using ServerBrowser.Entity.Structure.Packet.Response;

namespace ServerBrowser.Handler.CommandHandler.ServerInfo
{
    /// <summary>
    /// Get full rules for a server (for example, to get
    /// player information from a server that only has basic information so far)
    /// </summary>
    public class ServerRulesHandler : CommandHandlerBase
    {
        private ServerRulesRequest _request;
        private KeyValuePair<EndPoint, GameServer> _server;
        public ServerRulesHandler(SBSession session, byte[] recv) : base(session, recv)
        {
        }

        public override void CheckRequest(SBSession session, byte[] recv)
        {
            base.CheckRequest(session, recv);
            _request = new ServerRulesRequest();
            if (!_request.Parse(recv))
            {
                _errorCode = SBErrorCode.Parse;
                return;
            }
            base.DataOperation(session, recv);
        }

        public override void DataOperation(SBSession session, byte[] recv)
        {
            var servers = QueryReport.Server.QRServer.GameServerList.
                Where(c => c.Value.PublicIP == _request.IP
                && c.Value.ServerData.StandardKeyValue["hostport"] == _request.HostPort.ToString());
            if (servers.Count() != 1)
            {
                _errorCode = SBErrorCode.NoServersFound;
                return;
            }
            _server = servers.FirstOrDefault();
        }

        public override void ConstructResponse(SBSession session, byte[] recv)
        {
            List<byte> data = new List<byte>();
            
            data.Add((byte)SBServerResponseType.PushServerMessage);
            byte[] info = GenerateServerInfo().ToArray();
            
            // we add server info here
            data.AddRange(GenerateServerInfo());

            byte[] byteLength = BitConverter.GetBytes((short)info.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteLength);
            }
            data.InsertRange(0,byteLength);


            GOAEncryption enc = new GOAEncryption(session.EncState);
            
            _sendingBuffer = enc.Encrypt(data.ToArray());
            session.EncState = enc.State;
        }

        private List<byte> GenerateServerInfo()
        {
            List<byte> data = new List<byte>();
            data.AddRange(GenerateServerInfoHeader(_server));

            foreach (var kv in _server.Value.ServerData.CustomKeyValue)
            {
                data.AddRange(Encoding.ASCII.GetBytes(kv.Key));
                data.Add(SBStringFlag.StringSpliter);
                data.AddRange(Encoding.ASCII.GetBytes(kv.Value));
                data.Add(SBStringFlag.StringSpliter);
            }
            return data;
        }
        private List<byte> GenerateServerInfoHeader(KeyValuePair<EndPoint, GameServer> server)
        {
            // you will only have HasKeysFlag or HasFullRule you can not have both
            List<byte> header = new List<byte>();

            //add has key flag
            header.Add((byte)GameServerFlags.HasFullRulesFlag);

            //we add server public ip here
            header.AddRange(BitConverter.GetBytes(server.Value.PublicIP));

            //we check host port is standard port or not
            CheckNonStandardPort(header, server);

            // now we check if there are private ip
            CheckPrivateIP(header, server);

            // we check private port here
            CheckPrivatePort(header, server);

            //TODO we have to check icmp_ip_flag

            return header;
        }
        private void CheckPrivateIP(List<byte> header, KeyValuePair<EndPoint, GameServer> server)
        {
            // now we check if there are private ip
            if (server.Value.ServerData.StandardKeyValue.ContainsKey("localip0"))
            {
                header[0] ^= (byte)GameServerFlags.PrivateIPFlag;
                byte[] address = IPAddress.Parse(server.Value.ServerData.StandardKeyValue["localip0"]).GetAddressBytes();
                header.AddRange(address);
            }
            else if (server.Value.ServerData.StandardKeyValue.ContainsKey("localip1"))
            {
                header[0] ^= (byte)GameServerFlags.PrivateIPFlag;
                byte[] address = IPAddress.Parse(server.Value.ServerData.StandardKeyValue["localip1"]).GetAddressBytes();
                header.AddRange(address);
            }
        }
        private void CheckNonStandardPort(List<byte> header, KeyValuePair<EndPoint, GameServer> server)
        {
            //we check host port is standard port or not
            if (server.Value.ServerData.StandardKeyValue.ContainsKey("hostport"))
            {
                if (server.Value.ServerData.StandardKeyValue["hostport"] != "6500")
                {
                    header[0] ^= (byte)GameServerFlags.NonStandardPort;
                    //we do not need to reverse port bytes
                    byte[] port = BitConverter.GetBytes(ushort.Parse(server.Value.ServerData.StandardKeyValue["hostport"]));

                    header.AddRange(port);
                }
            }
        }
        private void CheckPrivatePort(List<byte> header, KeyValuePair<EndPoint, GameServer> server)
        {
            // we check private port here
            if (server.Value.ServerData.StandardKeyValue.ContainsKey("localport"))
            {
                header[0] ^= (byte)GameServerFlags.NonStandardPrivatePortFlag;
                byte[] localPort =
                 BitConverter.GetBytes(ushort.Parse(server.Value.ServerData.StandardKeyValue["localport"]));

                header.AddRange(localPort);
            }
        }

    }
}
