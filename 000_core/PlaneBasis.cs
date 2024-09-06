
namespace FLCore;

using Godot;

public struct PlaneBasis
{
	#region Properties
	
	public Vector3 Forward;
	public Vector3 Right;
	public Vector3 Up;
	
	public PlaneBasis(Vector3 forward, Vector3 right, Vector3 up)
	{
		this.Forward = forward;
		this.Right = right;
		this.Up = up;
	}
	
	#endregion // Properties
}
