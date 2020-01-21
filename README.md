# Petal Spear
A top-down arena (space) shooter game. Play 1v1 against an AI opponent.
Made in Unity and designed for PC.
The game was created for non-profit, educational purposes.

## Game Rules
Both the player and the AI opponent control their own ship and are both constrained by the same movement rules. Ships can thrust forward and backward in the direction that they are facing. To change direction, ships can rotate at constant angular velocity clockwise or counterclockwise. There is also no deceleration, so the ship will need to manually thrust in the opposite direction in order to slow down. Ships can also perform a dodge roll either toward the left or right. Performing a dodge roll will also grant that ship 0.2 seconds of invincibility.

By default, both ships have a standard weapon with infinite ammunition to fire with. Random weapon pickups will spawn at random intervals, and ships can pick them up by colliding with them. Picking these up will provide the ship with a heavy weapon. Heavy weapons have limited ammunition, but also have greater firepower than the standard weapon. A ship can only carry one heavy weapon at a time, so picking up a new one will overwrite the current weapon.

Both ships start with five lives. Each time a ship is destroyed, their lives are reduced by one. The first ship to reduce their opponent’s number of lives to zero is the winner. Ships respawn after 3 seconds at a random location in the arena.

## Download
Check [releases](https://github.com/HenryLoo/PetalSpear/releases) for the latest build.

---

Made by: Henry Loo

Space background asset: [VectorStock](https://www.vectorstock.com/)

Sound effects: [Subspace Continuum](https://store.steampowered.com/app/352700/Subspace_Continuum/)

Ship model: [Quaternius](http://quaternius.com/assets.html)
