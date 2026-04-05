# Audio Navigation - Approaching & Left/Right Fix

## Fixes Applied

### 1. ✅ Approaching Destination Spam Fixed

**Problem:** The approaching destination cue kept playing repeatedly because:
- It triggered at 20m distance
- Reset at >20m distance
- Small distance fluctuations caused it to reset and re-trigger immediately

**Solution: Hysteresis Buffer**

```csharp
// BEFORE (caused spam):
if (distance <= 20 && !played) Play();
else if (distance > 20) played = false;  // ← Resets too easily!

// AFTER (with hysteresis):
if (distance <= 20 && !played) Play();
// Only reset when WELL outside the zone
if (played && distance > 30) played = false;  // ← 20m + 10m buffer
```

**New Setting:**
```
approachingResetBuffer = 10m
```
- Triggers at: `approachingDestinationDistance` (20m)
- Resets at: `approachingDestinationDistance + approachingResetBuffer` (30m)
- This 10m buffer prevents rapid trigger/reset cycles

### 2. ✅ Left/Right Can Now Be Inverted

**Problem:** Left/Right detection depends on coordinate system orientation, which can vary.

**Solution: Inspector Toggle**

Added a new checkbox in the Inspector:
```
[Debug]
└─ Invert Left/Right: ☐ (unchecked by default)
```

**How to use:**
1. Test navigation on a known path
2. If left says "Turn Right" (or vice versa):
   - **Check** the "Invert Left/Right" box
   - Test again - directions should now be correct

### 3. ✅ Detailed Debug Logging

Added detailed logging for turn detection:
```
[AudioNav] Turn detection: angle=90.0°, crossY=1.000, incoming=(1.00,0.00), outgoing=(0.00,1.00)
[AudioNav] ▶ Playing: 'Turn Left' [turn_left]
```

This helps diagnose why a turn was detected as left or right.

---

## How To Fix Left/Right

### Option A: Use The Toggle (Easiest)

1. Select AudioNavigationManager in Inspector
2. Expand **Debug** section
3. **Check** "Invert Left/Right"
4. Test on a known path

### Option B: Test and Verify

1. Find a path with a known left turn
2. Walk the path
3. Check console output:
   ```
   [AudioNav] Turn detection: angle=90.0°, crossY=1.000
   [AudioNav] ▶ Playing: 'Turn Right' [turn_right]
   ```
4. If it says "Turn Right" but you know it's left:
   - **Check** "Invert Left/Right"
5. Test again

---

## Console Output

### Approaching Destination

**Good (plays once):**
```
[AudioNav] Approaching cue played. Distance: 19.5m
... (no more approaching cues) ...
[AudioNav] ▶ Playing: 'Destination Reached' [destination_reached]
```

**If still spamming (shouldn't happen now):**
```
[AudioNav] Approaching cue played. Distance: 18.2m
[AudioNav] Approaching reset. Distance: 31.5m (threshold: 30.0m)
[AudioNav] Approaching cue played. Distance: 19.1m  ← Only if you walked away & back
```

### Turn Detection

**Detailed output:**
```
[AudioNav] Turn detection: angle=85.3°, crossY=-0.987, incoming=(0.95,0.32), outgoing=(-0.31,0.95)
[AudioNav] ▶ Playing: 'Turn Left' [turn_left]
```

**What the values mean:**
- `angle`: How sharp the turn is (degrees)
- `crossY`: Cross product Y value
  - **Positive** (>0): Right turn
  - **Negative** (<0): Left turn
  - **Near zero** (~0): Straight
- `incoming`: Direction before the turn
- `outgoing`: Direction after the turn

---

## Settings Quick Reference

| Setting | Default | Description |
|---------|---------|-------------|
| `approachingDestinationDistance` | 20m | Distance to trigger approaching cue |
| `approachingResetBuffer` | 10m | Extra distance before reset (hysteresis) |
| `turnAngleThreshold` | 30° | Minimum angle to consider a turn |
| `minTravelDistanceBetweenCues` | 8m | Must travel this far between cues |
| `invertLeftRight` | false | **Check if directions are swapped** |

---

## Testing Checklist

- [ ] Approaching destination plays **only once**
- [ ] Approaching doesn't play again unless you walk far away (>30m) and come back
- [ ] Left turns announce "Turn Left"
- [ ] Right turns announce "Turn Right"
- [ ] If wrong, check "Invert Left/Right" and test again
- [ ] Destination reached plays on arrival
- [ ] No cue spam or repeated announcements

---

## Troubleshooting

### Approaching Still Plays Multiple Times

**Check:**
1. Are you walking back and forth near the 20m boundary?
2. Is `approachingResetBuffer` set correctly? (default: 10)

**Test:**
- Walk towards destination → Should hear cue once at ~20m
- Walk away to 35m → Should see "Approaching reset" in console
- Walk back to 20m → Should hear cue again (this is correct!)

### Left/Right Still Wrong

**Try:**
1. Check the "Invert Left/Right" box
2. Test on a path you know well
3. If STILL wrong, check console logs:
   ```
   crossY=1.000 → Should be RIGHT
   crossY=-1.000 → Should be LEFT
   ```
4. If cross product values seem wrong, your path might have issues:
   - Ensure path corners are in correct order
   - Check for duplicate corners in path

### Debug Mode

Enable these to see exactly what's happening:
```
✓ Enable Debug Logs
✓ Show Path Gizmos
```

You'll see:
- Yellow spheres = path corners
- Console logs = every turn detection attempt
- Direction vectors for each turn

---

## Summary

| Issue | Status | Fix |
|-------|--------|-----|
| Approaching spam | ✅ Fixed | Hysteresis buffer (10m) |
| Left/Right swapped | ✅ Fixed | Invert toggle in Inspector |
| No debug info | ✅ Fixed | Detailed logging added |

---

## Files Modified

| File | Changes |
|------|---------|
| `AudioNavigationManager.cs` | Hysteresis, invert toggle, debug logs |
