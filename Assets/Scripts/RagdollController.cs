using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollController : MonoBehaviour {

    Rigidbody[] jointRigidbodies;
    NaturalSelection naturalSelection;
    List<ForceGene> gene;
    public Transform root;

    private float startZ;
    private const int minChecks = 5;

    private string botName;
    private float startTime;

    public void Initialize(NaturalSelection natSel, string name)
    {
        jointRigidbodies = GetComponentsInChildren<Rigidbody>();
        naturalSelection = natSel;
        startTime = Time.time;
        botName = name;
    }

    public void StartMoving(List<ForceGene> inheritedGene)
    {
        gene = new List<ForceGene>();
        startZ = root.position.z;
        StartCoroutine(ApplyForces(inheritedGene));
    }

    public IEnumerator ApplyForces(List<ForceGene> inheritedGene)
    {
        int geneCount = 0;

        StartCoroutine(ProgressCheck());

        while (true)
        {
            if (geneCount == inheritedGene.Count)
                geneCount = 0;

            Vector3[] forces = inheritedGene[geneCount].forces;
            
            gene.Add(new ForceGene(forces));

            for (int i = 0; i < forces.Length; i++)
                jointRigidbodies[i].AddRelativeForce(forces[i]);

            geneCount++;
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator ProgressCheck()
    {
        float prevZ = startZ;
        int curFrame = 0;

        while (curFrame < minChecks || root.position.z - prevZ > 0.1f)
        {
            yield return new WaitForSeconds(1f);
            
            prevZ = root.position.z;
            curFrame++;
        }

        Finished();
    }

    private bool hasHitGround = false;
    public void GroundHit()
    {
        if (!hasHitGround)
        {
            hasHitGround = true;
            Finished();
        }
    }

    private void Finished()
    {
        float fitness = (root.position.z - startZ) * (Time.time - startTime);
        naturalSelection.RegisterGene(new GeneScore(botName, fitness, gene));
        Destroy(gameObject);
    }

    public int GetNumOfJoints()
    {
        return jointRigidbodies.Length;
    }
}
