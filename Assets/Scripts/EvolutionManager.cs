using UnityEngine;
using System.Collections.Generic;
using System.IO;
public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance;

    public int generation = 1, popsize = 5;
    public Vector2 speedrange = new Vector2(1.0f, 3.0f), senserange = new Vector2(2.0f, 6.0f), massrange = new Vector2(0.5f, 2.0f);
    public float mutstrength = 0.25f, basefitness = 0.5f;

    private int surviv_count = 0;
    public List<SpiderGenome> curgenome = new List<SpiderGenome>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if(curgenome.Count == 0)
        {
            generate_first_pop();
        }
    }
    SpiderGenome create_random_genes()
    {
        SpiderGenome genome = new SpiderGenome(
            Random.Range(speedrange.x, speedrange.y),
            Random.Range(senserange.x, senserange.y),
            Random.Range(massrange.x, massrange.y), 0f
        );
        return genome;
    }
    void generate_first_pop()
    {
        for(int i = 0; i < popsize; i++)
        {
            SpiderGenome genome = create_random_genes();
            curgenome.Add(genome);
        }
    }
    public List<SpiderGenome> get_curgen()
    {
        return curgenome;
    }

    public void create_next_gen(List<SpiderBehavior>spiders)
    {
        surviv_count = 0;
        generation++;
        spiders.Sort((a, b) => b.getfitness().CompareTo(a.getfitness()));
        List<SpiderGenome> newgenomes = new List<SpiderGenome>();

        //keep the best one
        SpiderGenome elite = spiders[0].genome.clone();
        elite.age++;
        newgenomes.Add(elite);
        for(int i = 0; i < spiders.Count; i++)
        {
            if(newgenomes.Count >= popsize-1) break; //always have at least one new randomly generated genome
            if(spiders[i].getfitness() > basefitness)
            {
                SpiderGenome child = mutategenome(spiders[i].genome);
                child.age++;
                newgenomes.Add(child);
            }
        }
        surviv_count = newgenomes.Count;
        Debug.Log($"gen {generation} gen created, past survivor count = {surviv_count}");
        while(newgenomes.Count<popsize)
        {
            SpiderGenome genome = create_random_genes();
            newgenomes.Add(genome);
        }
        curgenome = newgenomes;
    }
    SpiderGenome mutategenome(SpiderGenome genome)
    {
        SpiderGenome temp = genome.clone();
        temp.speed+= Random.Range(-mutstrength, mutstrength);
        temp.mass+= Random.Range(-mutstrength, mutstrength);
        temp.sense+= Random.Range(-mutstrength, mutstrength);
        temp.speed = Mathf.Clamp(temp.speed, speedrange.x, speedrange.y);
        temp.sense = Mathf.Clamp(temp.sense, senserange.x, senserange.y);
        temp.mass = Mathf.Clamp(temp.mass, massrange.x, massrange.y);

        return temp;
    }
    public void SaveToJson()
    {
        string path = Path.Combine(Application.persistentDataPath, "evolution.json");
        EvolutionSaveData data;
        if (File.Exists(path))
        {
            string existingJson = File.ReadAllText(path);
            data = JsonUtility.FromJson<EvolutionSaveData>(existingJson);
            if (data == null || data.history == null)
            {
                data = new EvolutionSaveData();
            }
        }
        else
        {
            data = new EvolutionSaveData();
        }
        GenerationRecord newRecord = new GenerationRecord
        {
            generation = this.generation,
            survivors = this.surviv_count,
            genomes = new List<SpiderGenome>()
        };
        // clone genomes
        foreach (var g in curgenome)
        {
            newRecord.genomes.Add(g.clone());
        }
        data.history.Add(newRecord);
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("Appended generation " + generation + " to: " + path);

    }
}
