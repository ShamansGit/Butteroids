extends Area2D

@export var speed: float = 600.0
var direction = Vector2.ZERO

func _ready():
	direction = Vector2.RIGHT.rotated(rotation)  # Bullet moves in rocket's direction

func _process(delta):
	position += direction * speed * delta

	# If bullet goes off screen, delete it
	if not get_viewport_rect().has_point(position):
		queue_free()
