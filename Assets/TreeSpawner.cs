﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

class Root
{
    public Vector3 pos;
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


    public Root(Vector3 end, float rot, float branchMul, int i, float lenght, Root previous = null)
    {
        pos = end;
        rotation = rot;
        branchLenghtMul = branchMul;
        branchLevel = i;
        this.length = lenght;
        width = length * 0.1f;
        this.previous = previous;
    }
    public float GetEdgeRotation()
    {
        if (previous == null) return rotation;
        float rot = Vector2.SignedAngle(previous.pos, pos);

        return rot;
    }


    public void ActivateTreeRoot()
    {
        float leftRot = rotation + UnityEngine.Random.Range(TreeSpawner.minRot, TreeSpawner.maxRot);
        float rightRot = rotation - UnityEngine.Random.Range(TreeSpawner.minRot, TreeSpawner.maxRot);
        Vector3 leftV = pos + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * leftRot) * length,
            Mathf.Sin(Mathf.Deg2Rad * leftRot) * length);
        Vector3 rightV = pos + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * rightRot) * length,
            Mathf.Sin(Mathf.Deg2Rad * rightRot) * length);
        root.Add(new Root(leftV, leftRot, branchLenghtMul, branchLevel + 1, length, this));
        root.Add(new Root(rightV, rightRot, branchLenghtMul, branchLevel + 1, length, this));
    }

    public void ActivateLineRoot()
    {
        Vector3 point = pos + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * rotation) * length,
            Mathf.Sin(Mathf.Deg2Rad * rotation) * length);
        root.Add(new Root(point, rotation, branchLenghtMul, branchLevel + 1, length, this));
    }
    public void ActivateLineRoot(float customLenght)
    {
        Vector3 point = pos + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * rotation) * customLenght,
            Mathf.Sin(Mathf.Deg2Rad * rotation) * customLenght);
        root.Add(new Root(point, rotation, branchLenghtMul, branchLevel + 1, length, this));
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
public class TreeSpawner : MonoBehaviour
{
    public enum Models { fractalTree = 0, LsystemTree, plant }
    public Models modelChoice = Models.LsystemTree;
    int ModelChoice
    {
        get { return (int)modelChoice; }
    }
    public float StartHeight = 1.0f;
    //[Range(0.01f, 1.0f)] public float branchRangeMultier = 0.8f;
    public float branchWidth = 0.5f;
    public float rotationOffset = 0;
    public int Branchings = 1;
    public int cleanupFactor = 60;

    public static float minRot = 9.0f;
    public static float maxRot = 30.0f;

    int rootCount = 0;
    List<Root> roots = new List<Root>();
    RuleContainer[] rules = new RuleContainer[1];
    Mesh mesh;
    Root root;

    Stack<Root> rootStates = new Stack<Root>();
    Stack<float> rotationStates = new Stack<float>();
    Root currentRoot;

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

        string s = rules[ModelChoice].startString;
        for (int i = 0; i < Branchings; i++)
        {
            s = calulateL(rules[ModelChoice].rules, s);
        }
        root = new Root(new Vector3(0, 0), 90, 1, 0, UnityEngine.Random.Range(StartHeight * 0.9f, StartHeight * 1.1f));
        currentRoot = root;
        rules[ModelChoice].translation(s);
        Debug.Log(s);
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
            if (r.root[i].lengthFromEnd < r.lengthFromEnd - (int)UnityEngine.Random.Range(cleanupFactor - 10, cleanupFactor + 10))
            {
                r.root.RemoveAt(i);
            }
        }
    }

    string calulateL(List<Rule> rules, string s)
    {
        StartHeight *= 0.5f;
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
        TreeTravel = (r) =>
        {
            roots.Add(r);
            r.ID = rootCount;
            rootCount++;
            maxBranches = maxBranches < r.root.Count ? r.root.Count : maxBranches;
            foreach (Root ro in r.root)
            {
                TreeTravel(ro);
            }
            if (r.previous != null)
            {
                if (r.previous.previous != null)
                {
                    r.branchStrenght = (float)(r.lengthFromEnd + r.previous.lengthFromEnd + r.previous.previous.lengthFromEnd) / 3;
                }
                else
                {
                    r.branchStrenght = (float)(r.lengthFromEnd + r.root[0].lengthFromEnd + r.root[0].root[0].lengthFromEnd) / 3;
                }
            }
            else
            {
                r.branchStrenght = (float)(r.lengthFromEnd + r.root[0].lengthFromEnd + r.root[0].root[0].lengthFromEnd) / 3;
            }
        };
        TreeTravel(root);


        MeshFilter mf = GetComponent<MeshFilter>();
        mesh = new Mesh();
        mf.mesh = mesh;
        Vector3[] vertices = new Vector3[roots.Count * 2];  //   2^(branchings+2)
        Vector2[] uvs = new Vector2[roots.Count * 2];
        Vector2[] uvs2 = new Vector2[roots.Count * 2];
        maxBranches *= 6;
        int[] tri = Enumerable.Repeat(-1, maxBranches * roots.Count).ToArray();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        Parallel.For(0, roots.Count, (int i) =>
        {
            Vector3 pos = roots[i].pos;
            float rotation = roots[i].rotation + rotationOffset;

            float uvLever = (float)roots[i].lengthFromEnd / (float)maxDepth;
            uvs[roots[i].ID * 2] = new Vector2(0, 1.0f - uvLever);
            uvs[roots[i].ID * 2 + 1] = new Vector2(0, 1.0f - uvLever);

            vertices[roots[i].ID * 2] = pos + new
            Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation - 90)),
            Mathf.Sin(Mathf.Deg2Rad * (rotation - 90)), uvLever * 0.1f) * (uvLever + 0.02f) * branchWidth;

            vertices[roots[i].ID * 2 + 1] = pos + new
            Vector3(Mathf.Cos(Mathf.Deg2Rad * (rotation + 90)),
            Mathf.Sin(Mathf.Deg2Rad * (rotation + 90)), uvLever * 0.1f) * (uvLever + 0.02f) * branchWidth;

            float branchStrenght = 1 - (float)roots[i].branchStrenght / (float)maxDepth;
            uvs2[roots[i].ID * 2] = new Vector2(0, branchStrenght);
            uvs2[roots[i].ID * 2 + 1] = new Vector2(0, branchStrenght);

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
        Debug.Log(tri.Count());
        tri = tri.Where(x => x > -1).ToArray();
        Debug.Log(tri.Count());
        mesh.vertices = vertices;
        mesh.triangles = tri;
        mesh.uv = uvs;
        mesh.uv2 = uvs2;
    }

}

