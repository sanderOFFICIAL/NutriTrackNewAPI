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
                Credential = GoogleCredential.FromFile("nutri-track-app-kotlin-firebase-adminsdk-fbsvc-a6d5cc7a8f.json")
            });
            _initialized = true;
        }
    }
}
