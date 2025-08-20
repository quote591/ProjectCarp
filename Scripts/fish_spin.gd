#extends Node3D

# welcome to fish spin, we honour this script as being the classic
# FISH JUMPSCARE
# this fish has helped us a lot during the development of the game
# and its crucial that this three line GDscript never is damaged or removed
# take good care of it x
# oh and the fish

# simple varible storing rotation speed
#var rotation_speed = Vector3(0, 90, 0) # degrees per second on Y-axis

# func _process(delta) is called every frame
#func _process(delta):
	# rotation_degrees is inherited from Node3D
	#rotation_degrees += rotation_speed * delta


extends Node3D

@onready var spinner = $Fish2

var MUSIC_BUS_INDEX = AudioServer.get_bus_index("Music")

func _process(delta):
	# Get peak volume in decibels for the left (or right) channel
	var db = AudioServer.get_bus_peak_volume_left_db(MUSIC_BUS_INDEX, 0)

	# Convert dB to linear scale (0.0 to 1.0+)
	var linear = db_to_linear(db)

	# Use linear loudness to control spin speed
	var speed = lerp(0.1, 5.0, clamp(linear, 0.0, 1.0))
	spinner.rotate_y(speed * delta * 5)
