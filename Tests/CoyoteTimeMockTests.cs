using Moq;

namespace Tests;

// Interface pre hráčsky stav
public interface IPlayerState
{
    float VelocityY { get; set; }
    float OldVelocityY { get; set; }
    bool IsJumpPressed { get; }
    bool IsGrounded { get; }
}

// Interface pre tick systém
public interface ITickSystem
{
    uint CurrentTick { get; }
}

// Coyote Time systém s mocknutými závislosťami
public class CoyoteTimeSystem
{
    private readonly ITickSystem _tickSystem;
    private uint _startTick;
    private uint _endTick;
    
    public const uint DefaultDuration = 13u;
    public uint Duration { get; set; } = DefaultDuration;
    
    public bool IsActive => _tickSystem.CurrentTick < _endTick;
    public uint RemainingTicks => IsActive ? _endTick - _tickSystem.CurrentTick : 0;
    
    public CoyoteTimeSystem(ITickSystem tickSystem)
    {
        _tickSystem = tickSystem;
    }
    
    public void Update(IPlayerState player)
    {
        // Aktivuj timer keď hráč začne padať z platformy
        bool startedFalling = player.OldVelocityY == 0f && player.VelocityY > 0f;
        
        if (startedFalling)
        {
            _startTick = _tickSystem.CurrentTick;
            _endTick = _startTick + Duration;
        }
    }
    
    public bool CanCoyoteJump(IPlayerState player)
    {
        // Môže skočiť ak je vo vzduchu, stláča jump a timer je aktívny
        return player.VelocityY != 0f && player.IsJumpPressed && IsActive;
    }
    
    public void ConsumeTimer()
    {
        _endTick = 0;
    }
}

public class CoyoteTimeMockTests
{
    [Fact]
    public void CoyoteTime_ShouldActivateWhenFallingOffLedge()
    {
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(100);
        
        var mockPlayer = new Mock<IPlayerState>();
        mockPlayer.Setup(p => p.OldVelocityY).Returns(0f); // Bol na zemi
        mockPlayer.Setup(p => p.VelocityY).Returns(1f);     // Teraz padá
        
        var system = new CoyoteTimeSystem(mockTick.Object);
        
        // ACT
        system.Update(mockPlayer.Object);
        
        // ASSERT
        Assert.True(system.IsActive);
        Assert.Equal(13u, system.RemainingTicks);
        
        // VERIFY
        mockTick.Verify(t => t.CurrentTick, Times.AtLeastOnce());
        mockPlayer.Verify(p => p.OldVelocityY, Times.Once());
        mockPlayer.Verify(p => p.VelocityY, Times.Once());
    }
    
    [Fact]
    public void CoyoteTime_ShouldNotActivateWhenAlreadyFalling()
    {
        // ARRANGE
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(100);
        
        var mockPlayer = new Mock<IPlayerState>();
        mockPlayer.Setup(p => p.OldVelocityY).Returns(5f); // Už padal
        mockPlayer.Setup(p => p.VelocityY).Returns(6f);    // Stále padá
        
        var system = new CoyoteTimeSystem(mockTick.Object);
        
        // ACT
        system.Update(mockPlayer.Object);
        
        // ASSERT
        Assert.False(system.IsActive);
        Assert.Equal(0u, system.RemainingTicks);
    }
    
    [Fact]
    public void CoyoteTime_ShouldExpireAfterDurationTicks()
    {
        // ARRANGE
        var currentTick = 100u;
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(() => currentTick);
        
        var mockPlayer = new Mock<IPlayerState>();
        mockPlayer.Setup(p => p.OldVelocityY).Returns(0f);
        mockPlayer.Setup(p => p.VelocityY).Returns(1f);
        
        var system = new CoyoteTimeSystem(mockTick.Object);
        
        // ACT - Aktivuj timer
        system.Update(mockPlayer.Object);
        Assert.True(system.IsActive);
        Assert.Equal(13u, system.RemainingTicks);
        
        // Simuluj prechod času - tick 105 (5 tikov neskôr)
        currentTick = 105;
        Assert.True(system.IsActive);
        Assert.Equal(8u, system.RemainingTicks);
        
        // Simuluj prechod času - tick 113 (presne na konci)
        currentTick = 113;
        Assert.False(system.IsActive);
        Assert.Equal(0u, system.RemainingTicks);
        
        // VERIFY
        mockTick.Verify(t => t.CurrentTick, Times.AtLeast(3));
    }
    
    [Theory]
    [InlineData(5u, 5u)]
    [InlineData(13u, 13u)]
    [InlineData(20u, 20u)]
    [InlineData(30u, 30u)]
    public void CoyoteTime_ShouldRespectCustomDuration(uint customDuration, uint expectedRemaining)
    {
        // ARRANGE
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(100);
        
        var mockPlayer = new Mock<IPlayerState>();
        mockPlayer.Setup(p => p.OldVelocityY).Returns(0f);
        mockPlayer.Setup(p => p.VelocityY).Returns(1f);
        
        var system = new CoyoteTimeSystem(mockTick.Object)
        {
            Duration = customDuration
        };
        
        // ACT
        system.Update(mockPlayer.Object);
        
        // ASSERT
        Assert.Equal(expectedRemaining, system.RemainingTicks);
    }
    
    [Fact]
    public void CoyoteTime_CanCoyoteJump_WhenConditionsMet()
    {
        // ARRANGE
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(100);
        
        var system = new CoyoteTimeSystem(mockTick.Object);
        
        // Aktivuj timer
        var mockPlayerFalling = new Mock<IPlayerState>();
        mockPlayerFalling.Setup(p => p.OldVelocityY).Returns(0f);
        mockPlayerFalling.Setup(p => p.VelocityY).Returns(1f);
        system.Update(mockPlayerFalling.Object);
        
        // Teraz hráč stláča jump vo vzduchu
        var mockPlayerJumping = new Mock<IPlayerState>();
        mockPlayerJumping.Setup(p => p.VelocityY).Returns(2f);      // Vo vzduchu
        mockPlayerJumping.Setup(p => p.IsJumpPressed).Returns(true); // Stláča jump
        
        // ACT
        bool canJump = system.CanCoyoteJump(mockPlayerJumping.Object);
        
        // ASSERT
        Assert.True(canJump);
        mockPlayerJumping.Verify(p => p.VelocityY, Times.Once());
        mockPlayerJumping.Verify(p => p.IsJumpPressed, Times.Once());
    }
    
    [Fact]
    public void CoyoteTime_CannotCoyoteJump_WhenNotPressingJump()
    {
        // ARRANGE
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(100);
        
        var system = new CoyoteTimeSystem(mockTick.Object);
        
        // Aktivuj timer
        var mockPlayerFalling = new Mock<IPlayerState>();
        mockPlayerFalling.Setup(p => p.OldVelocityY).Returns(0f);
        mockPlayerFalling.Setup(p => p.VelocityY).Returns(1f);
        system.Update(mockPlayerFalling.Object);
        
        // Hráč vo vzduchu ale NEStláča jump
        var mockPlayerNoJump = new Mock<IPlayerState>();
        mockPlayerNoJump.Setup(p => p.VelocityY).Returns(2f);
        mockPlayerNoJump.Setup(p => p.IsJumpPressed).Returns(false);
        
        // ACT
        bool canJump = system.CanCoyoteJump(mockPlayerNoJump.Object);
        
        // ASSERT
        Assert.False(canJump);
    }
    
    [Fact]
    public void CoyoteTime_CannotCoyoteJump_WhenTimerExpired()
    {
        // ARRANGE
        var currentTick = 100u;
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(() => currentTick);
        
        var system = new CoyoteTimeSystem(mockTick.Object);
        
        // Aktivuj timer
        var mockPlayerFalling = new Mock<IPlayerState>();
        mockPlayerFalling.Setup(p => p.OldVelocityY).Returns(0f);
        mockPlayerFalling.Setup(p => p.VelocityY).Returns(1f);
        system.Update(mockPlayerFalling.Object);
        
        // Simuluj uplynutie času (14 tikov = po expirácii)
        currentTick = 114;
        
        var mockPlayerJumping = new Mock<IPlayerState>();
        mockPlayerJumping.Setup(p => p.VelocityY).Returns(2f);
        mockPlayerJumping.Setup(p => p.IsJumpPressed).Returns(true);
        
        // ACT
        bool canJump = system.CanCoyoteJump(mockPlayerJumping.Object);
        
        // ASSERT
        Assert.False(canJump);
        Assert.False(system.IsActive);
    }
    
    [Fact]
    public void CoyoteTime_ConsumeTimer_ShouldDeactivate()
    {
        // ARRANGE
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(100);
        
        var system = new CoyoteTimeSystem(mockTick.Object);
        
        // Aktivuj timer
        var mockPlayer = new Mock<IPlayerState>();
        mockPlayer.Setup(p => p.OldVelocityY).Returns(0f);
        mockPlayer.Setup(p => p.VelocityY).Returns(1f);
        system.Update(mockPlayer.Object);
        
        Assert.True(system.IsActive);
        
        // ACT
        system.ConsumeTimer();
        
        // ASSERT
        Assert.False(system.IsActive);
        Assert.Equal(0u, system.RemainingTicks);
    }
    
    [Fact]
    public void CoyoteTime_MultipleActivations_ShouldResetTimer()
    {
        // ARRANGE
        var currentTick = 100u;
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(() => currentTick);
        
        var system = new CoyoteTimeSystem(mockTick.Object);
        
        var mockPlayer = new Mock<IPlayerState>();
        mockPlayer.Setup(p => p.OldVelocityY).Returns(0f);
        mockPlayer.Setup(p => p.VelocityY).Returns(1f);
        
        // ACT - Prvá aktivácia
        system.Update(mockPlayer.Object);
        Assert.Equal(13u, system.RemainingTicks);
        
        // Uplynie 5 tikov
        currentTick = 105;
        Assert.Equal(8u, system.RemainingTicks);
        
        // Hráč pristane a znovu spadne (druhá aktivácia)
        mockPlayer.Setup(p => p.OldVelocityY).Returns(0f);
        mockPlayer.Setup(p => p.VelocityY).Returns(1f);
        system.Update(mockPlayer.Object);
        
        // ASSERT - Timer by mal byť resetnutý
        Assert.Equal(13u, system.RemainingTicks);
        Assert.True(system.IsActive);
    }
    
    [Theory]
    [InlineData(60, 13u, 216.67)] // 60 FPS
    [InlineData(30, 13u, 433.33)] // 30 FPS
    [InlineData(120, 13u, 108.33)] // 120 FPS
    public void CoyoteTime_TicksToMilliseconds_ShouldBeReasonable(int fps, uint ticks, double expectedMs)
    {
        // ARRANGE - Konverzia tikov na milisekundy
        double msPerTick = 1000.0 / fps;
        double actualMs = ticks * msPerTick;
        
        // ACT & ASSERT
        Assert.InRange(actualMs, expectedMs - 1, expectedMs + 1);
        Assert.InRange(actualMs, 100, 500); // Rozumné pre hrateľnosť
    }
    
    [Fact]
    public void CoyoteTime_WindowProgression_ShouldDecreaseOverTime()
    {
        // ARRANGE
        var currentTick = 100u;
        var mockTick = new Mock<ITickSystem>();
        mockTick.Setup(t => t.CurrentTick).Returns(() => currentTick);
        
        var system = new CoyoteTimeSystem(mockTick.Object);
        
        // Aktivuj timer
        var mockPlayer = new Mock<IPlayerState>();
        mockPlayer.Setup(p => p.OldVelocityY).Returns(0f);
        mockPlayer.Setup(p => p.VelocityY).Returns(1f);
        system.Update(mockPlayer.Object);
        
        // ACT & ASSERT - Sleduj postupné znižovanie
        var remainingValues = new List<uint>();
        
        for (uint i = 0; i <= 15; i++)
        {
            currentTick = 100 + i;
            remainingValues.Add(system.RemainingTicks);
        }
        
        // Overenie, že hodnoty klesajú
        Assert.Equal(13u, remainingValues[0]);
        Assert.Equal(7u, remainingValues[6]);
        Assert.Equal(0u, remainingValues[13]);
        Assert.Equal(0u, remainingValues[14]);
        
        // Overenie, že každá hodnota je <= predchádzajúca
        for (int i = 1; i < remainingValues.Count; i++)
        {
            Assert.True(remainingValues[i] <= remainingValues[i - 1]);
        }
    }
}
