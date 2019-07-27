﻿using System;
using System.Collections.Generic;
using System.Linq;
using SharpDX;
using HipHopFile;
using System.IO;

namespace IndustrialPark
{
    public partial class ArchiveEditorFunctions
    {
        public bool Shuffle(Random r, RandomizerFlags flags, RandomizerFlagsP2 flags2, float minSpeed, float maxSpeed, float minTime, float maxTime)
        {
            bool shuffled = false;

            if (ShouldShuffle(flags, RandomizerFlags.Textures, AssetType.RWTX))
            {
                ShuffleData(r, AssetType.RWTX);
                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Boulder_Settings, AssetType.BOUL))
            {
                ShuffleBoulders(r);
                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Sounds, AssetType.SNDI))
            {
                ShuffleSounds(r, ShouldShuffle(flags2, RandomizerFlagsP2.Mix_SND_SNDS));
                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Pickup_Positions, AssetType.PKUP))
            {
                ShufflePKUPPositions(r);
                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.MovePoint_Radius, AssetType.MVPT))
            {
                ShuffleMVPT(r);
                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Tiki_Types, AssetType.VIL))
            {
                ShuffleVilTypes(r, new VilType[] {
                    VilType.tiki_shhhh_bind,
                    VilType.tiki_stone_bind,
                    VilType.tiki_thunder_bind,
                    VilType.tiki_wooden_bind },

                    ShouldShuffle(flags, RandomizerFlags.Tiki_Models),
                    ShouldShuffle(flags, RandomizerFlags.Tiki_Allow_Any_Type));

                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Enemy_Types, AssetType.VIL))
            {
                ShuffleVilTypes(r, new VilType[] {
                    VilType.g_love_bind,
                    VilType.ham_bind,
                    VilType.robot_0a_bomb_bind,
                    VilType.robot_0a_bzzt_bind,
                    VilType.robot_0a_chomper_bind,
                    VilType.robot_0a_fodder_bind,
                    VilType.robot_4a_monsoon_bind,
                    VilType.robot_9a_bind,
                    VilType.robot_chuck_bind,
                    VilType.robot_sleepytime_bind,
                    VilType.robot_tar_bind
                },
                ShouldShuffle(flags2, RandomizerFlagsP2.Enemy_Models),
                ShouldShuffle(flags2, RandomizerFlagsP2.Enemies_Allow_Any_Type));

                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Marker_Positions, AssetType.MRKR)
                && !new string[] { "hb02", "b101", "b201", "b302", "b303" }
                .Contains(LevelName))
            {
                ShuffleMRKRPositions(r, 
                    ShouldShuffle(flags, RandomizerFlags.Pointer_Positions),
                    ShouldShuffle(flags, RandomizerFlags.Player_Start),
                    ShouldShuffle(flags2, RandomizerFlagsP2.Bus_Stop_Positions),
                    ShouldShuffle(flags2, RandomizerFlagsP2.Teleport_Box_Positions),
                    ShouldShuffle(flags2, RandomizerFlagsP2.Taxi_Positions));

                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Platform_Speeds, AssetType.PLAT))
            {
                ShufflePlatSpeeds(r, minSpeed, maxSpeed, minTime, maxTime);
                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Timers, AssetType.TIMR))
            {
                ShuffleTimers(r);
                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Cameras, AssetType.CAM))
            {
                ShuffleCameras(r);
                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Disco_Floors, AssetType.DSCO))
            {
                ShuffleDisco(r);
                shuffled = true;
            }

            if (ShouldShuffle(flags2, RandomizerFlagsP2.Models, AssetType.MODL))
            {
                ShuffleData(r, AssetType.MODL);
                shuffled = true;
            }

            if (ShouldShuffle(flags2, RandomizerFlagsP2.SIMP_Positions, AssetType.SIMP))
            {
                ShuffleSIMPPositions(r);
                shuffled = true;
            }

            if (ShouldShuffle(flags, RandomizerFlags.Music))
            {
                RandomizePlaylistLocal();
                shuffled = true;
            }

            return shuffled;
        }

        private bool ShouldShuffle(RandomizerFlags flags, RandomizerFlags flag)
            => (flags & flag) != 0;
        private bool ShouldShuffle(RandomizerFlagsP2 flags, RandomizerFlagsP2 flag)
            => (flags & flag) != 0;

        private bool ShouldShuffle(RandomizerFlags flags, RandomizerFlags flag, AssetType assetType)
            => (flags & flag) != 0 && GetAssetsOfType(assetType).Any();

        private bool ShouldShuffle(RandomizerFlagsP2 flags, RandomizerFlagsP2 flag, AssetType assetType)
            => (flags & flag) != 0 && GetAssetsOfType(assetType).Any();

        private void ShuffleData(Random r, AssetType assetType)
        {
            List<Asset> assets = (from asset in assetDictionary.Values where asset.AHDR.assetType == assetType select asset).ToList();

            List<byte[]> datas = (from asset in assets where true select asset.Data).ToList();
            
            foreach (Asset a in assets)
            {
                int value = r.Next(0, datas.Count);

                a.Data = datas[value];

                datas.RemoveAt(value);
            }
        }

        private void ShufflePKUPPositions(Random r)
        {
            List<AssetPKUP> assets = (from asset in assetDictionary.Values where asset.AHDR.assetType == AssetType.PKUP select asset).Cast<AssetPKUP>().ToList();

            switch (LevelName)
            {
                case "hb02":
                    for (int i = 0; i < assets.Count; i++)
                        if (assets[i].AHDR.ADBG.assetName.Contains("RED"))
                        {
                            int shinyNum = Convert.ToInt32(assets[i].AHDR.ADBG.assetName.Split('_')[2]);
                            if (shinyNum >= 32 && shinyNum <= 47 && shinyNum != 42)
                            {
                                assets.RemoveAt(i);
                                i--;
                            }
                        }
                    break;
                case "gl01":
                    if (ContainsAsset(new AssetID("SHINY_YELLOW_004")))
                        assets.Remove((AssetPKUP)GetFromAssetID(new AssetID("SHINY_YELLOW_004")));
                    break;
                case "gl03":
                    if (ContainsAsset(0x0B48E8AC))
                    {
                        AssetDYNA dyna = (AssetDYNA)GetFromAssetID(0x0B48E8AC);
                        dyna.LinksBFBB = new LinkBFBB[0];

                        if (ContainsAsset(0xF70F6FEE))
                        {
                            ((AssetPKUP)GetFromAssetID(0xF70F6FEE)).PickupFlags = 2;
                            ((AssetPKUP)GetFromAssetID(0xF70F6FEE)).Visible = true;
                        }
                    }
                    break;
                case "jf01":
                case "bc01":
                case "rb03":
                case "sm01":
                    for (int i = 0; i < assets.Count; i++)
                    {
                        foreach (LinkBFBB link in assets[i].LinksBFBB)
                            if (link.EventSendID == EventBFBB.Mount)
                            {
                                assets.RemoveAt(i);
                                i--;
                                break;
                            }
                    }
                    break;
            }

            List<Vector3> positions = (from asset in assets where true select (new Vector3(asset.PositionX, asset.PositionY, asset.PositionZ))).ToList();

            foreach (AssetPKUP a in assets)
            {
                int value = r.Next(0, positions.Count);

                a.PositionX = positions[value].X;
                a.PositionY = positions[value].Y;
                a.PositionZ = positions[value].Z;

                positions.RemoveAt(value);
            }
        }

        private void ShuffleCameras(Random r)
        {
            List<AssetCAM> assets = (from asset in assetDictionary.Values
                                     where asset.AHDR.assetType == AssetType.CAM && asset.AHDR.ADBG.assetName != "STARTCAM"
                                     select asset).Cast<AssetCAM>().ToList();

            for (int i = 0; i < assets.Count; i++)
            {
                var whoTargets = FindWhoTargets(assets[i].AssetID);
                if (whoTargets.Count > 0 && GetFromAssetID(whoTargets[0]) is AssetDYNA dyna
                    && (dyna.Type_BFBB == DynaType_BFBB.game_object__BusStop || dyna.Type_BFBB == DynaType_BFBB.game_object__Taxi))
                {
                    assets.RemoveAt(i);
                    i--;
                }
            }

            List<Vector3> positions = (from asset in assets where true select (new Vector3(asset.PositionX, asset.PositionY, asset.PositionZ))).ToList();
            List<Vector3[]> angles = (from asset in assets
                                      where true
                                      select (new Vector3[] {
                new Vector3(asset.NormalizedForwardX, asset.NormalizedForwardY, asset.NormalizedForwardZ),
                new Vector3(asset.NormalizedUpX, asset.NormalizedUpY, asset.NormalizedUpZ),
                new Vector3(asset.NormalizedLeftX, asset.NormalizedLeftY, asset.NormalizedLeftZ),
                new Vector3(asset.ViewOffsetX, asset.ViewOffsetY, asset.ViewOffsetZ)
            })).ToList();

            foreach (AssetCAM a in assets)
            {
                int value1 = r.Next(0, positions.Count);
                int value2 = r.Next(0, angles.Count);

                a.PositionX = positions[value1].X;
                a.PositionY = positions[value1].Y;
                a.PositionZ = positions[value1].Z;

                a.NormalizedForwardX = angles[value2][0].X;
                a.NormalizedForwardY = angles[value2][0].Y;
                a.NormalizedForwardZ = angles[value2][0].Z;

                a.NormalizedUpX = angles[value2][1].X;
                a.NormalizedUpY = angles[value2][1].Y;
                a.NormalizedUpZ = angles[value2][1].Z;

                a.NormalizedLeftX = angles[value2][2].X;
                a.NormalizedLeftY = angles[value2][2].Y;
                a.NormalizedLeftZ = angles[value2][2].Z;

                a.ViewOffsetX = angles[value2][3].X;
                a.ViewOffsetY = angles[value2][3].Y;
                a.ViewOffsetZ = angles[value2][3].Z;

                positions.RemoveAt(value1);
                angles.RemoveAt(value2);
            }
        }

        private void ShuffleDisco(Random r)
        {
            List<AssetDSCO> assets = (from asset in assetDictionary.Values
                                     where asset.AHDR.assetType == AssetType.DSCO
                                     select asset).Cast<AssetDSCO>().ToList();
            
            foreach (AssetDSCO d in assets)
            {
                byte[] bytes = d.PatternController;
                r.NextBytes(bytes);
                d.PatternController = bytes;
            }
        }

        private void ShuffleSIMPPositions(Random r)
        {
            List<AssetSIMP> assets = (from asset in assetDictionary.Values where
                                      asset is AssetSIMP simp && FindWhoTargets(simp.AssetID).Count == 0
                                      select asset).Cast<AssetSIMP>().ToList();

            List<Vector3> positions = (from asset in assets where true select (new Vector3(asset.PositionX, asset.PositionY, asset.PositionZ))).ToList();
            
            foreach (AssetSIMP a in assets)
            {
                int value = r.Next(0, positions.Count);

                a.PositionX = positions[value].X;
                a.PositionY = positions[value].Y;
                a.PositionZ = positions[value].Z;

                positions.RemoveAt(value);
            }
        }
        
        private void ShuffleVilTypes(Random r, VilType[] allowed, bool mixModels, bool veryRandom)
        {
            List<AssetVIL> assets = (from asset in assetDictionary.Values where asset is AssetVIL tiki && allowed.Contains(tiki.VilType) select asset).Cast<AssetVIL>().ToList();
            List<VilType> viltypes = (from asset in assets where true select asset.VilType).ToList();
            List<AssetID> models = (from asset in assets where true select asset.Model_AssetID).ToList();
            
            foreach (AssetVIL a in assets)
            {
                int viltypes_value = r.Next(0, viltypes.Count);
                int model_value = mixModels ? r.Next(0, viltypes.Count) : viltypes_value;

                a.VilType = veryRandom ? allowed[r.Next(0, allowed.Length)] : viltypes[viltypes_value];
                a.Model_AssetID = models[model_value];
                
                viltypes.RemoveAt(viltypes_value);
                models.RemoveAt(model_value);
            }
        }

        private void ShuffleMRKRPositions(Random r, bool pointers, bool plyrs, bool busStops, bool teleBox, bool taxis)
        {
            List<IClickableAsset> assets = new List<IClickableAsset>();

            foreach (Asset a in assetDictionary.Values)
                if (a is AssetMRKR mrkr && VerifyMarkerStep1(mrkr, busStops, teleBox, taxis))
                    assets.Add(mrkr);
                else if (a is AssetDYNA dyna && pointers && dyna.Type_BFBB == DynaType_BFBB.pointer)
                    assets.Add(dyna);
                else if (plyrs && a is AssetPLYR plyr)
                    assets.Add(plyr);

            List<Vector3> positions = (from asset in assets where true select (new Vector3(asset.PositionX, asset.PositionY, asset.PositionZ))).ToList();
            
            foreach (IClickableAsset a in assets)
            {
                int value = r.Next(0, positions.Count);

                a.PositionX = positions[value].X;
                a.PositionY = positions[value].Y;
                a.PositionZ = positions[value].Z;

                positions.RemoveAt(value);
            }
        }

        private bool VerifyMarkerStep1(AssetMRKR mrkr, bool busStops, bool teleBox, bool taxis)
        {
            string assetName = mrkr.AHDR.ADBG.assetName;

            if (assetName.Contains("TEMP"))
                return false;

            List<uint> whoTargets = FindWhoTargets(mrkr.AHDR.assetID);
            if (whoTargets.Count > 0)
            {
                if (GetFromAssetID(whoTargets[0]) is AssetDYNA dyna)
                {
                    if ((busStops && dyna.Type_BFBB == DynaType_BFBB.game_object__BusStop) ||
                        (taxis && dyna.Type_BFBB == DynaType_BFBB.game_object__Taxi) ||
                        (teleBox && dyna.Type_BFBB == DynaType_BFBB.game_object__Teleport))
                        return true;
                }
                else if (GetFromAssetID(whoTargets[0]) is AssetTRIG trig)
                {
                    bool hasSetCheckpoint = false;
                    foreach (LinkBFBB link in trig.LinksBFBB)
                        if (link.EventSendID == EventBFBB.SetCheckPoint)
                            hasSetCheckpoint |= true;

                    if (hasSetCheckpoint && assetName.StartsWith("CHECKPOINT"))
                        return VerifyMarkerStep2(assetName);
                }
                else if (GetFromAssetID(whoTargets[0]) is AssetPORT)
                    return true;
                
                return false;
            }

            return VerifyMarkerStep2(assetName);
        }

        private bool VerifyMarkerStep2(string assetName)
        {
            try
            {
                if (Functions.currentGame == Game.BFBB)
                    switch (LevelName)
                    {
                        case "rb03":
                            if (assetName == "RB06MK12" || assetName == "RB06MK13")
                                return false;
                            break;
                        case "sm03":
                            if (Convert.ToInt32(assetName.Split('_')[2]) > 4)
                                return false;
                            break;
                        case "sm04":
                            if (Convert.ToInt32(assetName.Split('_')[2]) > 4)
                                return false;
                            break;
                        case "kf01":
                            if (Convert.ToInt32(assetName.Split('_')[2]) == 3)
                                return false;
                            break;
                        case "kf04":
                            if (Convert.ToInt32(assetName.Split('_')[2]) == 2)
                                return false;
                            break;
                        case "kf05":
                            if (Convert.ToInt32(assetName.Split('_')[2]) > 3)
                                return false;
                            break;
                        case "db02":
                            if (Convert.ToInt32(assetName.Split('_')[2]) > 6)
                                return false;
                            break;
                    }
                else
                {
                    if (LevelName == "r020")
                    {
                        if (assetName.Contains("FROMR003"))
                            return false;
                    }
                }
            }
            catch { }

            return true;
        }

        private void ShufflePlatSpeeds(Random r, float minMultiSpeed, float maxMultiSpeed, float minMultiTime, float maxMultiTime)
        {
            foreach (Asset a in assetDictionary.Values)
                if (a is AssetPLAT plat)
                {
                    if (plat.PlatSpecific is PlatSpecific_ConveryorBelt p)
                    {
                        p.Speed *= r.NextFloat(minMultiSpeed, maxMultiSpeed);
                        plat.PlatSpecific = p;
                    }
                    else if (plat.PlatSpecific is PlatSpecific_FallingPlatform pf)
                    {
                        pf.Speed *= r.NextFloat(minMultiSpeed, maxMultiSpeed);
                        plat.PlatSpecific = pf;
                    }
                    else if (plat.PlatSpecific is PlatSpecific_BreakawayPlatform b)
                    {
                        b.BreakawayDelay *= r.NextFloat(minMultiTime, maxMultiTime);
                        b.ResetDelay *= r.NextFloat(minMultiTime, maxMultiTime);
                        plat.PlatSpecific = b;
                    }
                    else if (plat.PlatSpecific is PlatSpecific_TeeterTotter tt)
                    {
                        tt.InverseMass *= r.NextFloat(minMultiSpeed, maxMultiSpeed);
                        plat.PlatSpecific = tt;
                    }

                    if (plat.Motion is Motion_MovePoint mp)
                    {
                        mp.Speed *= r.NextFloat(minMultiSpeed, maxMultiSpeed);
                        plat.Motion = mp;
                    }
                    else if (plat.Motion is Motion_Mechanism mc)
                    {
                        mc.PostRetractDelay *= r.NextFloat(minMultiTime, maxMultiTime);
                        mc.RetractDelay *= r.NextFloat(minMultiTime, maxMultiTime);
                        mc.RotateAccelTime *= r.NextFloat(minMultiTime, maxMultiTime);
                        mc.RotateDecelTime *= r.NextFloat(minMultiTime, maxMultiTime);
                        mc.RotateTime *= r.NextFloat(minMultiTime, maxMultiTime);
                        mc.SlideAccelTime *= r.NextFloat(minMultiTime, maxMultiTime);
                        mc.SlideDecelTime *= r.NextFloat(minMultiTime, maxMultiTime);
                        mc.SlideTime *= r.NextFloat(minMultiTime, maxMultiTime);
                        plat.Motion = mc;
                    }
                }
        }

        private void ShuffleBoulders(Random r)
        {
            float min = 0.5f;
            float max = 2f;

            foreach (Asset a in assetDictionary.Values)
                if (a is AssetBOUL boul)
                {
                    boul.Gravity *= r.NextFloat(min, max);
                    boul.Mass *= r.NextFloat(min, max);
                    boul.BounceFactor *= r.NextFloat(min, max);
                    boul.Friction *= r.NextFloat(min, max);
                    boul.StartFriction *= r.NextFloat(min, max); 
                    boul.MaxLinearVelocity *= r.NextFloat(min, max);
                    boul.MaxAngularVelocity *= r.NextFloat(min, max);
                    boul.Stickiness *= r.NextFloat(min, max);
                    boul.BounceDamp *= r.NextFloat(min, max);
                    boul.KillTimer *= r.NextFloat(min, max);
                    boul.InnerRadius *= r.NextFloat(min, max);
                    boul.OuterRadius *= r.NextFloat(min, max);
                }
        }

        public bool ShuffleSounds(Random r, bool mixTypes, bool scoobyBoot = false)
        {
            bool result = false;

            foreach (Asset a in assetDictionary.Values)
                if (a is AssetSNDI_GCN_V1 sndi)
                {
                    List<EntrySoundInfo_GCN_V1> snd = sndi.Entries_SND.ToList();
                    List<EntrySoundInfo_GCN_V1> snds = sndi.Entries_SNDS.ToList();
                    
                    if (scoobyBoot)
                    {
                        List<(byte[], byte[])> sounds = new List<(byte[], byte[])>();

                        foreach (var v in snds)
                            if (!GetFromAssetID(v.SoundAssetID).AHDR.ADBG.assetName.Contains("thera"))
                                sounds.Add((v.SoundHeader, GetFromAssetID(v.SoundAssetID).Data));

                        foreach (var v in snds)
                            if (!GetFromAssetID(v.SoundAssetID).AHDR.ADBG.assetName.Contains("thera"))
                            {
                                int index = r.Next(0, sounds.Count);
                                v.SoundHeader = sounds[index].Item1;
                                GetFromAssetID(v.SoundAssetID).Data = sounds[index].Item2;
                                sounds.RemoveAt(index);
                            }
                    }
                    else if (mixTypes)
                    {
                        List<(byte[], byte[])> sounds = new List<(byte[], byte[])>();

                        foreach (var v in snd)
                            sounds.Add((v.SoundHeader, GetFromAssetID(v.SoundAssetID).Data));

                        foreach (var v in snds)
                            sounds.Add((v.SoundHeader, GetFromAssetID(v.SoundAssetID).Data));

                        foreach (var v in snd)
                        {
                            int index = r.Next(0, sounds.Count);
                            v.SoundHeader = sounds[index].Item1;
                            GetFromAssetID(v.SoundAssetID).Data = sounds[index].Item2;
                            sounds.RemoveAt(index);
                        }

                        foreach (var v in snds)
                        {
                            int index = r.Next(0, sounds.Count);
                            v.SoundHeader = sounds[index].Item1;
                            GetFromAssetID(v.SoundAssetID).Data = sounds[index].Item2;
                            sounds.RemoveAt(index);
                        }
                    }
                    else
                    {
                        List<(byte[], byte[])> soundsSND = new List<(byte[], byte[])>();

                        foreach (var v in snd)
                            soundsSND.Add((v.SoundHeader, GetFromAssetID(v.SoundAssetID).Data));

                        foreach (var v in snd)
                        {
                            int index = r.Next(0, soundsSND.Count);
                            v.SoundHeader = soundsSND[index].Item1;
                            GetFromAssetID(v.SoundAssetID).Data = soundsSND[index].Item2;
                            soundsSND.RemoveAt(index);
                        }

                        List<(byte[], byte[])> soundsSNDS = new List<(byte[], byte[])>();

                        foreach (var v in snds)
                            soundsSNDS.Add((v.SoundHeader, GetFromAssetID(v.SoundAssetID).Data));

                        foreach (var v in snds)
                        {
                            int index = r.Next(0, soundsSNDS.Count);
                            v.SoundHeader = soundsSNDS[index].Item1;
                            GetFromAssetID(v.SoundAssetID).Data = soundsSNDS[index].Item2;
                            soundsSNDS.RemoveAt(index);
                        }
                    }

                    sndi.Entries_SND = snd.ToArray();
                    sndi.Entries_SNDS = snds.ToArray();

                    result = true;
                }

            return result;
        }
        
        private void ShuffleMVPT(Random r)
        {
            float min = 0.9f;
            float max = 1.8f;

            foreach (Asset a in assetDictionary.Values)
                if (a is AssetMVPT_Scooby mvpt)
                {
                    if (mvpt.ArenaRadius != -1)
                        mvpt.ArenaRadius *= r.NextFloat(min, max);
                    if (mvpt is AssetMVPT mvpts)
                    {
                        if (mvpts.ZoneRadius != -1)
                            mvpts.ZoneRadius *= r.NextFloat(min, max);
                        if (mvpts.Delay != -1)
                            mvpts.Delay *= r.NextFloat(min, max);
                    }
                }
        }

        private void ShuffleTimers(Random r)
        {
            foreach (Asset a in assetDictionary.Values)
                if (a is AssetTIMR timr)
                    timr.Time *= r.NextFloat(0.75f, 1.75f);
        }

        private string LevelName => Path.GetFileNameWithoutExtension(currentlyOpenFilePath).ToLower();

        private bool IsWarpToSameLevel(string warpName) =>
            LevelName.ToLower().Equals(warpName.ToLower()) || new string(warpName.Reverse().ToArray()).ToLower().Equals(LevelName.ToLower());

        public void GetWarpNames(ref List<string> warpNames, List<string> toSkip)
        {
            foreach (Asset a in assetDictionary.Values)
                if (a is AssetPORT port && !IsWarpToSameLevel(port.DestinationLevel) && !PortInToSkip(port, toSkip))                
                    warpNames.Add(port.DestinationLevel);
        }

        private bool PortInToSkip(AssetPORT port, List<string> toSkip)
        {
            string dest = port.DestinationLevel.ToLower();

            if (Functions.currentGame == Game.BFBB)
                switch (LevelName)
                {
                    case "bb01":
                        if (port.AHDR.ADBG.assetName == "TOHB01")
                            return true;
                        else if (dest == "bb03")
                            return true;
                        break;
                    case "bc01":
                        if (dest == "hb01")
                            return true;
                        break;
                    case "bc02":
                        if (dest == "bc04")
                            return true;
                        break;
                    case "hb01":
                        if (dest == "tl01")
                            return true;
                        else if (port.AHDR.ADBG.assetName == "TOJF01")
                            return true;
                        else if (port.AHDR.ADBG.assetName == "TOBB01")
                            return true;
                        else if (port.AHDR.ADBG.assetName == "TOGL01")
                            return true;
                        else if (port.AHDR.ADBG.assetName == "TOBC01")
                            return true;
                        break;
                    case "jf01":
                        if (port.AHDR.ADBG.assetName == "TAXISTAND_PORTAL_01")
                            return true;
                        break;
                    case "jf02":
                        if (dest == "jf04")
                            return true;
                        break;
                    case "jf04":
                        if (dest == "jf02")
                            return true;
                        else if (dest == "jf80")
                            return true;
                        break;
                    case "rb01":
                        if (port.AHDR.ADBG.assetName == "TOHUB_PORTAL")
                            return true;
                        break;
                    case "sm01":
                        if (port.AHDR.ADBG.assetName == "TOHB01")
                            return true;
                        break;
                }

            foreach (string s in toSkip)
                if (dest.Contains(s.ToLower()))
                    return true;
                else if (new string(dest.ToArray()).ToLower().Contains(s.ToLower()))
                    return true;

            return false;
        }

        public void SetWarpNames(Random r, ref List<string> warpNames, List<string> lines, ref List<(string, string, string)> warpRandomizerOutput)
        {
            foreach (Asset a in assetDictionary.Values)
                if (a is AssetPORT port && !IsWarpToSameLevel(port.DestinationLevel) && !PortInToSkip(port, lines))
                {
                    if (warpNames.Count == 0)
                        throw new Exception("warpNames is empty");

                    int index;
                    int times = 0;
                    do
                    { 
                        index = r.Next(0, warpNames.Count);
                        times++;
                    }
                    while (IsWarpToSameLevel(warpNames[index]) && times < 500);

                    warpRandomizerOutput.Add((LevelName.ToUpper(), port.DestinationLevel, warpNames[index]));

                    port.DestinationLevel = warpNames[index];

                    warpNames.RemoveAt(index);
                }
        }

        private string musicDispAssetName => "IP_RANDO_DISP";
        private string musicGroupAssetName => "IP_RANDO_GROUP";

        public void RandomizePlaylistLocal()
        {
            string musicDispAssetName = "MUSIC_DISP";

            if (ContainsAsset(new AssetID(musicDispAssetName)))
            {
                List<uint> assetIDs = FindWhoTargets(new AssetID(musicDispAssetName));
                foreach (uint assetID in assetIDs)
                {
                    ObjectAsset objectAsset = (ObjectAsset)GetFromAssetID(assetID);
                    LinkBFBB[] links = objectAsset.LinksBFBB;
                    foreach (LinkBFBB link in links)
                        if (link.EventSendID == EventBFBB.PlayMusic)
                        {
                            link.EventSendID = EventBFBB.Run;
                            link.Arguments_Float = new float[] { 0, 0, 0, 0 };
                            link.TargetAssetID = musicGroupAssetName;
                        }
                    objectAsset.LinksBFBB = links;
                }
            }
        }

        public bool RandomizePlaylist()
        {
            if (ContainsAsset(new AssetID(musicDispAssetName + "_01")) && ContainsAsset(new AssetID(musicGroupAssetName + "_01")))
                return false;

            int defaultLayerIndex = -1;
            for (int i = 0; i < DICT.LTOC.LHDRList.Count; i++)
                if (DICT.LTOC.LHDRList[i].layerType == (int)LayerType_BFBB.DEFAULT)
                {
                    defaultLayerIndex = i;
                    break;
                }

            List<uint> outAssetIDs = new List<uint>();
            uint dpat = PlaceTemplate(new Vector3(), defaultLayerIndex, out _, ref outAssetIDs, musicDispAssetName, template: AssetTemplate.Dispatcher);

            AssetGRUP group = (AssetGRUP)GetFromAssetID(PlaceTemplate(new Vector3(), defaultLayerIndex, out _, ref outAssetIDs, musicGroupAssetName, template: AssetTemplate.Group));
            group.ReceiveEventDelegation = AssetGRUP.Delegation.RandomItem;
            group.LinksBFBB = new LinkBFBB[]
            {
                new LinkBFBB()
                {
                    Arguments_Float = new float[] {0, 0, 0, 0},
                    EventReceiveID = EventBFBB.ScenePrepare,
                    EventSendID = EventBFBB.Run,
                    TargetAssetID = group.AssetID
                },
                new LinkBFBB()
                {
                    Arguments_Float = new float[] {0, 0, 0, 0},
                    EventReceiveID = EventBFBB.SceneEnter,
                    EventSendID = EventBFBB.Run,
                    TargetAssetID = group.AssetID
                },
                new LinkBFBB()
                {
                    Arguments_Float = new float[] {0, 0, 0, 0},
                    EventReceiveID = EventBFBB.SceneReset,
                    EventSendID = EventBFBB.Run,
                    TargetAssetID = group.AssetID
                }
            };

            outAssetIDs = new List<uint>();
            for (int i = 0; i < 17; i++)
            {
                if (i == 7 || i == 14)
                    continue;

                AssetTIMR timer = (AssetTIMR)GetFromAssetID(PlaceTemplate(new Vector3(), defaultLayerIndex, out _, ref outAssetIDs, "IP_RANDO_TIMR", template: AssetTemplate.Timer));
                timer.Time = 0.1f;
                var links = new List<LinkBFBB>()
                {
                    new LinkBFBB()
                    {
                        Arguments_Float = new float[] {i, 0, 0, 0},
                        EventReceiveID = EventBFBB.Expired,
                        EventSendID = EventBFBB.PlayMusic,
                        TargetAssetID = dpat
                    },
                    new LinkBFBB()
                    {
                        Arguments_Float = new float[] {0, 0, 0, 0},
                        EventReceiveID = EventBFBB.Expired,
                        EventSendID = EventBFBB.Reset,
                        TargetAssetID = timer.AssetID
                    },
                };
                
                timer.LinksBFBB = links.ToArray();
            }

            List<AssetID> assetIDs = new List<AssetID>();
            foreach (uint i in outAssetIDs)
                assetIDs.Add(new AssetID(i));
            group.GroupItems = assetIDs.ToArray();

            return true;
        }

        public bool DoubleLODT()
        {
            foreach (Asset a in assetDictionary.Values)
                if (a is AssetLODT lodt && lodt.AHDR.ADBG.assetFileName != "lodt_doubled")
                {
                    lodt.AHDR.ADBG.assetFileName = "lodt_doubled";

                    EntryLODT[] lodtEntries = lodt.LODT_Entries;
                    foreach (var t in lodtEntries)
                    {
                        t.MaxDistance *= 2f;
                        t.LOD1_Distance *= 2f;
                        t.LOD2_Distance *= 2f;
                        t.LOD3_Distance *= 2f;
                    }

                    lodt.LODT_Entries = lodtEntries;
                }

            return true;
        }
    }
}