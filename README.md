# Blasty Boy - VR Jam 2020

https://request.itch.io/blasty-boy

## About 
This game was made in 7 days for VR Jam 2020. In this game, you wear a blaster/gauntlet (not unlike Samus from the Metroid series), and can battle against free Unity Asset store robots~. "Use your blaster to pull yourself towards any environment - or to pull your enemies towards you (and make them ragdoll in the process!)".

Of course, please excuse my spaghetti. Game jams be like that

### Some topics of interest

Here's a high level list of things used to make this game, that might be of interest:
* Unity, SteamVR 
* Finite State Machine (idle state, shoot state, ragdoll state)
* Inverse kinematics 
  * In the Shoot State, enemies aim their gun towards the player, independent of the base aiming animation. The enemies also will rotate their heads to face the player. 
  * This was necessary the case when the player is above or below the enemy. It wouldn't look good if they were trying to shoot the player, while aiming way below them!
* Simple ragdoll enemies (switching back and forth between kinematic animations with IK, and non-kinematic ragdoll)
* Force grab (select an object/ragdoll, and move it around the scene at a distance)
* Grappling as a locomotion system (grapple environment to pull yourself to it, monkeybar-style climbing)
* Very light usage of the Lightweight Render Pipeline (adding noise to projectiles and blaster glow materials, and for the hologram material) 

### Next Steps

* Properly add lighting
* Add more levels, that properly demo the grapple mechanics (vaulting over walls, grab ragdoll enemies, grab non-static environment)
* Audio (Actually add BGM)
* Chase state for enemies via navmeshes
* Actually use LWRP for post-processing

## Assets

* Engine: Unity 2019.3.3f1
	* URP
	* Animation Rig 0.2.6
	* TextMeshPro
	* Probuilder
	* Snaps (Office, sci-fi)
* SDK: SteamVR 2.5.0 (sdk 1.8.19)
* Assets:
	* Enemy models and animations: Mixamo
	* Sound: Zapsplat.com
	* Hologram texture: https://github.com/Brackeys/Shader-Graph-Tutorials
	* Skybox: "Starfield Skybox" by Pulsar Bytes
