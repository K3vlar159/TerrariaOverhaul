// Copyright (c) 2020-2025 Mirsario & Contributors.
// Released under the GNU General Public License 3.0.
// See LICENSE.md for details.

namespace TerrariaOverhaul.Common.Movement;

internal class CoyoteTimeLogic
{
	public uint DurationInTicks { get; set; } = 13;
	public uint RemainingTicks { get; private set; }
	public bool IsActive => RemainingTicks > 0;
	
	public bool ShouldActivate(float oldVelocityY, float velocityY)
	{
		return oldVelocityY == 0f && velocityY > 0f;
	}
	
	public void Activate()
	{
		RemainingTicks = DurationInTicks;
	}
	
	public void Deactivate()
	{
		RemainingTicks = 0;
	}
	
	public void Update()
	{
		if (RemainingTicks > 0) {
			RemainingTicks--;
		}
	}
	
	public bool CanCoyoteJump(float velocityY, bool controlJump)
	{
		return velocityY != 0f && controlJump && IsActive;
	}

	public static double TicksToMilliseconds(uint ticks, int fps)
	{
		return ticks / (double)fps * 1000.0;
	}
}
