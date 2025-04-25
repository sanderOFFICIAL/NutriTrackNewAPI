# NutriTrack API Documentation

This API allows users to interact with the database. It also includes a description of the EndPoints and how to properly interact with them, as well as database models.

## Installation and Setup

### Prerequisites

To run the project, the following tools need to be installed:

- [Visual Studio](https://visualstudio.microsoft.com/) (for development and running the project)
- [.NET SDK](https://dotnet.microsoft.com/download) (version 7.0 or newer)
- [MSSQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) specifically MSSQL Server
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for interacting with the database
- ASP.NET API for Web SDK (create a project on this platform)

Ось доповнений крок, що містить інструкцію для налаштування Firebase для використання токенів авторизації:

### Step 1: Cloning the Repository

Clone the repository to your local machine:

```
git clone https://github.com/sanderOFFICIAL/NutriTrackAPI.git
```

### Step 2: Running the Project

In the cloned project, you need to replace the "DefaultConnection" string in the `appsettings.json` file with your own connection string for MSSQL Server.  
Also, replace the "applicationUrl" in the `launchSettings.json` file with your local IP address where your computer is connected.

### Step 3: Firebase Authentication Configuration

To use Firebase Authentication for token validation, you need to add the Firebase Admin SDK configuration.

1. Go to the [Firebase Console](https://console.firebase.google.com/).
2. Select your project or create a new one.
3. Navigate to the **Project Settings** (gear icon), and in the **Service accounts** tab, click **Generate new private key**.
4. Download the JSON file containing your Firebase Admin SDK credentials.
5. Place this file in the root of your project.

Then, make sure to initialize Firebase Admin SDK in your `FirebaseService.cs` by loading the credentials file, like this:

```csharp
FirebaseApp.Create(new AppOptions()
{
    Credential = GoogleCredential.FromFile("path_to_your_firebase_credentials_file.json"),
});
```

Replace `"path_to_your_firebase_credentials_file.json"` with the actual path to the Firebase Admin SDK JSON file you downloaded.

Now your API will be able to verify Firebase ID tokens for authentication.

### Step 4: Creating the Database

Create a database in MSSQL Server after that in the root folder of the project, run the following commands in the console:

```
dotnet tool install --global dotnet-ef
```

```
dotnet ef database update
```

### Step 5: Accessing the API

Access the API using the link: "http://192.168.0.106:5182/", but replace it with your own computer’s IP address.
After that, you can run the project in Swagger UI and check if all the controllers are working.

## Description of Project EndPoints

# AdminController

## Overview

The `AdminController` provides administrative functionalities to manage users and consultants, including removing data, retrieving user/consultant information, updating profiles, and viewing statistics.

## Endpoints

### 1. **Remove User and Related Data**

- **URL**: `DELETE /api/Admin/remove-user/{userUid}`
- **Description**: Removes a user and all related data (e.g., weight measurements, meal entries, exercise entries, etc.) from the database.
- **Parameters**:
  - `userUid` (string): The unique identifier for the user to be removed.
- **Response**:
  - `200 OK`: User and all related data removed successfully.
  - `500 Internal Server Error`: Error during the removal process.

### 2. **Remove Consultant and Related Data**

- **URL**: `DELETE /api/Admin/remove-consultant/{consultantUid}`
- **Description**: Removes a consultant and all related data (e.g., consultant notes, user-consultant relationships, etc.) from the database.
- **Parameters**:
  - `consultantUid` (string): The unique identifier for the consultant to be removed.
- **Response**:
  - `200 OK`: Consultant and all related data removed successfully.
  - `500 Internal Server Error`: Error during the removal process.

### 3. **Get User Information**

- **URL**: `GET /api/Admin/get-user-info`
- **Description**: Retrieves user information based on nickname, account creation date, and last login date.
- **Parameters**:
  - `nickname` (string): User's nickname.
  - `createdAt` (DateTime): Account creation date.
  - `lastLogin` (DateTime): Last login date.
- **Response**:
  - `200 OK`: User information retrieved successfully.
  - `404 Not Found`: If the user is not found.

### 4. **Get Consultant Information**

- **URL**: `GET /api/Admin/get-consultant-info`
- **Description**: Retrieves consultant information based on nickname, account creation date, and last login date.
- **Parameters**:
  - `nickname` (string): Consultant's nickname.
  - `createdAt` (DateTime): Account creation date.
  - `lastLogin` (DateTime): Last login date.
- **Response**:
  - `200 OK`: Consultant information retrieved successfully.
  - `404 Not Found`: If the consultant is not found.

### 5. **Get Statistics**

- **URL**: `GET /api/Admin/get-statistics`
- **Description**: Retrieves general statistics about users and consultants (e.g., total users, active users, total consultants, active consultants).
- **Response**:
  - `200 OK`: The statistics are returned successfully.

### 6. **Update User Profile**

- **URL**: `PATCH /api/Admin/update-user-profile/{user_uid}`
- **Description**: Updates the profile information of a user.
- **Parameters**:
  - `user_uid` (string): The unique identifier for the user.
- **Request Body**:
  ```json
  {
    "Nickname": "string",
    "ProfilePicture": "string",
    "ProfileDescription": "string"
  }
  ```
- **Response**:
  - `200 OK`: The user's profile was updated successfully.
  - `404 Not Found`: If the user is not found.

### 7. **Update Consultant Profile**

- **URL**: `PATCH /api/Admin/update-consultant-profile/{consultant_uid}`
- **Description**: Updates the profile information of a consultant.
- **Parameters**:
  - `consultant_uid` (string): The unique identifier for the consultant.
- **Request Body**:
  ```json
  {
    "Nickname": "string",
    "ProfilePicture": "string",
    "ProfileDescription": "string",
    "ExperienceYears": "int"
  }
  ```
- **Response**:
  - `200 OK`: The consultant's profile was updated successfully.
  - `404 Not Found`: If the consultant is not found.

# AuthController

## Overview

The `AuthController` handles the user authentication and registration process for different roles in the application: users, consultants, and admins. It provides endpoints for registering and logging in users, consultants, and admins with Firebase authentication.

## Endpoints

### 1. **Register User**

- **URL**: `POST /api/Auth/register/user`
- **Description**: Registers a new user by verifying the provided Firebase ID token and storing the user's details.
- **Request Body**:
  ```json
  {
    "idToken": "string",
    "nickname": "string",
    "profile_picture": "string",
    "profile_description": "string",
    "gender": "string",
    "height": "int",
    "current_weight": "double",
    "activity_level": Enum value (0-Sedentary, 1-Light, 2-Moderate, 3-High) indicating the user's activity level,
    "birth_year": 0
  }
  ```
- **Response**:
  - `200 OK`: User registered successfully.
  - `400 Bad Request`: User already registered.
  - `401 Unauthorized`: Invalid token.

### 2. **Register Consultant**

- **URL**: `POST /api/Auth/register/consultant`
- **Description**: Registers a new consultant by verifying the provided Firebase ID token and storing the consultant's details.
- **Request Body**:
  ```json
  {
    "idToken": "string",
    "nickname": "string",
    "profile_picture": "string",
    "profile_description": "string",
    "experience_years": "int",
    "max_clients": "int",
    "gender": "string"
  }
  ```
- **Response**:
  - `200 OK`: Consultant registered successfully.
  - `400 Bad Request`: Consultant already registered.
  - `401 Unauthorized`: Invalid token.

### 3. **Register Admin**

- **URL**: `POST /api/Auth/register/admin`
- **Description**: Registers a new admin by verifying the provided Firebase ID token and storing the admin's details.
- **Request Body**:
  ```json
  {
    "idToken": "string",
    "name": "string",
    "email": "string",
    "phone_number": "string"
  }
  ```
- **Response**:
  - `200 OK`: Admin registered successfully.
  - `400 Bad Request`: Admin already registered.
  - `401 Unauthorized`: Invalid token.

### 4. **Login User**

- **URL**: `POST /api/Auth/login/user`
- **Description**: Logs in a user by verifying the provided Firebase ID token and updating their last login timestamp.
- **Request Body**:
  ```json
  {
    "idToken": "string"
  }
  ```
- **Response**:
  - `200 OK`: User login successful.
  - `401 Unauthorized`: User not found or invalid token.

### 5. **Login Consultant**

- **URL**: `POST /api/Auth/login/consultant`
- **Description**: Logs in a consultant by verifying the provided Firebase ID token and updating their last login timestamp.
- **Request Body**:
  ```json
  {
    "idToken": "string"
  }
  ```
- **Response**:
  - `200 OK`: Consultant login successful.
  - `401 Unauthorized`: Consultant not found or invalid token.

### 6. **Login Admin**

- **URL**: `POST /api/Auth/login/admin`
- **Description**: Logs in an admin by verifying the provided Firebase ID token and returning admin details.
- **Request Body**:
  ```json
  {
    "idToken": "string"
  }
  ```
- **Response**:
  - `200 OK`: Admin login successful.
  - `401 Unauthorized`: Admin not found or invalid token.

# ConsultantController

## Overview

The `ConsultantController` handles actions related to consultants, including inviting users, updating consultant profiles, managing client assignments, and retrieving consultant details. It interacts with the `Consultants` and `Users` models, and uses Firebase for user authentication.

## Routes

### `POST /api/consultant/send-invite-to-user`

Sends an invitation from the consultant to a user to become a client.

#### Request Body

```json
{
  "idToken": "string",
  "user_uid": "string"
}
```

#### Responses

- `200 OK`: Invite sent successfully.
- `400 Bad Request`: No available slots for new clients or consultant not found.
- `404 Not Found`: User not found.

### `POST /api/consultant/user-respond-invite`

Allows a user to accept or reject an invitation from a consultant.

#### Request Body

```json
{
  "idToken": "string",
  "consultant_uid": "string",
  "is_accepted": true
}
```

#### Responses

- `200 OK`: Invite response recorded.
- `404 Not Found`: Invite not found, user or consultant not found.

### `DELETE /api/consultant/consultant-remove-user`

Removes a user from a consultant’s client list and deletes associated requests.

#### Request Body

```json
{
  "idToken": "string",
  "user_uid": "string"
}
```

#### Responses

- `200 OK`: User removed successfully.
- `400 Bad Request`: Unauthorized or consultant not found.
- `404 Not Found`: User is not assigned to this consultant.

### `PUT /api/consultant/update-nickname`

Updates the nickname of the consultant.

#### Request Body

```json
{
  "idToken": "string",
  "new_nickname": "string"
}
```

#### Responses

- `200 OK`: Nickname updated successfully.
- `400 Bad Request`: New nickname is required.
- `404 Not Found`: Consultant not found.

### `PUT /api/consultant/update-profile-picture`

Updates the profile picture of the consultant.

#### Request Body

```json
{
  "idToken": "string",
  "new_profile_picture": "string"
}
```

#### Responses

- `200 OK`: Profile picture updated successfully.
- `400 Bad Request`: New profile picture is required.
- `404 Not Found`: Consultant not found.

### `PUT /api/consultant/update-profile-description`

Updates the profile description of the consultant.

#### Request Body

```json
{
  "idToken": "string",
  "new_profile_description": "string"
}
```

#### Responses

- `200 OK`: Profile description updated successfully.
- `400 Bad Request`: New profile description is required.
- `404 Not Found`: Consultant not found.

### `PUT /api/consultant/update-max-clients`

Updates the maximum number of clients a consultant can have.

#### Request Body

```json
{
  "idToken": "string",
  "new_max_clients": "int"
}
```

#### Responses

- `200 OK`: Max clients updated successfully.
- `400 Bad Request`: New max clients count is required.
- `404 Not Found`: Consultant not found.

### `GET /api/consultant/get-all-consultants`

Retrieves a list of all consultants with basic details.

#### Responses

- `200 OK`: List of consultants.
- `400 Bad Request`: Error retrieving consultants.

### `GET /api/consultant/get-consultant/{uid}`

Retrieves the details of a specific consultant by UID.

#### Responses

- `200 OK`: Consultant found.
- `404 Not Found`: Consultant not found.

# ConsultantNoteController

## Overview

The `ConsultantNoteController` provides endpoints for managing consultant notes related to user goals. It allows consultants to create, update, retrieve, and delete notes associated with specific goals. It also ensures that actions are performed by authenticated consultants and that the consultant's requests to the user are accepted before modifying any notes.

## Endpoints

### 1. **Add Note**

- **URL**: `POST /api/ConsultantNote/add-note`
- **Description**: Adds a new note for a user's goal.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "goal_id": "int",
    "content": "string"
  }
  ```
- **Response**:
  - `200 OK`: Note added successfully.
  - `400 Bad Request`: If the goal is not found or the consultation request is not accepted.
  - `404 Not Found`: If the goal or consultant is not found.

### 2. **Update Note**

- **URL**: `PUT /api/ConsultantNote/update-note`
- **Description**: Updates an existing note for a user's goal.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "note_id": "int",
    "content": "string"
  }
  ```
- **Response**:
  - `200 OK`: Note updated successfully.
  - `400 Bad Request`: If the consultation request is not accepted.
  - `404 Not Found`: If the note is not found.
  - `401 Unauthorized`: If the consultant is not authorized to update the note.

### 3. **Get Notes**

- **URL**: `GET /api/ConsultantNote/get-notes`
- **Description**: Retrieves all notes for a specific goal.
- **Query Parameters**:
  - `goalId` (int): The ID of the goal to fetch notes for.
- **Response**:
  - `200 OK`: A list of notes associated with the goal.
  - `404 Not Found`: If no notes are found for the goal.

### 4. **Delete Note**

- **URL**: `DELETE /api/ConsultantNote/delete-note`
- **Description**: Deletes an existing note for a user's goal.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "note_id": "int"
  }
  ```
- **Response**:
  - `200 OK`: Note deleted successfully.
  - `400 Bad Request`: If the consultation request is not accepted.
  - `404 Not Found`: If the note is not found.
  - `401 Unauthorized`: If the consultant is not authorized to delete the note.

# UserController

## Overview

The `UserController` provides endpoints for managing user profiles and associated actions, such as updating user details and removing consultants. It allows users to update their nickname, profile picture, description, and current weight. Additionally, users can remove consultants they are associated with. All actions are validated with Firebase authentication tokens.

## Endpoints

### 1. **Update Nickname**

- **URL**: `PUT /api/User/update-nickname`
- **Description**: Updates the user's nickname.
- **Request Body**:
  ```json
  {
    "idToken": "string",
    "new_nickname": "string"
  }
  ```
- **Response**:
  - `200 OK`: Nickname updated successfully.
  - `404 Not Found`: If the user is not found.

### 2. **Update Profile Picture**

- **URL**: `PUT /api/User/update-profile-picture`
- **Description**: Updates the user's profile picture.
- **Request Body**:
  ```json
  {
    "idToken": "string",
    "new_profile_picture": "string"
  }
  ```
- **Response**:
  - `200 OK`: Profile picture updated successfully.
  - `404 Not Found`: If the user is not found.

### 3. **Update Profile Description**

- **URL**: `PUT /api/User/update-profile-description`
- **Description**: Updates the user's profile description.
- **Request Body**:
  ```json
  {
    "idToken": "string",
    "new_profile_description": "string"
  }
  ```
- **Response**:
  - `200 OK`: Profile description updated successfully.
  - `404 Not Found`: If the user is not found.

### 4. **Update Current Weight**

- **URL**: `PUT /api/User/update-current-weight`
- **Description**: Updates the user's current weight.
- **Request Body**:
  ```json
  {
    "idToken": "string",
    "new_current_weight": "double"
  }
  ```
- **Response**:
  - `200 OK`: Current weight updated successfully.
  - `404 Not Found`: If the user is not found.

### 5. **Remove Consultant**

- **URL**: `DELETE /api/User/remove-consultant`
- **Description**: Removes a consultant from the user's list and deletes any pending requests.
- **Request Body**:
  ```json
  {
    "idToken": "string",
    "consultant_uid": "string"
  }
  ```
- **Response**:
  - `200 OK`: Consultant removed successfully and pending requests deleted.
  - `404 Not Found`: If the consultant is not found or not assigned to the user.

### 6. **Get User by UID**

- **URL**: `GET /api/User/get-user-by-uid`
- **Description**: Retrieves a user by their UID.
- **Query Parameters**:
  - `uid`: The unique identifier of the user.
- **Response**:
  - `200 OK`: The user details.
  - `404 Not Found`: If the user is not found.

### 7. **Get All Users**

- **URL**: `GET /api/User/get-all-users`
- **Description**: Retrieves all users' basic information.
- **Response**:
  - `200 OK`: List of all users.
  - `404 Not Found`: If no users are found.

# ExerciseController

## Overview

The `ExerciseController` provides endpoints for managing exercise entries, including adding, updating, retrieving, and deleting exercise data for users. The controller integrates with Firebase authentication to ensure that users can only interact with their own exercise data.

## Endpoints

### 1. **Add Exercise**

- **URL**: `POST /api/Exercise/add-exercise`
- **Description**: Adds a new exercise entry for the user.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "exercise_name": "string",
    "duration_minutes": "int",
    "calories_burned": "float",
    "exercise_type": "string",
    "entry_date": "datetime"
  }
  ```
- **Response**:
  - `200 OK`: Exercise entry added successfully.
  - `404 Not Found`: If the user is not found.

### 2. **Update Exercise**

- **URL**: `PUT /api/Exercise/update-exercise`
- **Description**: Updates an existing exercise entry for the user.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "exercise_name": "string",
    "duration_minutes": "int",
    "calories_burned": "float",
    "exercise_type": "string",
    "entry_date": "datetime",
    "ExerciseId": "int"
  }
  ```
- **Response**:
  - `200 OK`: Exercise entry updated successfully.
  - `404 Not Found`: If the exercise entry is not found or does not belong to the user.

### 3. **Get Exercises**

- **URL**: `GET /api/Exercise/get-exercises`
- **Description**: Retrieves all exercise entries for a user.
- **Query Parameters**:
  - `IdToken`: The Firebase authentication token for the user.
- **Response**:
  - `200 OK`: List of exercise entries.
  - `404 Not Found`: If no exercises are found for the user.

### 4. **Delete Exercise**

- **URL**: `DELETE /api/Exercise/delete-exercise`
- **Description**: Deletes an exercise entry for the user.
- **Request Body**:
  ```json
  {
    "ExerciseId": "int",
    "IdToken": "string"
  }
  ```
- **Response**:
  - `200 OK`: Exercise entry deleted successfully.
  - `404 Not Found`: If the exercise entry is not found or does not belong to the user.

# GoalController

## Overview

The `GoalController` provides endpoints for managing user goals, including creating, retrieving, updating, and approving goals. It integrates with Firebase authentication to ensure that only authenticated users and authorized consultants can interact with the goals.

## Endpoints

### 1. **Create User Goal**

- **URL**: `POST /api/Goal/create-user-goal`
- **Description**: Creates a new goal for the user, including nutrition and exercise plans based on their current data.
- **Request Body**:
  ```json
  {
    "idToken": "string",
    "consultant_uid": "uid or just del if consult not need",
    "goal_type": "Enum value (0-Loss, 1-Gain, 2-Maintain) indicating the goal objective.",
    "target_weight": "double",
    "duration_weeks": "int"
  }
  ```
- **Response**:
  - `201 Created`: Goal created successfully.
  - `404 Not Found`: If the user is not found.
  - `400 Bad Request`: If user data is incomplete for goal creation.

### 2. **Get Specific Goal by ID**

- **URL**: `GET /api/Goal/get-specific-goal-by-id/{goalId}`
- **Description**: Retrieves a specific goal by its ID.
- **Response**:
  - `200 OK`: Returns the goal details.
  - `404 Not Found`: If the goal is not found.

### 3. **Get All User Goal IDs**

- **URL**: `GET /api/Goal/get-all-user-goal-ids`
- **Query Parameters**:
  - `idToken`: The Firebase authentication token for the user.
- **Description**: Retrieves all goal IDs for the authenticated user.
- **Response**:
  - `200 OK`: List of goal IDs.
  - `404 Not Found`: If no goals are found for the user.

### 4. **Get Goal ID by User UID**

- **URL**: `GET /api/Goal/get-goal-id-by-user-uid/{userUid}`
- **Description**: Retrieves the goal ID for a specific user.
- **Response**:
  - `200 OK`: Returns the goal ID.
  - `404 Not Found`: If no goal is found for the specified user.

### 5. **Update User Weight**

- **URL**: `PUT /api/Goal/update-goal-weight`
- **Query Parameters**:
  - `idToken`: The Firebase authentication token for the user.
- **Request Body**:
  ```json
  {
    "goal_id": 0,
    "new_weight": "double"
  }
  ```
- **Response**:
  - `200 OK`: Target weight updated and goal recalculated successfully.
  - `404 Not Found`: If the goal or user is not found.

### 6. **Approve Goal by Consultant**

- **URL**: `PUT /api/Goal/approve-goal-by-consultant`
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "GoalId": "int"
  }
  ```
- **Description**: Approves a user goal by the assigned consultant.
- **Response**:
  - `200 OK`: Goal successfully approved.
  - `404 Not Found`: If the goal is not found or the consultant is not authorized to approve it.

# MealController

## Overview

The `MealController` provides endpoints for managing meal entries, allowing users to add, delete, and view their meal data, including detailed nutritional information for each product in the meal.

## Endpoints

### 1. **Add Meal**

- **URL**: `POST /api/Meal/add-meal`
- **Description**: Adds a new meal for the user by storing meal entries with nutritional data for each product.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "meal_type": "string",
    "products": [
      {
        "product_name": "string",
        "quantity_grams": "double",
        "calories": "double",
        "protein": "double",
        "carbs": "double",
        "fats": "double"
      }
    ]
  }
  ```
- **Response**:
  - `200 OK`: Meal and products added successfully.
  - `404 Not Found`: If the user is not found.
  - `400 Bad Request`: If there is an error with the request.

### 2. **Delete Meal**

- **URL**: `POST /api/Meal/delete-meal`
- **Description**: Deletes one or more meal entries for the user based on the provided entry ID.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "EntryId": "int?"
  }
  ```
- **Response**:
  - `200 OK`: Meal entries deleted successfully.
  - `404 Not Found`: If no meals are found for the user matching the provided criteria.
  - `400 Bad Request`: If `EntryId` is not provided.

### 3. **Get All Meals**

- **URL**: `GET /api/Meal/get-all-meals`
- **Query Parameters**:
  - `idToken`: Firebase authentication token for the user.
- **Description**: Retrieves all meal entries for the authenticated user.
- **Response**:
  - `200 OK`: List of meal entries.
  - `404 Not Found`: If no meal entries are found for the user.

# StreakController

## Overview

The `StreakController` provides endpoints for managing streaks, including starting a new streak, updating an existing streak, retrieving streak histories, and disabling the active streak for a user.

## Endpoints

### 1. **Add Streak**

- **URL**: `POST /api/Streak/add-streak`
- **Description**: Starts a new streak for the user and adds it to the database.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "current_streak": "int"
  }
  ```
- **Response**:
  - `200 OK`: New streak started successfully.
  - `404 Not Found`: If the user is not found.

### 2. **Update Streak**

- **URL**: `PUT /api/Streak/update-streak`
- **Description**: Updates an existing streak with a new streak count and status (active/inactive).
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "current_streak": "int",
    "is_active": "bool"
  }
  ```
- **Response**:
  - `200 OK`: Streak updated successfully.
  - `404 Not Found`: If the user or active streak is not found.

### 3. **Get Streaks**

- **URL**: `GET /api/Streak/get-streaks`
- **Query Parameters**:
  - `idToken`: Firebase authentication token for the user.
- **Description**: Retrieves the streak history for the authenticated user.
- **Response**:
  - `200 OK`: List of streak histories.
  - `404 Not Found`: If the user is not found.

### 4. **Disable Streak**

- **URL**: `DELETE /api/Streak/disable-streak`
- **Description**: Disables the user's active streak, marking it as inactive.
- **Request Body**:
  ```json
  {
    "IdToken": "string"
  }
  ```
- **Response**:
  - `200 OK`: Streak disabled successfully.
  - `404 Not Found`: If the user or active streak is not found.

# WaterController

## Overview

The `WaterController` provides endpoints for managing the user's water intake records. It allows users to add, update, delete, and retrieve water intake entries.

## Endpoints

### 1. **Add Water Intake**

- **URL**: `POST /api/Water/add-water`
- **Description**: Adds a new water intake entry for the user.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "amount_ml": "double",
    "entry_date": "DateTime"
  }
  ```
- **Response**:
  - `200 OK`: Water intake added successfully.
  - `404 Not Found`: If the user is not found.

### 2. **Update Water Intake**

- **URL**: `PUT /api/Water/update-water`
- **Description**: Updates an existing water intake entry with a new amount.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "intakeId": "int",
    "amount_ml": "double"
  }
  ```
- **Response**:
  - `200 OK`: Water intake updated successfully.
  - `404 Not Found`: If the user or the specific intake entry is not found.

### 3. **Delete Water Intake**

- **URL**: `DELETE /api/Water/delete-water`
- **Description**: Deletes a water intake entry for the user.
- **Request Body**:
  ```json
  {
    "IdToken": "string",
    "intakeId": "int"
  }
  ```
- **Response**:
  - `200 OK`: Water intake entry deleted successfully.
  - `404 Not Found`: If the user or the specific intake entry is not found.

### 4. **Get Water Intake**

- **URL**: `GET /api/Water/get-water`
- **Query Parameters**:
  - `idToken`: Firebase authentication token for the user.
- **Description**: Retrieves the user's water intake records.
- **Response**:
  - `200 OK`: A list of water intake entries.
  - `404 Not Found`: If the user is not found.

# WeightMeasurementsController

## Overview

The `WeightMeasurementsController` provides endpoints for managing the user's weight measurements. It allows users to post new weight measurements, retrieve weight measurements by user, and get a specific weight measurement by its ID.

## Endpoints

### 1. **Add Weight Measurement**

- **URL**: `POST /api/WeightMeasurements`
- **Description**: Adds a new weight measurement for a user.
- **Request Body**:
  ```json
  {
    "UserUid": "string",
    "Weight": "double",
    "MeasuredAt": "DateTime",
    "DeviceId": "string",
    "IsSynced": "boolean"
  }
  ```
- **Response**:
  - `201 Created`: Weight measurement added successfully.
  - `400 Bad Request`: If the request body is invalid.
  - `404 Not Found`: If the user is not found.

### 2. **Get Weight Measurements by User**

- **URL**: `GET /api/WeightMeasurements/user/{userUid}`
- **Description**: Retrieves all weight measurements for a specified user.
- **Response**:
  - `200 OK`: A list of weight measurements for the user.
  - `404 Not Found`: If no weight measurements are found for the user.

### 3. **Get Weight Measurement by ID**

- **URL**: `GET /api/WeightMeasurements/{id}`
- **Description**: Retrieves a specific weight measurement by its ID.
- **Response**:
  - `200 OK`: The weight measurement with the specified ID.
  - `404 Not Found`: If the weight measurement is not found.
