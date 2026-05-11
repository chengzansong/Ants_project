using System;
using UnityEngine;
using System.Collections.Generic;

public class PheromoneBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float decay = 1f, evaporationtime = 1f, distance = 1f;
    private float lifetime = 0f;
    SpriteRenderer sr;
    Color baseColor;
    [SerializeField] Color tohome, tofood;
    [SerializeField] float radius;
    public GameObject food;

    public ContactFilter2D alarm;
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
}