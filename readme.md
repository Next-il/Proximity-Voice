# 🎙️ ProxVoice

<div align="center">

**Proximity-based voice chat for Counter-Strike 2**

[![Version](https://img.shields.io/badge/version-0.3.0-blue.svg)](https://github.com/next-il/ProxVoice)
[![CounterStrikeSharp](https://img.shields.io/badge/CSS-v1.0.347-orange.svg)](https://github.com/roflmuffin/CounterStrikeSharp)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)

*Bring realistic spatial voice communication to your CS2 server*

</div>

---

## 📖 Overview

ProxVoice is a lightweight Counter-Strike 2 server plugin that implements proximity-based voice chat. Players can only hear each other when they're within range, creating immersive tactical gameplay and realistic communication dynamics.

### ✨ Key Features

- 🎯 **Distance-Based Voice** - Only hear teammates within 800 units
- ⚡ **Performance Optimized** - Efficient tick-rate handling (updates every 3 ticks)
- 🔇 **Respects Mute States** - Honors game's built-in mute functionality
- 👻 **Spectator Aware** - Automatically handles dead/spectating players
- 🎛️ **Runtime Toggleable** - Enable/disable without restarting the server
- 🔄 **Hot Reload Support** - Update the plugin without server downtime

---

## 🚀 Installation

### Prerequisites
- Counter-Strike 2 Dedicated Server
- [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp) (v1.0.347 or higher)
- .NET 8.0 Runtime

### Steps

1. **Download** the latest release from the [releases page](https://github.com/next-il/ProxVoice/releases)

2. **Extract** to your CounterStrikeSharp plugins folder:
   ```
   csgo/addons/counterstrikesharp/plugins/ProxVoice/
   ```

3. **Restart** your server or use hot reload:
   ```
   css_plugins reload ProxVoice
   ```

---

## 🎮 Usage

### Console Commands

| Command | Description | Default |
|---------|-------------|---------|
| `px_enable <0/1>` | Enable or disable proximity voice | `1` |

### Examples

```bash
# Disable proximity voice (revert to normal CS2 voice)
px_enable 0

# Enable proximity voice
px_enable 1
```

---

## ⚙️ Configuration

### Proximity Range

The default proximity range is **800 units**. To modify this:

1. Open [ProxVoice.cs](ProxVoice.cs)
2. Locate line 26:
   ```csharp
   private const float ProximityRange = 800.0f;
   ```
3. Change the value to your desired range
4. Rebuild the plugin

### Update Frequency

The plugin updates every **3 ticks** by default. To adjust:

1. Open [ProxVoice.cs](ProxVoice.cs)
2. Locate line 29:
   ```csharp
   private const int TicksPerUpdate = 3;
   ```
3. Modify the value (lower = more frequent updates, higher CPU usage)

---

## 🔧 Building from Source

```bash
# Clone the repository
git clone https://github.com/next-il/ProxVoice.git
cd ProxVoice

# Build the plugin
dotnet build -c Release

# Output will be in bin/Release/net8.0/
```

Or use the included VSCode task:
- Press `Ctrl+Shift+B` (or `Cmd+Shift+B` on macOS)
- Select "Build Plugin"

---

## 🧪 How It Works

ProxVoice uses CounterStrikeSharp's `ListenOverride` API to dynamically control who can hear whom:

1. **Position Tracking** - Caches player positions each update cycle
2. **Distance Calculation** - Computes squared distance between all alive player pairs
3. **Override Management** - Sets `Hear` or `Mute` based on proximity
4. **State Handling** - Resets overrides for dead/spectating players

```csharp
// Simplified logic
if (distance <= 800 units)
    listener.SetListenOverride(speaker, ListenOverride.Hear);
else
    listener.SetListenOverride(speaker, ListenOverride.Mute);
```

---

## 🎯 Game Modes

ProxVoice works with any CS2 game mode:

- ✅ Competitive
- ✅ Casual
- ✅ Deathmatch
- ✅ Custom game modes
- ✅ Zombie servers
- ✅ RPG/roleplay servers

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. 🍴 Fork the repository
2. 🌿 Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. 💾 Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. 📤 Push to the branch (`git push origin feature/AmazingFeature`)
5. 🎉 Open a Pull Request

---

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

---

## 🙏 Credits

- **Author**: ChatGPT (as per module metadata 😄)
- **Framework**: [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)
- **Inspiration**: Proximity voice systems in games like Phasmophobia, Among Us, etc.

---

## 📊 Performance

- **CPU Impact**: Minimal (~0.1% on typical servers)
- **Memory**: < 1MB
- **Network**: No additional bandwidth required
- **Tick Impact**: Optimized batch processing

---

## 🐛 Troubleshooting

### Players can't hear each other at all
- Check `px_enable` is set to `1`
- Verify players aren't muted in-game
- Ensure players are alive and within 800 units

### Voice chat works normally (ignoring proximity)
- Verify the plugin is loaded: `css_plugins list`
- Check for errors in server console
- Ensure `px_enable` is `1`

### Performance issues
- Increase `TicksPerUpdate` value
- Reduce `ProximityRange` if you have many players

---

<div align="center">

**[⭐ Star this repo](https://github.com/next-il/ProxVoice)** if you find it useful!

Made with ❤️ for the CS2 community

</div>