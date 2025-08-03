# 🌸 Multisensory VR Game for Olfactory Training 🧠

**An immersive virtual reality rehabilitation system** that supports olfactory recovery for individuals with anosmia (loss of smell).  
This Unity-based project integrates real-time **scent delivery**, **visual cues**, and **audio feedback** to create a **multisensory experience** proven to enhance neural pathways and promote recovery.

---

## 📚 Table of Contents

- [🎯 Features](#-features)
- [📽️ Demo](#-demo)
- [🧠 Technologies Used](#-technologies-used)
- [🔧 System Architecture](#-system-architecture)
- [👥 Authors](#-authors)

---

## 🎯 Features

### 🌟 Multisensory Rehabilitation Modes

- **Training Mode**  
  Interactive environment with guided scent recognition using coordinated **smell**, **visual**, and **auditory cues**.
  
- **Assessment Mode**  
  Four-round scent identification test with randomized stimuli, **timed responses**, and **performance tracking**.

- **Real-time feedback** and **gamification features** such as scores, progress tracking, and reminders.

---

## 📽️ Demo

<div align="center">
  <video src="Demo/test mode for olfactory training - Made with Clipchamp.mp4" controls width="640"></video>
</div>

> 🎥 _Gameplay footage captured in Unity_  

---

## 🧠 Technologies Used

| Category            | Stack / Tools                                      |
|---------------------|----------------------------------------------------|
| Game Engine         | Unity (C#), Blender (3D Modeling)                  |
| Scent Hardware      | ESP32-S3, Arduino, 2N2222 transistors, Atomizers   |
| Database & Sync     | Firebase Realtime Database                         |
| Communication       | Unity ↔ Firebase ↔ ESP32 (Wi-Fi via HTTP requests) |
| App                 | Flutter (for tracking and scheduling sessions)     |

---

## 🔧 System Architecture

- **Unity VR Game** triggers scent events through Firebase updates.
- **ESP32-based scent device** reads real-time Firebase changes and activates appropriate atomizers.
- **Mobile app** monitors training sessions, scores, and scheduling.

> 🔁 Low-latency synchronization between software and hardware ensures immersive, accurate olfactory feedback.

---

## 👥 Authors

- **Hager Samir**
- **Youssef Ahmed Shawki**
- **Mohamed Ibrahim**
- **Malak Nasser**
- **Kareem Noureddine**

**Supervised by:**  
Prof. Dr. Aliaa Rehan Youssef  
_Systems and Biomedical Engineering Department, Cairo University_

---


