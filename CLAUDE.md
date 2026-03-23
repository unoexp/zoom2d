# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity 2D project (Unity 2022.3.62f3) with a survival game architecture. The project uses a layered architecture with numbered directories to enforce dependency flow.

## Architecture & Code Structure

### Layered Directory Structure (`Assets/_Game/Scripts/`)
The codebase uses a numbered prefix system to enforce architectural layers and dependencies:

1. **`01_Data/`** - Data layer (ScriptableObjects, save data, configurations)
2. **`02_Base/`** - Infrastructure layer (foundational systems, services)
3. **`03_Core/`** - Core systems layer (game logic, survival systems)
4. **`04_Gameplay/`** - Gameplay systems layer (currently empty, for future expansion)
5. **`05_Show/`** - Presentation layer (currently empty, for UI/visuals)
6. **`06_Extensions/`** - Extensions and utilities (Mod system, Editor tools)
7. **`07_Shared/`** - Shared constants and enums

**Dependency Flow:** Lower numbered layers can depend on higher numbered layers, but not vice versa (e.g., `02_Base` can use `07_Shared`, but `07_Shared` cannot use `02_Base`).

### Key Infrastructure Systems

1. **ServiceLocator** (`02_Base/ServiceLocater/ServiceLocator.cs`) - Lightweight service registration system used instead of global singletons for better testability.

2. **MonoSingleton** (`02_Base/Singleton/MonoSingleton.cs`) - MonoBehaviour singleton base class, used only for infrastructure managers (Audio/VFX). Business logic should use ServiceLocator.

3. **TimerSystem** (`02_Base/Timer/`) - Object-pooled timer system with handle-based API. Supports single, loop, and limited-loop timers with real-time/scaled time modes.

4. **EventBus** (`02_Base/EventBus/`) - Event system for decoupled communication. Events are defined in the `Events` subdirectory.

### Core Systems

1. **SurvivalStatusSystem** (`03_Core/SurvivalStatus/`) - Manages survival attributes (health, hunger, thirst, temperature) with status effects and decay systems.

2. **Save System** (`03_Core/Save/ISaveable.cs`) - Interface for saveable objects.

### Data Layer

1. **ScriptableObjects** (`01_Data/ScriptableObjects/`) - Game configuration and item definitions.
2. **SaveData** (`01_Data/SaveData/`) - Save data structures.

## Development Workflow

### Opening the Project
- Open `doom2d.sln` in Visual Studio/Rider for code editing
- Open the Unity project in Unity 2022.3.62f3 or compatible version

### Package Management
- Packages are managed via `Packages/manifest.json`
- Unity Registry packages only - no external package sources configured

### Testing
- Unity Test Framework (`com.unity.test-framework`) is included in dependencies
- No custom test assemblies found in the project structure

### Code Style & Conventions
- Chinese comments used for documentation (代码注释使用中文)
- C# files include detailed headers with file paths and purpose descriptions
- Performance optimizations noted in comments (e.g., "[PERF]" markers)
- Object pooling used for performance-critical systems (TimerSystem)

## Important Notes

1. **Service Registration**: Core systems register themselves with ServiceLocator in their `Awake()` method (see `SurvivalStatusSystem.Awake()` for example).

2. **Timer Usage**: Use `TimerSystem.Instance.Create()` for timers instead of Unity's `Invoke` for better control and performance.

3. **Event-Based Communication**: Use EventBus for decoupled system communication rather than direct references.

4. **No Assembly Definitions**: The project does not use .asmdef files for assembly separation - all scripts are in the default assembly.

5. **Third-Party Code**: The `ThirdPart/` directory contains external tools (claude-code-proxy), not game code.

6. **No CI/CD Configuration**: No build scripts, CI files, or automated deployment found.

7. **Editor Tools**: Check `06_Extensions/Editor/` for custom editor validation tools (e.g., `ItemAssetValidator.cs`).