using System;
using UnityEngine;
using System.Collections.Generic;

public class PheromoneBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float decay = 1f, evaporationtime = 1f, distance = 1f;
    private float lifetime = 0f, pheromone_check_time = 3f, countdown;
    SpriteRenderer sr;
    Color baseColor;
    [SerializeField] Color tohome, tofood;
    [SerializeField] float radius;
    public GameObject food;
    List<Collider2D> alarm_pheromone_colliders = new List<Collider2D>();
    List<GameObject> alarm_pheromones_touching = new List<GameObject>();

    public ContactFilter2D alarm;
    [SerializeField] bool marked_avoid;
    void Awake()
    {
        countdown = UnityEngine.Random.Range(0f,pheromone_check_time);
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        if(countdown<pheromone_check_time)
        {
            checkpheromone();
        }
        marked_avoid = false;
        for(int i = alarm_pheromones_touching.Count-1; i >=0; i--)
        {
            if(alarm_pheromones_touching[i] == null)
            {
                alarm_pheromones_touching.RemoveAt(i);
                continue;
            }
            marked_avoid = true;
        }
        lifetime += Time.deltaTime;
        decay = 1 - Math.Min(1, lifetime/evaporationtime);
        Color c = baseColor;
        c.a = decay;
        sr.color = c;
        if(decay==0)
        {
            Destroy(gameObject);
        }
    }
    
    public void initialize(String type, GameObject ant_object, float time)
    {
        distance = time;
        //Debug.Log($"normalized distance {normalized_distance}");
        if(type == "tofood")
        {
            baseColor = tofood;
            gameObject.tag = "tofood_pheromone";
            sr.color = baseColor;
            gameObject.layer = LayerMask.NameToLayer("tofood_pheromone");
            AntBehavior ant = ant_object.GetComponent<AntBehavior>();
        }
        else if(type == "tohome")
        {
            baseColor = tohome;
            gameObject.tag = "tohome_pheromone";
            sr.color = baseColor;
            gameObject.layer = LayerMask.NameToLayer("tohome_pheromone");
        }
    }
    void checkpheromone()
    {
        Vector2 center;
        alarm_pheromone_colliders.Clear();
        center = transform.position;
        Physics2D.OverlapCircle(center, radius, alarm, alarm_pheromone_colliders); 
        for (int j = 0; j < alarm_pheromone_colliders.Count; j++)
        {
            GameObject log = alarm_pheromone_colliders[j].gameObject;
            if(log == null) continue;
            //Debug.Log($"[checkfood] zone {i} detected: {log.name}", log); 
            alarm_pheromones_touching.Add(log);
        }
    }
    public bool check_avoid()
    {
        return marked_avoid;
    }
}
