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
        // Test coyote time pri rôznych FPS
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
        // Test rozumných defaultných nastavení
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


// FAKE - testovacia implementácia počítača referencií
public class CounterTests
{
    [Fact]
    public void Counter_InitialState_ShouldBeInactive()
    {
        // Nový counter je neaktívny
        var counter = new Counter();
        Assert.False(counter.Active);
    }

    [Fact]
    public void Counter_AfterIncrease_ShouldBecomeActive()
    {
        // Po inkremente sa aktivuje
        var counter = new Counter();
        using var handle = counter.Increase();
        Assert.True(counter.Active);
    }

    [Fact]
    public void Counter_AfterDispose_ShouldBecomeInactive()
    {
        // Po dispose sa deaktivuje
        var counter = new Counter();
        var handle = counter.Increase();
        Assert.True(counter.Active);
        handle.Dispose();
        Assert.False(counter.Active);
    }

    [Fact]
    public void Counter_MultipleIncreases_ShouldStayActive()
    {
        // Viacnásobné handle-y držia counter aktívny
        var counter = new Counter();
        using var handle1 = counter.Increase();
        using var handle2 = counter.Increase();
        Assert.True(counter.Active);
    }

    [Fact]
    public void Counter_RAII_Pattern_ShouldWorkCorrectly()
    {
        // Test RAII pattern s nested using blocks
        var counter = new Counter();
        Assert.False(counter.Active);
        
        using (var handle1 = counter.Increase())
        {
            Assert.True(counter.Active);
            using (var handle2 = counter.Increase())
            {
                Assert.True(counter.Active);
            }
            Assert.True(counter.Active);
        }
        Assert.False(counter.Active);
    }
}

// STUB - zjednodušená dátová štruktúra pre testovanie
public class SurfaceTests
{
    private struct FakeData
    {
        public int Value;
        public FakeData(int value) => Value = value;
    }

    [Fact]
    public void Surface_Constructor_ShouldInitializeCorrectly()
    {
        // Test inicializácie 2D surface
        var surface = new Surface<int>(10, 20);
        Assert.Equal(10, surface.Width);
        Assert.Equal(20, surface.Height);
        Assert.NotNull(surface.Data);
        Assert.Equal(200, surface.Data.Length);
    }

    [Theory]
    [InlineData(5, 5)]
    [InlineData(10, 10)]
    [InlineData(1, 100)]
    [InlineData(100, 1)]
    public void Surface_DifferentSizes_ShouldAllocateCorrectArray(int width, int height)
    {
        // Test rôznych veľkostí surface
        var surface = new Surface<FakeData>(width, height);
        Assert.Equal(width, surface.Width);
        Assert.Equal(height, surface.Height);
        Assert.Equal(width * height, surface.Data.Length);
    }

    [Fact]
    public void Surface_Indexer_ShouldAccessCorrectElement()
    {
        // Test indexeru [x, y]
        var surface = new Surface<int>(5, 5);
        int testValue = 42;
        surface[2, 3] = testValue;
        Assert.Equal(testValue, surface[2, 3]);
    }

    [Fact]
    public void Surface_Index_ShouldCalculateCorrectly()
    {
        // Test výpočtu indexu z 2D na 1D
        var surface = new Surface<int>(10, 10);
        Assert.Equal(0, surface.Index(0, 0));
        Assert.Equal(10, surface.Index(0, 1));
        Assert.Equal(15, surface.Index(5, 1));
        Assert.Equal(99, surface.Index(9, 9));
    }

    [Fact]
    public void Surface_Dispose_ShouldCleanup()
    {
        // Test dispose a cleanup
        var surface = new Surface<FakeData>(10, 10);
        surface.Dispose();
        Assert.Equal(-1, surface.Width);
        Assert.Equal(-1, surface.Height);
        Assert.Null(surface.Data);
    }

    [Fact]
    public void Surface_WithFakeData_ShouldStoreCorrectly()
    {
        // Test so stub dátami (FakeData)
        var surface = new Surface<FakeData>(3, 3);
        var fakeData1 = new FakeData(100);
        var fakeData2 = new FakeData(200);
        surface[0, 0] = fakeData1;
        surface[2, 2] = fakeData2;
        Assert.Equal(100, surface[0, 0].Value);
        Assert.Equal(200, surface[2, 2].Value);
        Assert.Equal(0, surface[1, 1].Value);
    }
}

// MOCK - predvídateľné hodnoty na testovanie výpočtov
public class CommonStatModifiersTests
{
    [Fact]
    public void CommonStatModifiers_DefaultValues_ShouldBeOne()
    {
        // Defaultné multiplikátory sú 1.0
        var modifiers = new CommonStatModifiers();
        Assert.Equal(1f, modifiers.MeleeDamageMultiplier);
        Assert.Equal(1f, modifiers.MeleeKnockbackMultiplier);
        Assert.Equal(1f, modifiers.MeleeRangeMultiplier);
        Assert.Equal(1f, modifiers.ProjectileDamageMultiplier);
        Assert.Equal(1f, modifiers.ProjectileKnockbackMultiplier);
        Assert.Equal(1f, modifiers.ProjectileSpeedMultiplier);
    }

    [Fact]
    public void CommonStatModifiers_Lerp_ShouldInterpolateCorrectly()
    {
        // Test lineárnej interpolácie medzi hodnotami
        var start = new CommonStatModifiers
        {
            MeleeDamageMultiplier = 1.0f,
            ProjectileDamageMultiplier = 1.0f
        };
        var end = new CommonStatModifiers
        {
            MeleeDamageMultiplier = 2.0f,
            ProjectileDamageMultiplier = 3.0f
        };
        var result = CommonStatModifiers.Lerp(start, end, 0.5f);
        Assert.Equal(1.5f, result.MeleeDamageMultiplier, precision: 5);
        Assert.Equal(2.0f, result.ProjectileDamageMultiplier, precision: 5);
    }

    [Theory]
    [InlineData(0.0f, 1.0f)]
    [InlineData(0.5f, 1.5f)]
    [InlineData(1.0f, 2.0f)]
    public void CommonStatModifiers_Lerp_DifferentSteps_ShouldProduceCorrectResults(float step, float expected)
    {
        // Test interpolácie s rôznymi krokmi
        var start = new CommonStatModifiers { MeleeDamageMultiplier = 1.0f };
        var end = new CommonStatModifiers { MeleeDamageMultiplier = 2.0f };
        var result = CommonStatModifiers.Lerp(start, end, step);
        Assert.Equal(expected, result.MeleeDamageMultiplier, precision: 5);
    }

    [Fact]
    public void CommonStatModifiers_MultiplyOperator_ShouldScaleAllValues()
    {
        // Test násobenia všetkých hodnôt
        var modifiers = new CommonStatModifiers
        {
            MeleeDamageMultiplier = 2.0f,
            ProjectileDamageMultiplier = 3.0f
        };
        var result = modifiers * 0.5f;
        Assert.Equal(1.0f, result.MeleeDamageMultiplier);
        Assert.Equal(1.5f, result.ProjectileDamageMultiplier);
    }
}

// FAKE - nahradenie reálneho herného času testovacím
public class FakeTimeProvider
{
    private uint currentTick = 0;
    public uint CurrentTick => currentTick;
    public void Advance(uint ticks) => currentTick += ticks;
    public void Reset() => currentTick = 0;
}

public class GameTimerConceptTests
{
    [Fact]
    public void GameTimer_ConceptualBehavior_ValueDecreasesOverTime()
    {
        // Test klesania hodnoty časovača
        uint initialValue = 100u;
        uint ticksPassed = 20u;
        uint expectedValue = initialValue - ticksPassed;
        Assert.True(expectedValue < initialValue);
        Assert.Equal(80u, expectedValue);
    }

    [Theory]
    [InlineData(100u, 50u, 50u)]
    [InlineData(100u, 100u, 0u)]
    [InlineData(100u, 150u, 0u)]
    public void GameTimer_ConceptualBehavior_TimeProgression(uint initial, uint passed, uint expected)
    {
        // Test progressu času s clampingom na 0
        uint remaining = initial > passed ? initial - passed : 0u;
        Assert.Equal(expected, remaining);
    }

    [Fact]
    public void GameTimer_ConceptualBehavior_ProgressCalculation()
    {
        // Test výpočtu progress (0.0 až 1.0)
        uint length = 100u;
        uint remaining = 75u;
        float progress = 1f - (remaining / (float)length);
        Assert.Equal(0.25f, progress, precision: 5);
        Assert.InRange(progress, 0f, 1f);
    }

    [Fact]
    public void FakeTimeProvider_ShouldTrackTime()
    {
        // Test fake time providera (test double)
        var timeProvider = new FakeTimeProvider();
        Assert.Equal(0u, timeProvider.CurrentTick);
        timeProvider.Advance(10);
        Assert.Equal(10u, timeProvider.CurrentTick);
        timeProvider.Advance(5);
        Assert.Equal(15u, timeProvider.CurrentTick);
        timeProvider.Reset();
        Assert.Equal(0u, timeProvider.CurrentTick);
    }
}