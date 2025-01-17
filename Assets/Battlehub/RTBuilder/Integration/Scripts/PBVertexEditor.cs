﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Battlehub.ProBuilderIntegration
{
    public class PBVertexEditor : PBBaseEditor
    {
        private PBVertexSelection m_vertexSelection;
        private SceneSelection m_selection = new SceneSelection();
        private float m_nextUpdate;

        private int[][] m_initialIndexes;
        private Vector3[][] m_initialPositions;
        private Vector3 m_initialPostion;
        private Quaternion m_initialRotation;
        private Vector3 m_initialNormal;

        public override bool HasSelection
        {
            get { return m_vertexSelection.VerticesCount > 0; }
        }

        public override Vector3 Position
        {
            get { return CenterMode ? m_vertexSelection.CenterOfMass : m_vertexSelection.LastPosition; }
            set { MoveTo(value); }
        }

        public override Vector3 Normal
        {
            get { return (!GlobalMode && m_vertexSelection.LastMesh != null) ? m_vertexSelection.LastMesh.transform.forward : Vector3.forward; }
        }


        public override Quaternion Rotation
        {
            get
            {
                if (GlobalMode || m_vertexSelection.LastMesh == null)
                {
                    return Quaternion.identity;
                }

                MeshSelection selection = GetSelection();
                if (selection == null)
                {
                    return Quaternion.identity;
                }
                if (selection.HasVertices)
                {
                    selection.VerticesToFaces(false);
                }
                IList<int> faceIndexes;
                if (selection.SelectedFaces.TryGetValue(m_vertexSelection.LastMesh, out faceIndexes))
                {
                    if (faceIndexes.Count != 0)
                    {
                        return HandleUtility.GetRotation(m_vertexSelection.LastMesh, m_vertexSelection.LastMesh.faces[faceIndexes.Last()].distinctIndexes);
                    }
                }

                IList<int> vertices;
                if (!selection.SelectedIndices.TryGetValue(m_vertexSelection.LastMesh, out vertices) || vertices.Count == 0)
                {
                    return Quaternion.identity;
                }

                return HandleUtility.GetRotation(m_vertexSelection.LastMesh, vertices);
            }
        }

        public override GameObject Target
        {
            get { return m_vertexSelection.LastMesh != null ? m_vertexSelection.LastMesh.gameObject : null; }
        }

        private  void Awake()
        {
            m_vertexSelection = gameObject.AddComponent<PBVertexSelection>();
        }

        private void OnDestroy()
        {
            if (m_vertexSelection != null)
            {
                Destroy(m_vertexSelection);
            }
        }

        public override MeshSelection GetSelection()
        {
            MeshSelection selection = new MeshSelection();

            foreach (ProBuilderMesh mesh in m_vertexSelection.Meshes)
            {
                selection.SelectedIndices.Add(mesh, m_vertexSelection.GetVertices(mesh).ToArray());
            }
            if (selection.SelectedIndices.Count > 0)
            {
                return selection;
            }
            return null;
        }

        public override MeshSelection ClearSelection()
        {
            MeshSelection selection = new MeshSelection();

            foreach(ProBuilderMesh mesh in m_vertexSelection.Meshes)
            {
                selection.UnselectedIndices.Add(mesh, m_vertexSelection.GetVertices(mesh).ToArray());
            }

            m_vertexSelection.Clear();
            m_selection.Clear();

            if(selection.UnselectedIndices.Count > 0)
            {
                return selection;
            }
            return null;
        }

        public override void Hover(Camera camera, Vector3 pointer)
        {
            if(m_nextUpdate > Time.time)
            {
                return;
            }

            m_nextUpdate = Time.time + 0.3f;

            if(m_vertexSelection.MeshesCount == 0)
            {
                return;
            }

            float result = PBUtility.PickVertex(camera, pointer, 20, null, m_vertexSelection.Meshes, ref m_selection);
            if (result != Mathf.Infinity)
            {
                m_vertexSelection.Hover(m_selection.mesh, m_selection.vertex);
            }
            else
            {
                m_vertexSelection.Leave();
            }
        }

        public override MeshSelection Select(Camera camera, Vector3 pointer, bool shift, bool ctrl)
        {
            MeshSelection selection = null;
            GameObject pickedObject = PBUtility.PickObject(camera, pointer);
            float result = PBUtility.PickVertex(camera, pointer, 20, pickedObject, m_vertexSelection.Meshes, ref m_selection);

            if(result != Mathf.Infinity)
            {
                if(m_vertexSelection.IsSelected(m_selection.mesh, m_selection.vertex))
                {
                    if(shift)
                    {
                        List<int> indices = m_selection.mesh.GetCoincidentVertices(new[] { m_selection.vertex });
                        m_vertexSelection.Remove(m_selection.mesh, indices);
                        selection = new MeshSelection();
                        selection.UnselectedIndices.Add(m_selection.mesh, indices);
                    }
                    else
                    {
                        List<int> indices = m_selection.mesh.GetCoincidentVertices(new[] { m_selection.vertex });

                        selection = ReadSelection();
                        selection.UnselectedIndices[m_selection.mesh] = selection.UnselectedIndices[m_selection.mesh].Where(i => i != m_selection.vertex).ToArray();
                        selection.SelectedIndices.Add(m_selection.mesh, indices);
                        m_vertexSelection.Clear();
                        m_vertexSelection.Add(m_selection.mesh, indices);
                    }
                }
                else
                {
                    if(shift)
                    {
                        selection = new MeshSelection();
                    }
                    else
                    {
                        selection = ReadSelection();
                        m_vertexSelection.Clear();
                    }

                    List<int> indices = m_selection.mesh.GetCoincidentVertices(new[] { m_selection.vertex });

                    m_vertexSelection.Add(m_selection.mesh, indices);
                    selection.SelectedIndices.Add(m_selection.mesh, indices);
                }
            }
            else
            {
                if (!shift)
                {
                    selection = ReadSelection();
                    if (selection.UnselectedIndices.Count == 0)
                    {
                        selection = null;
                    }
                    m_vertexSelection.Clear(); 
                }
            }
            return selection;
        }

        private MeshSelection ReadSelection()
        {
            MeshSelection selection = new MeshSelection();
            ProBuilderMesh[] meshes = m_vertexSelection.Meshes.OrderBy(m => m == m_vertexSelection.LastMesh).ToArray();
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<int> indices = m_vertexSelection.GetVertices(mesh);
                if (indices != null)
                {
                    selection.UnselectedIndices.Add(mesh, indices.ToArray());
                }
            }
            return selection;
        }

        public override MeshSelection Select(Camera camera, Rect rect, GameObject[] gameObjects, MeshEditorSelectionMode mode)
        {
            Dictionary<ProBuilderMesh, HashSet<int>> pickResult = PBUtility.PickVertices(camera, rect, gameObjects);
            if(pickResult.Count == 0)
            {
                return null;
            }

            MeshSelection selection = new MeshSelection();
            foreach (KeyValuePair<ProBuilderMesh, HashSet<int>> kvp in pickResult)
            {
                ProBuilderMesh mesh = kvp.Key;
                HashSet<int> sharedIndexes = kvp.Value;
                IList<SharedVertex> sharedVertices = mesh.sharedVertices;

                HashSet<int> indices = new HashSet<int>();
                foreach(int sharedIndex in sharedIndexes)
                {
                    SharedVertex sharedVertex = sharedVertices[sharedIndex];
                    for(int j = 0; j < sharedVertex.Count; ++j)
                    {
                        if(!indices.Contains(sharedVertex[j]))
                        {
                            indices.Add(sharedVertex[j]);
                        }
                    }    
                }

                if (mode == MeshEditorSelectionMode.Substract || mode == MeshEditorSelectionMode.Difference)
                {

                    IList<int> selected = mesh.GetCoincidentVertices(indices).Where(index => m_vertexSelection.IsSelected(mesh, index)).ToArray();
                    selection.UnselectedIndices.Add(mesh, selected);
                    m_vertexSelection.Remove(mesh, selected);
                }

                if (mode == MeshEditorSelectionMode.Add || mode == MeshEditorSelectionMode.Difference)
                {
                    IList<int> notSelected = mesh.GetCoincidentVertices(indices).Where(index => !m_vertexSelection.IsSelected(mesh, index)).ToArray();
                    selection.SelectedIndices.Add(mesh, notSelected);
                    m_vertexSelection.Add(mesh, notSelected);
                }
            }
            return selection;
        }

        public override MeshSelection Select(Material material)
        {
            MeshSelection selection = IMeshEditorExt.Select(material);
            selection.FacesToVertices(false);

            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in selection.SelectedIndices.ToArray())
            {
                IList<int> indices = kvp.Value;
                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    int index = indices[i];
                    if (m_vertexSelection.IsSelected(kvp.Key, index))
                    {
                        indices.Remove(index);
                    }
                }

                if (indices.Count == 0)
                {
                    selection.SelectedIndices.Remove(kvp.Key);
                }
                else
                {
                    m_vertexSelection.Add(kvp.Key, indices);
                }
            }
            if (selection.SelectedIndices.Count == 0)
            {
                return null;
            }
            return selection;
        }

        public override MeshSelection Unselect(Material material)
        {
            MeshSelection selection = IMeshEditorExt.Select(material);
            selection.FacesToVertices(true);

            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in selection.UnselectedIndices.ToArray())
            {
                IList<int> indices = kvp.Value;
                for (int i = indices.Count - 1; i >= 0; i--)
                {
                    int index = indices[i];
                    if (!m_vertexSelection.IsSelected(kvp.Key, index))
                    {
                        indices.Remove(index);
                    }
                }

                if (indices.Count == 0)
                {
                    selection.UnselectedIndices.Remove(kvp.Key);
                }
                else
                {
                    m_vertexSelection.Remove(kvp.Key, indices);
                }
            }
            if (selection.UnselectedIndices.Count == 0)
            {
                return null;
            }
            return selection;
        }

        public override void SetState(MeshEditorState state)
        {
            
        }

        public override void ApplySelection(MeshSelection selection)
        {
            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in selection.UnselectedIndices)
            {
                m_vertexSelection.Remove(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in selection.SelectedIndices)
            {
                m_vertexSelection.Add(kvp.Key, kvp.Value);
            }
        }


        public override void RollbackSelection(MeshSelection selection)
        {
            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in selection.SelectedIndices)
            {
                m_vertexSelection.Remove(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<ProBuilderMesh, IList<int>> kvp in selection.UnselectedIndices)
            {
                m_vertexSelection.Add(kvp.Key, kvp.Value);
            }
        }

        private void MoveTo(Vector3 to)
        {
            Vector3 from = Position;
            Vector3 offset = to - from;

            IEnumerable<ProBuilderMesh> meshes = m_vertexSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                Vector3 localOffset = mesh.transform.InverseTransformVector(offset);
                IEnumerable<int> vertices = m_vertexSelection.GetVertices(mesh);
                mesh.TranslateVertices(vertices, localOffset);

                mesh.ToMesh();
                mesh.Refresh();
            }

            ProBuilderMesh lastMesh = m_vertexSelection.LastMesh;
            if (lastMesh != null)
            {
                m_vertexSelection.Synchronize(
                    m_vertexSelection.CenterOfMass + offset,
                    m_vertexSelection.LastPosition + offset,
                    m_vertexSelection.LastNormal);
            }

            RaisePBMeshesChanged(true);
        }

        public override void BeginRotate(Quaternion initialRotation)
        {
            m_initialPostion = m_vertexSelection.LastPosition;
            m_initialNormal = m_vertexSelection.LastNormal;
            m_initialPositions = new Vector3[m_vertexSelection.MeshesCount][];
            m_initialIndexes = new int[m_vertexSelection.MeshesCount][];
            m_initialRotation = Quaternion.Inverse(initialRotation);

            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_vertexSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                m_initialIndexes[meshIndex] = mesh.GetCoincidentVertices(m_vertexSelection.GetVertices(mesh)).ToArray();
                m_initialPositions[meshIndex] = mesh.GetVertices(m_initialIndexes[meshIndex]).Select(v => mesh.transform.TransformPoint(v.position)).ToArray();
                meshIndex++;
            }
        }

        public override void EndRotate()
        {
            m_initialPositions = null;
            m_initialIndexes = null;
        }

        public override void Rotate(Quaternion rotation)
        {
            Vector3 center = Position;
            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_vertexSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Vector3> positions = mesh.positions.ToArray();
                Vector3[] initialPositions = m_initialPositions[meshIndex];
                int[] indexes = m_initialIndexes[meshIndex];

                for (int i = 0; i < initialPositions.Length; ++i)
                {
                    Vector3 position = initialPositions[i];
                    position = center + rotation * m_initialRotation * (position - center);
                    position = mesh.transform.InverseTransformPoint(position);
                    positions[indexes[i]] = position;
                }

                mesh.positions = positions;
                mesh.Refresh();
                mesh.ToMesh();
                meshIndex++;
            }

            m_vertexSelection.Synchronize(
                m_vertexSelection.CenterOfMass,
                center + rotation * m_initialRotation * (m_initialPostion - center),
                rotation * m_initialRotation * m_vertexSelection.LastNormal);

            RaisePBMeshesChanged(true);
        }


        public override void BeginScale()
        {
            m_initialPostion = m_vertexSelection.LastPosition;
            m_initialPositions = new Vector3[m_vertexSelection.MeshesCount][];
            m_initialIndexes = new int[m_vertexSelection.MeshesCount][];

            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_vertexSelection.Meshes;
            foreach (ProBuilderMesh mesh in meshes)
            {
                m_initialIndexes[meshIndex] = mesh.GetCoincidentVertices(m_vertexSelection.GetVertices(mesh)).ToArray();
                m_initialPositions[meshIndex] = mesh.GetVertices(m_initialIndexes[meshIndex]).Select(v => mesh.transform.TransformPoint(v.position)).ToArray();
                meshIndex++;
            }
        }

        public override void EndScale()
        {
            m_initialPositions = null;
            m_initialIndexes = null;
        }

        public override void Scale(Vector3 scale, Quaternion rotation)
        {
            Vector3 center = Position;
            int meshIndex = 0;
            IEnumerable<ProBuilderMesh> meshes = m_vertexSelection.Meshes;

            foreach (ProBuilderMesh mesh in meshes)
            {
                IList<Vector3> positions = mesh.positions.ToArray();
                Vector3[] initialPositions = m_initialPositions[meshIndex];
                int[] indexes = m_initialIndexes[meshIndex];

                for (int i = 0; i < initialPositions.Length; ++i)
                {
                    Vector3 position = initialPositions[i];
                    position = center + rotation * Vector3.Scale(Quaternion.Inverse(rotation) * (position - center), scale);
                    position = mesh.transform.InverseTransformPoint(position);
                    positions[indexes[i]] = position;
                }

                mesh.positions = positions;
                mesh.Refresh();
                mesh.ToMesh();
                meshIndex++;
            }

            m_vertexSelection.Synchronize(
                m_vertexSelection.CenterOfMass,
                center + rotation * Vector3.Scale(Quaternion.Inverse(rotation) * (m_initialPostion - center), scale),
                rotation * m_initialRotation * m_vertexSelection.LastNormal);

            RaisePBMeshesChanged(true);
        }

        private void RaisePBMeshesChanged(bool positionsOnly)
        {
            foreach (PBMesh mesh in m_vertexSelection.PBMeshes)
            {
                mesh.RaiseChanged(positionsOnly);
            }
        }
    }
}
