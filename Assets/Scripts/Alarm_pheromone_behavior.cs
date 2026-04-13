using System;
using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class Alarm_pheromone_behavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float decay = 1f, evaporationtime = 1f;
    private float lifetime = 0f, size, countdown;
    SpriteRenderer sr;
    Color baseColor;
    [SerializeField] Color alarm_color;
    [SerializeField] float initial_size = 1f, final_size = 1f, ant_check_time = 0.5f, secondary_radius = 5f;
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
        transform.localScale = new Vector2(size * 2, size * 2);

        if(decay==0)
        {
            Destroy(gameObject);
        }

        countdown -= Time.deltaTime;
        if(countdown<ant_check_time)
        {
            checkants();
            alertants();
        }
    }
    /*
    void OnDrawGizmos()
    {
        //size
        Vector2 center = transform.position;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, size);
    }*/
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
    public void changeradius()
    {
        final_size = secondary_radius;
    }
    void alertants()
    {
        for(int i = ants_touching_pheromones.Count-1; i>=0; i--)
        {
            if(ants_touching_pheromones[i] == null)
            {
                ants_touching_pheromones.RemoveAt(i);
                continue;
            }
            AntBehavior ant = ants_touching_pheromones[i].GetComponent<AntBehavior>();
            Vector2 tempvec = ant.transform.position - this.transform.position;
            ant.modify_panicvector(tempvec);
            if(ant.panicmode)
            {
                continue;
            }
            ant.initiate_panic_mode();
        }
    }
    void checkants()
    {
        Vector2 center;
        ants_colliders.Clear();
        ants_touching_pheromones.Clear();
        center = transform.position;
        Physics2D.OverlapCircle(center, size, ants, ants_colliders); 
        for (int i = 0; i < ants_colliders.Count; i++)
        {
            GameObject log = ants_colliders[i].gameObject;
            if(log == null) continue;
            ants_touching_pheromones.Add(log);
        }
    }
}
