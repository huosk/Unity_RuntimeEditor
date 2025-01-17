﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public static class PBAutoUVConversion
    {
        /// <summary>
        /// Sets the passed faces to use Auto or Manual UVs, and (if previously manual) splits any vertex connections.
        /// </summary>
        internal static void SetAutoUV(ProBuilderMesh mesh, Face[] faces, bool auto)
        {
            if (auto)
            {
                SetAutoAndAlignUnwrapParamsToUVs(mesh, faces.Where(x => x.manualUV));
            }
            else
            {
                foreach (Face f in faces)
                {
                    f.textureGroup = -1;
                    f.manualUV = true;
                }
            }
        }

        /// <summary>
        /// Reset the AutoUnwrapParameters of a set of faces to best match their current UV coordinates.
        /// </summary>
        /// <remarks>
        /// Auto UVs do not support distortion, so this conversion process cannot be loss-less. However as long as there
        /// is minimal skewing the results are usually very close.
        /// </remarks>
        internal static void SetAutoAndAlignUnwrapParamsToUVs(ProBuilderMesh mesh, IEnumerable<Face> facesToConvert)
        {
            var destinationTextures = mesh.textures.ToArray();
            var faces = facesToConvert as Face[] ?? facesToConvert.ToArray();

            foreach (var face in faces)
            {
                face.uv = AutoUnwrapSettings.defaultAutoUnwrapSettings;
                //face.elementGroup = -1;
                face.textureGroup = -1;
                face.manualUV = false;
            }

            mesh.RefreshUV(faces);
            //var unmodifiedProjection = mesh.texturesInternal;
            var unmodifiedProjection = mesh.textures;

            foreach (var face in faces)
            {
                //var trs = CalculateDelta(unmodifiedProjection, face.indexesInternal, destinationTextures, face.indexesInternal);
                IList<int> indexes = face.indexes;
                var trs = CalculateDelta(unmodifiedProjection, indexes, destinationTextures, indexes);
                var uv = face.uv;

                // offset is flipped for legacy reasons. people were confused that positive offsets moved the texture
                // in negative directions in the scene-view, but positive in UV window. if changed we would need to
                // write some way of upgrading the unwrap settings to account for this.
                uv.offset = -trs.translation;
                uv.rotation = trs.rotation;
                uv.scale = trs.scale;
                face.uv = uv;
            }

            mesh.RefreshUV(faces);
        }

        public struct UVTransform
        {
            public Vector2 translation;
            public float rotation;
            public Vector2 scale;

            public override string ToString()
            {
                return translation.ToString("F2") + ", " + rotation + ", " + scale.ToString("F2");
            }
        }

        static List<Vector2> s_UVTransformProjectionBuffer = new List<Vector2>(8);

        /// <summary>
        /// Returns the auto unwrap settings for a face. In cases where the face is auto unwrapped
        /// (manualUV = false), this returns the settings straight. If the face is
        /// manually unwrapped, it returns the auto unwrap settings computed from GetUVTransform.
        /// </summary>
        /// <returns></returns>
        internal static AutoUnwrapSettings GetAutoUnwrapSettings(ProBuilderMesh mesh, Face face)
        {
            if (!face.manualUV)
                return new AutoUnwrapSettings(face.uv);

            var trs = GetUVTransform(mesh, face);
            var uvSettings = AutoUnwrapSettings.defaultAutoUnwrapSettings;
            uvSettings.offset = trs.translation;
            uvSettings.rotation = 360 - trs.rotation;
            uvSettings.scale = uvSettings.scale / trs.scale;

            return uvSettings;
        }

        /// <summary>
        /// Attempt to calculate the UV transform for a face. In cases where the face is auto unwrapped
        /// (manualUV = false), this returns the offset, rotation, and scale from <see cref="Face.uv"/>. If the face is
        /// manually unwrapped, a transform will be calculated by trying to match an unmodified planar projection to the
        /// current UVs. The results
        /// </summary>
        /// <returns></returns>
        internal static UVTransform GetUVTransform(ProBuilderMesh mesh, Face face)
        {
            IList<Vector3> positions = mesh.positions;
            IList<Vector2> textures = mesh.textures;
            IList<int> indexes = face.indexes;

            //Projection.PlanarProject(mesh.positionsInternal, face.indexesInternal, Math.Normal(mesh, face), s_UVTransformProjectionBuffer);
            //return CalculateDelta(mesh.texturesInternal, face.indexesInternal, s_UVTransformProjectionBuffer, null);

            s_UVTransformProjectionBuffer = Projection.PlanarProject(positions, indexes, Math.Normal(mesh, face)).ToList();
            return CalculateDelta(textures, indexes, s_UVTransformProjectionBuffer, null);
        }



        // messy hack to support cases where you want to iterate a collection of values with an optional collection of
        // indices. do not make public.
        static int GetIndex(IList<int> collection, int index)
        {
            return collection == null ? index : collection[index];
        }

        // indices arrays are optional - if null is passed the index will be 0, 1, 2... up to values array length.
        // this is done to avoid allocating a separate array just to pass linear indices
        internal static UVTransform CalculateDelta(IList<Vector2> src, IList<int> srcIndices, IList<Vector2> dst, IList<int> dstIndices)
        {
            // rotate to match target points by comparing the angle between old UV and new auto projection
            Vector2 dstAngle = dst[GetIndex(dstIndices, 1)] - dst[GetIndex(dstIndices, 0)];
            Vector2 srcAngle = src[GetIndex(srcIndices, 1)] - src[GetIndex(srcIndices, 0)];

            float rotation = Vector2.Angle(dstAngle, srcAngle);

            if (Vector2.Dot(Vector2.Perpendicular(dstAngle), srcAngle) < 0)
                rotation = 360f - rotation;

            Vector2 dstCenter = dstIndices == null ? PBBounds2D.Center(dst) : PBBounds2D.Center(dst, dstIndices);

            // inverse the rotation to get an axis-aligned scale
            Vector2 dstSize = GetRotatedSize(dst, dstIndices, dstCenter, -rotation);

            var srcBounds = srcIndices == null ? new PBBounds2D(src) : new PBBounds2D(src, srcIndices);

            return new UVTransform()
            {
                translation = dstCenter - srcBounds.center,
                rotation = rotation,
                scale = dstSize.DivideBy(srcBounds.size)
            };
        }

        static Vector2 GetRotatedSize(IList<Vector2> points, IList<int> indices, Vector2 center, float rotation)
        {
            int size = indices == null ? points.Count : indices.Count;

            Vector2 point = points[GetIndex(indices, 0)].RotateAroundPoint(center, rotation);

            float xMin = point.x;
            float yMin = point.y;
            float xMax = xMin;
            float yMax = yMin;

            for (int i = 1; i < size; i++)
            {
                point = points[GetIndex(indices, i)].RotateAroundPoint(center, rotation);

                float x = point.x;
                float y = point.y;

                if (x < xMin) xMin = x;
                if (x > xMax) xMax = x;

                if (y < yMin) yMin = y;
                if (y > yMax) yMax = y;
            }

            return new Vector2(xMax - xMin, yMax - yMin);
        }
    }

}
