//using System.Collections;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class Tree3D : MonoBehaviour
//{
//    Mesh mesh;
//    public float StartHeight = 1.0f, StartHalfWidth = 0.05f;

//    Root root;
//    [Range(0.01f, 0.9f)] public float branchRangeMultier = 0.8f;
//    public static float minRot = 9.0f;
//    public static float maxRot = 30.0f;

//    int rootCount = 0;
//    List<Root> roots = new List<Root>();

//    public int Branchings = 1;

//    private void Start()
//    {
//        root = new Root(new Vector3(0, -5, 0), new Vector3(0, -3, 0), 90, branchRangeMultier, 0);
//        CreateTree();
//    }

//    private void CreateTree()
//    {
//        for (int index = rootCount; index < Branchings; index++)
//        {
//            root.Branch();
//        }
//        Action<Root> TreeTravel = null;
//        TreeTravel = (r) =>
//        {
//            roots.Add(r);
//            r.ID = rootCount;
//            rootCount++;
//            if (r.left != null || r.right != null)
//            {
//                TreeTravel(r.left);
//                TreeTravel(r.right);
//            }
//        };
//        TreeTravel(root);

//        MeshFilter mf = GetComponent<MeshFilter>();
//        mesh = new Mesh();
//        mf.mesh = mesh;
//        Vector3[] vertices = new Vector3[2 + roots.Count * 2];  //   2^(branchings+2)
//        Vector2[] uvs = new Vector2[2 + roots.Count * 2];
//        int[] tri = new int[6 + 12 * roots.Count];              // 6*2^(branchings+1)-1
//        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

//        for (int i = 0; i < roots.Count; i++)
//        {
//            Vector3 pos = roots[i].pos;
//            float rotation = roots[i].rotation;
//            vertices[roots[i].ID * 2] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation - 90)), Mathf.Sin(Mathf.Deg2Rad * (rotation - 90)), 0) * roots[i].width;
//            vertices[roots[i].ID * 2 + 1] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation + 90)), Mathf.Sin(Mathf.Deg2Rad * (rotation + 90)), 0) * roots[i].width;
//            float uvLever = (float)roots[i].branchLevel / (float)Branchings;
//            uvs[roots[i].ID * 2] = new Vector2(0, uvLever);
//            uvs[roots[i].ID * 2 + 1] = new Vector2(0, uvLever);

//            if (roots[i].left != null)
//            {
//                tri[roots[i].ID * 12] = roots[i].ID * 2;
//                tri[roots[i].ID * 12 + 1] = roots[i].ID * 2 + 1;
//                tri[roots[i].ID * 12 + 2] = roots[i].left.ID * 2 + 1;
//                tri[roots[i].ID * 12 + 3] = roots[i].ID * 2;
//                tri[roots[i].ID * 12 + 4] = roots[i].left.ID * 2 + 1;
//                tri[roots[i].ID * 12 + 5] = roots[i].left.ID * 2;

//                tri[roots[i].ID * 12 + 6] = roots[i].ID * 2;
//                tri[roots[i].ID * 12 + 7] = roots[i].ID * 2 + 1;
//                tri[roots[i].ID * 12 + 8] = roots[i].right.ID * 2 + 1;
//                tri[roots[i].ID * 12 + 9] = roots[i].ID * 2;
//                tri[roots[i].ID * 12 + 10] = roots[i].right.ID * 2 + 1;
//                tri[roots[i].ID * 12 + 11] = roots[i].right.ID * 2;
//            }
//        }
//        int ind = roots.Count;
//        vertices[ind * 2] = new Vector3(StartHalfWidth, -6, 0);
//        vertices[ind * 2 + 1] = new Vector3(-StartHalfWidth, -6, 0);

//        uvs[ind * 2] = Vector2.zero;
//        uvs[ind * 2 + 1] = Vector2.zero;

//        tri[ind * 12] = ind * 2;
//        tri[ind * 12 + 1] = ind * 2 + 1;
//        tri[ind * 12 + 2] = 1;
//        tri[ind * 12 + 3] = ind * 2;
//        tri[ind * 12 + 4] = 1;
//        tri[ind * 12 + 5] = 0;

//        mesh.vertices = vertices;
//        mesh.triangles = tri;
//        mesh.uv = uvs;
//    }
//}
