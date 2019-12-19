using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

class Root
{
    public Vector3 position;
    public List<Root> root = new List<Root>();
    public Root previous;
    public int ID = -1;
    public float rotation;
    public float width = 0.05f;
    public float length;
    float branchLenghtMul;
    public int branchLevel = -1;
    public float branchStrenght = -1;
    public int lengthFromEnd;
    public bool flipOverride = false;


    public Root(Vector3 pos, float rot, float branchMul, int i, float lenght, Root previous = null)
    {
        this.position = pos;
        rotation = rot;
        branchLenghtMul = branchMul;
        branchLevel = i;
        this.length = lenght * branchLenghtMul;
        width = length * 0.1f;
        this.previous = previous;
    }
    public float GetEdgeRotation()
    {
        if (previous == null) return rotation;
        float rot = Vector2.SignedAngle(previous.position, position);

        return rot;
    }


    public void ActivateTreeRoot(bool flip, float leftRotation = 0, float rightRotation = 0, bool flipBool = false)
    {
        float leftRot = 0;
        float rightRot = 0;
        if (flipBool && !flipOverride)
        {
            leftRot = flip ? rotation - leftRotation : rotation + rightRotation;
            rightRot = flip ? rotation - rightRotation : rotation + leftRotation;
        }
        else
        {
            leftRot = rotation - leftRotation;
            rightRot = rotation + rightRotation;
        }
        Vector3 leftV = position + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * leftRot) * length,
            Mathf.Sin(Mathf.Deg2Rad * leftRot) * length);
        Vector3 rightV = position + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * rightRot) * length,
            Mathf.Sin(Mathf.Deg2Rad * rightRot) * length);
        root.Add(new Root(leftV, leftRot, branchLenghtMul, branchLevel + 1, length, this));
        root.Add(new Root(rightV, rightRot, branchLenghtMul, branchLevel + 1, length, this));
    }

    public void ActivateLineRoot()
    {
        Vector3 point = position + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * rotation) * length * 1,
            Mathf.Sin(Mathf.Deg2Rad * rotation) * length * 1);
        root.Add(new Root(point, rotation, branchLenghtMul, branchLevel + 1, length, this));
    }

    public void Branch(bool flip, float left = 0, float right = 0, bool flipbool = false)
    {
        if (flipbool)
        {
            flip = !flip;
        }
        if (root.Count == 0)
        {
            ActivateTreeRoot(flip, left, right, flipbool);
        }
        else
        {
            foreach (Root r in root)
            {
                r.Branch(flip, left, right, flipbool);
            }
        }
    }
}

public class TreeSpawner : MonoBehaviour
{
    public bool flipBranches = false;
    public float StartHeight = 1.0f;
    float height;
    public float heightMultiplier = 1.0f;
    public float leftRotation = 30.0f;
    public float rightRotation = 30.0f;
    //[Range(0.01f, 1.0f)] public float branchRangeMultier = 0.8f;
    public float branchWidth = 0.5f;
    public int Branchings = 1;
    //public int cleanupFactor = 60;
    //public int edgeCutt = 6;
    public float leafSize = 1.0f;


    //public GameObject gObject;

    int rootCount = 0;
    List<Root> roots = new List<Root>();
    Mesh mesh;
    Root root;
    string endString;

    //Start yo
    private void Start()
    {
        makeNewTree();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            makeNewTree();
        }
    }
    void makeNewTree()
    {
        height = StartHeight;
        rootCount = 0;
        roots = new List<Root>();
        root = new Root(new Vector3(0, 0), 90, heightMultiplier, 0, height);
        root.ActivateLineRoot();
        root.root[0].flipOverride = true;
        for (int i = 0; i < Branchings; i++)
        {
            root.Branch(false, leftRotation, rightRotation, flipBranches);
        }

        CreateTree();
    }

    int getDepth(Root r)
    {
        if (r.root.Count == 0)
        {
            r.lengthFromEnd = 0;
            return 0;
        }

        r.lengthFromEnd = 1;
        int i = 1;
        foreach (Root ro in r.root)
        {
            int temp = getDepth(ro);
            if (temp > i) i = temp;
        }
        r.lengthFromEnd += i;
        return r.lengthFromEnd;
    }

    //void cleanupTree(Root r)
    //{
    //    for (int i = r.root.Count - 1; i >= 0; i--)
    //    {
    //        cleanupTree(r.root[i]);
    //        if (r.root[i].lengthFromEnd < r.lengthFromEnd - cleanupFactor || r.root[i].lengthFromEnd < edgeCutt)
    //        {
    //            r.root.RemoveAt(i);
    //        }
    //    }
    //}

    private void CreateTree()
    {
        int maxDepth = getDepth(root);
        //cleanupTree(root);
        int maxBranches = 0;
        Action<Root> TreeTravel = null;
        List<Vector3> leavePositions = new List<Vector3>();
        TreeTravel = (r) =>
        {
            roots.Add(r);
            r.ID = rootCount;
            rootCount++;
            if (r.lengthFromEnd == 0)
            {
                leavePositions.Add(r.position);
            }
            maxBranches = maxBranches < r.root.Count ? r.root.Count : maxBranches;
            foreach (Root ro in r.root)
            {
                TreeTravel(ro);
            }
            if (r.previous != null)
            {
                r.branchStrenght = (float)(r.lengthFromEnd + r.previous.lengthFromEnd) / 2;
            }
            else
            {
                r.branchStrenght = 0;
            }
        };
        TreeTravel(root);
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mf.mesh = mesh;
        Vector3[] vertices = new Vector3[roots.Count * 3 + leavePositions.Count * 4];  //   2^(branchings+2)
        Vector2[] uvs = new Vector2[roots.Count * 3 + leavePositions.Count * 4];    //UV1 is used for texture mapping
        Vector2[] uvs2 = new Vector2[roots.Count * 3 + leavePositions.Count * 4];   //UV2 maps branch strenght for wind physics
        Vector2[] uvs3 = new Vector2[roots.Count * 3 + leavePositions.Count * 4];   //UV3 maps leave textures


        maxBranches *= 9;
        int[] tri = Enumerable.Repeat(-1, maxBranches * roots.Count + leavePositions.Count * 6).ToArray();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Parallel.For(0, roots.Count, (int i) =>
        {
            Vector3 pos = roots[i].position;
            float rotation = roots[i].rotation;

            float uvLever = (float)roots[i].lengthFromEnd / (float)maxDepth;
            uvs[roots[i].ID * 3] = new Vector2(0, 1.0f - uvLever);
            uvs[roots[i].ID * 3 + 1] = new Vector2(0, 1.0f - uvLever);
            uvs[roots[i].ID * 3 + 2] = new Vector2(0, 1.0f - uvLever);

            int depth = roots[i].lengthFromEnd == 0 ? 1 : 0;

            vertices[roots[i].ID * 3] = pos + new
            Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation - 90)),
            Mathf.Sin(Mathf.Deg2Rad * (rotation - 90)), -depth) * (uvLever + 0.02f) * branchWidth;

            vertices[roots[i].ID * 3 + 1] = pos + new
            Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation + 90)),
            Mathf.Sin(Mathf.Deg2Rad * (rotation + 90)), -depth) * (uvLever + 0.02f) * branchWidth;

            vertices[roots[i].ID * 3 + 2] = pos + new
            Vector3(Mathf.Cos(Mathf.Deg2Rad * rotation),
            Mathf.Sin(Mathf.Deg2Rad * rotation), -depth) * (uvLever + 0.02f) * branchWidth;

            float branchStrenght = 1 - (float)roots[i].branchStrenght / (float)maxDepth;
            if (roots[i].branchStrenght == 0) branchStrenght = 0;
            uvs2[roots[i].ID * 3] = new Vector2(branchStrenght, branchStrenght);
            uvs2[roots[i].ID * 3 + 1] = new Vector2(branchStrenght, branchStrenght);
            uvs2[roots[i].ID * 3 + 2] = new Vector2(branchStrenght, branchStrenght);

            uvs3[roots[i].ID * 3 + 0] = new Vector2(0, 0);
            uvs3[roots[i].ID * 3 + 1] = new Vector2(0, 0);
            uvs3[roots[i].ID * 3 + 2] = new Vector2(0, 0);

            if (roots[i].root.Count == 2)
            {
                tri[roots[i].ID * 15 + 0] = roots[i].ID * 3 + 1;
                tri[roots[i].ID * 15 + 1] = roots[i].ID * 3 + 2;
                tri[roots[i].ID * 15 + 2] = roots[i].ID * 3 + 0;

                tri[roots[i].ID * 15 + 3] = roots[i].ID * 3;
                tri[roots[i].ID * 15 + 4] = roots[i].ID * 3 + 2;
                tri[roots[i].ID * 15 + 5] = roots[i].root[0].ID * 3 + 1;

                tri[roots[i].ID * 15 + 6] = roots[i].ID * 3;
                tri[roots[i].ID * 15 + 7] = roots[i].root[0].ID * 3 + 1;
                tri[roots[i].ID * 15 + 8] = roots[i].root[0].ID * 3;

                tri[roots[i].ID * 15 + 9] = roots[i].ID * 3 + 1;
                tri[roots[i].ID * 15 + 10] = roots[i].root[1].ID * 3 + 1;
                tri[roots[i].ID * 15 + 11] = roots[i].ID * 3 + 2;

                tri[roots[i].ID * 15 + 12] = roots[i].ID * 3 + 2;
                tri[roots[i].ID * 15 + 13] = roots[i].root[1].ID * 3 + 1;
                tri[roots[i].ID * 15 + 14] = roots[i].root[1].ID * 3;
            }
            else if (roots[i].root.Count == 1)
            {
                tri[roots[i].ID * 15 + 0] = roots[i].ID * 3;
                tri[roots[i].ID * 15 + 1] = roots[i].ID * 3 + 1;
                tri[roots[i].ID * 15 + 2] = roots[i].root[0].ID * 3 + 1;

                tri[roots[i].ID * 15 + 3] = roots[i].ID * 3;
                tri[roots[i].ID * 15 + 4] = roots[i].root[0].ID * 3 + 1;
                tri[roots[i].ID * 15 + 5] = roots[i].root[0].ID * 3 + 0;
            }
        });
        Parallel.For(0, leavePositions.Count, (int i) =>
        {
            vertices[roots.Count * 3 + i * 4] = leavePositions[i] + new Vector3(leafSize * 0.5f, -leafSize * 0.5f, 0);
            vertices[roots.Count * 3 + i * 4 + 1] = leavePositions[i] + new Vector3(-leafSize * 0.5f, -leafSize * 0.5f, 0);
            vertices[roots.Count * 3 + i * 4 + 2] = leavePositions[i] + new Vector3(-leafSize * 0.5f, leafSize * 0.5f, 0);
            vertices[roots.Count * 3 + i * 4 + 3] = leavePositions[i] + new Vector3(leafSize * 0.5f, leafSize * 0.5f, 0);

            float strenght = 1 - 1 / (float)maxDepth;
            uvs2[roots.Count * 3 + i * 4] = new Vector2(1, 1);
            uvs2[roots.Count * 3 + i * 4 + 1] = new Vector2(1, 1);
            uvs2[roots.Count * 3 + i * 4 + 2] = new Vector2(1, 1);
            uvs2[roots.Count * 3 + i * 4 + 3] = new Vector2(1, 1);

            uvs3[roots.Count * 3 + i * 4] = new Vector2(1, 0);
            uvs3[roots.Count * 3 + i * 4 + 1] = new Vector2(0, 0);
            uvs3[roots.Count * 3 + i * 4 + 2] = new Vector2(0, 1);
            uvs3[roots.Count * 3 + i * 4 + 3] = new Vector2(1, 1);


            tri[roots.Count * 15 + i * 6] = roots.Count * 3 + i * 4;
            tri[roots.Count * 15 + i * 6 + 1] = roots.Count * 3 + i * 4 + 1;
            tri[roots.Count * 15 + i * 6 + 2] = roots.Count * 3 + i * 4 + 2;

            tri[roots.Count * 15 + i * 6 + 3] = roots.Count * 3 + i * 4;
            tri[roots.Count * 15 + i * 6 + 4] = roots.Count * 3 + i * 4 + 2;
            tri[roots.Count * 15 + i * 6 + 5] = roots.Count * 3 + i * 4 + 3;
        });
        Debug.Log(vertices.Count());
        tri = tri.Where(x => x > -1).ToArray();
        mesh.vertices = vertices;
        mesh.triangles = tri;
        mesh.uv = uvs;      //UV1 is used for texture mapping
        mesh.uv2 = uvs2;    //UV2 maps branch strenght for wind physics
        mesh.uv3 = uvs3;    //UV3 maps leave textures

    }

}

