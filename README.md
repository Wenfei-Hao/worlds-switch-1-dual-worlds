# Spacetime Dual-World Prototype

A small Unity 3D prototype exploring **time-shift / dual-world traversal** mechanics inspired by *Titanfall 2* and *Dishonored 2*.
The project focuses on how **level design, camera systems, and shaders** can work together to turn “time travel” into a concrete gameplay tool rather than a pure visual effect.

---

## Overview

This prototype implements a first-person controller that can instantly switch between two synchronized versions of the same level:

* **World A** – the “present” world (baseline environment).
* **World B** – an alternate timeline stacked exactly **50 units above** World A on the Y-axis.

The player carries a **time device** that reveals the other world **directly inside the main camera image**: the device’s screen is not a separate mini-camera feed, but a **screen-space portal** that replaces the pixels behind it with the corresponding pixels from the alternate world.

The project is designed as a **technical exploration** for a game development–oriented CS/Math student portfolio, and as a starting point for research on spatial computing and AI-driven gameplay.

---

## Key Features

* **Instant World Switching**

  * Two structurally identical 3D levels stacked vertically.
  * A `TimeShiftController` teleports the player between worlds by applying a fixed translation in world space while preserving velocity and camera orientation.
  * Switching is effectively cost-free at runtime (no scene reloads), enabling frequent use as a core mechanic.

* **Screen-Space Time Device (Portal)**

  * A dedicated `OtherWorldCamera` mirrors the main camera’s FOV, aspect ratio, and orientation, but observes the **other** world (offset by +/−Y).
  * Its output is rendered to a `RenderTexture`.
  * A custom HLSL shader (`TimeDevicePortal`) uses clip-space / screen-space coordinates (`UnityObjectToClipPos`, `ComputeScreenPos`) to sample that texture:

    * Every pixel of the device’s quad shows **exactly** what the other world would look like at that screen position.
    * Visually, the device behaves like a small “window cut out of reality” rather than a floating monitor.

* **First-Person Character Controller + Animations**

  * Custom `PlayerController3D` built on Unity’s `CharacterController`.
  * Handles movement, gravity, jumping, and mouse look.
  * Drives Animator parameters (`IsRun`, `IsJump`) based on grounded state and speed for responsive first-person animations.

* **Level Authoring for Two Timelines**

  * World B is built as a variant of World A:

    * Shared colliders and geometry for guaranteed structural alignment.
    * Visual differences (materials, props) to emphasize the contrast between timelines.
  * This pattern keeps the two worlds logically in sync while allowing distinct moods and hazards.

---

## Tech Stack

* **Engine**: Unity 2022 LTS (3D, Built-in Render Pipeline)
* **Language**: C# (gameplay, controllers, time-shift logic)
* **Rendering**:

  * Built-in pipeline cameras + `RenderTexture`
  * Custom **Unlit HLSL shader** for screen-space portal rendering
* **Other**:

  * CharacterController-based first-person movement
  * Animator Controller for basic run/jump states

---

## Status & Intended Use

This project is a **focused prototype**, not a full game.
It is intended for:

* Demonstrating a **dual-world time-shift mechanic** in a minimal, inspectable setup.
* Serving as a reference for:

  * Level designers exploring stacked-world layouts.
  * Programmers interested in **screen-space portals** using multiple cameras and custom shaders.
* Providing a concrete portfolio piece for graduate applications in **game development, graphics, and spatial computing**.

You are welcome to explore, modify, or extend the prototype—for example, by adding enemies, AI behaviors that differ between worlds, or puzzle elements that require precise timing of world switches.
