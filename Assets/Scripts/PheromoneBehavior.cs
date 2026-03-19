using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PheromoneBehavior : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public Vector2 direction;
    public float decay = 1f, evaporationtime = 1f;
    private float lifetime = 0f;
    SpriteRenderer sr;
    Color baseColor;
    [SerializeField] Color tohome, tofood;
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
    public void initialize(String type)
    {
        if(type == "tofood")
        {
            baseColor = tofood;
            gameObject.tag = "tofood_pheromone";
            sr.color = baseColor;
            gameObject.layer = LayerMask.NameToLayer("tofood_pheromone");
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
