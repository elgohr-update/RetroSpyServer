﻿using GameSpyLib.Common.BaseClass;
using GameSpyLib.Common.Entity.Interface;
using NatNegotiation.Entity.Enumerator;
using NatNegotiation.Entity.Structure;
using NatNegotiation.Entity.Structure.Packet;


namespace NatNegotiation.Handler.CommandHandler
{
    /// <summary>
    /// because we are using self defined error code so we do not need
    /// to send it to client, when we detect errorCode != noerror we just log it
    /// </summary>
    public class NatNegCommandHandlerBase : CommandHandlerBase
    {
        protected NNErrorCode _errorCode = NNErrorCode.NoError;
        protected byte[] _sendingBuffer;
        protected InitPacket _initPacket;
        protected ConnectPacket _connPacket;
        protected ReportPacket _reportPacket;
        protected NatNegClientInfo _clientInfo;
        protected byte[] _recv;

        public NatNegCommandHandlerBase(ISession client, NatNegClientInfo clientInfo, byte[] recv) : base(client)
        {
            _clientInfo = clientInfo;
            _recv = recv;
        }

        public override void Handle()
        {
            base.Handle();

            CheckRequest();
            if (_errorCode != NNErrorCode.NoError)
            {
                return;
            }

            DataOperation();
            if (_errorCode != NNErrorCode.NoError)
            {
                return;
            }
            ConstructResponse();
            if (_errorCode != NNErrorCode.NoError)
            {
                return;
            }
            Response();
        }

        protected virtual void CheckRequest()
        {
        }

        protected virtual void DataOperation()
        {
        }

        protected virtual void ConstructResponse()
        {
        }

        protected virtual void Response()
        {
            if (_sendingBuffer == null)
            {
                return;
            }
            _session.Send(_sendingBuffer);
        }
    }
}