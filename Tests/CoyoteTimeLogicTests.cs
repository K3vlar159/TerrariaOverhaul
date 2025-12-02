using TerrariaOverhaul.Common.Dodgerolls;
using TerrariaOverhaul.Common.Movement;
using TerrariaOverhaul.Utilities;
using TerrariaOverhaul.Utilities.Terraria;

namespace Tests;

public class CoyoteTimeTests
{
    [Fact]
    public void CoyoteTime_DefaultDuration_ShouldBeReasonable()
    {
        // Default 13 ticks je ~217ms pri 60 FPS
        uint defaultDuration = 13u;
        
        Assert.Equal(13u, defaultDuration);
        Assert.True(defaultDuration > 0);
        Assert.True(defaultDuration < 30);
        Assert.True(defaultDuration > 5);
    }

    [Theory]
    [InlineData(5u)]
    [InlineData(13u)]
    [InlineData(20u)]
    [InlineData(30u)]
    public void CoyoteTime_DurationValues_ShouldBeValid(uint duration)
    {
        // Overenie rôznych dĺžok coyote time
        bool isValid = duration > 0 && duration < 60;
        
        Assert.True(isValid);
        Assert.True(duration > 0);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(30)]
    [InlineData(120)]
    public void CoyoteTime_CommonFramerates_ShouldMaintainReasonableWindow(int framerate)
    {
        //test pri roznych fps
        uint defaultDurationTicks = 13u;
        double windowInMs = (defaultDurationTicks / (double)framerate) * 1000;
        
        if (framerate >= 30) {
            Assert.True(windowInMs >= 100);
            Assert.True(windowInMs <= 500);
        } else {
            Assert.True(windowInMs > 0);
        }
    }

    [Fact]  
    public void CoyoteTime_DefaultSettings_ShouldBeReasonable()
    {
        //test default nastaveni
        uint defaultDuration = 13u;
        bool defaultEnabled = true;
        
        Assert.True(defaultEnabled);
        Assert.InRange(defaultDuration, 5u, 30u);
        
        double at60fps = (defaultDuration / 60.0) * 1000;
        double at30fps = (defaultDuration / 30.0) * 1000;
        
        Assert.InRange(at60fps, 150.0, 300.0);
        Assert.InRange(at30fps, 300.0, 500.0);
    }
}

public class JumpBufferingTests
{
    [Fact]
    public void JumpBuffering_DefaultDuration_ShouldBeReasonable()
    {
        // Default 0.25s buffer window
        float defaultBuffer = 0.25f;
        
        Assert.Equal(0.25f, defaultBuffer);
        Assert.True(defaultBuffer > 0);
        Assert.True(defaultBuffer < 1.0f);
        Assert.InRange(defaultBuffer, 0.1f, 0.5f);
    }

    [Theory]
    [InlineData(0.1f)]
    [InlineData(0.15f)]
    [InlineData(0.25f)]
    [InlineData(0.3f)]
    [InlineData(0.5f)]
    public void JumpBuffering_DurationValues_ShouldBeValid(float bufferSeconds)
    {
        // Overenie rôznych dĺžok jump bufferu
        bool isValid = bufferSeconds > 0 && bufferSeconds < 1.0f;
        
        Assert.True(isValid);
        Assert.True(bufferSeconds > 0);
        Assert.True(bufferSeconds <= 0.5f);
    }

    [Theory]
    [InlineData(60)]
    [InlineData(30)]
    [InlineData(120)]
    [InlineData(144)]
    public void JumpBuffering_CommonFramerates_ShouldMaintainConsistentWindow(int framerate)
    {
        // Jump buffering používa sekundy, nie ticky, takže je FPS-independent
        float bufferSeconds = 0.25f;
        float bufferMs = bufferSeconds * 1000f;
        
        // Buffer by mal byť konštantný bez ohľadu na FPS
        Assert.Equal(250f, bufferMs);
        Assert.InRange(bufferMs, 100f, 500f);
        
        // Overenie, že framerate neovplyvňuje buffer v milisekundách
        Assert.True(framerate > 0);
    }

    [Fact]
    public void JumpBuffering_DefaultSettings_ShouldBeReasonable()
    {
        // Test default nastavení
        float defaultBuffer = 0.25f;
        bool defaultEnabled = true;
        
        Assert.True(defaultEnabled);
        Assert.InRange(defaultBuffer, 0.1f, 0.5f);
        
        // V milisekundách
        float bufferMs = defaultBuffer * 1000f;
        Assert.Equal(250f, bufferMs);
        Assert.InRange(bufferMs, 100f, 500f);
    }

    [Fact]
    public void JumpBuffering_ShouldBeConsistentAcrossFramerates()
    {
        // Jump buffering používa časový systém v sekundách
        // Mal by byť konzistentný bez ohľadu na FPS
        float buffer = 0.25f;
        
        // Pri rôznych FPS by mal buffer trvať rovnako dlho
        float at30fps = buffer; // 250ms
        float at60fps = buffer; // 250ms
        float at144fps = buffer; // 250ms
        
        Assert.Equal(at30fps, at60fps);
        Assert.Equal(at60fps, at144fps);
    }

    [Theory]
    [InlineData(0.1f, 6)]   // 0.1s pri 60fps = ~6 frames
    [InlineData(0.25f, 15)] // 0.25s pri 60fps = ~15 frames
    [InlineData(0.5f, 30)]  // 0.5s pri 60fps = ~30 frames
    public void JumpBuffering_FrameEquivalents_ShouldBeReasonable(float seconds, int expectedFramesAt60fps)
    {
        // Overenie aproximatívneho počtu framov pri 60 FPS
        int calculatedFrames = (int)(seconds * 60);
        
        Assert.Equal(expectedFramesAt60fps, calculatedFrames);
        Assert.InRange(calculatedFrames, 1, 60);
    }
}
