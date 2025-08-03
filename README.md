# 🌸 Multisensory VR Game for Olfactory Training 🧠

**An immersive virtual reality rehabilitation system** designed to aid recovery from anosmia (loss of smell).  
This system delivers synchronized **scent**, **visual**, and **audio cues** to enhance olfactory neuroplasticity through Unity-powered gameplay and real-time hardware integration.

---

## 📚 Table of Contents

- [🎯 Features](#-features)
  - [🧠 Training Mode](#-training-mode)
  - [🧪 Assessment Mode](#-assessment-mode)
- [📽️ Demo](#-demo)
- [🧠 Technologies Used](#-technologies-used)
- [🔧 System Architecture](#-system-architecture)
- [👥 Authors](#-authors)

---

## 🎯 Features

### 🧠 Training Mode

- 🌺 **Guided Scent Zones**: Players enter specific scent zones inside a virtual environment (e.g., a plant nursery) designed in Unity. Each zone is associated with a distinct scent (e.g., rose, lavender).
- 🧠 **Multisensory Stimulation**: The user receives:
  - A **scent**, dispensed in real time via ultrasonic atomizers.
  - A **visual cue**, such as the appearance of flowers or herbs.
- 🔁 **Closed-loop interaction**: When the player steps into a zone, Unity updates Firebase, which then activates the appropriate atomizer.
- 📊 **Real-Time Feedback**: Player actions are tracked to ensure accuracy and consistency, encouraging engagement and adherence.
- 🎮 **Goal**: Strengthen odor-memory associations through repetition and immersion.

---

### 🧪 Evaluation Mode

- 🎯 **4-Round Challenge**: Each session consists of four randomized scent tests without assistance.
- ⏱️ **Timed Response Window**: Players must identify the correct scent within a fixed time after the scent is triggered.
- 🔄 **Scene-Based Triggering**: Unity signals Firebase to activate a randomized scent. Players make selections via in-game interaction (e.g., choose one of two objects).
- ✅ **Performance Metrics**:
  - Accuracy of identification.
  - Time taken to respond.
- 📱 **Data Logging**: Results are logged in Firebase and visualized in the mobile app to track progress over time.
- 🏆 **Gamified Experience**: Scores are used for motivation and leaderboard ranking.

---

## 📽️ Demo
### 1️⃣ Training Mode


https://github.com/user-attachments/assets/edd01fde-8ce9-41c6-8bad-6e11bcfe159b


### 2️⃣ Evaluation Mode
      

https://github.com/user-attachments/assets/141b6a3d-08d9-4520-9d59-3b0f4d4cf33b



> 🎥 _Recorded demos of Unity gameplay for olfactory rehabilitation_

---

## 🧠 Technologies Used

| Category            | Stack / Tools                                      |
|---------------------|----------------------------------------------------|
| Game Engine         | Unity (C#), Blender (3D Modeling)                  |
| Scent Hardware      | ESP32-S3, Arduino, 2N2222 transistors, Atomizers   |
| Database & Sync     | Firebase Realtime Database                         |
| Communication       | Unity ↔ Firebase ↔ ESP32 (Wi-Fi via HTTP requests) |
| App                 | Flutter (for session tracking and reminders)       |

---

## 🔧 System Architecture

- 🕹️ **Unity** delivers real-time VR experience and writes to Firebase based on player location or choices.
- 🌐 **Firebase** acts as a bridge between Unity and scent hardware.
- 💨 **ESP32** listens to Firebase changes and activates the corresponding atomizer via connected circuits.
- 📱 **Mobile App** logs session data, shows progress, and reminds users to complete training.

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
