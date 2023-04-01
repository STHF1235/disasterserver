﻿using BetterServer.Entities;
using BetterServer.Session;
using BetterServer.State;
using ExeNet;
using System.Diagnostics;

namespace BetterServer.Maps
{
    public abstract class Map
    {
        public Game Game;

        public int Timer = 2 * Ext.FRAMESPSEC * Ext.FRAMESPSEC;
        public List<Entity> Entities = new();
        public bool BigRingSpawned = false;
        public bool BigRingReady = false;
        public ushort RingIDs = 1;

        private int _ringActivateTime = (Ext.FRAMESPSEC * Ext.FRAMESPSEC) - (10 * Ext.FRAMESPSEC);
        private int _ringCoff = 3;
        private int _ringTimer = -(Ext.FRAMESPSEC * 4);
        private Random _rand = new();

        public virtual void Init(Server server)
        {
            lock(server.Peers)
            {
                if (server.Peers.Count > 3)
                    _ringCoff = 2;
            }
        }
        
        public virtual void Tick(Server server)
        {
            if(Timer % Ext.FRAMESPSEC == 0)
            {
                var packet = new TcpPacket(PacketType.SERVER_GAME_TIME_SYNC);
                packet.Write((ushort)(Timer));
                server.TCPMulticast(packet);
            }

            DoRingTimer(server);
            DoBigRingTimer(server);

            if (Timer > 0)
                Timer--;

            lock (Entities)
            {
                for(var i = 0; i < Entities.Count; i++)
                {
                    var ent = Entities[i];
                    var packet = ent.Tick(server, Game, this);

                    if (packet == null)
                        continue;

                    server.UDPMulticast(ref Game.IPEndPoints, packet);
                }
            }
        }

        public virtual void PeerTCPMessage(Server server, TcpSession session, BinaryReader reader)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var passtrough = reader.ReadBoolean();
            var type = reader.ReadByte();

            switch ((PacketType)type)
            {
                /* Tails' projectile */
                case PacketType.CLIENT_TPROJECTILE:
                    {
                        var projectile = new TailsProjectile
                        {
                            X = reader.ReadUInt16(),
                            Y = reader.ReadUInt16(),
                            Direction = reader.ReadSByte(),
                            Damage = reader.ReadByte(),
                            IsExe = reader.ReadBoolean(),
                            Charge = reader.ReadByte()
                        };

                        Spawn(server, projectile);
                        break;
                    }

                case PacketType.CLIENT_TPROJECTILE_HIT:
                    {
                        Destroy<TailsProjectile>(server);
                        break;
                    }

                case PacketType.CLIENT_ETRACKER:
                    {
                        var tracker = new EggmanTracker
                        {
                            X = reader.ReadUInt16(),
                            Y = reader.ReadUInt16()
                        };

                        Spawn(server, tracker);
                        break;
                    }

                case PacketType.CLIENT_ETRACKER_ACTIVATED:
                    {
                        lock (Entities)
                        {
                            var id = reader.ReadByte();
                            var activator = reader.ReadUInt16();

                            var trackers = FindOfType<EggmanTracker>();
                            if (trackers == null)
                                break;

                            var tracker = trackers.Where(e => e.ID == id).FirstOrDefault();
                            if (tracker == null)
                                break;

                            tracker.ActivatorID = activator;
                            Destroy(server, tracker);
                        }
                        break;
                    }

                case PacketType.CLIENT_CREAM_SPAWN_RINGS:
                    {
                        var _x = reader.ReadUInt16();
                        var _y = reader.ReadUInt16();
                        var _cnt = reader.ReadByte();
                        var _redRing = reader.ReadBoolean();

                        for (var i = 0; i < _cnt; i++)
                        {
                            if(_redRing)
                            {
                                Spawn(server, new CreamRing()
                                {
                                    X = (int)(_x + Math.Sin(Math.PI * 2.5 - (i * Math.PI)) * 26),
                                    Y = (int)(_y + Math.Cos(Math.PI * 2.5 - (i * Math.PI)) * 26),
                                    IsRedRing = true
                                });
                            }
                            else
                            {

                                Spawn(server, new CreamRing()
                                {
                                    X = (int)(_x + Math.Sin(Math.PI * 2.5 + (i * (Math.PI / 3))) * 26),
                                    Y = (int)(_y + Math.Cos(Math.PI * 2.5 + (i * (Math.PI / 3))) * 26),
                                    IsRedRing = false
                                });
                            }
                        }

                        break;
                    }

                case PacketType.CLIENT_RING_COLLECTED:
                    {
                        lock (Entities)
                        {
                            var id = reader.ReadByte();
                            var uid = reader.ReadUInt16();

                            var rings = FindOfType<Ring>();
                            if (rings == null)
                                break;

                            var ring = rings.Where(e => e.ID == uid).FirstOrDefault();
                            if (ring == null)
                                break;

                            var packet = new TcpPacket(PacketType.SERVER_RING_COLLECTED, ring.IsRedRing);
                            server.TCPSend(session, packet);

                            Destroy(server, ring);
                        }
                        break;
                    }
            }
        }

        public void SetTimer(Server server, int seconds)
        {
            Timer = (seconds * Ext.FRAMESPSEC) + (GetPlayerOffset(server) * Ext.FRAMESPSEC);
            Logger.LogDebug($"Timer is set to {Timer} frames");
        }

        public void ActivateRingAfter(int afterSeconds)
        {
            _ringActivateTime = (Ext.FRAMESPSEC * Ext.FRAMESPSEC) - (afterSeconds * Ext.FRAMESPSEC);
            Logger.LogDebug($"Ring activate time is set to {Timer} frames");
        }

        #region Entities

        public Entity? Spawn<T>(Server server) where T : Entity
        {
            T? entity = Ext.CreateOfType<T>();

            if (entity == null)
                return null;

            TcpPacket? pack;
            lock (Entities)
            {
                Entities.Add(entity);
                pack = entity.Spawn(server, Game, this);
            }

            if (pack != null)
                server.TCPMulticast(pack);

            Logger.LogDebug($"Entity {entity} spawned.");
            return entity;
        }

        public T Spawn<T>(Server server, T entity) where T : Entity
        {
            TcpPacket? pack;
            lock (Entities)
            {
                Entities.Add(entity);
                pack = entity.Spawn(server, Game, this);
            }

            if (pack != null)
                server.TCPMulticast(pack);

            Logger.LogDebug($"Entity {entity} spawned.");
            return entity;
        }

        public void Destroy(Server server, Entity entity)
        {
            TcpPacket? pack;
            lock (Entities)
            {
                Entities.Remove(entity);
                pack = entity.Destroy(server, Game, this);
            }

            if (pack != null)
                server.TCPMulticast(pack);

            Logger.LogDebug($"Entity {entity} destroyed.");
        }

        public void Destroy<T>(Server server) where T : Entity
        {
            lock (Entities)
            {
                var pick = FindOfType<T>();

                if (pick == null)
                    return;

                foreach (var p in pick)
                {
                    Destroy(server, p);
                    Logger.LogDebug($"Entity {p} destroyed.");
                }
            }
        }

        public T[]? FindOfType<T>() where T : Entity
        {
            lock(Entities)
            {
                var pick = Entities.Where(e => e is T).ToArray();
                Logger.LogDebug($"Entity search found {pick.Length} entities of type {typeof(T).FullName}");
                return Array.ConvertAll(pick, e => (T)e);
            }
        }

        #endregion

        private void DoRingTimer(Server server)
        {
            if(_ringTimer >= (_ringCoff * Ext.FRAMESPSEC))
            {
                Spawn<Ring>(server);
                _ringTimer = 0;
            }
            _ringTimer++;
        }

        private void DoBigRingTimer(Server server)
        {
            if (Timer - (Ext.FRAMESPSEC * Ext.FRAMESPSEC) <= 0 && !BigRingSpawned)
            {
                var packet = new TcpPacket(PacketType.SERVER_GAME_SPAWN_RING);
                packet.Write((byte)_rand.Next(255));
                server.TCPMulticast(packet);

                BigRingSpawned = true;
            }

            var min = _ringActivateTime; // 1 min - 10 sec
            if (Timer - min <= 0 && !BigRingReady)
            {
                var packet = new TcpPacket(PacketType.SERVER_GAME_RING_READY);
                server.TCPMulticast(packet);

                BigRingSpawned = true;
            }
        }

        private int GetPlayerOffset(Server server)
        {
            lock (server.Peers)
                return (server.Peers.Count - 1) * 20;
        }
    }
}
