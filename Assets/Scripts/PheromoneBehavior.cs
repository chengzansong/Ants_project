using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PheromoneBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float decay = 1f, evaporationtime = 1f;
    private float lifetime = 0f, normalized_distance;
    SpriteRenderer sr;
    Color baseColor;
    [SerializeField] Color tohome, tofood;
    public Transform top_wall, left_wall, anthole;
    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();  
        top_wall = GameObject.Find("Wall_Top").transform;
        left_wall = GameObject.Find("Wall_Left").transform;
        anthole = GameObject.Find("Anthole").transform;
    }

    // Update is called once per frame
    void Update()
    {
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
    public void initialize(String type)
    {
        Vector2 corner = new Vector2(top_wall.position.y, left_wall.position.x);
        float max_distance = Vector2.Distance(anthole.position, corner);
        float distance_from_anthole = Vector2.Distance(transform.position, anthole.position);
        normalized_distance = Mathf.Clamp01(distance_from_anthole / max_distance);
        //Debug.Log($"normalized distance {normalized_distance}");
        if(type == "tofood")
        {
            baseColor = tofood;
            gameObject.tag = "tofood_pheromone";
            sr.color = baseColor;
            gameObject.layer = LayerMask.NameToLayer("tofood_pheromone");
            evaporationtime *= Mathf.Lerp(0.1f, 1.8f, normalized_distance);
            
        }
        else if(type == "tohome")
        {
            baseColor = tohome;
            gameObject.tag = "tohome_pheromone";
            sr.color = baseColor;
            gameObject.layer = LayerMask.NameToLayer("tohome_pheromone");
            evaporationtime *= Mathf.Lerp(1.8f, 0.1f, normalized_distance);
        }
    }
}
