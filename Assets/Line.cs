using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line
{
    public Material material;
    Color startColour, endColour;
    Vector3 pointA;
    Vector3 pointB;
    public Line left;
    public Line right;
    bool branched = false;
    float rotation;
    float branchLenghtMul;
    float hueChange = -0.05f, minRot = 10, maxRot = 30;

    public Line(Vector3 start, Vector3 end, float rot, float branchMul, Color col, Material mat, float hue, float miR, float maR)
    {
        pointA = start;
        pointB = end;
        material = mat;
        rotation = rot;
        branchLenghtMul = branchMul;
        startColour = col;
        hueChange = hue;
        minRot = miR;
        maxRot = maR;
    }

    public void ActivateRoot()
    {
        float H, S, V;
        Color.RGBToHSV(startColour, out H, out S, out V);
        H -= hueChange + Random.Range(-0.01f, 0.01f);
        endColour = Color.HSVToRGB(H, S, V);
        float length = Vector3.Distance(pointA, pointB) * branchLenghtMul;
        float leftRot = rotation + Random.Range(minRot, maxRot);
        float rightRot = rotation - Random.Range(minRot, maxRot);
        Vector3 leftV = pointB + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * leftRot) * length,
            Mathf.Sin(Mathf.Deg2Rad * leftRot) * length, 0);
        Vector3 rightV = pointB + new Vector3(
            Mathf.Cos(Mathf.Deg2Rad * rightRot) * length,
            Mathf.Sin(Mathf.Deg2Rad * rightRot) * length, 0);

        left = new Line(pointB, leftV, leftRot, branchLenghtMul, endColour, material, hueChange, minRot, maxRot);
        right = new Line(pointB, rightV, rightRot, branchLenghtMul, endColour, material, hueChange, minRot, maxRot);
    }

    public void Branch()
    {
        if (!branched)
        {
            ActivateRoot();
            branched = true;
        }
        else
        {
            left.Branch();
            right.Branch();
        }
    }

    public void DrawNext()
    {
        if (left != null && right != null)
        {
            left.DrawLine();
            right.DrawLine();
        }
    }

    public void shiftPost()
    {
        pointA += new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f));
    }

    public void DrawLine()
    {
        GL.Begin(GL.LINES);
        material.SetPass(0);
        GL.Color(startColour);
        GL.Vertex(pointA);
        GL.Color(endColour);
        GL.Vertex(pointB);

        GL.End();
    }
}
