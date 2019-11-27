using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree : MonoBehaviour
{
    public Material material;
    Line line;
    public Color colour = Color.white;
    [Range(0.01f, 0.9f)] public float branchRangeMultier = 0.5f;
    public float hueChange = -0.05f, minRot = 10, maxRot = 30;

    int rootCount = 1;
    List<Line> roots = new List<Line>();

    public int Branchings = 1;
    //public Vector3[] positions = new Vector3[1];

    private void Start()
    {
        line = new Line(new Vector3(0, -5, 0), new Vector3(0, -3, 0), 90, branchRangeMultier, colour, material, hueChange, minRot, maxRot);

        roots.Add(line);
        line.Branch();
        roots.Add(roots[0].left);
        roots.Add(roots[0].right);
        CreateTree();
    }

    private void CreateTree()
    {
        for (int index = rootCount; index < Branchings; index++)
        {
            line.Branch();

            int max = roots.Count;
            for (int i = rootCount; i < max; i++)
            {
                if (roots[i].left != null && roots[i].right != null)
                {
                    roots.Add(roots[i].left);
                    roots.Add(roots[i].right);
                    rootCount += 1;
                }
            }
        }
    }
    void TraverseTree()
    {
        foreach (Line l in roots)
        {
            l.DrawLine();
            l.DrawNext();
        }

    }
    void RenderTree(Line lin)
    {
        lin.DrawLine();
        lin.shiftPost();
        if (lin.left != null)
        {
            RenderTree(lin.left);
            RenderTree(lin.right);
        }
    }

    public void OnPostRender()
    {
        //TraverseTree();
        RenderTree(line);

    }
}


//public class Tree : MonoBehaviour
//{
//    public Material material;
//    Line line;
//    public Color colour = Color.white;
//    [Range(0.01f, 0.9f)] public float branchRangeMultier = 0.5f;
//    int rootCount = 1;
//    List<Line> roots = new List<Line>();

//    private void Start()
//    {
//        line = new Line(new Vector3(0, -5, 0), new Vector3(0, -3, 0), 90, branchRangeMultier, colour, material);

//        roots.Add(line);
//        line.Branch();
//        roots.Add(roots[0].left);
//        roots.Add(roots[0].right);
//    }

//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Mouse0))
//        {
//            line.Branch();

//            int max = roots.Count;
//            for (int i = rootCount; i < max; i++)
//            {
//                if (roots[i].left != null && roots[i].right != null)
//                {
//                    roots.Add(roots[i].left);
//                    roots.Add(roots[i].right);
//                    rootCount += 1;
//                }
//            }
//        }
//    }
//    void TraverseTree()
//    {
//        foreach (Line l in roots)
//        {
//            l.DrawLine();
//            l.DrawNext();
//        }

//    }

//    public void OnPostRender()
//    {
//        TraverseTree();
//    }
//}