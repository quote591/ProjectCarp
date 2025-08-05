extends Node3D

# welcome to fish spin, we honour this script as being the classic
# FISH JUMPSCARE
# this fish has helped us a lot during the development of the game
# and its crucial that this three line GDscript never is damaged or removed
# take good care of it x
# oh and the fish

# simple varible storing rotation speed
var rotation_speed = Vector3(0, 90, 0) # degrees per second on Y-axis

# func _process(delta) is called every frame
func _process(delta):
	# rotation_degrees is inherited from Node3D
	rotation_degrees += rotation_speed * delta
