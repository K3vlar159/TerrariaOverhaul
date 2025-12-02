using TerrariaOverhaul.Common.Movement;

namespace Tests;

// Fake trieda pre testovanie
internal class FakeCoyoteTimeLogic : CoyoteTimeLogic
{
	public void SetRemainingTicks(uint ticks)
	{
		Activate();
		while (RemainingTicks > ticks && RemainingTicks > 0) {
			Update();
		}
	}
	
	public void SimulateFrames(int frameCount)
	{
		for (int i = 0; i < frameCount; i++) {
			Update();
		}
	}
}

public class CoyoteTimeLogicTests
{
	[Fact]
	public void CoyoteTime_ShouldActivate_WhenPlayerStartsFalling()
	{
		var logic = new CoyoteTimeLogic();
		float oldVelocityY = 0f;  // Stál na zemi
		float velocityY = 1f;      // Začal padať
		
		bool shouldActivate = logic.ShouldActivate(oldVelocityY, velocityY);
		
		Assert.True(shouldActivate);
	}
	
	[Fact]
	public void CoyoteTime_ShouldNotActivate_WhenPlayerIsJumping()
	{
		var logic = new CoyoteTimeLogic();
		float oldVelocityY = -5f;  // Skákal
		float velocityY = -3f;     // Stále v skoku
		
		bool shouldActivate = logic.ShouldActivate(oldVelocityY, velocityY);
		
		Assert.False(shouldActivate);
	}
	
	[Fact]
	public void CoyoteTime_Activate_ShouldSetRemainingTicks()
	{
		var logic = new CoyoteTimeLogic();
		
		logic.Activate();
		
		Assert.True(logic.IsActive);
		Assert.Equal(13u, logic.RemainingTicks);
	}
	
	[Fact]
	public void CoyoteTime_Update_ShouldDecreaseRemainingTicks()
	{
		var logic = new CoyoteTimeLogic();
		logic.Activate();
		
		logic.Update();
		
		Assert.Equal(12u, logic.RemainingTicks);
		Assert.True(logic.IsActive);
	}
	
	[Fact]
	public void CoyoteTime_Update_ShouldExpireAfterDuration()
	{
		var logic = new CoyoteTimeLogic();
		logic.Activate();
		
		for (int i = 0; i < 13; i++) {
			logic.Update();
		}
		
		Assert.Equal(0u, logic.RemainingTicks);
		Assert.False(logic.IsActive);
	}
	
	[Fact]
	public void CoyoteTime_CanCoyoteJump_WhenActive()
	{
		var logic = new CoyoteTimeLogic();
		logic.Activate();
		
		bool canJump = logic.CanCoyoteJump(
			velocityY: 2f,      // Padá
			controlJump: true   // Stlačil skok
		);
		
		Assert.True(canJump);
	}
	
	[Fact]
	public void CoyoteTime_CannotCoyoteJump_WhenInactive()
	{
		var logic = new CoyoteTimeLogic();
		
		bool canJump = logic.CanCoyoteJump(
			velocityY: 2f,
			controlJump: true
		);
		
		Assert.False(canJump);
	}
	
	[Fact]
	public void CoyoteTime_Deactivate_ShouldClearTimer()
	{
		var logic = new CoyoteTimeLogic();
		logic.Activate();
		Assert.True(logic.IsActive);
		
		logic.Deactivate();
		
		Assert.False(logic.IsActive);
		Assert.Equal(0u, logic.RemainingTicks);
	}
	
	[Fact]
	public void CoyoteTime_FullLifecycle()
	{
		var logic = new CoyoteTimeLogic();
		
		Assert.False(logic.IsActive);
		
		Assert.True(logic.ShouldActivate(0f, 1f));
		logic.Activate();
		
		Assert.True(logic.IsActive);
		Assert.Equal(13u, logic.RemainingTicks);
		
		for (int i = 0; i < 5; i++) {
			logic.Update();
		}
		
		Assert.True(logic.IsActive);
		Assert.Equal(8u, logic.RemainingTicks);
		
		Assert.True(logic.CanCoyoteJump(2f, true));
		logic.Deactivate();
		
		Assert.False(logic.IsActive);
		Assert.Equal(0u, logic.RemainingTicks);
	}
	
	// ===== FAKE TESTY =====
	
	[Fact]
	public void FakeCoyoteTime_SimulateFrames_ShouldUpdateMultipleTimes()
	{
		var fake = new FakeCoyoteTimeLogic();
		fake.Activate();
		
		fake.SimulateFrames(5);
		
		Assert.Equal(8u, fake.RemainingTicks);
		Assert.True(fake.IsActive);
	}
	
	[Fact]
	public void FakeCoyoteTime_SimulateFrames_ShouldExpireCorrectly()
	{
		var fake = new FakeCoyoteTimeLogic();
		fake.Activate();
		
		fake.SimulateFrames(20);  // Viac než duration
		
		Assert.Equal(0u, fake.RemainingTicks);
		Assert.False(fake.IsActive);
	}
	
}
