# Unity Project Setup with Firebase

This project uses Firebase to manage authentication and real-time database functionality. Follow the steps below to set up Firebase in your local environment.

## Prerequisites

- Unity Editor (Compatible with your project version)
- Firebase SDK for Unity, Version 12.4
- A Firebase account with access to Firebase Console

## Installation Guide

### 1. Download Firebase SDK for Unity

1. Go to the [Firebase Unity SDK](https://firebase.google.com/docs/unity/setup) download page.
2. Download version **12.4** of the SDK.

### 2. Import Firebase SDK into Unity

1. Open your Unity project.
2. Import the Firebase SDK by selecting `Assets > Import Package > Custom Package...`.
3. Choose the `.unitypackage` files for **Firebase Auth** and **Firebase Database** from the SDK package.
4. Ensure all necessary files are selected and click **Import**.

### 3. Add Firebase to Your Unity Project

1. Go to the [Firebase Console](https://console.firebase.google.com/), create a new project, or open your existing project.
2. For **Android**:
   - In the Firebase Console, add an Android app and register it with your project.
   - Download the `google-services.json` file and place it in your Unity project under `Assets`.
3. For **iOS**:
   - In the Firebase Console, add an iOS app and register it with your project.
   - Download the `GoogleService-Info.plist` file and place it in your Unity project under `Assets`.

### 4. Set Up Authentication and Database

- Go to the **Firebase Console > Authentication** and enable the sign-in methods you need (e.g., Email/Password, Google, etc.).
- Go to the **Firebase Console > Database** and set up a new Realtime Database.
  - Configure the rules and security settings as per your project requirements.

### 5. Import Firebase Packages in Unity Scripts

To use Firebase Authentication and Database in your scripts, include the following imports at the top of each script where you need Firebase functionality:

```csharp
using Firebase.Auth;
using Firebase.Database;
