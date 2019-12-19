using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

struct Rule
{
    public char rule;
    public string result;
}
struct RuleContainer
{
    public string startString;
    public List<Rule> rules;
    public Action<string> translation;
}
public class CreateLSystemTree : MonoBehaviour
{
    public enum Models { fractalTree = 0, LsystemTree, plant }
    public Models modelChoice = Models.LsystemTree;
    int ModelChoice
    {
        get { return (int)modelChoice; }
    }
    public float StartHeight = 1.0f;
    float branchLength;
    //[Range(0.01f, 1.0f)] public float branchRangeMultier = 0.8f;
    public float branchWidth = 0.5f;
    public float rotationOffset = 0;
    public int Branchings = 1;
    public int cleanupFactor = 60;
    public int edgeCutt = 6;
    public float leafSize = 1.0f;

    public static float minRot = 9.0f;
    public static float maxRot = 30.0f;

    //public GameObject gObject;

    int rootCount = 0;
    List<Root> roots = new List<Root>();
    RuleContainer[] rules = new RuleContainer[1];
    Mesh mesh;
    Root root;

    Stack<Root> rootStates = new Stack<Root>();
    Stack<float> rotationStates = new Stack<float>();
    Root currentRoot;
    string endString;

    //Start yo
    private void Start()
    {
        rules = new RuleContainer[Enum.GetNames(typeof(Models)).Length];
        rules[(int)Models.fractalTree] = new RuleContainer()
        {
            startString = "0",
            rules = new List<Rule>()
            {
                new Rule(){ rule ='1', result ="11"},
                new Rule(){ rule ='0', result ="1[0]0"},
            },
            translation = (str) =>
            {
                foreach (char c in str)
                {
                    switch (c)
                    {
                        case '0':
                            currentRoot.ActivateLineRoot();
                            break;

                        case '1':
                            currentRoot.ActivateLineRoot();
                            currentRoot = currentRoot.root[currentRoot.root.Count - 1];
                            break;
                        case '[':
                            rootStates.Push(currentRoot);
                            rotationStates.Push(currentRoot.rotation);
                            currentRoot.rotation -= 45;
                            break;
                        case ']':
                            currentRoot = rootStates.Pop();
                            currentRoot.rotation = rotationStates.Pop();
                            currentRoot.rotation += 45;
                            break;
                    }
                }
            }
        };
        rules[(int)Models.plant] = new RuleContainer()
        {
            startString = "X",
            rules = new List<Rule>()
            {
                new Rule(){ rule ='X', result ="F+[[X]-X]-F[-FX]+X"},
                new Rule(){ rule ='F', result ="FF"},
            },
            translation = (str) =>
            {
                foreach (char c in str)
                {
                    switch (c)
                    {
                        case 'F':
                            currentRoot.ActivateLineRoot();
                            currentRoot = currentRoot.root[currentRoot.root.Count - 1];
                            break;

                        case '-':
                            currentRoot.rotation -= 25;
                            break;

                        case '+':
                            currentRoot.rotation += 25;
                            break;

                        case '[':
                            rootStates.Push(currentRoot);
                            rotationStates.Push(currentRoot.rotation);
                            break;
                        case ']':
                            currentRoot = rootStates.Pop();
                            currentRoot.rotation = rotationStates.Pop();
                            break;
                    }
                }
            }
        };
        rules[(int)Models.LsystemTree] = new RuleContainer()
        {
            startString = "F",
            rules = new List<Rule>()
            {
                new Rule(){
            rule = 'F',
            result = "FF+[+F-F-F]-[-F+F+F]"}
            },
            translation = (str) =>
            {
                foreach (char c in str)
                {
                    switch (c)
                    {
                        case 'F':
                            currentRoot.ActivateLineRoot();
                            currentRoot = currentRoot.root[currentRoot.root.Count - 1];
                            break;
                        case '+':
                            currentRoot.rotation += UnityEngine.Random.Range(23.5f, 26.5f);
                            break;
                        case '-':
                            currentRoot.rotation -= UnityEngine.Random.Range(23.5f, 26.5f);
                            break;
                        case '[':
                            rootStates.Push(currentRoot);
                            rotationStates.Push(currentRoot.rotation);
                            break;
                        case ']':
                            currentRoot = rootStates.Pop();
                            currentRoot.rotation = rotationStates.Pop();
                            break;
                    }
                }
            }
        };

        endString = rules[ModelChoice].startString;
        for (int i = 0; i < Branchings; i++)
        {
            endString = calulateL(rules[ModelChoice].rules, endString);
        }
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
        branchLength = StartHeight;
        rootCount = 0;
        rootStates = new Stack<Root>();
        rotationStates = new Stack<float>();
        roots = new List<Root>();
        root = new Root(new Vector3(0, 0), 90, 1, 0, branchLength);
        currentRoot = root;
        rules[ModelChoice].translation(endString);
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

    void cleanupTree(Root r)
    {
        for (int i = r.root.Count - 1; i >= 0; i--)
        {
            cleanupTree(r.root[i]);
            if (r.root[i].lengthFromEnd < r.lengthFromEnd - cleanupFactor || r.root[i].lengthFromEnd < edgeCutt)
            {
                r.root.RemoveAt(i);
            }
        }
    }

    string calulateL(List<Rule> rules, string s)
    {
        branchLength *= 0.5f;
        string returnString = "";
        foreach (char c in s)
        {
            foreach (Rule rule in rules)
            {
                if (rule.rule == c)
                {
                    returnString += rule.result;
                    goto DoubleBreak;
                }
            }
            returnString += c;
        DoubleBreak:;
        }
        return returnString;
    }

    private void CreateTree()
    {
        int maxDepth = getDepth(root);
        cleanupTree(root);
        int maxBranches = 0;
        Action<Root> TreeTravel = null;
        List<Vector3> leavePositions = new List<Vector3>();
        TreeTravel = (r) =>
        {
            roots.Add(r);
            r.ID = rootCount;
            rootCount++;
            if (r.lengthFromEnd == edgeCutt)
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
                r.branchStrenght = (float)(r.lengthFromEnd + r.root[0].lengthFromEnd) / 2;
            }
        };
        TreeTravel(root);
        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mf.mesh = mesh;
        Vector3[] vertices = new Vector3[roots.Count * 2 + leavePositions.Count * 4];  //   2^(branchings+2)
        Vector2[] uvs = new Vector2[roots.Count * 2 + leavePositions.Count * 4];    //UV1 is used for texture mapping
        Vector2[] uvs2 = new Vector2[roots.Count * 2 + leavePositions.Count * 4];   //UV2 maps branch strenght for wind physics
        Vector2[] uvs3 = new Vector2[roots.Count * 2 + leavePositions.Count * 4];   //UV3 maps leave textures


        maxBranches *= 6;
        int[] tri = Enumerable.Repeat(-1, maxBranches * roots.Count + leavePositions.Count * 6).ToArray();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Parallel.For(0, roots.Count, (int i) =>
        {
            Vector3 pos = roots[i].position;
            float rotation = roots[i].rotation + rotationOffset;

            float uvLever = (float)roots[i].lengthFromEnd / (float)maxDepth;
            uvs[roots[i].ID * 2] = new Vector2(0, 1.0f - uvLever);
            uvs[roots[i].ID * 2 + 1] = new Vector2(0, 1.0f - uvLever);

            int depth = roots[i].lengthFromEnd == 0 ? 1 : 0;

            vertices[roots[i].ID * 2] = pos + new
            Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation - 90)),
            Mathf.Sin(Mathf.Deg2Rad * (rotation - 90)), -depth) * (uvLever + 0.02f) * branchWidth;

            vertices[roots[i].ID * 2 + 1] = pos + new
            Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation + 90)),
            Mathf.Sin(Mathf.Deg2Rad * (rotation + 90)), -depth) * (uvLever + 0.02f) * branchWidth;

            float branchStrenght = 1 - (float)roots[i].branchStrenght / (float)maxDepth;
            uvs2[roots[i].ID * 2] = new Vector2(0, branchStrenght);
            uvs2[roots[i].ID * 2 + 1] = new Vector2(0, branchStrenght);

            uvs3[roots[i].ID * 2 + 1] = new Vector2(0, 0);
            uvs3[roots[i].ID * 2 + 1] = new Vector2(0, 0);

            for (int first = 0; first < roots[i].root.Count; first++)
            {
                int index = first * 6;
                tri[roots[i].ID * maxBranches + index] = roots[i].ID * 2;
                tri[roots[i].ID * maxBranches + index + 1] = roots[i].ID * 2 + 1;
                tri[roots[i].ID * maxBranches + index + 2] = roots[i].root[first].ID * 2 + 1;

                tri[roots[i].ID * maxBranches + index + 3] = roots[i].ID * 2;
                tri[roots[i].ID * maxBranches + index + 4] = roots[i].root[first].ID * 2 + 1;
                tri[roots[i].ID * maxBranches + index + 5] = roots[i].root[first].ID * 2;
            }
        });
        float strength = 1 - 1 / (float)(maxDepth- edgeCutt);
        Parallel.For(0, leavePositions.Count, (int i) =>
        {
            vertices[roots.Count * 2 + i * 4] = leavePositions[i] + new Vector3(leafSize * 0.5f, -leafSize * 0.5f, 0);
            vertices[roots.Count * 2 + i * 4 + 1] = leavePositions[i] + new Vector3(-leafSize * 0.5f, -leafSize * 0.5f, 0);
            vertices[roots.Count * 2 + i * 4 + 2] = leavePositions[i] + new Vector3(-leafSize * 0.5f, leafSize * 0.5f, 0);
            vertices[roots.Count * 2 + i * 4 + 3] = leavePositions[i] + new Vector3(leafSize * 0.5f, leafSize * 0.5f, 0);

            uvs2[roots.Count * 2 + i * 4] = new Vector2(strength, strength);
            uvs2[roots.Count * 2 + i * 4 + 1] = new Vector2(strength, strength);
            uvs2[roots.Count * 2 + i * 4 + 2] = new Vector2(strength, strength);
            uvs2[roots.Count * 2 + i * 4 + 3] = new Vector2(strength, strength);

            uvs3[roots.Count * 2 + i * 4] = new Vector2(1, 0);
            uvs3[roots.Count * 2 + i * 4 + 1] = new Vector2(0, 0);
            uvs3[roots.Count * 2 + i * 4 + 2] = new Vector2(0, 1);
            uvs3[roots.Count * 2 + i * 4 + 3] = new Vector2(1, 1);


            tri[roots.Count * 6 + i * 6] = roots.Count * 2 + i * 4;
            tri[roots.Count * 6 + i * 6 + 1] = roots.Count * 2 + i * 4 + 1;
            tri[roots.Count * 6 + i * 6 + 2] = roots.Count * 2 + i * 4 + 2;

            tri[roots.Count * 6 + i * 6 + 3] = roots.Count * 2 + i * 4;
            tri[roots.Count * 6 + i * 6 + 4] = roots.Count * 2 + i * 4 + 2;
            tri[roots.Count * 6 + i * 6 + 5] = roots.Count * 2 + i * 4 + 3;
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

