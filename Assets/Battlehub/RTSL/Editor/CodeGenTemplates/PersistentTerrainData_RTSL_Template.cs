﻿//#define RTSL_COMPILE_TEMPLATES
#if RTSL_COMPILE_TEMPLATES
//<TEMPLATE_USINGS_START>
using ProtoBuf;
using System;
using UnityEngine;
using UnityEngine.Battlehub.SL2;
//<TEMPLATE_USINGS_END>
#endif

namespace Battlehub.RTSL.Internal
{
    using PersistentDetailPrototype = PersistentSurrogateTemplate;
    using PersistentTreeInstance = PersistentSurrogateTemplate;
    using PersistentTreePrototype = PersistentSurrogateTemplate;

    [PersistentTemplate("UnityEngine.TerrainData", new[] { "detailResolution", "detailResolutionPerPatch", "heightmapResolution", "size", "detailPrototypes", "treeInstances", "treePrototypes" },
        new[] { "UnityEngine.TreePrototype", "UnityEngine.TreeInstance", "UnityEngine.DetailPrototype" })]
    public class PersistentTerrainData_RTSL_Template : PersistentSurrogateTemplate
    {
#if RTSL_COMPILE_TEMPLATES
        //<TEMPLATE_BODY_START>
        [ProtoMember(1)]
        public int m_detailResolution;

        [ProtoMember(2)]
        public int m_detailResolutionPerPatch;

        [ProtoMember(3)]
        public int m_heightmapResolution;

        [ProtoMember(4)]
        public int m_heightMapWidth;

        [ProtoMember(5)]
        public int m_heightMapHeight;

        [ProtoMember(7)]
        public float[] m_data;

        [ProtoMember(8)]
        public PersistentVector3 m_size;

        [ProtoMember(9)]
        public PersistentDetailPrototype[] detailPrototypes;

        [ProtoMember(10)]
        public PersistentTreeInstance[] treeInstances;

        [ProtoMember(11)]
        public PersistentTreePrototype[] treePrototypes;

        [ProtoMember(12)]
        public int m_alphamapWidth;

        [ProtoMember(13)]
        public int m_alphamapHeight;

        [ProtoMember(14)]
        public int m_terrainLayersLength;

        [ProtoMember(15)]
        public float[] m_alphamaps;

        public override object WriteTo(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            TerrainData o = (TerrainData)obj;
            o.SetDetailResolution(m_detailResolution, m_detailResolutionPerPatch);
            o.heightmapResolution = m_heightmapResolution;
            o.size = m_size;

            if(detailPrototypes != null)
            {
                o.detailPrototypes = Assign(detailPrototypes, v_ => (DetailPrototype)v_);
            }
            else
            {
                o.detailPrototypes = new DetailPrototype[0];
            }

            if(treeInstances != null)
            {
                o.treeInstances = Assign(treeInstances, v_ => (TreeInstance)v_);
            }
            else
            {
                o.treeInstances = new TreeInstance[0];
            }
            
            if(treePrototypes != null)
            {
                o.treePrototypes = Assign(treePrototypes, v_ => (TreePrototype)v_);
            }
            else
            {
                o.treePrototypes = new TreePrototype[0];
            }

            if (m_data != null)
            {
                float[,] data = new float[m_heightMapWidth, m_heightMapHeight];
                Buffer.BlockCopy(m_data, 0, data, 0, m_data.Length * sizeof(float));
                o.SetHeights(0, 0, data);
            }

            if(m_alphamaps != null)
            {
                float[,,] alphamaps = new float[m_alphamapWidth, m_alphamapHeight, m_terrainLayersLength];
                Buffer.BlockCopy(m_alphamaps, 0, alphamaps, 0, m_alphamaps.Length * sizeof(float));
                o.SetAlphamaps(0, 0, alphamaps);
            }

            return base.WriteTo(obj);
        }

        public override void ReadFrom(object obj)
        {
            base.ReadFrom(obj);
            if (obj == null)
            {
                return;
            }

            TerrainData o = (TerrainData)obj;

            m_detailResolution = o.detailResolution;
            m_detailResolutionPerPatch = o.detailResolutionPerPatch;
            m_heightmapResolution = o.heightmapResolution;
            m_heightMapWidth = o.heightmapWidth;
            m_heightMapHeight = o.heightmapHeight;

            m_size = o.size;

            detailPrototypes = Assign(o.detailPrototypes, v_ => (PersistentDetailPrototype)v_);
            treeInstances = Assign(o.treeInstances, v_ => (PersistentTreeInstance)v_);
            treePrototypes = Assign(o.treePrototypes, v_ => (PersistentTreePrototype)v_);

            float[,] data = o.GetHeights(0, 0, m_heightMapWidth, m_heightMapHeight);
            m_data = new float[data.GetLength(0) * data.GetLength(1)];
            Buffer.BlockCopy(data, 0, m_data, 0, m_data.Length * sizeof(float));

            m_alphamapWidth = o.alphamapWidth;
            m_alphamapHeight = o.alphamapHeight;
            m_terrainLayersLength = o.terrainLayers.Length;

            float[,,] alphamaps = o.GetAlphamaps(0, 0, m_alphamapWidth, m_alphamapHeight);
            m_alphamaps = new float[alphamaps.GetLength(0) * alphamaps.GetLength(1) * alphamaps.GetLength(2)];
            Buffer.BlockCopy(alphamaps, 0, m_alphamaps, 0, m_alphamaps.Length * sizeof(float));
        }


        //<TEMPLATE_BODY_END>
#endif
    }
}


