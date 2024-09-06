
namespace FLCore;

using Godot;

internal static class Extension_Camera
{
	#region Public Methods
	
	public static RaycastInfo Pick(this Camera3D camera, Vector2 position, uint layer)
	{
		Vector3 origin = camera.ProjectRayOrigin(position);
		Vector3 direction = camera.ProjectRayNormal(position);
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(
			origin,
			direction * camera.Far,
			layer
		);
		
		return (RaycastInfo)(camera.GetWorld3D().DirectSpaceState.IntersectRay(query));
	}
	
	#endregion // Public Methods
}
