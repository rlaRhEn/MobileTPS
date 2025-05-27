using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

public class AuthManager : MonoBehaviour
{
    public bool IsFirebaseReady { get; private set; } //연동 준비 
    public bool IsSignInOnProgress { get; private set; } //지금 로그인하고있는 상황

    public InputField emailField;
    public InputField passwordField;
    public Button signInButton;

    public static FirebaseApp firebaseApp;
    public static FirebaseAuth firebaseAuth;

    public static FirebaseUser user;


    private void Start()
    {
        signInButton.interactable = false;

        FirebaseApp.CheckDependenciesAsync().ContinueWith(continuationAction: task =>
        {
            var result = task.Result;

            if(result != DependencyStatus.Available)
            {
                Debug.LogError(message: result.ToString());
                IsFirebaseReady = false;
            }
            else
            {
                IsFirebaseReady = true;
                firebaseApp = FirebaseApp.DefaultInstance;
                firebaseAuth = FirebaseAuth.DefaultInstance;
            }

            signInButton.interactable = true;
        });
    }
    public void SignIn()
    {
       if(!IsFirebaseReady || IsSignInOnProgress || user != null)
        {
            return;
        }

        IsSignInOnProgress = true;
        signInButton.interactable = false;

        firebaseAuth.SignInWithEmailAndPasswordAsync(emailField.text, passwordField.text).ContinueWithOnMainThread(continuation: task => {
            IsSignInOnProgress = false;
            signInButton.interactable = true;

            if(task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
            else if(task.IsCanceled)
            {
                Debug.LogError(message: "Sign-in Cancled");
            }
            else
            {
                user = task.Result.User;
                SceneManager.LoadScene("Lobby");
            }
       });
    }
}
