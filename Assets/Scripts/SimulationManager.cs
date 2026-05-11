using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using System;

public class SimulationManager : MonoBehaviour
{
    public GameObject spiderprefab, antprefab, antholeprefab, foodprefab;
    public bool running = false;
    [SerializeField] float spider_spawn_time = 60f, simtime = 200f, timer, simtimer;
    private List<SpiderBehavior> spawnedspiders = new List<SpiderBehavior>(), survivors = new List<SpiderBehavior>();
    GameObject anthole;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] Vector2 spawn1, spawn2, spawn3, spawn4, spawn5; 
    void Start()
    {
        timer = spider_spawn_time;
        simtimer = simtime;
        startsim();
    }

    // Update is called once per frame
    void Update()
    {
        simtimer -= Time.deltaTime;
        timer -= Time.deltaTime;
        if(timer < 0)
        {
            timer = Mathf.Infinity;
            spawnspiders();
        }
        if(simtimer < 0)
        {
            simtimer = Mathf.Infinity;
            endsimulation();
        }
        for(int i = spawnedspiders.Count-1; i>=0; i--)
        {
            if(spawnedspiders[i]==null)//dead
            {
                spawnedspiders.RemoveAt(i);
                continue;
            }
            else if(spawnedspiders[i].full)
            {
                survivors.Add(spawnedspiders[i]);
                spawnedspiders.RemoveAt(i);
            }
        }
        if(spawnedspiders.Count==1 && survivors.Count == 0 && running)//ensures always one survivor.
        {
            survivors.Add(spawnedspiders[0]);
            spawnedspiders.RemoveAt(0);
            simtimer = 0;// end the simulation early;
            Debug.Log("Ended early, no survivors, preserved last survivor");
        }
        if(spawnedspiders.Count == 0 && survivors.Count > 0 && running)// also end simulation early;
        {
            simtimer = 0;
            Debug.Log("Ended early, no survivors, restart fresh");
        }
    }
    void endsimulation()
    {
        running = false;
        Debug.Log($"Ended normally, {survivors.Count} survivors, all genes preserved");
        EvolutionManager.Instance.create_next_gen(survivors);//survivors are weeded out.
        clearworld();//second clear

        timer = spider_spawn_time;
        simtimer = simtime;
        destroy_spiders();
        survivors.Clear();
        startsim();
    }
    void spawnspiders()
    {
        List<SpiderGenome> genomes = EvolutionManager.Instance.get_curgen();
        spawnedspiders.Clear();
        instantiate_at_loc(genomes[0], spawn1);
        instantiate_at_loc(genomes[1], spawn2);
        instantiate_at_loc(genomes[2], spawn3);
        instantiate_at_loc(genomes[3], spawn4);
        instantiate_at_loc(genomes[4], spawn5);
    }
    void instantiate_at_loc(SpiderGenome genes, Vector2 location)
    {
        GameObject spider = Instantiate(spiderprefab, location, quaternion.identity);
        SpiderBehavior spiderobj = spider.GetComponent<SpiderBehavior>();
        spiderobj.setlocation(location);
        if (spider == null)
        {
            Debug.LogError("spider prefab missing, or some other instantiation error");
            return;
        }
        spiderobj.apply_genome(genes);
        spawnedspiders.Add(spiderobj);
    }
    public void startsim()
    {
        EvolutionManager.Instance.SaveToJson();
        Debug.Log("Saved to JSON");
        clearworld();
        spawnanthole();
        spawnfood();
        EvolutionManager.Instance.death_by_ant = 0;
        Debug.Log("Simulation started, anthole spawned");
        running = true;
    }
    void spawnfood()
    {
        Instantiate(foodprefab, this.transform.position, quaternion.identity);
    }
    public void spawnanthole()
    {
        Vector2 spawnpos = new Vector2(0,0);
        anthole = Instantiate(antholeprefab, spawnpos, quaternion.identity);
    }
    void destroy_spiders()
    {
        for(int i = survivors.Count-1; i>=0; i--)
        {
            if(survivors[i] == null)
            {
                survivors.RemoveAt(i);
                continue;
            }
            Destroy(survivors[i].gameObject);
            survivors.RemoveAt(i);
        }
        for(int i = spawnedspiders.Count-1; i>=0; i--)
        {
            if(spawnedspiders[i] == null)
            {
                spawnedspiders.RemoveAt(i);
                continue;
            }
            Destroy(spawnedspiders[i].gameObject);
            spawnedspiders.RemoveAt(i);
        }
    }
    string[] wipe = new string[] {"Pheromone", "tofood_pheromone", "tohome_pheromone", "attack_pheromone", "avoid_pheromone", "Food"};
    void destroy_misc_objects()
    {
        foreach(string s in wipe)
        {
            GameObject[] gos = GameObject.FindGameObjectsWithTag(s);
            foreach(GameObject go in gos)
                Destroy(go);
        }
    }
    public void clearworld()
    {
        destroy_misc_objects();
        destroy_spiders();
        if(anthole != null)
        {
            AntholeBehavior anthole_obj = anthole.gameObject.GetComponent<AntholeBehavior>();
            anthole_obj.destroyants();
            Destroy(anthole.gameObject);
        }
        anthole = null;
    }
}
