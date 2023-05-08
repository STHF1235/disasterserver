﻿using BetterServer.Entities;
using BetterServer.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterServer.Maps
{
    public class TortureCave : Map
    {
        public override void Init(Server server)
        {
            Spawn<TCGom>(server);

            SetTime(server, 155);
            base.Init(server);
        }

        protected override int GetRingSpawnCount()
        {
            return 27;
        }
    }
}
