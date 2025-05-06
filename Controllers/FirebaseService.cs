using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;

public class FirebaseService
{
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (!_initialized)
        {
            FirebaseApp.Create(new AppOptions()
            {
                Credential = GoogleCredential.FromFile("nutritrackkotlin-firebase-adminsdk-fbsvc-6517be9f24.json")
            });
            _initialized = true;
        }
    }
}
