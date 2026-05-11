# EmergentColony: A unity simulation representing the dynamic interaction between ants and predators
A Unity ant simulator set in an environment with multiple evolving predators across hundreds of generations. This is the Senior Project of Luke Song, BISV Class of 2026. I have written a series of blogs linked [here](https://basisindependent.com/schools/ca/silicon-valley/academics/the-senior-year/senior-projects/luke-s/) detailing my journey.

## Installation
Download Unity Hub, and install editor version 6000.3.11f1 from archive.
Download and extract the repository as a .zip file, add the folder as a project on Unity Hub, and run using version 6000.3.11f1
**NOTE:** if unity shows that it does not detect a version, it's likely that the uploaded project is in the incorrect format. Most likely there is a sub-folder containing all Assets and code within the larger extracted folder.

## Usage
After opening Unity Hub, choose a scene to load in. There are currently three scenes included in the simulation
### Scene 1: Simulation
This is the base model for an ant simulation, complete with food sources and a small obstacle. Upon start, ants will pour out of the anthole located in the center, and will randomly wander around as they lay down "to-home" pheromones. Upon finding food, ants will then follow the to-home pheromones and carry the food back to the anthole, while laying down new "to-food" pheromones to guide new ants. Food can be used to spawn in more ants. A certain percentage of ants will not be laying pheromones, and will instead wander around the map acting as scouts. 

Antlions lie outside the world map, but can be placed in the simulation. When an ant is caught by an antlion, they will release alarm pheromones that sends nearby ants into a "panic" mode, trying to run as far away as possible from the source of the panic. This means placing antlions in the path to a food source will often lead to alarm pheromones that send the ants to navigate and create a secondary path.

**Key modifiable objects:**

1. Preplaced_food: a gameObject with script that systematically generates food in a square shape from coordinates (x_left, y_left) to (x_right, y_right) with set spacing. Modifying any of these variables can result in a food generated in a different location with a different concentration
2. Anthole: a prefab where the ants are spawned from. Maxspawn controlls the maximum amount of ants that will be spawned with no food. Ants are spawned in at evenly spaced times, separated by variable Time_to_spawn. Ant_req defines the number of food objects the ants must bring in order to spwawn a new ant.
3. Ant: a prefab describing spawned ants. Maxspeed controls the highest speed ants can have, MaxForce determines how fast they can turn, Time_between_pheromone determines frequency of laying pheromones, Check_pheromone determine the time between every pheromone check, and percent_worker determines the percentage of ants that are worker ants (ants that will lay pheromones)

**NOTE:** Modification of any variables outside of ones given may lead to unpredictable changes.

### Scene 2: Base_Evolution
This is the base model used for evolution within spiders. It is a large open world with simple borders. Upon build, rounds where predators (wolf-spiders) compete to eat ants will occur. Ants are given a head start and will rush out of the anthole, while five predators (wolf spiders) will be spawned in a bit later. Wolf spiders have an Energy bar, which replenishes whenever an ant is eaten. If a wolf spider runs out of energy, it dies.

In addition, Wolf spiders all have a SpiderGenome attached to them, with four main characteristics: mass, speed, sense, and scaredness. Mass is used in determining initial HP and calculating HP loss when eating an ant (higher mass difference leads to less HP loss), speed determines travel speed in simulation, sense determines its detection radius for ants, and scaredness represents the maximum amount of ants a spider is willing to have in its detection radius at any given time. Higher values in mass, speed, and sense leads to faster energy loss. The genome also has an "age" variable, representing the total number of spiders before itself that has survived posessing this genome.

Each round, a wolf spider must eat a predetermined amount of ants to survive. After eating that amount, it will then need to survive for 30 additional seconds, before becoming passive and being able to guarantee passing down their genome in the next round. 

Eating an ant will cause the wolf spider to lose HP, and if HP ever reaches 0, it dies. Eating ants within a short time frame will cause the damage taken to scale exponentially. Eating an ant also stuns the wolf spider for a set amount of time, during which it is vulnerable to continuous attacks.

At the end of a round, the wolf spider with the highest fitness (current energy / max energy ratio) has its genome cloned directly into the next generation. In addition, all surviving spiders posessing a fitness above a basefitness value, except the one with lowest fitness, will also be able to pass on their genes, albit with a random mutation attached. The simulation will be cleared before restarting a second round automatically.

The simulation will automatically save the 5 genomes used in creation of its round within a .json file. Additionally, it will save the current generation number, number of survivors from the past round, and number of spiders that died from losing too much HP from ants.

**Key modifiable objects:**
1. EvolutionManager: a gameObject in charge of generating various genomes for ants and processing their mutation. Speedrange, Senserange, Massrange, Minscared and Maxscard all determine the possible ranges of corresponding traits. Mutstrength represents the float value to the strength of mutations occuring on speed, sense, and mass. Basefitness represents the current energy / max energy ratio needed to pass on a genome
2. Simulation manager: a gameObject in charge of handling and processing rounds of simulations. Key modifiable variables include Spider_spawn_time and Simtime, representing the time spiders are spawned in and the total time for a round of simulation, respectively. In addition, Spawn1, 2, 3, 4, and 5 represents the xy coordinates of spiders that will spawn in
3. Wolf_Spider: a prefab that represents the spiders that will be spawned in (found in prefab folder). MaxEnergyMult represents the starting energy, Ant_nutricious_value represents the energy gained from eating an ant, base_HP is the constant value that scales the HP ants have (HP = base_hp * mass), stuntimer represents the time a wolf spider is stunned after eating an ant, and ants_stack_timer represents the timeframe in which wolf spiders would take higher damage when eating ants within this timeframe.

**NOTE:** Modification of any variables outside of ones given may lead to unpredictable changes.

### Scene 3: Evolution_With_Food
This is a scene that combines the previous two scenes; Ants are moving in a dynamic environment with food, while wolf spiders are spawned in every round to eat ant and pass on genes. Note that within this environment, intruding on an ant trail to food often means death for the wolf spider as the amount of ants on the trail often leads to rapid health loss.

This scene contains all objects from previous scenes, so modification rules are the same.

## Contact
If you have any questions regarding the simulation and/or process of how it's made, feel free to reach out!

Luke Song - lukesong2008@gmail.com

Project Link: [https://github.com/your_username/repo_name](https://github.com/chengzansong/Ants_project)


