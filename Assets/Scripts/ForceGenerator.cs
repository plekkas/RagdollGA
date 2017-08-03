using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceGene
{
    public Vector3[] forces;

    public ForceGene(Vector3[] fGene)
    {
        forces = fGene;
    }

    public ForceGene CreateCopy()
    {
        Vector3[] cForces = new Vector3[forces.Length];
        for (int i = 0; i < forces.Length; i++)
            cForces[i] = new Vector3(forces[i].x, forces[i].y, forces[i].z);
        return new ForceGene(cForces);
    }
}

public static class GeneticHelper
{
    private const float contraint = 100f;

    public static Vector3 GetRandomForce()
    {
        return new Vector3(Random.Range(-contraint, contraint), Random.Range(-contraint, contraint), Random.Range(-contraint, contraint));
    }

    public static Vector3[] GetRandomForces(int length)
    {
        Vector3[] forces = new Vector3[length];
        for (int i = 0; i < length; i++)
            forces[i] = GeneticHelper.GetRandomForce();

        return forces;
    }
}