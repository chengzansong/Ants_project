using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class AntBehavior : MonoBehaviour
{
    public float maxspeed = 1f, maxforce = 1f, wanderstrength = 1f,rotationspeed = 1f, time_between_pheromone = 1f, countdown = 1f;
    public GameObject foodtarget = null, anthill, pheromone;
    private GameObject carriedfood;
    public Transform head;
    Vector2 position, velocity, desired_direction, desired_velocity, desired_force, acceleration;
    [SerializeField] bool selectedfood = false;
    public bool carryingfood = false;
    List<GameObject> pheromone_in_range = new List<GameObject>();
    AntBehavior ant;

    private readonly Guid id = Guid.NewGuid();
    public Guid Id => id;
    void Start()
    {
        position = transform.position;
        velocity = new Vector2(0,0);
    }
    void Update()
    {
        directionDecision();
        movementPhysics();
    }
    void movementPhysics()
    {
        desired_velocity = desired_direction * maxspeed;
        desired_force = (desired_velocity - velocity);
        acceleration = Vector2.ClampMagnitude(desired_force, maxforce);
        velocity = Vector2.ClampMagnitude(velocity + acceleration * Time.deltaTime, maxspeed);
        position += velocity * Time.deltaTime;
        if(velocity != Vector2.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, velocity);
            transform.SetPositionAndRotation(position, toRotation);
        }
    }
    void directionDecision()
    {
        //Debug.Log("Made decision about direction");
        if(foodtarget == null) selectedfood = false;//safeguard in case another ant destroys food;
        if(selectedfood) //heading directly to food
        {
            desired_direction = ((Vector2)foodtarget.transform.position - (Vector2)transform.position).normalized;
        }
        else if(!selectedfood && carryingfood)//foundfood, heading back to base
        {
            Debug.Log("Placing Pheromone/heading home");
            desired_direction = ((Vector2)anthill.transform.position - (Vector2)transform.position).normalized;
            lay_pheromones();
        }
        else //randomly wander, with pheromones
        {
            Vector2 newdirection = UnityEngine.Random.insideUnitCircle*wanderstrength;
            desired_direction = (desired_direction + calculatepheromones() + newdirection * wanderstrength).normalized;
        }
    }
    Vector2 calculatepheromones()
    {
        Vector2 direction = new Vector2(0,0);
        float distance;
        PheromoneBehavior temppheromone; 
        for(int i = pheromone_in_range.Count - 1; i >= 0; i--)
        {
            if(pheromone_in_range[i] == null)
            {
                pheromone_in_range.RemoveAt(i);
            }
            else
            {
                temppheromone = pheromone_in_range[i].GetComponent<PheromoneBehavior>();
                distance = ((Vector2)temppheromone.transform.position - (Vector2)transform.position).sqrMagnitude;
                //note: later fix, currently sqr magnitude is the magnitude squared, which is really fast, but may throw it off balance.
                direction += temppheromone.direction/distance*temppheromone.decay;
            }
        }
        return direction;
    }
    void lay_pheromones()
    {
        countdown -= Time.deltaTime;
        if(countdown <= 0)
        {
            countdown = time_between_pheromone;
            GameObject gameObject = Instantiate(pheromone, transform.position, Quaternion.identity);
            PheromoneBehavior newPheromone = gameObject.GetComponent<PheromoneBehavior>();
            newPheromone.direction = desired_direction*(-1);
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Food"))
        {
            FoodBehavior food = collision.GetComponent<FoodBehavior>();
            if(food != null && !food.istaken && !selectedfood && !carryingfood)
            {
                selectedfood = true;
                food.istaken = true;
                foodtarget = collision.gameObject;
                food.selectedcarrier = Id;
                Debug.Log("selected food: " + foodtarget.name);
            }
        }
        if(collision.CompareTag("Pheromone"))
        {
            pheromone_in_range.Add(collision.gameObject);
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Pheromone"))
        {
            pheromone_in_range.Remove(collision.gameObject);    
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Anthole" && carryingfood)
        {
            velocity = new Vector2(0,0);
            desired_direction = new Vector2(0,0);
            carryingfood = false;
            Destroy(carriedfood);   
            carriedfood = null;
        }
        if(collision.gameObject.tag == "Wall")
        {
            velocity *= -1;
            desired_direction *= -1;
        }
    }
    public void pickUpFood(GameObject food)
    {
        Debug.Log("Picked up food: " + food.name);
        selectedfood = false;
        carriedfood = food;
        food.transform.SetParent(head, false);
        food.transform.localPosition = Vector2.zero;
        SpriteRenderer foodRenderer = food.GetComponent<SpriteRenderer>();
        SpriteRenderer antRenderer = GetComponent<SpriteRenderer>();
        foodRenderer.sortingOrder = antRenderer.sortingOrder + 1;
    }
}
