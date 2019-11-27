using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

class Root
{
    public Vector3 pointA;
    public Vector3 pos;
    public List<Root> root = new List<Root>();
    public int ID = -1;
    public float rotation;
    public float width;
    float branchLenghtMul;
    public int branchLevel = -1;
    string L;

    public Root(Vector3 start, Vector3 end, float rot, float branchMul, int i)
    {
        pointA = start;
        pos = end;
        rotation = rot;
        branchLenghtMul = branchMul;
        width = Vector3.Distance(start, end) * 0.03f;
        branchLevel = i;
    }

    string calulateL(string s)
    {
        if (s == "A")
        {
            return "AB";
        }
        return "A";
    }


    public void ActivateTreeRoot()
    {
        float length = Vector3.Distance(pointA, pos) * branchLenghtMul;
        float leftRot = rotation + UnityEngine.Random.Range(TreeSpawner.minRot, TreeSpawner.maxRot);
        float rightRot = rotation - UnityEngine.Random.Range(TreeSpawner.minRot, TreeSpawner.maxRot);
        Vector3 leftV = pos + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * leftRot) * length,
            Mathf.Sin(Mathf.Deg2Rad * leftRot) * length, 0);
        Vector3 rightV = pos + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * rightRot) * length,
            Mathf.Sin(Mathf.Deg2Rad * rightRot) * length, 0);
        root.Add(new Root(pos, leftV, leftRot, branchLenghtMul, branchLevel + 1));
        root.Add(new Root(pos, rightV, rightRot, branchLenghtMul, branchLevel + 1));
    }

    public void ActivateLineRoot()
    {
        float length = Vector3.Distance(pointA, pos) * branchLenghtMul;
        Vector3 point = pos + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * rotation) * length,
            Mathf.Sin(Mathf.Deg2Rad * rotation) * length, 0);
        root.Add(new Root(pos, point, rotation, branchLenghtMul, branchLevel + 1));
    }

    public void Branch()
    {
        if (root.Count == 0)
        {
            ActivateTreeRoot();
        }
        else
        {
            foreach (Root r in root)
            {
                r.Branch();
            }
        }
    }
}

public class TreeSpawner : MonoBehaviour
{
    Mesh mesh;
    public float StartHeight = 1.0f, StartHalfWidth = 0.05f;

    Root root;
    [Range(0.01f, 0.9f)] public float branchRangeMultier = 0.8f;
    public static float minRot = 9.0f;
    public static float maxRot = 30.0f;

    int rootCount = 0;
    List<Root> roots = new List<Root>();

    public int Branchings = 1;

    private void Start()
    {
        root = new Root(new Vector3(0, -2.5f, 0), new Vector3(0, -5, 0), 90, branchRangeMultier, 0);
        roots.Add(root);
        root.ActivateLineRoot();
        CreateTree();
    }

    private void CreateTree()
    {
        for (int index = rootCount; index < Branchings; index++)
        {
            root.Branch();
        }
        Action<Root> TreeTravel = null;
        TreeTravel = (r) =>
        {
            roots.Add(r);
            r.ID = rootCount;
            rootCount++;
            if (r.root.Count > 0)
            {
                for (int i = 0; i < r.root.Count; i++)
                {
                    TreeTravel(r.root[i]);
                }
            }
        };
        TreeTravel(root);

        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mf.mesh = mesh;
        Vector3[] vertices = new Vector3[roots.Count * 2];  //   2^(branchings+2)
        Vector2[] uvs = new Vector2[roots.Count * 2];
        int[] tri = new int[12 * roots.Count];              // 6*2^(branchings+1)-1

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        Parallel.For(0, roots.Count, (int i) => {
            Vector3 pos = roots[i].pos;
            //pos.z = -roots[i].branchLevel*0.01f;
            float rotation = roots[i].rotation;

            vertices[roots[i].ID * 2] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation - 90)), Mathf.Sin(Mathf.Deg2Rad * (rotation - 90)), 0) * roots[i].width;
            vertices[roots[i].ID * 2 + 1] = pos + new Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation + 90)), Mathf.Sin(Mathf.Deg2Rad * (rotation + 90)), 0) * roots[i].width;

            float uvLever = (float)roots[i].branchLevel / (float)Branchings;
            uvs[roots[i].ID * 2] = new Vector2(0, uvLever);
            uvs[roots[i].ID * 2 + 1] = new Vector2(0, uvLever);

            if (roots[i].root.Count > 0)
            {
                for (int first = 0; first < roots[i].root.Count; first++)
                {
                    int index = first * 6;
                    tri[roots[i].ID * 12 + index] = roots[i].ID * 2;
                    tri[roots[i].ID * 12 + index + 1] = roots[i].ID * 2 + 1;
                    tri[roots[i].ID * 12 + index + 2] = roots[i].root[first].ID * 2 + 1;
                    tri[roots[i].ID * 12 + index + 3] = roots[i].ID * 2;
                    tri[roots[i].ID * 12 + index + 4] = roots[i].root[first].ID * 2 + 1;
                    tri[roots[i].ID * 12 + index + 5] = roots[i].root[first].ID * 2;
                }
            }
        });
        

        mesh.vertices = vertices;
        mesh.triangles = tri;
        mesh.uv = uvs;
    }
}

