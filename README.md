# RagdollGA

WIP

This a genetic algorithm that hopes to evolve walking for unity ragdoll.
The algorithm generates a series of random forces for each ragdoll to be used as genome.
The genome is crossed-over with the best candidates and randomly mutated.
The fitness score is calculated by the distance moved * the time before the ragdoll body hits the ground
