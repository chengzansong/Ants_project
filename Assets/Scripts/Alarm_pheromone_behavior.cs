using System;
using UnityEngine;
using System.Collections.Generic;

public class Alarm_pheromone_behavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float decay = 1f, evaporationtime = 1f;
    private float lifetime = 0f, size;
    SpriteRenderer sr;
    Color baseColor;
    [SerializeField] Color alarm_color;
    [SerializeField] float initial_size = 1f, final_size = 1f;
    List<Collider2D> ants_colliders = new List<Collider2D>(); 
    List<GameObject> ants_touching_pheromones = new List<GameObject>();
    public ContactFilter2D ants;
    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>(); 
    }

    // Update is called once per frame
    void Update()
    {
        lifetime += Time.deltaTime;
        decay = 1 - Math.Min(1, lifetime/evaporationtime);
        Color c = baseColor;
        c.a = decay;
        sr.color = c;
        
        size = Mathf.Lerp(initial_size, final_size, 1-decay);
        transform.localScale = size*new Vector2(1,1);

        if(decay==0)
        {
            Destroy(gameObject);
        }
    }
    public void initialize(String type)
    {
        //Debug.Log($"normalized distance {normalized_distance}");
        if(type == "avoid")
        {
            baseColor = alarm_color;
            gameObject.tag = "avoid_pheromone";
            sr.color = baseColor;
            gameObject.layer = LayerMask.NameToLayer("avoid_pheromone");
        }
    }
    void checkants()
    {
        Vector2 center;
        ants_colliders.Clear();
        ants_touching_pheromones.Clear();
        center = transform.position;
        Physics2D.OverlapCircle(center, size, ants, ants_colliders); 
        for (int j = 0; j < ants_colliders.Count; j++)
        {
            GameObject log = ants_colliders[j].gameObject;
            if(log == null) continue;
            //Debug.Log($"[checkfood] zone {i} detected: {log.name}", log); 
            ants_touching_pheromones.Add(log);
        }
    }
}
