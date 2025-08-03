extends Node3D

# simple varible storing rotation speed
var rotation_speed = Vector3(0, 90, 0) # degrees per second on Y-axis
var burger = Vector3(0,90,0) 

# func _process(delta) is called every frame
func _process(delta):
	# rotation_degrees is inherited from Node3D
	rotation_degrees += rotation_speed * delta
