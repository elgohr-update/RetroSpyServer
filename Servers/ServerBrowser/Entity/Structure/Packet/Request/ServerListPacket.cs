﻿using System;
using System.Linq;
using System.Text;
using GameSpyLib.Encryption;
using ServerBrowser.Entity.Enumerator;

namespace ServerBrowser.Entity.Structure.Packet.Request
{
    /// <summary>
    /// ServerList also called ServerRule
    /// </summary>
    public class ServerListPacket
    {
        public SBErrorCode ErrorCode;
        public short RequestLenth { get; protected set; }
        public byte RequestVersion { get; protected set; }
        public byte ProtocolVersion { get; protected set; }
        public byte EncodingVersion { get; protected set; }
        public int GameVersion { get; protected set; }
        public int QueryOptions { get; protected set; }

        public string DevGameName { get; protected set; }
        public string GameName { get; protected set; }
        public string Challenge { get; protected set; }
        public SBServerListUpdateOption UpdateOption { get; protected set; }

        public string[] FieldList { get; protected set; }
        public string Filter;
        public byte[] SourceIP { get; protected set; }
        public int MaxServers { get; protected set; }


        public ServerListPacket()
        {
            SourceIP = new byte[4];
            ErrorCode = SBErrorCode.NoError;
        }

        /// <summary>
        /// Parse all value to this class
        /// </summary>
        /// <param name="recv"></param>
        public void Parse(byte[] recv)
        {
            
            byte[] byteRequestLength = ByteTools.SubBytes(recv, 0, 2);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteRequestLength);
            }
            RequestLenth = BitConverter.ToInt16(byteRequestLength);

            if (RequestLenth != recv.Length)
            {
                ErrorCode = SBErrorCode.Parse;
                return;
            }

            RequestVersion = recv[2];
            ProtocolVersion = recv[3];
            EncodingVersion = recv[4];
            GameVersion = BitConverter.ToInt32(ByteTools.SubBytes(recv, 5, 4));

            //because there are empty string we can not use StringSplitOptions.RemoveEmptyEntries
            string remainData = Encoding.ASCII.GetString(recv.Skip(9).ToArray());
            remainData.IndexOf('\0');
            DevGameName = remainData.Substring(0, remainData.IndexOf('\0'));
            remainData = remainData.Substring(remainData.IndexOf('\0')+1);
            GameName = remainData.Substring(0, remainData.IndexOf('\0'));
            remainData = remainData.Substring(remainData.IndexOf('\0')+1);
            Challenge = remainData.Substring(0, remainData.IndexOf('\0')).Substring(0,8);
            if (remainData.Substring(0, remainData.IndexOf('\0')).Length > 8)
            {
                Filter = remainData.Substring(0, remainData.IndexOf('\0')).Substring(7, remainData.IndexOf('0'));
            }
            
            remainData = remainData.Substring(remainData.IndexOf('\0') + 1);
            FieldList = remainData.Substring(0, remainData.IndexOf('\0')).Split("\\",StringSplitOptions.RemoveEmptyEntries);
            remainData = remainData.Substring(remainData.IndexOf('\0') + 1);

            byte[] byteUpdateOptions = Encoding.ASCII.GetBytes(remainData.Substring(0, 4));

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(byteUpdateOptions);
            }

            UpdateOption = (SBServerListUpdateOption)BitConverter.ToInt32(byteUpdateOptions);

  
            if ((UpdateOption & SBServerListUpdateOption.AlternateSourceIP) != 0)
            {
                SourceIP = Encoding.ASCII.GetBytes(remainData.Substring(0, 4));
                remainData = remainData.Substring(7);
            }

            if ((UpdateOption & SBServerListUpdateOption.LimitResultCount) != 0)
            {
                byte[] byteMaxServer = Encoding.ASCII.GetBytes(remainData.Substring(0, 4));

                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(byteMaxServer);
                }
                MaxServers = BitConverter.ToInt32(byteMaxServer);
            }
        }
    }
}