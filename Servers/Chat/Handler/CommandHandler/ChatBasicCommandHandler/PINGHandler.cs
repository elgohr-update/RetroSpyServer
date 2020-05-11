﻿using System;
using Chat.Entity.Structure.ChatCommand;
using Chat.Entity.Structure.ChatResponse;
using GameSpyLib.Common.Entity.Interface;

namespace Chat.Handler.CommandHandler
{
    public class PINGHandler : ChatCommandHandlerBase
    {
        public PINGHandler(ISession client, ChatCommandBase cmd) : base(client, cmd)
        {
        }

        public override void ConstructResponse()
        {
            base.ConstructResponse();
            _sendingBuffer = ChatReply.BuildPingReply(_session.UserInfo);
        }
    }
}
