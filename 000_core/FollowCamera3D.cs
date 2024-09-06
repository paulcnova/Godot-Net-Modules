
namespace FLCore;

using Godot;

[GlobalClass] public partial class FollowCamera3D : Node3D
{
	#region Properties
	
	[Export] public Node3D Target { get; set; }
	[Export] public float FollowSpeed { get; set; } = 3.0f;
	[Export] private float minDistance = 1.0f;
	[Export] private float maxDistance = 35.0f;
	[Export] private float distance = 15.0f;
	
	public float MinDistance
	{
		get => this.minDistance;
		set
		{
			float max = this.maxDistance;
			
			this.minDistance = Mathf.Min(value, max);
			this.maxDistance = Mathf.Max(value, max);
		}
	}
	
	public float MaxDistance
	{
		get => this.maxDistance;
		set
		{
			float min = this.minDistance;
			
			this.minDistance = Mathf.Min(value, min);
			this.maxDistance = Mathf.Max(value, min);
		}
	}
	
	public float Distance
	{
		get => this.distance;
		set => this.distance = Mathf.Clamp(value, this.minDistance, this.maxDistance);
	}
	
	public Vector3 CameraRelativePosition => this.Target.Position + this.Transform.Basis.Column2 * this.distance;
	
	#endregion // Properties
	
	#region Godot Methods
	
	public override void _EnterTree()
	{
		base._EnterTree();
		
		float distance = (this.Position - this.Target.Position).Length();
		
		this.Distance = distance;
		this.Position = this.CameraRelativePosition;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
		this.Position = this.Position.Lerp(this.CameraRelativePosition, this.FollowSpeed * (float)delta);
	}
	
	#endregion // Godot Methods
}
