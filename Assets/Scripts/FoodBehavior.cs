using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FoodBehavior : MonoBehaviour
{
    public bool istaken = false;
    public Guid selectedcarrier;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Ant")
        {
            GameObject gameObject = collision.gameObject;
            AntBehavior ant = gameObject.GetComponent<AntBehavior>();
            if(ant.Id == selectedcarrier)
            {
                ant.carryingfood = true;
                ant.pickUpFood(this.gameObject);
                GetComponent<CircleCollider2D>().enabled = false;
                ant.revert_direction();
            }
            /*if(!ant.carryingfood && ant != null && istaken)
            {
                ant.carryingfood = true;
                ant.pickUpFood(this.gameObject);
                GetComponent<CircleCollider2D>().enabled = false;
            }*/
        }
    }
}
