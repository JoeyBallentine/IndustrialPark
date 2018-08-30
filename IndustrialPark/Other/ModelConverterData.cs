﻿using System.Collections.Generic;
using RenderWareFile;
using SharpDX;

namespace IndustrialPark.Models
{
    public struct ModelConverterData
    {
        public List<string> MaterialList;
        public List<Vertex> VertexList;
        public List<Vector2> UVList;
        public List<SharpDX.Color> ColorList;
        public List<Triangle> TriangleList;
        public string MTLLib;
    }
}