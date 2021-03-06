﻿using HipHopFile;
using SharpDX;
using System.Collections.Generic;
using System;
using System.Linq;
using System.ComponentModel;
using IndustrialPark.Models;

namespace IndustrialPark
{
    public class AssetMVPT_Scooby : BaseAsset, IRenderableAsset, IClickableAsset, IScalableAsset
    {
        private Matrix world;
        private BoundingBox boundingBox;

        public static bool dontRender = false;

        protected override int EventStartOffset => 0x20 + 4 * SiblingAmount;

        public AssetMVPT_Scooby(Section_AHDR AHDR, Game game, Platform platform) : base(AHDR, game, platform)
        {
            _position = new Vector3(ReadFloat(0x8), ReadFloat(0xC), ReadFloat(0x10));
            _arenaRadius = ReadFloat(0x1C);
            CreateTransformMatrix();
            ArchiveEditorFunctions.renderableAssets.Add(this);
        }

        public override bool HasReference(uint assetID)
        {
            foreach (AssetID a in NextMVPTs)
                if (a == assetID)
                    return true;

            return base.HasReference(assetID);
        }

        public override void Verify(ref List<string> result)
        {
            base.Verify(ref result);

            foreach (AssetID a in NextMVPTs)
                Verify(a, ref result);
        }

        public void CreateTransformMatrix()
        {
            if (IsZone == 1 || _arenaRadius == -1f)
                world = Matrix.RotationX(MathUtil.PiOverTwo) * Matrix.Translation(_position + new Vector3(0f, 0.5f, 0f));
            else
                world = Matrix.Scaling(_arenaRadius * 2f) * Matrix.Translation(_position);

            CreateBoundingBox();
        }

        public BoundingSphere boundingSphere;

        protected void CreateBoundingBox()
        {
            if (IsZone == 1 || _arenaRadius == -1f)
            {
                var vertices = new Vector3[SharpRenderer.pyramidVertices.Count];

                for (int i = 0; i < SharpRenderer.pyramidVertices.Count; i++)
                    vertices[i] = (Vector3)Vector3.Transform(SharpRenderer.pyramidVertices[i], world);

                boundingBox = BoundingBox.FromPoints(vertices);
                boundingSphere = BoundingSphere.FromBox(boundingBox);
            }
            else
            {
                boundingSphere = new BoundingSphere(_position, _arenaRadius);
                boundingBox = BoundingBox.FromSphere(boundingSphere);
            }
        }

        public float? GetIntersectionPosition(SharpRenderer renderer, Ray ray)
        {
            if (!ShouldDraw(renderer))
                return null;

            if (ray.Intersects(ref boundingSphere))
            {
                if (IsZone == 1 || _arenaRadius == -1f)
                    return TriangleIntersection(ray, SharpRenderer.pyramidTriangles, SharpRenderer.pyramidVertices, world);
                return TriangleIntersection(ray, SharpRenderer.sphereTriangles, SharpRenderer.sphereVertices, world);
            }
            return null;
        }

        public bool ShouldDraw(SharpRenderer renderer)
        {
            if (isSelected)
                return true;
            if (dontRender)
                return false;
            if (isInvisible)
                return false;

            if (AssetMODL.renderBasedOnLodt)
            {
                if (GetDistanceFrom(renderer.Camera.Position) < SharpRenderer.DefaultLODTDistance)
                    return renderer.frustum.Intersects(ref boundingBox);
                return false;
            }

            return renderer.frustum.Intersects(ref boundingBox);
        }

        public void Draw(SharpRenderer renderer)
        {
            if (IsZone == 1 || _arenaRadius == -1f)
                renderer.DrawPyramid(world, isSelected, 1f);
            else
                renderer.DrawSphere(world, isSelected, renderer.mvptColor);
        }

        [Browsable(false)]
        public bool SpecialBlendMode => true;

        public BoundingBox GetBoundingBox()
        {
            return boundingBox;
        }

        public float GetDistanceFrom(Vector3 cameraPosition)
        {
            return Vector3.Distance(cameraPosition, _position) - (_arenaRadius == -1f ? 0 : _arenaRadius);
        }

        protected Vector3 _position;
        [Browsable(false)]
        public Vector3 Position => new Vector3(PositionX, PositionY, PositionZ);

        [Category("Move Point"), TypeConverter(typeof(FloatTypeConverter))]
        public float PositionX
        {
            get { return _position.X; }
            set
            {
                _position.X = value;
                Write(0x8, _position.X);
                CreateTransformMatrix();
            }
        }

        [Category("Move Point"), TypeConverter(typeof(FloatTypeConverter))]
        public float PositionY
        {
            get { return _position.Y; }
            set
            {
                _position.Y = value;
                Write(0xC, _position.Y);
                CreateTransformMatrix();
            }
        }

        [Category("Move Point"), TypeConverter(typeof(FloatTypeConverter))]
        public float PositionZ
        {
            get { return _position.Z; }
            set
            {
                _position.Z = value;
                Write(0x10, _position.Z);
                CreateTransformMatrix();
            }
        }

        [Category("Move Point")]
        [TypeConverter(typeof(HexUShortTypeConverter))]
        [Description("Usually 0x2710")]
        public ushort Wt
        {
            get => ReadUShort(0x14);
            set => Write(0x14, value);
        }
        
        [Category("Move Point")]
        [TypeConverter(typeof(HexByteTypeConverter))]
        [Description("0x00 for arena (can see you), 0x01 for zone")]
        public byte IsZone
        {
            get => ReadByte(0x16);
            set => Write(0x16, value);
        }

        [Category("Move Point")]
        [TypeConverter(typeof(HexByteTypeConverter))]
        [Description("Usually 0x00")]
        public byte BezIndex
        {
            get => ReadByte(0x17);
            set => Write(0x17, value);
        }

        [Category("Move Point")]
        [TypeConverter(typeof(HexByteTypeConverter))]
        public byte Flg_Props
        {
            get => ReadByte(0x18);
            set => Write(0x18, value);
        }

        [Category("Move Point")]
        [TypeConverter(typeof(HexByteTypeConverter))]
        public byte Padding19
        {
            get => ReadByte(0x19);
            set => Write(0x19, value);
        }

        [Category("Move Point")]
        [ReadOnly(true)]
        public short SiblingAmount
        {
            get => ReadShort(0x1A);
            set => Write(0x1A, value);
        }

        protected float _arenaRadius;
        [Category("Move Point"), TypeConverter(typeof(FloatTypeConverter))]
        public virtual float ArenaRadius
        {
            get => _arenaRadius;
            set
            {
                _arenaRadius = value;
                Write(0x1C, _arenaRadius);
                CreateTransformMatrix();
            }
        }

        protected virtual int NextStartOffset => 0x20;

        [Category("Move Point")]
        public AssetID[] NextMVPTs
        {
            get
            {
                try
                {
                    AssetID[] _otherMVPTs = new AssetID[SiblingAmount];
                    for (int i = 0; i < SiblingAmount; i++)
                        _otherMVPTs[i] = ReadUInt(NextStartOffset + 4 * i);

                    return _otherMVPTs;
                }
                catch
                {
                    return new AssetID[0];
                }
            }
            set
            {
                List<byte> newData = Data.Take(NextStartOffset).ToList();
                List<byte> restOfOldData = Data.Skip(NextStartOffset + 4 * SiblingAmount).ToList();

                foreach (AssetID i in value)
                {
                    if (platform == Platform.GameCube)
                        newData.AddRange(BitConverter.GetBytes(i).Reverse());
                    else
                        newData.AddRange(BitConverter.GetBytes(i));
                }

                newData.AddRange(restOfOldData);
                
                Data = newData.ToArray();

                SiblingAmount = (short)value.Length;
            }
        }

        [Browsable(false)]
        public float ScaleX
        {
            get => GetScale();
            set => SetScale(value);
        }
        [Browsable(false)]
        public float ScaleY
        {
            get => GetScale();
            set => SetScale(value);
        }
        [Browsable(false)]
        public float ScaleZ
        {
            get => GetScale();
            set => SetScale(value);
        }

        private float GetScale()
        {
            if (IsZone == 0x00 && _arenaRadius != -1f)
                return _arenaRadius;

            return 1f;
        }

        private void SetScale(float scale)
        {
            if (IsZone == 0x00)
                ArenaRadius = scale;
        }
    }
}