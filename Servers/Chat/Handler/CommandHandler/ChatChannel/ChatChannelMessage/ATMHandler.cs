﻿using System;
using Chat.Entity.Structure.ChatChannel;
using Chat.Entity.Structure.ChatCommand;
using Chat.Entity.Structure.ChatResponse;
using GameSpyLib.Common.Entity.Interface;

namespace Chat.Handler.CommandHandler
{
    public class ATMHandler : ChatMessageHandlerBase
    {
        new ATM _request;

        public ATMHandler(ISession session, ChatRequestBase cmd) : base(session, cmd)
        {
            _request = (ATM)cmd;
        }

        protected override void DataOperation()
        {
            base.DataOperation();
            switch (_request.RequestType)
            {
                case ChatMessageType.ChannelMessage:
                    _sendingBuffer =
                        ChatReply.BuildATMReply(
                        _user.UserInfo, _request.ChannelName, _request.Message);
                    break;
                case ChatMessageType.UserMessage:
                    _sendingBuffer =
                        ChatReply.BuildATMReply(
                        _session.UserInfo, _request.NickName, _request.Message);
                    break;
            }
        }
    }
}
