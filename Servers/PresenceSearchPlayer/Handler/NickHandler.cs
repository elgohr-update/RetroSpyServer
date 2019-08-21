﻿using GameSpyLib.Common;
using GameSpyLib.Logging;
using PresenceSearchPlayer.Enumerator;
using System;
using System.Collections.Generic;
using System.Text;

/////////////////////////Finished/////////////////////////////////
namespace PresenceSearchPlayer.Handler
{

    /// <summary>
    /// Uses a nick to find how many uniquenick is in this nick
    /// </summary>
    public class NickHandler
    {

        /// <summary>
        /// Get nickname through email and password
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dict"></param>
        public static void SearchNicks(GPSPClient client, Dictionary<string, string> dict)
        {
            //Format the password for our database storage
            GPSPHandler.ProessPassword(dict);
            //if not recieved correct request we terminate
            GPErrorCode error = GPSPHandler.IsSearchNicksContianAllKeys(dict);
            if (error != GPErrorCode.NoError)
            {
                GameSpyUtils.SendGPError(client.Stream, (int)error, "Error recieving SearchNicks request.");
                return;
            }

            List<Dictionary<string, object>> queryResult;

            try
            {
                //get nicknames from GPSPDBQuery class
                queryResult = GPSPHandler.DBQuery.RetriveNicknames(dict);
            }
            catch (Exception ex)
            {
                LogWriter.Log.Write(ex.Message, LogLevel.Error);
                GameSpyUtils.SendGPError(client.Stream, GPErrorCode.DatabaseError, "This request cannot be processed because of a database error.");
                return;
            }

            if (queryResult.Count < 1)
            {
                GameSpyUtils.SendGPError(client.Stream, GPErrorCode.DatabaseError, "No match found !");
                return;
            }

            string sendingBuffer;
            sendingBuffer = @"\nr\";
            foreach (Dictionary<string, object> row in queryResult)
            {
                sendingBuffer += @"\nick\";
                sendingBuffer += row["nick"];
                sendingBuffer += @"\uniquenick\";
                sendingBuffer += row["uniquenick"];

            }

            sendingBuffer += @"\ndone\final\";
            client.Stream.SendAsync(sendingBuffer);
        }
    }
}