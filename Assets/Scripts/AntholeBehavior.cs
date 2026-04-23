using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class AntholeBehavior : MonoBehaviour
{
    public GameObject spawn;
    [SerializeField] Transform parent;
    public float time_to_spawn = 1f, spawn_countdown = 1f;
    public int maxspawn = 50; 
    private int curfood = 0;
    [SerializeField] int ant_req = 10;
    List<GameObject> spawnedants = new List<GameObject>();
    int count = 0;
    void Start()
    {
    }
    void Update()
    {
        spawn_countdown -= Time.deltaTime;
        GameObject ant;
        if(spawn_countdown <= 0 && count<maxspawn)
        {
            spawn_countdown = time_to_spawn;
            int random_rotation = Random.Range(0,360);
            ant = Instantiate(spawn, transform.position, Quaternion.Euler(0,0,random_rotation), parent);
            spawnedants.Add(ant);
            count++;
        }
        if(curfood >= ant_req)
        {
            curfood = 0;
            int random_rotation = Random.Range(0,360);
            ant = Instantiate(spawn, transform.position, Quaternion.Euler(0,0, random_rotation), parent);
            spawnedants.Add(ant);
        }
    }
    public void destroyants()
    {
        for(int i = spawnedants.Count-1; i >= 0; i--)
        {
            Destroy(spawnedants[i]);
        }
        spawnedants.Clear();
    }
    public void accumulate_food()
    {
        curfood++;
    }
}
