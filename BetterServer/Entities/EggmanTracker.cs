﻿using BetterServer.Maps;
using BetterServer.Session;
using BetterServer.State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Entities
{
    /// <summary>
    /// Eggman's tracker
    /// Empty because it does nothing on server
    /// </summary>
    internal class EggmanTracker : Entity
    {
        public static byte TrackerIDs = 0;

        public ushort ActivatorID;
        public byte ID;

        public override TcpPacket? Spawn(Server server, Game game, Map map)
        {
            ID = TrackerIDs++;

            return new TcpPacket
            (
                PacketType.SERVER_ETRACKER_STATE,

                (byte)0,
                ID, 
                (ushort)X, 
                (ushort)Y
            );
        }

        public override TcpPacket? Destroy(Server server, Game game, Map map)
        {
            return new TcpPacket
            (
                PacketType.SERVER_ETRACKER_STATE,

                (byte)1,
                ID,
                ActivatorID
            );
        }

        public override UdpPacket? Tick(Server server, Game game, Map map)
        {
            return null;
        }
    }
}
