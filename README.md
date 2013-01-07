FieldPong
=========

FieldPong is a physics-based pong game played in a gravitational field which is represented with a grid very much inspired by that in the Xbox Live Arcade game Geometry Wars. It's written in C# and uses XNA 2.0 and the free Farseer Physics library.

It's really not about competing with the computer as most pong games are. Rather, it's more of an endurance game where you try to go as long as you can and see how high a score you can get. Every time the ball passes the computer paddle, you gain a level. Every time the ball passes your paddle, you lose a life. You start off with three lives, and for every two levels you gain one extra life. Every time you reach the next level, the computer gets a little more vigorous and moves more quickly. Since this is an endurance game about pushing your skills to the limit, there's no cap as to how fast the computer can get. During the first few levels he's very slow; at level 10 he's as fast as you are; from there on he's faster.

It requires an Xbox 360 gamepad to play. Hit X to launch a gravity ball into the arena, which is a short-lived object with a strong gravitational pull that sucks in the ball and the obstacles in the arena.

Move your paddle around with the left thumbstick.

Spin your paddle left and right with the left and right thumbsticks, respectively. You'll want to use this ability strategically to whack the ball when it comes your way. It takes a bit of time to master it.

Finally, you can fire bullets in any direction by pointing the right thumbstick in the desired direction. The bullets don't do any sort of "damage"; rather, upon colliding with the ball or an obstacle they interact in a physically realistic way and bump the object. Thus, for example, you can fire at the ball to push it past your opponent.
