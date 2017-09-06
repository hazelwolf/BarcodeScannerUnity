using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cognizant.SmartPick.ScanningManager;

//using LitJson;
public class ApplicationManager : MonoBehaviour
{
    // private variables
    private bool isLoggedIn = false;
    // public variables
    // public Notification notifier;
    // public DisplayManager displayManager;
    private BarcodeScannerManager scanManager;

    // public SpeechManager sysSpeak;
    public RawImage Image;

    public Text TextHeader;
    // private string userType;
    //  public AudioSource Audio;
    // public User initiateUser;
    void Awake()
    {
        
    }
    void Start()
    {
        scanManager = new BarcodeScannerManager();
        scanManager.StartScannerCamera(Image);
        // sysSpeak.Speak("Welcome, Please scan your QR code to Login");
        // notifier.SendNotification("Welcome", "Scan your Code to login", 3f);
    }

    // Update is called once per frame
    void Update()
    {
        scanManager.UpdateScanner("ImageScanned", this.gameObject, this);

    }

//     public void login(string input)
//     {
//         // Feedback
//         Audio.Play();
//         var Uid = input.Split(':');
//         Debug.Log(Uid);
//         VerificationModule verify = new VerificationModule();
//         JsonData data = verify.confirm("userData", Uid[1], "Uid");
//         Debug.Log("code " + input);
//         Debug.Log("data " + data);
//         if (data != null)
//         {
//             string Activity = data["Activity"].ToString();

//             //switch between various users
//             switch (Activity)
//             {
//                 case "unit":
//                     userType = Activity;
//                     break;
//                 case "grocery":
//                     userType = Activity;
//                     break;
//                 case "case lot":
//                     userType = Activity;
//                     break;
//                 default:
//                     userType = null;
//                     break;
//             }

//             initiateUser = new User(data["Uid"].ToString(), data["Name"].ToString(), userType);
//             sysSpeak.Speak("Hello " + initiateUser.Name + ", Would you, like to get started for your" + userType + " picking tasks today!");
//             notifier.SendNotification("Login Succesful !:", "Welcome, " + initiateUser.Name, 500f);
//             isLoggedIn = true;
//             if(isLoggedIn){
//                 notifier.SendNotification("Determining Location:", "Scan the nearest location", 3f);
//             }
//         }
//         else
//         {
//             notifier.SendNotification("Login Unsuccesful", "Try again", 3f);
//         }
//     }


//     void determineLocation(string input)
//     {
//         // Feedback
//         Audio.Play();
//         Debug.Log(input);
//         JsonData data = JSONParser.JsontoObj(input);
//         sysSpeak.Speak("You are currently at Aisle " + data["AisleId"] + ", Would you like to set this as your current location"); //(Yes/No)
//         notifier.SendNotification("Location found", "You are at Aisle : " + data["AisleId"], 5f);
//     }

    public void ImageScanned(object barcodeValue)
    {
        string value = barcodeValue.ToString();
        // if (value.Contains("Uid") && !isLoggedIn)
        // {
        //     login(value);
        //    //notifier.SendNotification("Determing location:", "Scan the nearest aisle to continue", 3f);
        // }
        // else if (value.Contains("Aisle") && isLoggedIn)
        // {
        //     determineLocation(value);
        // }
        // else if(value.Contains("Aisle") && !isLoggedIn){
        //     sysSpeak.Speak("Please login first to continue!");
        // }
        // else if (value.Contains("Uid") && isLoggedIn)
        // {
        //     sysSpeak.Speak("You are already logged in as "+ initiateUser.Name);
           
        // } 
        TextHeader.text = value;
    }
}

enum AuthenticationState
{
    Authenticated,
    Unauthenticated
}

