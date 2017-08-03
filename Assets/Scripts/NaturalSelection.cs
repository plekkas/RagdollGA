using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class NaturalSelection : MonoBehaviour {

    public GameObject ragdoll;

    public Text generationText;
    public Text leaderboard;

    const int generationTotal = 100;

    public Vector3 startPos;

    int curGeneration = 0;

    int finishedBotCount;

    const int mostFittingTotal = 4;

    List<GeneScore> geneScores;

    private void Start()
    {
        geneScores = new List<GeneScore>();
        StartNewGeneration();
    }

    private void StartNewGeneration()
    {
        curGeneration++;

        finishedBotCount = 0;

        generationText.text = "Generation " + curGeneration.ToString();

        GeneScore baseGene = null;
        List<GeneScore> crossoverCandidates = new List<GeneScore>();

        if (geneScores.Count > 0)
        {
            List<GeneScore> orderedGenes = geneScores.OrderByDescending(x => x.score).ToList();
            UpdateLeaderBoard(orderedGenes);

            baseGene = new GeneScore(orderedGenes.First().CreateForceGeneCopy());
            List<GeneScore> mostFitting = GetMostFitting(orderedGenes.Skip(1).ToList());
            foreach (GeneScore gs in mostFitting)
                crossoverCandidates.Add(new GeneScore(gs.CreateForceGeneCopy()));

            geneScores = new List<GeneScore>(orderedGenes);
            CleanGeneList();
        }

        for (int i=0; i< generationTotal; i++)
            CreateNewBot(i, new Vector3(startPos.x + (i*4), startPos.y, startPos.z), baseGene, crossoverCandidates);
    }

    private void CreateNewBot(int curBotId, Vector3 botPos, GeneScore baseGene, List<GeneScore> crossoverCandidates)
    {
        GameObject curBot = Instantiate(ragdoll, botPos, Quaternion.identity) as GameObject;
        RagdollController ragdollController = curBot.GetComponent<RagdollController>();
        ragdollController.Initialize(this, "G" + curGeneration.ToString() + "B" + curBotId.ToString());

        if (baseGene == null)
            ragdollController.StartMoving(GetNewGene(ragdollController.GetNumOfJoints()));
        else
        {
            if (curBotId == 0)
                ragdollController.StartMoving(baseGene.CreateForceGeneCopy());
            else
                ragdollController.StartMoving(GetCrossoverAndMutatedGene(baseGene, crossoverCandidates));
        }
            
    }

    private List<ForceGene> GetNewGene(int jointLength)
    {
        int geneLength = Random.Range(500, 3001);
        List<ForceGene> newGene = new List<ForceGene>();
        for (int i = 0; i < geneLength; i++)
            newGene.Add(new ForceGene(GeneticHelper.GetRandomForces(jointLength)));

        return newGene;
    }

    private List<ForceGene> GetCrossoverAndMutatedGene(GeneScore baseGene, List<GeneScore> crossoverCandidates)
    {
        GeneScore newGene = new GeneScore(baseGene.CreateForceGeneCopy());
        GeneScore crossoverGene = new GeneScore(newGene.CrossOver(crossoverCandidates));
        List<ForceGene> mutatedGene = new GeneScore(crossoverGene.Mutate()).CreateForceGeneCopy();

        return mutatedGene;
    }

    public void RegisterGene(GeneScore geneScore)
    {
        finishedBotCount++;

        geneScores.Add(geneScore);
      
        if (finishedBotCount == generationTotal)
            StartNewGeneration();

    }

    private List<GeneScore> GetMostFitting(List<GeneScore> orderedGenes)
    {
        List<GeneScore> mostFitting = new List<GeneScore>();
        for (int i = 0; i < mostFittingTotal; i++)
            mostFitting.Add(new GeneScore(orderedGenes[i].CreateForceGeneCopy()));

        return mostFitting;
    }

    private void UpdateLeaderBoard(List<GeneScore> orderedGenes)
    {
        leaderboard.text = "";

        int count = 0;
        while(count < 10 && orderedGenes.Count > count)
        {
            leaderboard.text += orderedGenes[count].botName + " " + orderedGenes[count].score.ToString() + '\n';
            count++;
        }
    }

    private void CleanGeneList()
    {
        geneScores.RemoveRange(50, geneScores.Count - 50);
    }
}

public class GeneScore
{
    public string botName;
    public float score;
    public List<ForceGene> forceGene;

    public GeneScore(string name, float scr, List<ForceGene> gene)
    {
        botName = name;
        score = scr;
        forceGene = gene;
    }

    public GeneScore(List<ForceGene> gene)
    {
        forceGene = gene;
    }

    public List<ForceGene> CreateForceGeneCopy()
    {
        List<ForceGene> newForceGene = new List<ForceGene>();
        foreach(ForceGene fg in forceGene)
            newForceGene.Add(fg.CreateCopy());

        return newForceGene;
    }

    public List<ForceGene> Mutate()
    {
        List<ForceGene> mutatedGenes = new List<ForceGene>();
        List<ForceGene> copyGene = new List<ForceGene>(CreateForceGeneCopy());

        for (int i = 0; i < forceGene.Count; i++)
        {
            if (Random.Range(0, 10) == 0)
                mutatedGenes.Add(new ForceGene(GeneticHelper.GetRandomForces(forceGene[i].forces.Length)));
            else
                mutatedGenes.Add(new ForceGene(copyGene[i].forces));
        }

        return mutatedGenes;
    }

    public List<ForceGene> CrossOver(List<GeneScore> otherGenes)
    {
        List<ForceGene> crossOverGenes = new List<ForceGene>();
        List<ForceGene> copyGene = new List<ForceGene>(CreateForceGeneCopy());
        List<GeneScore> copyOther = new List<GeneScore>(otherGenes);

        for (int i = 0; i < forceGene.Count; i++)
        {
            copyOther = new List<GeneScore>(ClearOthers(i, copyOther));
            if (copyOther.Count == 0)
                return crossOverGenes;

            crossOverGenes.Add(new ForceGene(Random.Range(0, 2) == 0 
                ? copyGene[i].forces 
                : copyOther[Random.Range(0, copyOther.Count)].forceGene[i].forces));
        }

        return crossOverGenes;
    }

    public List<GeneScore> ClearOthers(int cIdx, List<GeneScore> otherGenes)
    {
        List<GeneScore> tempOther = new List<GeneScore>(otherGenes);

        foreach (GeneScore ogs in otherGenes)
        {
            if (ogs.forceGene.Count == cIdx)
                tempOther.Remove(ogs);
        }

        return tempOther;
    }
}
